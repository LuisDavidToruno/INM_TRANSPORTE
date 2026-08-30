import { INVENTARIO } from './inventario.generado';
import type { PantallaInventariada, SituacionDePantalla } from './tipos';

/**
 * Qué pantallas del inventario están construidas, y dónde viven.
 *
 * ── Por qué esto se escribe a mano ──────────────────────────────────────────
 * El inventario se genera del documento porque el documento es la autoridad sobre **qué
 * pantallas existen**. Esto es lo contrario: una afirmación sobre **qué hay en el código**, y
 * derivarla del documento sería dar por construida una pantalla porque está inventariada. Sólo
 * entra acá lo que se puede abrir y usar hoy.
 *
 * ── Una ruta puede cubrir varias pantallas ──────────────────────────────────
 * El inventario cuenta pantallas del diseño y la aplicación cuenta rutas, y no son lo mismo: la
 * asignación resuelve `PT-026`, `PT-027`, `PT-028` y `PT-031` en un solo recorrido, porque
 * partirlas obligaría a quien programa a ir y volver entre cuatro páginas para tomar una sola
 * decisión.
 */

interface Construida {
  readonly ruta: string;
  /** Dónde vive, para poder saltar del mapa al archivo. */
  readonly archivo: string;
  /**
   * Qué le falta, si le falta algo. **Presente = `parcial`.**
   *
   * Se nombra en concreto: «hecha a medias» sin decir qué mitad no se puede planificar.
   */
  readonly incompleta?: string;
}

