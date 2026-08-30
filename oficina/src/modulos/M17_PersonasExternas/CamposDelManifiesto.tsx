import type { ReactElement } from 'react';
import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { CircleAlert, ShieldQuestion, TriangleAlert } from 'lucide-react';

import { Boton, Campo, Nota, Panel, Pastilla } from '../../ui';
import { BloqueoDuro, pedir } from '../../api/misiones';
import { avisar } from '../../ui/avisos';
import { usarQuienEjecuta } from '../../app/puesto';
import { diaYHora } from '../M06_Autorizacion/formato';

/**
 * `PT-128` — Fundamentación del campo sensible: base legal y necesidad operativa.
 *
 * ── La medida de protección más barata es no capturar ───────────────────────
 * `RN-51`: <i>«un dato que no se captura no se puede filtrar, no se puede publicar por error y
 * <b>no se puede pedir por hábeas data</b>»</i>. Por eso el catálogo del manifiesto es cerrado
 * —identificación, institución que motiva el traslado, origen y destino— y todo lo demás exige
 * decir por qué.
 *
 * ── Y activar sin fundamento no se bloquea: se marca ────────────────────────
 * Va contra la intuición y es lo que `HU-112` pide. Bloquear parece más seguro y es peor: quien
 * necesita el dato hoy lo va a capturar igual —en observaciones, en una libreta, en un mensaje—
 * y ahí queda <b>fuera de todo control</b>. Marcado, el dato está dentro del sistema, con su
 * acceso registrado, y aparece en el reporte que revisa Auditoría Interna.
 */
