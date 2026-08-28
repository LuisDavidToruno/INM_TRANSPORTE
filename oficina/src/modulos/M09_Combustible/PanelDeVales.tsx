import type { ReactElement } from 'react';
import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { CircleAlert } from 'lucide-react';

import { Boton, Campo, Modal, Nota, Panel, Pastilla, avisar } from '../../ui';
import type { Tono } from '../../ui';
import { BloqueoDuro } from '../../api/misiones';
import { conductores } from '../../api/flota';
import {
  TEXTO_DE_VALE,
  conciliarVale,
  emitirVale,
  entregarVale,
  fondos as pedirFondos,
  galones as enGalones,
  lempiras,
  liquidarVale,
  moverVale,
  registrarConsumo,
  valesDeLaMision,
} from '../../api/combustible';
import type { Vale } from '../../api/combustible';
import { momentoCompleto } from '../M06_Autorizacion/formato';
import CampoDeActor from './CampoDeActor';

/**
 * Los vales de una misión — máquina §10.1.
 *
 * ── Por qué es un panel y no una pantalla ───────────────────────────────────
 * Porque el vale <b>no se mira solo</b>. Se emite al programar, se entrega al despachar, se
 * consume en ruta y se liquida al cerrar: cada acto ocurre dentro de otra pantalla, con la
 * misión delante. Una pantalla de vales obligaría a salir del expediente justo en el momento
 * en que se decide sobre él.
 *
 * ── Lo que este panel evita ─────────────────────────────────────────────────
 * Que el cierre diga <i>«no se puede cerrar»</i> sin decir <b>cuál</b> vale lo impide.
 * `T-19` exige todas las asignaciones liquidadas y `T-21` todas conciliadas; sin esta lista,
 * quien cierra recibe un bloqueo sin objeto al que ir.
 */
/**
 * @param dependencia La de la misión. Su presencia significa <b>«acá se emite»</b>; ausente,
 * el panel sólo opera lo que ya existe — en el cierre la misión ya volvió y un vale nuevo no
 * tendría a qué viaje servir. No es una restricción de permisos: es que el acto no cabe ahí.
 *
 * @param motoristaDeLaOrden Quién quedó reservado. Se usa para <b>precargar</b> al receptor,
 * que es lo que `RN-32` manda —<i>«el sistema precarga vehículo y motorista, no los captura
 * libremente»</i>— sin cerrarle la puerta al caso que la regla existe para atrapar: si el que
 * llega es otro, se cambia y el bloqueo dispara.
 */
export default function PanelDeVales({
  misionId,
  estadoDeLaMision,
  dependencia,
  motoristaDeLaOrden,
}: {
  misionId: string;
  /**
   * En qué estado está la misión. <b>Decide qué actos caben</b>: `V-02` ocurre dentro del
   * despacho y `V-04` sólo en ruta, y ofrecerlos antes es ofrecer lo que el servidor va a
   * rechazar — que es como se enseña a ignorar los errores.
   */
  estadoDeLaMision: string;
  dependencia?: string;
  motoristaDeLaOrden?: string;
}): ReactElement {
  const [accion, setAccion] = useState<{ vale: Vale; tipo: Accion } | null>(null);
  const [emitiendo, setEmitiendo] = useState(false);

  const { data, isPending, isError } = useQuery({
    queryKey: ['vales', misionId],
    queryFn: () => valesDeLaMision(misionId),
  });

  if (isError) {
    return (
      <Panel titulo="Combustible">
        <Nota tono="riesgo" icono={<CircleAlert />}>
          No se pudieron cargar los vales de esta misión. Hasta saberlo, no se puede afirmar que
          el expediente esté listo para liquidar.
        </Nota>
      </Panel>
    );
  }

  const vales = data ?? [];
  const sinLiquidar = vales.filter((v) => !v.resuelta);
  const sinConciliar = vales.filter(
    (v) => !['Conciliada', 'ConciliadaConDesviacion', 'Anulada', 'Devuelta'].includes(v.estado),
  );

  return (
    <Panel
      titulo="Combustible"
      acciones={
        dependencia !== undefined ? (
          <Boton variante="secundario" tamano="sm" onClick={() => setEmitiendo(true)}>
            Emitir vale
          </Boton>
        ) : undefined
      }
    >
      <div className="tw:flex tw:flex-col tw:gap-4">
        {isPending ? (
          <p className="tw:text-sm tw:text-tinta-mid">Cargando los vales…</p>
        ) : vales.length === 0 ? (
          // Cero es un dato, no un vacío: el vehículo salió con el tanque lleno, y esa misión
          // liquida sin nada que cuadrar. Decirlo evita leer el renglón en blanco como «falta».
          <p className="tw:text-sm tw:text-tinta-mid">
            Esta misión no tiene combustible asignado. Sus precondiciones de liquidación y
            cierre se cumplen sin nada que revisar.
          </p>
        ) : (
          <>
            {sinLiquidar.length > 0 && (
              <Nota tono="aviso">
                {sinLiquidar.length === 1 ? '1 vale sigue' : `${sinLiquidar.length} vales siguen`}{' '}
                sin liquidar, y <b>la misión no puede liquidarse hasta que se resuelvan</b>{' '}
                (<code className="tw:font-mono tw:text-xs">INV-34</code>). Declararla cuadrada
                ahora sería cerrar el resultado económico de un viaje cuyo dinero nadie cuadró.
              </Nota>
            )}

            {sinLiquidar.length === 0 && sinConciliar.length > 0 && (
              <Nota tono="info">
                {sinConciliar.length === 1 ? 'Queda 1 vale' : `Quedan ${sinConciliar.length} vales`}{' '}
                por conciliar contra el kilometraje. Una desviación <i>explicada</i> no impide
                cerrar; un vale que nadie contrastó, sí.
              </Nota>
            )}

            <ul className="tw:flex tw:flex-col tw:gap-3">
              {vales.map((v) => (
                <ValeVista
                  key={v.id}
                  vale={v}
                  estadoDeLaMision={estadoDeLaMision}
                  onAccion={(tipo) => setAccion({ vale: v, tipo })}
                />
              ))}
            </ul>
          </>
        )}
      </div>

      {emitiendo && dependencia !== undefined && (
        <DialogoDeEmision
          misionId={misionId}
          dependencia={dependencia}
          motoristaDeLaOrden={motoristaDeLaOrden}
          onCerrar={() => setEmitiendo(false)}
        />
      )}

      {accion && (
        <DialogoDeVale
          vale={accion.vale}
          tipo={accion.tipo}
          misionId={misionId}
          onCerrar={() => setAccion(null)}
        />
      )}
    </Panel>
  );
}

