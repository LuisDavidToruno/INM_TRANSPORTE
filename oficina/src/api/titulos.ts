import { pedir } from './misiones';

/**
 * `RN-62` — el título de tenencia del vehículo.
 *
 * ── Por qué es una serie y no un campo ──────────────────────────────────────
 * Un vehículo que pasa de comodato a propiedad **conserva el título anterior**. Las misiones de
 * ese período se hicieron bajo comodato y sus rubros los cubría el cedente; reescribir el
 * régimen borraría el contexto contable de todo lo ya ejecutado. De la serie manda el que
 * regía **a la fecha del hecho**, no el vigente hoy.
 *
 * ── Lo que el cliente NO decide ─────────────────────────────────────────────
 * Cuál título rige, si el bien es propio y cuántos días le quedan los resuelve el servidor.
 * Es la misma razón por la que `flota.ts` no evalúa `BD-02`: `esBienPropio` decide **cuál de
 * los dos terminales corresponde**, y dos implementaciones de esa pregunta producirían el
 * asiento falso que la regla existe para impedir.
 */

/** Quién asume un rubro. **Tres valores, no dos** — ver `rubrosSinPactar`. */
export type QuienAsume = 'Institucion' | 'Titular' | 'SinPactar';

export type Regimen =
  | 'Propiedad'
  | 'Comodato'
  | 'Alquiler'
  | 'DonacionEnTramite'
  | 'AsignacionPorOtraInstitucion';

export interface RubroAsumido {
  rubro: string;
  quien: QuienAsume;
}

export interface TituloDeTenencia {
  id: string;
  vehiculo: string;
  regimen: Regimen;
  /** Quién es el propietario o cedente. Sin él no hay a quién devolverle el bien. */
  titular: string;
  /** Convenio, contrato, acta o resolución. **Una prórroga verbal no existe.** */
  documento: string;
  desde: string;
  /** **Nula en propiedad**: el bien es del Estado y no vence. */
  hasta: string | null;
  /** **Nulos en propiedad.** Un número inventado alertaría sobre un vencimiento que no existe. */
  diasRestantes: number | null;
  vigente: boolean;
  /** Lo que decide cuál de los dos terminales corresponde — `HB3-17`. */
  esBienPropio: boolean;
  rubros: RubroAsumido[];
  /** Lo que cubre el titular **no se imputa a nuestro presupuesto**. */
  rubrosDelTitular: string[];
  /** El rubro que aparece cuando llega la factura y empieza la discusión con el contrato. */
  rubrosSinPactar: string[];
}

/** Un vehículo de la flota con el título que rige hoy — o sin ninguno. */
export interface CoberturaDeTitulo {
  vehiculo: string;
  siglas: string;
  placa: string | null;
  tipoDeVehiculo: string;
  /**
   * **Nulo es «no consta bajo qué régimen lo tenemos»**, y eso no es «propio».
   *
   * En este vehículo `RN-62` queda sin evaluar: la ventana de sus misiones no se contrasta
   * contra ninguna vigencia, y el terminal correcto se advierte en vez de juzgarse.
   */
  titulo: TituloDeTenencia | null;
  /**
   * El más reciente de la serie, **esté vigente o no**.
   *
   * Existe porque «nunca tuvo título» y «se le venció» son cosas opuestas y sin este campo se
   * verían iguales: las dos llegan con `titulo` nulo. La primera es un dato de alta que nadie
   * llenó; la segunda es un comodato corrido de plazo, con un bien ajeno que ya debería
   * haberse devuelto.
   */
  ultimo: TituloDeTenencia | null;
  /** Mayor que uno significa que el régimen cambió en algún momento. */
  enLaSerie: number;
  /** §10.2. **Nulo** es «nunca se declaró», no «disponible». */
  estado: string | null;
  /**
   * Si el vehículo ya salió de la flota — dado de baja o retirado.
   *
   * **Lo calcula el servidor**, como `inutilizable` en `flota.ts`: la lista de estados
   * terminales es de §10.2, y duplicarla acá la dejaría divergir el día que se agregue uno.
   *
   * A una unidad fuera de la flota **no le queda ningún control que encender**: no se le va a
   * programar nada y su salida ya ocurrió. Contarla entre las que «faltan» inflaría el hueco
   * con vehículos que no lo son.
   */
  fueraDeLaFlota: boolean;
}

