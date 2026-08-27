import { CircleAlert, CircleCheck, Info, TriangleAlert } from 'lucide-react';
import { Toaster as ToasterSonner, toast } from 'sonner';
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
 */

/** Va UNA vez, en el shell. Sin esto los `avisar.*` no muestran nada. */
export function Avisos(): ReactElement {
  const { tema } = useTema();
  const base = TEMAS[tema].base;

  return (
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
  );
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
