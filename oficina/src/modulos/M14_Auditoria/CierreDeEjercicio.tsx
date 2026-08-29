import type { ReactElement } from 'react';
import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { CalendarRange, CircleAlert, Gauge, Landmark, Scale, ShieldAlert, SlidersHorizontal, Split, Ticket } from 'lucide-react';

import { Campo, Nota, Panel, Pastilla, Boton, avisar } from '../../ui';
import { lempiras } from '../../api/combustible';
import {
  actasDeCierre,
  anularFolios,
  cortesDelEjercicio,
  producirActa,
  reversionDeCompromisos,
  vistaPreviaDelCierre,
} from '../../api/cierre-de-ejercicio';
import type {
  ActaDeCierre,
  CambioDeParametro,
  CierreApurado,
  CortesDelEjercicio,
  CortesSinResolver,
  FolioPorAnular,
  MisionQueCruza,
  MotivoCompartido,
  ReporteDeReversion,
  VentanaDeCierre,
  VentanaSinResolver,
} from '../../api/cierre-de-ejercicio';
import { soloFecha } from '../M06_Autorizacion/formato';

/**
 * `RN-96` — el cierre de ejercicio.
 *
 * ── Esta pantalla no tiene un botón que cierre misiones, y ése es el punto ───
 * *«El cierre de ejercicio fiscal no ejecuta ni habilita ninguna transición de la Orden de
 * Misión. **Ningún expediente cambia de estado por efecto de una fecha**»*.
 *
 * `RN-96` nombra lo que pasaría sin ella: *«sin esta regla escrita la primera implementación va
 * a poner un cierre masivo por fecha, porque es lo que resuelve ese problema»*. Lo que la
 * pantalla hace es **mostrar el apuro**, no resolverlo cerrando en bloque.
 */
