# RN-21 — No se excede la capacidad de pasajeros ni la capacidad de carga declarada en la ficha técnica

| Campo | Valor |
|---|---|
| **Módulos** | M-07, M-06, M-03, M-17 |
| **Origen** | Norma [NRM-02](../normativa/NRM-02-bienes-del-estado.md) — ficha maestra con capacidad; [NRM-06](../normativa/NRM-06-transito-y-licencias.md) — peso bruto vehicular |
| **Verificación** | `[I]` la exigencia de registrar capacidad de pasajeros y carga en kg y m³ — es **implicación de requerimiento** escrita por el equipo en [NRM-02](../normativa/NRM-02-bienes-del-estado.md), no articulado citable. Corregido desde `[V]` por la regla de no escalar el nivel (`CLAUDE.md`, hallazgo `HN1-03`) |
| **Tipo** | Bloqueo duro |
| **Configurable** | No el bloqueo. Sí el `margen_tolerancia_carga`, con valor inicial cero |

## Enunciado

El sistema **no debe** permitir programar ni despachar una misión en la que:

- el número de personas a bordo, **incluido el motorista**, supere la capacidad de pasajeros de la ficha técnica; o
- el peso declarado de la carga supere la capacidad de carga en kilogramos; o
- el volumen declarado supere la capacidad en metros cúbicos, cuando ambos datos existan.

En misiones mixtas de personas y carga, la evaluación **debe** considerar ambas simultáneamente y, cuando la ficha lo permita, el **peso bruto vehicular** como límite superior conjunto.

## Justificación

[NRM-02](../normativa/NRM-02-bienes-del-estado.md) exige que la ficha maestra registre *capacidad (pasajeros / carga en kg y m³)*. El dato existe para decidir, no para archivarse.

El exceso de capacidad es simultáneamente un riesgo de vida, una infracción de tránsito y una fuente de daño al bien del Estado — sobrecarga que destruye suspensión y llantas. Los tres desembocan en responsabilidad de quien autorizó.

Además, el peso bruto es el mismo atributo que gobierna [RN-09](RN-09-matriz-licencia-vehiculo.md) y [RN-33](RN-33-categoria-de-peaje-derivada-de-ficha-tecnica.md): un dato mal cargado desalinea tres reglas a la vez.

## Condiciones de aplicación

Aplica a la programación, al despacho y a toda modificación del manifiesto antes de la salida.

**No aplica** durante la ejecución como bloqueo: un vehículo en ruta que recoge un pasajero adicional no se puede "bloquear". Ese hecho se registra como novedad y produce hallazgo en la liquidación ([RN-53](RN-53-cierre-del-manifiesto-al-despacho.md)).

## Comportamiento esperado

1. La solicitud declara número de personas y, si hay carga, peso y volumen estimados. Sin esos datos, la programación se bloquea: no se puede evaluar lo que no se declara.
2. El bloqueo es cuantitativo y explícito: *"El vehículo <correlativo> tiene capacidad para 5 pasajeros incluido el motorista. La misión declara 7."*
3. El sistema propone vehículos con capacidad suficiente, o la **división en dos órdenes de misión**, que es la salida operativa real.
4. Si a la ficha técnica le falta la capacidad, el sistema bloquea e indica el dato faltante. **Nunca asume** una capacidad por marca o modelo.
5. **El peso y la ocupación efectivos se capturan al despachar** y se comparan contra lo declarado; **la desviación se acumula como indicador por dependencia solicitante** ([RN-82](RN-82-indicadores-de-calidad-de-la-programacion.md)), además de alimentar el reporte de liquidación. Una dependencia que declara sistemáticamente por debajo de lo que embarca es un dato de gestión, no un error de captura.
6. Cuando dos objetos legítimos compiten por el mismo vehículo, **la reducción se aplica primero al objeto no principal** declarado en la solicitud. **En ningún caso la configuración se resuelve trasladando personas fuera de plazas homologadas** ([RN-67](RN-67-matriz-de-compatibilidad-objeto-objeto.md)).

## Casos límite

- **Peso de la carga desconocido al solicitar.** Frecuente: "unas cajas de expedientes". Se admite estimación por rango del catálogo de tipos de carga, marcada como estimada, y se exige peso al despachar si el tipo lo requiere. `[C]` levantar con el Jefe de Transporte qué tipos de carga exigen peso cierto.
- **Niños o personas trasladadas en brazos.** Cuentan como personas a bordo. `[C]` confirmar si la institución traslada menores y bajo qué régimen (M-17).
- **Capacidad de pasajeros de un pickup con paila.** La paila **no es capacidad de pasajeros**. La ficha registra únicamente las plazas homologadas en cabina. Si alguien pretende trasladar personas en la paila, la regla debe bloquear — y ese es precisamente el caso que justifica no dejar el dato al criterio del capturador.
- **Vehículo con remolque.** El remolque suma capacidad de carga y cambia el peso bruto. Se evalúa la configuración declarada de la misión, no el vehículo en vacío — igual que en [RN-09](RN-09-matriz-licencia-vehiculo.md).
- **Emergencia con evacuación de personas.** No hay excepción configurable de capacidad. Si la institución necesita una, debe ser una decisión de la máxima autoridad registrada como tal, y hasta hoy `[C]` no consta que exista. La salida prevista es dividir en varias misiones.
- **Margen de tolerancia.** El parámetro existe porque una institución puede querer admitir un 5% de holgura sobre el peso estimado. Su valor inicial es **cero**, y activarlo es un acto registrado con fundamento. Un margen invisible es la forma de que la regla deje de significar algo.
- **Carga que se entrega en el primer destino y libera capacidad.** La evaluación se hace por **tramo**, no por misión completa, cuando la misión declara multi-destino. Evaluar solo el peor tramo bloquearía misiones perfectamente ejecutables.
- **Carga que se recoge en un destino intermedio** y consume capacidad. Es el reverso del caso anterior y esta regla, por sí sola, no lo veía: lo cubre [RN-68](RN-68-compatibilidad-y-capacidad-por-tramo.md), que evalúa cada tramo sobre su configuración efectiva, incluida la que se incorpora en ruta.

## Trazabilidad

- Normas: [NRM-02](../normativa/NRM-02-bienes-del-estado.md), [NRM-06](../normativa/NRM-06-transito-y-licencias.md)
- Reglas relacionadas: [RN-20](RN-20-compatibilidad-vehiculo-objeto-del-traslado.md), [RN-09](RN-09-matriz-licencia-vehiculo.md), [RN-33](RN-33-categoria-de-peaje-derivada-de-ficha-tecnica.md), [RN-53](RN-53-cierre-del-manifiesto-al-despacho.md), [RN-67](RN-67-matriz-de-compatibilidad-objeto-objeto.md), [RN-68](RN-68-compatibilidad-y-capacidad-por-tramo.md), [RN-69](RN-69-inventario-de-la-carga-y-acta-de-entrega.md)
- Actores: ACT-02, ACT-04, ACT-05, ACT-06
- Casos especiales: [CE-18](../../02-requisitos/casos-especiales/CE-18-carga-y-pasajeros-en-la-misma-mision.md)
- Historias: pendientes — Bloque 4
