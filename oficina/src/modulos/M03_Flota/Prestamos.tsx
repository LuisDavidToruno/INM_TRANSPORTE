import type { ReactElement } from 'react';
import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { CalendarClock, CircleAlert, HandCoins, Search } from 'lucide-react';

import { Campo, CampoFecha, Nota, Panel, Pastilla } from '../../ui';
import { prestamos, quienRespondiaPor } from '../../api/prestamos';
import type { ExpedienteDePrestamo } from '../../api/prestamos';
import { soloFecha } from '../M06_Autorizacion/formato';

/**
 * `RN-63` — el préstamo de vehículo como expediente del bien.
 *
 * ── Lo que esta pantalla contesta ───────────────────────────────────────────
 * *«En cualquier fecha del período, el sistema responde **quién respondía por la unidad**. Esa
 * consulta es el entregable de la regla»*. Cuando llega una multa de agosto, la pregunta no es
 * quién tiene el vehículo hoy: es quién lo tenía ese día.
 */
export default function PrestamosPantalla(): ReactElement {
  const expedientes = useQuery({ queryKey: ['prestamos'], queryFn: prestamos });

  if (expedientes.isError) {
    return (
      <Nota tono="riesgo" icono={<CircleAlert />}>
        No se pudieron cargar los expedientes de préstamo.
      </Nota>
    );
  }

  const vencidos = expedientes.data?.filter((p) => p.estaVencido) ?? [];

  return (
    <div className="tw:flex tw:flex-col tw:gap-5">
      <header className="tw:flex tw:flex-col tw:gap-1">
        <h1 className="tw:text-xl tw:font-semibold tw:tracking-tight">
          Préstamos de vehículo
        </h1>
        <p className="tw:text-sm tw:text-tinta-mid">
          La cesión de la <b>tenencia</b> a otra dependencia o institución es un expediente del
          bien, con acto autorizante, receptor nombrado y actas. Cedido{' '}
          <b>con motorista propio</b> no es un préstamo: es una misión de apoyo institucional,
          porque la tenencia no se cedió.
        </p>
      </header>

      {vencidos.length > 0 && (
        <Panel titulo={`${vencidos.length} préstamo(s) vencido(s)`}>
          <Nota tono="riesgo" icono={<CalendarClock />}>
            Vehículos del Estado en tenencia ajena fuera de la fecha comprometida.{' '}
            <b>No se cierra el período con préstamos vencidos</b> — y ese bloqueo ya dispara sobre
            el saldo de apertura.
          </Nota>
        </Panel>
      )}

      <PanelDeConsulta />

      {expedientes.isPending ? (
        <p className="tw:text-sm tw:text-tinta-mid">Cargando expedientes…</p>
      ) : (
        <PanelDeExpedientes expedientes={expedientes.data} />
      )}
    </div>
  );
}

/**
 * **El entregable de la regla** — `RN-63` punto 7.
 *
 * Se resuelve por la fecha, no por el estado de hoy: un vehículo que hoy está disponible pudo
 * estar prestado el día que se cometió la infracción.
 */
function PanelDeConsulta(): ReactElement {
  const [vehiculo, setVehiculo] = useState('');
  const [fecha, setFecha] = useState(() => new Date().toISOString().slice(0, 10));

  const consulta = useQuery({
    queryKey: ['quien-respondia', vehiculo, fecha],
    queryFn: () => quienRespondiaPor(vehiculo, fecha),

    // 26 caracteres es un ULID completo. Consultar con uno a medias devolvería un 400 por cada
    // tecla mientras alguien escribe.
    enabled: vehiculo.length === 26,
  });

  return (
    <Panel titulo="Quién respondía por la unidad">
      <div className="tw:flex tw:flex-col tw:gap-3">
        <div className="tw:grid tw:gap-4 tw:sm:grid-cols-2">
          <Campo
            etiqueta="Vehículo"
            ayuda="El identificador de la unidad. Contra él se resuelve quién la tenía en la fecha."
          >
            {(control) => (
              <input
                {...control}
                value={vehiculo}
                onChange={(e) => { setVehiculo(e.target.value.trim().toUpperCase()); }}
                placeholder="ULID del vehículo"
                className="loki-foco tw:w-full tw:rounded-control tw:border tw:border-linea tw:bg-panel tw:px-2 tw:py-1.5 tw:text-cuerpo-2 tw:font-mono tw:text-tinta-hi"
              />
            )}
          </Campo>

          <Campo
            etiqueta="Fecha"
            ayuda="El día del hecho, no hoy: cuando llega una multa de agosto la pregunta es quién tenía la unidad ese día."
          >
            <CampoFecha valor={fecha} onCambiar={setFecha} etiqueta="Fecha" />
          </Campo>
        </div>

        {vehiculo.length !== 26 ? (
          <p className="tw:flex tw:items-center tw:gap-1.5 tw:text-xs tw:text-tinta-mid">
            <Search className="tw:size-3.5" aria-hidden />
            Ingrese el identificador de la unidad para resolver la consulta.
          </p>
        ) : consulta.isPending ? (
          <p className="tw:text-sm tw:text-tinta-mid">Resolviendo…</p>
        ) : consulta.isError ? (
          <Nota tono="aviso">No se pudo resolver la consulta para esa unidad.</Nota>
        ) : consulta.data.esTenenciaAjena ? (
          <div className="tw:flex tw:flex-col tw:gap-1 tw:border-l-2 tw:border-aviso-fg tw:pl-3">
            <p className="tw:text-sm">
              El {soloFecha(consulta.data.fecha)} respondía{' '}
              <b>{consulta.data.persona}</b>, {consulta.data.cargo} de{' '}
              <b>{consulta.data.institucion}</b>.
            </p>
            <span className="tw:text-xs tw:text-tinta-mid">
              La unidad estaba en <b>tenencia ajena</b>. Las infracciones y daños de la ventana se
              imputan al tenedor a la fecha del hecho — sin extinguir la responsabilidad de la
              institución propietaria.
            </span>
          </div>
        ) : (
          <div className="tw:flex tw:flex-col tw:gap-1 tw:border-l-2 tw:border-borde tw:pl-3">
            <p className="tw:text-sm">
              El {soloFecha(consulta.data.fecha)} la unidad <b>no estaba prestada</b>.
            </p>
            <span className="tw:text-xs tw:text-tinta-mid">
              Respondía la institución propietaria por su custodio ordinario.
            </span>
          </div>
        )}
      </div>
    </Panel>
  );
}

