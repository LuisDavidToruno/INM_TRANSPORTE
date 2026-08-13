# RN-77 — Cada extensión produce una versión del alcance autorizado, y toda validación posterior se hace contra la versión vigente a la fecha del hecho

| Campo | Valor |
|---|---|
| **Módulos** | M-07, M-08, M-13, M-18, M-14 |
| **Origen** | Caso especial [CE-06](../../02-requisitos/casos-especiales/CE-06-la-mision-se-extiende-mas-dias-destinos-o-kilometros.md) · Norma [NRM-01](../normativa/NRM-01-control-interno-tsc.md) |
| **Verificación** | `[P]` la exigencia de autorización previa y de trazabilidad de las modificaciones — [NRM-01](../normativa/NRM-01-control-interno-tsc.md). `[I]` el versionado del alcance: implicación de requerimiento del equipo |
| **Tipo** | Bloqueo duro + derivación |
| **Configurable** | Sí — umbral de escalamiento por magnitud de la extensión `[C]` |

## Enunciado

Cada extensión de una Orden de Misión —más días, más destinos, más kilómetros, mayor costo estimado— **debe** producir una **versión del alcance autorizado**, con: ventana, destinos, ruta, estimado, **autorizador** y **vigencia de la versión**.

**Toda validación posterior se hace contra la versión vigente a la fecha del hecho** que se está validando. La coherencia de casetas, el kilometraje y la ruta se evalúan contra el alcance vigente al momento de cada hecho: **un paso amparado por una extensión autorizada no es hallazgo**.

Todo **destino agregado en ruta** identifica la **dependencia requirente**, el **objeto que se agrega** y su **autorizador**, que **no puede ser quien lo pidió** ([`RN-01`](RN-01-segregacion-de-funciones.md)).

## Justificación

Hoy la Orden de Misión tiene un alcance, y cuando se prorroga, ese alcance se modifica. Si se modifica en su lugar, se pierden dos cosas: **contra qué se autorizó originalmente**, y **contra qué se debía evaluar cada hecho**.

El efecto práctico aparece en la conciliación. Una misión aprobada para Tegucigalpa–Danlí que se extiende a Trojes va a pasar por casetas que la ruta original no contemplaba y va a recorrer 180 km más. Sin versionado, [`RN-37`](RN-37-coherencia-de-la-secuencia-de-casetas.md) marca cada paso como incoherente y [`RN-30`](RN-30-conciliacion-galonaje-kilometraje.md) marca una desviación grave — **todo falso, y todo consumiendo el tiempo de quien lo investiga**.

Con versionado, el sistema sabe que el paso por la caseta ocurrió el día 3, que la versión vigente el día 3 incluía Trojes, y que por tanto no hay nada que investigar.

## Condiciones de aplicación

Aplica a toda extensión, ocurra antes de la salida o con la misión `EN_RUTA`.

Aplica al **retorno anticipado**, que también cambia el alcance: recorta la ventana efectiva y libera recursos ([`RN-79`](RN-79-el-retorno-constatado-libera-al-vehiculo.md)).

**No aplica** a la corrección de un dato mal capturado del alcance original, que es asiento de corrección ([`RN-04`](RN-04-anulacion-como-asiento-reverso.md)), no una versión nueva.

## Comportamiento esperado

1. Cada versión conserva la anterior íntegra. La consulta del expediente muestra la **secuencia completa de alcances** con su autorizador y su momento.
2. Toda prórroga que mueva el fin de la ventana **revalida licencia y documentación del vehículo contra la nueva fecha de fin** y bloquea si fallan. La [máquina de estados](../../03-arquitectura/estados/orden-de-mision.md) ya lo exige en `T-17`; esta regla lo enuncia como regla de negocio y remite a ella como autoridad.
3. El **estimado de peajes y de combustible** se recalcula por la extensión y se vuelve a congelar, con asiento de diferencia contra el anterior ([`RN-41`](RN-41-congelamiento-del-valor-al-autorizar.md), [`RN-42`](RN-42-correccion-retroactiva-con-asiento-de-diferencia.md)).
4. El **consumo que excede el fondo asignado se registra igual**, marcado como **excedido**, con su comprobante y su odómetro; su cobertura se resuelve en la liquidación, **nunca omitiendo el registro**.
5. Cuando la extensión invade la reserva de otra misión ya programada, **no la desplaza automáticamente**: el sistema abre el conflicto y lo resuelve [`RN-56`](RN-56-prelacion-entre-solicitudes-que-compiten.md).
6. El cambio de **ventana efectiva** se expone a ARGOS con la clave de vinculación de la Orden de Misión ([`RN-81`](RN-81-sigti-expone-hechos-a-argos.md)). **SIGTI no calcula, no estima y no muestra el viático.**

