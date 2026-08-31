import { salir, sesionActual } from '../app/sesion';
import type { Expediente, MotivoDeAnulacion, Transicion } from '../dominio/mision';
import { expedientesDeMuestra } from './muestra';

/**
 * Cliente de la API de misiones.
 *
 * El contrato es el que `Sigti.Api` ya expone: `POST /misiones`,
 * `POST /misiones/{id}/aprobar`, y las precondiciones vuelven como **409 con el
 * identificador de la precondición** — no como un error genérico.
 *
 * Mientras la API no esté levantada, `VITE_API` sin valor sirve datos de muestra.
 * <b>El adaptador es la única pieza que cambia</b>: las pantallas no saben de dónde
 * vienen los datos, y por eso cambiar de muestra a servidor no las toca.
 */

const BASE = import.meta.env.VITE_API as string | undefined;

/** Una precondición de bloqueo duro que el servidor rechazó. */
export class BloqueoDuro extends Error {
  constructor(
    readonly precondicion: string,
    mensaje: string,
    /**
     * La tercera parte de `R-3`: cómo salir.
     *
     * **Nulo cuando el servidor no tiene camino documentado** para esta precondición. La
     * pantalla lo dice así en vez de inventar «comuníquese con el administrador»: eso
     * convertiría el silencio en una instrucción, y `ACT-01` no tiene acceso al negocio.
     */
    readonly salida: CaminoDeSalida | null = null,
  ) {
    super(mensaje);
    this.name = 'BloqueoDuro';
  }

  /**
   * El rechazo listo para mostrar.
   *
   * **El identificador se antepone sólo si el mensaje no lo dice ya.** Un `BD-xx` no aparece
   * en su propio texto y sin el prefijo el usuario no tiene cómo citarlo; una transición sí
   * —«La transición T-06 exige el estado Solicitada»—, y anteponerlo producía «T-06 — La
   * transición T-06 exige…», que se lee como una falla del sistema.
   */
  get paraMostrar(): string {
    if (this.precondicion === 'desconocida' || this.message.includes(this.precondicion)) {
      return this.message;
    }

    return `${this.precondicion} — ${this.message}`;
  }
}

/**
 * Las formas que tiene un 409 en esta API.
 *
 * Son varias a propósito: el nombre del campo dice de qué familia es el rechazo, y por lo
 * tanto por dónde se sale de él. Ver el `switch` de excepciones en `Program.cs`.
 */
export interface CaminoDeSalida {
  readonly quePuedeHacer: string;
  /** Nulo es «no se sabe quién resuelve», distinto de «resuélvalo usted». */
  readonly aQuienAcudir: string | null;
  readonly ficha: string | null;
}

interface RechazoDelServidor {
  readonly mensaje?: string;
  /** La tercera parte de . Nula cuando no hay camino documentado. */
  readonly salida?: CaminoDeSalida | null;
  /** Un `BD-xx` o una `RN-xx`. */
  readonly precondicion?: string;
  /** Un `T-xx` de la Orden de Misión o del vale: el expediente no está en el estado que exige. */
  readonly transicion?: string;
  /** El fondo de combustible no admite el movimiento en su estado actual. */
  readonly movimiento?: string;
  /** Una carga rechazada por el motor de sincronización. */
  readonly motivo?: string;
  /** La aprobación venció: la salida no es cambiar de vehículo sino anular con motivo. */
  readonly caducada?: boolean;
}

