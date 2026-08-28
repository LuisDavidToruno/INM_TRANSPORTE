import type { TransicionCapturada } from './DiarioLocal.ts';
import { CapturaInvalida } from './CargaDeCombustible.ts';
import type { ContextoDelDispositivo } from './CargaDeCombustible.ts';

/**
 * La escala con que se leyó el tanque — `RN-83` punto 2.
 *
 * <b>Se registra</b>, y no es un detalle: <i>«un octavo de tanque no es lo mismo en un pickup
 * que en un bus»</i>. Dos lecturas de escalas distintas no se restan sin la capacidad del
 * tanque, que la ficha técnica no declara.
 */
export const EscalaDeNivel = {
  /** Fracción del indicador: de 0 a 1. <b>0.125 es un octavo</b>, que es lo que la aguja da. */
  FraccionDelIndicador: 'FraccionDelIndicador',
  /** Galones, cuando el instrumento los da. */
  Galones: 'Galones',
} as const;

export type EscalaDeNivel = (typeof EscalaDeNivel)[keyof typeof EscalaDeNivel];

export interface NivelDeTanque {
  readonly escala: EscalaDeNivel;
  readonly valor: number;
}

/**
 * ¿Se leyó el tanque, o no?
 *
 * ── Por qué esto es un tipo y no un número opcional ─────────────────────────
 * Porque <b>«no lo leí» y «marcaba cero» son cosas opuestas</b>, y un campo numérico vacío no
 * las distingue. `RN-80` es explícita: el campo no consignado <b>se declara</b> y no se
 * estima. Un cero silencioso diría que el vehículo salió con el tanque vacío.
 *
 * Obligar a elegir entre las dos es lo que impide que el motorista deje el campo en blanco y
 * el sistema lo interprete solo.
 */
export type LecturaDelTanque =
  | { readonly leido: true; readonly nivel: NivelDeTanque }
  | {
      readonly leido: false;
      /**
       * Por qué no se leyó. El indicador averiado, la hoja mojada, la prisa del predio a las
       * cinco de la mañana — lo que sea, pero dicho.
       */
      readonly porQueNo: string;
    };

/** Cómo volvió el vehículo — decide si `BD-05` bloquea o sólo marca (`RN-79`, `HB3-04`). */
export const SubtipoDeRetorno = {
  /** Lo registra quien conducía. Una lectura menor que la de salida es imposible: se detiene. */
  Ordinario: 'Ordinario',
  /**
   * Lo constata un tercero con el vehículo ya en el predio. <b>No se bloquea</b>: negarse a
   * registrarlo dejaría el vehículo secuestrado por un trámite mientras la delegación se
   * queda sin unidad.
   */
  Constatado: 'Constatado',
} as const;

export type SubtipoDeRetorno = (typeof SubtipoDeRetorno)[keyof typeof SubtipoDeRetorno];

export interface SalidaCapturada {
  readonly idSalida: string;
  readonly idExpediente: string;
  readonly ejecuta: string;
  /** La fecha del <b>hecho</b>, no la de captura (`P-4`, `RN-46`). */
  readonly ocurridoEn: string;
  readonly odometro: number;
  readonly tanque: LecturaDelTanque;
}

export interface RetornoCapturado {
  readonly idRetorno: string;
  readonly idExpediente: string;
  readonly ejecuta: string;
  readonly ocurridoEn: string;
  readonly odometro: number;
  readonly tanque: LecturaDelTanque;
  readonly subtipo: SubtipoDeRetorno;
  /**
   * Obligatoria cuando la lectura de retorno <b>iguala</b> la de salida en una misión que se
   * ejecutó. No bloquea, pero no pasa en silencio: <i>es el patrón de la misión que nunca se
   * hizo</i>, y ése es el que busca el Tribunal Superior de Cuentas.
   */
  readonly justificacion?: string;
}

/**
 * `T-14` — el vehículo sale del predio.
 *
 * ── Por qué el nivel se pide acá y no después ───────────────────────────────
 * Porque después no existe. Sin el nivel de salida, <i>«salió lleno y volvió vacío»</i> no se
 * distingue de un faltante, y la conciliación de una misión corta con tanque grande no
 * significa nada. `RN-83` lo hace <b>dato obligatorio de bitácora</b>, y la bitácora se llena
 * en el predio, con el vehículo delante.
 */
export function prepararSalida(salida: SalidaCapturada): TransicionCapturada {
  exigirOdometro(salida.odometro);
  exigirLectura(salida.tanque);

  return {
    idTransicion: salida.idSalida,
    idExpediente: salida.idExpediente,
    transicion: 'T-14',
    ejecuta: salida.ejecuta,
    ocurridoEn: salida.ocurridoEn,
    datos: {
      odometro: salida.odometro,
      ...datosDelTanque(salida.tanque),
    },
  };
}