export default function CierreDeEjercicioPantalla(): ReactElement {
  // El ejercicio que se cierra es el anterior: esta pantalla se mira en enero.
  const anio = new Date().getFullYear() - 1;

  const [ejercicio, setEjercicio] = useState(`${anio}`);

  const cola = useQueryClient();

  // ── Los cortes ya no se escriben acá: se resuelven del parámetro ──────────
  // `RN-96` los declara configurables con vigencia. Un campo editable dejaría producir el acta
  // contra un corte que alguien escribió en el momento, y los cortes deciden qué expedientes
  // entran al inventario y a qué ejercicio se imputa cada hecho.
  const cortes = useQuery({
    queryKey: ['cortes-del-ejercicio', ejercicio],
    queryFn: () => cortesDelEjercicio(ejercicio),
  });

  const vista = useQuery({
    queryKey: ['cierre-vista-previa', ejercicio],
    queryFn: () => vistaPreviaDelCierre(ejercicio),

    // Sin cortes parametrizados el acta no se arma, y el servidor devuelve un bloqueo duro.
    // Pedirla igual dejaría un error rojo donde lo correcto es explicar qué falta cargar.
    enabled: cortes.data?.cortes != null,
  });

  const producidas = useQuery({ queryKey: ['actas-de-cierre'], queryFn: actasDeCierre });

  // `RN-96` punto 5 — lo que se reporta a ARGOS y SIAFI. Sale vacío hasta que los folios se
  // anulen: un compromiso listado y no anulado sigue vivo.
  const reversion = useQuery({
    queryKey: ['reversion-de-compromisos', ejercicio],
    queryFn: () => reversionDeCompromisos(ejercicio),
    enabled: cortes.data?.cortes != null,
  });

  const producida = producidas.data?.find((a) => a.ejercicio === ejercicio);

  const producir = useMutation({
    mutationFn: () =>
      producirActa({
        folio: `AC-${ejercicio}-001`,
        ejercicio,
        persona: 'P-ADMIN',
        puesto: 'PU-GERENCIA',
      }),
    onSuccess: () => {
      avisar.exito(`Acta de cierre del ejercicio ${ejercicio} producida.`);
      void cola.invalidateQueries({ queryKey: ['actas-de-cierre'] });
    },
    onError: (error: Error) => { avisar.error(error.message); },
  });

  const anular = useMutation({
    mutationFn: () =>
      anularFolios(
        ejercicio,
        'P-ADMIN',
        `Folio no consumido al cierre del ejercicio ${ejercicio}; ` +
          `el compromiso no se arrastra a ${Number(ejercicio) + 1}.`,
      ),
    onSuccess: (r) => {
      avisar.exito(`${r.anulados} folio(s) anulado(s) citando el acta.`);
      void cola.invalidateQueries({ queryKey: ['actas-de-cierre'] });
      void cola.invalidateQueries({ queryKey: ['cierre-vista-previa'] });

      // El panel de reversión aparece recién al anular: hasta entonces no hay compromiso
      // liberado que reportar.
      void cola.invalidateQueries({ queryKey: ['reversion-de-compromisos'] });
    },
    onError: (error: Error) => { avisar.error(error.message); },
  });

  if (cortes.isError || vista.isError) {
    return (
      <Nota tono="riesgo" icono={<CircleAlert />}>
        No se pudo armar el acta de cierre.{" "}
        {((cortes.error ?? vista.error) as Error).message}
      </Nota>
    );
  }

  return (
    <div className="tw:flex tw:flex-col tw:gap-5">
      <header className="tw:flex tw:flex-col tw:gap-1">
        <h1 className="tw:text-xl tw:font-semibold tw:tracking-tight">Cierre de ejercicio</h1>
        <p className="tw:text-sm tw:text-tinta-mid">
          Un <b>corte de imputación y de reporte</b>. No cierra ninguna misión: ningún expediente
          cambia de estado por efecto de una fecha. Lo que sigue abierto sigue abierto, y su
          cierre se evalúa uno por uno.
        </p>
      </header>

      <Panel>
        <div className="tw:flex tw:flex-col tw:gap-3">
          <div className="tw:max-w-[10rem]">
            <Campo etiqueta="Ejercicio" ayuda="A qué año se imputa lo que cerró.">
              {(control) => (
                <input
                  {...control}
                  inputMode="numeric"
                  value={ejercicio}
                  onChange={(e) => { setEjercicio(e.target.value); }}
                  className="loki-foco tw:w-full tw:rounded-control tw:border tw:border-linea tw:bg-panel tw:px-2 tw:py-1.5 tw:text-cuerpo-2 tw:font-mono tw:tabular-nums tw:text-tinta-hi"
                />
              )}
            </Campo>
          </div>

          {/* ── Los cortes se muestran, no se escriben ────────────────────────
              `RN-96` los declara configurables con vigencia. Un campo editable acá dejaría
              producir el acta contra un corte que nadie autorizó, y los cortes deciden qué
              expedientes entran al inventario y a qué ejercicio se imputa cada hecho. */}
          <PanelDeCortes
            cortes={cortes.data?.cortes ?? null}
            sinCortes={cortes.data?.sinCortes ?? null}
            cargando={cortes.isPending}
          />
        </div>
      </Panel>

      {cortes.data?.cortes == null ? null : vista.isPending ? (
        <p className="tw:text-sm tw:text-tinta-mid">Armando el acta…</p>
      ) : (
        <>
          {/* Las observaciones NO van en un bloque aparte arriba: cada una tiene su panel, y
              repetirlas duplicaria el mismo hallazgo dos veces en la misma pantalla. Van al
              acta congelada, que es donde el documento las necesita. */}
          <PanelDelApuro
            apuro={vista.data.apuro}
            ventana={vista.data.ventana}
            sinVentana={vista.data.sinVentana}
          />

          {vista.data.motivosCompartidos.length > 0 && (
            <PanelDeMotivos motivos={vista.data.motivosCompartidos} />
          )}

          <PanelDelInventario acta={vista.data} />

          {vista.data.misionesQueCruzan.length > 0 && (
            <PanelDeCruces misiones={vista.data.misionesQueCruzan} />
          )}

          <PanelDeFolios
            folios={vista.data.foliosPorAnular}
            monto={vista.data.montoPorAnular}
            hayActa={producida !== undefined}
            pendientesDeAnular={(producida?.folios ?? 0) - (producida?.anulados ?? 0)}
            onAnular={() => { anular.mutate(); }}
            anulando={anular.isPending}
          />

          {vista.data.cambiosDeParametros.length > 0 && (
            <PanelDeParametros cambios={vista.data.cambiosDeParametros} />
          )}

          {reversion.data != null && reversion.data.renglones.length > 0 && (
            <PanelDeReversion reporte={reversion.data} ejercicio={ejercicio} />
          )}

          <Panel>
            {producida === undefined ? (
              <div className="tw:flex tw:flex-wrap tw:items-center tw:gap-3">
                <Boton
                  onClick={() => { producir.mutate(); }}
                  disabled={producir.isPending}
                >
                  Producir el acta del ejercicio {ejercicio}
                </Boton>
                <span className="tw:text-xs tw:text-tinta-mid">
                  Congela el folio del saldo que cita, las diferencias vistas hoy y la lista de
                  folios. <b>No anula nada</b>: eso es un acto aparte.
                </span>
              </div>
            ) : (
              <p className="tw:text-sm tw:text-tinta-mid">
                Acta <span className="tw:font-mono">{producida.folio}</span> producida. Cita el
                saldo de apertura{' '}
                {producida.saldoDeAperturaFolio !== null ? (
                  <span className="tw:font-mono">{producida.saldoDeAperturaFolio}</span>
                ) : (
                  <b className="tw:text-aviso-fg">
                    — ninguno: no había saldo producido con qué cuadrar
                  </b>
                )}
                .
              </p>
            )}
          </Panel>
        </>
      )}
    </div>
  );
}

