import { pedir } from './misiones';

/**
 * `RN-63` — el préstamo de un vehículo es un expediente del bien.
 *
 * **Nunca una Orden de Misión.** Cedido con motorista de la institución propietaria, la tenencia
 * no se cedió: se prestó un servicio, y eso es una misión con motivo «apoyo institucional».
 */

export interface ExpedienteDePrestamo {
  id: string;
  vehiculo: string;
  acto: { folio: string; firmante: string; fecha: string };
  autoriza: string;
  /** Con cargo e institución: es lo que permite responder quién respondía por la unidad. */
  receptor: { persona: string; cargo: string; institucion: string };
  motivo: string;
  desde: string;
  devolucionComprometida: string;
  estaVigente: boolean;
  /** `RN-63` punto 4 — escalamiento diario, y `RN-97` punto 4 le da poder de bloqueo. */
  diasDeMora: number;
  estaVencido: boolean;
  /** **No entra** en la conciliación galonaje–kilometraje: no hubo consumo nuestro. */
  kilometrosBajoTenenciaAjena: number | null;
  /** Hallazgo frecuente de auditoría, y por eso se reconstata al devolver. */
  volvioSinRotulacion: boolean;
  /** Un rubro sin pactar es el que aparece cuando llega la multa. */
  rubrosSinPactar: string[];
  devolucion: {
    fecha: string;
    odometro: number;
    rotulacionConstatada: boolean;
    novedades: string | null;
    firma: string | null;
  } | null;
}

/** **El entregable de `RN-63`** punto 7: quién respondía por la unidad en una fecha. */
export interface QuienRespondia {
  fecha: string;
  /** Falso significa que respondía la institución propietaria por su custodio ordinario. */
  esTenenciaAjena: boolean;
  persona: string | null;
  cargo: string | null;
  institucion: string | null;
  prestamo: string | null;
}

export const prestamos = (): Promise<ExpedienteDePrestamo[]> =>
  pedir<ExpedienteDePrestamo[]>('/prestamos');

export const quienRespondiaPor = (vehiculo: string, fecha: string): Promise<QuienRespondia> =>
  pedir<QuienRespondia>(`/prestamos/quien-respondia/${vehiculo}/${fecha}`);
