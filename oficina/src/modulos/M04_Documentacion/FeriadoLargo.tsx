import { useMemo, useState, type ReactElement } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { CircleAlert, MapPin, PenLine, ShieldCheck, TriangleAlert } from 'lucide-react';

import { Boton, FilaKpis, Nota, Panel, Pastilla, RangoFechas, Vacio, avisar } from '../../ui';
import { BloqueoDuro, pedir } from '../../api/misiones';
import { usarQuienEjecuta } from '../../app/puesto';

/**
 * `PT-022` — El reporte previo al feriado largo y la firma en lote (`HU-020`).
 *
 * ── Por qué esta pantalla existe ────────────────────────────────────────────
 * El Tribunal Superior de Cuentas hace operativos de fiscalización vehicular <b>específicamente
 * en Semana Santa</b>. Es el pico anual de riesgo, y es <b>predecible</b> — lo que lo vuelve el
 * caso más fácil de resolver bien y el más caro de resolver mal.
 *
 * Un flujo que le exige a la máxima autoridad abrir veinte expedientes uno por uno a las cinco
 * de la tarde del jueves santo produce una de dos cosas: <b>permisos que no se firman y misiones
 * que salen sin amparo, o la clave prestada a un asistente</b>. La segunda es la que el sistema
 * entero está diseñado para evitar.
 *
 * ── ⚠️ Y por qué el reporte tiene tres listas y no una ──────────────────────
 * Un reporte que liste sólo los que circulan deja al resto invisible, y <b>un vehículo del que
 * nadie confirmó dónde está es exactamente lo que un operativo encuentra</b>. Las tres listas
 * suman la flota entera: ésa es la propiedad que hace útil el reporte, no un detalle de
 * presentación.
 */
