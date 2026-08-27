import type { ReactElement, ReactNode } from 'react';
import { useMemo, useState } from 'react';
import { CircleCheck } from 'lucide-react';

import { Boton, Nota, Panel, Pastilla } from '../../ui';
import { CONDUCTORES, VEHICULOS, evaluar } from '../../api/muestraFlota';
import { ROTULO_CLASE, descripcionDelVehiculo } from '../../dominio/habilitacion';
import type { Asignacion as AsignacionDto, Conductor, Vehiculo } from '../../dominio/habilitacion';
import { soloFecha } from '../M06_Autorizacion/formato';
import RechazoPorLicencia from './RechazoPorLicencia';

/**
 * `PT-026` / `PT-027` — Asignación de vehículo y declaración de quien conduce,
 * con `PT-028` incrustado como resultado.
 *
 * ── Por qué el rechazo NO es otra pantalla ───────────────────────────────────
 * Porque el mapa lo pide literal: «ofrecer los caminos de salida en la misma
 * pantalla». Mandar el rechazo a otra ruta obliga a volver para cambiar lo que
 * lo causó, y en el camino se pierde qué se había elegido.
 *
 * ── La evaluación corre al elegir, no al guardar ─────────────────────────────
 * Si el bloqueo apareciera al presionar «Programar», el usuario ya invirtió la
 * decisión completa antes de saber que no procedía. Acá elige el vehículo y ya
 * sabe quién puede conducirlo.
 */
export default function Asignacion(): ReactElement {
  const [vehiculo, setVehiculo] = useState<Vehiculo>(VEHICULOS[0]!);
  const [conductor, setConductor] = useState<Conductor>(CONDUCTORES[0]!);

  // La ventana sale del expediente aprobado. Fija acá mientras `M-06` y `M-07`
  // no estén cosidos: lo que esta pantalla resuelve es la habilitación.
  const finDeRango = useMemo(() => {
    const f = new Date();
    f.setDate(f.getDate() + 8);
    f.setHours(23, 59, 0, 0);
    return f.toISOString();
  }, []);

  const [nocturna, setNocturna] = useState(false);

  const asignacion: AsignacionDto = useMemo(
    () => evaluar(vehiculo, conductor, finDeRango, nocturna),
    [vehiculo, conductor, finDeRango, nocturna],
  );

  return (
    <div className="tw:flex tw:flex-col tw:gap-6">
      <header className="tw:flex tw:flex-col tw:gap-1">
        <h1 className="tw:text-xl tw:font-semibold tw:tracking-tight">Programar la misión</h1>
        <p className="tw:text-sm tw:text-[var(--txt-2)]">
          Elija el vehículo y declare quién conduce. La habilitación se verifica al elegir, no al
          guardar.
        </p>
      </header>

      <div className="tw:grid tw:gap-5 tw:lg:grid-cols-[minmax(0,1fr)_minmax(0,1fr)]">
        <Panel titulo="Vehículo">
          <fieldset className="tw:flex tw:flex-col tw:gap-2.5">
            <legend className="tw:sr-only">Vehículo de la misión</legend>
            {VEHICULOS.map((v) => (
              <Eleccion
                key={v.id}
                grupo="vehiculo"
                idDetalle={`det-veh-${v.id}`}
                elegida={v.id === vehiculo.id}
                onElegir={() => setVehiculo(v)}
                titulo={v.siglas}
                detalle={`${v.tipo} · ${v.pesoBrutoKg.toLocaleString('es-HN')} kg · ${ROTULO_CLASE[v.clase]}${v.llevaRemolque ? ' · con remolque' : ''}`}
                pie={
                  v.placa ? (
                    <span className="tw:font-mono tw:text-xs tw:tabular-nums">{v.placa}</span>
                  ) : (
                    // «Sin placa metálica» es estado válido: hay desabastecimiento
                    // nacional. Se muestra como dato, no como falta.
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
            {CONDUCTORES.map((c) => (
              <Eleccion
                key={c.id}
                grupo="conductor"
                idDetalle={`det-con-${c.id}`}
                elegida={c.id === conductor.id}
                onElegir={() => setConductor(c)}
                titulo={c.nombre}
                detalle={`Licencia ${c.numeroDeLicencia} · categoría ${c.categoria} · vence ${soloFecha(c.venceLicencia)}`}
                pie={
                  c.esDelPadron ? undefined : (
                    // RN-57 verifica sobre quien EFECTIVAMENTE conduce, esté o no en
                    // el padrón. El funcionario con vehículo asignado no se exceptúa.
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
              Lo pregunta la pantalla porque una restricción médica solo bloquea si la misión la
              contradice.
            </span>
          </span>
        </label>
      </Panel>

      {asignacion.resultado.habilita ? (
        <Habilitada asignacion={asignacion} />
      ) : (
        <RechazoPorLicencia
          asignacion={asignacion}
          onElegirConductor={setConductor}
          onElegirVehiculo={setVehiculo}
        />
      )}
    </div>
  );
}

/**
 * La evidencia se muestra **también cuando habilita**.
 *
 * «Guardar solo "verificado" no defiende a nadie» (`BD-02`). Quien programa tiene
 * que poder ver contra qué se resolvió, no solo que salió bien — y esa constancia
 * es la que `PT-031` imprime.
 */
function Habilitada({ asignacion }: { asignacion: AsignacionDto }): ReactElement {
  const { resultado, vehiculo, conductor } = asignacion;

  return (
    <Panel titulo="Verificación de habilitación">
      <div className="tw:flex tw:flex-col tw:gap-4">
        <Nota tono="ok" icono={<CircleCheck />}>
          <p className="tw:font-medium">
            La licencia categoría {resultado.categoria} habilita {descripcionDelVehiculo(vehiculo)}.
          </p>
        </Nota>

        <dl className="tw:grid tw:gap-x-8 tw:gap-y-3 tw:text-sm tw:sm:grid-cols-2">
          <Insumo termino="Quien conduce" valor={conductor.nombre} />
          <Insumo termino="Licencia" valor={`${resultado.numeroDeLicencia} · ${resultado.categoria}`} />
          <Insumo
            termino="Vence"
            valor={soloFecha(resultado.venceLicencia)}
          />
          <Insumo
            termino="Rango evaluado hasta"
            valor={soloFecha(resultado.finDeRangoEvaluado)}
          />
          <Insumo termino="Versión de la matriz" valor={resultado.versionDeMatriz} mono />
          <Insumo
            termino="Atributos usados"
            valor={`${vehiculo.pesoBrutoKg.toLocaleString('es-HN')} kg · ${vehiculo.capacidadPasajeros} pasajeros · ${vehiculo.llevaRemolque ? 'con remolque' : 'sin remolque'}`}
          />
        </dl>

        <Boton variante="primario">Programar la misión</Boton>
      </div>
    </Panel>
  );
}

/**
 * Un radio de verdad, no un `div` con `onClick`.
 *
 * Con `input[type=radio]` el grupo se recorre con las flechas, se anuncia como
 * grupo al lector de pantalla y el foco se ve sin escribir nada. Simularlo con
 * divs cuesta más código y da menos.
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
        // El nombre accesible es la identidad —«INS-P-014»—, no el bloque entero.
        // Sin esto el lector lee «INS-P-014Pick-up doble cabina · 2,800 kg…» de
        // corrido, y quien elige con lector no distingue una opción de la otra.
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
