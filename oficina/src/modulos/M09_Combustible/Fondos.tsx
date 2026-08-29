import type { ReactElement } from 'react';
import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { CircleAlert, Fuel } from 'lucide-react';

import {
  Boton,
  Campo,
  MenuAcciones,
  Modal,
  Nota,
  Panel,
  Pastilla,
  RangoFechas,
  Vacio,
  avisar,
} from '../../ui';
import type { Tono } from '../../ui';
import { BloqueoDuro } from '../../api/misiones';
import {
  TEXTO_DE_FONDO,
  ampliarFondo,
  aprobarFondo,
  cerrarFondo,
  fondos as pedirFondos,
  lempiras,
  solicitarFondo,
} from '../../api/combustible';
import type { Fondo } from '../../api/combustible';
import { momentoCompleto, soloFecha } from '../M06_Autorizacion/formato';
import CampoDeActor from './CampoDeActor';
import PanelDeArqueo from './PanelDeArqueo';

/**
 * El fondo de combustible del período — `RN-26`.
 *
 * ── Por qué esto NO es una tabla ────────────────────────────────────────────
 * Porque no se comparan fondos: se mira <b>uno</b> y se actúa sobre él. Hay un fondo por
 * dependencia y por mes, y la pregunta al abrir es <i>«¿cuánto queda del de Choluteca?»</i>,
 * no <i>«¿cuál de los ocho tiene más?»</i>. Una tabla optimiza la comparación y entierra el
 * saldo entre columnas de igual peso.
 *
 * ── El saldo es lo único que se lee de lejos ────────────────────────────────
 * Todo lo demás —aprobado, comprometido, partida, período— explica el saldo. Es la cifra
 * que decide si una misión sale, y la que Gerencia Administrativa cita.
 *
 * ⚠️ <b>Ninguna cifra se calcula acá.</b> Vienen del servidor, donde son la resta sobre
 * asientos de `RN-26`. Restar en el cliente produciría un segundo número con la misma
 * apariencia de autoridad y con derecho a discrepar.
 */
export default function Fondos(): ReactElement {
  const [solicitando, setSolicitando] = useState(false);
  const [accion, setAccion] = useState<{ fondo: Fondo; tipo: Accion } | null>(null);

  const { data, isPending, isError } = useQuery({ queryKey: ['fondos'], queryFn: pedirFondos });

  if (isError) {
    return (
      <Nota tono="riesgo" icono={<CircleAlert />}>
        No se pudieron cargar los fondos de combustible.
      </Nota>
    );
  }

  const lista = data ?? [];
  const sinPartida = lista.filter((f) => f.partida === null && f.estado !== 'Cerrado');
  const agotados = lista.filter((f) => f.estado !== 'Cerrado' && f.saldo <= 0 && f.aprobado > 0);

  return (
    <div className="tw:flex tw:flex-col tw:gap-5">
      <header className="tw:flex tw:items-start tw:justify-between tw:gap-4">
        <div className="tw:flex tw:flex-col tw:gap-1">
          <h1 className="tw:text-xl tw:font-semibold tw:tracking-tight">Fondo de combustible</h1>
          <p className="tw:text-sm tw:text-tinta-mid">
            {isPending
              ? 'Cargando…'
              : `${lista.length} ${lista.length === 1 ? 'fondo' : 'fondos'} registrados.`}
          </p>
        </div>

        <Boton onClick={() => setSolicitando(true)}>Solicitar fondo</Boton>
      </header>

      {agotados.length > 0 && (
        <Nota tono="riesgo" icono={<CircleAlert />}>
          {agotados.length === 1 && agotados[0]
            ? `El fondo de ${agotados[0].ambitoDeclarado} no tiene saldo`
            : `${agotados.length} fondos no tienen saldo`}
          : <b>no se puede emitir ni un vale más</b>. La salida es la ampliación, que sigue el
          mismo circuito de aprobación — no hay vía corta, porque el control se perdería
          justo cuando más presión hay.
        </Nota>
      )}

      {sinPartida.length > 0 && (
        <Nota tono="aviso">
          {sinPartida.length === 1 ? '1 fondo no tiene' : `${sinPartida.length} fondos no tienen`}{' '}
          partida presupuestaria, y <b>su cierre está bloqueado</b> hasta que se complete. La
          estructura la define ARGOS: si el espejo no la tiene, se completa al cerrar — no se
          inventa un código.
        </Nota>
      )}

      {isPending ? (
        <p className="tw:text-sm tw:text-tinta-mid">Cargando fondos…</p>
      ) : lista.length === 0 ? (
        <Vacio
          icono={<Fuel />}
          titulo="No hay ningún fondo registrado"
          descripcion="Sin fondo aprobado vigente no se puede emitir un solo vale. Empiece por solicitar el del período."
          accion={<Boton onClick={() => setSolicitando(true)}>Solicitar fondo</Boton>}
        />
      ) : (
        <div className="tw:flex tw:flex-col tw:gap-4">
          {lista.map((f) => (
            <TarjetaDeFondo
              key={f.id}
              fondo={f}
              onAccion={(tipo) => setAccion({ fondo: f, tipo })}
            />
          ))}
        </div>
      )}

      {/* El arqueo va **debajo del fondo y en la misma pantalla**, no en otra ruta. Lo que
          está afuera es parte del saldo del período: separarlos deja el cuadre de Gerencia
          Administrativa incompleto en la única vista donde se mira. */}
      <div className="tw:mt-2 tw:border-t tw:border-borde tw:pt-5">
        <PanelDeArqueo />
      </div>

      {solicitando && <DialogoSolicitar onCerrar={() => setSolicitando(false)} />}

      {accion && (
        <DialogoDeAccion
          fondo={accion.fondo}
          tipo={accion.tipo}
          onCerrar={() => setAccion(null)}
        />
      )}
    </div>
  );
}

