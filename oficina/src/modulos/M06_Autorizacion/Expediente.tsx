import type { ReactElement } from 'react';
import { useState } from 'react';
import { useNavigate, useParams } from 'react-router';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { CircleAlert, CircleCheck, ShieldBan, TriangleAlert } from 'lucide-react';

import {
  Boton,
  Campo,
  Modal,
  Nota,
  Panel,
  Pastilla,
  RastreadorEtapas,
  avisar,
} from '../../ui';
import {
  BloqueoDuro,
  autorizar,
  catalogoDeMotivosDeRechazo,
  devolverParaCorreccion,
  expediente as traerExpediente,
  rechazar,
} from '../../api/misiones';
import {
  ETAPAS_DE_MISION,
  ROTULO_ESTADO,
  advertencias,
  hayBloqueo,
  indiceDeEtapa,
} from '../../dominio/mision';
import type { Expediente as ExpedienteDto, Validacion } from '../../dominio/mision';
import { laDependencia, momentoCompleto } from './formato';

/**
 * `PT-014` — El expediente en decisión, en una sola pantalla.
 *
 * «En una sola pantalla» es el requisito, no una preferencia: si la jefatura
 * tiene que navegar para ver las validaciones, va a autorizar sin verlas.
 *
 * ── El orden de la página es el orden de la decisión ─────────────────────────
 * Primero qué se pide, después qué dice el sistema al respecto, y sólo entonces
 * la acción. Poner el botón arriba invita a resolver sin leer.
 */
export default function Expediente(): ReactElement {
  const { id = '' } = useParams();
  const navegar = useNavigate();
  const cliente = useQueryClient();
  const [motivo, setMotivo] = useState('');
  const [acusadas, setAcusadas] = useState<Set<string>>(new Set());
  const [negativo, setNegativo] = useState<'rechazar' | 'devolver' | null>(null);

  const { data, isPending, isError } = useQuery({
    queryKey: ['expediente', id],
    queryFn: () => traerExpediente(id),
  });

  const autorizacion = useMutation({
    mutationFn: () => autorizar(id, 'Rolando Discua', motivo || undefined),
    onSuccess: async () => {
      avisar.exito('Expediente autorizado. La constancia quedó en el diario.');
      await cliente.invalidateQueries({ queryKey: ['bandeja-autorizacion'] });
      navegar('/autorizacion');
    },
    onError: (e) => {
      if (e instanceof BloqueoDuro) {
        avisar.error(`${e.precondicion} — ${e.message}`);
        return;
      }
      avisar.error('No se pudo autorizar. El expediente quedó como estaba.');
    },
  });

  // Los motivos de rechazo se PIDEN, no se cablean: el catálogo es configurable por la
  // institución (`HU-014`, insumo #1), y una lista duplicada acá sería una lista que se
  // separa de la que el servidor valida — y el rechazo fallaría al guardar, no al elegir.
  const motivosDeRechazo = useQuery({
    queryKey: ['motivos-de-rechazo'],
    queryFn: catalogoDeMotivosDeRechazo,
    enabled: negativo === 'rechazar',
  });

  if (isPending) return <Cargando />;
  if (isError || !data) {
    return (
      <Nota tono="riesgo" icono={<CircleAlert />}>
        No se encontró el expediente. Puede que otra persona lo haya anulado mientras esta pantalla
        estaba abierta.
      </Nota>
    );
  }

  const bloqueado = hayBloqueo(data.validaciones);
  const avisos = advertencias(data.validaciones);
  const faltanAcuses = avisos.filter((a) => !acusadas.has(a.regla));
  const exigeMotivo = avisos.length > 0;

  return (
    <div className="tw:flex tw:flex-col tw:gap-6">
      <Cabecera expediente={data} />

      <RastreadorEtapas etapas={ETAPAS_DE_MISION} etapaActual={indiceDeEtapa(data.estado)} />

      <div className="tw:grid tw:gap-5 tw:lg:grid-cols-[minmax(0,1fr)_360px]">
        <div className="tw:flex tw:flex-col tw:gap-5">
          <Panel titulo="Qué se solicita">
            <Datos expediente={data} />
          </Panel>

          <Panel titulo="Qué dice el sistema">
            <div className="tw:flex tw:flex-col tw:gap-3">
              {data.validaciones.map((v) => (
                <ValidacionVista
                  key={v.regla}
                  validacion={v}
                  acusada={acusadas.has(v.regla)}
                  onAcusar={() =>
                    setAcusadas((previas) => {
                      const siguiente = new Set(previas);
                      siguiente.has(v.regla) ? siguiente.delete(v.regla) : siguiente.add(v.regla);
                      return siguiente;
                    })
                  }
                />
              ))}
            </div>
          </Panel>

          <Panel titulo="Diario del expediente">
            <Diario transiciones={data.diario} />
          </Panel>
        </div>

        <Decision
          bloqueado={bloqueado}
          faltanAcuses={faltanAcuses.length}
          exigeMotivo={exigeMotivo}
          onRechazar={() => setNegativo('rechazar')}
          onDevolver={() => setNegativo('devolver')}
          motivo={motivo}
          onMotivo={setMotivo}
          enviando={autorizacion.isPending}
          onAutorizar={() => autorizacion.mutate()}
        />
      </div>

      {negativo && (
        <PronunciamientoNegativo
          clase={negativo}
          expediente={data}
          motivos={motivosDeRechazo.data ?? []}
          cargandoMotivos={motivosDeRechazo.isPending}
          onCerrar={() => setNegativo(null)}
        />
      )}
    </div>
  );
}

