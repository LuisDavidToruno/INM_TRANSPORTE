import { CircleAlert, CircleCheck, Info, TriangleAlert } from 'lucide-react';
import { Toaster as ToasterSonner, toast } from 'sonner';
import { useEffect, useRef } from 'react';
import type { ReactElement } from 'react';

import { TEMAS, useTema } from '../tema/TemaProvider';

/**
 * Avisos (toasts) y confirmaciones del frontend nuevo.
 *
 * ═══════════════════════════════════════════════════════════════════════════
 *  ⚠️ POR QUÉ **NO** SE USA SweetAlert2 ACÁ
 * ═══════════════════════════════════════════════════════════════════════════
 * La mitad Razor lo usa en 73 archivos a través del envoltorio `SweetAlertPhoenix`,
 * y ahí seguirá: no se toca. Pero traerlo al frontend nuevo sería un error, y no por
 * gusto — por tres motivos concretos:
 *
 * 1. **No sabe nada de nuestros tokens.** SweetAlert2 trae su propia hoja de estilos
 *    con sus colores. Para que respete el tema habría que sobrescribir sus clases
 *    (`.swal2-popup`, `.swal2-title`, `.swal2-confirm`…) tema por tema. Eso es
 *    exactamente lo que el contrato cerrado de tokens existe para evitar: un tema
 *    nuevo tendría que traer reglas de componente propias, que es la regla 1 de D2
 *    rota. Con sonner el diálogo es DOM nuestro y `var(--loki-*)` ya funciona: un
 *    tema nuevo no toca nada.
 *
 * 2. **Maneja el DOM por su cuenta.** Crea y destruye nodos fuera de React, igual que
 *    DataTables. Este repositorio ya pagó ese precio: el antipatrón documentado de
 *    mezclar `row.remove()` animado con `ajax.reload()` vaciaba las bandejas.
 *
 * 3. **Su modo `confirm()` devuelve una promesa y bloquea.** En React la confirmación
 *    es estado, no una pausa en el flujo: `Dialogo` ya lo resuelve con `<dialog>`
 *    nativo, que trae trampa de foco y Esc del navegador.
 *
 * ⇒ **La regla:** avisos efímeros con `avisar.*` (sonner); confirmaciones con
 *   `Dialogo`. Nada de `alert()`, `confirm()`, ni SweetAlert2 en este frontend.
 *
 * ── El tema se sigue solo ────────────────────────────────────────────────────
 * `Avisos` lee el tema con `useTema()` y le pasa la base a sonner. Los colores salen
 * de los tokens vía `toastOptions.classNames`, así que un tema nuevo se refleja sin
 * tocar este archivo.
 *
 * ── ⚠️ Por qué los avisos viven en la CAPA SUPERIOR ──────────────────────────
 * `Modal` es un `<dialog>` abierto con `showModal()`, y eso lo mete en la **capa
 * superior** del navegador, que se pinta encima de todo `z-index` — incluso del
 * 999999999 de sonner. Un aviso disparado con un modal abierto quedaba **debajo del
 * modal, invisible**.
 *
 * El efecto era peor que un problema de pintura: el éxito se veía —porque cierra el
 * modal— y **el bloqueo duro no**, porque el modal se queda abierto. Quien apretaba
 * «Guardar» sobre una precondición incumplida veía que no pasaba nada, y el motivo
 * del rechazo aparecía recién si se rendía y cancelaba.
 *
 * La capa superior se ordena por **orden de promoción**: un `popover` mostrado
 * después de que el diálogo se abrió se pinta encima de él. Por eso no alcanza con
 * promover una vez al montar — hay que volver a promover **cada vez que llega un
 * aviso**, que es cuando puede haber un modal abierto por encima.
 */

/** Va UNA vez, en el shell. Sin esto los `avisar.*` no muestran nada. */
export function Avisos(): ReactElement {
  const { tema } = useTema();
  const base = TEMAS[tema].base;

  return (
    <EnLaCapaSuperior>
      <ToasterSonner
      // Esquina SUPERIOR derecha por contrato (COMPONENTS.md 0.3.2). No es
      // preferencia: abajo a la derecha es donde vive el pie de las bandejas —el
      // paginador y el resumen de selección—, y un aviso ahí tapa justo el control
      // que el oficial acaba de usar.
      position="top-right"
      // `theme` sólo le dice a sonner si el fondo es claro u oscuro (afecta su sombra
      // y su capa base). El color real lo ponen las clases de abajo, con tokens.
      theme={base === 'oscuro' ? 'dark' : 'light'}
      // Sin ella sonner apila los avisos en un mazo del que sólo se ve el de arriba;
      // en una interfaz de trabajo suelen llegar dos o tres seguidos.
      expand
      duration={4500}
      toastOptions={{
        classNames: {
          toast:
            'loki-toast tw:rounded-panel tw:border tw:border-linea tw:bg-panel tw:text-tinta-base tw:shadow-lg tw:font-sans',
          title: 'tw:text-sm tw:font-semibold tw:text-tinta-hi',
          description: 'tw:text-xs tw:text-tinta-mid',
          actionButton:
            'tw:rounded-control tw:bg-btn tw:px-2 tw:py-1 tw:text-xs tw:font-semibold tw:text-btn-fg',
          cancelButton:
            'tw:rounded-control tw:border tw:border-linea tw:px-2 tw:py-1 tw:text-xs tw:text-tinta-base',
          closeButton: 'tw:border-linea tw:bg-panel tw:text-tinta-mid',
        },
        }}
      />
    </EnLaCapaSuperior>
  );
}

