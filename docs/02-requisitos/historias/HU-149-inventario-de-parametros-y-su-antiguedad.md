# HU-149 — Ver el inventario completo de parámetros: cuáles faltan, cuáles esperan aprobación y cuáles llevan sin revisar demasiado tiempo

| Campo | Valor |
|---|---|
| **Módulo** | M-02 Catálogos Maestros · M-14 Reportes, Indicadores y Auditoría |
| **Actor** | ACT-12 Auditor Interno · ACT-08 Gerencia Administrativa |
| **Prioridad** | Media |
| **Sprint** | sin asignar |
| **Estado** | Borrador — `plazo_revision_parametro` no tiene valor confirmado |

## Historia

**Como** Auditor Interno
**quiero** un inventario que liste todos los parámetros del sistema con su valor vigente, quién lo cargó, quién lo aprobó, qué regla lo usa, cuándo se verificó por última vez y cuáles están vacíos
**para** poder revisar en una pantalla si la base de cálculo de la institución está sostenida en fuentes vigentes, en lugar de descubrirlo expediente por expediente

## Contexto

`RN-39` lo exige en su comportamiento esperado 4: *"Existe un **inventario de parámetros** consultable: cuáles hay, qué regla los usa, quién los cargó y quién los aprobó, cuáles están pendientes de aprobación, cuándo se verificaron por última vez, y cuáles llevan sin revisar más que el parámetro `plazo_revision_parametro`."*

Y `RNF-19` lo convierte en criterio de verificación del producto: *"se recorren todas las reglas `RN-xx` y todos los `RNF-xx` extrayendo cada valor configurable citado, y se verifica que aparece en el catálogo de parámetros. **Un valor citado en una regla y ausente del catálogo está cableado en alguna parte.**"*

El inventario es, en la práctica, **la lista de todo lo que el sistema puede calcular mal**. Que un parámetro lleve dos años sin verificarse no lo invalida, pero es exactamente lo que Auditoría Interna necesita ver antes de que lo vea el TSC.

Y el propio plazo de revisión **es un parámetro**: escribirlo dentro de la regla sería el mismo defecto que la regla prohíbe.

## Reglas que la gobiernan

- [RN-39](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md) — El inventario de parámetros y el histórico de cambios son objeto de auditoría de primera clase
- [RN-94](../../01-negocio/reglas/RN-94-fecha-de-corte-de-conocimiento-en-todo-reporte.md) — El inventario declara su fecha de corte y es reproducible
- [RN-40](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md) — El inventario permite consultar qué valor tenía un parámetro en cualquier fecha
- [RN-04](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md) — El histórico no se depura ni se compacta
- [RN-52](../../01-negocio/reglas/RN-52-registro-de-consultas-a-manifiestos.md) — Las consultas del auditor también quedan registradas

## Casos especiales que la afectan

- [CE-28](../casos-especiales/CE-28-hallazgo-posterior-sobre-mision-cerrada.md) — El hallazgo posterior suele nacer de un parámetro desactualizado
- [CE-27](../casos-especiales/CE-27-cierre-de-ejercicio-fiscal-con-hallazgo-abierto.md) — El corte de ejercicio es el momento natural de revisar el inventario

## Criterios de aceptación

