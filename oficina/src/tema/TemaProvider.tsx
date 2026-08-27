import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react';
import type { ReactElement, ReactNode } from 'react';

import {
  DENSIDADES,
  DENSIDAD_POR_DEFECTO,
  NOMBRES_TEMA,
  TEMAS,
  TEMA_POR_DEFECTO,
} from './temas.def';
import type { DensidadId, TemaId } from './temas.def';

/**
 * Proveedor de tema del contrato de diseño 0.3.3.
 *
 * ── La regla que sostiene todo el sistema ────────────────────────────────────
 * Éste es el ÚNICO lugar que escribe el tema. Ningún componente pregunta cuál
 * está activo: recibe `tono`, `variante` o `estado`, y el tema se resuelve solo
 * en CSS. Un `if (tema === 'x')` dentro de un componente ES el contrato roto —
 * y es exactamente lo que impide que agregar un séptimo tema sea gratis.
 *
 * Si te encontrás necesitando leer el tema dentro de un componente, lo que falta
 * es un token. Pedilo. La excepción real y única es el envoltorio de avisos, que
 * le tiene que decir a una librería de terceros si el fondo es claro u oscuro
 * porque esa librería no lee nuestros tokens.
 *
 * ── Por qué el estado inicial se LEE del DOM y no se calcula ─────────────────
 * El valor ya lo aplicó el script bloqueante del `<head>`, antes del primer
 * pintado. Si acá lo recalculáramos podríamos llegar a otra conclusión que la
 * del script —basta que cambie el orden de la cascada— y el usuario vería el
 * tema saltar al montar React. Que es precisamente lo que el script existe para
 * evitar.
 */

export { DENSIDADES, NOMBRES_TEMA, TEMAS, TEMA_POR_DEFECTO };
export type { DensidadId, TemaId };

/**
 * Las mismas claves que lee el script bloqueante de `index.html`.
 *
 * ⚠️ Están escritas en dos lugares porque el script corre antes de que existan
 * los módulos. Si cambian acá y no allá, el tema se guarda y no se restaura: la
 * preferencia parece no funcionar, sin ningún error.
 */
const CLAVE_TEMA = 'loki.tema';
const CLAVE_DENSIDAD = 'loki.densidad';

export interface TemaContexto {
  tema: TemaId;
  densidad: DensidadId;
  setTema(t: TemaId): void;
  setDensidad(d: DensidadId): void;
}

const Contexto = createContext<TemaContexto | null>(null);

function esTema(v: string | null): v is TemaId {
  return v !== null && (NOMBRES_TEMA as readonly string[]).includes(v);
}

function esDensidad(v: string | null): v is DensidadId {
  return v !== null && (DENSIDADES as readonly string[]).includes(v);
}

export function aplicarTema(tema: TemaId): void {
  document.documentElement.setAttribute('data-tema', tema);
}

export function aplicarDensidad(densidad: DensidadId): void {
  document.documentElement.setAttribute('data-densidad', densidad);
}

/** Tema efectivo del documento. */
export function temaActual(): TemaId {
  if (typeof document === 'undefined') return TEMA_POR_DEFECTO;
  const propio = document.documentElement.getAttribute('data-tema');
  return esTema(propio) ? propio : TEMA_POR_DEFECTO;
}

export function densidadActual(): DensidadId {
  if (typeof document === 'undefined') return DENSIDAD_POR_DEFECTO;
  const propia = document.documentElement.getAttribute('data-densidad');
  return esDensidad(propia) ? propia : DENSIDAD_POR_DEFECTO;
}

/**
 * `localStorage` puede lanzar: modo privado de Safari, cuota llena, o una
 * política de empresa que lo bloquea. Que no se pueda RECORDAR la preferencia no
 * es motivo para que no se pueda USAR — así que se traga el error y el tema
 * igual se aplica, sólo que no sobrevive a la recarga.
 */
function guardar(clave: string, valor: string): void {
  try {
    localStorage.setItem(clave, valor);
  } catch {
    /* sin persistencia; el tema igual queda aplicado en esta sesión */
  }
}

function leerGuardado(clave: string): string | null {
  try {
    return localStorage.getItem(clave);
  } catch {
    return null;
  }
}

/**
 * Aplica lo que el usuario eligió la última vez.
 *
 * El script del `<head>` ya lo hizo, así que esto es una red: cubre el caso de
 * que el bundle se monte en una página que no lo trae.
 */
export function aplicarPreferenciaGuardada(): void {
  const tema = leerGuardado(CLAVE_TEMA);
  if (esTema(tema)) aplicarTema(tema);

  const densidad = leerGuardado(CLAVE_DENSIDAD);
  if (esDensidad(densidad)) aplicarDensidad(densidad);
}

export function TemaProvider({ children }: { readonly children: ReactNode }): ReactElement {
  const [tema, setTemaEstado] = useState<TemaId>(temaActual);
  const [densidad, setDensidadEstado] = useState<DensidadId>(densidadActual);

  const setTema = useCallback((t: TemaId): void => {
    aplicarTema(t);
    guardar(CLAVE_TEMA, t);
    setTemaEstado(t);
  }, []);

  const setDensidad = useCallback((d: DensidadId): void => {
    aplicarDensidad(d);
    guardar(CLAVE_DENSIDAD, d);
    setDensidadEstado(d);
  }, []);

  /**
   * Si alguien mueve los atributos por fuera de este proveedor —el inspector del
   * navegador, un script de la página anfitriona, una prueba— el estado de React
   * quedaría diciendo una cosa y la pantalla mostrando otra. Este observador es
   * lo que mantiene las dos versiones de la verdad pegadas.
   */
  useEffect(() => {
    const observador = new MutationObserver(() => {
      setTemaEstado(temaActual());
      setDensidadEstado(densidadActual());
    });
    observador.observe(document.documentElement, {
      attributes: true,
      attributeFilter: ['data-tema', 'data-densidad'],
    });
    return () => observador.disconnect();
  }, []);

  const valor = useMemo<TemaContexto>(
    () => ({ tema, densidad, setTema, setDensidad }),
    [tema, densidad, setTema, setDensidad],
  );

  return <Contexto.Provider value={valor}>{children}</Contexto.Provider>;
}

export function useTema(): TemaContexto {
  const ctx = useContext(Contexto);
  if (ctx === null) {
    throw new Error('useTema() se usó fuera de <TemaProvider>.');
  }
  return ctx;
}
