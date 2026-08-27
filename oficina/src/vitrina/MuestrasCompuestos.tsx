import { FileText } from 'lucide-react';
import { useState } from 'react';
import type { ReactElement, ReactNode } from 'react';

import Boton from '../ui/Boton';
import CajonExpediente from '../ui/CajonExpediente';
import Campo from '../ui/Campo';
import Modal from '../ui/Modal';
import Pastilla from '../ui/Pastilla';
import RastreadorEtapas from '../ui/RastreadorEtapas';
import SelectorBuscable from '../ui/SelectorBuscable';
import Tabla from '../ui/Tabla';
import { ESTADO_INTERNO, ETAPA } from '../ui/tipos';
import type { EstadoEtapa } from '../ui/RastreadorEtapas';

/** Un paso: su estado, y si además está cumplido — que NO es lo mismo. */
type Paso = readonly [EstadoEtapa, boolean];
import type { ColumnaDef, Plazo } from '../ui/tipos';


/** Muestras de los tres compuestos: etapas, modal y la anatomía de la fila. */

export function MuestraEtapas(): ReactElement {
  const [devuelta, setDevuelta] = useState(false);
  return (
    <div className="tw:flex tw:flex-col tw:gap-3">
      <RastreadorEtapas
        etapaActual={ETAPA.PRESUPUESTO}
        devueltaEn={devuelta ? ETAPA.REV_VIATICOS : undefined}
        cargando={false}
      />
      <div className="tw:flex tw:items-center tw:gap-3">
        <Boton tamano="sm" onClick={() => setDevuelta((d) => !d)}>
          {devuelta ? 'Quitar la devolución' : 'Marcar devuelta en Viáticos'}
        </Boton>
        <span className="tw:text-cuerpo-2 tw:text-tinta-low">
          El conector se tiñe <strong>hasta donde llegó</strong> el trámite.
        </span>
      </div>
      <p className="tw:text-cuerpo-2 tw:text-tinta-low">
        El estado del flujo era una pastilla, y una pastilla no dice{' '}
        <strong>cuánto queda</strong>. Los cuatro estados se distinguen por{' '}
        <strong>forma además de color</strong> —disco lleno, anillo con punto, anillo hueco—
        porque en una fotocopia el color desaparece y la forma sigue ahí.
      </p>
    </div>
  );
}

/**
 * Una barra de pasos cualquiera, dibujada con el vocabulario del rastreador.
 *
 * No usa `RastreadorEtapas` a propósito: ese componente tiene las ocho etapas del workflow
 * cableadas dentro y deriva su estado del ORDEN. Acá hace falta lo contrario — poder poner
 * cualquier combinación, incluidas las que el workflow no puede producir.
 */
function BarraDePasos({ estados }: { readonly estados: readonly Paso[] }): ReactElement {
  return (
    <ol className="tw:flex tw:items-start tw:pb-0.5">
      {estados.map(([e, cumplido], i) => (
        <li key={i} data-e={e} data-hecho={cumplido ? '' : undefined} className="loki-et">
          <i aria-hidden="true" />
          <b>Paso {i + 1}</b>
          <em>{cumplido && e !== 'hecho' ? `${e} · cumplido` : e}</em>
        </li>
      ))}
    </ol>
  );
}

/**
 * Todas las combinaciones que la barra puede recibir — incluidas las que no son monótonas.
 *
 * ── Por qué existe esta muestra ──────────────────────────────────────────────
 * El rastreador se dibujaba con el conector teñido según el paso al que LLEGA. Con un avance
 * monótono —el del workflow, donde el estado sale del orden— eso da siempre bien: si llegaste a
 * un paso, el anterior está hecho por construcción.
 *
 * La barra del wizard **no es monótona**: ahí «completo» es una condición de datos, no una
 * posición. Se puede estar en el paso 3 con el 2 sin terminar. El 2026-08-12 se vio en pantalla:
 * un disco gris del que salía una línea verde — el dibujo afirmando un avance que el propio
 * disco negaba.
 *
 * ── La regla, en su tercera versión ─────────────────────────────────────────
 * Primero se teñía por el paso de LLEGADA. Después por el de ORIGEN, que arregló el disco gris
 * con línea verde saliendo. Pero el 2026-08-13, mirando la 1323 parado en el 3 con el 2 sin
 * terminar, quedó a la vista lo que faltaba: **el tramo verde moría contra un disco pendiente**,
 * y eso se lee como «el avance llegó hasta acá» sobre un paso que no está hecho.
 *
 * La regla final: **un tramo se tiñe sólo si SUS DOS EXTREMOS están cumplidos** — es un camino
 * recorrido, y un camino no atraviesa un paso incompleto. Única excepción, el tramo que llega al
 * paso ACTUAL: ahí el segundo extremo es donde uno está parado, no algo que deba estar hecho.
 *
 * Estos casos son la prueba de que vale para cualquier combinación, no sólo para la que el
 * workflow produce.
 */
