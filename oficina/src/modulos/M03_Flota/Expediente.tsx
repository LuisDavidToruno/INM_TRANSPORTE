import type { ReactElement } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useParams } from 'react-router';
import { CircleAlert, IdCard, TriangleAlert } from 'lucide-react';

import { EnlaceBoton, Nota, Panel, Pastilla, Vacio } from '../../ui';
import type { Tono } from '../../ui';
import { estadoDelVehiculo, padronDeFlota } from '../../api/flota';
import type { VehiculoDelPadron } from '../../api/flota';
import { serieDeTitulos } from '../../api/titulos';
import { prestamos } from '../../api/prestamos';
import { incidentes } from '../../api/incidentes';
import { diaYHora, soloFecha } from '../M06_Autorizacion/formato';

/**
 * `PT-073`, `PT-075` y `PT-076` — El expediente del vehículo.
 *
 * ── Por qué esta pantalla es el centro de `M-03` ────────────────────────────
 * *«Así como Talento Humano cuida de todo lo referente a los empleados, SIGTI cuida de todo lo
 * referente a los vehículos»*. El expediente es **una entidad de primera clase con ciclo de
 * vida completo**, no una fila de un catálogo: acá se junta lo que hasta ahora vivía repartido
 * —estado operativo, títulos de tenencia, préstamos, incidentes, documentación— y se responde
 * la pregunta que ninguna lista contesta: <b>¿qué le ha pasado a esta unidad?</b>
 *
 * ── Por qué `PT-075` y `PT-076` van acá y no aparte ─────────────────────────
 * `PT-075` es la placa y el estado de la lámina; `PT-076`, la ficha técnica que habilita. Son
 * dos bloques de este expediente, y separarlos en pantallas propias obligaría a abrir tres
 * páginas para contestar «¿qué es este vehículo?».
 *
 * ── Lo que todavía NO tiene, y se dice ──────────────────────────────────────
 * **Mantenimiento (`M-11`) y el detalle de siniestros de `M-12`.** No hay historias para
 * ellos, así que no hay datos: el expediente lo declara en vez de mostrar una sección vacía
 * que se lee como «no le pasó nada».
 */
export default function ExpedienteDeVehiculo(): ReactElement {
  const { id = '' } = useParams<{ id: string }>();

  const flota = useQuery({ queryKey: ['padron-flota'], queryFn: padronDeFlota });
  const estado = useQuery({
    queryKey: ['estado-vehiculo', id],
    queryFn: () => estadoDelVehiculo(id),
    enabled: id !== '',
  });
  const titulos = useQuery({
    queryKey: ['serie-titulos', id],
    queryFn: () => serieDeTitulos(id),
    enabled: id !== '',
  });
  const cesiones = useQuery({ queryKey: ['prestamos'], queryFn: prestamos });
  const sucesos = useQuery({ queryKey: ['incidentes'], queryFn: incidentes });

  const vehiculo = flota.data?.find((v) => v.id === id);

  if (flota.isError) {
    return (
      <Nota tono="riesgo" icono={<CircleAlert />}>
        No se pudo cargar la flota. Vuelva a intentar en unos segundos.
      </Nota>
    );
  }

  if (flota.isPending) {
    return <p className="tw:text-sm tw:text-tinta-mid">Cargando el expediente…</p>;
  }

  if (vehiculo === undefined) {
    return (
      <Vacio
        icono={<CircleAlert />}
        titulo="Ese vehículo no está en el padrón"
        descripcion="Puede que el enlace sea de una unidad que ya salió de la flota, o que el identificador esté incompleto."
      />
    );
  }

  const suyos = {
    titulos: titulos.data ?? [],
    prestamos: (cesiones.data ?? []).filter((p) => p.vehiculo === id),
    incidentes: (sucesos.data ?? []).filter((i) => i.vehiculo === id),
  };

  return (
    <div className="tw:flex tw:flex-col tw:gap-5">
      <Cabecera vehiculo={vehiculo} />

      <div className="tw:grid tw:gap-5 tw:lg:grid-cols-2">
        <Identificacion vehiculo={vehiculo} />
        <FichaTecnica vehiculo={vehiculo} />
      </div>

      <Documentacion vehiculo={vehiculo} />

      <Tenencia titulos={suyos.titulos} cargando={titulos.isPending} />

      <Cesiones prestamos={suyos.prestamos} cargando={cesiones.isPending} />

      <Sucesos incidentes={suyos.incidentes} cargando={sucesos.isPending} />

      <HistorialDeEstado
        historial={estado.data?.historial ?? []}
        cargando={estado.isPending}
      />

      {/* Un expediente sin estas dos secciones se lee como «no le pasó nada». Se declara. */}
      <Nota tono="info">
        <b>Faltan dos capítulos, y no es un descuido de esta pantalla.</b> El mantenimiento
        (`M-11`) y el detalle de siniestros de `M-12` más allá de la interrupción en ruta{' '}
        <b>no tienen historias escritas</b> —el inventario de pantallas lo declara en su §7.1—,
        así que no hay datos que mostrar. Un expediente con esas secciones vacías diría que la
        unidad nunca entró a taller, que es distinto de que nadie lo haya registrado.
      </Nota>
    </div>
  );
}

