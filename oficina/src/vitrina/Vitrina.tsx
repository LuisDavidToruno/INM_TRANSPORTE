import { useEffect, useRef, useState } from 'react';
import type { ReactElement, ReactNode } from 'react';

import { Nota, Panel, SelectorApariencia } from '../ui';
import { useSeccionActiva } from './useSeccionActiva';

import ColorYContraste from './ColorYContraste';
import { MuestraCarrilesDeFlota } from './MuestraCarriles';
import EscalaTipografica from './EscalaTipografica';
import { EscalaEspaciado, GeometriaYElevacion, Iconografia } from './Fundamentos';
import {
  AvisosDeEsquina,
  EsqueletoContraRueda,
  EstadosDeCarga,
  MatrizEstados,
  MuestraBusqueda,
  MuestraFechas,
  MuestraFormulario,
} from './MuestrasComponentes';
import {
  AnatomiaFila,
  MuestraBarraDePasos,
  MuestraEtapas,
  MuestraModal,
  MuestraSelectorBuscable,
} from './MuestrasCompuestos';
import { DensidadComparada, LongitudesExtremas } from './Rigor';
import {
  CorrectoEIncorrecto,
  ReglasDelContrato,
  TonoDeVoz,
  VersionDelSistema,
} from './Gobierno';

/**
 * La vitrina del sistema de diseño.
 *
 * Existe para responder de un vistazo «¿con qué estamos construyendo?» — y para
 * que la respuesta no sea abrir cinco pantallas y deducirla.
 *
 * ⚠️ **Cada bloque de acá es el componente REAL**, no una maqueta. Si algo se ve
 * mal en esta página, está mal en producción. Una vitrina construida con marcado
 * suelto se ve idéntica el primer día y miente a las tres semanas, porque los
 * componentes evolucionan y la maqueta no.
 *
 * Es también donde se prueba lo que más se rompe en silencio: que TODO cambie de
 * tema junto. Un componente que trae su propia hoja de estilos —los diálogos de
 * librerías de terceros son el caso típico— se delata acá enseguida, porque se
 * queda claro cuando el resto se va a oscuro.
 */

/**
 * Sección con CANAL NUMERADO a la izquierda.
 *
 * Es la pieza central de la identidad, y resuelve un diagnóstico concreto: una
 * página así se lee como plantilla cuando **todos los bloques pesan igual** —
 * título, párrafo, controles, repetido sin jerarquía. El canal ancla todo a una
 * única línea vertical fuerte y numera el contenido, que es lo que la hace leer
 * como un registro y no como una lista de tarjetas.
 *
 * La rejilla es `3rem / 4.75rem`: el número vive en su propia columna, separado
 * del contenido por un filete que recorre la página entera.
 */
function Seccion({
  id,
  numero,
  titulo,
  descripcion,
  children,
}: {
  readonly id: string;
  readonly numero: number;
  readonly titulo: string;
  readonly descripcion: string;
  readonly children: ReactNode;
}): ReactElement {
  return (
    <section
      id={id}
      className="tw:grid tw:scroll-mt-4 tw:grid-cols-[3rem_1fr] tw:border-t tw:border-linea tw:bg-canvas tw:sm:grid-cols-[4.75rem_1fr]"
    >
      <div
        aria-hidden="true"
        className="loki-cifra tw:flex tw:justify-center tw:border-r tw:border-linea tw:bg-panel tw:pt-5 tw:text-ayuda tw:text-tinta-low"
      >
        {String(numero).padStart(2, '0')}
      </div>
      <div className="tw:min-w-0 tw:px-4 tw:py-5 tw:sm:px-7">
        <h2 className="tw:text-titulo tw:font-semibold tw:tracking-tight tw:text-tinta-hi">
          {titulo}
        </h2>
        <p className="tw:mt-1 tw:mb-3.5 tw:max-w-3xl tw:text-cuerpo-2 tw:text-tinta-mid">
          {descripcion}
        </p>
        <div className="tw:flex tw:flex-col tw:gap-5">{children}</div>
      </div>
    </section>
  );
}