export default function FeriadoLargo(): ReactElement {
  const quienEjecuta = usarQuienEjecuta();
  const cliente = useQueryClient();

  const [desde, setDesde] = useState('2026-03-30');
  const [hasta, setHasta] = useState('2026-04-05');

  /** Los permisos elegidos para la sesión de firma. */
  const [elegidos, setElegidos] = useState<ReadonlySet<string>>(new Set());

  const clave = ['periodo', desde, hasta] as const;

  const { data, isPending, isError } = useQuery({
    queryKey: clave,
    queryFn: () => pedir<Reporte>(`/periodos/reporte?desde=${desde}&hasta=${hasta}`),
  });

  const firmables = useMemo(
    () => (data?.circulan ?? []).filter((v) => !v.firmado && v.porQueNoSeFirma === null),
    [data],
  );

  const firmar = useMutation({
    mutationFn: (permisos: string[]) =>
      pedir<Lote>('/periodos/firmar-lote', {
        method: 'POST',
        body: JSON.stringify({
          permisos,
          firma: quienEjecuta,
          momento: new Date().toISOString(),
        }),
      }),
    onSuccess: async (r) => {
      // ⚠️ **Los no firmados se nombran uno por uno.** «4 de 5 firmados» sin decir cuál faltó
      // deja a quien firma buscando el que quedó, que es el que va a salir sin amparo.
      if (r.noFirmados.length === 0) {
        avisar.exito(`Se firmaron ${r.firmados.length} permisos.`);
      } else {
        avisar.error(
          `${r.noFirmados.length} de ${r.firmados.length + r.noFirmados.length} permisos no se ` +
            `firmó: ${r.noFirmados.map((n) => `${n.folio} — ${n.motivo}`).join(' · ')}`,
        );
      }

      setElegidos(new Set());
      await cliente.invalidateQueries({ queryKey: clave });
    },
    onError: (e) =>
      avisar.error(
        e instanceof BloqueoDuro ? e.paraMostrar : 'No se pudo registrar la firma del lote.',
      ),
  });

  if (isError) {
    return (
      <Nota tono="riesgo" icono={<CircleAlert />}>
        No se pudo cargar el reporte del período.
      </Nota>
    );
  }

  return (
    <div className="tw:flex tw:flex-col tw:gap-5">
      <header className="tw:flex tw:flex-col tw:gap-1">
        <h1 className="tw:text-xl tw:font-semibold tw:tracking-tight">
          Antes del feriado largo
        </h1>
        <p className="tw:text-sm tw:text-tinta-mid">
          Qué hace cada vehículo de la flota durante el período: los que{' '}
          <b>circulan con permiso firmado</b>, los que quedan <b>resguardados con confirmación</b>{' '}
          y los <b>exceptuados</b>. Las tres listas suman la flota entera.
        </p>
      </header>

      <Panel>
        <RangoFechas
          desde={desde}
          hasta={hasta}
          onCambiar={(d, h) => {
            setDesde(d);
            setHasta(h);
            // La selección no sobrevive al cambio de período: firmarían permisos de otro.
            setElegidos(new Set());
          }}
          etiqueta="Período del feriado"
        />
      </Panel>

      {isPending ? (
        <p className="tw:text-sm tw:text-tinta-mid">Cargando…</p>
      ) : (
        <>
          {/* ⚠️ Nulo es que cuadra. No es una advertencia decorativa: si aparece, hay
              vehículos que el reporte no está mostrando. */}
          {data.noCuadraPorque !== null && (
            <Nota tono="riesgo" icono={<TriangleAlert />}>
              {data.noCuadraPorque}
            </Nota>
          )}

          <FilaKpis
            kpis={[
              {
                id: 'firmables',
                rotulo: 'Permisos que puede firmar hoy',
                valor: String(data.firmables),
                tono: 'info',
                // «Cinco propuestos» y «cinco firmables» no son lo mismo: quien firma
                // necesita saber cuántos va a resolver antes de sentarse.
                nota: `de ${data.circulan.length} propuestos`,
              },
              {
                id: 'sin-confirmar',
                rotulo: 'Sin confirmar dónde están',
                valor: String(data.sinConfirmar),
                tono: data.sinConfirmar > 0 ? 'riesgo' : 'ok',
                nota: `de ${data.resguardados.length} resguardados`,
              },
              {
                id: 'exceptuados',
                rotulo: 'Servicios exceptuados',
                valor: String(data.exceptuados.length),
                tono: 'neutro',
                nota: 'no necesitan permiso',
              },
            ]}
          />

          {/* ── Los que circulan ─────────────────────────────────────────── */}
          <Panel titulo={`Circulan con permiso · ${data.circulan.length}`}>
            {data.circulan.length === 0 ? (
              <Vacio
                icono={<ShieldCheck />}
                titulo="Ninguna misión circula en el período"
                descripcion="Toda la flota queda en resguardo."
              />
            ) : (
              <div className="tw:flex tw:flex-col tw:gap-4">
                <div className="tw:flex tw:flex-col tw:gap-2">
                  {data.circulan.map((v) => (
                    <label
                      key={v.vehiculo}
                      className="tw:flex tw:cursor-pointer tw:items-start tw:gap-2.5 tw:rounded-md tw:border tw:border-linea tw:p-3 tw:text-sm"
                    >
                      <input
                        type="checkbox"
                        aria-label={`Incluir el permiso de ${v.identificacion} en la firma`}
                        className="tw:mt-0.5 tw:size-4 tw:shrink-0 tw:accent-acento"
                        // ⚠️ El que no se puede firmar **se muestra igual y no se puede
                        // elegir**: ocultarlo haría creer que no hay nada pendiente ahí. Y el
                        // ya firmado tampoco se elige: una segunda firma no agrega amparo,
                        // duplica el documento que el agente en carretera compara.
                        disabled={v.firmado || v.porQueNoSeFirma !== null}
                        checked={v.permiso !== null && elegidos.has(v.permiso)}
                        onChange={() => v.permiso !== null && alternar(setElegidos, v.permiso)}
                      />

                      <span className="tw:flex tw:flex-col tw:gap-1">
                        <span className="tw:flex tw:flex-wrap tw:items-center tw:gap-2">
                          <b>{v.identificacion}</b>
                          {v.folio !== null && (
                            <span className="tw:font-mono tw:text-xs tw:text-tinta-mid">
                              {v.folio}
                            </span>
                          )}

                          {/* ⚠️ «Firmado» va aparte del motivo, y no es redundante: los dos
                              dicen «no se puede firmar» y son cosas opuestas — uno es el
                              problema y el otro es el resultado. */}
                          {v.firmado ? (
                            <Pastilla tono="ok">firmado</Pastilla>
                          ) : (
                            v.porQueNoSeFirma === null && (
                              <Pastilla tono="info">se puede firmar</Pastilla>
                            )
                          )}
                        </span>

                        {/* El bloqueo, con su razón al lado. Sale del dominio: la pantalla
                            no reimplementa la regla. Y no se pinta de riesgo lo que ya se
                            resolvió: en un permiso firmado el texto dice que dejó de cubrir. */}
                        {v.porQueNoSeFirma !== null && (
                          <span
                            className={`tw:text-xs ${
                              v.firmado ? 'tw:text-aviso-fg' : 'tw:text-riesgo-fg'
                            }`}
                          >
                            {v.porQueNoSeFirma}
                          </span>
                        )}
                      </span>
                    </label>
                  ))}
                </div>

                <div className="tw:flex tw:flex-wrap tw:items-center tw:gap-3">
                  <Boton
                    onClick={() => firmar.mutate([...elegidos])}
                    cargando={firmar.isPending}
                    disabled={elegidos.size === 0}
                    icono={<PenLine />}
                  >
                    Firmar {elegidos.size} {elegidos.size === 1 ? 'permiso' : 'permisos'}
                  </Boton>

                  {firmables.length > 0 && elegidos.size !== firmables.length && (
                    <Boton
                      variante="fantasma"
                      onClick={() =>
                        setElegidos(
                          new Set(firmables.map((v) => v.permiso).filter((p) => p !== null)),
                        )
                      }
                    >
                      Elegir los {firmables.length} que se pueden firmar
                    </Boton>
                  )}
                </div>
              </div>
            )}
          </Panel>

          {/* ── Los que se quedan ────────────────────────────────────────── */}
          <Panel titulo={`Quedan resguardados · ${data.resguardados.length}`}>
            {data.resguardados.length === 0 ? (
              <Vacio
                icono={<MapPin />}
                titulo="Ningún vehículo queda en resguardo"
                descripcion="Toda la flota circula o está exceptuada."
              />
            ) : (
              <ul className="tw:flex tw:flex-col tw:gap-2 tw:text-sm">
                {/* Los no confirmados vienen primero desde el servidor: el orden es la
                    mitad del valor, y un reporte alfabético obliga a buscar los tres que
                    importan. */}
                {data.resguardados.map((v) => (
                  <li
                    key={v.vehiculo}
                    className="tw:flex tw:flex-wrap tw:items-center tw:gap-2 tw:rounded-md tw:border tw:border-linea tw:p-3"
                  >
                    <b>{v.identificacion}</b>

                    {v.resguardo === 'Confirmado' ? (
                      <>
                        <Pastilla tono="ok">confirmado</Pastilla>
                        <span className="tw:text-tinta-mid">
                          {v.predio} · visto el {fecha(v.confirmadoEl!)}
                        </span>
                      </>
                    ) : (
                      // ⚠️ **No es «está perdido»: es que nadie fue a mirar.** Y es
                      // exactamente lo que un operativo encuentra.
                      <>
                        <Pastilla tono="riesgo">nadie fue a mirar</Pastilla>
                        <span className="tw:text-tinta-mid">
                          Confirme dónde quedó, con evidencia fechada.
                        </span>
                      </>
                    )}
                  </li>
                ))}
              </ul>
            )}
          </Panel>

          {/* ── Los exceptuados ──────────────────────────────────────────── */}
          {data.exceptuados.length > 0 && (
            <Panel titulo={`Servicios exceptuados · ${data.exceptuados.length}`}>
              {/* `RN-24`. Van aparte y **sin permiso a firmar**: meterlos entre los que se
                  firman haría que la máxima autoridad firmara permisos que la regla dice
                  que no hacen falta. */}
              <ul className="tw:flex tw:flex-wrap tw:gap-2 tw:text-sm">
                {data.exceptuados.map((v) => (
                  <li key={v.vehiculo}>
                    <Pastilla tono="neutro">{v.identificacion}</Pastilla>
                  </li>
                ))}
              </ul>
            </Panel>
          )}

          <p className="tw:text-xs tw:text-tinta-mid">
            Reporte al {new Date(data.corteDeConocimiento).toLocaleString('es-HN')}. Una consulta
            con la misma fecha de corte reproduce el mismo resultado.
          </p>
        </>
      )}
    </div>
  );
}

