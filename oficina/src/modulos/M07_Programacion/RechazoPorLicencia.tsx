import type { ReactElement, ReactNode } from 'react';
import { ShieldBan } from 'lucide-react';

import { Enlace, Nota, Panel } from '../../ui';
import { ROTULO_CLASE, descripcionDelVehiculo } from '../../dominio/habilitacion';
import type { Asignacion, Conductor, Vehiculo } from '../../dominio/habilitacion';
import { soloFecha } from '../M06_Autorizacion/formato';

/**
 * `PT-028` — Rechazo por licencia no habilitante.
 *
 * ── Lo que hace difícil esta pantalla ────────────────────────────────────────
 * **El usuario no puede resolverlo reintentando.** Tiene que hacer una gestión
 * administrativa: pedir la licencia, buscar otro conductor, cambiar el vehículo.
 * Si el mensaje no dice exactamente qué falta, va a probar otra vez con la misma
 * persona, después va a llamar por teléfono, y después va a sacar el vehículo sin
 * orden de misión. Ese es el fallo que esta pantalla existe para impedir.
 *
 * ── Lo que NO ofrece, y es criterio de aceptación literal ────────────────────
 * Ninguna opción de continuar. Ni por jerarquía, ni por urgencia, ni por régimen
 * de uso del vehículo. `BD-02` es bloqueo duro **sin excepción configurable**: una
 * excepción registrada en el sistema sería evidencia en contra ante un siniestro.
 * El funcionario que quiere conducir su propio vehículo asignado se somete al
 * mismo rigor.
 */
export default function RechazoPorLicencia({
  asignacion,
  onElegirConductor,
  onElegirVehiculo,
}: {
  asignacion: Asignacion;
  onElegirConductor(c: Conductor): void;
  onElegirVehiculo(v: Vehiculo): void;
}): ReactElement {
  const { resultado, vehiculo, conductor, alternativas } = asignacion;

  return (
    <Panel titulo="No se puede programar con esta asignación">
      <div className="tw:flex tw:flex-col tw:gap-5">
        <Nota tono="riesgo" icono={<ShieldBan />}>
          <div className="tw:flex tw:flex-col tw:gap-2">
            <p className="tw:font-medium">{Titular(asignacion)}</p>
            <p className="tw:text-sm">{QueHacer(asignacion)}</p>
          </div>
        </Nota>

        <div className="tw:grid tw:gap-5 tw:md:grid-cols-2">
          <Salida
            titulo="Quienes sí habilitan este vehículo"
            vacio={`Nadie del padrón habilita ${vehiculo.siglas} para esta ventana. Hay que gestionar la licencia o cambiar de vehículo.`}
          >
            {alternativas.conductoresQueHabilitan.map((c) => (
              <Opcion
                key={c.id}
                onElegir={() => onElegirConductor(c)}
                titulo={c.nombre}
                detalle={`Categoría ${c.categoria} · vence ${soloFecha(c.venceLicencia)}`}
              />
            ))}
          </Salida>

          <Salida
            titulo={`Vehículos que la categoría ${resultado.categoria} sí habilita`}
            vacio={`La categoría ${resultado.categoria} no habilita ningún otro vehículo disponible.`}
          >
            {alternativas.vehiculosQueHabilita.map((v) => (
              <Opcion
                key={v.id}
                onElegir={() => onElegirVehiculo(v)}
                titulo={v.siglas}
                detalle={`${v.tipo} · ${v.pesoBrutoKg.toLocaleString('es-HN')} kg`}
              />
            ))}
          </Salida>
        </div>

        <p className="tw:text-sm">
          <Enlace href={`/motoristas/${conductor.id}`}>
            Ver el expediente de habilitación de {conductor.nombre}
          </Enlace>
        </p>

        <Constancia {...asignacion} />
      </div>
    </Panel>
  );
}

/**
 * El titular nombra **la categoría que se necesita**, no solo la que falta.
 *
 * «La licencia categoría B no habilita un vehículo de 12,000 kg» deja al usuario
 * igual de perdido que antes. Lo que resuelve es la segunda mitad: «el vehículo
 * INS-C-002 requiere categoría C».
 */