type Accion = 'entregar' | 'anular' | 'consumo' | 'devolver' | 'extravio' | 'liquidar' | 'conciliar';

const TONO_DE_VALE: Record<string, Tono> = {
  Emitida: 'info',
  Entregada: 'aviso',
  Consumida: 'aviso',
  Devuelta: 'ok',
  Extraviada: 'riesgo',
  Liquidada: 'ok',
  Conciliada: 'ok',
  ConciliadaConDesviacion: 'aviso',
  Anulada: 'neutro',
};

/**
 * Qué se puede hacer, por estado. <b>Sale de la máquina §10.1 y no de un permiso</b>: ofrecer
 * una acción que el servidor va a rechazar enseña a ignorar los errores.
 */
const ACCIONES: Record<string, Accion[]> = {
  Emitida: ['entregar', 'anular'],
  Entregada: ['consumo', 'devolver', 'extravio'],
  Consumida: ['consumo', 'liquidar'],
  Extraviada: ['liquidar'],
  Liquidada: ['conciliar'],
};

const ROTULO: Record<Accion, string> = {
  entregar: 'Entregar contra firma',
  anular: 'Anular el vale',
  consumo: 'Registrar consumo',
  devolver: 'Devolver íntegro',
  extravio: 'Declarar extravío',
  liquidar: 'Liquidar',
  conciliar: 'Conciliar',
};

/**
 * Qué actos permite el estado de la MISIÓN — las reglas de acoplamiento de §10.1.
 *
 * `V-02` entregar ocurre <b>dentro de</b> `T-12` despachar, y `V-04` consumir sólo mientras
 * la misión está en ruta. No son restricciones de esta pantalla: son de la máquina, y acá
 * sólo se dejan de ofrecer.
 */
function cabeEnLaMision(accion: Accion, estadoDeLaMision: string): boolean {
  if (accion === 'entregar')
    return estadoDeLaMision === 'Despachada' || estadoDeLaMision === 'EnRuta';

  // `RETORNADA` se admite porque el consumo se captura sin conectividad y llega días
  // después, con el vehículo ya en el predio. Rechazarlo por llegar tarde perdería el hecho.
  if (accion === 'consumo')
    return estadoDeLaMision === 'EnRuta' || estadoDeLaMision === 'Retornada';

  return true;
}