function alternar(
  fijar: (f: (previo: ReadonlySet<string>) => ReadonlySet<string>) => void,
  id: string,
): void {
  fijar((previo) => {
    const copia = new Set(previo);
    if (!copia.delete(id)) copia.add(id);
    return copia;
  });
}

const fecha = (d: string): string => new Date(`${d}T00:00:00`).toLocaleDateString('es-HN');

interface EnElPeriodo {
  vehiculo: string;
  identificacion: string;
  mision?: string | null;
  /** Nulo es que no hay trámite abierto todavía. */
  permiso?: string | null;
  folio?: string | null;
  /** Nulo es que sí se puede firmar. La regla vive en el dominio, no acá. */
  porQueNoSeFirma?: string | null;
  /**
   * ⚠️ **Aparte del motivo, y no es redundante.** Los dos dicen «no se puede firmar» y son
   * cosas opuestas: uno es el problema y el otro es el resultado.
   */
  firmado?: boolean;
}

interface Resguardado {
  vehiculo: string;
  identificacion: string;
  resguardo: 'Confirmado' | 'NoConfirmado';
  /** Nulos mientras nadie fue a mirar. */
  confirmadoEl: string | null;
  predio: string | null;
}

interface Reporte {
  desde: string;
  hasta: string;
  /** `RN-94`: a qué momento está hecho el reporte. */
  corteDeConocimiento: string;
  firmables: number;
  sinConfirmar: number;
  /** ⚠️ Nulo es que cuadra. Si trae texto, hay vehículos que el reporte no muestra. */
  noCuadraPorque: string | null;
  circulan: (EnElPeriodo & {
    permiso: string | null;
    porQueNoSeFirma: string | null;
    firmado: boolean;
  })[];
  resguardados: Resguardado[];
  exceptuados: { vehiculo: string; identificacion: string }[];
}

interface Lote {
  firmados: string[];
  noFirmados: { id: string; folio: string; motivo: string }[];
}
