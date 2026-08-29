import type { ReactElement } from 'react';
import { useQuery } from '@tanstack/react-query';
import { CircleAlert, MapPinOff, Radio } from 'lucide-react';
import { Link } from 'react-router';

import { Nota, Panel, Pastilla, Vacio } from '../../ui';
import type { Tono } from '../../ui';
import { pedir } from '../../api/misiones';
import { diaYHora } from '../M06_Autorizacion/formato';

/**
 * `PT-058` — El tablero de seguimiento en ruta, <b>con la antigüedad del dato</b>.
 *
 * ── La antigüedad no es un adorno, es el requisito ──────────────────────────
 * `HU-057` se llama, literalmente, «mostrar la última posición conocida con su antigüedad,
 * nunca como si fuera actual». Un tablero que muestra una posición de hace once horas como si
 * fuera de ahora <b>es peor que un tablero vacío</b>: produce decisiones seguras sobre
 * información falsa.
 *
 * ── Y no hay ningún indicador de «en línea» ─────────────────────────────────
 * Deliberado. `HU-057` lo prohíbe en un escenario aparte, y la razón es que en Honduras el
 * silencio de un vehículo es <b>la condición esperada</b>, no una anomalía: más de dos millones
 * de personas del área rural no tienen conectividad. Un punto verde parpadeando convertiría la
 * falta de cobertura en una alarma, y las alarmas que suenan siempre se dejan de mirar.
 */
