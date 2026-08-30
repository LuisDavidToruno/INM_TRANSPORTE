import type { ReactElement } from 'react';
import { useState } from 'react';
import { useParams } from 'react-router';
import { useQuery } from '@tanstack/react-query';
import { CircleCheck, ScanLine, ShieldX, TriangleAlert } from 'lucide-react';

import { Boton, Campo, Nota, Panel } from '../../ui';
import { pedir } from '../../api/misiones';

/**
 * El punto de verificación al que resuelve el QR del salvoconducto — `RN-25`.
 *
 * ── ⚠️ Quien lee esta pantalla no es un usuario del sistema ─────────────────
 * Es un agente del TSC o de la DNVT parado junto a un vehículo en una carretera. <b>No se
 * autentica, no sabe qué es una Orden de Misión y no va a leer dos párrafos</b>: necesita saber
 * en tres segundos si deja pasar el vehículo o no.
 *
 * Por eso el veredicto va primero, grande y en palabras que dicen qué hacer — y los datos
 * impresos van después, para que los compare con lo que tiene enfrente.
 *
 * ── Los dos caminos de entrada ──────────────────────────────────────────────
 * Por <b>folio</b> cuando escaneó el QR. Por <b>código corto</b> cuando no tenía señal en la
 * carretera, lo anotó y consulta al volver: la verificación en línea no puede ser el único
 * mecanismo en un país con la conectividad que documenta `NRM-09`.
 *
 * ── Y por qué «desactualizado» no es «anulado» ──────────────────────────────
 * Un documento emitido válidamente deja de corresponder cuando la misión cambia debajo de él —
 * un relevo de motorista, una ventana corrida— <b>y nadie lo anuló</b>. Decir «vigente» ahí
 * sería contestar correctamente a la pregunta equivocada.
 */
export default function Verificacion(): ReactElement {
  const { codigo = '' } = useParams();
  const [buscado, setBuscado] = useState(codigo);
  const [consulta, setConsulta] = useState(codigo);

  const { data, isPending, isError } = useQuery({
    queryKey: ['verificar', consulta],
    queryFn: () => pedir<Resultado>(`/salvoconductos/verificar/${encodeURIComponent(consulta)}`),
    enabled: consulta.trim() !== '',
    // El 404 trae cuerpo con el veredicto: no se reintenta, es una respuesta.
    retry: false,
  });

  return (
    <div className="tw:mx-auto tw:flex tw:w-full tw:max-w-2xl tw:flex-col tw:gap-5">
      <header className="tw:flex tw:flex-col tw:gap-1">
        <h1 className="tw:text-xl tw:font-semibold tw:tracking-tight">
          Verificar un salvoconducto
        </h1>
        <p className="tw:text-sm tw:text-tinta-mid">
          Escanee el código QR del documento, o escriba acá el <b>código de verificación</b> de
          ocho caracteres que aparece impreso.
        </p>
      </header>

      <Panel titulo="Código o folio">
        <form
          className="tw:flex tw:flex-wrap tw:items-end tw:gap-3"
          onSubmit={(e) => {
            e.preventDefault();
            setConsulta(buscado.trim());
          }}
        >
          <div className="tw:min-w-56 tw:flex-1">
            <Campo etiqueta="Código de verificación o folio" mono>
              <input
                value={buscado}
                onChange={(e) => setBuscado(e.target.value)}
                placeholder="P4K7-9WQM"
                autoCapitalize="characters"
              />
            </Campo>
          </div>

          <Boton type="submit" icono={<ScanLine />} disabled={buscado.trim() === ''}>
            Verificar
          </Boton>
        </form>
      </Panel>

      {consulta.trim() !== '' && isPending && (
        <p className="tw:text-sm tw:text-tinta-mid">Consultando…</p>
      )}

      {/* El «no encontrado» llega como 404 y es una RESPUESTA: quien verifica necesita saber
          que el documento no existe, no ver una pantalla en blanco que pueda confundir con un
          fallo de red. */}
      {isError && (
        <Nota tono="riesgo" icono={<ShieldX />}>
          <b>No existe ningún salvoconducto con ese folio ni con ese código.</b> Verifique la
          transcripción; si es correcta, el documento no fue emitido por este sistema.
        </Nota>
      )}

      {data !== undefined && data.encontrado && (
        <>
          {/* ── El veredicto, primero y grande ─────────────────────────────
              Es lo único que un agente necesita en los primeros tres segundos. */}
          <Nota
            tono={data.estado === 'Vigente' ? 'ok' : 'riesgo'}
            icono={data.estado === 'Vigente' ? <CircleCheck /> : <TriangleAlert />}
          >
            {data.veredicto}
          </Nota>

          <Panel titulo={`Documento ${data.folio}`}>
            <dl className="tw:grid tw:gap-x-6 tw:gap-y-3 tw:text-sm tw:sm:grid-cols-2">
              <Dato rotulo="Ampara desde">{fecha(data.contenido.desde)}</Dato>
              <Dato rotulo="Ampara hasta">{fecha(data.contenido.hasta)}</Dato>
              <Dato rotulo="Vehículo">{data.contenido.vehiculo}</Dato>
              <Dato rotulo="Motorista">{data.contenido.motorista}</Dato>
              <Dato rotulo="Destino">{data.contenido.destino}</Dato>
              <Dato rotulo="Firmó">{data.contenido.firmadoPor}</Dato>
            </dl>

            <p className="tw:mt-4 tw:text-sm">
              <b>Compare estos datos con lo que tiene enfrente.</b> El documento ampara
              únicamente al vehículo, al motorista, al destino y a la ventana consignados.
            </p>

            {data.contenido.tramosInhabiles.length > 0 && (
              <p className="tw:mt-2 tw:text-xs tw:text-tinta-mid">
                Tramos inhábiles cubiertos: {data.contenido.tramosInhabiles.join(' · ')}
              </p>
            )}

            {/* Un documento reimpreso muchas veces no es irregular por sí solo, y sí es
                información: se dice, sin calificarla. */}
            {data.impresiones.length > 1 && (
              <p className="tw:mt-2 tw:text-xs tw:text-tinta-mid">
                Este folio se imprimió {data.impresiones.length} veces.
              </p>
            )}
          </Panel>
        </>
      )}
    </div>
  );
}

function Dato({
  rotulo,
  children,
}: {
  rotulo: string;
  children: React.ReactNode;
}): ReactElement {
  return (
    <div className="tw:flex tw:flex-col tw:gap-0.5">
      <dt className="tw:text-xs tw:text-tinta-mid">{rotulo}</dt>
      <dd className="tw:font-medium">{children}</dd>
    </div>
  );
}

const fecha = (d: string): string => new Date(`${d}T00:00:00`).toLocaleDateString('es-HN');

interface Resultado {
  encontrado: boolean;
  folio: string;
  estado: 'Vigente' | 'Desactualizado' | 'Anulado';
  /** En palabras que dicen qué hacer. El estado solo no le sirve a un agente. */
  veredicto: string;
  contenido: {
    vehiculo: string;
    motorista: string;
    destino: string;
    desde: string;
    hasta: string;
    tramosInhabiles: string[];
    firmadoPor: string;
  };
  impresiones: { orden: number }[];
}
