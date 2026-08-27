import type { ReactElement, ReactNode } from 'react';
import { useId, useMemo } from 'react';

import { CLASE_TONO } from './tipos';
import type { Tono } from './tipos';

/**
 * Línea de tiempo por carriles — un carril por recurso, barras sobre un eje de tiempo.
 *
 * ── La pregunta que contesta, y que una tabla no ────────────────────────────
 * **¿Qué se solapa con qué, y dónde queda el hueco?** Una lista ordenada por hora pone
 * las filas una debajo de otra y deja el traslape para que el usuario lo deduzca. El
 * caso que lo prueba es `PT-038`: el despachador a las 5:30 de la mañana con ocho
 * salidas encimadas. Y `PT-026` — para saber si el pick-up está libre el jueves, con
 * una lista hay que abrir las misiones una por una.
 *
 * ── Tres usos, una sola primitiva ───────────────────────────────────────────
 * 1. **Ocupación de un recurso** — un carril por vehículo o por motorista, una barra
 *    por misión (`PT-026`, `029`, `030`, `032`, `033`, `038`, `072`, `087`).
 * 2. **Vigencias** — un carril por documento, la barra es el rango vigente
 *    (`PT-019`, `027`, `078`, `082`, `085`, `092`, `096`, `099`).
 * 3. **Hitos de una misión** — un carril por etapa (`PT-042`, `059`, `062`, `073`, `102`).
 *
 * Los tres son el mismo dibujo: rangos con nombre sobre un eje común. Por eso el
 * componente no sabe de vehículos ni de vencimientos — recibe carriles y barras.
 *
 * ── Lo que NO hace, y es deliberado ─────────────────────────────────────────
 * **No decide nada.** No marca cuál barra es el conflicto ni pinta de rojo lo vencido:
 * recibe el `tono` ya resuelto. La regla vive en el servidor, y un componente que
 * además la calculara sería una segunda copia de la regla — que es exactamente donde
 * lo que se ve deja de coincidir con lo que bloquea al guardar.
 *
 * **No hace zoom ni desplaza el eje.** La ventana se recibe. Un eje que el usuario
 * mueve convierte «no hay solape» en una afirmación sobre lo que quedó en pantalla, y
 * eso, en una decisión de asignación, es peor que no mostrar nada.
 *
 * ── Accesibilidad: el dibujo no es la única salida ──────────────────────────
 * Cada barra lleva texto real y un rótulo que dice de qué carril es y entre qué fechas
 * va. Un diagrama que sólo existe como posición en píxeles deja fuera a quien navega
 * por teclado o no ve la pantalla, y esta es una pieza de decisión, no un adorno.
 */

/** Una barra: un rango con nombre dentro de un carril. */
export interface BarraDeCarril {
  id: string;
  /** Lo que se lee dentro de la barra y lo que anuncia el lector de pantalla. */
  titulo: string;
  /** Inclusivo. */
  desde: Date;
  /**
   * **Inclusivo también** — es el último día ocupado, no el primero libre.
   *
   * Se eligió así porque el dato del dominio viene así: el retorno previsto de una
   * misión es un día en que el vehículo sigue tomado. Un extremo exclusivo obligaría a
   * sumar un día en cada llamador, y el día que alguien lo olvide el vehículo se
   * ofrece libre en su último día de misión.
   */
  hasta: Date;
  tono?: Tono;
  /** Con esto la barra es un enlace. Sin esto es texto: no todo rango se abre. */
  href?: string;
  /** Segunda línea del rótulo accesible — el destino, el motivo, el folio. */
  detalle?: string;
  /**
   * Qué es ESTA barra, cuando no es lo mismo que el resto del dibujo.
   *
   * Un carril de flota mezcla **misiones** con **bloqueos de taller**, y no son lo
   * mismo: una se puede reprogramar y la otra no. Sin esto el lector de pantalla
   * anunciaba «misión Correctivo», que es falso y además sugiere que se puede mover.
   */
  queEs?: string;
}

export interface CarrilDeLinea {
  id: string;
  /** El recurso: las siglas del vehículo, el nombre del documento, la etapa. */
  titulo: string;
  /** Bajo el título, en tono secundario. La placa, el tipo, el número. */
  detalle?: string;
  barras: readonly BarraDeCarril[];
  /** Se pinta apagado y se anuncia como tal. Para el vehículo fuera de servicio. */
  inhabilitado?: boolean;
}

