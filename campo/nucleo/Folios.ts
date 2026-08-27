/**
 * Se acabaron los folios del dispositivo.
 *
 * Es un error de **abastecimiento**, no de programación: alguien tenía que recargar el
 * subrango antes de que el dispositivo saliera a zona sin cobertura. Por eso lleva el
 * dispositivo y el prefijo — quien lo lea en un registro necesita saber a qué recargar.
 */
export class SinFoliosDisponibles extends Error {
  readonly idDispositivo: string;
  readonly prefijo: string;

  constructor(idDispositivo: string, prefijo: string) {
    super(
      `El dispositivo ${idDispositivo} agotó su subrango de folios ${prefijo}. ` +
        'No puede emitir documentos hasta que se le asigne uno nuevo, y sin documento ' +
        'el vehículo no sale.',
    );
    this.name = 'SinFoliosDisponibles';
    this.idDispositivo = idDispositivo;
    this.prefijo = prefijo;
  }
}

export interface DefinicionDeSubrango {
  /**
   * <b>El dispositivo, no la delegación.</b> Es la corrección de `HB34-52`: dos
   * dispositivos de la misma delegación, ambos sin red, con el mismo rango descargado,
   * tomarían el mismo número. El rango se asigna en dos niveles y este es el segundo.
   */
  readonly idDispositivo: string;
  /** `OM-2026`, `SC-2026`… Distingue tipo de documento y ejercicio. */
  readonly prefijo: string;
  readonly desde: number;
  readonly hasta: number;
}

/**
 * El subrango de folios que porta **un dispositivo concreto**.
 *
 * ── Qué problema resuelve, y por qué es crítico ──────────────────────────────
 * `RN-44` exige emitir documentos con folio **antes de salir**, sin consultar al
 * servidor: si el folio lo asigna el servidor, no hay documento imprimible en zona sin
 * cobertura, y todo `RNF-03` cae.
 *
 * Pero un rango por **delegación** no alcanza. `HB34-52`, clasificado crítico: dos
 * dispositivos de la misma delegación sin red toman el mismo número. Y el folio es la
 * unidad de trazabilidad del combustible (`RN-27`) — dos folios iguales **destruyen la
 * conciliación**, y el daño aparece semanas después.
 *
 * ⚠️ Igual que el `DiarioLocal`, el saldo vive **en memoria** en esta implementación. La
 * persistencia real es SQLite cifrado en el dispositivo. Lo que se prueba aquí es la
 * regla de consumo, que es la misma con o sin disco.
 */
export class SubrangoDeFolios {
  readonly #definicion: DefinicionDeSubrango;
  #siguiente: number;
  readonly #anulados: string[] = [];

  constructor(definicion: DefinicionDeSubrango) {
    this.#definicion = definicion;
    this.#siguiente = definicion.desde;
  }

  /**
   * Toma el siguiente folio. <b>Avanza siempre</b>, y por eso no hay forma de reciclar
   * uno: la anulación no devuelve el número al saldo (`RN-44`).
   */
  consumir(): string {
    if (this.#siguiente > this.#definicion.hasta) {
      throw new SinFoliosDisponibles(
        this.#definicion.idDispositivo,
        this.#definicion.prefijo,
      );
    }

    const numero = this.#siguiente;
    this.#siguiente += 1;

    return `${this.#definicion.prefijo}-${String(numero).padStart(6, '0')}`;
  }

  /**
   * El documento se anuló. <b>El folio no vuelve al saldo</b> — queda registrado como
   * anulado, que es distinto de no haber existido.
   *
   * `RN-44`: un folio no se recicla nunca. Reciclarlo produciría dos documentos
   * distintos con el mismo número a lo largo del tiempo — el mismo daño que la colisión
   * entre dispositivos, solo que más difícil de encontrar.
   */
  anular(folio: string): void {
    this.#anulados.push(folio);
  }

  anulados(): readonly string[] {
    return this.#anulados;
  }

  /**
   * Cuántos quedan.
   *
   * Existe para **avisar antes de salir**, no para explicar después. Un dispositivo que
   * descubre en el predio a las cinco de la mañana que no tiene folios deja al vehículo
   * sin documento, y entonces sale sin él o no sale.
   */
  saldo(): number {
    return Math.max(0, this.#definicion.hasta - this.#siguiente + 1);
  }
}
