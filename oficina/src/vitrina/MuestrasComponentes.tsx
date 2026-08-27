import { useState } from 'react';
import type { ReactElement } from 'react';

import { avisar } from '../ui/avisos';
import CampoBusqueda from '../ui/CampoBusqueda';
import CampoFecha from '../ui/CampoFecha';
import RangoFechas from '../ui/RangoFechas';
import Boton from '../ui/Boton';
import Campo from '../ui/Campo';
import { EsqueletoFichas, EsqueletoKpis, EsqueletoLista, EsqueletoTabla } from '../ui/Esqueleto';
import FilaKpis from '../ui/FilaKpis';
import { EsqueletoTarjetasKpi, TarjetaKpi } from '../ui/TarjetaKpi';
import { Users } from 'lucide-react';
import type { ColumnaDef, Tono } from '../ui/tipos';

/**
 * Muestras de la vitrina: matriz de estados, carga, formulario y avisos.
 * Traducidas de las secciones homónimas del paquete 0.3.2.
 */

/* ═══════════════════════════════════════════════════════════════════════════
   MATRIZ DE ESTADOS
   ═══════════════════════════════════════════════════════════════════════════ */

const VARIANTES = [
  { v: 'primario', texto: 'Aprobar' },
  { v: 'secundario', texto: 'Devolver' },
  { v: 'peligro', texto: 'Anular' },
  { v: 'fantasma', texto: 'Cancelar' },
] as const;

const ESTADOS = ['reposo', 'hover', 'foco', 'activo', 'cargando', 'deshabilitado'] as const;

/** Clases que fuerzan el estado. En producción no existen: las resuelven las
 *  pseudoclases. Acá hacen falta para poder ver los seis a la vez. */
const FORZADO: Partial<Record<(typeof ESTADOS)[number], string>> = {
  hover: 'es-hover',
  foco: 'es-foco',
  activo: 'es-activo',
};

