import type { ReactElement } from 'react';

import LineaDeCarriles from '../ui/LineaDeCarriles';
import type { CarrilDeLinea } from '../ui/LineaDeCarriles';

/**
 * Las tres muestras de la línea por carriles.
 *
 * No son tres componentes: es **el mismo** con datos de tres dominios. Ponerlas juntas
 * es lo que sostiene la afirmación de que una sola primitiva sirve los tres usos — si
 * alguno necesitara un caso especial dentro del componente, se vería acá.
 *
 * ⚠️ Las fechas se fijan a un lunes concreto y **no salen del reloj**. Una vitrina que
 * usa `new Date()` cambia de dibujo cada día, y entonces «se ve bien» deja de ser
 * comprobable: el fin de semana se corre, las barras se recortan distinto y nadie sabe
 * si lo que cambió fue el componente o el calendario.
 */

/** Lunes 31 de agosto de 2026. Elegido para que la ventana caiga sobre un fin de semana. */
const LUNES = new Date(2026, 7, 31);
const dia = (n: number): Date => new Date(2026, 7, 31 + n);

/** Ocupación de flota — `PT-026`. Un carril por vehículo, una barra por misión. */
const FLOTA: readonly CarrilDeLinea[] = [
  {
    id: 'v1',
    titulo: 'INM-PU-014',
    detalle: 'Pick-up · HAB 4521',
    barras: [
      // Entra recortada: empezó el sábado anterior. Sin la marca de corte se leería
      // como que el vehículo estuvo libre hasta el lunes.
      {
        id: 'm1',
        titulo: 'OM-000112',
        desde: dia(-2),
        hasta: dia(1),
        detalle: 'Choluteca',
        tono: 'info',
      },
      { id: 'm2', titulo: 'OM-000119', desde: dia(4), hasta: dia(5), detalle: 'Danlí' },
    ],
  },
  {
    id: 'v2',
    titulo: 'INM-CA-003',
    detalle: 'Camión · sin placa metálica',
    barras: [
      { id: 'm3', titulo: 'OM-000117', desde: dia(2), hasta: dia(2), detalle: 'Tegucigalpa' },
      // Sale de la ventana: sigue ocupado la semana siguiente.
      { id: 'm4', titulo: 'OM-000121', desde: dia(6), hasta: dia(11), detalle: 'San Pedro Sula' },
    ],
  },
  {
    id: 'v3',
    titulo: 'INM-MO-021',
    detalle: 'Motocicleta · en taller',
    inhabilitado: true,
    barras: [
      // Un bloqueo de taller no es una misión: lo dice él, no el dibujo.
      {
        id: 'm5',
        titulo: 'Correctivo',
        desde: dia(0),
        hasta: dia(8),
        tono: 'riesgo',
        queEs: 'orden de trabajo',
      },
    ],
  },
  { id: 'v4', titulo: 'INM-BU-002', detalle: 'Bus · HAB 1180', barras: [] },
];

/** Vigencias — `PT-019`, `PT-078`. Un carril por documento, la barra es el rango vigente. */
const VIGENCIAS: readonly CarrilDeLinea[] = [
  {
    id: 'd1',
    titulo: 'Matrícula',
    detalle: 'INM-PU-014',
    barras: [{ id: 'g1', titulo: 'Vigente', desde: dia(-30), hasta: dia(9), tono: 'ok' }],
  },
  {
    id: 'd2',
    titulo: 'Seguro',
    detalle: 'Póliza 88-4412',
    barras: [{ id: 'g2', titulo: 'Vence el 3', desde: dia(-30), hasta: dia(3), tono: 'aviso' }],
  },
  {
    id: 'd3',
    titulo: 'Licencia B',
    detalle: 'R. Discua',
    // Ya vencida: la barra termina antes de que abra la ventana y no se dibuja nada.
    // Un carril vacío por vencimiento y uno vacío por falta de dato se ven igual, y
    // por eso la pantalla que use esto tiene que decirlo aparte — el componente no.
    barras: [{ id: 'g3', titulo: 'Vencida', desde: dia(-40), hasta: dia(-3), tono: 'riesgo' }],
  },
];

export function MuestraCarrilesDeFlota(): ReactElement {
  return (
    <div className="tw:flex tw:flex-col tw:gap-6">
      <Muestra
        titulo="Ocupación de flota"
        nota="Un carril por vehículo, una barra por misión. El fin de semana va sombreado y la línea vertical es hoy. `‹` y `›` marcan lo que se sale de la ventana."
      >
        <LineaDeCarriles
          carriles={FLOTA}
          desde={LUNES}
          hasta={dia(6)}
          queEsUnaBarra="misión"
          referencia={{ fecha: dia(2), titulo: 'Hoy' }}
        />
      </Muestra>

      <Muestra
        titulo="Vigencias de documentos"
        nota="El mismo componente. Cambia lo que es un carril —el documento— y lo que es una barra —el rango vigente."
      >
        <LineaDeCarriles
          carriles={VIGENCIAS}
          desde={LUNES}
          hasta={dia(6)}
          queEsUnaBarra="vigencia"
          referencia={{ fecha: dia(2), titulo: 'Hoy' }}
        />
      </Muestra>

      <Muestra
        titulo="Sin carriles"
        nota="El vacío se recibe. Un dibujo en blanco no distingue «no hay flota» de «no cargó»."
      >
        <LineaDeCarriles
          carriles={[]}
          desde={LUNES}
          hasta={dia(6)}
          vacio="Ningún vehículo de la delegación tiene tipo compatible con lo que se pide mover."
        />
      </Muestra>
    </div>
  );
}

function Muestra({
  titulo,
  nota,
  children,
}: {
  titulo: string;
  nota: string;
  children: ReactElement;
}): ReactElement {
  return (
    <div className="tw:flex tw:flex-col tw:gap-2">
      <div>
        <h4 className="tw:text-cuerpo tw:font-medium tw:text-tinta-hi">{titulo}</h4>
        <p className="tw:max-w-3xl tw:text-cuerpo-2 tw:text-tinta-mid">{nota}</p>
      </div>
      <div className="tw:rounded tw:border tw:border-linea tw:bg-panel tw:p-3">{children}</div>
    </div>
  );
}
