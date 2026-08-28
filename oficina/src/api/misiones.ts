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
  ) {
    super(mensaje);
    this.name = 'BloqueoDuro';
  }
}

export async function pedir<T>(ruta: string, opciones?: RequestInit): Promise<T> {
  const respuesta = await fetch(`${BASE}${ruta}`, {
    ...opciones,
    headers: { 'content-type': 'application/json', ...opciones?.headers },
  });

  if (respuesta.status === 409) {
    const cuerpo = (await respuesta.json()) as { precondicion?: string; mensaje?: string };
    throw new BloqueoDuro(
      cuerpo.precondicion ?? 'desconocida',
      cuerpo.mensaje ?? 'La operación no cumple una precondición.',
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
export interface CriterioDetectado {
  criterio: string;
  detalle: string;
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
  criterios: CriterioDetectado[],
  justificacion: string | null,
): Promise<void> {
  if (!BASE) return conRetardo(undefined);
  await pedir(`/misiones/${id}/cerrar`, {
    method: 'POST',
    body: JSON.stringify({ ejecuta, momento: new Date().toISOString(), criterios, justificacion }),
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
