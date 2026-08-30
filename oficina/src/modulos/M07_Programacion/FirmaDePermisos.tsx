import type { ReactElement } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { CalendarOff, CircleAlert, PenLine, ShieldCheck } from 'lucide-react';

import { Boton, Nota, Panel, Pastilla, Vacio, avisar } from '../../ui';
import { BloqueoDuro, pedir } from '../../api/misiones';
import { usarQuienEjecuta } from '../../app/puesto';

/**
 * `PT-021` — Firma del permiso de circulación en día u hora inhábil.
 *
 * ── Dos toques, y en un teléfono ────────────────────────────────────────────
 * Es criterio de `HU-016` y no es comodidad: si la pantalla no resuelve rápido y desde el
 * celular, <b>la máxima autoridad delega informalmente su clave un viernes a las seis de la
 * tarde</b> — que es exactamente el riesgo que el permiso existe para evitar. Por eso cada
 * trámite es una tarjeta con todo lo que hace falta decidir y un solo botón.
 *
 * ── Lo que se muestra es lo que hay que juzgar ──────────────────────────────
 * <b>La justificación primero.</b> Un vehículo, un destino y unas fechas no son una decisión:
 * firmar sobre eso es aprobar lo que aparece. Lo único que se puede juzgar es por qué esa
 * misión tiene que circular un domingo.
 *
 * ── ⚠️ Y por qué aparecen los que no se pueden firmar ───────────────────────
 * Un trámite abierto sobre una misión sin programar <b>no se puede firmar todavía</b>, y aun
 * así se lista: si se ocultara, quien firma vería una bandeja vacía y creería que no hay nada
 * pendiente. El trámite se descubriría el sábado.
 */