function Titular({ resultado, vehiculo, conductor }: Asignacion): string {
  switch (resultado.motivo) {
    case 'CategoriaNoHabilitaElVehiculo':
      return (
        `La licencia categoría ${resultado.categoria} de ${conductor.nombre} no habilita ` +
        `un ${ROTULO_CLASE[vehiculo.clase]} de ${vehiculo.pesoBrutoKg.toLocaleString('es-HN')} kg` +
        `${vehiculo.llevaRemolque ? ' con remolque enganchado' : ''}. ` +
        `El vehículo ${vehiculo.siglas} requiere categoría ${resultado.categoriaRequerida}.`
      );

    case 'LicenciaVenceDentroDelRango':
      return (
        `La licencia ${resultado.numeroDeLicencia} vence el ` +
        `${soloFecha(resultado.venceLicencia)} y la ventana efectiva de la ` +
        `misión termina el ${soloFecha(resultado.finDeRangoEvaluado)}, ` +
        `incluida la holgura posterior.`
      );

    case 'RestriccionMedicaIncompatible':
      return (
        `${conductor.nombre} tiene la restricción «${resultado.restriccionEnConflicto}» y la misión ` +
        `declara conducción de 19:00 a 23:00.`
      );

    default:
      return 'La asignación no cumple una precondición de habilitación.';
  }
}

/** Las tres causas se resuelven distinto, así que la salida también se dice distinto. */
function QueHacer({ resultado, vehiculo }: Asignacion): string {
  switch (resultado.motivo) {
    case 'CategoriaNoHabilitaElVehiculo':
      return 'Cambie de conductor o de vehículo. La categoría no se puede levantar desde acá, y reintentar con la misma persona va a dar el mismo resultado.';

    case 'LicenciaVenceDentroDelRango':
      return `No basta que esté vigente el día de salida: el conductor manejaría sin licencia el tramo final. O se renueva la licencia antes de la salida, o se asigna a otra persona, o se acorta la ventana de ${vehiculo.siglas}.`;

    case 'RestriccionMedicaIncompatible':
      return 'O se reprograma la misión dentro del horario que la restricción permite, o se asigna a alguien sin esa restricción.';

    default:
      return '';
  }
}

/**
 * El intento bloqueado **es información de control, no ruido**.
 *
 * Y la versión de la matriz va acá porque la matriz es parámetro con vigencia:
 * sin ella el rechazo no es reproducible, y un rechazo que no se puede reproducir
 * no se puede defender.
 */
function Constancia({ resultado, conductor, vehiculo }: Asignacion): ReactElement {
  return (
    <div className="tw:flex tw:flex-col tw:gap-1.5 tw:border-t tw:border-[var(--borde)] tw:pt-4 tw:text-xs tw:text-[var(--txt-2)]">
      <p>
        Este intento queda asentado en la bitácora: {conductor.nombre}, licencia{' '}
        <span className="tw:font-mono">{resultado.numeroDeLicencia}</span>, sobre{' '}
        {descripcionDelVehiculo(vehiculo)}.
      </p>
      <p>
        Evaluado con la matriz licencia↔vehículo{' '}
        <span className="tw:font-mono">{resultado.versionDeMatriz}</span>, rango hasta el{' '}
        {soloFecha(resultado.finDeRangoEvaluado)}.
      </p>
    </div>
  );
}

function Salida({
  titulo,
  vacio,
  children,
}: {
  titulo: string;
  vacio: string;
  children: ReactNode;
}): ReactElement {
  const hay = Array.isArray(children) ? children.length > 0 : Boolean(children);

  return (
    <section className="tw:flex tw:flex-col tw:gap-2.5">
      <h3 className="tw:text-sm tw:font-medium">{titulo}</h3>
      {hay ? (
        <div className="tw:flex tw:flex-col tw:gap-2">{children}</div>
      ) : (
        <p className="tw:text-sm tw:text-[var(--txt-2)]">{vacio}</p>
      )}
    </section>
  );
}

function Opcion({
  titulo,
  detalle,
  onElegir,
}: {
  titulo: string;
  detalle: string;
  onElegir(): void;
}): ReactElement {
  return (
    <button
      type="button"
      onClick={onElegir}
      className="tw:flex tw:flex-col tw:items-start tw:gap-0.5 tw:rounded tw:border tw:border-[var(--borde)] tw:px-3 tw:py-2 tw:text-left tw:text-sm tw:transition-colors tw:hover:border-[var(--acento)] tw:focus-visible:outline-2 tw:focus-visible:outline-offset-2 tw:focus-visible:outline-[var(--acento)]"
    >
      <span className="tw:font-medium">{titulo}</span>
      <span className="tw:text-xs tw:text-[var(--txt-2)]">{detalle}</span>
    </button>
  );
}
