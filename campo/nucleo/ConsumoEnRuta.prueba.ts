import { test } from 'node:test';
import { strict as assert } from 'node:assert';

import { CapturaInvalida, prepararConsumo, remanenteDelVale } from './ConsumoEnRuta.ts';
import type { CargaCapturada } from './ConsumoEnRuta.ts';
import { ColaDeAdjuntos } from './ColaDeAdjuntos.ts';
import { DiarioLocal } from './DiarioLocal.ts';

/**
 * La carga de combustible en la estación, sin red — `V-04`.
 *
 * ── Lo que este archivo defiende ─────────────────────────────────────────────
 * §10.1 dice que `V-04` <b>se ejecuta sin conectividad</b>. Eso no es una comodidad: la
 * estación donde el motorista carga camino a La Mosquitia no tiene señal, y si el consumo
 * no se captura ahí se captura de memoria tres días después — sin odómetro, que es
 * exactamente el dato que `RN-30` necesita para saber dónde se fue la diferencia.
 *
 * ── La línea que estas pruebas trazan ────────────────────────────────────────
 * <b>Qué comprueba el dispositivo y qué no.</b> Lo que la persona con el surtidor delante
 * puede corregir ahora, sí. El saldo del fondo, `RN-32` y `BD-06`, no — el dispositivo no
 * tiene esos datos y fingir que los valida daría por conforme lo que nadie comprobó.
 */

/**
 * Ejecuta y devuelve el fallo, en vez de sólo comprobar que hubo uno.
 *
 * `assert.throws` confirma que algo tronó y descarta el error. Acá hace falta mirarle el
 * <b>campo</b>: una prueba que sólo dice «tronó» seguiría en verde si el módulo rechazara
 * por el motivo equivocado.
 */
function capturar(accion: () => unknown): CapturaInvalida {
  try {
    accion();
  } catch (error) {
    assert.ok(error instanceof CapturaInvalida, `esperaba CapturaInvalida, vino ${String(error)}`);
    return error;
  }

  assert.fail('se esperaba que la captura fuera rechazada, y no lo fue');
}

const MOMENTO = '2026-09-03T11:20:00-06:00';

const CARGA: CargaCapturada = {
  idConsumo: '01JQ8Z0000000000000000CRG1',
  idAsignacion: '01JQ8Z00000000000000000VAL',
  idExpediente: '01JQ8Z0000000000000000MIS1',
  ejecuta: 'Wilmer Alvarado',
  ocurridoEn: MOMENTO,
  galones: 12.5,
  monto: 1_250,
  estacion: 'Estación Uno, Choluteca',
  odometro: 84_120,
  comprobante: 'F-0011-9932',
};

test('una carga válida produce una transición V-04 con los cinco datos', () => {
  const { transicion } = prepararConsumo(CARGA);

  assert.equal(transicion.transicion, 'V-04');
  assert.equal(transicion.idTransicion, CARGA.idConsumo);

  // Los cinco que §10.1 exige juntos. Ninguno sobra: el odómetro ancla el galón a un
  // tramo, y sin él la conciliación compara un total contra otro total.
  assert.equal(transicion.datos.galones, 12.5);
  assert.equal(transicion.datos.monto, 1_250);
  assert.equal(transicion.datos.estacion, 'Estación Uno, Choluteca');
  assert.equal(transicion.datos.odometro, 84_120);
  assert.equal(transicion.datos.comprobante, 'F-0011-9932');
});

test('el destino es la ASIGNACIÓN, no la misión', () => {
  // Una misión puede llevar varios vales, y el consumo se imputa a uno. Mandar sólo el
  // expediente obligaría al servidor a adivinar cuál — y adivinar sobre dinero es
  // exactamente lo que el folio existe para impedir.
  const { transicion } = prepararConsumo(CARGA);

  assert.equal(transicion.datos.idAsignacion, CARGA.idAsignacion);
  assert.equal(transicion.idExpediente, CARGA.idExpediente);
});

