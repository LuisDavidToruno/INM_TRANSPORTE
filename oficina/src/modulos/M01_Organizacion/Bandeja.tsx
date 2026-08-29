import type { ReactElement } from 'react';
import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { BellOff, CircleAlert, Inbox } from 'lucide-react';

import { Boton, Campo, Modal, Nota, Panel, Pastilla, Vacio, avisar } from '../../ui';
import type { Tono } from '../../ui';
import { BloqueoDuro, pedir } from '../../api/misiones';
import { diaYHora } from '../M06_Autorizacion/formato';

/**
 * La bandeja de tareas pendientes — <b>§5.3.B.3</b>.
 *
 * ── Por qué existe, y por qué no es un correo ───────────────────────────────
 * *«El sistema encola la acción como pendiente de resolución»*, y la misión *«queda visiblemente
 * pendiente en la bandeja de alguien»*. **La bandeja es el sistema de registro; el aviso es una
 * cortesía.** Un correo que no llega deja el trabajo perdido y nadie se entera; una bandeja que
 * se abre al entrar no depende de que haya red, servidor de correo ni teléfono — y esto se
 * despliega *on-premise* en instituciones donde nada de eso está garantizado.
 *
 * ── Lo que impide que sea un trámite ────────────────────────────────────────
 * **Quien originó la tarea no la puede cerrar.** La tarea existe porque a esa persona se le
 * impidió el acto; dejarla cerrarla convertiría el escalamiento en apretar un botón. El
 * escalamiento la puso en otra bandeja justamente para que decida alguien más.
 */
export default function Bandeja(): ReactElement {
  const [aCerrar, setACerrar] = useState<Tarea | null>(null);

  const { data, isPending, isError } = useQuery({
    queryKey: ['tareas'],
    queryFn: () => pedir<Bandeja>('/tareas'),
  });

  if (isError) {
    return (
      <Nota tono="riesgo" icono={<CircleAlert />}>
        No se pudo cargar la bandeja de tareas.
      </Nota>
    );
  }

  return (
    <div className="tw:flex tw:flex-col tw:gap-5">
      <header className="tw:flex tw:flex-col tw:gap-1">
        <h1 className="tw:text-xl tw:font-semibold tw:tracking-tight">Tareas pendientes</h1>
        <p className="tw:text-sm tw:text-tinta-mid">
          Actos que el sistema impidió y <b>escaló a alguien</b>. La misión no queda trabada por
          un problema de organización: queda pendiente en una bandeja concreta.
        </p>
      </header>

      {isPending ? (
        <p className="tw:text-sm tw:text-tinta-mid">Cargando la bandeja…</p>
      ) : (
        <>
          {/* Lo que impide que una bandeja llena se lea como gente que ignora su trabajo. */}
          {data.sinAvisar > 0 && (
            <Nota tono="aviso" icono={<BellOff />}>
              <b>
                {data.sinAvisar === 1
                  ? 'A 1 destinatario no se le avisó'
                  : `A ${data.sinAvisar} destinatarios no se les avisó`}
              </b>
              . <b>No es que no contestaran: es que nadie les escribió.</b> El motivo va en cada
              tarea, y son tres distintos —la institución no eligió canal, el canal elegido no
              está construido, o el envío falló— porque los arreglan personas distintas.
            </Nota>
          )}

          {data.diasDeLaMasVieja !== null && data.diasDeLaMasVieja >= 3 && (
            <Nota tono="riesgo" icono={<CircleAlert />}>
              La más vieja lleva <b>{data.diasDeLaMasVieja} días</b> esperando. Sin aviso
              automático, ese número sólo baja si alguien entra a mirar.
            </Nota>
          )}

          {data.tareas.length === 0 ? (
            <Vacio
              icono={<Inbox />}
              titulo="No hay nada pendiente"
              descripcion="Ningún acto quedó escalado. Es lo esperable, y no prueba que el escalamiento funcione: prueba que no se ha necesitado."
            />
          ) : (
            <Panel titulo={`${data.pendientes} pendiente(s) de ${data.tareas.length}`}>
              <ul className="tw:flex tw:flex-col tw:gap-4">
                {data.tareas.map((t) => (
                  <Fila key={t.id} tarea={t} onCerrar={() => setACerrar(t)} />
                ))}
              </ul>
            </Panel>
          )}
        </>
      )}

      {aCerrar && <DialogoDeCierre tarea={aCerrar} onCerrar={() => setACerrar(null)} />}
    </div>
  );
}