function Cabecera({ expediente }: { expediente: ExpedienteDto }): ReactElement {
  const rotulo = ROTULO_ESTADO[expediente.estado];

  return (
    <header className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-4 tw:gap-y-2">
      <h1 className="tw:font-mono tw:text-xl tw:font-semibold tw:tabular-nums tw:tracking-tight">
        {expediente.folio}
      </h1>
      <Pastilla tono={rotulo.tono}>{rotulo.texto}</Pastilla>
      <p className="tw:text-sm tw:text-tinta-mid">
        {expediente.dependencia} · a nombre de {expediente.solicitanteDeDerecho}
      </p>
    </header>
  );
}

function Datos({ expediente }: { expediente: ExpedienteDto }): ReactElement {
  return (
    <dl className="tw:grid tw:gap-x-8 tw:gap-y-4 tw:sm:grid-cols-2">
      <Dato termino="Objeto del traslado" valor={expediente.objetoDelTraslado} />
      <Dato termino="Destino" valor={expediente.destino} />
      <Dato termino="Salida prevista" valor={momentoCompleto(expediente.salidaPrevista)} />
      <Dato termino="Retorno previsto" valor={momentoCompleto(expediente.retornoPrevisto)} />
      <Dato termino="Capturada por" valor={expediente.capturadaPor} />
      <Dato termino="Solicitante de derecho" valor={expediente.solicitanteDeDerecho} />
    </dl>
  );
}

function Dato({ termino, valor }: { termino: string; valor: string }): ReactElement {
  return (
    <div className="tw:flex tw:flex-col tw:gap-0.5">
      <dt className="tw:text-xs tw:text-tinta-mid">{termino}</dt>
      <dd className="tw:text-sm">{valor}</dd>
    </div>
  );
}

/**
 * Las tres clases se distinguen por forma, no sólo por color: el 8 % de los
 * hombres no distingue rojo de verde, y esta pantalla decide sobre dinero público.
 */
