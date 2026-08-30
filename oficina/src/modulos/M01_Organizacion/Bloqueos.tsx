import type { ReactElement } from 'react';
import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { CircleAlert } from 'lucide-react';

import { Campo, Nota, Panel, SelectorBuscable } from '../../ui';
import Bloqueo from '../../ui/Bloqueo';
import { BloqueoDuro, pedir } from '../../api/misiones';
import type { CaminoDeSalida } from '../../api/misiones';

/**
 * `PT-004` — El patrón de pantalla de bloqueo duro, con las precondiciones reales del sistema.
 *
 * ── Por qué el patrón necesita su propia pantalla ───────────────────────────
 * `PT-004` es lo que el inventario llama un <b>patrón</b>: no tiene datos propios, se aplica
 * dentro de otras pantallas. Pero un patrón que sólo existe repartido no se puede revisar —
 * nadie puede contestar «¿cómo se ve un bloqueo?» sin provocar uno—, y las siete historias que
 * lo citan quedarían sin nada que señalar.
 *
 * Acá se ve cada precondición con su camino de salida, y se puede juzgar si el texto sirve
 * <b>antes</b> de que alguien quede detenido frente a él en el predio a las seis de la mañana.
 */
export default function Bloqueos(): ReactElement {
  const [elegida, setElegida] = useState('BD-01');

  const { data, isPending, isError } = useQuery({
    queryKey: ['salidas'],
    queryFn: () => pedir<Salidas>('/bloqueos'),
  });

  if (isError) {
    return (
      <Nota tono="riesgo" icono={<CircleAlert />}>
        No se pudo cargar el catálogo de precondiciones.
      </Nota>
    );
  }

  const actual = data?.precondiciones.find((p) => p.precondicion === elegida);

  return (
    <div className="tw:flex tw:flex-col tw:gap-5">
      <header className="tw:flex tw:flex-col tw:gap-1">
        <h1 className="tw:text-xl tw:font-semibold tw:tracking-tight">
          Patrón de bloqueo duro
        </h1>
        <p className="tw:text-sm tw:text-tinta-mid">
          Cómo se ve un bloqueo, con sus <b>tres partes</b>: qué se impidió, por qué exactamente
          — con nombres y números —, y cuál es el camino de salida.
        </p>
      </header>

      <Nota tono="info">
        <b>Un bloqueo duro no tiene botón de «continuar de todos modos».</b> Es lo que lo separa
        de una advertencia, que sí deja seguir cobrando el peaje de un motivo escrito. Si los dos
        se parecieran, la gente dejaría de leer los dos — por eso este patrón no ofrece ninguna
        acción que avance.
      </Nota>

      <Panel titulo="Qué precondición">
        <div className="tw:sm:max-w-lg">
          <Campo
            etiqueta="Precondición"
            ayuda="Las trece de la sección 4 de la máquina de estados, más las del estado operativo del vehículo."
          >
            {(control) => (
              <SelectorBuscable
                {...control}
                valor={elegida}
                onCambio={setElegida}
                opciones={(data?.precondiciones ?? []).map((p) => ({
                  valor: p.precondicion,
                  etiqueta: `${p.precondicion} — ${p.titulo}`,
                  buscarTambien: p.salida?.quePuedeHacer ?? '',
                }))}
                vacio="Elija una precondición…"
              />
            )}
          </Campo>
        </div>
      </Panel>

      {isPending ? (
        <p className="tw:text-sm tw:text-tinta-mid">Cargando…</p>
      ) : actual === undefined ? (
        <Nota tono="aviso">Esa precondición no está en el catálogo.</Nota>
      ) : (
        <Bloqueo
          queSeImpidio={actual.titulo.toLowerCase()}
          bloqueo={
            new BloqueoDuro(
              actual.precondicion,
              // La segunda parte de `R-3` la aporta el dominio **en el momento**, con la placa,
              // la categoría o el monto del caso. Acá se dice que ahí van, en vez de inventar
              // un mensaje de ejemplo que después nadie reconocería en la pantalla real.
              `Se evalúa en ${actual.seEvaluaEn}. En el bloqueo real, esta línea trae el motivo ` +
                `con los nombres y números del caso: la placa, la categoría que falta, el saldo ` +
                `y el monto.`,
              actual.salida,
            )
          }
        />
      )}

    </div>
  );
}

interface Salidas {
  precondiciones: {
    precondicion: string;
    /** El título de su ficha en la autoridad, transcrito. */
    titulo: string;
    /** En qué transiciones se revalida. Contesta «¿por qué me apareció ahora?». */
    seEvaluaEn: string;
    salida: CaminoDeSalida;
  }[];
}