/**
 * Mantiene a sus hijos en la capa superior del navegador, por encima de cualquier
 * `<dialog>` abierto.
 *
 * El atributo `popover` se pone **desde el efecto y no en el JSX**: un popover que
 * nunca se muestra queda en `display: none`, así que si el navegador no lo soporta y
 * el atributo estuviera puesto igual, los avisos desaparecerían del todo — peor que
 * el problema que esto resuelve.
 */
function EnLaCapaSuperior({ children }: { children: React.ReactNode }): ReactElement {
  const caja = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const e = caja.current;
    if (e === null || typeof e.showPopover !== 'function') return;

    // El atributo se pone acá y no en el JSX: un popover que nunca se muestra queda en
    // `display: none`, así que si el navegador no lo soportara y el atributo estuviera
    // puesto igual, los avisos desaparecerían del todo — peor que el problema que resuelve.
    e.setAttribute('popover', 'manual');
    alFrente();

    // **La promoción que sirve es la de cuando el aviso ya está montado.** Medido en vivo:
    // promover en el mismo instante de `avisar.*` —y también un cuadro después— dejaba el
    // aviso debajo del modal; recién con el nodo puesto queda encima. Por eso se observa la
    // llegada del nodo en lugar de esperar un número de milisegundos que hoy alcanza y
    // mañana no.
    const observador = new MutationObserver((cambios) => {
      const llego = cambios.some((c) =>
        [...c.addedNodes].some(
          (n) => n instanceof HTMLElement && n.querySelector('[data-sonner-toast]') !== null,
        ),
      );

      if (llego) alFrente();
    });

    observador.observe(e, { childList: true, subtree: true });
    return () => observador.disconnect();
  }, []);

  return (
    <div ref={caja} className="loki-capa-avisos">
      {children}
    </div>
  );
}

/**
 * Pone la capa de avisos por encima de todo.
 *
 * ── Por qué hace falta ──────────────────────────────────────────────────────
 * La capa superior del navegador se ordena por **orden de entrada**: salir y volver a
 * entrar es lo que deja el aviso encima de un `<dialog>` que se abrió después.
 *
 * ── Por qué busca el elemento en vez de recibirlo ───────────────────────────
 * Se intentó registrarlo desde el efecto en una variable de módulo y la referencia terminaba
 * en el sustituto vacío. Una consulta al DOM no tiene ciclo de vida que se desincronice, y
 * la capa es única en toda la aplicación.
 */
function alFrente(): void {
  const e = document.querySelector<HTMLElement>('.loki-capa-avisos');
  if (e === null || typeof e.showPopover !== 'function') return;

  try {
    if (e.matches(':popover-open')) e.hidePopover();
    e.showPopover();
  } catch {
    // Un navegador que rechace la promoción deja el aviso donde estaba: debajo del modal,
    // que es exactamente el comportamiento anterior a este arreglo.
  }
}

/** Ícono por tono. Se pasa explícito para que el color venga de los tokens. */
const ICONOS = {
  exito: <CircleCheck className="tw:size-4 tw:text-ok-fg" aria-hidden="true" />,
  error: <CircleAlert className="tw:size-4 tw:text-riesgo-fg" aria-hidden="true" />,
  alerta: <TriangleAlert className="tw:size-4 tw:text-aviso-fg" aria-hidden="true" />,
  info: <Info className="tw:size-4 tw:text-info-fg" aria-hidden="true" />,
};

interface Opciones {
  readonly detalle?: string;
  /** Duración en ms. Por omisión 4500; los errores se quedan hasta que se cierren. */
  readonly duracion?: number;
}

/**
 * API única de avisos.
 *
 * Decisión deliberada: **el error no se va solo.** Un aviso de éxito que desaparece
 * está bien —el usuario ya vio lo que quería—, pero un error que se desvanece en
 * cuatro segundos deja a alguien preguntándose qué decía. Se cierra a mano.
 */
export const avisar = {
  exito: (mensaje: string, o: Opciones = {}) =>
    toast.success(mensaje, {
      icon: ICONOS.exito,
      ...(o.detalle ? { description: o.detalle } : {}),
      ...(o.duracion ? { duration: o.duracion } : {}),
    }),

  error: (mensaje: string, o: Opciones = {}) =>
    toast.error(mensaje, {
      icon: ICONOS.error,
      ...(o.detalle ? { description: o.detalle } : {}),
      duration: o.duracion ?? Infinity,
      closeButton: true,
    }),

  alerta: (mensaje: string, o: Opciones = {}) =>
    toast.warning(mensaje, {
      icon: ICONOS.alerta,
      ...(o.detalle ? { description: o.detalle } : {}),
      ...(o.duracion ? { duration: o.duracion } : {}),
    }),

  info: (mensaje: string, o: Opciones = {}) =>
    toast(mensaje, {
      icon: ICONOS.info,
      ...(o.detalle ? { description: o.detalle } : {}),
      ...(o.duracion ? { duration: o.duracion } : {}),
    }),

  /**
   * Aviso atado a una operación: muestra "cargando", y al terminar se convierte en
   * éxito o error. Es lo correcto para guardar y enviar — el usuario ve que algo
   * está pasando y después cómo terminó, en el mismo lugar.
   */
  promesa: <T,>(
    promesa: Promise<T>,
    textos: { readonly cargando: string; readonly exito: string; readonly error: string },
  ) =>
    toast.promise(promesa, {
      loading: textos.cargando,
      success: textos.exito,
      error: (e: unknown) => (e instanceof Error ? e.message : textos.error),
    }),
};
