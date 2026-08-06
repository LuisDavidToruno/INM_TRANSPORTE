# Plantilla — Caso especial

Archivo: `docs/02-requisitos/casos-especiales/CE-xx-slug-corto.md`

Un caso especial es una situación de la **operación real** que el flujo feliz no contempla. No es un error del usuario ni un caso de prueba: es algo que pasa, que hoy se resuelve de alguna manera con papel y criterio, y que el sistema tiene que absorber sin trabar la operación.

**Ningún caso especial se cierra sin regla de resolución.** Si no sabemos cómo resolverlo, se marca `[C]` y se escala al PO.

---

## Esqueleto

```markdown
# CE-xx — <La situación en una línea, en el lenguaje de quien la vive>

| Campo | Valor |
|---|---|
| **Módulos** | M-xx |
| **Estados afectados** | <estados del ciclo de vida donde puede ocurrir> |
| **Frecuencia** | Frecuente / Ocasional / Raro pero grave |
| **Impacto** | Operativo / Financiero / Legal / Auditoría |
| **Resolución** | Definida / `[C]` Por confirmar con la institución |

## La situación

<Descrita como la contaría el motorista o el encargado. Concreta, con nombres y
números si ayudan. Nada de abstracciones.>

## Qué se hace hoy sin sistema

<Cómo lo resuelve la institución actualmente. Casi siempre revela una regla que
nadie escribió nunca.>

## Por qué el flujo normal no lo cubre

<La razón exacta por la que el camino feliz falla aquí.>

## Regla de resolución

<Qué debe hacer el sistema. Enlaza a la RN-xx que la formaliza, o créala.>

## Evidencia que debe quedar

<Qué tiene que poder mostrarle la institución al auditor después de este caso.>

## Trazabilidad
```

---

## Ejemplo completo

# CE-07 — El vehículo se avería en ruta y la misión no puede continuar

| Campo | Valor |
|---|---|
| **Módulos** | M-08 Ejecución y Bitácora, M-11 Mantenimiento, M-12 Incidentes, M-13 Liquidación |
| **Estados afectados** | `EN_RUTA` |
| **Frecuencia** | Ocasional — flota con años de uso y carreteras en mal estado |
| **Impacto** | Operativo, financiero y de auditoría |
| **Resolución** | Definida |

## La situación

Una comisión sale de Tegucigalpa hacia Puerto Lempira con equipo de cómputo para una delegación. A la altura de Catacamas el pickup pierde el sistema de frenos. El motorista se detiene, no hay señal telefónica estable, y la carga sigue en el vehículo a media carretera.

Desde ahí puede pasar cualquiera de estas cosas: llega una grúa y el vehículo se remolca a un taller; se envía otro vehículo desde la delegación más cercana y la misión continúa; o la misión se aborta y todos regresan. Cada una deja el expediente en un estado distinto.

Mientras tanto: se emitieron vales de combustible por el recorrido completo, se anticiparon viáticos por cuatro noches, y el kilometraje de retorno no va a corresponder con nada.

## Qué se hace hoy sin sistema

El motorista llama cuando consigue señal. El encargado de transporte anota en un cuaderno. La bitácora se completa a mano al regresar, con el kilometraje del punto de la avería, y se adjunta el reporte del taller. Los viáticos se liquidan "como se pueda" y los vales sobrantes se devuelven — a veces con acta, a veces no.

**El "a veces no" es exactamente el hallazgo de auditoría.**

## Por qué el flujo normal no lo cubre

El flujo feliz asume que la misión termina donde estaba planeada, con el vehículo que salió y el motorista que salió. Aquí se rompen las tres cosas a la vez, y además el registro tiene que hacerse **sin conectividad**, desde la carretera, por alguien que en ese momento está resolviendo un problema mecánico y no quiere pelear con una aplicación.

## Regla de resolución

El sistema incorpora el evento **`INTERRUPCION_EN_RUTA`**, registrable desde el cliente de campo **sin conexión**, con la mínima fricción posible: ubicación, odómetro, hora, motivo, y fotografías.

Registrar el evento **congela** la Orden de Misión en `EN_RUTA` y habilita exactamente tres desenlaces:

| Desenlace | Qué hace el sistema |
|---|---|
| **Continúa con vehículo sustituto** | Crea un **tramo nuevo** bajo la misma Orden de Misión con el vehículo y motorista sustitutos. La bitácora del vehículo original se cierra en el punto de la avería con su odómetro. El combustible y el kilometraje se imputan a cada vehículo por separado. Los viáticos no se alteran. |
| **Se aborta la misión y retorna** | La Orden de Misión pasa a `RETORNADA` con motivo `INTERRUPCION`. Fuerza la liquidación por lo efectivamente ejecutado: viáticos por las noches reales, vales no consumidos a devolución con acta. |
| **Queda pendiente de resolución** | Estado transitorio con responsable y fecha límite asignados. No permite cerrar el ejercicio con misiones en este estado. |

En los tres casos, el evento genera automáticamente una **orden de mantenimiento correctivo** (M-11) para el vehículo y lo marca `NO_DISPONIBLE`, para que no pueda ser asignado a otra misión.

Formalizado en `RN-23` (interrupción de misión en ruta) y `RN-24` (imputación de consumo por tramo).

## Evidencia que debe quedar

Ante una auditoría, la institución debe poder mostrar, encadenado a la misma Orden de Misión:

1. El registro del evento con hora, ubicación, odómetro y fotografías
2. El acta de devolución de vales no consumidos, o la justificación de su consumo
3. La liquidación de viáticos por las noches efectivamente pernoctadas
4. La orden de mantenimiento y su resultado
5. La bitácora de cada vehículo involucrado, cerrada con su propio kilometraje
6. Quién autorizó el desenlace elegido y cuándo

## Trazabilidad

- Reglas: `RN-23`, `RN-24`, `RN-31` (devolución de vales por misión interrumpida)
- Historias: `HU-055` (registrar interrupción en ruta), `HU-056` (sustituir vehículo en misión activa)
- Casos especiales relacionados: `CE-11` (cambio de motorista en ruta), `CE-13` (viaje cancelado con vales emitidos), `CE-18` (bitácora en papel por falta de señal)
