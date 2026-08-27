import type { ReactElement } from 'react';

import { ETAPA } from './tipos';


/**
 * Rastreador de las ocho etapas del flujo.
 * Traducido de `.etapas` / `.et` de `referencia/argos5/ui.css`.
 *
 * ── Por qué existe teniendo una pastilla de estado ───────────────────────────
 * El estado del flujo era una pastilla, y una pastilla no dice CUÁNTO QUEDA. Acá
 * la cadena entera se ve de un vistazo: qué pasó, dónde está y qué falta.
 *
 * ── Los nombres NO se ponen en esqueleto ─────────────────────────────────────
 * Las ocho etapas son nuestras, no del servidor: se pintan siempre. Lo único que
 * espera al dato es SABER EN CUÁL ESTÁ. Pintarlas en gris fingiría una
 * ignorancia que no tenemos y obligaría a esperar para saber dónde mirar.
 *
 * ── Los estados se distinguen por FORMA además de por color ──────────────────
 * `hecho` es un disco lleno, `actual` un anillo con punto y halo, `devuelto` un
 * anillo ámbar hueco. En una fotocopia en blanco y negro —el tema `gris` existe
 * para eso— el color desaparece y la forma sigue ahí.
 */

export type EstadoEtapa = 'hecho' | 'actual' | 'devuelto' | 'pendiente';

/** Una etapa de un flujo cualquiera. El componente no sabe de qué flujo. */
export interface Etapa {
  id: number;
  nombre: string;
  nota: string;
}

/**
 * Las ocho de ARGOS, que siguen siendo el valor por omisión para no romper la
 * vitrina. SIGTI pasa las suyas: son nueve y se llaman distinto.
 *
 * DIVERGENCIA con el contrato 0.3.3: el componente cableaba estas ocho. Se
 * parametrizó porque el flujo es del dominio, no del sistema de diseño — es lo
 * que la propia plantilla predica cuando dice que el menú es DATO, no marcado.
 */
const ETAPAS_ARGOS: readonly Etapa[] = [
  { id: ETAPA.SOLICITANTE, nombre: 'Solicitante', nota: 'creada' },
  { id: ETAPA.GERENTE, nombre: 'Aprobación Gerente Solicitante', nota: 'visto bueno' },
  { id: ETAPA.REV_VIATICOS, nombre: 'Revisión Inicial Viáticos', nota: 'cálculo y zonas' },
  { id: ETAPA.PRESUPUESTO, nombre: 'Revisión Presupuesto', nota: 'partida' },
  { id: ETAPA.LEGAL, nombre: 'Revisión Legal', nota: '—' },
  { id: ETAPA.DIRECTOR, nombre: 'Aprobación Director', nota: 'firma' },
  { id: ETAPA.PARALELO_6A, nombre: 'Procesamiento Paralelo 6a', nota: 'viáticos' },
  { id: ETAPA.CIERRE, nombre: 'Cierre y liquidación', nota: '5 días hábiles' },
];

export interface RastreadorEtapasProps {
  /** Las etapas del flujo. Sin esto se pintan las ocho de ARGOS. */
  etapas?: readonly Etapa[];
  etapaActual: number;
  /** Nodo en ámbar: la etapa desde la que se devolvió. */
  devueltaEn?: number;
  /** Los nombres SÍ se pintan; sólo el estado espera. */
  cargando?: boolean;
}

function estadoDe(
  id: number,
  actual: number,
  devuelta: number | undefined,
  cargando: boolean,
): EstadoEtapa {
  if (cargando) return 'pendiente';
  if (devuelta !== undefined && id === devuelta) return 'devuelto';
  if (id === actual) return 'actual';
  return id < actual ? 'hecho' : 'pendiente';
}

export default function RastreadorEtapas({
  etapas = ETAPAS_ARGOS,
  etapaActual,
  devueltaEn,
  cargando = false,
}: RastreadorEtapasProps): ReactElement {
  return (
    <ol className="tw:flex tw:items-start tw:overflow-x-auto tw:pb-0.5">
      {etapas.map((e) => {
        const estado = estadoDe(e.id, etapaActual, devueltaEn, cargando);
        return (
          <li
            key={e.id}
            data-e={estado}
            className="loki-et"
            aria-current={estado === 'actual' ? 'step' : undefined}
          >
            <i aria-hidden="true" />
            {/* El título ENVUELVE: las etiquetas reales son largas
                («Procesamiento Paralelo 6a») y recortarlas perdería el dato. */}
            <b>{e.nombre}</b>
            <em>{cargando ? '—' : e.nota}</em>
          </li>
        );
      })}
    </ol>
  );
}
