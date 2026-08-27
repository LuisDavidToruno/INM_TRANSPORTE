/**
 * Qué contiene el adjunto, a efectos de **poder depurarlo**.
 *
 * Existe por `HB34-53`: la depuración de datos personales **alcanza a los adjuntos**, y
 * sin clasificación no hay forma de encontrar la foto de un manifiesto entre treinta mil
 * fotos de odómetro. Sin eso, el hábeas data no se puede atender.
 *
 * <b>No es un `enum`</b> porque `erasableSyntaxOnly` lo prohíbe: Node no puede borrar un
 * `enum` sin compilar. Un objeto constante da lo mismo y se ejecuta directo.
 */
export const ClasificacionDeContenido = {
  /** Odómetro, comprobante de combustible, ticket de peaje, estado del vehículo. */
  Operativo: 'OPERATIVO',
  /** Manifiesto, lista de pasajeros, cualquier imagen con una persona identificable. */
  DatoPersonal: 'DATO_PERSONAL',
} as const;

export type ClasificacionDeContenido =
  (typeof ClasificacionDeContenido)[keyof typeof ClasificacionDeContenido];

/**
 * Un archivo capturado en el dispositivo, esperando su turno para subir.
 *
 * <b>El binario no está aquí.</b> `ADR-004`: el archivo vive en el sistema de archivos y
 * lo que se registra es ruta, hash, tipo, tamaño y clasificación. La aritmética que lo
 * decide: ≈ 8 GB anuales de datos relacionales contra ≈ 30 GB de adjuntos.
 */
export interface AdjuntoPendiente {
  /** ULID generado en el dispositivo (`ADR-005`), igual que todo lo demás. */
  readonly idAdjunto: string;
  /** A qué hecho respalda. La foto sin su transición no prueba nada. */
  readonly idTransicion: string;
  readonly ruta: string;
  /**
   * `ADR-004`: es lo que permite detectar que un adjunto **fue sustituido o se
   * corrompió**, y lo que sostiene los paquetes de evidencia. Una foto de odómetro que
   * cambió entre la captura y la auditoría, sin que nadie lo note, es exactamente el
   * agujero que este campo cierra.
   */
  readonly hash: string;
  readonly tipo: string;
  readonly bytes: number;
  readonly clasificacion: ClasificacionDeContenido;
  /** Fecha del hecho, no de la subida. Es lo que responde «desde cuándo». */
  readonly capturadoEn: string;
}

export interface ResumenDeLaCola {
  readonly pendientes: number;
  /**
   * El más viejo sin subir. <b>Es el dato que importa</b>, no el conteo: tres adjuntos
   * de hace una hora es normal; tres de hace nueve días significa que este dispositivo
   * no está sincronizando y nadie se enteró.
   */
  readonly masAntiguo: string | null;
}

export interface OpcionesDeCola {
  /**
   * Cuánto espacio se le permite a los adjuntos pendientes.
   *
   * `RNF-03` pide **≥ 200 fotografías con compresión automática**. El presupuesto existe
   * para poder avisar **antes** de agotarlo, no para explicar después por qué no cupo.
   */
  readonly presupuestoBytes?: number;
}

/**
 * La cola de adjuntos diferidos.
 *
 * ── Por qué es una cola aparte del diario ────────────────────────────────────
 * `RN-43` punto 3: los adjuntos se sincronizan **sin bloquear el envío del registro
 * principal**. Una foto pesa dos órdenes de magnitud más que la transición que respalda;
 * si el hecho esperara a su foto, un motorista con señal intermitente no sincronizaría
 * nada — ni el odómetro, que ocupa cuarenta bytes y es lo que la conciliación necesita.
 *
 * ── Por qué el acuse es por adjunto y no por lote ────────────────────────────
 * Subir 200 fotos por la red de un retén no es una operación: son 200. Que cada una se
 * confirme por separado hace que una interrupción cueste **una foto**, no la sesión.
 *
 * ⚠️ Igual que el diario, esta implementación es en memoria. La persistencia va sobre el
 * mismo SQLite cifrado (`AlmacenSqlite` muestra el patrón), y **el archivo nunca entra a
 * la base**: solo su ruta y su hash.
 */
export class ColaDeAdjuntos {
  readonly #pendientes = new Map<string, AdjuntoPendiente>();
  readonly #presupuestoBytes: number | null;

  constructor(opciones: OpcionesDeCola = {}) {
    this.#presupuestoBytes = opciones.presupuestoBytes ?? null;
  }

  encolar(adjunto: AdjuntoPendiente): void {
    // Mismo criterio que el diario: el primero que quedó, queda (`RN-45`). Un reenvío
    // del mismo adjunto no lo sustituye — y con adjuntos importa más, porque sustituir
    // el archivo es justamente lo que el hash existe para detectar.
    if (this.#pendientes.has(adjunto.idAdjunto)) return;

    this.#pendientes.set(adjunto.idAdjunto, adjunto);
  }

  /** El servidor acusó **este** adjunto. Uno, no el lote. */
  confirmar(idAdjunto: string): void {
    this.#pendientes.delete(idAdjunto);
  }

  pendientes(): readonly AdjuntoPendiente[] {
    return [...this.#pendientes.values()];
  }

  /** Lo que `RN-43` obliga a mostrar en todo momento: cuántos, y desde cuándo. */
  resumen(): ResumenDeLaCola {
    const capturas = this.pendientes().map((a) => a.capturadoEn).sort();

    return {
      pendientes: this.#pendientes.size,
      masAntiguo: capturas[0] ?? null,
    };
  }

  /** Cuánto ocupan los adjuntos que todavía no subieron. */
  espacioComprometido(): number {
    return this.pendientes().reduce((total, a) => total + a.bytes, 0);
  }

  /**
   * Si conviene avisar ya.
   *
   * El umbral es el 80 % a propósito, y no el 95 %: el aviso tiene que llegar cuando
   * **todavía se puede hacer algo** —sincronizar en el próximo punto con señal, bajar la
   * calidad de captura—, no cuando ya no cabe la foto.
   *
   * Descubrirlo al pulsar el obturador en el sitio de un accidente es descubrirlo tarde:
   * esa foto es la evidencia y no se puede volver a tomar.
   */
  cercaDelLimite(): boolean {
    if (this.#presupuestoBytes === null) return false;

    return this.espacioComprometido() >= this.#presupuestoBytes * 0.8;
  }
}
