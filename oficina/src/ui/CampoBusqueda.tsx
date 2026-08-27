import { Search, X } from 'lucide-react';
import { useId, useRef } from 'react';
import type { AriaAttributes, KeyboardEvent, ReactElement, RefObject } from 'react';

import Boton from './Boton';

/**
 * Campo de búsqueda del sistema.
 *
 * ── Las dos familias, y por qué el componente distingue ───────────────────────
 * Se midieron los 12 buscadores del frontend nuevo y no eran doce cosas: eran **dos**.
 *
 * | | Quién | Cómo se comporta |
 * |---|---|---|
 * | **Filtra** (defecto) | Recorta una lista que **ya está** en el cliente | Filtra a cada tecla. No hay nada que esperar |
 * | **Consulta** (`alBuscar`) | **Le pregunta al servidor** | Se dispara con Enter o con el botón, **no** a cada tecla |
 *
 * Mezclarlas es el defecto caro: si «Consulta» filtrara a cada tecla, escribir
 * «Katherin» son ocho viajes al servidor de los que **siete se descartan** — y el
 * último que conteste gana, que no siempre es el último que se pidió.
 *
 * 🚫 **Un botón «Buscar» en el modo Filtra sobra y confunde**: sugiere que hay que
 * pulsarlo para ver algo, cuando la lista ya se recortó mientras escribía.
 *
 * ── Lo que trae siempre, y qué faltaba ───────────────────────────────────────
 * De los 12 medidos: 7 eran `type="search"`, **6** traían ícono y **2** tenían con qué
 * borrar. El componente da los tres a los 12.
 *
 * ⚠️ **La ✕ es NUESTRA, no la del navegador.** `type="search"` dibuja una en WebKit y
 * **ninguna en Firefox**, así que la mitad de la gente no tenía forma visible de
 * limpiar. La nativa se apaga en el CSS del contrato (`.loki-find input[type=search]`,
 * con `::-webkit-search-cancel-button`) — sin eso Chrome mostraría **dos** cruces.
 *
 * La nuestra **devuelve el foco al campo**: sin eso, quien navega con teclado limpia y
 * queda con el foco en un botón que acaba de desaparecer, o sea en ninguna parte.
 */
export default function CampoBusqueda({
  etiqueta,
  valor,
  onCambio,
  alBuscar,
  textoBoton = 'Buscar',
  autoFoco = false,
  className,
  deshabilitado = false,
  marco = true,
  refCampo,
  alTeclado,
  atributos,
  alEnfocar,
}: {
  /**
   * Qué se busca, en palabras del usuario: «Buscar por solicitud, memo, solicitante…».
   * Es el `placeholder` **y** el nombre accesible — no hay rótulo aparte que se pueda
   * desincronizar del texto que se ve.
   */
  readonly etiqueta: string;
  readonly valor: string;
  readonly onCambio: (valor: string) => void;
  /**
   * Presente ⇒ **modo Consulta**: se dispara con Enter o con el botón, nunca al tipear.
   * Ausente ⇒ **modo Filtra**: `onCambio` ya recorta la lista.
   */
  readonly alBuscar?: (valor: string) => void;
  readonly textoBoton?: string;
  readonly autoFoco?: boolean;
  readonly className?: string;
  /**
   * `false` cuando **quien lo monta ya pone su propia caja** — la cabecera de la paleta de
   * comandos, el marco de un combobox. Se va el borde y el alto fijo; **NO** se van el ícono,
   * la ✕, el nombre accesible ni el apagado de la ✕ nativa, que es de lo que se trata.
   */
  readonly marco?: boolean;
  /** Para que quien lo monta pueda devolverle el foco (abrir un desplegable, cerrar un modal). */
  readonly refCampo?: RefObject<HTMLInputElement | null>;
  /**
   * Corre **antes** que el teclado propio. Si el anfitrión hace `preventDefault()`, el campo no
   * actúa — así la paleta se queda con `Escape` (cerrar) y el combobox con ↑ ↓ Enter, sin que
   * dos manejadores peleen por la misma tecla.
   */
  readonly alTeclado?: (e: KeyboardEvent<HTMLInputElement>) => void;
  /**
   * Atributos extra para el `input`: `role`, `aria-expanded`, `aria-activedescendant`… Existe
   * para el patrón combobox, que necesita ARIA que un campo de búsqueda suelto no tiene.
   * **No se usa para estilos** — para eso está `className`.
   */
  readonly atributos?: AriaAttributes & { readonly id?: string; readonly role?: string };
  readonly alEnfocar?: () => void;
  readonly deshabilitado?: boolean;
}): ReactElement {
  const idGenerado = useId();
  const id = atributos?.id ?? idGenerado;
  const propio = useRef<HTMLInputElement>(null);
  const campo = refCampo ?? propio;
  const esConsulta = alBuscar !== undefined;

  const limpiar = (): void => {
    onCambio('');
    // En modo Consulta, vaciar el campo tiene que **rehacer la consulta**: si no, la
    // lista se queda mostrando el resultado de una búsqueda que ya no está escrita.
    if (esConsulta) alBuscar('');
    campo.current?.focus();
  };

  return (
    <div className={`tw:flex tw:items-center tw:gap-2 ${className ?? ''}`.trim()}>
      <label className={marco ? 'loki-find' : 'loki-find-plano'} htmlFor={id}>
        <Search size={14} className="tw:shrink-0" aria-hidden="true" />
        <input
          {...atributos}
          ref={campo}
          id={id}
          type="search"
          value={valor}
          autoFocus={autoFoco}
          disabled={deshabilitado}
          placeholder={etiqueta}
          aria-label={etiqueta}
          onFocus={alEnfocar}
          onChange={(e) => onCambio(e.target.value)}
          onKeyDown={(e) => {
            // El anfitrión primero: si se queda con la tecla, acá no se hace nada más.
            alTeclado?.(e);
            if (e.defaultPrevented) return;

            if (esConsulta && e.key === 'Enter') {
              e.preventDefault();
              alBuscar(valor);
            }
            // Escape limpia, como en cualquier buscador. Es lo que la gente ya intenta.
            if (e.key === 'Escape' && valor !== '') {
              e.preventDefault();
              limpiar();
            }
          }}
        />
        {valor !== '' && !deshabilitado && (
          <button
            type="button"
            onClick={limpiar}
            aria-label="Limpiar la búsqueda"
            className="loki-foco tw:shrink-0 tw:rounded-full tw:p-0.5 tw:text-tinta-low tw:transition tw:duration-150 tw:hover:text-tinta-hi"
          >
            <X size={13} aria-hidden="true" />
          </button>
        )}
      </label>

      {esConsulta && (
        <Boton tamano="sm" onClick={() => alBuscar(valor)} className="tw:shrink-0">
          {textoBoton}
        </Boton>
      )}
    </div>
  );
}
