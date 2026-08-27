/**
 * DEFINICIÓN de los temas — fuente única.
 *
 * Este archivo es **datos puros**: sin React, sin DOM. Eso es a propósito. Lo que
 * habilita es que lo consuman varios lugares sin duplicarse — el runtime del
 * tema, el selector de apariencia, y (en un proyecto con backend) el endpoint
 * que valida qué tema está activo.
 *
 * ⇒ Agregar un tema son DOS ediciones y ninguna más: una entrada acá y un bloque
 *   de valores en `marca/<marca>/tokens.css`. Nada más se entera, y ése es el
 *   punto: si agregar un tema obligara a tocar un componente, el contrato está
 *   roto.
 *
 * ── Por qué el proveedor importa de acá en vez de declarar su propia lista ────
 * En ARGOS llegaron a convivir DOS catálogos llamados `TEMAS` y DOS hooks
 * llamados `useTema`. Quien escribía `useTema()` obtenía uno u otro según su
 * línea de `import`, y ninguno sabía que el otro existía: el shell podía estar
 * en claro mientras los componentes se pintaban oscuros. En pantalla se veía
 * como un tema incrustado dentro de otro. Una sola definición es lo que impide
 * que eso vuelva.
 */

export interface DefinicionTema {
  /** Etiqueta para la UI del selector. */
  readonly etiqueta: string;
  /** Familia visual: agrupa las variantes de una misma identidad. */
  readonly familia: string;
  /** Nombre de la familia para la UI (se repite entre sus variantes, a propósito). */
  readonly familiaEtiqueta: string;
  /**
   * Para qué situación es. Es lo que se muestra al elegir.
   *
   * NO dice de qué color es —eso lo dice la muestra— sino CUÁNDO conviene: quien
   * elige un tema no está eligiendo un color, está resolviendo un problema (poca
   * luz, una sala, una jornada larga leyendo). «Fondo claro» describe; esto
   * orienta.
   */
  readonly cuandoUsarlo: string;

  /**
   * Cuatro colores del tema para la muestra del selector: fondo, tinta, acento y
   * superficie.
   *
   * ⚠️ **Están duplicados respecto de `tokens.css`, y es a propósito.** Los
   * tokens se declaran sobre `:root[data-tema=…]`, así que no se pueden leer
   * desde un elemento anidado: pintar la muestra con los de verdad exigiría
   * cambiarle el tema a la página entera seis veces, una por muestra. La copia
   * se paga con un test que la fija contra el CSS, así que no puede desviarse en
   * silencio — pero se paga.
   */
  readonly muestra: readonly [string, string, string, string];

  /** Base del tema. Es lo que decide si el contenido va sobre claro u oscuro. */
  readonly base: 'claro' | 'oscuro';
}

/**
 * Los seis temas del contrato 0.3.3.
 *
 * `claro` y `oscuro` comparten familia porque son las dos caras de la identidad
 * principal. Los otros cuatro son identidades propias de un solo uso — cada uno
 * resuelve una situación concreta y no tiene contraparte.
 */
export const TEMAS = {
  claro: {
    etiqueta: 'Claro',
    familia: 'argos',
    familiaEtiqueta: 'Identidad ARGOS',
    base: 'claro',
    cuandoUsarlo: 'Oficina, pantalla iluminada',
    muestra: ['rgb(244 246 250)', 'rgb(13 23 37)', '#b8975b', 'rgb(255 255 255)'],
  },
  oscuro: {
    etiqueta: 'Oscuro',
    familia: 'argos',
    familiaEtiqueta: 'Identidad ARGOS',
    base: 'oscuro',
    cuandoUsarlo: 'Turnos largos, poca luz',
    muestra: ['rgb(15 17 26)', 'rgb(242 245 249)', '#cba869', 'rgb(23 27 38)'],
  },
  navy: {
    etiqueta: 'Navy institucional',
    familia: 'navy',
    familiaEtiqueta: 'Navy institucional',
    base: 'oscuro',
    cuandoUsarlo: 'Sala y presentación',
    muestra: ['rgb(13 28 51)', 'rgb(244 248 255)', '#cba869', 'rgb(19 41 73)'],
  },
  sepia: {
    etiqueta: 'Sepia',
    familia: 'sepia',
    familiaEtiqueta: 'Sepia',
    base: 'claro',
    cuandoUsarlo: 'Lectura prolongada de bitácoras',
    muestra: ['rgb(241 233 219)', 'rgb(36 29 18)', '#a8874b', 'rgb(251 247 239)'],
  },
  consola: {
    etiqueta: 'Consola',
    familia: 'consola',
    familiaEtiqueta: 'Consola',
    base: 'oscuro',
    cuandoUsarlo: 'Monitoreo · casi-negro y cian',
    muestra: ['rgb(6 8 12)', 'rgb(238 244 248)', '#5cc8f5', 'rgb(12 16 22)'],
  },
  gris: {
    etiqueta: 'Gris impresión',
    familia: 'gris',
    familiaEtiqueta: 'Gris impresión',
    base: 'claro',
    cuandoUsarlo: 'Destinado a PDF o fotocopia',
    muestra: ['rgb(245 245 245)', 'rgb(17 17 17)', '#6b6b6b', 'rgb(255 255 255)'],
  },
} as const satisfies Record<string, DefinicionTema>;

export type TemaId = keyof typeof TEMAS;

export const TEMA_POR_DEFECTO: TemaId = 'claro';

export const NOMBRES_TEMA = Object.keys(TEMAS) as TemaId[];

/** Densidades del contrato. Viven acá por la misma razón que los temas: fuente única. */
export type DensidadId = 'comoda' | 'compacta';

export const DENSIDADES: readonly DensidadId[] = ['comoda', 'compacta'];

export const DENSIDAD_POR_DEFECTO: DensidadId = 'comoda';

/** Alto de fila que declara cada densidad — lo muestra el selector de apariencia. */
export const ALTO_DENSIDAD: Record<DensidadId, string> = {
  comoda: '44px',
  compacta: '36px',
};