function Fila({ tarea, onCerrar }: { tarea: Tarea; onCerrar(): void }): ReactElement {
  const pendiente = tarea.estado === 'Pendiente';

  return (
    <li
      className={`tw:flex tw:flex-col tw:gap-1 tw:border-l-2 tw:pl-3 ${
        pendiente ? 'tw:border-riesgo-fg' : 'tw:border-linea'
      }`}
    >
      <div className="tw:flex tw:flex-wrap tw:items-center tw:justify-between tw:gap-2">
        <div className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-3 tw:text-sm">
          <span className="tw:font-medium">{tarea.asunto}</span>
          <Pastilla tono={TONO[tarea.estado] ?? 'neutro'}>{tarea.estado}</Pastilla>

          {/* Los días sólo corren mientras espera: una resuelta llevó los días que llevó. */}
          {pendiente && tarea.diasEsperando > 0 && (
            <span className="tw:text-xs tw:text-aviso-fg">
              {tarea.diasEsperando} días esperando
            </span>
          )}
        </div>

        {pendiente && (
          <Boton variante="secundario" tamano="sm" onClick={onCerrar}>
            Atender
          </Boton>
        )}
      </div>

      <span className="tw:text-xs tw:text-tinta-mid">{tarea.detalle}</span>

      <span className="tw:text-xs tw:text-tinta-mid">
        le toca a{' '}
        <b>
          {/* Nulo es Gerencia Administrativa: el último recurso no es un puesto de la
              jerarquía de quien quedó bloqueado. */}
          {tarea.puestoDestino ?? 'Gerencia Administrativa (ACT-08)'}
        </b>
        {tarea.personasDestino.length > 0 && ` · ${tarea.personasDestino}`} · lo originó{' '}
        {tarea.quienLaOrigino} el {diaYHora(tarea.momento)}
      </span>

      {/* Nulo es «no se avisó», no «se avisó y no contestaron» — y se dice POR QUÉ. */}
      {pendiente && tarea.avisos.map((a) => (
        <span
          key={a.destinatario}
          className={`tw:text-xs ${
            a.resultado === 'Entregado' ? 'tw:text-tinta-mid' : 'tw:text-aviso-fg'
          }`}
        >
          {a.resultado === 'Entregado' ? (
            <>
              avisado a {a.destinatario} por{' '}
              {TEXTO_DEL_CANAL[a.canal ?? ''] ?? a.canal}
            </>
          ) : (
            <>
              <b>no se le avisó a {a.destinatario}</b> — {a.detalle}
            </>
          )}
        </span>
      ))}

      {!pendiente && tarea.resolucion !== null && (
        <span className="tw:text-xs tw:text-tinta-mid">
          {tarea.estado === 'Descartada' ? 'descartada' : 'resuelta'} por {tarea.resuelve}:{' '}
          {tarea.resolucion}
        </span>
      )}
    </li>
  );
}

/**
 * Atender una tarea.
 *
 * ── «Resolver» y «descartar» son dos botones y no una casilla ───────────────
 * Porque son cosas distintas: descartar dice que **nadie tuvo que hacer nada**, y un reporte que
 * las junte no puede distinguir el control que operó del que se volvió innecesario.
 */
