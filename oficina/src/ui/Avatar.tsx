import type { CSSProperties, ReactElement } from 'react';

/**
 * Avatar. Canon: COMPONENTS.md §2.
 *
 * Sin foto cae a iniciales, que es el caso normal en este sistema: casi nadie
 * tiene foto cargada. Las iniciales salen del nombre real —primera letra del
 * primer nombre y del primer apellido— y no de un recorte ciego de dos
 * caracteres, que con «Nolvia Esperanza Cruz Interiano de Villanueva» daría «NO».
 */
export interface AvatarProps {
  nombre: string;
  url?: string;
  tamano?: number;
}

export function iniciales(nombre: string): string {
  const partes = nombre.trim().split(/\s+/).filter(Boolean);
  if (partes.length === 0) return '?';
  const primera = partes[0]?.[0] ?? '';
  // La segunda inicial es del primer APELLIDO, que en un nombre hondureño de
  // cuatro partes es la tercera palabra, no la segunda.
  const indiceApellido = partes.length >= 3 ? 2 : 1;
  const segunda = partes[indiceApellido]?.[0] ?? '';
  return (primera + segunda).toUpperCase();
}

export default function Avatar({ nombre, url, tamano = 30 }: AvatarProps): ReactElement {
  const estilo: CSSProperties = { width: tamano, height: tamano };

  if (url !== undefined && url !== '') {
    return (
      <img
        src={url}
        alt=""
        style={estilo}
        className="loki-avatar tw:rounded-badge tw:object-cover"
      />
    );
  }

  return (
    <span
      style={estilo}
      title={nombre}
      className="loki-avatar tono-neutro tw:inline-flex tw:items-center tw:justify-center tw:rounded-badge tw:border tw:font-semibold"
      aria-hidden="true"
    >
      {iniciales(nombre)}
    </span>
  );
}
