import type { ReactElement } from 'react';
import { useQuery } from '@tanstack/react-query';
import { CircleAlert, HandCoins } from 'lucide-react';

import { Nota, Panel, Pastilla, Vacio } from '../../ui';
import {
  TEXTO_DE_CAUSA,
  TEXTO_DE_OBLIGACION,
  arqueoDeReintegros,
  lempiras,
} from '../../api/combustible';
import type { LoQueDebe, Obligacion, SaldoAfuera } from '../../api/combustible';
import { soloFecha } from '../M06_Autorizacion/formato';

/**
 * El arqueo por persona — `RN-86` punto 6.
 *
 * ── La pregunta que contesta, y que hoy no contesta nadie ───────────────────
 * `CE-26`: <i>«quién tiene cuánto dinero del Estado en la mano, desde cuándo. Hoy ese dato no
 * existe en ninguna parte, y es la primera pregunta de un arqueo»</i>.
 *
 * ── Por qué el que más debe va primero ──────────────────────────────────────
 * Un arqueo ordenado por nombre esconde el caso que importa en medio de una lista. El orden
 * lo da el servidor; acá no se reordena, porque dos criterios de orden sobre la misma lista
 * es una lista que discrepa consigo misma según quién la abrió.
 *
 * ── Y por qué lo que se le debe al servidor está en la misma fila ───────────
 * `CE-26`: <i>«un sistema que solo mide lo que el servidor le debe a la institución no es un
 * sistema de control: es un sistema de cobro»</i>. Ponerlo en otra pestaña sería lo mismo que
 * no tenerlo.
 */
export default function PanelDeArqueo(): ReactElement {
  const { data, isPending, isError } = useQuery({
    queryKey: ['arqueo-de-reintegros'],
    queryFn: arqueoDeReintegros,
  });

  if (isError) {
    return (
      <Nota tono="riesgo" icono={<CircleAlert />}>
        No se pudo cargar el arqueo de saldos pendientes.
      </Nota>
    );
  }

  // **No se afirma un negativo mientras se carga.** «Nadie tiene dinero afuera» dicho antes de
  // saberlo es exactamente la frase que este panel existe para dejar de decir a ciegas.
  if (isPending) {
    return <p className="tw:text-sm tw:text-tinta-mid">Cargando el arqueo…</p>;
  }

  const lista = data ?? [];
  const vencidos = lista.filter((p) => p.vencido);

  if (lista.length === 0) {
    return (
      <Vacio
        icono={<HandCoins />}
        titulo="Nadie tiene dinero del fondo afuera"
        descripcion="Ningún vale vivo con saldo sin comprobar y ninguna obligación de reintegro abierta."
      />
    );
  }

  return (
    <div className="tw:flex tw:flex-col tw:gap-4">
      <div className="tw:flex tw:flex-col tw:gap-1">
        <h2 className="tw:text-base tw:font-semibold tw:tracking-tight">
          Quién tiene dinero afuera
        </h2>
        <p className="tw:text-sm tw:text-tinta-mid">
          {lista.length} {lista.length === 1 ? 'persona' : 'personas'} con saldo sin comprobar u
          obligación de reintegro abierta.
        </p>
      </div>

      {vencidos.length > 0 && (
        <Nota tono="riesgo" icono={<CircleAlert />}>
          {vencidos.length === 1
            ? '1 persona no puede recibir'
            : `${vencidos.length} personas no pueden recibir`}{' '}
          nueva asignación de fondo. El levantamiento es acto de Gerencia Administrativa con
          motivo escrito — <b>no decisión de quien programa ni de quien emite</b>. La otra
          salida es sustituir al motorista de la misión.
        </Nota>
      )}

      <div className="tw:flex tw:flex-col tw:gap-3">
        {lista.map((p) => (
          <FilaDelArqueo key={p.responsable} persona={p} />
        ))}
      </div>
    </div>
  );
}

