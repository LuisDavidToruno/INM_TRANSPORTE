import type { ReactElement } from 'react';
import { useQuery } from '@tanstack/react-query';
import { CircleAlert, Clock3, HardDrive, Wifi } from 'lucide-react';

import { Nota, Panel, Pastilla } from '../../ui';
import { pedir } from '../../api/misiones';
import { diaYHora } from '../M06_Autorizacion/formato';

/**
 * `PT-052` — Panel de sincronización de dispositivos.
 *
 * ── Las dos cosas que no se mezclan ─────────────────────────────────────────
 * Un registro <b>en espera</b> se resuelve solo en cuanto llegue el que falta. Un registro
 * <b>en desacuerdo</b> espera a que una persona decida, y no se va a mover hasta entonces.
 *
 * Ponerlos en la misma lista haría que alguien intentara «resolver» un hueco de orden — que no
 * tiene nada que decidir — y que un desacuerdo real pareciera que se va a arreglar solo.
 *
 * ── Y por qué los intentos importan ─────────────────────────────────────────
 * <b>Un registro con veinte intentos no espera un predecesor: espera algo que no va a
 * llegar.</b> Sin ese número, un hueco permanente se ve idéntico a uno que se cierra mañana, y
 * nadie lo mira hasta que el motorista pregunta por qué su registro nunca entró.
 */
export default function Dispositivos(): ReactElement {
  const porDispositivo = useQuery({
    queryKey: ['conflictos', 'por-dispositivo'],
    queryFn: () => pedir<PorDispositivo[]>('/conflictos/por-dispositivo'),
  });

  const enEspera = useQuery({
    queryKey: ['conflictos', 'en-espera'],
    queryFn: () => pedir<Retenido[]>('/conflictos/en-espera'),
  });

  if (porDispositivo.isError || enEspera.isError) {
    return (
      <Nota tono="riesgo" icono={<CircleAlert />}>
        No se pudo cargar el panel de sincronización.
      </Nota>
    );
  }

  const retenidos = enEspera.data ?? [];
  const trabados = retenidos.filter((r) => r.intentos >= 5);

  return (
    <div className="tw:flex tw:flex-col tw:gap-5">
      <header className="tw:flex tw:flex-col tw:gap-1">
        <h1 className="tw:text-xl tw:font-semibold tw:tracking-tight">
          Envíos desde los dispositivos
        </h1>
        <p className="tw:text-sm tw:text-tinta-mid">
          Qué llegó de cada equipo de campo, y qué quedó sin entrar.
        </p>
      </header>

      {/* ── Lo que espera a otro registro ─────────────────────────────────── */}
      <Panel
        titulo={
          retenidos.length === 0
            ? 'Registros esperando a otro anterior'
            : `Registros esperando a otro anterior · ${retenidos.length}`
        }
      >
        {enEspera.isPending ? (
          <p className="tw:text-sm tw:text-tinta-mid">Cargando…</p>
        ) : retenidos.length === 0 ? (
          <p className="tw:text-sm tw:text-tinta-mid">
            Ninguno. Cuando un registro llega antes que aquel del que depende, se guarda acá y{' '}
            <b>entra solo</b> en cuanto llegue el que falta — no se pierde ni se rechaza.
          </p>
        ) : (
          <div className="tw:flex tw:flex-col tw:gap-3">
            {trabados.length > 0 && (
              <Nota tono="aviso" icono={<Clock3 />}>
                <b>
                  {trabados.length === 1
                    ? '1 registro lleva muchos intentos'
                    : `${trabados.length} registros llevan muchos intentos`}
                </b>
                . Un registro que se reintentó muchas veces <b>ya no espera a otro que va a
                llegar</b>: espera a uno que probablemente se perdió. Conviene revisarlo con
                quien lo capturó.
              </Nota>
            )}

            <ul className="tw:flex tw:flex-col tw:gap-2">
              {retenidos.map((r) => (
                <li
                  key={r.idDeCaptura}
                  className={`tw:flex tw:flex-col tw:gap-0.5 tw:border-l-2 tw:py-1 tw:pl-3 ${
                    r.intentos >= 5 ? 'tw:border-aviso-fg' : 'tw:border-linea'
                  }`}
                >
                  <div className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-3 tw:text-sm">
                    <span className="tw:font-medium">{r.enPalabras}</span>
                    <Pastilla tono={r.intentos >= 5 ? 'aviso' : 'neutro'}>
                      {r.intentos === 0
                        ? 'sin reintentos todavía'
                        : `${r.intentos} intento(s)`}
                    </Pastilla>
                  </div>
                  <span className="tw:text-xs tw:text-tinta-mid">
                    lo registró {r.ejecuta} · pasó el {diaYHora(r.ocurrioEl)} · esperando desde
                    hace {r.diasEsperando} día(s)
                    {r.dispositivo !== null && ` · desde ${r.dispositivo}`}
                  </span>
                </li>
              ))}
            </ul>
          </div>
        )}
      </Panel>

      {/* ── Lo que necesita que alguien decida, por equipo ────────────────── */}
      <Panel titulo="Desacuerdos por equipo">
        {porDispositivo.isPending ? (
          <p className="tw:text-sm tw:text-tinta-mid">Cargando…</p>
        ) : porDispositivo.data.length === 0 ? (
          <p className="tw:text-sm tw:text-tinta-mid">
            Ningún equipo ha enviado algo que no coincida con lo que la oficina tenía.
          </p>
        ) : (
          <ul className="tw:flex tw:flex-col tw:gap-2">
            {porDispositivo.data.map((d) => (
              <li
                key={d.dispositivo ?? 'sin-equipo'}
                className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-3 tw:border-l-2 tw:border-linea tw:py-1 tw:pl-3 tw:text-sm"
              >
                {/* Nulo es «el registro no dijo de qué equipo vino»: un dato que falta, no un
                    equipo que se llama «desconocido». */}
                {d.dispositivo === null ? (
                  <span className="tw:italic tw:text-tinta-mid">
                    sin identificar el equipo
                  </span>
                ) : (
                  <span className="tw:flex tw:items-center tw:gap-1.5 tw:font-mono">
                    <HardDrive className="tw:size-4 tw:text-tinta-mid" aria-hidden />
                    {d.dispositivo}
                  </span>
                )}

                <span className="tw:tabular-nums">
                  {d.pendientes} sin decidir de {d.total}
                </span>

                {d.deAltoImpacto > 0 && (
                  <Pastilla tono="riesgo">
                    {d.deAltoImpacto} frenan liquidaciones
                  </Pastilla>
                )}
              </li>
            ))}
          </ul>
        )}
      </Panel>

      <Nota tono="info" icono={<Wifi />}>
        <b>Que un equipo pase días sin enviar es normal.</b> Lo que no es normal es que un mismo
        equipo genere desacuerdos con frecuencia: eso es un problema a corregir, no un hecho a
        tolerar.
      </Nota>
    </div>
  );
}

interface Retenido {
  idDeCaptura: string;
  esperaExpediente: string;
  transicion: string;
  ejecuta: string;
  dispositivo: string | null;
  ocurrioEl: string;
  retenidoEl: string;
  diasEsperando: number;
  /** Un número alto significa que espera algo que probablemente no va a llegar. */
  intentos: number;
  /** La frase exacta que `HU-067` exige. */
  enPalabras: string;
}

interface PorDispositivo {
  /** **Nulo es «no dijo de qué equipo vino»**, no un equipo llamado «desconocido». */
  dispositivo: string | null;
  total: number;
  pendientes: number;
  deAltoImpacto: number;
}