## Casos límite

- **`[C]` ¿La magnitud de la extensión cambia quién la autoriza?** Insumo #49. Opciones y costo:

  | Opción | Costo |
  |---|---|
  | Siempre ACT-04, salvo franja inhábil que exige ACT-09 | Es lo que hoy dice `T-17`. Se puede duplicar la duración y el costo sin que el nivel que aprobó la original se entere |
  | Umbral configurable: pasado cierto porcentaje de días, kilómetros o costo, escala al autorizador de la solicitud original | Coherente con el nivel competente y con [`RN-54`](RN-54-cuota-trimestral-de-compromiso.md). El umbral es un parámetro más, y en ruta puede no haber a quién escalar |
  | Reautorización completa por el circuito original | Impracticable desde la carretera: garantiza que se haga por teléfono y se registre falso |

  **Recomendación del análisis, no decisión:** la segunda, con el umbral como parámetro con vigencia y con la salida ya prevista por `T-17` — si no hay forma de obtener la autorización, se registra el hecho y se resuelve en la liquidación ([`RN-73`](RN-73-convalidacion-de-actos-sin-autorizacion-previa.md)).
- **Extensión que no se pudo autorizar** por falta de señal. Se registra la ejecución con la marca de extensión no autorizada y se convalida después; sin convalidación, hallazgo.
- **Varias extensiones sucesivas.** Cada una es una versión. La conciliación usa la vigente a cada hecho, no la última.
- **Extensión que agrega noches de pernocta.** El efecto sobre el viático es de ARGOS. SIGTI expone el hecho con la clave de vinculación y nada más ([DP-001 D-01](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md)).
- **La frecuencia de extensiones es un indicador**, acumulado por dependencia, conductor y tipo de misión ([`RN-82`](RN-82-indicadores-de-calidad-de-la-programacion.md)). Una dependencia cuyas misiones se extienden siempre no tiene un problema de transporte: tiene un problema de planificación.

## Trazabilidad

- Autoridad: [orden-de-mision.md](../../03-arquitectura/estados/orden-de-mision.md) — `T-17` prórroga y destino adicional, con revalidación de `BD-02` y `BD-03`
- Norma: [NRM-01](../normativa/NRM-01-control-interno-tsc.md) `[P]` · Decisión: [DP-001 D-01](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md)
- Reglas relacionadas: [RN-01](RN-01-segregacion-de-funciones.md), [RN-30](RN-30-conciliacion-galonaje-kilometraje.md), [RN-37](RN-37-coherencia-de-la-secuencia-de-casetas.md), [RN-41](RN-41-congelamiento-del-valor-al-autorizar.md), [RN-42](RN-42-correccion-retroactiva-con-asiento-de-diferencia.md), [RN-55](RN-55-habilitacion-vencida-durante-la-mision.md), [RN-56](RN-56-prelacion-entre-solicitudes-que-compiten.md), [RN-73](RN-73-convalidacion-de-actos-sin-autorizacion-previa.md), [RN-81](RN-81-sigti-expone-hechos-a-argos.md), [RN-82](RN-82-indicadores-de-calidad-de-la-programacion.md)
- Casos especiales: [CE-06](../../02-requisitos/casos-especiales/CE-06-la-mision-se-extiende-mas-dias-destinos-o-kilometros.md) — candidatas `RN-c:versionado-del-alcance-autorizado`, `RN-c:revalidacion-de-habilitaciones-al-prorrogar`, `RN-c:destino-adicional-con-dependencia-y-autorizador`, `RN-c:conciliacion-contra-el-alcance-vigente`, `RN-c:consumo-excedido-sobre-el-fondo`
- Insumos pendientes: #49 magnitud de la extensión y nivel autorizante