/**
 * Las dos fechas de corte — `RN-96`, **parámetros con vigencia**.
 *
 * ── Sin ellas no hay acta, y no es lo mismo que con la ventana ──────────────
 * La ventana apaga dos reportes. Los cortes deciden **qué expedientes entran al inventario y a
 * qué ejercicio se imputa cada hecho**: un acta producida con fechas supuestas afirmaría cosas
 * falsas sobre todo lo demás.
 */
function PanelDeCortes({
  cortes,
  sinCortes,
  cargando,
}: {
  cortes: CortesDelEjercicio | null;
  sinCortes: CortesSinResolver | null;
  cargando: boolean;
}): ReactElement {
  if (cargando) {
    return <p className="tw:text-sm tw:text-tinta-mid">Resolviendo las fechas de corte…</p>;
  }

  if (cortes === null) {
    return (
      <div className="tw:flex tw:flex-col tw:gap-2">
        <Nota tono="riesgo" icono={<CircleAlert />}>
          <b>Las fechas de corte del ejercicio no están parametrizadas.</b> Sin ellas no se puede
          armar el acta: los cortes deciden qué expedientes entran al inventario y a qué ejercicio
          se imputa cada hecho.
        </Nota>

        {sinCortes !== null && (
          <p className="tw:text-xs tw:text-tinta-mid">
            {sinCortes.porQueNo} Se cargan en{' '}
            <span className="tw:font-mono">cierre.corte_legal_dia_y_mes</span> —el día y mes en
            formato <span className="tw:font-mono">MM-DD</span>— y{' '}
            <span className="tw:font-mono">cierre.corte_operativo_dias_despues</span>, con
            vigencia y doble control.
          </p>
        )}
      </div>
    );
  }

  return (
    <div className="tw:flex tw:flex-col tw:gap-1">
      <div className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-5 tw:gap-y-1 tw:text-sm">
        <span className="tw:flex tw:items-center tw:gap-1.5">
          <CalendarRange className="tw:size-4 tw:text-tinta-mid" aria-hidden />
          <span className="tw:text-tinta-mid">corte legal</span>{' '}
          <b>{soloFecha(cortes.legal)}</b>
        </span>
        <span>
          <span className="tw:text-tinta-mid">corte operativo</span>{' '}
          <b>{soloFecha(cortes.operativo)}</b>
        </span>
      </div>

      {/* De qué versiones salieron. Dos actas con cortes distintos no se pueden comparar si
          ninguna dice cuál usó. */}
      <p className="tw:font-mono tw:text-xs tw:text-tinta-mid">{cortes.origen}</p>
    </div>
  );
}

