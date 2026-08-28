import type { ReactElement } from 'react';
import { useMemo, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { CalendarClock, CircleAlert, TriangleAlert } from 'lucide-react';

import {
  Boton,
  Campo,
  Enlace,
  EnlaceBoton,
  Modal,
  Nota,
  Pastilla,
  Segmentado,
  Tabla,
  Vacio,
  avisar,
} from '../../ui';
import type { ColumnaDef } from '../../ui';
import {
  MOTIVOS_DE_ANULACION,
  anular,
  anularProgramada,
  colaDeProgramacion,
  colaDeProgramadas,
  desprogramar,
} from '../../api/misiones';
import type { Expediente, MotivoDeAnulacion } from '../../dominio/mision';
import { diaYHora, faltanDias, laDependencia, soloFecha } from '../M06_Autorizacion/formato';

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
 *
 * ── Y por qué las PROGRAMADAS también viven acá ──────────────────────────────
 * Porque son las que tienen recursos tomados, y hasta que existieron `T-11` y `T-13`
 * **no había forma de soltarlos**: un vehículo asignado por error quedaba comprometido
 * hasta que alguien lo despachara. Es la misma persona —el Jefe de Transporte— y la misma
 * decisión —qué hago con la flota esta semana—, así que es la misma pantalla.
 */
export default function Cola(): ReactElement {
  const [filtro, setFiltro] = useState<'porProgramar' | 'caducadas' | 'programadas'>(
    'porProgramar',
  );
  const [aAnular, setAAnular] = useState<Expediente | null>(null);
  const [aDesprogramar, setADesprogramar] = useState<Expediente | null>(null);
  const [aMatar, setAMatar] = useState<Expediente | null>(null);

  const { data, isPending, isError, error } = useQuery({
    queryKey: ['cola-programacion'],
    queryFn: colaDeProgramacion,
  });

  // Consulta aparte: son otro estado y otras acciones. Traerlas juntas obligaría al
  // servidor a devolver dos colas en una respuesta que ninguna pantalla pide entera.
  const programadas = useQuery({
    queryKey: ['cola-programadas'],
    queryFn: colaDeProgramadas,
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

  const filas =
    filtro === 'caducadas' ? caducadas
    : filtro === 'programadas' ? (programadas.data ?? [])
    : porProgramar;

  return (
    <div className="tw:flex tw:flex-col tw:gap-5">
      <header className="tw:flex tw:flex-col tw:gap-1">
        <h1 className="tw:text-xl tw:font-semibold tw:tracking-tight">Cola de programación</h1>
        <p className="tw:text-sm tw:text-tinta-mid">
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
          {
            valor: 'programadas',
            etiqueta: 'Programadas',
            nota: programadas.data?.length ?? 0,
          },
        ]}
        valor={filtro}
        onCambio={(v) => setFiltro(v as typeof filtro)}
      />

      {!isPending && filas.length === 0 ? (
        <Vacio
          icono={<CalendarClock />}
          titulo={
            filtro === 'caducadas' ? 'Ninguna aprobación caducada'
            : filtro === 'programadas' ? 'Ninguna misión programada'
            : 'Nada esperando programación'
          }
          descripcion={
            filtro === 'caducadas'
              ? 'La cola está depurada: no hay aprobaciones que se hayan vencido sin atender.'
              : filtro === 'programadas'
                ? 'Ningún vehículo está comprometido en este momento. Toda la flota está libre.'
                : 'Cuando una jefatura autorice una solicitud, aparecerá acá para asignarle vehículo y motorista.'
          }
        />
      ) : (
        <Tabla
          columnas={
            filtro === 'caducadas' ? COLUMNAS_CADUCADAS(setAAnular)
            : filtro === 'programadas' ? COLUMNAS_PROGRAMADAS(setADesprogramar, setAMatar)
            : COLUMNAS
          }
          filas={filas}
          claveDe={(e) => e.id}
          cargando={filtro === 'programadas' ? programadas.isPending : isPending}
        />
      )}

      {aAnular && <DialogoDeAnulacion expediente={aAnular} onCerrar={() => setAAnular(null)} />}

      {aDesprogramar && (
        <DialogoDeDesprogramacion
          expediente={aDesprogramar}
          onCerrar={() => setADesprogramar(null)}
        />
      )}

      {aMatar && (
        <DialogoDeAnulacion
          expediente={aMatar}
          programada
          onCerrar={() => setAMatar(null)}
        />
      )}
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
        <span className="tw:text-xs tw:text-tinta-mid">
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
      <span className="tw:tabular-nums tw:text-tinta-mid">{soloFecha(e.salidaPrevista)}</span>
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
 * Las dos salidas de una misión programada, y <b>se ven distintas a propósito</b>.
 *
 * <b>Tres salidas, en orden de daño.</b> «Cambiar recurso» (`T-10`) no suelta la misión ni
 * pierde el folio: es la más barata y va primero. «Devolver a la cola» (`T-11`) la suelta
 * pero la deja viva. «Anular» (`T-13`) la mata y no se vuelve.
 *
 * Ponerlas con el mismo peso visual invitaría a usar la más destructiva cuando bastaba la
 * anterior — y la diferencia entre la primera y la última es que la dependencia tenga que
 * volver a pedir el viaje o ni se entere.
 */
const COLUMNAS_PROGRAMADAS = (
  alDesprogramar: (e: Expediente) => void,
  alAnular: (e: Expediente) => void,
): ColumnaDef<Expediente>[] => [
  COLUMNAS[0]!,
  COLUMNAS[1]!,
  {
    id: 'ventana',
    cabecera: 'Ventana comprometida',
    ancho: 210,
    celda: (e) => (
      <span className="tw:tabular-nums tw:text-tinta-mid">
        {soloFecha(e.salidaPrevista)} al {soloFecha(e.retornoPrevisto)}
      </span>
    ),
    ordenable: true,
    valorOrden: (e) => e.salidaPrevista,
  },
  {
    id: 'acciones',
    cabecera: '',
    ancho: 360,
    celda: (e) => (
      <div className="tw:flex tw:gap-2">
        {/* Es un enlace y no un botón porque lleva a otra pantalla: la reasignación exige
            elegir vehículo y motorista, y esa selección ya existe una sola vez. */}
        <EnlaceBoton href={`/programacion/${e.id}`} variante="secundario" tamano="sm">
          Cambiar recurso
        </EnlaceBoton>
        <Boton variante="fantasma" tamano="sm" onClick={() => alDesprogramar(e)}>
          Devolver a la cola
        </Boton>
        <Boton variante="fantasma" tamano="sm" onClick={() => alAnular(e)}>
          Anular
        </Boton>
      </div>
    ),
  },
];

/**
 * `T-11` — devolver a la cola.
 *
 * ── Por qué el motivo es texto libre acá y lista en la anulación ─────────────
 * Porque contestan preguntas distintas. El motivo tipificado de la anulación <b>es</b> el
 * indicador de déficit de flota: hay que poder sumarlo. Éste no alimenta ningún indicador
 * —la misión sigue viva y se va a reprogramar—: explica a la dependencia por qué perdió el
 * vehículo que ya tenía. Meterlo en el catálogo ensuciaría el indicador con hechos que no
 * son déficit.
 */
function DialogoDeDesprogramacion({
  expediente,
  onCerrar,
}: {
  expediente: Expediente;
  onCerrar(): void;
}): ReactElement {
  const cliente = useQueryClient();
  const [motivo, setMotivo] = useState('');

  const operacion = useMutation({
    mutationFn: () => desprogramar(expediente.id, 'Rolando Discua', motivo),
    onSuccess: async () => {
      avisar.exito(
        `${expediente.folio} volvió a la cola. El vehículo y el motorista quedaron libres.`,
      );
      await cliente.invalidateQueries({ queryKey: ['cola-programadas'] });
      await cliente.invalidateQueries({ queryKey: ['cola-programacion'] });
      onCerrar();
    },
    onError: () => avisar.error('No se pudo desprogramar. El expediente quedó como estaba.'),
  });

  return (
    <Modal
      abierto
      titulo={`Devolver ${expediente.folio} a la cola`}
      descripcion={`El vehículo y el motorista quedan libres, y la misión conserva su aprobación: no hace falta que la jefatura vuelva a firmar. Se notifica a ${laDependencia(expediente.dependencia)}.`}
      onCerrar={onCerrar}
      // Sin botón de salida propio: `Modal` ya rinde el suyo con `etiquetaCerrar`.
      // Agregar otro deja dos que hacen lo mismo, y el usuario tiene que decidir cuál
      // cancela — una decisión que no debería existir.
      acciones={
        <Boton
          onClick={() => operacion.mutate()}
          disabled={!motivo.trim() || operacion.isPending}
        >
          {operacion.isPending ? 'Devolviendo…' : 'Devolver a la cola'}
        </Boton>
      }
    >
      <Campo
        etiqueta="Por qué pierde el vehículo"
        obligatorio
        ayuda="Lo lee la dependencia. «Desplazada por prioridad superior» o «el vehículo entró a taller» le dicen qué esperar; «se libera» no le dice nada."
      >
        {(props) => (
          <textarea
            {...props}
            rows={3}
            value={motivo}
            onChange={(e) => setMotivo(e.target.value)}
          />
        )}
      </Campo>
    </Modal>
  );
}

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
          <span className="tw:text-xs tw:text-tinta-mid">{cuando}</span>
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
  programada = false,
  onCerrar,
}: {
  expediente: Expediente;
  /**
   * Si la misión ya tenía recursos tomados — `T-13` en vez de `T-09`.
   *
   * <b>El catálogo de motivos es el mismo, y la transición no.</b> Reusar el diálogo
   * mantiene una sola lista de motivos —dos se separarían y el indicador tendría que sumar
   * dos vocabularios—, pero llamar al endpoint equivocado dejaría el diario diciendo que
   * se anuló una aprobada cuando lo que se anuló fue una programada con vehículo asignado.
   */
  programada?: boolean;
  onCerrar(): void;
}): ReactElement {
  const cliente = useQueryClient();
  const [motivo, setMotivo] = useState<MotivoDeAnulacion | ''>('');
  const [comentario, setComentario] = useState('');

  const anulacion = useMutation({
    mutationFn: () =>
      (programada ? anularProgramada : anular)(
        expediente.id, 'Rolando Discua', motivo as MotivoDeAnulacion, comentario,
      ),
    onSuccess: async () => {
      avisar.exito(
        programada
          ? `${expediente.folio} anulada. El vehículo y el motorista quedaron libres, y el motivo suma al indicador de déficit.`
          : `${expediente.folio} anulada. El motivo suma al indicador de déficit de flota.`,
      );
      await cliente.invalidateQueries({ queryKey: ['cola-programacion'] });
      await cliente.invalidateQueries({ queryKey: ['cola-programadas'] });
      onCerrar();
    },
    onError: () => avisar.error('No se pudo anular. El expediente quedó como estaba.'),
  });

  return (
    <Modal
      abierto
      titulo={`Anular ${expediente.folio}`}
      descripcion={
        programada
          ? `La anulación no se deshace, y esta misión tiene vehículo y motorista tomados: quedarán libres. Si sólo quiere liberarlos, use «Devolver a la cola» — la misión sobrevive y no hay que pedirla de nuevo. Se notifica a ${laDependencia(expediente.dependencia)}.`
          : `La anulación no se deshace. El expediente sigue consultable con el filtro de caducadas, y se notifica a ${laDependencia(expediente.dependencia)}.`
      }
      destructivo
      onCerrar={onCerrar}
      // «Volver» sobraba: `Modal` ya rinde su propia salida. Dos botones que cierran, con
      // dos palabras distintas, es peor que dos idénticos — sugiere que hacen cosas
      // distintas. Venía de antes; se corrige acá porque es el mismo defecto.
      acciones={
        <Boton
          variante="peligro"
          disabled={!motivo || anulacion.isPending}
          cargando={anulacion.isPending}
          onClick={() => anulacion.mutate()}
        >
          Anular expediente
        </Boton>
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
                className="tw:mt-0.5 tw:size-4 tw:shrink-0 tw:accent-acento"
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
          <p className="tw:text-xs tw:text-tinta-mid">
            Seleccione el motivo del catálogo. El texto libre no produce indicador.
          </p>
        )}
      </div>
    </Modal>
  );
}
