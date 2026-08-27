import type { CSSProperties, ReactElement, ReactNode } from 'react';

import type { Tono } from './tipos';

/**
 * Fila de indicadores. Traducida de `.kpis` / `.kpi` de ui.css.
 *
 * ── Es UN panel dividido, no cuatro tarjetas ─────────────────────────────────
 * Cuatro tarjetas separadas por aire se leen como cuatro cosas distintas. Acá son
 * cuatro caras del mismo dato —214 en proceso, de las cuales 31 vencen y 7 ya
 * vencieron— y el panel único lo dice sin escribirlo.
 *
 * ── Cada KPI es un FILTRO, no un cartel ──────────────────────────────────────
 * Tocar «Vencidas 7» filtra la tabla. El indicador que sólo informa obliga a
 * buscar después el filtro que le corresponde; éste ES el filtro. El
 * seleccionado se marca con una barra de 2 px abajo, en el acento.
 */

export interface KpiDato {
  id: string;
  rotulo: string;
  /** Punto de color al lado del rótulo: el semáforo del indicador. */
  tono?: Tono;
  valor: string;
  /** Variación destacada: «+12 hoy», «−3 vs. ayer». */
  delta?: { texto: string; tono: Tono };
  /** Contexto en gris: «≤ 24 h», «de 214». */
  nota?: string;
  /** Seis a doce valores, 0–100. Respaldo si no se pasa `grafico`. */
  serie?: number[];
  /**
   * El gráfico de verdad. Tiene prioridad sobre `serie`.
   *
   * Existe porque el canon del proyecto exige ECharts para toda gráfica, y el
   * paquete de diseño resuelve el sparkline con barras de CSS. En vez de elegir
   * uno y perder el otro, el componente acepta el gráfico ya construido: la
   * anatomía del KPI —rótulo, cifra, delta, nota— la sigue poniendo el contrato.
   */
  grafico?: ReactNode;
}

const PUNTO_TONO: Record<Tono, string> = {
  ok: 'tw:bg-ok-fg',
  info: 'tw:bg-info-fg',
  aviso: 'tw:bg-aviso-fg',
  riesgo: 'tw:bg-riesgo-fg',
  neutro: 'tw:bg-neutro-fg',
};

const TEXTO_TONO: Record<Tono, string> = {
  ok: 'tw:text-ok-fg',
  info: 'tw:text-info-fg',
  aviso: 'tw:text-aviso-fg',
  riesgo: 'tw:text-riesgo-fg',
  neutro: 'tw:text-neutro-fg',
};

export interface FilaKpisProps {
  kpis: KpiDato[];
  seleccionado?: string;
  onElegir?(id: string): void;

  /**
   * Cuántos por fila. **Sin esta prop la fila es de cuatro**, exactamente como
   * antes — la vitrina no cambia.
   *
   * Existe porque hay pantallas que cuentan más de cuatro cosas: el monitor de
   * liquidaciones tiene seis, y con la rejilla fija de cuatro la segunda fila
   * quedaba coja (dos celdas y dos huecos). Al pasarla, el panel se dibuja con
   * líneas de rejilla en vez de bordes por celda, para que las filas se separen
   * igual que las columnas.
   */
  columnas?: number;
}

export default function FilaKpis({
  kpis,
  seleccionado,
  onElegir,
  columnas,
}: FilaKpisProps): ReactElement {
  return (
    <div
      className={`loki-kpis${columnas !== undefined ? ' loki-kpis-rejilla' : ''}`}
      style={columnas !== undefined ? ({ '--kpi-cols': columnas } as CSSProperties) : undefined}
    >
      {kpis.map((k) => (
        <button
          key={k.id}
          type="button"
          onClick={() => onElegir?.(k.id)}
          data-sel={seleccionado === k.id ? '' : undefined}
          aria-pressed={seleccionado === k.id}
          className="loki-kpi loki-foco"
        >
          <span className="loki-kpi-lb">
            {k.tono !== undefined ? <i className={PUNTO_TONO[k.tono]} /> : null}
            {k.rotulo}
          </span>
          <span className="loki-kpi-v">{k.valor}</span>
          <span className="loki-kpi-d">
            <span className="tw:flex tw:flex-col tw:items-start tw:gap-1">
              {k.delta !== undefined ? (
                <em className={TEXTO_TONO[k.delta.tono]}>{k.delta.texto}</em>
              ) : null}
              {k.nota !== undefined ? <span>{k.nota}</span> : null}
            </span>
            {/* El gráfico REAL tiene prioridad sobre las barras de abajo.
                El canon del proyecto (DISENO-FRONTEND.md) manda ECharts para
                toda gráfica: «un sparkline artesanal hoy es un gráfico de barras
                artesanal mañana». Estas barras de CSS son el respaldo del
                paquete de diseño, y quedan para cuando no hay ECharts a mano —
                la vitrina, un correo, una vista sin el bundle de gráficos. */}
            {k.grafico ??
              (k.serie !== undefined ? (
                <span className="loki-spark" aria-hidden="true">
                  {k.serie.map((v, i) => (
                    <i
                      key={i}
                      style={{ height: `${Math.max(8, v)}%` }}
                      className={k.tono !== undefined ? PUNTO_TONO[k.tono] : 'tw:bg-neutro-fg'}
                    />
                  ))}
                </span>
              ) : null)}
          </span>
        </button>
      ))}
    </div>
  );
}
