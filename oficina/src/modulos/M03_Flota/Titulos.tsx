import type { ReactElement } from 'react';
import { useMemo, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { CalendarClock, CircleAlert, FileSignature, ScrollText } from 'lucide-react';

import {
  Boton,
  Campo,
  CampoBusqueda,
  CampoFecha,
  Modal,
  Nota,
  Panel,
  Pastilla,
  Tabla,
  Vacio,
  avisar,
} from '../../ui';
import type { ColumnaDef, Tono } from '../../ui';
import { BloqueoDuro } from '../../api/misiones';
import {
  REGIMENES,
  RUBROS,
  coberturaDeTitulos,
  nuevoUlid,
  registrarTitulo,
  serieDeTitulos,
} from '../../api/titulos';
import type {
  CoberturaDeTitulo,
  QuienAsume,
  Regimen,
  RubrosNuevos,
  TituloDeTenencia,
} from '../../api/titulos';
import { soloFecha } from '../M06_Autorizacion/formato';

/**
 * `RN-62` — Los títulos de tenencia de la flota.
 *
 * ── Lo que esta pantalla contesta ───────────────────────────────────────────
 * **¿Bajo qué régimen tenemos cada vehículo?** De esa respuesta cuelgan tres controles: si el
 * vehículo se puede habilitar, si la ventana de una misión cabe dentro de la vigencia, y
 * **cuál de los dos terminales corresponde** cuando sale de la flota — el descargo es de
 * bienes propios y el retiro, de ajenos (`HB3-17`).
 *
 * ── Por qué la cobertura va primero y no la lista ───────────────────────────
 * Mientras un vehículo no tenga título, el sistema **advierte y deja pasar**: frenar el
 * descargo de toda la flota por un dato de alta que nadie llenó sería peor que el asiento que
 * se quiere evitar. Eso convierte cada vehículo sin título en **un control apagado, uno por
 * uno**, y un control apagado que nadie ve es indistinguible de uno que nunca hizo falta.
 *
 * ── Lo que esta pantalla NO hace ────────────────────────────────────────────
 * **No cierra ni corrige un título.** El régimen cambia cerrando el anterior y abriendo el
 * nuevo, y ninguna de las dos operaciones existe todavía en la API. Ofrecer un botón que el
 * servidor no atiende se lee como falla del sistema, no como regla (`ADR-008`).
 */
export default function Titulos(): ReactElement {
  const [filtro, setFiltro] = useState('');
  const [aRegistrar, setARegistrar] = useState<CoberturaDeTitulo | null>(null);
  const [aVerSerie, setAVerSerie] = useState<CoberturaDeTitulo | null>(null);

  const { data, isPending, isError } = useQuery({
    queryKey: ['cobertura-titulos'],
    queryFn: coberturaDeTitulos,
  });

  const filas = useMemo(() => {
    const todos = data ?? [];
    const busqueda = filtro.trim().toLowerCase();
    if (!busqueda) return todos;

    // Se busca contra **lo que la fila muestra**. Quien teclea «comodato» o «sin título»
    // busca esas palabras porque están escritas en la tabla, no en los campos crudos.
    return todos.filter((c) =>
      [
        c.siglas,
        c.placa ?? 'sin placa metálica',
        c.tipoDeVehiculo,
        c.titulo ? TEXTO_DE_REGIMEN[c.titulo.regimen] ?? c.titulo.regimen : situacion(c).texto,
        c.titulo?.titular ?? '',
        c.titulo?.documento ?? '',
      ]
        .join(' ')
        .toLowerCase()
        .includes(busqueda),
    );
  }, [data, filtro]);

  if (isError) {
    return (
      <Nota tono="riesgo" icono={<CircleAlert />}>
        No se pudieron cargar los títulos de tenencia. Vuelva a intentar en unos segundos.
      </Nota>
    );
  }

  const todos = data ?? [];

  // Las tres situaciones se cuentan por separado porque **exigen tres acciones distintas**:
  // llenar un dato de alta, recuperar un bien que ya debía volver, y renovar un documento
  // antes de que frene la próxima misión.
  //
  // Y las tres se cuentan **sólo sobre la flota viva**: pedir el título de un vehículo que ya
  // se descargó es trabajo que no enciende ningún control, y sumarlo al hueco vuelve el número
  // inservible para decidir por dónde empezar.
  const enFlota = todos.filter((c) => !c.fueraDeLaFlota);
  const nunca = enFlota.filter((c) => situacion(c).clase === 'nunca');
  const vencidos = enFlota.filter((c) => situacion(c).clase === 'vencido');
  const porVencer = enFlota.filter((c) => situacion(c).clase === 'por-vencer');

  return (
    <div className="tw:flex tw:flex-col tw:gap-5">
      <header className="tw:flex tw:flex-col tw:gap-1">
        <h1 className="tw:text-xl tw:font-semibold tw:tracking-tight">Títulos de tenencia</h1>
        <p className="tw:text-sm tw:text-tinta-mid">
          Bajo qué régimen tenemos cada vehículo, con qué documento y hasta cuándo. De acá sale
          si el bien es del Estado — y con eso, <b>cuál de los dos terminales corresponde</b>{' '}
          cuando el vehículo sale de la flota.
        </p>
      </header>

      {/* El bien ajeno corrido de plazo va PRIMERO: no es un dato que falta, es un vehículo
          del Estado en manos de otro fuera de la fecha comprometida. */}
      {vencidos.length > 0 && (
        <Nota tono="riesgo" icono={<CalendarClock />}>
          <b>
            {vencidos.length === 1
              ? '1 vehículo tiene el título vencido'
              : `${vencidos.length} vehículos tienen el título vencido`}
          </b>{' '}
          y ninguna misión suya se puede programar. Un comodato prorrogado verbalmente{' '}
          <b>no existe para el sistema</b>: la vigencia es la del documento, y sin la adenda
          adjunta el bloqueo opera. Es incómodo y es correcto.
        </Nota>
      )}

      {/* Cada uno es un control apagado, y por eso se nombra QUÉ deja de evaluarse. Un
          «faltan datos» a secas no dice qué se está perdiendo por no llenarlos. */}
      {nunca.length > 0 && (
        <Nota tono="aviso" icono={<CircleAlert />}>
          {nunca.length === 1
            ? '1 vehículo no tiene título de tenencia declarado'
            : `${nunca.length} vehículos no tienen título de tenencia declarado`}, y en ellos{' '}
          <code className="tw:font-mono tw:text-xs">RN-62</code> <b>queda sin evaluar</b>: la
          ventana de sus misiones no se contrasta contra ninguna vigencia, y al darlos de baja
          el sistema advierte en vez de juzgar si el terminal corresponde. Hoy{' '}
          <b>eso no los frena</b> —frenar el descargo de toda la flota por un dato de alta que
          nadie llenó sería peor que el asiento que se quiere evitar—, pero cada uno es un
          control apagado.
        </Nota>
      )}

      {porVencer.length > 0 && (
        <Nota tono="aviso">
          {porVencer.length === 1
            ? '1 título vence dentro de 60 días'
            : `${porVencer.length} títulos vencen dentro de 60 días`}
          . El bloqueo no espera al vencimiento:{' '}
          <b>la ventana de la misión tiene que caber entera</b> dentro de la vigencia, así que
          una misión que vuelve después de esa fecha ya no se programa.
        </Nota>
      )}

      <CampoBusqueda
        etiqueta="Buscar por siglas, régimen o titular…"
        valor={filtro}
        onCambio={setFiltro}
      />

      {!isPending && filas.length === 0 ? (
        <Vacio
          icono={<ScrollText />}
          titulo={filtro ? 'Ningún vehículo coincide' : 'No hay vehículos en la flota'}
          descripcion={
            filtro
              ? 'Pruebe con las siglas completas, o limpie la búsqueda.'
              : 'El título se registra sobre un vehículo del padrón, y el padrón está vacío.'
          }
        />
      ) : (
        <Tabla
          columnas={COLUMNAS(setARegistrar, setAVerSerie)}
          filas={filas}
          claveDe={(c) => c.vehiculo}
          cargando={isPending}
        />
      )}

      {aRegistrar && (
        <DialogoDeTitulo vehiculo={aRegistrar} onCerrar={() => setARegistrar(null)} />
      )}

      {aVerSerie && <DialogoDeSerie vehiculo={aVerSerie} onCerrar={() => setAVerSerie(null)} />}
    </div>
  );
}

/**
 * En qué situación está la tenencia del vehículo.
 *
 * ── Las cuatro no son grados de lo mismo ────────────────────────────────────
 * **«Nunca tuvo título» y «se le venció» llegan las dos sin título vigente**, y son opuestas:
 * la primera es un dato de alta que nadie llenó; la segunda es un bien ajeno que ya debía
 * haberse devuelto. Pintarlas igual esconde la segunda entre las primeras, que es justo la
 * que hay que ver.
 */
function situacion(c: CoberturaDeTitulo): {
  clase: 'nunca' | 'vencido' | 'por-vencer' | 'vigente' | 'fuera';
  texto: string;
  tono: Tono;
} {
  // Va antes que todo lo demás: una unidad descargada sin título no es un dato pendiente, es
  // un expediente cerrado. Lo que quedó sin verificarse ahí ya no se arregla llenando el
  // formulario — es hallazgo de auditoría, y esta pantalla no es esa.
  if (c.fueraDeLaFlota) {
    return { clase: 'fuera', texto: 'Fuera de la flota', tono: 'neutro' };
  }

  if (c.titulo === null) {
    return c.ultimo === null
      ? { clase: 'nunca', texto: 'Sin título declarado', tono: 'neutro' }
      : { clase: 'vencido', texto: 'Título vencido', tono: 'riesgo' };
  }

  // Los días restantes son **nulos en propiedad**: no vence, y no hay nada que avisar.
  if (c.titulo.diasRestantes !== null && c.titulo.diasRestantes <= DIAS_DE_AVISO) {
    return { clase: 'por-vencer', texto: `Vence en ${c.titulo.diasRestantes} días`, tono: 'aviso' };
  }

  return { clase: 'vigente', texto: 'Vigente', tono: 'ok' };
}

/**
 * Sesenta días.
 *
 * **No es parámetro con vigencia y no debe serlo**: no decide ningún número que alguien vaya a
 * cobrar ni pagar, ni bloquea nada. Es cuándo esta pantalla empieza a avisar, y el bloqueo
 * real lo sigue poniendo la vigencia del documento contra la ventana de cada misión.
 */
const DIAS_DE_AVISO = 60;

/**
 * El régimen va primero: es lo que decide todo lo demás de esta pantalla, y se lee sin abrir
 * nada.
 */
const COLUMNAS = (
  alRegistrar: (c: CoberturaDeTitulo) => void,
  alVerSerie: (c: CoberturaDeTitulo) => void,
): ColumnaDef<CoberturaDeTitulo>[] => [
  {
    id: 'regimen',
    cabecera: 'Régimen',
    ancho: 190,
    celda: (c) => <RegimenVista cobertura={c} />,
    ordenable: true,
    // Lo que hay que resolver, arriba: el bien ajeno corrido de plazo, después el hueco.
    valorOrden: (c) => PESO_DE_SITUACION[situacion(c).clase],
  },
  {
    id: 'siglas',
    cabecera: 'Vehículo',
    ancho: 150,
    celda: (c) => (
      <div className="tw:flex tw:flex-col">
        <span className="tw:font-mono tw:text-[13px]">{c.siglas}</span>
        <span className="tw:text-xs tw:text-tinta-mid">
          {c.placa ?? <span className="tw:italic">sin placa metálica</span>}
        </span>
      </div>
    ),
    ordenable: true,
    valorOrden: (c) => c.siglas,
  },
  {
    id: 'titular',
    cabecera: 'Titular y documento',
    celda: (c) =>
      c.titulo === null ? (
        <span className="tw:text-tinta-mid">—</span>
      ) : (
        <div className="tw:flex tw:flex-col">
          <span>{c.titulo.titular}</span>
          {/* «Una prórroga verbal no existe»: el documento es la vigencia, así que se
              muestra junto al titular y no escondido en el detalle. */}
          <span className="tw:text-xs tw:text-tinta-mid">{c.titulo.documento}</span>
        </div>
      ),
    ordenable: true,
    valorOrden: (c) => c.titulo?.titular ?? '',
  },
  {
    id: 'vigencia',
    cabecera: 'Vigencia',
    ancho: 175,
    celda: (c) => <VigenciaVista cobertura={c} />,
    ordenable: true,
    // Sin vencimiento va al final: es lo que nunca hay que atender.
    valorOrden: (c) => c.titulo?.hasta ?? (c.ultimo?.hasta ?? '9999-12-31'),
  },
  {
    id: 'rubros',
    cabecera: 'No lo pagamos nosotros',
    ancho: 210,
    celda: (c) => <RubrosVista titulo={c.titulo} />,
  },
  {
    id: 'accion',
    cabecera: '',
    ancho: 165,
    celda: (c) => (
      <div className="tw:flex tw:gap-2">
        {/* La serie sólo se ofrece cuando hay más de uno: un botón que abre un diálogo con
            una sola fila que ya está en la tabla enseña a no apretarlo. */}
        {c.enLaSerie > 1 && (
          <Boton variante="fantasma" tamano="sm" onClick={() => alVerSerie(c)}>
            Serie ({c.enLaSerie})
          </Boton>
        )}
        {/* Registrar un título sobre una unidad que ya salió de la flota no enciende ningún
            control: no se le va a programar nada y su terminal ya ocurrió. */}
        {!c.fueraDeLaFlota && (
          <Boton variante="secundario" tamano="sm" onClick={() => alRegistrar(c)}>
            Registrar
          </Boton>
        )}
      </div>
    ),
  },
];

const PESO_DE_SITUACION: Record<ReturnType<typeof situacion>['clase'], number> = {
  vencido: 0,
  nunca: 1,
  'por-vencer': 2,
  vigente: 3,
  // Al final: no hay nada que hacer con ellas.
  fuera: 4,
};

const TEXTO_DE_REGIMEN: Record<string, string> = Object.fromEntries(
  REGIMENES.map((r) => [r.valor, r.texto]),
);

/**
 * El régimen con su situación.
 *
 * <b>Se decide por identificador y nunca por el texto.</b> Si el servidor agrega un régimen
 * que esta pantalla no conoce, se muestra el identificador crudo antes que ocultarlo: uno sin
 * traducir se ve raro, uno escondido se ve como si no hubiera régimen.
 */
function RegimenVista({ cobertura }: { cobertura: CoberturaDeTitulo }): ReactElement {
  const s = situacion(cobertura);

  if (s.clase === 'fuera' || cobertura.titulo === null) {
    const previo = cobertura.titulo ?? cobertura.ultimo;

    return (
      <div className="tw:flex tw:flex-col tw:gap-1 tw:items-start">
        <Pastilla tono={s.tono}>{s.texto}</Pastilla>

        {/* Se dice CUÁL régimen, porque el bien sigue siendo de ese titular.
            En pasado sólo cuando el título dejó de regir: una unidad que salió de la flota
            puede conservar su comodato vigente, y decir «era» ahí sería falso. */}
        {previo && (
          <span className="tw:text-xs tw:text-tinta-mid">
            {s.clase === 'vencido' ? 'era ' : ''}
            {TEXTO_DE_REGIMEN[previo.regimen] ?? previo.regimen}
          </span>
        )}
      </div>
    );
  }

  return (
    <div className="tw:flex tw:flex-col tw:gap-1 tw:items-start">
      <Pastilla tono={cobertura.titulo.esBienPropio ? 'ok' : 'info'}>
        {TEXTO_DE_REGIMEN[cobertura.titulo.regimen] ?? cobertura.titulo.regimen}
      </Pastilla>
      {/* Lo que decide el terminal, dicho en la fila y no sólo en el diálogo del descargo:
          quien mira esta pantalla está decidiendo justamente eso. */}
      <span className="tw:text-xs tw:text-tinta-mid">
        {cobertura.titulo.esBienPropio ? 'bien del Estado' : 'bien ajeno'}
      </span>
    </div>
  );
}

function VigenciaVista({ cobertura }: { cobertura: CoberturaDeTitulo }): ReactElement {
  const t = cobertura.titulo ?? cobertura.ultimo;

  if (t === null) return <span className="tw:text-tinta-mid">—</span>;

  // Nula en propiedad: el bien es del Estado y no vence. Se dice, no se deja en blanco.
  if (t.hasta === null) {
    return <span className="tw:text-tinta-mid">sin vencimiento</span>;
  }

  const s = situacion(cobertura);

  return (
    <div className="tw:flex tw:flex-col">
      <span className="tw:tabular-nums">{soloFecha(t.hasta)}</span>
      {(s.clase === 'vencido' || s.clase === 'por-vencer') && (
        <span
          className={`tw:text-xs ${s.clase === 'vencido' ? 'tw:text-riesgo-fg' : 'tw:text-aviso-fg'}`}
        >
          {s.clase === 'vencido' ? 'vencido' : s.texto.toLowerCase()}
        </span>
      )}
    </div>
  );
}

/**
 * Los rubros que cubre el titular, y los que nadie pactó.
 *
 * ── «Sin pactar» no es «la institución» ─────────────────────────────────────
 * Es el rubro que aparece cuando llega la factura y empieza la discusión con el contrato en la
 * mano. Se nombra aparte y no se suma a ninguno de los dos lados.
 */
function RubrosVista({ titulo }: { titulo: TituloDeTenencia | null }): ReactElement {
  if (titulo === null) return <span className="tw:text-tinta-mid">—</span>;

  return (
    <div className="tw:flex tw:flex-col tw:gap-0.5 tw:text-xs">
      {titulo.rubrosDelTitular.length > 0 ? (
        <span>{titulo.rubrosDelTitular.join(', ')}</span>
      ) : (
        <span className="tw:text-tinta-mid">todo lo pagamos nosotros</span>
      )}

      {titulo.rubrosSinPactar.length > 0 && (
        <span className="tw:text-aviso-fg">
          sin pactar: {titulo.rubrosSinPactar.join(', ')}
        </span>
      )}
    </div>
  );
}

/**
 * La serie completa del vehículo.
 *
 * **De la serie manda el que regía a la fecha del hecho**, no el vigente hoy. Cuando llega una
 * factura de marzo, la pregunta no es bajo qué régimen tenemos el vehículo ahora.
 */
function DialogoDeSerie({
  vehiculo,
  onCerrar,
}: {
  vehiculo: CoberturaDeTitulo;
  onCerrar(): void;
}): ReactElement {
  const serie = useQuery({
    queryKey: ['serie-titulos', vehiculo.vehiculo],
    queryFn: () => serieDeTitulos(vehiculo.vehiculo),
  });

  return (
    <Modal
      abierto
      titulo={`Títulos de ${vehiculo.siglas}`}
      descripcion="El régimen cambió en algún momento. Las misiones de cada período se juzgan contra el título que regía entonces, no contra el vigente hoy."
      onCerrar={onCerrar}
      // Es un diálogo de lectura: no hay nada que confirmar, así que la única salida es
      // cerrarlo. Un segundo botón al lado obligaría a leer cuál de los dos no hace nada.
      etiquetaCerrar="Cerrar"
      acciones={null}
    >
      {serie.isPending ? (
        <p className="tw:text-sm tw:text-tinta-mid">Cargando la serie…</p>
      ) : serie.isError ? (
        <Nota tono="aviso">No se pudo cargar la serie de títulos de esta unidad.</Nota>
      ) : (
        <div className="tw:flex tw:flex-col tw:gap-3">
          {serie.data.map((t) => (
            <div
              key={t.id}
              className={`tw:flex tw:flex-col tw:gap-1 tw:border-l-2 tw:pl-3 ${
                t.vigente ? 'tw:border-ok-fg' : 'tw:border-linea'
              }`}
            >
              <div className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-3 tw:text-sm">
                <span className="tw:font-medium">
                  {TEXTO_DE_REGIMEN[t.regimen] ?? t.regimen}
                </span>
                <span className="tw:text-tinta-mid">{t.titular}</span>
                {t.vigente && <Pastilla tono="ok">Rige hoy</Pastilla>}
              </div>

              <span className="tw:text-xs tw:text-tinta-mid">
                {t.documento} · desde el {soloFecha(t.desde)}
                {t.hasta === null ? ' · sin vencimiento' : ` hasta el ${soloFecha(t.hasta)}`}
              </span>

              {t.rubrosDelTitular.length > 0 && (
                <span className="tw:text-xs tw:text-tinta-mid">
                  cubría el titular: {t.rubrosDelTitular.join(', ')}
                </span>
              )}

              {t.rubrosSinPactar.length > 0 && (
                <span className="tw:text-xs tw:text-aviso-fg">
                  sin pactar: {t.rubrosSinPactar.join(', ')}
                </span>
              )}
            </div>
          ))}
        </div>
      )}
    </Modal>
  );
}

/**
 * Registrar un título — la carga que enciende los tres controles.
 *
 * ── Por qué la fecha de fin aparece y desaparece ────────────────────────────
 * **La propiedad es el único régimen que no vence.** Ponerle fecha haría que el vehículo se
 * inhabilitara solo el día que alguien eligió sin que ninguna norma lo mandara, y el servidor
 * la rechaza. Los demás sí la exigen: un comodato que no vence es una apropiación.
 */
function DialogoDeTitulo({
  vehiculo,
  onCerrar,
}: {
  vehiculo: CoberturaDeTitulo;
  onCerrar(): void;
}): ReactElement {
  const cliente = useQueryClient();

  const [regimen, setRegimen] = useState<Regimen | ''>('');
  const [titular, setTitular] = useState('');
  const [documento, setDocumento] = useState('');
  const [desde, setDesde] = useState(() => new Date().toISOString().slice(0, 10));
  const [hasta, setHasta] = useState('');

  // Los siete arrancan **sin pactar**, no en «la institución». Suponer que pagamos nosotros
  // es exactamente la conclusión que hay que dejar en manos de quien llena el formulario.
  const [rubros, setRubros] = useState<RubrosNuevos>({
    combustible: 'SinPactar',
    mantenimiento: 'SinPactar',
    llantas: 'SinPactar',
    seguro: 'SinPactar',
    peajes: 'SinPactar',
    multas: 'SinPactar',
    danios: 'SinPactar',
  });

  const elegido = REGIMENES.find((r) => r.valor === regimen);
  const vence = elegido !== undefined && elegido.valor !== 'Propiedad';

  const completo =
    regimen !== '' &&
    titular.trim().length >= 3 &&
    documento.trim().length >= 3 &&
    desde !== '' &&
    (!vence || hasta !== '');

  const operacion = useMutation({
    mutationFn: () =>
      registrarTitulo({
        id: nuevoUlid(),
        idVehiculo: vehiculo.vehiculo,
        regimen: regimen as Regimen,
        titular: titular.trim(),
        documento: documento.trim(),
        desde,
        // Nula en propiedad, y el servidor rechaza que venga con valor.
        hasta: vence ? hasta : null,
        ...rubros,
      }),
    onSuccess: async () => {
      avisar.exito(`${vehiculo.siglas} quedó con título de tenencia: ${elegido!.texto}.`);
      await cliente.invalidateQueries({ queryKey: ['cobertura-titulos'] });
      await cliente.invalidateQueries({ queryKey: ['serie-titulos', vehiculo.vehiculo] });
      onCerrar();
    },
    onError: (e) => {
      // El servidor rechaza por `RN-62` —solape con otro título, propiedad con vencimiento,
      // régimen temporal sin él— y el motivo es lo único que dice cuál fue.
      if (e instanceof BloqueoDuro) {
        avisar.error(e.message);
        return;
      }
      avisar.error('No se pudo registrar el título. Nada quedó guardado.');
    },
  });

  return (
    <Modal
      abierto
      titulo={`Título de tenencia de ${vehiculo.siglas}`}
      descripcion={
        vehiculo.titulo === null
          ? 'Hoy no consta bajo qué régimen tenemos esta unidad, así que RN-62 no se evalúa sobre ella.'
          : `Hoy rige un título de ${(TEXTO_DE_REGIMEN[vehiculo.titulo.regimen] ?? vehiculo.titulo.regimen).toLowerCase()}. Uno nuevo que se solape va a ser rechazado: dos títulos vigentes a la vez dejarían al vehículo en dos regímenes al mismo tiempo.`
      }
      onCerrar={onCerrar}
      acciones={
        <Boton
          variante="primario"
          disabled={!completo || operacion.isPending}
          cargando={operacion.isPending}
          onClick={() => operacion.mutate()}
        >
          Registrar título
        </Boton>
      }
    >
      <div className="tw:flex tw:flex-col tw:gap-4">
        <fieldset className="tw:flex tw:flex-col tw:gap-2">
          <legend className="tw:mb-1 tw:text-sm tw:font-medium">Régimen</legend>

          {REGIMENES.map((r) => (
            <label
              key={r.valor}
              className="tw:flex tw:cursor-pointer tw:items-start tw:gap-2 tw:text-sm"
            >
              <input
                type="radio"
                name="regimen-de-tenencia"
                checked={regimen === r.valor}
                onChange={() => setRegimen(r.valor)}
                className="tw:mt-0.5 tw:size-4 tw:shrink-0 tw:accent-acento"
              />
              <span className="tw:flex tw:flex-col">
                <span>{r.texto}</span>
                <span className="tw:text-xs tw:text-tinta-mid">{r.ayuda}</span>
              </span>
            </label>
          ))}
        </fieldset>

        <div className="tw:grid tw:gap-4 tw:sm:grid-cols-2">
          <Campo
            etiqueta="Titular"
            obligatorio
            ayuda="Quién es el propietario o cedente. Sin él no hay a quién devolverle el bien."
          >
            {(control) => (
              <input
                {...control}
                value={titular}
                onChange={(e) => setTitular(e.target.value)}
                placeholder="Secretaría de Salud"
              />
            )}
          </Campo>

          <Campo
            etiqueta="Documento"
            obligatorio
            ayuda="Convenio, contrato, acta o resolución. Un comodato prorrogado verbalmente no existe para el sistema."
          >
            {(control) => (
              <input
                {...control}
                value={documento}
                onChange={(e) => setDocumento(e.target.value)}
                placeholder="Convenio de comodato SS-2026-04"
              />
            )}
          </Campo>

          <Campo etiqueta="Rige desde" obligatorio>
            {(control) => (
              <CampoFecha
                id={control.id}
                valor={desde}
                onCambiar={setDesde}
                etiqueta="Rige desde"
              />
            )}
          </Campo>

          {/* Aparece sólo cuando corresponde: en propiedad el servidor la rechaza, y un
              campo que sólo sirve para que lo rechacen es una trampa. */}
          {vence && (
            <Campo
              etiqueta="Hasta"
              obligatorio
              ayuda="La del documento. Ninguna misión que vuelva después de esta fecha se va a poder programar."
            >
              {(control) => (
                <CampoFecha
                  id={control.id}
                  valor={hasta}
                  onCambiar={setHasta}
                  etiqueta="Hasta"
                  // Un título que termina antes de empezar no existe, y el servidor no
                  // tiene por qué ser el primero en decirlo.
                  min={desde}
                />
              )}
            </Campo>
          )}
        </div>

        {elegido?.valor === 'Propiedad' && (
          <Nota tono="info" icono={<FileSignature />}>
            La propiedad <b>no lleva fecha de fin</b>: el bien es del Estado y no vence.
            Ponerle una inhabilitaría el vehículo el día que alguien eligió, sin que ninguna
            norma lo mandara.
          </Nota>
        )}

        {elegido !== undefined && !elegido.esBienPropio && (
          <Nota tono="aviso">
            Con este régimen el vehículo <b>no es un bien del Estado</b>, así que no se puede
            descargar del registro de bienes: sale de la flota por{' '}
            <b>retiro con acta de devolución</b>. Declararlo descargado sería un asiento falso
            sobre un bien ajeno.
          </Nota>
        )}

        <Panel titulo="Quién asume cada rubro">
          <div className="tw:flex tw:flex-col tw:gap-3">
            <p className="tw:text-xs tw:text-tinta-mid">
              Lo que cubre el titular <b>no se imputa a nuestro presupuesto</b>: un
              mantenimiento que paga el arrendador y se carga igual es gasto público pagado dos
              veces. <b>«Sin pactar» no es «la institución»</b> — es el rubro que aparece
              cuando llega la factura y empieza la discusión con el contrato en la mano.
            </p>

            {RUBROS.map((r) => (
              <FilaDeRubro
                key={r.campo}
                texto={r.texto}
                valor={rubros[r.campo]}
                onCambiar={(q) => setRubros((antes) => ({ ...antes, [r.campo]: q }))}
              />
            ))}
          </div>
        </Panel>
      </div>
    </Modal>
  );
}

const QUIENES: { valor: QuienAsume; texto: string }[] = [
  { valor: 'Institucion', texto: 'Nosotros' },
  { valor: 'Titular', texto: 'El titular' },
  { valor: 'SinPactar', texto: 'Sin pactar' },
];

function FilaDeRubro({
  texto,
  valor,
  onCambiar,
}: {
  texto: string;
  valor: QuienAsume;
  onCambiar(q: QuienAsume): void;
}): ReactElement {
  return (
    <div className="tw:flex tw:flex-wrap tw:items-center tw:justify-between tw:gap-2">
      <span className="tw:text-sm">{texto}</span>

      <div className="tw:flex tw:gap-3">
        {QUIENES.map((q) => (
          <label
            key={q.valor}
            className="tw:flex tw:cursor-pointer tw:items-center tw:gap-1.5 tw:text-xs"
          >
            <input
              type="radio"
              name={`rubro-${texto}`}
              checked={valor === q.valor}
              onChange={() => onCambiar(q.valor)}
              className="tw:size-3.5 tw:shrink-0 tw:accent-acento"
            />
            <span className={q.valor === 'SinPactar' ? 'tw:text-aviso-fg' : undefined}>
              {q.texto}
            </span>
          </label>
        ))}
      </div>
    </div>
  );
}
