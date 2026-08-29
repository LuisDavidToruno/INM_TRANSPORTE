import type { ReactElement } from 'react';
import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { CircleAlert, Scale } from 'lucide-react';

import { Campo, CampoFecha, Nota, Panel, Pastilla } from '../../ui';
import {
  CLASE_EN_PALABRAS,
  CLASE_QUE_PIDE,
  QUE_DICE_LA_NORMA,
  matrizDeLicencias,
} from '../../api/motoristas';
import type { FilaDeMatriz, MatrizDeLicencias } from '../../api/motoristas';
import { soloFecha } from '../M06_Autorizacion/formato';

/**
 * `PT-084` — Qué tipos de vehículo habilita cada categoría de licencia.
 *
 * ── La respuesta viene del servidor, y no es negociable ─────────────────────
 * Esta matriz sostiene `BD-02`, que traslada responsabilidad legal directa a quien autoriza.
 * Derivarla en el cliente —aunque fuera «sólo para mostrar»— produciría **dos implementaciones
 * de la misma precondición**, y la de la pantalla sería la que nadie verifica. Acá sólo se
 * pinta lo que el servidor resolvió.
 *
 * ── Contra la flota real, no contra vehículos de muestra ────────────────────
 * La primera versión del endpoint evaluaba la matriz contra fichas técnicas inventadas y
 * **mintió**: declaró que `B1` y `C1` no habilitaban nada, cuando las dos tienen entrada — lo
 * que faltaba era un triciclo y un camión liviano entre los ejemplos. Contra la flota la
 * pregunta es la que de verdad se hace: *«con una licencia B, ¿cuáles de nuestras unidades
 * puedo conducir?»*.
 *
 * ── Y por eso una fila vacía se explica ─────────────────────────────────────
 * «Ninguno porque no tenemos autobuses» y «ninguno porque el umbral no alcanza» son cosas
 * distintas, y las dos llegan como lista vacía. Sin distinguirlas, la fila se lee como que la
 * categoría no sirve para nada.
 */
export default function Matriz(): ReactElement {
  const [fecha, setFecha] = useState(() => new Date().toISOString().slice(0, 10));

  const { data, isPending, isError } = useQuery({
    queryKey: ['matriz-licencias', fecha],
    queryFn: () => matrizDeLicencias(fecha),
  });

  if (isError) {
    return (
      <Nota tono="riesgo" icono={<CircleAlert />}>
        No se pudo cargar la matriz de licencias.
      </Nota>
    );
  }

  return (
    <div className="tw:flex tw:flex-col tw:gap-5">
      <header className="tw:flex tw:flex-col tw:gap-1">
        <h1 className="tw:text-xl tw:font-semibold tw:tracking-tight">
          Matriz licencia ↔ vehículo
        </h1>
        <p className="tw:text-sm tw:text-tinta-mid">
          Qué unidades de la flota habilita cada categoría. <b>Nueve categorías</b> —
          <code className="tw:font-mono tw:text-xs">BE</code> es <b>B enganchada a remolque</b>, y{' '}
          <b>no existe ninguna DE</b>.
        </p>
      </header>

      <Panel titulo="A qué fecha">
        <div className="tw:flex tw:flex-col tw:gap-3 tw:sm:max-w-md">
          <Campo
            etiqueta="Fecha del hecho"
            ayuda="La matriz es parámetro con vigencia. Preguntar qué habilita la B sin decir cuándo no tiene una sola respuesta: manda la tabla vigente a la fecha del hecho, no a la de hoy."
          >
            {(control) => (
              <CampoFecha
                id={control.id}
                valor={fecha}
                onCambiar={setFecha}
                etiqueta="Fecha del hecho"
              />
            )}
          </Campo>

          {data !== undefined && (
            <p className="tw:text-xs tw:text-tinta-mid">
              Respondió la versión{' '}
              <span className="tw:font-mono">{data.version}</span>, vigente al{' '}
              {soloFecha(data.fecha)}, contra las {data.vehiculosEnLaFlota} unidades de la flota.
            </p>
          )}
        </div>
      </Panel>

      {/* `[V]` Artículo 4 del Acuerdo 1012-2021. El nivel no sube al bajar de abstracción. */}
      <Nota tono="info" icono={<Scale />}>
        Las nueve categorías son del <b>Artículo 4 del Acuerdo 1012-2021</b>{' '}
        <code className="tw:font-mono tw:text-xs">[V]</code>. Lo que se muestra abajo{' '}
        <b>lo resuelve el servidor</b> contra esa matriz: esta pantalla no la interpreta, porque
        dos lecturas de la regla que traslada responsabilidad legal son la peor duplicación
        posible de este sistema.
      </Nota>

      {isPending ? (
        <p className="tw:text-sm tw:text-tinta-mid">Resolviendo la matriz…</p>
      ) : (
        <div className="tw:flex tw:flex-col tw:gap-3">
          {data.categorias.map((c) => (
            <FilaCategoria key={c.categoria} fila={c} matriz={data} />
          ))}
        </div>
      )}
    </div>
  );
}

function FilaCategoria({
  fila,
  matriz,
}: {
  fila: FilaDeMatriz;
  matriz: MatrizDeLicencias;
}): ReactElement {
  const habilitaAlgo = fila.habilita.length > 0;

  return (
    <Panel>
      <div className="tw:flex tw:flex-col tw:gap-2">
        <div className="tw:flex tw:flex-wrap tw:items-center tw:gap-3">
          <span className="tw:rounded-control tw:border tw:border-linea tw:px-2 tw:py-0.5 tw:font-mono tw:text-sm tw:font-semibold">
            {fila.categoria}
          </span>

          <span className="tw:text-sm tw:text-tinta-mid">
            {QUE_DICE_LA_NORMA[fila.categoria] ?? 'El Acuerdo no la describe acá.'}
          </span>
        </div>

        {habilitaAlgo ? (
          <div className="tw:flex tw:flex-wrap tw:gap-2">
            {fila.habilita.map((v) => (
              <Pastilla key={v.id} tono="ok">
                {v.siglas} · {v.tipo}
              </Pastilla>
            ))}
          </div>
        ) : (
          <SinHabilitar categoria={fila.categoria} matriz={matriz} />
        )}
      </div>
    </Panel>
  );
}

/**
 * La fila vacía, explicada.
 *
 * <b>«No tenemos ninguno de esa clase» y «los que tenemos no pasan el umbral» piden cosas
 * distintas.</b> Lo primero se resuelve comprando o dando de alta un vehículo; lo segundo es la
 * norma funcionando. Un «ninguno» a secas los junta y hace parecer inútil a la categoría.
 */
function SinHabilitar({
  categoria,
  matriz,
}: {
  categoria: string;
  matriz: MatrizDeLicencias;
}): ReactElement {
  const clase = CLASE_QUE_PIDE[categoria];
  const hayDeEsaClase = clase !== undefined && matriz.clasesEnLaFlota.includes(clase);

  if (clase !== undefined && !hayDeEsaClase) {
    return (
      <p className="tw:text-sm tw:text-tinta-mid">
        Ninguna unidad —<b>la flota no tiene {CLASE_EN_PALABRAS[clase] ?? clase}</b>. No es que
        la categoría no habilite: es que no hay contra qué habilitarla.
      </p>
    );
  }

  return (
    <p className="tw:text-sm tw:text-tinta-mid">
      Ninguna unidad de la flota. <b>Sí hay vehículos de esa clase</b>, así que lo que no alcanza
      es el umbral de la categoría —peso, pasajeros o remolque—, y eso es la norma operando.
    </p>
  );
}
