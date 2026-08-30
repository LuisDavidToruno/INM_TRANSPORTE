import type { ReactElement } from 'react';
import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { CircleAlert, Pencil, TriangleAlert } from 'lucide-react';

import { Boton, Campo, Nota, Panel, Pastilla } from '../../ui';
import { BloqueoDuro, pedir } from '../../api/misiones';
import { avisar } from '../../ui/avisos';
import { usarQuienEjecuta } from '../../app/puesto';
import { diaYHora } from '../M06_Autorizacion/formato';

/**
 * `PT-053` y `PT-054` — La cola de conflictos y el comparador lado a lado.
 *
 * ── <i>«Es la pantalla más difícil del sistema y la que nadie diseña hasta que ya duele»</i> ──
 * El caso que la define: <b>el motorista anotó odómetro 93,610 el 16 de mayo con foto del
 * tablero; la delegación digitó 93,061 el 28 de mayo con foto del original. Los dos son de buena
 * fe. Uno está mal, y la diferencia son 549 kilómetros</b> que van a entrar en una conciliación
 * de combustible.
 *
 * ── Las cuatro cosas que esta pantalla se niega a hacer ─────────────────────
 * <b>No edita.</b> `R-6`: ninguna pantalla edita un hecho pasado. El usuario va a buscar ese
 * botón, y cuando lo busque la pantalla contesta.
 *
 * <b>No combina.</b> Dos versiones que difieren en campos distintos se deciden por separado:
 * fusionarlas produciría un registro que nadie capturó.
 *
 * <b>No resuelve sola.</b> Ninguna resolución automática es aceptable cuando lo que diverge son
 * odómetros, galones y montos.
 *
 * <b>Y no habla de datos.</b> Ni <i>merge</i>, ni <i>timestamp</i>, ni <i>versión</i>, ni
 * <i>hash divergente</i>. Es criterio de aceptación literal de `HU-068`: quien la usa es el Jefe
 * de Transporte, que no entiende de sincronización <b>y no tiene por qué</b>.
 */
