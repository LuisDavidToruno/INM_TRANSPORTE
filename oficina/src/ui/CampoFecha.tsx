import { CalendarDays, ChevronLeft, ChevronRight } from 'lucide-react';
import { useEffect, useMemo, useRef, useState } from 'react';
import type { ReactElement } from 'react';
import { usePosicionFlotante } from './posicionFlotante';

/**
 * Una fecha suelta — la que **no** forma período.
 *
 * ── Cuándo se usa esto y cuándo `RangoFechas` ────────────────────────────────
 * Si el dato tiene un «hasta», es un período y va con `RangoFechas` (regla D8). Esto es para lo
 * otro: la fecha de una tasa de cambio, el corte de un reporte, el día de un pago. Ahí no hay
 * rango que mostrar y dos campos serían inventar uno.
 *
 * ── Por qué existe, si el nativo ya funcionaba ───────────────────────────────
 * Porque el nativo trae **su propio desplegable**, y ése es el del navegador: fondo claro,
 * tipografía de Chrome, ajeno a los seis temas del sistema. Se veía perfectamente al lado del
 * calendario de rango — uno con el tema y el otro sin él. La conclusión no fue tirar el campo
 * nativo: fue apagarle el desplegable y darle el mismo calendario que el rango.
 *
 * Así que acá se conserva todo lo que hacía valioso al nativo —tecleo, formato local, validación
 * del navegador, y el picker del sistema operativo en móvil, que se abre al tocar el campo— y se
 * reemplaza sólo la parte que desentonaba.
 *
 * ── Un mes, no dos ───────────────────────────────────────────────────────────
 * Para una fecha suelta no hay rango que cruce el fin de mes, así que el segundo mes sería
 * espacio ocupado sin nada que resolver.
 */

const DIAS = ['L', 'M', 'M', 'J', 'V', 'S', 'D'] as const;
const MESES = [
  'enero', 'febrero', 'marzo', 'abril', 'mayo', 'junio',
  'julio', 'agosto', 'septiembre', 'octubre', 'noviembre', 'diciembre',
] as const;

/** `yyyy-mm-dd` desde una fecha local. `toISOString` daría UTC y correría un día. */
function aIso(d: Date): string {
  const m = String(d.getMonth() + 1).padStart(2, '0');
  const dia = String(d.getDate()).padStart(2, '0');
  return `${String(d.getFullYear())}-${m}-${dia}`;
}

function desdeIso(s: string): Date | null {
  const d = new Date(`${s}T00:00:00`);
  return Number.isNaN(d.getTime()) ? null : d;
}

/** Lunes de la semana en que cae el día 1: con eso arranca la grilla. */
function inicioGrilla(anio: number, mes: number): Date {
  const primero = new Date(anio, mes, 1);
  const corrimiento = (primero.getDay() + 6) % 7;
  primero.setDate(primero.getDate() - corrimiento);
  return primero;
}

export interface CampoFechaProps {
  readonly valor: string;
  readonly onCambiar: (iso: string) => void;
  /** Para el lector de pantalla. El rótulo visible lo pone el `Campo` que lo envuelve. */
  readonly etiqueta?: string;
  readonly min?: string;
  readonly max?: string;
  readonly disabled?: boolean;
  readonly id?: string;
}