function DialogoDeCierre({
  tarea,
  onCerrar,
}: {
  tarea: Tarea;
  onCerrar(): void;
}): ReactElement {
  const cliente = useQueryClient();
  const [motivo, setMotivo] = useState('');

  const operacion = useMutation({
    mutationFn: (descartar: boolean) =>
      pedir(`/tareas/${tarea.id}/cerrar`, {
        method: 'POST',
        body: JSON.stringify({
          ejecuta: 'Rolando Discua',
          motivo,
          descartar,
          momento: new Date().toISOString(),
        }),
      }),
    onSuccess: async () => {
      avisar.exito('La tarea quedó cerrada, con lo que se hizo escrito.');
      await cliente.invalidateQueries({ queryKey: ['tareas'] });
      onCerrar();
    },
    onError: (e) => {
      // El caso que más importa: quien originó la tarea intentando cerrarla.
      if (e instanceof BloqueoDuro) {
        avisar.error(e.paraMostrar);
        return;
      }

      avisar.error('No se pudo cerrar la tarea. Quedó como estaba.');
    },
  });

  const listo = motivo.trim().length >= 8 && !operacion.isPending;

  return (
    <Modal
      abierto
      titulo={tarea.asunto}
      descripcion={`Escalada el ${diaYHora(tarea.momento)} porque a ${tarea.quienLaOrigino} se le impidió el acto. Quien la originó no la puede cerrar: el escalamiento la puso acá para que decida otra persona.`}
      onCerrar={onCerrar}
      acciones={
        <div className="tw:flex tw:flex-wrap tw:gap-2">
          <Boton
            variante="secundario"
            disabled={!listo}
            onClick={() => operacion.mutate(true)}
          >
            Ya no aplica
          </Boton>
          <Boton
            variante="primario"
            disabled={!listo}
            cargando={operacion.isPending}
            onClick={() => operacion.mutate(false)}
          >
            Resolver
          </Boton>
        </div>
      }
    >
      <div className="tw:flex tw:flex-col tw:gap-4">
        <Nota tono="info">{tarea.detalle}</Nota>

        <Campo
          etiqueta="Qué se hizo"
          obligatorio
          ayuda="Lo lee quien audite. «Lo autorizó la Gerencia por oficio 2026-31» y «ya no hacía falta» son cosas distintas, y sin esto dejan el mismo rastro vacío."
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

        <Nota tono="aviso">
          <b>«Ya no aplica» no es lo mismo que «resolver».</b> El primero dice que nadie tuvo
          que hacer nada —el expediente siguió otro camino—; el segundo, que alguien decidió.
          Juntarlos impediría distinguir el control que operó del que se volvió innecesario.
        </Nota>
      </div>
    </Modal>
  );
}

/**
 * El canal como lo llamaría quien opera.
 *
 * <b><c>SoloBandeja</c> no es «ningún canal»</b>: es un canal legítimo y puede ser el único
 * posible. En una delegación sin señal el correo y el mensaje de texto no llegan, y más de dos
 * millones de personas del área rural no tienen internet. Lo que declara es que el aviso depende
 * de que la persona entre al sistema.
 */
const TEXTO_DEL_CANAL: Record<string, string> = {
  SoloBandeja: 'la bandeja del sistema',
  CorreoInstitucional: 'correo institucional',
  MensajeDeTexto: 'mensaje de texto',
};

const TONO: Record<string, Tono> = {
  Pendiente: 'riesgo',
  Resuelta: 'ok',
  Descartada: 'neutro',
};

interface Tarea {
  id: string;
  tipo: string;
  asunto: string;
  detalle: string;
  expediente: string;
  quienLaOrigino: string;
  /** **Nulo es Gerencia Administrativa**, el último recurso del escalamiento. */
  puestoDestino: string | null;
  personasDestino: string;
  momento: string;
  estado: 'Pendiente' | 'Resuelta' | 'Descartada';
  /** **Nulo es «no se avisó»**, no «se avisó y no contestaron». */
  notificado: string | null;
  resuelve: string | null;
  resuelta: string | null;
  resolucion: string | null;
  diasEsperando: number;
  /**
   * Los intentos de aviso, uno por destinatario.
   *
   * **Uno por destinatario y no uno por tarea**: un puesto puede estar coocupado durante un
   * traspaso, y una sola entrada diría que se avisó cuando a una de las dos personas no le
   * llegó.
   */
  avisos: {
    destinatario: string;
    /** **Nulo es «la institución no fijó el canal»**, que no es lo mismo que un canal que falló. */
    canal: string | null;
    resultado: string;
    detalle: string | null;
  }[];
}

interface Bandeja {
  pendientes: number;
  /** Cuántas no se avisaron. Hoy son todas: no hay canal. */
  sinAvisar: number;
  /** **Nulo cuando no hay pendientes**, que no es cero días. */
  diasDeLaMasVieja: number | null;
  tareas: Tarea[];
}
