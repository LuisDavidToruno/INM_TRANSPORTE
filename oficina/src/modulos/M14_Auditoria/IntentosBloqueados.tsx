import type { ReactElement } from 'react';
import { useQuery } from '@tanstack/react-query';
import { CircleAlert, ShieldAlert } from 'lucide-react';

import { Nota, Panel, Pastilla, Vacio } from '../../ui';
import { pedir } from '../../api/misiones';
import { diaYHora } from '../M06_Autorizacion/formato';

/**
 * `PT-091` — Intentos bloqueados por segregación de funciones.
 *
 * ── Por qué esta pantalla muestra lo que NO pasó ────────────────────────────
 * §5.3.B.2: *«el intento bloqueado es información de control, no ruido. Un mismo usuario
 * intentando quince veces autorizar sus propias solicitudes es exactamente lo que Auditoría
 * Interna quiere ver»*.
 *
 * Un sistema que sólo guarda lo consumado no puede contestar la pregunta que el TSC hace: **si
 * el control operó**. Sin esta pista, un bloqueo perfecto y un bloqueo que nunca se activó se
 * ven exactamente igual — no hay rastro de ninguno de los dos.
 *
 * ── La reincidencia va primero, y no es lo mismo que el total ───────────────
 * Un intento aislado suele ser una delegación chica resolviendo como puede. **Quince intentos
 * de la misma persona sobre el mismo par es otra cosa**, y ordenar la lista por fecha la
 * esconde entre los aislados.
 */
export default function IntentosBloqueados(): ReactElement {
  const { data, isPending, isError } = useQuery({
    queryKey: ['intentos-bloqueados'],
    queryFn: () => pedir<Pista>('/segregacion/intentos'),
  });

  if (isError) {
    return (
      <Nota tono="riesgo" icono={<CircleAlert />}>
        No se pudo cargar la pista de intentos bloqueados.
      </Nota>
    );
  }

  return (
    <div className="tw:flex tw:flex-col tw:gap-5">
      <header className="tw:flex tw:flex-col tw:gap-1">
        <h1 className="tw:text-xl tw:font-semibold tw:tracking-tight">
          Intentos bloqueados por segregación
        </h1>
        <p className="tw:text-sm tw:text-tinta-mid">
          Actos que el sistema <b>impidió consumar</b> porque quien los intentaba ya había
          ejercido una función incompatible sobre el mismo expediente. <b>No se guardó el
          acto</b>; sí el intento.
        </p>
      </header>

      {isPending ? (
        <p className="tw:text-sm tw:text-tinta-mid">Cargando la pista…</p>
      ) : data.total === 0 ? (
        // Cero no es «el control funciona»: es que nadie lo activó todavía. Decirlo evita que
        // una pantalla vacía se lea como un certificado.
        <Vacio
          icono={<ShieldAlert />}
          titulo="Ningún intento bloqueado"
          descripcion="Nadie ha intentado ejercer dos funciones incompatibles sobre el mismo expediente. Es lo esperable, y no prueba por sí solo que el control esté operando: prueba que no se ha necesitado."
        />
      ) : (
        <>
          {/* Lo que Auditoría busca primero. */}
          {data.reincidentes.length > 0 && (
            <Nota tono="riesgo" icono={<ShieldAlert />}>
              <b>
                {data.reincidentes.length === 1
                  ? '1 persona reincidió'
                  : `${data.reincidentes.length} personas reincidieron`}
              </b>
              : {data.reincidentes.map((r) => `${r.persona} (${r.intentos})`).join(', ')}. Un
              intento aislado suele ser una delegación chica resolviendo como puede;{' '}
              <b>la reincidencia es otra cosa</b>.
            </Nota>
          )}

          <Panel titulo="Por par de incompatibilidad">
            <div className="tw:flex tw:flex-wrap tw:gap-2">
              {data.porPar.map((p) => (
                <Pastilla key={p.par} tono="aviso">
                  {p.par} · {p.intentos}
                </Pastilla>
              ))}
            </div>
          </Panel>

          <Panel titulo={`${data.total} intento(s)`}>
            <ul className="tw:flex tw:flex-col tw:gap-3">
              {data.intentos.map((i) => (
                <li
                  key={i.id}
                  className="tw:flex tw:flex-col tw:gap-0.5 tw:border-l-2 tw:border-riesgo-fg tw:pl-3"
                >
                  <div className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-3 tw:text-sm">
                    <span className="tw:font-mono tw:font-medium">{i.par}</span>
                    <span className="tw:font-medium">{i.quien}</span>
                    <span className="tw:text-tinta-mid">
                      quiso {i.pretendia.toLowerCase()} sobre {i.expediente}
                    </span>
                  </div>

                  <span className="tw:text-xs tw:text-tinta-mid">
                    ya había ejercido {i.chocaCon.toLowerCase()} — {i.referencia}
                  </span>

                  <span className="tw:text-xs tw:text-tinta-mid">
                    {diaYHora(i.momento)} ·{' '}
                    {/* Nulo es «no se supo», no «desde el servidor». */}
                    {i.origen === null ? (
                      <span className="tw:italic">origen no registrado</span>
                    ) : (
                      <span className="tw:font-mono">{i.origen}</span>
                    )}
                  </span>
                </li>
              ))}
            </ul>
          </Panel>
        </>
      )}

      {/* Lo que falta, dicho: §5.3.B.3 pide encolar la resolución, no sólo registrarla. */}
      <Nota tono="info">
        <b>El escalamiento todavía no encola nada.</b> §5.3.B.3 pide que el acto quede{' '}
        <i>«visiblemente pendiente en la bandeja de alguien»</i> —el puesto superior, el respaldo
        de sede, o Gerencia Administrativa—. El bloqueo ya nombra el destino, pero{' '}
        <b>los dos primeros saltos exigen la jerarquía de puestos</b>, y el espejo del
        organigrama sólo trae persona↔puesto. Hoy la pista registra; no encamina.
      </Nota>
    </div>
  );
}

interface Pista {
  total: number;
  reincidentes: { persona: string; intentos: number }[];
  porPar: { par: string; intentos: number }[];
  intentos: {
    id: string;
    quien: string;
    pretendia: string;
    expediente: string;
    par: string;
    chocaCon: string;
    referencia: string;
    momento: string;
    /** **Nulo es «no se supo»**, no «desde el servidor». */
    origen: string | null;
  }[];
}