function ValeVista({
  vale,
  estadoDeLaMision,
  onAccion,
}: {
  vale: Vale;
  estadoDeLaMision: string;
  onAccion(tipo: Accion): void;
}): ReactElement {
  const [verDiario, setVerDiario] = useState(false);
  const posibles = ACCIONES[vale.estado] ?? [];
  const acciones = posibles.filter((a) => cabeEnLaMision(a, estadoDeLaMision));

  // Lo que el estado del vale permitiría y la misión todavía no. Se DICE, en vez de dejar
  // un vale sin ningún botón y que quien lo mira crea que está atascado.
  const esperando = posibles.filter((a) => !cabeEnLaMision(a, estadoDeLaMision));

  return (
    <li className="tw:flex tw:flex-col tw:gap-3 tw:rounded-control tw:border tw:border-linea-suave tw:p-3">
      <div className="tw:flex tw:flex-wrap tw:items-center tw:justify-between tw:gap-2">
        <div className="tw:flex tw:items-center tw:gap-2">
          <code className="tw:font-mono tw:text-[13px]">{vale.folio}</code>
          <Pastilla tono={TONO_DE_VALE[vale.estado] ?? 'neutro'}>
            {TEXTO_DE_VALE[vale.estado] ?? vale.estado}
          </Pastilla>
        </div>

        <span className="tw:text-xs tw:text-tinta-mid">
          {vale.instrumento} de {vale.tipoDeCombustible}
        </span>
      </div>

      {/* Las tres cifras del cuadre, juntas y en la misma escala. Separarlas obligaría a
          buscar la que falta para poder restar. */}
      <div className="tw:flex tw:flex-wrap tw:gap-x-6 tw:gap-y-1 tw:text-sm">
        <Cifra rotulo="Asignado" valor={lempiras(vale.monto)} />
        <Cifra
          rotulo="Consumido"
          valor={
            vale.tuvoConsumo
              ? `${lempiras(vale.consumido)} · ${enGalones(vale.galonesConsumidos)}`
              : '—'
          }
        />
        <Cifra rotulo="Devuelto" valor={vale.devuelto > 0 ? lempiras(vale.devuelto) : '—'} />
      </div>

      {/* Lo que decide entre `T-15` y `T-16` en la misión. Es la consecuencia menos evidente
          del consumo, y la que más cara sale descubrir tarde. */}
      {vale.tuvoConsumo && (
        <p className="tw:text-xs tw:text-aviso-fg">
          Este vale ya tuvo consumo: la misión <b>ya no se puede anular</b>. Si no se ejecuta,
          el camino es retornarla sin ejecutar y liquidarla igual — anular sería borrar un hecho
          económico.
        </p>
      )}

      <div className="tw:flex tw:flex-wrap tw:items-center tw:gap-2">
        {acciones.map((a) => (
          <Boton
            key={a}
            variante={a === 'anular' || a === 'extravio' ? 'peligro' : 'secundario'}
            tamano="sm"
            onClick={() => onAccion(a)}
          >
            {ROTULO[a]}
          </Boton>
        ))}

        <Boton variante="fantasma" tamano="sm" onClick={() => setVerDiario((v) => !v)}>
          {verDiario ? 'Ocultar el diario' : `Diario (${vale.diario.length})`}
        </Boton>
      </div>

      {esperando.length > 0 && (
        <p className="tw:text-xs tw:text-tinta-mid">
          {esperando.includes('entregar')
            ? 'Se entrega dentro del despacho: mientras la misión no se despacha, el vale existe emitido y no sale de la custodia de quien lo guarda.'
            : 'El consumo se registra en ruta. La misión todavía no ha salido.'}
        </p>
      )}

      {verDiario && (
        <ol className="tw:flex tw:flex-col tw:gap-2 tw:border-t tw:border-linea-suave tw:pt-2 tw:text-xs">
          {vale.diario.map((t, i) => (
            <li key={`${t.transicion}-${i}`} className="tw:flex tw:flex-col tw:gap-0.5">
              <div className="tw:flex tw:items-center tw:gap-2">
                <code className="tw:font-mono tw:text-[11px] tw:text-tinta-mid">
                  {t.transicion}
                </code>
                <span className="tw:font-medium">
                  {TEXTO_DE_VALE[t.destino] ?? t.destino}
                </span>
              </div>
              <span className="tw:text-tinta-mid">
                {t.ejecuta} · {momentoCompleto(t.momento)}
              </span>
              {t.consumo && (
                <span className="tw:text-tinta-mid">
                  {enGalones(t.consumo.galones)} por {lempiras(t.consumo.monto)} en{' '}
                  {t.consumo.estacion} · odómetro{' '}
                  {t.consumo.odometro.toLocaleString('es-HN')} km ·{' '}
                  {t.consumo.comprobante ?? (
                    <span className="tw:text-aviso-fg">sin comprobante</span>
                  )}
                </span>
              )}
              {t.motivo && <span className="tw:text-tinta-mid">{t.motivo}</span>}
            </li>
          ))}
        </ol>
      )}
    </li>
  );
}

function Cifra({ rotulo, valor }: { rotulo: string; valor: string }): ReactElement {
  return (
    <span className="tw:flex tw:items-baseline tw:gap-1.5">
      <span className="tw:text-xs tw:text-tinta-mid">{rotulo}</span>
      <span className="tw:font-mono tw:tabular-nums">{valor}</span>
    </span>
  );
}

// ── El diálogo ──────────────────────────────────────────────────────────────

