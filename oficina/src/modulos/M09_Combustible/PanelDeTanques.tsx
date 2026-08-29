import type { ReactElement } from 'react';
import { useQuery } from '@tanstack/react-query';
import { CircleAlert, Droplets } from 'lucide-react';

import { Nota, Panel, Pastilla } from '../../ui';
import { despachosSinRespaldo, galones, tanques as pedirTanques } from '../../api/combustible';
import type { DespachoSinRespaldo, Tanque } from '../../api/combustible';
import { momentoCompleto } from '../M06_Autorizacion/formato';

/**
 * Las existencias del tanque institucional — `RN-83` punto 5.
 *
 * ── Por qué esta pantalla existe ────────────────────────────────────────────
 * Porque `TANQUE_INSTITUCIONAL` se podía declarar como fuente y **no descontaba de ninguna
 * parte**. El galón quedaba imputado al vehículo y el tanque de la sede no se enteraba:
 * exactamente igual de invisible que antes de `RN-83`, sólo que con la apariencia de estar
 * registrado.
 *
 * ── El panel no se esconde cuando no hay tanques ────────────────────────────
 * Se calla, que es distinto. `[C]` insumo #36 — que la institución tenga almacenamiento propio
 * no está confirmado, y si no lo tiene, no hay tanques y esto no ocupa espacio. Pero los
 * despachos sin respaldo se muestran **igual**: un galón declarado del tanque cuando no hay
 * ningún tanque registrado es precisamente el hallazgo más fuerte que este panel puede dar.
 */
export default function PanelDeTanques(): ReactElement | null {
  const lista = useQuery({ queryKey: ['tanques'], queryFn: pedirTanques });
  const huerfanos = useQuery({
    queryKey: ['despachos-sin-respaldo'],
    queryFn: despachosSinRespaldo,
  });

  if (lista.isError || huerfanos.isError) {
    return (
      <Nota tono="riesgo" icono={<CircleAlert />}>
        No se pudieron cargar las existencias del tanque institucional.
      </Nota>
    );
  }

  // No se afirma un negativo mientras se carga.
  if (lista.isPending || huerfanos.isPending) {
    return <p className="tw:text-sm tw:text-tinta-mid">Cargando existencias…</p>;
  }

  const tanquesDeLaInstitucion = lista.data ?? [];
  const sinRespaldo = huerfanos.data ?? [];

  // Sin tanques y sin discrepancias no hay nada que decir. La institución que no tiene
  // cisterna no debería ver un panel vacío recordándoselo en cada visita.
  if (tanquesDeLaInstitucion.length === 0 && sinRespaldo.length === 0) return null;

  return (
    <div className="tw:flex tw:flex-col tw:gap-4">
      <div className="tw:flex tw:flex-col tw:gap-1">
        <h2 className="tw:text-base tw:font-semibold tw:tracking-tight">
          Tanque institucional
        </h2>
        <p className="tw:text-sm tw:text-tinta-mid">
          {tanquesDeLaInstitucion.length === 0
            ? 'No hay ningún tanque registrado.'
            : `${tanquesDeLaInstitucion.length} ${
                tanquesDeLaInstitucion.length === 1 ? 'tanque' : 'tanques'
              }, con ${galones(
                tanquesDeLaInstitucion.reduce((suma, t) => suma + t.existencia, 0),
              )} en libros.`}
        </p>
      </div>

      {sinRespaldo.length > 0 && (
        <Nota tono="riesgo" icono={<CircleAlert />}>
          {sinRespaldo.length === 1 ? '1 abastecimiento declara' : `${sinRespaldo.length} abastecimientos declaran`}{' '}
          haber salido del tanque institucional y <b>ningún tanque lo registró</b> —{' '}
          {galones(sinRespaldo.reduce((suma, d) => suma + d.galones, 0))} en total. No es
          necesariamente un faltante: lo más común es que el despacho se haya hecho y nadie lo
          asentara. Lo que sí es seguro es que <b>hay dos registros que no se corresponden</b>.
        </Nota>
      )}

      {tanquesDeLaInstitucion.map((t) => (
        <TarjetaDeTanque key={t.id} tanque={t} />
      ))}

      {sinRespaldo.length > 0 && (
        <Panel>
          <div className="tw:flex tw:flex-col tw:gap-2">
            <h3 className="tw:text-sm tw:font-medium">Declarados del tanque, sin despacho</h3>
            {sinRespaldo.map((d) => (
              <FilaSinRespaldo key={d.abastecimiento} despacho={d} />
            ))}
          </div>
        </Panel>
      )}
    </div>
  );
}

