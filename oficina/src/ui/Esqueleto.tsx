import type { CSSProperties, ReactElement } from 'react';

import type { ColumnaDef } from './tipos';

/**
 * Esqueletos de carga del contrato 0.3.3.
 * Canon: COMPONENTS.md §2 · criterio medible en ACCEPTANCE.md §2
 *
 * ── La única función de un esqueleto: que nada salte ─────────────────────────
 * `delta === 0` entre el alto en carga y el alto con el dato. Si la vista se
 * mueve al llegar la respuesta, el esqueleto no sirvió para nada — sólo agregó
 * un parpadeo gris antes del salto.
 *
 * ── Cómo se garantiza acá, y por qué no basta con «poner la altura» ──────────
 * El error clásico es dar al esqueleto una altura fija: la barra mide 22 px y la
 * línea que sustituye mide 26, y la vista salta 4 px por fila. Y es un error que
 * se corrige una vez y vuelve, porque las alturas viven lejos del texto.
 *
 * Acá el alto NO lo pone la barra: lo pone el contenedor, con las MISMAS clases
 * tipográficas que va a tener el texto real. Dentro se emite un carácter de
 * ancho cero, que establece la caja de línea con las métricas de esa tipografía,
 * y la barra va posicionada en absoluto encima. Resultado: en carga y con dato,
 * el alto sale de la misma fuente y del mismo `line-height` — es el mismo
 * cálculo, así que no puede diferir.
 *
 * ── Lo que YA SABEMOS no se pone en esqueleto ────────────────────────────────
 * Cabeceras de tabla, rótulos de campo y los nombres de las ocho etapas son
 * nuestros, no del servidor. Pintarlos en gris finge una ignorancia que no
 * tenemos y obliga al usuario a esperar para saber dónde va a mirar.
 */

export interface EsqueletoProps {
  ancho?: number | string;
  alto: number | string;
}

/**
 * Barra suelta. Úsese cuando el contenedor YA lleva la tipografía real; si no,
 * preferí uno de los esqueletos compuestos de abajo, que la llevan puesta.
 *
 * ⚠️ **Exige un ancestro posicionado.** La barra va en `position: absolute` —es
 * lo que le deja cubrir la caja de línea sin alterarla— y un absoluto sin
 * ancestro posicionado no se queda en su contenedor: se va al bloque contenedor
 * inicial. `Linea` y los compuestos de abajo ya lo llevan; si la ponés a mano,
 * el contenedor necesita `position: relative`.
 *
 * El síntoma no se parece a la causa. Con un solo esqueleto se ve una barra
 * corrida de sitio y se le echa la culpa al `alto`; con varios —una lista de
 * diagramas esperando turno— se apilan todos en la misma caja y aparece una
 * banda que tapa la pantalla, que es lo que se ve, no lo que pasa. Y el `alto`
 * que se le pasa no manda: la caja sale de `top: 12%` y `bottom: 12%` sobre el
 * ancestro que la posicione.
 */
export function Esqueleto({ ancho = '100%', alto }: EsqueletoProps): ReactElement {
  const estilo: CSSProperties = { width: ancho, height: alto };
  return <span className="loki-esqueleto" style={estilo} aria-hidden="true" />;
}

/**
 * Línea de esqueleto que hereda la caja de línea de su contenedor.
 *
 * El `​` (espacio de ancho cero) no es decorativo: es lo que hace existir la
 * caja de línea. Sin él el contenedor colapsaría a cero y volveríamos al salto.
 *
 * Se exporta porque los ENCABEZADOS de pantalla la necesitan: cuando el título o el
 * subtítulo vienen del servidor, tienen que ocupar **exactamente** el alto que va a tener
 * su texto, y eso sólo sale de heredar su tipografía. Una barra con `alto` fijo dentro de
 * un `<h1>` da un alto *parecido*, no el mismo — que es justo el defecto que este módulo
 * existe para no tener.
 */
export function Linea({ ancho }: { readonly ancho: number | string }): ReactElement {
  return (
    <span className="loki-esqueleto-linea">
      {'​'}
      <span className="loki-esqueleto" style={{ width: ancho }} aria-hidden="true" />
    </span>
  );
}

/**
 * Fila de indicadores. El valor de un KPI son 26 px — de ahí `tw:text-kpi`.
 *
 * ⚠️ **Las TRES líneas de la tarjeta, no dos.** La anatomía del KPI es rótulo · cifra ·
 * nota, y este esqueleto dibujaba sólo las dos primeras: **faltaban ~20 px por tarjeta**,
 * así que la fila entera crecía al llegar el dato y empujaba todo lo de abajo. Medido en
 * Usuarios el 2026-08-12, después de haber creído que el salto ya estaba resuelto.
 *
 * Es el caso de manual de `delta === 0`: el esqueleto se veía bien y aun así saltaba, que
 * es el defecto que este componente existe para no tener.
 */
