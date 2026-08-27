import type { ReactElement } from 'react';

import Icono from '../ui/Icono';

/**
 * Las tres secciones de «Fundamentos» que no son componentes: espaciado,
 * geometría e iconografía. Traducidas de la vitrina del paquete.
 */

/* ═══════════════════════════════════════════════════════════════════════════
   ESPACIADO
   ═══════════════════════════════════════════════════════════════════════════ */

const ESCALA: readonly [string, number][] = [
  ['--sp-1', 4],
  ['--sp-2', 8],
  ['--sp-3', 12],
  ['--sp-4', 16],
  ['--sp-5', 22],
  ['--sp-6', 30],
  ['--sp-7', 40],
];

export function EscalaEspaciado(): ReactElement {
  return (
    <div className="tw:flex tw:flex-col tw:gap-2">
      {ESCALA.map(([nombre, px]) => (
        <div key={nombre} className="tw:flex tw:items-center tw:gap-3">
          <code className="tw:w-16 tw:shrink-0 tw:font-mono tw:text-ayuda tw:text-acento-ink">
            {nombre}
          </code>
          {/* La barra ES la medida: se ve que los saltos crecen, que es la
              decisión de la escala. Un número en una tabla no lo muestra. */}
          <span className="loki-barra-sp" style={{ width: px }} />
          <span className="tw:font-mono tw:text-ayuda tw:text-tinta-low">{px} px</span>
        </div>
      ))}
      <p className="tw:mt-1 tw:text-cuerpo-2 tw:text-tinta-low">
        Base 4, con saltos que <strong>crecen</strong>: dos niveles vecinos nunca se confunden a
        simple vista. El relleno del panel y el gap de la rejilla{' '}
        <strong>no están en esta escala</strong> — los gobierna la densidad (
        <code className="tw:font-mono tw:text-tinta-mid">--pad-panel</code>,{' '}
        <code className="tw:font-mono tw:text-tinta-mid">--gap-grid</code>). Y nada de{' '}
        <code className="tw:font-mono tw:text-tinta-mid">space-y-*</code> entre bloques: el ritmo
        vertical lo da el gap del contenedor.
      </p>
    </div>
  );
}

/* ═══════════════════════════════════════════════════════════════════════════
   GEOMETRÍA Y ELEVACIÓN
   ═══════════════════════════════════════════════════════════════════════════ */

const RADIOS: readonly [string, string, string][] = [
  ['--r-badge', 'tw:rounded-badge', '4 px · pastilla, casilla, avatar cuadrado'],
  ['--r-control', 'tw:rounded-control', '6 px · botón, campo, chip'],
  ['--r-panel', 'tw:rounded-panel', '8 px · panel, modal, cajón'],
];

const ELEVACIONES: readonly [string, string, string][] = [
  ['sin elevación', '', 'lo que está en la página'],
  ['--shadow', 'tw:shadow-loki', 'panel · apenas separa del lienzo'],
  ['--shadow-lift', 'tw:shadow-lift', 'menú, aviso, modal, cajón'],
];

export function GeometriaYElevacion(): ReactElement {
  return (
    <div className="tw:flex tw:flex-col tw:gap-4">
      <div className="tw:flex tw:flex-wrap tw:gap-3">
        {RADIOS.map(([nombre, clase, para]) => (
          <div key={nombre} className="tw:flex tw:items-center tw:gap-2.5">
            <span className={`loki-muestra-radio tw:border tw:border-linea tw:bg-inset ${clase}`} />
            <span>
              <code className="tw:block tw:font-mono tw:text-ayuda tw:text-acento-ink">
                {nombre}
              </code>
              <span className="tw:text-ayuda tw:text-tinta-low">{para}</span>
            </span>
          </div>
        ))}
      </div>

      <div className="tw:flex tw:flex-wrap tw:gap-3">
        {ELEVACIONES.map(([nombre, clase, para]) => (
          <div
            key={nombre}
            className={`tw:rounded-panel tw:border tw:border-linea tw:bg-panel tw:px-3 tw:py-2 ${clase}`}
          >
            <code className="tw:block tw:font-mono tw:text-ayuda tw:text-acento-ink">{nombre}</code>
            <span className="tw:text-ayuda tw:text-tinta-low">{para}</span>
          </div>
        ))}
      </div>

      <p className="tw:text-cuerpo-2 tw:text-tinta-low">
        <strong>Poca sombra, mucho borde.</strong> Los paneles se separan por línea; la elevación
        se reserva para lo que de verdad flota. Tarjetas con sombra sobre gris es la firma de la
        plantilla que estamos dejando atrás.
      </p>
    </div>
  );
}

