import type { ReactElement } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { CircleAlert, FileWarning, History, ShieldCheck } from 'lucide-react';

import { Boton, Nota, Panel, Pastilla, Vacio } from '../../ui';
import { avisar } from '../../ui';
import { BloqueoDuro, pedir } from '../../api/misiones';
import { usarQuienEjecuta } from '../../app/puesto';

/**
 * `PT-100` — Puesta en vigencia de un parámetro normativo: <b>el doble control</b>.
 *
 * ── Por qué la pantalla muestra tanto ───────────────────────────────────────
 * Aprobar un parámetro normativo <b>es un acto de control interno</b>, y quien lo firma responde
 * por él ante el Tribunal Superior de Cuentas. Un botón «Aprobar» junto a un número suelto
 * convierte ese acto en un trámite: se aprueba lo que aparece, sin nada contra qué contrastarlo.
 *
 * El valor anterior es el que más se olvida y el que más importa. <b>Sin él no hay comparación</b>,
 * y «25 %» no dice si es un ajuste menor o si duplica la tolerancia.
 */
export default function Aprobaciones(): ReactElement {
  const quienEjecuta = usarQuienEjecuta();
  const cliente = useQueryClient();

  const { data, isPending, isError } = useQuery({
    queryKey: ['parametros-pendientes'],
    queryFn: () => pedir<Carga[]>('/parametros/pendientes'),
  });

  const aprobar = useMutation({
    mutationFn: (id: string) =>
      pedir<{ concedida: boolean; motivo: string | null }>(`/parametros/${id}/aprobar`, {
        method: 'POST',
        body: JSON.stringify({ ejecuta: quienEjecuta, momento: new Date().toISOString() }),
      }),
    onSuccess: async (r) => {
      // ⚠️ El rechazo llega con 200: **no es un error del sistema, es el control funcionando**,
      // y queda asentado en la bitácora igual. Tratarlo como falla lo escondería.
      if (r.concedida) avisar.exito('Parámetro en vigencia. Desde ahora se aplica en el cálculo.');
      else avisar.error(r.motivo ?? 'La aprobación no se concedió.');
      await cliente.invalidateQueries({ queryKey: ['parametros-pendientes'] });
      await cliente.invalidateQueries({ queryKey: ['salud'] });
    },
    onError: (e) =>
      avisar.error(e instanceof BloqueoDuro ? e.paraMostrar : 'No se pudo registrar el intento.'),
  });

  if (isError) {
    return (
      <Nota tono="riesgo" icono={<CircleAlert />}>
        No se pudieron cargar las cargas pendientes.
      </Nota>
    );
  }

  return (
    <div className="tw:flex tw:flex-col tw:gap-5">
      <header className="tw:flex tw:flex-col tw:gap-1">
        <h1 className="tw:text-xl tw:font-semibold tw:tracking-tight">
          Parámetros esperando entrar en vigencia
        </h1>
        <p className="tw:text-sm tw:text-tinta-mid">
          Cargados y <b>sin aplicarse todavía</b>. Mientras no se aprueben, el cálculo sigue
          usando el valor anterior — no el que está acá.
        </p>
      </header>

      {isPending ? (
        <p className="tw:text-sm tw:text-tinta-mid">Cargando…</p>
      ) : data.length === 0 ? (
        <Vacio
          icono={<ShieldCheck />}
          titulo="No hay nada esperando aprobación"
          descripcion="Toda carga registrada ya fue resuelta."
        />
      ) : (
        <div className="tw:flex tw:flex-col tw:gap-4">
          {data.map((c) => (
            <Panel key={c.id} titulo={c.clave}>
              <div className="tw:flex tw:flex-col tw:gap-4">
                {/* ── Lo que cambia ──────────────────────────────────────── */}
                <div className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-3 tw:gap-y-1">
                  {c.valorAnterior !== null ? (
                    <>
                      <span className="tw:font-mono tw:text-sm tw:text-tinta-mid tw:line-through">
                        {c.valorAnterior}
                      </span>
                      <span className="tw:text-tinta-mid">→</span>
                    </>
                  ) : null}
                  <span className="tw:font-mono tw:text-lg tw:font-semibold">{c.valor}</span>
                </div>

                {/* **Nulo no es un dato que falte**: es información distinta, y se dice
                    distinto. Mostrarlo como un guion lo haría parecer un hueco. */}
                {c.valorAnterior === null && (
                  <Nota tono="aviso">
                    <b>Es la primera carga de esta clave.</b> No hay valor anterior porque hasta
                    hoy el control estaba apagado: aprobar no cambia un cálculo, lo enciende.
                  </Nota>
                )}

                {/* ── El respaldo, y si de verdad está ───────────────────── */}
                {!c.respaldo.existe && (
                  <Nota tono="riesgo" icono={<FileWarning />}>
                    <b>El respaldo documental no está.</b> La carga declara una fuente y una fecha
                    de verificación —que son texto que alguien escribió— pero el documento
                    adjunto no existe. <b>No se puede aprobar</b>: firmarlo sería dar por
                    verificada una fuente que nadie vio. Devuélvalo al Administrador del Sistema.
                  </Nota>
                )}

                {/* ── El alcance ─────────────────────────────────────────── */}
                {c.alcance.esRetroactivo && (
                  <Nota tono="riesgo" icono={<History />}>
                    <b>La vigencia arranca en el pasado</b>, el {fecha(c.alcance.desde)}. Aprobar
                    no cambia sólo lo que viene: el cálculo usa la tabla vigente <b>a la fecha del
                    hecho</b>, así que este valor pasa a regir sobre hechos ya registrados.
                    {c.alcance.misionesAlcanzadas > 0 && (
                      <>
                        {' '}
                        Hay <b>{c.alcance.misionesAlcanzadas}</b>{' '}
                        {c.alcance.misionesAlcanzadas === 1 ? 'misión' : 'misiones'} con salida
                        dentro de esa ventana. Es <b>una cota superior, no una cuenta de
                        afectadas</b>: no toda misión usa este parámetro.
                      </>
                    )}
                  </Nota>
                )}

                <dl className="tw:grid tw:gap-x-6 tw:gap-y-2 tw:text-sm tw:sm:grid-cols-2">
                  <Dato rotulo="Rige desde">
                    {fecha(c.vigenteDesde)}
                    {c.vigenteHasta !== null && ` al ${fecha(c.vigenteHasta)}`}
                    {!c.alcance.esRetroactivo && (
                      <Pastilla tono="ok">sólo hacia adelante</Pastilla>
                    )}
                  </Dato>
                  <Dato rotulo="Fuente">{c.respaldo.fuente}</Dato>
                  <Dato rotulo="Verificada el">{fecha(c.respaldo.verificadoEl)}</Dato>
                  <Dato rotulo="Respaldo adjunto">
                    {c.respaldo.existe ? (
                      <span className="tw:font-mono tw:text-xs">{c.respaldo.adjunto}</span>
                    ) : (
                      <Pastilla tono="riesgo">no está</Pastilla>
                    )}
                  </Dato>
                  <Dato rotulo="Cargado por">{c.cargadoPor}</Dato>
                  <Dato rotulo="Cargado el">{fechaHora(c.cargadoEl)}</Dato>
                </dl>

                <div className="tw:flex tw:flex-wrap tw:items-center tw:gap-3">
                  <Boton
                    onClick={() => aprobar.mutate(c.id)}
                    cargando={aprobar.isPending && aprobar.variables === c.id}
                    disabled={!c.respaldo.existe}
                  >
                    Poner en vigencia
                  </Boton>

                  {/* El bloqueo se dice acá también. Un botón apagado sin explicación al lado
                      manda a buscar la razón, y la razón es lo único que se puede accionar. */}
                  <span className="tw:text-xs tw:text-tinta-mid">
                    {c.respaldo.existe
                      ? `Quien cargó (${c.cargadoPor}) no puede aprobar: el doble control exige dos personas distintas.`
                      : 'Bloqueado mientras falte el respaldo.'}
                  </span>
                </div>
              </div>
            </Panel>
          ))}
        </div>
      )}
    </div>
  );
}

