import type { ReactElement } from 'react';
import { useQuery } from '@tanstack/react-query';
import { CircleAlert, ShieldAlert } from 'lucide-react';

import { Nota, Panel, Pastilla, Vacio } from '../../ui';
import type { Tono } from '../../ui';
import { pedir } from '../../api/misiones';
import { diaYHora } from '../M06_Autorizacion/formato';

/**
 * `PT-091` — Intentos bloqueados por segregación de funciones.
 *
 * ── Por qué esta pantalla muestra lo que NO pasó ────────────────────────────
 * §5.3.B.2: *«el intento bloqueado es información de control, no ruido. Un mismo usuario
 * intentando quince veces autorizar sus propias solicitudes es exactamente lo que Auditoría
 * Interna quiere ver»*.
 *
 * Un sistema que sólo guarda lo consumado no puede contestar la pregunta que el TSC hace: **si
 * el control operó**. Sin esta pista, un bloqueo perfecto y un bloqueo que nunca se activó se
 * ven exactamente igual — no hay rastro de ninguno de los dos.
 *
 * ── La reincidencia va primero, y no es lo mismo que el total ───────────────
 * Un intento aislado suele ser una delegación chica resolviendo como puede. **Quince intentos
 * de la misma persona sobre el mismo par es otra cosa**, y ordenar la lista por fecha la
 * esconde entre los aislados.
 */
export default function IntentosBloqueados(): ReactElement {
  const { data, isPending, isError } = useQuery({
    queryKey: ['intentos-bloqueados'],
    queryFn: () => pedir<Pista>('/segregacion/intentos'),
  });

  if (isError) {
    return (
      <Nota tono="riesgo" icono={<CircleAlert />}>
        No se pudo cargar la pista de intentos bloqueados.
      </Nota>
    );
  }

  return (
    <div className="tw:flex tw:flex-col tw:gap-5">
      <header className="tw:flex tw:flex-col tw:gap-1">
        <h1 className="tw:text-xl tw:font-semibold tw:tracking-tight">
          Intentos bloqueados por segregación
        </h1>
        <p className="tw:text-sm tw:text-tinta-mid">
          Actos que el sistema <b>impidió consumar</b> porque quien los intentaba ya había
          ejercido una función incompatible sobre el mismo expediente. <b>No se guardó el
          acto</b>; sí el intento.
        </p>
      </header>

      {isPending ? (
        <p className="tw:text-sm tw:text-tinta-mid">Cargando la pista…</p>
      ) : data.total === 0 ? (
        // Cero no es «el control funciona»: es que nadie lo activó todavía. Decirlo evita que
        // una pantalla vacía se lea como un certificado.
        <Vacio
          icono={<ShieldAlert />}
          titulo="Ningún intento bloqueado"
          descripcion="Nadie ha intentado ejercer dos funciones incompatibles sobre el mismo expediente. Es lo esperable, y no prueba por sí solo que el control esté operando: prueba que no se ha necesitado."
        />
      ) : (
        <>
          {/* Lo que Auditoría busca primero. */}
          {data.reincidentes.length > 0 && (
            <Nota tono="riesgo" icono={<ShieldAlert />}>
              <b>
                {data.reincidentes.length === 1
                  ? '1 persona reincidió'
                  : `${data.reincidentes.length} personas reincidieron`}
              </b>
              : {data.reincidentes.map((r) => `${r.persona} (${r.intentos})`).join(', ')}. Un
              intento aislado suele ser una delegación chica resolviendo como puede;{' '}
              <b>la reincidencia es otra cosa</b>.
            </Nota>
          )}

          <Panel titulo="Por par de incompatibilidad">
            <div className="tw:flex tw:flex-wrap tw:gap-2">
              {data.porPar.map((p) => (
                <Pastilla key={p.par} tono="aviso">
                  {p.par} · {p.intentos}
                </Pastilla>
              ))}
            </div>
          </Panel>

          <Panel titulo={`${data.total} intento(s)`}>
            <ul className="tw:flex tw:flex-col tw:gap-3">
              {data.intentos.map((i) => (
                <li
                  key={i.id}
                  className="tw:flex tw:flex-col tw:gap-0.5 tw:border-l-2 tw:border-riesgo-fg tw:pl-3"
                >
                  <div className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-3 tw:text-sm">
                    <span className="tw:font-mono tw:font-medium">{i.par}</span>
                    <span className="tw:font-medium">{i.quien}</span>
                    <span className="tw:text-tinta-mid">
                      quiso ejercer {enPalabras(i.pretendia)} sobre {i.expediente}
                    </span>
                  </div>

                  <span className="tw:text-xs tw:text-tinta-mid">
                    ya había ejercido {enPalabras(i.chocaCon)} — {i.referencia}
                  </span>

                  <span className="tw:text-xs tw:text-tinta-mid">
                    {diaYHora(i.momento)} ·{' '}
                    {/* Nulo es «no se supo», no «desde el servidor». */}
                    {i.origen === null ? (
                      <span className="tw:italic">origen no registrado</span>
                    ) : (
                      <span className="tw:font-mono">{i.origen}</span>
                    )}
                  </span>

                  <Destino intento={i} />
                </li>
              ))}
            </ul>
          </Panel>
        </>
      )}

      {/* Lo que sigue faltando, dicho con precisión: el destino se resuelve, la notificación no. */}
      <Nota tono="info">
        <b>El destino se resuelve; la notificación no existe todavía.</b> §5.3.B.3 pide que el
        acto quede <i>«visiblemente pendiente en la bandeja de alguien»</i> y que el sistema{' '}
        <i>«notifique al destinatario que corresponda»</i>. Los tres saltos ya operan —puesto
        superior, respaldo de sede, Gerencia Administrativa— y quedan registrados acá, pero{' '}
        <b>a quien le toca resolverlo no se le avisa</b>: tiene que abrir esta pantalla. Las
        notificaciones no están construidas en ningún módulo.
      </Nota>
    </div>
  );
}

