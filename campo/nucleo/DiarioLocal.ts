/**
 * Una transición capturada en el dispositivo.
 *
 * <b>Es un hecho, no una intención.</b> Lo que el dispositivo manda al servidor son
 * estas — nunca «el estado de la misión» —, porque el principio `P-1` lo exige: dos
 * dispositivos no negocian estado, intercambian transiciones.
 */
export interface TransicionCapturada {
  /**
   * El identificador **lo genera el dispositivo** (`ADR-005`), y es lo que hace que
   * reenviar sea inofensivo: el servidor reconoce lo que ya tiene.
   */
  readonly idTransicion: string;
  readonly idExpediente: string;
  /** `T-14`, `T-18`… El catálogo lo manda la máquina de estados, no este archivo. */
  readonly transicion: string;
  readonly ejecuta: string;
  /**
   * La **fecha del hecho**, no la de captura (`P-4`, `RN-46`). En UTC con desfase
   * (`ADR-007`): el dispositivo puede estar en otra franja o con el reloj corrido, y
   * ese dato se audita aparte.
   */
  readonly ocurridoEn: string;
  readonly datos: Readonly<Record<string, unknown>>;
}

/**
 * Dónde vive el diario.
 *
 * Es un **puerto**, y existe porque el almacenamiento real —SQLite cifrado en el
 * dispositivo— no se puede ejecutar en cualquier máquina, pero la regla de qué se
 * captura y qué queda pendiente sí. Con esto la regla se prueba en milisegundos y el
 * almacén se cambia sin tocarla.
 */
export interface AlmacenDeDiario {
  guardar(transicion: TransicionCapturada): void;
  marcarConfirmadas(ids: readonly string[]): void;
  pendientes(): readonly TransicionCapturada[];
  total(): number;
}

/**
 * Almacén en memoria — para probar la regla, y para nada más.
 *
 * ⚠️ **Pierde todo cuando el proceso muere**, y Android mata procesos sin avisar en gama
 * baja, que es el equipo que `RNF-12` obliga a soportar. En el dispositivo va
 * `AlmacenSqlite` o su equivalente cifrado.
 */
export class AlmacenEnMemoria implements AlmacenDeDiario {
  readonly #transiciones = new Map<string, TransicionCapturada>();
  readonly #confirmadas = new Set<string>();

  guardar(transicion: TransicionCapturada): void {
    // `RN-45`: el primero que quedó, queda. Un reenvío no sobrescribe el hecho original.
    if (this.#transiciones.has(transicion.idTransicion)) return;

    this.#transiciones.set(transicion.idTransicion, transicion);
  }

  marcarConfirmadas(ids: readonly string[]): void {
    for (const id of ids) this.#confirmadas.add(id);
  }

  pendientes(): readonly TransicionCapturada[] {
    return [...this.#transiciones.values()].filter(
      (t) => !this.#confirmadas.has(t.idTransicion),
    );
  }

  total(): number {
    return this.#transiciones.size;
  }
}

/**
 * El diario de transiciones del dispositivo — la **fuente de verdad local**, no una
 * caché (`ADR-003`).
 *
 * La regla vive acá; **dónde se guarda es del almacén**. Esa separación es lo que permite
 * probar en cualquier máquina lo que en el dispositivo corre sobre **SQLite cifrado con
 * SQLCipher** (`ADR-002`, `ADR-003`).
 */
export class DiarioLocal {
  readonly #almacen: AlmacenDeDiario;

  constructor(almacen: AlmacenDeDiario = new AlmacenEnMemoria()) {
    this.#almacen = almacen;
  }

  registrar(transicion: TransicionCapturada): void {
    this.#almacen.guardar(transicion);
  }

  /** Lo que el servidor acusó. Lo que no venga en esta lista **sigue pendiente**. */
  confirmar(idsAcusados: readonly string[]): void {
    this.#almacen.marcarConfirmadas(idsAcusados);
  }

  pendientes(): readonly TransicionCapturada[] {
    return this.#almacen.pendientes();
  }

  /** Todo lo capturado, confirmado o no. El diario **no se vacía al sincronizar**. */
  total(): number {
    return this.#almacen.total();
  }
}
