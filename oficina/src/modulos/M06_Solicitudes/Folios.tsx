import type { ReactElement } from 'react';
import { useQuery } from '@tanstack/react-query';
import { CircleAlert, Hash } from 'lucide-react';

import { Nota, Panel, Pastilla, Vacio } from '../../ui';
import type { Tono } from '../../ui';
import { pedir } from '../../api/misiones';

/**
 * El control de folios por delegación — `RNF-21`.
 *
 * ── Por qué esto tiene que estar «disponible en línea» ──────────────────────
 * `RNF-21` lo pide así, y la razón es operativa: **reponer un rango exige conectividad**. Una
 * delegación que se entera de que se le acabaron los folios cuando ya no le quedan, y que lleva
 * cuatro días sin enlace, no puede emitir una orden de misión — y el control en carretera es
 * físico, así que el vehículo no sale.
 *
 * El aviso tiene que llegar **con antelación suficiente para reponer**, que es lo que la métrica
 * llama <i>cero agotamientos sin aviso previo</i>.
 */
export default function Folios(): ReactElement {
  const { data, isPending, isError } = useQuery({
    queryKey: ['folios'],
    queryFn: () => pedir<Rango[]>('/folios'),
  });

  if (isError) {
    return (
      <Nota tono="riesgo" icono={<CircleAlert />}>
        No se pudo cargar el control de folios.
      </Nota>
    );
  }

  const sinUmbral = (data ?? []).filter((r) => r.grado === 'NoSeEvalua').length;

  return (
    <div className="tw:flex tw:flex-col tw:gap-5">
      <header className="tw:flex tw:flex-col tw:gap-1">
        <h1 className="tw:text-xl tw:font-semibold tw:tracking-tight">Control de folios</h1>
        <p className="tw:text-sm tw:text-tinta-mid">
          Los rangos reservados a cada delegación, con su saldo. <b>Reponer exige conectividad</b>
          , así que el aviso tiene que llegar antes de que se acaben.
        </p>
      </header>

      {sinUmbral > 0 && (
        <Nota tono="aviso">
          <b>No hay umbral de aviso fijado</b> (
          <code className="tw:font-mono tw:text-xs">insumo #34</code>), así que{' '}
          {sinUmbral === 1 ? 'un rango no se evalúa' : `${sinUmbral} rangos no se evalúan`} y{' '}
          <b>no habrá aviso previo cuando se agoten</b>. Un tablero callado por falta de
          parámetro se ve igual que uno callado porque todo está bien.
        </Nota>
      )}

      {isPending ? (
        <p className="tw:text-sm tw:text-tinta-mid">Cargando…</p>
      ) : data.length === 0 ? (
        <Vacio
          icono={<Hash />}
          titulo="Ninguna delegación tiene rango asignado"
          descripcion="Mientras no lo tengan, las órdenes de misión se emiten con folio provisional — marcado como tal, para que nadie lo cite en un descargo."
        />
      ) : (
        <Panel titulo={`${data.length} rango(s)`}>
          <ul className="tw:flex tw:flex-col tw:gap-3">
            {data.map((r) => (
              <li
                key={r.rango}
                className={`tw:flex tw:flex-col tw:gap-0.5 tw:border-l-2 tw:py-1 tw:pl-3 ${
                  BORDE[r.grado] ?? 'tw:border-linea'
                }`}
              >
                <div className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-3 tw:text-sm">
                  <span className="tw:font-medium">{r.delegacion}</span>
                  <span className="tw:text-tinta-mid">{r.tipoDeDocumento}</span>

                  {/* Sin dispositivo, el rango es de toda la delegación — y eso sólo sirve
                      con un equipo emitiendo. */}
                  {r.dispositivo !== null && (
                    <span className="tw:font-mono tw:text-xs tw:text-tinta-mid">
                      {r.dispositivo}
                    </span>
                  )}

                  <Pastilla tono={TONO[r.grado] ?? 'neutro'}>{ETIQUETA[r.grado]}</Pastilla>
                </div>

                <span className="tw:font-mono tw:text-xs tw:text-tinta-mid tw:tabular-nums">
                  {r.desde}–{r.hasta} · {r.emitidos} emitidos · {r.disponibles} disponibles
                </span>

                <span className="tw:text-xs tw:text-tinta-mid">{r.porQue}</span>
              </li>
            ))}
          </ul>
        </Panel>
      )}

      <Nota tono="info">
        <b>Los emitidos incluyen los anulados.</b> El folio de un documento anulado no vuelve al
        rango: deja un hueco, y el hueco se explica con su asiento reverso. Un correlativo con
        huecos es normal; <b>uno reutilizado es un expediente que sustituye a otro</b>.
      </Nota>
    </div>
  );
}

const ETIQUETA: Record<string, string> = {
  Suficiente: 'con saldo',
  PorAgotarse: 'por agotarse',
  Agotado: 'agotado',
  NoSeEvalua: 'sin umbral que lo juzgue',
};

const TONO: Record<string, Tono> = {
  Suficiente: 'ok',
  PorAgotarse: 'aviso',
  Agotado: 'riesgo',
  NoSeEvalua: 'neutro',
};

const BORDE: Record<string, string> = {
  Suficiente: 'tw:border-ok-fg',
  PorAgotarse: 'tw:border-aviso-fg',
  Agotado: 'tw:border-riesgo-fg',
  NoSeEvalua: 'tw:border-linea',
};

interface Rango {
  rango: string;
  delegacion: string;
  tipoDeDocumento: string;
  /** **Nulo es «toda la delegación»**, y sólo sirve con un equipo emitiendo. */
  dispositivo: string | null;
  desde: number;
  hasta: number;
  /** Incluye los anulados: un folio anulado no vuelve al rango. */
  emitidos: number;
  disponibles: number;
  grado: string;
  porQue: string;
}
