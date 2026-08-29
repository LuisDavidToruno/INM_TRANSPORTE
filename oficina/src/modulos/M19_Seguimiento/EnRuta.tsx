import type { ReactElement } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useParams } from 'react-router';
import { CircleAlert, MapPin, TriangleAlert } from 'lucide-react';

import { Nota, Panel, Pastilla, Vacio } from '../../ui';
import type { Tono } from '../../ui';
import { pedir } from '../../api/misiones';
import { diaYHora } from '../M06_Autorizacion/formato';

/**
 * `PT-059` — El detalle de la misión en ruta con sus hitos.
 *
 * ── Qué la separa del tablero ───────────────────────────────────────────────
 * El tablero contesta «¿de cuáles no sé nada?». Ésta contesta «¿qué pasó en esta ruta?», y esa
 * pregunta se responde con la secuencia completa: cada arribo, cada salida, cada estado
 * declarado, y <b>cuánto se esperó en cada sitio y por qué</b>.
 *
 * ── El tiempo en sitio no se le pregunta a nadie ────────────────────────────
 * `RN-76` lo prohíbe expresamente: se deriva de los eventos de arribo y salida. Un tiempo
 * digitado es un tiempo redondeado a la media hora —siempre a favor de quien lo digita— y no
 * serviría para atribuirle un costo a nadie, que es para lo que se mide.
 */
