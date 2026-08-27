import { test } from 'node:test';
import { strict as assert } from 'node:assert';
import { mkdtempSync, rmSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';

import { AlmacenSqlite } from './AlmacenSqlite.ts';
import { DiarioLocal } from './DiarioLocal.ts';

/**
 * La persistencia del diario — `ADR-003`.
 *
 * ── Por qué esto no es un detalle de infraestructura ─────────────────────────
 * `ADR-003` no dice «con caché local». Dice **SQLite cifrado como fuente de
 * verdad local, no como caché**. La diferencia se ve en una sola prueba: lo que
 * el motorista capturó tiene que seguir ahí **después de cerrar la aplicación**.
 *
 * Un diario que vive en memoria pierde siete días de trabajo cuando Android mata
 * el proceso para recuperar RAM — y Android lo hace, sin avisar, en gama baja,
 * que es el equipo que `RNF-12` obliga a soportar.
 *
 * ── Lo que estas pruebas NO cubren, y hay que decirlo ────────────────────────
 * **El cifrado.** SQLCipher es una compilación distinta de SQLite: se abre con
 * `PRAGMA key` y desde ahí **todo el SQL es idéntico**. Node trae SQLite sin
 * cifrar, así que acá se prueba el esquema, las consultas y la durabilidad —
 * que es exactamente lo que corre en el dispositivo— y **no** que el archivo
 * esté cifrado en reposo. Eso se verifica en el dispositivo, abriendo el archivo
 * sin la clave y comprobando que no se puede leer.
 */

test('lo capturado sobrevive cerrar y volver a abrir la base', () => {
  // Es *la* prueba de «fuente de verdad, no caché». Si esto falla, el motorista
  // pierde la bitácora cuando Android mata el proceso.
  const carpeta = mkdtempSync(join(tmpdir(), 'sigti-'));
  const archivo = join(carpeta, 'campo.db');

  try {
    const primeraSesion = new AlmacenSqlite(archivo);
    const diario = new DiarioLocal(primeraSesion);

    diario.registrar({
      idTransicion: '01JQ8Z0000000000000000000A',
      idExpediente: '01JQ8Z000000000000000000M1',
      transicion: 'T-14',
      ejecuta: 'P-MOTORISTA',
      ocurridoEn: '2026-03-20T06:40:00-06:00',
      datos: { odometroSalida: 84_320 },
    });

    primeraSesion.cerrar();

    // El proceso murió. Alguien vuelve a abrir la aplicación.
    const segundaSesion = new AlmacenSqlite(archivo);
    const recuperado = new DiarioLocal(segundaSesion);

    const pendientes = recuperado.pendientes();

    assert.equal(pendientes.length, 1);
    assert.equal(pendientes[0]?.transicion, 'T-14');
    // Los datos del hecho también sobreviven — el odómetro es el dato que se concilia.
    assert.equal(pendientes[0]?.datos['odometroSalida'], 84_320);

    segundaSesion.cerrar();
  } finally {
    rmSync(carpeta, { recursive: true, force: true });
  }
});

test('la confirmación también sobrevive: no se reenvía lo que ya se acusó', () => {
  // Si la confirmación viviera solo en memoria, cada reinicio reenviaría el diario
  // entero. Con siete días de captura eso es un lote enorme por una batería agotada.
  const carpeta = mkdtempSync(join(tmpdir(), 'sigti-'));
  const archivo = join(carpeta, 'campo.db');

  try {
    const primera = new AlmacenSqlite(archivo);
    const diario = new DiarioLocal(primera);

    diario.registrar(unHecho('01JQ8Z0000000000000000000A'));
    diario.registrar(unHecho('01JQ8Z0000000000000000000B'));
    diario.confirmar(['01JQ8Z0000000000000000000A']);
    primera.cerrar();

    const segunda = new AlmacenSqlite(archivo);
    const recuperado = new DiarioLocal(segunda);

    assert.equal(recuperado.pendientes().length, 1);
    assert.equal(recuperado.total(), 2);

    segunda.cerrar();
  } finally {
    rmSync(carpeta, { recursive: true, force: true });
  }
});

test('la unicidad del hecho la impone la BASE, no el código que la llama', () => {
  // El mismo hecho recapturado tras un reinicio no puede duplicarse. Que la clave
  // primaria lo garantice importa: una comprobación en código se puede olvidar al
  // escribir el próximo camino de escritura.
  const carpeta = mkdtempSync(join(tmpdir(), 'sigti-'));
  const archivo = join(carpeta, 'campo.db');

  try {
    const almacen = new AlmacenSqlite(archivo);
    const diario = new DiarioLocal(almacen);

    diario.registrar(unHecho('01JQ8Z0000000000000000000A'));
    diario.registrar(unHecho('01JQ8Z0000000000000000000A'));

    assert.equal(diario.total(), 1);

    almacen.cerrar();
  } finally {
    rmSync(carpeta, { recursive: true, force: true });
  }
});

function unHecho(idTransicion: string) {
  return {
    idTransicion,
    idExpediente: '01JQ8Z000000000000000000M1',
    transicion: 'T-14',
    ejecuta: 'P-MOTORISTA',
    ocurridoEn: '2026-03-20T06:40:00-06:00',
    datos: {},
  };
}
