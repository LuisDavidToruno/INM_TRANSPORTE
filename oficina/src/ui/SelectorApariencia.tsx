import type { ReactElement } from 'react';

import { DENSIDADES, NOMBRES_TEMA, useTema } from '../tema/TemaProvider';
import type { DensidadId, TemaId } from '../tema/TemaProvider';
import Segmentado from './Segmentado';

/**
 * Panel de Apariencia: los seis temas y las dos densidades.
 * Traducido de `.temas` / `.tema` / `.seg` de `referencia/argos5/ui.css`.
 *
 * ── Es uno de los DOS sitios que pueden llamar a `useTema()` ─────────────────
 * El otro es el conmutador de la barra superior. Ningún componente más pregunta
 * por el tema: reciben `tono` o `variante` y el tema se resuelve en CSS. Esa
 * regla es la que hace que agregar un séptimo tema no toque un solo componente.
 *
 * ── Las cuatro muestras de color no son decoración ───────────────────────────
 * Son lienzo, acción, acento y riel — las cuatro decisiones que distinguen un
 * tema de otro. Con una sola muestra, `oscuro` y `navy` se verían iguales.
 *
 * ── La densidad NO cambia tamaños de letra ───────────────────────────────────
 * Sólo alturas de control, alto de fila, relleno y gap. Si además encogiera la
 * tipografía dejaría de ser una preferencia y sería un tema distinto.
 */

interface FichaTema {
  id: TemaId;
  nombre: string;
  para: string;
  /** lienzo · acción · acento · riel */
  muestras: [string, string, string, string];
}

/** Los valores son los de la vitrina del paquete, no una reinterpretación. */
const FICHAS: FichaTema[] = [
  { id: 'claro', nombre: 'Claro', para: 'Oficina, pantalla iluminada', muestras: ['#f4f6fa', '#0f2040', '#b8975b', '#0a1628'] },
  { id: 'oscuro', nombre: 'Oscuro', para: 'Turnos largos, poca luz', muestras: ['#0f111a', '#cba869', '#171b26', '#131418'] },
  { id: 'navy', nombre: 'Navy institucional', para: 'Sala y presentación', muestras: ['#0d1c33', '#cba869', '#132949', '#0a1628'] },
  { id: 'sepia', nombre: 'Sepia', para: 'Lectura prolongada de bitácoras', muestras: ['#f1e9db', '#0f2040', '#a8874b', '#2b2318'] },
  { id: 'consola', nombre: 'Consola', para: 'Monitoreo · casi-negro y cian', muestras: ['#06080c', '#5cc8f5', '#0c1016', '#141a23'] },
  { id: 'gris', nombre: 'Gris impresión', para: 'Destinado a PDF o fotocopia', muestras: ['#f5f5f5', '#1f1f1f', '#6b6b6b', '#1c1c1c'] },
];

const ETIQUETA_DENSIDAD: Record<DensidadId, { texto: string; fila: string }> = {
  comoda: { texto: 'Cómoda', fila: '44px' },
  compacta: { texto: 'Compacta', fila: '36px' },
};

export default function SelectorApariencia(): ReactElement {
  const { tema, densidad, setTema, setDensidad } = useTema();

  return (
    <div className="tw:flex tw:flex-col tw:gap-4">
      <div className="tw:flex tw:flex-wrap tw:items-start tw:justify-between tw:gap-3">
        <p className="tw:text-cuerpo-2 tw:text-tinta-low">
          El administrador habilita; el usuario elige. Se guarda en{' '}
          <code className="tw:font-mono tw:text-tinta-mid">localStorage</code> y se aplica antes
          del primer pintado.
        </p>

        {/* `.seg` — conmutador segmentado. Acá la nota NO es una cuenta sino la altura
            de fila: por eso el componente la llama `nota` y no `cuenta`. */}
        <Segmentado
          etiqueta="Densidad"
          valor={densidad}
          onCambio={setDensidad}
          className="tw:shrink-0"
          opciones={DENSIDADES.map((d) => ({
            valor: d,
            etiqueta: ETIQUETA_DENSIDAD[d].texto,
            nota: ETIQUETA_DENSIDAD[d].fila,
          }))}
        />
      </div>

      {/* `.temas` — rejilla que se llena sola; añadir un séptimo tema es una fila más */}
      <div className="loki-rejilla-temas">
        {FICHAS.filter((f) => (NOMBRES_TEMA as readonly string[]).includes(f.id)).map((f) => (
          <button
            key={f.id}
            type="button"
            aria-pressed={tema === f.id}
            onClick={() => setTema(f.id)}
            className={[
              'loki-tema tw:flex tw:items-start tw:gap-3 tw:rounded-panel tw:border tw:bg-panel tw:px-3.5 tw:py-[13px] tw:text-left',
              tema === f.id
                ? 'loki-tema-activo tw:border-acento-ink'
                : 'tw:border-linea tw:hover:border-linea-activa',
            ].join(' ')}
          >
            <span className="loki-muestra tw:shrink-0 tw:rounded-badge tw:border tw:border-linea">
              {f.muestras.map((c, i) => (
                <i key={i} style={{ background: c }} />
              ))}
            </span>
            <span className="tw:min-w-0">
              <b className="tw:block tw:text-cuerpo tw:font-semibold tw:text-tinta-hi">
                {f.nombre}
              </b>
              <em className="tw:mt-[3px] tw:block tw:text-ayuda tw:leading-[1.4] tw:not-italic tw:text-tinta-low">
                {f.para}
              </em>
            </span>
          </button>
        ))}
      </div>
    </div>
  );
}
