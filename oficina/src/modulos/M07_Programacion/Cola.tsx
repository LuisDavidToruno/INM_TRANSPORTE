import type { ReactElement } from 'react';
import { useMemo, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { CalendarClock, CircleAlert, TriangleAlert } from 'lucide-react';

import { Boton, Campo, Enlace, Modal, Nota, Pastilla, Segmentado, Tabla, Vacio, avisar } from '../../ui';
import type { ColumnaDef } from '../../ui';
import { MOTIVOS_DE_ANULACION, anular, colaDeProgramacion } from '../../api/misiones';
import type { Expediente, MotivoDeAnulacion } from '../../dominio/mision';
import { diaYHora, faltanDias, soloFecha } from '../M06_Autorizacion/formato';

/**
 * `PT-025` — Cola de programación con caducidad de la aprobación.
 *
 * ── Por qué la caducidad se ve, y no se descubre al guardar ──────────────────
 * Porque el Jefe de Transporte planifica sobre esta lista. Si la caducidad
 * apareciera recién al intentar programar, habría elegido vehículo y motorista
 * para una solicitud que ya no procede — y esa elección tiene costo real: el
 * vehículo estuvo reservado mientras tanto.
 *
 * ── Depurar la cola no es limpieza, es el indicador ──────────────────────────
 * «Una cola de aprobadas que nadie depura oculta el déficit real de flota, que es
 * justamente el indicador que la institución necesita.» Por eso anular exige
 * motivo del catálogo: un texto libre no produce ningún indicador.
 */
export default function Cola(): ReactElement {
  const [filtro, setFiltro] = useState<'porProgramar' | 'caducadas'>('porProgramar');
  const [aAnular, setAAnular] = useState<Expediente | null>(null);

  const { data, isPending, isError, error } = useQuery({
    queryKey: ['cola-programacion'],
    queryFn: colaDeProgramacion,
  });

  const { porProgramar, caducadas } = useMemo(() => {
    const todas = data ?? [];
    return {
      porProgramar: todas.filter((e) => !e.aprobacionCaducada),
      caducadas: todas.filter((e) => e.aprobacionCaducada),
    };
  }, [data]);

  if (isError) {
    return (
      <Nota tono="riesgo" icono={<CircleAlert />}>
        No se pudo cargar la cola: {error instanceof Error ? error.message : 'error desconocido'}.
      </Nota>
    );
  }

  const filas = filtro === 'caducadas' ? caducadas : porProgramar;

  return (
    <div className="tw:flex tw:flex-col tw:gap-5">
      <header className="tw:flex tw:flex-col tw:gap-1">
        <h1 className="tw:text-xl tw:font-semibold tw:tracking-tight">Cola de programación</h1>
        <p className="tw:text-sm tw:text-[var(--txt-2)]">
          Expedientes aprobados esperando vehículo y motorista.
        </p>
      </header>

      {caducadas.length > 0 && filtro === 'porProgramar' && (
        <Nota tono="aviso" icono={<TriangleAlert />}>
          {caducadas.length === 1
            ? 'Hay 1 aprobación caducada sin depurar.'
            : `Hay ${caducadas.length} aprobaciones caducadas sin depurar.`}{' '}
          Anularlas con motivo tipificado es lo que convierte el atraso en un dato: sin eso,
          el déficit de flota no aparece en ningún reporte.
        </Nota>
      )}

      <Segmentado
        etiqueta="Qué se muestra en la cola"
        opciones={[
          { valor: 'porProgramar', etiqueta: 'Por programar', nota: porProgramar.length },
          { valor: 'caducadas', etiqueta: 'Caducadas', nota: caducadas.length },
        ]}
        valor={filtro}
        onCambio={(v) => setFiltro(v as typeof filtro)}
      />

      {!isPending && filas.length === 0 ? (
        <Vacio
          icono={<CalendarClock />}
          titulo={
            filtro === 'caducadas' ? 'Ninguna aprobación caducada' : 'Nada esperando programación'
          }
          descripcion={
            filtro === 'caducadas'
              ? 'La cola está depurada: no hay aprobaciones que se hayan vencido sin atender.'
              : 'Cuando una jefatura autorice una solicitud, aparecerá acá para asignarle vehículo y motorista.'
          }
        />
      ) : (
        <Tabla
          columnas={filtro === 'caducadas' ? COLUMNAS_CADUCADAS(setAAnular) : COLUMNAS}
          filas={filas}
          claveDe={(e) => e.id}
          cargando={isPending}
        />
      )}

      {aAnular && <DialogoDeAnulacion expediente={aAnular} onCerrar={() => setAAnular(null)} />}
    </div>
  );
}

const COLUMNAS: ColumnaDef<Expediente>[] = [
  {
    id: 'folio',
    cabecera: 'Folio',
    ancho: 148,
    celda: (e) => (
      <Enlace href={`/programacion/${e.id}`}>
        <span className="tw:font-mono tw:text-[13px] tw:tabular-nums">{e.folio}</span>
      </Enlace>
    ),
    ordenable: true,
    valorOrden: (e) => e.folio,
  },
  {
    id: 'objeto',
    cabecera: 'Qué se moviliza',
    celda: (e) => (
      <div className="tw:flex tw:flex-col">
        <span className="tw:line-clamp-1">{e.objetoDelTraslado}</span>
        <span className="tw:text-xs tw:text-[var(--txt-2)]">
          {e.dependencia} · destino: {e.destino}
        </span>
      </div>
    ),
  },
  {
    id: 'salida',
    // La caducidad se cuenta contra la salida, no contra el retorno: por eso la
    // columna es «Sale», y el aviso de cercanía va pegado a ese número.
    cabecera: 'Sale',
    ancho: 190,
    celda: (e) => <Salida expediente={e} />,
    ordenable: true,
    valorOrden: (e) => e.salidaPrevista,
  },
];

const COLUMNAS_CADUCADAS = (
  alAnular: (e: Expediente) => void,
): ColumnaDef<Expediente>[] => [
  COLUMNAS[0]!,
  COLUMNAS[1]!,
  {
    id: 'caduco',
    cabecera: 'Caducó el',
    ancho: 190,
    celda: (e) => (
      <span className="tw:tabular-nums tw:text-[var(--txt-2)]">{soloFecha(e.salidaPrevista)}</span>
    ),
    ordenable: true,
    valorOrden: (e) => e.salidaPrevista,
  },
  {
    id: 'accion',
    cabecera: '',
    ancho: 132,
    celda: (e) => (
      <Boton variante="secundario" tamano="sm" onClick={() => alAnular(e)}>
        Anular
      </Boton>
    ),
  },
];

/**
 * La marca de cercanía existe porque el riesgo no es uniforme: una solicitud que
 * sale mañana y no tiene vehículo es un problema hoy; una que sale en tres semanas
 * no lo es. Sin la marca, las dos se ven igual en la lista.
 */
function Salida({ expediente }: { expediente: Expediente }): ReactElement {
  const cuando = faltanDias(expediente.salidaPrevista);
  const dias = Math.round(
    (new Date(expediente.salidaPrevista).getTime() - Date.now()) / 86_400_000,
  );

  return (
    <div className="tw:flex tw:flex-col tw:gap-1">
      <span className="tw:tabular-nums">{diaYHora(expediente.salidaPrevista)}</span>
      {cuando &&
        (dias <= 2 ? (
          <Pastilla tono="aviso">{cuando}</Pastilla>
        ) : (
          <span className="tw:text-xs tw:text-[var(--txt-2)]">{cuando}</span>
        ))}
    </div>
  );
}

/**
 * ── Por qué el motivo es una lista y no un campo de texto ────────────────────
 * Porque la tipificación **es** el indicador de déficit de flota. Un campo libre
 * produce «no había carro», «no hubo unidad», «sin flota» y «falta vehículo» para
 * el mismo hecho, y ningún reporte los suma.
 *
 * El comentario sigue existiendo porque el motivo tipificado no cuenta la historia
 * — pero es complemento, no sustituto, y por eso no habilita el botón por sí solo.
 */
function DialogoDeAnulacion({
  expediente,
  onCerrar,
}: {
  expediente: Expediente;
  onCerrar(): void;
}): ReactElement {
  const cliente = useQueryClient();
  const [motivo, setMotivo] = useState<MotivoDeAnulacion | ''>('');
  const [comentario, setComentario] = useState('');

  const anulacion = useMutation({
    mutationFn: () => anular(expediente.id, 'Rolando Discua', motivo as MotivoDeAnulacion, comentario),
    onSuccess: async () => {
      avisar.exito(`${expediente.folio} anulada. El motivo suma al indicador de déficit de flota.`);
      await cliente.invalidateQueries({ queryKey: ['cola-programacion'] });
      onCerrar();
    },
    onError: () => avisar.error('No se pudo anular. El expediente quedó como estaba.'),
  });

  return (
    <Modal
      abierto
      titulo={`Anular ${expediente.folio}`}
      descripcion={`La anulación no se deshace. El expediente sigue consultable con el filtro de caducadas, y se notifica a ${expediente.dependencia}.`}
      destructivo
      onCerrar={onCerrar}
      acciones={
        <>
          <Boton variante="fantasma" onClick={onCerrar} disabled={anulacion.isPending}>
            Volver
          </Boton>
          <Boton
            variante="peligro"
            disabled={!motivo || anulacion.isPending}
            cargando={anulacion.isPending}
            onClick={() => anulacion.mutate()}
          >
            Anular expediente
          </Boton>
        </>
      }
    >
      <div className="tw:flex tw:flex-col tw:gap-4">
        <fieldset className="tw:flex tw:flex-col tw:gap-2">
          <legend className="tw:mb-1 tw:text-sm tw:font-medium">Motivo</legend>
          {MOTIVOS_DE_ANULACION.map((m) => (
            <label key={m.valor} className="tw:flex tw:cursor-pointer tw:items-start tw:gap-2 tw:text-sm">
              <input
                type="radio"
                name="motivo-anulacion"
                checked={motivo === m.valor}
                onChange={() => setMotivo(m.valor)}
                aria-label={m.texto}
                className="tw:mt-0.5 tw:size-4 tw:shrink-0 tw:accent-[var(--acento)]"
              />
              <span>{m.texto}</span>
            </label>
          ))}
        </fieldset>

        <Campo
          etiqueta="Comentario"
          ayuda="Acompaña al motivo; no lo reemplaza. Lo lee quien revise el indicador dentro de un año."
        >
          {(props) => (
            <textarea
              {...props}
              rows={3}
              value={comentario}
              onChange={(e) => setComentario(e.target.value)}
              className="loki-input"
            />
          )}
        </Campo>

        {!motivo && (
          <p className="tw:text-xs tw:text-[var(--txt-2)]">
            Seleccione el motivo del catálogo. El texto libre no produce indicador.
          </p>
        )}
      </div>
    </Modal>
  );
}
