import type { ReactElement } from 'react';
import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { CircleAlert } from 'lucide-react';

import { Boton, Campo, Modal, Nota, Panel, Pastilla, avisar } from '../../ui';
import type { Tono } from '../../ui';
import { BloqueoDuro } from '../../api/misiones';
import {
  FUENTES_REGISTRABLES,
  TEXTO_DE_FUENTE,
  abastecimientosDeLaMision,
  galones as enGalones,
  lempiras,
  registrarAbastecimiento,
} from '../../api/combustible';
import type { Abastecimiento } from '../../api/combustible';
import { momentoCompleto } from '../M06_Autorizacion/formato';
import CampoDeActor from './CampoDeActor';

/**
 * Todo lo que entró al tanque — `RN-83`.
 *
 * ── Por qué esto va al lado de los vales y no dentro ────────────────────────
 * Porque son <b>dos preguntas distintas</b>. El vale contesta <i>«qué se hizo con el dinero
 * del fondo»</i>; esto contesta <i>«cuántos galones entraron a este tanque»</i>, que es el
 * numerador de la conciliación. Mezclarlos haría que el despacho del tanque de la sede
 * pareciera un movimiento de caja, y no lo es: no salió de ningún folio.
 *
 * ── Lo que esta lista evita ─────────────────────────────────────────────────
 * Que `RN-30` señale un rendimiento imposible y nadie pueda ver por qué. Con la composición
 * delante, <i>«900 km con 20 galones»</i> deja de ser una acusación y pasa a ser una suma
 * incompleta que alguien puede completar.
 */
export default function PanelDeAbastecimientos({
  misionId,
  vehiculoId,
  puedeRegistrar = true,
}: {
  misionId: string;
  /** Contra qué tanque. Sin él no se puede registrar: el galón no sabría a qué vehículo entró. */
  vehiculoId?: string;
  puedeRegistrar?: boolean;
}): ReactElement {
  const [registrando, setRegistrando] = useState(false);

  const { data, isPending, isError } = useQuery({
    queryKey: ['abastecimientos', misionId],
    queryFn: () => abastecimientosDeLaMision(misionId),
  });

  if (isError) {
    return (
      <Panel titulo="Combustible que entró al tanque">
        <Nota tono="riesgo" icono={<CircleAlert />}>
          No se pudieron cargar los abastecimientos. Sin ellos no se puede afirmar cuántos
          galones entraron a este tanque.
        </Nota>
      </Panel>
    );
  }

  const lista = data ?? [];
  const total = lista.reduce((suma, a) => suma + a.galones, 0);

  // La composición por fuente. Es lo que `RN-30` manda exponer: sin ella, cuarenta galones del
  // tanque de la sede y cuarenta comprados con el vale se leen igual.
  const porFuente = lista.reduce<Record<string, number>>((mapa, a) => {
    mapa[a.fuente] = (mapa[a.fuente] ?? 0) + a.galones;
    return mapa;
  }, {});

  const fuentes = Object.entries(porFuente).sort((a, b) => b[1] - a[1]);
  const soloDelFondo = fuentes.length === 1 && fuentes[0]?.[0] === 'FondoDeLaMision';

  return (
    <Panel
      titulo="Combustible que entró al tanque"
      acciones={
        puedeRegistrar && vehiculoId !== undefined ? (
          <Boton variante="secundario" tamano="sm" onClick={() => setRegistrando(true)}>
            Registrar abastecimiento
          </Boton>
        ) : undefined
      }
    >
      <div className="tw:flex tw:flex-col tw:gap-4">
        {isPending ? (
          <p className="tw:text-sm tw:text-tinta-mid">Cargando los abastecimientos…</p>
        ) : lista.length === 0 ? (
          // Cero es un dato: el vehículo salió con el tanque lleno y no cargó. Decirlo evita
          // leer el renglón en blanco como «falta registrar».
          <p className="tw:text-sm tw:text-tinta-mid">
            Esta misión no registró ningún abastecimiento. Si el vehículo cargó de alguna
            fuente —el tanque de la sede, una donación, el bolsillo del motorista—{' '}
            <b>esos galones no están en la conciliación</b>, y su ausencia se ve como
            rendimiento imposible.
          </p>
        ) : (
          <>
            {/* El total y su composición, arriba: es el numerador de `RN-30` y lo único que
                se lee de lejos. */}
            <div className="tw:flex tw:flex-wrap tw:items-end tw:gap-x-6 tw:gap-y-2">
              <div className="tw:flex tw:flex-col">
                <span className="tw:text-xs tw:uppercase tw:tracking-wide tw:text-tinta-mid">
                  Entró al tanque
                </span>
                <span className="tw:font-mono tw:text-xl tw:tabular-nums">
                  {enGalones(total)}
                </span>
              </div>

              {/* Se calla cuando todo vino del fondo: decir «100% del fondo» en cada misión
                  entrena a saltarse la línea, y con ella se pierde la que sí decía algo. */}
              {!soloDelFondo && (
                <div className="tw:flex tw:flex-wrap tw:gap-x-4 tw:gap-y-1 tw:text-sm">
                  {fuentes.map(([fuente, cantidad]) => (
                    <span key={fuente} className="tw:flex tw:items-baseline tw:gap-1.5">
                      <span className="tw:font-mono tw:tabular-nums">
                        {enGalones(cantidad)}
                      </span>
                      <span className="tw:text-xs tw:text-tinta-mid">
                        {TEXTO_DE_FUENTE[fuente] ?? fuente}
                      </span>
                    </span>
                  ))}
                </div>
              )}
            </div>

            <ul className="tw:flex tw:flex-col tw:gap-2">
              {lista.map((a) => (
                <AbastecimientoVista key={a.id} abastecimiento={a} />
              ))}
            </ul>
          </>
        )}
      </div>

      {registrando && vehiculoId !== undefined && (
        <DialogoDeAbastecimiento
          misionId={misionId}
          vehiculoId={vehiculoId}
          onCerrar={() => setRegistrando(false)}
        />
      )}
    </Panel>
  );
}

