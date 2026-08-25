# HU-043 — Sustituir el vehículo o el motorista con la misión en `PROGRAMADA`, revalidando todo para el entrante

| Campo | Valor |
|---|---|
| **Módulo** | M-07 Programación y Despacho |
| **Actor** | ACT-04 Jefe de Transporte · ACT-10 Encargado de Delegación en su ámbito |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Refinada |
| **Deriva de** | [CU-07](../casos-de-uso/CU-07-sustituir-vehiculo-o-motorista.md) flujo principal, A1, A2, A3, E1, E2, E5 · `T-10` · `BD-02`, `BD-03`, `BD-07`, `BD-10`, `BD-11` |

## Historia

**Como** Jefe de Transporte
**quiero** sustituir el vehículo o el motorista de una misión programada con motivo tipificado, revalidando sobre el recurso entrante **todas** las precondiciones de la asignación original
**para** que la sustitución no sea la puerta trasera por donde entra un motorista sin licencia habilitante o un vehículo sin categoría de peaje resuelta

## Contexto

La sustitución es lo que más ocurre en la operación real: el vehículo entra a taller, el motorista se incapacita, la licencia vence dentro del rango, el requerimiento cambia. Y es también el punto donde el control se relaja: "ya se había verificado" es la frase que precede al hallazgo.

**No se da por buena ninguna verificación previa.** El recurso entrante pasa por `BD-02`, `BD-03`, `BD-07`, `BD-10` y `BD-11` completos, como si fuera la primera asignación.

Dos consecuencias que no son obvias:

- **El folio reservado no cambia**: es el mismo expediente, la misma necesidad de movilización.
- **Si cambia el vehículo, cambia todo lo que se derivaba de él**: categoría de peaje, tarifas esperadas, rendimiento esperado, estimado de combustible. Se recalcula, se vuelve a congelar y **queda asiento de la diferencia** — el valor histórico no se sobrescribe.

## Reglas que la gobiernan

- [RN-14](../../01-negocio/reglas/RN-14-sustitucion-de-motorista.md) — **Regla rectora**: la sustitución revalida todas las habilitaciones y conserva la asignación original
- [RN-09](../../01-negocio/reglas/RN-09-matriz-licencia-vehiculo.md) · [RN-10](../../01-negocio/reglas/RN-10-licencia-vigente-en-todo-el-rango.md) · [RN-11](../../01-negocio/reglas/RN-11-restricciones-medicas-del-motorista.md) — `BD-02` sobre el motorista entrante
- [RN-12](../../01-negocio/reglas/RN-12-disponibilidad-del-motorista.md) · [RN-13](../../01-negocio/reglas/RN-13-sin-doble-asignacion.md) — `BD-10` y `BD-11` sobre el entrante
- [RN-19](../../01-negocio/reglas/RN-19-vehiculo-no-operativo-no-se-asigna.md) · [RN-60](../../01-negocio/reglas/RN-60-indisponibilidad-sobrevenida-y-reservas.md) — Estado operativo y desenlace explícito de cada reserva afectada
- [RN-33](../../01-negocio/reglas/RN-33-categoria-de-peaje-derivada-de-ficha-tecnica.md) — El vehículo entrante también necesita categoría resuelta
- [RN-61](../../01-negocio/reglas/RN-61-sustitucion-de-vehiculo-recalcula-valores-congelados.md) · [RN-42](../../01-negocio/reglas/RN-42-correccion-retroactiva-con-asiento-de-diferencia.md) — Recálculo, recongelamiento y asiento de diferencia
- [RN-82](../../01-negocio/reglas/RN-82-indicadores-de-calidad-de-la-programacion.md) — El motivo tipificado alimenta el indicador; un texto libre no
- [RN-01](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md) — **Segregación de funciones sobre el motorista entrante.** La regla exige la comprobación *«antes de asignar o **sustituir** al motorista»*, y `I-11` es **núcleo irreductible**. Incorporada por `HB34-02`

La matriz de incompatibilidades es autoridad de [`actores-y-roles.md §5.2`](../../01-negocio/actores-y-roles.md): esta historia **no la copia**, la aplica.

> **Nota de corrección — `HB34-02`.** La lista de revalidaciones del entrante —`BD-02`, `BD-03`, `BD-07`, `BD-10`, `BD-11`— **no incluía la segregación**, y la sustitución es justamente la puerta por donde entra el caso que `RN-01` describe en sus casos límite: *«el encargado autoriza el lunes, el motorista se incapacita el martes a las 05:50, y se pone él»*. La regla lo resuelve sin margen: **bloqueo duro por `I-11`; no hay confirmación con motivo, no hay advertencia registrada, no hay urgencia que lo habilite**. La salida es sustituir por otro motorista habilitado o escalar por `RN-02`. El principio de la historia —*«no se da por buena ninguna verificación previa»*— exige que la matriz también se reevalúe: en la asignación original el entrante no existía.

