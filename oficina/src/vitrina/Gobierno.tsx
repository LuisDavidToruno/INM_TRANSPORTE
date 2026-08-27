import type { ReactElement, ReactNode } from 'react';

import Boton from '../ui/Boton';
import Pastilla from '../ui/Pastilla';


/**
 * Bloque «Gobierno» de la vitrina: cómo se mantiene vivo el sistema.
 *
 * Es lo único que frena a alguien de degradarlo en el primer sprint — y por eso
 * viaja DENTRO del producto y no en un documento aparte que nadie abre.
 */

/* ── Correcto e incorrecto ─────────────────────────────────────────────────── */

function Par({
  n,
  regla,
  correcto,
  porCorrecto,
  incorrecto,
  porIncorrecto,
}: {
  readonly n: number;
  readonly regla: string;
  readonly correcto: ReactNode;
  readonly porCorrecto: string;
  readonly incorrecto: ReactNode;
  readonly porIncorrecto: string;
}): ReactElement {
  return (
    <div className="tw:flex tw:flex-col tw:gap-2 tw:border-t tw:border-linea-suave tw:pt-3">
      <p className="tw:text-cuerpo-2 tw:font-semibold tw:text-tinta-hi">
        <span className="tw:font-mono tw:text-tinta-axis">{n} · </span>
        {regla}
      </p>
      <div className="tw:flex tw:flex-wrap tw:gap-3">
        {[
          { rot: 'Correcto', tono: 'tono-ok', muestra: correcto, por: porCorrecto },
          { rot: 'Incorrecto', tono: 'tono-riesgo', muestra: incorrecto, por: porIncorrecto },
        ].map((c) => (
          <div
            key={c.rot}
            className="tw:min-w-0 tw:flex-1 tw:rounded-control tw:border tw:border-linea tw:bg-panel tw:p-3"
          >
            <span
              className={`${c.tono} tw:mb-2 tw:inline-block tw:rounded-badge tw:border tw:px-1.5 tw:text-cabecera tw:font-semibold tw:tracking-[.08em] tw:uppercase`}
            >
              {c.rot}
            </span>
            <div className="tw:flex tw:flex-wrap tw:items-center tw:gap-2">{c.muestra}</div>
            <p className="tw:mt-2 tw:text-ayuda tw:text-tinta-low">{c.por}</p>
          </div>
        ))}
      </div>
    </div>
  );
}

