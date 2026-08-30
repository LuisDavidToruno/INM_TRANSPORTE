import type { ReactElement, ReactNode } from 'react';
import { useState } from 'react';
import { useNavigate, useParams } from 'react-router';
import { useMutation, useQueries, useQuery, useQueryClient } from '@tanstack/react-query';
import { CircleCheck } from 'lucide-react';

import { Boton, Campo, LineaDeCarriles, Nota, Panel, Pastilla, avisar } from '../../ui';
import PanelDeVales from '../M09_Combustible/PanelDeVales';
import PanelDeAbastecimientos from '../M09_Combustible/PanelDeAbastecimientos';
import type { CarrilDeLinea } from '../../ui';
import {
  MOTIVOS_DE_REASIGNACION,
  conductores,
  evaluarAsignacion,
  flota,
  ocupacionDeFlota,
  programar,
  reasignar,
} from '../../api/flota';
import type {
  ConductorDisponible,
  OcupacionDeFlota,
  ResultadoDeAsignacion,
  VehiculoDeFlota,
} from '../../api/flota';
import { BloqueoDuro, expediente as traerExpediente } from '../../api/misiones';
import { recursoVigente } from '../../dominio/mision';
import type { Expediente, MotivoDeReasignacion } from '../../dominio/mision';
import { soloFecha, soloHora } from '../M06_Autorizacion/formato';
import ConflictoDeAgenda from './ConflictoDeAgenda';
import RechazoPorLicencia from './RechazoPorLicencia';
import { usarQuienEjecuta } from '../../app/puesto';

/**
 * `PT-026` / `PT-027` — Asignación de vehículo y declaración de quien conduce, con
 * `PT-028` incrustado como resultado.
 *
 * ── La ventana no se elige acá ───────────────────────────────────────────────
 * Sale del expediente aprobado. Quien programa no la declara: si pudiera, la
 * acortaría hasta que la licencia del motorista disponible alcanzara, y `BD-02`
 * dejaría de proteger justo a quien autorizó. El contrato de la API ni siquiera
 * admite mandarla.
 *
 * ── La evaluación la hace el servidor ────────────────────────────────────────
 * Esta pantalla **muestra** el resultado; no lo calcula. La regla vive en un solo
 * lugar, que es lo único que garantiza que lo que se ve al elegir sea lo mismo que
 * bloquea al guardar.
 *
 * ── El cronograma va ARRIBA de la lista, no al lado ──────────────────────────
 * Porque la pregunta llega antes: *«¿cuál está libre?»* precede a *«¿este habilita?»*.
 * Se construyó al revés —sólo la lista— y el dictamen de elementos visuales lo marcó
 * como el error de mayor daño del inventario: con una lista, la única forma de saber si
 * el pick-up está libre el jueves es abrir las misiones una por una.
 *
 * ── La misma pantalla sirve para REASIGNAR ──────────────────────────────────
 * `T-10`. Es la misma decisión —qué vehículo y quién conduce— con las mismas reglas
 * evaluándose, así que duplicar la pantalla habría duplicado la selección, el cronograma
 * y la lectura del resultado. Lo único que cambia es que hay un recurso saliente, y por eso
 * aparece el motivo: la ficha de `T-10` lo exige tipificado, y es lo que distingue un
 * vehículo que se avería seguido de uno que se cambió por consolidación.
 *
 * **El cronograma no bloquea nada.** Un vehículo ocupado se puede elegir igual: quien
 * programa puede saber que esa misión se va a anular, o estar reprogramando a propósito.
 * Lo que bloquea es `BD-02` y `BD-03`, en el servidor.
 */