export async function pedir<T>(ruta: string, opciones?: RequestInit): Promise<T> {
  // ⚠️ **Un `FormData` no lleva `content-type` puesto a mano.** El navegador tiene que
  // escribirlo él, porque el valor incluye el `boundary` que separa las partes y sólo él lo
  // conoce. Ponerlo acá produce una petición que el servidor no puede desarmar: llega como
  // formulario mal formado, sin archivo y sin campos, y el error no menciona el encabezado.
  const esFormulario = opciones?.body instanceof FormData;

  // ⚠️ **La identidad va en el token, no en el cuerpo.** El servidor ya no acepta que el
  // cliente declare quién actúa: si esta cabecera falta, la petición es anónima y SIGTI la
  // rechaza — que es exactamente lo que tiene que pasar.
  const sesion = sesionActual();

  const identidad: Record<string, string> =
    sesion === null ? {} : { authorization: `Bearer ${sesion.token}` };

  const respuesta = await fetch(`${BASE}${ruta}`, {
    ...opciones,
    headers: esFormulario
      ? { ...identidad, ...opciones?.headers }
      : { 'content-type': 'application/json', ...identidad, ...opciones?.headers },
  });

  // **401 no es un rechazo de negocio.** No hay dato que corregir: o no hay identidad, o
  // venció. La salida es volver a entrar, y por eso la sesión se cierra acá — dejarla puesta
  // haría que la pantalla siga mostrando un nombre mientras todo devuelve 401.
  if (respuesta.status === 401) {
    salir();
    throw new Error('La sesión venció o no hay identidad. Vuelva a entrar.');
  }

  if (respuesta.status === 409) {
    const cuerpo = (await respuesta.json()) as RechazoDelServidor;

    throw new BloqueoDuro(
      // **El 409 no siempre se llama `precondicion`.** La API nombra el rechazo según de qué
      // familia sea —una transición inválida trae `transicion`, el fondo trae `movimiento`,
      // una carga rechazada trae `motivo`— y cada nombre está elegido para que la salida se
      // busque donde corresponde. Leer sólo `precondicion` dejaba a las otras tres como
      // «desconocida», que es la palabra que aparecía impresa en la pantalla.
      cuerpo.precondicion ?? cuerpo.transicion ?? cuerpo.movimiento ?? cuerpo.motivo ??
        (cuerpo.caducada === true ? 'aprobación caducada' : 'desconocida'),
      cuerpo.mensaje ?? 'La operación no cumple una precondición.',
      cuerpo.salida ?? null,
    );
  }

  if (!respuesta.ok) {
    throw new Error(`El servidor respondió ${respuesta.status}.`);
  }

  return (await respuesta.json()) as T;
}

/**
 * Lo que el servidor devuelve hoy.
 *
 * <b>Trae el expediente, no el juicio completo sobre él.</b> De las dos mitades de
 * `HU-009`, la antigüedad del espejo del organigrama ya se mide —`M-01`— y viaja
 * aparte, por `/organigrama/antiguedad`: es un dato global de la bandeja, no de la
 * fila. Los reparos por expediente siguen necesitando `M-13`.
 */
interface ExpedienteDelServidor {
  id: string;
  folio: string;
  estado: Expediente['estado'];
  capturadaPor: string;
  solicitanteDeDerecho: string;
  dependencia: string;
  objetoDelTraslado: string;
  destino: string;
  salidaPrevista: string;
  retornoPrevisto: string;
  horaDeSalida: string | null;
  horaDeRetorno: string | null;
  holguraDias: number;
  aprobacionCaducada: boolean;
  diario: Transicion[];
}

const alExpediente = (s: ExpedienteDelServidor): Expediente => ({
  ...s,
  // Las fechas sin hora viajan tal cual: `comoFecha()` en `formato.ts` las sitúa en
  // local. Corregirlas acá también sería tener dos mecanismos para lo mismo, y el
  // día que uno cambie el otro seguiría corriendo la fecha en silencio.
  // La antigüedad del espejo ya NO va acá: se mide de verdad y se muestra en la cabecera
  // de la bandeja. Lo que sigue faltando es el reparo **por expediente** —misiones sin
  // liquidar del solicitante— que necesita `M-13`. Por eso esto no es una lista vacía:
  // una bandeja sin reparos y una que no sabe si los hay son cosas distintas.
  validaciones: [
    {
      clase: 'advertencia',
      regla: 'M-13',
      titulo: 'Los reparos por expediente todavía no se calculan',
      detalle:
        'Las misiones sin liquidar del solicitante necesitan M-13, que no está ' +
        'construido. La antigüedad del organigrama sí se mide, y está en la cabecera.',
    },
  ],
});

/**
 * El catálogo cerrado de , con el texto que ve el usuario.
 *
 * Vive acá y no en el dominio porque el rótulo es de interfaz; el valor es el que
 * viaja al servidor y ese sí es del dominio.
 */
