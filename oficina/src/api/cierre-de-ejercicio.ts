import { pedir } from './misiones';

/**
 * `RN-96` — el cierre de ejercicio como corte de imputación y de reporte.
 *
 * ── Lo que este módulo NO ofrece, y es su definición ────────────────────────
 * **Ninguna función que cierre misiones.** `RN-96`: *«no ejecuta ni habilita ninguna transición
 * de la Orden de Misión. Ningún expediente cambia de estado por efecto de una fecha»*.
 *
 * La regla nombra el riesgo: *«sin esta regla escrita la primera implementación va a poner un
 * cierre masivo por fecha, porque es lo que resuelve ese problema»*. Y ante el Tribunal Superior
 * de Cuentas, *«cincuenta expedientes cerrados el 31 de diciembre a la misma hora con el mismo
 * motivo **son el hallazgo**, no su solución»*.
 */

/** Un hecho económico imputado al ejercicio de **su propia fecha**, no a la de la misión. */
export interface HechoImputado {
  ejercicio: string;
  fechaDelHecho: string;
  concepto: string;
  monto: number;
  /** Con qué se valoró. **Nula es el hueco real**: sin ella el cálculo no se puede rehacer. */
  tablaParametrica: string | null;
}

/**
 * Una misión que cruzó el corte. **No se divide**: lo que se reparte entre ejercicios son sus
 * hechos.
 */
export interface MisionQueCruza {
  mision: string;
  referencia: string;
  salida: string;
  retorno: string | null;
  porEjercicio: Record<string, number>;
  hechos: HechoImputado[];
  sinTablaParametrica: number;
}

/**
 * Un folio reservado y no consumido al corte.
 *
 * `sePuedeAnular` es falso para el vale **entregado**: `V-03` no corre sobre él. Eso es dinero
 * fuera de la caja al cierre — un problema mayor que el folio ocioso, no menor.
 */
export interface FolioPorAnular {
  asignacion: string;
  folio: string;
  delegacion: string;
  monto: number;
  emitido: string;
  estado: string;
  sePuedeAnular: boolean;
}

/**
 * Un cambio de parámetro dentro de la ventana de cierre.
 *
 * `RN-96` punto 6: *«es la evidencia de que **nadie aflojó un umbral en diciembre para cerrar
 * limpio**, o de que alguien lo hizo y quedó a la vista»*.
 */
export interface CambioDeParametro {
  clave: string;
  /** Nulo cuando es la primera versión de la clave. **No es cero**: es que no había antes. */
  valorAnterior: string | null;
  valorNuevo: string;
  vigenteDesde: string;
  registrado: string;
  cargadoPor: string;
  aprobadoPor: string | null;
}

/** Dos o más misiones cerradas con el mismo motivo — lo que `RN-96` punto 3 prohíbe. */
export interface MotivoCompartido {
  motivo: string;
  misiones: string[];
  primero: string;
  ultimo: string;
  /** **Minutos son peor que días**: el mismo motivo en una hora es un cierre en bloque. */
  ventanaEnMinutos: number;
}

/**
 * La ventana de cierre — `RN-96`, **parámetro con vigencia**.
 *
 * Resuelta a la fecha del corte legal, no a hoy: reevaluar el cierre de 2026 usa la ventana que
 * regía entonces.
 */
export interface VentanaDeCierre {
  desde: string;
  hasta: string;
  dias: number;
  /** De qué versión del parámetro salió, con su vigencia. Sin esto no se puede reproducir. */
  origen: string;
}

/** Por qué no se pudo resolver. **Se declara, no se sustituye por un valor razonable.** */
export interface VentanaSinResolver {
  clave: string;
  porQueNo: string;
}

export interface CierreApurado {
  cerradasEnLaVentana: number;
  cerradasEnElAnio: number;
  diasDeLaVentana: number;
  promedioDiarioEnLaVentana: number;
  /** Nulo cuando no hay cierres fuera de la ventana con que comparar. **Nulo no es cero.** */
  promedioDiarioDelAnio: number | null;
  /** Cuántas veces el ritmo del año. Nulo cuando el indicador no se puede evaluar. */
  veces: number | null;
}

export interface ActaDeCierre {
  id: string;
  folio: string;
  ejercicio: string;
  corteLegal: string;
  corteOperativo: string;
  ejecuta: string;
  momento: string;
  inventario: number;
  diferenciasConElSaldo: string[];
  misionesQueCruzan: MisionQueCruza[];
  foliosPorAnular: FolioPorAnular[];
  montoPorAnular: number;
  cambiosDeParametros: CambioDeParametro[];
  motivosCompartidos: MotivoCompartido[];
  /** La ventana contra la que se midieron los motivos compartidos y el ritmo de cierre. */
  ventana: VentanaDeCierre | null;
  /** Presente exactamente cuando `ventana` es nula. */
  sinVentana: VentanaSinResolver | null;
  /** **Nulo cuando no hay ventana**: el indicador no se evaluó, que no es lo mismo que cero. */
  apuro: CierreApurado | null;
  /** El saldo que el acta cita. **Nulo es que no hay saldo producido**, no que cuadró. */
  saldoDeAperturaFolio: string | null;
  observaciones: string[];
}

export interface ActaProducida {
  ejercicio: string;
  folio: string;
  corteLegal: string;
  corteOperativo: string;
  folios: number;
  anulados: number;
  monto: number;
  /** El saldo que el acta cita. **Nulo es que no había con qué cuadrar**, no que cuadró. */
  saldoDeAperturaFolio: string | null;
}

export const vistaPreviaDelCierre = (
  ejercicio: string,
  corteLegal: string,
  corteOperativo: string,
): Promise<ActaDeCierre> =>
  pedir<ActaDeCierre>(
    `/cierre-de-ejercicio/${ejercicio}/vista-previa` +
      `?corteLegal=${corteLegal}&corteOperativo=${corteOperativo}`,
  );

export const actasDeCierre = (): Promise<ActaProducida[]> =>
  pedir<ActaProducida[]>('/cierre-de-ejercicio');

export const producirActa = (cuerpo: {
  folio: string;
  ejercicio: string;
  corteLegal: string;
  corteOperativo: string;
  persona: string;
  puesto: string;
}): Promise<ActaDeCierre> =>
  pedir<ActaDeCierre>('/cierre-de-ejercicio', {
    method: 'POST',
    body: JSON.stringify({ ...cuerpo, momento: new Date().toISOString() }),
  });

/**
 * `RN-96` punto 5 — anular los folios que el acta listó.
 *
 * **Va aparte de producir el acta a propósito.** Un documento que anulara decenas de folios al
 * producirse sería un cierre masivo por fecha con otro nombre, un nivel más abajo.
 */
export const anularFolios = (
  ejercicio: string,
  persona: string,
  motivo: string,
): Promise<{ anulados: number }> =>
  pedir<{ anulados: number }>(`/cierre-de-ejercicio/${ejercicio}/anular-folios`, {
    method: 'POST',
    body: JSON.stringify({ persona, motivo, momento: new Date().toISOString() }),
  });
