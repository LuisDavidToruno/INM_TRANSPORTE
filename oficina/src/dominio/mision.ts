import type { Tono } from '../ui';

/**
 * El vocabulario de la Orden de Misión, del lado del cliente.
 *
 * Vive en `dominio/` y no en un módulo porque lo usan varios: la bandeja de
 * autorización, el expediente y mañana el despacho. Lo compartido baja acá; los
 * `modulos/` no se importan entre sí (`ADR-009`).
 *
 * La máquina de estados es autoridad de `docs/03-arquitectura/estados/orden-de-mision.md`.
 * Esto la **cita**, no la redefine: si divergen, manda aquel documento.
 */

export const ESTADOS = [
  'Borrador',
  'Solicitada',
  'Aprobada',
  'Programada',
  'Despachada',
  'EnRuta',
  'Retornada',
  'Liquidada',
  'Cerrada',
] as const;

export type Estado = (typeof ESTADOS)[number] | 'Rechazada' | 'Anulada' | 'CerradaConHallazgo';

/**
 * Cómo se rotula y se pinta cada estado.
 *
 * El tono es de **dato**, no de acción: `riesgo` acá significa «terminó mal», no
 * «cuidado al hacer clic».
 */
export const ROTULO_ESTADO: Record<Estado, { texto: string; tono: Tono }> = {
  Borrador: { texto: 'Borrador', tono: 'neutro' },
  Solicitada: { texto: 'Solicitada', tono: 'aviso' },
  Aprobada: { texto: 'Aprobada', tono: 'info' },
  Programada: { texto: 'Programada', tono: 'info' },
  Despachada: { texto: 'Despachada', tono: 'info' },
  EnRuta: { texto: 'En ruta', tono: 'info' },
  Retornada: { texto: 'Retornada', tono: 'info' },
  Liquidada: { texto: 'Liquidada', tono: 'ok' },
  Cerrada: { texto: 'Cerrada', tono: 'ok' },
  CerradaConHallazgo: { texto: 'Cerrada con hallazgo', tono: 'riesgo' },
  Rechazada: { texto: 'Rechazada', tono: 'riesgo' },
  Anulada: { texto: 'Anulada', tono: 'neutro' },
};

/**
 * Qué pasa en cada etapa, en una línea.
 *
 * No es decoración: el rastreador lo muestra al enfocar, y es lo que evita que
 * alguien confunda «aprobada» con «ya tiene vehículo». Aprobar no es programar.
 */
const NOTA_DE_ETAPA: Record<(typeof ESTADOS)[number], string> = {
  Borrador: 'Se captura. Todavía no la ve nadie más',
  Solicitada: 'Espera el pronunciamiento de la jefatura',
  Aprobada: 'Autorizada. Sin vehículo ni motorista asignados todavía',
  Programada: 'Con vehículo y motorista reservados',
  Despachada: 'Entregado el vehículo. Aún no sale',
  EnRuta: 'El vehículo salió',
  Retornada: 'Volvió. Falta el resultado económico',
  Liquidada: 'Con resultado económico registrado',
  Cerrada: 'Expediente cerrado',
};

/** Las etapas que el expediente recorre, para el rastreador. Las ramas no son etapas. */
export const ETAPAS_DE_MISION = ESTADOS.map((estado, indice) => ({
  id: indice,
  nombre: ROTULO_ESTADO[estado].texto,
  nota: NOTA_DE_ETAPA[estado],
}));

export function indiceDeEtapa(estado: Estado): number {
  const encontrado = (ESTADOS as readonly string[]).indexOf(estado);
  // Las ramas terminales —rechazada, anulada— no están en la línea. Se muestran
  // con su pastilla, no con una posición inventada en el rastreador.
  return encontrado;
}

/** Una transición del diario. El estado es la proyección de esto, nunca un campo. */
export interface Transicion {
  /** `T-01` a `T-22` de la tabla de transiciones. */
  id: string;
  destino: Estado;
  ejecuta: string;
  momento: string;
  motivo: string | null;
  /** El vehículo que esta transición tomó. Nulo en toda transición que no reserva. */
  vehiculoTomado: string | null;
  conductorTomado: string | null;
}

/**
 * Qué recurso tiene tomado la misión <b>ahora</b>.
 *
 * <b>La última que reservó, no la primera.</b> Una misión reasignada tiene dos o más
 * transiciones con recursos en el diario —`T-08` y luego `T-10`— y la vigente es la
 * última; tomar la primera mostraría el vehículo que ya se cambió, que es exactamente el
 * dato con el que alguien reasignaría al mismo que ya estaba.
 *
 * Devuelve nulo si nunca se reservó: una aprobada sin programar no tiene recurso.
 */
export function recursoVigente(diario: readonly Transicion[]): Transicion | null {
  for (let i = diario.length - 1; i >= 0; i--) {
    const t = diario[i]!;
    if (t.vehiculoTomado) return t;
  }
  return null;
}

export type MotivoDeReasignacion =
  | 'VehiculoATaller'
  | 'MotoristaNoDisponible'
  | 'CambioDeRequerimiento'
  | 'Consolidacion';

export type MotivoDeAnulacion =
  | 'SinFlotaDisponible'
  | 'SinMotoristaHabilitado'
  | 'CaducadaPorFaltaDeProgramacion'
  | 'DesistimientoDelSolicitante'
  | 'CausaExterna';

export interface Expediente {
  id: string;
  /** El número impreso que la institución cita en su descargo. Nunca se muestra el ULID. */
  folio: string;
  estado: Estado;
  capturadaPor: string;
  solicitanteDeDerecho: string;
  dependencia: string;
  objetoDelTraslado: string;
  destino: string;
  salidaPrevista: string;
  /** `HH:mm:ss`, o nula si el expediente es anterior al campo. */
  horaDeSalida: string | null;
  horaDeRetorno: string | null;
  retornoPrevisto: string;
  diario: Transicion[];
  validaciones: Validacion[];
  /** La ventana ya inició sin que nadie programara. Lo calcula el servidor. */
  aprobacionCaducada?: boolean;
}

/**
 * Lo que la jefatura tiene que ver **antes** de pronunciarse (`HU-009`).
 *
 * `bloqueo` y `advertencia` no son grados de lo mismo:
 *
 * - **Bloqueo** — la acción no existe. `BD-01`: el solicitante de derecho no
 *   autoriza lo suyo.
 * - **Advertencia** — la acción sigue ahí y exige acuse con motivo. `RN-50`
 *   **prohíbe bloquear** por antigüedad del espejo: una delegación con cuatro
 *   días sin enlace tiene que poder operar. Ese fue el hallazgo `HB34-03`.
 *
 * Pintarlas iguales las vuelve ruido; tratar una advertencia como bloqueo deja
 * sin operar a media institución.
 */
export interface Validacion {
  clase: 'bloqueo' | 'advertencia' | 'conforme';
  /** `BD-01`, `RN-50`… El identificador va a la vista: el usuario cita la regla. */
  regla: string;
  titulo: string;
  detalle: string;
}

export const hayBloqueo = (validaciones: Validacion[]): boolean =>
  validaciones.some((v) => v.clase === 'bloqueo');

export const advertencias = (validaciones: Validacion[]): Validacion[] =>
  validaciones.filter((v) => v.clase === 'advertencia');