## Casos especiales que la afectan

- [CE-16](../casos-especiales/CE-16-vehiculo-a-taller-con-misiones-programadas.md) — Entra a taller con misiones ya programadas: ninguna reserva queda sin desenlace
- [CE-13](../casos-especiales/CE-13-motorista-no-disponible-por-talento-humano.md) — Permiso, vacaciones o incapacidad llegan por el espejo
- [CE-11](../casos-especiales/CE-11-licencia-vence-durante-la-mision.md) — La licencia vence dentro del rango
- [CE-12](../casos-especiales/CE-12-dos-solicitudes-compiten-por-el-mismo-vehiculo.md) — El sustituto disponible ya está reservado por otra misión

## Criterios de aceptación

```gherkin
# language: es
Característica: Sustitución de recurso con la misión en PROGRAMADA

  Antecedentes:
    Dada una Orden de Misión "OM-2026-0451" en estado "PROGRAMADA",
      con folio reservado "OM-2026-0451", ventana del "2026-09-15" al "2026-09-17"
    Y un vehículo asignado "Pickup Toyota Hilux" con correlativo "INS-P-014",
      categoría de peaje "Categoría 1" y estimado de peajes congelado de "160.00" lempiras
    Y un motorista asignado "José Martínez"
    Y que "Carlos Rodríguez" autorizó la Orden de Misión "OM-2026-0451" el "2026-09-12"
    Y un umbral de variación del estimado que exige reautorización del "20" por ciento

  Escenario: Se rechaza sustituir al motorista por quien autorizó la misma misión
    Dado un motorista entrante "Carlos Rodríguez" con licencia "04-1983-22910" categoría "C"
      vigente hasta el "2028-04-30", disponible en la ventana y sin restricciones médicas
    Cuando el Jefe de Transporte intenta sustituir a "José Martínez" por "Carlos Rodríguez"
      con motivo "motorista no disponible"
    Entonces el sistema rechaza la sustitución
    Y muestra "Carlos Rodríguez autorizó la Orden de Misión OM-2026-0451 el 12/09/2026. Por incompatibilidad I-11 (RN-01) no puede ser asignado como motorista de esa misión. Es núcleo irreductible: no admite excepción."
    Y no ofrece confirmación con motivo, advertencia superable ni excepción por urgencia
    Y propone sustituir por otro motorista habilitado o escalar la autorización a otro puesto (RN-02)
    Y la asignación de "José Martínez" permanece vigente
    Y registra el intento con el par de incompatibilidad detectado

  Escenario: Se rechaza la sustitución sin motivo tipificado
    Cuando el Jefe de Transporte sustituye el motorista escribiendo solo "lo cambiaron"
    Entonces el sistema rechaza la sustitución
    Y muestra "Seleccione el motivo del catálogo: vehículo a taller, motorista no disponible, licencia que vence dentro del rango, cambio de requerimiento, consolidación, desplazamiento por prioridad superior."

  Escenario: Se rechaza el motorista entrante sin licencia habilitante
    Dado un motorista entrante "Marvin Discua" con licencia "05-1990-11987" categoría "B"
    Y un vehículo asignado "Camión Isuzu FVR" con correlativo "INS-C-002" de "12000" kg de peso bruto
    Cuando el Jefe de Transporte intenta sustituir a "José Martínez" por "Marvin Discua"
      con motivo "motorista no disponible"
    Entonces el sistema rechaza la sustitución
    Y muestra "La licencia categoría B no habilita un vehículo de 12,000 kg de peso bruto. El vehículo INS-C-002 requiere categoría C."
    Y la asignación de "José Martínez" permanece vigente
    Y registra el intento con los datos evaluados

  Escenario: Se rechaza el vehículo entrante sin categoría de peaje resuelta
    Dado un vehículo entrante "Pickup Mitsubishi L200" con correlativo "INS-P-030",
      sin categoría de peaje asignada
    Cuando el Jefe de Transporte intenta sustituir el "INS-P-014" por el "INS-P-030"
      con motivo "vehículo a taller"
    Entonces el sistema rechaza la sustitución
    Y muestra "El vehículo INS-P-030 no tiene categoría de peaje resuelta: el estimado de peajes no sería verificable."

  Escenario: Se rechaza el recurso entrante ya reservado en la franja
    Dado un vehículo entrante "Pickup Nissan Frontier" con correlativo "INS-P-021",
      reservado por la Orden de Misión "OM-2026-0460" de la Unidad de Bienes
      del "2026-09-15" al "2026-09-16"
    Cuando el Jefe de Transporte intenta sustituir el "INS-P-014" por el "INS-P-021"
    Entonces el sistema rechaza la sustitución
    Y muestra "El vehículo INS-P-021 está reservado por la Orden de Misión OM-2026-0460 de la Unidad de Bienes, del 15/09/2026 al 16/09/2026."
    Y ofrece, en este orden: "Consolidar con OM-2026-0460", "Asignar otro recurso", "Reprogramar una de las dos", "Escalar la prioridad"

  Escenario: La sustitución de vehículo recalcula y deja asiento de la diferencia
    Dado un vehículo entrante "Microbús Toyota Coaster" con correlativo "INS-B-003",
      categoría de peaje "Categoría 2" resuelta y disponible en la ventana
    Cuando el Jefe de Transporte sustituye el "INS-P-014" por el "INS-B-003"
      con motivo "vehículo a taller"
    Entonces el sistema recalcula el estimado de peajes con la tabla vigente a la fecha programada
    Y vuelve a congelar categoría de peaje, tarifas esperadas, rendimiento esperado
      y estimado de combustible
    Y deja asiento con el valor anterior "160.00" y el valor nuevo, sin sobrescribir el histórico
    Y el folio reservado "OM-2026-0451" no cambia

  Escenario: La variación del estimado por encima del umbral exige reautorización antes de despachar
    Dado que el estimado de peajes recalculado es de "260.00" lempiras
    Cuando el Jefe de Transporte confirma la sustitución del vehículo
    Entonces el sistema marca la misión como "requiere reautorización"
    Y muestra "El estimado de peajes pasó de 160.00 a 260.00 lempiras, una variación del 62.5% sobre el umbral del 20%. Se requiere nueva autorización antes de despachar."
    Y el despacho queda bloqueado hasta que exista la reautorización

  Escenario: La sustitución conserva la asignación original en el diario
    Cuando el Jefe de Transporte sustituye a "José Martínez" por "Elder Zavala"
      con motivo "motorista no disponible"
    Entonces la reserva de "José Martínez" se libera y se crea la de "Elder Zavala"
    Y el diario muestra a "José Martínez" como asignación original, el motivo del cambio
      y a "Elder Zavala" como asignación vigente
    Y se notifica al motorista saliente, al entrante y a la dependencia solicitante

  Escenario: Se rechaza la sustitución fuera del ámbito de competencia
    Dada una misión de la sede
    Cuando el Encargado de Delegación de Choluteca intenta sustituir su vehículo
    Entonces el sistema rechaza la sustitución
    Y muestra "Esta misión no pertenece al ámbito de la Delegación Choluteca. Solicite la sustitución a quien tiene competencia o escale a sede."
    Y registra el intento

  Escenario: Cada reserva afectada por la indisponibilidad exige desenlace explícito
    Dado que el Encargado de Mantenimiento declara el "INS-P-014" en "EN_TALLER"
      con ventana estimada del "2026-09-14" al "2026-09-20"
    Y que el vehículo tiene tres misiones programadas dentro de esa ventana
    Cuando el Jefe de Transporte abre la lista de reservas afectadas
    Entonces el sistema lista las tres misiones con su ventana y su dependencia solicitante
    Y exige por cada una un desenlace: sustituir, desprogramar o anular
    Y ninguna misión afectada puede quedar sin desenlace registrado
```

