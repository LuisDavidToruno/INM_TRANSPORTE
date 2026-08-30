import type { ReactElement } from 'react';
import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { FileText, Info, RefreshCcw, ShieldCheck, Stamp, TriangleAlert } from 'lucide-react';

import { Boton, Campo, Nota, Panel, Pastilla, avisar } from '../../ui';
import { BloqueoDuro, pedir } from '../../api/misiones';
import { useNavigate } from 'react-router';

import { usarQuienEjecuta } from '../../app/puesto';

/**
 * `PT-020` — Trámite del permiso de circulación en día u hora inhábil.
 *
 * ── ⚠️ Lo que este panel vino a destrabar ───────────────────────────────────
 * `BD-04` bloquea el despacho de toda misión que circule en franja inhábil sin permiso de la
 * máxima autoridad, y estaba operando. <b>No existía forma de emitir el permiso</b>: cualquier
 * misión que tocara un sábado, un domingo o un feriado era indespachable, y el mensaje del
 * bloqueo decía «no hay ningún permiso registrado para esta misión» sin que hubiera cómo
 * registrar uno.
 *
 * ── Va en el expediente, no en una pantalla aparte ──────────────────────────
 * Junto al señalamiento de tramos inhábiles, que es donde el problema se ve. Quien lee «esta
 * misión cruza el sábado 21» tiene que poder pedir el permiso ahí mismo: mandarlo a otra
 * pantalla es cómo un trámite se posterga hasta el viernes por la tarde.
 *
 * ── Y se abre sin vehículo ni motorista, a propósito ────────────────────────
 * `RN-23` no exige que la misión esté programada para tramitar el permiso — tiene que poder
 * adelantarse. Lo que no se puede es <b>firmarlo</b> así: el permiso es nominativo porque el
 * agente en carretera compara el nombre del papel con quien va al volante.
 */
