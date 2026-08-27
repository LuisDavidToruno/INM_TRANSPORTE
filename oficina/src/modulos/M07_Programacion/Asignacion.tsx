import type { ReactElement, ReactNode } from 'react';
import { useState } from 'react';
import { useNavigate, useParams } from 'react-router';
import { useMutation, useQueries, useQuery, useQueryClient } from '@tanstack/react-query';
import { CircleCheck } from 'lucide-react';

import { Boton, Nota, Panel, Pastilla, avisar } from '../../ui';
import { conductores, evaluarAsignacion, flota, programar } from '../../api/flota';
import type { ConductorDisponible, ResultadoDeAsignacion, VehiculoDeFlota } from '../../api/flota';
import { BloqueoDuro, expediente as traerExpediente } from '../../api/misiones';
import type { Expediente } from '../../dominio/mision';
import { soloFecha } from '../M06_Autorizacion/formato';
import RechazoPorLicencia from './RechazoPorLicencia';

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
 */
export default function Asignacion(): ReactElement {
  const { id = '' } = useParams();
  const navegar = useNavigate();
  const cliente = useQueryClient();

  const [idVehiculo, setVehiculo] = useState('');
  const [idConductor, setConductor] = useState('');
  const [nocturna, setNocturna] = useState(false);

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

  const programacion = useMutation({
    mutationFn: () => programar(id, 'Rolando Discua', idVehiculo, idConductor),
    onSuccess: async () => {
      avisar.exito('Misión programada. El vehículo y el motorista quedaron reservados.');
      await cliente.invalidateQueries({ queryKey: ['cola-programacion'] });
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
      avisar.error('No se pudo programar. El expediente quedó como estaba.');
    },
  });

  const expediente = expedienteQ.data;
  const vehiculos = flotaQ.data;
  const personas = conductoresQ.data;

  if (!expediente || !vehiculos || !personas) return <Cargando />;

  const vehiculo = vehiculos.find((v) => v.id === idVehiculo);
  const conductor = personas.find((c) => c.id === idConductor);

  return (
    <div className="tw:flex tw:flex-col tw:gap-6">
      <Cabecera expediente={expediente} />

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

      <Panel titulo="Condiciones de la misión">
        <label className="tw:flex tw:cursor-pointer tw:items-start tw:gap-2 tw:text-sm">
          <input
            type="checkbox"
            aria-label="La misión declara conducción nocturna, entre las 19:00 y las 23:00"
            checked={nocturna}
            onChange={(e) => setNocturna(e.target.checked)}
            className="tw:mt-0.5 tw:size-4 tw:shrink-0 tw:accent-[var(--acento)]"
          />
          <span>
            La misión declara conducción entre las 19:00 y las 23:00.
            <span className="tw:block tw:text-xs tw:text-[var(--txt-2)]">
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
      ) : resultado.habilita ? (
        <Habilitada
          resultado={resultado}
          vehiculo={vehiculo!}
          conductor={conductor!}
          enviando={programacion.isPending}
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

function Cabecera({ expediente }: { expediente: Expediente }): ReactElement {
  return (
    <header className="tw:flex tw:flex-col tw:gap-2">
      <div className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-4 tw:gap-y-1">
        <h1 className="tw:font-mono tw:text-xl tw:font-semibold tw:tabular-nums tw:tracking-tight">
          {expediente.folio}
        </h1>
        <Pastilla tono="info">Aprobada</Pastilla>
      </div>
      <p className="tw:text-sm tw:text-[var(--txt-2)]">
        {expediente.objetoDelTraslado} · destino {expediente.destino}
      </p>
      {/* La ventana se muestra, no se edita: es lo que declara quien pide. */}
      <p className="tw:text-sm">
        Ventana solicitada: <b>{soloFecha(expediente.salidaPrevista)}</b> al{' '}
        <b>{soloFecha(expediente.retornoPrevisto)}</b>. La licencia tiene que cubrirla completa,
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
  onProgramar,
}: {
  resultado: ResultadoDeAsignacion;
  vehiculo: VehiculoDeFlota;
  conductor: ConductorDisponible;
  enviando: boolean;
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
              <p className="tw:text-xs tw:text-[var(--txt-2)]">
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
          disabled={enviando || (exigeAcuse && !acusada)}
          onClick={onProgramar}
        >
          Programar la misión
        </Boton>
      </div>
    </Panel>
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
      <dt className="tw:text-xs tw:text-[var(--txt-2)]">{termino}</dt>
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
          ? 'tw:border-[var(--acento)] tw:bg-[var(--sup-2)]'
          : 'tw:border-[var(--borde)] tw:hover:border-[var(--txt-2)]'
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
        className="tw:mt-1 tw:size-4 tw:shrink-0 tw:accent-[var(--acento)]"
      />
      <span className="tw:flex tw:min-w-0 tw:flex-col tw:gap-1">
        <span className="tw:text-sm tw:font-medium">{titulo}</span>
        <span id={idDetalle} className="tw:text-xs tw:text-[var(--txt-2)]">
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
      <div className="tw:h-7 tw:w-48 tw:animate-pulse tw:rounded tw:bg-[var(--sup-2)]" />
      <div className="tw:grid tw:gap-5 tw:lg:grid-cols-2">
        <div className="tw:h-64 tw:animate-pulse tw:rounded tw:bg-[var(--sup-2)]" />
        <div className="tw:h-64 tw:animate-pulse tw:rounded tw:bg-[var(--sup-2)]" />
      </div>
      <span className="tw:sr-only">Cargando el expediente y la flota…</span>
    </div>
  );
}
