import type {
  AnchorHTMLAttributes,
  ButtonHTMLAttributes,
  ReactElement,
  ReactNode,
} from 'react';

import Enlace from './Enlace';

import type { Tamano } from './tipos';

/**
 * Botón del contrato de diseño 0.3.3.
 * Canon: handoff-argos-0.3.2/COMPONENTS.md §2
 *
 * ── Las dos variantes destructivas NO son intercambiables ────────────────────
 * `peligro` es teñida: el «Anular» de una fila o de un panel. Convive con
 * acciones normales sin gritar.
 * `peligro-solido` es sólida y va SÓLO en la acción confirmante de un modal
 * destructivo. Si todo grita, nada grita — y el botón que anula seis
 * comprobantes no puede pesar lo mismo que el que guarda.
 *
 * `--btn-riesgo-*` NO se deriva de `--riesgo-*`: `--riesgo-bg` es fondo de
 * pastilla y da 3.68:1 con texto blanco.
 *
 * ── El deshabilitado NUNCA se oculta ─────────────────────────────────────────
 * Si el usuario debería saber que la acción existe, se muestra deshabilitada y
 * el `title` dice por qué. Ocultarla lo obliga a adivinar si le falta un permiso,
 * si la etapa no lo permite o si el sistema está roto.
 */

export interface BotonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variante?: 'primario' | 'secundario' | 'peligro' | 'peligro-solido' | 'fantasma';
  tamano?: Tamano;
  /** Rueda de 13 px dentro del botón. Deshabilita sin ocultar ni cambiar el ancho. */
  cargando?: boolean;
  /** Ícono de Lucide a 15 px. Va antes del texto. */
  icono?: ReactNode;
}

/**
 * Traducción literal de las reglas `.btn-*` de `referencia/argos5/ui.css`.
 *
 * Los valores NO se eligen acá: se leen de ahí. El relleno cambia por variante
 * (16 / 14 / 12 px) y el peso también (600 la primaria, 500 las demás) — dos
 * cosas que se pierden si uno «deduce» el botón desde la descripción en prosa,
 * como hice la primera vez.
 */
/**
 * ⚠️ **Cada variante trae su propio color de borde, y el base NINGUNO.**
 *
 * El base tenía `tw:border-transparent` y las variantes agregaban el suyo
 * (`tw:border-linea-campo`). Son dos utilidades de la MISMA propiedad y la misma
 * especificidad: no gana la que va después en el atributo `class`, gana la que
 * Tailwind emite después en la hoja — y era `border-transparent`. Efecto: el
 * botón **secundario y el `peligro` quedaban SIN BORDE en toda la aplicación**,
 * en silencio y sin error. Medido en el navegador: `border-color` resolvía a
 * `rgba(0,0,0,0)` con las dos clases puestas.
 *
 * Por eso el borde no se declara dos veces. Una utilidad de `border-color` por
 * botón, y el conflicto deja de existir.
 */
const VARIANTES: Record<NonNullable<BotonProps['variante']>, string> = {
  // .btn-primario — relleno 16px, peso 600 (los del `.btn` base)
  primario: 'tw:bg-btn tw:border-btn tw:text-btn-fg tw:hover:bg-btn-hover tw:hover:border-btn-hover',
  // .btn-secundario — peso 500 y relleno 14px, no los del base
  secundario:
    'tw:bg-panel tw:border-linea-campo tw:text-tinta-mid tw:font-medium tw:px-3.5 tw:hover:border-linea-activa tw:hover:bg-subtle tw:hover:text-tinta-base',
  // .btn-peligro — teñido: el «Anular» de una fila o un panel
  peligro: 'tw:bg-riesgo-bg tw:border-riesgo-bd tw:text-riesgo-fg tw:hover:border-riesgo-fg',
  // .btn-peligro-solido — sólo la acción confirmante de un modal destructivo
  'peligro-solido':
    'tw:bg-btn-riesgo tw:border-btn-riesgo tw:text-btn-riesgo-fg tw:hover:bg-btn-riesgo-hover tw:hover:border-btn-riesgo-hover',
  // .btn-fantasma — peso 500 y relleno 12px. El único sin borde visible, y por
  // eso el único que declara `border-transparent`.
  fantasma:
    'tw:border-transparent tw:text-tinta-mid tw:font-medium tw:px-3 tw:hover:bg-inset tw:hover:text-tinta-hi',
};