export default function TramiteDelPermiso({
  mision,
}: {
  readonly mision: string;
}): ReactElement | null {
  const quienEjecuta = usarQuienEjecuta();
  const cliente = useQueryClient();
  const ir = useNavigate();
  const [justificacion, setJustificacion] = useState('');
  const [reemitiendo, setReemitiendo] = useState<string | null>(null);
  const [cambio, setCambio] = useState('');

  const { data, isPending, isError } = useQuery({
    queryKey: ['permisos', mision],
    queryFn: () => pedir<Permiso[]>(`/misiones/${mision}/permisos`),
  });

  const reemitir = useMutation({
    mutationFn: (permiso: string) =>
      pedir<{ id: string; mensaje: string }>(`/permisos/${permiso}/reemitir`, {
        method: 'POST',
        body: JSON.stringify({
          ejecuta: quienEjecuta,
          motivo: cambio,
          momento: new Date().toISOString(),
        }),
      }),
    onSuccess: async (r) => {
      // Se dice explícito porque la gente lo supone al revés: un permiso reemitido **parece**
      // una corrección del anterior, y es un acto nuevo que necesita firma nueva.
      avisar.exito(r.mensaje);
      setReemitiendo(null);
      setCambio('');
      await cliente.invalidateQueries({ queryKey: ['permisos', mision] });
      await cliente.invalidateQueries({ queryKey: ['salvoconducto', mision] });
    },
    onError: (e) =>
      avisar.error(e instanceof BloqueoDuro ? e.paraMostrar : 'No se pudo reemitir el permiso.'),
  });

  const emitir = useMutation({
    mutationFn: (permiso: string) =>
      pedir<{ id: string }>(`/permisos/${permiso}/salvoconducto`, {
        method: 'POST',
        body: JSON.stringify({ ejecuta: quienEjecuta, momento: new Date().toISOString() }),
      }),
    onSuccess: () => {
      avisar.exito('Salvoconducto emitido. Imprímalo: sin el papel no se despacha.');
      ir(`/misiones/${mision}/salvoconducto`);
    },
    onError: (e) =>
      avisar.error(e instanceof BloqueoDuro ? e.paraMostrar : 'No se pudo emitir el documento.'),
  });

  const tramitar = useMutation({
    mutationFn: () =>
      pedir<Apertura>(`/misiones/${mision}/permiso`, {
        method: 'POST',
        body: JSON.stringify({
          justificacion,
          solicita: quienEjecuta,
          momento: new Date().toISOString(),
        }),
      }),
    onSuccess: async (r) => {
      // ⚠️ **«No hacía falta» no es un error.** Un vehículo con excepción de servicio
      // exceptuado sale sin permiso, y una ventana que no toca franja inhábil tampoco lo
      // necesita. Tratarlo como falla mandaría a resolver un problema que no existe.
      if (r.abierto) {
        avisar.exito('Trámite encaminado a la máxima autoridad. Todavía no ampara nada.');
        setJustificacion('');
      } else {
        avisar.info(r.mensaje);
      }
      await cliente.invalidateQueries({ queryKey: ['permisos', mision] });
    },
    onError: (e) =>
      avisar.error(e instanceof BloqueoDuro ? e.paraMostrar : 'No se pudo abrir el trámite.'),
  });

  if (isError || isPending) {
    return (
      <Panel titulo="Permiso de circulación">
        <p className="tw:text-sm tw:text-tinta-mid">
          {isError ? 'No se pudieron consultar los permisos.' : 'Consultando…'}
        </p>
      </Panel>
    );
  }

  const vivos = data.filter((p) => p.estado !== 'Desistido');
  const ampara = vivos.some((p) => p.ampara);

  // ⚠️ Firmado y "ampara" **no son lo mismo**: un permiso firmado deja de cubrir cuando cambia
  // el vehículo, el motorista, el destino o la ventana. Ésa es la diferencia que `PT-024`
  // existe para hacer visible antes del sábado por la mañana.
  // ⚠️ Sólo los que **exigen** reemisión. Una misión desprogramada no ampara nada en este
  // momento y puede volver a amparar sola: ofrecer «reemitir» ahí quemaría un folio —que no se
  // recicla— y pediría otra firma de la máxima autoridad para nada.
  const caducos = vivos.filter((p) => p.exigeReemision);

  return (
    <Panel titulo="Permiso de circulación en día u hora inhábil">
      <div className="tw:flex tw:flex-col tw:gap-4">
        {vivos.length === 0 ? (
          <p className="tw:text-sm tw:text-tinta-mid">
            No hay ningún permiso tramitado para esta misión. Si la ventana toca día u hora
            inhábil, <b>el despacho se va a bloquear</b> hasta que la máxima autoridad firme.
          </p>
        ) : (
          <ul className="tw:flex tw:flex-col tw:gap-3">
            {vivos.map((p) => (
              <li
                key={p.id}
                className={`tw:flex tw:flex-col tw:gap-1 tw:border-l-2 tw:py-1 tw:pl-3 ${
                  p.ampara ? 'tw:border-ok-fg' : 'tw:border-aviso-fg'
                }`}
              >
                <div className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-3 tw:text-sm">
                  <span className="tw:font-mono">{p.folio}</span>

                  {/* Tres estados y no dos. **Solicitado no ampara** —si se leyera como «hay
                      permiso», alguien creería que la misión puede salir—, y **firmado
                      tampoco basta**: deja de cubrir cuando cambia lo que ampara. */}
                  <Pastilla
                    tono={p.ampara ? 'ok' : p.porQueYaNoCubre !== null ? 'riesgo' : 'aviso'}
                  >
                    {p.ampara
                      ? 'firmado, ampara'
                      : p.porQueYaNoCubre !== null
                        ? 'firmado, YA NO CUBRE'
                        : 'esperando firma, no ampara'}
                  </Pastilla>

                  {p.reemplaza !== null && (
                    <span className="tw:text-xs tw:text-tinta-mid">reemite a uno anterior</span>
                  )}
                </div>

                {/* ── El diagnóstico, que dice QUÉ cambió ──────────────────── */}
                {p.porQueYaNoCubre !== null && (
                  <Nota tono="riesgo" icono={<TriangleAlert />}>
                    {p.porQueYaNoCubre}
                  </Nota>
                )}

                {p.vehiculo !== null && (
                  <span className="tw:text-xs tw:text-tinta-mid">
                    Ampara: {p.vehiculo}
                    {p.motorista !== null && ` · ${p.motorista}`}
                  </span>
                )}

                <span className="tw:text-sm tw:text-tinta-mid">{p.justificacion}</span>

                <span className="tw:text-xs tw:text-tinta-mid">
                  {fecha(p.desde)} al {fecha(p.hasta)}
                  {p.tramosInhabiles.length > 0 && ` · cubre ${p.tramosInhabiles.join(', ')}`}
                  {p.firmadoPor !== null && ` · firmó ${p.firmadoPor}`}
                </span>
              </li>
            ))}
          </ul>
        )}

        {/* ── Reemitir: el permiso firmado que dejó de cubrir ───────────────── */}
        {caducos.map((p) => (
          <div key={`re-${p.id}`} className="tw:flex tw:flex-col tw:gap-3">
            {reemitiendo === p.id ? (
              <>
                <Campo
                  etiqueta="Qué cambió"
                  obligatorio
                  ayuda="Queda en el asiento de anulación del salvoconducto anterior. Sin esto nadie puede reconstruir por qué hay dos folios."
                >
                  <input
                    value={cambio}
                    onChange={(e) => setCambio(e.target.value)}
                    placeholder="Sustitución de vehículo INS-P-014 por INS-P-021"
                  />
                </Campo>

                <div className="tw:flex tw:flex-wrap tw:items-center tw:gap-3">
                  <Boton
                    onClick={() => reemitir.mutate(p.id)}
                    cargando={reemitir.isPending}
                    disabled={cambio.trim() === ''}
                    icono={<RefreshCcw />}
                  >
                    Reemitir
                  </Boton>

                  <Boton variante="fantasma" onClick={() => setReemitiendo(null)}>
                    Cancelar
                  </Boton>
                </div>

                <Nota tono="aviso">
                  Al reemitir, el <b>salvoconducto anterior queda anulado</b> —el punto de
                  verificación lo dirá de inmediato— y el permiso nuevo <b>nace sin firma</b>.
                  La firma de la máxima autoridad no se arrastra: lo que firmó fue otro
                  vehículo con otro motorista.
                </Nota>
              </>
            ) : (
              <div>
                <Boton onClick={() => setReemitiendo(p.id)} icono={<RefreshCcw />}>
                  Reemitir el permiso {p.folio}
                </Boton>
              </div>
            )}
          </div>
        ))}

        {ampara ? (
          <>
            <Nota tono="ok" icono={<ShieldCheck />}>
              La misión tiene permiso firmado. <b>Ampara este vehículo, este motorista, este
              destino y esta ventana</b> — un relevo de motorista lo invalida y obliga a
              reemitirlo.
            </Nota>

            {/* ⚠️ El permiso firmado **no basta para salir**: `RN-25` exige el salvoconducto
                impreso, y no hay excepción. El control en carretera es físico y el agente
                pide un papel, no consulta un sistema. */}
            <div className="tw:flex tw:flex-wrap tw:items-center tw:gap-3">
              <Boton
                onClick={() => emitir.mutate(vivos.find((p) => p.ampara)!.id)}
                cargando={emitir.isPending}
                icono={<FileText />}
              >
                Emitir el salvoconducto
              </Boton>

              <span className="tw:text-xs tw:text-tinta-mid">
                <b>Sin el papel impreso no se despacha</b> en día u hora inhábil. Si ya lo
                emitió, ábralo para imprimirlo o reimprimirlo.
              </span>
            </div>
          </>
        ) : (
          <>
            <Campo
              etiqueta="Por qué tiene que circular en franja inhábil"
              obligatorio
              ayuda="Es lo único que la máxima autoridad tiene para decidir. Sin esto, firmar es aprobar lo que aparece."
            >
              <textarea
                rows={3}
                value={justificacion}
                onChange={(e) => setJustificacion(e.target.value)}
              />
            </Campo>

            <div className="tw:flex tw:flex-wrap tw:items-center tw:gap-3">
              <Boton
                onClick={() => tramitar.mutate()}
                cargando={tramitar.isPending}
                disabled={justificacion.trim() === ''}
                icono={<Stamp />}
              >
                Encaminar a la máxima autoridad
              </Boton>

              <span className="tw:text-xs tw:text-tinta-mid">
                Se puede tramitar antes de programar la misión. <b>No se puede firmar así</b>:
                el permiso es nominativo.
              </span>
            </div>

            <Nota tono="neutro" icono={<Info />}>
              Si el vehículo tiene <b>excepción de servicio exceptuado</b> vigente —emergencia,
              seguridad, salud— no hace falta ningún permiso, y el sistema lo dirá al intentar
              tramitarlo. La excepción es atributo del vehículo, no del viaje.
            </Nota>
          </>
        )}
      </div>
    </Panel>
  );
}