export default function CampoFecha({
  valor, onCambiar, etiqueta = 'Fecha', min, max, disabled = false, id,
}: CampoFechaProps): ReactElement {
  const [abierto, setAbierto] = useState(false);
  const [foco, setFoco] = useState(valor);
  const caja = useRef<HTMLDivElement>(null);
  const panel = useRef<HTMLDivElement>(null);
  const posicion = usePosicionFlotante(abierto, caja, panel);

  const base = desdeIso(valor) ?? new Date();
  const [visto, setVisto] = useState({ anio: base.getFullYear(), mes: base.getMonth() });

  useEffect(() => {
    function fuera(ev: MouseEvent): void {
      if (caja.current && !caja.current.contains(ev.target as Node)) setAbierto(false);
    }
    document.addEventListener('mousedown', fuera);
    return () => { document.removeEventListener('mousedown', fuera); };
  }, []);

  useEffect(() => {
    if (!abierto) return;
    const d = desdeIso(valor) ?? new Date();
    setVisto({ anio: d.getFullYear(), mes: d.getMonth() });
    setFoco(valor === '' ? aIso(d) : valor);
  }, [abierto, valor]);

  const celdas = useMemo(() => {
    const arranque = inicioGrilla(visto.anio, visto.mes);
    // Seis semanas fijas: con menos, la grilla cambia de alto entre meses y el calendario salta.
    return Array.from({ length: 42 }, (_, i) => {
      const d = new Date(arranque);
      d.setDate(arranque.getDate() + i);
      return d;
    });
  }, [visto]);

  function permitido(iso: string): boolean {
    if (min !== undefined && min !== '' && iso < min) return false;
    if (max !== undefined && max !== '' && iso > max) return false;
    return true;
  }

  function elegir(iso: string): void {
    if (!permitido(iso)) return;
    onCambiar(iso);
    setAbierto(false);
  }

  function mover(deltaDias: number): void {
    const d = desdeIso(foco) ?? new Date();
    d.setDate(d.getDate() + deltaDias);
    const iso = aIso(d);
    setFoco(iso);
    setVisto({ anio: d.getFullYear(), mes: d.getMonth() });
  }

  return (
    <div ref={caja} className="tw:relative tw:flex tw:items-center tw:gap-1.5">
      <input
        id={id}
        type="date"
        aria-label={etiqueta}
        value={valor}
        min={min}
        max={max}
        disabled={disabled}
        onChange={(e) => { onCambiar(e.target.value); }}
        // `loki-fecha` apaga el ícono y el desplegable nativos — regla D8.
        className="loki-foco loki-fecha tw:min-w-32 tw:rounded-control tw:border tw:border-linea tw:bg-panel tw:px-2 tw:py-1.5 tw:text-cuerpo-2 tw:font-mono tw:tabular-nums tw:text-tinta-hi tw:disabled:opacity-60"
      />

      <button
        type="button"
        aria-label={abierto ? 'Cerrar el calendario' : `Elegir ${etiqueta.toLowerCase()} en un calendario`}
        aria-expanded={abierto}
        disabled={disabled}
        onClick={() => { setAbierto((v) => !v); }}
        className="loki-foco tw:rounded-control tw:border tw:border-linea tw:px-2 tw:py-1.5 tw:text-tinta-low tw:hover:border-linea-activa tw:hover:text-tinta-hi tw:disabled:opacity-60"
      >
        <CalendarDays className="tw:size-4" aria-hidden="true" />
      </button>

      {abierto && (
        <div
          ref={panel}
          // `fixed` y no `absolute`: dentro de un modal, un panel en el flujo hace aparecer la
          // barra de desplazamiento del diálogo y queda recortado. Ver `usePosicionFlotante`.
          style={posicion}
          className="loki-flotante tw:z-30 tw:rounded-panel tw:border tw:border-linea tw:bg-panel tw:p-3"
          onKeyDown={(e) => {
            if (e.key === 'Escape') { e.preventDefault(); setAbierto(false); }
            else if (e.key === 'ArrowLeft') { e.preventDefault(); mover(-1); }
            else if (e.key === 'ArrowRight') { e.preventDefault(); mover(1); }
            else if (e.key === 'ArrowUp') { e.preventDefault(); mover(-7); }
            else if (e.key === 'ArrowDown') { e.preventDefault(); mover(7); }
            else if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); elegir(foco); }
          }}
        >
          <div className="tw:mb-2 tw:flex tw:items-center tw:justify-between tw:gap-3">
            <button
              type="button"
              aria-label="Mes anterior"
              onClick={() => { setVisto((v) => (v.mes === 0 ? { anio: v.anio - 1, mes: 11 } : { ...v, mes: v.mes - 1 })); }}
              className="loki-foco tw:rounded-control tw:p-1 tw:text-tinta-low tw:hover:text-tinta-hi"
            >
              <ChevronLeft className="tw:size-4" aria-hidden="true" />
            </button>
            <span aria-live="polite" className="tw:text-cuerpo-2 tw:font-semibold tw:text-tinta-hi">
              {MESES[visto.mes]} {visto.anio}
            </span>
            <button
              type="button"
              aria-label="Mes siguiente"
              onClick={() => { setVisto((v) => (v.mes === 11 ? { anio: v.anio + 1, mes: 0 } : { ...v, mes: v.mes + 1 })); }}
              className="loki-foco tw:rounded-control tw:p-1 tw:text-tinta-low tw:hover:text-tinta-hi"
            >
              <ChevronRight className="tw:size-4" aria-hidden="true" />
            </button>
          </div>

          <div className="tw:grid tw:grid-cols-7 tw:text-center">
            {DIAS.map((d, i) => (
              <span key={`${d}${String(i)}`} className="tw:py-0.5 tw:text-ayuda tw:text-tinta-low">
                {d}
              </span>
            ))}
          </div>

          <div role="grid" aria-label={etiqueta} className="tw:grid tw:grid-cols-7">
            {celdas.map((d) => {
              const iso = aIso(d);
              const delMes = d.getMonth() === visto.mes;
              const elegida = iso === valor;
              const habilitado = permitido(iso);
              return (
                <button
                  key={iso}
                  type="button"
                  role="gridcell"
                  tabIndex={iso === foco ? 0 : -1}
                  aria-selected={elegida}
                  disabled={!habilitado}
                  ref={(el) => {
                    if (el && abierto && iso === foco) el.focus({ preventScroll: true });
                  }}
                  onClick={() => { setFoco(iso); elegir(iso); }}
                  className={[
                    'loki-foco tw:size-8 tw:rounded-control tw:font-mono tw:text-ayuda tw:tabular-nums',
                    !habilitado
                      ? 'tw:cursor-not-allowed tw:text-tinta-low tw:opacity-30'
                      : elegida
                        ? 'tw:bg-btn tw:font-semibold tw:text-btn-fg'
                        : delMes
                          ? 'tw:text-tinta-mid tw:hover:bg-superficie-2'
                          : 'tw:text-tinta-low tw:opacity-50',
                  ].join(' ')}
                >
                  {d.getDate()}
                </button>
              );
            })}
          </div>

          <button
            type="button"
            onClick={() => { elegir(aIso(new Date())); }}
            className="loki-foco tw:mt-2 tw:w-full tw:rounded-control tw:py-1 tw:text-ayuda tw:text-tinta-low tw:hover:text-tinta-hi"
          >
            Hoy
          </button>
        </div>
      )}
    </div>
  );
}
