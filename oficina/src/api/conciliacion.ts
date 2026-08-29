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

// ── `RN-93` — el expediente de hallazgo posterior ───────────────────────────

/**
 * Un asiento reverso — §8.3. **Lleva los tres valores**: original, reverso y resultado. Nunca
 * sólo el resultado.
 */
export interface AsientoReverso {
  id: string;
  naturaleza: string;
  /** El identificador exacto. **No existe el reverso genérico «de la misión».** */
  asientoRevertido: string;
  tipoDeAsiento: string;
  descripcion: string;
  valorAnterior: string;
  /** Nulo significa **sin valor correcto conocido**, que es distinto de no declararlo. */
  valorNuevo: string | null;
  efectoEconomico: number | null;
  periodoAfectado: string;
  /** El corriente. Los históricos ya publicados siguen siendo reproducibles. */
  periodoDeImputacion: string;
  motivo: string;
  fundamento: string;
  autoriza: string;
  cadena: string;
}

/**
 * El expediente. **Ni su apertura ni su resolución alteran el objeto vinculado**: una misión
 * `CERRADA` no se reabre, ni por auditoría.
 */
export interface HallazgoPosterior {
  id: string;
  tipo: string;
  /** Cuándo ocurrió. **La antigüedad se cuenta desde acá.** */
  fechaDelHecho: string;
  /** Cuándo se descubrió. Campo distinto, y ambos obligatorios. */
  fechaDelDescubrimiento: string;
  antiguedadEnDias: number;
  /** Cuánto tardó en descubrirse. Es un indicador por sí mismo. */
  diasHastaElDescubrimiento: number;
  comoSeDescubrio: string;
  fuente: string;
  documentoAdjunto: string | null;
  /** Cero, una o varias. **Cero es el caso interesante.** */
  misiones: string[];
  vehiculo: string | null;
  motorista: string | null;
  periodo: string | null;
  abierto: boolean;
  resolucion: string | null;
  fundamento: string | null;
  reversos: number;
  efectoEconomicoTotal: number;
}

export const hallazgosPosteriores = (): Promise<HallazgoPosterior[]> =>
  pedir<HallazgoPosterior[]>('/hallazgos');

export const hallazgosDeLaMision = (mision: string): Promise<HallazgoPosterior[]> =>
  pedir<HallazgoPosterior[]>(`/hallazgos/mision/${mision}`);

export const TEXTO_DE_RESOLUCION: Record<string, string> = {
  ConAsientoReverso: 'Resuelto con asiento reverso',
  SinEfectoEconomico: 'Real, sin efecto económico',
  SinEfecto: 'Sin efecto — era un error del descubridor',
};

// ── `RN-97` — el saldo de apertura de control interno ───────────────────────

export interface RenglonDelSaldo {
  tipo: string;
  referencia: string;
  descripcion: string;
  /** La del hecho **original**. La antigüedad no se reinicia con el cambio de ejercicio. */
  fechaDelHecho: string;
  antiguedadEnDias: number;
  causa: string;
  responsable: string;
  estado: string;
  /** En cuántos saldos anteriores ya venía. Lo que impide presentarlo como nuevo cada enero. */
  saldosAnteriores: number;
  monto: number | null;
  /** `RN-97` punto 4: ningún período se cierra con préstamos vencidos ni interrupciones. */
  impideCerrar: boolean;
}

/**
 * Una fuente del inventario. **Va aunque no se haya podido consultar**: un saldo que omite en
 * silencio los préstamos vencidos es el abandono que la regla existe para impedir, con formato
 * de reporte.
 */
export interface FuenteDelSaldo {
  tipo: string;
  sePudoConsultar: boolean;
  renglones: number;
  porQueNo: string | null;
}

export interface InventarioDelSaldo {
  corte: string;
  renglones: RenglonDelSaldo[];
  fuentes: FuenteDelSaldo[];
  /** Falso cuando alguna fuente no se pudo consultar. */
  completo: boolean;
  bloqueantes: number;
}

export const inventarioDelSaldo = (corte: string): Promise<InventarioDelSaldo> =>
  pedir<InventarioDelSaldo>(`/saldo-de-apertura/inventario/${corte}`);

export interface SaldoDeApertura {
  id: string;
  folio: string;
  ejercicio: string;
  corte: string;
  produce: string;
  momento: string;
  renglones: number;
  /** Los que ya venían de saldos anteriores. **Son los que más importan.** */
  arrastrados: number;
  antiguedadMaximaEnDias: number;
  montoTotal: number;
  bloqueantes: number;
  /** El primero tras el despliegue: no se compara contra los siguientes. */
  esInicialDeImplantacion: boolean;
}

export const saldosDeApertura = (): Promise<SaldoDeApertura[]> =>
  pedir<SaldoDeApertura[]>('/saldo-de-apertura');

export const TEXTO_DE_RENGLON: Record<string, string> = {
  MisionSinCerrar: 'Orden de misión sin cerrar',
  ValeSinLiquidar: 'Vale sin liquidar',
  ObligacionDeReintegro: 'Obligación de reintegro',
  HallazgoPosteriorAbierto: 'Hallazgo posterior abierto',
  ImputacionExternaNoResuelta: 'Imputación externa no resuelta',
  PrestamoVencido: 'Préstamo vencido',
  InterrupcionSinDesenlace: 'Interrupción sin desenlace',
  ReclamoDePeaje: 'Reclamo de peaje',
  ExpedienteDeIncidente: 'Expediente de incidente',
  BitacoraPendienteDeDigitacion: 'Bitácora sin digitar',
};

export const TEXTO_DE_CAUSA_DEL_RENGLON: Record<string, string> = {
  PendienteDeGestionInterna: 'pendiente de gestión interna',
  FueraDelControlInstitucional: 'fuera del control institucional',
  BienNoRecuperado: 'bien no recuperado',
  SaldoInicialDeImplantacion: 'saldo inicial de implantación',
};
