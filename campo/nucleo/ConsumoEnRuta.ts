import type { TransicionCapturada } from './DiarioLocal.ts';
import type { AdjuntoPendiente } from './ColaDeAdjuntos.ts';
import { ClasificacionDeContenido } from './ColaDeAdjuntos.ts';
import { CapturaInvalida, exigirDatosDeLaCarga, exigirRespaldo } from './CargaDeCombustible.ts';
import type { ContextoDelDispositivo, FotoDeLaCarga } from './CargaDeCombustible.ts';

// Reexportados: eran de este archivo antes de que la carga de otras fuentes existiera, y
// romper los llamadores por mover una definición no le sirve a nadie.
export { CapturaInvalida } from './CargaDeCombustible.ts';
export type { ContextoDelDispositivo, FotoDeLaCarga } from './CargaDeCombustible.ts';

/**
 * La carga de combustible, tal como la teclea el motorista en la estación — `V-04`.
 *
 * <b>El binario de la foto no está aquí</b>, igual que en el resto del núcleo: lo que
 * viaja es ruta, hash, tipo y tamaño (`ADR-004`).
 */
export interface CargaCapturada {
  /** ULID del dispositivo (`ADR-005`). Es lo que hace inofensivo el reenvío. */
  readonly idConsumo: string;
  /** El vale contra el que se consume. <b>No es la misión</b>: una misión puede llevar varios. */
  readonly idAsignacion: string;
  readonly idExpediente: string;
  readonly ejecuta: string;
  /** La fecha del <b>hecho</b>, no la de captura ni la de sincronización (`P-4`, `RN-46`). */
  readonly ocurridoEn: string;
  readonly galones: number;
  readonly monto: number;
  readonly estacion: string;
  /**
   * El odómetro <b>del momento de la carga</b>. Es lo que ancla el galón a un tramo: sin
   * él la conciliación de `RN-30` compara un total contra otro total y no puede decir
   * <b>dónde</b> se fue la diferencia.
   */
  readonly odometro: number;
  /**
   * La referencia de la factura. <b>Nulo es un caso previsto, no un descuido</b>: `RN-85`
   * tipifica la ausencia de comprobante, y el principio es que <i>el registro del
   * abastecimiento no se omite nunca por falta de papel</i>.
   */
  readonly comprobante: string | null;
  /**
   * Por qué no hay comprobante. <b>Obligatoria cuando `comprobante` es nulo</b> — es lo que
   * distingue una ausencia declarada de un campo que nadie llenó.
   *
   * `[C]` <b>El catálogo de causas tipificadas no existe.</b> `RN-85` lo exige y la
   * institución no lo ha entregado, así que hoy es texto libre. Cuando exista, esto pasa a
   * ser una clave del catálogo y el texto queda como detalle.
   */
  readonly causaSinComprobante?: string;
  /** La fotografía del comprobante o del surtidor. Opcional: no retiene al hecho. */
  readonly foto?: FotoDeLaCarga;
}


export interface ConsumoListoParaSincronizar {
  readonly transicion: TransicionCapturada;
  /** Nulo cuando la carga no lleva foto. La ausencia de foto <b>no impide</b> el registro. */
  readonly adjunto: AdjuntoPendiente | null;
}

/**
 * Prepara una carga para el diario local — la regla de `V-04` del lado del dispositivo.
 *
 * ── Lo que este módulo comprueba, y por qué sólo eso ────────────────────────
 * <b>Únicamente lo que la persona con el surtidor delante puede corregir ahora.</b> Un
 * odómetro tecleado al revés se arregla mirando el tablero; el saldo del fondo, la
 * segregación de `BD-06` y el receptor de `RN-32` <b>no</b>, porque el dispositivo no tiene
 * esos datos y no los va a tener sin red.
 *
 * Fingir que los valida sería peor que no hacerlo: daría por conforme lo que nadie
 * comprobó. El servidor los evalúa al recibir, y lo que rechace vuelve como conflicto con
 * su motivo.
 *
 * ── Y por qué el hecho se registra aunque el servidor vaya a discutirlo ─────
 * `P-2`: el combustible <b>ya entró al tanque</b>. Negarse a registrarlo no deshace la
 * carga — la vuelve invisible, que es justo lo que `RN-83` señala como el defecto que
 * produce rendimientos imposiblemente buenos.
 */