export default function Asignacion(): ReactElement {
  const { id = '' } = useParams();
  const navegar = useNavigate();
  const cliente = useQueryClient();

  const [idVehiculo, setVehiculo] = useState('');
  const [idConductor, setConductor] = useState('');
  const [nocturna, setNocturna] = useState(false);
  const [motivo, setMotivo] = useState<MotivoDeReasignacion | ''>('');
  const [comentario, setComentario] = useState('');

  const [expedienteQ, flotaQ, conductoresQ] = useQueries({
    queries: [
      { queryKey: ['expediente', id], queryFn: () => traerExpediente(id) },
      { queryKey: ['flota'], queryFn: flota },
      { queryKey: ['conductores'], queryFn: conductores },
    ],
  });

  const hayEleccion = Boolean(idVehiculo && idConductor);

  const { data: resultado, isFetching } = useQuery({
    queryKey: ['evaluacion', id, idVehiculo, idConductor, nocturna],
    queryFn: () => evaluarAsignacion(id, idVehiculo, idConductor, nocturna),
    enabled: hayEleccion,
  });

  // `Programada` ⇒ es una reasignación, no una programación. Se decide por el estado y no
  // por una ruta aparte: la misma pantalla, la misma decisión, y una sola implementación
  // de la selección, el cronograma y la lectura del resultado.
  const reasignando = expedienteQ.data?.estado === 'Programada';

  // Quien ejecuta sale del puesto vigente, no de una constante: si fuera
  // siempre la misma persona, la segregación de `I-01` a `I-19` compararía
  // al actor contra sí mismo y no bloquearía nunca.
  const quienEjecuta = usarQuienEjecuta();

  const programacion = useMutation({
    mutationFn: () =>
      reasignando
        ? reasignar(id, quienEjecuta, idVehiculo, idConductor,
                    motivo as MotivoDeReasignacion, comentario)
        : programar(id, quienEjecuta, idVehiculo, idConductor),
    onSuccess: async () => {
      avisar.exito(
        reasignando
          ? 'Recurso reasignado. El folio de la misión no cambió: es el mismo expediente.'
          : 'Misión programada. El vehículo y el motorista quedaron reservados.',
      );
      await cliente.invalidateQueries({ queryKey: ['cola-programacion'] });
      await cliente.invalidateQueries({ queryKey: ['cola-programadas'] });
      navegar('/programacion');
    },
    onError: (e) => {
      // El servidor vuelve a evaluar al guardar. Si el bloqueo aparece acá y no en la
      // vista previa, algo cambió entre medio — y decirlo así es más útil que un
      // «no se pudo».
      if (e instanceof BloqueoDuro) {
        avisar.error(`${e.precondicion} — ${e.message}`);
        return;
      }
      avisar.error(
        reasignando
          ? 'No se pudo reasignar. La misión quedó con el recurso que ya tenía.'
          : 'No se pudo programar. El expediente quedó como estaba.',
      );
    },
  });

  // Consulta aparte de la flota: si la ocupación falla, la pantalla tiene que dejar
  // asignar igual. Es información para decidir, no una precondición.
  const ocupacion = useQuery({
    queryKey: ['ocupacion', expedienteQ.data?.salidaPrevista, expedienteQ.data?.retornoPrevisto],
    queryFn: () =>
      ocupacionDeFlota(
        soloDia(expedienteQ.data!.salidaPrevista),
        soloDia(expedienteQ.data!.retornoPrevisto),
      ),
    enabled: Boolean(expedienteQ.data),
  });

  const expediente = expedienteQ.data;
  const vehiculos = flotaQ.data;
  const personas = conductoresQ.data;

  if (!expediente || !vehiculos || !personas) return <Cargando />;

  const vehiculo = vehiculos.find((v) => v.id === idVehiculo);
  const conductor = personas.find((c) => c.id === idConductor);

  return (
    <div className="tw:flex tw:flex-col tw:gap-6">
      <Cabecera expediente={expediente} reasignando={reasignando} vehiculos={vehiculos} />

      {/* El combustible sólo cabe desde `PROGRAMADA`: antes no hay vehículo ni motorista
          contra los que `RN-32` pueda evaluar al receptor (`INV-11`, aprobar no es
          programar). Mientras la misión no esté programada, ni siquiera se ofrece. */}
      {reasignando && (
        <PanelDeVales
          misionId={id}
          estadoDeLaMision={expediente.estado}
          dependencia={expediente.dependencia}
          // La reserva sale del ÚLTIMO asiento que reservó, no del primero: `T-10`
          // reasigna sin soltar la misión, y quedarse con el de `T-08` precargaría al
          // motorista que ya fue sustituido.
          motoristaDeLaOrden={
            [...expediente.diario].reverse().find((t) => t.conductorTomado)
              ?.conductorTomado ?? undefined
          }
        />
      )}

      {reasignando && (
        <PanelDeAbastecimientos
          misionId={id}
          vehiculoId={
            [...expediente.diario].reverse().find((t) => t.vehiculoTomado)?.vehiculoTomado ??
            undefined
          }
        />
      )}

      <Panel titulo="Ocupación de la flota en la ventana solicitada">
        <Cronograma
          ocupacion={ocupacion.data}
          fallo={ocupacion.isError}
          elegido={idVehiculo}
          desde={expediente.salidaPrevista}
          hasta={expediente.retornoPrevisto}
        />
      </Panel>

      <div className="tw:grid tw:gap-5 tw:lg:grid-cols-2">
        <Panel titulo="Vehículo">
          <fieldset className="tw:flex tw:flex-col tw:gap-2.5">
            <legend className="tw:sr-only">Vehículo de la misión</legend>
            {vehiculos.map((v) => (
              <Eleccion
                key={v.id}
                grupo="vehiculo"
                idDetalle={`det-veh-${v.id}`}
                elegida={v.id === idVehiculo}
                onElegir={() => setVehiculo(v.id)}
                titulo={v.siglas}
                detalle={`${v.ficha.tipoDeVehiculo} · ${v.ficha.pesoBrutoKg.toLocaleString('es-HN')} kg${v.ficha.llevaRemolque ? ' · con remolque' : ''}`}
                pie={
                  v.placa ? (
                    <span className="tw:font-mono tw:text-xs tw:tabular-nums">{v.placa}</span>
                  ) : (
                    <Pastilla tono="neutro">Sin placa metálica</Pastilla>
                  )
                }
              />
            ))}
          </fieldset>
        </Panel>

        <Panel titulo="Quien conduce">
          <fieldset className="tw:flex tw:flex-col tw:gap-2.5">
            <legend className="tw:sr-only">Quien conduce la misión</legend>
            {personas.map((c) => (
              <Eleccion
                key={c.id}
                grupo="conductor"
                idDetalle={`det-con-${c.id}`}
                elegida={c.id === idConductor}
                onElegir={() => setConductor(c.id)}
                titulo={c.nombre}
                detalle={`Licencia ${c.licencia.numero} · categoría ${c.licencia.categoria} · vence ${soloFecha(c.licencia.vencimiento)}`}
                pie={
                  c.esDelPadron ? undefined : (
                    // RN-57 verifica sobre quien EFECTIVAMENTE conduce, esté o no en el
                    // padrón: el funcionario con vehículo asignado no se exceptúa.
                    <Pastilla tono="info">Conductor declarado, fuera del padrón</Pastilla>
                  )
                }
              />
            ))}
          </fieldset>
        </Panel>
      </div>

      {reasignando && (
        <Panel titulo="Por qué se cambia el recurso">
          <MotivoDelCambio
            motivo={motivo}
            comentario={comentario}
            onMotivo={setMotivo}
            onComentario={setComentario}
          />
        </Panel>
      )}

      <Panel titulo="Condiciones de la misión">
        <label className="tw:flex tw:cursor-pointer tw:items-start tw:gap-2 tw:text-sm">
          <input
            type="checkbox"
            aria-label="La misión declara conducción nocturna, entre las 19:00 y las 23:00"
            checked={nocturna}
            onChange={(e) => setNocturna(e.target.checked)}
            className="tw:mt-0.5 tw:size-4 tw:shrink-0 tw:accent-acento"
          />
          <span>
            La misión declara conducción entre las 19:00 y las 23:00.
            <span className="tw:block tw:text-xs tw:text-tinta-mid">
              Se pregunta porque una restricción médica solo bloquea si la misión la contradice.
            </span>
          </span>
        </label>
      </Panel>

      {!hayEleccion ? (
        <Nota tono="info">
          Elija vehículo y quien conduce. La habilitación se verifica al elegir, no al guardar.
        </Nota>
      ) : isFetching || !resultado ? (
        <Nota tono="info">Verificando la habilitación…</Nota>
      ) : resultado.conflicto ? (
        // El conflicto de agenda va ANTES que el rechazo por licencia porque es otro
        // problema con otras salidas: uno se resuelve cambiando de par, el otro hablando
        // con la dependencia que tiene el recurso. Mostrar «la licencia no habilita»
        // cuando lo que pasa es que el vehículo está tomado manda a buscar donde no es.
        <ConflictoDeAgenda
          conflicto={resultado.conflicto}
          vehiculos={vehiculos}
          habilitan={resultado.vehiculosQueHabilita}
          onElegirVehiculo={setVehiculo}
        />
      ) : resultado.habilita ? (
        <Habilitada
          resultado={resultado}
          vehiculo={vehiculo!}
          conductor={conductor!}
          enviando={programacion.isPending}
          // Sin motivo no se reasigna, y el botón lo dice antes de que lo aprieten: el
          // servidor bloquea igual, pero descubrirlo ahí obliga a rehacer la elección.
          faltaMotivo={reasignando && !motivo}
          etiqueta={reasignando ? 'Reasignar el recurso' : 'Programar la misión'}
          onProgramar={() => programacion.mutate()}
        />
      ) : (
        <RechazoPorLicencia
          resultado={resultado}
          vehiculo={vehiculo!}
          conductor={conductor!}
          vehiculos={vehiculos}
          conductores={personas}
          onElegirConductor={setConductor}
          onElegirVehiculo={setVehiculo}
        />
      )}
    </div>
  );
}

