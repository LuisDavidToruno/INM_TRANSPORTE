import type { ReactElement, ReactNode } from 'react';
import { useState } from 'react';
import { useQueries } from '@tanstack/react-query';
import { CircleAlert, Clock, LogIn, LogOut, TriangleAlert, Truck } from 'lucide-react';

import { CampoFecha, Enlace, LineaDeCarriles, Nota, Panel, Pastilla, Vacio } from '../../ui';
import type { CarrilDeLinea, Tono } from '../../ui';
import { diaDeDespacho, ocupacionDeFlota } from '../../api/flota';
import type { MisionDelDia } from '../../api/flota';
import { laDependencia, soloFecha, soloHora } from '../M06_Autorizacion/formato';

/**
 * `PT-038` — Tablero de despacho del día. **La raíz del Encargado de Despacho.**
 *
 * ── Por qué cuatro listas y no una tabla ordenable ──────────────────────────
 * Porque son cuatro acciones distintas con cuatro urgencias distintas: hay que **entregar**
 * lo que sale, **recibir** lo que vuelve, **no contar con** lo que está afuera, y **ir a
 * buscar** lo que debía haber vuelto. Una tabla con columna de estado obliga a filtrar
 * mentalmente cada vez que se abre la pantalla, y el despachador la abre veinte veces al día.
 *
 * **La cuarta es la que justifica rehacer esta pantalla.** El dictamen de elementos visuales
 * la marcó como el error de mayor daño del inventario: estaba declarada «completa» y
 * maquetada como lista. Y una lista ordenada por fecha **no muestra un retorno vencido**: no
 * aparece arriba, aparece en el pasado, donde nadie mira.
 *
 * ── Lo que esta pantalla NO hace, y no se finge ─────────────────────────────
 * El dictamen pide una **línea de tiempo del día sobre el eje de horas** — la ráfaga de las
 * 5:30 con ocho salidas encimadas. **No se puede construir**: la ventana de la misión es
 * sólo fecha. La solicitud no declara a qué hora sale, así que no hay dato con el que
 * ordenar el día por dentro, y un eje de horas dibujado sobre medianoches sería un gráfico
 * que miente con precisión.
 *
 * Lo que sí se dibuja es el **cronograma de la semana**, que contesta la otra mitad de la
 * pregunta —qué se traslapa con qué— y para la que el dato sí existe.
 */