export const MOTIVOS_DE_ANULACION: { valor: MotivoDeAnulacion; texto: string }[] = [
  { valor: 'SinFlotaDisponible', texto: 'Sin flota disponible' },
  { valor: 'SinMotoristaHabilitado', texto: 'Sin motorista habilitado' },
  { valor: 'CaducadaPorFaltaDeProgramacion', texto: 'Caducada por falta de programación' },
  { valor: 'DesistimientoDelSolicitante', texto: 'Desistimiento del solicitante' },
  { valor: 'CausaExterna', texto: 'Causa externa' },
];

/** Los expedientes aprobados esperando vehículo y motorista. */
export async function colaDeProgramacion(): Promise<Expediente[]> {
  if (!BASE) return conRetardo([]);
  const crudos = await pedir<ExpedienteDelServidor[]>('/misiones?estado=Aprobada');
  return crudos.map(alExpediente);
}

/** `T-09` — Anular con motivo tipificado. El comentario acompaña; no sustituye. */
export async function anular(
  id: string,
  ejecuta: string,
  motivo: MotivoDeAnulacion,
  comentario: string,
): Promise<void> {
  if (!BASE) return conRetardo(undefined);
  await pedir(`/misiones/${id}/anular`, {
    method: 'POST',
    body: JSON.stringify({ ejecuta, motivo, comentario, momento: new Date().toISOString() }),
  });
}

/** Las misiones ya programadas — las que tienen vehículo y motorista tomados. */
export async function colaDeProgramadas(): Promise<Expediente[]> {
  if (!BASE) return conRetardo([]);
  const crudos = await pedir<ExpedienteDelServidor[]>('/misiones?estado=Programada');
  return crudos.map(alExpediente);
}

/**
 * `T-11` — devolver la misión a la cola liberando vehículo y motorista.
 *
 * <b>El motivo es texto libre, y no es un descuido.</b> A diferencia de la anulación, acá
 * la misión sigue viva y se va a reprogramar: el motivo no alimenta el indicador de
 * déficit, explica a la dependencia por qué perdió el vehículo que ya tenía.
 */
export async function desprogramar(id: string, ejecuta: string, motivo: string): Promise<void> {
  if (!BASE) return conRetardo(undefined);
  await pedir(`/misiones/${id}/desprogramar`, {
    method: 'POST',
    body: JSON.stringify({ ejecuta, motivo, momento: new Date().toISOString() }),
  });
}

/**
 * `T-13` — anular una misión ya programada. Motivo <b>tipificado</b>, como `T-09`.
 *
 * No es lo mismo que <c>desprogramar</c>: ésta la mata y no se vuelve. La otra la devuelve
 * a la cola.
 */
export async function anularProgramada(
  id: string,
  ejecuta: string,
  motivo: MotivoDeAnulacion,
  comentario: string,
): Promise<void> {
  if (!BASE) return conRetardo(undefined);
  await pedir(`/misiones/${id}/anular-programada`, {
    method: 'POST',
    body: JSON.stringify({ ejecuta, motivo, comentario, momento: new Date().toISOString() }),
  });
}

/** Los expedientes que esperan pronunciamiento de esta jefatura. */
export async function bandejaDeAutorizacion(): Promise<Expediente[]> {
  if (!BASE) return conRetardo(expedientesDeMuestra());
  const crudos = await pedir<ExpedienteDelServidor[]>('/misiones?estado=Solicitada');
  return crudos.map(alExpediente);
}

export async function expediente(id: string): Promise<Expediente> {
  if (!BASE) {
    const encontrado = expedientesDeMuestra().find((e) => e.id === id);
    if (!encontrado) throw new Error(`No existe el expediente ${id}.`);
    return conRetardo(encontrado);
  }
  return alExpediente(await pedir<ExpedienteDelServidor>(`/misiones/${id}`));
}

/**
 * `T-05` — Autorizar.
 *
 * El motivo viaja siempre que haya advertencias acusadas: `HU-009` exige que la
 * constancia diga **sobre qué dato** se autorizó, y esa constancia se imprime.
 */
