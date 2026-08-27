import { useEffect, useState } from 'react';

/**
 * Cuál de las secciones está leyendo la persona.
 *
 * ── Por qué un observador y no la posición del scroll ────────────────────────
 * Calcularlo con `scrollTop` obliga a leer la posición de cada sección en cada
 * cuadro. Eso fuerza al navegador a recalcular el diseño sesenta veces por
 * segundo — el desplazamiento se siente pegajoso justo en la página que existe
 * para demostrar que el sistema se siente bien.
 *
 * `IntersectionObserver` lo resuelve al revés: el navegador avisa cuando algo
 * entra o sale, y entre medio no cuesta nada.
 *
 * ── El detalle que hace que se sienta bien: `rootMargin` ─────────────────────
 * `-45% 0px -50% 0px` reduce la zona de detección a **una franja delgada a la
 * altura de los ojos**, más o menos donde uno mira. Sin eso, con la ventana
 * mostrando tres secciones a la vez, «activa» sería siempre la primera visible —
 * y el índice se quedaría marcando una sección que quedó arriba, fuera de la
 * pantalla, mientras se lee otra.
 *
 * ⚠️ El contenedor que hace scroll **no es la ventana**: es el `<main>` del
 * shell. Por eso hace falta `root`. Con el valor por omisión (la ventana) el
 * observador no dispara nunca y el índice se queda quieto, sin ningún error.
 */
export function useSeccionActiva(
  ids: readonly string[],
  contenedor: HTMLElement | null,
): string | null {
  const [activa, setActiva] = useState<string | null>(ids[0] ?? null);

  useEffect(() => {
    if (contenedor === null) return;

    const nodos = ids
      .map((id) => document.getElementById(id))
      .filter((n): n is HTMLElement => n !== null);

    if (nodos.length === 0) return;

    // Se guarda cuáles están dentro de la franja y se elige la PRIMERA en orden
    // de documento. Sin esto, con dos secciones cortas dentro de la franja, la
    // activa dependería del orden en que llegaron las notificaciones — y el
    // índice parpadearía entre las dos.
    const dentro = new Set<string>();

    const observador = new IntersectionObserver(
      (entradas) => {
        for (const e of entradas) {
          if (e.isIntersecting) dentro.add(e.target.id);
          else dentro.delete(e.target.id);
        }
        const primera = ids.find((id) => dentro.has(id));
        if (primera !== undefined) setActiva(primera);
      },
      { root: contenedor, rootMargin: '-45% 0px -50% 0px', threshold: 0 },
    );

    for (const n of nodos) observador.observe(n);
    return () => observador.disconnect();
  }, [ids, contenedor]);

  return activa;
}
