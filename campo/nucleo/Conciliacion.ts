import type { TransicionCapturada } from './DiarioLocal.ts';

/** Una captura, con el dispositivo que la produjo. Sin el origen no se puede arbitrar. */
export type CapturaConOrigen = TransicionCapturada & { readonly idDispositivo: string };

/**
 * Dos o más versiones del mismo hecho que no coinciden.
 *
 * <b>Ninguna gana automáticamente.</b> `RN-45`: ambas se conservan, el conflicto entra
 * a una **cola de resolución humana**, y resolverlo es un acto identificado y registrado
 * con motivo — que además **conserva la versión descartada**.
 */
export interface ConflictoDeSincronizacion {
  readonly idTransicion: string;
  readonly idExpediente: string;
  readonly transicion: string;
  readonly versiones: readonly CapturaConOrigen[];
}

export interface ResultadoDeConciliacion {
  /** Lo que entra sin discusión: un solo origen, o varios que dicen lo mismo. */
  readonly aceptadas: readonly CapturaConOrigen[];
  /** Lo que **nadie resuelve automáticamente**. */
  readonly conflictos: readonly ConflictoDeSincronizacion[];
}

/**
 * Concilia lo que traen dos o más dispositivos sobre los mismos expedientes.
 *
 * ── La regla, en una línea ───────────────────────────────────────────────────
 * **Cero sobrescritura silenciosa** (`RN-45`). No hay «gana el más reciente», no hay
 * «gana el servidor», no hay «gana el motorista».
 *
 * ── Por qué no hay una regla automática, aunque la habría ────────────────────
 * *«Gana la marca de tiempo más reciente»* es la respuesta tentadora y es la peor: el
 * reloj del dispositivo se puede alterar —deliberadamente o no— y el hecho que se
 * capturó después no es el que ocurrió después. En este dominio lo que está en
 * conflicto son **odómetros, galones y montos**, y una sobrescritura automática
 * destruye el término de una conciliación de auditoría sin que nadie se entere.
 *
 * ── Lo que sí se resuelve solo, y por qué es seguro ──────────────────────────
 * Dos capturas **idénticas** del mismo hecho no son conflicto: son un reenvío. Si
 * entraran a la cola humana, la cola se llenaría de ruido, en dos semanas nadie la
 * miraría, y el conflicto de verdad pasaría de largo.
 */
export function conciliar(
  ...lotes: readonly (readonly CapturaConOrigen[])[]
): ResultadoDeConciliacion {
  // El hecho es la identidad. Dos dispositivos que capturaron el mismo acontecimiento
  // traen el mismo `idTransicion` (`ADR-005`), y por eso se encuentran acá.
  const porHecho = new Map<string, CapturaConOrigen[]>();

  for (const lote of lotes) {
    for (const captura of lote) {
      const versiones = porHecho.get(captura.idTransicion) ?? [];
      versiones.push(captura);
      porHecho.set(captura.idTransicion, versiones);
    }
  }

  const aceptadas: CapturaConOrigen[] = [];
  const conflictos: ConflictoDeSincronizacion[] = [];

  for (const versiones of porHecho.values()) {
    const primera = versiones[0]!;

    if (versiones.every((v) => mismosDatos(v, primera))) {
      // Un solo origen, o varios que dicen exactamente lo mismo. Es un reenvío.
      aceptadas.push(primera);
      continue;
    }

    conflictos.push({
      idTransicion: primera.idTransicion,
      idExpediente: primera.idExpediente,
      transicion: primera.transicion,
      versiones,
    });
  }

  return { aceptadas, conflictos };
}

/**
 * Si dos capturas dicen lo mismo.
 *
 * Compara **el contenido del hecho**, no el envoltorio: el dispositivo que lo capturó y
 * el momento en que lo hizo son distintos por construcción y no son la discrepancia.
 * Lo que importa es si el odómetro coincide.
 */
function mismosDatos(a: CapturaConOrigen, b: CapturaConOrigen): boolean {
  const clavesA = Object.keys(a.datos).sort();
  const clavesB = Object.keys(b.datos).sort();

  if (clavesA.length !== clavesB.length) return false;
  if (clavesA.some((clave, i) => clave !== clavesB[i])) return false;

  return clavesA.every((clave) => Object.is(a.datos[clave], b.datos[clave]));
}
