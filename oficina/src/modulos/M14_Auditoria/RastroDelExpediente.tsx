import type { ReactElement } from 'react';
import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { CircleAlert, Link2, Search } from 'lucide-react';

import { Campo, Nota, Panel, Pastilla, Vacio } from '../../ui';
import type { Tono } from '../../ui';
import { pedir } from '../../api/misiones';
import { soloFecha } from '../M06_Autorizacion/formato';

/**
 * `PT-089` — El rastro del expediente de extremo a extremo, <b>con sus huecos visibles</b>.
 *
 * ── Por qué «con sus huecos visibles» es todo el requisito ──────────────────
 * Un rastro que sólo muestra lo que está **no sirve para auditar**: lo que el TSC busca es
 * dónde se cortó la cadena. Un reporte que enumera cinco asientos presentes y calla los dos que
 * faltan es exactamente el que deja pasar el hallazgo.
 *
 * ── Cuatro estados, no dos ──────────────────────────────────────────────────
 * *«Falta»*, *«no correspondía»* y *«todavía no toca»* se ven iguales en una casilla vacía y no
 * son lo mismo. Juntarlos produce los dos daños a la vez: **alarma sobre lo que está bien** —y
 * una pista con alarmas falsas se deja de mirar— **y silencio sobre lo que está mal**.
 */
export default function RastroDelExpediente(): ReactElement {
  const [id, setId] = useState('');

  const { data, isPending, isError } = useQuery({
    queryKey: ['rastro', id],
    queryFn: () => pedir<Rastro>(`/auditoria/expediente/${id}`),

    // 26 caracteres es un ULID completo. Consultar con uno a medias devolvería un 404 por cada
    // tecla mientras alguien escribe.
    enabled: id.length === 26,
  });

  return (
    <div className="tw:flex tw:flex-col tw:gap-5">
      <header className="tw:flex tw:flex-col tw:gap-1">
        <h1 className="tw:text-xl tw:font-semibold tw:tracking-tight">Rastro del expediente</h1>
        <p className="tw:text-sm tw:text-tinta-mid">
          La cadena que revisa <code className="tw:font-mono tw:text-xs">ACT-12</code>:
          solicitud → autorización → orden de misión → bitácora → vale → comprobante →
          liquidación. <b>Con sus huecos visibles</b>, que es para lo que sirve.
        </p>
      </header>

      <Panel titulo="Qué expediente">
        <div className="tw:sm:max-w-md">
          <Campo
            etiqueta="Expediente"
            ayuda="El identificador de la Orden de Misión. La cadena se arma contra su diario, que es la única fuente: el estado es una proyección del diario, no al revés."
          >
            {(control) => (
              <input
                {...control}
                value={id}
                onChange={(e) => setId(e.target.value.trim().toUpperCase())}
                placeholder="ULID del expediente"
                className="loki-foco tw:w-full tw:rounded-control tw:border tw:border-linea tw:bg-panel tw:px-2 tw:py-1.5 tw:text-cuerpo-2 tw:font-mono tw:text-tinta-hi"
              />
            )}
          </Campo>
        </div>
      </Panel>

      {id.length !== 26 ? (
        <p className="tw:flex tw:items-center tw:gap-1.5 tw:text-sm tw:text-tinta-mid">
          <Search className="tw:size-4" aria-hidden />
          Ingrese el identificador completo para armar la cadena.
        </p>
      ) : isError ? (
        <Vacio
          icono={<CircleAlert />}
          titulo="No se encontró ese expediente"
          descripcion="Puede que el identificador esté incompleto, o que el expediente no exista."
        />
      ) : isPending ? (
        <p className="tw:text-sm tw:text-tinta-mid">Armando la cadena…</p>
      ) : (
        <>
          {/* El hallazgo primero. Es lo que se vino a buscar. */}
          {data.huecos > 0 ? (
            <Nota tono="riesgo" icono={<CircleAlert />}>
              <b>
                {data.huecos === 1
                  ? 'La cadena se cortó en 1 eslabón'
                  : `La cadena se cortó en ${data.huecos} eslabones`}
              </b>
              . Correspondían y no hay asiento — es lo que hay que poder explicar ante el
              Tribunal Superior de Cuentas.
            </Nota>
          ) : data.completa ? (
            <Nota tono="ok">
              <b>Cadena completa.</b> Todos los eslabones que correspondían tienen asiento.
            </Nota>
          ) : (
            <Nota tono="info">
              <b>Sin huecos, y todavía no está completa.</b> El expediente sigue su curso: hay
              eslabones que aún no tocan. <b>No es un hallazgo</b> — darla por completa cerraría
              un expediente vivo en el reporte.
            </Nota>
          )}

          <Panel titulo={`${data.folio} · ${data.estado}`}>
            <ol className="tw:flex tw:flex-col">
              {data.eslabones.map((e, i) => (
                <li
                  key={e.eslabon}
                  className={`tw:flex tw:flex-col tw:gap-0.5 tw:border-l-2 tw:py-2 tw:pl-3 ${
                    BORDE[e.estado] ?? 'tw:border-linea'
                  }`}
                >
                  <div className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-3 tw:text-sm">
                    <span className="tw:w-5 tw:shrink-0 tw:font-mono tw:text-xs tw:text-tinta-mid">
                      {i + 1}
                    </span>
                    <span className="tw:font-medium">{TEXTO[e.eslabon] ?? e.eslabon}</span>
                    <Pastilla tono={TONO[e.estado] ?? 'neutro'}>
                      {ETIQUETA[e.estado] ?? e.estado}
                    </Pastilla>
                  </div>

                  {/* Nulos cuando el eslabón no está: inventar un autor para algo que no
                      ocurrió es la peor forma de llenar un reporte de auditoría. */}
                  {e.referencia !== null && (
                    <span className="tw:pl-8 tw:text-xs tw:text-tinta-mid">
                      <span className="tw:font-mono">{e.referencia}</span>
                      {e.quien !== null && ` · ${e.quien}`}
                      {e.fecha !== null && ` · ${soloFecha(e.fecha)}`}
                    </span>
                  )}

                  {/* Nulo cuando está presente: un motivo ahí sería ruido. */}
                  {e.porQue !== null && (
                    <span
                      className={`tw:pl-8 tw:text-xs ${
                        e.estado === 'Ausente' ? 'tw:text-riesgo-fg' : 'tw:text-tinta-mid'
                      }`}
                    >
                      {e.porQue}
                    </span>
                  )}
                </li>
              ))}
            </ol>
          </Panel>

          {data.noAplican > 0 && (
            <Nota tono="info" icono={<Link2 />}>
              <b>
                {data.noAplican} eslabón(es) no correspondían a este expediente
              </b>{' '}
              y por eso no cuentan como huecos. Una misión sin fondo asignado no tiene vale ni
              comprobante — marcarlos faltantes llenaría la pista de alarmas falsas, y{' '}
              <b>una pista con alarmas falsas se deja de mirar</b>.
            </Nota>
          )}
        </>
      )}
    </div>
  );
}