type Accion = 'aprobar' | 'ampliar' | 'cerrar';

const TONO_DE_FONDO: Record<string, Tono> = {
  Solicitado: 'info',
  Aprobado: 'ok',
  Entregado: 'ok',
  Agotado: 'riesgo',
  Cerrado: 'neutro',
};

function TarjetaDeFondo({
  fondo,
  onAccion,
}: {
  fondo: Fondo;
  onAccion(tipo: Accion): void;
}): ReactElement {
  const [verDiario, setVerDiario] = useState(false);

  // Lo comprometido no viene como campo: es la diferencia entre el techo y lo que queda, y
  // el servidor ya calculó las dos puntas. Restar acá no inventa una cifra nueva — nombra la
  // que ya está implícita entre ellas.
  const comprometido = fondo.aprobado - fondo.saldo;
  const proporcion = fondo.aprobado > 0 ? Math.min(1, comprometido / fondo.aprobado) : 0;
  const sinSaldo = fondo.aprobado > 0 && fondo.saldo <= 0;

  // Qué se puede hacer sale del ESTADO, no de un permiso de pantalla: aprobar sólo tiene
  // sentido sobre un solicitado, y de cerrado no se sale. Ofrecer una acción que el
  // servidor va a rechazar es peor que no ofrecerla — enseña a ignorar los errores.
  const puedeAprobar = fondo.estado === 'Solicitado';
  const puedeMover = fondo.estado !== 'Solicitado' && fondo.estado !== 'Cerrado';

  return (
    <Panel>
      <div className="tw:flex tw:flex-col tw:gap-4">
      <div className="tw:flex tw:items-start tw:justify-between tw:gap-4">
        <div className="tw:flex tw:flex-col tw:gap-1">
          <div className="tw:flex tw:items-center tw:gap-2">
            <h2 className="tw:font-medium">{fondo.ambitoDeclarado}</h2>
            <Pastilla tono={TONO_DE_FONDO[fondo.estado] ?? 'neutro'}>
              {TEXTO_DE_FONDO[fondo.estado] ?? fondo.estado}
            </Pastilla>
          </div>
          <p className="tw:text-xs tw:text-tinta-mid">
            {soloFecha(fondo.desde)} — {soloFecha(fondo.hasta)} · ámbito {fondo.ambito}
          </p>
        </div>

        {(puedeAprobar || puedeMover) && (
          <MenuAcciones etiqueta={`Acciones del fondo de ${fondo.ambitoDeclarado}`}>
            {puedeAprobar && (
              <Boton variante="fantasma" tamano="sm" onClick={() => onAccion('aprobar')}>
                Aprobar el fondo
              </Boton>
            )}
            {puedeMover && (
              <>
                <Boton variante="fantasma" tamano="sm" onClick={() => onAccion('ampliar')}>
                  Ampliar el fondo
                </Boton>
                <Boton variante="fantasma" tamano="sm" onClick={() => onAccion('cerrar')}>
                  Cerrar el período
                </Boton>
              </>
            )}
          </MenuAcciones>
        )}
      </div>

      {/* El saldo es lo único que se lee de lejos. Todo lo demás lo explica. */}
      <div className="tw:flex tw:flex-wrap tw:items-end tw:gap-x-8 tw:gap-y-3">
        <div className="tw:flex tw:flex-col">
          <span className="tw:text-xs tw:uppercase tw:tracking-wide tw:text-tinta-mid">
            Saldo disponible
          </span>
          <span
            className={`tw:font-mono tw:text-2xl tw:tabular-nums ${
              sinSaldo ? 'tw:text-riesgo-fg' : ''
            }`}
          >
            {fondo.estado === 'Solicitado' ? '—' : lempiras(fondo.saldo)}
          </span>
        </div>

        <Cifra rotulo="Aprobado" valor={lempiras(fondo.aprobado)} />
        <Cifra rotulo="Comprometido" valor={lempiras(comprometido)} />

        <div className="tw:flex tw:flex-col">
          <span className="tw:text-xs tw:uppercase tw:tracking-wide tw:text-tinta-mid">
            Partida
          </span>
          <span className="tw:font-mono tw:text-sm tw:tabular-nums">
            {fondo.partida ?? (
              // Nula no es un campo vacío: es lo que impide cerrar el período.
              <span className="tw:text-aviso-fg">Pendiente — bloquea el cierre</span>
            )}
          </span>
        </div>
      </div>

      {/* La proporción, que es lo que convierte «quedan 2,500» en una decisión. La cifra sola
          no dice si eso es holgado o es el último vale del mes. */}
      {fondo.aprobado > 0 && (
        <div className="tw:flex tw:flex-col tw:gap-1">
          <div className="tw:h-1.5 tw:overflow-hidden tw:rounded-full tw:bg-inset">
            <div
              className={`tw:h-full ${sinSaldo ? 'tw:bg-riesgo-fg' : 'tw:bg-acento'}`}
              style={{ width: `${proporcion * 100}%` }}
            />
          </div>
          <p className="tw:text-xs tw:text-tinta-mid">
            {Math.round(proporcion * 100)}% comprometido
          </p>
        </div>
      )}

      <div className="tw:flex tw:items-center tw:justify-between tw:gap-4 tw:border-t tw:border-linea-suave tw:pt-3">
        <p className="tw:text-xs tw:text-tinta-mid">
          Solicita <b>{fondo.solicita}</b>
          {fondo.aprueba && (
            <>
              {' '}
              · aprueba <b>{fondo.aprueba}</b>
            </>
          )}
        </p>

        <Boton variante="fantasma" tamano="sm" onClick={() => setVerDiario((v) => !v)}>
          {verDiario ? 'Ocultar el diario' : `Ver el diario (${fondo.diario.length})`}
        </Boton>
      </div>

      {verDiario && (
        <ol className="tw:flex tw:flex-col tw:gap-2 tw:text-xs">
          {fondo.diario.map((m, i) => (
            <li key={`${m.movimiento}-${i}`} className="tw:flex tw:flex-col tw:gap-0.5">
              <div className="tw:flex tw:items-center tw:gap-2">
                <code className="tw:font-mono tw:text-[11px] tw:text-tinta-mid">
                  {m.movimiento}
                </code>
                <span className="tw:font-medium">
                  {TEXTO_DE_FONDO[m.destino] ?? m.destino}
                </span>
                {m.monto !== null && (
                  <span className="tw:font-mono tw:tabular-nums tw:text-ok-fg">
                    +{lempiras(m.monto)}
                  </span>
                )}
              </div>
              <span className="tw:text-tinta-mid">
                {m.ejecuta} · {momentoCompleto(m.momento)}
              </span>
              {m.motivo && <span className="tw:text-tinta-mid">{m.motivo}</span>}
            </li>
          ))}
        </ol>
      )}
      </div>
    </Panel>
  );
}