function ValidacionVista({
  validacion,
  acusada,
  onAcusar,
}: {
  validacion: Validacion;
  acusada: boolean;
  onAcusar(): void;
}): ReactElement {
  const { clase, regla, titulo, detalle } = validacion;

  if (clase === 'conforme') {
    return (
      <div className="tw:flex tw:gap-2.5 tw:text-sm">
        <CircleCheck size={16} className="tw:mt-px tw:shrink-0 tw:text-ok-fg" aria-hidden />
        <div className="tw:flex tw:flex-col tw:gap-0.5">
          <p>
            <span className="tw:font-mono tw:text-xs tw:text-tinta-mid">{regla}</span> {titulo}
          </p>
          <p className="tw:text-xs tw:text-tinta-mid">{detalle}</p>
        </div>
      </div>
    );
  }

  const esBloqueo = clase === 'bloqueo';

  return (
    <Nota tono={esBloqueo ? 'riesgo' : 'aviso'} icono={esBloqueo ? <ShieldBan /> : <TriangleAlert />}>
      <div className="tw:flex tw:flex-col tw:gap-2">
        <p className="tw:font-medium">
          <span className="tw:font-mono tw:text-xs tw:opacity-80">{regla}</span> {titulo}
        </p>
        <p className="tw:text-sm">{detalle}</p>

        {!esBloqueo && (
          <label className="tw:mt-1 tw:flex tw:cursor-pointer tw:items-start tw:gap-2 tw:text-sm">
            <input
              type="checkbox"
              aria-label="Doy por leído este aviso"
              checked={acusada}
              onChange={onAcusar}
              className="tw:mt-0.5 tw:size-4 tw:shrink-0 tw:accent-acento"
            />
            <span>
              Doy por leído este aviso y asumo la decisión con este dato a la vista.
            </span>
          </label>
        )}
      </div>
    </Nota>
  );
}

function Diario({ transiciones }: { transiciones: ExpedienteDto['diario'] }): ReactElement {
  return (
    <ol className="tw:flex tw:flex-col tw:gap-3">
      {transiciones.map((t) => (
        <li key={`${t.id}-${t.momento}`} className="tw:flex tw:gap-3 tw:text-sm">
          <span className="tw:font-mono tw:text-xs tw:text-tinta-mid tw:tabular-nums">{t.id}</span>
          <div className="tw:flex tw:flex-col tw:gap-0.5">
            <span>
              {ROTULO_ESTADO[t.destino].texto} · {t.ejecuta}
            </span>
            <span className="tw:text-xs tw:text-tinta-mid tw:tabular-nums">
              {momentoCompleto(t.momento)}
            </span>
            {t.motivo && <span className="tw:text-xs tw:text-tinta-mid">Motivo: {t.motivo}</span>}
          </div>
        </li>
      ))}
    </ol>
  );
}

/**
 * ── Por qué el botón sigue existiendo con advertencias sin acusar ────────────
 * Porque `RN-50` prohíbe retirarlo, y `HU-009` lo dice literal: «en ningún momento
 * retira ni oculta la acción de autorizar». Lo que hace la pantalla es **pedir el
 * acuse**, no esconder la salida. Un botón que desaparece se lee como sistema roto
 * y termina en una llamada a soporte; un botón que explica qué falta se resuelve solo.
 *
 * El bloqueo duro sí lo deshabilita: ahí la acción de verdad no existe, y el
 * servidor la rechazaría igual.
 */
/**
 * El pronunciamiento negativo — `T-06` rechazar y `T-04` devolver.
 *
 * ── Un diálogo para las dos, y no dos pantallas ─────────────────────────────
 * Porque la decisión es la misma —decirle que no a esta solicitud— y lo que cambia es
 * cuánto. Separarlas obligaría a elegir la pantalla antes de saber cuál corresponde, que
 * es exactamente al revés de como se piensa: primero se ve qué falta, después se decide
 * si eso se arregla o no.
 *
 * ── Y por qué el motivo se comporta distinto en cada una ────────────────────
 * Rechazar exige **catálogo + texto**: el catálogo dice qué se cuenta y el texto dice a la
 * dependencia qué pasó. Devolver exige **sólo texto**: no se está midiendo por qué se dijo
 * que no —no se dijo—, se está diciendo qué falta, y un catálogo no puede enumerar lo que
 * falta en un expediente concreto.
 */