## Fuera de alcance

- La sustitución con la misión ya `DESPACHADA` — es [HU-044](HU-044-sustituir-con-la-mision-despachada.md)
- El relevo de motorista con la misión `EN_RUTA` — es [HU-045](HU-045-relevo-de-motorista-en-ruta.md)
- La declaración del estado `EN_TALLER` y la orden de trabajo — son de M-11
- El desplazamiento por prioridad superior, que solo ejerce la Gerencia Administrativa — es [HU-027](HU-027-reserva-exclusiva-y-conflicto-con-su-titular.md)

## Notas y pendientes

- `[C]` **Umbral de variación del estimado que exige reautorización** — insumo #1 / #19.
- `[C]` **Ventana de indisponibilidad estimada exigible** al enviar un vehículo a taller — insumo #59. Sin ella, el sistema no puede decir qué misiones programadas quedan afectadas.
- `[C]` **Criterio de prelación** cuando el recurso sustituto está tomado — insumo #31.
- Corregido por `HB34-19`: el rechazo del recurso entrante ya reservado tenía una **descripción de contenido** en lugar de un mensaje. El DoR exige el texto que ve el usuario, no la lista de lo que debe contener.
- Si había permiso de circulación en día inhábil emitido para el vehículo saliente, **deja de cubrir la misión y hay que reemitirlo** ([CU-03](../casos-de-uso/CU-03-permiso-de-circulacion-en-dia-inhabil.md) E2).