function PanelDeExpedientes({
  expedientes,
}: {
  expedientes: ExpedienteDePrestamo[];
}): ReactElement {
  if (expedientes.length === 0) {
    return (
      <Panel>
        <p className="tw:text-sm tw:text-tinta-mid">No hay préstamos registrados.</p>
      </Panel>
    );
  }

  // Los vencidos primero: son los que impiden cerrar el período.
  const orden = [...expedientes].sort((a, b) => b.diasDeMora - a.diasDeMora);

  return (
    <Panel titulo={`${expedientes.length} expediente(s)`}>
      <div className="tw:flex tw:flex-col tw:gap-3">
        {orden.map((p) => (
          <div
            key={p.id}
            className={`tw:flex tw:flex-col tw:gap-1 tw:border-l-2 tw:pl-3 ${
              p.estaVencido
                ? 'tw:border-riesgo-fg'
                : p.estaVigente
                  ? 'tw:border-borde'
                  : 'tw:border-ok-fg'
            }`}
          >
            <div className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-3 tw:text-sm">
              <HandCoins className="tw:size-3.5 tw:text-tinta-mid" aria-hidden />
              <span className="tw:font-medium">{p.receptor.persona}</span>
              <span className="tw:text-tinta-mid">
                {p.receptor.cargo} · {p.receptor.institucion}
              </span>
              {p.estaVencido && (
                <Pastilla tono="riesgo">{p.diasDeMora} días de mora</Pastilla>
              )}
              {!p.estaVigente && <Pastilla tono="ok">Devuelto</Pastilla>}
            </div>

            <span className="tw:text-xs tw:text-tinta-mid">
              {p.motivo} · acto <span className="tw:font-mono">{p.acto.folio}</span> firmado por{' '}
              {p.acto.firmante} · autorizó {p.autoriza}
            </span>

            <span className="tw:text-xs tw:text-tinta-mid">
              del {soloFecha(p.desde)} al {soloFecha(p.devolucionComprometida)} comprometido
              {p.devolucion !== null && ` · devuelto el ${soloFecha(p.devolucion.fecha)}`}
              {/* `RN-63` punto 3 — no entran en la conciliación galonaje-kilometraje. */}
              {p.kilometrosBajoTenenciaAjena !== null &&
                ` · ${p.kilometrosBajoTenenciaAjena.toLocaleString('es-HN')} km bajo tenencia ajena`}
            </span>

            {/* Hallazgo frecuente de auditoría, y por eso se reconstata al devolver. */}
            {p.volvioSinRotulacion && (
              <Nota tono="riesgo" icono={<CircleAlert />}>
                <b>Volvió sin la identificación del Estado.</b> Salió con ella constatada y la
                reconstatación falló: es hallazgo de auditoría.
              </Nota>
            )}

            {/* Un rubro sin pactar es el que aparece cuando llega la multa. */}
            {p.rubrosSinPactar.length > 0 && (
              <span className="tw:text-xs tw:text-aviso-fg">
                sin pactar: {p.rubrosSinPactar.join(', ')}
              </span>
            )}
          </div>
        ))}
      </div>
    </Panel>
  );
}