/* ═══════════════════════════════════════════════════════════════════════════
   ICONOGRAFÍA
   ═══════════════════════════════════════════════════════════════════════════ */

/** Nombre del sistema → ícono de Lucide. Los de la derecha son los que emite el
 *  menú del servidor, así que la tabla vale para los dos usos. */
const ICONOS: readonly [string, string][] = [
  ['inicio', 'home'], ['solicitud', 'file-text'], ['gira', 'map-pin'], ['bandeja', 'inbox'],
  ['plazo', 'clock'], ['liquidacion', 'layers'], ['equipo', 'users'], ['metricas', 'bar-chart-2'],
  ['sistema', 'settings'], ['catalogos', 'book-open'], ['usuario', 'user'], ['salir', 'log-out'],
  ['buscar', 'search'], ['filtro', 'filter'], ['exportar', 'download'], ['guardar', 'save'],
  ['sumar', 'plus'], ['aprobar', 'check'], ['devolver', 'corner-up-left'], ['anular', 'trash-2'],
  ['documento', 'file'], ['adjuntar', 'paperclip'], ['alerta', 'alert-triangle'],
  ['informacion', 'info'], ['notificacion', 'bell'], ['plegar', 'panel-left-close'],
  ['avanzar', 'chevron-right'], ['cerrar', 'x'],
];

const TAMANOS: readonly [13 | 15 | 18 | 22 | 34, string][] = [
  [13, 'dentro de una pastilla'],
  [15, 'en botón y menú'],
  [18, 'en banda de aviso y cabecera'],
  [22, 'en acción destacada'],
  [34, 'en estado vacío'],
];

export function Iconografia(): ReactElement {
  return (
    <div className="tw:flex tw:flex-col tw:gap-4">
      <div className="tw:flex tw:flex-wrap tw:items-end tw:gap-5">
        {TAMANOS.map(([px, para]) => (
          <div key={px} className="tw:flex tw:flex-col tw:items-center tw:gap-1.5">
            <Icono nombre="clock" tamano={px} className="tw:text-tinta-base" />
            <span className="tw:font-mono tw:text-ayuda tw:text-tinta-low">{px} px</span>
            <span className="tw:text-ayuda tw:text-tinta-low">{para}</span>
          </div>
        ))}
      </div>

      <div className="loki-rejilla-iconos">
        {ICONOS.map(([rol, lucide]) => (
          <div key={rol} className="tw:flex tw:flex-col tw:items-center tw:gap-1 tw:py-2">
            <Icono nombre={lucide} tamano={22} className="tw:text-tinta-base" />
            <span className="tw:font-mono tw:text-ayuda tw:text-tinta-low">{rol}</span>
          </div>
        ))}
      </div>

      <p className="tw:text-cuerpo-2 tw:text-tinta-low">
        Una sola familia: <strong>Lucide</strong>, trazo 1.8,{' '}
        <code className="tw:font-mono tw:text-tinta-mid">currentColor</code>, remates redondos.
        Nunca un tamaño intermedio. Mezclar familias de íconos es lo primero que delata un sistema
        sin dueño — y por eso el menú del servidor, que emite nombres de Feather, se resuelve
        contra Lucide en vez de traer su propio juego.
      </p>
    </div>
  );
}