export default function Tablero(): ReactElement {
  // La fecha se elige, y por omisión es hoy. Poder mover el día es lo que permite preparar
  // mañana la tarde anterior, que es cuando el despachador arma su día.
  const [fecha, setFecha] = useState(() => new Date().toISOString().slice(0, 10));

  const [tableroQ, ocupacionQ] = useQueries({
    queries: [
      { queryKey: ['despacho-dia', fecha], queryFn: () => diaDeDespacho(fecha) },
      {
        queryKey: ['ocupacion-semana', fecha],
        queryFn: () => ocupacionDeFlota(fecha, sumarDias(fecha, 6)),
      },
    ],
  });

  if (tableroQ.isError) {
    return (
      <Nota tono="riesgo" icono={<CircleAlert />}>
        No se pudo cargar el tablero del día. Las misiones siguen donde estaban; nada se
        perdió. Vuelva a intentar en unos segundos.
      </Nota>
    );
  }

  const dia = tableroQ.data;

  return (
    <div className="tw:flex tw:flex-col tw:gap-5">
      <header className="tw:flex tw:flex-wrap tw:items-end tw:justify-between tw:gap-4">
        <div className="tw:flex tw:flex-col tw:gap-1">
          <h1 className="tw:text-xl tw:font-semibold tw:tracking-tight">Tablero de despacho</h1>
          <p className="tw:text-sm tw:text-tinta-mid">
            {tableroQ.isPending
              ? 'Cargando el día…'
              : `${resumen(dia!)} · ${soloFecha(fecha)}`}
          </p>
        </div>

        <div className="tw:w-52">
          <CampoFecha etiqueta="Día del tablero" valor={fecha} onCambiar={setFecha} />
        </div>
      </header>

      {/* Lo atrasado va PRIMERO y en riesgo. Es lo único de esta pantalla que ya salió mal:
          todo lo demás es trabajo del día, y ponerlo al mismo nivel lo escondería entre
          renglones que se ven iguales. */}
      {dia && dia.atrasadas.length > 0 && (
        <Grupo
          tono="riesgo"
          icono={<TriangleAlert />}
          titulo={
            dia.atrasadas.length === 1
              ? '1 misión no volvió cuando debía'
              : `${dia.atrasadas.length} misiones no volvieron cuando debían`
          }
          nota="El vehículo sigue afuera y el retorno previsto ya pasó. Es lo primero que hay que resolver: no es trabajo del día, es algo que ya salió mal."
          misiones={dia.atrasadas}
          queSignifica="Todo lo que salió, volvió cuando debía."
          mostrarAtraso
        />
      )}

      <div className="tw:grid tw:gap-5 tw:xl:grid-cols-2">
        <Grupo
          tono="info"
          icono={<LogOut />}
          titulo="Sale hoy"
          nota="Hay que entregar vehículo, documentos y fondo. Una misión que debía salir antes y sigue acá no salió — y por eso no desaparece del tablero al día siguiente."
          misiones={dia?.salenHoy ?? []}
          cargando={tableroQ.isPending}
          vacio="Ninguna salida programada para este día"
          queSignifica="Nadie tiene que recibir vehículo, documentos ni fondo hoy."
          mostrarSalidaVencida={fecha}
        />

        <Grupo
          tono="ok"
          icono={<LogIn />}
          titulo="Vuelve hoy"
          nota="Hay que recibir el vehículo, tomar el odómetro y levantar el acta de recepción."
          misiones={dia?.vuelvenHoy ?? []}
          cargando={tableroQ.isPending}
          vacio="Ningún retorno previsto para este día"
          queSignifica="No hay actas de recepción que levantar hoy."
        />
      </div>

      <Grupo
        tono="neutro"
        icono={<Truck />}
        titulo="Afuera"
        nota="Vehículos con los que hoy no se puede contar. No es una alarma: es lo que falta de la flota cuando alguien pregunte si hay unidad disponible."
        misiones={dia?.afuera ?? []}
        cargando={tableroQ.isPending}
        // El vacío de esta lista NO significa que la flota esté en el predio: una misión
        // atrasada también tiene el vehículo afuera, sólo que se cuenta arriba. Decir «toda
        // la flota está en el predio» con un vehículo sin volver es una tranquilidad falsa,
        // y es la clase de frase sobre la que alguien promete una unidad que no existe.
        vacio={
          (dia?.atrasadas.length ?? 0) > 0
            ? 'Nada afuera con retorno pendiente'
            : 'Toda la flota está en el predio'
        }
        queSignifica={
          (dia?.atrasadas.length ?? 0) > 0
            ? 'Pero hay vehículos sin volver: están contados arriba, en lo atrasado.'
            : 'Ningún vehículo está comprometido en una misión en curso.'
        }
      />

      <Panel titulo="Cronograma de la semana">
        <div className="tw:flex tw:flex-col tw:gap-2">
          <Cronograma fecha={fecha} carriles={carrilesDe(ocupacionQ.data)} fallo={ocupacionQ.isError} />

          <p className="tw:text-xs tw:text-tinta-mid">
            <b>El día no se puede desglosar por horas.</b> La solicitud declara fechas, no
            horas de salida — así que el traslape dentro de un mismo día no se puede dibujar.
            Es el mismo dato que le falta a <code className="tw:font-mono">BD-04</code> para
            juzgar la <i>hora</i> inhábil.
          </p>
        </div>
      </Panel>
    </div>
  );
}

