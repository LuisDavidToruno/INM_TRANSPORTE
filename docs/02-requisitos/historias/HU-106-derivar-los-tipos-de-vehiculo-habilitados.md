# HU-106 — Derivar qué vehículos concretos puede conducir cada motorista, contra la matriz vigente

| Campo | Valor |
|---|---|
| **Módulo** | M-05 Motoristas y Habilitación · M-02 Catálogos Maestros |
| **Actor** | ACT-04 Jefe de Transporte |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — bloqueada por el texto de la reforma al Art. 48 (2025) sobre las categorías `CD` y `CE` (insumos #20 y #23): **sin él la matriz definitiva licencia↔vehículo no se puede fijar**, y esa matriz es el objeto entero de la historia. Es el pendiente más importante de `NRM-06` |

## Historia

**Como** Jefe de Transporte
**quiero** que el sistema me diga **qué vehículos concretos de la flota** puede conducir cada motorista y cuáles no, con el atributo que excluye a cada uno, y que guarde el resultado con todos sus insumos
**para** que el despacho deje de trabajar por ensayo y error, y para poder responder ante un siniestro con qué se verificó y contra qué versión de la matriz

## Contexto

La derivación se resuelve por los atributos de la ficha técnica —tipo, **peso bruto vehicular en kg**, capacidad de pasajeros y condición de articulado—, **nunca por el nombre comercial del modelo**.

Y lo que se guarda no es "verificado": se guarda **el resultado con todos sus insumos** — número de licencia, categoría, vencimiento consultado, versión de la matriz, atributos del vehículo usados y fecha de fin de rango evaluada. Guardar solo un sí o un no no defiende a nadie ante un siniestro.

La evaluación se repite **al programar y otra vez al despachar**: entre una cosa y la otra pueden pasar días y una licencia puede haber vencido.

## Reglas que la gobiernan

- [RN-09](../../01-negocio/reglas/RN-09-matriz-licencia-vehiculo.md) — **Regla eje.** La categoría debe habilitar tipo, peso bruto y capacidad. **Bloqueo duro sin excepción**
- [RN-10](../../01-negocio/reglas/RN-10-licencia-vigente-en-todo-el-rango.md) — Vigencia durante **todo el rango** de la misión, no solo el día de salida
- [RN-40](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md) — La matriz se resuelve a la fecha del hecho
- [RN-41](../../01-negocio/reglas/RN-41-congelamiento-del-valor-al-autorizar.md) — La evaluación se congela con el identificador de la versión de la matriz usada
- [RN-39](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md) — La matriz es catálogo con vigencia y doble control
- [RN-11](../../01-negocio/reglas/RN-11-restricciones-medicas-del-motorista.md) — Las restricciones médicas se evalúan contra las condiciones de la misión

## Casos especiales que la afectan

- [CE-11](../casos-especiales/CE-11-licencia-vence-durante-la-mision.md) — La licencia vence dentro del rango
- [CE-13](../casos-especiales/CE-13-motorista-no-disponible-por-talento-humano.md) — Habilitado no es lo mismo que disponible
- [CE-05](../casos-especiales/CE-05-cambio-de-motorista-con-la-mision-en-curso.md) — El relevo revalida la habilitación

## Criterios de aceptación

```gherkin
# language: es
Característica: Derivación de tipos de vehículo habilitados por licencia

  Antecedentes:
    Dado un motorista "José Martínez" con categoría "C1" vigente hasta el "2027-03-15" y categoría "B" vigente hasta el "2028-05-12"
    Y un vehículo "TR-0098" tipo "Camión", peso bruto "12,000" kg, no articulado
    Y un vehículo "TR-0045" tipo "Pickup", peso bruto "2,800" kg, no articulado
    Y una matriz licencia↔vehículo vigente y aprobada

  Escenario: Se rechaza la asignación por categoría insuficiente
    Cuando el Encargado de Despacho intenta asignar a "José Martínez" al vehículo "TR-0098"
    Entonces el sistema rechaza la asignación
    Y muestra "La licencia categoría C1 habilita hasta 7,500 kg. TR-0098 tiene 12,000 kg y requiere categoría C."
    Y el intento queda registrado en la bitácora de auditoría

  Escenario: Se rechaza por licencia vencida antes del retorno
    Dado una misión programada del "2027-03-10" al "2027-03-20"
    Cuando el Jefe de Transporte intenta asignar a "José Martínez" al vehículo "TR-0045"
    Entonces el sistema rechaza la asignación
    Y muestra "La categoría C1 vence el 15/03/2027, antes del retorno previsto el 20/03/2027."

  Escenario: El bloqueo propone alternativas en el mismo acto
    Cuando el sistema bloquea la asignación por licencia
    Entonces muestra la lista de motoristas habilitados para "TR-0098" con licencia vigente en todo el rango
    Y muestra "Bloquear sin ofrecer alternativa es lo que empuja a operar fuera del sistema."

  Escenario: La derivación no usa el nombre comercial del modelo
    Cuando el Jefe de Transporte consulta qué vehículos puede conducir "José Martínez"
    Entonces el sistema resuelve por tipo, peso bruto vehicular, capacidad de pasajeros y condición de articulado
    Y no resuelve por marca ni modelo

  Escenario: Se muestra el resultado en el lenguaje del despacho
    Cuando el Jefe de Transporte consulta la habilitación de "José Martínez"
    Entonces el sistema lista los vehículos concretos de la flota que puede conducir
    Y lista los que no puede, con el atributo que excluye a cada uno
    Y muestra "TR-0098: excluido por peso bruto 12,000 kg, requiere categoría C."

  Escenario: Se guarda el resultado con todos sus insumos
    Cuando el sistema evalúa la habilitación de "José Martínez" para "TR-0045"
    Entonces conserva número de licencia, categoría evaluada, vencimiento consultado, versión de la matriz, atributos del vehículo usados y fecha de fin de rango evaluada
    Y no se admite guardar únicamente el resultado "habilitado"

  Escenario: La evaluación se repite al despachar, no se hereda de la programación
    Dado una asignación programada el "2027-03-01" con la licencia vigente
    Y que la categoría "C1" venció el "2027-03-15"
    Cuando el Encargado de Despacho despacha la misión el "2027-03-16"
    Entonces el sistema rechaza el despacho
    Y muestra "La categoría C1 de José Martínez venció el 15/03/2027. La verificación de la programación no sustituye a la del despacho."

  Escenario: No existe pantalla de excepción para el bloqueo de licencia
    Cuando cualquier rol, incluida la Máxima Autoridad, intenta continuar con la asignación bloqueada
    Entonces el sistema no ofrece ninguna opción de excepción, acuse ni autorización superior
    Y muestra "La habilitación licencia↔vehículo no admite excepción configurable."

  Escenario: La habilitación parcial no es pérdida total
    Dado que la categoría "C1" de "José Martínez" venció y la "B" sigue vigente
    Cuando el sistema recalcula su habilitación
    Entonces "José Martínez" queda habilitado para los vehículos que cubre la categoría "B"
    Y queda no habilitado para los que exigían "C1"
    Y el sistema lista las misiones programadas que dependían de la categoría perdida

  Escenario: La restricción médica se evalúa contra las condiciones de la misión
    Dado una restricción "prohibición de conducción nocturna" en la licencia de "José Martínez"
    Y una misión con retorno previsto a las "22:30"
    Cuando el Jefe de Transporte intenta asignarlo a esa misión
    Entonces el sistema aplica el efecto configurado para esa restricción
    Y muestra "José Martínez tiene restricción de conducción nocturna. El retorno previsto es a las 22:30."
```

## Fuera de alcance

- La captura de la licencia — es [HU-105](HU-105-capturar-la-licencia-como-dato-propio-de-sigti.md)
- La disponibilidad del motorista por permisos, vacaciones o incapacidad: viene del espejo de Talento Humano — es [HU-110](HU-110-inhabilitar-con-causa-y-encaminar-misiones.md)
- La ficha técnica del vehículo — es [HU-098](HU-098-completar-la-ficha-tecnica-que-habilita.md)
- La validación contra el registro de la DNVT: no hay integración disponible

## Notas y pendientes

- `[C]` **Texto de la reforma al Art. 48 (2025)** sobre las categorías `CD` y `CE`. **Sin él la matriz definitiva no se puede fijar** — insumos **#20** y **#23**. Es el pendiente más importante de [NRM-06](../../01-negocio/normativa/NRM-06-transito-y-licencias.md)
- `[V]` Las ocho categorías conocidas —A, B, B1, C1, C, D1, D y CE— por fuentes concordantes; `[C]` el contraste con el texto oficial
- `[I]` La formulación *"bloquear si la licencia estará vencida en cualquier fecha del rango"* es **implicación de requerimiento del equipo**, no articulado citable. **No se eleva el nivel**
- Los pesos y capacidades de los ejemplos son ilustrativos: la matriz es catálogo con vigencia y **no se cablea ningún umbral**
- `[C]` Catálogo `restriccion_medica` con su efecto —bloqueo o advertencia— y su vigencia. Se entrega vacío — insumo **#42**
