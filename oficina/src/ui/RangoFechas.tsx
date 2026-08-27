import { CalendarRange, ChevronLeft, ChevronRight } from 'lucide-react';
import { useEffect, useId, useMemo, useRef, useState } from 'react';
import type { ReactElement } from 'react';
import { usePosicionFlotante } from './posicionFlotante';

/**
 * Calendario de selección de **rango**. Dos meses a la vista.
 *
 * ── Por qué un solo control y no dos campos ──────────────────────────────────
 * Dos campos sueltos obligan a abrir el calendario dos veces y a razonar la duración de cabeza.
 * Acá el rango se pinta mientras se elige, así que «del 3 al 7» se ve como cinco días y no como
 * dos fechas que hay que restar. En una gira **la duración es el dato que se está decidiendo**:
 * es lo que multiplica la tarifa.
 *
 * ── Por qué DOS meses (2026-08-10) ──────────────────────────────────────────
 * Un calendario de un mes obliga a navegar para elegir un rango que cruza el fin de mes — y
 * cruzar el fin de mes es lo normal en una gira de una semana. Con los dos meses juntos el rango
 * se ve entero mientras se arma, sin perder de vista de dónde arrancó. Es lo que hacen los
 * buscadores de vuelos, y por la misma razón.
 *
 * En pantallas angostas se muestra uno solo: dos meses a 375 px no entran sin encoger los días
 * hasta hacerlos imposibles de tocar.
 *
 * ── Lo que NO se pierde respecto del campo nativo ───────────────────────────
 * El nativo trae gratis teclado, lector de pantalla y el selector del sistema en móvil, y
 * perder eso para ganar estética sería un mal negocio. Por eso los dos campos de texto **siguen
 * siendo** `type="date"` y editables a mano —quien prefiera teclear, teclea— y el calendario es
 * un agregado que se abre con el botón. Dentro del calendario las flechas mueven el día, Enter
 * elige y Esc cierra.
 *
 * ── Un solo día es un rango válido ──────────────────────────────────────────
 * Es el caso más común: ida y vuelta el mismo día. Se resuelve con dos clics sobre la misma
 * fecha, y también con uno solo — al elegir el inicio, el fin se iguala.
 *
 * ── El acotado (`min` / `max`) ──────────────────────────────────────────────
 * Una gira vive dentro de las fechas de su solicitud, y un destino dentro de las de su gira. Los
 * días fuera del rango permitido **se deshabilitan en el calendario**, no se dejan elegir para
 * que el servidor los rechace después. Es la regla de UI por capacidad aplicada a un campo.
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
  // `getDay()` da 0 el domingo; acá la semana empieza el lunes.
  const corrimiento = (primero.getDay() + 6) % 7;
  primero.setDate(primero.getDate() - corrimiento);
  return primero;
}

function sumarMes({ anio, mes }: { anio: number; mes: number }, delta: number): { anio: number; mes: number } {
  const d = new Date(anio, mes + delta, 1);
  return { anio: d.getFullYear(), mes: d.getMonth() };
}

/** Días calendario del rango, inclusivo. Cero si está incompleto o al revés. */
function contarDias(desde: string, hasta: string): number {
  const a = desdeIso(desde)?.getTime();
  const b = desdeIso(hasta)?.getTime();
  if (a === undefined || b === undefined || b < a) return 0;
  return Math.round((b - a) / 86_400_000) + 1;
}

export interface RangoFechasProps {
  readonly desde: string;
  readonly hasta: string;
  readonly onCambiar: (desde: string, hasta: string) => void;
  readonly etiqueta?: string;
  /** Primer día elegible (`yyyy-mm-dd`). Los anteriores quedan deshabilitados. */
  readonly min?: string;
  /** Último día elegible (`yyyy-mm-dd`). Los posteriores quedan deshabilitados. */
  readonly max?: string;
  readonly disabled?: boolean;
  /** Se muestra bajo el control cuando el rango está acotado, para decir por qué. */
  readonly ayuda?: string;
}

