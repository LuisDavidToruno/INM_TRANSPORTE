import type { ReactElement } from 'react';
import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Info, ShieldCheck, Stamp } from 'lucide-react';

import { Boton, Campo, Nota, Panel, Pastilla, avisar } from '../../ui';
import { BloqueoDuro, pedir } from '../../api/misiones';
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
  const [justificacion, setJustificacion] = useState('');

  const { data, isPending, isError } = useQuery({
    queryKey: ['permisos', mision],
    queryFn: () => pedir<Permiso[]>(`/misiones/${mision}/permisos`),
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

                  {/* **Solicitado NO ampara.** Si se leyera como «hay permiso», alguien
                      creería que la misión puede salir y se enteraría el sábado. */}
                  <Pastilla tono={p.ampara ? 'ok' : 'aviso'}>
                    {p.ampara ? 'firmado, ampara' : 'esperando firma, no ampara'}
                  </Pastilla>
                </div>

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

        {ampara ? (
          <Nota tono="ok" icono={<ShieldCheck />}>
            La misión tiene permiso firmado. <b>Ampara este vehículo, este motorista, este
            destino y esta ventana</b> — un relevo de motorista lo invalida y obliga a
            reemitirlo.
          </Nota>
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
  /** Lo que `BD-04` mira. Sólo los firmados. */
  ampara: boolean;
}

interface Apertura {
  /** Falso **no es un error**: es que no hacía falta. */
  abierto: boolean;
  motivo?: string;
  mensaje: string;
  tramosInhabiles?: string[];
}
