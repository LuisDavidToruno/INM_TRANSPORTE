import type { ReactElement } from 'react';
import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Link, Navigate } from 'react-router';
import { CircleAlert, EyeOff, Search } from 'lucide-react';

import { CampoBusqueda, Nota, Panel, Pastilla, Vacio } from '../../ui';
import { pedir } from '../../api/misiones';
import { usarPuesto } from '../../app/puesto';
import { soloFecha } from '../M06_Autorizacion/formato';

/**
 * `PT-005` — Buscador de expedientes <b>con alcance de datos aplicado</b>.
 *
 * ── Esa última frase del nombre era lo que faltaba ──────────────────────────
 * El alcance de datos estaba modelado, otorgado y consultable desde `M-01`, y <b>no filtraba ni
 * una sola consulta</b>: toda pantalla de lista mostraba todos los expedientes a cualquiera.
 * Esta es la primera que lo aplica.
 *
 * ── Se dice cuántos quedaron fuera, y no cuáles ─────────────────────────────
 * Un buscador que oculta sin decir que oculta hace creer que el expediente no existe, y eso
 * manda a la gente a crear uno duplicado — que después hay que conciliar. Saber que hay más es
 * información de control interno; ver cuáles sería el permiso que no se tiene.
 */
export default function Buscador(): ReactElement {
  const { elegido } = usarPuesto();
  const [texto, setTexto] = useState('');

  const { data, isPending, isError } = useQuery({
    queryKey: ['puesto', elegido?.puesto, 'expedientes', elegido?.persona, texto],
    queryFn: () =>
      pedir<Resultado>(
        `/puesto/${elegido!.puesto}/expedientes` +
          `?persona=${encodeURIComponent(elegido!.persona)}&q=${encodeURIComponent(texto)}`,
      ),
    enabled: elegido !== null,
  });

  if (elegido === null) return <Navigate to="/ingreso" replace />;

  if (isError) {
    return (
      <Nota tono="riesgo" icono={<CircleAlert />}>
        No se pudo consultar los expedientes.
      </Nota>
    );
  }

  return (
    <div className="tw:flex tw:flex-col tw:gap-5">
      <header className="tw:flex tw:flex-col tw:gap-1">
        <h1 className="tw:text-xl tw:font-semibold tw:tracking-tight">Buscar expedientes</h1>
        <p className="tw:text-sm tw:text-tinta-mid">
          Dentro de lo que alcanza <b>{data?.denominacion ?? elegido.puesto}</b>. Otro puesto de
          la misma persona ve otro conjunto — el alcance se otorga al puesto, no a quien lo ocupa.
        </p>
      </header>

      <CampoBusqueda
        etiqueta="Folio, destino, objeto del traslado, dependencia o solicitante…"
        valor={texto}
        onCambio={setTexto}
      />

      {isPending ? (
        <p className="tw:text-sm tw:text-tinta-mid">Buscando…</p>
      ) : (
        <>
          {/* `R-7` llevado al alcance: la pantalla dice con qué regla filtró. Una lista que
              no dice por qué es corta no se puede auditar ni reclamar. */}
          {!data.alcanceResuelto ? (
            <Nota tono="riesgo" icono={<CircleAlert />}>
              <b>No se pudo resolver el alcance de este puesto, así que no se muestra nada.</b>{' '}
              {data.porQueNo}
            </Nota>
          ) : (
            <Nota tono="info">
              Alcance <b>{data.nivel}</b>.{' '}
              {data.fueraDelAlcance === 0
                ? 'No hay ningún expediente fuera de lo que este puesto alcanza.'
                : `Hay ${data.fueraDelAlcance} expediente(s) fuera de este alcance. Se dice cuántos, no cuáles: saber que existen es control interno, verlos sería el permiso que este puesto no tiene.`}
            </Nota>
          )}

          {data.resultados.length === 0 ? (
            <Vacio
              icono={texto === '' ? <EyeOff /> : <Search />}
              titulo={
                texto === ''
                  ? 'Este puesto no alcanza ningún expediente'
                  : 'Nada coincide dentro de su alcance'
              }
              descripcion={
                texto === ''
                  ? 'No es que no haya expedientes: es que ninguno cae dentro de lo que este puesto puede ver.'
                  : 'Puede que el expediente exista y esté fuera de su alcance. El número de arriba lo dice.'
              }
            />
          ) : (
            <Panel
              titulo={
                data.total > data.resultados.length
                  ? `${data.resultados.length} de ${data.total} · se muestran los más recientes`
                  : `${data.total} expediente(s)`
              }
            >
              <ul className="tw:flex tw:flex-col tw:gap-2">
                {data.resultados.map((r) => (
                  <li
                    key={r.mision}
                    className="tw:flex tw:flex-col tw:gap-0.5 tw:border-l-2 tw:border-linea tw:py-1 tw:pl-3"
                  >
                    <div className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-3">
                      <Link
                        to={`/rastro?expediente=${r.mision}`}
                        className="loki-foco tw:font-medium tw:text-sm tw:underline-offset-2 tw:hover:underline"
                      >
                        {r.folio}
                      </Link>
                      <Pastilla tono="neutro">{r.estado}</Pastilla>
                      <span className="tw:text-sm tw:text-tinta-mid">{r.destino}</span>
                    </div>
                    <span className="tw:text-xs tw:text-tinta-mid">
                      {r.dependencia} · {r.objetoDelTraslado} · solicita{' '}
                      {r.solicitanteDeDerecho} · sale {soloFecha(r.salida)}
                    </span>
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

interface Resultado {
  puesto: string;
  denominacion: string | null;
  /** Con qué nivel se filtró: `Propio`, `Dependencia`, `Delegacion` o `Institucion`. */
  nivel: string;
  /** **Falso es «no se sabe qué ve»**, no «no ve nada». */
  alcanceResuelto: boolean;
  porQueNo: string | null;
  /** Cuántos quedaron fuera. El número sí, los datos no. */
  fueraDelAlcance: number;
  total: number;
  resultados: {
    mision: string;
    folio: string;
    estado: string;
    dependencia: string;
    destino: string;
    objetoDelTraslado: string;
    solicitanteDeDerecho: string;
    salida: string;
  }[];
}