/**
 * `V-01` — emitir el vale contra la misión.
 *
 ── El fondo se ELIGE, y sólo entre los que pueden cubrirla ─────────────────
 * `RN-26` exige fondo <b>aprobado, vigente a la fecha del hecho y del mismo ámbito</b>. La
 * lista se recorta a ésos: ofrecer un fondo de otra delegación o uno cerrado sería ofrecer
 * una acción que el servidor va a rechazar, y eso enseña a ignorar los errores.
 *
 * ⚠️ <b>El folio se teclea.</b> Debería salir del rango de la delegación (`RN-44`), que es lo
 * que permite emitirlo sin conectividad — pero ese rango vive en el cliente de campo y la
 * oficina todavía no lo consume. Mientras tanto se captura, que es exactamente lo que `RN-27`
 * prevé para la institución que usa folios preimpresos.
 */
function DialogoDeEmision({
  misionId,
  dependencia,
  motoristaDeLaOrden,
  onCerrar,
}: {
  misionId: string;
  dependencia: string;
  motoristaDeLaOrden?: string;
  onCerrar(): void;
}): ReactElement {
  const cliente = useQueryClient();

  const [ejecuta, setEjecuta] = useState('');
  // Precargado con el de la orden, que es el caso normal. Cambiarlo es declarar que quien
  // llegó es otro — y ahí `RN-32` bloquea, que es exactamente para lo que existe.
  const [receptor, setReceptor] = useState(motoristaDeLaOrden ?? '');
  const [folio, setFolio] = useState('');
  const [idFondo, setFondo] = useState('');
  const [monto, setMonto] = useState('');
  const [galones, setGalones] = useState('');
  const [instrumento, setInstrumento] = useState('vale');
  const [tipo, setTipo] = useState('Diesel');

  const listaDeFondos = useQuery({ queryKey: ['fondos'], queryFn: pedirFondos });

  // **Quién está en la ventanilla se ELIGE.** `RN-32` compara al receptor presente contra
  // el motorista de la orden; deducirlo de la orden dejaría la regla comparando algo
  // consigo mismo — que es exactamente el defecto que ya se corrigió en el servicio.
  const padron = useQuery({ queryKey: ['conductores'], queryFn: conductores });

  const hoy = new Date().toISOString().slice(0, 10);

  // Los tres filtros de `RN-26`, en el mismo orden en que el servidor los va a aplicar.
  const elegibles = (listaDeFondos.data ?? []).filter(
    (f) =>
      f.estado !== 'Solicitado' &&
      f.estado !== 'Cerrado' &&
      f.desde <= hoy &&
      f.hasta >= hoy &&
      f.ambitoDeclarado.toLowerCase() === dependencia.toLowerCase(),
  );

  const fondo = elegibles.find((f) => f.id === idFondo);

  const operacion = useMutation({
    mutationFn: () =>
      emitirVale({
        id: ulid(),
        folio: folio.trim(),
        idFondo,
        idMision: misionId,
        idMotoristaReceptor: receptor,
        ejecuta: ejecuta.trim(),
        monto: Number(monto),
        galones: galones.trim() === '' ? null : Number(galones),
        instrumento,
        tipoDeCombustible: tipo,
        momento: new Date().toISOString(),
      }),
    onSuccess: async () => {
      avisar.exito(`Vale ${folio.trim()} emitido. No sale de la custodia hasta el despacho.`);
      await cliente.invalidateQueries({ queryKey: ['vales', misionId] });
      await cliente.invalidateQueries({ queryKey: ['fondos'] });
      onCerrar();
    },
    onError: (e) => {
      if (e instanceof BloqueoDuro) {
        avisar.error(e.message);
        return;
      }
      avisar.error('No se pudo emitir el vale.');
    },
  });

  const excede = fondo !== undefined && Number(monto) > fondo.saldo;

  const completo =
    ejecuta.trim() !== '' &&
    receptor !== '' &&
    folio.trim() !== '' &&
    idFondo !== '' &&
    Number(monto) > 0;

  return (
    <Modal
      abierto
      titulo="Emitir vale de combustible"
      descripcion="El vale nace emitido y no sale de la custodia de quien lo guarda: la entrega ocurre dentro del despacho."
      onCerrar={onCerrar}
      acciones={
        <Boton
          disabled={!completo || operacion.isPending}
          cargando={operacion.isPending}
          onClick={() => operacion.mutate()}
        >
          Emitir
        </Boton>
      }
    >
      <div className="tw:flex tw:flex-col tw:gap-4">
        {/* **Cargando NO es «no hay».** Decir «no hay ningún fondo» antes de haber recibido
            la lista es un negativo definitivo afirmado sin saberlo, y manda a solicitar un
            fondo que ya existe. */}
        {listaDeFondos.isPending ? (
          <p className="tw:text-sm tw:text-tinta-mid">Buscando fondos vigentes…</p>
        ) : elegibles.length === 0 ? (
          <Nota tono="riesgo" icono={<CircleAlert />}>
            No hay ningún fondo aprobado y vigente para <b>{dependencia}</b> a la fecha
            de hoy. Sin fondo no se emite un solo vale: la salida es solicitarlo, no emitir
            contra nada.
          </Nota>
        ) : (
          <>
            <CampoDeActor
              valor={ejecuta}
              onCambiar={setEjecuta}
              etiqueta="Quién emite"
              ayuda="Jefatura de Transporte. No va a poder entregarlo ni liquidarlo: son eslabones distintos del mismo circuito."
            />

            <Campo
              etiqueta="Fondo"
              obligatorio
              ayuda="Sólo aparecen los aprobados, vigentes hoy y del mismo ámbito que la misión."
            >
              {(props) => (
                <select {...props} value={idFondo} onChange={(e) => setFondo(e.target.value)}>
                  <option value="">Elija el fondo…</option>
                  {elegibles.map((f) => (
                    <option key={f.id} value={f.id}>
                      {f.ambitoDeclarado} — quedan {lempiras(f.saldo)}
                    </option>
                  ))}
                </select>
              )}
            </Campo>

            <Campo
              etiqueta="Quién recibe"
              obligatorio
              ayuda="El motorista que está en la ventanilla. Si no es el asignado a la orden, el sistema lo bloquea: sacar el vale a nombre de una misión real y cargarlo en otro vehículo es el desvío más simple que existe."
            >
              {(props) => (
                <select
                  {...props}
                  value={receptor}
                  onChange={(e) => setReceptor(e.target.value)}
                >
                  <option value="">Elija a quien recibe…</option>
                  {(padron.data ?? []).map((c) => (
                    <option key={c.id} value={c.id}>
                      {c.nombre}
                    </option>
                  ))}
                </select>
              )}
            </Campo>

            {receptor !== '' &&
              motoristaDeLaOrden !== undefined &&
              receptor !== motoristaDeLaOrden && (
                <Nota tono="riesgo" icono={<CircleAlert />}>
                  Quien recibe <b>no es</b> el motorista asignado a esta orden. El sistema va
                  a rechazar la emisión: el único camino para cambiarlo es la sustitución de
                  motorista, que revalida licencia y habilitación.
                </Nota>
              )}

            <Campo
              etiqueta="Folio"
              obligatorio
              ayuda="Único en la institución y no reciclable. Es lo que contesta de qué fondo salió este galón, quién lo recibió y a qué misión sirvió."
            >
              {(props) => (
                <input {...props} value={folio} onChange={(e) => setFolio(e.target.value)} />
              )}
            </Campo>

            <div className="tw:grid tw:grid-cols-2 tw:gap-3">
              <Campo etiqueta="Monto" obligatorio>
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

              <Campo
                etiqueta="Galones"
                ayuda="Opcional: `RN-27` admite monto y/o galones."
              >
                {(props) => (
                  <input
                    {...props}
                    type="number"
                    min={0.01}
                    step="0.01"
                    value={galones}
                    onChange={(e) => setGalones(e.target.value)}
                  />
                )}
              </Campo>
            </div>

            <div className="tw:grid tw:grid-cols-2 tw:gap-3">
              <Campo etiqueta="Instrumento" obligatorio>
                {(props) => (
                  <select
                    {...props}
                    value={instrumento}
                    onChange={(e) => setInstrumento(e.target.value)}
                  >
                    <option value="vale">Vale</option>
                    <option value="cupón">Cupón</option>
                    <option value="efectivo">Efectivo</option>
                    <option value="orden de pago">Orden de pago</option>
                  </select>
                )}
              </Campo>

              <Campo etiqueta="Combustible" obligatorio>
                {(props) => (
                  <select {...props} value={tipo} onChange={(e) => setTipo(e.target.value)}>
                    <option value="Diesel">Diésel</option>
                    <option value="Gasolina">Gasolina</option>
                  </select>
                )}
              </Campo>
            </div>

            {/* El saldo, ANTES de intentarlo. `RN-26` manda mostrarlo antes de cada
                asignación, y descubrir que no alcanza después de teclear el folio es
                perder un folio que ya no se recicla. */}
            {fondo && (
              <p
                className={`tw:text-sm ${excede ? 'tw:text-riesgo-fg' : 'tw:text-tinta-mid'}`}
              >
                {excede ? (
                  <>
                    <b>El fondo no alcanza:</b> quedan {lempiras(fondo.saldo)} y se piden{' '}
                    {lempiras(Number(monto))}. La salida es la ampliación, no emitir igual.
                  </>
                ) : (
                  <>
                    Después de este vale quedarían{' '}
                    {lempiras(fondo.saldo - Number(monto || 0))}.
                  </>
                )}
              </p>
            )}

            <Nota tono="aviso">
              ⚠️ <b>La compatibilidad del combustible no se comprueba.</b> La ficha del
              vehículo todavía no declara qué usa, así que un vale de diésel para un vehículo
              de gasolina pasa. El sistema lo deja dicho en el diario en vez de suponer que
              coincide.
            </Nota>
          </>
        )}
      </div>
    </Modal>
  );
}

