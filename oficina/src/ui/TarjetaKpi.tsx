import type { ReactElement, ReactNode } from 'react';

import Panel from './Panel';
import TileIcono from './TileIcono';
import type { Tono } from './tipos';

/**
 * La tarjeta de indicador con disco de ícono — y su esqueleto, al lado.
 *
 * ── Por qué existe: había DIECISIETE copias ──────────────────────────────────
 * Diecisiete pantallas declaraban su propio `function Kpi({…})` con el mismo cuerpo. Ya no
 * eran idénticas entre sí (unas tipaban `valor` como texto, otras como número, una no
 * recibía ícono), que es lo que pasa siempre con una copia: nadie las cambia todas a la vez.
 *
 * ── Y por qué el esqueleto vive EN ESTE archivo ──────────────────────────────
 * Es la parte que importa. El esqueleto tiene que medir **exactamente** lo que mide la
 * tarjeta, y la única forma de que no se separen es que se lean juntos: si mañana la
 * tarjeta gana un renglón, quien lo agregue tiene el esqueleto tres líneas más abajo.
 *
 * Esto no es una precaución teórica. Estas pantallas usaban `EsqueletoKpis` del contrato,
 * que dibuja **otra** tarjeta —la de `FilaKpis`, que es un panel dividido y no una tarjeta
 * con disco— y por eso la fila de indicadores medía 9 px de más mientras cargaba: al llegar
 * el dato, todo lo de abajo subía. Medido en Empleados el 2026-08-12 con la respuesta
 * retrasada a propósito: la tabla arrancaba en 265 px y terminaba en 256.
 *
 * `FilaKpis` + `EsqueletoKpis` siguen siendo la pareja correcta entre ellos (medido: 1 px,
 * redondeo). Son dos familias distintas de indicador, cada una con su esqueleto.
 *
 * ── Vocabulario: el del contrato 0.3.3 ──────────────────────────────────────
 * `tono` es el del contrato (`ok · info · aviso · riesgo · neutro`), no el juego anterior de
 * seis. **«Oro» no existe como estado**: en el contrato el oro es el acento de marca. Lo que
 * lo usaba pasa a `neutro`, que es la decisión ya tomada al migrar el Expediente.
 *
 * El ícono entra como **nodo**, no como componente, porque así lo pide el `TileIcono` del
 * contrato: quien llama decide el tamaño y los atributos, en vez de que este archivo los
 * imponga.
 */

export interface TarjetaKpiProps {
  readonly titulo: string;
  /** Texto o número: las pantallas lo formatean antes (`toLocaleString`, `String(n)`). */
  readonly valor: string | number;
  readonly ayuda: string;
  /** Ya construido: `<Users className="tw:size-4" />`. */
  readonly icono: ReactNode;
  readonly tono: Tono;
}

export function TarjetaKpi({ titulo, valor, ayuda, icono, tono }: TarjetaKpiProps): ReactElement {
  return (
    <Panel>
      <div className="tw:flex tw:items-start tw:gap-3">
        <TileIcono tono={tono}>{icono}</TileIcono>
        <div className="tw:min-w-0">
          <p className="loki-rotulo tw:text-cabecera tw:text-tinta-low">{titulo}</p>
          <p className="tw:font-mono tw:text-kpi tw:font-semibold tw:tabular-nums tw:text-tinta-hi">
            {valor}
          </p>
          <p className="tw:text-ayuda tw:text-tinta-low">{ayuda}</p>
        </div>
      </div>
    </Panel>
  );
}

/**
 * La otra tarjeta: **la cifra primero, el rótulo debajo**, y sin línea de ayuda.
 *
 * ── Por qué NO es `TarjetaKpi` con las líneas al revés ───────────────────────
 * Es una anatomía distinta y a propósito. La usan «Personal en gira» y «Personal en comisión»,
 * dos pantallas hermanas donde lo que se lee de un vistazo es **cuántos son** — el rótulo sólo
 * dice de qué. En `TarjetaKpi` manda el rótulo y la cifra lo acompaña.
 *
 * Se saca acá porque estaba escrita **dos veces**, una en cada hermana, con el mismo cuerpo y
 * hasta el mismo tipo `'info' | 'exito' | 'alerta' | 'oro'` recortado a mano. Dos copias de
 * pantallas hermanas es justo el caso que termina en «una se cambió y la otra no».
 *
 * No lleva esqueleto porque ninguna de las dos dibuja uno de indicadores hoy. Si mañana lo
 * necesita, va **en este archivo**, por el mismo motivo que el de arriba.
 */
export function TarjetaConteo({ icono, etiqueta, valor, tono }: {
  readonly icono: ReactNode;
  readonly etiqueta: string;
  readonly valor: number;
  readonly tono: Tono;
}): ReactElement {
  return (
    <Panel>
      <div className="tw:flex tw:items-center tw:gap-3">
        <TileIcono tono={tono}>{icono}</TileIcono>
        <div className="tw:min-w-0">
          <p className="tw:font-mono tw:text-kpi tw:font-semibold tw:tabular-nums tw:text-tinta-hi">
            {valor}
          </p>
          <p className="loki-rotulo tw:text-cabecera tw:text-tinta-low">{etiqueta}</p>
        </div>
      </div>
    </Panel>
  );
}

/**
 * Línea de esqueleto que hereda la caja de línea de su contenedor.
 *
 * Es la misma técnica del esqueleto del contrato: el espacio de ancho cero hace existir la
 * caja de línea, y la barra va encima. Así el alto no lo pone la barra —lo pondría mal—
 * sino la tipografía del renglón que va a ocupar el texto real.
 */
function Linea({ ancho }: { readonly ancho: string }): ReactElement {
  return (
    <span className="loki-esqueleto-linea">
      {'​'}
      <span className="loki-esqueleto" style={{ width: ancho }} aria-hidden="true" />
    </span>
  );
}

/**
 * La fila de tarjetas mientras carga.
 *
 * ⚠️ Las TRES líneas y el disco, con las mismas clases de la tarjeta de arriba. El rótulo
 * («REGISTRADOS», «SIN CUENTA») es NUESTRO y se conoce antes que el dato, pero acá no se
 * puede escribir porque cada pantalla tiene los suyos: va en gris, y por eso la fila reserva
 * su alto igual. El disco es el tile de verdad con un ícono vacío dentro.
 */
export function EsqueletoTarjetasKpi({
  columnas = 4,
}: {
  readonly columnas?: number;
}): ReactElement {
  return (
    <div className="tw:grid tw:gap-3 tw:sm:grid-cols-2 tw:xl:grid-cols-4" aria-busy="true">
      {Array.from({ length: columnas }, (_, i) => (
        <Panel key={i}>
          <div className="tw:flex tw:items-start tw:gap-3">
            {/* El disco es el TILE DE VERDAD, vacío: así el tamaño sale del mismo componente
                y no de una medida copiada a mano, que es como se desincronizan estas cosas. */}
            <TileIcono tono="neutro">
              <span className="tw:size-4" />
            </TileIcono>
            <div className="tw:min-w-0 tw:flex-1">
              <p className="loki-rotulo tw:text-cabecera tw:text-tinta-low">
                <Linea ancho="70%" />
              </p>
              <p className="tw:font-mono tw:text-kpi tw:font-semibold tw:tabular-nums tw:text-tinta-hi">
                <Linea ancho="45%" />
              </p>
              <p className="tw:text-ayuda tw:text-tinta-low">
                <Linea ancho="80%" />
              </p>
            </div>
          </div>
        </Panel>
      ))}
    </div>
  );
}