export function MuestraBarraDePasos(): ReactElement {
  const casos: readonly { readonly titulo: string; readonly estados: readonly Paso[] }[] = [
    { titulo: 'Recién empezando', estados: [['actual', false], ['pendiente', false], ['pendiente', false], ['pendiente', false]] },
    { titulo: 'Avance parejo', estados: [['hecho', true], ['hecho', true], ['actual', false], ['pendiente', false]] },
    {
      titulo: 'Salteado — se está en el 3 con el 2 sin terminar',
      estados: [['hecho', true], ['pendiente', false], ['actual', false], ['pendiente', false]],
    },
    {
      titulo: 'Parado en un paso YA CUMPLIDO — sólo lectura, o una corrección',
      estados: [['hecho', true], ['actual', true], ['hecho', true], ['pendiente', false]],
    },
    {
      titulo: 'Todo cumplido, parado en el primero — la solicitud ya se envió',
      estados: [['actual', true], ['hecho', true], ['hecho', true], ['hecho', true]],
    },
    {
      titulo: 'Volvió atrás a corregir el 1, con el 3 ya hecho',
      estados: [['actual', false], ['pendiente', false], ['hecho', true], ['pendiente', false]],
    },
    { titulo: 'Nada hecho todavía', estados: [['pendiente', false], ['pendiente', false], ['pendiente', false], ['pendiente', false]] },
    { titulo: 'Con una devolución en el medio', estados: [['hecho', true], ['devuelto', false], ['actual', false], ['pendiente', false]] },
    {
      titulo: 'Devolución sobre un tramo sin recorrer',
      estados: [['pendiente', false], ['devuelto', false], ['pendiente', false], ['pendiente', false]],
    },
  ];

  return (
    <div className="tw:flex tw:flex-col tw:gap-5">
      {casos.map((c) => (
        <div key={c.titulo} className="tw:flex tw:flex-col tw:gap-1.5">
          <p className="tw:text-cuerpo-2 tw:text-tinta-low">{c.titulo}</p>
          <BarraDePasos estados={c.estados} />
        </div>
      ))}
      <p className="tw:text-cuerpo-2 tw:text-tinta-low">
        En los nueve, <strong>un tramo está teñido exactamente cuando sus dos extremos se
        recorrieron</strong>. Mírense los dos salteados: el avance no es continuo, así que{' '}
        <strong>no hay ni un tramo verde</strong> — lo que quedó hecho lo dice su propio disco. La
        excepción es el tramo que llega al paso <em>actual</em>: ahí el segundo extremo es donde
        uno está parado, no algo que deba estar cumplido.
      </p>
      <p className="tw:text-cuerpo-2 tw:text-tinta-low">
        Y un paso puede ser el <em>actual</em> y estar cumplido a la vez —{' '}
        <code>data-e</code> no puede decir las dos cosas, así que el cumplimiento viaja aparte en{' '}
        <code>data-hecho</code>. Sin eso, una solicitud terminada abierta en un paso del medio se
        dibujaba <strong>verde · gris · verde</strong>.
      </p>
    </div>
  );
}