export default function FirmaDePermisos(): ReactElement {
  const quienEjecuta = usarQuienEjecuta();
  const cliente = useQueryClient();

  const { data, isPending, isError } = useQuery({
    queryKey: ['permisos-pendientes'],
    queryFn: () => pedir<Pendiente[]>('/permisos/pendientes'),
  });

  const firmar = useMutation({
    mutationFn: (id: string) =>
      pedir<{ concedida: boolean; motivo: string | null }>(`/permisos/${id}/firmar`, {
        method: 'POST',
        body: JSON.stringify({ ejecuta: quienEjecuta, momento: new Date().toISOString() }),
      }),
    onSuccess: async (r) => {
      // El rechazo llega con 200: **no es un error del sistema, es el control funcionando**, y
      // el intento queda asentado igual. Tratarlo como falla lo escondería.
      if (r.concedida) avisar.exito('Permiso firmado. La misión ya puede despacharse.');
      else avisar.error(r.motivo ?? 'La firma no se concedió.');
      await cliente.invalidateQueries({ queryKey: ['permisos-pendientes'] });
    },
    onError: (e) =>
      avisar.error(e instanceof BloqueoDuro ? e.paraMostrar : 'No se pudo registrar la firma.'),
  });

  if (isError) {
    return (
      <Nota tono="riesgo" icono={<CircleAlert />}>
        No se pudieron cargar los permisos pendientes de firma.
      </Nota>
    );
  }

  return (
    <div className="tw:flex tw:flex-col tw:gap-5">
      <header className="tw:flex tw:flex-col tw:gap-1">
        <h1 className="tw:text-xl tw:font-semibold tw:tracking-tight">
          Permisos esperando su firma
        </h1>
        <p className="tw:text-sm tw:text-tinta-mid">
          Misiones que <b>circulan en día u hora inhábil</b>. Sin esta firma no pueden salir:
          un vehículo del Estado sin permiso expone a la institución a un operativo del Tribunal
          Superior de Cuentas.
        </p>
      </header>

      {isPending ? (
        <p className="tw:text-sm tw:text-tinta-mid">Cargando…</p>
      ) : data.length === 0 ? (
        <Vacio
          icono={<ShieldCheck />}
          titulo="No hay permisos esperando firma"
          descripcion="Ninguna misión pendiente circula en franja inhábil."
        />
      ) : (
        <div className="tw:flex tw:flex-col tw:gap-4">
          {data.map((p) => (
            <Panel key={p.id} titulo={p.folioDeLaMision ?? p.folio}>
              <div className="tw:flex tw:flex-col tw:gap-4">
                {/* ── Lo único que hay que juzgar, y va primero ──────────── */}
                <p className="tw:text-base">{p.justificacion}</p>

                {/* ── Contra qué franja ──────────────────────────────────── */}
                <Nota tono="aviso" icono={<CalendarOff />}>
                  <b>
                    {p.tramosInhabiles.length === 1
                      ? 'Circula 1 tramo inhábil'
                      : `Circula ${p.tramosInhabiles.length} tramos inhábiles`}
                  </b>
                  : {p.tramosInhabiles.join(' · ')}
                </Nota>

                <dl className="tw:grid tw:gap-x-6 tw:gap-y-2 tw:text-sm tw:sm:grid-cols-2">
                  <Dato rotulo="Destino">{p.destino}</Dato>
                  <Dato rotulo="Ventana">
                    {fecha(p.desde)} al {fecha(p.hasta)}
                  </Dato>

                  {/* **Nulo es que la misión no está programada**, y ésa es la razón de que
                      no se pueda firmar — no un dato que falte mostrar. */}
                  <Dato rotulo="Vehículo">
                    {p.vehiculo ?? <Pastilla tono="riesgo">sin asignar</Pastilla>}
                  </Dato>
                  <Dato rotulo="Motorista">
                    {p.motorista ?? <Pastilla tono="riesgo">sin asignar</Pastilla>}
                  </Dato>

                  <Dato rotulo="Dependencia">{p.dependencia}</Dato>
                  <Dato rotulo="Solicitado por">{p.solicita}</Dato>
                </dl>

                {/* ── El bloqueo, con su razón al lado ───────────────────── */}
                {p.porQueNoSeFirma !== null && (
                  <Nota tono="riesgo">{p.porQueNoSeFirma}</Nota>
                )}

                <div>
                  <Boton
                    onClick={() => firmar.mutate(p.id)}
                    cargando={firmar.isPending && firmar.variables === p.id}
                    disabled={p.porQueNoSeFirma !== null}
                    icono={<PenLine />}
                  >
                    Firmar el permiso
                  </Boton>
                </div>
              </div>
            </Panel>
          ))}
        </div>
      )}
    </div>
  );
}

function Dato({
  rotulo,
  children,
}: {
  rotulo: string;
  children: React.ReactNode;
}): ReactElement {
  return (
    <div className="tw:flex tw:flex-col tw:gap-0.5">
      <dt className="tw:text-xs tw:text-tinta-mid">{rotulo}</dt>
      <dd className="tw:flex tw:flex-wrap tw:items-center tw:gap-2">{children}</dd>
    </div>
  );
}

const fecha = (d: string): string => new Date(`${d}T00:00:00`).toLocaleDateString('es-HN');

interface Pendiente {
  id: string;
  folio: string;
  estado: string;
  /** Nulo cuando la misión todavía no consumió su folio oficial. */
  folioDeLaMision: string | null;
  dependencia: string;
  objetoDelTraslado: string;
  destino: string;
  desde: string;
  hasta: string;
  /** **Lo único que hay para decidir.** Sin esto, firmar es aprobar lo que aparece. */
  justificacion: string;
  /** Los días y franjas que el permiso viene a cubrir. Van en el papel que lee el agente. */
  tramosInhabiles: string[];
  /** **Nulo es que la misión no está programada**, no un dato que falte. */
  vehiculo: string | null;
  motorista: string | null;
  solicita: string;
  /** Nulo es que sí se puede firmar. La regla vive en el dominio, no acá. */
  porQueNoSeFirma: string | null;
}