export default function Tablero(): ReactElement {
  const { data, isPending, isError } = useQuery({
    queryKey: ['seguimiento'],
    queryFn: () => pedir<RespuestaDelTablero>('/seguimiento'),

    // Se refresca solo, porque la antigüedad crece aunque nadie toque nada: un tablero abierto
    // media hora mostraría «hace 20 minutos» cuando ya son cincuenta.
    refetchInterval: 60_000,
  });

  if (isError) {
    return (
      <Nota tono="riesgo" icono={<CircleAlert />}>
        No se pudo cargar el tablero de seguimiento.
      </Nota>
    );
  }

  const nuncaDeclararon =
    data?.misiones.filter((m) => m.frescura.grado === 'NuncaHuboDato').length ?? 0;

  return (
    <div className="tw:flex tw:flex-col tw:gap-5">
      <header className="tw:flex tw:flex-col tw:gap-1">
        <h1 className="tw:text-xl tw:font-semibold tw:tracking-tight">Seguimiento en ruta</h1>
        <p className="tw:text-sm tw:text-tinta-mid">
          Lo último que <b>declaró el motorista</b> de cada misión que está afuera, siempre con
          la antigüedad del dato. Nada de lo que se muestra acá se dedujo del silencio.
        </p>
      </header>

      {/* El parámetro que falta se declara, y no se sustituye por uno inventado. */}
      {data !== undefined && data.umbralHoras === null && (
        <Nota tono="aviso">
          <b>No hay umbral de degradación fijado</b> (
          <code className="tw:font-mono tw:text-xs">insumo #68</code>). La antigüedad se muestra
          igual —es lo que exige <code className="tw:font-mono tw:text-xs">HU-057</code>— pero el
          tablero no puede decir a partir de cuándo un dato deja de servir. Un umbral inventado
          degradaría según un número que nadie decidió.
        </Nota>
      )}

      {nuncaDeclararon > 0 && (
        <Nota tono="info" icono={<MapPinOff />}>
          {nuncaDeclararon === 1
            ? 'De 1 misión no se ha recibido ninguna declaración'
            : `De ${nuncaDeclararon} misiones no se ha recibido ninguna declaración`}
          . <b>Eso no dice que algo haya pasado</b>: dice que el dispositivo no ha tenido señal,
          o que nadie declaró. Van primero porque son las que no se pueden explicar.
        </Nota>
      )}

      {isPending ? (
        <p className="tw:text-sm tw:text-tinta-mid">Cargando el tablero…</p>
      ) : data.misiones.length === 0 ? (
        <Vacio
          icono={<Radio />}
          titulo="Ninguna misión en ruta"
          descripcion="No hay vehículos con la salida registrada y sin retorno. El tablero se llena cuando el motorista registra su salida."
        />
      ) : (
        <Panel titulo={`${data.misiones.length} misión(es) en ruta`}>
          <ul className="tw:flex tw:flex-col tw:gap-3">
            {data.misiones.map((m) => (
              <li
                key={m.mision}
                className={`tw:flex tw:flex-col tw:gap-1 tw:border-l-2 tw:py-1 tw:pl-3 ${
                  BORDE[m.frescura.grado] ?? 'tw:border-linea'
                }`}
              >
                <div className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-3">
                  <Link
                    to={`/seguimiento/${m.mision}`}
                    className="loki-foco tw:font-medium tw:text-sm tw:underline-offset-2 tw:hover:underline"
                  >
                    {m.folio}
                  </Link>
                  <span className="tw:text-sm tw:text-tinta-mid">{m.destino}</span>
                  {m.vehiculo !== null && (
                    <span className="tw:font-mono tw:text-xs tw:text-tinta-mid">
                      {m.vehiculo}
                    </span>
                  )}
                  {m.motorista !== null && (
                    <span className="tw:text-xs tw:text-tinta-mid">{m.motorista}</span>
                  )}
                </div>

                {/* ── Lo declarado, con su hora. Nunca un estado calculado. ── */}
                <div className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-2 tw:text-sm">
                  {m.ultimoEstado === null ? (
                    <span className="tw:italic tw:text-tinta-mid">
                      sin estado declarado
                    </span>
                  ) : (
                    <>
                      <span>{m.ultimoEstado}</span>
                      {/* La hora de ESE reporte, no la de la última señal: un estado de las
                          17:00 seguido de un arribo a las 21:00 se muestra con las 17:00. */}
                      <span className="tw:text-xs tw:text-tinta-mid">
                        declarado por el motorista
                        {m.declaradoEl !== null && ` a las ${diaYHora(m.declaradoEl)}`}
                      </span>
                    </>
                  )}
                </div>

                {/* La antigüedad va SIEMPRE, en la misma línea que el dato. */}
                <div className="tw:flex tw:flex-wrap tw:items-center tw:gap-2">
                  <Pastilla tono={TONO[m.frescura.grado] ?? 'neutro'}>
                    {antiguedad(m.frescura)}
                  </Pastilla>
                  <span className="tw:text-xs tw:text-tinta-mid">{m.frescura.porQue}</span>
                </div>
              </li>
            ))}
          </ul>
        </Panel>
      )}
    </div>
  );
}

/**
 * Cómo se lee la antigüedad en la pastilla.
 *
 * **Nulo no se dibuja como «hace 0 minutos»**, que sería la lectura más engañosa posible: diría
 * que acabamos de saber de un vehículo del que no sabemos nada. Y la antigüedad negativa —el
 * reloj del dispositivo adelantado— tampoco se aplasta a cero: el dato menos confiable del
 * tablero aparecería como el más fresco.
 */
function antiguedad(f: Frescura): string {
  if (f.minutos === null) return 'sin dato';
  if (f.minutos < 0) return 'reloj del equipo adelantado';
  if (f.minutos < 1) return 'hace menos de un minuto';

  const dias = Math.floor(f.minutos / 1440);
  const horas = Math.floor((f.minutos % 1440) / 60);
  const min = Math.floor(f.minutos % 60);

  // No se redondea a «hace un día»: eso borra la diferencia entre veintiséis horas y cuarenta y
  // siete, que es justamente la que hay que ver.
  const partes: string[] = [];
  if (dias > 0) partes.push(`${dias} d`);
  if (horas > 0) partes.push(`${horas} h`);
  if (dias === 0 && min > 0) partes.push(`${min} min`);

  return `hace ${partes.join(' ')}`;
}

const TONO: Record<string, Tono> = {
  Fresco: 'ok',
  Degradado: 'aviso',
  NoSeClasifica: 'neutro',
  RelojAdelantado: 'riesgo',
  NuncaHuboDato: 'info',
};

const BORDE: Record<string, string> = {
  Fresco: 'tw:border-ok-fg',
  Degradado: 'tw:border-aviso-fg',
  NoSeClasifica: 'tw:border-linea',
  RelojAdelantado: 'tw:border-riesgo-fg',
  NuncaHuboDato: 'tw:border-info-fg',
};

interface Frescura {
  grado: 'Fresco' | 'Degradado' | 'NoSeClasifica' | 'RelojAdelantado' | 'NuncaHuboDato';
  /** **Nulo es «nunca hubo dato»** y negativo es el reloj adelantado. Ninguno se colapsa a cero. */
  minutos: number | null;
  porQue: string;
}

interface RespuestaDelTablero {
  ahora: string;
  /** Nulo cuando la institución no lo fijó — insumo #68. */
  umbralHoras: number | null;
  misiones: {
    mision: string;
    folio: string;
    dependencia: string;
    destino: string;
    objetoDelTraslado: string;
    vehiculo: string | null;
    motorista: string | null;
    retorno: string;
    /** Lo declarado por el motorista. Nulo es que no declaró. */
    ultimoEstado: string | null;
    /** La hora de **ese** reporte, que puede ser más vieja que la de la última señal. */
    declaradoEl: string | null;
    /** La última señal de vida, del tipo que sea. Sobre ésta se mide la antigüedad. */
    ultimoHecho: string | null;
    posicion: { latitud: number; longitud: number; precisionMetros: number | null } | null;
    frescura: Frescura;
  }[];
}