function FilaDelArqueo({ persona }: { persona: LoQueDebe }): ReactElement {
  return (
    <Panel>
      <div className="tw:flex tw:flex-col tw:gap-3">
        <div className="tw:flex tw:flex-wrap tw:items-start tw:justify-between tw:gap-3">
          <div className="tw:flex tw:flex-col tw:gap-1">
            {/* ⚠️ El ULID del padrón. El nombre del motorista lo tiene `M-05` y el arqueo
                todavía no lo cruza — se muestra lo que identifica sin ambigüedad en vez de
                dejar la fila sin sujeto. */}
            <span className="tw:font-mono tw:text-xs tw:text-tinta-mid">
              {persona.responsable}
            </span>

            <div className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-4 tw:gap-y-1">
              {persona.sinComprobar > 0 && (
                <span className="tw:text-lg tw:font-semibold tw:tabular-nums">
                  {lempiras(persona.sinComprobar)}{' '}
                  <span className="tw:text-xs tw:font-normal tw:text-tinta-mid">
                    sin comprobar
                  </span>
                </span>
              )}

              {persona.aCargo > 0 && (
                <span className="tw:text-lg tw:font-semibold tw:tabular-nums tw:text-riesgo-fg">
                  {lempiras(persona.aCargo)}{' '}
                  <span className="tw:text-xs tw:font-normal tw:text-tinta-mid">
                    de reintegro a su cargo
                  </span>
                </span>
              )}

              {/* Va con el mismo peso visual que lo que debe. Es la mitad del control que
                  distingue un sistema de control de uno de cobro. */}
              {persona.aFavor > 0 && (
                <span className="tw:text-lg tw:font-semibold tw:tabular-nums tw:text-ok-fg">
                  {lempiras(persona.aFavor)}{' '}
                  <span className="tw:text-xs tw:font-normal tw:text-tinta-mid">
                    a favor del servidor
                  </span>
                </span>
              )}
            </div>
          </div>

          <Pastilla tono={persona.vencido ? 'riesgo' : 'aviso'}>
            {persona.vencido ? 'No puede recibir fondo' : 'Dinero afuera, dentro de plazo'}
          </Pastilla>
        </div>

        {persona.saldos.map((s) => (
          <SaldoDeUnVale key={s.vale} saldo={s} />
        ))}

        {persona.obligaciones.map((o) => (
          <ObligacionAbierta key={o.id} obligacion={o} />
        ))}
      </div>
    </Panel>
  );
}

/**
 * Un vale con dinero afuera. **La explicación va siempre**, incluso cuando no está vencido:
 * un monto sin la ventana de tiempo no demuestra si el dinero estuvo afuera dos días o dos
 * meses, que es exactamente lo que el arqueo necesita (`CE-26`, evidencia #5).
 */
function SaldoDeUnVale({ saldo }: { saldo: SaldoAfuera }): ReactElement {
  return (
    <div className="tw:flex tw:flex-col tw:gap-1 tw:border-l-2 tw:border-borde tw:pl-3">
      <div className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-3 tw:text-sm">
        <span className="tw:font-medium tw:tabular-nums">{lempiras(saldo.monto)}</span>
        <span className="tw:text-tinta-mid">vale {saldo.vale}</span>
        {saldo.desde !== null && (
          <span className="tw:text-xs tw:text-tinta-mid">
            afuera desde el {soloFecha(saldo.desde)} · {saldo.diasAfuera}{' '}
            {saldo.diasAfuera === 1 ? 'día' : 'días'}
          </span>
        )}
      </div>

      <p
        className={`tw:text-xs ${
          saldo.vencido ? 'tw:text-riesgo-fg' : 'tw:text-tinta-mid'
        }`}
      >
        {saldo.explicacion}
      </p>
    </div>
  );
}

/**
 * Una obligación abierta. Muestra **monto original y saldo** cuando difieren: `CE-26` exige
 * que el reporte muestre el valor original, el reverso y el resultado — nunca sólo el
 * resultado.
 */
function ObligacionAbierta({ obligacion: o }: { obligacion: Obligacion }): ReactElement {
  const aFavor = o.direccion === 'AFavorDelServidor';

  return (
    <div
      className={`tw:flex tw:flex-col tw:gap-1 tw:border-l-2 tw:pl-3 ${
        aFavor ? 'tw:border-ok-fg' : 'tw:border-riesgo-fg'
      }`}
    >
      <div className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-3 tw:text-sm">
        <span className="tw:font-medium tw:tabular-nums">{lempiras(o.saldo)}</span>
        <span className="tw:text-tinta-mid">
          {TEXTO_DE_CAUSA[o.causa] ?? o.causa}
          {aFavor ? ' — a favor del servidor' : ''}
        </span>
        {o.pagado > 0 && (
          <span className="tw:text-xs tw:text-tinta-mid">
            de {lempiras(o.monto)} originales, {lempiras(o.pagado)} abonados
          </span>
        )}
      </div>

      <p className="tw:text-xs tw:text-tinta-mid">
        {TEXTO_DE_OBLIGACION[o.estado] ?? o.estado} · hecho del {soloFecha(o.fechaDelHecho)},{' '}
        {o.antiguedadEnDias} {o.antiguedadEnDias === 1 ? 'día' : 'días'} de antigüedad
      </p>
    </div>
  );
}