function Cifra({ rotulo, valor }: { rotulo: string; valor: string }): ReactElement {
  return (
    <div className="tw:flex tw:flex-col">
      <span className="tw:text-xs tw:uppercase tw:tracking-wide tw:text-tinta-mid">{rotulo}</span>
      <span className="tw:font-mono tw:text-sm tw:tabular-nums">{valor}</span>
    </div>
  );
}

// ── Diálogos ────────────────────────────────────────────────────────────────

function ahora(): string {
  return new Date().toISOString();
}

function usarInvalidacion() {
  const cliente = useQueryClient();
  return () => cliente.invalidateQueries({ queryKey: ['fondos'] });
}

function reportar(e: unknown, porDefecto: string): void {
  // El servidor rechaza por `RN-26` —segregación, saldo, partida, asignaciones vivas— y el
  // mensaje es lo único que dice cuál de las cuatro fue.
  if (e instanceof BloqueoDuro) {
    avisar.error(e.message);
    return;
  }
  avisar.error(porDefecto);
}

function DialogoSolicitar({ onCerrar }: { onCerrar(): void }): ReactElement {
  const invalidar = usarInvalidacion();
  const [solicita, setSolicita] = useState('');
  const [ambitoDeclarado, setAmbito] = useState('');
  const [desde, setDesde] = useState('');
  const [hasta, setHasta] = useState('');
  const [monto, setMonto] = useState('');
  const [justificacion, setJustificacion] = useState('');

  const operacion = useMutation({
    mutationFn: () =>
      solicitarFondo({
        id: ulid(),
        // `[C]` El ámbito por delegación no está confirmado; hoy se solicita por dependencia,
        // que es lo que la misión declara y contra lo que `RN-26` compara al emitir.
        ambito: 'Dependencia',
        ambitoDeclarado: ambitoDeclarado.trim(),
        desde,
        hasta,
        solicita: solicita.trim(),
        monto: Number(monto),
        justificacion: justificacion.trim(),
        momento: ahora(),
      }),
    onSuccess: async () => {
      avisar.exito('Fondo solicitado. Falta la aprobación de Gerencia Administrativa.');
      await invalidar();
      onCerrar();
    },
    onError: (e) => reportar(e, 'No se pudo registrar la solicitud.'),
  });

  const completo =
    solicita.trim() !== '' &&
    ambitoDeclarado.trim() !== '' &&
    desde !== '' &&
    hasta !== '' &&
    Number(monto) > 0 &&
    justificacion.trim().length >= 10;

  return (
    <Modal
      abierto
      titulo="Solicitar el fondo del período"
      descripcion="Lo solicita Transporte y lo aprueba Gerencia Administrativa. Solicitar no crea saldo: el monto aprobado es el que manda."
      onCerrar={onCerrar}
      acciones={
        <Boton
          disabled={!completo || operacion.isPending}
          cargando={operacion.isPending}
          onClick={() => operacion.mutate()}
        >
          Solicitar
        </Boton>
      }
    >
      <div className="tw:flex tw:flex-col tw:gap-4">
        <CampoDeActor
          valor={solicita}
          onCambiar={setSolicita}
          etiqueta="Quién solicita"
          ayuda="Jefatura de Transporte. No va a poder aprobar este mismo fondo ni liquidarlo al cierre: son tres funciones y tres personas."
        />

        <Campo
          etiqueta="Dependencia o delegación"
          obligatorio
          ayuda="Tiene que coincidir con la dependencia de las misiones que va a cubrir: una misión no se imputa al fondo de otra delegación."
        >
          {(props) => (
            <input
              {...props}
              value={ambitoDeclarado}
              onChange={(e) => setAmbito(e.target.value)}
            />
          )}
        </Campo>

        {/* **Un rango, no dos campos sueltos.** El sistema de diseño lo dice: si el dato
            tiene un «hasta», es un período. Y acá importa más que en otros lados — el
            período es lo que hace del fondo un objeto de período y no de misión, que es
            toda la distinción que sostiene la segregación de `RN-26`. */}
        <Campo
          etiqueta="Período que cubre"
          obligatorio
          ayuda="Un vale sólo se emite si hay fondo vigente a la fecha del hecho, no a la de captura."
        >
          {() => (
            <RangoFechas
              desde={desde}
              hasta={hasta}
              etiqueta="Período del fondo"
              onCambiar={(d, h) => {
                setDesde(d);
                setHasta(h);
              }}
            />
          )}
        </Campo>

        <Campo
          etiqueta="Monto solicitado"
          obligatorio
          ayuda="Lo que se pide. Lo que se aprueba es otra cifra, y es la que fija el techo del fondo."
        >
          {(props) => (
            <input
              {...props}
              type="number"
              min={1}
              step="0.01"
              value={monto}
              onChange={(e) => setMonto(e.target.value)}
            />
          )}
        </Campo>

        <Campo
          etiqueta="Justificación operativa"
          obligatorio
          ayuda="Cuántas misiones cubre y por qué ese monto. Un monto sin sustento es lo que después no se puede defender ante el Tribunal."
        >
          {(props) => (
            <textarea
              {...props}
              rows={3}
              value={justificacion}
              onChange={(e) => setJustificacion(e.target.value)}
            />
          )}
        </Campo>
      </div>
    </Modal>
  );
}

