import type { ReactElement } from 'react';
import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { CircleAlert, Clock, PackageX, ShieldAlert, TriangleAlert } from 'lucide-react';

import { Nota, Panel, Pastilla, Boton, avisar } from '../../ui';
import {
  bienesNoRecuperados,
  incidentes,
  registrarDesenlace,
} from '../../api/incidentes';
import type {
  BienFueraDelAlcance,
  DesenlaceDeLaInterrupcion,
  ExpedienteDeIncidente,
} from '../../api/incidentes';
import { soloFecha } from '../M06_Autorizacion/formato';

/**
 * M-12 — incidentes, siniestros y sanciones.
 *
 * ── Esta pantalla no pregunta de quién fue la culpa, y ése es el punto ──────
 * `RN-74`: *«un campo "¿de quién fue la culpa?" en esa pantalla produce dos daños: una
 * declaración tomada bajo presión que después pesa en un expediente, y una atribución hecha por
 * quien no tiene competencia para hacerla»*.
 *
 * Y la consecuencia práctica, que es la que decide: *«si registrar el hecho implica
 * autoinculparse, **el hecho no se registra**. Y un accidente no registrado es peor que
 * cualquier atribución mal hecha»*.
 *
 * Lo que sí se muestra es la **determinación de responsabilidad** cuando la instancia competente
 * la emitió — como documento adjunto, con su número y su emisor.
 */
export default function IncidentesPantalla(): ReactElement {
  const expedientes = useQuery({ queryKey: ['incidentes'], queryFn: incidentes });
  const bienes = useQuery({ queryKey: ['bienes-no-recuperados'], queryFn: bienesNoRecuperados });

  if (expedientes.isError) {
    return (
      <Nota tono="riesgo" icono={<CircleAlert />}>
        No se pudieron cargar los expedientes de incidente.
      </Nota>
    );
  }

  const abiertos = expedientes.data?.filter((i) => i.estaAbierto) ?? [];
  const sinDesenlace = abiertos.filter((i) => i.esInterrupcionSinDesenlace);

  return (
    <div className="tw:flex tw:flex-col tw:gap-5">
      <header className="tw:flex tw:flex-col tw:gap-1">
        <h1 className="tw:text-xl tw:font-semibold tw:tracking-tight">
          Incidentes, siniestros y sanciones
        </h1>
        <p className="tw:text-sm tw:text-tinta-mid">
          El expediente registra <b>el hecho</b>: hora, lugar, odómetro, qué pasó. La
          responsabilidad la determina la instancia competente en su propio acto, y acá se
          adjunta cuando existe — <b>este módulo no la produce ni la pregunta</b>.
        </p>
      </header>

      {sinDesenlace.length > 0 && <PanelSinDesenlace incidentes={sinDesenlace} />}

      {bienes.data !== undefined && bienes.data.length > 0 && (
        <PanelDeBienes bienes={bienes.data} />
      )}

      {expedientes.isPending ? (
        <p className="tw:text-sm tw:text-tinta-mid">Cargando expedientes…</p>
      ) : (
        <PanelDeExpedientes expedientes={expedientes.data} />
      )}
    </div>
  );
}

/**
 * `RN-70` — *«ninguna misión con marca de interrupción sin desenlace puede quedar viva al cierre
 * del período»* (`RN-97` punto 4).
 *
 * Va arriba porque es lo que **impide cerrar el período**: quien abre esta pantalla en enero
 * necesita ver esto antes que nada.
 */
