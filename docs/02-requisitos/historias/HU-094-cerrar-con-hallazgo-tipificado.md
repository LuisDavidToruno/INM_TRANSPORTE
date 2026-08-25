# HU-094 — Cerrar con hallazgo cuando se cumple un criterio, sin que el expediente quede abierto para siempre

| Campo | Valor |
|---|---|
| **Módulo** | M-13 Liquidación y Cierre · M-14 Reportes, Indicadores y Auditoría |
| **Actor** | ACT-08 Gerencia Administrativa · ACT-12 Auditor Interno (puede requerirlo) |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — faltan los umbrales de los criterios configurables (insumos #1 y #19), la decisión sobre si los hallazgos reiterados de un mismo motorista o vehículo **bloquean o advierten** antes de nuevas asignaciones (insumo #1) y los informes previos de Auditoría Interna o del TSC sobre flota, que son el catálogo real de tipos de hallazgo (insumo #19) |

## Historia

**Como** Gerencia Administrativa
**quiero** cerrar con hallazgo el expediente que cumple algún criterio, con el hallazgo tipificado, sus datos concretos, su responsable de seguimiento y su plazo
**para** que ningún expediente quede abierto indefinidamente, porque *un expediente que no puede cerrarse se abandona, y un expediente abandonado no produce el hallazgo que el auditor necesita ver*

## Contexto

**`CERRADA_CON_HALLAZGO` no imputa responsabilidad a nadie, no sanciona y no debe presentarse como falta en ningún reporte.** Un vehículo robado en ruta produce hallazgo y nadie es culpable. Es una marca de seguimiento.

Dos límites duros la sostienen. Primero: **si algún criterio se cumple, el cierre limpio no está disponible** — quien cierra no elige entre limpio y con hallazgo; el criterio decide y él lo confirma. Segundo: **no es un cajón de sastre.** Si el criterio no está en la lista, no se cierra con hallazgo. Un estado que absorbe todo lo que incomoda deja de significar algo en seis meses, y entonces el auditor deja de mirarlo.

**La misión queda cerrada.** Lo que queda abierto es el expediente de hallazgo, que es otra entidad con su propio ciclo.

## Reglas que la gobiernan

- [RN-08](../../01-negocio/reglas/RN-08-cadena-de-trazabilidad-para-cierre.md) — Salida por cierre con hallazgo, con eslabón, motivo y responsable consignados
- [RN-39](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md) — Umbrales y catálogo de criterios como parámetros con vigencia y doble control
- [RN-86](../../01-negocio/reglas/RN-86-obligacion-de-reintegro-por-saldo-no-devuelto.md) — La obligación de reintegro **no impide cerrar**: sobrevive al cierre con ciclo propio
- [RN-92](../../01-negocio/reglas/RN-92-reclamo-por-discrepancia-de-peaje.md) — El reclamo pendiente ante la SAPP **ya no condiciona el cierre** de la misión
- [RN-52](../../01-negocio/reglas/RN-52-registro-de-consultas-a-manifiestos.md) — Las consultas del Auditor Interno quedan registradas
- [RN-05](../../01-negocio/reglas/RN-05-registro-cerrado-no-se-edita.md) — Resolver el hallazgo no reescribe la historia de la misión

## Casos especiales que la afectan

- [CE-21](../casos-especiales/CE-21-galonaje-que-no-cuadra-con-kilometraje.md) — Origen típico del criterio de consumo
- [CE-24](../casos-especiales/CE-24-cobro-en-categoria-de-peaje-equivocada.md) — Discrepancia de clasificación con reclamo pendiente
- [CE-25](../casos-especiales/CE-25-comprobante-perdido-o-estacion-sin-factura.md) — Ausencia de comprobante obligatorio
- [CE-11](../casos-especiales/CE-11-licencia-vence-durante-la-mision.md) — Bloqueo duro que falló al revalidarse
- [CE-26](../casos-especiales/CE-26-sobrante-o-faltante-al-liquidar.md) — Fondo no devuelto ni comprobado al vencer el plazo

## Criterios de aceptación

```gherkin
# language: es
Característica: Cierre del expediente con hallazgo

  Antecedentes:
    Dado una Orden de Misión "OM-2026-0468" en estado "LIQUIDADA"
    Y todas sus asignaciones de fondo conciliadas, una de ellas "CONCILIADA_CON_DESVIACION"
    Y una desviación de rendimiento del "50.0" por ciento sin justificación aceptada

  Escenario: El cierre limpio no está disponible cuando un criterio se cumple
    Cuando la Gerencia Administrativa abre el expediente "OM-2026-0468"
    Entonces el sistema no ofrece la opción de cerrar sin hallazgo
    Y muestra "Se cumple el criterio H-01. El cierre limpio no está disponible."

  Escenario: No existe la opción de desactivar un criterio para una misión concreta
    Cuando la Gerencia Administrativa busca desactivar el criterio "H-01" para "OM-2026-0468"
    Entonces el sistema no ofrece esa opción
    Y muestra "Los umbrales y el catálogo de criterios son parámetros con vigencia y doble control. No se desactivan por caso."

  Escenario: Se rechaza el cierre con hallazgo sin motivo ni tipificación
    Cuando la Gerencia Administrativa intenta cerrar "OM-2026-0468" sin motivo ni tipo de hallazgo
    Entonces el sistema rechaza el cierre
    Y muestra "El cierre con hallazgo exige motivo obligatorio, tipo de hallazgo del catálogo y responsable de seguimiento."

  Escenario: Se cierra con hallazgo y se crea el expediente de seguimiento
    Cuando la Gerencia Administrativa cierra "OM-2026-0468" con tipo "consumo sin justificación aceptada", motivo escrito y responsable "Marvin Aguilar"
    Entonces la misión pasa a estado "CERRADA_CON_HALLAZGO"
    Y el sistema crea un expediente de hallazgo con el criterio "H-01", los datos que lo dispararon, el responsable y su plazo
    Y notifica al Auditor Interno y a Gerencia Administrativa

  Escenario: El expediente de hallazgo conserva los datos concretos, no solo el criterio
    Cuando se crea el expediente de hallazgo de "OM-2026-0468"
    Entonces conserva "rendimiento observado 6.00 km/galón, esperado 12.00, desviación 50.0 % por debajo, 240 km, 40.0 galones"
    Y no se limita a registrar el identificador del criterio

  Escenario: La misión con hallazgo es terminal e inmutable igual que la cerrada limpia
    Dado que "OM-2026-0468" está en "CERRADA_CON_HALLAZGO"
    Cuando alguien intenta modificar un dato del expediente
    Entonces el sistema rechaza la modificación
    Y muestra "El expediente OM-2026-0468 está cerrado. Toda corrección posterior es un asiento reverso visible."

  Escenario: Resolver el hallazgo no cambia el estado de la misión
    Cuando el responsable cierra el expediente de hallazgo de "OM-2026-0468"
    Entonces el expediente de hallazgo queda resuelto
    Y la misión sigue en estado "CERRADA_CON_HALLAZGO"
    Y el sistema no reescribe la clasificación de cierre de la misión

  Escenario: El requerimiento del Auditor Interno obliga
    Dado que el Auditor Interno requiere el cierre con hallazgo de "OM-2026-0491" con su fundamento
    Cuando la Gerencia Administrativa intenta cerrar "OM-2026-0491" sin hallazgo
    Entonces el sistema rechaza el cierre limpio
    Y muestra "El Auditor Interno requirió el cierre con hallazgo el 03/10/2026 con fundamento registrado."

  Escenario: El Auditor Interno no produce actos de negocio
    Cuando el Auditor Interno intenta cerrar directamente "OM-2026-0491"
    Entonces el sistema rechaza la acción
    Y muestra "El Auditor Interno requiere y verifica; no cierra. El cierre es acto de Gerencia Administrativa."
    Y la consulta del Auditor Interno queda registrada

  Escenario: Una obligación de reintegro abierta no impide cerrar
    Dado una obligación de reintegro abierta de "L 350.00" a cargo de "Wilmer Cáceres"
    Cuando la Gerencia Administrativa cierra la misión
    Entonces la misión se cierra con hallazgo por fondo no devuelto ni comprobado al vencer el plazo
    Y la obligación sigue abierta con ciclo propio
    Y "Wilmer Cáceres" sigue bloqueado para recibir nueva asignación de fondo

  Escenario: Un reclamo de peaje pendiente ante la SAPP no impide cerrar
    Dado 3 discrepancias de clasificación con reclamo presentado y sin resolver
    Cuando la Gerencia Administrativa cierra la misión
    Entonces el sistema permite el cierre
    Y muestra "El reclamo de peaje sigue su curso en el expediente de M-18. Su resultado económico se registrará por asiento."
    Y el expediente de reclamo permanece abierto

  Escenario: Un criterio que no está en la lista no habilita el cierre con hallazgo
    Dado una situación incómoda que no corresponde a ningún criterio del catálogo vigente
    Cuando la Gerencia Administrativa intenta cerrar con hallazgo
    Entonces el sistema rechaza el cierre con hallazgo
    Y muestra "Ningún criterio del catálogo vigente se cumple. Cierre con hallazgo no procede: no es un cajón de sastre."
```

## Fuera de alcance

- El cierre limpio y el sellado del expediente — es [HU-093](HU-093-cerrar-la-mision-con-la-cadena-completa.md)
- El hallazgo posterior sobre misión ya cerrada — es [HU-095](HU-095-registrar-hallazgo-posterior-sobre-mision-cerrada.md)
- La deducción de responsabilidad administrativa: la instruye quien corresponde, fuera de SIGTI
- El ciclo del expediente de hallazgo en sí: pertenece a M-12 e M-14

## Notas y pendientes

- ⚠️ **Hallazgo `HB4-03` incorporado.** Si el reclamo de peaje bloqueara el cierre, la misión quedaría atrapada en `LIQUIDADA` durante meses sin salida por cierre con hallazgo, porque una discrepancia de clasificación **no figura entre los criterios de la lista cerrada**. Esta historia aplica la interpretación de [CU-16](../casos-de-uso/CU-16-cerrar-el-expediente-de-la-mision.md): el bloqueo recae sobre el cierre de la **discrepancia**, no sobre el de la misión. Queda dirigido a [`orden-de-mision.md`](../../03-arquitectura/estados/orden-de-mision.md) y a [`RN-92`](../../01-negocio/reglas/RN-92-reclamo-por-discrepancia-de-peaje.md) para que uno de los dos lo precise
- `[C]` **Umbrales de los criterios configurables** — insumos **#1** y **#19**
- `[C]` ¿Los hallazgos reiterados de un mismo motorista o vehículo **bloquean** o **advierten** antes de nuevas asignaciones? La recomendación es que adviertan: bloquear por un hallazgo no resuelto es sancionar antes de investigar — insumo **#1**
- `[C]` Informes previos de Auditoría Interna o del TSC sobre flota: cada hallazgo pasado describe algo que salió mal en la operación real — insumo **#19**
- `[V]` Que el hallazgo típico del TSC en flota es el incremento de consumo sin relación con el uso habitual — [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md)