function Cabecera({ vehiculo }: { vehiculo: VehiculoDelPadron }): ReactElement {
  return (
    <header className="tw:flex tw:flex-col tw:gap-2">
      <div className="tw:flex tw:flex-wrap tw:items-center tw:gap-3">
        <h1 className="tw:text-xl tw:font-semibold tw:tracking-tight tw:font-mono">
          {vehiculo.siglas}
        </h1>
        <EstadoVista estado={vehiculo.estado} />
      </div>

      <p className="tw:text-sm tw:text-tinta-mid">
        {vehiculo.ficha.tipoDeVehiculo} ·{' '}
        {vehiculo.custodio ?? <span className="tw:text-riesgo-fg">sin custodio · BD-13</span>}
      </p>
    </header>
  );
}

/**
 * `PT-075` — Placa y estado de la lámina.
 *
 * ── Sin placa metálica es un estado VÁLIDO ──────────────────────────────────
 * Hay desabastecimiento nacional de láminas. Un campo `placa` obligatorio y único rompe el
 * sistema, y un renglón en blanco se lee como dato faltante. Se nombra.
 */
function Identificacion({ vehiculo }: { vehiculo: VehiculoDelPadron }): ReactElement {
  return (
    <Panel titulo="Identificación">
      <dl className="tw:flex tw:flex-col tw:gap-3">
        <Dato termino="Siglas institucionales" valor={vehiculo.siglas} mono />

        <div className="tw:flex tw:flex-col tw:gap-0.5">
          <dt className="tw:text-xs tw:uppercase tw:tracking-wide tw:text-tinta-mid">
            Placa metálica
          </dt>
          <dd className="tw:text-sm">
            {vehiculo.placa !== null ? (
              <span className="tw:font-mono">{vehiculo.placa}</span>
            ) : (
              <div className="tw:flex tw:flex-col tw:gap-1">
                <Pastilla tono="neutro">Sin placa metálica</Pastilla>
                <span className="tw:text-xs tw:text-tinta-mid">
                  <b>Es un estado válido</b>, no un dato faltante: hay desabastecimiento
                  nacional de láminas. La unidad opera igual.
                </span>
              </div>
            )}
          </dd>
        </div>

        {/* Hallazgo frecuente de auditoría: franjas azul–blanco–azul, leyenda y correlativo. */}
        <Nota tono="aviso">
          <b>La identificación del vehículo del Estado no está en el sistema.</b> Las franjas,
          la leyenda y el correlativo son campo verificable con fecha y foto, y{' '}
          <b>hallazgo frecuente de auditoría</b>. Hoy sólo se reconstata al devolver un
          préstamo; el alta con foto es de `PT-124`, del cliente de campo.
        </Nota>
      </dl>
    </Panel>
  );
}