export function MuestraModal(): ReactElement {
  const [cual, setCual] = useState<null | 'confirmar' | 'destruir' | 'capturar'>(null);
  const [cajon, setCajon] = useState(false);

  return (
    <div className="tw:flex tw:flex-col tw:gap-3">
      <div className="tw:flex tw:flex-wrap tw:gap-2">
        <Boton onClick={() => setCual('confirmar')}>Confirmar</Boton>
        <Boton variante="peligro" onClick={() => setCual('destruir')}>
          Destruir
        </Boton>
        <Boton variante="secundario" onClick={() => setCual('capturar')}>
          Capturar
        </Boton>
        <span className="tw:w-px tw:self-stretch tw:bg-linea" aria-hidden="true" />
        <Boton variante="secundario" onClick={() => setCajon(true)}>
          Abrir cajón
        </Boton>
      </div>

      <Modal
        abierto={cual === 'confirmar'}
        onCerrar={() => setCual(null)}
        rotulo="Etapa 3 de 8 · Revisión Inicial Viáticos"
        titulo="Aprobar solicitud SOL-01293"
        descripcion="Pasa a Revisión Presupuesto y se reserva la partida de la gerencia. La reserva se libera sola si la etapa se devuelve."
        acciones={<Boton variante="primario">Aprobar y continuar</Boton>}
      >
        <dl className="loki-dl-dos-columnas tw:grid tw:gap-1.5">
          <dt className="tw:text-tinta-mid">Solicitante</dt>
          <dd className="tw:text-tinta-base">Katherin Casildo</dd>
          <dt className="tw:text-tinta-mid">Monto a reservar</dt>
          <dd className="tw:font-mono tw:tabular-nums tw:text-tinta-hi">L. 49,990.65</dd>
          <dt className="tw:text-tinta-mid">Partida disponible</dt>
          <dd className="tw:font-mono tw:tabular-nums tw:text-tinta-hi">L. 312,400.00</dd>
        </dl>
      </Modal>

      <Modal
        abierto={cual === 'destruir'}
        onCerrar={() => setCual(null)}
        rotulo="Acción irreversible"
        titulo="Anular la gira SOL-01293"
        descripcion="Se anulan también 6 comprobantes y el informe del líder. La partida reservada vuelve a la gerencia."
        destructivo
        confirmacion="SOL-01293"
        acciones={<Boton variante="peligro-solido">Anular la gira</Boton>}
      >
        <p className="tw:rounded-control tw:border tw:px-3 tw:py-2 tono-riesgo">
          Esta solicitud ya fue aprobada por Dirección. Anularla obliga a crear una nueva si el
          viaje sigue en pie.
        </p>
      </Modal>

      {/* CAPTURAR — la tercera forma. Existe para que nadie invente una cuarta:
          cuando hace falta un dato antes de seguir, se usa ésta, no un modal a
          medida. Sin rótulo de etapa y sin banda: no decide nada del flujo, sólo
          recoge un dato. */}
      <Modal
        abierto={cual === 'capturar'}
        onCerrar={() => setCual(null)}
        titulo="Devolver SOL-01293 a Presupuesto"
        descripcion="El motivo llega al oficial que la tiene asignada y queda en el expediente. La solicitud vuelve a la etapa anterior."
        acciones={<Boton variante="primario">Devolver con el motivo</Boton>}
      >
        <div className="tw:flex tw:flex-col tw:gap-3">
          <Campo
            etiqueta="Motivo de la devolución"
            obligatorio
            ayuda="Lo lee el oficial, no el sistema: decí qué falta, no «datos incorrectos»."
          >
            {/* Pelados a propósito: es la muestra del contrato, y el contrato dice que
                <Campo> estiliza a su hijo. Traían una copia a mano de borde, fondo y
                tipografía — la página que enseña la regla enseñaba a saltársela. */}
            <textarea
              rows={3}
              defaultValue="La zona aplicada es 2 y el destino es Puerto Cortés, que corresponde a zona 1. Recalcular el monto antes de reenviar."
            />
          </Campo>
          <Campo etiqueta="Etapa de destino">
            <select defaultValue="presupuesto">
              <option value="presupuesto">Revisión Presupuesto</option>
              <option value="viaticos">Revisión Inicial Viáticos</option>
              <option value="solicitante">Solicitante</option>
            </select>
          </Campo>
        </div>
      </Modal>

      <MuestraCajon abierto={cajon} onCerrar={() => setCajon(false)} />

      <p className="tw:text-cuerpo-2 tw:text-tinta-low">
        <strong>Tres formas y sólo tres.</strong> <strong>Confirmar</strong> decide algo del flujo;{' '}
        <strong>destruir</strong> pide <strong>escribir la referencia</strong>, porque un botón rojo
        se aprieta por reflejo después del tercero, y la banda dice el{' '}
        <strong>alcance real del daño</strong> —«se anulan también 6 comprobantes»—, no «esta acción
        no se puede deshacer»; <strong>capturar</strong> recoge un dato y no decide nada. La cuarta
        forma no existe: si algo no entra en las tres, se replantea la acción, no el modal.
      </p>
      <p className="tw:text-cuerpo-2 tw:text-tinta-low">
        El <strong>cajón no es un modal</strong> y por eso está separado: el expediente se consulta{' '}
        <strong>mientras</strong> se trabaja la fila. Son 512 px justamente para que lo de la
        izquierda siga siendo legible —uno que tapa todo es un modal con otro nombre— y entra
        deslizándose desde el borde para decir de dónde viene.
      </p>
    </div>
  );
}