export function MatrizEstados(): ReactElement {
  return (
    <div className="tw:flex tw:flex-col tw:gap-3">
      <div className="tw:overflow-x-auto">
        <table className="tw:w-full tw:text-cuerpo-2">
          <thead>
            <tr className="tw:text-cabecera tw:font-semibold tw:tracking-[.08em] tw:text-tinta-mid tw:uppercase">
              <th className="loki-celda tw:py-2 tw:text-left">Variante</th>
              {ESTADOS.map((e) => (
                <th key={e} className="loki-celda tw:py-2 tw:text-left">
                  {e}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {VARIANTES.map(({ v, texto }) => (
              <tr key={v} className="tw:border-t tw:border-linea-suave">
                <td className="loki-celda tw:py-2 tw:font-mono tw:text-ayuda tw:text-acento-ink">
                  {v}
                </td>
                {ESTADOS.map((e) => (
                  <td key={e} className="loki-celda tw:py-2">
                    <Boton
                      variante={v}
                      className={FORZADO[e] ?? ''}
                      cargando={e === 'cargando'}
                      disabled={e === 'deshabilitado'}
                      title={e === 'deshabilitado' ? 'No tiene permiso en esta etapa' : undefined}
                    >
                      {e === 'cargando' ? `${texto.slice(0, -1)}ando` : texto}
                    </Boton>
                  </td>
                ))}
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      <p className="tw:text-cuerpo-2 tw:text-tinta-low">
        Un sistema de diseño <strong>se juzga por los estados</strong>, no por el ejemplar bonito.
        Y el deshabilitado <strong>nunca se oculta</strong>: si el usuario debería saber que la
        acción existe, se muestra apagada y el <code className="tw:font-mono">title</code> dice por
        qué — pasá el puntero por el de la última columna. Ocultarla lo obliga a adivinar si le
        falta un permiso, si la etapa no lo permite o si el sistema está roto.
      </p>
    </div>
  );
}

/* ═══════════════════════════════════════════════════════════════════════════
   ESTADOS DE CARGA
   ═══════════════════════════════════════════════════════════════════════════ */

const COLUMNAS: ColumnaDef[] = [
  { id: 'referencia', cabecera: 'Referencia' },
  { id: 'solicitante', cabecera: 'Solicitante' },
  { id: 'gerencia', cabecera: 'Gerencia' },
  { id: 'monto', cabecera: 'Monto', numerica: true },
];

const DATOS: readonly (readonly string[])[] = [
  ['MSV-26-1255-GT', 'Nolvia Esperanza Cruz', 'Gerencia de Operaciones', 'L. 1,284,990.65'],
  ['SOL-01293', 'Katherin Casildo', 'Gerencia de Tecnología', 'L. 49,990.65'],
  ['SOL-01291', 'Óscar Manuel Zavala', 'Gerencia de Operaciones', 'L. 12,480.00'],
  ['EXT-01292', 'Dilcia Ramos Portillo', 'Asesoría Legal', 'USD 1,240.00'],
];

export function EstadosDeCarga(): ReactElement {
  const [cargando, setCargando] = useState(true);

  return (
    <div className="tw:flex tw:flex-col tw:gap-3">
      <div className="tw:flex tw:items-center tw:gap-3">
        <Boton tamano="sm" onClick={() => setCargando((c) => !c)}>
          {cargando ? 'Alternar a cargado' : 'Alternar a cargando'}
        </Boton>
        <span className="tw:font-mono tw:text-ayuda tw:text-tinta-low">
          fase: {cargando ? 'cargando' : 'cargado'}
        </span>
      </div>

      {/*
        Las DOS familias de indicador, cada una con SU esqueleto — y se muestran juntas
        justamente porque confundirlas fue un defecto real: siete pantallas usaban la tarjeta
        con disco y el esqueleto del panel dividido, y la fila medía 9 px de más mientras
        cargaba. Acá se ve que no son intercambiables.
      */}
      <p className="tw:text-ayuda tw:text-tinta-low">
        Panel dividido — cada indicador <strong>es un filtro</strong>. Su esqueleto:{' '}
        <code>EsqueletoKpis</code>.
      </p>
      {cargando ? (
        <EsqueletoKpis columnas={4} />
      ) : (
        <FilaKpis
          kpis={[
            { id: 'proc', rotulo: 'En proceso', valor: '214', nota: 'de 260' },
            { id: 'vence', rotulo: 'Por vencer', valor: '31', tono: 'aviso', nota: '≤ 24 h' },
            { id: 'venc', rotulo: 'Vencidas', valor: '7', tono: 'riesgo', nota: 'pasaron su plazo' },
            { id: 'monto', rotulo: 'Monto del mes', valor: '2.14 M', nota: 'en lempiras' },
          ]}
        />
      )}

      <p className="tw:text-ayuda tw:text-tinta-low">
        Tarjeta suelta con disco de ícono — <strong>informa, no filtra</strong>. Su esqueleto:{' '}
        <code>EsqueletoTarjetasKpi</code>.
      </p>
      {cargando ? (
        <EsqueletoTarjetasKpi columnas={4} />
      ) : (
        <div className="tw:grid tw:gap-3 tw:sm:grid-cols-2 tw:xl:grid-cols-4">
          <TarjetaKpi titulo="En proceso" valor="214" ayuda="de 260" icono={<Users className="tw:size-4" />} tono="info" />
          <TarjetaKpi titulo="Por vencer" valor="31" ayuda="≤ 24 h" icono={<Users className="tw:size-4" />} tono="aviso" />
          <TarjetaKpi titulo="Vencidas" valor="7" ayuda="pasaron su plazo" icono={<Users className="tw:size-4" />} tono="riesgo" />
          <TarjetaKpi titulo="Monto del mes" valor="2.14 M" ayuda="en lempiras" icono={<Users className="tw:size-4" />} tono="ok" />
        </div>
      )}

      <div className="tw:overflow-x-auto tw:rounded-panel tw:border tw:border-linea tw:bg-panel">
        {cargando ? (
          <EsqueletoTabla columnas={COLUMNAS} filas={4} />
        ) : (
          <table className="tw:w-full">
            <thead>
              <tr>
                {COLUMNAS.map((c) => (
                  <th
                    key={c.id}
                    className={`loki-celda tw:text-cabecera tw:font-semibold tw:tracking-[.08em] tw:text-tinta-mid tw:uppercase ${
                      c.numerica === true ? 'tw:text-right' : 'tw:text-left'
                    }`}
                  >
                    {c.cabecera}
                  </th>
                ))}
              </tr>
            </thead>
            <tbody>
              {DATOS.map((fila) => (
                <tr key={fila[0]} className="loki-fila tw:border-t tw:border-linea-suave">
                  {fila.map((celda, i) => (
                    <td
                      key={i}
                      className={`loki-celda ${
                        i === fila.length - 1
                          ? 'tw:text-right tw:font-mono tw:text-importe tw:tabular-nums tw:text-tinta-base'
                          : 'tw:text-cuerpo-2 tw:text-tinta-base'
                      }`}
                    >
                      {celda}
                    </td>
                  ))}
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      <div className="tw:max-w-sm">
        {cargando ? (
          <EsqueletoLista etiquetas={['Gerencia', 'Destino', 'Fechas', 'Monto']} />
        ) : (
          <dl className="loki-dl-dos-columnas tw:grid tw:gap-2">
            {[
              ['Gerencia', 'Gerencia de Operaciones'],
              ['Destino', 'Puerto Cortés'],
              ['Fechas', '04–08/08/2026'],
              ['Monto', 'L. 49,990.65'],
            ].map(([k, v]) => (
              <div key={k} className="tw:contents">
                <dt className="tw:text-cuerpo-2 tw:text-tinta-mid">{k}</dt>
                <dd className="tw:text-cuerpo-2 tw:text-tinta-base">{v}</dd>
              </div>
            ))}
          </dl>
        )}
      </div>

      <p className="tw:text-cuerpo-2 tw:text-tinta-low">
        Alterná y fijate que <strong>nada salta</strong>: el alto sale de la misma tipografía en
        los dos estados. Y lo que ya sabemos <strong>no va en esqueleto</strong> — las cabeceras de
        la tabla y los rótulos de la lista se pintan de una vez, porque son nuestros y no del
        servidor. Pintarlos en gris finge una ignorancia que no tenemos.
      </p>
    </div>
  );
}

/* ═══════════════════════════════════════════════════════════════════════════
   FORMULARIO
   ═══════════════════════════════════════════════════════════════════════════ */

export function MuestraFormulario(): ReactElement {
  return (
    <div className="tw:flex tw:max-w-md tw:flex-col tw:gap-4">
      <Campo etiqueta="Zona aplicada" obligatorio ayuda="Determina la tarifa diaria junto con la categoría.">
        <input defaultValue="Zona 2 — costa norte" />
      </Campo>

      <Campo
        etiqueta="Fecha de finalización"
        obligatorio
        error="Sin una fecha posterior al inicio no se puede calcular la duración de la gira."
      >
        <input type="date" defaultValue="2026-08-02" />
      </Campo>

      <Campo etiqueta="Monto estimado" mono ayuda="Se recalcula por zona × categoría × duración.">
        <input defaultValue="L. 49,990.65" />
      </Campo>

      <p className="tw:text-cuerpo-2 tw:text-tinta-low">
        El error dice <strong>la causa</strong>, no «campo requerido»: qué esperaba el sistema y
        por qué le importa. Y se marca <strong>lo obligatorio</strong>, nunca lo opcional — en este
        sistema casi todo lo es, así que marcar lo opcional sería marcar casi nada.
      </p>
    </div>
  );
}

/* ═══════════════════════════════════════════════════════════════════════════
   AVISOS DE ESQUINA
   ═══════════════════════════════════════════════════════════════════════════ */

/**
 * Los cuatro tonos, cada uno con el disparador que lo produce.
 *
 * Hasta 2026-08-06 esto era una MAQUETA: cuatro `<div>` dibujados en la página que
 * se parecían a un aviso. Servía para mirar el color y para nada más — no probaba
 * la posición, ni el apilado, ni la animación de entrada, ni que el de riesgo se
 * quede hasta que lo cierren. Justo lo que hay que ver antes de replicarlo en las
 * pantallas. Ahora cada botón dispara el aviso REAL del sistema.
 */
const AVISOS: readonly {
  tono: Tono;
  boton: string;
  titulo: string;
  detalle: string;
  disparar: () => void;
}[] = [
  {
    tono: 'ok',
    boton: 'Éxito',
    titulo: 'SOL-01293 pasó a Revisión Presupuesto',
    detalle: 'Se reservaron L. 49,990.65 de la partida de la gerencia.',
    disparar: () =>
      avisar.exito('SOL-01293 pasó a Revisión Presupuesto', {
        detalle: 'Se reservaron L. 49,990.65 de la partida de la gerencia.',
      }),
  },
  {
    tono: 'riesgo',
    boton: 'Error',
    titulo: 'No se pudo anular la liquidación',
    detalle: 'El servidor la rechazó: ya fue pagada. Este aviso no se cierra solo.',
    disparar: () =>
      avisar.error('No se pudo anular la liquidación', {
        detalle: 'El servidor la rechazó: ya fue pagada. Este aviso no se cierra solo.',
      }),
  },
  {
    tono: 'aviso',
    boton: 'Alerta',
    titulo: 'Quedan 2 ciclos de corrección',
    detalle: 'Al sexto la solicitud se bloquea y sólo un administrador la destraba.',
    disparar: () =>
      avisar.alerta('Quedan 2 ciclos de corrección', {
        detalle: 'Al sexto la solicitud se bloquea y sólo un administrador la destraba.',
      }),
  },
  {
    tono: 'info',
    boton: 'Información',
    titulo: 'Tipo de cambio actualizado',
    detalle: 'Tesorería registró la tasa del día: L. 24.6180 por dólar.',
    disparar: () =>
      avisar.info('Tipo de cambio actualizado', {
        detalle: 'Tesorería registró la tasa del día: L. 24.6180 por dólar.',
      }),
  },
];

export function AvisosDeEsquina(): ReactElement {
  return (
    <div className="tw:flex tw:flex-col tw:gap-3">
      <div className="tw:flex tw:flex-wrap tw:gap-2">
        {AVISOS.map((a) => (
          <Boton key={a.boton} variante="secundario" onClick={a.disparar}>
            {a.boton}
          </Boton>
        ))}
        <span className="tw:w-px tw:self-stretch tw:bg-linea" aria-hidden="true" />
        {/* El quinto no es un tono: es el aviso ATADO a una operación. Empieza en
            «guardando», y al terminar el mismo aviso se convierte en el resultado.
            Es lo correcto para guardar y enviar: el usuario ve que algo pasa y
            después cómo terminó, en el mismo lugar y sin dos avisos. */}
        <Boton
          variante="secundario"
          onClick={() =>
            avisar.promesa(
              new Promise((resolver) => setTimeout(resolver, 1800)),
              {
                cargando: 'Generando orden de pago…',
                exito: 'OP-26-1255-GT generada con el membrete del INM',
                error: 'No se pudo generar la orden de pago',
              },
            )
          }
        >
          Atado a una operación
        </Boton>
      </div>

      <p className="tw:text-cuerpo-2 tw:text-tinta-low">
        Esquina superior derecha, <strong>sin telón</strong>: la pantalla sigue usable mientras el
        aviso está ahí. Apretá varios seguidos y se <strong>apilan</strong> en vez de taparse.
      </p>
      <p className="tw:text-cuerpo-2 tw:text-tinta-low">
        <strong>El error no se cierra solo</strong> — uno que se desvanece en cuatro segundos deja
        a alguien preguntándose qué decía; ése hay que cerrarlo a mano. Y el texto dice{' '}
        <strong>qué pasó y qué se puede hacer</strong>, no «Operación exitosa».
      </p>
    </div>
  );
}

/* ═══════════════════════════════════════════════════════════════════════════
   FECHAS — la regla D8
   ═══════════════════════════════════════════════════════════════════════════ */

/**
 * Un período se pide con UN control, no con dos campos.
 *
 * La muestra pone los dos lado a lado a propósito: el par de campos sueltos se ve razonable
 * hasta que está el otro al lado y se nota que hay que restar de cabeza para saber cuánto dura.
 */
export function MuestraFechas(): ReactElement {
  const [malIni, setMalIni] = useState('2026-09-14');
  const [malFin, setMalFin] = useState('2026-09-23');
  const [bienIni, setBienIni] = useState('2026-09-14');
  const [bienFin, setBienFin] = useState('2026-09-23');
  const [suelta, setSuelta] = useState('2026-09-14');

  const campo =
    'loki-foco tw:rounded-control tw:border tw:border-linea tw:bg-panel tw:px-2 tw:py-1.5 tw:text-cuerpo-2 tw:text-tinta-hi';

  return (
    <div className="tw:flex tw:flex-col tw:gap-4">
      <div className="tw:grid tw:gap-4 tw:lg:grid-cols-2">
        {/* Incorrecto */}
        <div className="tw:rounded-control tw:border tw:border-riesgo-bd tw:bg-riesgo-bg tw:p-3">
          <p className="tw:mb-2 tw:text-cuerpo-2 tw:font-semibold tw:text-tinta-hi">
            Incorrecto — dos campos para un período
          </p>
          <div className="tw:flex tw:flex-wrap tw:gap-3">
            <label className="tw:flex tw:flex-col tw:gap-1">
              <span className="tw:text-ayuda tw:text-tinta-low">Fecha de inicio</span>
              <input
                type="date"
                value={malIni}
                onChange={(e) => { setMalIni(e.target.value); }}
                className={campo}
              />
            </label>
            <label className="tw:flex tw:flex-col tw:gap-1">
              <span className="tw:text-ayuda tw:text-tinta-low">Fecha de finalización</span>
              <input
                type="date"
                value={malFin}
                onChange={(e) => { setMalFin(e.target.value); }}
                className={campo}
              />
            </label>
          </div>
          <p className="tw:mt-2 tw:text-ayuda tw:text-tinta-low">
            Hay que abrir el calendario dos veces, y la duración —lo que multiplica la tarifa—
            queda para restarla de cabeza.
          </p>
        </div>

        {/* Correcto */}
        <div className="tw:rounded-control tw:border tw:border-ok-bd tw:bg-ok-bg tw:p-3">
          <p className="tw:mb-2 tw:text-cuerpo-2 tw:font-semibold tw:text-tinta-hi">
            Correcto — un control, con la duración a la vista
          </p>
          <RangoFechas
            desde={bienIni}
            hasta={bienFin}
            etiqueta="Período de la gira"
            min="2026-09-01"
            max="2026-10-31"
            onCambiar={(d, h) => { setBienIni(d); setBienFin(h); }}
            ayuda="Acotado al período de la solicitud: los días de fuera están apagados."
          />
          <p className="tw:mt-2 tw:text-ayuda tw:text-tinta-low">
            Dos meses a la vista —una gira de una semana cruza el fin de mes casi siempre—, los
            campos nativos siguen ahí para teclear, y lo que el servidor rechazaría no se puede
            elegir.
          </p>
        </div>
      </div>

      {/* La excepción */}
      <div className="tw:rounded-control tw:border tw:border-linea tw:p-3">
        <p className="tw:mb-2 tw:text-cuerpo-2 tw:font-semibold tw:text-tinta-hi">
          La única excepción — una fecha suelta que no forma período
        </p>
        <label className="tw:flex tw:flex-col tw:gap-1">
          <span className="tw:text-ayuda tw:text-tinta-low">Fecha de la tasa</span>
          <CampoFecha valor={suelta} onCambiar={setSuelta} etiqueta="Fecha de la tasa" />
        </label>
        <p className="tw:mt-2 tw:text-ayuda tw:text-tinta-low">
          La fecha de un tipo de cambio o el corte de un reporte no tienen «hasta»: ahí no hay
          rango que mostrar. Pero el calendario <b className="tw:text-tinta-mid">es el mismo</b> —
          el desplegable del navegador, con su fondo claro y su tipografía, se apaga en los dos
          casos.
        </p>
      </div>
    </div>
  );
}

/* ═══════════════════════════════════════════════════════════════════════════
   BÚSQUEDA — la regla D11
   ═══════════════════════════════════════════════════════════════════════════ */

/**
 * Los dos comportamientos de un buscador, y por qué no son intercambiables.
 *
 * La muestra los pone lado a lado porque el error se ve sólo así: los dos campos parecen
 * el mismo control, y la diferencia —si el resultado ya está acá o hay que ir a pedirlo—
 * decide si el botón sobra o hace falta.
 */
export function MuestraBusqueda(): ReactElement {
  const EMPLEADOS = [
    'Adrián Reyes Santos',
    'Gerardo Guzmán Mejía',
    'Katherin Casildo',
    'Tirza Pérez',
    'Benilda Pérez',
  ];

  const [filtro, setFiltro] = useState('');
  const [termino, setTermino] = useState('');
  const [pedido, setPedido] = useState<string | null>(null);

  const visibles = EMPLEADOS.filter((e) =>
    e.toLowerCase().includes(filtro.trim().toLowerCase()),
  );

  return (
    <div className="tw:flex tw:flex-col tw:gap-4">
      <div className="tw:grid tw:gap-4 tw:lg:grid-cols-2">
        {/* Filtra */}
        <div className="tw:rounded-control tw:border tw:border-linea tw:bg-panel tw:p-3">
          <p className="tw:mb-1 tw:text-cuerpo-2 tw:font-semibold tw:text-tinta-hi">
            Filtra — la lista ya está acá
          </p>
          <p className="tw:mb-3 tw:text-ayuda tw:text-tinta-low">
            Recorta mientras se escribe. <b className="tw:text-tinta-mid">Sin botón</b>: no hay
            nada que esperar, y un botón sugeriría que hay que pulsarlo para ver algo.
          </p>
          <CampoBusqueda etiqueta="Buscar empleado…" valor={filtro} onCambio={setFiltro} />
          <ul className="tw:mt-3 tw:flex tw:flex-col tw:gap-1">
            {visibles.length === 0 ? (
              <li className="tw:text-cuerpo-2 tw:text-tinta-low">Ninguno coincide.</li>
            ) : (
              visibles.map((e) => (
                <li key={e} className="tw:text-cuerpo-2 tw:text-tinta-base">{e}</li>
              ))
            )}
          </ul>
        </div>

        {/* Consulta */}
        <div className="tw:rounded-control tw:border tw:border-linea tw:bg-panel tw:p-3">
          <p className="tw:mb-1 tw:text-cuerpo-2 tw:font-semibold tw:text-tinta-hi">
            Consulta — hay que ir a pedirla
          </p>
          <p className="tw:mb-3 tw:text-ayuda tw:text-tinta-low">
            Se dispara con <b className="tw:text-tinta-mid">Enter o el botón</b>, nunca al tipear:
            escribir ocho letras serían ocho viajes de los que siete se descartan, y contesta el
            que llega último, no el que se pidió último.
          </p>
          <CampoBusqueda
            etiqueta="Buscar en el hilo…"
            valor={termino}
            onCambio={setTermino}
            alBuscar={(v) => { setPedido(v); }}
          />
          <p className="tw:mt-3 tw:text-cuerpo-2 tw:text-tinta-base">
            {pedido === null
              ? 'Todavía no se pidió nada.'
              : pedido === ''
                ? 'Se limpió: la consulta se rehizo vacía.'
                : `Se le pidió al servidor: «${pedido}»`}
          </p>
        </div>
      </div>

      <p className="tw:text-ayuda tw:text-tinta-low">
        Los dos traen <b className="tw:text-tinta-mid">ícono, ✕ para limpiar y nombre accesible</b>.
        La ✕ es propia: <code className="tw:font-mono">type=&quot;search&quot;</code> dibuja una en
        Chrome y <b className="tw:text-tinta-mid">ninguna en Firefox</b>, así que la nativa se apaga
        y va la del contrato — que además devuelve el foco al campo, y no a un botón que acaba de
        desaparecer. <kbd>Esc</kbd> limpia en los dos.
      </p>
    </div>
  );
}

/**
 * Cuándo esqueleto y cuándo rueda — la distinción que hace útil a la regla D13.
 *
 * La muestra las pone juntas porque el error es elegir la equivocada, no dibujarlas mal.
 */
export function EsqueletoContraRueda(): ReactElement {
  const [fase, setFase] = useState<'cargando' | 'cargado'>('cargando');

  return (
    <div className="tw:flex tw:flex-col tw:gap-3">
      <div className="tw:flex tw:items-center tw:gap-3">
        <Boton tamano="sm" onClick={() => setFase((f) => (f === 'cargando' ? 'cargado' : 'cargando'))}>
          {fase === 'cargando' ? 'Alternar a cargado' : 'Alternar a cargando'}
        </Boton>
        <span className="tw:font-mono tw:text-ayuda tw:text-tinta-low">fase: {fase}</span>
      </div>

      <div className="tw:grid tw:gap-4 tw:lg:grid-cols-2">
        <div className="tw:rounded-control tw:border tw:border-linea tw:bg-panel tw:p-3">
          <p className="tw:mb-1 tw:text-cuerpo-2 tw:font-semibold tw:text-tinta-hi">
            Esqueleto — llega un dato con forma conocida
          </p>
          <p className="tw:mb-3 tw:text-ayuda tw:text-tinta-low">
            Reserva el lugar exacto, así que <b className="tw:text-tinta-mid">nada salta</b> al
            llegar. Alterná la fase y mirá que la altura no cambia.
          </p>
          {fase === 'cargando' ? (
            <EsqueletoFichas cantidad={2} lineas={1} />
          ) : (
            <div className="tw:flex tw:flex-col tw:gap-2">
              {[
                ['Gira Norte-Atlántico', 'Del 12 al 19 de agosto · 4 personas'],
                ['Gira Occidente', 'Del 2 al 5 de septiembre · 2 personas'],
              ].map(([t, m]) => (
                <div
                  key={t}
                  className="loki-pad-panel tw:rounded-panel tw:border tw:border-linea tw:bg-panel"
                >
                  <div className="tw:text-cuerpo tw:font-semibold tw:text-tinta-hi">{t}</div>
                  <div className="tw:text-cuerpo-2 tw:text-tinta-mid">{m}</div>
                </div>
              ))}
            </div>
          )}
        </div>

        <div className="tw:rounded-control tw:border tw:border-linea tw:bg-panel tw:p-3">
          <p className="tw:mb-1 tw:text-cuerpo-2 tw:font-semibold tw:text-tinta-hi">
            Rueda — corre una acción y después nos vamos
          </p>
          <p className="tw:mb-3 tw:text-ayuda tw:text-tinta-low">
            Acá <b className="tw:text-tinta-mid">no hay forma que reservar</b>: al terminar, la
            pantalla cambia. Un esqueleto prometería un lugar que nunca se va a llenar.
          </p>
          <div className="tw:flex tw:items-center tw:gap-2 tw:py-6 tw:text-cuerpo-2 tw:text-tinta-mid">
            <span
              className="tw:size-4 tw:animate-spin tw:rounded-full tw:border-2 tw:border-linea tw:border-t-btn"
              aria-hidden="true"
            />
            <span role="status">Abriendo una nueva solicitud nacional…</span>
          </div>
        </div>
      </div>
    </div>
  );
}
