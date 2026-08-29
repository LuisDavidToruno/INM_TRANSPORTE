import type { ReactElement, ReactNode } from 'react';
import { useMemo, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Link } from 'react-router';
import { CircleAlert, Truck } from 'lucide-react';

import {
  Boton,
  Campo,
  CampoBusqueda,
  Modal,
  Nota,
  Pastilla,
  Tabla,
  Vacio,
  avisar,
} from '../../ui';
import type { ColumnaDef, Tono } from '../../ui';
import { BloqueoDuro } from '../../api/misiones';
import { ESTADOS_DECLARABLES, declararEstado, padronDeFlota } from '../../api/flota';
import type { VehiculoDelPadron } from '../../api/flota';
import { soloFecha } from '../M06_Autorizacion/formato';

/**
 * `PT-072` — El padrón de flota.
 *
 * ── Las dos preguntas que se hacen al abrirlo ───────────────────────────────
 * **¿Con cuáles puedo contar?** y **¿quién responde por cada uno?** Por eso la tabla lleva
 * el estado operativo y el custodio, y no sólo la ficha técnica. Sin esas dos columnas la
 * pantalla es un catálogo de vehículos, no un padrón — y un catálogo no sirve para decidir
 * nada.
 *
 * ── Por qué esta pantalla es una tabla, y está bien que lo sea ──────────────
 * El dictamen de elementos visuales dice que la tabla es correcta en 23 de 138 pantallas, y
 * ésta es una de las 23: **compara elementos homogéneos por atributos homogéneos**, que es
 * exactamente para lo que sirve una tabla. Lo que le añade es la disponibilidad por
 * carriles, que vive en el cronograma de `PT-026` y `PT-038` y no se duplica acá.
 *
 * ── Lo que esta pantalla NO es ──────────────────────────────────────────────
 * **No es la ficha del vehículo.** El expediente completo —documentación con vencimientos,
 * mantenimiento, incidentes, especificaciones, custodios históricos— es lo que `CLAUDE.md`
 * llama *«entidad de primera clase con ciclo de vida completo»*, y necesita `M-04`, `M-11` y
 * `M-12`. Ninguno existe. Esto es el padrón y el estado operativo, que es lo que sí hay.
 */
