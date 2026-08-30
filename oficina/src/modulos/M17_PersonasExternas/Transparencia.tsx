import type { ReactElement } from 'react';
import { useState } from 'react';
import { useMutation, useQuery } from '@tanstack/react-query';
import { CircleAlert, Globe, Trash2, TriangleAlert } from 'lucide-react';

import { Boton, Nota, Panel, RangoFechas, Campo } from '../../ui';
import { BloqueoDuro, pedir } from '../../api/misiones';
import { avisar } from '../../ui/avisos';
import { soloFecha } from '../M06_Autorizacion/formato';

/**
 * `PT-136` y `PT-137` — la exportación de transparencia y la depuración.
 *
 * ── Las dos son el final del ciclo del dato ─────────────────────────────────
 * Una lo publica <b>sin datos personales</b>; la otra los borra al vencer su plazo. Van juntas
 * porque contestan la misma pregunta desde dos lados: qué sale del sistema, y qué deja de estar
 * en él.
 *
 * ── El reporte no filtra: sale de otro origen ───────────────────────────────
 * `RN-51`: <i>«no por filtrado en el reporte, sino por separación de origen»</i>. La tabla de
 * personas <b>ni se consulta</b>. La diferencia importa porque un filtro se puede olvidar —
 * basta que alguien agregue una columna para publicar nombres.
 */