export const CONSTRUIDAS: Readonly<Record<string, Construida>> = {
  // ── M-06 Autorización ─────────────────────────────────────────────────────
  'PT-013': { ruta: '/autorizacion', archivo: 'M06_Autorizacion/Bandeja.tsx' },
  'PT-014': { ruta: '/autorizacion/:id', archivo: 'M06_Autorizacion/Expediente.tsx' },
  'PT-015': {
    ruta: '/autorizacion/:id',
    archivo: 'M06_Autorizacion/Expediente.tsx',
    incompleta: 'La constancia se registra en el diario; falta su versión imprimible con folio.',
  },
  'PT-016': { ruta: '/autorizacion/:id', archivo: 'M06_Autorizacion/Expediente.tsx' },
  'PT-017': {
    ruta: '/autorizacion/:id',
    archivo: 'M06_Autorizacion/Expediente.tsx',
    incompleta: 'Devuelve para corrección, pero no versiona: el reenvío pisa la solicitud.',
  },
  'PT-019': {
    ruta: '/autorizacion/:id',
    archivo: 'M06_Autorizacion/Expediente.tsx',
    incompleta: 'La delegación de firma no existe: M-01 no está construido.',
  },

  // ── M-07 Programación y despacho ──────────────────────────────────────────
  'PT-025': { ruta: '/programacion', archivo: 'M07_Programacion/Cola.tsx' },
  'PT-026': { ruta: '/programacion/:id', archivo: 'M07_Programacion/Asignacion.tsx' },
  'PT-027': {
    ruta: '/programacion/:id',
    archivo: 'M07_Programacion/Asignacion.tsx',
    incompleta: 'Declara al titular. Los relevos se registran en ruta y no acá.',
  },
  'PT-028': { ruta: '/programacion/:id', archivo: 'M07_Programacion/RechazoPorLicencia.tsx' },
  'PT-029': {
    ruta: '/programacion/:id',
    archivo: 'M07_Programacion/ConflictoDeAgenda.tsx',
    incompleta: 'Muestra el conflicto y sus salidas; la reserva exclusiva por prioridad no existe.',
  },
  'PT-031': { ruta: '/programacion/:id', archivo: 'M07_Programacion/Asignacion.tsx' },
  'PT-032': {
    ruta: '/programacion/:id',
    archivo: 'M07_Programacion/Asignacion.tsx',
    incompleta: 'Sustituye en PROGRAMADA. La sustitución con la misión DESPACHADA es PT-033.',
  },
  'PT-038': { ruta: '/despacho', archivo: 'M07_Programacion/Tablero.tsx' },

  // ── M-09 Combustible ──────────────────────────────────────────────────────
  'PT-045': { ruta: '/combustible', archivo: 'M09_Combustible/Fondos.tsx' },
  'PT-046': {
    ruta: '/combustible',
    archivo: 'M09_Combustible/Fondos.tsx',
    incompleta: 'Amplía el fondo; la resolución de la prelación entre solicitudes no está.',
  },
  'PT-050': { ruta: '/combustible', archivo: 'M09_Combustible/PanelDeArqueo.tsx' },
  'PT-051': { ruta: '/combustible', archivo: 'M09_Combustible/PanelDeAbastecimientos.tsx' },

  // ── M-13 Liquidación y cierre ─────────────────────────────────────────────
  'PT-063': { ruta: '/cierre', archivo: 'M13_Cierre/Cola.tsx' },
  'PT-064': {
    ruta: '/cierre/:id',
    archivo: 'M13_Cierre/Cierre.tsx',
    incompleta: 'Concilia galonaje contra kilometraje; el inventario la marca «difícil §7.4».',
  },
  'PT-066': { ruta: '/cierre/:id', archivo: 'M13_Cierre/Cierre.tsx' },
  'PT-068': { ruta: '/cierre/:id', archivo: 'M13_Cierre/Cierre.tsx' },
  'PT-069': { ruta: '/cierre/:id', archivo: 'M13_Cierre/Cierre.tsx' },
  'PT-070': { ruta: '/cierre/:id', archivo: 'M13_Cierre/Cierre.tsx' },

  // ── M-03 Flota ────────────────────────────────────────────────────────────
  'PT-072': { ruta: '/flota', archivo: 'M03_Flota/Padron.tsx' },
  'PT-074': {
    ruta: '/titulos',
    archivo: 'M03_Flota/Titulos.tsx',
    // Está entre las 29 bloqueadas por el insumo #2 y se construyó igual, sin el formato a
    // la vista. Lo que hay es la cobertura de títulos y su registro; el alta del vehículo con
    // sus campos de papel no está, y puede haber que rehacer el diálogo cuando aparezca.
    incompleta:
      'Bloqueada por el insumo #2 y construida igual: cubre régimen, vigencia y rubros, ' +
      'no el formato de alta del vehículo. Puede haber que rehacerla contra el papel.',
  },
  'PT-078': {
    ruta: '/flota',
    archivo: 'M03_Flota/Padron.tsx',
    incompleta: 'Muestra el vencimiento de matrícula. Las alertas dirigidas al puesto son de M-01.',
  },
  'PT-079': {
    ruta: '/flota',
    archivo: 'M03_Flota/Padron.tsx',
    incompleta: 'Habilita por estado operativo; la verificación documental de alta no está.',
  },
  'PT-081': {
    ruta: '/flota',
    archivo: 'M03_Flota/Padron.tsx',
    incompleta:
      'El retiro se declara y `HB3-17` lo juzga contra el título. Falta el acta de devolución ' +
      'con odómetro, que es el formato en papel del insumo #2.',
  },

  'PT-073': { ruta: '/flota/:id', archivo: 'M03_Flota/Expediente.tsx' },
  'PT-075': {
    ruta: '/flota/:id',
    archivo: 'M03_Flota/Expediente.tsx',
    incompleta:
      'Muestra la placa y declara que su ausencia es estado válido. La identificación del ' +
      'Estado —franjas, leyenda, correlativo— no se captura: es PT-124, del cliente de campo.',
  },
  'PT-076': { ruta: '/flota/:id', archivo: 'M03_Flota/Expediente.tsx' },

  // ── M-01 Organización y seguridad ─────────────────────────────────────────
  'PT-096': {
    ruta: '/organizacion',
    archivo: 'M01_Organizacion/Puestos.tsx',
    incompleta:
      'Muestra puestos, ocupantes y competencias. Los usuarios como credencial no existen: ' +
      'no hay autenticación todavía.',
  },
  'PT-097': { ruta: '/organizacion', archivo: 'M01_Organizacion/Puestos.tsx' },

  // ── M-01, las transversales de `R-1` y `R-2` ──────────────────────────────
  'PT-001': {
    ruta: '/ingreso',
    archivo: 'M01_Organizacion/Ingreso.tsx',
    incompleta:
      'Resuelve la selección de puesto vigente de R-1, pero NO autentica: nadie verifica ' +
      'que quien elige un puesto tenga derecho a ocuparlo.',
  },
  'PT-002': { ruta: '/inicio', archivo: 'M01_Organizacion/InicioDelPuesto.tsx' },
  'PT-004': {
    ruta: '/bloqueos',
    archivo: 'ui/Bloqueo.tsx',
    incompleta:
      'El patrón administrativo de R-3 está: tres partes y sin botón de continuar. Falta la ' +
      'versión de campo, que el dictamen pide aparte y necesita el cliente de campo.',
  },
  'PT-005': {
    ruta: '/buscar',
    archivo: 'M01_Organizacion/Buscador.tsx',
    incompleta:
      'Aplica el alcance de datos, que antes no filtraba nada. Falta el corte por objeto ' +
      'de §3.3 —alcance distinto sobre misiones y sobre vehículos—, insumo #104.',
  },

  // ── M-01, la bandeja ──────────────────────────────────────────────────────
  //
  // `PT-003` ya estaba construida sin saberlo: la bandeja de tareas escaladas de §5.3.B.3 **es**
  // «Bandeja de tareas escaladas por segregación de funciones». Se descubrió al buscar el
  // siguiente bloque, mirando el inventario en vez de la lista de pendientes.
  'PT-003': { ruta: '/tareas', archivo: 'M01_Organizacion/Bandeja.tsx' },

  // ── M-19 Seguimiento en Ruta ──────────────────────────────────────────────
  //
  // Sin cliente de campo todavía: los reportes entran por la API. Las dos pantallas ya
  // muestran lo que llegue, y **declaran que no hay dato** cuando no lo hay — que es
  // exactamente lo que `HU-057` pide que no se disimule.
  'PT-058': {
    ruta: '/seguimiento',
    archivo: 'M19_Seguimiento/Tablero.tsx',
    incompleta:
      'Muestra la antigüedad de cada dato, pero no puede degradarlo: el umbral de ' +
      'RN-50 no está fijado (insumo #68). El mapa lo aporta ARGOS — DP-001.',
  },
  'PT-059': { ruta: '/seguimiento/:id', archivo: 'M19_Seguimiento/EnRuta.tsx' },

  // ── M-14 Auditoría ────────────────────────────────────────────────────────
  'PT-088': {
    ruta: '/pista',
    archivo: 'M14_Auditoria/Pista.tsx',
    incompleta:
      'Junta las tres fuentes que hoy existen y declara las dos que faltan: los actos en ' +
      'régimen de excepción no se registran, y no hay vista transversal de transiciones.',
  },
  'PT-089': { ruta: '/rastro', archivo: 'M14_Auditoria/RastroDelExpediente.tsx' },
  'PT-092': { ruta: '/parametros-normativos', archivo: 'M14_Auditoria/ParametrosNormativos.tsx' },
  'PT-091': {
    ruta: '/intentos-bloqueados',
    archivo: 'M14_Auditoria/IntentosBloqueados.tsx',
    incompleta:
      'Registra los intentos con su par y su origen. El escalamiento de §5.3.B.3 todavía no ' +
      'encola nada: los dos primeros saltos exigen la jerarquía de puestos, que el espejo no trae.',
  },

  // ── M-05 Motoristas y habilitación ────────────────────────────────────────
  'PT-082': { ruta: '/motoristas', archivo: 'M05_Motoristas/Padron.tsx' },
  'PT-084': { ruta: '/motoristas/matriz', archivo: 'M05_Motoristas/Matriz.tsx' },
  'PT-085': {
    ruta: '/motoristas',
    archivo: 'M05_Motoristas/Padron.tsx',
    incompleta: 'Alerta en la pantalla. La alerta dirigida al puesto necesita M-01.',
  },

  // ── M-19 Seguimiento en ruta ──────────────────────────────────────────────
  'PT-061': {
    ruta: '/incidentes',
    archivo: 'M12_Incidentes/Incidentes.tsx',
    incompleta: 'Recibe la interrupción y su desenlace. El resto de M-12 no tiene historias.',
  },
};

