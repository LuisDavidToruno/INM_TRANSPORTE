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
  /** §10.2. <b>Nulo</b> cuando nunca se declaró — que no es lo mismo que disponible. */
  estado: string | null;
  /**
   * Si el vehículo no se puede comprometer — taller, no disponible, prestado o terminal.
   *
   * <b>Lo calcula el servidor.</b> La lista de estados inutilizables es de `BD-07`, y
   * duplicarla acá la dejaría divergir del bloqueo: el cronograma pintaría disponible un
   * vehículo que no se puede programar, y quien programa lo descubriría al guardar.
   */
  inutilizable: boolean;
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

/** Una misión en el tablero del día, con los nombres ya resueltos por el servidor. */
export interface MisionDelDia {
  mision: string;
  folio: string;
  estado: string;
  dependencia: string;
  destino: string;
  objetoDelTraslado: string;
  /** Las siglas. <b>Nulo</b> si la reserva apunta a un vehículo que ya no está en la flota. */
  vehiculo: string | null;
  motorista: string | null;
  salida: string;
  retorno: string;
  /** `HH:mm:ss`. <b>Nula</b> en los expedientes anteriores al campo. */
  horaDeSalida: string | null;
  horaDeRetorno: string | null;
  diasDeAtraso: number;
}

/**
 * `PT-038` — lo que el despachador tiene enfrente hoy.
 *
 * <b>Cuatro listas y no una tabla ordenable.</b> Son cuatro acciones distintas con cuatro
 * urgencias distintas, y la cuarta es la que ninguna lista ordenada por fecha muestra sola:
 * un retorno vencido no aparece «arriba», aparece en el pasado.
 */
export interface DiaDeDespacho {
  fecha: string;
  salenHoy: MisionDelDia[];
  vuelvenHoy: MisionDelDia[];
  afuera: MisionDelDia[];
  atrasadas: MisionDelDia[];
}

export const diaDeDespacho = (fecha: string): Promise<DiaDeDespacho> =>
  pedir<DiaDeDespacho>(`/despacho/dia?fecha=${fecha}`);

/** Un vehículo del padrón — `PT-072`, con lo que se pregunta al abrirlo. */
export interface VehiculoDelPadron {
  id: string;
  siglas: string;
  placa: string | null;
  ficha: FichaTecnica;
  venceMatricula: string;
  vencePoliza: string | null;
  venceRevisionMecanica: string | null;
  /** §10.2. <b>Nulo</b> es «nunca se declaró», no «disponible». */
  estado: string | null;
  /** <b>Nulo</b> es sin custodio, y `BD-13` lo bloquea al despachar. */
  custodio: string | null;
  excepcion: { tipo: string; desde: string; hasta: string | null } | null;
}

export const padronDeFlota = (): Promise<VehiculoDelPadron[]> =>
  pedir<VehiculoDelPadron[]>('/flota');

/** Un cambio de estado operativo, con quién y por qué. */
export interface CambioDeEstado {
  estado: string;
  momento: string;
  ejecuta: string;
  motivo: string | null;
  /** Si lo fijó el sistema por una transición de la misión, o lo declaró una persona. */
  automatico: boolean;
}

export interface EstadoDelVehiculo {
  actual: string | null;
  historial: CambioDeEstado[];
}

export const estadoDelVehiculo = (id: string): Promise<EstadoDelVehiculo> =>
  pedir<EstadoDelVehiculo>(`/flota/${id}/estado`);

/**
 * Los estados que una persona <b>puede</b> declarar — §10.2.
 *
 * <b>`ASIGNADO` y `EN_MISION` no están, y no es una omisión</b>: los fija el sistema como
 * consecuencia de una transición de la Orden de Misión. Ofrecerlos abriría la puerta a un
 * vehículo «en misión» sin misión que lo respalde, y el servidor los rechaza igual.
 */
export const ESTADOS_DECLARABLES: { valor: string; texto: string; terminal: boolean }[] = [
  { valor: 'Disponible', texto: 'Disponible', terminal: false },
  { valor: 'EnTaller', texto: 'En taller', terminal: false },
  { valor: 'NoDisponible', texto: 'No disponible', terminal: false },
  { valor: 'Prestado', texto: 'Prestado a otra dependencia', terminal: false },
  { valor: 'DadoDeBaja', texto: 'Dado de baja — descargo de un bien propio', terminal: true },
  { valor: 'RetiradoDeFlota', texto: 'Retirado de flota — fin de tenencia de un bien ajeno', terminal: true },
];

export const declararEstado = async (
  id: string,
  ejecuta: string,
  estado: string,
  motivo: string,
): Promise<void> => {
  await pedir(`/flota/${id}/estado`, {
    method: 'POST',
    body: JSON.stringify({ ejecuta, estado, motivo, momento: new Date().toISOString() }),
  });
};