export default function Transparencia(): ReactElement {
  const [desde, setDesde] = useState('2026-01-01');
  const [hasta, setHasta] = useState('2027-12-31');

  const reporte = useQuery({
    queryKey: ['transparencia', desde, hasta],
    queryFn: () => pedir<Reporte>(`/personas-externas/transparencia?desde=${desde}&hasta=${hasta}`),
  });

  const plazo = useQuery({
    queryKey: ['depuracion', 'plazo'],
    queryFn: () => pedir<Plazo>('/personas-externas/depuracion/plazo'),
  });

  const simulacion = useMutation({
    mutationFn: () =>
      pedir<Simulado>('/personas-externas/depuracion', {
        method: 'POST',
        body: JSON.stringify({ momento: new Date().toISOString(), simular: true }),
      }),
    onError: (e) =>
      avisar.error(e instanceof BloqueoDuro ? e.paraMostrar : 'No se pudo simular.'),
  });

  return (
    <div className="tw:flex tw:flex-col tw:gap-5">
      <header className="tw:flex tw:flex-col tw:gap-1">
        <h1 className="tw:text-xl tw:font-semibold tw:tracking-tight">
          Qué se publica y qué se depura
        </h1>
        <p className="tw:text-sm tw:text-tinta-mid">
          El reporte para el Portal Único de Transparencia, y el borrado de los datos personales
          al vencer su plazo.
        </p>
      </header>

      {/* ── PT-136 ──────────────────────────────────────────────────────────── */}
      <Panel titulo="Reporte de transparencia">
        <div className="tw:flex tw:flex-col tw:gap-3">
          <Nota tono="ok" icono={<Globe />}>
            <b>Este reporte no puede contener datos personales</b>, y no porque se filtren: sale
            de la vista de gestión y <b>la tabla de personas ni se consulta</b>. Lo único que
            cruza es cuántas personas se trasladaron. Nadie tiene que borrar nombres a mano antes
            de publicarlo.
          </Nota>

          <div className="tw:sm:max-w-lg">
            <Campo etiqueta="Período">
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

          {reporte.isError ? (
            <Nota tono="riesgo" icono={<CircleAlert />}>
              No se pudo generar el reporte.
            </Nota>
          ) : reporte.isPending ? (
            <p className="tw:text-sm tw:text-tinta-mid">Generando…</p>
          ) : (
            <>
              <p className="tw:text-sm">
                <b>{reporte.data.filas.length} traslado(s)</b> en el período, listos para
                publicar.
              </p>

              <div className="tw:overflow-x-auto">
                <table className="tw:w-full tw:text-sm">
                  <thead>
                    <tr className="tw:text-left tw:text-xs tw:text-tinta-mid">
                      <th className="tw:py-1 tw:pr-3">Folio</th>
                      <th className="tw:py-1 tw:pr-3">Dependencia</th>
                      <th className="tw:py-1 tw:pr-3">Destino</th>
                      <th className="tw:py-1 tw:pr-3">Objeto</th>
                      <th className="tw:py-1 tw:pr-3">Salida</th>
                      <th className="tw:py-1 tw:pr-3 tw:text-right">Personas</th>
                    </tr>
                  </thead>
                  <tbody>
                    {reporte.data.filas.slice(0, 25).map((f) => (
                      <tr key={f.folio} className="tw:border-t tw:border-linea">
                        <td className="tw:py-1 tw:pr-3 tw:font-mono tw:text-xs">{f.folio}</td>
                        <td className="tw:py-1 tw:pr-3">{f.dependencia}</td>
                        <td className="tw:py-1 tw:pr-3">{f.destino}</td>
                        <td className="tw:py-1 tw:pr-3">{f.objetoDelTraslado}</td>
                        <td className="tw:py-1 tw:pr-3">{soloFecha(f.salida)}</td>
                        {/* Cuántas, no quiénes. */}
                        <td className="tw:py-1 tw:pr-3 tw:text-right tw:tabular-nums">
                          {f.personasTrasladadas}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>

              {reporte.data.filas.length > 25 && (
                <p className="tw:text-xs tw:text-tinta-mid">
                  Se muestran 25 de {reporte.data.filas.length}. La exportación lleva todas.
                </p>
              )}
            </>
          )}
        </div>
      </Panel>

      {/* ── PT-137 ──────────────────────────────────────────────────────────── */}
      <Panel titulo="Depuración de datos personales">
        <div className="tw:flex tw:flex-col tw:gap-3">
          <Nota tono="riesgo" icono={<TriangleAlert />}>
            <b>Esto es lo único en todo el sistema que destruye contenido.</b> Todo lo demás se
            reversa, se anula o se marca. Por eso no se ejecuta sin plazo configurado, no toca
            nada financiero ni de bienes, y <b>no corre sin aviso previo</b>: una destrucción
            silenciosa es indistinguible de una pérdida de datos.
          </Nota>

          {plazo.isPending ? (
            <p className="tw:text-sm tw:text-tinta-mid">Consultando el plazo…</p>
          ) : plazo.isError ? (
            <Nota tono="riesgo">No se pudo consultar el plazo.</Nota>
          ) : !plazo.data.configurado ? (
            <Nota tono="aviso">
              <b>No hay plazo configurado, así que no se depura nada.</b> Y{' '}
              <b>no se aplica ninguno por omisión</b>: cuánto tiempo conserva la institución la
              identidad de quien trasladó no es una decisión técnica. Se acuerda con Auditoría
              Interna y el Oficial de Información Pública.
            </Nota>
          ) : (
            <p className="tw:text-sm">
              Los datos personales se depuran a los{' '}
              <b>{plazo.data.plazoEnDias} días</b> del cierre del manifiesto — contados desde que
              se cerró, no desde que se capturó.
            </p>
          )}

          <div>
            <Boton
              variante="secundario"
              icono={<Trash2 />}
              cargando={simulacion.isPending}
              onClick={() => simulacion.mutate()}
            >
              Ver cuánto alcanzaría (sin borrar nada)
            </Boton>
          </div>

          {simulacion.data !== undefined && (
            <Nota tono="info">
              <b>
                Alcanzaría {simulacion.data.manifiestos} manifiesto(s) y{' '}
                {simulacion.data.personas} persona(s).
              </b>{' '}
              No se borró nada: esto es lo que hay que ver <b>antes</b> de avisar.
              <p className="tw:mt-1 tw:text-xs">{simulacion.data.loQueNoSeToca}</p>
            </Nota>
          )}
        </div>
      </Panel>
    </div>
  );
}

interface Reporte {
  desde: string;
  hasta: string;
  sinDatosPersonales: boolean;
  filas: {
    folio: string;
    estado: string;
    dependencia: string;
    destino: string;
    objetoDelTraslado: string;
    salida: string;
    retorno: string;
    /** Cuántas, no quiénes. Es lo máximo que cruza la frontera. */
    personasTrasladadas: number;
  }[];
}

interface Plazo {
  /** **Nulo cuando no está configurado**, y no se sustituye por nada. */
  plazoEnDias: number | null;
  configurado: boolean;
  porQue: string;
}

interface Simulado {
  simulacion: boolean;
  plazoEnDias: number;
  manifiestos: number;
  personas: number;
  loQueNoSeToca: string;
}
