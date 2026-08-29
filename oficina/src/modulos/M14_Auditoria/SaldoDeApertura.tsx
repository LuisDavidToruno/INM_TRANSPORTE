import type { ReactElement } from 'react';
import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { CircleAlert, Archive, Clock } from 'lucide-react';

import { Campo, Nota, Panel, Pastilla } from '../../ui';
import { lempiras } from '../../api/combustible';
import {
  TEXTO_DE_CAUSA_DEL_RENGLON,
  TEXTO_DE_RENGLON,
  inventarioDelSaldo,
  saldosDeApertura,
} from '../../api/conciliacion';
import type { FuenteDelSaldo, RenglonDelSaldo, SaldoDeApertura } from '../../api/conciliacion';
import { soloFecha } from '../M06_Autorizacion/formato';

/**
 * `RN-97` — el saldo de apertura de control interno.
 *
 * ── Lo que esta pantalla existe para impedir ────────────────────────────────
 * *«Llega enero, el sistema arranca con reportes en cero, y una misión interrumpida en
 * noviembre, un préstamo vencido en agosto y una obligación de reintegro de mayo simplemente
 * dejan de aparecer en ninguna pantalla. **Nadie decidió abandonarlos: se abandonaron solos**»*.
 *
 * ── Y las dos cosas que nunca puede esconder ────────────────────────────────
 * **La antigüedad desde el hecho**, que no se reinicia con el cambio de ejercicio; y **las
 * fuentes que no se pudieron consultar**, porque un inventario que se ve completo estando
 * incompleto es el mismo abandono con formato de reporte.
 */
export default function SaldoDeAperturaPantalla(): ReactElement {
  // El corte por defecto es el 31 de diciembre del año anterior: es el que se mira en enero,
  // que es cuando esta pantalla importa.
  const [corte, setCorte] = useState(() => `${new Date().getFullYear() - 1}-12-31`);

  const inventario = useQuery({
    queryKey: ['inventario-del-saldo', corte],
    queryFn: () => inventarioDelSaldo(corte),
  });

  const serie = useQuery({ queryKey: ['saldos-de-apertura'], queryFn: saldosDeApertura });

  if (inventario.isError || serie.isError) {
    return (
      <Nota tono="riesgo" icono={<CircleAlert />}>
        No se pudo cargar el saldo de apertura de control interno.
      </Nota>
    );
  }

  return (
    <div className="tw:flex tw:flex-col tw:gap-5">
      <header className="tw:flex tw:flex-col tw:gap-1">
        <h1 className="tw:text-xl tw:font-semibold tw:tracking-tight">
          Saldo de apertura de control interno
        </h1>
        <p className="tw:text-sm tw:text-tinta-mid">
          Lo que sigue vivo al corte del ejercicio. Sin esto, en enero el sistema arranca en cero
          y lo que quedó abierto deja de aparecer en ninguna pantalla — <b>sin que nadie decida
          abandonarlo</b>.
        </p>
      </header>

      <div className="tw:max-w-xs">
        <Campo
          etiqueta="Corte"
          ayuda="La fecha contra la que se juzga qué estaba vivo. No es hoy: una misión que cerró en febrero no era un pendiente del 31 de diciembre."
        >
          <input
            type="date"
            value={corte}
            onChange={(evento) => setCorte(evento.target.value)}
            className="loki-foco loki-fecha tw:w-full tw:rounded-control tw:border tw:border-linea tw:bg-panel tw:px-2 tw:py-1.5 tw:text-cuerpo-2 tw:font-mono tw:tabular-nums tw:text-tinta-hi"
          />
        </Campo>
      </div>

      {inventario.isPending ? (
        <p className="tw:text-sm tw:text-tinta-mid">Armando el inventario…</p>
      ) : (
        <>
          {!inventario.data.completo && (
            <PanelDeFuentes fuentes={inventario.data.fuentes} />
          )}

          <PanelDeRenglones renglones={inventario.data.renglones} />
        </>
      )}

      {serie.data !== undefined && serie.data.length > 0 && (
        <PanelDeLaSerie saldos={serie.data} />
      )}
    </div>
  );
}

/**
 * Las fuentes que no se pudieron consultar. **Va arriba y no en una nota al pie**: si el lector
 * no ve esto, va a leer el inventario como si fuera todo lo que hay.
 */
function PanelDeFuentes({ fuentes }: { fuentes: FuenteDelSaldo[] }): ReactElement {
  const sinConsultar = fuentes.filter((f) => !f.sePudoConsultar);
  const bloqueantes = sinConsultar.filter(
    (f) => f.tipo === 'PrestamoVencido' || f.tipo === 'InterrupcionSinDesenlace',
  );

  return (
    <Panel>
      <div className="tw:flex tw:flex-col tw:gap-3">
        <Nota tono="riesgo" icono={<CircleAlert />}>
          Este inventario <b>está incompleto</b>: {sinConsultar.length} de {fuentes.length}{' '}
          fuentes no se pudieron consultar.
          {bloqueantes.length > 0 && (
            <>
              {' '}
              Y {bloqueantes.length} de ellas <b>deberían impedir cerrar el período</b> — así que
              ese bloqueo hoy no puede disparar.
            </>
          )}
        </Nota>

        <div className="tw:flex tw:flex-col tw:gap-2">
          {sinConsultar.map((f) => (
            <div
              key={f.tipo}
              className="tw:flex tw:flex-col tw:gap-1 tw:border-l-2 tw:border-riesgo-fg tw:pl-3"
            >
              <span className="tw:text-sm tw:font-medium">
                {TEXTO_DE_RENGLON[f.tipo] ?? f.tipo}
              </span>
              <p className="tw:text-xs tw:text-tinta-mid">{f.porQueNo}</p>
            </div>
          ))}
        </div>
      </div>
    </Panel>
  );
}

