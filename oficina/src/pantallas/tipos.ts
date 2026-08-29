/**
 * El vocabulario del inventario de pantallas.
 *
 * `docs/04-diseno/inventario-de-pantallas.md` es la autoridad: acá sólo se le pone tipo a lo
 * que ese documento ya dice.
 */

/** Qué cliente la sirve. */
export type ClienteDePantalla =
  /** Sólo el administrativo — la oficina. */
  | 'A'
  /** Sólo el de campo, que hoy **no tiene interfaz**: `campo/` es núcleo y nada más. */
  | 'C'
  /** Dual: la misma pantalla en los dos clientes. */
  | 'A/C'
  /** Pública — la verificación por QR, sin sesión. */
  | 'P';

/**
 * Si replica un formato en papel.
 *
 * **Es lo que decide si se puede dibujar hoy.** El inventario: *«no se dibujan hasta tener el
 * formato. Dibujarlas antes es garantizar que hay que rehacerlas»*. Depende del insumo #2.
 */
export type Papel =
  /** Sin equivalente en papel: trabajo disponible desde el primer día. */
  | 'No'
  /** Replica un formato en papel que la institución todavía no entregó. */
  | 'Sí'
  /** Una sección replica papel: se hace la estructura y se deja ese bloque como marco vacío. */
  | 'Parc.';

/** Una fila del inventario, tal cual la publica el documento. */
export interface PantallaInventariada {
  /** `PT-001` a `PT-138`. **No se reciclan.** */
  readonly id: string;
  readonly nombre: string;
  readonly cliente: ClienteDePantalla;
  /** Los `ACT-xx` que la usan, o «todos». */
  readonly roles: string;
  /** Casos de uso que la originan. `—` cuando el inventario no la trazó. */
  readonly cu: string;
  /** Historias de usuario. `—` cuando el inventario no la trazó. */
  readonly hu: string;
  /** Si funciona con el dispositivo desconectado: `Sí`, `No` o `Deg.` (degradada). */
  readonly sinRed: string;
  readonly papel: Papel;
  /** La sección del documento donde vive — hace de módulo. */
  readonly seccion: string;
}

/** En qué situación está una pantalla **en el código**, que no es lo mismo que en el documento. */
export type SituacionDePantalla =
  /** Construida y en uso. */
  | 'construida'
  /** Construida en parte: la ruta existe y hay huecos declarados dentro. */
  | 'parcial'
  /** No empezada, y **el inventario dice que se puede empezar**. */
  | 'pendiente'
  /** No empezada porque falta el formato en papel — insumo #2. */
  | 'bloqueada'
  /** Es del cliente de campo, que todavía no tiene ninguna interfaz. */
  | 'campo';