export interface LineaDeCarrilesProps {
  carriles: readonly CarrilDeLinea[];
  /** Primer día del eje, inclusivo. */
  desde: Date;
  /** Último día del eje, **inclusivo**. */
  hasta: Date;
  /**
   * Qué es cada barra, para el lector de pantalla: «misión», «vigencia», «etapa». Sin
   * esto el anuncio sería «tres barras», que no dice nada.
   */
  queEsUnaBarra?: string;
  /** Marca vertical de referencia. Fuera de la ventana, no se dibuja. */
  referencia?: { fecha: Date; titulo: string };
  /** Se pinta cuando no hay ni un carril. */
  vacio?: ReactNode;
}

const DIA_MS = 86_400_000;

/** A medianoche local. Comparar fechas con hora convierte «el mismo día» en falso. */
const aDia = (f: Date): number =>
  new Date(f.getFullYear(), f.getMonth(), f.getDate()).getTime();

const diasEntre = (a: Date, b: Date): number => Math.round((aDia(b) - aDia(a)) / DIA_MS);

/** Domingo primero, como `Date.getDay()`. */
const INICIAL_DIA = ['D', 'L', 'M', 'M', 'J', 'V', 'S'] as const;

const esFinDeSemana = (f: Date): boolean => f.getDay() === 0 || f.getDay() === 6;

const textoDeFecha = (f: Date): string =>
  f.toLocaleDateString('es-HN', { day: 'numeric', month: 'long' });

/** Alto de cada subfila de un carril. En `rem` para que siga a la tipografía del sistema. */
const ALTO_SUBFILA = '2.25rem';

interface Colocada {
  barra: BarraDeCarril;
  inicio: number;
  fin: number;
  largo: number;
  entraAntes: boolean;
  saleDespues: boolean;
}

/**
 * Reparte las barras en subfilas para que <b>lo encimado se vea encimado</b>.
 *
 * ── Por qué esto no es un detalle de presentación ───────────────────────────
 * Sin apilar, dos barras del mismo carril y los mismos días se dibujan una sobre otra y
 * <b>sólo se ve la última</b>. El dibujo que existe para revelar el solape lo escondería,
 * y quien programa concluiría que el vehículo tiene una sola misión ese día. Se detectó
 * mirando la pantalla con dos misiones reales sobre el mismo pick-up.
 *
 * ── El reparto ──────────────────────────────────────────────────────────────
 * Primera subfila donde quepa, recorriendo por fecha de inicio. Es voraz y no busca el
 * mínimo de subfilas: para un carril de flota —dos o tres barras— la diferencia no
 * existe, y un óptimo aquí costaría más de lo que vale.
 *
 * <b>Los dos extremos son inclusivos también acá.</b> Dos barras que se tocan —una termina
 * el jueves y la otra empieza el jueves— <b>se solapan</b>: el vehículo no puede estar
 * volviendo de Danlí y saliendo a Juticalpa el mismo día. Tratarlas como consecutivas las
 * pondría en la misma subfila y borraría justo el conflicto que hay que ver.
 */
function apilar(barras: readonly Colocada[]): readonly (Colocada & { subfila: number })[] {
  const finPorSubfila: number[] = [];

  return [...barras]
    .sort((a, b) => a.inicio - b.inicio || a.fin - b.fin)
    .map((c) => {
      let subfila = finPorSubfila.findIndex((fin) => fin < c.inicio);
      if (subfila === -1) subfila = finPorSubfila.length;
      finPorSubfila[subfila] = c.fin;
      return { ...c, subfila };
    });
}

export default function LineaDeCarriles({
  carriles,
  desde,
  hasta,
  queEsUnaBarra = 'barra',
  referencia,
  vacio,
}: LineaDeCarrilesProps): ReactElement {
  const idBase = useId();

  const dias = useMemo(() => {
    // +1 porque los dos extremos son inclusivos: del lunes al lunes es UN día, no cero.
    const total = Math.max(1, diasEntre(desde, hasta) + 1);
    return Array.from({ length: total }, (_, i) => new Date(aDia(desde) + i * DIA_MS));
  }, [desde, hasta]);

  if (carriles.length === 0) {
    return (
      <div className="tw:py-6 tw:text-center tw:text-sm tw:text-tinta-mid">{vacio}</div>
    );
  }

  const anchoDia = 100 / dias.length;

  return (
    // El desplazamiento horizontal vive acá adentro. Una ventana de treinta días no
    // puede empujar el ancho de la página entera.
    <div className="tw:overflow-x-auto">
      <div className="tw:min-w-[38rem]">
        <EjeDeDias dias={dias} anchoDia={anchoDia} />

        <div className="tw:flex tw:flex-col tw:gap-1 tw:pt-1">
          {carriles.map((carril) => (
            <Carril
              key={carril.id}
              carril={carril}
              dias={dias}
              desde={desde}
              hasta={hasta}
              anchoDia={anchoDia}
              queEsUnaBarra={queEsUnaBarra}
              referencia={referencia}
              idRotulo={`${idBase}-${carril.id}`}
            />
          ))}
        </div>
      </div>
    </div>
  );
}

