import type { ReactElement } from 'react';
import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { CircleAlert, Eye, Scale } from 'lucide-react';

import { CampoBusqueda, Nota, Panel, Pastilla, Vacio } from '../../ui';
import type { Tono } from '../../ui';
import { pedir } from '../../api/misiones';
import { diaYHora } from '../M06_Autorizacion/formato';

/**
 * `PT-133` — Reporte de accesos a manifiestos y patrón anómalo.
 *
 * ── Por qué existe este registro ────────────────────────────────────────────
 * El hábeas data del Artículo 182 está vigente y sólo el titular puede interponerlo. `RN-52`:
 * <i>«Si una persona pregunta quién accedió a sus datos, <b>la única respuesta defendible es el
 * registro de consultas. Sin él, la institución no puede afirmar nada</b>»</i>.
 *
 * Y no poder afirmar nada no es quedar en empate: es no poder demostrar que <b>no</b> hubo
 * acceso indebido.
 *
 * ── Nadie está exento ───────────────────────────────────────────────────────
 * <i>«Ningún rol, <b>incluido el Administrador del Sistema</b>, debe poder consultar estos datos
 * sin dejar rastro.»</i> El administrador es justamente quien podría borrar su propio rastro, y
 * por eso el registro es inmutable y lo lee el Auditor Interno.
 */