function Cabecera({
  expediente,
  reasignando,
  vehiculos,
}: {
  expediente: Expediente;
  reasignando: boolean;
  vehiculos: VehiculoDeFlota[];
}): ReactElement {
  // Cuál tiene HOY. Sale de la última transición que reservó, no de la primera: una misión
  // ya reasignada tiene varias, y la primera es justamente el vehículo que se cambió.
  const vigente = recursoVigente(expediente.diario);
  const actual = vigente && vehiculos.find((v) => v.id === vigente.vehiculoTomado);

  return (
    <header className="tw:flex tw:flex-col tw:gap-2">
      <div className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-4 tw:gap-y-1">
        <h1 className="tw:font-mono tw:text-xl tw:font-semibold tw:tabular-nums tw:tracking-tight">
          {expediente.folio}
        </h1>
        <Pastilla tono="info">{reasignando ? 'Programada' : 'Aprobada'}</Pastilla>
      </div>

      {reasignando && (
        <p className="tw:text-sm">
          {actual ? (
            <>
              Tiene asignado <b>{actual.siglas}</b>. Elija el recurso que lo reemplaza —{' '}
              <b>el folio no cambia</b>, es el mismo expediente.
            </>
          ) : (
            // No saber cuál tiene y no tener ninguno son cosas distintas, y callar aquí
            // dejaría a quien reasigna creyendo lo segundo.
            <>
              No se pudo determinar qué vehículo tiene asignado. Puede reasignar igual, pero{' '}
              <b>verifique contra el cronograma</b> cuál está ocupado por este folio.
            </>
          )}
        </p>
      )}
      <p className="tw:text-sm tw:text-tinta-mid">
        {expediente.objetoDelTraslado} · destino {expediente.destino}
      </p>
      {/* La ventana se muestra, no se edita: es lo que declara quien pide. */}
      <p className="tw:text-sm">
        Ventana solicitada: <b>{soloFecha(expediente.salidaPrevista)}</b> a las{' '}
        <b>{soloHora(expediente.horaDeSalida)}</b>, retorno{' '}
        <b>{soloFecha(expediente.retornoPrevisto)}</b> a las{' '}
        <b>{soloHora(expediente.horaDeRetorno)}</b>. La licencia tiene que cubrirla completa,
        holgura incluida.
      </p>
    </header>
  );
}