export function CorrectoEIncorrecto(): ReactElement {
  return (
    <div className="tw:flex tw:flex-col tw:gap-4">
      <Par
        n={1}
        regla="El estado se decide por identificador"
        correcto={<Pastilla tono="aviso">Por aprobar</Pastilla>}
        porCorrecto="ESTADO.EJECUTADA → aviso. El mapa vive en un solo lugar y la etiqueta es sólo presentación."
        incorrecto={<Pastilla tono="ok">Por aprobar</Pastilla>}
        porIncorrecto="texto.includes('aprob'). «Por aprobar» contiene «aprob» y se pinta de aprobada: justo lo contrario de lo que dice."
      />

      <Par
        n={2}
        regla="El color nunca viaja solo"
        correcto={
          <>
            <Pastilla tono="riesgo">Plazo vencido</Pastilla>
            <Pastilla tono="ok">Al día</Pastilla>
          </>
        }
        porCorrecto="Punto y texto. Funciona en daltonismo, monitor mal calibrado y fotocopia."
        incorrecto={
          <>
            <span className="loki-punto-suelto tw:bg-riesgo-fg" />
            <span className="loki-punto-suelto tw:bg-ok-fg" />
          </>
        }
        porIncorrecto="Sólo el punto. El usuario tiene que aprender un código de colores que nadie le enseñó."
      />

      <Par
        n={3}
        regla="Los importes alinean"
        correcto={
          <span className="tw:flex tw:flex-col tw:items-end tw:font-mono tw:tabular-nums tw:text-tinta-hi">
            <span>L. 1,284,990.65</span>
            <span>L. 49,990.65</span>
            <span>L. 1,950.00</span>
          </span>
        }
        porCorrecto="font-mono tabular-nums text-right. Los decimales forman columna y la magnitud se compara sin leer."
        incorrecto={
          <span className="tw:flex tw:flex-col tw:text-tinta-hi">
            <span>L. 1,284,990.65</span>
            <span>L. 49,990.65</span>
            <span>L. 1,950.00</span>
          </span>
        }
        porIncorrecto="Sans, a la izquierda, sin cifras tabulares. Hay que leer cada número entero para saber cuál es mayor."
      />

      <Par
        n={4}
        regla="El error dice la causa"
        correcto={
          <span className="tw:text-ayuda tw:text-riesgo-fg">
            Sin una fecha posterior al inicio no se puede calcular la duración de la gira.
          </span>
        }
        porCorrecto="Dice qué esperaba el sistema y por qué le importa."
        incorrecto={<span className="tw:text-ayuda tw:text-riesgo-fg">Campo inválido</span>}
        porIncorrecto="Obliga a adivinar. El usuario prueba formatos hasta que uno pasa."
      />

      <Par
        n={5}
        regla="La acción sin permiso se muestra deshabilitada"
        correcto={
          <>
            <Boton tamano="sm" variante="secundario" disabled title="Requiere permiso de Viáticos">
              Devolver
            </Boton>
            <Boton tamano="sm" variante="primario">
              Aprobar
            </Boton>
          </>
        }
        porCorrecto="Deshabilitada, con el motivo en el title. El usuario sabe que existe y a quién pedirla."
        incorrecto={
          <Boton tamano="sm" variante="primario">
            Aprobar
          </Boton>
        }
        porIncorrecto="Oculta. Nadie sabe si falta un permiso, si la etapa no lo permite o si el sistema está roto."
      />

      <Par
        n={6}
        regla="La serifa se queda en los títulos"
        correcto={
          <span>
            <span className="tw:block tw:font-serif tw:text-seccion tw:font-semibold tw:text-tinta-hi">
              Desglose del cálculo
            </span>
            <span className="tw:block tw:text-cuerpo-2 tw:text-tinta-mid">
              Zona 2 — costa norte · Categoría III
            </span>
          </span>
        }
        porCorrecto="Serifa en el título, sans en el dato. La jerarquía se lee sin subir el tamaño."
        incorrecto={
          <span className="tw:font-serif">
            <span className="tw:block tw:text-seccion tw:font-semibold tw:text-tinta-hi">
              Desglose del cálculo
            </span>
            <span className="tw:block tw:text-cuerpo-2 tw:text-tinta-mid">
              Zona 2 — costa norte · Categoría III
            </span>
          </span>
        }
        porIncorrecto="Serifa también en el cuerpo. En texto denso ralentiza el escaneo y todo pesa igual."
      />
    </div>
  );
}

/* ── Tono de voz ───────────────────────────────────────────────────────────── */

const TONO: readonly (readonly [string, string, string])[] = [
  ['Título de modal', 'Aprobar solicitud SOL-01293', '¿Está seguro?'],
  ['Botón', 'Aprobar y continuar', 'Aceptar'],
  ['Bajada de modal', 'Pasa a Revisión Presupuesto y se reserva la partida. La reserva se libera sola si la etapa se devuelve.', 'Esta acción modificará el estado del registro.'],
  ['Error de campo', 'Sin una fecha posterior al inicio no se puede calcular la duración de la gira.', 'El campo Fecha Fin es requerido.'],
  ['Error del servidor', 'No se pudo anular: la liquidación ya fue pagada.', 'Error 500. Contacte al administrador.'],
  ['Estado vacío', 'Ninguna solicitud espera tu revisión. La bandeja del área sigue teniendo 31 sin asignar.', 'No hay datos disponibles.'],
  ['Acción destructiva', 'Se anulan también 6 comprobantes y el informe del líder.', 'Esta acción no se puede deshacer.'],
  ['Confirmación', 'SOL-01293 pasó a Revisión Presupuesto. Se reservaron L. 49,990.65.', 'Operación exitosa.'],
  ['Persona', 'Segunda persona, voseo del país: «Corregí la zona y recalculá el monto.»', '«El usuario deberá proceder a corregir…»'],
  ['Números', '31 solicitudes · L. 2.14 M · vence en 1 día', 'Se encontraron treinta y un (31) registros.'],
];

