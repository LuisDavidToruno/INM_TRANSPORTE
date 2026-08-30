import type { ReactElement } from 'react';
import { ArrowRight, FileText, ShieldX, UserRoundSearch } from 'lucide-react';
import { Link } from 'react-router';

import type { BloqueoDuro } from '../api/misiones';

/**
 * `PT-004` — El patrón de pantalla de bloqueo duro.
 *
 * ── `R-3`: es una pantalla, no un cartel rojo ───────────────────────────────
 * <i>«Segregación de funciones, licencia no habilitante, documentación vencida, saldo
 * insuficiente: son bloqueos duros sin botón de "continuar de todos modos". Una pantalla de
 * bloqueo tiene siempre tres partes: qué se impidió · por qué exactamente, con nombres y
 * números · cuál es el camino de salida.»</i>
 *
 * ── Por qué esto reemplaza a un aviso flotante ──────────────────────────────
 * Un bloqueo mostrado como toast <b>se va solo</b>. Desaparece antes de que alguien pueda leer
 * la placa, la categoría que falta o el monto, y desaparece del todo si la persona parpadeó.
 * Eso es exactamente el cartel rojo que `R-3` rechaza: deja al usuario sabiendo que algo falló
 * y sin nada con qué actuar, que es la definición de la llamada a soporte.
 *
 * ── Y no hay botón de continuar ─────────────────────────────────────────────
 * Deliberado, y es la mitad de `R-4`: la advertencia sí deja seguir cobrando el peaje de un
 * motivo escrito, y el bloqueo no. <b>Si los dos se parecen, el usuario deja de leer ambos</b>,
 * así que este componente no ofrece ninguna acción que avance — sólo salidas que resuelven.
 */
export default function Bloqueo({
  bloqueo,
  queSeImpidio,
  onVolver,
}: {
  readonly bloqueo: BloqueoDuro;
  /** El acto que no se pudo hacer, en las palabras de quien lo intentaba. */
  readonly queSeImpidio: string;
  readonly onVolver?: () => void;
}): ReactElement {
  return (
    <section
      // `alert` y no `status`: interrumpe a quien usa lector de pantalla, porque la acción que
      // pidió no ocurrió y seguir escribiendo no sirve de nada (`RNF-16`).
      role="alert"
      className="tw:flex tw:flex-col tw:gap-4 tw:rounded-panel tw:border tw:border-riesgo-fg tw:bg-panel tw:p-4"
    >
      {/* ── 1 · Qué se impidió ────────────────────────────────────────────── */}
      <header className="tw:flex tw:items-start tw:gap-3">
        <ShieldX className="tw:mt-0.5 tw:size-5 tw:shrink-0 tw:text-riesgo-fg" aria-hidden />
        <div className="tw:flex tw:flex-col tw:gap-1">
          <h2 className="tw:font-semibold tw:tracking-tight">No se pudo {queSeImpidio}</h2>
          <p className="tw:text-xs tw:text-tinta-mid">
            Lo detuvo{' '}
            <code className="tw:font-mono tw:text-xs tw:text-tinta-hi">
              {bloqueo.precondicion}
            </code>
            . Es un <b>bloqueo duro</b>: no hay forma de continuar de todos modos, y por eso no
            se ofrece ninguna.
          </p>
        </div>
      </header>

      {/* ── 2 · Por qué exactamente, con nombres y números ────────────────── */}
      <p className="tw:border-l-2 tw:border-riesgo-fg tw:pl-3 tw:text-sm">{bloqueo.message}</p>

      {/* ── 3 · Cuál es el camino de salida ───────────────────────────────── */}
      {bloqueo.salida === null ? (
        <p className="tw:text-sm tw:text-tinta-mid">
          <b>No hay un camino de salida documentado para este bloqueo.</b> Se dice así en vez de
          sugerir a quién acudir, porque una sugerencia equivocada manda a alguien a una oficina
          que no lo resuelve — y lo manda con la confianza de estar leyendo al sistema.
        </p>
      ) : (
        <div className="tw:flex tw:flex-col tw:gap-2">
          <div className="tw:flex tw:items-start tw:gap-2">
            <ArrowRight className="tw:mt-0.5 tw:size-4 tw:shrink-0 tw:text-tinta-mid" aria-hidden />
            <p className="tw:text-sm">{bloqueo.salida.quePuedeHacer}</p>
          </div>

          {bloqueo.salida.aQuienAcudir !== null && (
            <div className="tw:flex tw:items-start tw:gap-2">
              <UserRoundSearch
                className="tw:mt-0.5 tw:size-4 tw:shrink-0 tw:text-tinta-mid"
                aria-hidden
              />
              <p className="tw:text-sm tw:text-tinta-mid">
                A quién acudir: {bloqueo.salida.aQuienAcudir}
              </p>
            </div>
          )}

          {bloqueo.salida.ficha !== null && (
            <div className="tw:flex tw:items-start tw:gap-2">
              <FileText className="tw:mt-0.5 tw:size-4 tw:shrink-0 tw:text-tinta-mid" aria-hidden />
              <p className="tw:font-mono tw:text-xs tw:text-tinta-mid">{bloqueo.salida.ficha}</p>
            </div>
          )}
        </div>
      )}

      {onVolver !== undefined && (
        <div>
          {/* La única acción, y no avanza: vuelve. Un botón que avanza acá sería el «continuar
              de todos modos» que la regla prohíbe, con otro nombre. */}
          <button
            type="button"
            onClick={onVolver}
            className="loki-foco tw:rounded-control tw:border tw:border-linea tw:px-3 tw:py-1.5 tw:text-cuerpo-2 tw:hover:border-tinta-mid"
          >
            Volver sin hacer el cambio
          </button>
        </div>
      )}
    </section>
  );
}