function PanelSinDesenlace({
  incidentes: lista,
}: {
  incidentes: ExpedienteDeIncidente[];
}): ReactElement {
  const cola = useQueryClient();
  const [abierto, setAbierto] = useState<string | null>(null);

  const resolver = useMutation({
    mutationFn: (v: { id: string; desenlace: DesenlaceDeLaInterrupcion }) =>
      registrarDesenlace(
        v.id,
        v.desenlace,
        // `RN-70` exige constancia de quién autorizó en los cuatro desenlaces.
        `Registrado desde la oficina por ACT-04.`,
        'P-TRANSPORTE',
      ),
    onSuccess: () => {
      avisar.exito('Desenlace registrado. La marca de interrupción se levantó.');
      void cola.invalidateQueries({ queryKey: ['incidentes'] });
      setAbierto(null);
    },
    onError: (error: Error) => { avisar.error(error.message); },
  });

  return (
    <Panel titulo={`${lista.length} interrupción(es) en ruta sin desenlace`}>
      <div className="tw:flex tw:flex-col tw:gap-3">
        <Nota tono="riesgo" icono={<TriangleAlert />}>
          Toda interrupción se resuelve con un <b>desenlace explícito</b>. Ninguna puede quedar
          viva al cierre del período — <b>y este bloqueo ya dispara</b> sobre el saldo de
          apertura.
        </Nota>

        {lista.map((i) => (
          <div
            key={i.id}
            className="tw:flex tw:flex-col tw:gap-2 tw:border-l-2 tw:border-riesgo-fg tw:pl-3"
          >
            <div className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-3 tw:text-sm">
              <span className="tw:font-medium">{TEXTO_DE_TIPO[i.tipo]}</span>
              <span className="tw:text-tinta-mid">{i.causa}</span>
              <span className="tw:flex tw:items-center tw:gap-1 tw:text-xs tw:text-tinta-mid">
                <Clock className="tw:size-3" aria-hidden />
                {soloFecha(i.fechaDelHecho)}
              </span>
              <span className="tw:text-xs tw:text-tinta-mid">
                a cargo de {i.responsableDeSeguimiento}
              </span>
            </div>

            <p className="tw:text-xs tw:text-tinta-mid">{i.descripcion}</p>

            {abierto === i.id ? (
              <div className="tw:flex tw:flex-wrap tw:gap-2">
                {DESENLACES.map(([valor, texto]) => (
                  <Boton
                    key={valor}
                    variante="secundario"
                    onClick={() => { resolver.mutate({ id: i.id, desenlace: valor }); }}
                    disabled={resolver.isPending}
                  >
                    {texto}
                  </Boton>
                ))}
              </div>
            ) : (
              <div>
                <Boton variante="secundario" onClick={() => { setAbierto(i.id); }}>
                  Registrar el desenlace
                </Boton>
              </div>
            )}
          </div>
        ))}
      </div>
    </Panel>
  );
}

/**
 * `RN-75` — *«el bien permanece en el registro patrimonial hasta su recuperación o su descargo
 * formal. **Nunca se elimina**»*.
 */
function PanelDeBienes({ bienes }: { bienes: BienFueraDelAlcance[] }): ReactElement {
  // Por antigüedad: lo más viejo primero. Un bien de tres años no puede quedar debajo de uno de
  // la semana pasada.
  const orden = [...bienes].sort((a, b) => b.diasFuera - a.diasFuera);

  return (
    <Panel titulo={`${bienes.length} bien(es) fuera del alcance de la institución`}>
      <div className="tw:flex tw:flex-col tw:gap-2">
        <p className="tw:text-xs tw:text-tinta-mid">
          Siguen en el registro patrimonial. Salen por <b>recuperación</b> o por <b>descargo
          formal</b> con acto de autoridad — nunca por borrado.
        </p>

        {orden.map((b) => (
          <div
            key={b.bien}
            className="tw:flex tw:flex-col tw:gap-1 tw:border-l-2 tw:border-aviso-fg tw:pl-3"
          >
            <div className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-3 tw:text-sm">
              <PackageX className="tw:size-3.5 tw:text-tinta-mid" aria-hidden />
              <span className="tw:font-medium">{b.descripcion}</span>
              {b.esElVehiculo && <Pastilla tono="riesgo">Es el vehículo</Pastilla>}
              <span className="tw:text-xs tw:text-tinta-mid">
                {b.diasFuera} días desde el {soloFecha(b.fechaDelHecho)}
              </span>
            </div>

            <span className="tw:text-xs tw:text-tinta-mid">
              {/* Nula es «no se sabe dónde está», que en una sustracción es lo normal. */}
              {b.autoridadCustodia !== null ? (
                <>
                  bajo custodia de {b.autoridadCustodia}
                  {b.numeroDeExpedienteExterno !== null &&
                    `, expediente ${b.numeroDeExpedienteExterno}`}
                </>
              ) : (
                <b className="tw:text-aviso-fg">sin ubicación conocida</b>
              )}
              {' · '}a cargo de {b.responsable}
            </span>
          </div>
        ))}
      </div>
    </Panel>
  );
}

