import { test } from 'node:test';
import { strict as assert } from 'node:assert';

import {
  EscalaDeNivel,
  SubtipoDeRetorno,
  diferenciaDeTanque,
  prepararRetorno,
  prepararSalida,
} from './SalidaYRetorno.ts';
import type { LecturaDelTanque } from './SalidaYRetorno.ts';
import { CapturaInvalida } from './CargaDeCombustible.ts';

/**
 * La salida y el retorno con <b>nivel de tanque</b> — `T-14`, `T-18` y `RN-83`.
 *
 * ── Por qué el nivel se pide en el predio ────────────────────────────────────
 * Porque después no existe. Sin él, <i>«salió lleno y volvió vacío»</i> no se distingue de un
 * faltante, y la conciliación de una misión corta con tanque grande no significa nada.
 * `RN-83` lo hace <b>dato obligatorio de bitácora</b>, y la bitácora se llena con el vehículo
 * delante.
 *
 * ── La distinción que este archivo protege ───────────────────────────────────
 * <b>«No lo leí» y «marcaba cero» son cosas opuestas.</b> Un campo numérico vacío no las
 * distingue, y `RN-80` es explícita: el campo no consignado se declara y <b>no se estima</b>.
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

const MOMENTO = '2026-09-03T06:40:00-06:00';

const LLENO: LecturaDelTanque = {
  leido: true,
  nivel: { escala: EscalaDeNivel.FraccionDelIndicador, valor: 1 },
};

const UN_OCTAVO: LecturaDelTanque = {
  leido: true,
  nivel: { escala: EscalaDeNivel.FraccionDelIndicador, valor: 0.125 },
};

const SALIDA = {
  idSalida: '01JQ8Z0000000000000000SAL1',
  idExpediente: '01JQ8Z0000000000000000MIS1',
  ejecuta: 'Wilmer Alvarado',
  ocurridoEn: MOMENTO,
  odometro: 84_000,
  tanque: LLENO,
};

const RETORNO = {
  idRetorno: '01JQ8Z0000000000000000RET1',
  idExpediente: '01JQ8Z0000000000000000MIS1',
  ejecuta: 'Wilmer Alvarado',
  ocurridoEn: '2026-09-04T17:10:00-06:00',
  odometro: 84_900,
  tanque: UN_OCTAVO,
  subtipo: SubtipoDeRetorno.Ordinario,
};

// ── La salida ───────────────────────────────────────────────────────────────

test('la salida lleva odómetro Y nivel de tanque', () => {
  const t = prepararSalida(SALIDA);

  assert.equal(t.transicion, 'T-14');
  assert.equal(t.datos.odometro, 84_000);
  assert.equal(t.datos.nivelDeTanque, 1);
  assert.equal(t.datos.escalaDelNivel, 'FraccionDelIndicador');
});

test('sin odómetro no sale el vehículo', () => {
  const fallo = capturar(() => prepararSalida({ ...SALIDA, odometro: 0 }));

  assert.equal(fallo.campo, 'odometro');
  assert.match(fallo.message, /hallazgo típico del Tribunal/);
});

test('el tanque NO leído viaja con su razón, no como un hueco', () => {
  // `RN-80`: el campo no consignado se declara y no se estima. Un cero silencioso diría que
  // el vehículo salió con el tanque vacío.
  const t = prepararSalida({
    ...SALIDA,
    tanque: { leido: false, porQueNo: 'El indicador está averiado — orden de trabajo 2026-0071.' },
  });

  assert.equal(t.datos.nivelDeTanque, null);
  assert.equal(
    t.datos.tanqueNoConsignado,
    'El indicador está averiado — orden de trabajo 2026-0071.',
  );
});

test('«no lo leí» sin razón NO se acepta', () => {
  // Es lo único que impide que el motorista deje el campo en blanco y el sistema lo
  // interprete solo.
  const fallo = capturar(() =>
    prepararSalida({ ...SALIDA, tanque: { leido: false, porQueNo: '   ' } }),
  );

  assert.equal(fallo.campo, 'tanque');
  assert.match(fallo.message, /no se\n?\s*estima|no se estima/);
});

test('en fracción del indicador el nivel va de 0 a 1', () => {
  // Quince en fracción es un error de escala: quien lo tecleó quiso decir galones. Aceptarlo
  // produciría un remanente de mil quinientos por ciento que nadie podría interpretar.
  const fallo = capturar(() =>
    prepararSalida({
      ...SALIDA,
      tanque: { leido: true, nivel: { escala: EscalaDeNivel.FraccionDelIndicador, valor: 15 } },
    }),
  );

  assert.equal(fallo.campo, 'tanque');
  assert.match(fallo.message, /cambie la escala/);
});

test('en galones sí se admite quince', () => {
  const t = prepararSalida({
    ...SALIDA,
    tanque: { leido: true, nivel: { escala: EscalaDeNivel.Galones, valor: 15 } },
  });

  assert.equal(t.datos.nivelDeTanque, 15);
  assert.equal(t.datos.escalaDelNivel, 'Galones');
});

// ── El retorno ──────────────────────────────────────────────────────────────

test('el retorno lleva su nivel, y con el de salida se puede restar', () => {
  const t = prepararRetorno(RETORNO, { ultimoOdometroConocido: 84_000 });

  assert.equal(t.transicion, 'T-18');
  assert.equal(t.datos.nivelDeTanque, 0.125);
  assert.equal(t.datos.subtipo, 'Ordinario');
});

test('un odómetro de retorno MENOR que el de salida se detiene en el predio', () => {
  // Es físicamente imposible y quien lo tecleó tiene el tablero delante. Dejarlo pasar lo
  // convierte en un conflicto que alguien resuelve una semana después, adivinando.
  const fallo = capturar(() =>
    prepararRetorno({ ...RETORNO, odometro: 83_800 }, { ultimoOdometroConocido: 84_000 }),
  );

  assert.equal(fallo.campo, 'odometro');
  assert.match(fallo.message, /físicamente imposible/);
});

test('en el retorno CONSTATADO no se bloquea: el vehículo ya está en el predio', () => {
  // `RN-79` y el hallazgo `HB3-04`: negarse a registrarlo dejaría el vehículo secuestrado por
  // un trámite mientras la delegación se queda sin unidad. Se anota tal cual y se marca.
  const t = prepararRetorno(
    { ...RETORNO, odometro: 83_800, subtipo: SubtipoDeRetorno.Constatado },
    { ultimoOdometroConocido: 84_000 },
  );

  assert.equal(t.datos.odometro, 83_800);
  assert.equal(t.datos.subtipo, 'Constatado');
});

test('volver con el MISMO odómetro exige justificación', () => {
  // No bloquea el hecho, pero no pasa en silencio: es el patrón de la misión que nunca se
  // hizo, y ése es el que busca el Tribunal.
  const fallo = capturar(() =>
    prepararRetorno({ ...RETORNO, odometro: 84_000 }, { ultimoOdometroConocido: 84_000 }),
  );

  assert.equal(fallo.campo, 'justificacion');
  assert.match(fallo.message, /nunca se hizo/);
});

test('con la justificación declarada sí se registra', () => {
  const t = prepararRetorno(
    {
      ...RETORNO,
      odometro: 84_000,
      justificacion: 'La misión se suspendió en el predio antes de salir.',
    },
    { ultimoOdometroConocido: 84_000 },
  );

  assert.equal(t.datos.justificacion, 'La misión se suspendió en el predio antes de salir.');
});

test('sin lectura previa el odómetro no se compara contra nada', () => {
  // Un dispositivo recién sincronizado no conoce la salida. Bloquear ahí impediría registrar
  // el retorno de toda misión que no capturó su propia salida.
  prepararRetorno(RETORNO, { ultimoOdometroConocido: null });
});

// ── La diferencia de tanque ─────────────────────────────────────────────────

test('dos lecturas de la misma escala se restan', () => {
  assert.equal(diferenciaDeTanque(LLENO, UN_OCTAVO), -0.875);
});

test('si falta una lectura la diferencia es NULA, no cero', () => {
  // «No se puede saber» y «no hay diferencia» son cosas opuestas. Dar por parejo lo que no se
  // midió es justo lo que `RN-80` prohíbe.
  assert.equal(
    diferenciaDeTanque(LLENO, { leido: false, porQueNo: 'Indicador averiado' }),
    null,
  );
});

test('escalas distintas tampoco se restan', () => {
  // Un octavo de indicador y quince galones no se comparan sin la capacidad del tanque, que
  // la ficha técnica no declara.
  assert.equal(
    diferenciaDeTanque(LLENO, {
      leido: true,
      nivel: { escala: EscalaDeNivel.Galones, valor: 15 },
    }),
    null,
  );
});
