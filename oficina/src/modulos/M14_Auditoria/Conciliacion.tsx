import type { ReactElement } from 'react';
import { useQuery } from '@tanstack/react-query';
import { CircleAlert, FileSearch, Link2Off } from 'lucide-react';

import { Nota, Panel, Pastilla, Vacio } from '../../ui';
import { lempiras } from '../../api/combustible';
import {
  TEXTO_DE_FUENTE,
  TEXTO_DE_LADO,
  diferenciasDeConciliacion,
  ejecucionesDeConciliacion,
  fuentesExternas,
} from '../../api/conciliacion';
import type {
  DiferenciaDeConciliacion,
  EjecucionDeConciliacion,
  FuenteExterna,
} from '../../api/conciliacion';
import { momentoCompleto, soloFecha } from '../M06_Autorizacion/formato';

/**
 * `RN-95` — la conciliación contra fuentes externas.
 *
 * ── Lo que esta pantalla existe para que se vea ─────────────────────────────
 * **El retraso de cada fuente.** `RN-95` punto 5: *«una fuente sin conciliar durante meses es
 * en sí misma una observación de control interno»*. Si eso no se muestra, la conciliación se
 * deja de hacer y nadie se entera hasta que llega el auditor.
 *
 * **Y que «no disponible» no se lea como «conciliada».** Una institución sin tag de peaje no
 * tiene estado de cuenta que conciliar; confundir las dos cosas hace que la ausencia de
 * diferencias se lea como conformidad.
 */
export default function Conciliacion(): ReactElement {
  const fuentes = useQuery({ queryKey: ['fuentes-externas'], queryFn: fuentesExternas });
  const diferencias = useQuery({
    queryKey: ['diferencias-de-conciliacion'],
    queryFn: diferenciasDeConciliacion,
  });
  const ejecuciones = useQuery({
    queryKey: ['ejecuciones-de-conciliacion'],
    queryFn: ejecucionesDeConciliacion,
  });

  if (fuentes.isError || diferencias.isError || ejecuciones.isError) {
    return (
      <Nota tono="riesgo" icono={<CircleAlert />}>
        No se pudo cargar la conciliación contra fuentes externas.
      </Nota>
    );
  }

  const lista = fuentes.data ?? [];
  const abiertas = diferencias.data ?? [];
  const corridas = ejecuciones.data ?? [];
  const atrasadas = lista.filter((f) => f.atrasada);

  return (
    <div className="tw:flex tw:flex-col tw:gap-5">
      <header className="tw:flex tw:flex-col tw:gap-1">
        <h1 className="tw:text-xl tw:font-semibold tw:tracking-tight">
          Conciliación contra fuentes externas
        </h1>
        <p className="tw:text-sm tw:text-tinta-mid">
          Lo que el proveedor dice contra lo que nosotros registramos. Comparar nuestros datos
          con nuestros datos verifica coherencia interna, no veracidad.
        </p>
      </header>

      {fuentes.isPending ? (
        <p className="tw:text-sm tw:text-tinta-mid">Cargando las fuentes…</p>
      ) : lista.length === 0 ? (
        <Vacio
          icono={<FileSearch />}
          titulo="No hay fuentes externas registradas"
          descripcion="Sin fuentes, la única conciliación que hace el sistema es contra sí mismo — y un registro completo y coherente puede ser completamente falso."
        />
      ) : (
        <>
          {atrasadas.length > 0 && (
            <Nota tono="aviso" icono={<CircleAlert />}>
              {atrasadas.length === 1 ? '1 fuente lleva' : `${atrasadas.length} fuentes llevan`}{' '}
              más de su periodicidad sin conciliar. <b>Una fuente sin conciliar durante meses es
              en sí misma una observación de control interno</b> — y las diferencias que se
              acumulan mientras tanto las encuentra el auditor, no nosotros.
            </Nota>
          )}

          <div className="tw:flex tw:flex-col tw:gap-3">
            {lista.map((f) => (
              <TarjetaDeFuente key={f.id} fuente={f} />
            ))}
          </div>
        </>
      )}

      {abiertas.length > 0 && <PanelDeDiferencias diferencias={abiertas} />}

      {corridas.length > 0 && <PanelDeEjecuciones ejecuciones={corridas} />}
    </div>
  );
}

function TarjetaDeFuente({ fuente: f }: { fuente: FuenteExterna }): ReactElement {
  // Los tres estados se dicen distinto a propósito. «No disponible» no es un pendiente: es una
  // fuente que la institución no tiene, y ponerla en la lista de atrasadas para siempre haría
  // que la lista dejara de mirarse.
  const estado = !f.disponible
    ? { tono: 'neutro' as const, texto: 'No disponible' }
    : f.atrasada
      ? { tono: 'aviso' as const, texto: 'Atrasada' }
      : { tono: 'ok' as const, texto: 'Al día' };

  return (
    <Panel>
      <div className="tw:flex tw:flex-col tw:gap-2">
        <div className="tw:flex tw:flex-wrap tw:items-start tw:justify-between tw:gap-3">
          <div className="tw:flex tw:flex-col tw:gap-1">
            <span className="tw:font-medium">
              {TEXTO_DE_FUENTE[f.tipo] ?? f.tipo}
            </span>
            <span className="tw:text-xs tw:text-tinta-mid">
              {f.emisor} · {f.formato} · carga {f.responsable}
            </span>
          </div>

          <Pastilla tono={estado.tono}>{estado.texto}</Pastilla>
        </div>

        {/* El texto viene del servidor con su razón: nunca conciliada, atrasada sobre su
            periodicidad, o sin periodicidad declarada. Los tres significan cosas distintas. */}
        <p
          className={`tw:text-xs ${
            f.atrasada ? 'tw:text-aviso-fg' : 'tw:text-tinta-mid'
          }`}
        >
          {f.retraso}
        </p>
      </div>
    </Panel>
  );
}