/**
 * La evidencia se muestra <b>también cuando habilita</b>. «Guardar solo "verificado"
 * no defiende a nadie» (`BD-02`): quien programa tiene que poder ver contra qué se
 * resolvió, y esa constancia es la que `PT-031` imprime.
 */
function Habilitada({
  resultado,
  vehiculo,
  conductor,
  enviando,
  faltaMotivo,
  etiqueta,
  onProgramar,
}: {
  resultado: ResultadoDeAsignacion;
  vehiculo: VehiculoDeFlota;
  conductor: ConductorDisponible;
  enviando: boolean;
  /** Reasignar sin motivo tipificado no se puede — `T-10`. */
  faltaMotivo: boolean;
  etiqueta: string;
  onProgramar(): void;
}): ReactElement {
  // `RN-11`: «La advertencia no se puede cerrar sin acuse: queda registrado quién la vio,
  // cuándo y con qué justificación decidió continuar.» El botón queda inerte hasta eso.
  const exigeAcuse = resultado.efectoDeLaRestriccion === 'Advertencia';
  const [acusada, setAcusada] = useState(false);

  return (
    <Panel titulo="Verificación de habilitación">
      <div className="tw:flex tw:flex-col tw:gap-4">
        <Nota tono="ok" icono={<CircleCheck />}>
          <p className="tw:font-medium">
            La licencia categoría {resultado.categoria} habilita {vehiculo.siglas} ·{' '}
            {vehiculo.ficha.tipoDeVehiculo}, {vehiculo.ficha.pesoBrutoKg.toLocaleString('es-HN')} kg.
          </p>
        </Nota>

        {resultado.advertenciasDeDocumentacion.length > 0 && (
          <Nota tono="aviso">
            Documentación con reparos que no bloquean:{' '}
            {resultado.advertenciasDeDocumentacion.join(', ')}.
          </Nota>
        )}

        {resultado.efectoDeLaRestriccion === 'Advertencia' && (
          <Nota tono="aviso">
            <div className="tw:flex tw:flex-col tw:gap-2">
              <p className="tw:font-medium">
                {conductor.nombre} tiene la restricción «{resultado.restriccionEnConflicto}».
              </p>
              {/* `BD-12`: el catálogo no la tipifica como incompatibilizante, así que no
                  bloquea. Pero `RN-11` exige que la advertencia no se cierre sin acuse —
                  y mientras el insumo #42 siga abierto, casi toda restricción real va a
                  llegar por acá, sin clasificar. */}
              <p className="tw:text-sm">
                El catálogo no la tipifica como incompatibilizante para esta misión, así que no
                bloquea la programación. <b>Queda registrado que usted la vio y decidió continuar</b>,
                y esa constancia va en el expediente y en la liquidación.
              </p>
              <p className="tw:text-xs tw:text-tinta-mid">
                El catálogo oficial de restricciones de la DNVT todavía no está cargado —
                insumo #42. Hasta que lo esté, las restricciones sin clasificar llegan como
                ésta, y advertir es lo correcto: callarlas sería peor.
              </p>
            </div>
          </Nota>
        )}

        <dl className="tw:grid tw:gap-x-8 tw:gap-y-3 tw:text-sm tw:sm:grid-cols-2">
          <Insumo termino="Quien conduce" valor={conductor.nombre} />
          <Insumo termino="Licencia" valor={`${resultado.numeroDeLicencia} · ${resultado.categoria}`} />
          <Insumo termino="Vence" valor={soloFecha(resultado.venceLicencia)} />
          <Insumo termino="Rango evaluado hasta" valor={soloFecha(resultado.finDeRangoEvaluado)} />
          <Insumo termino="Versión de la matriz" valor={resultado.versionDeMatriz} mono />
          <Insumo
            termino="Atributos usados"
            valor={`${vehiculo.ficha.pesoBrutoKg.toLocaleString('es-HN')} kg · ${vehiculo.ficha.capacidadPasajeros} pasajeros · ${vehiculo.ficha.llevaRemolque ? 'con remolque' : 'sin remolque'}`}
          />
        </dl>

        {exigeAcuse && (
          <label className="tw:flex tw:cursor-pointer tw:items-start tw:gap-2.5 tw:text-sm">
            <input
              type="checkbox"
              checked={acusada}
              onChange={(e) => setAcusada(e.target.checked)}
              className="tw:mt-0.5"
            />
            <span>
              He visto la restricción de {conductor.nombre} y decido continuar. Entiendo que
              queda registrado a mi nombre.
            </span>
          </label>
        )}

        <Boton
          variante="primario"
          cargando={enviando}
          disabled={enviando || (exigeAcuse && !acusada) || faltaMotivo}
          onClick={onProgramar}
        >
          {etiqueta}
        </Boton>
      </div>
    </Panel>
  );
}

