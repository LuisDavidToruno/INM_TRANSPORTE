import type { ReactElement } from 'react';
import { useQuery } from '@tanstack/react-query';
import { CircleAlert, HeartPulse, TriangleAlert } from 'lucide-react';

import { Nota, Panel, Pastilla } from '../../ui';
import { pedir } from '../../api/misiones';

/**
 * `PT-101` — Panel de salud: <b>qué está mal y qué hacer</b>.
 *
 * ── Por qué hace falta un resumen ───────────────────────────────────────────
 * El sistema está lleno de controles que <b>se apagan solos</b> cuando falta su parámetro, y
 * cada pantalla lo declara donde aparece: el tablero de seguimiento dice que no puede degradar,
 * el control de folios dice que no habrá aviso previo, la depuración dice que no depura.
 *
 * Cada aviso por separado es correcto y <b>ninguno alcanza</b>: nadie recorre once pantallas
 * para saber qué le falta configurar. Sin este resumen, un control apagado se descubre el día
 * que hacía falta.
 *
 * ── Y por qué se muestra el respaldo ────────────────────────────────────────
 * <b>Un valor cargado y uno decidido se ven iguales en una casilla marcada.</b> Sin ver de dónde
 * salió, un valor puesto para poder probar el sistema pasa por una decisión de la institución —
 * y nadie vuelve a preguntar por él.
 */
export default function Salud(): ReactElement {
  const { data, isPending, isError } = useQuery({
    queryKey: ['salud'],
    queryFn: () => pedir<Reporte>('/salud'),
  });

  if (isError) {
    return (
      <Nota tono="riesgo" icono={<CircleAlert />}>
        No se pudo evaluar el estado del sistema.
      </Nota>
    );
  }

  const deProbar = (data?.controles ?? []).filter(
    (c) => c.configurado && esDePrueba(c.respaldo),
  );

  return (
    <div className="tw:flex tw:flex-col tw:gap-5">
      <header className="tw:flex tw:flex-col tw:gap-1">
        <h1 className="tw:text-xl tw:font-semibold tw:tracking-tight">
          Qué está mal y qué hacer
        </h1>
        <p className="tw:text-sm tw:text-tinta-mid">
          Los controles que <b>se apagan solos</b> mientras falte lo que la institución tiene que
          decidir. Cada uno dice qué deja de funcionar.
        </p>
      </header>

      {isPending ? (
        <p className="tw:text-sm tw:text-tinta-mid">Evaluando…</p>
      ) : (
        <>
          {data.sinConfigurar === 0 ? (
            <Nota tono="ok" icono={<HeartPulse />}>
              <b>Todo lo que este panel sabe mirar está configurado.</b> No significa que todo
              funcione: significa que no falta nada de esta lista.
            </Nota>
          ) : (
            <Nota tono="riesgo" icono={<TriangleAlert />}>
              <b>
                {data.sinConfigurar === 1
                  ? 'Hay 1 control apagado'
                  : `Hay ${data.sinConfigurar} controles apagados`}
              </b>
              . No están fallando: <b>no pueden funcionar</b> porque les falta algo que sólo la
              institución puede decidir.
            </Nota>
          )}

          {/* ⚠️ Lo que más engaña de un tablero verde. */}
          {deProbar.length > 0 && (
            <Nota tono="aviso">
              <b>
                {deProbar.length === 1
                  ? '1 valor está cargado con respaldo de prueba'
                  : `${deProbar.length} valores están cargados con respaldo de prueba`}
              </b>
              . Se ven configurados y <b>no son decisiones de la institución</b>: se pusieron para
              poder probar el sistema. Cuentan como pendientes aunque el control funcione.
            </Nota>
          )}

          <Panel titulo={`${data.controles.length} controles`}>
            <ul className="tw:flex tw:flex-col tw:gap-3">
              {data.controles.map((c) => (
                <li
                  key={c.clave}
                  className={`tw:flex tw:flex-col tw:gap-1 tw:border-l-2 tw:py-1 tw:pl-3 ${
                    !c.configurado
                      ? 'tw:border-riesgo-fg'
                      : esDePrueba(c.respaldo)
                        ? 'tw:border-aviso-fg'
                        : 'tw:border-ok-fg'
                  }`}
                >
                  <div className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-3 tw:text-sm">
                    <span className="tw:font-medium">{c.nombre}</span>

                    <Pastilla
                      tono={
                        !c.configurado
                          ? 'riesgo'
                          : esDePrueba(c.respaldo)
                            ? 'aviso'
                            : 'ok'
                      }
                    >
                      {!c.configurado
                        ? 'apagado'
                        : esDePrueba(c.respaldo)
                          ? 'valor de prueba'
                          : 'configurado'}
                    </Pastilla>

                    {c.insumo !== null && (
                      <span className="tw:font-mono tw:text-xs tw:text-tinta-mid">
                        insumo {c.insumo}
                      </span>
                    )}
                  </div>

                  {/* Lo que hace útil el panel: sin esto, una lista de claves sin valor no le
                      dice a nadie qué está en riesgo. */}
                  <span className="tw:text-sm tw:text-tinta-mid">{c.queSeApaga}</span>

                  {c.configurado ? (
                    <span className="tw:text-xs tw:text-tinta-mid">
                      <span className="tw:font-mono">{c.valor}</span>
                      {c.respaldo !== null && ` · respaldo: ${c.respaldo}`}
                    </span>
                  ) : (
                    <span className="tw:text-xs">
                      <b>Qué hacer:</b> {c.queHacer}
                    </span>
                  )}
                </li>
              ))}
            </ul>
          </Panel>
        </>
      )}
    </div>
  );
}

/**
 * Si el respaldo delata que el valor se puso para poder probar.
 *
 * Es una heurística sobre el texto del respaldo, y por eso <b>avisa en vez de bloquear</b>: un
 * respaldo legítimo que mencione la palabra «prueba» aparecería marcado, y eso es preferible a
 * que uno de prueba pase por decidido.
 */
function esDePrueba(respaldo: string | null): boolean {
  if (respaldo === null) return false;
  const r = respaldo.toLowerCase();
  return r.includes('prueba') || r.includes('ejemplo') || r.includes('provisional');
}

interface Reporte {
  /** **Cero no significa que todo funcione**: que no falta nada de esta lista. */
  sinConfigurar: number;
  controles: {
    clave: string;
    nombre: string;
    configurado: boolean;
    /** Nulo cuando no está configurado, **y ésa es la razón de que el control esté apagado**. */
    valor: string | null;
    queSeApaga: string;
    queHacer: string;
    /** Nulo no significa decidido: que no se levantó como insumo. */
    insumo: string | null;
    /** De dónde salió el valor. Un valor cargado y uno decidido se ven iguales sin esto. */
    respaldo: string | null;
  }[];
}