export default function Padron(): ReactElement {
  const [filtro, setFiltro] = useState('');
  const [aDeclarar, setADeclarar] = useState<VehiculoDelPadron | null>(null);

  const { data, isPending, isError } = useQuery({
    queryKey: ['padron-flota'],
    queryFn: padronDeFlota,
  });

  const filas = useMemo(() => {
    const todos = data ?? [];
    const busqueda = filtro.trim().toLowerCase();
    if (!busqueda) return todos;

    // Se busca contra **lo que la fila muestra**, no contra los campos crudos. Quien teclea
    // «remolque» está buscando el caso `BE`, y esa palabra sólo existe en el texto derivado:
    // filtrar sólo por los campos del servidor le devuelve «ningún vehículo coincide» sobre
    // una tabla donde la palabra está escrita.
    return todos.filter((v) =>
      [
        v.siglas,
        v.placa ?? 'sin placa metálica',
        v.ficha.tipoDeVehiculo,
        v.estado ? TEXTO_DE_ESTADO[v.estado] ?? v.estado : 'sin declarar',
        v.custodio ?? 'sin custodio',
        v.ficha.llevaRemolque ? 'con remolque' : '',
      ]
        .join(' ')
        .toLowerCase()
        .includes(busqueda),
    );
  }, [data, filtro]);

  if (isError) {
    return (
      <Nota tono="riesgo" icono={<CircleAlert />}>
        No se pudo cargar el padrón de flota. Vuelva a intentar en unos segundos.
      </Nota>
    );
  }

  const sinEstado = (data ?? []).filter((v) => v.estado === null).length;
  const sinCustodio = (data ?? []).filter((v) => v.custodio === null).length;

  return (
    <div className="tw:flex tw:flex-col tw:gap-5">
      <header className="tw:flex tw:flex-col tw:gap-1">
        <h1 className="tw:text-xl tw:font-semibold tw:tracking-tight">Padrón de flota</h1>
        <p className="tw:text-sm tw:text-tinta-mid">
          {isPending
            ? 'Cargando la flota…'
            : `${data!.length} ${data!.length === 1 ? 'vehículo' : 'vehículos'} registrados.`}
        </p>
      </header>

      {/* Los dos huecos que impiden operar, dichos antes de que alguien lo descubra al
          intentar programar o despachar. No son advertencias decorativas: cada uno es un
          bloqueo duro esperando. */}
      {sinCustodio > 0 && (
        <Nota tono="riesgo" icono={<CircleAlert />}>
          {sinCustodio === 1
            ? '1 vehículo no tiene custodio vigente y no se puede despachar'
            : `${sinCustodio} vehículos no tienen custodio vigente y no se pueden despachar`}{' '}
          (<code className="tw:font-mono tw:text-xs">BD-13</code>). Un vehículo del Estado sin
          responsable identificado es un hallazgo esperando ocurrir.
        </Nota>
      )}

      {sinEstado > 0 && (
        <Nota tono="aviso">
          {sinEstado === 1
            ? '1 vehículo no tiene estado operativo declarado'
            : `${sinEstado} vehículos no tienen estado operativo declarado`}, y hoy{' '}
          <b>eso no los frena</b>:{' '}
          <code className="tw:font-mono tw:text-xs">BD-07</code> deja constancia de que no pudo
          evaluarse y la programación sigue. §10.2 cuenta el «alta reciente sin habilitar» entre
          las causas de <code className="tw:font-mono tw:text-xs">NO_DISPONIBLE</code>, así que
          declarar el estado es lo que cierra el hueco.
        </Nota>
      )}

      <CampoBusqueda
        etiqueta="Buscar por siglas, placa, tipo, estado o custodio…"
        valor={filtro}
        onCambio={setFiltro}
      />

      {!isPending && filas.length === 0 ? (
        <Vacio
          icono={<Truck />}
          titulo={filtro ? 'Ningún vehículo coincide' : 'No hay vehículos registrados'}
          descripcion={
            filtro
              ? 'Pruebe con las siglas completas, o limpie la búsqueda.'
              : 'El alta de vehículos todavía no tiene pantalla: hoy la flota se carga por la API.'
          }
        />
      ) : (
        <Tabla
          columnas={COLUMNAS(setADeclarar)}
          filas={filas}
          claveDe={(v) => v.id}
          cargando={isPending}
        />
      )}

      {aDeclarar && (
        <DialogoDeEstado vehiculo={aDeclarar} onCerrar={() => setADeclarar(null)} />
      )}
    </div>
  );
}

/**
 * El estado va primero, como la señal de la bandeja: es lo que decide si el vehículo entra
 * en la conversación, y se lee sin abrir nada.
 */
const COLUMNAS = (
  alDeclarar: (v: VehiculoDelPadron) => void,
): ColumnaDef<VehiculoDelPadron>[] => [
  {
    id: 'estado',
    cabecera: 'Estado',
    ancho: 150,
    celda: (v) => <EstadoVista estado={v.estado} />,
    ordenable: true,
    // Lo que no se puede usar, arriba. Y lo no declarado con lo inutilizable, porque
    // tampoco se puede usar.
    valorOrden: (v) => (v.estado === null ? 0 : PESO_DE_ESTADO[v.estado] ?? 1),
  },
  {
    id: 'siglas',
    cabecera: 'Siglas',
    ancho: 130,
    celda: (v) => (
      <div className="tw:flex tw:flex-col">
        {/* Las siglas son la entrada al expediente: es lo que se busca al mirar una fila, y
            un padrón sin puerta al detalle obliga a copiar el identificador a mano. */}
        <Link
          to={`/flota/${v.id}`}
          className="loki-foco tw:font-mono tw:text-[13px] tw:underline-offset-2 tw:hover:underline"
        >
          {v.siglas}
        </Link>
        {/* Sin placa metálica es estado VÁLIDO: hay desabastecimiento nacional. Se dice, no
            se deja en blanco como si faltara el dato. */}
        <span className="tw:text-xs tw:text-tinta-mid">
          {v.placa ?? <span className="tw:italic">sin placa metálica</span>}
        </span>
      </div>
    ),
    ordenable: true,
    valorOrden: (v) => v.siglas,
  },
  {
    id: 'ficha',
    cabecera: 'Qué es',
    celda: (v) => (
      <div className="tw:flex tw:flex-col">
        <span>{v.ficha.tipoDeVehiculo}</span>
        <span className="tw:text-xs tw:text-tinta-mid">
          {v.ficha.pesoBrutoKg.toLocaleString('es-HN')} kg · {v.ficha.capacidadPasajeros}{' '}
          pasajeros
          {v.ficha.llevaRemolque ? ' · con remolque' : ''}
        </span>
      </div>
    ),
  },
  {
    id: 'custodio',
    cabecera: 'Responde por él',
    ancho: 190,
    celda: (v) =>
      v.custodio ?? (
        // No es un dato que falta: es un bloqueo duro. `BD-13` impide despacharlo.
        <Pastilla tono="riesgo">Sin custodio · BD-13</Pastilla>
      ),
    ordenable: true,
    valorOrden: (v) => v.custodio ?? '',
  },
  {
    id: 'matricula',
    cabecera: 'Matrícula vence',
    ancho: 160,
    celda: (v) => (
      <span className="tw:tabular-nums tw:text-tinta-mid">{soloFecha(v.venceMatricula)}</span>
    ),
    ordenable: true,
    valorOrden: (v) => v.venceMatricula,
  },
  {
    id: 'accion',
    cabecera: '',
    ancho: 150,
    celda: (v) => (
      <Boton variante="secundario" tamano="sm" onClick={() => alDeclarar(v)}>
        Declarar estado
      </Boton>
    ),
  },
];

