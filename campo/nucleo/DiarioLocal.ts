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
 * El diario de transiciones del dispositivo — la **fuente de verdad local**, no una
 * caché (`ADR-003`).
 *
 * ⚠️ Esta implementación guarda en memoria. La persistencia real es **SQLite cifrado
 * con SQLCipher** (`ADR-002`, `ADR-003`), y vive en el módulo nativo del cliente
 * Android. Se separa a propósito: la **regla** de qué se captura, qué queda pendiente y
 * qué se confirma es la misma con o sin disco, y así se puede probar sin dispositivo.
 */
export class DiarioLocal {
  /**
   * Indexado por identificador, no una lista.
   *
   * Es lo que hace **idempotente** el registro: el mismo hecho reenviado se reconoce en
   * vez de duplicarse. El identificador nace en el dispositivo (`ADR-005`) y no cambia,
   * así que sirve de identidad aunque el servidor nunca lo haya visto.
   */
  readonly #transiciones = new Map<string, TransicionCapturada>();

  /**
   * Lo que el servidor ya acusó.
   *
   * Se guarda **aparte del registro**, y por eso una confirmación que llega después de
   * que el hecho se recapturó no lo pierde ni lo revive.
   */
  readonly #confirmadas = new Set<string>();

  registrar(transicion: TransicionCapturada): void {
    // Un hecho ya capturado no se sobrescribe con su reenvío: `RN-45` prohíbe la
    // sobrescritura silenciosa, y aquí ni siquiera hace falta decidir — es el mismo
    // hecho, con el mismo identificador, y el primero ya quedó.
    if (this.#transiciones.has(transicion.idTransicion)) return;

    this.#transiciones.set(transicion.idTransicion, transicion);
  }

  /** Lo que el servidor acusó. Lo que no venga en esta lista **sigue pendiente**. */
  confirmar(idsAcusados: readonly string[]): void {
    for (const id of idsAcusados) this.#confirmadas.add(id);
  }

  pendientes(): readonly TransicionCapturada[] {
    return [...this.#transiciones.values()].filter(
      (t) => !this.#confirmadas.has(t.idTransicion),
    );
  }

  /** Todo lo capturado, confirmado o no. El diario **no se vacía al sincronizar**. */
  total(): number {
    return this.#transiciones.size;
  }
}
