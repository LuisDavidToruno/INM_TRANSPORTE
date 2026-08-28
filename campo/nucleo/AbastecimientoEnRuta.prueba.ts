import { test } from 'node:test';
import { strict as assert } from 'node:assert';

import {
  FuenteDeCampo,
  HECHO_DE_ABASTECIMIENTO,
  deberiaTraerComprobante,
  galonesCapturados,
  prepararAbastecimiento,
} from './AbastecimientoEnRuta.ts';
import type { AbastecimientoCapturado } from './AbastecimientoEnRuta.ts';
import { CapturaInvalida } from './CargaDeCombustible.ts';
import { ColaDeAdjuntos } from './ColaDeAdjuntos.ts';
import { DiarioLocal } from './DiarioLocal.ts';

/**
 * El combustible que <b>no salió del vale</b>, capturado sin red — `RN-83`.
 *
 * ── El galón que hoy desaparece ──────────────────────────────────────────────
 * El motorista que llena de una donación camino a La Mosquitia, o que pone de su bolsillo
 * porque el vale no alcanzó, <b>no tenía dónde anotarlo</b>. Ese galón no llegaba nunca al
 * denominador de `RN-30`, y su ausencia se lee como rendimiento imposiblemente bueno — es
 * decir, como si alguien hubiera despachado combustible sin registrarlo.
 *
 * <b>Que es verdad.</b> Lo que faltaba era poder registrarlo donde ocurre, que es sin red.
 */

function capturar(accion: () => unknown): CapturaInvalida {
  try {
    accion();
  } catch (error) {
    assert.ok(
      error instanceof CapturaInvalida,
      `esperaba CapturaInvalida, vino ${String(error)}`,
    );
    return error;
  }

  assert.fail('se esperaba que la captura fuera rechazada, y no lo fue');
}

const MOMENTO = '2026-09-03T14:40:00-06:00';

const DE_LA_SEDE: AbastecimientoCapturado = {
  idAbastecimiento: '01JQ8Z000000000000000ABS1',
  idVehiculo: '01JQ8Z000000000000000VEH03',
  idExpediente: '01JQ8Z0000000000000000MIS1',
  ejecuta: 'Wilmer Alvarado',
  ocurridoEn: MOMENTO,
  fuente: FuenteDeCampo.TanqueInstitucional,
  galones: 35,
  estacion: 'Predio de la delegación',
  odometro: 84_050,
  monto: null,
  comprobante: null,
};

test('una carga de otra fuente produce un hecho de abastecimiento, no una transición del vale', () => {
  // El vale no se mueve: este galón no salió de él, y descontárselo haría que el cuadre del
  // fondo mintiera por un combustible que el fondo no pagó.
  const { transicion } = prepararAbastecimiento(DE_LA_SEDE);

  assert.equal(transicion.transicion, HECHO_DE_ABASTECIMIENTO);
  assert.equal(transicion.datos.fuente, 'TanqueInstitucional');
  assert.equal(transicion.datos.idVehiculo, DE_LA_SEDE.idVehiculo);
});

test('el destino es el VEHÍCULO, y la misión es opcional', () => {
  // `RN-83` aplica «a todo vehículo de la flota, **en misión o fuera de ella**». El
  // reabastecimiento de rutina en el predio no tiene expediente al que colgarse, y no se le
  // inventa uno.
  const { idExpediente: _, ...sinMision } = DE_LA_SEDE;
  const { transicion } = prepararAbastecimiento(sinMision);

  assert.equal(transicion.idExpediente, '');
  assert.equal(transicion.datos.idVehiculo, DE_LA_SEDE.idVehiculo);
});

test('la fecha es la del HECHO y sobrevive a sincronizar días después', () => {
  const { transicion } = prepararAbastecimiento(DE_LA_SEDE);

  assert.equal(transicion.ocurridoEn, MOMENTO);
});

// ── Lo que el dispositivo comprueba ─────────────────────────────────────────

test('cero galones no es un abastecimiento', () => {
  const fallo = capturar(() => prepararAbastecimiento({ ...DE_LA_SEDE, galones: 0 }));

  assert.equal(fallo.campo, 'galones');
});

test('sin odómetro no se registra', () => {
  const fallo = capturar(() => prepararAbastecimiento({ ...DE_LA_SEDE, odometro: 0 }));

  assert.equal(fallo.campo, 'odometro');
  assert.match(fallo.message, /anclado a ningún tramo/);
});