const TONO_DE_FUENTE: Record<string, Tono> = {
  FondoDeLaMision: 'info',
  TanqueInstitucional: 'aviso',
  OtraDependencia: 'aviso',
  Donacion: 'neutro',
  PeculioDelServidor: 'riesgo',
  TerceroEnApoyo: 'neutro',
};

function AbastecimientoVista({
  abastecimiento: a,
}: {
  abastecimiento: Abastecimiento;
}): ReactElement {
  return (
    <li className="tw:flex tw:flex-col tw:gap-1 tw:rounded-control tw:border tw:border-linea-suave tw:p-3">
      <div className="tw:flex tw:flex-wrap tw:items-center tw:justify-between tw:gap-2">
        <div className="tw:flex tw:items-center tw:gap-2">
          <span className="tw:font-mono tw:tabular-nums">{enGalones(a.galones)}</span>
          <Pastilla tono={TONO_DE_FUENTE[a.fuente] ?? 'neutro'}>
            {TEXTO_DE_FUENTE[a.fuente] ?? a.fuente}
          </Pastilla>
          {a.excedido && <Pastilla tono="riesgo">Excede el fondo</Pastilla>}
          {a.generaReintegro && <Pastilla tono="aviso">Genera reintegro</Pastilla>}
        </div>

        <span className="tw:font-mono tw:text-xs tw:tabular-nums tw:text-tinta-mid">
          odómetro {a.odometro.toLocaleString('es-HN')} km
        </span>
      </div>

      <p className="tw:text-xs tw:text-tinta-mid">
        {momentoCompleto(a.momento)} · {a.registra}
        {a.estacion && ` · ${a.estacion}`}
        {a.monto !== null ? ` · ${lempiras(a.monto)}` : ' · sin monto'}
      </p>

      {/* La ausencia de papel se dice, con su causa. `RN-85`: el registro no se omite nunca
          por falta de comprobante, pero tampoco se disimula. */}
      {a.comprobante === null && a.causaSinComprobante !== null && (
        <p className="tw:text-xs tw:text-aviso-fg">
          Sin comprobante: {a.causaSinComprobante}
        </p>
      )}

      {a.comprobante !== null && (
        <p className="tw:font-mono tw:text-xs tw:text-tinta-mid">
          comprobante {a.comprobante}
        </p>
      )}
    </li>
  );
}

// ── El diálogo ──────────────────────────────────────────────────────────────

