# HU-086 — No producir discrepancia de peaje sobre un punto cuya tarifa no está verificada

| Campo | Valor |
|---|---|
| **Módulo** | M-18 Peajes · M-02 Catálogos Maestros |
| **Actor** | ACT-01 Administrador del Sistema (carga) · ACT-08 Gerencia Administrativa (pone en vigencia) · ACT-04 Jefe de Transporte (consume) |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador |

## Historia

**Como** Jefe de Transporte
**quiero** que el detector de discrepancias de peaje se declare **no concluyente** sobre todo punto cuya tarifa no esté verificada con fuente y fecha
**para** que el sistema no marque como cobro indebido cada cruce del país y el primer reclamo institucional no se caiga solo

## Contexto

**Un detector de discrepancias montado sobre una tabla no verificada es peor que no tener detector.** Marcaría cada paso por caseta como cobro indebido; el Jefe de Transporte vería cientos de discrepancias falsas, dejaría de mirarlas, y la primera verdadera pasaría desapercibida. Tres falsas seguidas hacen que nadie vuelva a mirar las verdaderas.

Y hay una contradicción abierta que no se puede resolver desde adentro: la SIT confirmó el 28/02/2026 que **no habrá incremento para ninguna categoría** `[V]`, corroborado por tres medios; un agregador comercial publica tarifas distintas desde marzo de 2026. No hay ninguna fuente de abril a agosto de 2026 que confirme qué se cobra hoy.

Esta historia es la que permite que M-18 se construya sin esperar el insumo, y que se encienda solo cuando el dato exista.

## Reglas que la gobiernan

- [RN-34](../../01-negocio/reglas/RN-34-tarifa-de-peaje-por-punto-categoria-vigencia.md) — La tabla registra fuente y fecha de verificación de cada tarifa cargada
- [RN-39](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md) — Doble control: la carga ACT-01 con respaldo documental, la pone en vigencia ACT-08
- [RN-36](../../01-negocio/reglas/RN-36-discrepancia-de-clasificacion-en-caseta.md) — La discrepancia se marca sobre una base tarifaria confiable
- [RN-40](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md) — Todo cálculo usa la tabla vigente a la fecha del hecho
- [RN-42](../../01-negocio/reglas/RN-42-correccion-retroactiva-con-asiento-de-diferencia.md) — Una tarifa corregida retroactivamente recalcula con asiento de diferencia, sin sobrescribir
- [RN-35](../../01-negocio/reglas/RN-35-estimacion-de-peajes-antes-de-aprobar.md) — `estimacion_peaje_obligatoria_para_aprobar` está **apagado** mientras el insumo #21 siga abierto

## Casos especiales que la afectan

- [CE-24](../casos-especiales/CE-24-cobro-en-categoria-de-peaje-equivocada.md) — Riesgo declarado en su punto de tarifas no verificadas
- [CE-27](../casos-especiales/CE-27-cierre-de-ejercicio-fiscal-con-hallazgo-abierto.md) — Un ajuste retroactivo de tarifa que cruza el corte de ejercicio

## Criterios de aceptación