/**
 * Contenido del cajón. Vive acá y no en la vitrina para que la muestra sea el
 * componente real con datos reales de una solicitud, no un panel vacío.
 */
function MuestraCajon({ abierto, onCerrar }: {
  readonly abierto: boolean;
  readonly onCerrar: () => void;
}): ReactElement {
  const datos: readonly (readonly [string, ReactNode])[] = [
    ['Gerencia', 'Gerencia de Tecnología'],
    ['Destino', 'Puerto Cortés'],
    ['Fechas', '04–08/08/2026'],
    ['Días', '5'],
    ['Zona · categoría', 'Zona 2 — costa norte · Categoría III'],
    ['Monto', <span className="tw:font-mono tw:tabular-nums tw:text-tinta-hi">L. 49,990.65</span>],
    ['Oficial asignado', 'Marlon Ariel Suazo'],
    ['Plazo de atención', <Pastilla tono="riesgo">Plazo vencido</Pastilla>],
    ['Ciclos de corrección', '2 de 5'],
  ];

  const documentos: readonly { nombre: string; estado: string; tono: 'ok' | 'neutro' }[] = [
    { nombre: 'Memorándum de solicitud', estado: 'firmado', tono: 'ok' },
    { nombre: 'Cuadro de gira', estado: 'firmado', tono: 'ok' },
    { nombre: 'Autorización de anticipo', estado: 'firmado', tono: 'ok' },
    { nombre: 'Orden de pago', estado: 'pendiente', tono: 'neutro' },
  ];

  return (
    <CajonExpediente
      abierto={abierto}
      referencia="SOL-01293"
      titulo="Katherin Casildo"
      onCerrar={onCerrar}
      acciones={
        <>
          <Boton variante="secundario" onClick={onCerrar}>
            Devolver
          </Boton>
          <Boton variante="primario" onClick={onCerrar}>
            Aprobar
          </Boton>
        </>
      }
    >
      <div className="tw:flex tw:flex-col tw:gap-4">
        <RastreadorEtapas etapaActual={ETAPA.REV_VIATICOS} cargando={false} />

        <dl className="loki-dl-dos-columnas tw:grid tw:gap-x-4 tw:gap-y-2">
          {datos.map(([rotulo, valor]) => (
            <div key={rotulo} className="tw:contents">
              <dt className="tw:text-tinta-mid">{rotulo}</dt>
              <dd className="tw:text-right tw:text-tinta-base">{valor}</dd>
            </div>
          ))}
        </dl>

        <section className="tw:rounded-panel tw:border tw:border-linea">
          <header className="tw:border-b tw:border-linea-suave tw:px-3.5 tw:py-2.5">
            <h3 className="tw:text-cuerpo-2 tw:font-semibold tw:text-tinta-hi">
              Documentos del expediente
            </h3>
            <p className="tw:text-rotulo tw:text-tinta-low">{documentos.length} documentos</p>
          </header>
          <ul>
            {documentos.map((d) => (
              <li
                key={d.nombre}
                className="tw:flex tw:items-center tw:justify-between tw:gap-3 tw:border-b tw:border-linea-suave tw:px-3.5 tw:py-2.5 tw:last:border-b-0"
              >
                <span className="tw:flex tw:min-w-0 tw:items-center tw:gap-2 tw:text-tinta-base">
                  <FileText size={15} strokeWidth={1.8} className="tw:shrink-0 tw:text-tinta-low" />
                  <span className="tw:truncate">{d.nombre}</span>
                </span>
                <Pastilla tono={d.tono}>{d.estado}</Pastilla>
              </li>
            ))}
          </ul>
        </section>
      </div>
    </CajonExpediente>
  );
}

interface Fila {
  ref: string;
  solicitante: string;
  etapa: 1 | 2 | 3 | 4 | 5;
  monto: string;
  plazo: Plazo;
  dias: string;
}