export default function EnRuta(): ReactElement {
  const { id = '' } = useParams();

  const { data, isPending, isError } = useQuery({
    queryKey: ['seguimiento', id],
    queryFn: () => pedir<Detalle>(`/seguimiento/${id}`),
    refetchInterval: 60_000,
  });

  if (isError) {
    return (
      <Vacio
        icono={<CircleAlert />}
        titulo="No se encontró la misión"
        descripcion="Puede que el identificador esté incompleto, o que la misión no exista."
      />
    );
  }

  if (isPending) return <p className="tw:text-sm tw:text-tinta-mid">Cargando la ruta…</p>;

  return (
    <div className="tw:flex tw:flex-col tw:gap-5">
      <header className="tw:flex tw:flex-col tw:gap-1">
        <h1 className="tw:text-xl tw:font-semibold tw:tracking-tight">
          {data.folio} · {data.destino}
        </h1>
        <p className="tw:text-sm tw:text-tinta-mid">
          {data.dependencia} · {data.objetoDelTraslado} · {data.estado}
        </p>
      </header>

      {/* ── Lo último que se sabe, con su antigüedad ───────────────────────── */}
      <Panel titulo="Lo último que declaró el motorista">
        <div className="tw:flex tw:flex-col tw:gap-2">
          <div className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-3">
            {data.ultimoEstadoDeclarado === null ? (
              <span className="tw:italic tw:text-sm tw:text-tinta-mid">
                No hay ningún estado declarado.
              </span>
            ) : (
              <>
                <span className="tw:text-lg tw:font-medium">{data.ultimoEstadoDeclarado}</span>
                {data.declaradoEl !== null && (
                  <span className="tw:text-sm tw:text-tinta-mid">
                    declarado a las {diaYHora(data.declaradoEl)}
                  </span>
                )}
              </>
            )}
          </div>

          <div className="tw:flex tw:flex-wrap tw:items-center tw:gap-2">
            <Pastilla tono={TONO_FRESCURA[data.frescura.grado] ?? 'neutro'}>
              {antiguedad(data.frescura.minutos)}
            </Pastilla>
            <span className="tw:text-xs tw:text-tinta-mid">{data.frescura.porQue}</span>
          </div>
        </div>
      </Panel>

      {/* ── Las estadías ───────────────────────────────────────────────────── */}
      <Panel
        titulo={
          data.estadias.length === 0
            ? 'Tiempo en sitio'
            : `Tiempo en sitio · ${data.estadias.length} estadía(s)`
        }
      >
        {data.estadias.length === 0 ? (
          <p className="tw:text-sm tw:text-tinta-mid">
            Todavía no hay arribos registrados. El tiempo en sitio se deriva de los eventos de
            arribo y salida — no se le pide al motorista que lo cronometre.
          </p>
        ) : (
          <ul className="tw:flex tw:flex-col tw:gap-3">
            {data.estadias.map((e, i) => (
              <li
                key={`${e.destino}-${e.arribo}-${i}`}
                className="tw:flex tw:flex-col tw:gap-0.5 tw:border-l-2 tw:border-linea tw:pl-3"
              >
                <div className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-3 tw:text-sm">
                  <span className="tw:font-medium">{e.destino}</span>
                  <span className="tw:tabular-nums">{duracion(e.minutos)}</span>

                  {/* La salida derivada se marca. `RN-76` lo pide: un tiempo deducido no puede
                      leerse con la misma confianza que uno declarado. */}
                  {e.como !== 'Declarada' && (
                    <Pastilla tono={e.como === 'SinCerrar' ? 'info' : 'aviso'}>
                      {ETIQUETA_SALIDA[e.como]}
                    </Pastilla>
                  )}

                  {e.esImproductiva === true && <Pastilla tono="riesgo">improductiva</Pastilla>}
                  {e.esImproductiva === null && (
                    <Pastilla tono="neutro">sin tipificar</Pastilla>
                  )}
                </div>

                <span className="tw:text-xs tw:text-tinta-mid">
                  arribó {diaYHora(e.arribo)}
                  {e.salida !== null
                    ? ` · salió ${diaYHora(e.salida)}`
                    : ' · sigue en el sitio'}
                </span>

                {e.causa !== null && (
                  <span className="tw:text-xs tw:text-tinta-mid">
                    {e.causa}
                    {e.seAtribuyeA !== null && ` · se atribuye a ${e.seAtribuyeA}`}
                    {e.motorEncendido === true && ' · con el motor encendido'}
                  </span>
                )}
              </li>
            ))}
          </ul>
        )}
      </Panel>

      {/* Lo que no se pudo clasificar va al lado del total, no escondido. */}
      {(data.sinTipificar > 0 || data.sinCatalogoDeCausas) && (
        <Nota tono="aviso">
          {data.sinCatalogoDeCausas ? (
            <>
              <b>El catálogo de causas de espera no está poblado</b>, así que ninguna estadía
              está clasificada. El total improductivo no dice cero: dice que no se puede
              calcular.
            </>
          ) : (
            <>
              <b>
                {data.sinTipificar === 1
                  ? '1 estadía quedó sin tipificar'
                  : `${data.sinTipificar} estadías quedaron sin tipificar`}
              </b>
              . Sin causa declarada no se sabe si la espera fue productiva, y{' '}
              <b>«no se sabe» no es «fue productiva»</b>: contarlas como normales reportaría
              menos horas improductivas de las que hubo.
            </>
          )}
        </Nota>
      )}

      {/* Huecos: una salida sin arribo. */}
      {data.salidasSinArribo.length > 0 && (
        <Nota tono="riesgo" icono={<TriangleAlert />}>
          <b>
            {data.salidasSinArribo.length === 1
              ? 'Hay 1 salida sin su arribo'
              : `Hay ${data.salidasSinArribo.length} salidas sin su arribo`}
          </b>
          . No se completó con un arribo inventado: eso produciría una estadía de cero minutos
          que se leería como que no esperó nada.
          <ul className="tw:mt-2 tw:flex tw:flex-col tw:gap-1 tw:pl-4">
            {data.salidasSinArribo.map((x, i) => (
              <li key={`${x.destino}-${i}`} className="tw:list-disc tw:text-xs">
                {x.destino} · {diaYHora(x.momento)}
              </li>
            ))}
          </ul>
        </Nota>
      )}

      {/* ── Los hitos, en orden de la hora del HECHO ───────────────────────── */}
      <Panel titulo={`Hitos · ${data.hitos.length}`}>
        {data.hitos.length === 0 ? (
          <p className="tw:text-sm tw:text-tinta-mid">
            No se ha recibido ningún reporte de campo. Eso no dice que no haya pasado nada: dice
            que el dispositivo no ha tenido señal, o que nadie declaró.
          </p>
        ) : (
          <ol className="tw:flex tw:flex-col tw:gap-2">
            {data.hitos.map((h) => (
              <li
                key={h.id}
                className="tw:flex tw:flex-col tw:gap-0.5 tw:border-l-2 tw:border-linea tw:py-1 tw:pl-3"
              >
                <div className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-3 tw:text-sm">
                  <Pastilla tono={TONO_HITO[h.tipo] ?? 'neutro'}>
                    {ETIQUETA_HITO[h.tipo] ?? h.tipo}
                  </Pastilla>
                  <span className="tw:font-medium">{h.estado ?? h.destino}</span>
                  <span className="tw:text-xs tw:text-tinta-mid">
                    {diaYHora(h.momentoDelHecho)}
                  </span>
                </div>

                {/* El desfase NO es un error. Mide cuánto estuvo el dispositivo sin cobertura,
                    y `RN-43` lo espera. Se muestra cuando es grande porque explica por qué el
                    hito apareció recién ahora. */}
                {h.desfaseMinutos >= 60 && (
                  <span className="tw:text-xs tw:text-tinta-mid">
                    llegó al sistema {duracion(h.desfaseMinutos)} después — el equipo estuvo sin
                    señal
                  </span>
                )}

                {h.posicion !== null && (
                  <span className="tw:flex tw:items-center tw:gap-1 tw:font-mono tw:text-xs tw:text-tinta-mid">
                    <MapPin className="tw:size-3" aria-hidden />
                    {h.posicion.latitud}, {h.posicion.longitud}
                    {h.posicion.precisionMetros !== null &&
                      ` · ±${h.posicion.precisionMetros} m`}
                  </span>
                )}
              </li>
            ))}
          </ol>
        )}
      </Panel>
    </div>
  );
}

