import { pedir } from './misiones';

/**
 * `M-05` — el padrón de motoristas y la matriz que los habilita.
 *
 * ── Lo que este módulo NO hace ──────────────────────────────────────────────
 * **No decide qué habilita una categoría.** La matriz licencia↔vehículo sostiene `BD-02`, que
 * traslada responsabilidad legal directa a quien autoriza; derivarla acá —aunque fuera «sólo
 * para mostrar»— produciría dos implementaciones de la misma precondición, y la que se ve en
 * pantalla sería la que nadie verifica. El servidor la resuelve y acá se pinta.
 */

/** Las nueve categorías del Artículo 4 del Acuerdo 1012-2021. **No existe ninguna `DE`.** */
export type CategoriaDeLicencia = 'A' | 'B1' | 'B' | 'C1' | 'C' | 'D1' | 'D' | 'BE' | 'CE';

export interface Licencia {
  numero: string;
  categoria: string;
  vencimiento: string;
  /**
   * **Sólo si las hay, nunca cuáles.**
   *
   * `RN-52`: quien despacha ve que hay restricción, no el diagnóstico. El dato médico no sale
   * del expediente de Talento Humano.
   */
  tieneRestricciones: boolean;
}

export interface Motorista {
  id: string;
  nombre: string;
  /**
   * `RN-57` verifica sobre quien efectivamente conduce, esté o no en el padrón.
   *
   * <b>Falso no es «irregular»</b>: es una persona habilitada que no es motorista de planta.
   */
  esDelPadron: boolean;
  licencia: Licencia;
}

export const motoristas = (): Promise<Motorista[]> => pedir<Motorista[]>('/conductores');

/** Un vehículo de la flota que una categoría habilita. */
export interface VehiculoHabilitado {
  id: string;
  siglas: string;
  tipo: string;
}

export interface FilaDeMatriz {
  categoria: string;
  /** Los vehículos **reales** de la flota que esa categoría habilita a la fecha. */
  habilita: VehiculoHabilitado[];
}

/**
 * La matriz resuelta contra la flota real, no contra vehículos de muestra.
 *
 * `clasesEnLaFlota` es lo que permite distinguir <b>«ninguno porque no tenemos autobuses»</b> de
 * <b>«ninguno porque el umbral no alcanza»</b>. Sin ese dato, una fila vacía se lee como que la
 * categoría no sirve para nada, que es falso.
 */
export interface MatrizDeLicencias {
  fecha: string;
  /** Qué versión de la matriz respaldó la respuesta. */
  version: string;
  categorias: FilaDeMatriz[];
  clasesEnLaFlota: string[];
  vehiculosEnLaFlota: number;
}

/**
 * **Se pide a una fecha.** La matriz es parámetro con vigencia: preguntar qué habilita la `B`
 * sin decir cuándo no tiene una sola respuesta.
 */
export const matrizDeLicencias = (fecha?: string): Promise<MatrizDeLicencias> =>
  pedir<MatrizDeLicencias>(`/matriz-de-licencias${fecha === undefined ? '' : `?fecha=${fecha}`}`);

/**
 * A qué clase normativa corresponde cada categoría, según el Artículo 4.
 *
 * **Es texto de la norma, no una regla.** No decide nada: acompaña la fila para que quien mira
 * la matriz sepa qué dice el Acuerdo sin salir de la pantalla. Lo que habilita o no lo contesta
 * el servidor. `[V]` Artículo 4 del Acuerdo 1012-2021.
 */
export const QUE_DICE_LA_NORMA: Record<string, string> = {
  A: 'Ciclomotores y motocicletas, de motor o eléctricas.',
  B1: 'Todo tipo de triciclos y cuadriciclos de motor.',
  B: 'Livianos hasta 3,500 kg, para no más de ocho personas además del conductor.',
  BE: 'Automóviles de la categoría B enganchados a un remolque.',
  C1: 'No comprendidos en B, hasta 7,500 kg.',
  C: 'Vehículos de carga superiores a 7,500 kg, no articulados.',
  CE: 'Categoría C enganchada a remolque o semirremolque.',
  D1: 'Autobuses hasta 25 pasajeros.',
  D: 'Autobuses superiores a 26 pasajeros.',
};

/** El nombre de la clase como la escribe el dominio, para poder decir qué falta en la flota. */
export const CLASE_EN_PALABRAS: Record<string, string> = {
  Motocicleta: 'motocicletas',
  TricicloCuadriciclo: 'triciclos ni cuadriciclos',
  Automovil: 'automóviles',
  Camion: 'camiones',
  Autobus: 'autobuses',
};

/** Qué clase pide cada categoría — para explicar una fila vacía sin inventar la regla. */
export const CLASE_QUE_PIDE: Record<string, string> = {
  A: 'Motocicleta',
  B1: 'TricicloCuadriciclo',
  B: 'Automovil',
  BE: 'Automovil',
  C1: 'Camion',
  C: 'Camion',
  CE: 'Camion',
  D1: 'Autobus',
  D: 'Autobus',
};