/** Los siete días desde la fecha elegida, como carriles de flota. */
function carrilesDe(ocupacion: Awaited<ReturnType<typeof ocupacionDeFlota>> | undefined): CarrilDeLinea[] {
  if (!ocupacion) return [];

  return ocupacion.carriles.map((c) => ({
    id: c.vehiculo,
    titulo: c.siglas,
    detalle: [c.tipoDeVehiculo, c.placa ?? 'sin placa metálica'].join(' · '),
    barras: c.barras.map((b) => ({
      id: b.mision,
      titulo: b.folio,
      desde: comoDiaLocal(b.desde),
      hasta: comoDiaLocal(b.hasta),
      detalle: `${b.destino}, ${b.estado.toLowerCase()}`,
      queEs: 'misión',
      // `EnRuta` en ámbar: ese vehículo está afuera AHORA, y no es lo mismo que uno
      // reservado para el jueves.
      tono: (b.estado === 'EnRuta' ? 'aviso' : 'info') as Tono,
    })),
  }));
}

function Cronograma({
  fecha,
  carriles,
  fallo,
}: {
  fecha: string;
  carriles: CarrilDeLinea[];
  fallo: boolean;
}): ReactElement {
  // Callar acá dejaría al despachador creyendo que no hay nada tomado, que es la conclusión
  // que lleva a prometer una unidad que no existe.
  if (fallo) {
    return (
      <Nota tono="aviso">
        No se pudo cargar la ocupación de la semana. Las listas de arriba son correctas, pero{' '}
        <b>esta pantalla no le está mostrando los traslapes</b>.
      </Nota>
    );
  }

  return (
    <LineaDeCarriles
      carriles={carriles}
      desde={comoDiaLocal(fecha)}
      hasta={comoDiaLocal(sumarDias(fecha, 6))}
      queEsUnaBarra="misión"
      referencia={{ fecha: comoDiaLocal(fecha), titulo: 'El día del tablero' }}
      vacio="No hay vehículos registrados en la flota."
    />
  );
}

/**
 * Un grupo del tablero.
 *
 * ── El tono lo recibe, no lo deduce ─────────────────────────────────────────
 * Porque lo que hace urgente a un grupo no es su contenido sino **qué acción exige**, y eso
 * lo sabe la pantalla, no la lista. Un grupo vacío de «atrasadas» y uno vacío de «vuelve
 * hoy» significan cosas opuestas.
 */
function Grupo({
  tono,
  icono,
  titulo,
  nota,
  misiones,
  cargando = false,
  vacio,
  queSignifica,
  mostrarAtraso = false,
  mostrarSalidaVencida,
}: {
  tono: Tono;
  icono: ReactNode;
  titulo: string;
  nota: string;
  misiones: MisionDelDia[];
  cargando?: boolean;
  vacio?: string;
  /** Qué significa que esté vacío. No es lo mismo «no hay salidas» que «no hay atrasos». */
  queSignifica: string;
  mostrarAtraso?: boolean;
  /** El día del tablero, para marcar lo que debía haber salido antes. */
  mostrarSalidaVencida?: string;
}): ReactElement {
  return (
    <Panel titulo={titulo}>
      <div className="tw:flex tw:flex-col tw:gap-3">
        <p className="tw:text-sm tw:text-tinta-mid">{nota}</p>

        {cargando ? (
          <p className="tw:text-sm tw:text-tinta-mid">Cargando…</p>
        ) : misiones.length === 0 ? (
          <Vacio icono={icono} titulo={vacio ?? 'Nada por acá'} descripcion={queSignifica} />
        ) : (
          <ul className="tw:flex tw:flex-col tw:gap-2">
            {misiones.map((m) => (
              <li key={m.mision}>
                <Fila
                  mision={m}
                  tono={tono}
                  mostrarAtraso={mostrarAtraso}
                  mostrarSalidaVencida={mostrarSalidaVencida}
                />
              </li>
            ))}
          </ul>
        )}
      </div>
    </Panel>
  );
}

