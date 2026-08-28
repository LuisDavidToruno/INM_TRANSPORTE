import { pedir } from './misiones';

/**
 * `M-09` — el fondo del período y el vale de la misión.
 *
 * ── Ninguna cifra se calcula acá ────────────────────────────────────────────
 * El saldo, lo consumido y lo devuelto vienen del servidor, donde son la resta sobre
 * asientos que `RN-26` exige. Un cliente que restara por su cuenta produciría un segundo
 * número con la misma apariencia de autoridad y con derecho a discrepar — y el que se ve en
 * pantalla es el que la gente cita.
 */

// ── El fondo ────────────────────────────────────────────────────────────────

export interface MovimientoDelFondo {
  movimiento: string;
  destino: string;
  ejecuta: string;
  momento: string;
  motivo: string | null;
  /** Sólo lo llevan `F-02` aprobar y `F-05` ampliar: son los dos actos que crean saldo. */
  monto: number | null;
}

export interface Fondo {
  id: string;
  ambito: string;
  ambitoDeclarado: string;
  desde: string;
  hasta: string;
  estado: string;
  solicita: string;
  aprueba: string | null;
  /** Nula es **pendiente**, y es lo que bloquea el cierre del período. */
  partida: string | null;
  aprobado: number;
  saldo: number;
  diario: MovimientoDelFondo[];
}

export const fondos = (): Promise<Fondo[]> => pedir<Fondo[]>('/fondos');

export const solicitarFondo = (cuerpo: {
  id: string;
  ambito: string;
  ambitoDeclarado: string;
  desde: string;
  hasta: string;
  solicita: string;
  monto: number;
  justificacion: string;
  momento: string;
}): Promise<{ id: string }> =>
  pedir<{ id: string }>('/fondos', { method: 'POST', body: JSON.stringify(cuerpo) });

export const aprobarFondo = (
  id: string,
  cuerpo: { ejecuta: string; monto: number; partida: string | null; momento: string },
): Promise<{ estado: string }> =>
  pedir<{ estado: string }>(`/fondos/${id}/aprobar`, {
    method: 'POST',
    body: JSON.stringify(cuerpo),
  });

export const ampliarFondo = (
  id: string,
  cuerpo: { ejecuta: string; monto: number; motivo: string; momento: string },
): Promise<{ estado: string }> =>
  pedir<{ estado: string }>(`/fondos/${id}/ampliar`, {
    method: 'POST',
    body: JSON.stringify(cuerpo),
  });

export const cerrarFondo = (
  id: string,
  cuerpo: { ejecuta: string; partida: string | null; momento: string },
): Promise<{ estado: string }> =>
  pedir<{ estado: string }>(`/fondos/${id}/cerrar`, {
    method: 'POST',
    body: JSON.stringify(cuerpo),
  });

// ── El vale ─────────────────────────────────────────────────────────────────

export interface ConsumoDelVale {
  galones: number;
  monto: number;
  estacion: string;
  /** El odómetro del momento: lo que ancla el galón a un tramo recorrido. */
  odometro: number;
  /** Nulo es un caso previsto — `RN-85` lo tipifica, no se omite el registro. */
  comprobante: string | null;
}

export interface TransicionDelVale {
  transicion: string;
  destino: string;
  ejecuta: string;
  momento: string;
  motivo: string | null;
  consumo: ConsumoDelVale | null;
  devuelto: number | null;
}

export interface Vale {
  id: string;
  folio: string;
  estado: string;
  instrumento: string;
  tipoDeCombustible: string;
  monto: number;
  galones: number | null;
  consumido: number;
  galonesConsumidos: number;
  devuelto: number;
  /** Lo que decide entre `T-15` y `T-16`: un solo consumo y ya no hay anulación. */
  tuvoConsumo: boolean;
  /** Ya no cuenta contra el saldo ni impide cerrar. */
  resuelta: boolean;
  diario: TransicionDelVale[];
}

export const valesDeLaMision = (misionId: string): Promise<Vale[]> =>
  pedir<Vale[]>(`/combustible/mision/${misionId}`);

export const emitirVale = (cuerpo: {
  id: string;
  folio: string;
  idFondo: string;
  idMision: string;
  /**
   * Quién está en la ventanilla, por el ULID de su registro en el padrón. `RN-32` lo
   * compara contra el motorista de la orden.
   *
   * <b>El vehículo NO viaja en la petición</b>: lo precarga el servidor desde la reserva,
   * que es lo que la regla manda. Mandarlo dejaría al cliente declarando contra qué se
   * está validando.
   */
  idMotoristaReceptor: string;
  ejecuta: string;
  monto: number;
  galones: number | null;
  instrumento: string;
  tipoDeCombustible: string;
  momento: string;
}): Promise<{ id: string }> =>
  pedir<{ id: string }>('/combustible', {
    method: 'POST',
    body: JSON.stringify(cuerpo),
  });

export const entregarVale = (
  id: string,
  cuerpo: { ejecuta: string; constancia: string; momento: string },
): Promise<{ estado: string }> =>
  pedir<{ estado: string }>(`/combustible/${id}/entregar`, {
    method: 'POST',
    body: JSON.stringify(cuerpo),
  });

