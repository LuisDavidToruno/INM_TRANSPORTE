import type { ReactElement, ReactNode } from 'react';

/**
 * Estado vacío. Canon: COMPONENTS.md §2.
 *
 * Nunca una tabla vacía a secas: siempre LA acción que resuelve el vacío. El
 * texto dice además qué hay en otro lado — «la bandeja del área sigue teniendo 31
 * sin asignar» —, porque un vacío sin contexto se lee como sistema roto.
 *
 * La descripción se acota a 34 caracteres de ancho: un párrafo centrado más largo
 * obliga a barrer con la vista y deja de leerse de un vistazo.
 */
export interface VacioProps {
  /** 34 px. */
  icono: ReactNode;
  titulo: string;
  descripcion: string;
  /** LA acción que resuelve el vacío. */
  accion?: ReactNode;
  /**
   * Nivel del encabezado. Mismo criterio que en `Panel`: `3` por omisión, y
   * `2` cuando el vacío cuelga directo del `h1` de la página sin ninguna
   * sección en medio. El tamaño no cambia con él.
   */
  nivel?: 2 | 3 | 4;
}

export default function Vacio({
  icono,
  titulo,
  descripcion,
  accion,
  nivel = 3,
}: VacioProps): ReactElement {
  const Encabezado = `h${nivel}` as 'h2' | 'h3' | 'h4';

  return (
    // `.vacio`: rejilla centrada, 38px 22px de relleno, gap 10.
    <div className="tw:grid tw:place-items-center tw:gap-2.5 tw:px-[22px] tw:py-[38px] tw:text-center">
      <span className="loki-icono-vacio tw:text-tinta-axis" aria-hidden="true">
        {icono}
      </span>
      {/* Serifa 15px — más chico que un título de sección: es un mensaje, no un encabezado. */}
      <Encabezado className="tw:font-serif tw:text-[15px] tw:font-semibold tw:text-tinta-hi">
        {titulo}
      </Encabezado>
      <p className="loki-vacio-texto tw:text-cuerpo-2 tw:leading-[1.55] tw:text-tinta-low">
        {descripcion}
      </p>
      {accion !== undefined ? <div className="tw:mt-2">{accion}</div> : null}
    </div>
  );
}