export function prepararConsumo(
  carga: CargaCapturada,
  contexto: ContextoDelDispositivo = { ultimoOdometroConocido: null },
): ConsumoListoParaSincronizar {
  exigirDatosDeLaCarga(carga, contexto);

  if (!(carga.monto > 0))
    throw new CapturaInvalida('monto', 'Declare cuánto costó la carga.');

  // Lo del fondo SIEMPRE debería traer factura: se compró en una estación con dinero
  // público. Por eso pasa `true` y no consulta la fuente — no hay otra que valga acá.
  exigirRespaldo(true, carga.comprobante, carga.causaSinComprobante);

  const transicion: TransicionCapturada = {
    idTransicion: carga.idConsumo,
    // El expediente va porque el diario lo pide para todo hecho; el destino real de este
    // es la asignación, y viaja en los datos.
    idExpediente: carga.idExpediente,
    transicion: 'V-04',
    ejecuta: carga.ejecuta,
    ocurridoEn: carga.ocurridoEn,
    datos: {
      idAsignacion: carga.idAsignacion,
      galones: carga.galones,
      monto: carga.monto,
      estacion: carga.estacion.trim(),
      odometro: carga.odometro,
      comprobante: carga.comprobante,
      ...(carga.comprobante === null
        ? { causaSinComprobante: carga.causaSinComprobante!.trim() }
        : {}),
    },
  };

  // `RN-43` punto 3 y `ADR-004`: la foto va en **su propia cola**. Si el hecho esperara a
  // su foto, un motorista con señal intermitente no sincronizaría ni el odómetro — que
  // ocupa cuarenta bytes y es lo que la conciliación necesita.
  const adjunto: AdjuntoPendiente | null =
    carga.foto === undefined
      ? null
      : {
          idAdjunto: carga.foto.idAdjunto,
          idTransicion: carga.idConsumo,
          ruta: carga.foto.ruta,
          hash: carga.foto.hash,
          tipo: carga.foto.tipo,
          bytes: carga.foto.bytes,
          // Un comprobante de combustible es operativo: no lleva persona identificable, y
          // clasificarlo como dato personal lo metería en la depuración del hábeas data,
          // que es justo donde NO debe estar un respaldo contable.
          clasificacion: ClasificacionDeContenido.Operativo,
          capturadoEn: carga.ocurridoEn,
        };

  return { transicion, adjunto };
}

/**
 * Lo que el motorista ve del vale sin red — <b>lo que el dispositivo ya se trajo</b>.
 *
 * ⚠️ <b>El saldo no está.</b> El del fondo lo calcula el servidor sobre todas las
 * asignaciones de la institución, y un número desactualizado en la mano de quien está en
 * la estación es peor que ninguno: decidiría contra una cifra que dejó de ser cierta hace
 * cuatro días.
 */
export interface ValeEnElDispositivo {
  readonly idAsignacion: string;
  readonly folio: string;
  readonly monto: number;
  readonly tipoDeCombustible: string;
}

/**
 * Cuánto queda <b>de este vale</b>, contando sólo lo capturado en este dispositivo.
 *
 * Es una ayuda de captura, no un control: si el motorista cargó con otro instrumento o el
 * vale se movió en la oficina, este número no lo sabe. Por eso <b>no bloquea</b> — lo que
 * bloquea es la liquidación, con todos los asientos a la vista.
 */
export function remanenteDelVale(
  vale: ValeEnElDispositivo,
  cargasCapturadas: readonly CargaCapturada[],
): number {
  const consumido = cargasCapturadas
    .filter((c) => c.idAsignacion === vale.idAsignacion)
    .reduce((suma, c) => suma + c.monto, 0);

  return vale.monto - consumido;
}
