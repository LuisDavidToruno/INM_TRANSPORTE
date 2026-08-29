import type { ReactElement } from 'react';
import { useParams } from 'react-router';
import { CircleAlert, FileWarning, Hammer, Smartphone } from 'lucide-react';

import { Boton, EnlaceBoton, Nota, Panel, Pastilla, Vacio } from '../ui';
import type { Tono } from '../ui';
import { buscar } from './registro';
import type { PantallaConSituacion } from './registro';
import type { SituacionDePantalla } from './tipos';

/**
 * La pantalla que todavía no existe.
 *
 * ── Por qué no dice «próximamente» ──────────────────────────────────────────
 * Un «en construcción» genérico obliga a quien lo encuentra a ir a preguntar. Acá se dice **qué
 * pantalla es, qué la origina y por qué no está**, que son las tres preguntas que se hacen. Una
 * bloqueada por el insumo #2 y una que sencillamente no se empezó necesitan cosas distintas de
 * personas distintas, y verlas iguales las mezcla en la misma cola.
 *
 * ── Y muestra su trazabilidad ───────────────────────────────────────────────
 * `CU`, `HU` y roles vienen del inventario. No es decoración: es lo que permite que quien la
 * vaya a construir sepa contra qué historia se verifica sin volver al documento.
 */
export default function EnDesarrollo(): ReactElement {
  const { id } = useParams<{ id: string }>();
  const pantalla = id === undefined ? null : buscar(id);

  if (pantalla === null) {
    return (
      <Vacio
        icono={<CircleAlert />}
        titulo={id === undefined ? 'No se indicó ninguna pantalla' : `No existe ${id.toUpperCase()}`}
        descripcion="Los identificadores van de PT-001 a PT-138, y no se reciclan. Puede que el enlace sea de una versión anterior."
      />
    );
  }

  // Una pantalla construida no debería llegar acá: la ruta salta a la real. Si llega —porque
  // alguien tecleó la dirección— se dice, en vez de fingir que está en desarrollo.
  return (
    <div className="tw:flex tw:flex-col tw:gap-5">
      <header className="tw:flex tw:flex-col tw:gap-2">
        <div className="tw:flex tw:flex-wrap tw:items-center tw:gap-2">
          <span className="tw:font-mono tw:text-sm tw:text-tinta-mid">{pantalla.id}</span>
          <Pastilla tono={TONO[pantalla.situacion]}>{ETIQUETA[pantalla.situacion]}</Pastilla>
        </div>

        <h1 className="tw:text-xl tw:font-semibold tw:tracking-tight">{pantalla.nombre}</h1>
        <p className="tw:text-sm tw:text-tinta-mid">{pantalla.seccion}</p>
      </header>

      <PorQueNoEsta pantalla={pantalla} />

      <Panel titulo="De dónde sale">
        <dl className="tw:grid tw:gap-x-6 tw:gap-y-3 tw:sm:grid-cols-2">
          <Dato termino="Quién la usa" valor={pantalla.roles} />
          <Dato termino="Cliente" valor={CLIENTE[pantalla.cliente] ?? pantalla.cliente} />
          <Dato termino="Casos de uso" valor={pantalla.cu} />
          <Dato termino="Historias" valor={pantalla.hu} />
          <Dato termino="Funciona sin red" valor={SIN_RED[pantalla.sinRed] ?? pantalla.sinRed} />
          <Dato termino="Replica un formato en papel" valor={PAPEL[pantalla.papel] ?? pantalla.papel} />
        </dl>
      </Panel>

      <div className="tw:flex tw:flex-wrap tw:gap-2">
        <EnlaceBoton variante="secundario" href="/pantallas">
          Ver las 138 pantallas
        </EnlaceBoton>

        {pantalla.ruta !== null && (
          <EnlaceBoton variante="primario" href={pantalla.ruta}>
            Abrir la pantalla
          </EnlaceBoton>
        )}
      </div>
    </div>
  );
}

/**
 * El motivo, que es lo único que hace útil a esta pantalla.
 *
 * Cada situación pide algo de alguien distinto, y por eso se dice **quién destraba qué** en vez
 * de dejar un estado sin salida.
 */
