import type { ReactElement } from 'react';
import { CircleAlert, FileCheck2 } from 'lucide-react';
import { useQuery } from '@tanstack/react-query';

import { Enlace, Nota, Tabla, Vacio } from '../../ui';
import type { ColumnaDef } from '../../ui';
import { colaDeCierre } from '../../api/misiones';
import type { Expediente } from '../../dominio/mision';
import { diaYHora, soloFecha } from '../M06_Autorizacion/formato';

/**
 * Cola de cierre — los expedientes en `LIQUIDADA`.
 *
 * ── Por qué esta cola importa más de lo que parece ───────────────────────────
 * Un expediente liquidado y sin cerrar **no está terminado**, y `RN-97` lo dice
 * con consecuencia: lo no terminal al corte constituye el **saldo de apertura**
 * del ejercicio siguiente, con su antigüedad contada desde el hecho. Una cola
 * que nadie mira no se queda quieta — se arrastra al año próximo y llega al
 * auditor con meses encima.
 *
 * ── Lo que esta pantalla NO hace ─────────────────────────────────────────────
 * No decide si el cierre lleva hallazgo. Eso lo resuelve el servidor con los
 * criterios `H-nn`, y quien cierra lo confirma. Acá solo se ve quién espera.
 */
export default function Cola(): ReactElement {
  const { data, isPending, isError, error } = useQuery({
    queryKey: ['cola-cierre'],
    queryFn: colaDeCierre,
  });

  if (isError) {
    return (
      <Nota tono="riesgo" icono={<CircleAlert />}>
        No se pudo cargar la cola de cierre:{' '}
        {error instanceof Error ? error.message : 'error desconocido'}.
      </Nota>
    );
  }

  const filas = data ?? [];

  return (
    <div className="tw:flex tw:flex-col tw:gap-5">
      <header className="tw:flex tw:flex-col tw:gap-1">
        <h1 className="tw:text-xl tw:font-semibold tw:tracking-tight">Cola de cierre</h1>
        <p className="tw:text-sm tw:text-[var(--txt-2)]">
          Expedientes liquidados esperando el cierre de la Gerencia Administrativa.
        </p>
      </header>

      {filas.length > 0 && (
        <Nota tono="aviso">
          Un expediente liquidado y sin cerrar <b>no está terminado</b>. Lo que quede así al
          corte del ejercicio pasa al <b>saldo de apertura</b> del siguiente, con su antigüedad
          contada desde el hecho — no desde hoy.
        </Nota>
      )}

      {!isPending && filas.length === 0 ? (
        <Vacio
          icono={<FileCheck2 />}
          titulo="Nada esperando cierre"
          descripcion="Cuando una misión se liquide, aparecerá acá para que la Gerencia Administrativa la cierre."
        />
      ) : (
        <Tabla columnas={COLUMNAS} filas={filas} claveDe={(e) => e.id} cargando={isPending} />
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
      <Enlace href={`/cierre/${e.id}`}>
        <span className="tw:font-mono tw:text-[13px] tw:tabular-nums">{e.folio}</span>
      </Enlace>
    ),
    ordenable: true,
    valorOrden: (e) => e.folio,
  },
  {
    id: 'dependencia',
    cabecera: 'Dependencia',
    celda: (e) => e.dependencia,
    ordenable: true,
    valorOrden: (e) => e.dependencia,
  },
  {
    id: 'objeto',
    cabecera: 'Objeto del traslado',
    celda: (e) => e.objetoDelTraslado,
  },
  {
    id: 'retorno',
    cabecera: 'Retornó',
    ancho: 168,
    celda: (e) => soloFecha(e.retornoPrevisto),
    ordenable: true,
    valorOrden: (e) => e.retornoPrevisto,
  },
  {
    id: 'liquidada',
    cabecera: 'Liquidada',
    ancho: 168,
    // La antigüedad desde la liquidación es el dato que decide la prioridad de esta cola.
    celda: (e) => {
      const liquidacion = e.diario.filter((t) => t.id === 'T-19').at(-1);
      return liquidacion ? diaYHora(liquidacion.momento) : '—';
    },
  },
];
