import type { Expediente } from '../dominio/mision';
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

async function pedir<T>(ruta: string, opciones?: RequestInit): Promise<T> {
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

/** Los expedientes que esperan pronunciamiento de esta jefatura. */
export async function bandejaDeAutorizacion(): Promise<Expediente[]> {
  if (!BASE) return conRetardo(expedientesDeMuestra());
  return pedir<Expediente[]>('/misiones?estado=Solicitada');
}

export async function expediente(id: string): Promise<Expediente> {
  if (!BASE) {
    const encontrado = expedientesDeMuestra().find((e) => e.id === id);
    if (!encontrado) throw new Error(`No existe el expediente ${id}.`);
    return conRetardo(encontrado);
  }
  return pedir<Expediente>(`/misiones/${id}`);
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
