# Plantilla — Historia de usuario

Archivo: `docs/02-requisitos/historias/HU-xxx-slug-corto.md`

---

## Esqueleto

```markdown
# HU-xxx — <Título en una línea, orientado al resultado>

| Campo | Valor |
|---|---|
| **Módulo** | M-xx <nombre> |
| **Actor** | ACT-xx <rol> |
| **Prioridad** | Alta / Media / Baja |
| **Sprint** | <número o "sin asignar"> |
| **Estado** | Borrador / Refinada / Lista / En desarrollo / Terminada |

## Historia

**Como** <actor con el rol exacto del glosario>
**quiero** <capacidad concreta>
**para** <beneficio verificable, no una repetición del quiero>

## Contexto

<Dos o tres frases sobre por qué esto importa en la operación real. Si resuelve un
dolor actual del proceso en papel, dilo aquí explícitamente.>

## Reglas que la gobiernan

- [RN-xx](../../01-negocio/reglas/RN-xx-slug.md) — <resumen en media línea>

## Casos especiales que la afectan

- [CE-xx](../casos-especiales/CE-xx-slug.md) — <resumen en media línea>

## Criterios de aceptación

Ver [`docs/05-calidad/features/<slug>.feature`](../../05-calidad/features/<slug>.feature)

<O bien, si son pocos, escribirlos aquí en Gherkin español.>

## Fuera de alcance

<Lo que alguien podría asumir que entra y no entra. Evita discusiones después.>

## Notas y pendientes

- `[C]` <dato que falta confirmar con la institución>
```

---

## Ejemplo completo

# HU-012 — Impedir la asignación de un motorista sin licencia habilitante

| Campo | Valor |
|---|---|
| **Módulo** | M-07 Programación y Despacho |
| **Actor** | ACT-05 Encargado de Despacho |
| **Prioridad** | Alta |
| **Sprint** | 5 |
| **Estado** | Refinada |

## Historia

**Como** Encargado de Despacho
**quiero** que el sistema me impida asignar un motorista a un vehículo cuya categoría no cubre su licencia, o cuya licencia estará vencida el día del viaje
**para** no trasladar responsabilidad legal a la institución ni a quien autorizó la misión

## Contexto

Hoy la verificación depende de que el encargado recuerde la categoría de licencia de cada motorista y la fecha de vencimiento. Con más de cuarenta motoristas y rotación de personal, el error es cuestión de tiempo. Si ocurre un accidente con un motorista no habilitado, la responsabilidad recae directamente sobre quien autorizó la misión — no sobre el motorista únicamente.

Es la validación de mayor valor legal del sistema.

## Reglas que la gobiernan

- [RN-07](../../01-negocio/reglas/RN-07-licencia-habilitante.md) — La categoría de la licencia debe cubrir el tipo y peso bruto del vehículo asignado
- [RN-08](../../01-negocio/reglas/RN-08-vigencia-licencia.md) — La licencia debe estar vigente durante **todo** el rango de fechas de la misión, no solo el día de salida
- [RN-09](../../01-negocio/reglas/RN-09-restriccion-medica.md) — Las restricciones médicas anotadas en la licencia se validan contra las condiciones del viaje

## Casos especiales que la afectan

- [CE-04](../casos-especiales/CE-04-licencia-vence-durante-mision.md) — La licencia vence a mitad de una misión de varios días
- [CE-11](../casos-especiales/CE-11-cambio-motorista-en-ruta.md) — Se sustituye al motorista con la misión ya en curso
- [CE-19](../casos-especiales/CE-19-emergencia-sin-motorista-habilitado.md) — Emergencia sin ningún motorista habilitado disponible

## Criterios de aceptación

```gherkin
Característica: Habilitación del motorista para el vehículo asignado

  Antecedentes:
    Dado un vehículo "Camión Isuzu FVR" de tipo "Camión" con peso bruto de 12000 kg
    Y un motorista "José Martínez" con licencia categoría "C1" vigente hasta el "2027-03-15"

  Escenario: Se rechaza la asignación por categoría insuficiente
    Cuando el Encargado de Despacho intenta asignar a "José Martínez" al "Camión Isuzu FVR"
    Entonces el sistema rechaza la asignación
    Y muestra "La licencia categoría C1 habilita hasta 7,500 kg. El vehículo tiene 12,000 kg y requiere categoría C."
    Y registra el intento en la bitácora de auditoría

  Escenario: Se rechaza por licencia vencida antes del retorno
    Dado una misión programada del "2027-03-10" al "2027-03-20"
    Y un vehículo "Pickup Hilux" de tipo "Pickup" con peso bruto de 2800 kg
    Cuando el Encargado de Despacho intenta asignar a "José Martínez" a esa misión
    Entonces el sistema rechaza la asignación
    Y muestra "La licencia vence el 15/03/2027, antes del retorno previsto el 20/03/2027."

  Escenario: Se acepta la asignación habilitada
    Dado una misión programada del "2027-01-10" al "2027-01-12"
    Y un vehículo "Pickup Hilux" de tipo "Pickup" con peso bruto de 2800 kg
    Cuando el Encargado de Despacho asigna a "José Martínez" a esa misión
    Entonces el sistema acepta la asignación
    Y la Orden de Misión pasa al estado "PROGRAMADA"
```

## Fuera de alcance

- La captura y actualización del expediente del motorista — es [HU-006](HU-006-expediente-motorista.md)
- La alerta anticipada de vencimiento de licencias — es [HU-014](HU-014-alertas-vencimiento.md)
- La validación contra el registro de la DNVT: no hay integración disponible; el dato es el que capturó la institución

## Notas y pendientes

- `[C]` Confirmar con la institución si existe algún supuesto de excepción autorizada por la máxima autoridad. La postura por defecto es **no**: el bloqueo es duro y sin anulación posible, porque una excepción registrada en el sistema sería evidencia en contra ante un siniestro.
- `[P]` La reforma al Art. 48 de la Ley de Tránsito (2025) modificó las categorías CD y CE. Obtener el texto reformado antes de codificar la tabla completa. Ver [NRM-06](../../01-negocio/normativa/NRM-06-transito-y-licencias.md).