/** Lo inutilizable primero: es lo que hay que resolver. */
const PESO_DE_ESTADO: Record<string, number> = {
  DadoDeBaja: 0,
  RetiradoDeFlota: 0,
  EnTaller: 0,
  NoDisponible: 0,
  Prestado: 0,
  Asignado: 2,
  EnMision: 2,
  Disponible: 3,
};

/**
 * El texto del estado.
 *
 * El identificador del dominio es `EnTaller`, y así llega del servidor. **Nadie que abra un
 * padrón lee identificadores**: la pantalla dice «En taller», y la comparación sigue siendo
 * contra el identificador, nunca contra el texto.
 */
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

/**
 * El estado, con su tono.
 *
 * <b>Se decide por identificador y nunca por el texto</b>, que es la regla del sistema de
 * diseño: «No disponible» contiene «disponible», y una comparación de cadenas lo pintaría
 * verde — justo al revés de lo que dice.
 */
function EstadoVista({ estado }: { estado: string | null }): ReactElement {
  if (estado === null) {
    // «Sin declarar» y «disponible» son cosas opuestas, y ésta es la que más fácil se
    // confunde: un renglón en blanco se lee como «nada que reportar».
    return <Pastilla tono="neutro">Sin declarar</Pastilla>;
  }

  // Si el servidor agrega un estado que esta pantalla no conoce, se muestra el identificador
  // crudo antes que ocultarlo: un estado sin traducir se ve raro, uno escondido se ve verde.
  return (
    <Pastilla tono={TONO_DE_ESTADO[estado] ?? 'neutro'}>
      {TEXTO_DE_ESTADO[estado] ?? estado}
    </Pastilla>
  );
}

/**
 * Declarar el estado operativo — §10.2.
 *
 * ── Los terminales se separan del resto ─────────────────────────────────────
 * Porque de ellos **no se vuelve**, y presentarlos en la misma lista que «en taller» invita
 * a elegirlos con el mismo cuidado que se elige un taller. El descargo de un bien del Estado
 * no es un cambio de estado más.
 */
