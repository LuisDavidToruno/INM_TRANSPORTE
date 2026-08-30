import type { ReactElement } from 'react';
import { useState } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { Layers, TriangleAlert } from 'lucide-react';

import { Boton, Campo, Nota, Panel, Segmentado } from '../../ui';
import { BloqueoDuro, pedir } from '../../api/misiones';
import { avisar } from '../../ui/avisos';
import { usarQuienEjecuta } from '../../app/puesto';

/**
 * `PT-055` — Resolución por lote <b>con criterio declarado</b>.
 *
 * ── Por qué el lote existe ──────────────────────────────────────────────────
 * <i>«Cientos de conflictos tras semanas sin sincronizar. Resolver de a uno miles de conflictos
 * es inviable»</i> — un dispositivo que vuelve de nueve días sin señal trae 180 registros, y
 * pedirle a alguien que decida 180 veces garantiza que no decida ninguna.
 *
 * ── Y por qué exige escribir el criterio ────────────────────────────────────
 * <i>«Hacerlo sin declarar el criterio <b>es sobrescritura con más pasos</b>.»</i> Un botón de
 * «aceptar todo» sin motivo es exactamente lo que `RN-45` prohíbe, con una pantalla de por medio.
 *
 * ── Lo que el lote nunca toca ───────────────────────────────────────────────
 * Kilometraje, monto y autorización quedan fuera <b>siempre</b>, y el resultado los enumera. Son
 * los tres que terminan en una conciliación contable: resolverlos en bloque con una regla
 * general destruye el término de una conciliación de auditoría sin que nadie se entere.
 */
export default function ResolverPorLote({
  expediente,
  cuantos,
}: {
  readonly expediente: string;
  readonly cuantos: number;
}): ReactElement {
  const cliente = useQueryClient();
  const quienEjecuta = usarQuienEjecuta();

  const [abierto, setAbierto] = useState(false);
  const [criterio, setCriterio] = useState('');
  const [seToma, setSeToma] = useState<'Campo' | 'Servidor'>('Campo');
  const [fuera, setFuera] = useState<FueraDelLote[] | null>(null);

  const lote = useMutation({
    mutationFn: () =>
      pedir<Resultado>('/conflictos/lote', {
        method: 'POST',
        body: JSON.stringify({
          expediente,
          seToma,
          criterio,
          resuelve: quienEjecuta,
          momento: new Date().toISOString(),
        }),
      }),
    onSuccess: async (r) => {
      // ⚠️ **Se dice cuántos quedaron fuera aunque sean cero.** Un lote que informa «resueltos:
      // 12» y calla las exclusiones hace creer que la cola quedó limpia, y los que frenan
      // liquidaciones siguen ahí sin que nadie los mire.
      setFuera(r.fueraDelLote);
      setAbierto(false);
      setCriterio('');

      avisar.exito(
        r.fueraDelLote.length === 0
          ? `${r.resueltos} resueltos. No quedó ninguno fuera del lote.`
          : `${r.resueltos} resueltos. ${r.fueraDelLote.length} quedan fuera y se deciden uno por uno.`,
      );

      await cliente.invalidateQueries({ queryKey: ['conflictos'] });
    },
    onError: (e) =>
      avisar.error(e instanceof BloqueoDuro ? e.paraMostrar : 'No se pudo resolver el lote.'),
  });

  return (
    <Panel titulo={`Decidir varios a la vez · ${cuantos} de esta misión`}>
      <div className="tw:flex tw:flex-col tw:gap-3">
        {!abierto ? (
          <>
            <p className="tw:text-sm tw:text-tinta-mid">
              Cuando un equipo vuelve de varios días sin señal trae muchos registros. Se pueden
              decidir juntos <b>declarando con qué criterio</b>.
            </p>
            <div>
              <Boton variante="secundario" icono={<Layers />} onClick={() => setAbierto(true)}>
                Decidir varios con un mismo criterio
              </Boton>
            </div>
          </>
        ) : (
          <>
            <Nota tono="aviso" icono={<TriangleAlert />}>
              <b>Los kilometrajes, montos y autorizaciones no entran en esto.</b> Son los que
              terminan en una conciliación, y se deciden uno por uno aunque estén en la misma
              misión.
            </Nota>

            <Campo etiqueta="Cuál versión se toma">
              {() => (
                <Segmentado
                  etiqueta="Cuál versión se toma"
                  valor={seToma}
                  onCambio={(v) => setSeToma(v as 'Campo' | 'Servidor')}
                  opciones={[
                    { valor: 'Campo', etiqueta: 'La que llegó del campo' },
                    { valor: 'Servidor', etiqueta: 'La que tenía la oficina' },
                  ]}
                />
              )}
            </Campo>

            <Campo
              etiqueta="Con qué criterio"
              ayuda="Por ejemplo: «aceptar la versión de campo para todos los registros de esta misión». Queda escrito en cada registro que se resuelva."
            >
              {(control) => (
                <textarea
                  {...control}
                  value={criterio}
                  onChange={(e) => setCriterio(e.target.value)}
                  rows={2}
                  className="loki-foco tw:w-full tw:rounded-control tw:border tw:border-linea tw:bg-panel tw:px-2 tw:py-1.5 tw:text-cuerpo-2"
                />
              )}
            </Campo>

            <div className="tw:flex tw:flex-wrap tw:gap-2">
              <Boton
                variante="primario"
                cargando={lote.isPending}
                onClick={() => lote.mutate()}
              >
                Aplicar el criterio
              </Boton>
              <Boton variante="fantasma" onClick={() => setAbierto(false)}>
                Cancelar
              </Boton>
            </div>
          </>
        )}

        {/* Lo excluido, después de aplicar. Se enumera siempre. */}
        {fuera !== null && (
          <Nota tono={fuera.length === 0 ? 'ok' : 'riesgo'}>
            {fuera.length === 0 ? (
              <>Ninguno quedó fuera del criterio.</>
            ) : (
              <>
                <b>
                  {fuera.length === 1
                    ? '1 registro queda fuera y se decide uno por uno'
                    : `${fuera.length} registros quedan fuera y se deciden uno por uno`}
                </b>
                <ul className="tw:mt-1 tw:flex tw:flex-col tw:gap-1 tw:pl-4">
                  {fuera.map((f) => (
                    <li key={f.id} className="tw:list-disc tw:text-xs">
                      {f.campo} — {f.porQue}
                    </li>
                  ))}
                </ul>
              </>
            )}
          </Nota>
        )}
      </div>
    </Panel>
  );
}

interface FueraDelLote {
  id: string;
  campo: string;
  porQue: string;
}

interface Resultado {
  resueltos: number;
  /** **Se enumera siempre**, aunque esté vacío. */
  fueraDelLote: FueraDelLote[];
}
