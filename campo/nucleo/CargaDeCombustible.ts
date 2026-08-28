/**
 * Lo que toda carga de combustible tiene que traer, salga de donde salga — `RN-83`.
 *
 * ── Por qué esto vive aparte ────────────────────────────────────────────────
 * Porque el motorista hace <b>el mismo acto</b> en los dos casos: mete combustible al tanque
 * y anota el tablero. Lo único que cambia es de dónde salió el galón, y eso decide qué
 * registro produce —un asiento del vale o un abastecimiento suelto—, no qué hay que teclear.
 *
 * Duplicar estas comprobaciones en los dos caminos las dejaría divergir: la primera vez que
 * alguien corrija una y no la otra, el mismo dato quedaría aceptado por una puerta y
 * rechazado por la otra.
 */

export class CapturaInvalida extends Error {
  /**
   * Qué campo hay que corregir. Va aparte del mensaje porque la pantalla del dispositivo
   * tiene que poder <b>enfocar ese campo</b>: en un teléfono, decir «revise el odómetro» sin
   * llevar el cursor ahí obliga a buscarlo con el surtidor esperando.
   */
  readonly campo: string;

  constructor(campo: string, mensaje: string) {
    super(mensaje);
    this.name = 'CapturaInvalida';
    this.campo = campo;
  }
}

/** Lo que el dispositivo sabe del vehículo antes de esta carga. */
export interface ContextoDelDispositivo {
  /**
   * La última lectura que <b>este dispositivo</b> conoce. Nula si no capturó ninguna.
   *
   * ⚠️ <b>No es la última del vehículo.</b> El dispositivo sólo conoce su propia misión;
   * la que cruza misiones la tiene el servidor, y por eso revalida al recibir. Lo que se
   * comprueba acá es lo que el motorista <b>puede corregir con el tablero delante</b>.
   */
  readonly ultimoOdometroConocido: number | null;
}

/** La fotografía del comprobante o del surtidor. Opcional: no retiene al hecho. */
export interface FotoDeLaCarga {
  readonly idAdjunto: string;
  readonly ruta: string;
  readonly hash: string;
  readonly tipo: string;
  readonly bytes: number;
}

/**
 * Galones, estación y odómetro — los tres que ninguna fuente exime.
 *
 * ── Lo que el dispositivo comprueba, y por qué sólo esto ────────────────────
 * <b>Únicamente lo que la persona con el surtidor delante puede corregir ahora.</b> Un
 * odómetro tecleado al revés se arregla mirando el tablero; el saldo del fondo, la
 * segregación de `BD-06` y el receptor de `RN-32` <b>no</b>, porque el dispositivo no tiene
 * esos datos y no los va a tener sin red.
 */
export function exigirDatosDeLaCarga(
  carga: { galones: number; estacion: string; odometro: number },
  contexto: ContextoDelDispositivo,
): void {
  if (!(carga.galones > 0))
    throw new CapturaInvalida(
      'galones',
      'Una carga de cero galones no es un abastecimiento. Si no cargó, no la registre.',
    );

  if (carga.estacion.trim() === '')
    throw new CapturaInvalida(
      'estacion',
      'Declare dónde cargó. Es lo que permite cruzar el consumo contra la ruta declarada.',
    );

  if (!Number.isInteger(carga.odometro) || carga.odometro <= 0)
    throw new CapturaInvalida(
      'odometro',
      'Declare el odómetro del momento de la carga. Sin él el galón no queda anclado a ' +
        'ningún tramo, y la conciliación no puede decir dónde se fue la diferencia.',
    );

  // `BD-05` en el dispositivo. Es un número que retrocede: físicamente imposible, y quien
  // lo tecleó tiene el tablero delante. Dejarlo pasar lo convertiría en un conflicto que
  // alguien resuelve dentro de una semana, adivinando.
  if (
    contexto.ultimoOdometroConocido !== null &&
    carga.odometro < contexto.ultimoOdometroConocido
  )
    throw new CapturaInvalida(
      'odometro',
      `El odómetro (${carga.odometro.toLocaleString('es-HN')} km) es menor que la última ` +
        `lectura de este dispositivo (${contexto.ultimoOdometroConocido.toLocaleString('es-HN')} km). ` +
        'Verifique el tablero: un odómetro que retrocede es lo que el control busca.',
    );
}

/**
 * `RN-85` — la ausencia de comprobante se registra, <b>con causa</b>.
 *
 * ── Y no se le pide papel a quien no lo tiene ───────────────────────────────
 * Una donación y el despacho del tanque de la sede <b>no generan factura</b>. Exigirles causa
 * obligaría a escribir «no aplica» en cada registro, y una casilla que siempre dice lo mismo
 * deja de leerse — con ella se pierde la que sí significaba algo.
 *
 * La causa se exige donde <b>debería haber papel</b>: una compra en estación.
 */
export function exigirRespaldo(
  deberiaTraerComprobante: boolean,
  comprobante: string | null,
  causa: string | undefined,
): void {
  if (comprobante !== null && comprobante.trim() !== '') return;
  if (!deberiaTraerComprobante) return;

  if ((causa ?? '').trim() === '')
    throw new CapturaInvalida(
      'causaSinComprobante',
      'Sin comprobante hay que declarar por qué. El registro del abastecimiento no se omite ' +
        'nunca por falta de papel, pero tampoco se disimula.',
    );
}