/**
 * Un ULID generado en el cliente — `ADR-005`. Nace acá para que el reintento de una
 * petición que no se supo si llegó no emita dos vales.
 */
function ulid(): string {
  const ALFABETO = '0123456789ABCDEFGHJKMNPQRSTVWXYZ';
  let salida = '';
  for (let i = 0; i < 26; i++) {
    salida += ALFABETO[Math.floor(Math.random() * ALFABETO.length)];
  }
  return salida;
}

function DialogoDeVale({
  vale,
  tipo,
  misionId,
  onCerrar,
}: {
  vale: Vale;
  tipo: Accion;
  misionId: string;
  onCerrar(): void;
}): ReactElement {
  const cliente = useQueryClient();

  const [ejecuta, setEjecuta] = useState('');
  const [texto, setTexto] = useState('');
  const [galones, setGalones] = useState('');
  const [monto, setMonto] = useState('');
  const [estacion, setEstacion] = useState('');
  const [odometro, setOdometro] = useState('');
  const [comprobante, setComprobante] = useState('');
  const [devuelto, setDevuelto] = useState('');
  const [dentroDeUmbral, setDentroDeUmbral] = useState(true);

  const operacion = useMutation({
    mutationFn: () => {
      const momento = new Date().toISOString();

      switch (tipo) {
        case 'entregar':
          return entregarVale(vale.id, {
            ejecuta: ejecuta.trim(),
            constancia: texto.trim(),
            momento,
          });

        case 'anular':
        case 'devolver':
        case 'extravio':
          return moverVale(vale.id, tipo, {
            ejecuta: ejecuta.trim(),
            motivo: texto.trim(),
            momento,
          });

        case 'consumo':
          return registrarConsumo(vale.id, {
            ejecuta: ejecuta.trim(),
            galones: Number(galones),
            monto: Number(monto),
            estacion: estacion.trim(),
            odometro: Number(odometro),
            // Vacío se manda como nulo, no como cadena en blanco: `RN-85` distingue «sin
            // comprobante, con causa» de un campo que nadie llenó.
            comprobante: comprobante.trim() === '' ? null : comprobante.trim(),
            momento,
          });

        case 'liquidar':
          return liquidarVale(vale.id, {
            ejecuta: ejecuta.trim(),
            saldoDevuelto: Number(devuelto || 0),
            observacion: texto.trim() === '' ? null : texto.trim(),
            momento,
          });

        case 'conciliar':
          return conciliarVale(vale.id, {
            ejecuta: ejecuta.trim(),
            dentroDeUmbral,
            dictamen: texto.trim(),
            momento,
          });
      }
    },
    onSuccess: async () => {
      avisar.exito(`${vale.folio}: ${ROTULO[tipo].toLowerCase()} registrado.`);
      await cliente.invalidateQueries({ queryKey: ['vales', misionId] });
      // El saldo del fondo cambia con casi todo esto, y la misión puede haber quedado
      // liquidable. Refrescar sólo los vales dejaría las otras dos pantallas mintiendo.
      await cliente.invalidateQueries({ queryKey: ['fondos'] });
      await cliente.invalidateQueries({ queryKey: ['expediente', misionId] });
      onCerrar();
    },
    onError: (e) => {
      // El servidor rechaza por `BD-06`, por estado o por acta faltante, y el mensaje es lo
      // único que dice cuál de los tres.
      if (e instanceof BloqueoDuro) {
        avisar.error(e.message);
        return;
      }
      avisar.error('No se pudo aplicar el movimiento. El vale quedó como estaba.');
    },
  });

  const completo = (() => {
    if (ejecuta.trim() === '') return false;

    switch (tipo) {
      case 'entregar':
        return texto.trim().length >= 5;
      case 'anular':
      case 'devolver':
      case 'extravio':
        return texto.trim().length >= 10;
      case 'consumo':
        return (
          Number(galones) > 0 &&
          Number(monto) > 0 &&
          estacion.trim() !== '' &&
          Number(odometro) > 0
        );
      case 'liquidar':
        return Number(devuelto || 0) >= 0;
      case 'conciliar':
        // Fuera de umbral exige causa tipificada — `INV-35`. Dentro de umbral no la exige,
        // pero el dictamen es lo que hace auditable la conciliación, así que se pide igual.
        return texto.trim().length >= 10;
    }
  })();

  const destructivo = tipo === 'anular' || tipo === 'extravio';

  return (
    <Modal
      abierto
      titulo={`${ROTULO[tipo]} — ${vale.folio}`}
      descripcion={DESCRIPCION[tipo]}
      destructivo={destructivo}
      onCerrar={onCerrar}
      acciones={
        <Boton
          variante={destructivo ? 'peligro' : 'primario'}
          disabled={!completo || operacion.isPending}
          cargando={operacion.isPending}
          onClick={() => operacion.mutate()}
        >
          {ROTULO[tipo]}
        </Boton>
      }
    >
      <div className="tw:flex tw:flex-col tw:gap-4">
        <CampoDeActor
          valor={ejecuta}
          onCambiar={setEjecuta}
          etiqueta={ACTOR_DEL_VALE[tipo]}
          ayuda="Cada eslabón del vale exige una persona distinta: emite, entrega, consume, liquida y concilia. Es el par que habilita el fraude de combustible más simple."
        />

        {tipo === 'consumo' && (
          <>
            <div className="tw:grid tw:grid-cols-2 tw:gap-3">
              <Campo etiqueta="Galones" obligatorio>
                {(props) => (
                  <input
                    {...props}
                    type="number"
                    min={0.01}
                    step="0.01"
                    value={galones}
                    onChange={(e) => setGalones(e.target.value)}
                  />
                )}
              </Campo>

              <Campo etiqueta="Monto" obligatorio>
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
            </div>

            <Campo etiqueta="Estación" obligatorio>
              {(props) => (
                <input {...props} value={estacion} onChange={(e) => setEstacion(e.target.value)} />
              )}
            </Campo>

            <Campo
              etiqueta="Odómetro del momento"
              obligatorio
              ayuda="Es lo que ancla el galón a un tramo recorrido. Sin él la conciliación sólo puede comparar un total contra otro total, y no puede decir dónde se fue la diferencia."
            >
              {(props) => (
                <input
                  {...props}
                  type="number"
                  min={1}
                  value={odometro}
                  onChange={(e) => setOdometro(e.target.value)}
                />
              )}
            </Campo>

            <Campo
              etiqueta="Comprobante"
              ayuda="Si no hay factura, déjelo vacío: el abastecimiento se registra igual y queda marcado. El registro no se omite nunca por falta de papel."
            >
              {(props) => (
                <input
                  {...props}
                  value={comprobante}
                  onChange={(e) => setComprobante(e.target.value)}
                />
              )}
            </Campo>
          </>
        )}

        {tipo === 'liquidar' && (
          <>
            <Campo
              etiqueta="Saldo devuelto"
              obligatorio
              ayuda="Lo que volvió a la caja, constatado. Una devolución declarada y no constatada no libera saldo del fondo."
            >
              {(props) => (
                <input
                  {...props}
                  type="number"
                  min={0}
                  step="0.01"
                  value={devuelto}
                  onChange={(e) => setDevuelto(e.target.value)}
                />
              )}
            </Campo>

            {/* El cuadre, dicho antes de aplicarlo. Descubrir la diferencia después de
                liquidar obliga a explicarla en un asiento que ya no se puede quitar. */}
            <Cuadre vale={vale} devuelto={Number(devuelto || 0)} />
          </>
        )}

        {tipo === 'conciliar' && (
          <fieldset className="tw:flex tw:flex-col tw:gap-2">
            <legend className="tw:mb-1 tw:text-sm tw:font-medium">Resultado</legend>

            <Opcion
              elegido={dentroDeUmbral}
              valor
              onElegir={setDentroDeUmbral}
              texto="Dentro de umbral"
            />
            <Opcion
              elegido={dentroDeUmbral}
              valor={false}
              onElegir={setDentroDeUmbral}
              texto="Fuera de umbral — dispara hallazgo H-01 en la misión"
            />

            <Nota tono="aviso">
              <b>El umbral lo decide usted, no el sistema.</b> Los umbrales de desviación por
              tipo de vehículo son parámetro pendiente de la institución y el rendimiento
              esperado no está cargado. Calcularlo contra un umbral inexistente daría siempre
              «conforme», y una conciliación que siempre concilia es peor que ninguna.
            </Nota>
          </fieldset>
        )}

        <Campo
          etiqueta={ETIQUETA_DE_TEXTO[tipo]}
          obligatorio={tipo !== 'liquidar'}
          ayuda={AYUDA_DE_TEXTO[tipo]}
        >
          {(props) =>
            tipo === 'entregar' ? (
              <input {...props} value={texto} onChange={(e) => setTexto(e.target.value)} />
            ) : (
              <textarea
                {...props}
                rows={3}
                value={texto}
                onChange={(e) => setTexto(e.target.value)}
              />
            )
          }
        </Campo>

        {tipo === 'devolver' && vale.tuvoConsumo && (
          <Nota tono="riesgo" icono={<CircleAlert />}>
            Este vale ya tuvo consumo por {lempiras(vale.consumido)}. La devolución íntegra no
            aplica: devolver «íntegro» algo ya tocado es declarar que volvió un dinero que no
            volvió. El camino es liquidarlo por lo consumido.
          </Nota>
        )}
      </div>
    </Modal>
  );
}