/**
 * El motivo del cambio de recurso — `T-10`.
 *
 * ── Por qué es lista y no texto libre ────────────────────────────────────────
 * Porque la tipificación <b>es</b> el indicador de fiabilidad de la flota: distingue el
 * vehículo que entra a taller tres veces al mes del que se cambió por consolidación. Un
 * campo libre produce «se dañó», «falla mecánica» y «taller» para el mismo hecho, y ningún
 * reporte los suma.
 *
 * ── Y por qué no es el catálogo de la anulación ──────────────────────────────
 * Porque miden cosas distintas. El de anulación mide <b>déficit de flota</b> —la
 * movilización no se hizo—; éste mide que el recurso comprometido dejó de servir. Mezclarlos
 * haría que el reporte de déficit contara averías.
 */
function MotivoDelCambio({
  motivo,
  comentario,
  onMotivo,
  onComentario,
}: {
  motivo: MotivoDeReasignacion | '';
  comentario: string;
  onMotivo(v: MotivoDeReasignacion): void;
  onComentario(v: string): void;
}): ReactElement {
  return (
    <div className="tw:flex tw:flex-col tw:gap-4">
      <fieldset className="tw:flex tw:flex-col tw:gap-2">
        <legend className="tw:mb-1 tw:text-sm tw:font-medium">Motivo</legend>
        {MOTIVOS_DE_REASIGNACION.map((m) => (
          <label
            key={m.valor}
            className="tw:flex tw:cursor-pointer tw:items-start tw:gap-2 tw:text-sm"
          >
            <input
              type="radio"
              name="motivo-reasignacion"
              checked={motivo === m.valor}
              onChange={() => onMotivo(m.valor)}
              className="tw:mt-0.5 tw:size-4 tw:shrink-0 tw:accent-acento"
            />
            <span>{m.texto}</span>
          </label>
        ))}
      </fieldset>

      <Campo
        etiqueta="Comentario"
        ayuda="Complementa al motivo, no lo sustituye: «falla de frenos detectada en la revisión previa» dice qué pasó; el motivo dice qué se cuenta."
      >
        {(props) => (
          <textarea
            {...props}
            rows={2}
            value={comentario}
            onChange={(e) => onComentario(e.target.value)}
          />
        )}
      </Campo>
    </div>
  );
}