function TarjetaDeTanque({ tanque: t }: { tanque: Tanque }): ReactElement {
  // Nula es «nunca se arqueó», y eso no es cero: de un tanque nunca medido no se deduce que
  // cuadre. Los tres casos se dicen distinto a propósito.
  const arqueo =
    t.diferenciaDelUltimoArqueo === null
      ? { tono: 'aviso' as const, texto: 'Nunca se ha arqueado' }
      : t.diferenciaDelUltimoArqueo === 0
        ? { tono: 'ok' as const, texto: 'Último arqueo: cuadró exacto' }
        : t.diferenciaDelUltimoArqueo > 0
          ? {
              tono: 'riesgo' as const,
              texto: `Último arqueo: faltaban ${galones(t.diferenciaDelUltimoArqueo)}`,
            }
          : {
              tono: 'riesgo' as const,
              texto: `Último arqueo: sobraban ${galones(-t.diferenciaDelUltimoArqueo)}`,
            };

  return (
    <Panel>
      <div className="tw:flex tw:flex-col tw:gap-3">
        <div className="tw:flex tw:flex-wrap tw:items-start tw:justify-between tw:gap-3">
          <div className="tw:flex tw:flex-col tw:gap-1">
            <div className="tw:flex tw:items-center tw:gap-2">
              <Droplets className="tw:size-4 tw:text-tinta-mid" aria-hidden />
              <span className="tw:font-medium">{t.nombre}</span>
            </div>
            <span className="tw:text-xs tw:text-tinta-mid">
              {t.ambito} · {t.tipoDeCombustible}
              {t.capacidad !== null && ` · capacidad ${galones(t.capacidad)}`}
            </span>
          </div>

          <Pastilla tono={arqueo.tono}>{arqueo.texto}</Pastilla>
        </div>

        <div className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-5 tw:gap-y-1">
          <span className="tw:text-2xl tw:font-semibold tw:tabular-nums">
            {galones(t.existencia)}
          </span>
          {/* «En libros» y no «en el tanque». La diferencia es exactamente lo que un arqueo
              mide, y llamarla de otra forma prometería una certeza que sólo da la varilla. */}
          <span className="tw:text-xs tw:text-tinta-mid">
            en libros, sobre {t.movimientos} {t.movimientos === 1 ? 'asiento' : 'asientos'}
          </span>
          {t.ultimoArqueo !== null && (
            <span className="tw:text-xs tw:text-tinta-mid">
              medido por última vez el {momentoCompleto(t.ultimoArqueo)}
            </span>
          )}
        </div>
      </div>
    </Panel>
  );
}

function FilaSinRespaldo({ despacho: d }: { despacho: DespachoSinRespaldo }): ReactElement {
  return (
    <div className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-3 tw:border-l-2 tw:border-riesgo-fg tw:pl-3 tw:text-sm">
      <span className="tw:font-medium tw:tabular-nums">{galones(d.galones)}</span>
      <span className="tw:font-mono tw:text-xs tw:text-tinta-mid">{d.vehiculo}</span>
      <span className="tw:text-xs tw:text-tinta-mid">
        {momentoCompleto(d.momento)} · registró {d.registra}
      </span>
    </div>
  );
}