function Fila({
  mision,
  tono,
  mostrarAtraso,
  mostrarSalidaVencida,
}: {
  mision: MisionDelDia;
  tono: Tono;
  mostrarAtraso: boolean;
  mostrarSalidaVencida?: string;
}): ReactElement {
  const salioTarde = mostrarSalidaVencida !== undefined && mision.salida < mostrarSalidaVencida;

  return (
    <div className="tw:flex tw:flex-col tw:gap-1 tw:rounded tw:border tw:border-linea tw:px-3 tw:py-2">
      <div className="tw:flex tw:flex-wrap tw:items-center tw:gap-2">
        {/* La hora primero y en grande: es por lo que se ordena la ráfaga de la mañana, y
            lo que decide a quién se atiende antes. El folio identifica; la hora prioriza. */}
        <span
          className={[
            'tw:font-mono tw:text-sm tw:tabular-nums',
            mision.horaDeSalida === null ? 'tw:text-tinta-low tw:italic tw:text-xs' : 'tw:font-semibold',
          ].join(' ')}
        >
          {soloHora(mision.horaDeSalida)}
        </span>

        <Enlace href={`/programacion/${mision.mision}`}>
          <span className="tw:font-mono tw:text-[13px] tw:tabular-nums">{mision.folio}</span>
        </Enlace>

        {mostrarAtraso && (
          <Pastilla tono="riesgo">
            <Clock size={13} aria-hidden />
            {mision.diasDeAtraso === 1 ? '1 día de atraso' : `${mision.diasDeAtraso} días de atraso`}
          </Pastilla>
        )}

        {/* Debía salir antes y sigue sin salir. Es un problema distinto del atraso de
            retorno —el vehículo está en el predio, no afuera— y por eso no comparte pastilla. */}
        {salioTarde && <Pastilla tono="aviso">Debía salir el {soloFecha(mision.salida)}</Pastilla>}

        <Pastilla tono={tono}>{mision.estado}</Pastilla>
      </div>

      <span className="tw:text-sm">
        {/* El vehículo es lo primero que el despachador busca: es lo que va a entregar. */}
        {mision.vehiculo ?? <SinDato>Sin vehículo en la flota</SinDato>}
        {' · '}
        {mision.motorista ?? <SinDato>Sin motorista en el padrón</SinDato>}
      </span>

      <span className="tw:text-xs tw:text-tinta-mid">
        {mision.objetoDelTraslado || 'Sin objeto declarado'} · destino {mision.destino} ·{' '}
        {laDependencia(mision.dependencia)}
      </span>
    </div>
  );
}

/**
 * Un dato que la reserva apunta y la flota ya no tiene.
 *
 * No es un adorno: significa que <b>ese renglón no tiene con qué salir</b>. El vehículo se
 * dio de baja después de programar, y el despachador tiene que saberlo antes de ir a
 * buscarlo al predio.
 */
function SinDato({ children }: { children: ReactNode }): ReactElement {
  return <span className="tw:italic tw:text-riesgo-fg">{children}</span>;
}

function resumen(dia: Awaited<ReturnType<typeof diaDeDespacho>>): string {
  const partes = [
    `${dia.salenHoy.length} sale${dia.salenHoy.length === 1 ? '' : 'n'}`,
    `${dia.vuelvenHoy.length} vuelve${dia.vuelvenHoy.length === 1 ? '' : 'n'}`,
    `${dia.afuera.length} afuera`,
  ];

  // El atraso sólo se menciona cuando lo hay: un «0 atrasadas» permanente entrena a no leer
  // la línea, y entonces el día que diga 3 tampoco se lee.
  if (dia.atrasadas.length > 0) partes.push(`${dia.atrasadas.length} sin volver`);

  return partes.join(' · ');
}

/**
 * `YYYY-MM-DD` a `Date` <b>local a medianoche</b>.
 *
 * `new Date('2026-03-16')` la interpreta como UTC y en Honduras —UTC−6— la corre al 15 por
 * la tarde. Es el mismo error que ya se corrigió en `PT-026`.
 */
function comoDiaLocal(fecha: string): Date {
  const [a, m, d] = fecha.slice(0, 10).split('-').map(Number);
  return new Date(a!, m! - 1, d!);
}

const sumarDias = (fecha: string, dias: number): string => {
  const f = comoDiaLocal(fecha);
  f.setDate(f.getDate() + dias);
  return `${f.getFullYear()}-${String(f.getMonth() + 1).padStart(2, '0')}-${String(f.getDate()).padStart(2, '0')}`;
};