function PronunciamientoNegativo({
  clase,
  expediente,
  motivos,
  cargandoMotivos,
  onCerrar,
}: {
  clase: 'rechazar' | 'devolver';
  expediente: ExpedienteDto;
  motivos: string[];
  cargandoMotivos: boolean;
  onCerrar(): void;
}): ReactElement {
  const navegar = useNavigate();
  const cliente = useQueryClient();
  const [motivo, setMotivo] = useState('');
  const [comentario, setComentario] = useState('');

  const esRechazo = clase === 'rechazar';

  const operacion = useMutation({
    mutationFn: () =>
      esRechazo
        ? rechazar(expediente.id, 'Rolando Discua', motivo, comentario)
        : devolverParaCorreccion(expediente.id, 'Rolando Discua', comentario),
    onSuccess: async () => {
      avisar.exito(
        esRechazo
          ? `${expediente.folio} rechazada. La negativa queda en el diario y no se reabre.`
          : `${expediente.folio} volvió a ${laDependencia(expediente.dependencia)} para corrección.`,
      );
      await cliente.invalidateQueries({ queryKey: ['bandeja-autorizacion'] });
      navegar('/autorizacion');
    },
    onError: (e) => {
      if (e instanceof BloqueoDuro) {
        avisar.error(`${e.precondicion} — ${e.message}`);
        return;
      }
      avisar.error('No se pudo. El expediente quedó como estaba.');
    },
  });

  // Rechazar necesita las dos cosas; devolver, sólo el texto.
  const listo = comentario.trim().length >= 8 && (!esRechazo || motivo !== '');

  return (
    <Modal
      abierto
      titulo={esRechazo ? `Rechazar ${expediente.folio}` : `Devolver ${expediente.folio} para corrección`}
      descripcion={
        esRechazo
          ? `El rechazo NO se deshace: de rechazada no sale ninguna transición. Si la solicitud es arreglable, use «Devolver para corrección» — vuelve a ${laDependencia(expediente.dependencia)}, se corrige y se reenvía sin perder el expediente.`
          : `El expediente vuelve a ${laDependencia(expediente.dependencia)} como borrador. Conserva su identidad: al reenviarlo es el mismo expediente corregido, no uno nuevo.`
      }
      destructivo={esRechazo}
      onCerrar={onCerrar}
      acciones={
        <Boton
          variante={esRechazo ? 'peligro' : 'primario'}
          disabled={!listo || operacion.isPending}
          cargando={operacion.isPending}
          onClick={() => operacion.mutate()}
        >
          {esRechazo ? 'Rechazar expediente' : 'Devolver para corrección'}
        </Boton>
      }
    >
      <div className="tw:flex tw:flex-col tw:gap-4">
        {esRechazo && (
          <fieldset className="tw:flex tw:flex-col tw:gap-2">
            <legend className="tw:mb-1 tw:text-sm tw:font-medium">Motivo del catálogo</legend>

            {cargandoMotivos ? (
              <p className="tw:text-sm tw:text-tinta-mid">Cargando los motivos…</p>
            ) : motivos.length === 0 ? (
              // Un catálogo vacío hace IMPOSIBLE rechazar, y hay que decirlo: sin esto,
              // el botón queda inerte y nadie sabe por qué.
              <Nota tono="aviso">
                No hay motivos de rechazo configurados. Sin catálogo no se puede rechazar —
                el texto libre complementa el motivo tipificado, no lo sustituye.
              </Nota>
            ) : (
              motivos.map((m) => (
                <label
                  key={m}
                  className="tw:flex tw:cursor-pointer tw:items-start tw:gap-2 tw:text-sm"
                >
                  <input
                    type="radio"
                    name="motivo-rechazo"
                    checked={motivo === m}
                    onChange={() => setMotivo(m)}
                    className="tw:mt-0.5 tw:size-4 tw:shrink-0 tw:accent-acento"
                  />
                  <span>{m}</span>
                </label>
              ))
            )}
          </fieldset>
        )}

        <Campo
          etiqueta={esRechazo ? 'Qué decirle a la dependencia' : 'Qué hay que corregir'}
          obligatorio
          ayuda={
            esRechazo
              ? 'Lo lee quien pidió el viaje. El motivo del catálogo dice qué se cuenta; esto le dice si vale la pena replantearlo.'
              : 'Lo lee quien capturó la solicitud, y es lo único que tiene para saber qué arreglar antes de reenviarla.'
          }
        >
          {(props) => (
            <textarea
              {...props}
              rows={3}
              value={comentario}
              onChange={(e) => setComentario(e.target.value)}
              className="loki-input"
            />
          )}
        </Campo>
      </div>
    </Modal>
  );
}