export default function Conflictos(): ReactElement {
  const cliente = useQueryClient();
  const quienEjecuta = usarQuienEjecuta();
  const [abierto, setAbierto] = useState<string | null>(null);
  const [motivo, setMotivo] = useState('');

  const { data, isPending, isError } = useQuery({
    queryKey: ['conflictos'],
    queryFn: () => pedir<Cola>('/conflictos'),
  });

  const resolucion = useMutation({
    mutationFn: (v: { id: string; seToma: 'Servidor' | 'Campo' }) =>
      pedir(`/conflictos/${v.id}/resolver`, {
        method: 'POST',
        body: JSON.stringify({
          seToma: v.seToma,
          motivo,
          resuelve: quienEjecuta,
          momento: new Date().toISOString(),
        }),
      }),
    onSuccess: async () => {
      avisar.exito('Resuelto. La versión descartada queda en el expediente.');
      setAbierto(null);
      setMotivo('');
      await cliente.invalidateQueries({ queryKey: ['conflictos'] });
    },
    onError: (e) =>
      avisar.error(e instanceof BloqueoDuro ? e.paraMostrar : 'No se pudo resolver.'),
  });

  if (isError) {
    return (
      <Nota tono="riesgo" icono={<CircleAlert />}>
        No se pudo cargar la cola.
      </Nota>
    );
  }

  const altos = data?.conflictos.filter((c) => c.impacto === 'Alto').length ?? 0;

  return (
    <div className="tw:flex tw:flex-col tw:gap-5">
      <header className="tw:flex tw:flex-col tw:gap-1">
        <h1 className="tw:text-xl tw:font-semibold tw:tracking-tight">
          Registros que no coinciden
        </h1>
        <p className="tw:text-sm tw:text-tinta-mid">
          Dos personas registraron lo mismo de forma distinta. <b>Las dos de buena fe</b> — una
          de las dos describe lo que pasó, y hay que decir cuál.
        </p>
      </header>

      {isPending ? (
        <p className="tw:text-sm tw:text-tinta-mid">Cargando…</p>
      ) : data.conflictos.length === 0 ? (
        <Panel titulo="Nada pendiente">
          <p className="tw:text-sm tw:text-tinta-mid">
            No hay registros en desacuerdo. Aparecen solos cuando un dispositivo sincroniza algo
            que no coincide con lo que la oficina ya tenía.
          </p>
        </Panel>
      ) : (
        <>
          {altos > 0 && (
            <Nota tono="riesgo" icono={<TriangleAlert />}>
              <b>
                {altos === 1
                  ? '1 desacuerdo es sobre un kilometraje, un monto o una autorización'
                  : `${altos} desacuerdos son sobre kilometrajes, montos o autorizaciones`}
              </b>
              . Esos <b>frenan la liquidación</b> de su misión hasta que se resuelvan, y se
              deciden uno por uno — nunca en bloque.
            </Nota>
          )}

          <ul className="tw:flex tw:flex-col tw:gap-4">
            {data.conflictos.map((c) => (
              <li key={c.id}>
                <Panel
                  titulo={`${EN_PALABRAS[c.campo] ?? c.campo} · ${c.diasEsperando} día(s) esperando`}
                >
                  <div className="tw:flex tw:flex-col tw:gap-3">
                    <div className="tw:flex tw:flex-wrap tw:items-center tw:gap-2">
                      <Pastilla tono={c.impacto === 'Alto' ? 'riesgo' : 'neutro'}>
                        {c.impacto === 'Alto'
                          ? 'frena la liquidación'
                          : 'no frena la liquidación'}
                      </Pastilla>
                    </div>

                    {/* ── Las dos versiones, lado a lado y completas ───────── */}
                    <div className="tw:grid tw:gap-3 tw:sm:grid-cols-2">
                      <Lado
                        titulo="Lo que la oficina tenía"
                        version={c.delServidor}
                        elegida={abierto === c.id}
                        onElegir={() => resolucion.mutate({ id: c.id, seToma: 'Servidor' })}
                        puedeElegir={abierto === c.id}
                        enviando={resolucion.isPending}
                      />
                      <Lado
                        titulo="Lo que llegó del campo"
                        version={c.deCampo}
                        elegida={abierto === c.id}
                        onElegir={() => resolucion.mutate({ id: c.id, seToma: 'Campo' })}
                        puedeElegir={abierto === c.id}
                        enviando={resolucion.isPending}
                      />
                    </div>

                    {abierto === c.id ? (
                      <div className="tw:flex tw:flex-col tw:gap-2">
                        <Campo
                          etiqueta="Por qué toma esa versión"
                          ayuda="La decisión queda en el expediente y el auditor la va a leer."
                        >
                          {(control) => (
                            <textarea
                              {...control}
                              value={motivo}
                              onChange={(e) => setMotivo(e.target.value)}
                              rows={2}
                              className="loki-foco tw:w-full tw:rounded-control tw:border tw:border-linea tw:bg-panel tw:px-2 tw:py-1.5 tw:text-cuerpo-2"
                            />
                          )}
                        </Campo>
                        <p className="tw:text-xs tw:text-tinta-mid">
                          Escriba el motivo y después elija una de las dos versiones.
                        </p>
                      </div>
                    ) : (
                      <div className="tw:flex tw:flex-wrap tw:gap-2">
                        <Boton
                          variante="primario"
                          onClick={() => {
                            setAbierto(c.id);
                            setMotivo('');
                          }}
                        >
                          Decidir cuál describe lo que pasó
                        </Boton>
                        <BotonDeEditar mensaje={data.porQueNoSeEdita} />
                      </div>
                    )}
                  </div>
                </Panel>
              </li>
            ))}
          </ul>

          <Nota tono="info">{data.porQueNoSeCombina}</Nota>
        </>
      )}
    </div>
  );
}

/**
 * Una de las dos versiones, con <b>los tres datos que permiten decidir</b>.
 *
 * Quién la capturó, cuándo ocurrió el hecho y cuándo se registró. Los dos últimos son distintos
 * y esa distinción es la que resuelve el caso: una versión anotada en el momento pesa distinto
 * que una digitada del papel doce días después.
 */
