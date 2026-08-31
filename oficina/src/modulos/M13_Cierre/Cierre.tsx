import type { ReactElement } from 'react';
import { useState } from "react";
import { useParams } from "react-router";
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { CircleAlert, CircleCheck, CircleHelp, TriangleAlert } from 'lucide-react';

import { Boton, Campo, Enlace, Nota, Panel, Pastilla, avisar } from '../../ui';
import {
  BloqueoDuro,
  cerrar,
  devolverLiquidacion,
  expediente,
  propuestaDeCierre,
} from '../../api/misiones';
import type { CriterioEvaluado, EslabonDeLaCadena } from '../../api/misiones';
import { ROTULO_ESTADO } from '../../dominio/mision';

import PanelDeVales from '../M09_Combustible/PanelDeVales';
import PanelDeAbastecimientos from '../M09_Combustible/PanelDeAbastecimientos';
import PanelDeCoherencia from '../M18_Peajes/PanelDeCoherencia';
import PanelDeHallazgos from '../M14_Auditoria/PanelDeHallazgos';
import { soloFecha } from '../M06_Autorizacion/formato';

/**
 * Cierre del expediente — `T-21` y `T-22`, que **son un solo acto**.
 *
 * ── La decisión que esta pantalla NO ofrece ──────────────────────────────────
 * No hay dos botones. No hay «cerrar limpio» y «cerrar con hallazgo». `§7.2` de
 * la máquina de estados es explícito: *«quien cierra no elige entre cerrar limpio
 * o con hallazgo, el criterio decide y él lo confirma con su justificación»*.
 *
 * Si hubiera dos botones, en seis meses nadie usaría el segundo, y
 * `CERRADA_CON_HALLAZGO` dejaría de significar algo — que es exactamente lo
 * contrario de lo que ese estado existe para lograr.
 *
 * ── Lo que sí ofrece ─────────────────────────────────────────────────────────
 * Ver **qué se detectó** antes de firmar, y declarar **qué se hizo con ello**.
 * Y una salida que no es cerrar: devolver la liquidación para rehacerla (`T-20`),
 * porque la alternativa a devolver un descargo mal hecho es cerrarlo mal.
 */
