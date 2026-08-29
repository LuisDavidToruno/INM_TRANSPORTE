import type { ReactElement } from 'react';
import { useQuery } from '@tanstack/react-query';
import { CircleAlert, Milestone } from 'lucide-react';

import { Nota, Panel, Pastilla, Vacio } from '../../ui';
import { lempiras } from '../../api/combustible';
import {
  TEXTO_DE_ESTADO_DEL_PUNTO,
  discrepanciasDePeaje,
  puntosDePeaje,
} from '../../api/peajes';
import type { PasoPorCaseta, PuntoDePeaje } from '../../api/peajes';
import { momentoCompleto, soloFecha } from '../M06_Autorizacion/formato';

/**
 * `M-18` — el catálogo de peajes y las discrepancias de clasificación.
 *
 * ── Las dos cosas que esta pantalla tiene que dejar ver ─────────────────────
 * **Qué tarifas están cargadas y desde cuándo.** `NRM-10` documenta que la tarifa cambia al
 * menos una vez al año, en enero, con alta probabilidad de reversión a mitad de proceso: en
 * 2026 hubo anuncio el 08/01, suspensión hacia el 15/01, prórroga al 15/02 y confirmación el
 * 28/02 de que no habría incremento. Una tabla que nadie mira se queda vieja sin que nadie lo
 * note, y entonces cada estimado sale mal.
 *
 * **Dónde nos están cobrando mal.** Es el insumo del expediente de reclamo ante la SAPP.
 */
export default function Peajes(): ReactElement {
  const puntos = useQuery({ queryKey: ['puntos-de-peaje'], queryFn: puntosDePeaje });
  const discrepancias = useQuery({
    queryKey: ['discrepancias-de-peaje'],
    queryFn: discrepanciasDePeaje,
  });

  if (puntos.isError || discrepancias.isError) {
    return (
      <Nota tono="riesgo" icono={<CircleAlert />}>
        No se pudo cargar el catálogo de peajes.
      </Nota>
    );
  }

  const lista = puntos.data ?? [];
  const cobrosMalos = discrepancias.data ?? [];
  const sinTarifa = lista.filter((p) => p.estado === 'Activo' && p.tarifas.length === 0);
  const viejas = lista.filter((p) => p.tarifas.some((t) => t.sinRevisar));

  return (
    <div className="tw:flex tw:flex-col tw:gap-5">
      <header className="tw:flex tw:flex-col tw:gap-1">
        <h1 className="tw:text-xl tw:font-semibold tw:tracking-tight">Peajes</h1>
        <p className="tw:text-sm tw:text-tinta-mid">
          {puntos.isPending
            ? 'Cargando…'
            : `${lista.length} ${lista.length === 1 ? 'punto' : 'puntos'} en el catálogo.`}
        </p>
      </header>

      {/* El sistema arranca sin tarifas cargadas (`[C]` insumo #21), y eso es deliberado:
          `RN-34` prefiere bloquear la estimación con mensaje claro antes que arrancar con un
          número inventado que se convertiría en verdad institucional en una semana. Pero tiene
          que verse. */}
      {sinTarifa.length > 0 && (
        <Nota tono="riesgo" icono={<CircleAlert />}>
          {sinTarifa.length === 1 ? '1 punto cobra y no tiene' : `${sinTarifa.length} puntos cobran y no tienen`}{' '}
          tarifa cargada: <b>ningún estimado que los atraviese se puede calcular</b>. No se pone
          un número por omisión — la tarifa se confirma con la SAPP o con COVI-H y se carga con
          su fuente.
        </Nota>
      )}

      {viejas.length > 0 && (
        <Nota tono="aviso">
          {viejas.length === 1 ? '1 punto tiene' : `${viejas.length} puntos tienen`} tarifas sin
          revisar hace más de un año. La tarifa de peaje <b>cambia al menos una vez al año, en
          enero</b>, y con frecuencia se anuncia, se suspende y se revierte en el mismo
          trimestre. Los estimados siguen calculándose: una tarifa vieja es la mejor información
          que hay, pero conviene confirmarla.
        </Nota>
      )}

      {cobrosMalos.length > 0 && (
        <PanelDeDiscrepancias pasos={cobrosMalos} />
      )}

      {puntos.isPending ? (
        <p className="tw:text-sm tw:text-tinta-mid">Cargando el catálogo…</p>
      ) : lista.length === 0 ? (
        <Vacio
          icono={<Milestone />}
          titulo="No hay puntos de peaje cargados"
          descripcion="Sin catálogo no se puede estimar el costo de ninguna ruta. Los puntos y sus tarifas se cargan con su fuente — SAPP, COVI-H o el comunicado de la SIT — y su fecha de verificación."
        />
      ) : (
        <div className="tw:flex tw:flex-col tw:gap-3">
          {lista.map((p) => (
            <TarjetaDePunto key={p.id} punto={p} />
          ))}
        </div>
      )}
    </div>
  );
}