/**
 * Una pieza dentro de una sección, con su propia ancla.
 *
 * Que cada pieza tenga `id` es lo que hace navegable la vitrina. Con todas
 * colgando de un ancla única, el índice ofrece un solo destino y te deja al
 * principio de un bloque de veinte — que es lo mismo que no navegar.
 *
 * `scroll-mt` despega el título del borde superior al saltar: sin eso el rótulo
 * queda tocando el filete y se lee como si perteneciera al bloque anterior.
 */
function Bloque({
  id,
  titulo,
  bajada,
  children,
}: {
  readonly id: string;
  readonly titulo: string;
  readonly bajada: string;
  readonly children: ReactNode;
}): ReactElement {
  return (
    <div
      id={id}
      className="tw:scroll-mt-4 tw:border-t tw:border-linea tw:pt-4 tw:first:border-t-0 tw:first:pt-0"
    >
      <h3 className="tw:text-titulo tw:font-semibold tw:text-tinta-hi">{titulo}</h3>
      <p className="tw:mt-0.5 tw:mb-3 tw:max-w-3xl tw:text-cuerpo-2 tw:text-tinta-mid">{bajada}</p>
      {children}
    </div>
  );
}

/** El índice. Vive al lado, pegajoso, para no perder el lugar al recorrer. */
const INDICE: readonly { readonly id: string; readonly texto: string }[] = [
  { id: 'apariencia', texto: 'Apariencia' },
  { id: 'fundamentos', texto: 'Fundamentos' },
  { id: 'componentes', texto: 'Componentes' },
  { id: 'compuestos', texto: 'Compuestos' },
  { id: 'graficos', texto: 'Gráficos' },
  { id: 'rigor', texto: 'Rigor' },
  { id: 'gobierno', texto: 'Gobierno' },
];

const IDS = INDICE.map((s) => s.id);

