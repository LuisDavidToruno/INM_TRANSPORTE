import type { ReactElement } from 'react';
import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { CircleAlert, Scale, Search } from 'lucide-react';

import { Boton, Campo, Nota, Panel, Pastilla, Vacio } from '../../ui';
import { pedir } from '../../api/misiones';
import { usarPuesto } from '../../app/puesto';
import { diaYHora } from '../M06_Autorizacion/formato';

/**
 * `PT-134` — Hábeas data: todo lo guardado sobre una persona.
 *
 * ── «Expedita y no onerosa» es parte del requisito ──────────────────────────
 * El Artículo 182 lo exige así, y `HU-121` lo aterriza: <i>«sin depender de que alguien consulte
 * la base de datos a mano»</i>. Una institución que tarda semanas en contestar un hábeas data lo
 * incumple aunque termine contestando.
 *
 * ── Y contesta las dos preguntas, no una ────────────────────────────────────
 * <b>Qué guardan sobre mí</b> y <b>quién lo vio</b>. La segunda sólo se puede responder si cada
 * acceso quedó registrado — y es la que la institución no puede improvisar el día que se la
 * pidan.
 *
 * ── Esta consulta también deja asiento ──────────────────────────────────────
 * Atender un hábeas data implica leer datos personales. No registrarla dejaría fuera del control
 * justamente las consultas más sensibles del sistema.
 */
