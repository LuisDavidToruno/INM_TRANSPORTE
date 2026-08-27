/**
 * Vocabulario compartido del contrato de diseño 0.3.3.
 *
 * Canon: COMPONENTS.md §0 de la entrega 0.3.2 — vive en el repositorio de ARGOS,
 * no acá. Si no lo tenés a mano, lo que este archivo declara se sostiene solo.
 *
 * La regla que atraviesa todo: ningún componente recibe clases de color ni lee
 * el nombre del tema. Recibe `tono`, `variante` o `estado`, y el tema se
 * resuelve solo en CSS.
 */

/** Tono semántico. Es el ÚNICO vocabulario de color de estado del sistema. */
export type Tono = 'ok' | 'info' | 'aviso' | 'riesgo' | 'neutro';

/** Operación del dominio. Color de DATO, nunca de acción. */
export type Operacion = 'viatico' | 'liquidacion' | 'gira' | 'anulacion';

/** Plazo de atención. Sólo alimenta la marca de borde de fila. */
export type Plazo = 'aldia' | 'porvencer' | 'vencido';

export type Tamano = 'md' | 'sm';

export const ESTADO = {
  PENDIENTE: 1,
  APROBADO: 2,
  RECHAZADO: 3,
  ATENDIDA: 4,
  EJECUTADA: 5,
} as const;
export type EstadoId = (typeof ESTADO)[keyof typeof ESTADO];

export const ETAPA = {
  SOLICITANTE: 0,
  GERENTE: 1,
  REV_VIATICOS: 2,
  PRESUPUESTO: 3,
  LEGAL: 4,
  DIRECTOR: 5,
  PARALELO_6A: 6,
  CIERRE: 7,
} as const;
export type EtapaId = (typeof ETAPA)[keyof typeof ETAPA];

/**
 * DOS vocabularios a propósito.
 *
 * «Aprobado» internamente significa «autorizado para ejecutarse»; el solicitante
 * lo leería como resuelto cuando la gestión ni ha empezado. Por eso el mismo id
 * se rotula distinto según a quién se le muestra.
 */
export const ESTADO_INTERNO: Record<EstadoId, { texto: string; tono: Tono }> = {
  1: { texto: 'Pendiente', tono: 'aviso' },
  2: { texto: 'Asignada', tono: 'info' },
  3: { texto: 'Rechazada', tono: 'riesgo' },
  4: { texto: 'Finalizada', tono: 'ok' },
  5: { texto: 'Por aprobar', tono: 'aviso' },
};

export const ESTADO_CLIENTE: Record<EstadoId, { texto: string; tono: Tono }> = {
  1: { texto: 'Pendiente', tono: 'aviso' },
  2: { texto: 'En proceso', tono: 'info' },
  3: { texto: 'Rechazada', tono: 'riesgo' },
  4: { texto: 'Aprobada', tono: 'ok' },
  5: { texto: 'En proceso', tono: 'info' },
};

/**
 * 🚫 EL ESTADO SE DECIDE POR IDENTIFICADOR, NUNCA POR EL TEXTO.
 *
 * «Por aprobar» contiene «aprob». Cualquier comparación de cadenas
 * (`texto.includes('aprob')`) la pinta de aprobada — justo lo contrario de lo que
 * dice. El mapa por id es la única forma correcta, y vive en un solo archivo
 * para que no haya una segunda con la que discrepar.
 */

/**
 * Una clase por tono. Se resuelven en `estilos/index.css` con los tokens del
 * contrato — no con utilidades de Tailwind, porque dos de los quince nombres del
 * trío (`info-bg`, `neutro-bg`) colisionan con el vocabulario legacy y saldrían
 * con los colores viejos. Ver el comentario de `@layer components` allá.
 *
 * Es un MAPA y no una plantilla: Tailwind necesita ver la clase completa en el
 * código para generarla, así que `tw:bg-${tono}-bg` no produciría CSS.
 */
export const CLASE_TONO: Record<Tono, string> = {
  ok: 'tono-ok',
  info: 'tono-info',
  aviso: 'tono-aviso',
  riesgo: 'tono-riesgo',
  neutro: 'tono-neutro',
};

/**
 * El mismo tono, pero como **variable CSS** — para lo que no se pinta con una clase.
 *
 * ── Por qué hace falta además de `CLASE_TONO` ───────────────────────────────
 * Un gráfico no lleva clases: ECharts quiere un color y un trazo SVG quiere un `stroke`.
 * Eso se resolvía con una función `tokenDeTono` escrita **dos veces** —una en
 * `graficos/Sparkline` y otra en `pantallas/historial/Graficos`— y las dos apuntaban a los
 * tokens de la paleta anterior (`--loki-exito`). Dos copias de un puente entre vocabularios
 * es exactamente donde el gráfico y su leyenda terminan de distinto color.
 *
 * Se lee con `leerToken(TOKEN_TONO[tono])`. Devuelve el token de **primer plano**: es el que
 * tiene contraste contra el lienzo. Los de fondo (`--ok-bg`) son para superficies, y un
 * trazo pintado con ellos desaparece.
 *
 * ⚠️ **`oro` no está y no debe estar.** Es el acento de marca, no un tono de estado — misma
 * decisión que en el resto del contrato. La serie que lo usaba pasa a `neutro`.
 */