export default function Vitrina(): ReactElement {
  const raiz = useRef<HTMLDivElement>(null);
  const [contenedor, setContenedor] = useState<HTMLElement | null>(null);

  /**
   * El contenedor que hace scroll es el `<main>` del shell, no la ventana.
   *
   * Se busca subiendo desde acá con `closest` en vez de recibirlo por prop:
   * así la vitrina funciona igual montada dentro del shell, sola en una página,
   * o dentro de otro marco. Pedirlo por prop obligaría a que cada pantalla
   * supiera dónde vive, que es justo lo que no debería saber.
   */
  useEffect(() => {
    const main = raiz.current?.closest('main') ?? null;
    main?.classList.add('loki-scroll-suave');
    setContenedor(main);
    return () => main?.classList.remove('loki-scroll-suave');
  }, []);

  const activa = useSeccionActiva(IDS, contenedor);

  return (
    <div ref={raiz} className="loki-ruta tw:flex tw:gap-6">
      {/* El índice se oculta por debajo de `lg` en vez de apilarse arriba: en
          pantalla angosta un índice de siete destinos empuja el contenido fuera
          de la vista y hay que pasarlo de largo cada vez que se entra. */}
      <nav
        aria-label="Secciones del sistema de diseño"
        className="tw:hidden tw:w-44 tw:shrink-0 tw:lg:block"
      >
        <div className="tw:sticky tw:top-0 tw:flex tw:flex-col tw:gap-1 tw:py-5">
          <p className="loki-rotulo tw:mb-1 tw:text-cabecera tw:text-tinta-low">En esta página</p>
          {INDICE.map((s) => (
            <a
              key={s.id}
              href={`#${s.id}`}
              /* `aria-current` NO es decorativo: es lo que le dice a un lector de
                 pantalla cuál es la sección actual. La marca visual cuelga de ese
                 mismo atributo, así que lo que se ve y lo que se anuncia no pueden
                 divergir — que es el modo habitual en que un indicador «activo»
                 termina siendo sólo un color. */
              aria-current={activa === s.id}
              className={[
                'loki-indice-item loki-foco tw:rounded-control tw:py-1 tw:ps-3 tw:pe-2 tw:text-cuerpo-2',
                activa === s.id
                  ? 'tw:bg-subtle tw:font-semibold tw:text-tinta-hi'
                  : 'tw:text-tinta-mid tw:hover:bg-subtle tw:hover:text-tinta-hi',
              ].join(' ')}
            >
              {s.texto}
            </a>
          ))}
        </div>
      </nav>

      <div className="tw:min-w-0 tw:flex-1">
        <header className="tw:px-4 tw:py-6 tw:sm:px-7">
          <h1 className="tw:font-serif tw:text-pagina tw:text-tinta-hi">Sistema de diseño</h1>
          <p className="tw:mt-2 tw:max-w-3xl tw:text-cuerpo tw:text-tinta-mid">
            Todo lo que sigue son los componentes reales de{' '}
            <code className="tw:font-mono tw:text-tinta-base">src/ui</code>, con los tokens reales
            de <code className="tw:font-mono tw:text-tinta-base">src/marca</code>. Cambiá el tema o
            la densidad en la sección de abajo y mirá cómo se mueve todo junto: eso es lo que hay
            que conservar cuando agregues una pieza.
          </p>
        </header>

        <Seccion
          id="apariencia"
          numero={0}
          titulo="Apariencia"
          descripcion="Seis temas y dos densidades. Un tema reasigna VALORES de token y nunca agrega una regla de componente — si un tema necesitara una regla propia, faltaría un token."
        >
          <SelectorApariencia />
          <Nota tono="info">
            Ningún componente pregunta por el nombre del tema. Un{' '}
            <code className="tw:font-mono">if (tema === &apos;oscuro&apos;)</code> dentro de un
            componente es el contrato roto, y es exactamente lo que hace que agregar el séptimo
            tema deje de ser gratis.
          </Nota>
        </Seccion>

        <Seccion
          id="fundamentos"
          numero={1}
          titulo="Fundamentos"
          descripcion="Lo que está debajo de todo lo demás: color, tipografía, espaciado, geometría e iconografía. Cambiar algo de acá se ve en cada pantalla a la vez."
        >
          <Bloque
            id="color"
            titulo="Color y contraste"
            bajada="Cada par tinta/fondo del sistema, medido. El contraste no es una opinión: es un número, y por eso se audita en vez de discutirse."
          >
            <ColorYContraste />
          </Bloque>
          <Bloque
            id="tipografia"
            titulo="Escala tipográfica"
            bajada="Tres familias con un rol cada una. La serifa aparece SÓLO en títulos de página y de sección; la mono, en todo lo que se compara en columna."
          >
            <EscalaTipografica />
          </Bloque>
          <Bloque
            id="espaciado"
            titulo="Escala de espaciado"
            bajada="Base 4, con saltos que crecen. No es lineal a propósito: en una escala lineal dos niveles vecinos son indistinguibles y elegir cuál usar se vuelve arbitrario."
          >
            <EscalaEspaciado />
          </Bloque>
          <Bloque
            id="geometria"
            titulo="Geometría y elevación"
            bajada="Radios y sombras. La profundidad se da por borde y superficie, no por sombra pesada: una sombra fuerte sobrevive mal al cambio de tema."
          >
            <GeometriaYElevacion />
          </Bloque>
          <Bloque
            id="iconografia"
            titulo="Iconografía"
            bajada="Lucide, trazo 1.8, siempre en currentColor. Un ícono con color propio deja de seguir al tema y se nota justo en el que menos se prueba."
          >
            <Iconografia />
          </Bloque>
        </Seccion>

        <Seccion
          id="componentes"
          numero={2}
          titulo="Componentes"
          descripcion="Las piezas sueltas. Cada una resuelve un problema y ninguna sabe en qué pantalla está."
        >
          <Bloque
            id="estados"
            titulo="Matriz de estados"
            bajada="Un estado se decide por IDENTIFICADOR, nunca por su texto. «Por aprobar» contiene «aprob»: una comparación de cadenas la pinta de aprobada, y el error se ve recién en producción."
          >
            <MatrizEstados />
          </Bloque>
          <Bloque
            id="carga"
            titulo="Estados de carga"
            bajada="El esqueleto mide LO MISMO que el contenido que reemplaza. Si mide distinto, la página salta cuando llegan los datos."
          >
            <EstadosDeCarga />
          </Bloque>
          <Bloque
            id="esqueleto-vs-rueda"
            titulo="Esqueleto contra rueda"
            bajada="Cuándo va cada uno. La rueda no dice cuánto falta ni qué va a aparecer; el esqueleto sí, y por eso se siente más rápido aunque tarde igual."
          >
            <EsqueletoContraRueda />
          </Bloque>
          <Bloque
            id="formulario"
            titulo="Formulario"
            bajada="El error dice la CAUSA, no «campo requerido». Un mensaje que no explica qué hacer obliga a adivinar, y adivinar en un formulario largo se paga con el formulario entero."
          >
            <MuestraFormulario />
          </Bloque>
          <Bloque
            id="fechas"
            titulo="Fechas y rangos"
            bajada="Un solo día y un período. El calendario se abre donde hay lugar, no siempre hacia abajo."
          >
            <MuestraFechas />
          </Bloque>
          <Bloque
            id="busqueda"
            titulo="Búsqueda"
            bajada="El campo no puede prometer más de lo que hay: su texto dice qué se busca acá, porque quien escribe algo y no encuentra nada concluye que el sistema no lo tiene."
          >
            <MuestraBusqueda />
          </Bloque>
          <Bloque
            id="avisos"
            titulo="Avisos de esquina"
            bajada="Efímeros y sin decisión. Lo que exige una decisión va en un modal — un aviso que se va solo no puede pedir una confirmación."
          >
            <AvisosDeEsquina />
          </Bloque>
        </Seccion>

        <Seccion
          id="compuestos"
          numero={3}
          titulo="Compuestos"
          descripcion="Piezas que resuelven un problema entero. Son las que más se copian mal, porque parecen simples de rehacer y no lo son."
        >
          <Bloque
            id="fila"
            titulo="Anatomía de una fila"
            bajada="Los importes llevan tres clases: mono, tabular-nums y alineado a la derecha. Sin las tres los decimales no alinean y la columna deja de poder leerse de un vistazo."
          >
            <AnatomiaFila />
          </Bloque>
          <Bloque
            id="etapas"
            titulo="Rastreador de etapas"
            bajada="Dónde está un trámite y qué falta. El color nunca viaja solo: cada etapa lleva texto además del punto."
          >
            <MuestraEtapas />
          </Bloque>
          <Bloque
            id="pasos"
            titulo="Barra de pasos"
            bajada="Para un flujo con principio y fin. Distinta del rastreador: acá el usuario avanza, allá observa."
          >
            <MuestraBarraDePasos />
          </Bloque>
          <Bloque
            id="selector"
            titulo="Selector buscable"
            bajada="Cuando la lista es larga. Con menos de siete opciones un selector normal es mejor: abrir un buscador para elegir entre tres es fricción sin beneficio."
          >
            <MuestraSelectorBuscable />
          </Bloque>
          <Bloque
            id="modal"
            titulo="Modal"
            bajada="Sobre <dialog> nativo, no sobre una librería: la trampa de foco, Esc y el retorno del foco los da el navegador, y hacerlos a mano es donde se rompe la accesibilidad."
          >
            <MuestraModal />
          </Bloque>
        </Seccion>

        <Seccion
          id="rigor"
          numero={5}
          titulo="Rigor: lo que se rompe primero"
          descripcion="Un sistema se juzga por cómo aguanta lo que no se probó. Estas dos son las que más rápido delatan a una plantilla."
        >
          <Bloque
            id="densidad"
            titulo="Densidad comparada"
            bajada="La misma tabla en las dos densidades. La densidad NO cambia tamaños de letra: bajar el cuerpo para meter más filas es cómo una interfaz densa se vuelve ilegible."
          >
            <DensidadComparada />
          </Bloque>
          <Bloque
            id="carriles"
            titulo="Línea de tiempo por carriles"
            bajada="El mismo componente con datos de dos dominios: ocupación de flota y vigencias de documentos. Contesta lo que una tabla no — qué se solapa con qué y dónde queda el hueco."
          >
            <MuestraCarrilesDeFlota />
          </Bloque>
          <Bloque
            id="longitudes"
            titulo="Longitudes extremas"
            bajada="Nombres larguísimos, importes de siete cifras, celdas vacías. Los datos reales no son los de la maqueta, y el diseño que sólo funciona con datos cortos no funciona."
          >
            <LongitudesExtremas />
          </Bloque>
        </Seccion>

        <Seccion
          id="gobierno"
          numero={6}
          titulo="Gobierno"
          descripcion="Las reglas y de dónde salen. Sin esto el sistema dura hasta la primera pantalla con prisa."
        >
          <Bloque
            id="reglas"
            titulo="Reglas del contrato"
            bajada="Las que más se rompen, con su ejemplo. Cada una cierra un modo de falla concreto que ya ocurrió."
          >
            <ReglasDelContrato />
          </Bloque>
          <Bloque
            id="correcto"
            titulo="Correcto e incorrecto"
            bajada="El mismo componente bien y mal usado, lado a lado. Es lo único que enseña más rápido que la regla escrita."
          >
            <CorrectoEIncorrecto />
          </Bloque>
          <Bloque
            id="tono"
            titulo="Tono de voz"
            bajada="Cómo habla el sistema. Un botón que dice «Aceptar» y otro que dice «Guardar cambios» no son el mismo producto."
          >
            <TonoDeVoz />
          </Bloque>
          <Bloque
            id="version"
            titulo="Versión"
            bajada="Qué versión del contrato implementa esta copia, y dónde vive."
          >
            <VersionDelSistema />
          </Bloque>
        </Seccion>

        <footer className="tw:border-t tw:border-linea tw:px-4 tw:py-6 tw:sm:px-7">
          <Panel titulo="Cómo se agrega una pieza">
            <ol className="tw:flex tw:flex-col tw:gap-2 tw:text-cuerpo-2 tw:text-tinta-mid">
              <li>
                <strong className="tw:text-tinta-base">1 · Fijate si ya existe.</strong> La mitad de
                los componentes nuevos son uno viejo con otro nombre.
              </li>
              <li>
                <strong className="tw:text-tinta-base">2 · Escribila en</strong>{' '}
                <code className="tw:font-mono">src/ui</code>, consumiendo tokens. Si necesitás un
                color que no está, <strong>falta un token</strong>: pedilo, no lo escribas a mano.
              </li>
              <li>
                <strong className="tw:text-tinta-base">3 · Exportala en el barril</strong>{' '}
                <code className="tw:font-mono">src/ui/index.ts</code>. Lo que no está ahí es
                privado del sistema, aunque el archivo lo exporte.
              </li>
              <li>
                <strong className="tw:text-tinta-base">4 · Mostrala acá.</strong> Una pieza que no
                está en la vitrina no existe para el resto del equipo, y en dos meses alguien la
                vuelve a escribir.
              </li>
            </ol>
          </Panel>
        </footer>
      </div>
    </div>
  );
}
