import type { ReactElement } from 'react';
import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { CircleAlert, Eye, ScrollText, UserRoundX } from 'lucide-react';

import { Boton, Campo, Nota, Panel, Pastilla, Segmentado, Vacio } from '../../ui';
import type { Tono } from '../../ui';
import { pedir } from '../../api/misiones';
import { usarPuesto } from '../../app/puesto';
import { diaYHora } from '../M06_Autorizacion/formato';

/**
 * `PT-095` — Consulta del manifiesto <b>bajo necesidad de conocer, con registro</b>.
 *
 * ── Lo que esta pantalla hace antes de mostrar nada ─────────────────────────
 * Deja asiento. <b>No hay forma de ver un manifiesto sin que quede registrado</b> quién lo miró,
 * con qué rol, cuándo y <b>qué se le mostró</b> — y eso incluye a cualquier rol, sin excepción.
 *
 * ── Y el alcance no es un filtro de presentación ────────────────────────────
 * Con «sólo cuántos van», los nombres <b>no viajan</b>: no salen de la consulta. Si fueran a la
 * respuesta y se ocultaran acá, cualquiera podría verlos abriendo las herramientas del
 * navegador — y el asiento diría que sólo vio un número.
 */
export default function Manifiesto(): ReactElement {
  const { elegido } = usarPuesto();
  const [mision, setMision] = useState('');
  const [alcance, setAlcance] = useState('SoloRecuento');
  const [necesidad, setNecesidad] = useState('');
  const [consultar, setConsultar] = useState(0);

  const { data, isPending, isError, error } = useQuery({
    queryKey: ['manifiesto', mision, alcance, consultar],
    queryFn: () =>
      pedir<Visto>(
        `/personas-externas/manifiesto/${mision}` +
          `?consultante=${encodeURIComponent(elegido?.persona ?? '')}` +
          `&rol=${encodeURIComponent(elegido?.denominacion ?? 'sin puesto')}` +
          `&alcance=${alcance}` +
          (necesidad ? `&necesidad=${encodeURIComponent(necesidad)}` : ''),
      ),
    enabled: consultar > 0 && mision.length === 26,
  });

  const pideMotivo = alcance !== 'SoloRecuento';

  return (
    <div className="tw:flex tw:flex-col tw:gap-5">
      <header className="tw:flex tw:flex-col tw:gap-1">
        <h1 className="tw:text-xl tw:font-semibold tw:tracking-tight">
          Quiénes van en una misión
        </h1>
        <p className="tw:text-sm tw:text-tinta-mid">
          Esta consulta <b>queda registrada</b> con su nombre y con lo que se le muestre. El
          titular de los datos puede pedirla por hábeas data.
        </p>
      </header>

      <Panel titulo="Qué quiere ver">
        <div className="tw:flex tw:flex-col tw:gap-3 tw:sm:max-w-xl">
          <Campo etiqueta="Misión" ayuda="El identificador del expediente.">
            {(control) => (
              <input
                {...control}
                value={mision}
                onChange={(e) => setMision(e.target.value.trim().toUpperCase())}
                placeholder="ULID del expediente"
                className="loki-foco tw:w-full tw:rounded-control tw:border tw:border-linea tw:bg-panel tw:px-2 tw:py-1.5 tw:font-mono tw:text-cuerpo-2"
              />
            )}
          </Campo>

          <Campo
            etiqueta="Alcance"
            ayuda="Cuántos van no lleva datos personales. Las otras dos sí, y por eso piden motivo."
          >
            {() => (
              <Segmentado
                etiqueta="Alcance"
                valor={alcance}
                onCambio={setAlcance}
                opciones={[
                  { valor: 'SoloRecuento', etiqueta: 'Sólo cuántos van' },
                  { valor: 'ListaDeNombres', etiqueta: 'La lista de nombres' },
                  { valor: 'ManifiestoCompleto', etiqueta: 'El manifiesto completo' },
                ]}
              />
            )}
          </Campo>

          {pideMotivo && (
            <Campo
              etiqueta="Para qué necesita verlo"
              ayuda="Queda registrado con su nombre. Quien no puede completar esta frase suele no necesitar el dato."
            >
              {(control) => (
                <input
                  {...control}
                  value={necesidad}
                  onChange={(e) => setNecesidad(e.target.value)}
                  className="loki-foco tw:w-full tw:rounded-control tw:border tw:border-linea tw:bg-panel tw:px-2 tw:py-1.5 tw:text-cuerpo-2"
                />
              )}
            </Campo>
          )}

          <div>
            <Boton
              variante="primario"
              icono={<Eye />}
              onClick={() => setConsultar((n) => n + 1)}
            >
              Consultar, dejando constancia
            </Boton>
          </div>
        </div>
      </Panel>

      {consultar > 0 && mision.length !== 26 ? (
        <Nota tono="aviso">Ingrese el identificador completo del expediente.</Nota>
      ) : isError ? (
        <Nota tono="riesgo" icono={<CircleAlert />}>
          {error instanceof Error ? error.message : 'No se pudo consultar.'}
        </Nota>
      ) : consultar === 0 ? null : isPending ? (
        <p className="tw:text-sm tw:text-tinta-mid">Consultando…</p>
      ) : (
        <>
          <Panel titulo="Lo declarado y lo que pasó">
            <div className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-6 tw:gap-y-2">
              <Cifra valor={data.declaradas} texto="declaradas al despachar" />
              <Cifra valor={data.efectivas} texto="fueron de verdad" />

              {/* La diferencia no es un error: es lo que la liquidación compara. */}
              {data.declaradas !== data.efectivas && (
                <Pastilla tono="aviso">
                  hay diferencia entre lo declarado y lo que pasó
                </Pastilla>
              )}

              {!data.cerrado && <Pastilla tono="info">todavía no ha salido</Pastilla>}
            </div>

            {data.cerrado && data.cerradoEl !== null && (
              <p className="tw:mt-2 tw:text-xs tw:text-tinta-mid">
                El manifiesto se cerró el {diaYHora(data.cerradoEl)} al despachar.{' '}
                <b>Lo declarado ya no cambia</b>: lo que pasó después son novedades.
              </p>
            )}
          </Panel>

          {alcance === 'SoloRecuento' ? (
            <Nota tono="info">
              Pidió sólo el recuento, así que <b>los nombres no salieron del servidor</b>. No
              están ocultos en esta pantalla: no viajaron.
            </Nota>
          ) : (
            <Panel titulo={`${data.personas.length} persona(s) declarada(s)`}>
              <ul className="tw:flex tw:flex-col tw:gap-2">
                {data.personas.map((p, i) => (
                  <li
                    key={`${p.identificacion ?? 'sin-id'}-${i}`}
                    className="tw:flex tw:flex-col tw:gap-0.5 tw:border-l-2 tw:border-linea tw:py-1 tw:pl-3"
                  >
                    <div className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-3 tw:text-sm">
                      {/* Nulo no es un registro incompleto: es una persona que no traía
                          documento, y que figura igual. */}
                      {p.nombre === null ? (
                        <span className="tw:flex tw:items-center tw:gap-1.5 tw:italic tw:text-tinta-mid">
                          <UserRoundX className="tw:size-4" aria-hidden />
                          persona no identificada
                        </span>
                      ) : (
                        <span className="tw:font-medium">{p.nombre}</span>
                      )}

                      <Pastilla tono={TONO_FORMA[p.forma] ?? 'neutro'}>
                        {FORMA[p.forma] ?? p.forma}
                      </Pastilla>

                      {p.identificacion !== null && (
                        <span className="tw:font-mono tw:text-xs tw:text-tinta-mid">
                          {p.identificacion}
                        </span>
                      )}
                    </div>

                    <span className="tw:text-xs tw:text-tinta-mid">
                      {p.queMotivaElTraslado} · {p.origen} → {p.destino}
                      {p.requerimientoOperativo !== null &&
                        ` · requiere ${p.requerimientoOperativo}`}
                    </span>
                  </li>
                ))}
              </ul>
            </Panel>
          )}

          {data.novedades.length > 0 && (
            <Panel titulo={`${data.novedades.length} novedad(es) en ruta`}>
              <ul className="tw:flex tw:flex-col tw:gap-2">
                {data.novedades.map((n, i) => (
                  <li
                    key={`${n.tipo}-${i}`}
                    className="tw:flex tw:flex-col tw:gap-0.5 tw:border-l-2 tw:border-aviso-fg tw:py-1 tw:pl-3"
                  >
                    <div className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-3 tw:text-sm">
                      <span className="tw:font-medium">{NOVEDAD[n.tipo] ?? n.tipo}</span>
                      {n.aQuien !== null && (
                        <span className="tw:text-tinta-mid">{n.aQuien}</span>
                      )}
                      {/* Sólo lo lleva quien subió: es lo que separa una decisión de un favor. */}
                      {n.autoriza !== null && (
                        <Pastilla tono="info">autorizó {n.autoriza}</Pastilla>
                      )}
                    </div>
                    <span className="tw:text-xs tw:text-tinta-mid">
                      {n.motivo} · {diaYHora(n.fechaDelHecho)}
                      {n.dondePaso !== null && ` · ${n.dondePaso}`}
                    </span>
                  </li>
                ))}
              </ul>
            </Panel>
          )}

          {data.personas.length === 0 && alcance !== 'SoloRecuento' && (
            <Vacio
              icono={<ScrollText />}
              titulo="El manifiesto no tiene personas declaradas"
              descripcion="La misión no lleva personas externas, o todavía no se capturaron."
            />
          )}
        </>
      )}
    </div>
  );
}