/**
 * El eje.
 *
 * Los días se rotulan con el número y la inicial del día de la semana. La inicial no es
 * adorno: es lo que deja ver de un golpe que el hueco cae en sábado, y eso es media
 * decisión de programación — circular en día inhábil exige permiso firmado por la
 * máxima autoridad, con salvoconducto impreso.
 */
function EjeDeDias({
  dias,
  anchoDia,
}: {
  dias: readonly Date[];
  anchoDia: number;
}): ReactElement {
  return (
    <div className="tw:flex tw:border-b tw:border-linea-suave tw:pb-1">
      <div className="tw:w-40 tw:shrink-0" />
      <div className="tw:flex tw:min-w-0 tw:grow" aria-hidden>
        {dias.map((dia) => (
          <div
            key={dia.getTime()}
            style={{ width: `${anchoDia}%` }}
            className={[
              'tw:shrink-0 tw:text-center tw:text-[10px] tw:tabular-nums tw:leading-tight',
              esFinDeSemana(dia) ? 'tw:text-tinta-low' : 'tw:text-tinta-mid',
            ].join(' ')}
          >
            <span className="tw:block">{dia.getDate()}</span>
            <span className="tw:block tw:uppercase">{INICIAL_DIA[dia.getDay()]}</span>
          </div>
        ))}
      </div>
    </div>
  );
}

function Carril({
  carril,
  dias,
  desde,
  hasta,
  anchoDia,
  queEsUnaBarra,
  referencia,
  idRotulo,
}: {
  carril: CarrilDeLinea;
  dias: readonly Date[];
  desde: Date;
  hasta: Date;
  anchoDia: number;
  queEsUnaBarra: string;
  referencia?: { fecha: Date; titulo: string };
  idRotulo: string;
}): ReactElement {
  const total = dias.length;

  const colocadas = useMemo(
    () =>
      apilar(
        carril.barras
          .map((barra) => {
            // Recortar contra la ventana, no descartar. Una misión que empezó antes del
            // lunes ocupa el lunes igual, y esconderla ofrecería libre un carril tomado.
            const inicio = Math.max(0, diasEntre(desde, barra.desde));
            const fin = Math.min(total - 1, diasEntre(desde, barra.hasta));
            if (fin < inicio) return null;
            return {
              barra,
              inicio,
              fin,
              largo: fin - inicio + 1,
              entraAntes: aDia(barra.desde) < aDia(desde),
              saleDespues: aDia(barra.hasta) > aDia(hasta),
            };
          })
          .filter((c) => c !== null),
      ),
    [carril.barras, desde, hasta, total],
  );

  // Alto por subfilas: el carril crece con lo que tiene encimado en vez de esconderlo.
  const subfilas = colocadas.reduce((m, c) => Math.max(m, c.subfila + 1), 1);

  return (
    <div className="tw:flex tw:items-stretch">
      <div className="tw:flex tw:w-40 tw:shrink-0 tw:flex-col tw:justify-center tw:pr-3">
        <span
          id={idRotulo}
          className={[
            'tw:truncate tw:text-sm tw:font-medium',
            carril.inhabilitado ? 'tw:text-tinta-low' : '',
          ].join(' ')}
        >
          {carril.titulo}
        </span>
        {carril.detalle ? (
          <span className="tw:truncate tw:text-[11px] tw:text-tinta-mid">
            {carril.detalle}
          </span>
        ) : null}
      </div>

      <div
        className={[
          'tw:relative tw:min-w-0 tw:grow tw:overflow-hidden tw:rounded-sm',
          carril.inhabilitado ? 'tw:opacity-55' : '',
        ].join(' ')}
      >
        <Rejilla dias={dias} anchoDia={anchoDia} />
        {referencia ? (
          <Referencia referencia={referencia} desde={desde} total={total} anchoDia={anchoDia} />
        ) : null}

        {/* La lista textual del carril. Es lo que se anuncia y lo que se recorre con
            teclado; las posiciones en píxeles no llegan a un lector de pantalla. */}
        <ul
          aria-labelledby={idRotulo}
          className="tw:relative tw:m-0 tw:list-none tw:p-0"
          style={{ height: `calc(${subfilas} * ${ALTO_SUBFILA})` }}
        >
          {colocadas.length === 0 ? (
            <li className="tw:sr-only">Sin nada en la ventana.</li>
          ) : null}

          {colocadas.map(({ barra, inicio, largo, subfila, entraAntes, saleDespues }) => (
            <li
              key={barra.id}
              className="tw:absolute"
              style={{
                left: `calc(${inicio * anchoDia}% + 2px)`,
                width: `calc(${largo * anchoDia}% - 4px)`,
                top: `calc(${subfila} * ${ALTO_SUBFILA} + 0.25rem)`,
                height: `calc(${ALTO_SUBFILA} - 0.5rem)`,
              }}
            >
              <Barra
                barra={barra}
                entraAntes={entraAntes}
                saleDespues={saleDespues}
                queEsUnaBarra={queEsUnaBarra}
                recurso={carril.titulo}
              />
            </li>
          ))}
        </ul>
      </div>
    </div>
  );
}

