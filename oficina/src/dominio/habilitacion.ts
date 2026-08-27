/**
 * `BD-02` del lado del cliente — la habilitación de quien efectivamente conduce.
 *
 * Estos tipos son el espejo de `ResultadoDeHabilitacion` en `Sigti.Dominio`. El
 * cálculo **no se repite acá**: la matriz es parámetro con vigencia y evaluarla en
 * dos lados es garantizar que un día difieran. El cliente muestra lo que el
 * servidor resolvió, con toda su evidencia.
 */

/** Las nueve del Artículo 4 del Acuerdo 1012-2021. `BE` es B enganchada a remolque. */
export type CategoriaDeLicencia = 'A' | 'B1' | 'B' | 'C1' | 'C' | 'D1' | 'D' | 'BE' | 'CE';

/** Lo que el Artículo 4 distingue por clase y no por umbral. */
export type ClaseNormativa =
  | 'Motocicleta'
  | 'TricicloCuadriciclo'
  | 'Automovil'
  | 'Camion'
  | 'Autobus';

export const ROTULO_CLASE: Record<ClaseNormativa, string> = {
  Motocicleta: 'motocicleta',
  TricicloCuadriciclo: 'triciclo o cuadriciclo',
  Automovil: 'automóvil liviano',
  Camion: 'camión',
  Autobus: 'autobús',
};

export interface Vehiculo {
  id: string;
  /** El correlativo institucional. Puede no haber placa: hay desabastecimiento nacional. */
  siglas: string;
  placa: string | null;
  clase: ClaseNormativa;
  tipo: string;
  pesoBrutoKg: number;
  capacidadPasajeros: number;
  llevaRemolque: boolean;
}

export interface Conductor {
  id: string;
  nombre: string;
  /** Del padrón de `M-05`, o declarado: `RN-57` verifica sobre quien efectivamente conduce. */
  esDelPadron: boolean;
  numeroDeLicencia: string;
  categoria: CategoriaDeLicencia;
  venceLicencia: string;
  restricciones: string[];
}

/**
 * Las tres causas de `BD-02`, separadas porque **se resuelven distinto**.
 *
 * Es la razón por la que `PT-028` es difícil: el usuario no puede resolverlo
 * reintentando. Si el mensaje no dice exactamente qué falta, va a probar otra vez
 * con la misma persona, después va a llamar por teléfono, y después va a sacar el
 * vehículo sin orden de misión.
 */
export type MotivoDeNoHabilitacion =
  | 'Ninguno'
  | 'CategoriaNoHabilitaElVehiculo'
  | 'LicenciaVenceDentroDelRango'
  | 'RestriccionMedicaIncompatible';

/**
 * El resultado con **todos sus insumos**.
 *
 * «Guardar solo "verificado" no defiende a nadie» — `BD-02`. Se conserva igual
 * cuando la evaluación es favorable, y `versionDeMatriz` va porque la matriz es
 * parámetro con vigencia y el rechazo tiene que ser reproducible (`R-7`).
 */
export interface ResultadoDeHabilitacion {
  habilita: boolean;
  motivo: MotivoDeNoHabilitacion;
  numeroDeLicencia: string;
  categoria: CategoriaDeLicencia;
  venceLicencia: string;
  versionDeMatriz: string;
  finDeRangoEvaluado: string;
  /** Qué categoría sí habilitaría este vehículo. Nombrar lo que falta, no solo lo que sobra. */
  categoriaRequerida: CategoriaDeLicencia | null;
  /** La restricción concreta que la misión contradice, cuando ese es el motivo. */
  restriccionEnConflicto: string | null;
}

/** Los caminos de salida, que van en la misma pantalla del rechazo. */
export interface Alternativas {
  conductoresQueHabilitan: Conductor[];
  vehiculosQueHabilita: Vehiculo[];
}

export interface Asignacion {
  vehiculo: Vehiculo;
  conductor: Conductor;
  resultado: ResultadoDeHabilitacion;
  alternativas: Alternativas;
}

export const descripcionDelVehiculo = (v: Vehiculo): string =>
  `${v.siglas} · ${v.tipo}, ${v.pesoBrutoKg.toLocaleString('es-HN')} kg` +
  (v.llevaRemolque ? ', con remolque' : '');