/**
 * Las clases del botón, en un solo lugar.
 *
 * Se extraen porque hay DOS elementos que tienen que verse idénticos y no pueden
 * ser el mismo: el botón (`<button>`, ejecuta algo acá) y el enlace-botón
 * (`<a href>`, lleva a otra parte). Renderizar un enlace como `<button>` con
 * `onClick` rompe abrir en pestaña nueva, copiar la dirección y el clic con el
 * botón del medio; renderizar un botón como `<a href="#">` deja un enlace que no
 * va a ningún lado. La solución es una sola: mismo estilo, distinta semántica.
 */
export function clasesBoton(
  variante: NonNullable<BotonProps['variante']> = 'secundario',
  tamano: Tamano = 'md',
  className = '',
): string {
  return [
    'loki-foco',
    tamano === 'md' ? 'loki-control' : 'loki-control-sm',
    // `.btn` de ui.css: gap 7px, relleno 16px, 13px/600. El ANCHO del borde va
    // acá; el COLOR lo pone la variante — ver la nota de VARIANTES.
    'tw:inline-flex tw:shrink-0 tw:items-center tw:justify-center tw:gap-[7px]',
    'tw:rounded-control tw:border tw:px-4',
    'tw:text-cuerpo tw:font-semibold tw:whitespace-nowrap tw:leading-none',
    'tw:transition-colors tw:duration-150 tw:ease-loki',
    // Deshabilitado: se ve, no se esconde. El `title` lleva el motivo.
    'tw:disabled:opacity-45 tw:disabled:cursor-not-allowed',
    VARIANTES[variante],
    className,
  ]
    .filter(Boolean)
    .join(' ');
}

export default function Boton({
  variante = 'secundario',
  tamano = 'md',
  cargando = false,
  icono,
  children,
  className = '',
  disabled,
  type = 'button',
  ...resto
}: BotonProps): ReactElement {
  // Cargando implica deshabilitado, pero NO al revés: un botón puede estar
  // deshabilitado por permiso sin que haya nada en curso.
  const inerte = disabled === true || cargando;

  return (
    <button
      type={type}
      disabled={inerte}
      aria-busy={cargando || undefined}
      className={clasesBoton(variante, tamano, className)}
      {...resto}
    >
      {cargando ? <Rueda /> : icono}
      {children}
    </button>
  );
}

export interface BotonIconoProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  /** Ícono de Lucide. 14 px, como el del kebab. */
  icono: ReactNode;
  /** Obligatorio: es el ÚNICO nombre accesible que tiene. Nombra la acción y su objeto. */
  etiqueta: string;
  /** `riesgo` tiñe el hover, no el reposo. Ver abajo. */
  tono?: 'neutro' | 'riesgo';
}

/**
 * Botón de sólo ícono para las acciones de una fila.
 *
 * ── Por qué no es un `Boton` sin texto ───────────────────────────────────────
 * El `Boton` reserva 16 px de relleno a cada lado para que el texto respire; sin
 * texto eso deja un cuadro de 46 px por acción, y dos de ésos en cada destino
 * pesan más que el dato de la fila. Ésta es la misma receta del disparador del
 * kebab (`MenuAcciones`), que ya resolvía el problema: cuadrado, `px-1.5`.
 *
 * ── El destructivo NO se tiñe en reposo ──────────────────────────────────────
 * Un `Boton variante="peligro"` pinta el fondo siempre. Con una lista de diez
 * destinos eso son diez manchas rojas compitiendo por la atención cuando no pasa
 * nada. El rojo aparece al apuntar —cuando la acción está por ocurrir— y en la
 * confirmación, que es donde de verdad hace falta.
 *
 * ── El rótulo es obligatorio ─────────────────────────────────────────────────
 * Sin texto visible, `aria-label` es el único nombre que tiene el control. Un
 * botón sin nombre se anuncia como «botón» y el usuario de lector de pantalla no
 * puede saber si borra o edita.
 */
