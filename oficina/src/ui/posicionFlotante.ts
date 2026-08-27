import { useLayoutEffect, useState } from 'react';
import type { CSSProperties, RefObject } from 'react';

/**
 * Coloca un panel flotante —calendario, desplegable— **sin que lo recorte el contenedor**.
 *
 * ── El defecto que resuelve ──────────────────────────────────────────────────
 * Reportado el 2026-08-10 usando el formulario de gira: al abrir el calendario dentro del modal,
 * aparecía una barra de desplazamiento y el calendario quedaba cortado por abajo.
 *
 * La causa es estructural, no del calendario. El panel se dibujaba `absolute`, o sea **dentro del
 * flujo del cuerpo del diálogo**, que tiene `max-height: calc(100vh - 4rem)`. Un panel de 300 px
 * que aparece de golpe hace crecer el contenido, sale la barra, y lo que sobra se recorta. El
 * usuario ve medio calendario y una barra que nadie pidió.
 *
 * ── Por qué `fixed` y NO un portal a `document.body` ─────────────────────────
 * Es la trampa de este arreglo, y hay que decirla: `Dialogo` usa `<dialog>` con **`showModal()`**,
 * que lo pone en la **capa superior** del navegador. Cualquier cosa portada a `document.body`
 * queda **por DEBAJO** de esa capa — el calendario desaparecería del todo, que es peor que
 * recortado.
 *
 * `position: fixed` resuelve las dos mitades sin salir del diálogo:
 * · **No lo recorta** ningún `overflow` de sus ancestros.
 * · **No cuenta para el alto** del contenido, así que la barra de desplazamiento ya no aparece.
 * · Sigue siendo descendiente del `<dialog>`, así que **no pierde la capa superior**.
 *
 * ⚠️ Un ancestro con `transform`, `filter`, `perspective`, `contain` o `will-change` se vuelve el
 * bloque contenedor de los `fixed` y este arreglo dejaría de funcionar. Verificado el 2026-08-10:
 * `.loki-dialogo` no tiene ninguno. Si algún día se le anima con `transform`, esto vuelve a
 * romperse — y el síntoma va a ser el mismo de antes.
 *
 * ── Dónde se coloca, en este orden ───────────────────────────────────────────
 * 1. **Debajo del ancla**, si cabe. Es lo esperable y lo que menos hay que pensar.
 * 2. **Al costado del diálogo** —derecha, y si no izquierda—, alineado con el ancla.
 * 3. **Arriba del ancla**, sólo si no queda otra.
 *
 * El costado va ANTES que el volteo por lo que se ve al usarlo: en un modal angosto, un
 * calendario de 300 px volteado hacia arriba **tapa el formulario entero** —el título, el campo
 * que se está llenando— y uno pierde de vista lo que estaba haciendo. A los lados del modal suele
 * sobrar pantalla, y ahí el calendario convive con el formulario en vez de sepultarlo. Reportado
 * mirándolo: «que salga a un lado, mejor» (2026-08-10).
 *
 * ⚠️ Se mide contra el rectángulo del **diálogo**, no el del ancla: pegado al ancla quedaría
 * encima del propio modal. Fuera de un diálogo se usa el ancla, que es lo único que hay.
 */
export function usePosicionFlotante(
  abierto: boolean,
  ancla: RefObject<HTMLElement | null>,
  panel: RefObject<HTMLElement | null>,
): CSSProperties {
  const [estilo, setEstilo] = useState<CSSProperties>({ visibility: 'hidden' });

  useLayoutEffect(() => {
    if (!abierto) {
      // Se vuelve a ocultar al cerrar: si no, el primer fotograma de la próxima apertura se
      // dibujaría en la posición vieja y se vería un salto.
      setEstilo({ visibility: 'hidden' });
      return;
    }

    function colocar(): void {
      const a = ancla.current;
      const p = panel.current;
      if (!a || !p) return;

      const r = a.getBoundingClientRect();
      const alto = p.offsetHeight;
      const ancho = p.offsetWidth;
      const sep = 8;
      const borde = 8;

      // Al costado se mide contra el DIÁLOGO: pegado al ancla quedaría encima del propio modal.
      const contenedor = a.closest('dialog')?.getBoundingClientRect() ?? r;

      // Vertical del costado: a la altura del ancla, sin salirse por arriba ni por abajo.
      const arribaAlineado = Math.max(
        borde,
        Math.min(r.top, window.innerHeight - alto - borde),
      );

      // 1 · Debajo.
      if (r.bottom + sep + alto <= window.innerHeight) {
        setEstilo({
          position: 'fixed',
          left: `${String(Math.round(Math.max(borde, Math.min(r.left, window.innerWidth - ancho - borde))))}px`,
          top: `${String(Math.round(r.bottom + sep))}px`,
          visibility: 'visible',
        });
        return;
      }

      // 2 · Al costado del diálogo. Derecha primero: es donde el ojo ya está.
      if (contenedor.right + sep + ancho + borde <= window.innerWidth) {
        setEstilo({
          position: 'fixed',
          left: `${String(Math.round(contenedor.right + sep))}px`,
          top: `${String(Math.round(arribaAlineado))}px`,
          visibility: 'visible',
        });
        return;
      }
      if (contenedor.left - sep - ancho - borde >= 0) {
        setEstilo({
          position: 'fixed',
          left: `${String(Math.round(contenedor.left - sep - ancho))}px`,
          top: `${String(Math.round(arribaAlineado))}px`,
          visibility: 'visible',
        });
        return;
      }

      // 3 · Arriba, y si tampoco cabe, abajo igual: es mejor que se salga por donde se puede
      // desplazar que por el techo, donde no hay forma de alcanzarlo.
      const cabeArriba = r.top - sep - alto >= borde;
      setEstilo({
        position: 'fixed',
        left: `${String(Math.round(Math.max(borde, Math.min(r.left, window.innerWidth - ancho - borde))))}px`,
        top: cabeArriba
          ? `${String(Math.round(r.top - sep - alto))}px`
          : `${String(Math.round(r.bottom + sep))}px`,
        visibility: 'visible',
      });
    }

    colocar();

    // `capture: true` para enterarse del desplazamiento de CUALQUIER contenedor, no sólo de la
    // ventana: el panel está anclado a un elemento que puede vivir dentro de algo que se desplaza.
    window.addEventListener('resize', colocar);
    window.addEventListener('scroll', colocar, true);
    return () => {
      window.removeEventListener('resize', colocar);
      window.removeEventListener('scroll', colocar, true);
    };
  }, [abierto, ancla, panel]);

  return estilo;
}