function DialogoDeAbastecimiento({
  misionId,
  vehiculoId,
  onCerrar,
}: {
  misionId: string;
  vehiculoId: string;
  onCerrar(): void;
}): ReactElement {
  const cliente = useQueryClient();

  const [registra, setRegistra] = useState('');
  const [fuente, setFuente] = useState('TanqueInstitucional');
  const [galones, setGalones] = useState('');
  const [odometro, setOdometro] = useState('');
  const [monto, setMonto] = useState('');
  const [estacion, setEstacion] = useState('');
  const [comprobante, setComprobante] = useState('');
  const [causa, setCausa] = useState('');

  const elegida = FUENTES_REGISTRABLES.find((f) => f.valor === fuente);

  // `RN-85` sólo pide causa donde **debería haber papel**. A una donación pedirle la razón de
  // que no traiga factura obligaría a escribir «no aplica» siempre.
  const pideCausa = elegida?.traeComprobante === true && comprobante.trim() === '';

  const operacion = useMutation({
    mutationFn: () =>
      registrarAbastecimiento({
        id: ulid(),
        idVehiculo: vehiculoId,
        idMision: misionId,
        ocurridoEn: new Date().toISOString(),
        galones: Number(galones),
        odometro: Number(odometro),
        fuente,
        registra: registra.trim(),
        monto: monto.trim() === '' ? null : Number(monto),
        estacion: estacion.trim() === '' ? null : estacion.trim(),
        comprobante: comprobante.trim() === '' ? null : comprobante.trim(),
        causaSinComprobante: causa.trim() === '' ? null : causa.trim(),
      }),
    onSuccess: async () => {
      avisar.exito(`${galones} galones registrados. Ya cuentan en la conciliación.`);
      await cliente.invalidateQueries({ queryKey: ['abastecimientos', misionId] });
      // El dictamen cambia: los galones nuevos entran al denominador de `RN-30`.
      await cliente.invalidateQueries({ queryKey: ['dictamen'] });
      onCerrar();
    },
    onError: (e) => {
      if (e instanceof BloqueoDuro) {
        avisar.error(e.message);
        return;
      }
      avisar.error('No se pudo registrar el abastecimiento.');
    },
  });

  const completo =
    registra.trim() !== '' &&
    Number(galones) > 0 &&
    Number(odometro) > 0 &&
    (!pideCausa || causa.trim().length >= 10);

  return (
    <Modal
      abierto
      titulo="Registrar abastecimiento"
      descripcion="Todo galón que entra al tanque cuenta en la conciliación, venga de donde venga."
      onCerrar={onCerrar}
      acciones={
        <Boton
          disabled={!completo || operacion.isPending}
          cargando={operacion.isPending}
          onClick={() => operacion.mutate()}
        >
          Registrar
        </Boton>
      }
    >
      <div className="tw:flex tw:flex-col tw:gap-4">
        <CampoDeActor
          valor={registra}
          onCambiar={setRegistra}
          etiqueta="Quién lo registra"
          ayuda="Quien despachó el combustible o quien levanta el registro. Queda en la bitácora del vehículo."
        />

        <Campo etiqueta="Fuente" obligatorio ayuda={elegida?.ayuda ?? ''}>
          {(props) => (
            <select {...props} value={fuente} onChange={(e) => setFuente(e.target.value)}>
              {FUENTES_REGISTRABLES.map((f) => (
                <option key={f.valor} value={f.valor}>
                  {f.texto}
                </option>
              ))}
            </select>
          )}
        </Campo>

        {/* El del fondo no está en la lista, y decir por qué evita que alguien lo busque. */}
        <Nota tono="info">
          El combustible <b>con cargo al vale</b> no se registra acá: entra por el consumo del
          vale, porque además mueve el instrumento y descuenta del saldo del fondo.
        </Nota>

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

          <Campo
            etiqueta="Odómetro del momento"
            obligatorio
            ayuda="Ancla el galón a un tramo recorrido."
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
        </div>

        <div className="tw:grid tw:grid-cols-2 tw:gap-3">
          <Campo
            etiqueta="Monto"
            ayuda="Vacío si la fuente no lo tiene. Un galón sin precio sigue siendo un galón."
          >
            {(props) => (
              <input
                {...props}
                type="number"
                min={0}
                step="0.01"
                value={monto}
                onChange={(e) => setMonto(e.target.value)}
              />
            )}
          </Campo>

          <Campo etiqueta="Estación o lugar">
            {(props) => (
              <input {...props} value={estacion} onChange={(e) => setEstacion(e.target.value)} />
            )}
          </Campo>
        </div>

        {/* El comprobante y su causa sólo aparecen donde debería haber papel. Mostrarlos
            siempre haría que la casilla se rellenara con «no aplica» y dejara de leerse. */}
        {elegida?.traeComprobante === true && (
          <>
            <Campo etiqueta="Comprobante">
              {(props) => (
                <input
                  {...props}
                  value={comprobante}
                  onChange={(e) => setComprobante(e.target.value)}
                />
              )}
            </Campo>

            {pideCausa && (
              <Campo
                etiqueta="Por qué no hay comprobante"
                obligatorio
                ayuda="El registro no se omite nunca por falta de papel, pero tampoco se disimula: la causa es lo que sostiene el descargo alternativo."
              >
                {(props) => (
                  <textarea
                    {...props}
                    rows={2}
                    value={causa}
                    onChange={(e) => setCausa(e.target.value)}
                  />
                )}
              </Campo>
            )}
          </>
        )}

        {fuente === 'PeculioDelServidor' && (
          <Nota tono="aviso">
            Esto genera una <b>obligación de reintegro</b> a favor de quien pagó, y no toca el
            cuadre del fondo. ⚠️ El circuito de reintegro <b>no está construido</b>: queda
            registrado y pendiente, que es mejor que quedar fuera de todo registro.
          </Nota>
        )}
      </div>
    </Modal>
  );
}

/** Un ULID del cliente — `ADR-005`. Que el reintento no registre dos veces el mismo galón. */
function ulid(): string {
  const ALFABETO = '0123456789ABCDEFGHJKMNPQRSTVWXYZ';
  let salida = '';
  for (let i = 0; i < 26; i++) {
    salida += ALFABETO[Math.floor(Math.random() * ALFABETO.length)];
  }
  return salida;
}