export default function Accesos(): ReactElement {
  const [registro, setRegistro] = useState('');

  const { data, isPending, isError } = useQuery({
    queryKey: ['accesos', registro],
    queryFn: () =>
      pedir<Reporte>(
        `/personas-externas/accesos${registro ? `?registro=${encodeURIComponent(registro)}` : ''}`,
      ),
  });

  if (isError) {
    return (
      <Nota tono="riesgo" icono={<CircleAlert />}>
        No se pudo cargar el reporte de accesos.
      </Nota>
    );
  }

  const marcados = data?.patrones.filter((p) => p.marcado) ?? [];

  return (
    <div className="tw:flex tw:flex-col tw:gap-5">
      <header className="tw:flex tw:flex-col tw:gap-1">
        <h1 className="tw:text-xl tw:font-semibold tw:tracking-tight">
          Quién vio datos de personas trasladadas
        </h1>
        <p className="tw:text-sm tw:text-tinta-mid">
          Cada acceso deja asiento, <b>sin excepción de rol</b>. Es lo que la institución puede
          responder si alguien pregunta quién vio sus datos.
        </p>
      </header>

      <CampoBusqueda
        etiqueta="Filtrar por expediente — para responder «quién vio lo mío»…"
        valor={registro}
        onCambio={setRegistro}
      />

      {isPending ? (
        <p className="tw:text-sm tw:text-tinta-mid">Cargando…</p>
      ) : (
        <>
          {/* ⚠️ La cifra que dice cuánto del registro NO se puede auditar. Va arriba: sin
              ella, un reporte lleno de accesos se lee como un control que funciona. */}
          {data.sinNecesidadDeclarada > 0 && (
            <Nota tono="aviso">
              <b>
                {data.sinNecesidadDeclarada} de {data.total} accesos no dicen para qué
              </b>
              . Queda el rastro de quién miró y <b>ninguna forma de juzgar si debía</b>. Esa parte
              del registro no se puede auditar.
            </Nota>
          )}

          {marcados.length > 0 && (
            <Nota tono="aviso" icono={<Eye />}>
              <b>
                {marcados.length === 1
                  ? '1 persona consultó bastante más que el resto'
                  : `${marcados.length} personas consultaron bastante más que el resto`}
              </b>
              . <b>No significa que hicieran algo malo</b> — un despachador abre muchos
              manifiestos un lunes. Significa que vale preguntar.
            </Nota>
          )}

          {data.patrones.length > 0 && (
            <Panel titulo="Por persona">
              <ul className="tw:flex tw:flex-col tw:gap-2">
                {data.patrones.map((p) => (
                  <li
                    key={p.consultante}
                    className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-3 tw:border-l-2 tw:border-linea tw:py-1 tw:pl-3 tw:text-sm"
                  >
                    <span className="tw:font-medium">{p.consultante}</span>
                    <span className="tw:tabular-nums tw:text-tinta-mid">
                      {p.consultas} consulta(s) sobre {p.registrosDistintos} expediente(s)
                    </span>
                    {p.marcado && <Pastilla tono="aviso">conviene preguntar</Pastilla>}
                    {p.sinNecesidadDeclarada > 0 && (
                      <Pastilla tono="neutro">
                        {p.sinNecesidadDeclarada} sin decir para qué
                      </Pastilla>
                    )}
                  </li>
                ))}
              </ul>
            </Panel>
          )}

          {data.accesos.length === 0 ? (
            <Vacio
              icono={<Scale />}
              titulo={
                registro === ''
                  ? 'Nadie ha consultado datos de personas trasladadas'
                  : 'Nadie ha consultado ese expediente'
              }
              descripcion="Si alguien lo hubiera hecho, aparecería acá con su nombre, su rol y la hora — no hay forma de mirar sin dejar asiento."
            />
          ) : (
            <Panel titulo={`${data.accesos.length} acceso(s)`}>
              <ul className="tw:flex tw:flex-col tw:gap-2">
                {data.accesos.map((a, i) => (
                  <li
                    key={`${a.momento}-${i}`}
                    className="tw:flex tw:flex-col tw:gap-0.5 tw:border-l-2 tw:border-linea tw:py-1 tw:pl-3"
                  >
                    <div className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-3 tw:text-sm">
                      <span className="tw:font-medium">{a.consultante}</span>
                      <span className="tw:text-tinta-mid">{a.rol}</span>
                      <Pastilla tono={TONO[a.alcance] ?? 'neutro'}>
                        {ALCANCE[a.alcance] ?? a.alcance}
                      </Pastilla>
                      <span className="tw:font-mono tw:text-xs">{a.registro}</span>
                    </div>

                    <span className="tw:text-xs tw:text-tinta-mid">
                      {diaYHora(a.momento)}
                      {a.origen !== null && ` · desde ${a.origen}`}
                    </span>

                    {/* Nulo se dice, no se deja en blanco: es la ausencia que vuelve
                        inauditable el acceso. */}
                    <span
                      className={`tw:text-xs ${
                        a.necesidadDeConocer === null
                          ? 'tw:italic tw:text-aviso-fg'
                          : 'tw:text-tinta-mid'
                      }`}
                    >
                      {a.necesidadDeConocer ?? 'no declaró para qué'}
                    </span>
                  </li>
                ))}
              </ul>
            </Panel>
          )}
        </>
      )}
    </div>
  );
}

/** Qué se mostró, no sólo qué se abrió: son accesos distintos al mismo registro. */
const ALCANCE: Record<string, string> = {
  SoloRecuento: 'sólo cuántos van',
  ListaDeNombres: 'la lista de nombres',
  ManifiestoCompleto: 'el manifiesto completo',
};

const TONO: Record<string, Tono> = {
  SoloRecuento: 'neutro',
  ListaDeNombres: 'aviso',
  ManifiestoCompleto: 'riesgo',
};

interface Reporte {
  desde: string;
  total: number;
  /** Cuántos accesos no dijeron para qué. **Cuánto del registro es inauditable.** */
  sinNecesidadDeclarada: number;
  patrones: {
    consultante: string;
    consultas: number;
    registrosDistintos: number;
    sinNecesidadDeclarada: number;
    /** **No es una acusación**: es que alguien debería preguntar. */
    marcado: boolean;
  }[];
  accesos: {
    consultante: string;
    rol: string;
    momento: string;
    registro: string;
    alcance: string;
    /** Nulo es «no lo declaró». */
    necesidadDeConocer: string | null;
    origen: string | null;
  }[];
}
