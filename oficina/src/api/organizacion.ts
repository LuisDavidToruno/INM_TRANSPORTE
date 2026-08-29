import { pedir } from './misiones';

/**
 * `M-01` — puestos, competencias y segregación de funciones.
 *
 * ── Las dos mitades, que no son intercambiables ─────────────────────────────
 * **Quién ocupa qué puesto es espejo de ARGOS** (`RN-48`, `DP-001`): se puebla por integración
 * y ninguna pantalla de SIGTI puede editarlo. **Qué facultades tiene cada puesto dentro de
 * SIGTI sí es nuestro**, porque ARGOS no sabe qué es despachar un vehículo.
 *
 * Por eso hay `POST /competencias` y no hay `POST` de ocupación.
 *
 * ── Lo que el cliente NO evalúa ─────────────────────────────────────────────
 * **La tabla de incompatibilidades.** Es §5 de `actores-y-roles.md`, *«la sección que hace o
 * deshace este sistema»*, y una copia acá diría que `I-14` bloquea el día que alguien la
 * actualice a medias. El servidor la resuelve; la pantalla la muestra.
 */

/** Los `ACT-xx`. El servidor los publica por nombre, no por número. */
export type Rol =
  | 'Administrador'
  | 'Solicitante'
  | 'JefaturaInmediata'
  | 'JefeDeTransporte'
  | 'EncargadoDeDespacho'
  | 'Motorista'
  | 'EncargadoDeCombustible'
  | 'GerenciaAdministrativa'
  | 'MaximaAutoridad'
  | 'EncargadoDeDelegacion'
  | 'EncargadoDeMantenimiento'
  | 'AuditorInterno'
  | 'CustodioDelVehiculo'
  | 'EncargadoDeBienes'
  | 'VerificadorEnCarretera';

export type AlcanceDeDatos = 'Propio' | 'Dependencia' | 'Delegacion' | 'Institucion';

export interface Competencia {
  id: string;
  rol: Rol;
  alcance: AlcanceDeDatos;
  desde: string;
  /** Nulo es **indefinido, no eterno**. */
  hasta: string | null;
  otorga: string;
  /**
   * Los pares que quedaron latentes al otorgarla.
   *
   * **Nulo es «no quedó vigilada», no «no se evaluó»**: la evaluación es obligatoria al
   * otorgar, y guardar el resultado evita recalcular la tabla entera en cada carga.
   */
  paresVigilados: string | null;
}

export interface PuestoDelPadron {
  puesto: string;
  /** Puede haber **dos durante un traspaso**: la coocupación es acotada y se registra. */
  ocupantes: string[];
  /** **Vacante no es un error**: el puesto existe aunque esté vacío, y se configura antes. */
  vacante: boolean;
  competencias: Competencia[];
  /** Cuántas se cerraron. Importan para saber que el puesto tuvo más de las que tiene. */
  cerradas: number;
  /**
   * Cuántas **todavía no empiezan**.
   *
   * «Ya no rige» y «todavía no empieza» son cosas opuestas y las dos llegan como «no
   * vigente». Una es historia; la otra es una asignación programada que alguien espera que
   * entre a funcionar.
   */
  futuras: number;
}

export const padronDePuestos = (fecha?: string): Promise<PuestoDelPadron[]> =>
  pedir<PuestoDelPadron[]>(`/competencias${fecha === undefined ? '' : `?fecha=${fecha}`}`);

/** Un par de la tabla de §5.2, resuelto sobre una persona concreta. */
export interface ParVigilado {
  par: string;
  una: string;
  otra: string;
  nivel: 'NucleoIrreductible' | 'BloqueoDuro' | 'Configurable' | 'Advertencia';
  porQue: string;
}