function DialogoDeEstado({
  vehiculo,
  onCerrar,
}: {
  vehiculo: VehiculoDelPadron;
  onCerrar(): void;
}): ReactElement {
  const cliente = useQueryClient();
  const [estado, setEstado] = useState('');
  const [motivo, setMotivo] = useState('');

  const esTerminal = ESTADOS_DECLARABLES.find((e) => e.valor === estado)?.terminal ?? false;

  const operacion = useMutation({
    mutationFn: () => declararEstado(vehiculo.id, 'Rolando Discua', estado, motivo),
    onSuccess: async (resultado) => {
      // La advertencia gana al «quedó listo». Cuando el servidor no pudo verificar que el
      // terminal corresponda —porque el vehículo no tiene título de tenencia— el asiento se
      // hizo igual, y quien lo hizo tiene derecho a saber que `HB3-17` no lo juzgó.
      if (resultado.advertencia) {
        // Dura más que un éxito y no menos: quien acaba de dar de baja un vehículo tiene que
        // alcanzar a leer que el sistema no verificó si ese era el terminal correcto.
        avisar.alerta(resultado.advertencia, {
          duracion: 15_000,
          detalle: 'Se registró igual. Declare el título de tenencia para que se verifique.',
        });
      } else {
        avisar.exito(
          `${vehiculo.siglas} quedó ${(TEXTO_DE_ESTADO[estado] ?? estado).toLowerCase()}.`,
        );
      }

      await cliente.invalidateQueries({ queryKey: ['padron-flota'] });
      await cliente.invalidateQueries({ queryKey: ['cobertura-titulos'] });
      onCerrar();
    },
    onError: (e) => {
      // El servidor rechaza por §10.2 —terminal con misiones abiertas, salir de un
      // terminal, declarar uno automático— y el motivo es lo único que dice cuál fue.
      if (e instanceof BloqueoDuro) {
        avisar.error(e.message);
        return;
      }
      avisar.error('No se pudo declarar el estado. El vehículo quedó como estaba.');
    },
  });

  return (
    <Modal
      abierto
      titulo={`Declarar el estado de ${vehiculo.siglas}`}
      descripcion={
        vehiculo.estado === null
          ? 'Este vehículo nunca tuvo estado declarado, así que no está disponible para programar.'
          : `Hoy está ${(TEXTO_DE_ESTADO[vehiculo.estado] ?? vehiculo.estado).toLowerCase()}. El cambio queda en el historial con su motivo y su autor.`
      }
      destructivo={esTerminal}
      onCerrar={onCerrar}
      acciones={
        <Boton
          variante={esTerminal ? 'peligro' : 'primario'}
          disabled={!estado || motivo.trim().length < 8 || operacion.isPending}
          cargando={operacion.isPending}
          onClick={() => operacion.mutate()}
        >
          {esTerminal ? 'Retirar de la flota' : 'Declarar estado'}
        </Boton>
      }
    >
      <div className="tw:flex tw:flex-col tw:gap-4">
        <fieldset className="tw:flex tw:flex-col tw:gap-2">
          <legend className="tw:mb-1 tw:text-sm tw:font-medium">Estado</legend>

          {ESTADOS_DECLARABLES.filter((e) => !e.terminal).map((e) => (
            <Opcion key={e.valor} opcion={e} elegido={estado} onElegir={setEstado} />
          ))}
        </fieldset>

        <fieldset className="tw:flex tw:flex-col tw:gap-2 tw:rounded tw:border tw:border-riesgo-bd tw:p-3">
          <legend className="tw:px-1 tw:text-sm tw:font-medium">Salida definitiva de la flota</legend>
          <p className="tw:text-xs tw:text-tinta-mid">
            De estos dos <b>no se vuelve</b>, y no son lo mismo: el descargo extingue un bien
            propio; el retiro devuelve uno que nunca lo fue. Declarar descargado un vehículo en
            comodato es un asiento falso sobre un bien ajeno.
          </p>

          {ESTADOS_DECLARABLES.filter((e) => e.terminal).map((e) => (
            <Opcion key={e.valor} opcion={e} elegido={estado} onElegir={setEstado} />
          ))}
        </fieldset>

        <Campo
          etiqueta="Motivo"
          obligatorio
          ayuda="Causa tipificada, número de acta, o la explicación. Lo lee quien pregunte dentro de dos años por qué este vehículo no estuvo disponible."
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

        {/* Lo que el servidor va a rechazar, dicho antes de intentarlo. */}
        {esTerminal && (
          <Nota tono="aviso">
            Si el vehículo tiene misiones sin cerrar, el sistema no lo va a permitir: un
            expediente vivo colgando de un bien que ya no figura en el registro es un hallazgo
            que nadie puede explicar después.
          </Nota>
        )}
      </div>
    </Modal>
  );
}

function Opcion({
  opcion,
  elegido,
  onElegir,
}: {
  opcion: { valor: string; texto: string };
  elegido: string;
  onElegir(v: string): void;
}): ReactElement {
  return (
    <label className="tw:flex tw:cursor-pointer tw:items-start tw:gap-2 tw:text-sm">
      <input
        type="radio"
        name="estado-operativo"
        checked={elegido === opcion.valor}
        onChange={() => onElegir(opcion.valor)}
        className="tw:mt-0.5 tw:size-4 tw:shrink-0 tw:accent-acento"
      />
      <span>{opcion.texto}</span>
    </label>
  );
}

/** Sin uso fuera de este archivo; existe para que el tipo del nodo quede explícito. */
export type NodoDeFlota = ReactNode;
