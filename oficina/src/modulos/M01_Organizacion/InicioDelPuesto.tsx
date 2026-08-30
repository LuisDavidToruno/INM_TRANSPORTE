import type { ReactElement } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Link, Navigate } from 'react-router';
import { CircleAlert, Inbox } from 'lucide-react';

import { Nota, Panel, Vacio } from '../../ui';
import { pedir } from '../../api/misiones';
import { usarPuesto } from '../../app/puesto';
import { rutaDe } from '../../pantallas/registro';

/**
 * `PT-002` — Inicio del puesto: pendientes, alertas y accesos.
 *
 * ── `R-2`, que es lo que decide qué va acá ──────────────────────────────────
 * <i>«La raíz de cada rol es su bandeja de trabajo, no un tablero decorativo. Nadie entra a
 * SIGTI a ver indicadores: entra a resolver lo que tiene pendiente.»</i>
 *
 * Por eso cada número de esta pantalla es <b>algo que espera una decisión de este puesto</b>, y
 * cada uno lleva a la pantalla que lo resuelve. Un contador sin destino —«12 misiones este
 * mes»— sería exactamente el tablero decorativo que la regla rechaza: se mira, no se usa, y
 * ocupa el lugar de lo que sí había que hacer.
 */
export default function InicioDelPuesto(): ReactElement {
  const { elegido } = usarPuesto();

  const { data, isPending, isError } = useQuery({
    queryKey: ['puesto', elegido?.puesto, 'inicio', elegido?.persona],
    queryFn: () =>
      pedir<Inicio>(
        `/puesto/${elegido!.puesto}/inicio?persona=${encodeURIComponent(elegido!.persona)}`,
      ),
    enabled: elegido !== null,
  });

  if (elegido === null) return <Navigate to="/ingreso" replace />;

  if (isError) {
    return (
      <Nota tono="riesgo" icono={<CircleAlert />}>
        No se pudo cargar el inicio de este puesto.
      </Nota>
    );
  }

  if (isPending) return <p className="tw:text-sm tw:text-tinta-mid">Cargando sus pendientes…</p>;

  const total = data.pendientes.reduce((s, p) => s + p.cuantos, 0);

  return (
    <div className="tw:flex tw:flex-col tw:gap-5">
      <header className="tw:flex tw:flex-col tw:gap-1">
        <h1 className="tw:text-xl tw:font-semibold tw:tracking-tight">
          {data.denominacion ?? data.puesto}
        </h1>
        <p className="tw:text-sm tw:text-tinta-mid">
          {data.unidad}
          {data.delegacion !== null && ` · delegación ${data.delegacion}`} — lo que le toca
          resolver ahora.
        </p>
      </header>

      {/* ⚠️ Cero pendientes y alcance sin resolver se ven iguales, y son opuestos: uno
          significa que no hay trabajo y el otro que no se sabe cuál es. */}
      {!data.alcanceResuelto && (
        <Nota tono="riesgo" icono={<CircleAlert />}>
          <b>No se pudo resolver qué expedientes ve este puesto</b>, así que los contadores están
          en cero por eso y no porque no haya trabajo. {data.porQueNo}
        </Nota>
      )}

      {data.pendientes.length === 0 || total === 0 ? (
        <Vacio
          icono={<Inbox />}
          titulo={
            data.alcanceResuelto ? 'Nada pendiente' : 'No se puede decir si hay algo pendiente'
          }
          descripcion={
            data.alcanceResuelto
              ? 'Ninguna decisión de este puesto está esperando. Los pendientes aparecen solos cuando llegan.'
              : 'El alcance de datos de este puesto no se pudo resolver, y sin él no se puede saber qué expedientes le corresponden.'
          }
        />
      ) : (
        <div className="tw:grid tw:gap-3 tw:sm:grid-cols-2 tw:lg:grid-cols-3">
          {data.pendientes.map((p) => (
            <Pendiente key={`${p.pantalla}-${p.que}`} pendiente={p} />
          ))}
        </div>
      )}

      {/* Los accesos: a qué entra este puesto. Salen del mapa de navegación, no del menú. */}
      <Panel titulo="A qué entra este puesto">
        {data.raices.length === 0 ? (
          <p className="tw:text-sm tw:text-tinta-mid">
            El mapa de navegación <b>no declara una raíz</b> para los roles de este puesto. No es
            que no tenga trabajo: es que el diseño todavía no dijo cuál es su punto de entrada.
          </p>
        ) : (
          <ul className="tw:flex tw:flex-col tw:gap-2">
            {data.raices.map((r) => (
              <li key={r.nombre} className="tw:flex tw:flex-col tw:gap-0.5">
                <Destino pantalla={r.pantalla} nombre={r.nombre} />
                <span className="tw:text-xs tw:text-tinta-mid">{r.porQue}</span>
              </li>
            ))}
          </ul>
        )}
      </Panel>
    </div>
  );
}

function Pendiente({ pendiente }: { pendiente: PendienteDelPuesto }): ReactElement {
  const ruta = pendiente.pantalla === null ? null : rutaDe(pendiente.pantalla);

  const cuerpo = (
    <>
      <span className="tw:text-2xl tw:font-semibold tw:tabular-nums">{pendiente.cuantos}</span>
      <span className="tw:text-sm tw:text-tinta-mid">{pendiente.que}</span>
    </>
  );

  const clases =
    'tw:flex tw:flex-col tw:gap-1 tw:rounded-panel tw:border tw:border-linea tw:bg-panel tw:p-3';

  // Sin ruta, la tarjeta no es un enlace muerto: dice a dónde iría. Un enlace que no lleva a
  // ninguna parte se prueba una vez y enseña a no confiar en los demás.
  return ruta === null ? (
    <div className={clases}>
      {cuerpo}
      <span className="tw:text-xs tw:text-tinta-mid">
        {pendiente.pantalla === null
          ? 'El mapa no le asigna pantalla todavía'
          : `${pendiente.pantalla} · sin construir`}
      </span>
    </div>
  ) : (
    <Link to={ruta} className={`loki-foco ${clases} tw:hover:border-tinta-mid`}>
      {cuerpo}
      <span className="tw:text-xs tw:text-tinta-mid">{pendiente.pantalla}</span>
    </Link>
  );
}

function Destino({
  pantalla,
  nombre,
}: {
  pantalla: string | null;
  nombre: string;
}): ReactElement {
  const ruta = pantalla === null ? null : rutaDe(pantalla);

  if (ruta === null)
    return (
      <span className="tw:text-sm">
        {nombre}{' '}
        <span className="tw:text-xs tw:text-tinta-mid">
          {pantalla === null ? '· sin identificador en el mapa' : `· ${pantalla}, sin construir`}
        </span>
      </span>
    );

  return (
    <Link to={ruta} className="loki-foco tw:text-sm tw:underline-offset-2 tw:hover:underline">
      {nombre} <span className="tw:text-xs tw:text-tinta-mid">· {pantalla}</span>
    </Link>
  );
}

interface PendienteDelPuesto {
  /** Nulo cuando el mapa describe la raíz sin darle identificador. */
  pantalla: string | null;
  que: string;
  cuantos: number;
}

interface Inicio {
  puesto: string;
  denominacion: string | null;
  unidad: string | null;
  delegacion: string | null;
  raices: { pantalla: string | null; nombre: string; porQue: string }[];
  pendientes: PendienteDelPuesto[];
  /** **Falso no es «cero pendientes»**: es «no se sabe cuáles». */
  alcanceResuelto: boolean;
  porQueNo: string | null;
}