function TarjetaDePunto({ punto: p }: { punto: PuntoDePeaje }): ReactElement {
  // Los tres casos se dicen distinto. Nulo no es «activo»: es que nadie declaró el estado, y
  // sin él no se puede recalcular un viaje pasado por una caseta que ya no existe.
  const estado =
    p.estado === null
      ? { tono: 'aviso' as const, texto: 'Sin estado declarado' }
      : p.estado === 'Activo'
        ? { tono: 'ok' as const, texto: TEXTO_DE_ESTADO_DEL_PUNTO[p.estado] ?? p.estado }
        : { tono: 'aviso' as const, texto: TEXTO_DE_ESTADO_DEL_PUNTO[p.estado] ?? p.estado };

  return (
    <Panel>
      <div className="tw:flex tw:flex-col tw:gap-3">
        <div className="tw:flex tw:flex-wrap tw:items-start tw:justify-between tw:gap-3">
          <div className="tw:flex tw:flex-col tw:gap-1">
            <span className="tw:font-medium">{p.nombre}</span>
            <span className="tw:text-xs tw:text-tinta-mid">
              {p.carretera} · {p.operador}
              {/* Nulo es la condición normal: cobra en ambos sentidos. Importa para contar
                  cruces — un punto de sentido único no se cruza dos veces en ida y vuelta. */}
              {p.sentidoDeCobro !== null && ` · sólo sentido ${p.sentidoDeCobro}`}
            </span>
          </div>

          <Pastilla tono={estado.tono}>{estado.texto}</Pastilla>
        </div>

        {p.fundamentoDelEstado !== null && (
          <p className="tw:text-xs tw:text-tinta-mid">{p.fundamentoDelEstado}</p>
        )}

        {p.tarifas.length === 0 ? (
          <p className="tw:text-xs tw:text-aviso-fg">
            Sin tarifa vigente cargada. Ningún estimado que atraviese este punto se puede
            calcular, y el sistema lo dice en vez de suponer un monto.
          </p>
        ) : (
          <div className="tw:flex tw:flex-col tw:gap-1">
            {p.tarifas.map((t) => (
              <div
                key={t.categoria}
                className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-3 tw:text-sm"
              >
                <span className="tw:min-w-40 tw:text-tinta-mid">{t.categoria}</span>
                <span className="tw:font-medium tw:tabular-nums">{lempiras(t.monto)}</span>
                <span
                  className={`tw:text-xs ${
                    t.sinRevisar ? 'tw:text-aviso-fg' : 'tw:text-tinta-mid'
                  }`}
                >
                  vigente desde el {soloFecha(t.desde)} · fuente {t.fuente} · verificada el{' '}
                  {soloFecha(t.verificada)}
                  {t.sinRevisar && ' — sin revisar hace más de un año'}
                </span>
              </div>
            ))}
          </div>
        )}
      </div>
    </Panel>
  );
}

/**
 * Dónde nos están cobrando mal — `RN-36` punto 4, el insumo del reclamo ante la SAPP.
 *
 * Muestra **las dos categorías**, que es el punto entero: si el sistema ajustara la del vehículo
 * al cobro recibido, el error de la caseta se volvería la verdad institucional y el reclamo
 * nunca ocurriría.
 */
function PanelDeDiscrepancias({ pasos }: { pasos: PasoPorCaseta[] }): ReactElement {
  const deMas = pasos.reduce((suma, p) => suma + Math.max(0, p.diferencia ?? 0), 0);

  return (
    <Panel>
      <div className="tw:flex tw:flex-col tw:gap-3">
        <Nota tono="riesgo" icono={<CircleAlert />}>
          {pasos.length === 1 ? '1 paso se cobró' : `${pasos.length} pasos se cobraron`} con una
          categoría distinta a la del vehículo
          {deMas > 0 && <> — {lempiras(deMas)} de más</>}. La categoría del vehículo{' '}
          <b>no se ajusta al cobro recibido</b>: si lo hiciera, el error de la caseta sería la
          verdad institucional y el reclamo ante la SAPP nunca ocurriría.
        </Nota>

        <div className="tw:flex tw:flex-col tw:gap-2">
          {pasos.map((p) => (
            <div
              key={p.id}
              className="tw:flex tw:flex-col tw:gap-1 tw:border-l-2 tw:border-riesgo-fg tw:pl-3"
            >
              <div className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-3 tw:text-sm">
                <span className="tw:font-medium tw:tabular-nums">
                  {lempiras(p.montoPagado)}
                </span>
                {p.montoEsperado !== null && (
                  <span className="tw:text-xs tw:text-tinta-mid">
                    en vez de {lempiras(p.montoEsperado)}
                  </span>
                )}
                <span className="tw:text-xs tw:text-riesgo-fg">
                  cobrado como <b>{p.categoriaCobrada}</b>, corresponde{' '}
                  <b>{p.categoriaEsperada}</b>
                </span>
              </div>

              <span className="tw:text-xs tw:text-tinta-mid">
                {momentoCompleto(p.momento)} · registró {p.registra}
                {/* Sin ticket el expediente ante la SAPP es la palabra del motorista. */}
                {p.ticket === null && ' · SIN TICKET'}
              </span>
            </div>
          ))}
        </div>
      </div>
    </Panel>
  );
}