export function TonoDeVoz(): ReactElement {
  return (
    <div className="tw:flex tw:flex-col tw:gap-3">
      <div className="tw:overflow-x-auto">
        <table className="tw:w-full tw:text-cuerpo-2">
          <thead>
            <tr className="tw:text-cabecera tw:font-semibold tw:tracking-[.08em] tw:text-tinta-mid tw:uppercase">
              <th className="loki-celda tw:py-2 tw:text-left">Dónde</th>
              <th className="loki-celda tw:py-2 tw:text-left">Así sí</th>
              <th className="loki-celda tw:py-2 tw:text-left">Así no</th>
            </tr>
          </thead>
          <tbody>
            {TONO.map(([donde, si, no]) => (
              <tr key={donde} className="tw:border-t tw:border-linea-suave tw:align-top">
                <td className="loki-celda tw:py-2 tw:text-tinta-mid">{donde}</td>
                <td className="loki-celda tw:py-2 tw:text-ok-fg">{si}</td>
                <td className="loki-celda tw:py-2 tw:text-tinta-low tw:line-through">{no}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      <p className="tw:text-cuerpo-2 tw:text-tinta-low">
        Institucional sin ser burocrático. El sistema informa a un funcionario que hace esto todos
        los días: <strong>ni lo trata de tonto ni lo hace descifrar</strong>.
      </p>
    </div>
  );
}

/* ── Reglas del contrato ───────────────────────────────────────────────────── */

const REGLAS: readonly (readonly [string, string, string])[] = [
  ['El color informa', 'Verde = está bien. Ámbar = espera algo. Nunca por variedad visual.', 'Un color decorativo enseña al usuario a ignorar el color, y entonces el semáforo deja de funcionar.'],
  ['El color nunca va solo', 'Cada pastilla lleva texto; cada punto lleva su nombre al lado.', 'Daltonismo, monitor mal calibrado, fotocopia en blanco y negro.'],
  ['Estado por id', 'ETAPA.PARALELO_6A, nunca comparar la etiqueta.', '«Por aprobar» contiene «aprob»: una comparación de cadenas la pintaría de aprobada.'],
  ['Poca sombra, mucho borde', 'Los paneles se separan por línea. Sólo lleva elevación lo que flota.', 'Tarjetas con sombra sobre gris es la firma de la plantilla que estamos dejando atrás.'],
  ['Foco dorado, no azul', '0 0 0 3px var(--focus-ring) + borde en el acento. Nunca outline:none sin sustituto.', 'El azul del navegador choca con el navy de marca y no se distingue del borde en reposo.'],
  ['Serifa sólo en títulos', 'Título de página y de sección. En tabla o botón, jamás.', 'En cuerpo denso la serifa rompe la lectura y ralentiza el escaneo.'],
  ['Importes en tres clases', 'font-mono tabular-nums text-right.', 'Sin las tres, los decimales no alinean y la columna deja de ser comparable.'],
  ['Densidad alta', 'Ocho horas al día, cientos de filas. El aire de una landing acá es un costo.', 'El oficial procesa 20+ solicitudes diarias; cada píxel de aire es una fila menos en pantalla.'],
  ['Contraste 4.5:1', 'Todo par texto-fondo, en los seis temas. Los bordes de campo ≥ 3:1 por WCAG 1.4.11.', 'Verificado por tema, no una vez: el mismo token cambia de contraste al cambiar de superficie.'],
  ['Una familia de iconos', 'Lucide, trazo 1.8, currentColor, 15–18px.', 'Mezclar familias de iconos es lo primero que delata un sistema sin dueño.'],
  ['El error dice la causa', '«Sin una fecha posterior al inicio no se puede calcular la duración», no «campo requerido».', 'Un mensaje genérico obliga al usuario a adivinar qué esperaba el sistema.'],
  ['Ningún componente lee el tema', 'Un TemaProvider escribe los atributos en la raíz. Un if (tema === "x") es el contrato roto.', 'Es lo que permite que el séptimo tema no toque una sola línea de componente.'],
];

export function ReglasDelContrato(): ReactElement {
  return (
    <div className="tw:overflow-x-auto">
      <table className="tw:w-full tw:text-cuerpo-2">
        <thead>
          <tr className="tw:text-cabecera tw:font-semibold tw:tracking-[.08em] tw:text-tinta-mid tw:uppercase">
            <th className="loki-celda tw:py-2 tw:text-left">Regla</th>
            <th className="loki-celda tw:py-2 tw:text-left">Qué significa</th>
            <th className="loki-celda tw:py-2 tw:text-left">Por qué existe</th>
          </tr>
        </thead>
        <tbody>
          {REGLAS.map(([regla, que, porque]) => (
            <tr key={regla} className="tw:border-t tw:border-linea-suave tw:align-top">
              <td className="loki-celda tw:py-2 tw:font-semibold tw:text-tinta-hi">{regla}</td>
              <td className="loki-celda tw:py-2 tw:text-tinta-base">{que}</td>
              <td className="loki-celda tw:py-2 tw:text-tinta-low">{porque}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

/* ── Versión ───────────────────────────────────────────────────────────────── */

export function VersionDelSistema(): ReactElement {
  return (
    <div className="tw:flex tw:flex-col tw:gap-2">
      {/* Dos versiones, no una. Hasta el 0.3.3 coincidían en el número y eso
          las hacía parecer la misma cosa: la entrega que ARGOS nos dio, y lo
          que este sistema de diseño implementa hoy. La primera está congelada;
          la segunda avanza con cada componente que agregamos. */}
      <div className="tw:flex tw:flex-wrap tw:items-center tw:gap-3">
        <Pastilla tono="ok">contrato v0.3.3</Pastilla>
        <span className="tw:text-ayuda tw:text-tinta-low">desciende de la entrega ARGOS</span>
        <Pastilla tono="neutro" punto={false}>
          0.3.2
        </Pastilla>
        <span className="tw:font-mono tw:text-ayuda tw:text-tinta-low">05/08/2026</span>
      </div>
      <p className="tw:text-cuerpo-2 tw:text-tinta-low">
        Un sistema sin bitácora es un sistema que nadie sabe si está al día. Qué trae cada versión
        de este contrato está en <code className="tw:font-mono tw:text-tinta-mid">CAMBIOS.md</code>;
        un número sin registro de qué cambió es solo un número.
      </p>
      <p className="tw:text-cuerpo-2 tw:text-tinta-low">
        La <strong>entrega de ARGOS</strong> es otra cosa y es <strong>inmutable</strong>: una
        versión nueva es una carpeta nueva, no un parche sobre ésta. Acá se materializa en{' '}
        <code className="tw:font-mono tw:text-tinta-mid">src/marca/argos/tokens.css</code>, que se
        copia tal cual y <strong>no se edita</strong> — lo que hay que definir para re-marcar está
        en <code className="tw:font-mono tw:text-tinta-mid">src/marca/CONTRATO.md</code>. Su número
        no sube cuando sube el nuestro: renumerarla sería afirmar que ARGOS entregó algo que nunca
        entregó.
      </p>
      <p className="tw:text-cuerpo-2 tw:text-tinta-low">
        Si un valor está mal, se pide una versión nueva y se reemplaza el archivo entero. Editarlo
        acá hace que la próxima entrega no se pueda diferenciar de lo que tenemos — que es el modo
        en que un sistema de diseño deja de ser una fuente y pasa a ser una copia con parches.
      </p>
    </div>
  );
}