/**
 * El indicador de cierre apurado.
 *
 * *«El sistema no la resuelve; **la hace visible**. El indicador de misiones cerradas en la
 * ventana de cierre, contra el promedio del año, es el dato que expone el cierre apurado»*.
 */
function PanelDelApuro({
  apuro,
  ventana,
  sinVentana,
}: {
  apuro: CierreApurado | null;
  ventana: VentanaDeCierre | null;
  sinVentana: VentanaSinResolver | null;
}): ReactElement {
  // ── Sin ventana no hay cero: no hay medición ──────────────────────────────
  // `RN-96` la declara configurable con vigencia, y no tiene valor por omisión. Mostrar un
  // «0 cerradas» acá haría creer que se buscó y no había, cuando nadie configuró dónde buscar.
  if (apuro === null || ventana === null) {
    return (
      <Panel titulo="Ritmo de cierre en la ventana">
        <div className="tw:flex tw:flex-col tw:gap-2">
          <Nota tono="aviso" icono={<ShieldAlert />}>
            <b>La ventana de cierre no está parametrizada.</b> Ni el ritmo de cierre ni los
            motivos compartidos se evaluaron — están <b>sin medir, no en cero</b>.
          </Nota>

          {sinVentana !== null && (
            <p className="tw:text-xs tw:text-tinta-mid">
              {sinVentana.porQueNo} Se carga en{' '}
              <span className="tw:font-mono">{sinVentana.clave}</span>, con vigencia y doble
              control. Cuánto dura la ventana lo decide cada institución: no hay un valor por
              omisión que este sistema pueda suponer sin inventar el hallazgo.
            </p>
          )}
        </div>
      </Panel>
    );
  }

  return (
    <Panel titulo="Ritmo de cierre en la ventana">
      <div className="tw:flex tw:flex-col tw:gap-2">
        <div className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-5 tw:gap-y-1 tw:text-sm">
          <span className="tw:flex tw:items-center tw:gap-1.5">
            <Gauge className="tw:size-4 tw:text-tinta-mid" aria-hidden />
            <b className="tw:tabular-nums">{apuro.cerradasEnLaVentana}</b> cerradas en{' '}
            {apuro.diasDeLaVentana} días de ventana
          </span>
          <span className="tw:text-tinta-mid tw:tabular-nums">
            {apuro.cerradasEnElAnio} en el año
          </span>
        </div>

        {/* **Nulo no es cero.** Sin cierres fuera de la ventana no hay contra qué comparar, y
            decir «infinito» sería inventar el hallazgo. */}
        {apuro.veces === null ? (
          <p className="tw:text-xs tw:text-tinta-mid">
            El indicador <b>no se puede evaluar</b>: no hubo cierres fuera de la ventana con qué
            comparar. No es que el ritmo fuera normal — es que no hay medida.
          </p>
        ) : (
          <p className="tw:text-xs tw:text-tinta-mid">
            {apuro.promedioDiarioEnLaVentana.toFixed(2)} por día en la ventana contra{' '}
            {(apuro.promedioDiarioDelAnio ?? 0).toFixed(2)} en el resto del año —{' '}
            <b className={apuro.veces > 2 ? 'tw:text-aviso-fg' : undefined}>
              {apuro.veces.toFixed(1)} veces el ritmo
            </b>
            . El promedio del año <b>excluye la ventana</b>: incluirla la diluiría contra sí
            misma.
          </p>
        )}

        {/* De dónde salió la ventana. Un indicador que no dice contra qué se midió no se
            puede reproducir ni discutir años después. */}
        <p className="tw:text-xs tw:text-tinta-mid">
          Del {soloFecha(ventana.desde)} al {soloFecha(ventana.hasta)} —{' '}
          <span className="tw:font-mono">{ventana.origen}</span>
        </p>
      </div>
    </Panel>
  );
}