function PorQueNoEsta({ pantalla }: { pantalla: PantallaConSituacion }): ReactElement {
  if (pantalla.situacion === 'bloqueada') {
    return (
      <Nota tono="riesgo" icono={<FileWarning />}>
        <b>No se dibuja hasta tener el formato en papel.</b> Esta pantalla replica un formato que
        la institución todavía no entregó — es el <b>insumo #2</b>, y son 29 pantallas en la
        misma situación. El inventario es explícito:{' '}
        <i>«dibujarlas antes es garantizar que hay que rehacerlas»</i>. Lo que la destraba no es
        tiempo de programación: es que llegue el formato.
      </Nota>
    );
  }

  if (pantalla.situacion === 'campo') {
    return (
      <Nota tono="aviso" icono={<Smartphone />}>
        <b>Es una superficie del cliente de campo</b>, que todavía no tiene ninguna interfaz:{' '}
        <code className="tw:font-mono tw:text-xs">campo/</code> hoy es sólo núcleo —bitácora
        local, cola de adjuntos, folios, conciliación— con sus pruebas. Son <b>34 superficies</b>{' '}
        contando las nueve duales, y ninguna se construye desde la oficina.
      </Nota>
    );
  }

  if (pantalla.situacion === 'parcial') {
    return (
      <Nota tono="aviso" icono={<Hammer />}>
        <b>Está construida a medias.</b> {pantalla.incompleta}
      </Nota>
    );
  }

  if (pantalla.situacion === 'construida') {
    return (
      <Nota tono="ok">
        Esta pantalla <b>sí existe</b>. Llegó acá por la dirección directa; ábrala en{' '}
        <code className="tw:font-mono tw:text-xs">{pantalla.ruta}</code>.
      </Nota>
    );
  }

  return (
    <Nota tono="info" icono={<Hammer />}>
      <b>En desarrollo.</b> No está empezada, y <b>nada la bloquea</b>: el inventario la cuenta
      entre las que se pueden construir desde el primer día. Lo que falta es escribirla.
      {pantalla.papel === 'Parc.' && (
        <>
          {' '}
          Una de sus secciones sí replica papel: esa parte queda como marco vacío hasta que
          llegue el formato.
        </>
      )}
    </Nota>
  );
}

function Dato({ termino, valor }: { termino: string; valor: string }): ReactElement {
  return (
    <div className="tw:flex tw:flex-col tw:gap-0.5">
      <dt className="tw:text-xs tw:uppercase tw:tracking-wide tw:text-tinta-mid">{termino}</dt>
      <dd className="tw:text-sm">{valor === '—' ? <SinTrazar /> : valor}</dd>
    </div>
  );
}

/** El inventario dejó filas sin trazar, y eso se dice: un guion suelto se lee como «ninguno». */
function SinTrazar(): ReactElement {
  return <span className="tw:italic tw:text-tinta-mid">el inventario no lo trazó</span>;
}

export const ETIQUETA: Record<SituacionDePantalla, string> = {
  construida: 'Construida',
  parcial: 'A medias',
  pendiente: 'En desarrollo',
  bloqueada: 'Falta el formato',
  campo: 'Cliente de campo',
};

export const TONO: Record<SituacionDePantalla, Tono> = {
  construida: 'ok',
  parcial: 'aviso',
  pendiente: 'info',
  bloqueada: 'riesgo',
  campo: 'neutro',
};

const CLIENTE: Record<string, string> = {
  A: 'Administrativo — la oficina',
  C: 'De campo — sin interfaz todavía',
  'A/C': 'Dual: administrativo y de campo',
  P: 'Pública, sin sesión',
};

const PAPEL: Record<string, string> = {
  No: 'No — se puede construir ya',
  Sí: 'Sí — bloqueada por el insumo #2',
  'Parc.': 'Una sección sí',
};

const SIN_RED: Record<string, string> = {
  Sí: 'Sí, totalmente desconectada',
  No: 'No, exige conexión',
  'Deg.': 'Degradada, declarando qué no puede verificar',
};

/** Sin uso fuera de este archivo; existe para que el barril no crezca sin motivo. */
export type BotonDeDesarrollo = typeof Boton;