export function BotonIcono({
  icono,
  etiqueta,
  tono = 'neutro',
  className = '',
  type = 'button',
  ...resto
}: BotonIconoProps): ReactElement {
  return (
    <button
      type={type}
      aria-label={etiqueta}
      // `title` además del `aria-label`: el lector usa uno, el ratón el otro.
      title={etiqueta}
      className={[
        'loki-foco loki-control-sm',
        'tw:flex tw:shrink-0 tw:items-center tw:justify-center',
        'tw:rounded-control tw:border tw:border-linea tw:px-1.5',
        'tw:transition-colors tw:duration-150 tw:ease-loki',
        'tw:disabled:opacity-45 tw:disabled:cursor-not-allowed',
        tono === 'riesgo'
          ? 'tw:text-tinta-mid tw:hover:border-riesgo-bd tw:hover:bg-riesgo-bg tw:hover:text-riesgo-fg'
          : 'tw:text-tinta-mid tw:hover:border-linea-activa tw:hover:text-tinta-hi',
        className,
      ]
        .filter(Boolean)
        .join(' ')}
      {...resto}
    >
      {icono}
    </button>
  );
}

export interface EnlaceBotonProps extends AnchorHTMLAttributes<HTMLAnchorElement> {
  href: string;
  variante?: BotonProps['variante'];
  tamano?: Tamano;
  icono?: ReactNode;
  children: ReactNode;
  /**
   * Ir a Razor aunque el destino esté portado — para las salidas deliberadas de una pantalla
   * que sí está acá. Ver el porqué extendido en `Enlace`.
   */
  externo?: boolean;
}

/**
 * Enlace con aspecto de botón. Para lo que NAVEGA: «Ver el mapa», «Ver todas»,
 * «Nueva solicitud». Sin subrayado, porque ya se lee como botón.
 *
 * **Delega en `Enlace` la decisión de interno vs Razor**, y eso no es un detalle: casi todos
 * sus `href` vienen del servidor (`urlExpediente`, `urlWizard`, `urlDetalle`) y un `<a>` pelado
 * hace **navegación completa aunque el destino ya esté portado** — se pierde el shell, el tema
 * y el estado, y el usuario ve un parpadeo a la aplicación vieja para volver a algo que ya
 * teníamos acá.
 *
 * ⚠️ **Esto se perdió una vez, en silencio.** El `EnlaceBoton` de `componentes/ui` sí delegaba;
 * al migrar las pantallas a este componente del contrato —que rendereaba `<a>` directo— los
 * enlaces dejaron de resolver sin que nada fallara: el botón seguía funcionando, sólo que
 * abandonando la aplicación. Lo vigila `npm run verificar-enlaces`.
 */
export function EnlaceBoton({
  href,
  variante = 'secundario',
  tamano = 'md',
  icono,
  children,
  className = '',
  ...resto
}: EnlaceBotonProps): ReactElement {
  return (
    <Enlace href={href} className={clasesBoton(variante, tamano, `tw:no-underline ${className}`)} {...resto}>
      {icono}
      {children}
    </Enlace>
  );
}

/**
 * 13 px, el tamaño que fija el contrato. Hereda el color del botón (`currentColor`)
 * para no necesitar una variante por cada variante de botón.
 *
 * `prefers-reduced-motion` la deja quieta: el contrato pide que las animaciones se
 * reduzcan a opacidad, y una rueda girando es justo lo que molesta a quien pidió
 * menos movimiento. Se sigue viendo que hay algo en curso porque el botón queda
 * inerte y `aria-busy` lo anuncia.
 */
function Rueda(): ReactElement {
  return (
    <svg
      className="loki-rueda tw:animate-spin tw:motion-reduce:animate-none"
      viewBox="0 0 16 16"
      fill="none"
      aria-hidden="true"
    >
      <circle cx="8" cy="8" r="6.5" stroke="currentColor" strokeOpacity="0.25" strokeWidth="2" />
      <path
        d="M14.5 8A6.5 6.5 0 0 0 8 1.5"
        stroke="currentColor"
        strokeWidth="2"
        strokeLinecap="round"
      />
    </svg>
  );
}
