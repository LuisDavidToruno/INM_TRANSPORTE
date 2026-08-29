import { pedir } from './misiones';

/**
 * M-12 — el expediente de incidente.
 *
 * ── Lo que este módulo NO ofrece, y es lo que lo define ─────────────────────
 * **Ningún campo de responsabilidad, culpa o dolo.** `RN-74` lo prohíbe en la captura, y la
 * razón está escrita en la regla: *«un motorista que acaba de tener un accidente, a la orilla de
 * la carretera, con un tercero gritándole, no está en condiciones de calificar jurídicamente lo
 * que pasó — y no le corresponde»*.
 *
 * Lo único parecido es `determinacion`, que es un **documento emitido por otra instancia** y se
 * adjunta cuando existe.
 */

export type TipoDeIncidente =
  | 'AveriaMecanica'
  | 'Accidente'
  | 'Sustraccion'
  | 'RetencionPorAutoridad'
  | 'IncapacidadDelConductor'
  | 'ViaImpracticable'
  | 'CondicionDeSeguridad'
  | 'Multa'
  | 'UsoIndebido';

/** Los cuatro que `RN-70` enumera, ni uno más. */
export type DesenlaceDeLaInterrupcion =
  | 'Continuar'
  | 'ContinuarConSustitucion'
  | 'RetornoAnticipado'
  | 'RetornoSinVehiculo';

/** Ninguno de estos estados borra el bien del registro patrimonial (`RN-75`). */
export type EstadoDelBien = 'NoRecuperado' | 'Recuperado' | 'Descargado';

export interface BienAfectado {
  id: string;
  descripcion: string;
  esElVehiculo: boolean;
  estado: EstadoDelBien;
  fechaDelHecho: string;
  /** Desde el hecho, no desde hoy: un bien de tres años no se presenta como reciente. */
  diasFuera: number;
  /** Nula es **no se sabe dónde está**, que en una sustracción es lo normal. */
  ubicacionConocida: string | null;
  autoridadCustodia: string | null;
  numeroDeExpedienteExterno: string | null;
  descargo: { numero: string; autoridad: string; fecha: string } | null;
}

export interface MovimientoDelIncidente {
  movimiento: string;
  momento: string;
  ejecuta: string;
  detalle: string | null;
}

export interface ExpedienteDeIncidente {
  id: string;
  tipo: TipoDeIncidente;
  causa: string;
  /** Cuándo pasó. */
  fechaDelHecho: string;
  momentoDelHecho: string;
  /** Cuándo se registró. `RN-70` admite captura sin ninguna conectividad. */
  momentoDeCaptura: string;
  diasEntreElHechoYLaCaptura: number;
  descripcion: string;
  registra: string;
  mision: string | null;
  vehiculo: string | null;
  ubicacion: string | null;
  /** Nulo es **no leído**, no cero. */
  odometro: number | null;
  /** `RN-70` — marca la misión como interrumpida y **no le cambia el estado**. */
  interrumpe: boolean;
  desenlace: DesenlaceDeLaInterrupcion | null;
  detalleDelDesenlace: string | null;
  /** La propiedad que le da poder de bloqueo al cierre del período (`RN-97` punto 4). */
  esInterrupcionSinDesenlace: boolean;
  responsableDeSeguimiento: string;
  plazo: string;
  constancia: { numero: string; autoridad: string; fecha: string } | null;
  /** Su ausencia no impide registrar, pero genera obligación con plazo (`RN-75`). */
  debeConstancia: boolean;
  bienes: BienAfectado[];
  gestiones: {
    fecha: string;
    descripcion: string;
    responsable: string;
    plazo: string;
  }[];
  /** El acto de OTRA instancia. SIGTI lo registra, no lo produce (`RN-74`). */
  determinacion: {
    numero: string;
    instancia: string;
    fecha: string;
    resolucion: string;
  } | null;
  movimientos: MovimientoDelIncidente[];
  resueltoEn: string | null;
  comoSeResolvio: string | null;
  estaAbierto: boolean;
}

export interface BienFueraDelAlcance {
  incidente: string;
  tipo: TipoDeIncidente;
  responsable: string;
  bien: string;
  descripcion: string;
  esElVehiculo: boolean;
  fechaDelHecho: string;
  diasFuera: number;
  ubicacionConocida: string | null;
  autoridadCustodia: string | null;
  numeroDeExpedienteExterno: string | null;
}

export const incidentes = (): Promise<ExpedienteDeIncidente[]> =>
  pedir<ExpedienteDeIncidente[]>('/incidentes');

/** `RN-75` — los bienes que siguen fuera del alcance de la institución. */
export const bienesNoRecuperados = (): Promise<BienFueraDelAlcance[]> =>
  pedir<BienFueraDelAlcance[]>('/incidentes/bienes-no-recuperados');

/**
 * `RN-70` — el desenlace de la interrupción.
 *
 * **No le cambia el estado a la misión.** El desenlace dice cómo siguió; que la Orden de Misión
 * pase a `RETORNADA` o siga en ruta lo decide su propia máquina de estados.
 */
export const registrarDesenlace = (
  id: string,
  desenlace: DesenlaceDeLaInterrupcion,
  detalle: string,
  ejecuta: string,
): Promise<void> =>
  pedir(`/incidentes/${id}/desenlace`, {
    method: 'POST',
    body: JSON.stringify({
      desenlace,
      detalle,
      ejecuta,
      momento: new Date().toISOString(),
    }),
  });