const TEXTO: Record<string, string> = {
  Solicitud: 'Solicitud',
  Autorizacion: 'Autorización',
  OrdenDeMision: 'Orden de misión',
  Bitacora: 'Bitácora',
  Vale: 'Vale de combustible',
  Comprobante: 'Comprobante',
  Liquidacion: 'Liquidación',
};

const ETIQUETA: Record<string, string> = {
  Presente: 'Presente',
  Ausente: 'Falta, y correspondía',
  NoAplica: 'No correspondía',
  Pendiente: 'Todavía no toca',
};

const TONO: Record<string, Tono> = {
  Presente: 'ok',
  Ausente: 'riesgo',
  NoAplica: 'neutro',
  Pendiente: 'info',
};

const BORDE: Record<string, string> = {
  Presente: 'tw:border-ok-fg',
  Ausente: 'tw:border-riesgo-fg',
  NoAplica: 'tw:border-linea',
  Pendiente: 'tw:border-info-fg',
};

interface Rastro {
  expediente: string;
  folio: string;
  estado: string;
  /** Exige que no haya huecos **y** que no quede nada pendiente. */
  completa: boolean;
  huecos: number;
  noAplican: number;
  eslabones: {
    eslabon: string;
    estado: 'Presente' | 'Ausente' | 'NoAplica' | 'Pendiente';
    referencia: string | null;
    quien: string | null;
    fecha: string | null;
    porQue: string | null;
  }[];
}
