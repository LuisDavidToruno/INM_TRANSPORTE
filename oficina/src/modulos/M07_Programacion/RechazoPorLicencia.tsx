import type { ReactElement, ReactNode } from 'react';
import { ShieldBan } from 'lucide-react';

import { Enlace, Nota, Panel } from '../../ui';
import type { ConductorDisponible, ResultadoDeAsignacion, VehiculoDeFlota } from '../../api/flota';
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
 */
export default function RechazoPorLicencia({
  resultado,
  vehiculo,
  conductor,
  vehiculos,
  conductores,
  onElegirConductor,
  onElegirVehiculo,
}: {
  resultado: ResultadoDeAsignacion;
  vehiculo: VehiculoDeFlota;
  conductor: ConductorDisponible;
  vehiculos: VehiculoDeFlota[];
  conductores: ConductorDisponible[];
  onElegirConductor(id: string): void;
  onElegirVehiculo(id: string): void;
}): ReactElement {
  const habilitantes = conductores.filter((c) => resultado.conductoresQueHabilitan.includes(c.id));
  const compatibles = vehiculos.filter((v) => resultado.vehiculosQueHabilita.includes(v.id));

  return (
    <Panel titulo="No se puede programar con esta asignación">
      <div className="tw:flex tw:flex-col tw:gap-5">
        <Nota tono="riesgo" icono={<ShieldBan />}>
          <div className="tw:flex tw:flex-col tw:gap-2">
            <p className="tw:font-medium">{titular(resultado, vehiculo, conductor)}</p>
            <p className="tw:text-sm">{queHacer(resultado, vehiculo)}</p>
          </div>
        </Nota>

        <div className="tw:grid tw:gap-5 tw:md:grid-cols-2">
          <Salida
            titulo="Quienes sí habilitan este vehículo"
            vacio={`Nadie habilita ${vehiculo.siglas} para esta ventana. Hay que gestionar la licencia o cambiar de vehículo.`}
          >
            {habilitantes.map((c) => (
              <Opcion
                key={c.id}
                onElegir={() => onElegirConductor(c.id)}
                titulo={c.nombre}
                detalle={`Categoría ${c.licencia.categoria} · vence ${soloFecha(c.licencia.vencimiento)}`}
              />
            ))}
          </Salida>

          <Salida
            titulo={`Vehículos que la categoría ${resultado.categoria} sí habilita`}
            vacio={`La categoría ${resultado.categoria} no habilita ningún otro vehículo disponible.`}
          >
            {compatibles.map((v) => (
              <Opcion
                key={v.id}
                onElegir={() => onElegirVehiculo(v.id)}
                titulo={v.siglas}
                detalle={`${v.ficha.tipoDeVehiculo} · ${v.ficha.pesoBrutoKg.toLocaleString('es-HN')} kg`}
              />
            ))}
          </Salida>
        </div>

        <p className="tw:text-sm">
          <Enlace href={`/motoristas/${conductor.id}`}>
            Ver el expediente de habilitación de {conductor.nombre}
          </Enlace>
        </p>

        {/* El intento bloqueado ES información de control, no ruido. Y la versión de la
            matriz va acá porque la matriz es parámetro con vigencia: un rechazo que no
            se puede reproducir no se puede defender. */}
        <div className="tw:flex tw:flex-col tw:gap-1.5 tw:border-t tw:border-[var(--borde)] tw:pt-4 tw:text-xs tw:text-[var(--txt-2)]">
          <p>
            Este intento queda asentado: {conductor.nombre}, licencia{' '}
            <span className="tw:font-mono">{resultado.numeroDeLicencia}</span>, sobre{' '}
            {vehiculo.siglas} · {vehiculo.ficha.tipoDeVehiculo},{' '}
            {vehiculo.ficha.pesoBrutoKg.toLocaleString('es-HN')} kg.
          </p>
          <p>
            Evaluado con la matriz licencia↔vehículo{' '}
            <span className="tw:font-mono">{resultado.versionDeMatriz}</span>, rango hasta el{' '}
            {soloFecha(resultado.finDeRangoEvaluado)}.
          </p>
        </div>
      </div>
    </Panel>
  );
}

/**
 * El titular nombra **la categoría que se necesita**, no solo la que falta.
 *
 * «La licencia categoría B no habilita 12,000 kg» deja al usuario igual de perdido
 * que antes. Lo que resuelve es la segunda mitad: «requiere categoría C».
 */
function titular(
  r: ResultadoDeAsignacion,
  v: VehiculoDeFlota,
  c: ConductorDisponible,
): string {
  switch (r.motivo) {
    case 'CategoriaNoHabilitaElVehiculo':
      return (
        `La licencia categoría ${r.categoria} de ${c.nombre} no habilita ` +
        `${v.ficha.tipoDeVehiculo.toLowerCase()} de ${v.ficha.pesoBrutoKg.toLocaleString('es-HN')} kg` +
        `${v.ficha.llevaRemolque ? ' con remolque enganchado' : ''}. ` +
        (r.categoriaRequerida
          ? `El vehículo ${v.siglas} requiere categoría ${r.categoriaRequerida}.`
          : `Ninguna categoría del reglamento habilita ${v.siglas} con estos atributos.`)
      );

    case 'LicenciaVenceDentroDelRango':
      return (
        `La licencia ${r.numeroDeLicencia} vence el ${soloFecha(r.venceLicencia)} y la ventana ` +
        `efectiva de la misión termina el ${soloFecha(r.finDeRangoEvaluado)}, incluida la holgura ` +
        `posterior.`
      );

    case 'RestriccionMedicaIncompatible':
      return `${c.nombre} tiene la restricción «${r.restriccionEnConflicto}» y la misión declara conducción de 19:00 a 23:00.`;

    default:
      // El bloqueo vino de `BD-03`, no de `BD-02`.
      return `La documentación de ${v.siglas} no habilita esta misión: ${r.motivoDeDocumentacion}.`;
  }
}

/** Las tres causas se resuelven distinto, así que la salida también se dice distinto. */
function queHacer(r: ResultadoDeAsignacion, v: VehiculoDeFlota): string {
  switch (r.motivo) {
    case 'CategoriaNoHabilitaElVehiculo':
      return 'Cambie de conductor o de vehículo. La categoría no se puede levantar desde acá, y reintentar con la misma persona va a dar el mismo resultado.';

    case 'LicenciaVenceDentroDelRango':
      // La ventana ya no se puede acortar desde esta pantalla, y decirlo importa: era
      // justamente la salida fácil que volvía inútil el bloqueo.
      return 'No basta que esté vigente el día de salida: el conductor manejaría sin licencia el tramo final. O se renueva la licencia antes de la salida, o se asigna a otra persona, o la dependencia solicita una ventana más corta — que no se cambia desde acá.';

    case 'RestriccionMedicaIncompatible':
      return 'O se reprograma la misión dentro del horario que la restricción permite, o se asigna a alguien sin esa restricción.';

    default:
      return `Regularice la documentación de ${v.siglas} en su expediente vehicular.`;
  }
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
      className="tw:flex tw:flex-col tw:items-start tw:gap-0.5 tw:rounded tw:border tw:border-[var(--borde)] tw:px-3 tw:py-2 tw:text-left tw:text-sm tw:transition-colors tw:hover:border-[var(--acento)]"
    >
      <span className="tw:font-medium">{titulo}</span>
      <span className="tw:text-xs tw:text-[var(--txt-2)]">{detalle}</span>
    </button>
  );
}