/**
 * *«Ante el Tribunal Superior de Cuentas, cincuenta expedientes cerrados el 31 de diciembre a la
 * misma hora con el mismo motivo **son el hallazgo**, no su solución»*.
 */
function PanelDeMotivos({ motivos }: { motivos: MotivoCompartido[] }): ReactElement {
  return (
    <Panel titulo="Motivos de cierre compartidos">
      <div className="tw:flex tw:flex-col tw:gap-3">
        <Nota tono="riesgo" icono={<CircleAlert />}>
          `RN-08` exige <b>evaluación individual</b> de los criterios de hallazgo, con los datos
          concretos de cada expediente. Un motivo repetido en varios es lo que un auditor lee como
          cierre en bloque.
        </Nota>

        {motivos.map((m) => (
          <div
            key={m.motivo}
            className="tw:flex tw:flex-col tw:gap-1 tw:border-l-2 tw:border-riesgo-fg tw:pl-3"
          >
            <span className="tw:text-sm tw:font-medium">«{m.motivo}»</span>
            <span className="tw:text-xs tw:text-tinta-mid">
              {m.misiones.length} expedientes, entre {soloFecha(m.primero)} y{' '}
              {soloFecha(m.ultimo)}
              {m.ventanaEnMinutos <= 60 && (
                <b className="tw:text-riesgo-fg">
                  {' '}
                  — todos dentro de {m.ventanaEnMinutos} minuto(s)
                </b>
              )}
            </span>
          </div>
        ))}
      </div>
    </Panel>
  );
}

function PanelDelInventario({ acta }: { acta: ActaDeCierre }): ReactElement {
  return (
    <Panel titulo="Inventario al corte y saldo de apertura">
      <div className="tw:flex tw:flex-col tw:gap-2">
        <p className="tw:flex tw:items-center tw:gap-1.5 tw:text-sm">
          <Scale className="tw:size-4 tw:text-tinta-mid" aria-hidden />
          <b className="tw:tabular-nums">{acta.inventario}</b> expedientes no terminales al corte
          operativo.
        </p>

        {/* **Sin saldo no hay coincidencia, hay ausencia de comparación.** Decir «coincide»
            acá sería la misma mentira que un inventario que se ve completo estando incompleto:
            la lista de diferencias está vacía porque no hubo contra qué compararla. */}
        {acta.saldoDeAperturaFolio === null ? (
          <Nota tono="aviso" icono={<ShieldAlert />}>
            <b>No hay saldo de apertura producido para {acta.ejercicio}.</b> Este inventario no se
            cuadró contra nada — `RN-96` punto 2 manda que coincida renglón por renglón con el
            saldo, y sin él la ausencia de diferencias no significa que coincida.
          </Nota>
        ) : acta.diferenciasConElSaldo.length === 0 ? (
          <p className="tw:text-xs tw:text-tinta-mid">
            Coincide con el saldo de apertura{' '}
            <span className="tw:font-mono">{acta.saldoDeAperturaFolio}</span>, que es lo que
            `RN-96` punto 2 manda cuadrar <b>renglón por renglón</b>. El detalle vive en el saldo:
            repetirlo acá dejaría dos inventarios del mismo corte que se pueden separar.
          </p>
        ) : (
          <div className="tw:flex tw:flex-col tw:gap-1">
            <Nota tono="riesgo" icono={<CircleAlert />}>
              {acta.diferenciasConElSaldo.length} diferencia(s) contra el saldo de apertura.
            </Nota>
            {acta.diferenciasConElSaldo.map((d) => (
              <p key={d} className="tw:text-xs tw:text-tinta-mid">
                {d}
              </p>
            ))}
          </div>
        )}
      </div>
    </Panel>
  );
}

/**
 * `RN-96` punto 4 — el desglose de imputación por ejercicio.
 *
 * *«La Orden de Misión que cruza el corte **no se divide**. Cada hecho económico se imputa al
 * ejercicio de su fecha del hecho»*.
 */