const FILAS_BASE: Fila[] = [
  { ref: 'SOL-01293', solicitante: 'Katherin Casildo', etapa: 1, monto: 'L. 49,990.65', plazo: 'vencido', dias: '−2 d' },
  { ref: 'SOL-01291', solicitante: 'Óscar Manuel Zavala', etapa: 5, monto: 'L. 12,480.00', plazo: 'porvencer', dias: '1 d' },
  { ref: 'MSV-26-1255-GT', solicitante: 'Nolvia Esperanza Cruz Interiano', etapa: 3, monto: 'L. 1,284,990.65', plazo: 'vencido', dias: '−5 d' },
  { ref: 'EXT-01292', solicitante: 'Dilcia Ramos Portillo', etapa: 2, monto: 'USD 1,240.00', plazo: 'aldia', dias: '3 d' },
];

/**
 * Cuarenta filas, para que la muestra PAGINE.
 *
 * No es relleno: con las cuatro de arriba el paginador no se dibujaba nunca, así que el
 * sistema de diseño **no lo documentaba** — quien venía a consultarlo no podía saber que
 * existía ni cómo se ve. Y son cuarenta y no doce a propósito: con diez páginas aparece el
 * recorte con puntos suspensivos, que es justo la parte que hay que ver antes de confiar
 * en ella.
 */
const FILAS: Fila[] = Array.from({ length: 40 }, (_, i) => {
  const base = FILAS_BASE[i % FILAS_BASE.length]!;
  // La clave tiene que ser única o React reusaría filas; las cuatro primeras se dejan tal
  // cual para que la muestra siga abriendo con los casos de siempre.
  return i < FILAS_BASE.length ? base : { ...base, ref: `${base.ref}-${i}` };
});

export function AnatomiaFila(): ReactElement {
  const [elegidas, setElegidas] = useState<string[]>([]);

  const columnas: ColumnaDef<Fila>[] = [
    { id: 'ref', cabecera: 'Referencia', celda: (f) => <a className="loki-ref" href="#">{f.ref}</a> },
    { id: 'sol', cabecera: 'Solicitante', celda: (f) => <span className="loki-nm">{f.solicitante}</span> },
    { id: 'etapa', cabecera: 'Etapa', celda: (f) => <Pastilla tono={ESTADO_INTERNO[f.etapa].tono}>{ESTADO_INTERNO[f.etapa].texto}</Pastilla> },
    { id: 'monto', cabecera: 'Monto', numerica: true, celda: (f) => f.monto },
    { id: 'plazo', cabecera: 'Plazo', numerica: true, celda: (f) => f.dias },
  ];

  return (
    <div className="tw:flex tw:flex-col tw:gap-3">
      <div className="tw:rounded-panel tw:border tw:border-linea tw:bg-panel">
        <Tabla
          columnas={columnas}
          filas={FILAS}
          claveDe={(f) => f.ref}
          rotulo="Solicitudes de muestra"
          plazoDe={(f) => f.plazo}
          // Cuatro por página: alcanza para que se vean diez páginas sin que la muestra
          // ocupe media pantalla, y el paginador es lo que se viene a mirar acá.
          porPagina={4}
          seleccion={{ ids: elegidas, onChange: setElegidas }}
          expansion={{
            render: (f) => (
              <dl className="loki-dl-dos-columnas tw:grid tw:gap-1.5 tw:text-cuerpo-2">
                <dt className="tw:text-tinta-mid">Referencia</dt>
                <dd className="tw:font-mono tw:text-tinta-hi">{f.ref}</dd>
                <dt className="tw:text-tinta-mid">Monto</dt>
                <dd className="tw:font-mono tw:tabular-nums tw:text-tinta-hi">{f.monto}</dd>
              </dl>
            ),
          }}
          accionesFila={() => (
            <>
              <Boton tamano="sm" variante="secundario">Devolver</Boton>
              <Boton tamano="sm" variante="primario">Aprobar</Boton>
            </>
          )}
        />
      </div>
      <p className="tw:text-cuerpo-2 tw:text-tinta-low">
        La <strong>marca de 3 px</strong> del borde izquierdo es lo que permite escanear el estado
        sin leer la columna. Las acciones aparecen en <code className="tw:font-mono">:hover</code> Y
        en <code className="tw:font-mono">:focus-within</code> — tabulá por la tabla y vas a verlas
        aparecer. El detalle se abre como <strong>fila hermana</strong>, no en un modal: se compara
        con las vecinas.
      </p>
      <p className="tw:text-cuerpo-2 tw:text-tinta-low">
        El <strong>paginador</strong> dice dos cosas distintas y las dos hacen falta: el{' '}
        <strong>rango</strong> —«5–8 de 40»— contesta cuántas quedan, que es lo que importa
        trabajando una bandeja; los <strong>números</strong> contestan adónde se puede ir. Con más
        de siete páginas se recorta con puntos suspensivos, pero la tira <strong>mantiene su
        ancho</strong> en cualquier página: si cambiara, los números se correrían bajo el dedo
        justo mientras se los pulsa.
      </p>
    </div>
  );
}

