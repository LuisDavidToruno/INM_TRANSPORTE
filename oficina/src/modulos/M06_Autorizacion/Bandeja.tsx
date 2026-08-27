import type { ReactElement, ReactNode } from 'react';
import { useMemo, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { ClipboardCheck, CircleAlert, ShieldBan, TriangleAlert } from "lucide-react";

import { CampoBusqueda, Enlace, Nota, Pastilla, Tabla, Vacio } from "../../ui";
import type { ColumnaDef } from '../../ui';
import { antiguedadDelEspejo, bandejaDeAutorizacion } from '../../api/misiones';
import type { AntiguedadDelEspejo } from '../../api/misiones';
import { advertencias, hayBloqueo } from '../../dominio/mision';
import type { Expediente } from '../../dominio/mision';
import { diaYHora, faltanDias } from './formato';

/**
 * `PT-013` — Bandeja de autorización.
 *
 * ── Por qué las validaciones se ven en la LISTA y no sólo al abrir ───────────
 * `HU-009`: «quiero ver todas las validaciones antes de pronunciarme, para no
 * autorizar lo que no puedo verificar». Si el bloqueo aparece recién al abrir,
 * la jefatura recorre tres expedientes para descubrir que el primero no era
 * suyo. La columna de señal es lo que permite barrer y saber a qué entrar.
 *
 * ── Lo que NO hace, y es deliberado ──────────────────────────────────────────
 * No oculta ni deshabilita la fila con advertencia. `RN-50` prohíbe bloquear por
 * antigüedad del espejo — una delegación con cuatro días sin enlace tiene que
 * poder operar —, y ese fue el hallazgo `HB34-03`. La advertencia se ve; la
 * decisión sigue siendo de la jefatura.
 */
export default function Bandeja(): ReactElement {
  const [filtro, setFiltro] = useState('');

  const { data, isPending, isError, error } = useQuery({
    queryKey: ['bandeja-autorizacion'],
    queryFn: bandejaDeAutorizacion,
  });

  // Consulta aparte a propósito. Si la integración del organigrama está caída, la bandeja
  // tiene que cargar igual: `RN-50` advierte, no bloquea, y una cabecera que no se puede
  // pintar no puede llevarse por delante los expedientes que sí llegaron.
  const espejo = useQuery({
    queryKey: ['antiguedad-del-espejo'],
    queryFn: antiguedadDelEspejo,
  });

  const filas = useMemo(() => {
    if (!data) return [];
    const busqueda = filtro.trim().toLowerCase();
    if (!busqueda) return data;
    return data.filter((e) =>
      [e.folio, e.solicitanteDeDerecho, e.dependencia, e.destino]
        .join(' ')
        .toLowerCase()
        .includes(busqueda),
    );
  }, [data, filtro]);

  if (isError) {
    return (
      <Nota tono="riesgo" icono={<CircleAlert />}>
        No se pudo cargar la bandeja: {error instanceof Error ? error.message : 'error desconocido'}.
        Los expedientes siguen donde estaban; nada se perdió. Vuelva a intentar en unos segundos.
      </Nota>
    );
  }

  return (
    <div className="tw:flex tw:flex-col tw:gap-5">
      <Encabezado pendientes={data?.length ?? 0} cargando={isPending} />

      <EstadoDelEspejo dato={espejo.data ?? null} fallo={espejo.isError} />

      <CampoBusqueda
        etiqueta="Buscar por folio, solicitante, dependencia o destino"
        valor={filtro}
        onCambio={setFiltro}
      />

      {!isPending && filas.length === 0 ? (
        <Vacio
          icono={<ClipboardCheck />}
          titulo={filtro ? 'Ningún expediente coincide' : 'No hay nada esperando su pronunciamiento'}
          descripcion={
            filtro
              ? 'Pruebe con el folio completo, o limpie la búsqueda para ver los tres pendientes.'
              : 'Cuando una dependencia bajo su competencia envíe una solicitud, aparecerá acá.'
          }
        />
      ) : (
        <Tabla
          columnas={COLUMNAS}
          filas={filas}
          claveDe={(e) => e.id}
          cargando={isPending}
        />
      )}
    </div>
  );
}

function Encabezado({ pendientes, cargando }: { pendientes: number; cargando: boolean }): ReactElement {
  return (
    <header className="tw:flex tw:flex-col tw:gap-1">
      <h1 className="tw:text-xl tw:font-semibold tw:tracking-tight">Bandeja de autorización</h1>
      <p className="tw:text-sm tw:text-[var(--txt-2)]">
        {cargando
          ? 'Cargando los expedientes bajo su competencia…'
          : pendientes === 1
            ? '1 expediente espera su pronunciamiento.'
            : `${pendientes} expedientes esperan su pronunciamiento.`}
      </p>
    </header>
  );
}

/**
 * Desde cuándo no se confirma el organigrama contra ARGOS — `HU-009`, `RN-50`.
 *
 * ── Por qué no hay umbral en este código ─────────────────────────────────────
 * Porque no está decidido. `RN-50` marca `umbral_advertencia_desincronizacion` como
 * <b>`[C]`, por confirmar con el PO y con Talento Humano</b>, y cablear acá un «más de
 * siete días» sería inventar la norma en la capa que menos autoridad tiene para hacerlo.
 * La pantalla dice el número; <b>quién lo considera demasiado es de la regla</b>, y
 * cuando la regla tenga umbral, esta cabecera lo aplicará.
 *
 * ── El único caso que sí se puede juzgar sin umbral ──────────────────────────
 * Que <b>nunca</b> se haya confirmado. Ahí no hay antigüedad que comparar contra nada:
 * no hay espejo. Por eso ese caso sube de tono y los demás no.
 *
 * Y en ningún caso se impide autorizar: `HB1-10` corrigió justamente eso — bloquear por
 * un problema de integración paraliza a la institución, que es el fallo que no se quiere.
 */
function EstadoDelEspejo({
  dato,
  fallo,
}: {
  dato: AntiguedadDelEspejo | null;
  fallo: boolean;
}): ReactElement | null {
  // No saber y saber que está fresco son cosas distintas: callar en el primer caso
  // dejaría a la jefatura creyendo que el organigrama sí se verificó.
  if (fallo) {
    return (
      <Nota tono="aviso" icono={<TriangleAlert />}>
        No se pudo consultar desde cuándo se confirma el organigrama. Los expedientes de
        abajo son los suyos, pero <b>la competencia con que se listaron no se verificó</b>.
      </Nota>
    );
  }

  if (dato === null) return null;

  if (dato.nuncaConfirmado) {
    return (
      <Nota tono="riesgo" icono={<CircleAlert />}>
        <b>El organigrama nunca se ha confirmado contra ARGOS.</b> Puede autorizar —el
        sistema no se lo impide—, pero la jerarquía con que se armó esta bandeja no está
        respaldada por el sistema que es su dueño.
      </Nota>
    );
  }

  const dias = dato.diasSinConfirmar ?? 0;

  return (
    <p className="tw:text-xs tw:text-[var(--txt-2)]">
      Organigrama confirmado contra ARGOS{' '}
      {dias === 0 ? 'hoy' : dias === 1 ? 'ayer' : `hace ${dias} días`}.
    </p>
  );
}

/**
 * La columna de señal va primero a propósito: es la que decide si el expediente
 * se puede resolver o hay que escalarlo, y se lee sin abrir nada.
 */
const COLUMNAS: ColumnaDef<Expediente>[] = [
  {
    id: 'senal',
    cabecera: 'Señal',
    ancho: 132,
    celda: (e) => <Senal expediente={e} />,
    ordenable: true,
    valorOrden: (e) => (hayBloqueo(e.validaciones) ? 0 : advertencias(e.validaciones).length ? 1 : 2),
  },
  {
    id: 'folio',
    cabecera: 'Folio',
    ancho: 148,
    // El folio es el enlace, no la fila entera: así el recorrido por teclado
    // llega al expediente sin inventar un manejador de tecla sobre un <tr>.
    celda: (e) => (
      <Enlace href={`/autorizacion/${e.id}`}>
        <span className="tw:font-mono tw:text-[13px] tw:tabular-nums">{e.folio}</span>
      </Enlace>
    ),
    ordenable: true,
    valorOrden: (e) => e.folio,
  },
  {
    id: 'solicitante',
    cabecera: 'Solicitante de derecho',
    celda: (e) => (
      <div className="tw:flex tw:flex-col">
        <span>{e.solicitanteDeDerecho}</span>
        <span className="tw:text-xs tw:text-[var(--txt-2)]">{e.dependencia}</span>
      </div>
    ),
    ordenable: true,
    valorOrden: (e) => e.solicitanteDeDerecho,
  },
  {
    id: 'objeto',
    cabecera: 'Qué se moviliza',
    celda: (e) => (
      <div className="tw:flex tw:flex-col">
        {/* Un campo vacío se dice, no se deja en blanco: «Destino:» a secas parece
            un error de la pantalla, y esconde que el dato falta en el expediente. */}
        <span className="tw:line-clamp-1">
          {e.objetoDelTraslado || <SinDato>Sin objeto declarado</SinDato>}
        </span>
        <span className="tw:text-xs tw:text-[var(--txt-2)]">
          {e.destino ? `Destino: ${e.destino}` : <SinDato>Sin destino declarado</SinDato>}
        </span>
      </div>
    ),
  },
  {
    id: 'salida',
    cabecera: 'Salida prevista',
    ancho: 180,
    celda: (e) => (
      <div className="tw:flex tw:flex-col">
        <span className="tw:tabular-nums">{diaYHora(e.salidaPrevista)}</span>
        <span className="tw:text-xs tw:text-[var(--txt-2)]">{faltanDias(e.salidaPrevista)}</span>
      </div>
    ),
    ordenable: true,
    valorOrden: (e) => e.salidaPrevista,
  },
];

/**
 * Un dato que el expediente no trae.
 *
 * En cursiva y con el tono secundario para que se lea como <b>ausencia declarada</b>
 * y no como valor. Un guion o un espacio en blanco dejan al usuario preguntándose si
 * el sistema perdió el dato o si nunca lo hubo.
 */
function SinDato({ children }: { children: ReactNode }): ReactElement {
  return <span className="tw:italic tw:text-[var(--txt-2)]">{children}</span>;
}

/**
 * Bloqueo y advertencia no son grados de lo mismo, y por eso no comparten forma.
 *
 * El bloqueo lleva un icono de prohibición: la acción no existe para este usuario.
 * La advertencia lleva triángulo y **cuenta cuántas son**, porque dos advertencias
 * distintas exigen dos acuses distintos y conviene saberlo antes de entrar.
 */
function Senal({ expediente }: { expediente: Expediente }): ReactElement {
  if (hayBloqueo(expediente.validaciones)) {
    return (
      <Pastilla tono="riesgo">
        <ShieldBan size={13} aria-hidden />
        Bloqueada
      </Pastilla>
    );
  }

  const cuantas = advertencias(expediente.validaciones).length;
  if (cuantas > 0) {
    return (
      <Pastilla tono="aviso">
        <TriangleAlert size={13} aria-hidden />
        {cuantas === 1 ? '1 aviso' : `${cuantas} avisos`}
      </Pastilla>
    );
  }

  return <Pastilla tono="ok">Sin reparos</Pastilla>;
}