export default function Cierre(): ReactElement {
  const { id = "" } = useParams();
  const clienteDeConsultas = useQueryClient();
  const [justificacion, setJustificacion] = useState('');
  const [motivoDevolucion, setMotivoDevolucion] = useState('');

  // ⚠️ **La propuesta la hace el servidor.** La detección vivía acá y evaluaba uno de los
  // trece criterios; el cierre mandaba esa lista, así que la precondición de `T-21` la
  // declaraba la pantalla. Ahora sale de la misma evaluación que el cierre va a usar: si se
  // calculara acá, se mostraría una cosa y se registraría otra.
  const propuesta = useQuery({
    queryKey: ['propuesta-de-cierre', id],
    queryFn: () => propuestaDeCierre(id),
  });

  const { data, isPending, isError, error } = useQuery({
    queryKey: ['expediente', id],
    queryFn: () => expediente(id),
  });

  const cierre = useMutation({
    mutationFn: () =>
      cerrar(id, 'P-GERENCIA', justificacion.trim() || null),
    onSuccess: () => {
      avisar.exito('Expediente cerrado.');
      void clienteDeConsultas.invalidateQueries({ queryKey: ['cola-cierre'] });
      void clienteDeConsultas.invalidateQueries({ queryKey: ['expediente', id] });
    },
    onError: (e) =>
      avisar.error(
        e instanceof BloqueoDuro
          ? `${e.precondicion} — ${e.message}`
          : e instanceof Error
            ? e.message
            : 'No se pudo cerrar.',
      ),
  });

  const devolucion = useMutation({
    mutationFn: () => devolverLiquidacion(id, 'P-GERENCIA', motivoDevolucion.trim()),
    onSuccess: () => {
      avisar.exito('Liquidación devuelta para rehacerla.');
      void clienteDeConsultas.invalidateQueries({ queryKey: ['cola-cierre'] });
      void clienteDeConsultas.invalidateQueries({ queryKey: ['expediente', id] });
    },
    onError: (e) => avisar.error(e instanceof Error ? e.message : 'No se pudo devolver.'),
  });

  if (isError) {
    return (
      <Nota tono="riesgo" icono={<CircleAlert />}>
        No se pudo cargar el expediente:{' '}
        {error instanceof Error ? error.message : 'error desconocido'}.
      </Nota>
    );
  }

  if (isPending || !data) {
    return <Nota tono="info">Cargando el expediente…</Nota>;
  }

  // El expediente puede haber dejado `LIQUIDADA` mientras esta pantalla estaba abierta —
  // porque acabamos de cerrarlo, o porque otra persona lo cerró desde su propia sesión.
  // Sin esto, la pantalla seguiría ofreciendo un botón que el servidor ya rechazaría, y el
  // usuario descubriría el estado real por un error en vez de por lo que ve.
  if (data.estado !== 'Liquidada') {
    return (
      <div className="tw:flex tw:flex-col tw:gap-5">
        <header className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-4 tw:gap-y-1">
          <h1 className="tw:font-mono tw:text-xl tw:font-semibold tw:tabular-nums tw:tracking-tight">
            {data.folio}
          </h1>
          <Pastilla tono={ROTULO_ESTADO[data.estado].tono}>
            {ROTULO_ESTADO[data.estado].texto}
          </Pastilla>
        </header>

        <Nota tono="ok" icono={<CircleCheck />}>
          <div className="tw:flex tw:flex-col tw:gap-2">
            <p className="tw:font-medium">Este expediente ya no está esperando cierre.</p>
            <p className="tw:text-sm">
              Su estado es <b>{ROTULO_ESTADO[data.estado].texto}</b>.{' '}
              {data.estado === 'Cerrada' || data.estado === 'CerradaConHallazgo'
                ? 'El expediente es inmutable: lo que haya que corregir se corrige por asiento nuevo, nunca reabriéndolo.'
                : 'Volvió a un estado anterior — probablemente su liquidación fue devuelta para rehacerla.'}
            </p>
          </div>
        </Nota>

        <p className="tw:text-sm">
          <Enlace href="/cierre">Volver a la cola de cierre</Enlace>
        </p>
      </div>
    );
  }

  const cumplidos = (propuesta.data?.criterios ?? []).filter((c) => c.resultado === 'SeCumple');
  const sinVerificar = (propuesta.data?.criterios ?? []).filter(
    (c) => c.resultado === 'NoVerificado',
  );

  const hayHallazgo = cumplidos.length > 0;

  // ⚠️ **Mientras la propuesta no llegó, no se cierra.** «Todavía no cargó» no es «no hay
  // criterios», y habilitar el botón antes ofrecería cerrar limpio sin saberlo.
  //
  // `T-22` exige justificación; `T-21` no la pide, y pedirla sería ruido.
  const puedeCerrar =
    propuesta.data !== undefined && (!hayHallazgo || justificacion.trim().length > 0);

  return (
    <div className="tw:flex tw:flex-col tw:gap-5">
      <header className="tw:flex tw:flex-col tw:gap-2">
        <div className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-4 tw:gap-y-1">
          <h1 className="tw:font-mono tw:text-xl tw:font-semibold tw:tabular-nums tw:tracking-tight">
            {data.folio}
          </h1>
          <Pastilla tono="info">Liquidada</Pastilla>
        </div>
        <p className="tw:text-sm tw:text-tinta-mid">
          {data.objetoDelTraslado} · destino {data.destino} · retorno{' '}
          {soloFecha(data.retornoPrevisto)}
        </p>
      </header>

      {/* Va ANTES del pronunciamiento: lo que impide cerrar tiene que verse antes que el
          botón de cerrar, no después de que el servidor lo rechace. */}
      <PanelDeVales misionId={id} estadoDeLaMision={data.estado} />

      {/* El numerador de la conciliación, al lado de los vales. Sin esta lista, `RN-30`
          puede señalar un rendimiento imposible y nadie ve por qué: con la composición
          delante, «900 km con 20 galones» deja de ser una acusación y pasa a ser una suma
          incompleta que alguien puede completar. */}
      <PanelDeAbastecimientos
        misionId={id}
        vehiculoId={
          [...data.diario].reverse().find((t) => t.vehiculoTomado)?.vehiculoTomado ??
          undefined
        }
      />

      {/* El cruce de `RN-37` va antes del dictamen de cierre porque es insumo suyo: un peaje
          fuera de la ruta autorizada es un hallazgo que quien cierra tiene que haber visto
          antes de decidir. `NRM-10`: es lo que busca el auditor del TSC -- correlacion, no
          comprobantes archivados. */}
      <PanelDeCoherencia mision={id} />

      {/* §7.5: la mision cerrada muestra que tiene hallazgos posteriores vinculados. Se
          consulta desde el hallazgo y no se guarda una marca aca -- guardar algo en un
          expediente cerrado seria modificarlo, que es justo lo que la inmutabilidad
          prohibe. */}
      <PanelDeHallazgos mision={id} />

      {/* ── ⚠️ La lista de verificación de la cadena ─────────────────────
          `RN-08` la manda presentar al liquidador eslabón por eslabón, y va ANTES del
          pronunciamiento porque es su insumo: `H-09` sale de esta misma lista. El auditor
          del TSC no pide comprobantes sueltos — pide recorrer la cadena de una punta a la
          otra sobre un expediente concreto. */}
      {propuesta.data?.cadena != null && (
        <Panel titulo="Cadena de trazabilidad">
          <ul className="tw:flex tw:flex-col tw:gap-2">
            {propuesta.data.cadena.eslabones.map((e) => (
              <ItemDeEslabon key={e.eslabon} eslabon={e} />
            ))}
          </ul>
        </Panel>
      )}

      <Panel
        titulo={
          // «Todavia no cargo» no es «cierra limpio». Anunciarlo antes de saberlo es la
          // afirmacion que esta pantalla existe para no hacer.
          propuesta.data === undefined
            ? 'Evaluando los criterios de cierre'
            : hayHallazgo
              ? 'Este expediente cierra con hallazgo'
              : 'Este expediente cierra limpio'
        }
      >
        <div className="tw:flex tw:flex-col tw:gap-5">
          {/* ⚠️ **Mientras la propuesta no llegó no se afirma nada.** «Todavía no cargó» no es
              «no se cumplió ninguno»: el visto verde sobre cero criterios evaluados es
              exactamente la afirmación que esta pantalla existe para no hacer. */}
          {propuesta.data === undefined ? (
            <p className="tw:text-sm tw:text-tinta-mid">Evaluando los criterios de cierre…</p>
          ) : hayHallazgo ? (
            <>
              <Nota tono="aviso" icono={<TriangleAlert />}>
                <div className="tw:flex tw:flex-col tw:gap-2">
                  <p className="tw:font-medium">
                    Se cumplieron{' '}
                    {cumplidos.length === 1 ? 'un criterio' : `${cumplidos.length} criterios`} de
                    cierre con hallazgo.
                  </p>
                  <p className="tw:text-sm">
                    <b>Esto no es una elección suya y no imputa responsabilidad a nadie.</b> El
                    criterio lo decide el sistema; lo que usted declara es qué se hizo con él.
                    Un vehículo robado en ruta produce hallazgo y nadie es culpable.
                  </p>
                </div>
              </Nota>

              <ul className="tw:flex tw:flex-col tw:gap-2">
                {cumplidos.map((c) => (
                  <ItemDeCriterio key={c.criterio} criterio={c} />
                ))}
              </ul>

              <Campo
                etiqueta="Qué se hizo con el hallazgo"
                obligatorio
                ayuda="Va impreso en el expediente y es lo que el control interno va a seguir. «Se revisó» no dice nada."
              >
                {(props) => (
                  <textarea
                    {...props}
                    rows={3}
                    value={justificacion}
                    onChange={(e) => setJustificacion(e.target.value)}
                  />
                )}
              </Campo>
            </>
          ) : (
            <Nota tono="ok" icono={<CircleCheck />}>
              <p>
                No se cumplió ninguno de los{' '}
                <b>{propuesta.data?.verificados ?? 0} criterios que el sistema evalúa</b>.
              </p>
            </Nota>
          )}

          {/* ── ⚠️ Lo que nadie miró ─────────────────────────────────────────
              Se muestra SIEMPRE, y sobre todo cuando el expediente cierra limpio: es justo
              entonces cuando ocultarlo haría creer que se verificaron trece cosas.

              Y no produce hallazgo: marcar el expediente por lo que el sistema todavía no
              sabe mirar acusaría a la institución de una conducta que nadie constató. */}
          {sinVerificar.length > 0 && (
            <div className="tw:flex tw:flex-col tw:gap-3">
              <Nota tono="aviso" icono={<CircleHelp />}>
                <b>
                  {sinVerificar.length} de {propuesta.data?.criterios.length} criterios no se
                  pudieron verificar.
                </b>{' '}
                No son criterios limpios: son criterios que nadie miró. Cerrarlo los deja
                declarados en el expediente, con lo que a cada uno le falta.
              </Nota>

              <details>
                <summary className="tw:cursor-pointer tw:text-sm tw:text-tinta-mid">
                  Ver cuáles y qué les falta
                </summary>
                <ul className="tw:mt-2 tw:flex tw:flex-col tw:gap-2">
                  {sinVerificar.map((c) => (
                    <ItemDeCriterio key={c.criterio} criterio={c} />
                  ))}
                </ul>
              </details>
            </div>
          )}

          <Boton
            variante="primario"
            cargando={cierre.isPending}
            disabled={cierre.isPending || !puedeCerrar}
            onClick={() => cierre.mutate()}
          >
            {hayHallazgo ? 'Cerrar con hallazgo' : 'Cerrar el expediente'}
          </Boton>
        </div>
      </Panel>

      <Panel titulo="O devolver la liquidación">
        <div className="tw:flex tw:flex-col tw:gap-4">
          <p className="tw:text-sm">
            Si el descargo conciliado está mal elaborado, <b>devuélvalo</b>. La alternativa a
            devolver un descargo mal hecho es cerrarlo mal, y un expediente cerrado ya no se
            corrige: se revierte.
          </p>

          <Campo
            etiqueta="Qué hay que corregir"
            obligatorio
            ayuda="Quien lo rehaga solo tiene esto para saber qué mirar."
          >
            {(props) => (
              <textarea
                {...props}
                rows={3}
                value={motivoDevolucion}
                onChange={(e) => setMotivoDevolucion(e.target.value)}
              />
            )}
          </Campo>

          <Boton
            variante="secundario"
            cargando={devolucion.isPending}
            disabled={devolucion.isPending || motivoDevolucion.trim().length === 0}
            onClick={() => devolucion.mutate()}
          >
            Devolver para rehacer
          </Boton>
        </div>
      </Panel>
    </div>
  );
}