/**
 * `T-18` — el vehículo vuelve.
 *
 * ── El único bloqueo que el dispositivo se permite ──────────────────────────
 * Una lectura de retorno <b>menor</b> que la de salida es físicamente imposible, y quien la
 * tecleó tiene el tablero delante. Se detiene en el predio, no en la oficina una semana
 * después, donde ya nadie puede volver a mirar.
 *
 * <b>Salvo en el retorno constatado</b>, donde el vehículo ya está en el predio y lo registra
 * un tercero: ahí negarse lo dejaría secuestrado por un trámite. Se anota tal cual y se marca.
 */
export function prepararRetorno(
  retorno: RetornoCapturado,
  contexto: ContextoDelDispositivo = { ultimoOdometroConocido: null },
): TransicionCapturada {
  exigirOdometro(retorno.odometro);
  exigirLectura(retorno.tanque);

  const salida = contexto.ultimoOdometroConocido;

  if (
    salida !== null &&
    retorno.odometro < salida &&
    retorno.subtipo === SubtipoDeRetorno.Ordinario
  )
    throw new CapturaInvalida(
      'odometro',
      `El odómetro de retorno (${retorno.odometro.toLocaleString('es-HN')} km) es menor que el ` +
        `de salida (${salida.toLocaleString('es-HN')} km). Es físicamente imposible: verifique ` +
        'el tablero antes de cerrar la bitácora.',
    );

  // No bloquea, pero no pasa en silencio. El vehículo que vuelve con la misma lectura con que
  // salió no recorrió un solo kilómetro, y eso hay que explicarlo en el momento — dentro de
  // una semana nadie se acuerda.
  if (
    salida !== null &&
    retorno.odometro === salida &&
    (retorno.justificacion ?? '').trim() === ''
  )
    throw new CapturaInvalida(
      'justificacion',
      'El odómetro de retorno iguala al de salida: la misión no recorrió un solo kilómetro. ' +
        'Se puede registrar, pero exige justificación — es el patrón de la misión que nunca se hizo.',
    );

  return {
    idTransicion: retorno.idRetorno,
    idExpediente: retorno.idExpediente,
    transicion: 'T-18',
    ejecuta: retorno.ejecuta,
    ocurridoEn: retorno.ocurridoEn,
    datos: {
      odometro: retorno.odometro,
      subtipo: retorno.subtipo,
      ...(retorno.justificacion === undefined
        ? {}
        : { justificacion: retorno.justificacion.trim() }),
      ...datosDelTanque(retorno.tanque),
    },
  };
}

/**
 * Cuánto se movió el tanque entre las dos lecturas.
 *
 * ── Nulo es «no se puede saber», y no se disfraza ───────────────────────────
 * Devuelve nulo cuando falta una lectura o cuando las escalas no se comparan. <b>Dar por
 * parejo lo que no se midió</b> es justo lo que `RN-80` prohíbe, y acá se nota más que en
 * ningún lado: el remanente inventado después no se distingue de uno medido.
 */
export function diferenciaDeTanque(
  salida: LecturaDelTanque,
  retorno: LecturaDelTanque,
): number | null {
  if (!salida.leido || !retorno.leido) return null;
  if (salida.nivel.escala !== retorno.nivel.escala) return null;

  return retorno.nivel.valor - salida.nivel.valor;
}

// ── Lo compartido ───────────────────────────────────────────────────────────

function exigirOdometro(odometro: number): void {
  if (!Number.isInteger(odometro) || odometro <= 0)
    throw new CapturaInvalida(
      'odometro',
      'Declare el odómetro. Es el único ancla que el sistema tiene para detectar consumo de ' +
        'combustible sin relación con el uso, y es el hallazgo típico del Tribunal en flota.',
    );
}

/**
 * Que la lectura del tanque sea <b>una de las dos cosas</b>: un nivel, o una ausencia con su
 * razón. Lo que no se admite es el silencio.
 */
function exigirLectura(tanque: LecturaDelTanque): void {
  if (!tanque.leido) {
    if (tanque.porQueNo.trim() === '')
      throw new CapturaInvalida(
        'tanque',
        'Si no leyó el tanque, diga por qué. El campo no consignado se declara y no se ' +
          'estima: un cero silencioso diría que el vehículo salió vacío.',
      );

    return;
  }

  const { escala, valor } = tanque.nivel;

  if (!(valor >= 0))
    throw new CapturaInvalida('tanque', 'El nivel del tanque no puede ser negativo.');

  if (escala === EscalaDeNivel.FraccionDelIndicador && valor > 1)
    throw new CapturaInvalida(
      'tanque',
      'En fracción del indicador el nivel va de 0 a 1: un tanque lleno es 1, y un octavo ' +
        'es 0.125. Si lo quiere en galones, cambie la escala.',
    );
}

function datosDelTanque(tanque: LecturaDelTanque): Record<string, unknown> {
  // Sin nivel viaja la razón, no un hueco. El servidor lo registra como no consignado, y el
  // diario dice por qué — que es lo que `RN-80` pide y lo que permite reclamarlo después.
  return tanque.leido
    ? { nivelDeTanque: tanque.nivel.valor, escalaDelNivel: tanque.nivel.escala }
    : { nivelDeTanque: null, tanqueNoConsignado: tanque.porQueNo.trim() };
}