export const TOKEN_TONO: Record<Tono, string> = {
  ok: '--ok-fg',
  info: '--info-fg',
  aviso: '--aviso-fg',
  riesgo: '--riesgo-fg',
  neutro: '--neutro-fg',
};

/**
 * Definición de columna. La comparte la tabla real con su esqueleto: pasarle las
 * MISMAS columnas es lo que hace que los anchos y la alineación coincidan celda
 * por celda, en vez de aproximarse con un número de columnas.
 *
 * Canon: COMPONENTS.md §3.
 */
export interface ColumnaDef<T = unknown> {
  id: string;
  cabecera: string;
  /** → `font-mono tabular-nums text-right`. Sin las tres, la columna deja de comparar. */
  numerica?: boolean;
  ordenable?: boolean;
  ancho?: number;
  celda?(fila: T): import('react').ReactNode;
  /**
   * Por qué valor ordena la columna. **Sin esto, `ordenable` no hace nada** — la
   * celda es un árbol de React y no se puede comparar.
   *
   * Suele NO ser lo que se ve: la columna de plazo muestra «2d 5h» y ordena por el
   * porcentaje consumido, porque 13 de 16 h es más urgente que 20 de 40 y ordenar
   * por horas pondría la segunda arriba.
   */
  valorOrden?(fila: T): string | number;
}

/**
 * Ítem del riel, del lado del CONTRATO.
 *
 * ⚠️ No confundir con `ItemNav` de `api/tipos.ts`, que es el DTO del servidor
 * (`titulo`, `icono` como nombre, `url`, `badge`, `hijos`). Éste es lo que el
 * componente necesita, ya resuelto. La conversión entre uno y otro vive en el
 * adaptador, no dentro del riel: así el componente no sabe nada del backend y se
 * puede probar con datos a mano.
 *
 * Canon: COMPONENTS.md §3.
 */
export interface ItemNav {
  texto: string;
  icono: import('react').ReactNode;
  href: string;
  contador?: number;
  /** El contador va en `--nav-rail` sólo si hay algo pendiente de verdad. */
  accionable?: boolean;
  /**
   * Rótulo que marca que este destino SALE de la aplicación.
   *
   * Es deliberado, no un error: cuando un ítem del menú lleva a otro sistema —una
   * aplicación heredada, un portal externo— decirlo antes del clic es mejor que
   * que el usuario descubra el cambio de aspecto sin explicación y crea que algo
   * se rompió.
   *
   * El valor ES el texto que se muestra ("Sistema anterior", "Intranet"). Un
   * booleano obligaría a que todos los destinos externos se llamen igual.
   */
  externo?: string;
}

export interface GrupoNav {
  titulo?: string;
  items: ItemNav[];
}

/**
 * Un tramo de la barra de ubicación.
 *
 * Una cadena suelta es un tramo que **no lleva a ninguna parte**: un agrupador
 * que no es una pantalla —«Ejemplos»—, o el sitio donde ya estás. Con `href` es
 * un destino, y se dibuja como enlace.
 *
 * La distinción no es cosmética. Una miga que se ve como miga y no navega es la
 * unica pieza de la interfaz que miente sobre lo que hace: la convención dice
 * que los tramos anteriores llevan a donde dicen, y quien hace clic y obtiene
 * una selección de texto no concluye que la miga sea decorativa — concluye que
 * la aplicación se colgó.
 */
export type Miga = string | { readonly texto: string; readonly href: string };

/**
 * Un componente de ícono de Lucide, sin instanciar.
 *
 * ⚠️ No confundir con `contrato/Icono`, que es un COMPONENTE que dibuja un ícono
 * por nombre. Esto es el tipo de la referencia: `Save`, no `<Save />`.
 *
 * ── Cuándo usarlo, que es poco ──────────────────────────────────────────────
 * En el contrato los íconos viajan **ya como elemento** (`icono: ReactNode`), y
 * por una razón: quien llama decide el tamaño y el trazo, y el componente que lo
 * recibe no tiene que saber nada de la librería. Este tipo queda para los pocos
 * casos en que una pantalla arma un MAPA de íconos (estado → ícono) y lo
 * instancia después; ahí guardar el elemento sería crear uno por entrada aunque
 * no se dibuje.
 *
 * Venía de `componentes/ui`, que dejó de existir cuando su última pieza pasó al
 * contrato. Se conserva acá porque es vocabulario compartido, no un componente.
 */
export type IconoLucide = import('react').ComponentType<import('lucide-react').LucideProps>;