function Decision({
  bloqueado,
  faltanAcuses,
  exigeMotivo,
  motivo,
  onMotivo,
  enviando,
  onAutorizar,
  onRechazar,
  onDevolver,
}: {
  bloqueado: boolean;
  faltanAcuses: number;
  exigeMotivo: boolean;
  motivo: string;
  onMotivo(v: string): void;
  enviando: boolean;
  onAutorizar(): void;
  onRechazar(): void;
  onDevolver(): void;
}): ReactElement {
  const faltaMotivo = exigeMotivo && motivo.trim().length < 8;
  const impedido = bloqueado || faltanAcuses > 0 || faltaMotivo;

  return (
    <aside className="tw:lg:sticky tw:lg:top-4 tw:lg:self-start">
      <Panel titulo="Su pronunciamiento">
        <div className="tw:flex tw:flex-col tw:gap-4">
          {bloqueado ? (
            <p className="tw:text-sm tw:text-tinta-mid">
              Este expediente no lo puede autorizar usted. Escálelo al nivel inmediato superior
              — el sistema deja constancia de que llegó acá y de por qué no siguió.
            </p>
          ) : (
            exigeMotivo && (
              <Campo
                etiqueta="Motivo de la autorización"
                obligatorio
                ayuda="Queda en el diario y se imprime en la orden de misión. Escriba lo que un auditor necesitaría leer dentro de dos años."
              >
                {(props) => (
                  <textarea
                    {...props}
                    rows={4}
                    value={motivo}
                    onChange={(e) => onMotivo(e.target.value)}
                    className="loki-input"
                  />
                )}
              </Campo>
            )
          )}

          <Boton
            variante="primario"
            disabled={impedido || enviando}
            cargando={enviando}
            onClick={onAutorizar}
          >
            Autorizar expediente
          </Boton>

          {!bloqueado && impedido && (
            <p className="tw:text-xs tw:text-tinta-mid">
              {faltanAcuses > 0
                ? faltanAcuses === 1
                  ? 'Queda 1 aviso sin dar por leído.'
                  : `Quedan ${faltanAcuses} avisos sin dar por leídos.`
                : 'Escriba el motivo para continuar.'}
            </p>
          )}

          {/* Las dos salidas negativas, y en este orden. Devolver es reversible —la
              solicitud vuelve a quien la capturó y puede reenviarse—; rechazar es
              terminal. Con el mismo peso visual, se usa la irreversible cuando bastaba
              la otra, y la dependencia tiene que volver a pedir el viaje desde cero.

              Ninguna de las dos se deshabilita por bloqueo: `BD-01` impide AUTORIZAR y
              rechazar, pero si el bloqueo es de competencia, devolver tampoco procede y
              el servidor lo dirá. Se ofrecen y se explica, en vez de esconderlas. */}
          <Boton variante="secundario" disabled={enviando} onClick={onDevolver}>
            Devolver para corrección
          </Boton>

          <Boton variante="fantasma" disabled={enviando} onClick={onRechazar}>
            Rechazar con motivo
          </Boton>
        </div>
      </Panel>
    </aside>
  );
}

function Cargando(): ReactElement {
  return (
    <div className="tw:flex tw:flex-col tw:gap-6" aria-busy="true" aria-live="polite">
      <div className="tw:h-7 tw:w-48 tw:animate-pulse tw:rounded tw:bg-subtle" />
      <div className="tw:h-16 tw:animate-pulse tw:rounded tw:bg-subtle" />
      <div className="tw:grid tw:gap-5 tw:lg:grid-cols-[minmax(0,1fr)_360px]">
        <div className="tw:h-64 tw:animate-pulse tw:rounded tw:bg-subtle" />
        <div className="tw:h-48 tw:animate-pulse tw:rounded tw:bg-subtle" />
      </div>
      <span className="tw:sr-only">Cargando el expediente…</span>
    </div>
  );
}
