import type { Asignacion, Conductor, Vehiculo } from '../dominio/habilitacion';

/**
 * Flota y padrón de muestra.
 *
 * Los tres casos de rechazo de `BD-02` están representados **con su salida**, que
 * es lo que `PT-028` tiene que resolver: el usuario no puede reintentar, tiene que
 * hacer una gestión administrativa distinta según la causa.
 */

const enDias = (dias: number): string => {
  const f = new Date();
  f.setDate(f.getDate() + dias);
  f.setHours(23, 59, 0, 0);
  return f.toISOString();
};

export const VEHICULOS: Vehiculo[] = [
  {
    id: 'v-001',
    siglas: 'INS-P-014',
    placa: 'PBM8842',
    clase: 'Automovil',
    tipo: 'Pick-up doble cabina',
    pesoBrutoKg: 2_800,
    capacidadPasajeros: 5,
    llevaRemolque: false,
  },
  {
    id: 'v-002',
    siglas: 'INS-C-002',
    // Sin placa metálica: es estado válido, hay desabastecimiento nacional.
    placa: null,
    clase: 'Camion',
    tipo: 'Camión de carga',
    pesoBrutoKg: 12_000,
    capacidadPasajeros: 3,
    llevaRemolque: false,
  },
  {
    id: 'v-003',
    siglas: 'INS-P-021',
    placa: 'PCH1190',
    clase: 'Automovil',
    tipo: 'Pick-up con plataforma enganchada',
    pesoBrutoKg: 3_100,
    capacidadPasajeros: 5,
    llevaRemolque: true,
  },
];

export const CONDUCTORES: Conductor[] = [
  {
    id: 'c-001',
    nombre: 'José Ramón Cruz',
    esDelPadron: true,
    numeroDeLicencia: '08-1988-77120',
    categoria: 'B',
    venceLicencia: enDias(400),
    restricciones: [],
  },
  {
    id: 'c-002',
    nombre: 'Óscar Banegas',
    esDelPadron: true,
    numeroDeLicencia: '05-1979-31288',
    categoria: 'C',
    venceLicencia: enDias(620),
    restricciones: [],
  },
  {
    id: 'c-003',
    nombre: 'Elmer Sauceda',
    esDelPadron: true,
    numeroDeLicencia: '01-1991-44907',
    categoria: 'B',
    // Vence dentro de la ventana de la misión: el caso que más se olvida.
    venceLicencia: enDias(5),
    restricciones: [],
  },
  {
    id: 'c-004',
    nombre: 'Nery Portillo',
    esDelPadron: true,
    numeroDeLicencia: '03-1985-20411',
    categoria: 'BE',
    venceLicencia: enDias(510),
    restricciones: [],
  },
  {
    id: 'c-005',
    nombre: 'Dilcia Amaya',
    esDelPadron: false,
    numeroDeLicencia: '08-1994-10233',
    categoria: 'B',
    venceLicencia: enDias(700),
    restricciones: ['No conducir en horario nocturno'],
  },
];

/**
 * Evalúa como lo haría el servidor. **Es una imitación para la muestra**, no la
 * regla: la de verdad vive en `Sigti.Dominio` y es la única que decide. Cuando
 * `VITE_API` apunte al servidor, esto no se usa.
 */
export function evaluar(
  vehiculo: Vehiculo,
  conductor: Conductor,
  finDeRango: string,
  hayConduccionNocturna: boolean,
): Asignacion {
  const requerida = categoriaRequerida(vehiculo);
  const categoriaAlcanza = conductor.categoria === requerida;
  const venceAntes = new Date(conductor.venceLicencia) < new Date(finDeRango);
  const restriccion = hayConduccionNocturna
    ? (conductor.restricciones.find((r) => r.toLowerCase().includes('nocturn')) ?? null)
    : null;

  const motivo = !categoriaAlcanza
    ? 'CategoriaNoHabilitaElVehiculo'
    : venceAntes
      ? 'LicenciaVenceDentroDelRango'
      : restriccion
        ? 'RestriccionMedicaIncompatible'
        : 'Ninguno';

  return {
    vehiculo,
    conductor,
    resultado: {
      habilita: motivo === 'Ninguno',
      motivo,
      numeroDeLicencia: conductor.numeroDeLicencia,
      categoria: conductor.categoria,
      venceLicencia: conductor.venceLicencia,
      versionDeMatriz: 'ACUERDO-1012-2021-ART-4',
      finDeRangoEvaluado: finDeRango,
      categoriaRequerida: categoriaAlcanza ? null : requerida,
      restriccionEnConflicto: motivo === 'RestriccionMedicaIncompatible' ? restriccion : null,
    },
    alternativas: {
      conductoresQueHabilitan: CONDUCTORES.filter(
        (c) =>
          c.id !== conductor.id &&
          c.categoria === requerida &&
          new Date(c.venceLicencia) >= new Date(finDeRango) &&
          !(hayConduccionNocturna && c.restricciones.some((r) => r.toLowerCase().includes('nocturn'))),
      ),
      vehiculosQueHabilita: VEHICULOS.filter(
        (v) => v.id !== vehiculo.id && categoriaRequerida(v) === conductor.categoria,
      ),
    },
  };
}

/** Qué categoría exige este vehículo, según el Artículo 4. */
function categoriaRequerida(v: Vehiculo): Asignacion['resultado']['categoria'] {
  if (v.clase === 'Motocicleta') return 'A';
  if (v.clase === 'TricicloCuadriciclo') return 'B1';
  if (v.clase === 'Autobus') return v.capacidadPasajeros <= 25 ? 'D1' : 'D';
  if (v.clase === 'Camion') return v.llevaRemolque ? 'CE' : v.pesoBrutoKg <= 7_500 ? 'C1' : 'C';
  return v.llevaRemolque ? 'BE' : 'B';
}