/**
 * `PT-076` — La ficha técnica que habilita.
 *
 * No es una lista de características: **cada campo de acá decide qué categoría de licencia
 * puede conducirla**, y por eso se dice para qué sirve cada uno en vez de dejarlos sueltos.
 */
function FichaTecnica({ vehiculo }: { vehiculo: VehiculoDelPadron }): ReactElement {
  const f = vehiculo.ficha;

  return (
    <Panel titulo="Ficha técnica que habilita">
      <dl className="tw:flex tw:flex-col tw:gap-3">
        <Dato termino="Tipo" valor={f.tipoDeVehiculo} />
        <Dato termino="Clase normativa" valor={f.clase} ayuda="Decide qué familia de categorías aplica." />
        <Dato
          termino="Peso bruto"
          valor={`${f.pesoBrutoKg.toLocaleString('es-HN')} kg`}
          ayuda="Separa B de C1 y de C: los umbrales son 3,500 y 7,500 kg."
        />
        <Dato
          termino="Capacidad"
          valor={`${f.capacidadPasajeros} pasajeros`}
          ayuda="Separa D1 de D, con el corte en 25."
        />

        <div className="tw:flex tw:flex-col tw:gap-0.5">
          <dt className="tw:text-xs tw:uppercase tw:tracking-wide tw:text-tinta-mid">
            Remolque
          </dt>
          <dd className="tw:text-sm">
            {f.llevaRemolque ? (
              <div className="tw:flex tw:flex-col tw:gap-1">
                <Pastilla tono="aviso">Lleva remolque</Pastilla>
                <span className="tw:text-xs tw:text-tinta-mid">
                  <b>Exige categoría con `E`</b> —`BE` o `CE`—. Y ojo:{' '}
                  <b>el remolque no es «articulado»</b>. Un pick-up con plataforma enganchada
                  requiere <code className="tw:font-mono tw:text-xs">BE</code> y no es
                  articulado; confundirlos deja pasar el caso que el bloqueo existe para
                  impedir.
                </span>
              </div>
            ) : (
              <span className="tw:text-tinta-mid">No</span>
            )}
          </dd>
        </div>

        <EnlaceBoton variante="secundario" tamano="sm" href="/motoristas/matriz">
          Ver qué categorías la habilitan
        </EnlaceBoton>
      </dl>
    </Panel>
  );
}

/** Vencimientos. Sólo la matrícula es exigible hoy; las otras dos son regla configurable. */
function Documentacion({ vehiculo }: { vehiculo: VehiculoDelPadron }): ReactElement {
  return (
    <Panel titulo="Documentación y vencimientos">
      <div className="tw:grid tw:gap-4 tw:sm:grid-cols-3">
        <Vencimiento
          termino="Matrícula"
          fecha={vehiculo.venceMatricula}
          nota="Exigible. Su vencimiento impide despachar."
        />
        <Vencimiento
          termino="Póliza de seguro"
          fecha={vehiculo.vencePoliza}
          nota="No es obligatoria por ley vigente: rastreable y alertable, pero el bloqueo es regla configurable."
        />
        <Vencimiento
          termino="Revisión mecánica"
          fecha={vehiculo.venceRevisionMecanica}
          nota="Mismo criterio que la póliza: se alerta, y bloquear es decisión de la institución."
        />
      </div>

      {vehiculo.excepcion !== null && (
        <div className="tw:mt-4">
          <Nota tono="aviso">
            <b>Excepción vigente: {vehiculo.excepcion.tipo}.</b> Desde el{' '}
            {soloFecha(vehiculo.excepcion.desde)}
            {vehiculo.excepcion.hasta !== null
              ? ` hasta el ${soloFecha(vehiculo.excepcion.hasta)}`
              : ', sin fecha de fin'}
            .
          </Nota>
        </div>
      )}
    </Panel>
  );
}