function Insumo({
  termino,
  valor,
  mono = false,
}: {
  termino: string;
  valor: string;
  mono?: boolean;
}): ReactElement {
  return (
    <div className="tw:flex tw:flex-col tw:gap-0.5">
      <dt className="tw:text-xs tw:text-tinta-mid">{termino}</dt>
      <dd className={mono ? 'tw:font-mono tw:text-xs' : ''}>{valor}</dd>
    </div>
  );
}

/**
 * Un radio de verdad, no un `div` con `onClick`. Con `input[type=radio]` el grupo se
 * recorre con las flechas, se anuncia como grupo y el foco se ve sin escribir nada.
 */
function Eleccion({
  grupo,
  elegida,
  onElegir,
  titulo,
  detalle,
  pie,
  idDetalle,
}: {
  grupo: string;
  elegida: boolean;
  onElegir(): void;
  titulo: string;
  detalle: string;
  pie?: ReactNode;
  idDetalle: string;
}): ReactElement {
  return (
    <label
      className={`tw:flex tw:cursor-pointer tw:gap-3 tw:rounded tw:border tw:px-3 tw:py-2.5 tw:transition-colors ${
        elegida
          ? 'tw:border-acento tw:bg-subtle'
          : 'tw:border-linea tw:hover:border-linea-activa'
      }`}
    >
      <input
        type="radio"
        name={grupo}
        checked={elegida}
        onChange={onElegir}
        // El nombre accesible es la identidad, no el bloque entero: sin esto el lector
        // lee «INS-P-014Pick-up doble cabina · 2,800 kg» de corrido.
        aria-label={titulo}
        aria-describedby={idDetalle}
        className="tw:mt-1 tw:size-4 tw:shrink-0 tw:accent-acento"
      />
      <span className="tw:flex tw:min-w-0 tw:flex-col tw:gap-1">
        <span className="tw:text-sm tw:font-medium">{titulo}</span>
        <span id={idDetalle} className="tw:text-xs tw:text-tinta-mid">
          {detalle}
        </span>
        {pie}
      </span>
    </label>
  );
}

function Cargando(): ReactElement {
  return (
    <div className="tw:flex tw:flex-col tw:gap-6" aria-busy="true" aria-live="polite">
      <div className="tw:h-7 tw:w-48 tw:animate-pulse tw:rounded tw:bg-subtle" />
      <div className="tw:grid tw:gap-5 tw:lg:grid-cols-2">
        <div className="tw:h-64 tw:animate-pulse tw:rounded tw:bg-subtle" />
        <div className="tw:h-64 tw:animate-pulse tw:rounded tw:bg-subtle" />
      </div>
      <span className="tw:sr-only">Cargando el expediente y la flota…</span>
    </div>
  );
}