/** Anular, devolver y declarar extravío: los tres exigen acta, y va en `motivo`. */
export const moverVale = (
  id: string,
  accion: 'anular' | 'devolver' | 'extravio',
  cuerpo: { ejecuta: string; motivo: string; momento: string },
): Promise<{ estado: string }> =>
  pedir<{ estado: string }>(`/combustible/${id}/${accion}`, {
    method: 'POST',
    body: JSON.stringify(cuerpo),
  });

export const registrarConsumo = (
  id: string,
  cuerpo: {
    ejecuta: string;
    galones: number;
    monto: number;
    estacion: string;
    odometro: number;
    comprobante: string | null;
    momento: string;
  },
): Promise<{ estado: string }> =>
  pedir<{ estado: string }>(`/combustible/${id}/consumo`, {
    method: 'POST',
    body: JSON.stringify(cuerpo),
  });

export const liquidarVale = (
  id: string,
  cuerpo: {
    ejecuta: string;
    saldoDevuelto: number;
    observacion: string | null;
    momento: string;
  },
): Promise<{ estado: string }> =>
  pedir<{ estado: string }>(`/combustible/${id}/liquidar`, {
    method: 'POST',
    body: JSON.stringify(cuerpo),
  });

/**
 * El dictamen de `RN-30`, calculado por el servidor.
 *
 * ── El cliente NO lo decide, y antes sí ─────────────────────────────────────
 * La petición llevaba `dentroDeUmbral`, y eso dejaba a quien concilia eligiendo si su
 * propio caso era hallazgo. Es el mismo invariante que §7.2 impone al cierre: el criterio
 * decide y la persona lo confirma con su causa.
 */
export interface DictamenDeConciliacion {
  dictamen:
    | 'NoEvaluable'
    | 'NoConcluyente'
    | 'DentroDeUmbral'
    | 'ConsumoExcesivo'
    | 'RendimientoImposible';
  esHallazgo: boolean;
  kilometros: number;
  galones: number;
  /** Nulo cuando no se pudo calcular. **No es cero**: es que no hubo con qué dividir. */
  observado: number | null;
  esperado: {
    kmPorGalon: number;
    /**
     * De dónde salió. Un dictamen contra la media del propio vehículo y otro contra el
     * valor de la institución **no valen lo mismo**, y sólo el segundo sostiene un
     * hallazgo firme.
     */
    origen: 'Institucional' | 'Fabricante' | 'PropuestoDelHistorico';
    version: string;
  } | null;
  /** Fracción sobre el esperado. Negativa es consumo de más; positiva, rendimiento de más. */
  desviacion: number | null;
  evidencia: string;
}

export const dictamenDeConciliacion = (id: string): Promise<DictamenDeConciliacion> =>
  pedir<DictamenDeConciliacion>(`/combustible/${id}/conciliacion`);

export const conciliarVale = (
  id: string,
  cuerpo: {
    ejecuta: string;
    momento: string;
    /** Sólo hace falta si el dictamen dio hallazgo. */
    causa?: string;
    odometroAveriado?: boolean;
    nivelDeTanqueDispar?: boolean;
    esperaProlongadaRegistrada?: boolean;
  },
): Promise<{ estado: string }> =>
  pedir<{ estado: string }>(`/combustible/${id}/conciliar`, {
    method: 'POST',
    body: JSON.stringify(cuerpo),
  });

/** El texto del dictamen. El identificador no se le muestra a nadie. */
export const TEXTO_DE_DICTAMEN: Record<string, string> = {
  NoEvaluable: 'No se pudo evaluar',
  NoConcluyente: 'No concluyente',
  DentroDeUmbral: 'Dentro de umbral',
  ConsumoExcesivo: 'Consumo excesivo',
  RendimientoImposible: 'Rendimiento imposible',
};

// ── Presentación compartida ─────────────────────────────────────────────────

/**
 * Los lempiras, con separador de miles y dos decimales siempre.
 *
 * <b>Los dos decimales no son cosmética.</b> «L 2,500» y «L 2,500.00» se leen distinto en un
 * cuadre: el primero parece redondeado, y quien concilia necesita saber que es exacto.
 */
export const lempiras = (monto: number): string =>
  `L ${monto.toLocaleString('es-HN', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;

export const galones = (cantidad: number): string =>
  `${cantidad.toLocaleString('es-HN', { minimumFractionDigits: 2, maximumFractionDigits: 2 })} gal`;

/**
 * El texto del estado del vale.
 *
 * El identificador del dominio es `ConciliadaConDesviacion`, y así llega. <b>Nadie que abra
 * una liquidación lee identificadores</b>; la comparación sigue siendo contra el
 * identificador, nunca contra el texto.
 */
export const TEXTO_DE_VALE: Record<string, string> = {
  Emitida: 'Emitida, sin entregar',
  Entregada: 'Entregada',
  Consumida: 'Con consumo',
  Devuelta: 'Devuelta íntegra',
  Extraviada: 'Extraviada',
  Liquidada: 'Liquidada',
  Conciliada: 'Conciliada',
  ConciliadaConDesviacion: 'Conciliada con desviación',
  Anulada: 'Anulada',
};

export const TEXTO_DE_FONDO: Record<string, string> = {
  Solicitado: 'Solicitado',
  Aprobado: 'Aprobado',
  Entregado: 'Entregado',
  Agotado: 'Agotado',
  Cerrado: 'Cerrado',
};