export default function CamposDelManifiesto(): ReactElement {
  const cliente = useQueryClient();
  const quienEjecuta = usarQuienEjecuta();
  const [fundamentando, setFundamentando] = useState<string | null>(null);
  const [legal, setLegal] = useState('');
  const [necesidad, setNecesidad] = useState('');

  const { data, isPending, isError } = useQuery({
    queryKey: ['campos-del-manifiesto'],
    queryFn: () => pedir<Catalogo>('/personas-externas/campos'),
  });

  const fundamento = useMutation({
    mutationFn: (clave: string) =>
      pedir(`/personas-externas/campos/${clave}/fundamento`, {
        method: 'POST',
        body: JSON.stringify({
          baseLegal: legal,
          necesidadOperativa: necesidad,
          registra: quienEjecuta,
          momento: new Date().toISOString(),
        }),
      }),
    onSuccess: async () => {
      avisar.exito('Fundamento registrado. El campo deja de estar marcado.');
      setFundamentando(null);
      setLegal('');
      setNecesidad('');
      await cliente.invalidateQueries({ queryKey: ['campos-del-manifiesto'] });
    },
    onError: (e) =>
      avisar.error(e instanceof BloqueoDuro ? e.paraMostrar : 'No se pudo registrar.'),
  });

  if (isError) {
    return (
      <Nota tono="riesgo" icono={<CircleAlert />}>
        No se pudo cargar el catálogo de campos.
      </Nota>
    );
  }

  return (
    <div className="tw:flex tw:flex-col tw:gap-5">
      <header className="tw:flex tw:flex-col tw:gap-1">
        <h1 className="tw:text-xl tw:font-semibold tw:tracking-tight">
          Qué se pregunta de las personas trasladadas
        </h1>
        <p className="tw:text-sm tw:text-tinta-mid">
          El manifiesto captura <b>lo mínimo</b>: quién es, qué institución o condición motiva el
          traslado, de dónde y a dónde. Todo lo demás hay que justificarlo.
        </p>
      </header>

      {isPending ? (
        <p className="tw:text-sm tw:text-tinta-mid">Cargando…</p>
      ) : (
        <>
          {data.sinFundamento > 0 && (
            <Nota tono="riesgo" icono={<TriangleAlert />}>
              <b>
                {data.sinFundamento === 1
                  ? 'Hay 1 campo sensible activo sin justificar'
                  : `Hay ${data.sinFundamento} campos sensibles activos sin justificar`}
              </b>
              . Se están capturando datos de salud, etnia, situación migratoria o condición de
              vulnerabilidad <b>sin que conste por qué</b>. Aparecen en el reporte de Auditoría
              Interna hasta que alguien lo registre.
            </Nota>
          )}

          {/* La salida que evita el problema en vez de justificarlo. Va arriba de la lista:
              quien viene a activar un campo de salud debería leerla antes. */}
          <Nota tono="info" icono={<ShieldQuestion />}>
            <b>Antes de activar un campo de salud:</b> {data.laSalidaSinCapturar}
          </Nota>

          <Panel titulo={`${data.campos.length} campo(s) en el catálogo`}>
            <ul className="tw:flex tw:flex-col tw:gap-3">
              {data.campos.map((c) => (
                <li
                  key={c.clave}
                  className={`tw:flex tw:flex-col tw:gap-1 tw:border-l-2 tw:py-1 tw:pl-3 ${
                    c.sinFundamento ? 'tw:border-riesgo-fg' : 'tw:border-linea'
                  }`}
                >
                  <div className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-3 tw:text-sm">
                    <span className="tw:font-medium">{c.etiqueta}</span>

                    {c.sensible && (
                      <Pastilla tono={c.sinFundamento ? 'riesgo' : 'aviso'}>
                        {c.claseEnPalabras}
                      </Pastilla>
                    )}

                    {!c.activo && <Pastilla tono="neutro">no se captura</Pastilla>}

                    {/* El texto exacto que `HU-112` pide que se muestre. */}
                    {c.sinFundamento && (
                      <span className="tw:text-xs tw:font-semibold tw:text-riesgo-fg">
                        CAMPO SIN FUNDAMENTO REGISTRADO
                      </span>
                    )}
                  </div>

                  {c.fundamento !== null && (
                    <div className="tw:flex tw:flex-col tw:gap-0.5 tw:text-xs tw:text-tinta-mid">
                      <span>
                        <b>Base legal:</b> {c.fundamento.baseLegal}
                      </span>
                      <span>
                        <b>Necesidad operativa:</b> {c.fundamento.necesidadOperativa}
                      </span>
                      <span>
                        lo registró {c.fundamento.registra} ·{' '}
                        {diaYHora(c.fundamento.momento)}
                      </span>
                    </div>
                  )}

                  {c.sinFundamento &&
                    (fundamentando === c.clave ? (
                      <div className="tw:mt-1 tw:flex tw:flex-col tw:gap-2 tw:sm:max-w-xl">
                        <Campo
                          etiqueta="Base legal"
                          ayuda="Qué norma o convenio autoriza capturar este dato."
                        >
                          {(control) => (
                            <input
                              {...control}
                              value={legal}
                              onChange={(e) => setLegal(e.target.value)}
                              className="loki-foco tw:w-full tw:rounded-control tw:border tw:border-linea tw:bg-panel tw:px-2 tw:py-1.5 tw:text-cuerpo-2"
                            />
                          )}
                        </Campo>

                        {/* La mitad que se olvida, y la que de verdad limita: la base legal
                            sola autoriza capturar todo lo que la norma no prohíba. */}
                        <Campo
                          etiqueta="Necesidad operativa"
                          ayuda="Para qué operación del traslado hace falta. Hay campos que no pueden contestar esta pregunta."
                        >
                          {(control) => (
                            <textarea
                              {...control}
                              value={necesidad}
                              onChange={(e) => setNecesidad(e.target.value)}
                              rows={2}
                              className="loki-foco tw:w-full tw:rounded-control tw:border tw:border-linea tw:bg-panel tw:px-2 tw:py-1.5 tw:text-cuerpo-2"
                            />
                          )}
                        </Campo>

                        <div className="tw:flex tw:gap-2">
                          <Boton
                            variante="primario"
                            cargando={fundamento.isPending}
                            onClick={() => fundamento.mutate(c.clave)}
                          >
                            Registrar el fundamento
                          </Boton>
                          <Boton
                            variante="fantasma"
                            onClick={() => setFundamentando(null)}
                          >
                            Cancelar
                          </Boton>
                        </div>
                      </div>
                    ) : (
                      <div className="tw:mt-1">
                        <Boton
                          variante="secundario"
                          onClick={() => {
                            setFundamentando(c.clave);
                            setLegal('');
                            setNecesidad('');
                          }}
                        >
                          Registrar por qué se captura
                        </Boton>
                      </div>
                    ))}
                </li>
              ))}
            </ul>
          </Panel>
        </>
      )}
    </div>
  );
}

interface Catalogo {
  /** Campos activos, sensibles y sin justificar. Es lo que revisa Auditoría Interna. */
  sinFundamento: number;
  laSalidaSinCapturar: string;
  campos: {
    clave: string;
    etiqueta: string;
    clase: string;
    claseEnPalabras: string;
    sensible: boolean;
    activo: boolean;
    /** Activo, sensible y sin fundamento: **un estado real**, no un error de configuración. */
    sinFundamento: boolean;
    fundamento: {
      baseLegal: string;
      necesidadOperativa: string;
      registra: string;
      momento: string;
    } | null;
  }[];
}
