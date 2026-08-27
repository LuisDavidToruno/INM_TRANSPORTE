import type { ReactElement } from 'react';
import { useState } from 'react';
import { useNavigate, useParams } from 'react-router';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { CircleAlert, CircleCheck, ShieldBan, TriangleAlert } from 'lucide-react';

import { Boton, Campo, Nota, Panel, Pastilla, RastreadorEtapas, avisar } from '../../ui';
import { autorizar, expediente as traerExpediente } from '../../api/misiones';
import { BloqueoDuro } from '../../api/misiones';
import {
  ETAPAS_DE_MISION,
  ROTULO_ESTADO,
  advertencias,
  hayBloqueo,
  indiceDeEtapa,
} from '../../dominio/mision';
import type { Expediente as ExpedienteDto, Validacion } from '../../dominio/mision';
import { momentoCompleto } from './formato';

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
          motivo={motivo}
          onMotivo={setMotivo}
          enviando={autorizacion.isPending}
          onAutorizar={() => autorizacion.mutate()}
        />
      </div>
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
function Decision({
  bloqueado,
  faltanAcuses,
  exigeMotivo,
  motivo,
  onMotivo,
  enviando,
  onAutorizar,
}: {
  bloqueado: boolean;
  faltanAcuses: number;
  exigeMotivo: boolean;
  motivo: string;
  onMotivo(v: string): void;
  enviando: boolean;
  onAutorizar(): void;
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

          <Boton variante="fantasma" disabled={enviando}>
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