/**
 * Un vencimiento.
 *
 * **Nulo no es «vencido» ni «al día»**: es que nadie lo cargó. Se dice, porque las tres cosas
 * piden acciones distintas.
 */
function Vencimiento({
  termino,
  fecha,
  nota,
}: {
  termino: string;
  fecha: string | null;
  nota: string;
}): ReactElement {
  const hoy = new Date().toISOString().slice(0, 10);
  const vencido = fecha !== null && fecha < hoy;

  return (
    <div className="tw:flex tw:flex-col tw:gap-1">
      <span className="tw:text-xs tw:uppercase tw:tracking-wide tw:text-tinta-mid">
        {termino}
      </span>

      {fecha === null ? (
        <Pastilla tono="neutro">Sin registrar</Pastilla>
      ) : vencido ? (
        <Pastilla tono="riesgo">Vencida el {soloFecha(fecha)}</Pastilla>
      ) : (
        <span className="tw:tabular-nums tw:text-sm">{soloFecha(fecha)}</span>
      )}

      <span className="tw:text-xs tw:text-tinta-mid">{nota}</span>
    </div>
  );
}

/** La serie de títulos — `RN-62`. De ella sale si el bien es del Estado. */
function Tenencia({
  titulos,
  cargando,
}: {
  titulos: Awaited<ReturnType<typeof serieDeTitulos>>;
  cargando: boolean;
}): ReactElement {
  if (cargando) {
    return <Panel titulo="Régimen de tenencia"><Cargando /></Panel>;
  }

  if (titulos.length === 0) {
    return (
      <Panel titulo="Régimen de tenencia">
        <Nota tono="aviso">
          <b>No consta bajo qué régimen tenemos esta unidad.</b> Mientras no haya título,{' '}
          <code className="tw:font-mono tw:text-xs">RN-62</code> no se evalúa sobre ella: la
          ventana de sus misiones no se contrasta contra ninguna vigencia, y al darla de baja el
          sistema advierte en vez de juzgar si el terminal corresponde.{' '}
          <a href="/titulos" className="loki-foco tw:underline tw:underline-offset-2">
            Registrar el título
          </a>
          .
        </Nota>
      </Panel>
    );
  }

  return (
    <Panel titulo={`Régimen de tenencia · ${titulos.length}`}>
      <div className="tw:flex tw:flex-col tw:gap-3">
        {titulos.map((t) => (
          <div
            key={t.id}
            className={`tw:flex tw:flex-col tw:gap-1 tw:border-l-2 tw:pl-3 ${
              t.vigente ? 'tw:border-ok-fg' : 'tw:border-linea'
            }`}
          >
            <div className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-3 tw:text-sm">
              <span className="tw:font-medium">{t.regimen}</span>
              <span className="tw:text-tinta-mid">{t.titular}</span>
              {t.vigente && (
                <Pastilla tono={t.esBienPropio ? 'ok' : 'info'}>
                  {t.esBienPropio ? 'Bien del Estado' : 'Bien ajeno'}
                </Pastilla>
              )}
            </div>

            <span className="tw:text-xs tw:text-tinta-mid">
              {t.documento} · desde el {soloFecha(t.desde)}
              {t.hasta === null ? ' · sin vencimiento' : ` hasta el ${soloFecha(t.hasta)}`}
            </span>

            {t.rubrosSinPactar.length > 0 && (
              <span className="tw:text-xs tw:text-aviso-fg">
                sin pactar: {t.rubrosSinPactar.join(', ')}
              </span>
            )}
          </div>
        ))}
      </div>
    </Panel>
  );
}