function DialogoDeAccion({
  fondo,
  tipo,
  onCerrar,
}: {
  fondo: Fondo;
  tipo: Accion;
  onCerrar(): void;
}): ReactElement {
  const invalidar = usarInvalidacion();
  const [ejecuta, setEjecuta] = useState('');
  const [monto, setMonto] = useState('');
  const [partida, setPartida] = useState(fondo.partida ?? '');
  const [motivo, setMotivo] = useState('');

  const operacion = useMutation({
    mutationFn: () => {
      const momento = ahora();

      if (tipo === 'aprobar')
        return aprobarFondo(fondo.id, {
          ejecuta: ejecuta.trim(),
          monto: Number(monto),
          // Vacía se manda como nula, no como cadena en blanco: `RN-26` distingue «pendiente»
          // de «vacío», y es la nula la que bloquea el cierre.
          partida: partida.trim() === '' ? null : partida.trim(),
          momento,
        });

      if (tipo === 'ampliar')
        return ampliarFondo(fondo.id, {
          ejecuta: ejecuta.trim(),
          monto: Number(monto),
          motivo: motivo.trim(),
          momento,
        });

      return cerrarFondo(fondo.id, {
        ejecuta: ejecuta.trim(),
        partida: partida.trim() === '' ? null : partida.trim(),
        momento,
      });
    },
    onSuccess: async () => {
      avisar.exito(TEXTO_DE_EXITO[tipo]);
      await invalidar();
      onCerrar();
    },
    onError: (e) => reportar(e, 'No se pudo aplicar el movimiento.'),
  });

  const completo =
    ejecuta.trim() !== '' &&
    (tipo === 'cerrar'
      ? true
      : tipo === 'aprobar'
        ? Number(monto) > 0
        : Number(monto) > 0 && motivo.trim().length >= 10);

  return (
    <Modal
      abierto
      titulo={TITULO[tipo](fondo)}
      descripcion={DESCRIPCION[tipo]}
      destructivo={tipo === 'cerrar'}
      onCerrar={onCerrar}
      acciones={
        <Boton
          variante={tipo === 'cerrar' ? 'peligro' : 'primario'}
          disabled={!completo || operacion.isPending}
          cargando={operacion.isPending}
          onClick={() => operacion.mutate()}
        >
          {BOTON[tipo]}
        </Boton>
      }
    >
      <div className="tw:flex tw:flex-col tw:gap-4">
        <CampoDeActor
          valor={ejecuta}
          onCambiar={setEjecuta}
          etiqueta={ACTOR[tipo].etiqueta}
          ayuda={ACTOR[tipo].ayuda}
        />

        {tipo !== 'cerrar' && (
          <Campo
            etiqueta={tipo === 'aprobar' ? 'Monto aprobado' : 'Monto adicional'}
            obligatorio
            ayuda={
              tipo === 'aprobar'
                ? 'Puede diferir de lo solicitado. Ésta es la que fija el techo del fondo.'
                : `Se suma al techo actual de ${lempiras(fondo.aprobado)}.`
            }
          >
            {(props) => (
              <input
                {...props}
                type="number"
                min={0.01}
                step="0.01"
                value={monto}
                onChange={(e) => setMonto(e.target.value)}
              />
            )}
          </Campo>
        )}

        {tipo === 'ampliar' && (
          <Campo
            etiqueta="Motivo de la ampliación"
            obligatorio
            ayuda="Qué ocurrió que no estaba previsto. La ampliación sigue el mismo circuito que el fondo original: no hay vía corta."
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
        )}

        {tipo !== 'ampliar' && (
          <Campo
            etiqueta="Partida presupuestaria"
            ayuda="La define ARGOS. Si todavía no la tiene, déjela vacía: el fondo se registra igual y lo que queda bloqueado es su cierre."
          >
            {(props) => (
              <input {...props} value={partida} onChange={(e) => setPartida(e.target.value)} />
            )}
          </Campo>
        )}

        {tipo === 'aprobar' && (
          <Nota tono="aviso">
            La <b>cuota trimestral de compromiso</b> no se verifica: necesita el espejo
            presupuestario de ARGOS, que todavía no existe. El asiento de aprobación va a
            dejarlo dicho, para que no se confunda con una verificación que sí ocurrió.
          </Nota>
        )}

        {tipo === 'cerrar' && (
          <Nota tono="aviso">
            El cierre exige que <b>todas</b> las asignaciones del fondo estén liquidadas o
            anuladas, y que la partida esté completa. Y no lo puede cerrar quien lo solicitó ni
            quien lo aprobó: quien autoriza el gasto no es quien declara que el gasto cuadró.
          </Nota>
        )}
      </div>
    </Modal>
  );
}

