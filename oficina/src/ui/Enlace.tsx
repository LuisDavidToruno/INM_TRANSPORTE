import type { AnchorHTMLAttributes, ReactElement, ReactNode } from 'react';
import { Link } from 'react-router';

/**
 * Enlace que decide solo si navega dentro de la aplicación o sale de ella.
 *
 * Se usa exactamente como un `<a>`: misma forma, mismos atributos. Eso es
 * deliberado — puede sustituir a un `<a>` en cualquier sitio sin perder nada por
 * el camino, y `EnlaceBoton` depende de esa propiedad.
 *
 * ── Por qué existe, si `<Link>` ya hace esto ────────────────────────────────
 * Porque **el destino no siempre se sabe al escribir la pantalla**. Un `href`
 * que viene del servidor en una fila de datos puede apuntar adentro o afuera, y
 * elegir mal tiene dos costos distintos y los dos silenciosos:
 *
 *   · Un `<a>` donde correspondía `<Link>` recarga la página entera: se pierde
 *     el estado, el tema parpadea y el usuario paga una carga completa por algo
 *     que ya estaba en memoria.
 *   · Un `<Link>` donde correspondía `<a>` **no navega a ningún lado** — el
 *     router no conoce la ruta y no falla: simplemente no se mueve. Es el peor
 *     de los dos porque no deja rastro.
 *
 * ── En qué se diferencia del de ARGOS ───────────────────────────────────────
 * En ARGOS este componente consulta un REGISTRO de rutas migradas, porque allá
 * la mitad de las pantallas todavía vive en Razor y la otra mitad en React: un
 * mismo `href` puede ser interno hoy y no haberlo sido la semana pasada.
 *
 * Acá no hay dos mundos, así que la pregunta es más simple y más honesta: ¿el
 * destino es del mismo origen? Si tu proyecto llega a tener un backend con
 * pantallas propias fuera de la SPA, éste es el archivo donde se agrega ese
 * registro — y sigue siendo el único.
 */
export interface EnlaceProps extends Omit<AnchorHTMLAttributes<HTMLAnchorElement>, 'href'> {
  /**
   * URL de destino. Acepta nulo porque muchas filas traen el destino opcional
   * (un enlace al grupo sólo existe si el registro pertenece a uno).
   */
  readonly href: string | null | undefined;
  readonly children: ReactNode;
  /**
   * **Salir de la aplicación aunque el destino sea interno.** Es lo contrario de
   * lo que hace este componente, y por eso hay que pedirlo a mano.
   *
   * Existe para las salidas deliberadas: forzar una recarga completa después de
   * cambiar algo que vive fuera del estado de React.
   */
  readonly externo?: boolean;
  /**
   * Nombre accesible explícito.
   *
   * Hace falta cuando el texto visible se repite fila a fila. En una tabla con
   * un enlace «Ver detalle» por fila, quien navega por lista de enlaces oye «Ver
   * detalle» diez veces sin saber cuál es cuál; el `aria-label` agrega de qué
   * registro se trata sin ensuciar la columna con el número repetido.
   */
  readonly 'aria-label'?: string;
}

/**
 * Un destino es interno si es una ruta relativa a la raíz (`/algo`).
 *
 * `//otro-sitio.com` **no** lo es, aunque empiece con barra: es un enlace
 * protocolo-relativo y apunta afuera. Tratarlo como interno mandaría al router a
 * una ruta que no existe y el enlace no haría nada.
 */
function esInterno(href: string | null | undefined): href is string {
  return typeof href === 'string' && href.startsWith('/') && !href.startsWith('//');
}

export default function Enlace({
  href,
  children,
  externo = false,
  ...resto
}: EnlaceProps): ReactElement {
  if (!externo && esInterno(href)) {
    return (
      <Link to={href} {...resto}>
        {children}
      </Link>
    );
  }

  // `href ?? '#'` y no `undefined`: un `<a>` sin `href` no es enfocable ni
  // navegable por teclado, así que un destino ausente dejaría un texto que
  // parece un enlace y no responde al tabulador.
  return (
    <a href={href ?? '#'} {...resto}>
      {children}
    </a>
  );
}