/**
 * Un eslabón de la cadena, con el estado que el sistema le encontró.
 *
 * ── Por qué cada estado tiene su propio tono ────────────────────────────────
 * Son cuatro cosas distintas y se leen de un vistazo o no se leen: <b>presente</b> está,
 * <b>ausente</b> falta y es hallazgo, <b>no aplica</b> no corresponde —con su fundamento al
 * lado, que es lo que lo separa de una omisión—, y <b>en camino</b> impide cerrar sin
 * reprochar nada.
 */
function ItemDeEslabon({ eslabon }: { eslabon: EslabonDeLaCadena }): ReactElement {
  const { tono, rotulo } = ROTULO_ESLABON[eslabon.estado];

  return (
    <li className="tw:flex tw:flex-col tw:gap-0.5 tw:rounded tw:border tw:border-linea tw:px-3 tw:py-2">
      <span className="tw:flex tw:flex-wrap tw:items-center tw:gap-2">
        <b className="tw:text-sm">{eslabon.nombre}</b>
        <Pastilla tono={tono}>{rotulo}</Pastilla>
      </span>
      <span className="tw:text-xs tw:text-tinta-mid">{eslabon.detalle}</span>
    </li>
  );
}

/**
 * ⚠️ El estado se decide SIEMPRE por su identificador y nunca por su texto — regla del
 * vocabulario compartido. Y «no aplica» no lleva tono de riesgo: no es una falta.
 */
