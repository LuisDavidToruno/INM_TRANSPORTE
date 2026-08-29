import type { ReactElement } from 'react';
import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { CircleAlert, FileSearch } from 'lucide-react';

import { Campo, Nota, Panel, Pastilla, RangoFechas, Vacio } from '../../ui';
import type { Tono } from '../../ui';
import { pedir } from '../../api/misiones';
import { diaYHora } from '../M06_Autorizacion/formato';

/**
 * `PT-088` — La consulta de la pista de auditoría.
 *
 * ── Qué la distingue del rastro de un expediente ────────────────────────────
 * `PT-089` contesta *«¿qué pasó con esta misión?»*. Ésta contesta *«¿qué pasó en la
 * institución?»* — y son preguntas distintas: la reincidencia de una persona a través de varios
 * expedientes no se ve mirando un expediente a la vez.
 *
 * ── Por qué declara lo que le falta ─────────────────────────────────────────
 * La ficha de `ACT-12` enumera cinco fuentes y hoy existen tres. **Una pista que muestra tres
 * sin decir que faltan dos se lee como completa**, y quien audite concluiría que no hubo actos
 * en régimen de excepción cuando lo que pasa es que nadie los registra. Eso es peor que no
 * tener la pantalla.
 */
export default function Pista(): ReactElement {
  const [desde, setDesde] = useState(() => hace(30));
  const [hasta, setHasta] = useState(() => hoy());

  const { data, isPending, isError } = useQuery({
    queryKey: ['pista', desde, hasta],
    queryFn: () => pedir<PistaDeAuditoria>(`/auditoria?desde=${desde}&hasta=${hasta}`),
  });

  if (isError) {
    return (
      <Nota tono="riesgo" icono={<CircleAlert />}>
        No se pudo cargar la pista de auditoría.
      </Nota>
    );
  }

  return (
    <div className="tw:flex tw:flex-col tw:gap-5">
      <header className="tw:flex tw:flex-col tw:gap-1">
        <h1 className="tw:text-xl tw:font-semibold tw:tracking-tight">Pista de auditoría</h1>
        <p className="tw:text-sm tw:text-tinta-mid">
          Qué pasó en la institución en un rango. <b>No es el diario de un expediente</b> — eso
          se consulta expediente por expediente en el rastro.
        </p>
      </header>

      <Panel titulo="En qué rango">
        <div className="tw:sm:max-w-lg">
          <Campo
            etiqueta="Período"
            ayuda="Los dos extremos inclusive. Por omisión, los últimos treinta días."
          >
            <RangoFechas
              desde={desde}
              hasta={hasta}
              onCambiar={(d, h) => {
                setDesde(d);
                setHasta(h);
              }}
            />
          </Campo>
        </div>
      </Panel>

      {isPending ? (
        <p className="tw:text-sm tw:text-tinta-mid">Cargando la pista…</p>
      ) : (
        <>
          <div className="tw:grid tw:gap-3 tw:sm:grid-cols-3">
            <Recuento cantidad={data.intentosBloqueados} texto="intentos bloqueados" />
            <Recuento cantidad={data.tareasEscaladas} texto="tareas escaladas" />
            <Recuento cantidad={data.cambiosDeParametros} texto="cambios de parámetros" />
          </div>

          {/* Lo que hace honesta a esta pantalla. */}
          <Nota tono="aviso" icono={<CircleAlert />}>
            <b>Esta pista no está completa, y por eso lo dice.</b> La ficha de{' '}
            <code className="tw:font-mono tw:text-xs">ACT-12</code> enumera fuentes que el
            sistema todavía no registra:
            <ul className="tw:mt-2 tw:flex tw:flex-col tw:gap-1 tw:pl-4">
              {data.fuentesQueFaltan.map((f) => (
                <li key={f} className="tw:list-disc">
                  {f}
                </li>
              ))}
            </ul>
          </Nota>

          {data.asientos.length === 0 ? (
            <Vacio
              icono={<FileSearch />}
              titulo="Sin asientos en ese rango"
              descripcion="No hubo intentos bloqueados, tareas escaladas ni cambios de parámetros. Con las fuentes que faltan, eso no significa que no haya pasado nada."
            />
          ) : (
            <Panel titulo={`${data.asientos.length} asiento(s)`}>
              <ul className="tw:flex tw:flex-col tw:gap-2">
                {data.asientos.map((a, i) => (
                  <li
                    key={`${a.momento}-${i}`}
                    className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-3 tw:border-l-2 tw:border-linea tw:pl-3 tw:text-sm"
                  >
                    <Pastilla tono={TONO[a.tipo] ?? 'neutro'}>{a.tipo}</Pastilla>
                    <span className="tw:font-medium">{a.quien}</span>
                    <span className="tw:text-tinta-mid">{a.detalle}</span>
                    <span className="tw:text-xs tw:text-tinta-mid">{diaYHora(a.momento)}</span>
                  </li>
                ))}
              </ul>
            </Panel>
          )}
        </>
      )}
    </div>
  );
}

function Recuento({ cantidad, texto }: { cantidad: number; texto: string }): ReactElement {
  return (
    <div className="tw:flex tw:flex-col tw:gap-1 tw:rounded-panel tw:border tw:border-linea tw:bg-panel tw:p-3">
      <span className="tw:text-2xl tw:font-semibold tw:tabular-nums">{cantidad}</span>
      <span className="tw:text-sm tw:text-tinta-mid">{texto}</span>
    </div>
  );
}

const TONO: Record<string, Tono> = {
  'Intento bloqueado': 'riesgo',
  'Tarea escalada': 'aviso',
  'Cambio de parámetro': 'info',
};

const hoy = (): string => new Date().toISOString().slice(0, 10);

const hace = (dias: number): string =>
  new Date(Date.now() - dias * 86_400_000).toISOString().slice(0, 10);

interface PistaDeAuditoria {
  desde: string;
  hasta: string;
  intentosBloqueados: number;
  tareasEscaladas: number;
  cambiosDeParametros: number;
  /** Las fuentes que la ficha de `ACT-12` enumera y el sistema todavía no registra. */
  fuentesQueFaltan: string[];
  asientos: { momento: string; tipo: string; quien: string; detalle: string }[];
}
