import type { TransicionCapturada } from './DiarioLocal.ts';
import type { AdjuntoPendiente } from './ColaDeAdjuntos.ts';
import { ClasificacionDeContenido } from './ColaDeAdjuntos.ts';
import { exigirDatosDeLaCarga, exigirRespaldo } from './CargaDeCombustible.ts';
import type {
  ContextoDelDispositivo,
  FotoDeLaCarga,
} from './CargaDeCombustible.ts';

/**
 * De dónde salió el combustible, cuando <b>no salió del vale</b> — `RN-83`.
 *
 * ── Por qué el fondo no está acá ────────────────────────────────────────────
 * Porque ése mueve el instrumento: descuenta del vale y avanza su máquina de estados. Va por
 * `prepararConsumo`, que produce un asiento `V-04`. Ofrecerlo en esta lista crearía un galón
 * del fondo que no descontó de ningún folio.
 */
export const FuenteDeCampo = {
  /** El tanque de la sede. <b>No pasa por ningún folio</b>, y es el que más falta hace contar. */
  TanqueInstitucional: 'TanqueInstitucional',
  OtraDependencia: 'OtraDependencia',
  /** Sin monto si no lo hay. <b>Un galón sin precio sigue siendo un galón.</b> */
  Donacion: 'Donacion',
  /** Lo pagó el motorista. Genera obligación de reintegro a su favor (`RN-86`). */
  PeculioDelServidor: 'PeculioDelServidor',
  TerceroEnApoyo: 'TerceroEnApoyo',
} as const;

export type FuenteDeCampo = (typeof FuenteDeCampo)[keyof typeof FuenteDeCampo];

/**
 * Qué fuentes deberían traer factura. Las demás no la generan, y pedirla sería papeleo
 * inventado — la misma regla que el servidor aplica, escrita una vez de cada lado porque el
 * dispositivo tiene que poder decidirlo <b>sin red</b>.
 */
export function deberiaTraerComprobante(fuente: FuenteDeCampo): boolean {
  return fuente === FuenteDeCampo.PeculioDelServidor;
}

/**
 * Una carga que no salió del vale, capturada en la estación o en el predio.
 */
export interface AbastecimientoCapturado {
  /** ULID del dispositivo (`ADR-005`). Es lo que hace inofensivo el reenvío. */
  readonly idAbastecimiento: string;
  /**
   * A qué tanque entró. <b>Es lo único que no puede faltar</b>: `RN-83` cuelga el
   * abastecimiento del vehículo, no de la misión.
   */
  readonly idVehiculo: string;
  /**
   * A qué misión sirvió. <b>Opcional</b>: la regla aplica <i>«en misión o fuera de ella»</i>, y
   * el reabastecimiento de rutina en el predio no tiene expediente al que colgarse.
   */
  readonly idExpediente?: string;
  readonly ejecuta: string;
  /** La fecha del <b>hecho</b>, no la de captura ni la de sincronización (`P-4`, `RN-46`). */
  readonly ocurridoEn: string;
  readonly fuente: FuenteDeCampo;
  readonly galones: number;
  readonly estacion: string;
  /** El odómetro del momento: lo que ancla el galón a un tramo. */
  readonly odometro: number;
  /** Nulo cuando la fuente no lo tiene. Una donación no trae precio. */
  readonly monto: number | null;
  readonly comprobante: string | null;
  /**
   * Por qué no lo hay. Obligatoria <b>sólo si la fuente debería traerlo</b> — `RN-85`.
   *
   * `[C]` El catálogo de causas tipificadas no existe: hoy es texto libre.
   */
  readonly causaSinComprobante?: string;
  readonly foto?: FotoDeLaCarga;
}

export interface AbastecimientoListoParaSincronizar {
  readonly transicion: TransicionCapturada;
  /** Nulo cuando la carga no lleva foto. La ausencia de foto <b>no impide</b> el registro. */
  readonly adjunto: AdjuntoPendiente | null;
}

/**
 * El identificador de este hecho en el protocolo de sincronización.
 *
 * ⚠️ <b>No es una transición de ninguna máquina de estados.</b> Un abastecimiento no mueve un
 * expediente ni un vale: es un registro. Viaja por el mismo canal que las transiciones porque
 * eso es lo que da <b>una sola cola, una sola idempotencia y un solo acuse</b> — abrirle un
 * endpoint propio duplicaría los tres, y son justamente los tres que `RNF-03` obliga a que
 * funcionen sin fallo.
 */