export default function RangoFechas({
  desde, hasta, onCambiar, etiqueta = 'Período', min, max, disabled = false, ayuda,
}: RangoFechasProps): ReactElement {
  const [abierto, setAbierto] = useState(false);
  /** Cuando hay un inicio elegido y falta el fin, el calendario previsualiza el rango. */
  const [anclaje, setAnclaje] = useState<string | null>(null);
  const [sobre, setSobre] = useState<string | null>(null);
  const [foco, setFoco] = useState<string>(desde);
  const caja = useRef<HTMLDivElement>(null);
  const panel = useRef<HTMLDivElement>(null);
  const posicion = usePosicionFlotante(abierto, caja, panel);
  const idGrilla = useId();

  const base = desdeIso(foco) ?? desdeIso(desde) ?? new Date();
  const [visto, setVisto] = useState({ anio: base.getFullYear(), mes: base.getMonth() });
  const segundo = sumarMes(visto, 1);

  useEffect(() => {
    function fuera(ev: MouseEvent): void {
      if (caja.current && !caja.current.contains(ev.target as Node)) {
        setAbierto(false);
        setAnclaje(null);
      }
    }
    document.addEventListener('mousedown', fuera);
    return () => { document.removeEventListener('mousedown', fuera); };
  }, []);

  // Al abrir, el calendario se posiciona sobre el mes del inicio elegido, no sobre hoy.
  useEffect(() => {
    if (!abierto) return;
    const d = desdeIso(desde) ?? desdeIso(min ?? '') ?? new Date();
    setVisto({ anio: d.getFullYear(), mes: d.getMonth() });
    setFoco(desde === '' ? aIso(d) : desde);
  }, [abierto, desde, min]);

  /** El rango que se está viendo: el confirmado, o el que se previsualiza al elegir. */
  const [ini, fin] = (() => {
    if (anclaje !== null) {
      const otro = sobre ?? anclaje;
      return anclaje <= otro ? [anclaje, otro] : [otro, anclaje];
    }
    return [desde, hasta];
  })();

  const dias = contarDias(ini, fin);

  function permitido(iso: string): boolean {
    if (min !== undefined && min !== '' && iso < min) return false;
    if (max !== undefined && max !== '' && iso > max) return false;
    return true;
  }

  function elegir(iso: string): void {
    if (!permitido(iso)) return;
    if (anclaje === null) {
      // Primer clic: fija el inicio y deja el fin igual, que es el caso de ida y vuelta.
      setAnclaje(iso);
      setSobre(iso);
      onCambiar(iso, iso);
      return;
    }
    const [a, b] = anclaje <= iso ? [anclaje, iso] : [iso, anclaje];
    onCambiar(a, b);
    setAnclaje(null);
    setSobre(null);
    setAbierto(false);
  }

  function mover(deltaDias: number): void {
    const d = desdeIso(foco) ?? new Date();
    d.setDate(d.getDate() + deltaDias);
    const iso = aIso(d);
    setFoco(iso);
    // El mes visible sólo se mueve si el foco se salió de los DOS que están a la vista.
    const m = { anio: d.getFullYear(), mes: d.getMonth() };
    const dentro = (m.anio === visto.anio && m.mes === visto.mes)
      || (m.anio === segundo.anio && m.mes === segundo.mes);
    if (!dentro) setVisto(m);
    if (anclaje !== null) setSobre(iso);
  }

  // `loki-fecha` apaga el ícono y el desplegable NATIVOS del navegador — regla D8. El campo
  // sigue siendo `type="date"`: se teclea igual, con el formato local. Lo que se evita es que
  // haya DOS calendarios, y que el primero en aparecer sea el de Chrome sin nuestro tema.
  const campo =
    'loki-foco loki-fecha tw:rounded-control tw:border tw:border-linea tw:bg-panel tw:px-2 tw:py-1.5 tw:text-cuerpo-2 tw:font-mono tw:tabular-nums tw:text-tinta-hi tw:disabled:opacity-60';

  return (
    <div ref={caja} className="tw:relative tw:flex tw:flex-col tw:gap-1">
      <div className="tw:flex tw:flex-wrap tw:items-center tw:gap-1.5">
        {/* Los campos nativos se quedan: quien prefiere teclear la fecha, la teclea. */}
        <input
          type="date"
          aria-label={`${etiqueta}: inicio`}
          value={desde}
          min={min}
          max={max}
          disabled={disabled}
          onChange={(e) => { onCambiar(e.target.value, e.target.value > hasta ? e.target.value : hasta); }}
          className={campo}
        />
        <span className="tw:text-ayuda tw:text-tinta-low">a</span>
        <input
          type="date"
          aria-label={`${etiqueta}: fin`}
          value={hasta}
          min={desde === '' ? min : desde}
          max={max}
          disabled={disabled}
          onChange={(e) => { onCambiar(desde, e.target.value); }}
          className={campo}
        />

        <button
          type="button"
          aria-label={abierto ? 'Cerrar el calendario' : 'Elegir el período en un calendario'}
          aria-expanded={abierto}
          disabled={disabled}
          onClick={() => { setAbierto((v) => !v); }}
          className="loki-foco tw:rounded-control tw:border tw:border-linea tw:px-2 tw:py-1.5 tw:text-tinta-low tw:hover:border-linea-activa tw:hover:text-tinta-hi tw:disabled:opacity-60"
        >
          <CalendarRange className="tw:size-4" aria-hidden="true" />
        </button>

        {/* La duración, al lado y no escondida: es lo que multiplica la tarifa. */}
        {dias > 0 && (
          <span className="tw:text-ayuda tw:text-tinta-low">
            <b className="tw:font-mono tw:tabular-nums tw:text-tinta-mid">{dias}</b>{' '}
            {dias === 1 ? 'día' : 'días'}
          </span>
        )}
      </div>

      {ayuda !== undefined && ayuda !== '' && (
        <span className="tw:text-ayuda tw:text-tinta-low">{ayuda}</span>
      )}

      {abierto && (
        <div
          ref={panel}
          // `fixed` y no `absolute`: dentro de un modal, un panel en el flujo hace aparecer la
          // barra de desplazamiento del diálogo y queda recortado. Ver `usePosicionFlotante`.
          style={posicion}
          className="loki-flotante tw:z-30 tw:rounded-panel tw:border tw:border-linea tw:bg-panel tw:p-3"
          onKeyDown={(e) => {
            if (e.key === 'Escape') {
              e.preventDefault();
              setAbierto(false);
              setAnclaje(null);
            } else if (e.key === 'ArrowLeft') { e.preventDefault(); mover(-1); }
            else if (e.key === 'ArrowRight') { e.preventDefault(); mover(1); }
            else if (e.key === 'ArrowUp') { e.preventDefault(); mover(-7); }
            else if (e.key === 'ArrowDown') { e.preventDefault(); mover(7); }
            else if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); elegir(foco); }
          }}
        >
          {/* Las flechas viven fuera de los meses: una por calendario las duplicaría sin
              agregar nada, y con dos meses la de «anterior» del segundo no significa nada. */}
          <div className="tw:mb-2 tw:flex tw:items-center tw:justify-between tw:gap-4">
            <button
              type="button"
              aria-label="Mes anterior"
              onClick={() => { setVisto((v) => sumarMes(v, -1)); }}
              className="loki-foco tw:rounded-control tw:p-1 tw:text-tinta-low tw:hover:text-tinta-hi"
            >
              <ChevronLeft className="tw:size-4" aria-hidden="true" />
            </button>

            <span aria-live="polite" className="tw:sr-only">
              {MESES[visto.mes]} {visto.anio} y {MESES[segundo.mes]} {segundo.anio}
            </span>

            <button
              type="button"
              aria-label="Mes siguiente"
              onClick={() => { setVisto((v) => sumarMes(v, 1)); }}
              className="loki-foco tw:rounded-control tw:p-1 tw:text-tinta-low tw:hover:text-tinta-hi"
            >
              <ChevronRight className="tw:size-4" aria-hidden="true" />
            </button>
          </div>

          <div role="grid" aria-label={etiqueta} id={idGrilla} className="tw:flex tw:gap-4">
            {/* El segundo mes se oculta en pantallas angostas: a 375 px no entra sin encoger
                los días hasta hacerlos imposibles de tocar. */}
            {[visto, segundo].map((m, indice) => (
              <Mes
                key={`${String(m.anio)}-${String(m.mes)}`}
                anio={m.anio}
                mes={m.mes}
                ini={ini}
                fin={fin}
                foco={foco}
                abierto={abierto}
                permitido={permitido}
                onSobre={(iso) => { if (anclaje !== null) setSobre(iso); }}
                onElegir={(iso) => { setFoco(iso); elegir(iso); }}
                className={indice === 1 ? 'tw:hidden tw:sm:block' : undefined}
              />
            ))}
          </div>

          <p className="tw:pt-2 tw:text-ayuda tw:text-tinta-low">
            {anclaje === null
              ? 'Elija el día de inicio. Un solo día vale: ida y vuelta.'
              : 'Ahora el día de regreso.'}
          </p>
        </div>
      )}
    </div>
  );
}

