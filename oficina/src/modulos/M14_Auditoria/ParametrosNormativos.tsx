import type { ReactElement } from 'react';
import { useMemo, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { CircleAlert, ScrollText } from 'lucide-react';

import { CampoBusqueda, Nota, Panel, Pastilla, Vacio } from '../../ui';
import { pedir } from '../../api/misiones';
import { diaYHora, soloFecha } from '../M06_Autorizacion/formato';

/**
 * `PT-092` — El histórico de parámetros normativos <b>con vigencia</b>.
 *
 * ── Las dos parejas de fechas de `ADR-006`, completas ───────────────────────
 * El eje **normativo** dice desde cuándo regía; el de **transacción**, desde cuándo lo supimos.
 * Mostrar sólo el primero impediría explicar por qué una liquidación de marzo usó otro número:
 * la tarifa que rige hoy para marzo puede no ser la que se conocía en marzo, y las dos
 * respuestas son legítimas.
 *
 * ── Y por qué «sin aprobar» se muestra ──────────────────────────────────────
 * Una versión sin aprobar **no resuelve**: el doble control de `HU-145` sería decorativo si el
 * valor rigiera igual. Ocultarlas haría que quien audite no viera lo que está esperando firma.
 */
export default function ParametrosNormativos(): ReactElement {
  const [filtro, setFiltro] = useState('');

  const { data, isPending, isError } = useQuery({
    queryKey: ['parametros-normativos'],
    queryFn: () => pedir<Version[]>('/auditoria/parametros'),
  });

  const filas = useMemo(() => {
    const todas = data ?? [];
    const busqueda = filtro.trim().toLowerCase();
    if (!busqueda) return todas;

    return todas.filter((v) =>
      [v.clave, v.valor, v.cargadoPor, v.aprobadoPor ?? 'sin aprobar', v.respaldo.fuente]
        .join(' ')
        .toLowerCase()
        .includes(busqueda),
    );
  }, [data, filtro]);

  if (isError) {
    return (
      <Nota tono="riesgo" icono={<CircleAlert />}>
        No se pudo cargar el histórico de parámetros.
      </Nota>
    );
  }

  const sinAprobar = (data ?? []).filter((v) => !v.estaAprobada);

  // Agrupadas por clave: la pregunta es «cómo cambió esta tarifa», no «qué pasó ese día».
  const porClave = useMemo(() => {
    const m = new Map<string, Version[]>();
    filas.forEach((v) => m.set(v.clave, [...(m.get(v.clave) ?? []), v]));
    return [...m.entries()];
  }, [filas]);

  return (
    <div className="tw:flex tw:flex-col tw:gap-5">
      <header className="tw:flex tw:flex-col tw:gap-1">
        <h1 className="tw:text-xl tw:font-semibold tw:tracking-tight">
          Parámetros normativos
        </h1>
        <p className="tw:text-sm tw:text-tinta-mid">
          Cómo cambió cada parámetro, con <b>sus dos ejes de fecha</b>: desde cuándo regía, y
          desde cuándo lo supimos. Sin el segundo no se puede explicar por qué una liquidación
          vieja usó otro número.
        </p>
      </header>

      {sinAprobar.length > 0 && (
        <Nota tono="aviso">
          {sinAprobar.length === 1
            ? '1 versión está cargada y sin aprobar'
            : `${sinAprobar.length} versiones están cargadas y sin aprobar`}
          . <b>No rigen</b>: el doble control de{' '}
          <code className="tw:font-mono tw:text-xs">HU-145</code> sería decorativo si el valor
          rigiera igual. Están acá para que se vea qué espera firma.
        </Nota>
      )}

      <CampoBusqueda
        etiqueta="Buscar por clave, valor o quién la cargó…"
        valor={filtro}
        onCambio={setFiltro}
      />

      {isPending ? (
        <p className="tw:text-sm tw:text-tinta-mid">Cargando el histórico…</p>
      ) : porClave.length === 0 ? (
        <Vacio
          icono={<ScrollText />}
          titulo={filtro ? 'Ninguna versión coincide' : 'No hay parámetros cargados'}
          descripcion={
            filtro
              ? 'Pruebe con la clave completa, o limpie la búsqueda.'
              : 'Los parámetros se cargan con respaldo documental y los aprueba otra persona.'
          }
        />
      ) : (
        porClave.map(([clave, versiones]) => (
          <Panel key={clave} titulo={`${clave} · ${versiones.length} versión(es)`}>
            <ul className="tw:flex tw:flex-col tw:gap-3">
              {versiones.map((v) => (
                <li
                  key={v.id}
                  className={`tw:flex tw:flex-col tw:gap-0.5 tw:border-l-2 tw:pl-3 ${
                    v.estaAprobada ? 'tw:border-ok-fg' : 'tw:border-aviso-fg'
                  }`}
                >
                  <div className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-3 tw:text-sm">
                    <span className="tw:font-medium tw:font-mono">{v.valor}</span>
                    {!v.estaAprobada && <Pastilla tono="aviso">Sin aprobar — no rige</Pastilla>}
                  </div>

                  {/* Eje NORMATIVO. */}
                  <span className="tw:text-xs tw:text-tinta-mid">
                    regía del {soloFecha(v.vigenteDesde)}{' '}
                    {v.vigenteHasta === null ? (
                      <span className="tw:italic">sin fecha de fin</span>
                    ) : (
                      `al ${soloFecha(v.vigenteHasta)}`
                    )}
                  </span>

                  {/* Eje de TRANSACCIÓN. Nulo en `registradoHasta` es «sigue siendo lo que
                      creemos», no «se dejó de creer». */}
                  <span className="tw:text-xs tw:text-tinta-mid">
                    lo supimos el {diaYHora(v.registradoDesde)}
                    {v.registradoHasta !== null &&
                      ` · corregido el ${diaYHora(v.registradoHasta)}`}
                  </span>

                  <span className="tw:text-xs tw:text-tinta-mid">
                    cargó {v.cargadoPor}
                    {v.aprobadoPor !== null && ` · aprobó ${v.aprobadoPor}`} · respaldo:{' '}
                    {v.respaldo.fuente}, verificado el {soloFecha(v.respaldo.verificadoEl)}
                  </span>
                </li>
              ))}
            </ul>
          </Panel>
        ))
      )}
    </div>
  );
}

interface Version {
  id: string;
  clave: string;
  valor: string;
  /** Eje normativo: desde cuándo regía. */
  vigenteDesde: string;
  vigenteHasta: string | null;
  /** Eje de transacción: desde cuándo lo supimos. */
  registradoDesde: string;
  /** **Nulo es «sigue siendo lo que creemos»**, no «se dejó de creer». */
  registradoHasta: string | null;
  cargadoPor: string;
  /** **Nulo es sin aprobar, y sin aprobar no rige.** */
  aprobadoPor: string | null;
  estaAprobada: boolean;
  respaldo: { fuente: string; verificadoEl: string };
}