/** Los préstamos — `RN-63`. Contestan quién respondía por la unidad en una fecha. */
function Cesiones({
  prestamos: lista,
  cargando,
}: {
  prestamos: Awaited<ReturnType<typeof prestamos>>;
  cargando: boolean;
}): ReactElement {
  if (cargando) return <Panel titulo="Cesiones de tenencia"><Cargando /></Panel>;

  if (lista.length === 0) {
    return (
      <Panel titulo="Cesiones de tenencia">
        <p className="tw:text-sm tw:text-tinta-mid">
          Esta unidad nunca se prestó a otra dependencia ni institución.
        </p>
      </Panel>
    );
  }

  return (
    <Panel titulo={`Cesiones de tenencia · ${lista.length}`}>
      <div className="tw:flex tw:flex-col tw:gap-3">
        {lista.map((p) => (
          <div
            key={p.id}
            className={`tw:flex tw:flex-col tw:gap-1 tw:border-l-2 tw:pl-3 ${
              p.estaVencido ? 'tw:border-riesgo-fg' : 'tw:border-linea'
            }`}
          >
            <div className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-3 tw:text-sm">
              <span className="tw:font-medium">{p.receptor.persona}</span>
              <span className="tw:text-tinta-mid">{p.receptor.institucion}</span>
              {p.estaVencido && <Pastilla tono="riesgo">{p.diasDeMora} días de mora</Pastilla>}
              {!p.estaVigente && <Pastilla tono="ok">Devuelto</Pastilla>}
            </div>
            <span className="tw:text-xs tw:text-tinta-mid">
              del {soloFecha(p.desde)} al {soloFecha(p.devolucionComprometida)} comprometido
            </span>
          </div>
        ))}
      </div>
    </Panel>
  );
}

/** Incidentes — `M-12`, sólo lo que hay: la interrupción en ruta y su desenlace. */
function Sucesos({
  incidentes: lista,
  cargando,
}: {
  incidentes: Awaited<ReturnType<typeof incidentes>>;
  cargando: boolean;
}): ReactElement {
  if (cargando) return <Panel titulo="Incidentes"><Cargando /></Panel>;

  if (lista.length === 0) {
    return (
      <Panel titulo="Incidentes">
        <p className="tw:text-sm tw:text-tinta-mid">
          Sin incidentes registrados para esta unidad.
        </p>
      </Panel>
    );
  }

  return (
    <Panel titulo={`Incidentes · ${lista.length}`}>
      <div className="tw:flex tw:flex-col tw:gap-3">
        {lista.map((i) => (
          <div
            key={i.id}
            className={`tw:flex tw:flex-col tw:gap-1 tw:border-l-2 tw:pl-3 ${
              i.esInterrupcionSinDesenlace ? 'tw:border-riesgo-fg' : 'tw:border-linea'
            }`}
          >
            <div className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-3 tw:text-sm">
              <TriangleAlert className="tw:size-3.5 tw:text-tinta-mid" aria-hidden />
              <span className="tw:font-medium">{i.tipo}</span>
              <span className="tw:text-tinta-mid">{i.causa}</span>
              {i.esInterrupcionSinDesenlace && (
                <Pastilla tono="riesgo">Interrupción sin desenlace</Pastilla>
              )}
            </div>
            <span className="tw:text-xs tw:text-tinta-mid">
              {soloFecha(i.fechaDelHecho)}
              {i.diasEntreElHechoYLaCaptura > 0 &&
                ` · registrado ${i.diasEntreElHechoYLaCaptura} días después`}
            </span>
          </div>
        ))}
      </div>
    </Panel>
  );
}

/**
 * El historial de estado operativo — §10.2.
 *
 * **Conserva el camino entero**, no sólo dónde está hoy: es lo que contesta «¿por qué no
 * estuvo disponible en marzo?».
 */
