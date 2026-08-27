import { pedir } from './misiones';
import type { MotivoDeReasignacion } from '../dominio/mision';

/**
 * Flota, conductores y evaluación de asignación.
 *
 * <b>La evaluación de `BD-02` no se repite acá.</b> Vive en `Sigti.Dominio` y el
 * cliente pide el resultado: dos implementaciones de la precondición que traslada
 * responsabilidad legal directa a quien autoriza es la peor duplicación posible de
 * este sistema — y la que nadie notaría hasta el siniestro.
 *
 * Por la misma razón el catálogo de flota viene del servidor: si el cliente tuviera
 * su propia lista, tendría que saber evaluar para mostrar el resultado al elegir.
 */

export interface FichaTecnica {
  tipoDeVehiculo: string;
  clase: string;
  pesoBrutoKg: number;
  capacidadPasajeros: number;
  llevaRemolque: boolean;
}

export interface VehiculoDeFlota {
  id: string;
  siglas: string;
  /** Nula cuando no tiene placa metálica. Es estado válido: hay desabastecimiento. */
  placa: string | null;
  ficha: FichaTecnica;
}

export interface ConductorDisponible {
  id: string;
  nombre: string;
  /** `RN-57` verifica sobre quien efectivamente conduce, esté o no en el padrón. */
  esDelPadron: boolean;
  licencia: {
    numero: string;
    categoria: string;
    vencimiento: string;
    restricciones: string[];
  };
}

/** El resultado que calcula el servidor, con toda su evidencia. */
export interface ResultadoDeAsignacion {
  habilita: boolean;
  motivo: string;
  numeroDeLicencia: string;
  categoria: string;
  venceLicencia: string;
  versionDeMatriz: string;
  finDeRangoEvaluado: string;
  /** Qué categoría sí habilitaría este vehículo. Nombrar lo que falta, no lo que sobra. */
  categoriaRequerida: string | null;
  /**
   * `BD-12`, no `BD-02`.
   *
   * <b>`Advertencia` no impide programar</b>: exige acuse. Era la condición 3 de `BD-02`
   * y bloqueaba siempre, sin excepción posible — corregido con el hallazgo `HN1-13`,
   * porque esa etiqueta venía de `DP-001 D-12`, que nunca habló de restricciones médicas.
   */
  efectoDeLaRestriccion: 'Ninguno' | 'Advertencia' | 'Bloqueo';
  restriccionEnConflicto: string | null;
  condicionQueActivaLaRestriccion: string | null;
  motivoDeDocumentacion: string;
  advertenciasDeDocumentacion: string[];
  /** Los caminos de salida, que van en la misma pantalla del rechazo. */
  conductoresQueHabilitan: string[];
  vehiculosQueHabilita: string[];
  /**
   * `BD-11` — el solapamiento. <b>Nulo cuando no hay choque.</b>
   *
   * Viene en la vista previa y no sólo en el error del guardado porque las cuatro salidas
   * que `EF-01` ofrece —consolidar, asignar otro recurso, reprogramar, escalar— se
   * deciden <b>antes</b> de apretar el botón.
   */
  conflicto: ConflictoDeReserva | null;
}

/** Quién tiene tomado el recurso. Los tres datos que `EF-01` exige mostrar. */
export interface ConflictoDeReserva {
  folio: string;
  dependencia: string;
  desde: string;
  hasta: string;
  vehiculo: boolean;
  conductor: boolean;
}

export const flota = (): Promise<VehiculoDeFlota[]> => pedir<VehiculoDeFlota[]>('/flota');

export const conductores = (): Promise<ConductorDisponible[]> =>
  pedir<ConductorDisponible[]>('/conductores');

/**
 * Evalúa sin comprometer nada. La ventana <b>no viaja</b>: sale de la solicitud, y por
 * eso quien programa no puede acortarla para que una licencia alcance.
 */
export const evaluarAsignacion = (
  idExpediente: string,
  idVehiculo: string,
  idConductor: string,
  hayConduccionNocturna: boolean,
): Promise<ResultadoDeAsignacion> =>
  pedir<ResultadoDeAsignacion>(`/misiones/${idExpediente}/evaluar-asignacion`, {
    method: 'POST',
    body: JSON.stringify({
      idVehiculo,
      idConductor,
      hayConduccionNocturna,
      momento: new Date().toISOString(),
    }),
  });

/** `T-08` — Programar. Solo viajan identificadores. */
export const programar = async (
  idExpediente: string,
  ejecuta: string,
  idVehiculo: string,
  idConductor: string,
): Promise<void> => {
  await pedir(`/misiones/${idExpediente}/programar`, {
    method: 'POST',
    body: JSON.stringify({ ejecuta, idVehiculo, idConductor, momento: new Date().toISOString() }),
  });
};

/**
 * Un tramo en que el vehículo está tomado.
 *
 * <b>Los dos extremos son inclusivos.</b> `hasta` es el último día ocupado, no el primero
 * libre: el retorno previsto es un día en que el vehículo sigue afuera.
 */
export interface BarraDeOcupacion {
  mision: string;
  folio: string;
  destino: string;
  desde: string;
  hasta: string;
  /** `Programada`, `Despachada` o `EnRuta`. Son los tres que comprometen el vehículo. */
  estado: string;
}

export interface CarrilDeVehiculo {
  vehiculo: string;
  siglas: string;
  placa: string | null;
  tipoDeVehiculo: string;
  barras: BarraDeOcupacion[];
}

export interface OcupacionDeFlota {
  desde: string;
  hasta: string;
  carriles: CarrilDeVehiculo[];
}

/**
 * Qué tiene tomado cada vehículo en la ventana.
 *
 * <b>Es una proyección del diario, no una tabla de reservas.</b> Se pide al servidor y no
 * se deriva de la lista de misiones que ya tenga el cliente: eso obligaría al cliente a
 * saber qué estados ocupan, y el día que se agregue uno habría dos respuestas.
 */
export const ocupacionDeFlota = (desde: string, hasta: string): Promise<OcupacionDeFlota> =>
  pedir<OcupacionDeFlota>(`/flota/ocupacion?desde=${desde}&hasta=${hasta}`);

export const MOTIVOS_DE_REASIGNACION: { valor: MotivoDeReasignacion; texto: string }[] = [
  { valor: 'VehiculoATaller', texto: 'El vehículo entró a taller' },
  { valor: 'MotoristaNoDisponible', texto: 'El motorista dejó de estar disponible' },
  { valor: 'CambioDeRequerimiento', texto: 'Cambió lo que hay que mover' },
  { valor: 'Consolidacion', texto: 'Se consolida con otra misión' },
];

/**
 * `T-10` — cambiar el vehículo o quien conduce <b>sin soltar la misión</b>.
 *
 * No es desprogramar y volver a programar: ese rodeo devuelve la misión a la cola —donde
 * otro puede tomarle el vehículo entre medio— y anula el folio reservado. Acá el folio no
 * cambia, porque es el mismo expediente.
 */
export const reasignar = async (
  idExpediente: string,
  ejecuta: string,
  idVehiculo: string,
  idConductor: string,
  motivo: MotivoDeReasignacion,
  comentario: string,
): Promise<void> => {
  // Sin guarda de `BASE`, como el resto de este modulo: la flota no tiene datos de
  // muestra. Una pantalla de asignacion sin servidor no puede fingir que asigno.
  await pedir(`/misiones/${idExpediente}/reasignar`, {
    method: 'POST',
    body: JSON.stringify({
      ejecuta,
      idVehiculo,
      idConductor,
      motivo,
      comentario,
      momento: new Date().toISOString(),
    }),
  });
};