/**
 * Pantallas construidas que **el inventario no tiene**.
 *
 * No es una lista de sobras: cada una salió de una regla que el Bloque 3 escribió después del
 * inventario, y su ausencia allá es un hueco del documento, no del código. §7.1 lo dice de dos
 * de ellas —*«M-11 y M-12 más allá del registro en ruta: el Bloque 3 no escribió historias»*,
 * *«M-18 en su faceta de administración del catálogo: sin pantalla propia hasta que haya
 * historias»*— y de las otras cuatro no dice nada, porque son posteriores.
 */
export const SIN_INVENTARIAR: readonly {
  ruta: string;
  nombre: string;
  archivo: string;
  porQue: string;
}[] = [
  {
    ruta: '/prestamos',
    nombre: 'Préstamos de vehículo',
    archivo: 'M03_Flota/Prestamos.tsx',
    porQue: '`RN-63` es posterior al inventario. Ninguna fila cubre la cesión de tenencia.',
  },
  {
    ruta: '/peajes',
    nombre: 'Catálogo de peajes y tarifas',
    archivo: 'M18_Peajes/Peajes.tsx',
    porQue: '§7.1: M-18 «sin pantalla propia hasta que haya historias». Se construyó igual.',
  },
  {
    ruta: '/incidentes',
    nombre: 'Expedientes de incidente',
    archivo: 'M12_Incidentes/Incidentes.tsx',
    porQue: '§7.1: M-12 más allá del registro en ruta no tiene historias. Cubre `PT-061`.',
  },
  {
    ruta: '/conciliacion',
    nombre: 'Conciliación con fuentes externas',
    archivo: 'M14_Auditoria/Conciliacion.tsx',
    porQue: '`RN-88` y siguientes son posteriores al inventario.',
  },
  {
    ruta: '/saldo-de-apertura',
    nombre: 'Saldo de apertura del período',
    archivo: 'M14_Auditoria/SaldoDeApertura.tsx',
    porQue: '`RN-93` es posterior al inventario.',
  },
  {
    ruta: '/cierre-de-ejercicio',
    nombre: 'Cierre de ejercicio',
    archivo: 'M14_Auditoria/CierreDeEjercicio.tsx',
    porQue: '`RN-96` es posterior al inventario.',
  },
];

