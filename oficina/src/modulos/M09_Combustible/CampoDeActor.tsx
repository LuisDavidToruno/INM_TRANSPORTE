import type { ReactElement } from 'react';

import { Campo } from '../../ui';

/**
 * Quién ejecuta el acto.
 *
 * ── Por qué esto se declara y no se toma del usuario ────────────────────────
 * Porque <b>todavía no hay usuario</b>: `M-01` no está construido y la oficina opera con una
 * sola identidad. Y el circuito del combustible es precisamente el que <b>no funciona con una
 * sola persona</b>: `RN-26.4` exige que quien solicita, quien aprueba y quien liquida sean tres
 * distintas, y `BD-06` pide lo mismo sobre el vale — emite, entrega, consume, liquida, concilia.
 *
 * Con el nombre cableado, la pantalla no podía mover un solo fondo más allá de `Solicitado`:
 * <b>medido, el bloqueo de `RN-26.4` disparaba contra el propio operador</b>. Declararlo no es
 * un rodeo: es lo que hace operable el circuito mientras no exista autenticación, y de paso
 * pone la segregación a la vista en vez de esconderla.
 *
 * ⚠️ <b>Esto es provisional y no es un control de acceso.</b> Nada impide teclear el nombre de
 * otra persona; lo que hay es el registro de quién dijo haberlo hecho. Cuando exista `M-01`,
 * este campo desaparece y el actor sale del usuario autenticado — que es lo único que convierte
 * la segregación en un control y no en una declaración.
 */
export default function CampoDeActor({
  valor,
  onCambiar,
  etiqueta,
  ayuda,
}: {
  valor: string;
  onCambiar(v: string): void;
  etiqueta: string;
  ayuda: string;
}): ReactElement {
  return (
    <Campo etiqueta={etiqueta} obligatorio ayuda={ayuda}>
      {(props) => (
        <input
          {...props}
          value={valor}
          onChange={(e) => onCambiar(e.target.value)}
          placeholder="Nombre de la persona"
        />
      )}
    </Campo>
  );
}