```gherkin
# language: es
Característica: Tratamiento de tarifas de peaje no verificadas

  Antecedentes:
    Dado un punto de peaje "Zambrano" con tarifa "L 22.00" para "Liviano/Turismo", fuente "SAPP", fecha de verificación "2026-08-06", estado "verificada"
    Y un punto de peaje "San Manuel" con tarifa "L 25.00" cargada sin fuente oficial, estado "no verificada"

  Escenario: No se produce discrepancia sobre un punto con tarifa no verificada
    Cuando el motorista registra un paso por "San Manuel" con monto cobrado "L 40.00"
    Entonces el sistema registra el paso
    Y no marca discrepancia de clasificación
    Y presenta el resultado como "no concluyente: la tarifa del punto San Manuel no está verificada"

  Escenario: Se rechaza cargar una tarifa sin fuente ni fecha de verificación
    Cuando el Administrador carga una tarifa de "L 31.00" para "Liviano/Turismo" en "Zambrano" sin fuente ni fecha de verificación
    Entonces el sistema rechaza la carga
    Y muestra "Toda tarifa exige fuente y fecha de verificación. Sin ellas queda como no verificada y no sostiene ninguna discrepancia."

  Escenario: El Administrador no puede poner en vigencia lo que él mismo cargó
    Cuando el Administrador intenta poner en vigencia la tarifa que acaba de cargar
    Entonces el sistema rechaza la acción
    Y muestra "La puesta en vigencia de un parámetro normativo corresponde a Gerencia Administrativa. Quien carga no pone en vigencia."

  Escenario: Una tarifa con más de 12 meses sin revisar se alerta
    Dado una tarifa con fecha de verificación "2025-08-06"
    Cuando el sistema evalúa el catálogo el "2026-09-01"
    Entonces alerta al puesto responsable
    Y muestra "La tarifa del punto Zambrano lleva 12 meses sin revisar. La tarifa de peaje se revisa cada enero."

  Escenario: La tarifa no se deriva por fórmula de número de ejes
    Cuando el Administrador intenta cargar una regla de cálculo "tarifa = 45 x numero_ejes"
    Entonces el sistema rechaza la carga
    Y muestra "La tarifa es una tabla publicada por punto, categoría y vigencia. No se deriva de una fórmula por eje: un liviano y un vehículo de 2 ejes tienen ambos dos ejes y pagan distinto."

  Escenario: Corrección retroactiva de una tarifa ya aplicada
    Dado 34 misiones liquidadas con la tarifa "L 22.00" entre el "2026-03-01" y el "2026-08-31"
    Cuando la Gerencia Administrativa pone en vigencia una tarifa confirmada de "L 31.00" con vigencia desde el "2026-03-01"
    Entonces el sistema recalcula las 34 misiones afectadas
    Y deja asiento de la diferencia por cada una, con valor anterior y valor nuevo
    Y no sobrescribe ningún valor histórico

  Escenario: La estimación previa no bloquea la aprobación mientras el insumo esté abierto
    Dado el parámetro "estimacion_peaje_obligatoria_para_aprobar" en "apagado"
    Cuando la jefatura inmediata autoriza una solicitud cuya ruta atraviesa un punto con tarifa no verificada
    Entonces el sistema permite autorizar
    Y muestra el estimado marcado como referencial, con el detalle de qué puntos no están verificados
```

## Fuera de alcance

- El registro del paso por caseta — es [HU-085](HU-085-registrar-el-paso-por-caseta-y-marcar-discrepancia.md)
- La conciliación de peajes en la liquidación — es [HU-090](HU-090-conciliar-peajes-punto-por-punto.md)
- La gestión del reclamo ante la SAPP: SIGTI arma el expediente, la institución lo presenta
- La negociación tarifaria entre la SIT y COVI-H: dato externo, se registra su resultado

## Notas y pendientes

- 🔴 `[C]` **bloqueante — insumo #21: tarifa de peaje efectivamente vigente.** Condiciona toda la detección de discrepancias de M-18. **No se carga ninguna tarifa hasta confirmarla con COVI-H o la SAPP.** [NRM-10](../../01-negocio/normativa/NRM-10-peajes.md) §4 y §10
- `[V]` Comunicado de la SIT del 28/02/2026: no habrá incremento para ninguna categoría, corroborado por tres medios
- `[P]` La tarifa publicada por la SAPP (L 22 / 90 / 134 / 179 / 224 / 269 …) está `[V]` como publicación; que sea la efectivamente cobrada hoy es `[C]`. **No se eleva el nivel**
- `[C]` PDF de tarifas de la SAPP: escaneo sin capa de texto, requiere OCR para contrastar contra el HTML — insumo **#21**
- `[C]` Si hoy se cobra peaje en el Corredor Turístico (caseta de San Manuel, Cortés) — [NRM-10](../../01-negocio/normativa/NRM-10-peajes.md) §3
- El catálogo de puntos y de categorías es **tabla abierta y ampliable en producción sin cambio de código**: hay proyectos de peaje en cartera