/**
 * Lo que va a quedar escrito en el asiento, antes de escribirlo.
 *
 * La diferencia se nombra <b>aunque sea cero</b>: callarla cuando cuadra y decirla cuando no,
 * entrena a leer su ausencia como «no se calculó».
 */
function Cuadre({ vale, devuelto }: { vale: Vale; devuelto: number }): ReactElement {
  const diferencia = vale.monto - vale.consumido - devuelto;

  return (
    <div className="tw:flex tw:flex-col tw:gap-1 tw:rounded-control tw:bg-inset tw:p-3 tw:text-sm">
      <Linea rotulo="Asignado" valor={lempiras(vale.monto)} />
      <Linea rotulo="Consumido" valor={`− ${lempiras(vale.consumido)}`} />
      <Linea rotulo="Devuelto" valor={`− ${lempiras(devuelto)}`} />

      <div className="tw:mt-1 tw:border-t tw:border-linea tw:pt-1">
        <Linea
          rotulo={diferencia === 0 ? 'Cuadra exacto' : 'Diferencia sin explicar'}
          valor={lempiras(diferencia)}
          tono={diferencia === 0 ? 'ok' : 'riesgo'}
        />
      </div>

      {diferencia !== 0 && (
        <p className="tw:mt-1 tw:text-xs tw:text-riesgo-fg">
          Una diferencia sin explicar dispara <code className="tw:font-mono">H-11</code>, y la
          misión cierra con hallazgo.
        </p>
      )}
    </div>
  );
}

