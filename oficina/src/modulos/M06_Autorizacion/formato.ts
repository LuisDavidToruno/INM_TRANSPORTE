/**
 * Formato de fechas para la oficina.
 *
 * Todo llega en UTC con su desfase (`ADR-007`) y se muestra en hora local: el
 * usuario razona en la hora que vivió, no en UTC. Y a partir de las 18:00 en
 * Honduras las dos fechas son días distintos, así que convertir no es opcional.
 */

const FECHA = new Intl.DateTimeFormat('es-HN', {
  day: '2-digit',
  month: 'short',
  hour: '2-digit',
  minute: '2-digit',
  hour12: false,
});

const FECHA_LARGA = new Intl.DateTimeFormat('es-HN', {
  dateStyle: 'long',
  timeStyle: 'short',
  hour12: false,
});

export const diaYHora = (iso: string): string => FECHA.format(new Date(iso));

export const momentoCompleto = (iso: string): string => FECHA_LARGA.format(new Date(iso));

/**
 * Cuánto falta, en palabras.
 *
 * Se calcula contra el arranque de la vista y no contra un reloj vivo: un contador
 * que se mueve solo en una tabla de decisión distrae de la decisión.
 */
export function faltanDias(iso: string, ahora: Date = new Date()): string {
  const dias = Math.round((new Date(iso).getTime() - ahora.getTime()) / 86_400_000);

  if (dias < 0) return dias === -1 ? 'fue ayer' : `fue hace ${Math.abs(dias)} días`;
  if (dias === 0) return 'es hoy';
  if (dias === 1) return 'es mañana';
  return `faltan ${dias} días`;
}
