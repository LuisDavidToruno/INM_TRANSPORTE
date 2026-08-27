# RN-51 — En el traslado de personas externas solo se capturan los datos mínimos del catálogo autorizado

| Campo | Valor |
|---|---|
| **Módulos** | M-17, M-06, M-14 |
| **Origen** | Norma [NRM-07](../normativa/NRM-07-transparencia-y-datos-personales.md); decisión [DP-001 D-14](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) |
| **Verificación** | `[V]` que no hay ley general de datos personales vigente — `[V]` que el hábeas data constitucional sí está vigente — `[C]` qué traslados de personas externas realiza la institución |
| **Tipo** | Bloqueo duro |
| **Configurable** | Sí — catálogo `campo_manifiesto_persona_externa`, con conjunto mínimo no ampliable sin fundamento |

## Enunciado

En el manifiesto de traslado de personas externas, el sistema **debe** capturar únicamente los campos del catálogo autorizado: identificación de la persona, institución o condición que motiva el traslado, origen y destino.

El sistema **no debe** ofrecer campos de **salud, etnia, situación migratoria o condición de vulnerabilidad**, salvo que exista **base legal expresa y necesidad operativa documentada** registrada como fundamento del campo.

Los datos personales **deben** estar estructuralmente separados de los datos de gestión pública — vehículo, ruta, costo, unidad ejecutora, objeto del viaje — de modo que estos últimos puedan exportarse sin aquellos.

## Justificación

[NRM-07](../normativa/NRM-07-transparencia-y-datos-personales.md) exige aplicar **minimización de datos en M-17**: capturar solo lo necesario para el control, y *"evitar campos de salud, etnia, situación migratoria o condición de vulnerabilidad salvo que exista base legal expresa y necesidad operativa documentada"*.

No hay ley general de protección de datos personales vigente en Honduras `[V]` — y [DP-001 D-14](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) decidió **no diseñar para anticiparla**. Pero sí está vigente el **hábeas data del Artículo 182 constitucional** `[V]`, y que el MARCI exija además control de acceso y registro de consultas está `[C]` — [NRM-01](../normativa/NRM-01-control-interno-tsc.md) tiene esa familia por confirmar. Ver [RN-52](RN-52-registro-de-consultas-a-manifiestos.md) y el hallazgo `HN1-14`.

El razonamiento práctico es más simple que el jurídico: **un dato que no se captura no se puede filtrar, no se puede publicar por error y no se puede pedir por hábeas data**. La minimización es la medida de protección más barata y más efectiva.

La separación estructural, además, es lo que permite publicar en el Portal Único de Transparencia **sin depuración manual** ([NRM-07](../normativa/NRM-07-transparencia-y-datos-personales.md)).

## Condiciones de aplicación

Aplica a todo traslado que incluya personas ajenas a la institución.

**Aplica también, con el mismo criterio, a dos supuestos que no son traslado:**

- **Terceros involucrados en un siniestro** ([RN-74](RN-74-sin-atribucion-de-responsabilidad-en-campo.md)): solo los datos del catálogo autorizado —identificación, contacto, vehículo, aseguradora— **sin diagnóstico médico ni dato clínico**, y con registro de toda consulta posterior ([RN-52](RN-52-registro-de-consultas-a-manifiestos.md)).
- **Dato de salud del servidor** que se incapacita en ruta ([RN-70](RN-70-interrupcion-en-ruta-con-desenlace-obligatorio.md)): SIGTI registra **la existencia de la incapacidad y su efecto operativo**; nunca diagnóstico ni dato clínico. El expediente de salud pertenece a Talento Humano ([DP-001 D-07](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md)).

**No aplica** al personal de la institución en cuanto a sus datos de expediente, que ya viven en Talento Humano y se referencian por espejo ([RN-48](RN-48-datos-espejo-de-solo-lectura.md)) — no se duplican en el manifiesto.

`[C]` La institución debe declarar qué traslados de personas externas realiza y con qué fundamento. **Sin esa declaración, el catálogo de campos queda en el mínimo.**

## Comportamiento esperado