/**
 * `PT-139` — reservado, **no forma parte de las 138**.
 *
 * El inventario: *«el cronograma de flota semanal, el diseño lo dibujó y no está en este
 * inventario […] Si el PO lo acepta, entra como `PT-139`; el ID queda reservado y no se usa
 * para otra cosa»*. **Ya está construido de hecho** —`LineaDeCarriles` vive en el tablero y en
 * la asignación—, así que hay una pantalla en uso esperando una decisión que nunca se tomó.
 */
export const RESERVADA_PT_139 = {
  id: 'PT-139',
  nombre: 'Cronograma de flota semanal',
  ruta: '/despacho',
  archivo: 'ui/LineaDeCarriles.tsx',
} as const;

/** Dónde se abre una pantalla, si se puede abrir. */
export function rutaDe(id: string): string | null {
  return CONSTRUIDAS[id]?.ruta ?? null;
}

/**
 * En qué situación está.
 *
 * ── El orden de las preguntas importa ───────────────────────────────────────
 * Lo construido gana a todo lo demás: una pantalla que existe **no está bloqueada** aunque el
 * inventario diga que le falta el formato — `PT-074` es justamente ese caso, y decir que está
 * bloqueada cuando se puede abrir sería falso.
 */
export function situacionDe(p: PantallaInventariada): SituacionDePantalla {
  const construida = CONSTRUIDAS[p.id];
  if (construida) return construida.incompleta === undefined ? 'construida' : 'parcial';

  // El cliente de campo no tiene ninguna interfaz: `campo/` es núcleo y pruebas. Contarlas
  // como «pendientes» de la oficina las pondría en una cola donde no se pueden trabajar.
  if (p.cliente === 'C') return 'campo';

  if (p.papel === 'Sí') return 'bloqueada';

  return 'pendiente';
}

/** El inventario con su situación resuelta. */
export const PANTALLAS = INVENTARIO.map((p) => ({
  ...p,
  situacion: situacionDe(p),
  ruta: rutaDe(p.id),
  incompleta: CONSTRUIDAS[p.id]?.incompleta ?? null,
  archivo: CONSTRUIDAS[p.id]?.archivo ?? null,
}));

export type PantallaConSituacion = (typeof PANTALLAS)[number];

/**
 * Por qué el conteo de bloqueadas de esta aplicación **no da 29**.
 *
 * El inventario declara 29 bloqueadas por el insumo #2. Acá aparecen menos, y la diferencia no
 * es un error de ninguno de los dos: unas ya se construyeron —contra el criterio del propio
 * inventario— y otras son del cliente de campo, que no tiene interfaz y por eso no espera un
 * formato: espera un cliente entero.
 *
 * **Se calcula y se muestra en vez de dejar el número suelto.** Un conteo que discrepa del
 * documento sin decir por qué obliga a elegir a cuál de los dos creerle.
 */
export const BLOQUEADAS = (() => {
  const todas = INVENTARIO.filter((p) => p.papel === 'Sí');
  const construidas = todas.filter((p) => CONSTRUIDAS[p.id] !== undefined);
  const deCampo = todas.filter(
    (p) => CONSTRUIDAS[p.id] === undefined && p.cliente === 'C',
  );

  return {
    /** Lo que declara el documento. */
    segunElInventario: todas.length,
    /** Construidas igual, sin el formato a la vista. */
    yaConstruidas: construidas.map((p) => p.id),
    /** Del cliente de campo: no las frena el formato, las frena que no haya cliente. */
    deCampo: deCampo.map((p) => p.id),
    /** Las que de verdad están esperando el papel en la oficina. */
    enLaColaDeOficina: todas.length - construidas.length - deCampo.length,
  };
})();
/** Una pantalla por su identificador. Nulo si el `PT-xxx` no existe en el inventario. */
export function buscar(id: string): PantallaConSituacion | null {
  const buscado = id.trim().toUpperCase();
  return PANTALLAS.find((p) => p.id === buscado) ?? null;
}