export const coberturaDeTitulos = (): Promise<CoberturaDeTitulo[]> =>
  pedir<CoberturaDeTitulo[]>('/titulos');

/** La serie completa de un vehículo, del más reciente al más viejo. */
export const serieDeTitulos = (vehiculo: string): Promise<TituloDeTenencia[]> =>
  pedir<TituloDeTenencia[]>(`/titulos/${vehiculo}`);

/** Los siete rubros de `RN-62`, en el orden en que el servidor los publica. */
export const RUBROS: { campo: keyof RubrosNuevos; texto: string }[] = [
  { campo: 'combustible', texto: 'Combustible' },
  { campo: 'mantenimiento', texto: 'Mantenimiento' },
  { campo: 'llantas', texto: 'Llantas' },
  { campo: 'seguro', texto: 'Seguro' },
  { campo: 'peajes', texto: 'Peajes' },
  { campo: 'multas', texto: 'Multas' },
  { campo: 'danios', texto: 'Daños' },
];

export interface RubrosNuevos {
  combustible: QuienAsume;
  mantenimiento: QuienAsume;
  llantas: QuienAsume;
  seguro: QuienAsume;
  peajes: QuienAsume;
  multas: QuienAsume;
  danios: QuienAsume;
}

/**
 * Los regímenes, con lo que cada uno implica.
 *
 * **Sólo la propiedad es un bien del Estado.** `DonacionEnTramite` todavía no lo es: hasta que
 * el traspaso se perfeccione, darlo de baja del registro sería anticipar un título que no está.
 *
 * **Y sólo la propiedad no vence.** Los demás exigen fecha de fin, porque un comodato que no
 * vence es una apropiación.
 */
export const REGIMENES: {
  valor: Regimen;
  texto: string;
  esBienPropio: boolean;
  ayuda: string;
}[] = [
  {
    valor: 'Propiedad',
    texto: 'Propiedad del Estado',
    esBienPropio: true,
    ayuda: 'No vence. Es el único que sale del registro por descargo.',
  },
  {
    valor: 'Comodato',
    texto: 'Comodato',
    esBienPropio: false,
    ayuda: 'Cedido en uso por otra institución. Se devuelve con acta.',
  },
  {
    valor: 'Alquiler',
    texto: 'Alquiler',
    esBienPropio: false,
    ayuda: 'Contrato con un proveedor. Los rubros que cubre no se cargan a nuestro presupuesto.',
  },
  {
    valor: 'DonacionEnTramite',
    texto: 'Donación en trámite',
    esBienPropio: false,
    ayuda: 'Todavía no es del Estado: el traspaso no se perfeccionó.',
  },
  {
    valor: 'AsignacionPorOtraInstitucion',
    texto: 'Asignado por otra institución',
    esBienPropio: false,
    ayuda: 'La titularidad sigue siendo de quien lo asignó.',
  },
];

export interface TituloNuevo extends RubrosNuevos {
  id: string;
  idVehiculo: string;
  regimen: Regimen;
  titular: string;
  documento: string;
  desde: string;
  hasta: string | null;
}

export const registrarTitulo = async (titulo: TituloNuevo): Promise<void> => {
  await pedir('/titulos', { method: 'POST', body: JSON.stringify(titulo) });
};

/**
 * Un ULID de 26 caracteres, en el alfabeto de Crockford.
 *
 * Se genera en el cliente porque el identificador es **la clave de idempotencia**: si la
 * respuesta se pierde y alguien reintenta, el servidor rechaza el duplicado en vez de abrir
 * un segundo título sobre el mismo vehículo.
 */
export function nuevoUlid(): string {
  const alfabeto = '0123456789ABCDEFGHJKMNPQRSTVWXYZ';
  const azar = crypto.getRandomValues(new Uint8Array(26));
  return Array.from(azar, (b) => alfabeto[b % 32]).join('');
}
