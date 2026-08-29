import type { ReactElement } from 'react';
import { useQuery } from '@tanstack/react-query';
import { CircleAlert, FileWarning } from 'lucide-react';

import { Nota, Panel, Pastilla } from '../../ui';
import { lempiras } from '../../api/combustible';
import {
  TEXTO_DE_RESOLUCION,
  hallazgosDeLaMision,
  hallazgosPosteriores,
} from '../../api/conciliacion';
import type { HallazgoPosterior } from '../../api/conciliacion';
import { soloFecha } from '../M06_Autorizacion/formato';

/**
 * Los expedientes de hallazgo posterior — `RN-93`.
 *
 * ── Lo que esta pantalla no puede sugerir ───────────────────────────────────
 * Que el expediente vinculado se pueda reabrir. **Una misión `CERRADA` no se reabre, ni por
 * auditoría**: lo que se entrega a quien la pide es el paquete sellado tal como cerró **más**
 * este expediente. Por eso no hay ninguna acción sobre la misión, sólo sobre el hallazgo.
 *
 * ── Y las dos fechas van siempre juntas ─────────────────────────────────────
 * `RN-93` las exige como campos distintos. La antigüedad se cuenta desde el hecho, y el tiempo
 * que tardó en descubrirse **es un indicador por sí mismo**: un hallazgo de hace dos años
 * descubierto ayer dice algo del control, no sólo del hecho.
 */
export default function PanelDeHallazgos({
  mision,
}: {
  /** Cuando viene, muestra sólo los de esa misión — el caso del expediente de cierre. */
  mision?: string;
}): ReactElement | null {
  const { data, isPending, isError } = useQuery({
    queryKey: mision === undefined ? ['hallazgos'] : ['hallazgos', mision],
    queryFn: () =>
      mision === undefined ? hallazgosPosteriores() : hallazgosDeLaMision(mision),
  });

  if (isError) {
    return (
      <Nota tono="riesgo" icono={<CircleAlert />}>
        No se pudieron cargar los expedientes de hallazgo posterior.
      </Nota>
    );
  }

  if (isPending) {
    return <p className="tw:text-sm tw:text-tinta-mid">Cargando los expedientes…</p>;
  }

  const lista = data ?? [];

  // En el expediente de una misión, no tener hallazgos es lo normal: decirlo en cada cierre
  // sería ruido. En la vista general sí se dice, porque ahí la ausencia es la información.
  if (lista.length === 0 && mision !== undefined) return null;

  const abiertos = lista.filter((h) => h.abierto);
  const efecto = lista.reduce((s, h) => s + h.efectoEconomicoTotal, 0);

  return (
    <Panel titulo={mision === undefined ? undefined : 'Hallazgos posteriores'}>
      <div className="tw:flex tw:flex-col tw:gap-4">
        {mision === undefined && (
          <div className="tw:flex tw:flex-col tw:gap-1">
            <h2 className="tw:text-base tw:font-semibold tw:tracking-tight">
              Expedientes de hallazgo posterior
            </h2>
            <p className="tw:text-sm tw:text-tinta-mid">
              Lo que se descubrió después del cierre. El expediente cerrado{' '}
              <b>no se reabre</b>: se corrige el efecto económico con asiento reverso y el
              histórico ya publicado sigue siendo reproducible.
            </p>
          </div>
        )}

        {lista.length === 0 ? (
          <p className="tw:flex tw:items-center tw:gap-2 tw:text-sm tw:text-tinta-mid">
            <FileWarning className="tw:size-4" aria-hidden />
            No hay expedientes de hallazgo posterior.
          </p>
        ) : (
          <>
            {mision !== undefined && abiertos.length > 0 && (
              <Nota tono="aviso" icono={<CircleAlert />}>
                Este expediente <b>cerró como cerró y no cambia</b>. Lo que sigue abierto es
                el hallazgo posterior, que tiene su propio ciclo — y su resolución tampoco
                alterará esta misión.
              </Nota>
            )}

            {abiertos.length > 0 && mision === undefined && (
              <Nota tono="aviso" icono={<CircleAlert />}>
                {abiertos.length === 1 ? '1 expediente abierto' : `${abiertos.length} expedientes abiertos`}.
                <b> No se cierran sin resolución</b>, y los que queden abiertos al cierre del
                ejercicio integran el saldo de apertura del siguiente con su antigüedad.
                {efecto !== 0 && <> Efecto económico asentado: {lempiras(efecto)}.</>}
              </Nota>
            )}

            <div className="tw:flex tw:flex-col tw:gap-3">
              {lista.map((h) => (
                <FilaDeHallazgo key={h.id} hallazgo={h} />
              ))}
            </div>
          </>
        )}
      </div>
    </Panel>
  );
}

function FilaDeHallazgo({ hallazgo: h }: { hallazgo: HallazgoPosterior }): ReactElement {
  const estado = h.abierto
    ? { tono: 'aviso' as const, texto: 'Abierto' }
    : h.resolucion === 'SinEfecto'
      ? { tono: 'neutro' as const, texto: 'Sin efecto' }
      : { tono: 'ok' as const, texto: 'Resuelto' };

  return (
    <div
      className={`tw:flex tw:flex-col tw:gap-1 tw:border-l-2 tw:pl-3 ${
        h.abierto ? 'tw:border-aviso-fg' : 'tw:border-borde'
      }`}
    >
      <div className="tw:flex tw:flex-wrap tw:items-start tw:justify-between tw:gap-3">
        <span className="tw:text-sm tw:font-medium">{h.tipo}</span>
        <Pastilla tono={estado.tono}>{estado.texto}</Pastilla>
      </div>

      <p className="tw:text-xs tw:text-tinta-mid">
        {h.comoSeDescubrio} · contra {h.fuente}
      </p>

      {/* Las dos fechas juntas y siempre. El tiempo hasta el descubrimiento es un indicador
          por sí mismo — contarlo al revés premiaría descubrir tarde. */}
      <div className="tw:flex tw:flex-wrap tw:items-center tw:gap-x-3 tw:text-xs tw:text-tinta-mid">
        <span>hecho del {soloFecha(h.fechaDelHecho)}</span>
        <span>
          descubierto el {soloFecha(h.fechaDelDescubrimiento)} —{' '}
          {h.diasHastaElDescubrimiento} día(s) después
        </span>
        <span>{h.antiguedadEnDias} días de antigüedad</span>

        {/* Cero misiones no es un defecto: la ausencia de misión ES el hallazgo. */}
        {h.misiones.length === 0 && (
          <span className="tw:text-aviso-fg">sin misión vinculable</span>
        )}
      </div>

      {h.reversos > 0 && (
        <p className="tw:text-xs tw:text-tinta-mid">
          {h.reversos} asiento(s) reverso(s), efecto económico{' '}
          {lempiras(h.efectoEconomicoTotal)} — imputado al período corriente, no al original.
        </p>
      )}

      {h.resolucion !== null && (
        <p className="tw:text-xs tw:text-tinta-mid">
          {TEXTO_DE_RESOLUCION[h.resolucion] ?? h.resolucion}
          {h.fundamento !== null && `: ${h.fundamento}`}
        </p>
      )}
    </div>
  );
}
