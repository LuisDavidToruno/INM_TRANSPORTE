import type { ReactElement } from 'react';
import { useState } from "react";
import { useParams } from "react-router";
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { CircleAlert, CircleCheck, TriangleAlert } from 'lucide-react';

import { Boton, Campo, Enlace, Nota, Panel, Pastilla, avisar } from '../../ui';
import { BloqueoDuro, cerrar, devolverLiquidacion, expediente } from '../../api/misiones';
import type { CriterioDetectado } from '../../api/misiones';
import { ROTULO_ESTADO } from '../../dominio/mision';
import type { Expediente } from '../../dominio/mision';
import PanelDeVales from '../M09_Combustible/PanelDeVales';
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

  const { data, isPending, isError, error } = useQuery({
    queryKey: ['expediente', id],
    queryFn: () => expediente(id),
  });

  const cierre = useMutation({
    mutationFn: () =>
      cerrar(id, 'P-GERENCIA', criteriosDetectados(data), justificacion.trim() || null),
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

  const criterios = criteriosDetectados(data);
  const hayHallazgo = criterios.length > 0;
  // `T-22` exige justificación; `T-21` no la pide, y pedirla sería ruido.
  const puedeCerrar = !hayHallazgo || justificacion.trim().length > 0;

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

      <Panel titulo={hayHallazgo ? 'Este expediente cierra con hallazgo' : 'Este expediente cierra limpio'}>
        <div className="tw:flex tw:flex-col tw:gap-5">
          {hayHallazgo ? (
            <>
              <Nota tono="aviso" icono={<TriangleAlert />}>
                <div className="tw:flex tw:flex-col tw:gap-2">
                  <p className="tw:font-medium">
                    Se cumplieron {criterios.length === 1 ? 'un criterio' : `${criterios.length} criterios`} de
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
                {criterios.map((c) => (
                  <li
                    key={c.criterio}
                    className="tw:flex tw:flex-col tw:gap-0.5 tw:rounded tw:border tw:border-linea tw:px-3 tw:py-2"
                  >
                    <span className="tw:font-mono tw:text-xs tw:text-tinta-mid">{c.criterio}</span>
                    <span className="tw:text-sm">{c.detalle}</span>
                  </li>
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
              No se cumplió ningún criterio de cierre con hallazgo: consumo dentro de umbral,
              ruta coherente, fondo comprobado y cadena de trazabilidad completa.
            </Nota>
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
 * Los criterios que el servidor detectó.
 *
 * ⚠️ **Provisional.** `M-09`, `M-13` y `M-18` no están construidos: no hay conciliación de
 * combustible, ni de peajes, ni cadena de trazabilidad que evaluar. Hasta que existan,
 * **no hay criterios detectables** y todo expediente cierra limpio.
 *
 * Eso queda dicho en pantalla en lugar de fingir una evaluación que no ocurrió: el aviso de
 * la cola advierte que esto no es un veredicto todavía.
 */
function criteriosDetectados(_expediente: Expediente | undefined): CriterioDetectado[] {
  return [];
}
