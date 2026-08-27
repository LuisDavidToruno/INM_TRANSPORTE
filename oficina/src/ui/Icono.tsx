import * as lucide from 'lucide-react';
import type { ReactElement } from 'react';

/**
 * Resuelve el nombre de ícono que emite el servidor a un componente de Lucide.
 *
 * ── El hueco que resultó no ser un hueco ─────────────────────────────────────
 * La bitácora daba esto por bloqueante: «NavigationService da nombres de Font
 * Awesome; el stack nuevo usa lucide-react. Adivinar produce íconos en blanco,
 * así que hoy el menú va SIN íconos».
 *
 * La premisa era falsa. Los 53 nombres que emite el menú son de **Feather**
 * (`activity`, `file-text`, `map-pin`…), y **lucide-react es un fork de
 * Feather**: conservó los nombres del set original. Medido contra los exports
 * reales del paquete instalado: **52 de 53 existen tal cual** con sólo pasar de
 * kebab-case a PascalCase.
 *
 * El único que no es `tool`, que Lucide renombró a `Wrench`. Un alias, no una
 * tabla de 53 entradas.
 *
 * ── Por qué igual hay un respaldo ────────────────────────────────────────────
 * El menú lo arma el servidor y puede sumar un ítem con un ícono nuevo sin que
 * el frontend se entere. Un nombre desconocido cae a un punto neutro en vez de
 * romper el riel o dejar un hueco: el ítem se sigue pudiendo leer y hacer clic.
 *
 * Contrato: una sola familia, trazo 1.8, `currentColor`, tamaños 13/15/18/22/34
 * y nunca uno intermedio.
 */

type ComponenteIcono = (props: {
  size?: number;
  strokeWidth?: number;
  className?: string;
  'aria-hidden'?: boolean;
}) => ReactElement;

const ALIAS: Record<string, string> = {
  // Feather lo llamaba `tool`; Lucide lo renombró.
  tool: 'Wrench',
};

function aPascal(nombre: string): string {
  return nombre
    .split('-')
    .filter(Boolean)
    .map((p) => p[0]!.toUpperCase() + p.slice(1))
    .join('');
}

const catalogo = lucide as unknown as Record<string, ComponenteIcono | undefined>;

export interface IconoProps {
  /** El nombre tal como lo emite el servidor: kebab-case de Feather. */
  nombre: string;
  /** 13 en pastilla · 15 en botón y menú · 18 en banda y cabecera · 22 en acción destacada · 34 en vacío. */
  tamano?: 13 | 15 | 18 | 22 | 34;
  className?: string;
}

export default function Icono({ nombre, tamano = 15, className }: IconoProps): ReactElement {
  const clave = ALIAS[nombre] ?? aPascal(nombre);
  const Componente = catalogo[clave];

  if (Componente === undefined) {
    // Desconocido: punto neutro. El ítem sigue siendo legible y clicable, que es
    // más de lo que se gana dejando el hueco o reventando el riel.
    return (
      <span
        className={['loki-icono-desconocido', className].filter(Boolean).join(' ')}
        style={{ width: tamano, height: tamano }}
        aria-hidden="true"
      />
    );
  }

  return <Componente size={tamano} strokeWidth={1.8} className={className} aria-hidden={true} />;
}
