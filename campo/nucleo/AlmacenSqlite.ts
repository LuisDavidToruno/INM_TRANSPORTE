import { DatabaseSync } from 'node:sqlite';

import type { AlmacenDeDiario, TransicionCapturada } from './DiarioLocal.ts';

/**
 * El diario en SQLite — **fuente de verdad local, no caché** (`ADR-003`).
 *
 * ── Lo que esta clase es y no es ─────────────────────────────────────────────
 * <b>Es</b> el esquema y las consultas que corren en el dispositivo. <b>No es</b>
 * el cifrado.
 *
 * `ADR-002` y `ADR-003` deciden **SQLCipher**, que es una compilación distinta de
 * SQLite: se abre con `PRAGMA key = '…'` y a partir de ahí **todo el SQL es
 * idéntico**. Node trae SQLite sin cifrar, así que acá se prueba lo que de verdad
 * puede fallar en el esquema —durabilidad, unicidad, ida y vuelta de los datos— y
 * el cifrado queda como **un cambio de apertura** en el módulo nativo.
 *
 * Ese corte es honesto porque la superficie que el cifrado cambia es una línea. Lo
 * que no es honesto es dar por probado que el archivo queda ilegible sin la clave:
 * eso <b>se verifica en el dispositivo</b>, abriendo el archivo sin clave y
 * comprobando que no se puede leer. No está hecho.
 *
 * ── Por qué el JSON de `datos` no se normaliza en columnas ───────────────────
 * Porque lo que cada transición lleva cambia con el módulo —odómetro en `T-14`,
 * galones en el consumo, coordenadas en una parada— y `M-08` todavía no existe.
 * Normalizarlo ahora sería fijar en el esquema del dispositivo una forma que
 * todavía no conocemos, y migrar esquemas en equipos que están en el campo, sin
 * red y sin nadie que sepa hacerlo, es el peor sitio para descubrir que uno se
 * equivocó.
 */
export class AlmacenSqlite implements AlmacenDeDiario {
  readonly #base: DatabaseSync;

  constructor(archivo: string) {
    this.#base = new DatabaseSync(archivo);

    // En el dispositivo esto va precedido de `PRAGMA key`. Ver arriba.
    this.#base.exec(`
      CREATE TABLE IF NOT EXISTS transicion_capturada (
        id_transicion TEXT PRIMARY KEY,
        id_expediente TEXT NOT NULL,
        transicion    TEXT NOT NULL,
        ejecuta       TEXT NOT NULL,
        ocurrido_en   TEXT NOT NULL,
        datos         TEXT NOT NULL,
        confirmada    INTEGER NOT NULL DEFAULT 0
      );

      CREATE INDEX IF NOT EXISTS ix_pendientes
        ON transicion_capturada (confirmada);
    `);
  }

  /**
   * `INSERT OR IGNORE` y no `INSERT`: el mismo hecho recapturado tras un reinicio
   * choca contra la clave primaria y **no pasa nada**, que es exactamente lo que se
   * quiere. La unicidad la impone la base, no el código que la llama.
   *
   * Tampoco es `INSERT OR REPLACE`: eso **sobrescribiría** el hecho original, y
   * `RN-45` lo prohíbe. El primero que quedó, queda.
   */
  guardar(t: TransicionCapturada): void {
    this.#base
      .prepare(
        `INSERT OR IGNORE INTO transicion_capturada
           (id_transicion, id_expediente, transicion, ejecuta, ocurrido_en, datos)
         VALUES (?, ?, ?, ?, ?, ?)`,
      )
      .run(t.idTransicion, t.idExpediente, t.transicion, t.ejecuta, t.ocurridoEn, JSON.stringify(t.datos));
  }

  /**
   * Marca lo que el servidor acusó. **No borra**: el diario no se vacía al
   * sincronizar, porque es el registro de lo que este dispositivo capturó y el
   * motorista puede necesitar consultarlo sin red.
   */
  marcarConfirmadas(ids: readonly string[]): void {
    const sentencia = this.#base.prepare(
      'UPDATE transicion_capturada SET confirmada = 1 WHERE id_transicion = ?',
    );

    for (const id of ids) sentencia.run(id);
  }

  pendientes(): readonly TransicionCapturada[] {
    const filas = this.#base
      .prepare(
        `SELECT id_transicion, id_expediente, transicion, ejecuta, ocurrido_en, datos
           FROM transicion_capturada
          WHERE confirmada = 0
          ORDER BY rowid`,
      )
      .all() as readonly Record<string, string>[];

    return filas.map((f) => ({
      idTransicion: f['id_transicion']!,
      idExpediente: f['id_expediente']!,
      transicion: f['transicion']!,
      ejecuta: f['ejecuta']!,
      ocurridoEn: f['ocurrido_en']!,
      datos: JSON.parse(f['datos']!) as Record<string, unknown>,
    }));
  }

  total(): number {
    const fila = this.#base
      .prepare('SELECT COUNT(*) AS n FROM transicion_capturada')
      .get() as { n: number };

    return fila.n;
  }

  cerrar(): void {
    this.#base.close();
  }
}
