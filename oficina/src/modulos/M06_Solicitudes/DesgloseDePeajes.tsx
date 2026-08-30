import type { ReactElement } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Milestone } from 'lucide-react';

import { Nota, Panel } from '../../ui';
import { pedir } from '../../api/misiones';

/**
 * `PT-009` — Estimado de peajes <b>desglosado por punto</b>.
 *
 * ── `R-8`: todo total tiene su desglose a un toque ──────────────────────────
 * <i>«Un total opaco no se puede autorizar ni conciliar.»</i> Quien autoriza tiene que poder
 * ver de dónde sale la cifra sin salir de la pantalla — una jefatura que tiene que navegar para
 * verlo <b>autoriza sin verlo</b>.
 *
 * ── Lo parcial se dice, y esa es la parte que importa ───────────────────────
 * Un total parcial presentado como completo subestima el costo, y eso <b>se paga en efectivo
 * faltante a mitad de camino</b>: el motorista llega a una caseta con menos de lo que necesita.
 * Por eso las líneas sin valorar se cuentan aparte y nunca se suman como cero.
 */
export default function DesgloseDePeajes({
  mision,
}: {
  readonly mision: string;
}): ReactElement {
  const { data, isPending, isError } = useQuery({
    queryKey: ['peajes', mision],
    queryFn: () => pedir<Desglose>(`/misiones/${mision}/peajes`),
  });

  if (isError || isPending) {
    return (
      <Panel titulo="Peajes estimados">
        <p className="tw:text-sm tw:text-tinta-mid">
          {isError ? 'No se pudo cargar el estimado.' : 'Cargando…'}
        </p>
      </Panel>
    );
  }

  return (
    <Panel titulo="Peajes estimados">
      {data.total === null ? (
        <p className="tw:text-sm tw:text-tinta-mid">
          <b>Todavía no hay estimado congelado.</b> El paquete de peajes se congela al programar
          la misión — y eso <b>no es lo mismo que un estimado de cero</b>, que diría que la ruta
          no tiene casetas.
        </p>
      ) : (
        <div className="tw:flex tw:flex-col tw:gap-3">
          <div className="tw:flex tw:items-baseline tw:gap-2">
            <span className="tw:text-2xl tw:font-semibold tw:tabular-nums">
              L {data.total.toFixed(2)}
            </span>
            {data.parcial && (
              <span className="tw:text-sm tw:text-aviso-fg">total parcial</span>
            )}
          </div>

          {data.parcial && (
            <Nota tono="aviso" icono={<Milestone />}>
              <b>
                {data.sinValorar === 1
                  ? '1 punto no se pudo valorar'
                  : `${data.sinValorar} puntos no se pudieron valorar`}
              </b>{' '}
              — sin tarifa cargada, o sin categoría de peaje resuelta para el vehículo. El total
              de arriba <b>no incluye esos puntos</b>, así que el costo real es mayor. Salir con
              esa cifra deja al motorista corto en una caseta.
            </Nota>
          )}

          <ul className="tw:flex tw:flex-col tw:gap-1.5">
            {data.lineas.map((l) => (
              <li
                key={l.punto}
                className="tw:flex tw:flex-wrap tw:items-baseline tw:justify-between tw:gap-x-3 tw:border-l-2 tw:border-linea tw:pl-3 tw:text-sm"
              >
                <span>
                  {/* Nulo cuando el punto ya no está en el catálogo: se muestra su
                      identificador en vez de un guion, para poder rastrearlo. */}
                  {l.nombre ?? (
                    <span className="tw:font-mono tw:text-xs">{l.punto}</span>
                  )}
                  {l.cruces > 1 && (
                    <span className="tw:text-tinta-mid"> · {l.cruces} cruces</span>
                  )}
                </span>

                {/* Nulo NO se dibuja como «L 0.00». Un cero diría que este punto no cuesta. */}
                <span
                  className={`tw:tabular-nums ${
                    l.subtotal === null ? 'tw:text-aviso-fg' : ''
                  }`}
                >
                  {l.subtotal === null ? 'sin valorar' : `L ${l.subtotal.toFixed(2)}`}
                </span>
              </li>
            ))}
          </ul>

          <span className="tw:text-xs tw:text-tinta-mid">
            Es el estimado <b>congelado</b> al programar, no uno recalculado: si una tarifa
            cambió después, el número que se autorizó sigue siendo éste.
          </span>
        </div>
      )}
    </Panel>
  );
}

interface Desglose {
  /** **Nulo cuando no hay estimado congelado.** Distinto de cero. */
  total: number | null;
  parcial: boolean;
  sinValorar: number;
  lineas: {
    punto: string;
    nombre: string | null;
    cruces: number;
    /** Nulo es «no se pudo valorar», nunca cero. */
    subtotal: number | null;
  }[];
}