export async function autorizar(id: string, ejecuta: string, motivo?: string): Promise<void> {
  if (!BASE) return conRetardo(undefined);
  await pedir(`/misiones/${id}/aprobar`, {
    method: 'POST',
    body: JSON.stringify({ ejecuta, momento: new Date().toISOString(), motivo }),
  });
}

/** Un retardo corto para que los estados de carga sean visibles con datos de muestra. */
function conRetardo<T>(valor: T): Promise<T> {
  return new Promise((resolver) => setTimeout(() => resolver(valor), 320));
}

/** Para que la interfaz pueda decir de dónde vienen los datos, en vez de fingir. */
export const origenDeDatos = BASE ? 'servidor' : 'muestra';

/**
 * Un criterio `H-nn` que se cumplió, con el caso concreto que lo demuestra.
 *
 * **El catálogo no se cablea acá.** `H-01` a `H-13` son parámetro con vigencia (`RN-39`),
 * y quien los detecta es el servidor a partir de la conciliación — no esta pantalla.
 */
/**
 * Un criterio `H-nn` de §7.2, **tal como el servidor lo evaluó**.
 *
 * ⚠️ `resultado` tiene tres valores y el tercero es el que importa: `NoVerificado` no es
 * `NoSeCumple`. Con dos se vuelven indistinguibles, y el expediente cierra afirmando trece
 * verificaciones de las que hizo cuatro.
 */
export interface CriterioEvaluado {
  criterio: string;
  enunciado: string;
  resultado: 'SeCumple' | 'NoSeCumple' | 'NoVerificado';
  /** Obligatorio en los tres: el caso concreto, contra qué se miró, o **qué falta** para mirarlo. */
  detalle: string;
}

/**
 * Un eslabón de la cadena de `RN-08`.
 *
 * ⚠️ Cuatro estados, y los dos últimos son los que hacen que la lista sirva:
 * `NoAplicable` no es `Presente` —«lo que no se admite es cerrarlo como presente con consumo
 * cero»— y `PendienteDeSincronizacion` no es `Ausente`: los datos están en camino, y marcar
 * hallazgo acusaría de una omisión que nadie cometió.
 */
export interface EslabonDeLaCadena {
  eslabon: string;
  nombre: string;
  estado: 'Presente' | 'Ausente' | 'NoAplicable' | 'PendienteDeSincronizacion';
  /** En los no aplicables, esto es el **fundamento** que `RN-08` exige. */
  detalle: string;
}

export interface CadenaDeTrazabilidad {
  completa: boolean;
  /** Los que esperan sincronización. **Bloquean, no marcan.** */
  enCamino: number;
  eslabones: EslabonDeLaCadena[];
}

export interface PropuestaDeCierre {
  /** `RN-08` — la lista de verificación eslabón por eslabón. Nula si no se pudo armar. */
  cadena: CadenaDeTrazabilidad | null;
  hayHallazgo: boolean;
  destino: 'Cerrada' | 'CerradaConHallazgo';
  sinVerificar: number;
  verificados: number;
  criterios: CriterioEvaluado[];
}

/**
 * §7.2 — **la propuesta la hace el sistema**, no quien cierra.
 *
 * Se consulta antes de cerrar y sale de la **misma evaluación** que el `POST` va a usar. Si la
 * pantalla la calculara por su cuenta, mostraría una cosa y el cierre registraría otra.
 */
export async function propuestaDeCierre(id: string): Promise<PropuestaDeCierre> {
  if (!BASE) {
    return conRetardo({
      hayHallazgo: false,
      destino: 'Cerrada',
      sinVerificar: 0,
      verificados: 0,
      criterios: [],
      cadena: null,
    });
  }

  return pedir<PropuestaDeCierre>(`/misiones/${id}/propuesta-de-cierre`);
}

/** Los expedientes liquidados esperando cierre. */
export async function colaDeCierre(): Promise<Expediente[]> {
  if (!BASE) return conRetardo([]);
  const crudos = await pedir<ExpedienteDelServidor[]>('/misiones?estado=Liquidada');
  return crudos.map(alExpediente);
}