const ACTOR: Record<Accion, { etiqueta: string; ayuda: string }> = {
  aprobar: {
    etiqueta: 'Quién aprueba',
    ayuda: 'Gerencia Administrativa, y no la persona que solicitó el fondo. Se verifica por identidad de persona, no por rol: un mismo servidor con dos cuentas sigue siendo la misma persona.',
  },
  ampliar: {
    etiqueta: 'Quién aprueba la ampliación',
    ayuda: 'Tampoco puede ser quien solicitó el fondo: si no, bastaría aprobar un lempira y ampliarlo a cuarenta mil.',
  },
  cerrar: {
    etiqueta: 'Quién liquida el período',
    ayuda: 'Ni quien solicitó ni quien aprobó. Separar pedir de autorizar no sirve de nada si al final el mismo que autorizó declara que todo cuadró.',
  },
};

const TITULO: Record<Accion, (f: Fondo) => string> = {
  aprobar: (f) => `Aprobar el fondo de ${f.ambitoDeclarado}`,
  ampliar: (f) => `Ampliar el fondo de ${f.ambitoDeclarado}`,
  cerrar: (f) => `Cerrar el período de ${f.ambitoDeclarado}`,
};

const DESCRIPCION: Record<Accion, string> = {
  aprobar:
    'Lo aprueba Gerencia Administrativa, y no puede ser la misma persona que lo solicitó.',
  ampliar: 'La ampliación devuelve el fondo a aprobado y suma al techo. Queda en el diario.',
  cerrar: 'De cerrado no se sale. El período queda descargado con su partida.',
};

const BOTON: Record<Accion, string> = {
  aprobar: 'Aprobar',
  ampliar: 'Ampliar',
  cerrar: 'Cerrar el período',
};

const TEXTO_DE_EXITO: Record<Accion, string> = {
  aprobar: 'Fondo aprobado. Ya se pueden emitir vales contra él.',
  ampliar: 'Ampliación registrada.',
  cerrar: 'Período cerrado.',
};

/**
 * Un ULID generado en el cliente — `ADR-005`.
 *
 * Nace acá y no en el servidor para que el reintento de una petición que no se supo si
 * llegó no cree dos fondos. Es el mismo principio del identificador de captura del
 * dispositivo de campo, aplicado a la oficina.
 */
function ulid(): string {
  const ALFABETO = '0123456789ABCDEFGHJKMNPQRSTVWXYZ';
  let salida = '';
  for (let i = 0; i < 26; i++) {
    salida += ALFABETO[Math.floor(Math.random() * ALFABETO.length)];
  }
  return salida;
}
