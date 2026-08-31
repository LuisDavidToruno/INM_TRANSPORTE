/**
 * La sesión: el token con que esta oficina habla con SIGTI.
 *
 * ── Qué cambió, y por qué importa ───────────────────────────────────────────
 * Hasta acá la oficina **declaraba** quién actuaba: elegía un puesto de una lista y mandaba esa
 * persona en el cuerpo de cada petición. El servidor le creía. Eso dejaba inerte todo el aparato
 * de control: `BD-06` comparaba a quien liquidó contra quien cierra —dos cadenas que mandaba la
 * misma pantalla—, y la bitácora encadenada registraba, con hash y todo, <b>lo que el cliente
 * dijo ser</b>.
 *
 * Ahora la identidad la emite el **servicio de identidad institucional** —`ARGOS_API` en el
 * piloto del INM— y viaja firmada. La oficina ya no puede decir quién es: sólo puede presentar
 * el token que le dieron.
 *
 * ── El token vive en memoria, no en `localStorage` ──────────────────────────
 * Es la diferencia entre «alguien con acceso al navegador puede leer la sesión» y «no puede».
 * Cuesta tener que volver a entrar al recargar la página, y ese costo se paga: un token en
 * `localStorage` lo lee cualquier script que llegue a correr en el origen.
 *
 * El puesto elegido sí se guarda —no es una credencial, es una preferencia—, y sigue en
 * `puesto.tsx`.
 */

/** Dónde vive el emisor de identidad. En el piloto del INM, `ARGOS_API`. */
const IDENTIDAD = (import.meta.env.VITE_IDENTIDAD as string | undefined) ?? '';

export interface Sesion {
  readonly token: string;
  /** El identificador con que el sistema atribuye los actos. Sale del token, no de una lista. */
  readonly persona: string;
  readonly nombre: string;
  readonly roles: readonly string[];
  readonly expira: Date;
}

let sesion: Sesion | null = null;

/** Quién avisa a la aplicación que la sesión cambió, para que vuelva a dibujar. */
const oyentes = new Set<() => void>();

export function sesionActual(): Sesion | null {
  // ⚠️ **Vencida es igual que ausente.** Devolver una sesión expirada haría que la pantalla
  // muestre el nombre de alguien y que cada petición devuelva 401 sin explicación.
  if (sesion !== null && sesion.expira.getTime() <= Date.now()) sesion = null;

  return sesion;
}

export function suscribirseALaSesion(oyente: () => void): () => void {
  oyentes.add(oyente);
  return () => oyentes.delete(oyente);
}

function fijar(nueva: Sesion | null): void {
  sesion = nueva;
  oyentes.forEach((o) => o());
}

/**
 * Pide el token al servicio de identidad.
 *
 * ── ⚠️ La contraseña no se guarda en ningún lado ────────────────────────────
 * Entra por el formulario, viaja en esta petición y se descarta. No va a `localStorage`, no
 * queda en un estado de React que sobreviva, y no se registra.
 */
export async function entrar(usuario: string, clave: string): Promise<void> {
  if (IDENTIDAD === '') {
    throw new Error(
      'No hay servicio de identidad configurado (VITE_IDENTIDAD). Sin él la oficina no puede ' +
        'obtener un token, y SIGTI no atiende a nadie sin identidad.',
    );
  }

  const respuesta = await fetch(`${IDENTIDAD}/api/v1/auth/token`, {
    method: 'POST',
    headers: { 'content-type': 'application/json' },
    body: JSON.stringify({ usuario, clave }),
  });

  if (!respuesta.ok) {
    const cuerpo = (await respuesta.json().catch(() => ({}))) as { mensaje?: string };

    // El servicio distingue dos cosas y la pantalla tiene que mostrarlo: con `401` la
    // credencial está mal y hay que volver a escribirla; con `403` la credencial es correcta
    // y el problema es de la cuenta — repetirla no arregla nada.
    throw new Error(cuerpo.mensaje ?? 'No se pudo iniciar sesión.');
  }

  const datos = (await respuesta.json()) as {
    token: string;
    persona: string;
    nombre: string;
    roles: string[];
    expira: string;
  };

  fijar({
    token: datos.token,
    persona: datos.persona,
    nombre: datos.nombre,
    roles: datos.roles,
    expira: new Date(datos.expira),
  });
}

export function salir(): void {
  fijar(null);
}