function HistorialDeEstado({
  historial,
  cargando,
}: {
  historial: Awaited<ReturnType<typeof estadoDelVehiculo>>['historial'];
  cargando: boolean;
}): ReactElement {
  if (cargando) return <Panel titulo="Historial de estado operativo"><Cargando /></Panel>;

  if (historial.length === 0) {
    return (
      <Panel titulo="Historial de estado operativo">
        <Nota tono="aviso">
          <b>Nunca se declaró un estado.</b> Eso no es «disponible»: §10.2 cuenta el «alta
          reciente sin habilitar» entre las causas de{' '}
          <code className="tw:font-mono tw:text-xs">NO_DISPONIBLE</code>.
        </Nota>
      </Panel>
    );
  }

  // Lo más reciente arriba: la pregunta habitual es qué pasó último.
  const orden = [...historial].reverse();

  return (
    <Panel titulo={`Historial de estado operativo · ${historial.length}`}>
      <ul className="tw:flex tw:flex-col tw:gap-3">
        {orden.map((c, i) => (
          <li key={`${c.momento}-${i}`} className="tw:flex tw:flex-col tw:gap-0.5 tw:border-l-2 tw:border-linea tw:pl-3">
            <div className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-3 tw:text-sm">
              <EstadoVista estado={c.estado} />
              <span className="tw:text-tinta-mid">{diaYHora(c.momento)}</span>
              {/* Automático o declarado por una persona: cambia quién responde por el asiento. */}
              <span className="tw:text-xs tw:text-tinta-mid">
                {c.automatico ? 'lo fijó el sistema' : c.ejecuta}
              </span>
            </div>
            {c.motivo !== null && (
              <span className="tw:text-xs tw:text-tinta-mid">{c.motivo}</span>
            )}
          </li>
        ))}
      </ul>
    </Panel>
  );
}

function Dato({
  termino,
  valor,
  ayuda,
  mono = false,
}: {
  termino: string;
  valor: string;
  ayuda?: string;
  mono?: boolean;
}): ReactElement {
  return (
    <div className="tw:flex tw:flex-col tw:gap-0.5">
      <dt className="tw:text-xs tw:uppercase tw:tracking-wide tw:text-tinta-mid">{termino}</dt>
      <dd className={`tw:text-sm ${mono ? 'tw:font-mono' : ''}`}>{valor}</dd>
      {ayuda !== undefined && <span className="tw:text-xs tw:text-tinta-mid">{ayuda}</span>}
    </div>
  );
}

function Cargando(): ReactElement {
  return <p className="tw:text-sm tw:text-tinta-mid">Cargando…</p>;
}

const TEXTO_DE_ESTADO: Record<string, string> = {
  Disponible: 'Disponible',
  Asignado: 'Asignado a una misión',
  EnMision: 'En misión',
  EnTaller: 'En taller',
  NoDisponible: 'No disponible',
  Prestado: 'Prestado',
  DadoDeBaja: 'Dado de baja',
  RetiradoDeFlota: 'Retirado de flota',
};

const TONO_DE_ESTADO: Record<string, Tono> = {
  Disponible: 'ok',
  Asignado: 'info',
  EnMision: 'info',
  EnTaller: 'aviso',
  NoDisponible: 'aviso',
  Prestado: 'aviso',
  DadoDeBaja: 'riesgo',
  RetiradoDeFlota: 'riesgo',
};

/** Se decide por identificador, nunca por el texto: «No disponible» contiene «disponible». */
function EstadoVista({ estado }: { estado: string | null }): ReactElement {
  if (estado === null) return <Pastilla tono="neutro">Sin declarar</Pastilla>;

  return (
    <Pastilla tono={TONO_DE_ESTADO[estado] ?? 'neutro'}>
      {TEXTO_DE_ESTADO[estado] ?? estado}
    </Pastilla>
  );
}

/** Sin uso fuera de este archivo; existe para dejar explícito el ícono del vacío. */
export type IconoDeExpediente = typeof IdCard;
