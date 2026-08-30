import type { ReactElement } from 'react';
import { useQuery } from '@tanstack/react-query';
import { CircleAlert, PlugZap, TriangleAlert } from 'lucide-react';

import { Nota, Panel, Pastilla } from '../../ui';
import type { Tono } from '../../ui';
import { pedir } from '../../api/misiones';

/**
 * `PT-056` — Estado del espejo de ARGOS y Talento Humano.
 *
 * ── `HU-069`: que nunca diverja <b>en silencio</b> ──────────────────────────
 * <i>«Para no despachar contra un espejo viejo que dice que el motorista está activo cuando
 * Talento Humano lo tiene de vacaciones desde el lunes.»</i>
 *
 * ── Y lo que degrada, no bloquea ────────────────────────────────────────────
 * `RN-50` es la autoridad y no admite lectura: *«la operación no se impide: se marca»*. Un
 * espejo viejo hace que el despacho <b>advierta</b>, nunca que se detenga — una delegación con
 * cuatro días sin enlace tiene que poder seguir operando, que es la premisa 5 del producto.
 *
 * ── Nulo y cero son opuestos ────────────────────────────────────────────────
 * «Nunca se confirmó» y «se confirmó hace un momento» se ven iguales en un contador vacío, y
 * significan lo contrario: el primero es que <b>no hay integración corriendo</b>.
 */
export default function Espejos(): ReactElement {
  const { data, isPending, isError } = useQuery({
    queryKey: ['espejos'],
    queryFn: () => pedir<Respuesta>('/espejos'),
  });

  if (isError) {
    return (
      <Nota tono="riesgo" icono={<CircleAlert />}>
        No se pudo consultar el estado de los espejos.
      </Nota>
    );
  }

  const faltantes = (data?.espejos ?? []).filter((e) => !e.existe);

  return (
    <div className="tw:flex tw:flex-col tw:gap-5">
      <header className="tw:flex tw:flex-col tw:gap-1">
        <h1 className="tw:text-xl tw:font-semibold tw:tracking-tight">
          Datos que vienen de otros sistemas
        </h1>
        <p className="tw:text-sm tw:text-tinta-mid">
          SIGTI no es dueño de los puestos ni de los expedientes de personal: los <b>espeja</b>.
          Con qué antigüedad se está trabajando es un dato de la decisión, no del sistema.
        </p>
      </header>

      {faltantes.length > 0 && (
        <Nota tono="riesgo" icono={<TriangleAlert />}>
          <b>
            {faltantes.length === 1
              ? 'Uno de los dos espejos no está construido'
              : 'Ninguno de los dos espejos está construido'}
          </b>
          , así que lo que depende de él <b>no se verifica contra nada</b>. Se dice acá en vez de
          mostrar sólo el que sí existe: una pantalla que enseña un espejo al día se lee como si
          todo estuviera verificado.
        </Nota>
      )}

      {isPending ? (
        <p className="tw:text-sm tw:text-tinta-mid">Consultando…</p>
      ) : (
        <div className="tw:flex tw:flex-col tw:gap-3">
          {data.espejos.map((e) => (
            <Panel key={e.fuente} titulo={e.fuente}>
              <div className="tw:flex tw:flex-col tw:gap-2">
                <div className="tw:flex tw:flex-wrap tw:items-center tw:gap-2">
                  <Pastilla tono={TONO[grado(e)]}>{ETIQUETA[grado(e)]}</Pastilla>
                  {e.diasSinConfirmar !== null && (
                    <span className="tw:text-sm tw:tabular-nums">
                      {e.diasSinConfirmar === 0
                        ? 'confirmado hoy'
                        : `${e.diasSinConfirmar} día(s) sin confirmar`}
                    </span>
                  )}
                </div>

                <p className="tw:text-sm tw:text-tinta-mid">{e.queEspeja}</p>
                <p className="tw:text-sm">{e.porQue}</p>
              </div>
            </Panel>
          ))}
        </div>
      )}

      <Nota tono="info" icono={<PlugZap />}>
        <b>Un espejo viejo advierte; no bloquea.</b> La regla es explícita: la operación no se
        impide, se marca. Una delegación con cuatro días sin enlace tiene que poder seguir
        trabajando — si el atraso llegara a frenar despachos, sería una decisión aparte y
        todavía no está tomada.
      </Nota>
    </div>
  );
}

/**
 * En qué estado está el espejo.
 *
 * `NoExiste` es distinto de `NuncaConfirmado`: el primero es que no hay nada que construir
 * todavía; el segundo, que hay integración y no ha corrido nunca.
 */
function grado(e: Espejo): 'NoExiste' | 'NuncaConfirmado' | 'AlDia' | 'Viejo' {
  if (!e.existe) return 'NoExiste';
  if (e.nuncaConfirmado) return 'NuncaConfirmado';
  return (e.diasSinConfirmar ?? 0) >= 3 ? 'Viejo' : 'AlDia';
}

const ETIQUETA: Record<ReturnType<typeof grado>, string> = {
  NoExiste: 'no está construido',
  NuncaConfirmado: 'nunca se confirmó',
  AlDia: 'al día',
  Viejo: 'atrasado',
};

const TONO: Record<ReturnType<typeof grado>, Tono> = {
  NoExiste: 'riesgo',
  NuncaConfirmado: 'aviso',
  AlDia: 'ok',
  Viejo: 'aviso',
};

interface Espejo {
  fuente: string;
  queEspeja: string;
  /** Falso significa que el espejo **no se construyó**: lo que depende de él no se verifica. */
  existe: boolean;
  /** **Distinto de cero días.** Nunca confirmado es «no hay integración corriendo». */
  nuncaConfirmado: boolean;
  diasSinConfirmar: number | null;
  porQue: string;
}

interface Respuesta {
  espejos: Espejo[];
}
