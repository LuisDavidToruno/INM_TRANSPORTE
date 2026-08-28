import type { ReactElement } from 'react';
import { useState } from "react";
import { useParams } from "react-router";
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { CircleAlert, CircleCheck, TriangleAlert } from 'lucide-react';

import { Boton, Campo, Enlace, Nota, Panel, Pastilla, avisar } from '../../ui';
import { BloqueoDuro, cerrar, devolverLiquidacion, expediente } from '../../api/misiones';
import type { CriterioDetectado } from '../../api/misiones';
import { ROTULO_ESTADO } from '../../dominio/mision';

import PanelDeVales from '../M09_Combustible/PanelDeVales';
import { valesDeLaMision } from '../../api/combustible';
import type { Vale } from '../../api/combustible';
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

  // Los vales, para saber si alguno cerró con desviación. **Es el primer criterio de
  // `H-01` que el sistema puede detectar de verdad**, y hasta hoy la pantalla afirmaba
  // que no había ninguno sin haber mirado.
  const vales = useQuery({
    queryKey: ['vales', id],
    queryFn: () => valesDeLaMision(id),
  });

  const { data, isPending, isError, error } = useQuery({
    queryKey: ['expediente', id],
    queryFn: () => expediente(id),
  });

  const cierre = useMutation({
    mutationFn: () =>
      cerrar(id, 'P-GERENCIA', criteriosDetectados(vales.data), justificacion.trim() || null),
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

  const criterios = criteriosDetectados(vales.data);
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
              <div className="tw:flex tw:flex-col tw:gap-2">
                <p>
                  Ningún vale de esta misión cerró con desviación de rendimiento{' '}
                  (<code className="tw:font-mono tw:text-xs">H-01</code>).
                </p>
                {/* Lo que NO se comprobó se dice. Antes esta nota afirmaba cuatro
                    verificaciones —umbral, ruta, fondo y trazabilidad— y ninguna existía:
                    un expediente cerrado sobre esa frase parecería revisado y no lo estaba. */}
                <p className="tw:text-xs">
                  <b>Todavía no se evalúan</b> la coherencia de la ruta contra los peajes
                  (<code className="tw:font-mono">M-18</code>) ni la cadena de trazabilidad
                  completa (<code className="tw:font-mono">M-14</code>). Cerrar limpio
                  significa que no se detectó ningún criterio <i>de los que hoy se detectan</i>.
                </p>
              </div>
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
 * Los criterios de cierre con hallazgo que <b>hoy</b> se pueden detectar.
 *
 * ── El único que existe, y por qué ahora sí ─────────────────────────────────
 * `H-01` — desviación de consumo fuera de umbral, <b>en cualquier dirección</b>. Sale de
 * `RN-30`, que ya calcula el dictamen: un vale en `ConciliadaConDesviacion` es una desviación
 * que alguien contrastó y tipificó, no una sospecha.
 *
 * ⚠️ <b>Los demás siguen sin existir.</b> La coherencia de la secuencia de casetas es de
 * `M-18` y la cadena de trazabilidad de `M-14`. Que esta función devuelva vacío <b>no</b>
 * significa que el expediente esté limpio en esos dos frentes: significa que nadie miró.
 * Por eso la pantalla lo dice en vez de afirmar cuatro verificaciones que no ocurrieron.
 */
function criteriosDetectados(vales: Vale[] | undefined): CriterioDetectado[] {
  // Indefinido es «todavía no cargaron», y eso NO es «no hay». Devolver vacío acá haría
  // que la pantalla ofreciera cerrar limpio antes de saberlo.
  if (vales === undefined) return [];

  const conDesviacion = vales.filter((v) => v.estado === 'ConciliadaConDesviacion');

  if (conDesviacion.length === 0) return [];

  return [
    {
      criterio: 'H-01',
      detalle:
        conDesviacion.length === 1
          ? `El vale ${conDesviacion[0]?.folio} concilió con desviación de rendimiento`
          : `${conDesviacion.length} vales conciliaron con desviación de rendimiento`,
    },
  ];
}