```gherkin
# language: es
Característica: Inventario de parámetros del sistema

  Antecedentes:
    Dado un parámetro "plazo_revision_parametro" de "12" meses vigente y aprobado
    Y un parámetro "tarifa_peaje" del punto "Zambrano" verificado por última vez el "2025-04-10"
    Y un parámetro "matriz_licencia_vehiculo" sin ninguna vigencia cargada
    Y un parámetro "umbral_desviacion_consumo" con una carga pendiente de aprobación desde hace 6 días
    Y la fecha del sistema del "2026-09-20"

  Escenario: El inventario señala los parámetros vacíos como bloqueantes
    Cuando el Auditor Interno abre el inventario de parámetros
    Entonces "matriz_licencia_vehiculo" figura como "SIN VALOR CARGADO"
    Y muestra "Bloquea: RN-09 matriz licencia↔vehículo. Ninguna asignación de motorista se puede verificar. Insumo #20."
    Y no muestra ningún valor por defecto ni estimado

  Escenario: El inventario señala los parámetros vencidos de revisión
    Cuando el Auditor Interno abre el inventario
    Entonces "tarifa_peaje" del punto "Zambrano" figura como "SIN REVISAR HACE 17 MESES"
    Y muestra "Última verificación: 10/04/2025. El plazo configurado es de 12 meses."
    Y el parámetro sigue vigente y aplicándose

  Escenario: El inventario señala lo pendiente de aprobación con su antigüedad
    Cuando la Gerencia Administrativa abre el inventario
    Entonces "umbral_desviacion_consumo" figura como "PENDIENTE DE APROBACIÓN HACE 6 DÍAS"
    Y muestra el valor vigente que sigue rigiendo y el valor cargado que no se aplica

  Escenario: Cada parámetro muestra qué regla lo usa
    Cuando el Auditor Interno abre el detalle de "umbral_desviacion_consumo"
    Entonces ve que lo usan "RN-30 conciliación galonaje–kilometraje" y "RN-88 saldo proyectado del fondo"
    Y ve cuántos cálculos lo han usado en el período consultado

  Escenario: El inventario muestra carga y aprobación por separado
    Cuando el Auditor Interno abre el detalle de "tarifa_peaje" del punto "Zambrano"
    Entonces ve, por cada vigencia, quién la cargó y cuándo, y quién la aprobó y cuándo
    Y ve el respaldo documental adjunto de cada una
    Y ve que carga y aprobación son de personas distintas

  Escenario: El histórico de un parámetro no se puede depurar
    Cuando el Administrador del Sistema intenta eliminar las vigencias anteriores a 2026 de "tarifa_peaje"
    Entonces el sistema rechaza la operación
    Y muestra "El histórico de parámetros es objeto de auditoría y no se depura. Sin él, ningún reporte histórico es reproducible."

  Escenario: El inventario se exporta como paquete de evidencia
    Cuando el Auditor Interno exporta el inventario con corte al "2026-09-20"
    Entonces el sistema genera el paquete con índice, sello de tiempo y anexos
    Y el paquete declara su fecha de corte de conocimiento
    Y la exportación queda registrada como consulta del auditor

  Escenario: Todo valor configurable citado en una regla aparece en el inventario
    Cuando se ejecuta la verificación de cobertura del inventario
    Entonces todo umbral, plazo, tarifa y categoría citado en cualquier RN-xx o RNF-xx figura en él
    Y los que no figuran se reportan como posible valor cableado
```

## Fuera de alcance

- La carga y la aprobación de parámetros — es [HU-144](HU-144-cargar-parametro-normativo-con-vigencia.md) y [HU-145](HU-145-aprobar-la-puesta-en-vigencia-doble-control.md)
- El bloqueo de la operación por parámetro faltante — es [HU-147](HU-147-resolver-el-parametro-a-la-fecha-del-hecho.md) y [HU-150](HU-150-poner-en-marcha-la-institucion-con-parametros-vacios.md)
- El formato del paquete de evidencia — es [`RNF-18`](../no-funcionales/RNF-18-paquetes-de-evidencia-para-auditoria.md)

## Notas y pendientes

- `[C]` **`plazo_revision_parametro`** — el valor de referencia de 12 meses lo propone `RN-39` marcado `[C]`, **por ámbito y tipo de parámetro**. [NRM-10](../../01-negocio/normativa/NRM-10-peajes.md) exige revisión periódica para tarifas; el plazo concreto lo fija la institución
- `[C]` Si Auditoría Interna quiere el inventario como pantalla permanente o como reporte por campaña — el patrón de uso de `ACT-12` es *"por campaña, con picos altos y períodos sin uso"* `[I]`
- El inventario es también el artefacto que verifica [`RNF-19`](../no-funcionales/RNF-19-configurabilidad-multi-institucion.md): sin él, la métrica *"cobertura del catálogo de parámetros"* no se puede medir