function Linea({
  rotulo,
  valor,
  tono,
}: {
  rotulo: string;
  valor: string;
  tono?: 'ok' | 'riesgo';
}): ReactElement {
  const color = tono === 'ok' ? 'tw:text-ok-fg' : tono === 'riesgo' ? 'tw:text-riesgo-fg' : '';

  return (
    <div className={`tw:flex tw:items-baseline tw:justify-between tw:gap-4 ${color}`}>
      <span className={tono ? 'tw:font-medium' : 'tw:text-tinta-mid'}>{rotulo}</span>
      <span className="tw:font-mono tw:tabular-nums">{valor}</span>
    </div>
  );
}

function Opcion({
  elegido,
  valor,
  onElegir,
  texto,
}: {
  elegido: boolean;
  valor: boolean;
  onElegir(v: boolean): void;
  texto: string;
}): ReactElement {
  return (
    <label className="tw:flex tw:cursor-pointer tw:items-start tw:gap-2 tw:text-sm">
      <input
        type="radio"
        name="umbral"
        checked={elegido === valor}
        onChange={() => onElegir(valor)}
        className="tw:mt-0.5 tw:size-4 tw:shrink-0 tw:accent-acento"
      />
      <span>{texto}</span>
    </label>
  );
}

const ACTOR_DEL_VALE: Record<Accion, string> = {
  entregar: 'Quién entrega',
  anular: 'Quién anula',
  consumo: 'Quién consumió',
  devolver: 'Quién recibe la devolución',
  extravio: 'Quién declara el extravío',
  liquidar: 'Quién liquida',
  conciliar: 'Quién concilia',
};