function Cifra({ valor, texto }: { valor: number; texto: string }): ReactElement {
  return (
    <span className="tw:flex tw:items-baseline tw:gap-1.5">
      <span className="tw:text-2xl tw:font-semibold tw:tabular-nums">{valor}</span>
      <span className="tw:text-sm tw:text-tinta-mid">{texto}</span>
    </span>
  );
}

const FORMA: Record<string, string> = {
  Documento: 'con documento',
  Alternativa: 'identificación alternativa',
  NoIdentificada: 'sin identificar',
};

const TONO_FORMA: Record<string, Tono> = {
  Documento: 'ok',
  Alternativa: 'info',
  NoIdentificada: 'aviso',
};

const NOVEDAD: Record<string, string> = {
  NoSePresento: 'No se presentó',
  SubioEnRuta: 'Subió en ruta',
  BajoAntes: 'Bajó antes del destino',
};

interface Visto {
  /** Lo autorizado al despachar. **No cambia.** */
  declaradas: number;
  /** Lo que pasó, según las novedades. */
  efectivas: number;
  cerrado: boolean;
  cerradoEl: string | null;
  /** **Vacía con sólo recuento**: los nombres no viajan. */
  personas: {
    nombre: string | null;
    identificacion: string | null;
    forma: string;
    queMotivaElTraslado: string;
    origen: string;
    destino: string;
    requerimientoOperativo: string | null;
  }[];
  novedades: {
    tipo: string;
    aQuien: string | null;
    motivo: string;
    dondePaso: string | null;
    fechaDelHecho: string;
    autoriza: string | null;
  }[];
}