export function EsqueletoKpis({ columnas = 4 }: { readonly columnas?: number }): ReactElement {
  return (
    <div className="loki-rejilla-kpis" aria-busy="true">
      {Array.from({ length: columnas }, (_, i) => (
        <div key={i} className="tw:rounded-panel tw:border tw:border-linea tw:bg-panel loki-pad-panel">
          {/* El RÓTULO no va en esqueleto: es nuestro, lo sabemos antes que el dato. */}
          <div className="tw:text-cabecera tw:font-semibold tw:tracking-wide tw:text-tinta-mid tw:uppercase">
            <Linea ancho="55%" />
          </div>
          <div className="tw:mt-1 tw:font-mono tw:text-kpi tw:font-semibold">
            <Linea ancho="70%" />
          </div>
          <div className="tw:text-ayuda">
            <Linea ancho="45%" />
          </div>
        </div>
      ))}
    </div>
  );
}

/**
 * Tabla. Recibe las MISMAS columnas que la tabla real — no un número — para que
 * los anchos y la alineación coincidan celda por celda.
 *
 * Las cabeceras se pintan de una vez: las columnas se conocen antes que el dato.
 */
export function EsqueletoTabla({
  columnas,
  filas = 5,
}: {
  readonly columnas: ColumnaDef[];
  readonly filas?: number;
}): ReactElement {
  return (
    <table className="tw:w-full" aria-busy="true">
      <thead>
        <tr>
          {columnas.map((c) => (
            <th
              key={c.id}
              style={c.ancho !== undefined ? { width: c.ancho } : undefined}
              className={[
                'loki-celda tw:text-cabecera tw:font-semibold tw:tracking-wide tw:text-tinta-mid tw:uppercase',
                c.numerica === true ? 'tw:text-right' : 'tw:text-left',
              ].join(' ')}
            >
              {c.cabecera}
            </th>
          ))}
        </tr>
      </thead>
      <tbody>
        {Array.from({ length: filas }, (_, f) => (
          <tr key={f} className="loki-fila">
            {columnas.map((c) => (
              <td
                key={c.id}
                className={[
                  'loki-celda',
                  c.numerica === true
                    ? 'tw:text-right tw:font-mono tw:text-importe'
                    : 'tw:text-cuerpo-2',
                ].join(' ')}
              >
                {/* Anchos desparejos a propósito: una columna de barras idénticas se
                    lee como una tabla vacía, no como una que está cargando. */}
                <Linea ancho={c.numerica === true ? '60%' : `${55 + ((f * 7 + c.id.length * 5) % 35)}%`} />
              </td>
            ))}
          </tr>
        ))}
      </tbody>
    </table>
  );
}

/**
 * Pila de fichas: lo que carga una lista de tarjetas (giras, liquidaciones, mensajes,
 * adjuntos) — el otro formato del sistema junto a la tabla.
 *
 * `cantidad` es cuántas se dibujan **mientras carga**, no cuántas van a llegar: con
 * tres alcanza para que se lea «vienen varias» sin fingir que sabemos el total.
 *
 * Las líneas van de ancho desparejo por el mismo motivo que en la tabla: una pila de
 * barras idénticas se lee como una lista vacía, no como una cargando.
 */
export function EsqueletoFichas({
  cantidad = 3,
  lineas = 2,
}: {
  readonly cantidad?: number;
  readonly lineas?: number;
}): ReactElement {
  return (
    <div className="tw:flex tw:flex-col tw:gap-2" aria-busy="true">
      {Array.from({ length: cantidad }, (_, i) => (
        <div
          key={i}
          className="loki-pad-panel tw:rounded-panel tw:border tw:border-linea tw:bg-panel"
        >
          {/* El título de la ficha, en el mismo cuerpo que va a tener el texto real. */}
          <div className="tw:text-cuerpo tw:font-semibold">
            <Linea ancho={`${45 + ((i * 11) % 25)}%`} />
          </div>
          {Array.from({ length: lineas }, (_, l) => (
            <div key={l} className="tw:text-cuerpo-2">
              <Linea ancho={`${60 + ((i * 7 + l * 13) % 30)}%`} />
            </div>
          ))}
        </div>
      ))}
    </div>
  );
}

/** Lista de definiciones. Las etiquetas son nuestras: van en texto, no en gris. */
export function EsqueletoLista({ etiquetas }: { readonly etiquetas: string[] }): ReactElement {
  return (
    <dl className="loki-dl-dos-columnas tw:grid tw:gap-2" aria-busy="true">
      {etiquetas.map((e) => (
        <div key={e} className="tw:contents">
          <dt className="tw:text-cuerpo-2 tw:text-tinta-mid">{e}</dt>
          <dd className="tw:text-cuerpo-2 tw:text-tinta-base">
            <Linea ancho="65%" />
          </dd>
        </div>
      ))}
    </dl>
  );
}