const ROTULO_ESLABON: Record<
  EslabonDeLaCadena['estado'],
  { tono: 'ok' | 'riesgo' | 'neutro' | 'aviso'; rotulo: string }
> = {
  Presente: { tono: 'ok', rotulo: 'presente' },
  Ausente: { tono: 'riesgo', rotulo: 'falta' },
  NoAplicable: { tono: 'neutro', rotulo: 'no aplica' },
  PendienteDeSincronizacion: { tono: 'aviso', rotulo: 'en camino' },
};

/**
 * Un criterio `H-nn` con lo que el sistema contestó sobre él.
 *
 * ── Por qué el enunciado y el detalle van los dos ───────────────────────────
 * El enunciado dice <b>qué se preguntó</b> y el detalle <b>qué se encontró</b> — o, en los que
 * nadie pudo mirar, <b>qué falta para poder mirarlos</b>. Con sólo el identificador, quien lee
 * el expediente dentro de dos años tiene que ir a buscar §7.2 para saber qué significaba `H-04`.
 */
function ItemDeCriterio({ criterio }: { criterio: CriterioEvaluado }): ReactElement {
  return (
    <li className="tw:flex tw:flex-col tw:gap-0.5 tw:rounded tw:border tw:border-linea tw:px-3 tw:py-2">
      <span className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-2">
        <span className="tw:font-mono tw:text-xs tw:text-tinta-mid">{criterio.criterio}</span>
        <span className="tw:text-xs tw:text-tinta-mid">{criterio.enunciado}</span>
      </span>
      <span className="tw:text-sm">{criterio.detalle}</span>
    </li>
  );
}
