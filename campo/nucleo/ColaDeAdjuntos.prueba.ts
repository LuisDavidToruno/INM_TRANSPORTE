import { test } from 'node:test';
import { strict as assert } from 'node:assert';

import { ColaDeAdjuntos, ClasificacionDeContenido } from './ColaDeAdjuntos.ts';

/**
 * Los adjuntos diferidos — `RN-43` y `ADR-004`.
 *
 * ── La propiedad que decide el diseño ────────────────────────────────────────
 * `RN-43` punto 3: *«Las fotografías se almacenan localmente y se sincronizan
 * como adjuntos diferidos, **sin bloquear el envío del registro principal**»*.
 *
 * Y no es una comodidad. Una foto de comprobante pesa dos órdenes de magnitud
 * más que la transición que respalda. Si el hecho esperara a su foto, un
 * motorista con señal intermitente en la CA-5 no sincronizaría **nada** —
 * ni el odómetro, que ocupa cuarenta bytes y es lo que la conciliación necesita.
 *
 * ── La aritmética que sostiene `ADR-004` ─────────────────────────────────────
 * Datos relacionales ≈ 8 GB al año. Adjuntos ≈ 30 GB. Por eso el archivo vive en
 * el sistema de archivos y la base guarda **ruta, hash, tipo, tamaño y
 * clasificación de contenido** — no el binario.
 */

const CAPTURA = '2026-03-20T06:41:00-06:00';

test('el adjunto va en su propia cola: no retiene al hecho que respalda', () => {
  // Si esta prueba fallara, el diseño sería «el hecho espera a su foto», y con
  // señal intermitente eso significa que no llega nada.
  const cola = new ColaDeAdjuntos();

  cola.encolar(unAdjunto('01JQ8Z00000000000000ADJ001'));

  // La cola de adjuntos no sabe nada del diario, y el diario no sabe nada de ella.
  // Son dos colas que avanzan a su ritmo, y esa independencia ES el requisito.
  assert.equal(cola.pendientes().length, 1);
  assert.equal(cola.pendientes()[0]?.idTransicion, '01JQ8Z0000000000000000000A');
});

test('la cola dice cuántos hay y desde cuándo, que es lo que RN-43 exige mostrar', () => {
  // `RN-43` punto 4: «el cliente muestra en todo momento cuántos registros y adjuntos
  // están pendientes de sincronizar **y desde cuándo**». El «desde cuándo» es el dato
  // que importa: tres adjuntos de hace una hora es normal, tres de hace nueve días
  // significa que este dispositivo no está sincronizando y nadie se enteró.
  const cola = new ColaDeAdjuntos();

  cola.encolar({ ...unAdjunto('01JQ8Z00000000000000ADJ001'), capturadoEn: '2026-03-20T06:41:00-06:00' });
  cola.encolar({ ...unAdjunto('01JQ8Z00000000000000ADJ002'), capturadoEn: '2026-03-18T09:00:00-06:00' });

  const resumen = cola.resumen();

  assert.equal(resumen.pendientes, 2);
  assert.equal(resumen.masAntiguo, '2026-03-18T09:00:00-06:00');
});

test('confirmar uno no arrastra a los demás: la cola avanza de a poco', () => {
  // Sincronizar 200 fotos por una red de retén no es una operación, son 200. Que el
  // acuse sea por adjunto es lo que hace que una interrupción cueste una foto y no
  // la sesión entera.
  const cola = new ColaDeAdjuntos();

  cola.encolar(unAdjunto('01JQ8Z00000000000000ADJ001'));
  cola.encolar(unAdjunto('01JQ8Z00000000000000ADJ002'));
  cola.confirmar('01JQ8Z00000000000000ADJ001');

  assert.equal(cola.pendientes().length, 1);
  assert.equal(cola.pendientes()[0]?.idAdjunto, '01JQ8Z00000000000000ADJ002');
});

test('un adjunto con dato personal se clasifica, porque hay que poder depurarlo', () => {
  // `HB34-53` — la depuración de datos personales **alcanza a los adjuntos**. Sin
  // clasificación no hay forma de encontrar la foto del manifiesto entre treinta mil
  // fotos de odómetro, y entonces el hábeas data no se puede atender.
  const cola = new ColaDeAdjuntos();

  cola.encolar({
    ...unAdjunto('01JQ8Z00000000000000ADJ001'),
    clasificacion: ClasificacionDeContenido.DatoPersonal,
  });
  cola.encolar(unAdjunto('01JQ8Z00000000000000ADJ002'));

  const conDatoPersonal = cola.pendientes().filter(
    (a) => a.clasificacion === ClasificacionDeContenido.DatoPersonal,
  );

  assert.equal(conDatoPersonal.length, 1);
});

test('el espacio se avisa ANTES de agotarse, no cuando ya no cabe la foto', () => {
  // `RN-43`, caso límite: «almacenamiento local agotado por acumulación de fotografías
  // en una misión larga. El cliente **debe alertar con anticipación**».
  //
  // Descubrirlo al pulsar el obturador, en el sitio de un accidente, es descubrirlo
  // tarde: esa foto es la evidencia y no se puede volver a tomar.
  const cola = new ColaDeAdjuntos({ presupuestoBytes: 1_000 });

  cola.encolar({ ...unAdjunto('01JQ8Z00000000000000ADJ001'), bytes: 850 });

  assert.equal(cola.espacioComprometido(), 850);
  assert.equal(cola.cercaDelLimite(), true);
});

function unAdjunto(idAdjunto: string) {
  return {
    idAdjunto,
    idTransicion: '01JQ8Z0000000000000000000A',
    ruta: `adjuntos/2026-03/${idAdjunto}.jpg`,
    // `ADR-004`: el hash es lo que permite detectar que un adjunto fue sustituido o
    // se corrompió, y lo que sostiene los paquetes de evidencia.
    hash: 'sha256:0000000000000000000000000000000000000000000000000000000000000000',
    tipo: 'image/jpeg',
    bytes: 240_000,
    clasificacion: ClasificacionDeContenido.Operativo,
    capturadoEn: CAPTURA,
  };
}