function PanelDeCruces({ misiones }: { misiones: MisionQueCruza[] }): ReactElement {
  const sinTabla = misiones.reduce((s, m) => s + m.sinTablaParametrica, 0);

  return (
    <Panel titulo={`${misiones.length} misión(es) cruzaron el corte`}>
      <div className="tw:flex tw:flex-col tw:gap-3">
        <p className="tw:text-xs tw:text-tinta-mid">
          El expediente <b>no se parte</b>: lo que se reparte entre ejercicios son sus hechos, cada
          uno por su propia fecha.
          {sinTabla > 0 && (
            <>
              {' '}
              <b className="tw:text-aviso-fg">
                {sinTabla} hecho(s) sin tabla paramétrica declarada
              </b>{' '}
              — sin ella el cálculo no se puede rehacer.
            </>
          )}
        </p>

        {misiones.map((m) => (
          <div
            key={m.mision}
            className="tw:flex tw:flex-col tw:gap-1 tw:border-l-2 tw:border-borde tw:pl-3"
          >
            <div className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-3">
              <Split className="tw:size-3.5 tw:text-tinta-mid" aria-hidden />
              <span className="tw:font-mono tw:text-xs">{m.referencia}</span>
              <span className="tw:text-xs tw:text-tinta-mid">
                {soloFecha(m.salida)} → {m.retorno !== null ? soloFecha(m.retorno) : 'sin retorno'}
              </span>
            </div>

            <div className="tw:flex tw:flex-wrap tw:gap-x-4 tw:text-sm tw:tabular-nums">
              {Object.entries(m.porEjercicio).map(([ej, monto]) => (
                <span key={ej}>
                  <span className="tw:text-tinta-mid">{ej}: </span>
                  {lempiras(monto)}
                </span>
              ))}
            </div>

            {m.hechos.map((h, i) => (
              <p key={`${h.concepto}-${i}`} className="tw:text-xs tw:text-tinta-mid">
                {soloFecha(h.fechaDelHecho)} · {h.concepto} · {lempiras(h.monto)} ·{' '}
                {h.tablaParametrica ?? (
                  <b className="tw:text-aviso-fg">sin tabla paramétrica declarada</b>
                )}
              </p>
            ))}
          </div>
        ))}
      </div>
    </Panel>
  );
}

/**
 * `RN-96` punto 5 — *«todo folio reservado y no consumido se anula con acta. Ni el compromiso ni
 * el folio se arrastran al ejercicio siguiente»*.
 */
function PanelDeFolios({
  folios,
  monto,
  hayActa,
  pendientesDeAnular,
  onAnular,
  anulando,
}: {
  folios: FolioPorAnular[];
  monto: number;
  hayActa: boolean;
  pendientesDeAnular: number;
  onAnular: () => void;
  anulando: boolean;
}): ReactElement {
  const afuera = folios.filter((f) => !f.sePuedeAnular);

  if (folios.length === 0) {
    return (
      <Panel titulo="Folios reservados y no consumidos">
        <p className="tw:text-sm tw:text-tinta-mid">
          Ningún folio quedó reservado sin consumir al corte.
        </p>
      </Panel>
    );
  }

  return (
    <Panel titulo={`${folios.length} folio(s) reservado(s) y sin consumir`}>
      <div className="tw:flex tw:flex-col tw:gap-3">
        <p className="tw:text-sm tw:text-tinta-mid">
          <b className="tw:text-tinta-hi">{lempiras(monto)}</b> se revierten con asiento y no se
          arrastran al ejercicio siguiente.
        </p>

        {afuera.length > 0 && (
          <Nota tono="aviso" icono={<ShieldAlert />}>
            {afuera.length} de ellos están <b>entregados</b>, y ésos no se anulan: `V-03` sólo
            corre sobre un vale emitido. Es dinero fuera de la caja al cierre — el camino es la
            devolución con acta o la obligación de reintegro (`RN-86`).
          </Nota>
        )}

        <div className="tw:flex tw:flex-col tw:gap-1.5">
          {folios.map((f) => (
            <div
              key={f.asignacion}
              className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-3 tw:text-sm"
            >
              <Ticket className="tw:size-3.5 tw:text-tinta-mid" aria-hidden />
              <span className="tw:font-mono tw:text-xs">{f.folio}</span>
              <span className="tw:tabular-nums">{lempiras(f.monto)}</span>
              <span className="tw:text-xs tw:text-tinta-mid">{f.delegacion}</span>
              <Pastilla tono={f.sePuedeAnular ? 'neutro' : 'aviso'}>{f.estado}</Pastilla>
            </div>
          ))}
        </div>

        {/* **Anular es un acto aparte del acta.** Un documento que anulara decenas de folios al
            producirse sería un cierre masivo por fecha con otro nombre. */}
        {hayActa ? (
          pendientesDeAnular > 0 ? (
            <div className="tw:flex tw:flex-wrap tw:items-center tw:gap-3">
              <Boton onClick={onAnular} disabled={anulando}>
                Anular los folios citando el acta
              </Boton>
              <span className="tw:text-xs tw:text-tinta-mid">
                Cada uno queda con su asiento `V-03` propio, con autor y motivo.
              </span>
            </div>
          ) : (
            <p className="tw:text-xs tw:text-tinta-mid">
              Los folios anulables del acta ya se anularon.
            </p>
          )
        ) : (
          <p className="tw:text-xs tw:text-tinta-mid">
            Se anulan <b>después</b> de producir el acta, citándola: sin ella no consta que fueran
            los que quedaron sin consumir al corte.
          </p>
        )}
      </div>
    </Panel>
  );
}