/**
 * Lo que una persona puede hacer — **la unión de todos sus puestos**.
 *
 * `RN-100`: los permisos efectivos son la unión de los roles de todos los puestos que ocupa
 * vigentes a esa fecha. Y por eso las incompatibilidades se evalúan sobre la persona: mirar
 * puesto por puesto es exactamente cómo se cuela la acumulación.
 */
export interface CompetenciasDeLaPersona {
  persona: string;
  fecha: string;
  puestos: string[];
  roles: Rol[];
  funciones: string[];
  /** **Nulo es «no tiene alcance», que no es `Propio`**: `Propio` ya es un permiso. */
  alcanceMaximo: AlcanceDeDatos | null;
  sinCompetencia: boolean;
  vigilados: ParVigilado[];
  quedaVigilada: boolean;
}

export const competenciasDe = (persona: string, fecha?: string): Promise<CompetenciasDeLaPersona> =>
  pedir<CompetenciasDeLaPersona>(
    `/competencias/persona/${persona}${fecha === undefined ? '' : `?fecha=${fecha}`}`,
  );

export interface CatalogoDeOrganizacion {
  roles: { rol: Rol; funciones: string[] }[];
  alcances: AlcanceDeDatos[];
  incompatibilidades: {
    par: string;
    nivel: string;
    alcance: string;
    porQue: string;
    funciones: { una: string; otra: string }[];
  }[];
}

export const catalogoDeOrganizacion = (): Promise<CatalogoDeOrganizacion> =>
  pedir<CatalogoDeOrganizacion>('/competencias/catalogo');

export interface CompetenciaNueva {
  id: string;
  puesto: string;
  rol: Rol;
  alcance: AlcanceDeDatos;
  desde: string;
  hasta: string | null;
  otorga: string;
}

/** Devuelve si quedó vigilada. **El rechazo llega como 409**, no como resultado. */
export const otorgarCompetencia = (
  nueva: CompetenciaNueva,
): Promise<{ id: string; quedaVigilada: boolean; vigilados: { par: string; porQue: string }[] }> =>
  pedir('/competencias', { method: 'POST', body: JSON.stringify(nueva) });

export const cerrarCompetencia = async (id: string, hasta: string): Promise<void> => {
  await pedir(`/competencias/${id}/cerrar`, { method: 'POST', body: JSON.stringify({ hasta }) });
};

/** El texto del rol. El identificador manda; esto sólo se muestra. */
export const TEXTO_DE_ROL: Record<string, string> = {
  Administrador: 'ACT-01 · Administrador del Sistema',
  Solicitante: 'ACT-02 · Solicitante',
  JefaturaInmediata: 'ACT-03 · Jefatura Inmediata',
  JefeDeTransporte: 'ACT-04 · Jefe de Transporte',
  EncargadoDeDespacho: 'ACT-05 · Encargado de Despacho',
  Motorista: 'ACT-06 · Motorista',
  EncargadoDeCombustible: 'ACT-07 · Encargado de Combustible',
  GerenciaAdministrativa: 'ACT-08 · Gerencia Administrativa',
  MaximaAutoridad: 'ACT-09 · Máxima Autoridad',
  EncargadoDeDelegacion: 'ACT-10 · Encargado de Delegación',
  EncargadoDeMantenimiento: 'ACT-11 · Encargado de Mantenimiento',
  AuditorInterno: 'ACT-12 · Auditor Interno',
  CustodioDelVehiculo: 'ACT-13 · Custodio del Vehículo',
  EncargadoDeBienes: 'ACT-14 · Encargado de Bienes Institucionales',
  VerificadorEnCarretera: 'ACT-15 · Verificador en Carretera',
};

/** Qué ve cada alcance — §3.1. */
export const TEXTO_DE_ALCANCE: Record<string, string> = {
  Propio: 'Sólo donde es autor, solicitante, motorista o custodio',
  Dependencia: 'Su unidad organizativa y las descendientes',
  Delegacion: 'La delegación territorial, atravesando dependencias',
  Institucion: 'Todo',
};