export default function HabeasData(): ReactElement {
  const { elegido } = usarPuesto();
  const [identificacion, setIdentificacion] = useState('');
  const [buscar, setBuscar] = useState(0);

  const { data, isPending, isError } = useQuery({
    queryKey: ['habeas-data', identificacion, buscar],
    queryFn: () =>
      pedir<Expediente>(
        `/personas-externas/habeas-data/${encodeURIComponent(identificacion)}` +
          `?consultante=${encodeURIComponent(elegido?.persona ?? '')}` +
          `&rol=${encodeURIComponent(elegido?.denominacion ?? 'sin puesto')}`,
      ),
    enabled: buscar > 0 && identificacion.trim().length > 0,
  });

  return (
    <div className="tw:flex tw:flex-col tw:gap-5">
      <header className="tw:flex tw:flex-col tw:gap-1">
        <h1 className="tw:text-xl tw:font-semibold tw:tracking-tight">Hábeas data</h1>
        <p className="tw:text-sm tw:text-tinta-mid">
          Todo lo que el sistema guarda sobre una persona, y <b>quién lo ha visto</b>. Es lo que
          la institución tiene que poder responder cuando el titular lo pide.
        </p>
      </header>

      <Nota tono="info" icono={<Scale />}>
        <b>Esta consulta también queda registrada.</b> Atender un hábeas data implica leer datos
        personales, así que deja asiento como cualquier otro acceso — con su nombre y con el
        motivo.
      </Nota>

      <Panel titulo="Sobre quién">
        <div className="tw:flex tw:flex-col tw:gap-3 tw:sm:max-w-md">
          <Campo
            etiqueta="Identificación"
            ayuda="El número con que la persona figura en los manifiestos."
          >
            {(control) => (
              <input
                {...control}
                value={identificacion}
                onChange={(e) => setIdentificacion(e.target.value)}
                placeholder="0801-1990-12345"
                className="loki-foco tw:w-full tw:rounded-control tw:border tw:border-linea tw:bg-panel tw:px-2 tw:py-1.5 tw:font-mono tw:text-cuerpo-2"
              />
            )}
          </Campo>
          <div>
            <Boton variante="primario" icono={<Search />} onClick={() => setBuscar((n) => n + 1)}>
              Buscar todo lo guardado
            </Boton>
          </div>
        </div>
      </Panel>

      {isError ? (
        <Nota tono="riesgo" icono={<CircleAlert />}>
          No se pudo hacer la búsqueda.
        </Nota>
      ) : buscar === 0 ? null : isPending ? (
        <p className="tw:text-sm tw:text-tinta-mid">Buscando…</p>
      ) : data.apariciones.length === 0 && data.quienLoVio.length === 0 ? (
        <Vacio
          icono={<Scale />}
          titulo="El sistema no guarda nada sobre esa identificación"
          descripcion="Es una respuesta válida a un hábeas data, y hay que poder darla con la misma certeza que la contraria."
        />
      ) : (
        <>
          <Panel titulo={`Aparece en ${data.apariciones.length} traslado(s)`}>
            <ul className="tw:flex tw:flex-col tw:gap-2">
              {data.apariciones.map((a, i) => (
                <li
                  key={`${a.mision}-${i}`}
                  className="tw:flex tw:flex-col tw:gap-0.5 tw:border-l-2 tw:border-linea tw:py-1 tw:pl-3 tw:text-sm"
                >
                  <div className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-3">
                    <span className="tw:font-medium">{a.nombre ?? 'sin nombre registrado'}</span>
                    <span className="tw:font-mono tw:text-xs tw:text-tinta-mid">{a.mision}</span>
                  </div>
                  <span className="tw:text-xs tw:text-tinta-mid">
                    {a.queMotivaElTraslado} · {a.origen} → {a.destino}
                    {a.requerimientoOperativo !== null &&
                      ` · requiere ${a.requerimientoOperativo}`}
                  </span>
                </li>
              ))}
            </ul>
          </Panel>

          {data.rectificaciones.length > 0 && (
            <Panel titulo={`${data.rectificaciones.length} rectificación(es)`}>
              <ul className="tw:flex tw:flex-col tw:gap-2">
                {data.rectificaciones.map((r, i) => (
                  <li
                    key={`${r.campo}-${i}`}
                    className="tw:flex tw:flex-col tw:gap-0.5 tw:border-l-2 tw:border-info-fg tw:py-1 tw:pl-3 tw:text-sm"
                  >
                    <div className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-2">
                      <span className="tw:font-medium">{r.campo}</span>
                      {/* El valor anterior no se pierde: es lo que estaba en el papel. */}
                      <span className="tw:font-mono tw:text-xs tw:line-through tw:text-tinta-mid">
                        {r.valorAnterior}
                      </span>
                      <span className="tw:font-mono tw:text-xs">{r.valorRectificado}</span>
                    </div>
                    <span className="tw:text-xs tw:text-tinta-mid">
                      lo pidió {r.quienLaPidio} · {r.motivo} · {diaYHora(r.momento)}
                    </span>
                  </li>
                ))}
              </ul>
            </Panel>
          )}

          {/* La segunda pregunta, y la que no se puede improvisar. */}
          <Panel titulo={`${data.quienLoVio.length} acceso(s) a esos traslados`}>
            {data.quienLoVio.length === 0 ? (
              <p className="tw:text-sm tw:text-tinta-mid">
                Nadie ha consultado los manifiestos donde aparece.
              </p>
            ) : (
              <ul className="tw:flex tw:flex-col tw:gap-2">
                {data.quienLoVio.map((v, i) => (
                  <li
                    key={`${v.momento}-${i}`}
                    className="tw:flex tw:flex-col tw:gap-0.5 tw:border-l-2 tw:border-linea tw:py-1 tw:pl-3 tw:text-sm"
                  >
                    <div className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-3">
                      <span className="tw:font-medium">{v.consultante}</span>
                      <span className="tw:text-tinta-mid">{v.rol}</span>
                      <Pastilla tono={v.alcance === 'SoloRecuento' ? 'neutro' : 'aviso'}>
                        {ALCANCE[v.alcance] ?? v.alcance}
                      </Pastilla>
                    </div>
                    <span className="tw:text-xs tw:text-tinta-mid">
                      {diaYHora(v.momento)} ·{' '}
                      {v.necesidad ?? (
                        <span className="tw:italic tw:text-aviso-fg">no declaró para qué</span>
                      )}
                    </span>
                  </li>
                ))}
              </ul>
            )}
          </Panel>
        </>
      )}
    </div>
  );
}

const ALCANCE: Record<string, string> = {
  SoloRecuento: 'sólo cuántos iban',
  ListaDeNombres: 'la lista de nombres',
  ManifiestoCompleto: 'el manifiesto completo',
};

interface Expediente {
  identificacion: string;
  apariciones: {
    mision: string;
    nombre: string | null;
    forma: string;
    queMotivaElTraslado: string;
    origen: string;
    destino: string;
    requerimientoOperativo: string | null;
  }[];
  rectificaciones: {
    campo: string;
    /** No se pierde: es lo que estaba en el papel. */
    valorAnterior: string;
    valorRectificado: string;
    quienLaPidio: string;
    motivo: string;
    momento: string;
  }[];
  /** **La segunda pregunta del hábeas data**, y la que no se puede improvisar. */
  quienLoVio: {
    consultante: string;
    rol: string;
    momento: string;
    alcance: string;
    necesidad: string | null;
  }[];
}