/**
 * `T-21` y `T-22` — **un solo acto**.
 *
 * <b>No se manda el estado destino</b>, y esa ausencia es la regla: `orden-de-mision.md`
 * §7.2 dice que *«quien cierra no elige entre cerrar limpio o con hallazgo — el criterio
 * decide y él lo confirma con su justificación»*. Si esta función recibiera el destino,
 * la interfaz podría pedir el equivocado.
 */
export async function cerrar(
  id: string,
  ejecuta: string,
  justificacion: string | null,
): Promise<void> {
  if (!BASE) return conRetardo(undefined);
  await pedir(`/misiones/${id}/cerrar`, {
    method: 'POST',

    // ⚠️ **Los criterios no viajan.** Los evalúa el servidor: mandarlos desde acá dejaba que
    // la pantalla declarara la precondición de `T-21`, y con la lista vacía el expediente
    // cerraba limpio y el asiento decía que cerró limpio.
    body: JSON.stringify({ ejecuta, momento: new Date().toISOString(), justificacion }),
  });
}

/** `T-20` — Devolver la liquidación para rehacerla. El motivo dice qué corregir. */
export async function devolverLiquidacion(
  id: string,
  ejecuta: string,
  motivo: string,
): Promise<void> {
  if (!BASE) return conRetardo(undefined);
  await pedir(`/misiones/${id}/devolver-liquidacion`, {
    method: 'POST',
    body: JSON.stringify({ ejecuta, motivo, momento: new Date().toISOString() }),
  });
}

/**
 * Desde cuándo no se confirma el espejo del organigrama — `HU-009`.
 *
 * <b>`nuncaConfirmado` y `diasSinConfirmar: 0` son cosas opuestas.</b> Una integración
 * que jamás corrió y una que corrió hace un minuto no se pueden mostrar igual, y por eso
 * el contrato las distingue en vez de dejarlo a la interpretación del cliente.
 */
export interface AntiguedadDelEspejo {
  nuncaConfirmado: boolean;
  diasSinConfirmar: number | null;
}

export async function antiguedadDelEspejo(): Promise<AntiguedadDelEspejo | null> {
  // Sin servidor no se inventa un número: se dice que no se sabe.
  if (!BASE) return conRetardo(null);
  return pedir<AntiguedadDelEspejo>('/organigrama/antiguedad');
}

/**
 * Los motivos con que se puede rechazar — `T-06`.
 *
 * <b>Se piden al servidor, no se cablean.</b> `HU-014` declara el catálogo configurable por
 * la institución (insumo #1, `[C]`), y una lista duplicada en el cliente es una lista que
 * se separa de la que el servidor valida — con lo cual el rechazo fallaría al guardar y no
 * al elegir, que es el peor momento para enterarse.
 */
export async function catalogoDeMotivosDeRechazo(): Promise<string[]> {
  if (!BASE) return conRetardo([]);
  return pedir<string[]>('/motivos-de-rechazo');
}

/**
 * `T-06` — rechazar. <b>Terminal</b>: de `RECHAZADA` no sale ninguna transición.
 *
 * El motivo es del catálogo y el comentario es obligatorio: el primero dice qué se cuenta,
 * el segundo dice a la dependencia qué pasó.
 */
export async function rechazar(
  id: string,
  ejecuta: string,
  motivo: string,
  comentario: string,
): Promise<void> {
  if (!BASE) return conRetardo(undefined);
  await pedir(`/misiones/${id}/rechazar`, {
    method: 'POST',
    body: JSON.stringify({ ejecuta, motivo, comentario, momento: new Date().toISOString() }),
  });
}

/**
 * `T-04` — devolver para corrección. <b>No es rechazar</b>: el expediente vuelve a quien lo
 * capturó, se corrige y se reenvía. Sigue siendo el mismo expediente.
 *
 * El motivo es libre y no del catálogo: acá no se mide por qué se dijo que no —no se dijo—,
 * se dice qué falta, y un catálogo no puede enumerar lo que falta en un expediente concreto.
 */
export async function devolverParaCorreccion(
  id: string,
  ejecuta: string,
  motivo: string,
): Promise<void> {
  if (!BASE) return conRetardo(undefined);
  await pedir(`/misiones/${id}/devolver`, {
    method: 'POST',
    body: JSON.stringify({ ejecuta, motivo, momento: new Date().toISOString() }),
  });
}
