import type { CSSProperties, ReactElement } from 'react';

import Boton from '../ui/Boton';
import Pastilla from '../ui/Pastilla';


/**
 * Bloque «Rigor» de la vitrina: densidad comparada y longitudes extremas.
 * Es lo que se rompe primero, y por eso se muestra.
 */

/* ═══════════════════════════════════════════════════════════════════════════
   DENSIDAD COMPARADA
   ═══════════════════════════════════════════════════════════════════════════ */

/**
 * Las dos densidades lado a lado, con el MISMO dato.
 *
 * El interruptor existe arriba, pero nadie ve qué cambia hasta apretarlo — y al
 * apretarlo cambia todo a la vez, así que tampoco se puede comparar. Acá cada
 * columna fija sus variables de densidad localmente, que es la única forma de
 * tener las dos en pantalla al mismo tiempo.
 */
const COMODA: CSSProperties = {
  ['--h-control' as string]: '40px',
  ['--h-control-sm' as string]: '32px',
  ['--row-h' as string]: '44px',
  ['--pad-panel' as string]: '16px 18px',
};

const COMPACTA: CSSProperties = {
  ['--h-control' as string]: '34px',
  ['--h-control-sm' as string]: '28px',
  ['--row-h' as string]: '36px',
  ['--pad-panel' as string]: '11px 14px',
};

const FILAS: readonly (readonly [string, 'ok' | 'aviso' | 'info', string, string])[] = [
  ['SOL-01293', 'info', 'Revisión', '49,990.65'],
  ['SOL-01291', 'aviso', 'Corrección', '12,480.00'],
  ['EXT-01292', 'ok', 'Aprobada', '31,000.00'],
];

function LadoDensidad({
  estilo,
  rotulo,
  pie,
}: {
  readonly estilo: CSSProperties;
  readonly rotulo: string;
  readonly pie: string;
}): ReactElement {
  return (
    <div style={estilo} className="tw:min-w-0 tw:flex-1">
      <p className="tw:mb-2 tw:text-cabecera tw:font-semibold tw:tracking-[.08em] tw:text-tinta-mid tw:uppercase">
        {rotulo}
      </p>
      <div className="tw:overflow-hidden tw:rounded-panel tw:border tw:border-linea tw:bg-panel">
        <table className="loki-tabla">
          <thead>
            <tr>
              {['Referencia', 'Etapa', 'Monto'].map((c, i) => (
                <th key={c} className={`loki-th ${i === 2 ? 'tw:text-right' : 'tw:text-left'}`}>
                  {c}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {FILAS.map(([ref, tono, etapa, monto]) => (
              <tr key={ref} className="loki-fila-r">
                <td className="loki-td">
                  <span className="loki-ref">{ref}</span>
                </td>
                <td className="loki-td">
                  <Pastilla tono={tono}>{etapa}</Pastilla>
                </td>
                <td className="loki-td loki-td-n">{monto}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      <div className="tw:mt-2 tw:flex tw:gap-2">
        <Boton tamano="sm" variante="secundario">
          Devolver
        </Boton>
        <Boton tamano="sm" variante="primario">
          Aprobar
        </Boton>
      </div>
      <p className="tw:mt-2 tw:text-ayuda tw:text-tinta-low">{pie}</p>
    </div>
  );
}

export function DensidadComparada(): ReactElement {
  return (
    <div className="tw:flex tw:flex-col tw:gap-3">
      <div className="tw:flex tw:flex-wrap tw:gap-5">
        <LadoDensidad
          estilo={COMODA}
          rotulo="Cómoda · fila 44 px · control 40 px"
          pie="Por defecto. 3 filas en 132 px."
        />
        <LadoDensidad
          estilo={COMPACTA}
          rotulo="Compacta · fila 36 px · control 34 px"
          pie="Turnos de captura. Las mismas 3 filas en 108 px — +18 % de filas por pantalla."
        />
      </div>
      <p className="tw:text-cuerpo-2 tw:text-tinta-low">
        La densidad <strong>no cambia tamaños de letra</strong>. Sólo alturas de control, alto de
        fila, relleno de panel y gap. Si además encogiera la tipografía dejaría de ser una
        preferencia y sería un tema distinto.
      </p>
    </div>
  );
}

/* ═══════════════════════════════════════════════════════════════════════════
   LONGITUDES EXTREMAS
   ═══════════════════════════════════════════════════════════════════════════ */

/**
 * Cada componente probado con el texto REAL más largo del sistema, nunca con
 * lorem ipsum. Lo que sobrevive acá sobrevive en producción.
 */
export function LongitudesExtremas(): ReactElement {
  return (
    <div className="tw:flex tw:flex-col tw:gap-4">
      <Grupo rotulo="Pastilla larga">
        <Pastilla tono="neutro" punto={false}>Procesamiento Paralelo 6a — Viáticos</Pastilla>
        <Pastilla tono="info" punto={false}>Aprobación Gerente Solicitante</Pastilla>
        <Pastilla tono="aviso">Corrección — ciclo 3 de 5</Pastilla>
      </Grupo>

      <Grupo rotulo="Pastilla corta">
        <Pastilla tono="ok">OK</Pastilla>
        <Pastilla tono="neutro" punto={false}>—</Pastilla>
      </Grupo>

      <Grupo rotulo="Referencia">
        <span className="loki-ref">MSV-26-1255-GT</span>
        <span className="loki-ref">GLIQ-000042</span>
        <span className="loki-ref">SOL-1</span>
      </Grupo>

      <Grupo rotulo="Importe">
        <span className="tw:font-mono tw:tabular-nums tw:text-tinta-hi">L. 1,284,990.65</span>
        <span className="tw:font-mono tw:tabular-nums tw:text-tinta-hi">USD 1,860.00</span>
        <span className="tw:font-mono tw:tabular-nums tw:text-tinta-hi">L. 0.00</span>
      </Grupo>

      <Grupo rotulo="Nombre">
        <span className="loki-nm">Nolvia Esperanza Cruz Interiano de Villanueva</span>
        <span className="loki-nm">Ana Paz</span>
      </Grupo>

      <Grupo rotulo="Botón">
        <Boton variante="secundario">Generar orden de pago con membrete</Boton>
        <Boton variante="secundario">Ir</Boton>
      </Grupo>

      <Grupo rotulo="Título de página">
        <span className="tw:font-serif tw:text-pagina tw:font-semibold tw:text-tinta-hi">
          Monitor de liquidaciones grupales por gerencia
        </span>
      </Grupo>

      <p className="tw:text-cuerpo-2 tw:text-tinta-low">
        Regla: <strong>la pastilla no envuelve nunca</strong> y la etapa más larga del flujo cabe
        en su columna. El nombre sí envuelve, y el ítem del riel recorta con elipsis porque el
        contador debe seguir visible.
      </p>
    </div>
  );
}

function Grupo({
  rotulo,
  children,
}: {
  readonly rotulo: string;
  readonly children: React.ReactNode;
}): ReactElement {
  return (
    <div className="tw:flex tw:flex-wrap tw:items-center tw:gap-3">
      <span className="loki-especimen-rol tw:shrink-0 tw:text-cabecera tw:font-semibold tw:tracking-[.08em] tw:text-tinta-low tw:uppercase">
        {rotulo}
      </span>
      <div className="tw:flex tw:min-w-0 tw:flex-wrap tw:items-center tw:gap-2">{children}</div>
    </div>
  );
}
