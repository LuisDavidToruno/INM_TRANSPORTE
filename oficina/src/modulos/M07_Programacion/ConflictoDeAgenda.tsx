import type { ReactElement } from 'react';
import { CalendarX } from 'lucide-react';

import { Enlace, Nota, Panel, Pastilla } from '../../ui';
import type { ConflictoDeReserva, VehiculoDeFlota } from '../../api/flota';
import { laDependencia, soloFecha } from '../M06_Autorizacion/formato';

/**
 * `BD-11` — el recurso ya está tomado en esa franja.
 *
 * ── Por qué no reusa `RechazoPorLicencia` ────────────────────────────────────
 * Porque no es el mismo rechazo. Un bloqueo de `BD-02` dice *«esta licencia no sirve para
 * este vehículo»* y se sale cambiando de par. Éste dice *«el recurso está comprometido con
 * otra dependencia»*, y las salidas pasan por **hablar con alguien**. Meterlos en la misma
 * pantalla obligaría a que una de las dos hablara con el vocabulario de la otra.
 *
 * ── Nombrar al titular no es cortesía ────────────────────────────────────────
 * `EF-01` lo exige: *«el sistema muestra el conflicto con su titular — qué misión tiene
 * tomado el recurso, de qué dependencia, en qué franja»*. Las cuatro salidas que la regla
 * ofrece empiezan todas por saber a quién llamar, y un «el vehículo está ocupado» a secas
 * convierte un bloqueo accionable en un callejón.
 *
 * ── Lo que esta pantalla NO finge ────────────────────────────────────────────
 * De las cuatro salidas de `EF-01`, **hoy sólo una se ejerce desde acá**: asignar otro
 * recurso. Consolidar necesita el expediente rector y reprogramar necesita acuerdo
 * registrado; ninguno existe. **Escalar la prioridad sí es ejecutable** desde que `T-11`
 * existe —desplazar pasa por devolver la otra misión a la cola—, pero se hace sobre la
 * misión ajena, en la cola de programación, y no desde esta pantalla.
 *
 * Ofrecer botones que no hacen nada sería peor que decirlo: quien programa perdería el
 * viaje descubriendo que no funcionan.
 */
export default function ConflictoDeAgenda({
  conflicto,
  vehiculos,
  habilitan,
  onElegirVehiculo,
}: {
  conflicto: ConflictoDeReserva;
  vehiculos: VehiculoDeFlota[];
  /** Identificadores de los vehículos que la licencia sí habilita. */
  habilitan: string[];
  onElegirVehiculo(id: string): void;
}): ReactElement {
  const recurso = conflicto.vehiculo && conflicto.conductor
    ? 'El vehículo y quien conduce están tomados'
    : conflicto.vehiculo
      ? 'El vehículo está tomado'
      : 'Quien conduce está tomado';

  // Los que habilitan, con su ficha. El servidor devuelve identificadores; el nombre está
  // acá, y cruzarlos en la pantalla evita un segundo viaje para pintar cuatro siglas.
  const alternativas = vehiculos.filter((v) => habilitan.includes(v.id));

  return (
    <Panel titulo="No se puede programar: el recurso ya está comprometido">
      <div className="tw:flex tw:flex-col tw:gap-4">
        <Nota tono="riesgo" icono={<CalendarX />}>
          <div className="tw:flex tw:flex-col tw:gap-1">
            <span>
              <b>{recurso}</b> por la misión{' '}
              <span className="tw:font-mono tw:tabular-nums">{conflicto.folio}</span>, de{' '}
              <b>{laDependencia(conflicto.dependencia)}</b>, del {soloFecha(conflicto.desde)} al{' '}
              {soloFecha(conflicto.hasta)}.
            </span>
            <span className="tw:text-xs">
              El sistema no sobre-asigna, ni siquiera con acuse. Dos misiones con el mismo
              vehículo el mismo día terminan con alguien esperando en el predio.
            </span>
          </div>
        </Nota>

        <section className="tw:flex tw:flex-col tw:gap-2">
          <h3 className="tw:text-sm tw:font-medium">Vehículos libres que la licencia habilita</h3>

          {alternativas.length === 0 ? (
            // Que no haya alternativa es un dato, no un vacío: es déficit de flota, y es
            // justo lo que la institución necesita poder llevar a una gestión presupuestaria.
            <p className="tw:text-sm tw:text-tinta-mid">
              Ninguno. No hay otro vehículo compatible libre en esta franja —
              <b> eso es déficit de flota</b>, y queda registrado como tal.
            </p>
          ) : (
            <ul className="tw:flex tw:flex-col tw:gap-1.5">
              {alternativas.map((v) => (
                <li key={v.id}>
                  <button
                    type="button"
                    onClick={() => onElegirVehiculo(v.id)}
                    className="tw:flex tw:w-full tw:flex-col tw:items-start tw:gap-0.5 tw:rounded tw:border tw:border-linea tw:px-3 tw:py-2 tw:text-left tw:text-sm tw:transition-colors tw:hover:border-acento"
                  >
                    <span className="tw:font-medium">{v.siglas}</span>
                    <span className="tw:text-xs tw:text-tinta-mid">
                      {v.ficha.tipoDeVehiculo} ·{' '}
                      {v.placa ?? 'sin placa metálica'}
                    </span>
                  </button>
                </li>
              ))}
            </ul>
          )}
        </section>

        <section className="tw:flex tw:flex-col tw:gap-1.5">
          <h3 className="tw:text-sm tw:font-medium">Las otras salidas todavía no existen</h3>
          <p className="tw:text-sm tw:text-tinta-mid">
            <code className="tw:font-mono tw:text-xs">EF-01</code> prevé tres caminos más. Dos no
            están construidos y se resuelven fuera del sistema, hablando con{' '}
            {laDependencia(conflicto.dependencia)}:
          </p>
          <ul className="tw:flex tw:flex-wrap tw:gap-1.5">
            <Pendiente>Consolidar las dos misiones</Pendiente>
            <Pendiente>Reprogramar una de las dos</Pendiente>
          </ul>
          {/* Escalar SÍ se puede desde que existe `T-11`: desplazar una programación pasa
              por devolverla explícitamente a la cola. Se hace sobre la misión ajena, y por
              eso el enlace va a la cola de programación y no a un botón de acá. */}
          <p className="tw:text-sm tw:text-tinta-mid">
            <b>Escalar la prioridad sí se puede</b>: solo Gerencia Administrativa puede desplazar
            una programación existente, y se hace devolviendo la misión ajena a la cola desde{' '}
            <Enlace href="/programacion">la cola de programación</Enlace>. Nunca se le quita el
            vehículo a una misión sin devolverla explícitamente.
          </p>
        </section>
      </div>
    </Panel>
  );
}

function Pendiente({ children }: { children: string }): ReactElement {
  return (
    <li>
      <Pastilla tono="neutro">{children}</Pastilla>
    </li>
  );
}
