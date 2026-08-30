import type { ReactElement } from 'react';
import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Link, Navigate } from 'react-router';
import { CircleAlert, FileStack } from 'lucide-react';

import { CampoBusqueda, Nota, Panel, Pastilla, Vacio } from '../../ui';
import type { Tono } from '../../ui';
import { pedir } from '../../api/misiones';
import { usarPuesto } from '../../app/puesto';
import { soloFecha } from '../M06_Autorizacion/formato';

/**
 * `PT-006` — Mis solicitudes. La raíz de `ACT-02`.
 *
 * ── `R-2`: es una bandeja, no un tablero ────────────────────────────────────
 * El Solicitante es <b>el usuario más numeroso y el menos frecuente</b>: entra varias veces al
 * mes. Su navegación tiene que ser memorizable después de no usarla en seis semanas, así que
 * esta pantalla contesta una sola pregunta — <i>«¿en qué quedó lo que pedí?»</i> — y no ofrece
 * nada más.
 *
 * ── Y el alcance es `Propio`, que acá significa algo concreto ───────────────
 * Ve lo que capturó <b>y lo que solicitó</b>. No son lo mismo: con frecuencia quien captura es
 * la asistente de la unidad por encargo de su jefatura, y el solicitante de derecho tiene que
 * ver su propia solicitud aunque no la haya escrito. Lo resuelve el alcance del puesto, no esta
 * pantalla.
 */
export default function MisSolicitudes(): ReactElement {
  const { elegido } = usarPuesto();
  const [texto, setTexto] = useState('');

  const { data, isPending, isError } = useQuery({
    queryKey: ['puesto', elegido?.puesto, 'expedientes', elegido?.persona, texto],
    queryFn: () =>
      pedir<Resultado>(
        `/puesto/${elegido!.puesto}/expedientes` +
          `?persona=${encodeURIComponent(elegido!.persona)}&q=${encodeURIComponent(texto)}`,
      ),
    enabled: elegido !== null,
  });

  if (elegido === null) return <Navigate to="/ingreso" replace />;

  if (isError) {
    return (
      <Nota tono="riesgo" icono={<CircleAlert />}>
        No se pudieron cargar sus solicitudes.
      </Nota>
    );
  }

  return (
    <div className="tw:flex tw:flex-col tw:gap-5">
      <header className="tw:flex tw:flex-col tw:gap-1">
        <h1 className="tw:text-xl tw:font-semibold tw:tracking-tight">Mis solicitudes</h1>
        <p className="tw:text-sm tw:text-tinta-mid">
          En qué quedó lo que pidió, y lo que pidieron a su nombre.
        </p>
      </header>

      <CampoBusqueda
        etiqueta="Folio, destino u objeto del traslado…"
        valor={texto}
        onCambio={setTexto}
      />

      {isPending ? (
        <p className="tw:text-sm tw:text-tinta-mid">Cargando…</p>
      ) : !data.alcanceResuelto ? (
        <Nota tono="riesgo" icono={<CircleAlert />}>
          <b>No se pudo resolver qué expedientes le corresponden</b>, así que no se muestra
          ninguno. {data.porQueNo}
        </Nota>
      ) : data.resultados.length === 0 ? (
        <Vacio
          icono={<FileStack />}
          titulo={texto === '' ? 'Todavía no ha pedido ningún traslado' : 'Nada coincide'}
          descripcion={
            texto === ''
              ? 'Cuando pida uno, aparecerá acá con su estado y su folio.'
              : 'Pruebe con el folio completo, o limpie la búsqueda.'
          }
        />
      ) : (
        <Panel titulo={`${data.total} solicitud(es)`}>
          <ul className="tw:flex tw:flex-col tw:gap-2">
            {data.resultados.map((r) => (
              <li
                key={r.mision}
                className="tw:flex tw:flex-col tw:gap-0.5 tw:border-l-2 tw:border-linea tw:py-1 tw:pl-3"
              >
                <div className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-3">
                  <Link
                    to={`/rastro?expediente=${r.mision}`}
                    className="loki-foco tw:font-mono tw:text-sm tw:underline-offset-2 tw:hover:underline"
                  >
                    {r.folio}
                  </Link>

                  {/* ⚠️ El provisional se marca. Sin la marca, alguien lo cita en un descargo
                      creyendo que es el folio oficial — y no lo es hasta que la delegación
                      tenga rango asignado y la institución fije su formato. */}
                  {r.folio.startsWith('PROV-') && (
                    <Pastilla tono="aviso">folio provisional</Pastilla>
                  )}

                  <Pastilla tono={TONO[r.estado] ?? 'neutro'}>{r.estado}</Pastilla>
                  <span className="tw:text-sm tw:text-tinta-mid">{r.destino}</span>
                </div>

                <span className="tw:text-xs tw:text-tinta-mid">
                  {r.objetoDelTraslado} · sale {soloFecha(r.salida)}
                  {r.solicitanteDeDerecho !== elegido.persona &&
                    ` · a nombre de ${r.solicitanteDeDerecho}`}
                </span>
              </li>
            ))}
          </ul>
        </Panel>
      )}

      <Nota tono="info">
        <b>Todavía no se puede capturar una solicitud desde acá.</b> La requisición de vehículo
        —<code className="tw:font-mono tw:text-xs">PT-007</code>— replica un formato en papel que
        la institución no ha entregado (<code className="tw:font-mono tw:text-xs">insumo #2</code>
        ), y dibujarla sin verlo perdería campos que hoy alguien llena a mano.
      </Nota>
    </div>
  );
}

/** Los estados terminales se distinguen de los que siguen su curso. */
const TONO: Record<string, Tono> = {
  Borrador: 'neutro',
  Solicitada: 'info',
  Aprobada: 'ok',
  Programada: 'ok',
  Despachada: 'ok',
  EnRuta: 'ok',
  Retornada: 'ok',
  Liquidada: 'ok',
  Cerrada: 'neutro',
  Rechazada: 'riesgo',
  Anulada: 'riesgo',
  CerradaConHallazgo: 'aviso',
};

interface Resultado {
  alcanceResuelto: boolean;
  porQueNo: string | null;
  total: number;
  resultados: {
    mision: string;
    folio: string;
    estado: string;
    destino: string;
    objetoDelTraslado: string;
    solicitanteDeDerecho: string;
    salida: string;
  }[];
}