/**
 * `RN-96` punto 5 y `RN-81` — el reporte de reversión de compromisos.
 *
 * ── Por qué existe, en las palabras de `RN-81` ──────────────────────────────
 * *«`RN-48` prohíbe que SIGTI escriba en ARGOS, y hace bien. Pero de esa prohibición no se sigue
 * que SIGTI pueda **callar**: si SIGTI anula un compromiso de combustible y no lo reporta, el
 * descuadre aparece en SIAFI y nadie sabe de dónde vino»*.
 */
function PanelDeReversion({
  reporte,
  ejercicio,
}: {
  reporte: ReporteDeReversion;
  ejercicio: string;
}): ReactElement {
  const objetos = Object.entries(reporte.porObjetoDelGasto);

  return (
    <Panel titulo="Reversión de compromisos para ARGOS y SIAFI">
      <div className="tw:flex tw:flex-col tw:gap-3">
        {/* `RN-94` — las dos fechas, en el encabezado del reporte. */}
        <p className="tw:text-xs tw:text-tinta-mid">
          Hechos del {soloFecha(reporte.periodoDesde)} al {soloFecha(reporte.periodoHasta)} ·
          conocido al {soloFecha(reporte.corteDeConocimiento)} · respalda el acta{' '}
          <span className="tw:font-mono">{reporte.actaQueLoRespalda}</span>
        </p>

        <div className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-5 tw:gap-y-1 tw:text-sm">
          <span className="tw:flex tw:items-center tw:gap-1.5">
            <Landmark className="tw:size-4 tw:text-tinta-mid" aria-hidden />
            <b>{lempiras(reporte.totalLiberado)}</b> liberados
          </span>
          <span className="tw:text-xs tw:text-tinta-mid">
            de {lempiras(reporte.totalComprometido)} comprometidos
            {reporte.totalEjecutado !== 0 &&
              `, menos ${lempiras(reporte.totalEjecutado)} ya ejecutados`}
          </span>
        </div>

        {reporte.advertencias.map((a) => (
          <Nota key={a} tono="aviso" icono={<ShieldAlert />}>
            {a}
          </Nota>
        ))}

        {objetos.length > 0 && (
          <div className="tw:flex tw:flex-col tw:gap-1">
            <span className="tw:text-xs tw:text-tinta-mid">Por objeto del gasto</span>
            {objetos.map(([partida, monto]) => (
              <div
                key={partida}
                className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-3 tw:text-sm"
              >
                <span className="tw:font-mono tw:text-xs">{partida}</span>
                <span className="tw:tabular-nums">{lempiras(monto)}</span>
              </div>
            ))}
          </div>
        )}

        <div className="tw:flex tw:flex-col tw:gap-1.5">
          {reporte.renglones.map((r) => (
            <div
              key={r.folio}
              className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-3 tw:text-sm"
            >
              <span className="tw:font-mono tw:text-xs">{r.folio}</span>
              <span className="tw:tabular-nums">{lempiras(r.liberado)}</span>
              <span className="tw:text-xs tw:text-tinta-mid">{r.delegacion}</span>

              {/* Nulo es sin partida, no cero: ese renglón no se puede imputar en SIAFI. */}
              {r.objetoDelGasto !== null ? (
                <span className="tw:font-mono tw:text-xs tw:text-tinta-mid">
                  {r.objetoDelGasto}
                </span>
              ) : (
                <Pastilla tono="aviso">Sin partida</Pastilla>
              )}

              {r.tuvoEjecucionParcial && (
                <Pastilla tono="info">
                  neto de {lempiras(r.ejecutado)} ejecutados
                </Pastilla>
              )}
            </div>
          ))}
        </div>

        {/* El archivo de conciliación. Va como enlace y no como descarga por script: el
            servidor ya lo entrega con nombre y codificación declarada. */}
        <p className="tw:text-xs tw:text-tinta-mid">
          <a
            className="loki-foco tw:underline"
            href={`${import.meta.env.VITE_API ?? ''}/cierre-de-ejercicio/${ejercicio}/reversion.csv`}
          >
            Descargar el archivo de conciliación
          </a>
          {' — '}⚠️ <b>no es el formato de SIAFI</b>: `RN-81` deja el mecanismo inicial en el
          reporte con formato acordado, y los contratos de API siguen pendientes.
        </p>
      </div>
    </Panel>
  );
}