// ── Un mes de la grilla ───────────────────────────────────────────────────────

function Mes({ anio, mes, ini, fin, foco, abierto, permitido, onSobre, onElegir, className }: {
  readonly anio: number;
  readonly mes: number;
  readonly ini: string;
  readonly fin: string;
  readonly foco: string;
  readonly abierto: boolean;
  readonly permitido: (iso: string) => boolean;
  readonly onSobre: (iso: string) => void;
  readonly onElegir: (iso: string) => void;
  readonly className?: string;
}): ReactElement {
  const celdas = useMemo(() => {
    const arranque = inicioGrilla(anio, mes);
    // Seis semanas fijas: con menos, la grilla cambia de alto entre meses y el calendario
    // «salta» al navegar.
    return Array.from({ length: 42 }, (_, i) => {
      const d = new Date(arranque);
      d.setDate(arranque.getDate() + i);
      return d;
    });
  }, [anio, mes]);

  return (
    <div className={className}>
      <p className="tw:pb-1 tw:text-center tw:text-cuerpo-2 tw:font-semibold tw:text-tinta-hi">
        {MESES[mes]} {anio}
      </p>

      <div className="tw:grid tw:grid-cols-7 tw:text-center">
        {DIAS.map((d, i) => (
          <span key={`${d}${String(i)}`} className="tw:py-0.5 tw:text-ayuda tw:text-tinta-low">
            {d}
          </span>
        ))}
      </div>

      <div className="tw:grid tw:grid-cols-7">
        {celdas.map((d) => {
          const iso = aIso(d);
          const delMes = d.getMonth() === mes;
          const dentro = ini !== '' && iso >= ini && iso <= fin;
          const extremo = iso === ini || iso === fin;
          const habilitado = permitido(iso);

          return (
            <button
              key={iso}
              type="button"
              role="gridcell"
              tabIndex={iso === foco ? 0 : -1}
              aria-selected={dentro}
              disabled={!habilitado}
              ref={(el) => {
                // El foco sigue al día activo para que las flechas no se pierdan.
                if (el && abierto && iso === foco) el.focus({ preventScroll: true });
              }}
              onMouseEnter={() => { onSobre(iso); }}
              onClick={() => { onElegir(iso); }}
              className={[
                'loki-foco tw:size-8 tw:rounded-control tw:font-mono tw:text-ayuda tw:tabular-nums',
                !habilitado
                  ? 'tw:cursor-not-allowed tw:text-tinta-low tw:opacity-30'
                  : extremo
                    ? 'tw:bg-btn tw:font-semibold tw:text-btn-fg'
                    : dentro
                      ? 'tw:bg-info-bg tw:text-tinta-hi'
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
    </div>
  );
}
