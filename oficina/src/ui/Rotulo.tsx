import type { ReactElement, ReactNode } from 'react';

/**
 * El rótulo de un dato: versalitas, 10 px, en tinta baja.
 *
 * ── Por qué es un componente y no una clase suelta ──────────────────────────
 * La clase `loki-rotulo` ya era del contrato, pero **no ponía el tamaño ni el color**: sólo
 * la caja, el espaciado y el peso. Así que cada uso tenía que escribir el trío completo
 * —`loki-rotulo tw:text-cabecera tw:text-tinta-low`— y estaba escrito **111 veces en 31
 * archivos**. Tres clases repetidas ciento once veces no son una convención: son ciento once
 * oportunidades de escribir dos.
 *
 * Y de hecho ya había divergido. El `Rotulo` de `componentes/ui` ponía las tres; el
 * Expediente, al migrar, escribió sólo dos en un sitio y las tres en otro. Ninguna de las dos
 * versiones está mal a la vista — el tamaño se hereda del contenedor y a veces coincide — pero
 * que coincida *a veces* es justamente lo que hace que nadie lo note.
 *
 * ── Dónde va ────────────────────────────────────────────────────────────────
 * Encima del dato que nombra: «GERENCIA» sobre el nombre de la gerencia, «PLAZO» sobre las
 * horas. **No es un título de sección** —para eso está el `titulo` del `Panel`— ni una
 * cabecera de tabla, que la pone la `Tabla`.
 *
 * `como` existe porque el rótulo aparece dentro de un `<label>` tanto como suelto, y un `<p>`
 * anidado en un `<label>` es marcado inválido. Por omisión es `<p>`, que es el caso común.
 */
export interface RotuloProps {
  children: ReactNode;
  /** `span` cuando va dentro de un `<label>` u otro elemento en línea. */
  como?: 'p' | 'span';
}

export default function Rotulo({ children, como = 'p' }: RotuloProps): ReactElement {
  const Etiqueta = como;
  return (
    <Etiqueta className="loki-rotulo tw:text-cabecera tw:text-tinta-low">{children}</Etiqueta>
  );
}