/** Las divisiones del día. Sin ellas la barra flota y no se lee dónde empieza. */
function Rejilla({
  dias,
  anchoDia,
}: {
  dias: readonly Date[];
  anchoDia: number;
}): ReactElement {
  return (
    <div className="tw:absolute tw:inset-0 tw:flex" aria-hidden>
      {dias.map((dia, i) => (
        <div
          key={dia.getTime()}
          style={{ width: `${anchoDia}%` }}
          className={[
            'tw:h-full tw:shrink-0 tw:border-linea-suave',
            i > 0 ? 'tw:border-l' : '',
            // El fin de semana se sombrea: es la mitad de la decisión de si el hueco sirve.
            esFinDeSemana(dia) ? 'tw:bg-inset' : 'tw:bg-subtle',
          ].join(' ')}
        />
      ))}
    </div>
  );
}

function Referencia({
  referencia,
  desde,
  total,
  anchoDia,
}: {
  referencia: { fecha: Date; titulo: string };
  desde: Date;
  total: number;
  anchoDia: number;
}): ReactElement | null {
  const dia = diasEntre(desde, referencia.fecha);
  if (dia < 0 || dia > total - 1) return null;

  return (
    <div
      aria-hidden
      title={referencia.titulo}
      className="tw:pointer-events-none tw:absolute tw:inset-y-0 tw:z-10 tw:w-px tw:bg-acento"
      style={{ left: `${(dia + 0.5) * anchoDia}%` }}
    />
  );
}

/**
 * La barra.
 *
 * ── Las flechas de corte no son decoración ──────────────────────────────────
 * Una barra que empieza antes de la ventana lleva `‹`, y la que sigue después, `›`. Sin
 * eso, un rango recortado se ve idéntico a uno que efectivamente empieza el lunes, y
 * quien programa concluiría que el vehículo se libera el viernes cuando no.
 */
function Barra({
  barra,
  entraAntes,
  saleDespues,
  queEsUnaBarra,
  recurso,
}: {
  barra: BarraDeCarril;
  entraAntes: boolean;
  saleDespues: boolean;
  queEsUnaBarra: string;
  recurso: string;
}): ReactElement {
  // Lo que oye quien no ve el dibujo. Lleva el recurso porque una barra suelta no dice
  // de qué carril es, y el recorrido por teclado salta de barra en barra.
  const anuncio = [
    `${barra.queEs ?? queEsUnaBarra} ${barra.titulo}`,
    barra.detalle,
    `${recurso}, del ${textoDeFecha(barra.desde)} al ${textoDeFecha(barra.hasta)}`,
    entraAntes ? 'empieza antes de la ventana' : null,
    saleDespues ? 'sigue después de la ventana' : null,
  ]
    .filter(Boolean)
    .join('. ');

  const clases = [
    CLASE_TONO[barra.tono ?? 'info'],
    'tw:flex tw:h-full tw:items-center tw:gap-1 tw:overflow-hidden tw:rounded-sm tw:border',
    'tw:px-1.5 tw:text-[11px] tw:font-medium tw:leading-none',
  ].join(' ');

  const contenido = (
    <>
      {entraAntes ? <span aria-hidden>‹</span> : null}
      <span className="tw:truncate">{barra.titulo}</span>
      {saleDespues ? (
        <span aria-hidden className="tw:ml-auto">
          ›
        </span>
      ) : null}
    </>
  );

  if (barra.href) {
    return (
      <a href={barra.href} aria-label={anuncio} className={`${clases} tw:no-underline`}>
        {contenido}
      </a>
    );
  }

  return (
    <span className={clases} role="img" aria-label={anuncio}>
      {contenido}
    </span>
  );
}
