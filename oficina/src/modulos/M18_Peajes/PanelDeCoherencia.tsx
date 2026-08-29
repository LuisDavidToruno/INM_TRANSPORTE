import type { ReactElement } from 'react';
import { useQuery } from '@tanstack/react-query';
import { CircleAlert, CircleCheck, Route } from 'lucide-react';

import { Nota, Panel, Pastilla } from '../../ui';
import { TEXTO_DE_INCOHERENCIA, coherenciaDeLaMision } from '../../api/peajes';
import type { CoherenciaDeUnVehiculo, Incoherencia } from '../../api/peajes';

/**
 * `RN-37` — el cruce peaje × kilometraje × ruta autorizada, en la liquidación.
 *
 * ── Lo que esta pantalla no puede hacer ─────────────────────────────────────
 * Decir «coherente» cuando lo que pasó es que no se pudo mirar. `RN-37` lo exige para la misión
 * de ruta abierta y vale para las cuatro dimensiones: *«se marca así explícitamente para que la
 * ausencia de hallazgos no se lea como conformidad»*.
 *
 * Por eso hay **tres** estados y no dos: coherente, con hallazgos, y evaluado a medias.
 */
export default function PanelDeCoherencia({ mision }: { mision: string }): ReactElement | null {
  const { data, isPending, isError } = useQuery({
    queryKey: ['coherencia-de-peajes', mision],
    queryFn: () => coherenciaDeLaMision(mision),
  });

  if (isError) {
    return (
      <Nota tono="riesgo" icono={<CircleAlert />}>
        No se pudo evaluar la coherencia de la secuencia de casetas.
      </Nota>
    );
  }

  if (isPending) {
    return <p className="tw:text-sm tw:text-tinta-mid">Cruzando peajes contra la ruta…</p>;
  }

  const dictamenes = data ?? [];

  // Sin pasos registrados no hay nada que cruzar, y decirlo sería ruido en toda misión que no
  // atraviesa un peaje — que son la mayoría.
  if (dictamenes.length === 0) return null;

  return (
    <Panel titulo="Peajes contra la ruta autorizada">
      <div className="tw:flex tw:flex-col tw:gap-4">
        {dictamenes.map((d) => (
          <DictamenDeUnVehiculo key={d.vehiculo} dictamen={d} />
        ))}
      </div>
    </Panel>
  );
}

function DictamenDeUnVehiculo({
  dictamen: d,
}: {
  dictamen: CoherenciaDeUnVehiculo;
}): ReactElement {
  const hallazgos = d.incoherencias.filter((i) => i.esHallazgo);
  const explicadas = d.incoherencias.filter((i) => !i.esHallazgo);

  const estado = hallazgos.length > 0
    ? { tono: 'riesgo' as const, texto: `${hallazgos.length} hallazgo(s)` }
    : d.dimensiones.todas
      ? { tono: 'ok' as const, texto: 'Coherente' }
      : { tono: 'aviso' as const, texto: 'Sin hallazgos, evaluación incompleta' };

  return (
    <div className="tw:flex tw:flex-col tw:gap-3">
      <div className="tw:flex tw:flex-wrap tw:items-center tw:justify-between tw:gap-3">
        <div className="tw:flex tw:items-center tw:gap-2">
          <Route className="tw:size-4 tw:text-tinta-mid" aria-hidden />
          <span className="tw:font-mono tw:text-xs tw:text-tinta-mid">{d.vehiculo}</span>
          <span className="tw:text-xs tw:text-tinta-mid">
            {d.pasosEvaluados} {d.pasosEvaluados === 1 ? 'paso' : 'pasos'}
          </span>
        </div>

        <Pastilla tono={estado.tono}>{estado.texto}</Pastilla>
      </div>

      {/* Las dimensiones que no se pudieron mirar, con su razón. Es la mitad del dictamen: un
          resultado limpio que no pudo evaluar nada no es conformidad, es silencio. */}
      {!d.dimensiones.todas && (
        <div className="tw:flex tw:flex-col tw:gap-1 tw:rounded-control tw:bg-lienzo-alt tw:p-3">
          <span className="tw:text-xs tw:font-medium">Lo que no se pudo evaluar</span>
          {d.dimensiones.porQueNo.map((razon) => (
            <p key={razon} className="tw:text-xs tw:text-tinta-mid">
              {razon}
            </p>
          ))}
        </div>
      )}

      {hallazgos.map((i, n) => (
        <FilaDeIncoherencia key={`${i.tipo}-${n}`} incoherencia={i} />
      ))}

      {/* Las explicadas y las no concluyentes van igual. Que la incoherencia existió y que
          alguien la explicó son dos hechos, y el auditor pregunta por los dos. */}
      {explicadas.map((i, n) => (
        <FilaDeIncoherencia key={`exp-${i.tipo}-${n}`} incoherencia={i} />
      ))}

      {hallazgos.length === 0 && d.dimensiones.todas && (
        <p className="tw:flex tw:items-center tw:gap-2 tw:text-xs tw:text-ok-fg">
          <CircleCheck className="tw:size-4" aria-hidden />
          Las cuatro dimensiones se evaluaron y ninguna produjo hallazgo.
        </p>
      )}
    </div>
  );
}

function FilaDeIncoherencia({ incoherencia: i }: { incoherencia: Incoherencia }): ReactElement {
  const borde = i.esHallazgo
    ? 'tw:border-riesgo-fg'
    : i.justificada
      ? 'tw:border-ok-fg'
      : 'tw:border-aviso-fg';

  return (
    <div className={`tw:flex tw:flex-col tw:gap-1 tw:border-l-2 ${borde} tw:pl-3`}>
      <span className="tw:text-sm tw:font-medium">
        {TEXTO_DE_INCOHERENCIA[i.tipo] ?? i.tipo}
        {i.justificada && ' — justificada'}
        {!i.concluyente && ' — no concluyente'}
      </span>

      <p className="tw:text-xs tw:text-tinta-mid">{i.explicacion}</p>

      {i.justificacion !== null && (
        <p className="tw:text-xs tw:text-ok-fg">{i.justificacion}</p>
      )}
    </div>
  );
}