function PanelDeRenglones({ renglones }: { renglones: RenglonDelSaldo[] }): ReactElement {
  if (renglones.length === 0) {
    return (
      <Panel>
        <p className="tw:flex tw:items-center tw:gap-2 tw:text-sm tw:text-tinta-mid">
          <Archive className="tw:size-4" aria-hidden />
          No quedó nada vivo al corte, de las fuentes que se pudieron consultar.
        </p>
      </Panel>
    );
  }

  // Por antigüedad: lo más viejo primero. Un inventario ordenado por tipo esconde el renglón de
  // tres ejercicios en medio de una lista de pendientes de la semana pasada.
  const orden = [...renglones].sort((a, b) => b.antiguedadEnDias - a.antiguedadEnDias);
  const arrastrados = orden.filter((r) => r.saldosAnteriores > 0);
  const monto = orden.reduce((s, r) => s + (r.monto ?? 0), 0);

  return (
    <Panel titulo={`${renglones.length} pendiente(s) al corte`}>
      <div className="tw:flex tw:flex-col tw:gap-3">
        <div className="tw:flex tw:flex-wrap tw:gap-x-5 tw:gap-y-1 tw:text-sm">
          <span className="tw:text-tinta-mid">
            el más viejo lleva <b>{orden[0]?.antiguedadEnDias ?? 0} días</b>
          </span>
          {arrastrados.length > 0 && (
            <span className="tw:text-aviso-fg">
              {arrastrados.length} ya venían de saldos anteriores
            </span>
          )}
          {monto !== 0 && (
            <span className="tw:text-tinta-mid">{lempiras(monto)} en total</span>
          )}
        </div>

        <div className="tw:flex tw:flex-col tw:gap-2">
          {orden.map((r) => (
            <FilaDelRenglon key={`${r.tipo}-${r.referencia}`} renglon={r} />
          ))}
        </div>
      </div>
    </Panel>
  );
}

function FilaDelRenglon({ renglon: r }: { renglon: RenglonDelSaldo }): ReactElement {
  const borde = r.impideCerrar
    ? 'tw:border-riesgo-fg'
    : r.saldosAnteriores > 0
      ? 'tw:border-aviso-fg'
      : 'tw:border-borde';

  return (
    <div className={`tw:flex tw:flex-col tw:gap-1 tw:border-l-2 ${borde} tw:pl-3`}>
      <div className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-3">
        <span className="tw:text-sm tw:font-medium">
          {TEXTO_DE_RENGLON[r.tipo] ?? r.tipo}
        </span>
        <span className="tw:font-mono tw:text-xs tw:text-tinta-mid">{r.referencia}</span>
        {r.monto !== null && (
          <span className="tw:text-sm tw:tabular-nums">{lempiras(r.monto)}</span>
        )}
        {r.impideCerrar && <Pastilla tono="riesgo">Impide cerrar</Pastilla>}
      </div>

      <p className="tw:text-xs tw:text-tinta-mid">{r.descripcion}</p>

      <div className="tw:flex tw:flex-wrap tw:items-center tw:gap-x-3 tw:text-xs tw:text-tinta-mid">
        {/* La antigüedad desde el hecho, siempre. Es la parte incómoda de la regla y por eso
            mismo la que sirve. */}
        <span className="tw:flex tw:items-center tw:gap-1">
          <Clock className="tw:size-3" aria-hidden />
          {r.antiguedadEnDias} días desde el {soloFecha(r.fechaDelHecho)}
        </span>

        {r.saldosAnteriores > 0 && (
          <span className="tw:text-aviso-fg">
            ya venía de {r.saldosAnteriores} saldo(s) anterior(es)
          </span>
        )}

        <span>{TEXTO_DE_CAUSA_DEL_RENGLON[r.causa] ?? r.causa}</span>
        <span>a cargo de {r.responsable}</span>
      </div>
    </div>
  );
}

/**
 * La serie histórica — `RN-97` punto 5. Es lo que permite comparar un saldo contra el anterior,
 * y lo que hace visible si el inventario crece o baja.
 */
function PanelDeLaSerie({ saldos }: { saldos: SaldoDeApertura[] }): ReactElement {
  return (
    <Panel titulo="Serie de saldos producidos">
      <div className="tw:flex tw:flex-col tw:gap-2">
        {saldos.map((s) => (
          <div
            key={s.id}
            className="tw:flex tw:flex-col tw:gap-1 tw:border-l-2 tw:border-borde tw:pl-3"
          >
            <div className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-3 tw:text-sm">
              <span className="tw:font-medium">Ejercicio {s.ejercicio}</span>
              <span className="tw:font-mono tw:text-xs tw:text-tinta-mid">{s.folio}</span>
              <span className="tw:text-xs tw:text-tinta-mid">
                {s.renglones} renglón(es)
                {s.arrastrados > 0 && `, ${s.arrastrados} arrastrado(s)`}
              </span>
              {/* El primero no se compara contra los siguientes: es la primera vez que la
                  institución ve todo junto. */}
              {s.esInicialDeImplantacion && (
                <Pastilla tono="info">Inicial de implantación</Pastilla>
              )}
            </div>

            <span className="tw:text-xs tw:text-tinta-mid">
              corte al {soloFecha(s.corte)} · el más viejo, {s.antiguedadMaximaEnDias} días ·
              produjo {s.produce}
              {s.montoTotal !== 0 && ` · ${lempiras(s.montoTotal)}`}
            </span>
          </div>
        ))}
      </div>
    </Panel>
  );
}