/**
 * `RN-96` punto 6 — *«es la evidencia de que **nadie aflojó un umbral en diciembre para cerrar
 * limpio**, o de que alguien lo hizo y quedó a la vista»*.
 */
function PanelDeParametros({ cambios }: { cambios: CambioDeParametro[] }): ReactElement {
  return (
    <Panel titulo="Parámetros movidos en la ventana de cierre">
      <div className="tw:flex tw:flex-col tw:gap-3">
        <p className="tw:text-xs tw:text-tinta-mid">
          No acusa: registra. Un umbral que se movió en diciembre puede tener una razón perfecta —
          lo que no puede es no aparecer.
        </p>

        {cambios.map((c) => (
          <div
            key={`${c.clave}-${c.registrado}`}
            className="tw:flex tw:flex-col tw:gap-1 tw:border-l-2 tw:border-borde tw:pl-3"
          >
            <div className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-3">
              <SlidersHorizontal className="tw:size-3.5 tw:text-tinta-mid" aria-hidden />
              <span className="tw:font-mono tw:text-xs">{c.clave}</span>
              <span className="tw:text-sm">
                {/* **Las dos mitades.** «Se cargó 15» sin decir que venía de 5 no es evidencia. */}
                {c.valorAnterior !== null ? (
                  <>
                    <span className="tw:text-tinta-mid tw:line-through">{c.valorAnterior}</span>
                    {' → '}
                  </>
                ) : (
                  <span className="tw:text-tinta-mid">primera versión → </span>
                )}
                <b>{c.valorNuevo}</b>
              </span>
            </div>

            <span className="tw:text-xs tw:text-tinta-mid">
              registrado el {soloFecha(c.registrado)} por {c.cargadoPor}
              {c.aprobadoPor !== null ? `, aprobado por ${c.aprobadoPor}` : ', sin aprobar'} · rige
              desde el {soloFecha(c.vigenteDesde)}
              {c.vigenteDesde < c.registrado.slice(0, 10) && (
                <b className="tw:text-aviso-fg"> — con vigencia retroactiva</b>
              )}
            </span>
          </div>
        ))}
      </div>
    </Panel>
  );
}
