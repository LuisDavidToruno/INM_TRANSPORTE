import { test } from 'node:test';
import { strict as assert } from 'node:assert';

import { DiarioLocal } from './DiarioLocal.ts';

/**
 * El diario local del dispositivo — `P-1` aplicado al campo.
 *
 * ── Lo que este archivo defiende ─────────────────────────────────────────────
 * «Dos dispositivos no negocian **el estado**, intercambian **transiciones**»
 * (`orden-de-mision.md`, principio `P-1`). Esa frase parece de arquitectura y es
 * de supervivencia: si el dispositivo mandara *«esta misión está EN_RUTA»*, dos
 * dispositivos con la misma misión producirían una pelea que alguien tendría que
 * arbitrar sobre un dato que ya se perdió. Mandando transiciones, cada hecho
 * capturado sobrevive por separado y el estado se recalcula.
 *
 * ── Por qué esto es lo primero que se construye ──────────────────────────────
 * `RNF-03` no dice «con soporte offline». Dice **7 días continuos sin
 * conectividad y 0 registros perdidos al sincronizar**. Un registro perdido en
 * este diario es un kilometraje o un galonaje que no existe — y esos son los dos
 * datos sobre los que se hace la conciliación de auditoría.
 */

const MOMENTO = '2026-03-20T06:40:00-06:00';

test('una transición capturada sin red queda en el diario y sale como pendiente', () => {
  const diario = new DiarioLocal();

  diario.registrar({
    idTransicion: '01JQ8Z0000000000000000000A',
    idExpediente: '01JQ8Z000000000000000000M1',
    transicion: 'T-14',
    ejecuta: 'P-MOTORISTA',
    ocurridoEn: MOMENTO,
    datos: { odometroSalida: 84_320 },
  });

  const pendientes = diario.pendientes();

  assert.equal(pendientes.length, 1);
  assert.equal(pendientes[0]?.transicion, 'T-14');
  assert.equal(pendientes[0]?.ocurridoEn, MOMENTO);
});

test('lo confirmado deja de estar pendiente; lo que no confirmó, no', () => {
  // `RNF-03`: **0 registros perdidos**. La sincronización se corta a la mitad más veces
  // de las que termina — el motorista pasa bajo un puente, el servidor cierra la
  // conexión, la batería se agota. Lo que el servidor no acusó **sigue pendiente**, y
  // lo que acusó no se vuelve a mandar.
  const diario = new DiarioLocal();

  diario.registrar(unaTransicion('01JQ8Z0000000000000000000A', 'T-14'));
  diario.registrar(unaTransicion('01JQ8Z0000000000000000000B', 'T-15'));
  diario.registrar(unaTransicion('01JQ8Z0000000000000000000C', 'T-18'));

  // El servidor alcanzó a acusar dos antes de que se cortara.
  diario.confirmar(['01JQ8Z0000000000000000000A', '01JQ8Z0000000000000000000B']);

  const pendientes = diario.pendientes();

  assert.equal(pendientes.length, 1);
  assert.equal(pendientes[0]?.idTransicion, '01JQ8Z0000000000000000000C');
});

test('reenviar una transición ya confirmada no la duplica ni la resucita', () => {
  // La idempotencia no es una optimización: es lo que hace **segura** la reanudación.
  // El dispositivo que no supo si el servidor recibió, reenvía — y `ADR-005` hace que
  // eso sea inofensivo, porque el identificador nació en el dispositivo y no cambia.
  const diario = new DiarioLocal();

  diario.registrar(unaTransicion('01JQ8Z0000000000000000000A', 'T-14'));
  diario.confirmar(['01JQ8Z0000000000000000000A']);

  // Llega otra vez el mismo hecho — un reintento que se cruzó con el acuse.
  diario.registrar(unaTransicion('01JQ8Z0000000000000000000A', 'T-14'));

  assert.equal(diario.pendientes().length, 0);
  assert.equal(diario.total(), 1);
});

function unaTransicion(idTransicion: string, transicion: string) {
  return {
    idTransicion,
    idExpediente: '01JQ8Z000000000000000000M1',
    transicion,
    ejecuta: 'P-MOTORISTA',
    ocurridoEn: MOMENTO,
    datos: {},
  };
}