/**
 * Los expedientes abiertos, ordenados por plazo. Cada diferencia lleva **su lado**: no es lo
 * mismo que el proveedor cobre algo que no registramos que que nosotros registremos algo que él
 * no reporta — y la conciliación no presume qué significa ninguno de los dos.
 */
function PanelDeDiferencias({
  diferencias,
}: {
  diferencias: DiferenciaDeConciliacion[];
}): ReactElement {
  const sinVehiculo = diferencias.filter((d) => d.vehiculo === null);

  return (
    <Panel titulo="Diferencias abiertas">
      <div className="tw:flex tw:flex-col tw:gap-3">
        <Nota tono="riesgo" icono={<CircleAlert />}>
          {diferencias.length === 1 ? '1 diferencia' : `${diferencias.length} diferencias`} sin
          resolver, por {lempiras(diferencias.reduce((s, d) => s + d.monto, 0))}.
          {sinVehiculo.length > 0 && (
            <>
              {' '}
              <b>{sinVehiculo.length} no se pudieron atribuir a ningún vehículo</b>: ahí no hay a
              quién preguntarle, hay que ir al emisor.
            </>
          )}
        </Nota>

        {diferencias.map((d) => (
          <FilaDeDiferencia key={d.id} diferencia={d} />
        ))}
      </div>
    </Panel>
  );
}

function FilaDeDiferencia({
  diferencia: d,
}: {
  diferencia: DiferenciaDeConciliacion;
}): ReactElement {
  return (
    <div className="tw:flex tw:flex-col tw:gap-1 tw:border-l-2 tw:border-riesgo-fg tw:pl-3">
      <div className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-3 tw:text-sm">
        <span className="tw:font-medium tw:tabular-nums">{lempiras(d.monto)}</span>
        <span className="tw:text-xs tw:text-tinta-mid">
          {TEXTO_DE_LADO[d.lado] ?? d.lado}
        </span>
        {d.referencia !== null && (
          <span className="tw:font-mono tw:text-xs tw:text-tinta-mid">{d.referencia}</span>
        )}
      </div>

      <p className="tw:text-xs tw:text-tinta-mid">{d.explicacion}</p>

      <div className="tw:flex tw:flex-wrap tw:items-center tw:gap-x-3 tw:text-xs tw:text-tinta-mid">
        <span>hecho del {soloFecha(d.fechaDelHecho)}</span>

        {d.vehiculo === null ? (
          <span className="tw:flex tw:items-center tw:gap-1 tw:text-aviso-fg">
            <Link2Off className="tw:size-3" aria-hidden />
            sin vehículo resuelto
          </span>
        ) : (
          <span className="tw:font-mono">
            {d.vehiculo}
            {/* Resolver por placa admite discusión; por número de bien, no. El expediente
                tiene que decir cuál fue. */}
            {d.ancla !== null && ` · por ${d.ancla}`}
          </span>
        )}

        {d.responsable !== null && <span>sigue {d.responsable}</span>}
        {d.plazo !== null && <span>plazo {soloFecha(d.plazo)}</span>}
      </div>
    </div>
  );
}

function PanelDeEjecuciones({
  ejecuciones,
}: {
  ejecuciones: EjecucionDeConciliacion[];
}): ReactElement {
  return (
    <Panel titulo="Conciliaciones ejecutadas">
      <div className="tw:flex tw:flex-col tw:gap-2">
        {ejecuciones.map((e) => (
          <div
            key={e.id}
            className="tw:flex tw:flex-col tw:gap-1 tw:border-l-2 tw:border-borde tw:pl-3"
          >
            <div className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-3 tw:text-sm">
              <span className="tw:font-medium">
                {soloFecha(e.desde)} — {soloFecha(e.hasta)}
              </span>
              <span className="tw:text-xs tw:text-tinta-mid">
                {e.coincidentes} coincidentes · {e.soloEnLaFuente} solo del emisor ·{' '}
                {e.soloEnSigti} solo nuestras
              </span>
              {e.sinResolver > 0 && (
                <span className="tw:text-xs tw:text-riesgo-fg">
                  {e.sinResolver} sin resolver
                </span>
              )}
            </div>

            {/* El documento y el corte van juntos y siempre. Sin ellos, dos ejecuciones con
                datos distintos se ven idénticas y una diferencia no se puede recomprobar. */}
            <span className="tw:text-xs tw:text-tinta-mid">
              {e.documentoFuente} · corte al {momentoCompleto(e.fechaDeCorte)} · ejecutó{' '}
              {e.ejecuta}
            </span>
          </div>
        ))}
      </div>
    </Panel>
  );
}
