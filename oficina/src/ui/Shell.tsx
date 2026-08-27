import { useCallback, useEffect, useState } from 'react';
import type { ReactElement, ReactNode } from 'react';

import BarraSuperior from './BarraSuperior';
import Riel from './Riel';
import type { GrupoNav, Miga } from './tipos';

/**
 * Shell del contrato 0.3.3: riel + barra superior + contenido.
 * Canon: COMPONENTS.md §3
 *
 * Es PRESENTACIONAL a propósito: recibe el menú ya adaptado, no lo pide. Quien
 * lo monta decide de dónde salen los datos, y así el shell se puede ver con el
 * menú de 4 ítems y con el de 52 sin levantar una sesión — que son justamente
 * los dos casos que el contrato exige probar.
 *
 * ── El pliegue se recuerda ───────────────────────────────────────────────────
 * Quien trabaja ocho horas en una bandeja pliega el riel una vez, no en cada
 * carga. Se guarda con las otras preferencias de apariencia, bajo la misma
 * convención de clave.
 */

const CLAVE_PLEGADO = 'loki.riel.plegado';

/** `null` = el usuario nunca eligió, así que decide quien monta el shell. */
function leerPreferenciaPlegado(): boolean | null {
  try {
    const guardado = localStorage.getItem(CLAVE_PLEGADO);
    return guardado === null ? null : guardado === '1';
  } catch {
    return null;
  }
}

export interface ShellProps {
  grupos: GrupoNav[];
  usuario: { nombre: string; rol: string; foto?: string; iniciales?: string };
  /** Marca institucional para la barra superior. */
  marca?: { logo?: string; nombre: string; bajada: string };
  /** Dónde está el usuario. Un tramo con `href` navega; ver `BarraSuperior`. */
  migas: readonly Miga[];
  /**
   * Cómo arranca el riel cuando el usuario todavía no eligió.
   *
   * Un riel con un solo destino no es navegación: es margen. Medido en LOKI,
   * 78 px de ítems en 720 px de alto —89 % vacío— y 224 px de ancho fijos que
   * el contenido no puede usar. Plegado sigue estando ahí, con su ícono y su
   * botón, y la primera vez que alguien lo despliega su elección manda desde
   * entonces: esto decide el arranque, no lo que el usuario puede hacer.
   */
  plegadoPorOmision?: boolean;
  /** Ruta activa del riel. */
  activo?: string;
  /** Qué se puede buscar. Se propaga a la barra: ver el porqué en `BarraSuperior`. */
  etiquetaBusqueda?: string;
  /**
   * Oculta el disparador de búsqueda de la barra y desactiva el atajo ⌘K/Ctrl+K.
   * Se propaga a `BarraSuperior`: ver el porqué ahí. `onBuscar` se queda como
   * prop obligatoria — el contrato no cambia, sólo se apagan sus dos únicas
   * entradas.
   */
  sinBusqueda?: boolean;
  /** Campana, selector de tema: lo que la aplicación cuelgue a la derecha. */
  accionesBarra?: ReactNode;
  /** Abre la paleta de comandos. El shell no la monta: sólo la dispara. */
  onBuscar(): void;
  children: ReactNode;
}

/** Primera del primer nombre y del primer APELLIDO — la tercera palabra en un
 *  nombre hondureño de cuatro, no la segunda. */
function inicialesDe(nombre: string): string {
  const p = nombre.trim().split(/\s+/).filter(Boolean);
  if (p.length === 0) return '?';
  const i = p.length >= 3 ? 2 : 1;
  return ((p[0]?.[0] ?? '') + (p[i]?.[0] ?? '')).toUpperCase();
}



export default function Shell({
  grupos,
  usuario,
  marca,
  migas,
  activo,
  plegadoPorOmision = false,
  etiquetaBusqueda,
  sinBusqueda = false,
  accionesBarra,
  onBuscar,
  children,
}: ShellProps): ReactElement {
  const [plegado, setPlegado] = useState(() => leerPreferenciaPlegado() ?? plegadoPorOmision);

  const alternarPlegado = useCallback(() => {
    setPlegado((p) => {
      const siguiente = !p;
      try {
        localStorage.setItem(CLAVE_PLEGADO, siguiente ? '1' : '0');
      } catch {
        /* sin persistencia: el pliegue vale para esta sesión y ya */
      }
      return siguiente;
    });
  }, []);

  // ⌘K / Ctrl+K. Va en el shell y no en la paleta porque el atajo tiene que
  // funcionar cuando la paleta está cerrada, que es el 100% de las veces que se
  // usa. `preventDefault` evita que el navegador se lleve la combinación.
  //
  // Con `sinBusqueda` no se registra el listener: si la barra no anuncia que
  // hay búsqueda, el atajo tampoco puede disparar una — un ⌘K que hiciera algo
  // sin que nada en pantalla lo sugiera sería la misma promesa incumplida que
  // el botón muerto que `sinBusqueda` existe para evitar.
  useEffect(() => {
    if (sinBusqueda) return;
    function alTeclado(ev: KeyboardEvent): void {
      if ((ev.metaKey || ev.ctrlKey) && ev.key.toLowerCase() === 'k') {
        ev.preventDefault();
        onBuscar();
      }
    }
    document.addEventListener('keydown', alTeclado);
    return () => document.removeEventListener('keydown', alTeclado);
  }, [sinBusqueda, onBuscar]);

  return (
    <div className="loki-shell tw:flex tw:h-screen tw:overflow-hidden tw:bg-canvas">
      <Riel
        grupos={grupos}
        plegado={plegado}
        onPlegar={alternarPlegado}
        activo={activo}
        usuario={{
          nombre: usuario.nombre,
          rol: usuario.rol,
          iniciales: usuario.iniciales ?? inicialesDe(usuario.nombre),
        }}
      />

      <div className="tw:flex tw:min-w-0 tw:flex-1 tw:flex-col">
        <BarraSuperior
          marca={marca}
          migas={migas}
          usuario={usuario}
          onBuscar={onBuscar}
          etiquetaBusqueda={etiquetaBusqueda}
          sinBusqueda={sinBusqueda}
          acciones={accionesBarra}
        />

        {/* El desplazamiento vive acá, no en el `body`: así el riel y la barra se
            quedan fijos y sólo se mueve el contenido, que es lo que el usuario
            está leyendo. */}
        <main className="loki-scroll tw:min-h-0 tw:flex-1 tw:overflow-y-auto tw:p-4">
          {children}
        </main>
      </div>
    </div>
  );
}
