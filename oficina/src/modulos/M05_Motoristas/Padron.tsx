import type { ReactElement } from 'react';
import { useMemo, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { CalendarClock, CircleAlert, IdCard, ShieldAlert } from 'lucide-react';

import { CampoBusqueda, Nota, Pastilla, Tabla, Vacio } from '../../ui';
import type { ColumnaDef, Tono } from '../../ui';
import { motoristas } from '../../api/motoristas';
import type { Motorista } from '../../api/motoristas';
import { soloFecha } from '../M06_Autorizacion/formato';

/**
 * `PT-082` y `PT-085` — El padrón de motoristas con su habilitación vigente.
 *
 * ── Por qué las dos en una pantalla ─────────────────────────────────────────
 * `PT-082` pide el padrón *«con su habilitación vigente»* y `PT-085` pide *«vigencia de la
 * habilitación y alertas anticipadas»*. Son la misma tabla mirada dos veces: un padrón que no
 * dice cuándo vence cada licencia obliga a abrir uno por uno para saber con quién se puede
 * contar, que es justo lo que la pantalla existe para evitar.
 *
 * ── Lo que decide de verdad ─────────────────────────────────────────────────
 * **Una licencia vencida es bloqueo duro** (`BD-02`), y traslada responsabilidad legal directa a
 * quien autorizó. Por eso el vencimiento va en la fila y no en el detalle, y por eso se avisa
 * antes: la licencia que vence dentro de la ventana de una misión ya la impide, aunque hoy
 * todavía esté vigente.
 *
 * ── Lo que esta pantalla NO muestra ─────────────────────────────────────────
 * **Cuál es la restricción médica.** `RN-52`: quien despacha ve *que* hay restricción, no el
 * diagnóstico. El dato clínico es del expediente de Talento Humano y no entra acá.
 */
export default function PadronDeMotoristas(): ReactElement {
  const [filtro, setFiltro] = useState('');

  const { data, isPending, isError } = useQuery({
    queryKey: ['padron-motoristas'],
    queryFn: motoristas,
  });

  const hoy = useMemo(() => new Date().toISOString().slice(0, 10), []);

  const filas = useMemo(() => {
    const todos = data ?? [];
    const busqueda = filtro.trim().toLowerCase();
    if (!busqueda) return todos;

    return todos.filter((m) =>
      [
        m.nombre,
        m.licencia.numero,
        m.licencia.categoria,
        m.esDelPadron ? 'del padrón' : 'fuera del padrón',
        m.licencia.tieneRestricciones ? 'con restricciones' : '',
        situacion(m, hoy).texto,
      ]
        .join(' ')
        .toLowerCase()
        .includes(busqueda),
    );
  }, [data, filtro, hoy]);

  if (isError) {
    return (
      <Nota tono="riesgo" icono={<CircleAlert />}>
        No se pudo cargar el padrón de motoristas. Vuelva a intentar en unos segundos.
      </Nota>
    );
  }

  const todos = data ?? [];
  const vencidas = todos.filter((m) => situacion(m, hoy).clase === 'vencida');
  const porVencer = todos.filter((m) => situacion(m, hoy).clase === 'por-vencer');
  const conRestriccion = todos.filter((m) => m.licencia.tieneRestricciones);

  return (
    <div className="tw:flex tw:flex-col tw:gap-5">
      <header className="tw:flex tw:flex-col tw:gap-1">
        <h1 className="tw:text-xl tw:font-semibold tw:tracking-tight">Padrón de motoristas</h1>
        <p className="tw:text-sm tw:text-tinta-mid">
          {isPending
            ? 'Cargando el padrón…'
            : `${todos.length} ${todos.length === 1 ? 'persona habilitada' : 'personas habilitadas'} para conducir.`}
        </p>
      </header>

      {/* La vencida no es un aviso: es un bloqueo duro esperando, y traslada responsabilidad. */}
      {vencidas.length > 0 && (
        <Nota tono="riesgo" icono={<CircleAlert />}>
          <b>
            {vencidas.length === 1
              ? '1 licencia está vencida'
              : `${vencidas.length} licencias están vencidas`}
          </b>
          . Asignar a esa persona es bloqueo duro (
          <code className="tw:font-mono tw:text-xs">BD-02</code>) y{' '}
          <b>traslada responsabilidad directa a quien autorice</b>.
        </Nota>
      )}

      {porVencer.length > 0 && (
        <Nota tono="aviso" icono={<CalendarClock />}>
          {porVencer.length === 1
            ? '1 licencia vence dentro de 60 días'
            : `${porVencer.length} licencias vencen dentro de 60 días`}
          . El bloqueo <b>no espera al vencimiento</b>: la licencia tiene que cubrir la ventana
          completa de la misión, holgura incluida, así que una misión que vuelve después de esa
          fecha ya no se puede programar con esa persona.
        </Nota>
      )}

      {conRestriccion.length > 0 && (
        <Nota tono="info" icono={<ShieldAlert />}>
          {conRestriccion.length === 1
            ? '1 persona tiene restricciones en su licencia'
            : `${conRestriccion.length} personas tienen restricciones en su licencia`}
          . <b>Cuál es, no se muestra</b> —`RN-52`—: quien despacha ve que la hay, no el
          diagnóstico. Si la restricción choca con las condiciones de una misión concreta, la
          asignación lo dice ahí (<code className="tw:font-mono tw:text-xs">BD-12</code>), y{' '}
          <b>exige acuse en vez de bloquear</b>.
        </Nota>
      )}

      <CampoBusqueda
        etiqueta="Buscar por nombre, licencia o categoría…"
        valor={filtro}
        onCambio={setFiltro}
      />

      {!isPending && filas.length === 0 ? (
        <Vacio
          icono={<IdCard />}
          titulo={filtro ? 'Ninguna persona coincide' : 'No hay motoristas registrados'}
          descripcion={
            filtro
              ? 'Pruebe con el nombre completo, o limpie la búsqueda.'
              : 'El alta de motoristas todavía no tiene pantalla: hoy el padrón se carga por la API.'
          }
        />
      ) : (
        <Tabla
          columnas={COLUMNAS(hoy)}
          filas={filas}
          claveDe={(m) => m.id}
          cargando={isPending}
        />
      )}
    </div>
  );
}

/**
 * Sesenta días.
 *
 * **No es parámetro con vigencia y no debe serlo**: no decide ningún número que alguien vaya a
 * cobrar ni bloquea nada. Es cuándo esta pantalla empieza a avisar; el bloqueo real lo pone la
 * fecha de la licencia contra la ventana de cada misión.
 */
const DIAS_DE_AVISO = 60;

function situacion(
  m: Motorista,
  hoy: string,
): { clase: 'vencida' | 'por-vencer' | 'vigente'; texto: string; tono: Tono; dias: number } {
  const dias = Math.round(
    (Date.parse(`${m.licencia.vencimiento}T00:00:00Z`) - Date.parse(`${hoy}T00:00:00Z`)) / 86_400_000,
  );

  if (dias < 0) {
    return { clase: 'vencida', texto: 'Licencia vencida', tono: 'riesgo', dias };
  }

  if (dias <= DIAS_DE_AVISO) {
    return { clase: 'por-vencer', texto: `Vence en ${dias} días`, tono: 'aviso', dias };
  }

  return { clase: 'vigente', texto: 'Habilitado', tono: 'ok', dias };
}

/** Lo que decide si se puede contar con la persona va primero, como en el padrón de flota. */
const COLUMNAS = (hoy: string): ColumnaDef<Motorista>[] => [
  {
    id: 'situacion',
    cabecera: 'Habilitación',
    ancho: 165,
    celda: (m) => {
      const s = situacion(m, hoy);
      return <Pastilla tono={s.tono}>{s.texto}</Pastilla>;
    },
    ordenable: true,
    // Lo vencido arriba: es lo que hay que resolver antes de programar nada.
    valorOrden: (m) => situacion(m, hoy).dias,
  },
  {
    id: 'nombre',
    cabecera: 'Quién',
    celda: (m) => (
      <div className="tw:flex tw:flex-col">
        <span>{m.nombre}</span>
        {/* «Fuera del padrón» NO es irregular: `RN-57` verifica sobre quien efectivamente
            conduce, sea o no motorista de planta. Se dice para que no se lea como un hueco. */}
        {!m.esDelPadron && (
          <span className="tw:text-xs tw:text-tinta-mid">
            no es motorista de planta · se verifica igual
          </span>
        )}
      </div>
    ),
    ordenable: true,
    valorOrden: (m) => m.nombre,
  },
  {
    id: 'licencia',
    cabecera: 'Licencia',
    ancho: 190,
    celda: (m) => (
      <div className="tw:flex tw:flex-col">
        <span className="tw:font-mono tw:text-[13px]">{m.licencia.numero}</span>
        <span className="tw:text-xs tw:text-tinta-mid">
          categoría {m.licencia.categoria}
        </span>
      </div>
    ),
    ordenable: true,
    valorOrden: (m) => m.licencia.categoria,
  },
  {
    id: 'vence',
    cabecera: 'Vence',
    ancho: 150,
    celda: (m) => (
      <span className="tw:tabular-nums tw:text-tinta-mid">
        {soloFecha(m.licencia.vencimiento)}
      </span>
    ),
    ordenable: true,
    valorOrden: (m) => m.licencia.vencimiento,
  },
  {
    id: 'restricciones',
    cabecera: 'Restricciones',
    ancho: 160,
    celda: (m) =>
      m.licencia.tieneRestricciones ? (
        // Qué restricción es, NO se dice. `RN-52`.
        <Pastilla tono="aviso">Tiene, sin detallar</Pastilla>
      ) : (
        <span className="tw:text-tinta-mid">—</span>
      ),
    ordenable: true,
    valorOrden: (m) => (m.licencia.tieneRestricciones ? 0 : 1),
  },
];
