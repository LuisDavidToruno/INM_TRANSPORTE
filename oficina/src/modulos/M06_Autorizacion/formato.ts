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

/**
 * Una fecha que el sistema no tiene.
 *
 * Existe porque un expediente puede venir de antes de que el campo existiera, o de
 * un cliente de campo que sincronizó a medias. La alternativa —formatear igual—
 * produce «fue hace 739853 días», que en una bandeja de decisión no es un dato
 * degradado: es ruido que hace dudar de todo lo demás.
 */
const SIN_FECHA = 'Sin fecha registrada';

const esUtilizable = (iso: string): boolean => {
  if (!iso) return false;
  const fecha = new Date(iso);
  return !Number.isNaN(fecha.getTime()) && fecha.getFullYear() > 1900;
};

export const diaYHora = (iso: string): string =>
  esUtilizable(iso) ? FECHA.format(new Date(iso)) : SIN_FECHA;

export const momentoCompleto = (iso: string): string =>
  esUtilizable(iso) ? FECHA_LARGA.format(new Date(iso)) : SIN_FECHA;

const SOLO_FECHA = new Intl.DateTimeFormat('es-HN', { dateStyle: 'long' });

/**
 * La fecha sin la hora, para vencimientos.
 *
 * Existe porque partir `momentoCompleto` por « a las » se rompe el día que la
 * configuración regional cambie esa preposición — y nadie lo notaría hasta ver
 * una fecha de vencimiento vacía en una pantalla de bloqueo legal.
 */
export const soloFecha = (iso: string): string =>
  esUtilizable(iso) ? SOLO_FECHA.format(new Date(iso)) : SIN_FECHA;

/**
 * Cuánto falta, en palabras.
 *
 * Se calcula contra el arranque de la vista y no contra un reloj vivo: un contador
 * que se mueve solo en una tabla de decisión distrae de la decisión.
 */
export function faltanDias(iso: string, ahora: Date = new Date()): string {
  if (!esUtilizable(iso)) return '';

  const dias = Math.round((new Date(iso).getTime() - ahora.getTime()) / 86_400_000);

  if (dias < 0) return dias === -1 ? 'fue ayer' : `fue hace ${Math.abs(dias)} días`;
  if (dias === 0) return 'es hoy';
  if (dias === 1) return 'es mañana';
  return `faltan ${dias} días`;
}