const fecha = (d: string): string => new Date(`${d}T00:00:00`).toLocaleDateString('es-HN');

interface Permiso {
  id: string;
  folio: string;
  estado: 'Solicitado' | 'Firmado' | 'Desistido';
  destino: string;
  desde: string;
  hasta: string;
  justificacion: string;
  tramosInhabiles: string[];
  solicita: string;
  /** **Nulo es sin firmar, y sin firmar no ampara.** */
  firmadoPor: string | null;
  /** Lo que el permiso ampara, congelado al firmar. Un relevo posterior no lo cambia. */
  vehiculo: string | null;
  motorista: string | null;
  /** La referencia cruzada de `RN-04`. Nulo en el primero. */
  reemplaza: string | null;
  /** **Qué elemento cambió**, no sólo que cambió. Nulo es que sigue cubriendo. */
  porQueYaNoCubre: string | null;
  /** Falso es **espere**; verdadero es **actúe**. No todo lo que deja de cubrir se reemite. */
  exigeReemision: boolean;
  /** Firmado **y** todavía cubre. Las dos condiciones, y por eso van separadas. */
  ampara: boolean;
}

interface Apertura {
  /** Falso **no es un error**: es que no hacía falta. */
  abierto: boolean;
  motivo?: string;
  mensaje: string;
  tramosInhabiles?: string[];
}
