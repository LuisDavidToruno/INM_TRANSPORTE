import type { ReactElement } from 'react';
import { useMemo, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { CircleAlert, Eye, Lock, ShieldAlert, Users } from 'lucide-react';

import {
  Boton,
  Campo,
  CampoBusqueda,
  CampoFecha,
  Modal,
  Nota,
  Panel,
  Pastilla,
  Vacio,
  avisar,
} from '../../ui';
import { BloqueoDuro } from '../../api/misiones';
import { nuevoUlid } from '../../api/titulos';
import {
  TEXTO_DE_ALCANCE,
  TEXTO_DE_ROL,
  catalogoDeOrganizacion,
  cerrarCompetencia,
  competenciasDe,
  otorgarCompetencia,
  padronDePuestos,
} from '../../api/organizacion';
import type {
  AlcanceDeDatos,
  PuestoDelPadron,
  Rol,
} from '../../api/organizacion';
import { soloFecha } from '../M06_Autorizacion/formato';

/**
 * `PT-096` y `PT-097` — Puestos, competencias y acumulación vigilada.
 *
 * ── Las dos mitades de `M-01`, y por qué sólo una se edita ──────────────────
 * **Quién ocupa qué puesto es espejo de ARGOS.** `RN-48` y `DP-001` son taxativos: los datos
 * de otro dueño se guardan marcados como espejo y *«ninguna pantalla ni operación de SIGTI
 * debe permitir editarlos»*. Por eso la columna de ocupantes se lee y no se toca.
 *
 * **Qué facultades tiene cada puesto sí es de SIGTI**, porque ARGOS no sabe qué es despachar
 * un vehículo ni entregar un vale. Esa mitad se otorga acá, con su vigencia.
 *
 * ── Por qué `PT-097` no es una pantalla aparte ──────────────────────────────
 * `PT-097` es *«asignación puesto↔rol con control de acumulación incompatible»*, y el control
 * sólo significa algo **junto a lo que el puesto ya tiene**. Separarlo en otra pantalla
 * obligaría a memorizar la acumulación actual para entender qué está por pasar.
 *
 * ── El permiso se asigna al puesto. Siempre ─────────────────────────────────
 * §2.2. La rotación en el sector público es alta, y con el permiso colgando de la persona cada
 * rotación obliga a reconstruirlo a mano — *«y lo que se reconstruye a mano se reconstruye
 * mal: se copian los permisos del que se fue “para que no se trabe el trabajo”, y en seis
 * meses nadie sabe por qué el auxiliar de bodega puede aprobar fondos»*.
 */
export default function Puestos(): ReactElement {
  const [filtro, setFiltro] = useState('');
  const [aOtorgar, setAOtorgar] = useState<PuestoDelPadron | null>(null);
  const [aVer, setAVer] = useState<string | null>(null);

  const { data, isPending, isError } = useQuery({
    queryKey: ['padron-puestos'],
    queryFn: () => padronDePuestos(),
  });

  const filas = useMemo(() => {
    const todos = data ?? [];
    const busqueda = filtro.trim().toLowerCase();
    if (!busqueda) return todos;

    return todos.filter((p) =>
      [
        p.puesto,
        p.ocupantes.join(' '),
        p.vacante ? 'vacante' : '',
        p.competencias.map((c) => `${TEXTO_DE_ROL[c.rol] ?? c.rol} ${c.alcance}`).join(' '),
        p.competencias.some((c) => c.paresVigilados !== null) ? 'acumulación vigilada' : '',
      ]
        .join(' ')
        .toLowerCase()
        .includes(busqueda),
    );
  }, [data, filtro]);

  if (isError) {
    return (
      <Nota tono="riesgo" icono={<CircleAlert />}>
        No se pudo cargar el padrón de puestos.
      </Nota>
    );
  }

  const todos = data ?? [];
  const vigilados = todos.filter((p) => p.competencias.some((c) => c.paresVigilados !== null));
  const vacantes = todos.filter((p) => p.vacante);
  const coocupados = todos.filter((p) => p.ocupantes.length > 1);

  return (
    <div className="tw:flex tw:flex-col tw:gap-5">
      <header className="tw:flex tw:flex-col tw:gap-1">
        <h1 className="tw:text-xl tw:font-semibold tw:tracking-tight">Puestos y competencias</h1>
        <p className="tw:text-sm tw:text-tinta-mid">
          <b>El permiso se asigna al puesto. Siempre.</b> Una persona ejerce una facultad porque
          ocupa un puesto que la tiene, y por ninguna otra vía — así la rotación es un cambio de
          una fila y no un proyecto.
        </p>
      </header>

      {/* Lo que el tablero de `ACT-08` y `ACT-12` existe para mostrar. */}
      {vigilados.length > 0 && (
        <Nota tono="aviso" icono={<Eye />}>
          {vigilados.length === 1
            ? '1 puesto quedó de acumulación vigilada'
            : `${vigilados.length} puestos quedaron de acumulación vigilada`}
          . <b>No es un error ni una asignación mal hecha</b>: es una acumulación que sólo es
          incompatible sobre un expediente concreto, y prohibirla de entrada dejaría sin operar
          a las delegaciones. <b>El bloqueo llega al ejecutar el acto</b>, no acá.
        </Nota>
      )}

      {/* El espejo no se edita, y decirlo evita que alguien busque el botón que no está. */}
      <Nota tono="info" icono={<Lock />}>
        <b>Quién ocupa cada puesto viene de ARGOS y no se edita desde acá</b> (
        <code className="tw:font-mono tw:text-xs">RN-48</code>,{' '}
        <code className="tw:font-mono tw:text-xs">DP-001</code>): es un espejo, y corregir un
        nombramiento se hace en ARGOS. Lo que <b>sí</b> es de SIGTI son las competencias — ARGOS
        no sabe qué es despachar un vehículo.
      </Nota>

      <div className="tw:grid tw:gap-3 tw:sm:grid-cols-3">
        <Recuento cantidad={todos.length} texto="puestos con competencias" />
        <Recuento
          cantidad={vacantes.length}
          texto="vacantes"
          ayuda="El puesto existe aunque esté vacío, y se configura antes del nombramiento."
        />
        <Recuento
          cantidad={coocupados.length}
          texto="en traspaso"
          ayuda="Dos personas en el mismo puesto a la vez. Cada acto queda a nombre de quien lo hizo."
        />
      </div>

      <CampoBusqueda
        etiqueta="Buscar por puesto, persona o rol…"
        valor={filtro}
        onCambio={setFiltro}
      />

      {isPending ? (
        <p className="tw:text-sm tw:text-tinta-mid">Cargando el padrón…</p>
      ) : filas.length === 0 ? (
        <Vacio
          icono={<Users />}
          titulo={filtro ? 'Ningún puesto coincide' : 'No hay puestos con competencias'}
          descripcion={
            filtro
              ? 'Pruebe con el identificador completo, o limpie la búsqueda.'
              : 'El espejo del organigrama se puebla por la integración con ARGOS.'
          }
        />
      ) : (
        <div className="tw:flex tw:flex-col tw:gap-3">
          {filas.map((p) => (
            <FichaDePuesto
              key={p.puesto}
              puesto={p}
              onOtorgar={() => setAOtorgar(p)}
              onVerPersona={setAVer}
            />
          ))}
        </div>
      )}

      {aOtorgar && (
        <DialogoDeOtorgamiento puesto={aOtorgar} onCerrar={() => setAOtorgar(null)} />
      )}

      {aVer !== null && <DialogoDePersona persona={aVer} onCerrar={() => setAVer(null)} />}
    </div>
  );
}

function Recuento({
  cantidad,
  texto,
  ayuda,
}: {
  cantidad: number;
  texto: string;
  ayuda?: string;
}): ReactElement {
  return (
    <div className="tw:flex tw:flex-col tw:gap-1 tw:rounded-panel tw:border tw:border-linea tw:bg-panel tw:p-3">
      <span className="tw:text-2xl tw:font-semibold tw:tabular-nums">{cantidad}</span>
      <span className="tw:text-sm">{texto}</span>
      {ayuda !== undefined && <span className="tw:text-xs tw:text-tinta-mid">{ayuda}</span>}
    </div>
  );
}

function FichaDePuesto({
  puesto,
  onOtorgar,
  onVerPersona,
}: {
  puesto: PuestoDelPadron;
  onOtorgar(): void;
  onVerPersona(persona: string): void;
}): ReactElement {
  const cliente = useQueryClient();

  const cierre = useMutation({
    mutationFn: (id: string) => cerrarCompetencia(id, new Date().toISOString().slice(0, 10)),
    onSuccess: async () => {
      avisar.exito('Competencia cerrada. Queda en el historial, no se borró.');
      await cliente.invalidateQueries({ queryKey: ['padron-puestos'] });
    },
    onError: (e) =>
      avisar.error(e instanceof BloqueoDuro ? e.paraMostrar : 'No se pudo cerrar la competencia.'),
  });

  const vigilada = puesto.competencias.find((c) => c.paresVigilados !== null);

  return (
    <Panel>
      <div className="tw:flex tw:flex-col tw:gap-3">
        <div className="tw:flex tw:flex-wrap tw:items-center tw:justify-between tw:gap-3">
          <div className="tw:flex tw:flex-col tw:gap-1">
            <span className="tw:font-mono tw:text-sm tw:font-medium">{puesto.puesto}</span>

            <div className="tw:flex tw:flex-wrap tw:items-center tw:gap-2 tw:text-xs">
              {puesto.vacante ? (
                // Vacante NO es un hueco: el puesto existe aunque esté vacío.
                <Pastilla tono="neutro">Vacante</Pastilla>
              ) : (
                puesto.ocupantes.map((o) => (
                  <button
                    key={o}
                    type="button"
                    onClick={() => onVerPersona(o)}
                    className="loki-foco tw:font-mono tw:text-tinta-mid tw:underline-offset-2 tw:hover:underline"
                  >
                    {o}
                  </button>
                ))
              )}

              {/* La coocupación es acotada y se registra: el traspaso real dura días. */}
              {puesto.ocupantes.length > 1 && <Pastilla tono="info">En traspaso</Pastilla>}

              {vigilada && <Pastilla tono="aviso">Acumulación vigilada</Pastilla>}
            </div>
          </div>

          <Boton variante="secundario" tamano="sm" onClick={onOtorgar}>
            Otorgar rol
          </Boton>
        </div>

        <ul className="tw:flex tw:flex-col tw:gap-2">
          {puesto.competencias.map((c) => (
            <li
              key={c.id}
              className="tw:flex tw:flex-wrap tw:items-baseline tw:justify-between tw:gap-2 tw:border-l-2 tw:border-linea tw:pl-3"
            >
              <div className="tw:flex tw:flex-col tw:gap-0.5">
                <span className="tw:text-sm">{TEXTO_DE_ROL[c.rol] ?? c.rol}</span>
                <span className="tw:text-xs tw:text-tinta-mid">
                  alcance {c.alcance.toLowerCase()} — {TEXTO_DE_ALCANCE[c.alcance] ?? ''} · desde
                  el {soloFecha(c.desde)} · otorgó {c.otorga}
                </span>
                {c.paresVigilados !== null && (
                  <span className="tw:text-xs tw:text-aviso-fg">
                    pares vigilados: {c.paresVigilados}
                  </span>
                )}
              </div>

              <Boton
                variante="fantasma"
                tamano="sm"
                disabled={cierre.isPending}
                onClick={() => cierre.mutate(c.id)}
              >
                Cerrar
              </Boton>
            </li>
          ))}
        </ul>

        {/* Las dos se dicen por separado: una es historia, la otra es futuro. */}
        {puesto.cerradas > 0 && (
          <span className="tw:text-xs tw:text-tinta-mid">
            {puesto.cerradas} competencia(s) cerrada(s). <b>No se borraron</b>: un acto de
            febrero se juzga con la competencia vigente en febrero.
          </span>
        )}

        {puesto.futuras > 0 && (
          <span className="tw:text-xs tw:text-info-fg">
            {puesto.futuras} competencia(s) <b>todavía no empiezan</b>. No es que se hayan
            quitado: rigen desde una fecha posterior a hoy.
          </span>
        )}
      </div>
    </Panel>
  );
}

/**
 * `PT-097` — el otorgamiento, con su control preventivo.
 *
 * ── Lo que el diálogo tiene que dejar claro antes de apretar ────────────────
 * Que hay **dos resultados distintos y sólo uno es un no**: `I-12` e `I-13` rechazan la
 * asignación; todo lo demás la deja pasar y la marca. Presentarlos igual haría que quien
 * asigna leyera cualquier advertencia como un rechazo, y dejaría de asignar.
 */
function DialogoDeOtorgamiento({
  puesto,
  onCerrar,
}: {
  puesto: PuestoDelPadron;
  onCerrar(): void;
}): ReactElement {
  const cliente = useQueryClient();
  const [rol, setRol] = useState<Rol | ''>('');
  const [alcance, setAlcance] = useState<AlcanceDeDatos>('Dependencia');
  const [desde, setDesde] = useState(() => new Date().toISOString().slice(0, 10));

  const catalogo = useQuery({ queryKey: ['catalogo-organizacion'], queryFn: catalogoDeOrganizacion });

  const operacion = useMutation({
    mutationFn: () =>
      otorgarCompetencia({
        id: nuevoUlid(),
        puesto: puesto.puesto,
        rol: rol as Rol,
        alcance,
        desde,
        hasta: null,
        otorga: 'P-ADMIN',
      }),
    onSuccess: async (r) => {
      if (r.quedaVigilada) {
        // Gana a la enhorabuena: la asignación se hizo Y produjo acumulación vigilada, y las
        // dos cosas son ciertas a la vez.
        avisar.alerta(
          `Otorgado, y el puesto queda de acumulación vigilada: ${r.vigilados
            .map((v) => v.par)
            .join(', ')}.`,
          {
            duracion: 15_000,
            detalle:
              'No es un error. El bloqueo llega al ejecutar el acto sobre un expediente concreto.',
          },
        );
      } else {
        avisar.exito(`${TEXTO_DE_ROL[rol] ?? rol} otorgado a ${puesto.puesto}.`);
      }

      await cliente.invalidateQueries({ queryKey: ['padron-puestos'] });
      onCerrar();
    },
    onError: (e) => {
      // `I-12` o `I-13`. El mensaje nombra el par y por qué existe: un mensaje genérico
      // produce una llamada a soporte, uno preciso produce la acción correcta.
      if (e instanceof BloqueoDuro) {
        avisar.error(e.paraMostrar);
        return;
      }

      avisar.error('No se pudo otorgar el rol. Nada quedó guardado.');
    },
  });

  const yaTiene = puesto.competencias.map((c) => c.rol);

  return (
    <Modal
      abierto
      titulo={`Otorgar un rol a ${puesto.puesto}`}
      descripcion={
        puesto.vacante
          ? 'El puesto está vacante, y se configura igual: existe aunque esté vacío, y esperar al nombramiento obligaría a configurarlo con prisa ese día.'
          : `Lo ocupa ${puesto.ocupantes.join(' y ')}. La incompatibilidad se evalúa sobre la persona y no sobre el puesto, así que cuenta todo lo que ya acumula por otros puestos.`
      }
      onCerrar={onCerrar}
      acciones={
        <Boton
          variante="primario"
          disabled={rol === '' || operacion.isPending}
          cargando={operacion.isPending}
          onClick={() => operacion.mutate()}
        >
          Otorgar
        </Boton>
      }
    >
      <div className="tw:flex tw:flex-col tw:gap-4">
        <Campo
          etiqueta="Rol"
          obligatorio
          ayuda="Los ACT-xx de actores-y-roles.md. Cada uno trae las funciones que la segregación compara."
        >
          {(control) => (
            <select
              {...control}
              value={rol}
              onChange={(e) => setRol(e.target.value as Rol)}
            >
              <option value="">Elija un rol…</option>
              {(catalogo.data?.roles ?? []).map((r) => (
                <option key={r.rol} value={r.rol} disabled={yaTiene.includes(r.rol)}>
                  {TEXTO_DE_ROL[r.rol] ?? r.rol}
                  {yaTiene.includes(r.rol) ? ' — ya lo tiene' : ''}
                </option>
              ))}
            </select>
          )}
        </Campo>

        {/* Las funciones del rol elegido: es lo que la tabla de §5.2 compara, y verlo antes
            explica por qué una asignación va a quedar vigilada. */}
        {rol !== '' && (
          <Nota tono="info">
            Este rol ejerce:{' '}
            <b>
              {(catalogo.data?.roles.find((r) => r.rol === rol)?.funciones ?? []).join(', ') ||
                'ninguna función del expediente'}
            </b>
            . La segregación compara funciones, no roles.
          </Nota>
        )}

        <Campo
          etiqueta="Alcance de datos"
          obligatorio
          ayuda="Se otorga acá y no en el rol: el mismo ACT-04 tiene alcance institución si el puesto es de sede y delegación si es regional."
        >
          {(control) => (
            <select
              {...control}
              value={alcance}
              onChange={(e) => setAlcance(e.target.value as AlcanceDeDatos)}
            >
              {(catalogo.data?.alcances ?? []).map((a) => (
                <option key={a} value={a}>
                  {a} — {TEXTO_DE_ALCANCE[a] ?? ''}
                </option>
              ))}
            </select>
          )}
        </Campo>

        <Campo etiqueta="Rige desde" obligatorio>
          {(control) => (
            <CampoFecha
              id={control.id}
              valor={desde}
              onCambiar={setDesde}
              etiqueta="Rige desde"
            />
          )}
        </Campo>

        <Nota tono="aviso" icono={<ShieldAlert />}>
          <b>Dos resultados posibles, y sólo uno es un no.</b> Si la acumulación activa{' '}
          <code className="tw:font-mono tw:text-xs">I-12</code> o{' '}
          <code className="tw:font-mono tw:text-xs">I-13</code> —auditor con rol ejecutor, o
          administrador con facultad de autorizar o aprobar fondos— <b>se rechaza</b>: son del
          núcleo irreductible y no se levantan por nada. Cualquier otra acumulación{' '}
          <b>se otorga y queda vigilada</b>.
        </Nota>
      </div>
    </Modal>
  );
}

/**
 * Lo que una persona acumula, sumando todos sus puestos.
 *
 * <b>Es la vista que hace visible el problema de §5.4.</b> Una delegación de tres personas no
 * cumple la segregación *«por aritmética, no por falta de voluntad»*, y esta pantalla lo
 * muestra en vez de disimularlo.
 */
function DialogoDePersona({
  persona,
  onCerrar,
}: {
  persona: string;
  onCerrar(): void;
}): ReactElement {
  const { data, isPending, isError } = useQuery({
    queryKey: ['competencias-persona', persona],
    queryFn: () => competenciasDe(persona),
  });

  return (
    <Modal
      abierto
      titulo={`Lo que acumula ${persona}`}
      descripcion="Los permisos efectivos son la unión de los roles de todos los puestos que ocupa. Las incompatibilidades se evalúan acá, sobre la persona: mirarlas puesto por puesto es como se cuela la acumulación."
      onCerrar={onCerrar}
      etiquetaCerrar="Cerrar"
      acciones={null}
    >
      {isPending ? (
        <p className="tw:text-sm tw:text-tinta-mid">Resolviendo…</p>
      ) : isError ? (
        <Nota tono="aviso">No se pudo resolver la competencia de esta persona.</Nota>
      ) : (
        <div className="tw:flex tw:flex-col tw:gap-4">
          <div className="tw:flex tw:flex-col tw:gap-1">
            <span className="tw:text-xs tw:uppercase tw:tracking-wide tw:text-tinta-mid">
              Puestos que ocupa
            </span>
            <span className="tw:font-mono tw:text-sm">{data.puestos.join(', ') || '—'}</span>
          </div>

          <div className="tw:flex tw:flex-col tw:gap-1">
            <span className="tw:text-xs tw:uppercase tw:tracking-wide tw:text-tinta-mid">
              Funciones acumuladas
            </span>
            <div className="tw:flex tw:flex-wrap tw:gap-1.5">
              {data.funciones.map((f) => (
                <Pastilla key={f} tono="info">
                  {f}
                </Pastilla>
              ))}
            </div>
          </div>

          <div className="tw:flex tw:flex-col tw:gap-1">
            <span className="tw:text-xs tw:uppercase tw:tracking-wide tw:text-tinta-mid">
              Alcance de datos
            </span>
            {/* Nulo es «no tiene alcance», que no es `Propio`: `Propio` ya es un permiso. */}
            <span className="tw:text-sm">
              {data.alcanceMaximo === null ? (
                <span className="tw:italic tw:text-tinta-mid">
                  sin ningún alcance — no ocupa puesto con competencia vigente
                </span>
              ) : (
                `${data.alcanceMaximo} — ${TEXTO_DE_ALCANCE[data.alcanceMaximo] ?? ''}`
              )}
            </span>
          </div>

          {data.vigilados.length > 0 && (
            <Panel titulo={`${data.vigilados.length} par(es) incompatible(s) acumulado(s)`}>
              <div className="tw:flex tw:flex-col tw:gap-3">
                <p className="tw:text-xs tw:text-tinta-mid">
                  <b>Ninguno impide operar hoy.</b> Bloquean al ejecutar el acto sobre un
                  expediente concreto: la misma persona no puede solicitar y autorizar{' '}
                  <i>la misma misión</i>. Sobre misiones distintas, sí.
                </p>

                {data.vigilados.map((v) => (
                  <div
                    key={`${v.par}-${v.una}-${v.otra}`}
                    className={`tw:flex tw:flex-col tw:gap-0.5 tw:border-l-2 tw:pl-3 ${
                      v.nivel === 'NucleoIrreductible'
                        ? 'tw:border-riesgo-fg'
                        : 'tw:border-aviso-fg'
                    }`}
                  >
                    <div className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-2 tw:text-sm">
                      <span className="tw:font-mono tw:font-medium">{v.par}</span>
                      <span>
                        {v.una} × {v.otra}
                      </span>
                      {v.nivel === 'NucleoIrreductible' && (
                        <Pastilla tono="riesgo">Núcleo irreductible</Pastilla>
                      )}
                    </div>
                    <span className="tw:text-xs tw:text-tinta-mid">{v.porQue}</span>
                  </div>
                ))}
              </div>
            </Panel>
          )}

          {/* La aritmética de §5.1, dicha donde se ve el caso. */}
          {data.vigilados.length >= 10 && (
            <Nota tono="riesgo" icono={<CircleAlert />}>
              <b>Esta persona concentra las cinco funciones que el MARCI exige separadas.</b>{' '}
              Cumplir la segregación completa <b>exige cinco personas distintas por misión</b>, y
              una delegación de tres no puede cumplirla localmente — por aritmética, no por falta
              de voluntad. Lo que corresponde es el <b>escalamiento a sede</b>, no la excepción
              local.
            </Nota>
          )}
        </div>
      )}
    </Modal>
  );
}
