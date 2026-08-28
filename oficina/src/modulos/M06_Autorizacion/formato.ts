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

/**
 * Convierte lo que llega del servidor en una fecha bien situada.
 *
 * <b>Una fecha sin hora —`2028-04-30`— la interpreta JavaScript como medianoche
 * UTC</b>, y formatearla en UTC−6 la corre al día anterior. Ese defecto se veía en
 * la pantalla de asignación: una licencia que vence el 30 se mostraba venciendo el
 * 29. En una pantalla sobre vigencia de licencias eso no es cosmético — es decirle
 * a quien programa que le queda un día menos del que tiene.
 *
 * Las fechas con hora ya traen su desfase y se respetan tal cual (`ADR-007`).
 */
const comoFecha = (iso: string): Date =>
  /^\d{4}-\d{2}-\d{2}$/.test(iso) ? new Date(`${iso}T00:00:00`) : new Date(iso);

const esUtilizable = (iso: string): boolean => {
  if (!iso) return false;
  const fecha = comoFecha(iso);
  return !Number.isNaN(fecha.getTime()) && fecha.getFullYear() > 1900;
};

export const diaYHora = (iso: string): string =>
  esUtilizable(iso) ? FECHA.format(comoFecha(iso)) : SIN_FECHA;

export const momentoCompleto = (iso: string): string =>
  esUtilizable(iso) ? FECHA_LARGA.format(comoFecha(iso)) : SIN_FECHA;

const SOLO_FECHA = new Intl.DateTimeFormat('es-HN', { dateStyle: 'long' });

/**
 * La fecha sin la hora, para vencimientos.
 *
 * Existe porque partir `momentoCompleto` por « a las » se rompe el día que la
 * configuración regional cambie esa preposición — y nadie lo notaría hasta ver
 * una fecha de vencimiento vacía en una pantalla de bloqueo legal.
 */
export const soloFecha = (iso: string): string =>
  esUtilizable(iso) ? SOLO_FECHA.format(comoFecha(iso)) : SIN_FECHA;

/**
 * Cuánto falta, en palabras.
 *
 * Se calcula contra el arranque de la vista y no contra un reloj vivo: un contador
 * que se mueve solo en una tabla de decisión distrae de la decisión.
 */
export function faltanDias(iso: string, ahora: Date = new Date()): string {
  if (!esUtilizable(iso)) return '';

  const dias = Math.round((comoFecha(iso).getTime() - ahora.getTime()) / 86_400_000);

  if (dias < 0) return dias === -1 ? 'fue ayer' : `fue hace ${Math.abs(dias)} días`;
  if (dias === 0) return 'es hoy';
  if (dias === 1) return 'es mañana';
  return `faltan ${dias} días`;
}

/**
 * El nombre de la dependencia dentro de una frase, o algo que se pueda leer.
 *
 * ── Por qué hace falta ──────────────────────────────────────────────────────
 * Porque el expediente puede traerla <b>vacía</b> —el dominio no la exige— y nueve mensajes
 * de la oficina la interpolan. Sin esto, un diálogo que debía decir *«vuelve a Delegación de
 * Choluteca»* dice literalmente <b>«vuelve a , se corrige»</b>, que es lo que se veía en la
 * pantalla de autorización antes de esto.
 *
 * ── Y por qué no devuelve cadena vacía ──────────────────────────────────────
 * Porque la frase la necesita. Devolver `''` sólo mueve el problema: la coma sigue ahí. Lo
 * que se devuelve es un sustituto que <b>encaja gramaticalmente</b> y que además dice la
 * verdad — el expediente no declara dependencia, y quien lea el mensaje tiene derecho a
 * saber que ése es el estado del dato y no un fallo de la pantalla.
 */
export const laDependencia = (nombre: string): string =>
  nombre.trim() === '' ? 'la dependencia solicitante, que el expediente no declara' : nombre;

/**
 * `HH:mm` a partir de lo que manda el servidor, o la ausencia dicha.
 *
 * ── Por qué la ausencia se dice y no se rellena ─────────────────────────────
 * Porque un `00:00` se lee como <b>medianoche</b>, y sobre esa lectura el despachador ordena
 * su día y decide a quién llamar primero. Los expedientes anteriores al campo no declaran
 * hora, y eso es un dato distinto de salir a las doce de la noche.
 */
export const soloHora = (hora: string | null): string =>
  hora === null ? 'hora no declarada' : hora.slice(0, 5);

/** Igual, pero para cuando la frase ya dice que es una hora. */
export const soloHoraCorta = (hora: string | null): string =>
  hora === null ? 'sin hora' : hora.slice(0, 5);
