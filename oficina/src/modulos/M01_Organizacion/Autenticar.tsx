import type { ReactElement } from 'react';
import { useState } from 'react';
import { KeyRound, ShieldCheck, TriangleAlert } from 'lucide-react';

import { Boton, Campo, Nota, Panel } from '../../ui';
import { entrar } from '../../app/sesion';

/**
 * El inicio de sesión — <b>y ahora sí lo es</b>.
 *
 * ── Lo que había antes, y por qué no servía ─────────────────────────────────
 * Esta pantalla elegía una persona de una lista desplegable y la mandaba en el cuerpo de cada
 * petición. Su propio texto lo advertía: <i>«no es un inicio de sesión, nadie verifica que quien
 * elige un puesto tenga derecho a ocuparlo»</i>. Y esa advertencia honesta describía un sistema
 * donde <b>la segregación de funciones no existía</b>: `BD-06` comparaba a quien liquidó contra
 * quien cierra, y las dos cadenas las escribía la misma pantalla.
 *
 * ── De dónde sale ahora la identidad ────────────────────────────────────────
 * Del <b>servicio de identidad institucional</b>. La misma cuenta con que se entra al sistema de
 * viáticos, la misma contraseña, y la misma baja: el día que alguien deja de trabajar en la
 * institución, cerrarle la cuenta allá le cierra ésta también. Sin padrón de contraseñas propio
 * no hay un segundo lugar del que acordarse.
 */
export default function Autenticar({ alEntrar }: { alEntrar: () => void }): ReactElement {
  const [usuario, setUsuario] = useState('');
  const [clave, setClave] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [entrando, setEntrando] = useState(false);

  async function enviar(e: React.FormEvent): Promise<void> {
    e.preventDefault();
    setError(null);
    setEntrando(true);

    try {
      await entrar(usuario.trim(), clave);

      // ⚠️ **La contraseña deja de existir apenas se usó.** No va a `localStorage`, no queda en
      // un estado que sobreviva, y no se registra en ningún lado.
      setClave('');
      alEntrar();
    } catch (falla) {
      setError(falla instanceof Error ? falla.message : 'No se pudo iniciar sesión.');
    } finally {
      setEntrando(false);
    }
  }

  return (
    <div className="tw:mx-auto tw:flex tw:max-w-md tw:flex-col tw:gap-5">
      <header className="tw:flex tw:flex-col tw:gap-1">
        <h1 className="tw:text-xl tw:font-semibold tw:tracking-tight">Entrar a SIGTI</h1>
        <p className="tw:text-sm tw:text-tinta-mid">
          Con su cuenta institucional — la misma del sistema de viáticos.
        </p>
      </header>

      <Panel>
        <form onSubmit={(e) => void enviar(e)} className="tw:flex tw:flex-col tw:gap-4">
          <Campo etiqueta="Usuario" obligatorio>
            {(control) => (
              <input
                {...control}
                autoFocus
                autoComplete="username"
                value={usuario}
                onChange={(e) => setUsuario(e.target.value)}
              />
            )}
          </Campo>

          <Campo etiqueta="Contraseña" obligatorio>
            {(control) => (
              <input
                {...control}
                type="password"
                autoComplete="current-password"
                value={clave}
                onChange={(e) => setClave(e.target.value)}
              />
            )}
          </Campo>

          {/* El servicio distingue dos cosas y la pantalla las muestra tal cual: con la
              credencial mal, hay que volver a escribirla; con la cuenta inhabilitada o sin
              empleado asociado, repetirla no arregla nada. */}
          {error !== null && (
            <Nota tono="riesgo" icono={<TriangleAlert />}>
              {error}
            </Nota>
          )}

          <Boton
            type="submit"
            cargando={entrando}
            disabled={entrando || usuario.trim() === '' || clave === ''}
            icono={<KeyRound />}
          >
            Entrar
          </Boton>
        </form>
      </Panel>

      <Nota tono="info" icono={<ShieldCheck />}>
        Su sesión <b>vive sólo mientras esta pestaña esté abierta</b>. Al recargar hay que volver
        a entrar: guardar la sesión en el navegador la deja al alcance de cualquier script que
        llegue a correr acá, y ese costo no compensa el ahorro.
      </Nota>
    </div>
  );
}