1. El catálogo de campos del manifiesto es configurable, pero **agregar un campo sensible exige registrar base legal y necesidad operativa**, con autor y fecha. El fundamento es visible para ACT-12 Auditor Interno.
2. Los datos personales se almacenan cifrados en reposo, y toda comunicación va cifrada en tránsito, incluida la de las delegaciones ([NRM-07](../normativa/NRM-07-transparencia-y-datos-personales.md)).
3. Los reportes públicos y de transparencia se generan desde la vista de gestión pública, **sin acceso técnico** a los campos personales — no por filtrado en el reporte, sino por separación de origen.
4. Las políticas de retención son diferenciadas: los datos financieros y de bienes se conservan por el plazo de fiscalización; los datos personales de pasajeros tienen plazo de depuración o seudonimización más corto. `[C]` los plazos con Auditoría Interna y el OIP.
5. El ejercicio del hábeas data se soporta: buscar todos los registros de una persona, exportarlos y rectificarlos **dejando traza de la rectificación sin destruir el registro contable original** ([RN-04](RN-04-anulacion-como-asiento-reverso.md)).

## Casos límite

- **Traslado que operativamente exige un dato de salud** — persona que requiere ambulancia o asistencia. El campo no se agrega al manifiesto general: se registra como **requerimiento operativo del traslado** (necesita camilla, requiere acompañante) sin consignar diagnóstico. La necesidad se satisface sin capturar el dato sensible.
- **Persona sin documento de identidad.** Frecuente. El manifiesto debe admitir identificación alternativa o registro como *no identificada* con descripción mínima. Exigir un número de identidad bloquearía traslados legítimos.
- **Menores de edad.** `[C]` confirmar si la institución los traslada y bajo qué régimen. Hasta entonces, no se diseñan campos específicos.
- **Lista de pasajeros impresa que el motorista porta.** El papel sale del control técnico del sistema. El formato impreso debe llevar los datos **mínimos indispensables** para el control en carretera, no el manifiesto completo, y el documento tiene folio y verificación ([RN-25](RN-25-salvoconducto-con-folio-y-qr.md)).
- **Solicitud de información pública sobre un traslado.** Se responde con la vista de gestión pública. Si el solicitante pide los nombres, la decisión es del Oficial de Información Pública, no del sistema — pero el sistema debe poder entregar ambas versiones por separado y registrar qué se entregó.
- **Campo agregado sin fundamento por un administrador.** El sistema lo permite técnicamente pero lo marca como **campo sin fundamento registrado** y lo reporta a auditoría. Impedirlo por completo llevaría a que el dato se capture en un campo de observaciones, que es peor.
- **Datos ya capturados de más antes de aplicar la regla.** No se borran físicamente ([RN-04](RN-04-anulacion-como-asiento-reverso.md)): se seudonimizan según la política de retención, dejando el registro operativo intacto.

## Trazabilidad

- Norma: [NRM-07 — Transparencia y datos personales](../normativa/NRM-07-transparencia-y-datos-personales.md)
- Decisión: [DP-001, D-14](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md)
- Reglas relacionadas: [RN-52](RN-52-registro-de-consultas-a-manifiestos.md), [RN-53](RN-53-cierre-del-manifiesto-al-despacho.md), [RN-20](RN-20-compatibilidad-vehiculo-objeto-del-traslado.md), [RN-04](RN-04-anulacion-como-asiento-reverso.md), [RN-67](RN-67-matriz-de-compatibilidad-objeto-objeto.md), [RN-70](RN-70-interrupcion-en-ruta-con-desenlace-obligatorio.md), [RN-74](RN-74-sin-atribucion-de-responsabilidad-en-campo.md)
- Actores: ACT-01, ACT-02, ACT-05, ACT-12
- Casos especiales: [CE-03](../../02-requisitos/casos-especiales/CE-03-accidente-de-transito-en-mision.md), [CE-10](../../02-requisitos/casos-especiales/CE-10-motorista-incapacitado-en-ruta.md), [CE-18](../../02-requisitos/casos-especiales/CE-18-carga-y-pasajeros-en-la-misma-mision.md)
- Historias: [HU-111](../../02-requisitos/historias/HU-111-registrar-manifiesto-de-personas-externas.md) manifiesto y catálogo mínimo · [HU-112](../../02-requisitos/historias/HU-112-fundamentar-campo-sensible-del-manifiesto.md) campo sensible con base legal · [HU-113](../../02-requisitos/historias/HU-113-persona-sin-documento-de-identidad.md) persona sin documento · [HU-123](../../02-requisitos/historias/HU-123-exportar-transparencia-sin-datos-personales.md) exportación sin datos personales · [HU-124](../../02-requisitos/historias/HU-124-depurar-datos-personales-sin-romper-la-cadena.md) depuración sin romper la cadena
