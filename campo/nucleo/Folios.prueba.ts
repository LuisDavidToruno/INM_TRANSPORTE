import { test } from 'node:test';
import { strict as assert } from 'node:assert';

import { SubrangoDeFolios, SinFoliosDisponibles } from './Folios.ts';

/**
 * `RN-44` y `RNF-21` — folios pre-asignados, consumidos sin red.
 *
 * ── El caso que este archivo existe para impedir ─────────────────────────────
 * Es el hallazgo `HB34-52`, y fue clasificado **crítico** por una razón: no es
 * hipotético. En la delegación de Tocoa hay dos dispositivos. Los dos llevan
 * descargado el rango de la delegación. Los dos están sin red. Los dos emiten
 * una orden de misión el mismo día.
 *
 * **Toman el mismo número.**
 *
 * Y el folio es la unidad de trazabilidad del combustible (`RN-27`): dos folios
 * iguales destruyen la conciliación, y el error aparece semanas después, cuando
 * alguien concilia y encuentra dos documentos distintos con el mismo número.
 *
 * ── Por qué el rango de la delegación no basta ───────────────────────────────
 * `HB34-52` lo dice sin rodeos: la prueba de verificación de `RNF-21` —«cinco
 * dispositivos, **cada uno con el rango de una delegación distinta**»— pasaría
 * igual, porque está escrita para el caso fácil. El caso difícil es **dos
 * dispositivos de la misma delegación**, y es el que ocurre todos los días.
 *
 * La corrección: el rango se asigna en **dos niveles**. `rango_de_folio` a la
 * delegación, y un **subrango al dispositivo**, con su propio saldo.
 */

test('dos dispositivos de la misma delegación NO pueden emitir el mismo folio', () => {
  // El caso de Tocoa, tal cual. Un rango de delegación partido en dos subrangos.
  const enElPredio = new SubrangoDeFolios({
    idDispositivo: 'DEV-TOCOA-01',
    prefijo: 'OM-2026',
    desde: 1,
    hasta: 50,
  });

  const enLaOficina = new SubrangoDeFolios({
    idDispositivo: 'DEV-TOCOA-02',
    prefijo: 'OM-2026',
    desde: 51,
    hasta: 100,
  });

  const unos = Array.from({ length: 5 }, () => enElPredio.consumir());
  const otros = Array.from({ length: 5 }, () => enLaOficina.consumir());

  assert.equal(new Set([...unos, ...otros]).size, 10, 'hubo folios repetidos entre dispositivos');
});

test('el folio no se recicla, ni siquiera si el documento se anula', () => {
  // `RN-44`: «un folio **no debe** reciclarse nunca, ni siquiera si el documento se
  // anula». Reciclarlo produciría dos documentos distintos con el mismo número a lo
  // largo del tiempo — que es el mismo daño, solo que más difícil de encontrar.
  const subrango = new SubrangoDeFolios({
    idDispositivo: 'DEV-TOCOA-01',
    prefijo: 'OM-2026',
    desde: 1,
    hasta: 10,
  });

  const primero = subrango.consumir();
  subrango.anular(primero);
  const siguiente = subrango.consumir();

  assert.notEqual(siguiente, primero);
  assert.equal(subrango.anulados().length, 1);
});

test('agotar el subrango falla ANTES de salir, no en el predio a las cinco de la mañana', () => {
  // Un dispositivo sin folios en zona sin cobertura no puede emitir la orden de misión,
  // y el vehículo sale sin documento o no sale. El error tiene que ser explícito y
  // temprano — y por eso `saldo()` existe: para avisar antes, no para explicar después.
  const subrango = new SubrangoDeFolios({
    idDispositivo: 'DEV-TOCOA-01',
    prefijo: 'OM-2026',
    desde: 1,
    hasta: 2,
  });

  subrango.consumir();
  subrango.consumir();

  assert.equal(subrango.saldo(), 0);
  assert.throws(() => subrango.consumir(), SinFoliosDisponibles);
});

test('el folio se emite completo y legible, no como un número suelto', () => {
  // Va impreso y alguien lo va a citar por teléfono desde un retén. `OM-2026-000001`
  // se dicta; `1` no significa nada fuera de su contexto.
  const subrango = new SubrangoDeFolios({
    idDispositivo: 'DEV-TOCOA-01',
    prefijo: 'OM-2026',
    desde: 1,
    hasta: 10,
  });

  assert.equal(subrango.consumir(), 'OM-2026-000001');
});
