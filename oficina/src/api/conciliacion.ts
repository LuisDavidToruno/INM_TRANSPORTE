import { pedir } from './misiones';

/**
 * `RN-95` — la conciliación contra fuentes externas.
 *
 * ── Por qué existe siendo que `RN-30` ya concilia ───────────────────────────
 * `RN-30` compara nuestros datos con nuestros datos: eso verifica **coherencia interna, no
 * veracidad**. Un registro completo y coherente puede ser completamente falso, y sólo la fuente
 * externa lo revela.
 */

export interface FuenteExterna {
  id: string;
  tipo: string;
  emisor: string;
  formato: string;
  responsable: string;
  /** Falso significa **«no la tenemos»**, no «pendiente». No disponible ≠ conciliada. */
  disponible: boolean;
  porQueNoEstaDisponible: string | null;
  periodicidadEnDias: number | null;
  /** Nula significa **nunca conciliada**, que no es cero días de retraso. */
  ultimaConciliacion: string | null;
  diasDesdeLaUltima: number | null;
  atrasada: boolean;
  /** El texto que `RN-95` punto 5 manda mostrar, con su razón. */
  retraso: string;
}

export const fuentesExternas = (): Promise<FuenteExterna[]> =>
  pedir<FuenteExterna[]>('/conciliacion/fuentes');

/**
 * Una diferencia — el expediente que la conciliación abre. **En ambos sentidos**: lo que la
 * fuente tiene y nosotros no, y lo que nosotros tenemos y la fuente no.
 */
export interface DiferenciaDeConciliacion {
  id: string;
  lado: 'SoloEnLaFuente' | 'SoloEnSigti';
  fechaDelHecho: string;
  monto: number;
  referencia: string | null;
  origen: string | null;
  /** Nulo es **no resuelto**: no se asigna por parecido (`RN-66`). */
  vehiculo: string | null;
  /** Cuál ancla lo resolvió. Resolver por placa admite discusión; por número de bien, no. */
  ancla: string | null;
  explicacion: string;
  responsable: string | null;
  plazo: string | null;
}

export const diferenciasDeConciliacion = (): Promise<DiferenciaDeConciliacion[]> =>
  pedir<DiferenciaDeConciliacion[]>('/conciliacion/diferencias');

export interface EjecucionDeConciliacion {
  id: string;
  fuente: string;
  desde: string;
  hasta: string;
  /** El archivo del que salieron las líneas. Sin él una diferencia no se puede recomprobar. */
  documentoFuente: string;
  /** `RN-94` — hasta qué momento se conoce lo que este resultado afirma. */
  fechaDeCorte: string;
  ejecuta: string;
  coincidentes: number;
  soloEnLaFuente: number;
  soloEnSigti: number;
  sinResolver: number;
}

export const ejecucionesDeConciliacion = (): Promise<EjecucionDeConciliacion[]> =>
  pedir<EjecucionDeConciliacion[]>('/conciliacion/ejecuciones');

export const TEXTO_DE_FUENTE: Record<string, string> = {
  EstadoDeCuentaDeCombustible: 'Estado de cuenta de combustible',
  EstadoDeCuentaDePeaje: 'Estado de cuenta de peaje',
  InfraccionesDeTransito: 'Notificaciones de infracción',
  ActasDeAutoridad: 'Dictámenes y actas de autoridad',
};

export const TEXTO_DE_LADO: Record<string, string> = {
  SoloEnLaFuente: 'El emisor lo reporta y nosotros no lo tenemos',
  SoloEnSigti: 'Nosotros lo registramos y el emisor no lo reporta',
};