/**
 * El selector buscable, con el caso real que lo motivó.
 *
 * Las opciones son las oficinas de verdad, con sus nombres verdaderos — que es donde está el
 * argumento: escribir «tela» en el `<select>` nativo no encuentra nada, porque su búsqueda va
 * por prefijo y la opción se llama «Delegación Tela».
 */
export function MuestraSelectorBuscable(): ReactElement {
  const [valor, setValor] = useState('');
  const opciones = [
    { valor: '1', etiqueta: 'CAMI Belén · San Pedro Sula, Cortés · zona 1', grupo: 'Centro de Atención para el Migrante Irregular (CAMI)', buscarTambien: 'San Pedro Sula, Cortés' },
    { valor: '2', etiqueta: 'CAMR Omoa · Omoa, Cortés · zona 3', grupo: 'Centro de Atención para el Migrante Regular (CAMR)', buscarTambien: 'Omoa, Cortés' },
    { valor: '3', etiqueta: 'Delegación Tela · Tela, Atlántida · zona 1', grupo: 'Delegación terrestre', buscarTambien: 'Tela, Atlántida' },
    { valor: '4', etiqueta: 'Delegación Roatán · Roatan, Islas de la Bahía · zona 1', grupo: 'Delegación marítima', buscarTambien: 'Roatan, Islas de la Bahía' },
    { valor: '5', etiqueta: 'Delegación Choluteca · Choluteca, Choluteca · zona 1', grupo: 'Delegación terrestre', buscarTambien: 'Choluteca, Choluteca' },
    { valor: '6', etiqueta: 'Aeropuerto Palmerola · Comayagua, Comayagua · zona 1', grupo: 'Delegación aérea', buscarTambien: 'Comayagua' },
  ];

  return (
    <div className="tw:flex tw:flex-col tw:gap-3">
      <div className="tw:max-w-lg">
        <SelectorBuscable
          opciones={opciones}
          valor={valor}
          onCambio={setValor}
          buscarPlaceholder="Nombre, municipio o departamento…"
        />
      </div>
      <p className="tw:text-cuerpo-2 tw:text-tinta-low">
        Pruebe <strong>«tela»</strong>, <strong>«roatan»</strong> sin tilde, o{' '}
        <strong>«comayagua»</strong> — que es el municipio y no está al principio del nombre. En un{' '}
        <code>&lt;select&gt;</code> nativo ninguna de las tres encuentra nada: su búsqueda por
        tecleo va <strong>por prefijo</strong>, y de las 38 oficinas del INM{' '}
        <strong>27 empiezan con «Delegación»</strong>.
      </p>
      <div className="tw:max-w-lg">
        <p className="tw:mb-1 tw:text-ayuda tw:text-tinta-low">
          Con el catálogo todavía en vuelo y un valor ya elegido:
        </p>
        <SelectorBuscable opciones={[]} valor="3" onCambio={() => undefined} />
      </div>
      <p className="tw:text-cuerpo-2 tw:text-tinta-low">
        Ese segundo caso dura un cuarto de segundo y es el que se hace mal: al abrir una edición el
        formulario ya trae la oficina elegida, pero las 38 opciones todavía viajan, así que no se
        sabe cómo se llama. Decir ahí <em>«Seleccione…»</em> afirma que no hay nada elegido —{' '}
        <strong>es falso</strong>, y el usuario vuelve a elegir lo que ya estaba puesto.
      </p>

      <p className="tw:text-cuerpo-2 tw:text-tinta-low">
        🚫 <strong>No reemplaza al desplegable nativo en todos lados.</strong> Con tres o cinco
        opciones el nativo sigue siendo mejor: menos código, comportamiento táctil del sistema y
        accesibilidad de fábrica. Esto es para listas donde <strong>encontrar</strong> es el
        trabajo.
      </p>
    </div>
  );
}