/**
 * El bloqueo de segregación, que trae su propia salida: el escalamiento de §5.3.B.3.
 *
 * Se separa porque el par `I-nn` **dice por dónde se sale** y una precondición `BD-nn` no: la
 * pantalla puede ofrecer el escalamiento como una acción concreta en vez de describirla.
 */
export function BloqueoPorSegregacion({
  par,
  mensaje,
  escalarA,
  salida,
}: {
  readonly par: string;
  readonly mensaje: string;
  readonly escalarA: string | null;
  readonly salida: { quePuedeHacer: string; aQuienAcudir: string | null } | null;
}): ReactElement {
  return (
    <section
      role="alert"
      className="tw:flex tw:flex-col tw:gap-4 tw:rounded-panel tw:border tw:border-riesgo-fg tw:bg-panel tw:p-4"
    >
      <header className="tw:flex tw:items-start tw:gap-3">
        <ShieldX className="tw:mt-0.5 tw:size-5 tw:shrink-0 tw:text-riesgo-fg" aria-hidden />
        <div className="tw:flex tw:flex-col tw:gap-1">
          <h2 className="tw:font-semibold tw:tracking-tight">
            Segregación de funciones — {par}
          </h2>
          <p className="tw:text-xs tw:text-tinta-mid">
            Quien solicita, quien autoriza, quien despacha, quien entrega el fondo y quien
            liquida <b>no pueden ser la misma persona</b> en el mismo expediente.
          </p>
        </div>
      </header>

      <p className="tw:border-l-2 tw:border-riesgo-fg tw:pl-3 tw:text-sm">{mensaje}</p>

      {salida !== null && <p className="tw:text-sm">{salida.quePuedeHacer}</p>}

      {escalarA !== null && (
        <p className="tw:text-sm tw:text-tinta-mid">
          El acto se escala a <b>{escalarA}</b>. Queda en la{' '}
          <Link to="/tareas" className="loki-foco tw:underline tw:underline-offset-2">
            bandeja de tareas
          </Link>{' '}
          de quien corresponde, con el motivo del bloqueo.
        </p>
      )}
    </section>
  );
}