/** Nulo no se dibuja como cero, y negativo no se aplasta. Ver `PT-058`. */
function antiguedad(minutos: number | null): string {
  if (minutos === null) return 'sin dato';
  if (minutos < 0) return 'reloj del equipo adelantado';
  if (minutos < 1) return 'hace menos de un minuto';
  return `hace ${duracion(minutos)}`;
}

function duracion(minutos: number): string {
  const dias = Math.floor(minutos / 1440);
  const horas = Math.floor((minutos % 1440) / 60);
  const min = Math.floor(minutos % 60);

  const partes: string[] = [];
  if (dias > 0) partes.push(`${dias} d`);
  if (horas > 0) partes.push(`${horas} h`);
  if (dias === 0 && min > 0) partes.push(`${min} min`);

  return partes.length === 0 ? 'menos de un minuto' : partes.join(' ');
}

const ETIQUETA_SALIDA: Record<string, string> = {
  DerivadaDelSiguienteEvento: 'salida no declarada, derivada',
  SinCerrar: 'sigue en el sitio',
};

const ETIQUETA_HITO: Record<string, string> = {
  EstadoDeclarado: 'estado',
  Arribo: 'arribo',
  Salida: 'salida',
};

const TONO_HITO: Record<string, Tono> = {
  EstadoDeclarado: 'neutro',
  Arribo: 'info',
  Salida: 'ok',
};

const TONO_FRESCURA: Record<string, Tono> = {
  Fresco: 'ok',
  Degradado: 'aviso',
  NoSeClasifica: 'neutro',
  RelojAdelantado: 'riesgo',
  NuncaHuboDato: 'info',
};

interface Detalle {
  mision: string;
  folio: string;
  estado: string;
  dependencia: string;
  destino: string;
  objetoDelTraslado: string;
  ultimoEstadoDeclarado: string | null;
  declaradoEl: string | null;
  frescura: { grado: string; minutos: number | null; porQue: string };
  hitos: {
    id: string;
    tipo: string;
    estado: string | null;
    destino: string | null;
    momentoDelHecho: string;
    momentoDeCaptura: string;
    /** Cuánto tardó en llegar al sistema. No es un error: mide la falta de cobertura. */
    desfaseMinutos: number;
    posicion: { latitud: number; longitud: number; precisionMetros: number | null } | null;
    causaDeEspera: string | null;
    seAtribuyeA: string | null;
    motorEncendido: boolean | null;
    declara: string;
  }[];
  estadias: {
    destino: string;
    arribo: string;
    /** Nula mientras siga en el sitio. */
    salida: string | null;
    como: string;
    minutos: number;
    causa: string | null;
    seAtribuyeA: string | null;
    motorEncendido: boolean | null;
    /** **Nulo es «no se pudo clasificar»**, nunca falso. */
    esImproductiva: boolean | null;
  }[];
  salidasSinArribo: { destino: string; momento: string }[];
  improductivoMinutos: number;
  sinTipificar: number;
  sinCatalogoDeCausas: boolean;
}