test('la fecha es la del HECHO, y sobrevive a sincronizar cuatro días después', () => {
  // `P-4` y `RN-46`. El motorista cargó el 3 de septiembre; que el servidor se entere el
  // 7 no cambia a qué día pertenece ese galón ni contra qué tarifa se juzga.
  const { transicion } = prepararConsumo(CARGA);

  assert.equal(transicion.ocurridoEn, MOMENTO);
});

// ── Lo que el dispositivo SÍ comprueba ──────────────────────────────────────

test('un consumo de cero galones no es un abastecimiento', () => {
  const fallo = capturar(
    () => prepararConsumo({ ...CARGA, galones: 0 }),
  );

  assert.equal(fallo.campo, 'galones');
});

test('sin estación no se registra: es lo que cruza el consumo contra la ruta', () => {
  const fallo = capturar(
    () => prepararConsumo({ ...CARGA, estacion: '   ' }),
  );

  assert.equal(fallo.campo, 'estacion');
});

test('sin odómetro no se registra', () => {
  const fallo = capturar(
    () => prepararConsumo({ ...CARGA, odometro: 0 }),
  );

  assert.equal(fallo.campo, 'odometro');
  assert.match(fallo.message, /anclado a ningún tramo/);
});

test('un odómetro que retrocede se detiene EN LA ESTACIÓN, no en la oficina', () => {
  // Es el único bloqueo que el dispositivo se permite sobre un hecho consumado, y se lo
  // permite porque quien lo tecleó tiene el tablero delante. Dejarlo pasar lo convierte en
  // un conflicto que alguien resuelve dentro de una semana, adivinando.
  const fallo = capturar(
    () =>
      prepararConsumo(
        { ...CARGA, odometro: 83_900 },
        { ultimoOdometroConocido: 84_000 },
      ),
  );

  assert.equal(fallo.campo, 'odometro');
  assert.match(fallo.message, /83,900/);
  assert.match(fallo.message, /84,000/);
});

test('sin lectura previa el odómetro NO se compara contra nada, y pasa', () => {
  // El recíproco. Un dispositivo recién sincronizado no conoce la lectura anterior, y
  // bloquear ahí impediría la primera carga de toda misión.
  prepararConsumo(CARGA, { ultimoOdometroConocido: null });
});

test('el mismo odómetro que la última lectura pasa: se puede cargar sin haber movido', () => {
  prepararConsumo({ ...CARGA, odometro: 84_000 }, { ultimoOdometroConocido: 84_000 });
});

// ── RN-85: la ausencia de comprobante ───────────────────────────────────────

test('sin comprobante se registra igual, con causa declarada', () => {
  // `RN-85`: **el registro del abastecimiento no se omite nunca por falta de papel.**
  const { transicion } = prepararConsumo({
    ...CARGA,
    comprobante: null,
    causaSinComprobante: 'La estación no emitió factura: sistema caído.',
  });

  assert.equal(transicion.datos.comprobante, null);
  assert.equal(
    transicion.datos.causaSinComprobante,
    'La estación no emitió factura: sistema caído.',
  );
});

test('sin comprobante y sin causa NO se registra', () => {
  // La causa es lo único que distingue «la estación no dio factura» de un campo que nadie
  // llenó, y esa diferencia decide si el descargo alternativo procede.
  const fallo = capturar(
    () => prepararConsumo({ ...CARGA, comprobante: null }),
  );

  assert.equal(fallo.campo, 'causaSinComprobante');
  assert.match(fallo.message, /tampoco se disimula/);
});

// ── La foto no retiene al hecho ─────────────────────────────────────────────