test('el odómetro que retrocede se detiene acá también', () => {
  // La misma comprobación que el consumo del vale, y por la misma razón: quien lo tecleó
  // tiene el tablero delante. Que las dos puertas la compartan es lo que impide que
  // diverjan.
  const fallo = capturar(() =>
    prepararAbastecimiento(
      { ...DE_LA_SEDE, odometro: 83_900 },
      { ultimoOdometroConocido: 84_000 },
    ),
  );

  assert.equal(fallo.campo, 'odometro');
});

// ── RN-85: a quien no genera factura no se le pide causa ────────────────────

test('el tanque de la sede se registra SIN comprobante y SIN causa', () => {
  // No emite factura. Exigirle la causa obligaría a escribir «no aplica» en cada registro, y
  // una casilla que siempre dice lo mismo deja de leerse.
  const { transicion } = prepararAbastecimiento(DE_LA_SEDE);

  assert.equal(transicion.datos.comprobante, null);
  assert.equal(transicion.datos.causaSinComprobante, undefined);
});

test('una donación sin monto entra igual', () => {
  // «Un galón sin precio sigue siendo un galón en el denominador.»
  const { transicion } = prepararAbastecimiento({
    ...DE_LA_SEDE,
    fuente: FuenteDeCampo.Donacion,
    monto: null,
  });

  assert.equal(transicion.datos.monto, null);
  assert.equal(transicion.datos.galones, 35);
});

test('el peculio del servidor SÍ exige comprobante o causa', () => {
  // Se compró en una estación: hay factura, o hay que decir por qué no. Es dinero de una
  // persona que después va a reclamar el reintegro.
  const fallo = capturar(() =>
    prepararAbastecimiento({
      ...DE_LA_SEDE,
      fuente: FuenteDeCampo.PeculioDelServidor,
      monto: 500,
      comprobante: null,
    }),
  );

  assert.equal(fallo.campo, 'causaSinComprobante');
});

test('con la causa declarada el peculio se registra', () => {
  const { transicion } = prepararAbastecimiento({
    ...DE_LA_SEDE,
    fuente: FuenteDeCampo.PeculioDelServidor,
    monto: 500,
    comprobante: null,
    causaSinComprobante: 'La estación no emitió factura: sistema caído.',
  });

  assert.equal(transicion.datos.causaSinComprobante, 'La estación no emitió factura: sistema caído.');
});

test('qué fuentes deberían traer papel', () => {
  assert.equal(deberiaTraerComprobante(FuenteDeCampo.PeculioDelServidor), true);
  assert.equal(deberiaTraerComprobante(FuenteDeCampo.TanqueInstitucional), false);
  assert.equal(deberiaTraerComprobante(FuenteDeCampo.Donacion), false);
  assert.equal(deberiaTraerComprobante(FuenteDeCampo.TerceroEnApoyo), false);
});

// ── La cola y el reenvío ────────────────────────────────────────────────────

test('la foto va a su propia cola y el hecho sale sin esperarla', () => {
  const { transicion, adjunto } = prepararAbastecimiento({
    ...DE_LA_SEDE,
    foto: {
      idAdjunto: '01JQ8Z0000000000000000FOT9',
      ruta: '/almacen/2026/09/tanque.jpg',
      hash: 'c4d2',
      tipo: 'image/jpeg',
      bytes: 152_000,
    },
  });

  const diario = new DiarioLocal();
  const cola = new ColaDeAdjuntos();

  diario.registrar(transicion);
  cola.encolar(adjunto!);

  assert.equal(diario.pendientes().length, 1);
  assert.equal(cola.resumen().pendientes, 1);
  assert.equal(adjunto!.idTransicion, DE_LA_SEDE.idAbastecimiento);
  assert.equal(adjunto!.clasificacion, 'OPERATIVO');
});

test('reenviar el mismo abastecimiento no lo duplica', () => {
  // Un galón contado dos veces infla el denominador y produce una desviación inventada por
  // el propio sistema.
  const diario = new DiarioLocal();
  const { transicion } = prepararAbastecimiento(DE_LA_SEDE);

  diario.registrar(transicion);
  diario.registrar(transicion);

  assert.equal(diario.total(), 1);
});

// ── Lo que el motorista puede ver ───────────────────────────────────────────

test('los galones capturados suman TODAS las fuentes', () => {
  // Es lo que el motorista necesita para saber si le falta anotar algo: si el tanque recibió
  // sesenta galones y el dispositivo sólo conoce veinte, hay cuarenta que nadie va a poder
  // explicar cuando se concilie.
  assert.equal(galonesCapturados([{ galones: 20 }], [DE_LA_SEDE]), 55);
  assert.equal(galonesCapturados([], []), 0);
  assert.equal(galonesCapturados([{ galones: 20 }, { galones: 5 }], []), 25);
});
