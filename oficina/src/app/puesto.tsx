import type { ReactElement, ReactNode } from 'react';
import { createContext, useCallback, useContext, useMemo, useState } from 'react';

/**
 * Con qué puesto está trabajando la persona — `R-1`.
 *
 * ── Por qué esto existe ─────────────────────────────────────────────────────
 * Antes de esto, nueve pantallas pasaban `'Rolando Discua'` **escrito a mano** como el actor de
 * cada acto: autorizar, programar, despachar, declarar el estado de un vehículo. Eso hacía dos
 * daños a la vez. El obvio: nada de lo que el sistema registra dice quién lo hizo de verdad.
 * Y el que importa: **la segregación de funciones queda inerte** — `I-01` a `I-19` comparan al
 * actor del acto contra los actos previos del mismo expediente, y si el actor es siempre la
 * misma constante, lo que comparan no es a nadie.
 *
 * ── Lo que esto NO es ───────────────────────────────────────────────────────
 * **No es autenticación.** Nadie verifica que quien elige el puesto tenga derecho a ocuparlo:
 * el servidor recibe la persona como un parámetro y la cree. Mientras eso siga así, el alcance
 * de datos es un filtro de presentación y no un control de acceso, y llamarlo control sería
 * describir una protección que no existe.
 */
interface PuestoElegido {
  readonly persona: string;
  readonly puesto: string;
  readonly denominacion: string | null;
}

interface Contexto {
  readonly elegido: PuestoElegido | null;
  elegir(p: PuestoElegido): void;
  salir(): void;
}

const ContextoDelPuesto = createContext<Contexto | null>(null);

const CLAVE = 'sigti.puesto';

function leerGuardado(): PuestoElegido | null {
  try {
    const crudo = localStorage.getItem(CLAVE);
    if (crudo === null) return null;

    const p = JSON.parse(crudo) as Partial<PuestoElegido>;

    // Se valida lo leído: un `localStorage` de una versión anterior puede tener otra forma, y
    // un objeto a medias produciría llamadas con la persona en `undefined` — que el servidor
    // aceptaría como un identificador más.
    return typeof p.persona === 'string' && typeof p.puesto === 'string'
      ? { persona: p.persona, puesto: p.puesto, denominacion: p.denominacion ?? null }
      : null;
  } catch {
    // Modo privado, almacenamiento bloqueado o JSON corrupto. Sin puesto elegido se va a la
    // pantalla de ingreso, que es exactamente lo que corresponde.
    return null;
  }
}

export function ProveedorDelPuesto({ children }: { children: ReactNode }): ReactElement {
  const [elegido, setElegido] = useState<PuestoElegido | null>(leerGuardado);

  const elegir = useCallback((p: PuestoElegido) => {
    setElegido(p);
    try {
      localStorage.setItem(CLAVE, JSON.stringify(p));
    } catch {
      // Si no se puede guardar, la elección vale para esta sesión y se vuelve a pedir en la
      // próxima. Es molesto y es correcto: peor sería fallar la elección entera.
    }
  }, []);

  const salir = useCallback(() => {
    setElegido(null);
    try {
      localStorage.removeItem(CLAVE);
    } catch {
      /* nada que hacer */
    }
  }, []);

  const valor = useMemo(() => ({ elegido, elegir, salir }), [elegido, elegir, salir]);

  return <ContextoDelPuesto value={valor}>{children}</ContextoDelPuesto>;
}

export function usarPuesto(): Contexto {
  const ctx = useContext(ContextoDelPuesto);
  if (ctx === null) throw new Error('usarPuesto fuera de ProveedorDelPuesto.');
  return ctx;
}

/**
 * Quién ejecuta el acto que se va a registrar.
 *
 * Lanza cuando no hay puesto elegido. **Devolver un valor por omisión sería volver al problema
 * que esto resuelve**: un acto registrado a nombre de una constante. Que falle ruidosamente en
 * desarrollo es preferible a que se guarde un asiento con el autor equivocado, porque el asiento
 * no se puede corregir después — sólo reversar.
 */
export function usarQuienEjecuta(): string {
  const { elegido } = usarPuesto();

  if (elegido === null)
    throw new Error(
      'No hay puesto vigente elegido, y toda acción registra a su autor. ' +
        'Elija el puesto en la pantalla de ingreso.',
    );

  return elegido.persona;
}