function Lado({
  titulo,
  version,
  puedeElegir,
  onElegir,
  enviando,
}: {
  readonly titulo: string;
  readonly version: Version;
  readonly elegida: boolean;
  readonly puedeElegir: boolean;
  readonly onElegir: () => void;
  readonly enviando: boolean;
}): ReactElement {
  return (
    <div className="tw:flex tw:flex-col tw:gap-2 tw:rounded-panel tw:border tw:border-linea tw:p-3">
      <span className="tw:text-xs tw:text-tinta-mid">{titulo}</span>

      <span className="tw:text-xl tw:font-semibold tw:tabular-nums">{version.valor}</span>

      <div className="tw:flex tw:flex-col tw:gap-0.5 tw:text-xs tw:text-tinta-mid">
        <span>lo registró {version.capturadaPor}</span>
        <span>pasó el {diaYHora(version.ocurrioEl)}</span>
        {/* El segundo dato de tiempo, y el que casi siempre decide. */}
        <span>llegó al sistema el {diaYHora(version.registradoEl)}</span>
        {version.dispositivo !== null && <span>desde {version.dispositivo}</span>}
      </div>

      {/* Las dos fotos se ven al mismo tiempo, no detrás de un clic: la del tablero contra la
          del original es lo que en la práctica resuelve el conflicto. */}
      {version.foto === null ? (
        <span className="tw:text-xs tw:italic tw:text-tinta-mid">sin fotografía</span>
      ) : (
        <span className="tw:font-mono tw:text-xs tw:text-tinta-mid">
          fotografía {version.foto.slice(-6)}
        </span>
      )}

      {puedeElegir && (
        <Boton variante="secundario" onClick={onElegir} cargando={enviando}>
          Ésta describe lo que pasó
        </Boton>
      )}
    </div>
  );
}

/**
 * El botón que el usuario va a buscar, y que contesta en vez de existir.
 *
 * §7.1 punto 5: <i>«El usuario va a buscarla. Cuando la busque, la pantalla debe responder».</i>
 * Omitirlo no evita que lo busque — sólo hace que lo busque más tiempo.
 */
function BotonDeEditar({ mensaje }: { readonly mensaje: string }): ReactElement {
  const [dicho, setDicho] = useState(false);

  return dicho ? (
    <span className="tw:max-w-md tw:text-xs tw:text-tinta-mid">{mensaje}</span>
  ) : (
    <Boton variante="fantasma" onClick={() => setDicho(true)} icono={<Pencil />}>
      Corregir el dato
    </Boton>
  );
}

/**
 * El nombre del campo en palabras del negocio.
 *
 * `HU-068` lo pone como criterio literal. `odometroRetorno` es lenguaje de datos, y quien usa
 * esta pantalla no tiene por qué saber qué significa.
 */
const EN_PALABRAS: Record<string, string> = {
  odometroSalida: 'Kilometraje al salir',
  odometroRetorno: 'Kilometraje al volver',
  odometro: 'Kilometraje',
  monto: 'Monto',
  galones: 'Galones',
  autorizacion: 'Autorización',
  horaDeArribo: 'Hora de llegada',
  ubicacion: 'Ubicación',
  observaciones: 'Observaciones',
  transicion: 'Lo que se registró de la misión',
};


interface Version {
  valor: string;
  capturadaPor: string;
  /** Cuándo pasó el hecho. */
  ocurrioEl: string;
  /** Cuándo llegó al sistema. **Distinto del anterior**, y casi siempre lo que decide. */
  registradoEl: string;
  dispositivo: string | null;
  foto: string | null;
}

interface Cola {
  /** La respuesta a quien busca el botón de editar. */
  porQueNoSeEdita: string;
  porQueNoSeCombina: string;
  conflictos: {
    id: string;
    expediente: string;
    transicion: string;
    campo: string;
    impacto: string;
    diasEsperando: number;
    estado: string;
    delServidor: Version;
    deCampo: Version;
    resolucion: unknown | null;
  }[];
}
