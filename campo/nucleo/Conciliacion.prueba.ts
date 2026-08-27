import { test } from 'node:test';
import { strict as assert } from 'node:assert';

import { conciliar } from './Conciliacion.ts';
import type { TransicionCapturada } from './DiarioLocal.ts';

/**
 * `RN-45` — **cero sobrescritura silenciosa**.
 *
 * ── Por qué esta es la regla que no se puede agregar después ─────────────────
 * «En este dominio los datos en conflicto son **odómetros, galones y montos**.
 * Una sobrescritura automática destruye el término de una conciliación de
 * auditoría, y nadie se entera hasta que el auditor pregunta.»
 *
 * `ADR-001` lo llama por su nombre: la divergencia silenciosa es **la peor forma
 * de fallar**. No es que el sistema se caiga — es que sigue funcionando, con un
 * número que ya no corresponde a nada, durante meses.
 *
 * ── El caso real que esto cubre ──────────────────────────────────────────────
 * El motorista registra el retorno en su dispositivo con 84.320 km. En la
 * delegación, el encargado —que no sabe que el motorista ya lo hizo— lo digita
 * del papel con 84.302: un error de transposición al leer una hoja mojada.
 * Ambos sincronizan. **Nadie va a notar la diferencia de 18 km**, y esos 18 km
 * son el denominador de la conciliación galonaje–kilometraje de `RN-30`.
 */

const OTRO_MOMENTO = '2026-03-22T18:15:00-06:00';

test('dos capturas del mismo hecho con datos distintos NO se sobrescriben', () => {
  const delMotorista = capturaDe('01JQ8Z00000000000000DEV001', { odometroRetorno: 84_320 });
  const delEncargado = capturaDe('01JQ8Z00000000000000DEV002', { odometroRetorno: 84_302 });

  const resultado = conciliar([delMotorista], [delEncargado]);

  assert.equal(resultado.conflictos.length, 1);
  assert.equal(resultado.aceptadas.length, 0);

  // **Las dos versiones se conservan.** Ninguna gana automáticamente.
  const conflicto = resultado.conflictos[0]!;
  assert.equal(conflicto.versiones.length, 2);
  assert.deepEqual(
    conflicto.versiones.map((v) => v.datos['odometroRetorno']).sort(),
    [84_302, 84_320],
  );
});

test('el conflicto declara el origen de cada versión, o nadie puede arbitrarlo', () => {
  // `RN-45` exige que la cola lleve «registro afectado, versiones en conflicto,
  // **origen y fecha de cada una**». Sin el origen, quien resuelve tiene dos números
  // y ninguna forma de preguntar.
  const resultado = conciliar(
    [capturaDe('01JQ8Z00000000000000DEV001', { odometroRetorno: 84_320 })],
    [capturaDe('01JQ8Z00000000000000DEV002', { odometroRetorno: 84_302 })],
  );

  const origenes = resultado.conflictos[0]!.versiones.map((v) => v.idDispositivo).sort();

  assert.deepEqual(origenes, ['01JQ8Z00000000000000DEV001', '01JQ8Z00000000000000DEV002']);
});

test('el mismo hecho capturado dos veces IGUAL no es conflicto: es un reenvío', () => {
  // Distinguirlos importa. Si un reenvío entrara a la cola humana, la cola se llenaría
  // de ruido y en dos semanas nadie la miraría — y entonces el conflicto de verdad
  // pasaría de largo.
  const mismo = { odometroRetorno: 84_320 };

  const resultado = conciliar(
    [capturaDe('01JQ8Z00000000000000DEV001', mismo)],
    [capturaDe('01JQ8Z00000000000000DEV002', { ...mismo })],
  );

  assert.equal(resultado.conflictos.length, 0);
  assert.equal(resultado.aceptadas.length, 1);
});

test('transiciones distintas del mismo expediente conviven sin conflicto', () => {
  // El motorista captura el retorno; el encargado captura una parada anterior. **No
  // compiten**: son hechos distintos. Tratar todo lo del mismo expediente como
  // conflicto haría inútil la captura en paralelo, que es justamente lo que una
  // delegación necesita.
  const retorno = { ...capturaDe('01JQ8Z00000000000000DEV001', { odometroRetorno: 84_320 }) };
  const parada = {
    ...capturaDe('01JQ8Z00000000000000DEV002', { lugar: 'Danlí' }),
    idTransicion: '01JQ8Z0000000000000000000B',
    transicion: 'T-15',
  };

  const resultado = conciliar([retorno], [parada]);

  assert.equal(resultado.conflictos.length, 0);
  assert.equal(resultado.aceptadas.length, 2);
});

function capturaDe(
  idDispositivo: string,
  datos: Record<string, unknown>,
): TransicionCapturada & { idDispositivo: string } {
  return {
    // **El mismo hecho, capturado dos veces, lleva el mismo identificador de hecho.**
    // No es el identificador de la fila: es el del acontecimiento —esta misión, esta
    // transición— y por eso las dos capturas se encuentran en vez de convivir.
    idTransicion: '01JQ8Z0000000000000000000A',
    idExpediente: '01JQ8Z000000000000000000M1',
    transicion: 'T-18',
    ejecuta: 'P-MOTORISTA',
    ocurridoEn: OTRO_MOMENTO,
    datos,
    idDispositivo,
  };
}