/**
 * La función dicha como la diría quien opera.
 *
 * El servidor la publica por el nombre del enum —`ApruebaFondo`— porque es lo que va a la pista
 * de auditoría y tiene que ser estable. **En minúscula se lee «apruebafondo»**, que no es
 * castellano, y esta pantalla la lee Auditoría Interna.
 *
 * Si el servidor agrega una función que esta pantalla no conoce se muestra el identificador
 * crudo: uno sin traducir se ve raro, uno escondido deja la frase sin sujeto.
 */
function enPalabras(funcion: string): string {
  return EN_PALABRAS[funcion] ?? funcion;
}

const EN_PALABRAS: Record<string, string> = {
  Solicita: 'la solicitud',
  Autoriza: 'la autorización',
  Despacha: 'el despacho',
  EntregaFondo: 'la entrega del fondo',
  Liquida: 'la liquidación',
  Conduce: 'la conducción',
  SolicitaFondo: 'la solicitud del fondo',
  ApruebaFondo: 'la aprobación del fondo',
  HabilitaLicencia: 'la habilitación de la licencia',
  Custodia: 'la custodia del vehículo',
  ProponeDescargo: 'la propuesta de descargo',
  ApruebaDescargo: 'la aprobación del descargo',
  OrdenaMantenimiento: 'la orden de mantenimiento',
  RecibeConforme: 'la recepción conforme',
  EmiteOrdenDeMision: 'la emisión de la Orden de Misión',
  Audita: 'la auditoría',
  Administra: 'la administración del sistema',
};

/**
 * A dónde quedó pendiente el acto — §5.3.B.3.
 *
 * ── Por qué se muestra POR QUÉ no fue a los saltos anteriores ───────────────
 * Un escalamiento que siempre termina en Gerencia Administrativa sin explicarse se lee como que
 * la jerarquía no sirve. Lo que puede estar pasando es que **el puesto superior esté vacante**,
 * que es un problema de organización que alguien tiene que resolver — y sólo se ve si se dice.
 */
function Destino({
  intento,
}: {
  intento: Pista['intentos'][number];
}): ReactElement {
  // **Nulo es «no se resolvió»**, no «fue a Gerencia»: los intentos anteriores al escalamiento
  // existen, y decir que fueron al último recurso sería inventar un encaminamiento.
  if (intento.salto === null) {
    return (
      <span className="tw:text-xs tw:italic tw:text-tinta-mid">
        sin destino registrado — el intento es anterior al escalamiento
      </span>
    );
  }

  return (
    <span className="tw:flex tw:flex-wrap tw:items-center tw:gap-x-2 tw:text-xs">
      <Pastilla tono={TONO_DEL_SALTO[intento.salto] ?? 'neutro'}>
        {TEXTO_DEL_SALTO[intento.salto] ?? intento.salto}
      </Pastilla>

      <span className="tw:text-tinta-mid">
        pendiente en{' '}
        <span className="tw:font-mono">
          {intento.escalaA ?? 'Gerencia Administrativa (ACT-08)'}
        </span>
      </span>

      {intento.porQueNoAntes !== null && (
        <span className="tw:text-aviso-fg">— {intento.porQueNoAntes}</span>
      )}
    </span>
  );
}

/**
 * El orden de los tres saltos, en tono.
 *
 * **El último recurso se pinta como riesgo**, no como neutro: cuando todo termina en Gerencia
 * Administrativa lo que hay es un problema de organización, no un encaminamiento normal.
 */
const TEXTO_DEL_SALTO: Record<string, string> = {
  PuestoSuperior: 'Puesto superior',
  RespaldoDeSede: 'Respaldo de sede',
  GerenciaAdministrativa: 'Último recurso',
};

const TONO_DEL_SALTO: Record<string, Tono> = {
  PuestoSuperior: 'ok',
  RespaldoDeSede: 'aviso',
  GerenciaAdministrativa: 'riesgo',
};

interface Pista {
  total: number;
  reincidentes: { persona: string; intentos: number }[];
  porPar: { par: string; intentos: number }[];
  intentos: {
    id: string;
    quien: string;
    pretendia: string;
    expediente: string;
    par: string;
    chocaCon: string;
    referencia: string;
    momento: string;
    /** **Nulo es «no se supo»**, no «desde el servidor». */
    origen: string | null;
    /**
     * Por cuál de los tres saltos de §5.3.B.3 se resolvió el destino.
     *
     * **Nulo es «no se resolvió»** —el intento es anterior al escalamiento— y no «fue a
     * Gerencia». Los intentos viejos existen, y decir que fueron al último recurso sería
     * inventar un encaminamiento que nunca ocurrió.
     */
    salto: string | null;
    /** El puesto donde quedó pendiente. **Nulo cuando el destino es Gerencia Administrativa**. */
    escalaA: string | null;
    /** Qué falló en los saltos previos. Nulo cuando el primero sirvió. */
    porQueNoAntes: string | null;
  }[];
}