const DESCRIPCION: Record<Accion, string> = {
  entregar:
    'La entrega ocurre dentro del despacho. Desde acá el dinero está fuera de la caja.',
  anular: 'Sólo antes de entregar. El folio queda anulado y su valor vuelve al fondo.',
  consumo: 'Se pueden registrar varias cargas: el motorista carga a la ida y a la vuelta.',
  devolver: 'Volvió íntegro y sin consumo, con acta firmada por quien entregó y quien devuelve.',
  extravio: 'El instrumento se pierde; el descargo no. Se liquida después con el acta.',
  liquidar: 'Cuadra asignado, consumido y devuelto. Quien liquida no puede ser quien consumió.',
  conciliar: 'Galones contra kilómetros. Quien concilia no puede ser ninguno de los anteriores.',
};

const ETIQUETA_DE_TEXTO: Record<Accion, string> = {
  entregar: 'Constancia de recepción',
  anular: 'Motivo y acta',
  consumo: 'Observación',
  devolver: 'Acta de devolución',
  extravio: 'Acta de extravío',
  liquidar: 'Observación',
  conciliar: 'Dictamen',
};

const AYUDA_DE_TEXTO: Record<Accion, string> = {
  entregar:
    'Sin constancia el vale queda «emitido no entregado», y en ese estado no es consumible ni liquidable.',
  anular: 'El folio se anula y no se recicla. La razón es lo que sostiene el descargo.',
  consumo: 'Opcional.',
  devolver: 'Firmada por quien entregó y por quien devuelve. Sin ella no se libera saldo.',
  extravio:
    'Con motivo y responsable. Un extravío no declarado deja un vale que sigue figurando entregado y que puede aparecer canjeado en la factura del proveedor.',
  liquidar: 'Opcional. Si hay diferencia, acá se explica.',
  conciliar:
    'Rendimiento real contra esperado, y la causa si se sale del umbral. Sin causa tipificada la misión no se puede cerrar.',
};