/** `YYYY-MM-DD` a partir de lo que viaja, que puede traer hora. */
const soloDia = (fecha: string): string => fecha.slice(0, 10);

/**
 * Una fecha del servidor a `Date` **local a medianoche**.
 *
 * `new Date('2026-03-20')` la interpreta como UTC y en Honduras —UTC−6— la corre al 19
 * por la tarde. La barra empezaría un día antes que la misión, que es exactamente el
 * error que el cronograma existe para no cometer.
 */
function comoDiaLocal(fecha: string): Date {
  const [a, m, d] = soloDia(fecha).split('-').map(Number);
  return new Date(a!, m! - 1, d!);
}

/**
 * El cronograma de flota.
 *
 * ── Por qué el vehículo elegido se resalta y los demás no ────────────────────
 * Porque la pantalla tiene dos momentos: barrer para elegir, y confirmar lo elegido. Sin
 * la marca, después de elegir hay que volver a buscar la fila en la lista para ver contra
 * qué se cruza. El resalte es del carril, no de las barras: lo que cambia es a cuál
 * mirar, no qué significa cada tramo.
 */
function Cronograma({
  ocupacion,
  fallo,
  elegido,
  desde,
  hasta,
}: {
  ocupacion: OcupacionDeFlota | undefined;
  fallo: boolean;
  elegido: string;
  desde: string;
  hasta: string;
}): ReactElement {
  // Callar acá dejaría a quien programa creyendo que la flota está libre, que es la
  // conclusión que lleva a asignar un vehículo ya tomado.
  if (fallo) {
    return (
      <Nota tono="aviso">
        No se pudo consultar la ocupación de la flota. Puede asignar igual —la habilitación
        se verifica aparte—, pero <b>esta pantalla no le está diciendo cuál está libre</b>.
      </Nota>
    );
  }

  if (!ocupacion) {
    return <p className="tw:text-sm tw:text-tinta-mid">Consultando qué tiene tomado cada vehículo…</p>;
  }

  const carriles: CarrilDeLinea[] = ocupacion.carriles.map((c) => ({
    id: c.vehiculo,
    titulo: c.siglas,
    // El estado va en el detalle cuando lo hay: un carril vacío por estar en taller y uno
    // vacío por estar libre se ven igual, y son cosas opuestas.
    detalle: [c.tipoDeVehiculo, c.placa ?? 'sin placa metálica', c.estado ?? 'sin estado declarado']
      .join(' · '),
    inhabilitado: c.inutilizable,
    barras: c.barras.map((b) => ({
      id: b.mision,
      titulo: b.folio,
      desde: comoDiaLocal(b.desde),
      hasta: comoDiaLocal(b.hasta),
      detalle: `${b.destino}, ${b.estado.toLowerCase()}`,
      queEs: 'misión',
      // `EnRuta` en ámbar: el vehículo está afuera AHORA, y reprogramarlo no es lo
      // mismo que mover una misión que todavía no sale.
      tono: b.estado === 'EnRuta' ? 'aviso' : 'info',
    })),
  }));

  return (
    <div className="tw:flex tw:flex-col tw:gap-2">
      <LineaDeCarriles
        carriles={carriles.map((c) =>
          // Lo NO elegido se apaga cuando ya hay elección. Antes de elegir, todos pesan
          // igual: apagar por omisión sugeriría que algunos no son candidatos.
          elegido && c.id !== elegido ? { ...c, inhabilitado: true } : c,
        )}
        desde={comoDiaLocal(desde)}
        hasta={comoDiaLocal(hasta)}
        queEsUnaBarra="misión"
        vacio="La flota está vacía. No hay vehículos registrados para mostrar."
      />
      <p className="tw:text-xs tw:text-tinta-mid">
        Un carril vacío es un vehículo libre en toda la ventana. Que esté libre{' '}
        <b>no significa que habilite</b>: eso lo resuelve la verificación de licencia y
        documentación al elegir.
      </p>
    </div>
  );
}