function PanelDeExpedientes({
  expedientes,
}: {
  expedientes: ExpedienteDeIncidente[];
}): ReactElement {
  if (expedientes.length === 0) {
    return (
      <Panel>
        <p className="tw:text-sm tw:text-tinta-mid">
          No hay expedientes de incidente registrados.
        </p>
      </Panel>
    );
  }

  return (
    <Panel titulo={`${expedientes.length} expediente(s)`}>
      <div className="tw:flex tw:flex-col tw:gap-3">
        {expedientes.map((i) => (
          <div
            key={i.id}
            className={`tw:flex tw:flex-col tw:gap-1 tw:border-l-2 tw:pl-3 ${
              i.estaAbierto ? 'tw:border-borde' : 'tw:border-ok-fg'
            }`}
          >
            <div className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-3 tw:text-sm">
              <span className="tw:font-medium">{TEXTO_DE_TIPO[i.tipo]}</span>
              <span className="tw:text-tinta-mid">{i.causa}</span>
              <span className="tw:text-xs tw:text-tinta-mid">
                {soloFecha(i.fechaDelHecho)}
              </span>
              {i.interrumpe && <Pastilla tono="aviso">Interrumpió</Pastilla>}
              {!i.estaAbierto && <Pastilla tono="ok">Resuelto</Pastilla>}
            </div>

            <p className="tw:text-xs tw:text-tinta-mid">{i.descripcion}</p>

            <div className="tw:flex tw:flex-wrap tw:items-center tw:gap-x-3 tw:text-xs tw:text-tinta-mid">
              {/* Las dos fechas: `RN-70` admite captura sin conectividad, y la distancia es un
                  dato del expediente, no un error. */}
              {i.diasEntreElHechoYLaCaptura > 0 && (
                <span>
                  capturado {i.diasEntreElHechoYLaCaptura} día(s) después
                </span>
              )}

              {i.odometro !== null && <span>odómetro {i.odometro.toLocaleString('es-HN')}</span>}
              {i.ubicacion !== null && <span>{i.ubicacion}</span>}
              <span>registró {i.registra}</span>

              {i.desenlace !== null && (
                <span className="tw:text-ok-fg">
                  desenlace: {TEXTO_DE_DESENLACE[i.desenlace]}
                </span>
              )}
            </div>

            {/* Su ausencia no impide registrar el evento, pero genera obligación con plazo. */}
            {i.debeConstancia && (
              <Nota tono="aviso" icono={<ShieldAlert />}>
                Falta la <b>constancia de denuncia o acta ante autoridad</b>. Su ausencia no
                impidió registrar el hecho, pero deja una obligación con plazo.
              </Nota>
            )}

            {/* **El acto de otra instancia.** Lo más cerca que este módulo llega de la
                responsabilidad, y no lo produce: lo adjunta. */}
            {i.determinacion !== null && (
              <p className="tw:text-xs tw:text-tinta-mid">
                Determinación de responsabilidad:{' '}
                <span className="tw:font-mono">{i.determinacion.numero}</span> de{' '}
                {i.determinacion.instancia} — {i.determinacion.resolucion}
              </p>
            )}
          </div>
        ))}
      </div>
    </Panel>
  );
}

const TEXTO_DE_TIPO: Record<ExpedienteDeIncidente['tipo'], string> = {
  AveriaMecanica: 'Avería mecánica',
  Accidente: 'Accidente de tránsito',
  Sustraccion: 'Sustracción',
  RetencionPorAutoridad: 'Retención por autoridad',
  IncapacidadDelConductor: 'Incapacidad del conductor',
  ViaImpracticable: 'Vía impracticable',
  CondicionDeSeguridad: 'Condición de seguridad',
  Multa: 'Multa de tránsito',
  UsoIndebido: 'Uso indebido',
};

const TEXTO_DE_DESENLACE: Record<DesenlaceDeLaInterrupcion, string> = {
  Continuar: 'continuó',
  ContinuarConSustitucion: 'continuó con sustitución',
  RetornoAnticipado: 'retorno anticipado',
  RetornoSinVehiculo: 'retorno sin vehículo',
};

/** Los cuatro de `RN-70`. Un quinto «otro» dejaría la mitad sin decir cómo se resolvió. */
const DESENLACES: [DesenlaceDeLaInterrupcion, string][] = [
  ['Continuar', 'Continuó'],
  ['ContinuarConSustitucion', 'Continuó con sustitución'],
  ['RetornoAnticipado', 'Retorno anticipado'],
  ['RetornoSinVehiculo', 'Retorno sin vehículo'],
];
