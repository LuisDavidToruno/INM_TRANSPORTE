import type { ReactElement } from 'react';
import { useQuery } from '@tanstack/react-query';
import { CalendarClock } from 'lucide-react';

import { Nota, Panel, Pastilla } from '../../ui';
import { pedir } from '../../api/misiones';
import { soloFecha } from '../M06_Autorizacion/formato';

/**
 * `PT-010` — Señalamiento de tramos inhábiles, <b>sin bloquear</b>.
 *
 * ── Lo que hace la última palabra del nombre ────────────────────────────────
 * `HU-006` es explícita: los tramos se <b>señalan</b>, no impiden la solicitud. El permiso de la
 * máxima autoridad se gestiona después, y `BD-04` lo exige al despachar. Bloquear acá
 * adelantaría un control de otro momento y dejaría al solicitante sin poder ni pedir lo que ya
 * sabe que necesita permiso.
 *
 * ── Y por qué se declara lo que no se pudo mirar ────────────────────────────
 * `BD-04` tiene dos mitades —día inhábil y hora inhábil— y cada una puede faltar por su cuenta.
 * Sin feriados cargados el calendario <b>subdeclara</b>: dirá que el 15 de septiembre es hábil.
 * Sin horario declarado, la hora no se evalúa. Un panel que muestre «ningún tramo inhábil» sin
 * decir cuál mitad no se miró <b>afirma algo que nadie comprobó</b>.
 */
export default function TramosInhabiles({ mision }: { readonly mision: string }): ReactElement {
  const { data, isPending, isError } = useQuery({
    queryKey: ['tramos', mision],
    queryFn: () => pedir<Tramos>(`/misiones/${mision}/tramos-inhabiles`),
  });

  if (isError || isPending) {
    return (
      <Panel titulo="Tramos en día u hora inhábil">
        <p className="tw:text-sm tw:text-tinta-mid">
          {isError ? 'No se pudieron evaluar los tramos.' : 'Evaluando…'}
        </p>
      </Panel>
    );
  }

  const incompleto = !data.conFeriadosCargados || !data.conHorarioDeclarado;

  return (
    <Panel titulo="Tramos en día u hora inhábil">
      <div className="tw:flex tw:flex-col tw:gap-3">
        {data.haySeñalamiento ? (
          <>
            {/* Señala; no impide. El tono es de aviso, nunca de bloqueo: `R-4` exige que se
                distingan, porque si se parecen la gente deja de leer los dos. */}
            <Nota tono="aviso" icono={<CalendarClock />}>
              <b>Esta misión circula en día u hora inhábil.</b> No impide solicitarla: lo que
              exige es el <b>permiso firmado por la máxima autoridad</b>, que genera el
              salvoconducto impreso. Se verifica al despachar.
            </Nota>

            {data.diasInhabiles.length > 0 && (
              <div className="tw:flex tw:flex-col tw:gap-1">
                <span className="tw:text-xs tw:text-tinta-mid">
                  Días inhábiles dentro de la ventana, holgura incluida — el vehículo sigue
                  afuera durante ella:
                </span>
                <div className="tw:flex tw:flex-wrap tw:gap-1.5">
                  {data.diasInhabiles.map((d) => (
                    <Pastilla key={d} tono="aviso">
                      {soloFecha(d)}
                    </Pastilla>
                  ))}
                </div>
              </div>
            )}

            {data.horasInhabiles.length > 0 && (
              <div className="tw:flex tw:flex-wrap tw:items-center tw:gap-1.5">
                <span className="tw:text-xs tw:text-tinta-mid">Fuera del horario:</span>
                {data.horasInhabiles.map((h) => (
                  <Pastilla key={h} tono="aviso">
                    {h}
                  </Pastilla>
                ))}
              </div>
            )}
          </>
        ) : (
          <p className="tw:text-sm">
            {incompleto
              ? 'No se señaló ningún tramo — pero la evaluación está incompleta, y eso no es lo mismo que no haber ninguno.'
              : 'Toda la ventana cae en día y hora hábiles.'}
          </p>
        )}

        {/* ⚠️ Cada mitad de `BD-04` se declara por separado, porque falla por su cuenta. */}
        {incompleto && (
          <Nota tono="info">
            <b>Falta parte de lo que hay que mirar:</b>
            <ul className="tw:mt-1 tw:flex tw:flex-col tw:gap-1 tw:pl-4">
              {!data.conFeriadosCargados && (
                <li className="tw:list-disc">
                  <b>Sin feriados cargados</b> (
                  <code className="tw:font-mono tw:text-xs">insumo #14</code>), así que el
                  calendario <b>subdeclara</b>: dará por hábil un 15 de septiembre. No se
                  inventan: el articulado sobre los feriados de octubre no se pudo verificar.
                </li>
              )}
              {!data.conHorarioDeclarado ? (
                <li className="tw:list-disc">
                  <b>Sin horario hábil declarado</b> (
                  <code className="tw:font-mono tw:text-xs">insumo #1</code>), así que la{' '}
                  <b>hora no se evalúa</b>. Es la mitad de <code>BD-04</code> que decide si una
                  salida a las cinco de la mañana exige salvoconducto.
                </li>
              ) : (
                !data.laMisionDeclaraHoras && (
                  <li className="tw:list-disc">
                    La misión <b>no declara sus horas</b>, así que la hora no se puede juzgar
                    aunque la institución sí tenga su horario.
                  </li>
                )
              )}
            </ul>
          </Nota>
        )}

        {/* `R-7`: toda cifra normativa se muestra con la tabla con que se calculó. */}
        <span className="tw:font-mono tw:text-xs tw:text-tinta-mid">
          calendario {data.versionDelCalendario} · ventana hasta {soloFecha(data.finDelRango)}
        </span>
      </div>
    </Panel>
  );
}

interface Tramos {
  /** Con qué calendario se juzgó. Va a la vista porque un asiento sin eso no se audita. */
  versionDelCalendario: string;
  /** Incluye la holgura: el vehículo sigue afuera durante ella. */
  finDelRango: string;
  diasInhabiles: string[];
  horasInhabiles: string[];
  haySeñalamiento: boolean;
  /** Falso significa que el calendario **subdeclara**. */
  conFeriadosCargados: boolean;
  /** Falso significa que la hora **no se evaluó**. */
  conHorarioDeclarado: boolean;
  laMisionDeclaraHoras: boolean;
}