test('la foto va a su propia cola y el hecho sale sin esperarla', () => {
  // `RN-43` punto 3: los adjuntos se sincronizan sin bloquear el registro principal. Una
  // foto pesa dos órdenes de magnitud más que la transición que respalda.
  const { transicion, adjunto } = prepararConsumo({
    ...CARGA,
    foto: {
      idAdjunto: '01JQ8Z0000000000000000FOT1',
      ruta: '/almacen/2026/09/comprobante-9932.jpg',
      hash: 'a3f1c9',
      tipo: 'image/jpeg',
      bytes: 184_320,
    },
  });

  const diario = new DiarioLocal();
  const cola = new ColaDeAdjuntos();

  diario.registrar(transicion);
  cola.encolar(adjunto!);

  // El hecho está pendiente de sincronizar por su cuenta; la foto por la suya.
  assert.equal(diario.pendientes().length, 1);
  assert.equal(cola.resumen().pendientes, 1);

  // Y apunta al hecho que respalda: una foto sin su transición no prueba nada.
  assert.equal(adjunto!.idTransicion, CARGA.idConsumo);

  // Operativo, no dato personal: un comprobante de combustible no lleva persona
  // identificable, y clasificarlo mal lo metería en la depuración del hábeas data — que es
  // justo donde NO debe estar un respaldo contable.
  assert.equal(adjunto!.clasificacion, 'OPERATIVO');
});

test('una carga sin foto se registra igual', () => {
  const { adjunto } = prepararConsumo(CARGA);

  assert.equal(adjunto, null);
});

test('la foto se fecha con el HECHO, no con la subida', () => {
  // `P-4`: siete días sin red no cambian a qué mes pertenece una fotografía, y el almacén
  // la archiva por año y mes.
  const { adjunto } = prepararConsumo({
    ...CARGA,
    foto: {
      idAdjunto: '01JQ8Z0000000000000000FOT2',
      ruta: '/almacen/2026/09/f.jpg',
      hash: 'b2',
      tipo: 'image/jpeg',
      bytes: 100,
    },
  });

  assert.equal(adjunto!.capturadoEn, MOMENTO);
});

// ── Reenviar es inofensivo ──────────────────────────────────────────────────

test('reenviar la misma carga no la duplica', () => {
  // El dispositivo que no supo si el servidor recibió va a reintentar. Un galón contado
  // dos veces inventa una desviación de conciliación que nadie va a poder explicar.
  const diario = new DiarioLocal();
  const { transicion } = prepararConsumo(CARGA);

  diario.registrar(transicion);
  diario.registrar(transicion);

  assert.equal(diario.total(), 1);
});

// ── El remanente es una ayuda, no un control ────────────────────────────────

test('el remanente del vale resta sólo lo capturado en este dispositivo', () => {
  const vale = {
    idAsignacion: CARGA.idAsignacion,
    folio: 'VAL-CHO-2026-000418',
    monto: 2_500,
    tipoDeCombustible: 'Diesel',
  };

  assert.equal(remanenteDelVale(vale, []), 2_500);
  assert.equal(remanenteDelVale(vale, [CARGA]), 1_250);

  // Las cargas de OTRO vale no cuentan: cada folio cuadra por separado.
  const deOtroVale = { ...CARGA, idAsignacion: 'otro', monto: 900 };
  assert.equal(remanenteDelVale(vale, [CARGA, deOtroVale]), 1_250);
});

test('el remanente puede quedar en negativo, y NO se bloquea', () => {
  // Excederse es un hecho, no un intento: el combustible ya entró al tanque. `RN-83` manda
  // registrarlo marcado como excedido y resolver la cobertura en la liquidación —
  // «nunca omitiendo el registro». Un bloqueo acá dejaría el galón fuera del denominador.
  const vale = {
    idAsignacion: CARGA.idAsignacion,
    folio: 'VAL-CHO-2026-000418',
    monto: 1_000,
    tipoDeCombustible: 'Diesel',
  };

  assert.equal(remanenteDelVale(vale, [CARGA]), -250);

  // Y la captura procede: el dispositivo no conoce el saldo del fondo y no lo va a conocer
  // sin red. Quien decide es la liquidación, con todos los asientos a la vista.
  prepararConsumo(CARGA);
});
