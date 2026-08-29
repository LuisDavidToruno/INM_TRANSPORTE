import { pedir } from './misiones';

/**
 * `M-18` — peajes.
 *
 * ── Ninguna tarifa se calcula acá ───────────────────────────────────────────
 * `RN-34` prohíbe la fórmula: la progresión de 2 a 9 ejes es casi lineal y por eso alguien va
 * a proponer calcularla, pero **una fórmula inferida se vuelve falsa al primer ajuste
 * asimétrico**. Es una tabla publicada y se lee del servidor.
 */

export interface TarifaVigente {
  categoria: string;
  monto: number;
  fuente: string;
  verificada: string;
  desde: string;
  /** La tarifa cambia al menos una vez al año, en enero. Se advierte, no invalida. */
  sinRevisar: boolean;
}

export interface PuntoDePeaje {
  id: string;
  nombre: string;
  operador: string;
  carretera: string;
  sentidoDeCobro: string | null;
  /**
   * Nulo es **sin estado declarado**, no «activo». Suponerlo activo estimaría de más sobre
   * una caseta que quizá cerró; suponerlo cerrado, de menos, y eso es faltante en ruta.
   */
  estado: string | null;
  fundamentoDelEstado: string | null;
  tarifas: TarifaVigente[];
}

export const puntosDePeaje = (): Promise<PuntoDePeaje[]> =>
  pedir<PuntoDePeaje[]>('/peajes/puntos');

/**
 * Un paso por caseta. **Lleva las dos categorías**: si el sistema guardara sólo la cobrada,
 * el error de la caseta se volvería la verdad institucional y el reclamo nunca ocurriría.
 */
export interface PasoPorCaseta {
  id: string;
  vehiculo: string;
  mision: string | null;
  momento: string;
  montoPagado: number;
  montoEsperado: number | null;
  /** Nula cuando no había previsión. Cero diría que pagó exactamente lo previsto. */
  diferencia: number | null;
  medio: string;
  categoriaEsperada: string | null;
  categoriaCobrada: string | null;
  discrepancia: boolean;
  ticket: string | null;
  puntoNoCatalogado: boolean;
  ubicacion: string | null;
  registra: string;
}

export const discrepanciasDePeaje = (): Promise<PasoPorCaseta[]> =>
  pedir<PasoPorCaseta[]>('/peajes/discrepancias');

/** El estado del punto, en el vocabulario de `NRM-10`. */
export const TEXTO_DE_ESTADO_DEL_PUNTO: Record<string, string> = {
  Activo: 'Cobrando',
  Suspendido: 'Cobro suspendido por resolución',
  Cerrado: 'Dejó de cobrar',
};

// ── `RN-37` — la coherencia de la secuencia ─────────────────────────────────

export interface Incoherencia {
  tipo: string;
  explicacion: string;
  pasos: string[];
  /** Falso cuando el dato de base no es confiable — un reloj de dispositivo desajustado. */
  concluyente: boolean;
  /** Hay un desvío declarado desde el campo que la cubre. **No la borra.** */
  justificada: boolean;
  justificacion: string | null;
  esHallazgo: boolean;
}

/**
 * Qué dimensiones se pudieron evaluar. **Va siempre**, y es la mitad del dictamen: un
 * resultado sin hallazgos que no pudo mirar nada no es conformidad, es silencio.
 */
export interface DimensionesEvaluadas {
  geografica: boolean;
  temporal: boolean;
  contraLaRutaAutorizada: boolean;
  contraElKilometraje: boolean;
  todas: boolean;
  porQueNo: string[];
}

/**
 * Un dictamen por **vehículo**, no por misión: en una sustitución en ruta dos vehículos pasan
 * por la misma caseta a horas distintas legítimamente.
 */
export interface CoherenciaDeUnVehiculo {
  vehiculo: string;
  pasosEvaluados: number;
  /** Sin hallazgos **y** con las cuatro dimensiones evaluadas. Las dos cosas. */
  coherente: boolean;
  dimensiones: DimensionesEvaluadas;
  incoherencias: Incoherencia[];
}

export const coherenciaDeLaMision = (mision: string): Promise<CoherenciaDeUnVehiculo[]> =>
  pedir<CoherenciaDeUnVehiculo[]>(`/peajes/coherencia/${mision}`);

/** Los cinco tipos de `RN-37` punto 2, en el vocabulario del expediente. */
export const TEXTO_DE_INCOHERENCIA: Record<string, string> = {
  SecuenciaGeograficamenteImposible: 'Salta sobre una caseta que había que cruzar',
  IntervaloInviable: 'Intervalo entre casetas no viable',
  PuntoFueraDeRutaAutorizada: 'Peaje fuera de la ruta autorizada',
  PasoDuplicado: 'Paso duplicado',
  PeajeSinKilometrajeQueLoRespalde: 'Las casetas no caben en el kilometraje declarado',
};