function Dato({ rotulo, children }: { rotulo: string; children: React.ReactNode }): ReactElement {
  return (
    <div className="tw:flex tw:flex-col tw:gap-0.5">
      <dt className="tw:text-xs tw:text-tinta-mid">{rotulo}</dt>
      <dd className="tw:flex tw:flex-wrap tw:items-center tw:gap-2">{children}</dd>
    </div>
  );
}

const fecha = (d: string): string => new Date(`${d}T00:00:00`).toLocaleDateString('es-HN');
const fechaHora = (d: string): string => new Date(d).toLocaleString('es-HN');

interface Carga {
  id: string;
  clave: string;
  valor: string;
  /** **Nulo es la primera carga de la clave**, no un dato que falte. */
  valorAnterior: string | null;
  anteriorVigenteDesde: string | null;
  vigenteDesde: string;
  vigenteHasta: string | null;
  cargadoPor: string;
  cargadoEl: string;
  respaldo: {
    fuente: string;
    verificadoEl: string;
    adjunto: string;
    /** El identificador del adjunto **no es** el adjunto. Falso bloquea la aprobación. */
    existe: boolean;
  };
  alcance: {
    esRetroactivo: boolean;
    desde: string;
    hasta: string | null;
    /** **Cota superior, no cuenta de afectadas**: no toda misión usa todo parámetro. */
    misionesAlcanzadas: number;
  };
}