export const HECHO_DE_ABASTECIMIENTO = 'A-01';

/**
 * Prepara una carga de otra fuente para el diario local — `RN-83` del lado del dispositivo.
 *
 * ── El galón que hoy desaparece ─────────────────────────────────────────────
 * El motorista que llena de una donación camino a La Mosquitia, o que pone de su bolsillo
 * porque el vale no alcanzó, <b>no tiene dónde anotarlo</b>. Ese galón no llega nunca al
 * denominador de `RN-30`, y su ausencia se lee como un rendimiento imposiblemente bueno —
 * es decir, como si alguien hubiera despachado combustible sin registrarlo.
 *
 * Que es verdad. Lo que faltaba era poder registrarlo <b>donde ocurre</b>, que es sin red.
 */
export function prepararAbastecimiento(
  carga: AbastecimientoCapturado,
  contexto: ContextoDelDispositivo = { ultimoOdometroConocido: null },
): AbastecimientoListoParaSincronizar {
  exigirDatosDeLaCarga(carga, contexto);
  exigirRespaldo(
    deberiaTraerComprobante(carga.fuente),
    carga.comprobante,
    carga.causaSinComprobante,
  );

  const transicion: TransicionCapturada = {
    idTransicion: carga.idAbastecimiento,
    // Cadena vacía cuando no hay misión: el reabastecimiento de rutina no tiene expediente, y
    // **no se le inventa uno**. El servidor lo trata como abastecimiento sin misión.
    idExpediente: carga.idExpediente ?? '',
    transicion: HECHO_DE_ABASTECIMIENTO,
    ejecuta: carga.ejecuta,
    ocurridoEn: carga.ocurridoEn,
    datos: {
      idVehiculo: carga.idVehiculo,
      fuente: carga.fuente,
      galones: carga.galones,
      estacion: carga.estacion.trim(),
      odometro: carga.odometro,
      monto: carga.monto,
      comprobante: carga.comprobante,
      ...(carga.causaSinComprobante === undefined
        ? {}
        : { causaSinComprobante: carga.causaSinComprobante.trim() }),
    },
  };

  // `RN-43` punto 3 y `ADR-004`: la foto va en **su propia cola**. Si el hecho esperara a su
  // foto, un motorista con señal intermitente no sincronizaría ni el odómetro — que ocupa
  // cuarenta bytes y es lo que la conciliación necesita.
  const adjunto: AdjuntoPendiente | null =
    carga.foto === undefined
      ? null
      : {
          idAdjunto: carga.foto.idAdjunto,
          idTransicion: carga.idAbastecimiento,
          ruta: carga.foto.ruta,
          hash: carga.foto.hash,
          tipo: carga.foto.tipo,
          bytes: carga.foto.bytes,
          // Operativo: un comprobante de combustible no lleva persona identificable, y
          // clasificarlo como dato personal lo metería en la depuración del hábeas data —
          // que es justo donde NO debe estar un respaldo contable.
          clasificacion: ClasificacionDeContenido.Operativo,
          capturadoEn: carga.ocurridoEn,
        };

  return { transicion, adjunto };
}

/**
 * Cuántos galones entraron al tanque según <b>este dispositivo</b>, contando todas las fuentes.
 *
 * Es lo que el motorista necesita ver para saber si le falta anotar algo: si el tanque recibió
 * sesenta galones y el dispositivo sólo conoce veinte, hay cuarenta que nadie va a poder
 * explicar cuando se concilie.
 *
 * ⚠️ <b>No es el numerador de `RN-30`.</b> Ése lo arma el servidor con todo lo sincronizado,
 * incluido lo que registró la oficina. Éste es lo que hay en la mano.
 */
export function galonesCapturados(
  cargasDelVale: readonly { readonly galones: number }[],
  abastecimientos: readonly AbastecimientoCapturado[],
): number {
  return (
    cargasDelVale.reduce((suma, c) => suma + c.galones, 0) +
    abastecimientos.reduce((suma, a) => suma + a.galones, 0)
  );
}
