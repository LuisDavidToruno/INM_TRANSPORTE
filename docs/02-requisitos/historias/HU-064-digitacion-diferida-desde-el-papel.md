# HU-064 — Digitar la bitácora desde el papel días después, declarada como transcripción

| Campo | Valor |
|---|---|
| **Módulo** | M-16 Sincronización y Operación Desconectada · M-08 Ejecución y Bitácora |
| **Actor** | ACT-10 Encargado de Delegación |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — falta el plazo máximo de digitación diferida y desde cuándo corre el plazo de liquidación (insumo #32), el pronunciamiento de Auditoría Interna sobre si puede digitar quien después liquida esa misma misión (insumo #47) y los formatos en papel vigentes campo por campo (insumo #2) |

## Historia

**Como** Encargado de Delegación
**quiero** digitar la hoja de bitácora en papel dejando constancia de que yo la digité, de quién fue el autor del hecho y de la fecha que consta en el original
**para** que el expediente no presente como registro del día algo que se reconstruyó doce días después, y para que la falta de señal no se me impute a mí ni al motorista

## Contexto

La digitación diferida **no es un conflicto de sincronización: es un modo de captura** ([RN-47](../../01-negocio/reglas/RN-47-digitacion-diferida-desde-papel.md)). Tiene su propia constancia: quién digitó, quién es el autor del hecho —que no es el mismo—, el original fotografiado, y la fecha del hecho que consta en el papel.

**Lo que el papel no trae, no se inventa.** El odómetro intermedio que nadie anotó se registra como *no consignado en el original*; no se deduce restando. Un dato deducido entra al expediente indistinguible de uno leído, y sostiene después una conciliación que nunca ocurrió.

**El registro diferido es visible para siempre.** Un hecho registrado el mismo día y uno reconstruido doce días después no pueden pesar igual ante el auditor.

Y el motivo del diferimiento **se imputa a quien corresponde**: sin conectividad es condición de la delegación; sin dispositivo asignado es condición institucional; papel entregado a tiempo y digitado tarde es de la delegación. Ninguna de las dos primeras es falta del motorista. Sin esa distinción, el indicador castiga a quien opera donde no hay señal y el resultado es predecible: dejan de reportar.

## Reglas que la gobiernan

- [RN-47](../../01-negocio/reglas/RN-47-digitacion-diferida-desde-papel.md) — **Regla rectora**: la digitación diferida deja constancia de quién digitó y del original escaneado
- [RN-46](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md) — La fecha del hecho es la del papel; la de captura es la de la digitación, no editable
- [RN-89](../../01-negocio/reglas/RN-89-kilometraje-acumulado-invariante-del-expediente.md) — La continuidad del odómetro se evalúa sobre la serie ordenada por fecha del hecho
- [RN-45](../../01-negocio/reglas/RN-45-cero-sobrescritura-silenciosa.md) — Lo digitado que contradice lo constatado va a la cola, no sobrescribe
- [RN-80](../../01-negocio/reglas/RN-80-hoja-de-bitacora-impresa-con-folio.md) — Paridad exacta entre el papel y la pantalla de digitación
- [RN-82](../../01-negocio/reglas/RN-82-indicadores-de-calidad-de-la-programacion.md) — El diferimiento se acumula por causa tipificada y se atribuye al responsable correcto

## Casos especiales que la afectan

- [CE-09](../casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md) — Bitácora en papel digitada días después
- [CE-22](../casos-especiales/CE-22-odometro-inconsistente.md) — El odómetro que el papel no trae
- [CE-25](../casos-especiales/CE-25-comprobante-perdido-o-estacion-sin-factura.md) — La hoja de bitácora que se perdió

## Criterios de aceptación

```gherkin
# language: es
Característica: Digitación diferida de la bitácora desde el papel

  Antecedentes:
    Dada la Orden de Misión "OM-2026-0451" en estado "RETORNADA" con marca "BITACORA_PENDIENTE_DE_DIGITACION"
    Y una hoja de bitácora en papel con folio "CHO-2026-000401"
    Y un Encargado de Delegación "Ana Zelaya" que digita el "2026-05-28"

  Escenario: Se rechaza la digitación sin adjunto del original
    Cuando "Ana Zelaya" digita la bitácora del folio "CHO-2026-000401" sin adjuntar fotografía del original
    Entonces el sistema rechaza la digitación
    Y muestra "Adjunte la fotografía de la hoja de bitácora folio CHO-2026-000401. Sin el original, lo digitado no tiene respaldo ante el auditor."

  Escenario: Se rechaza la digitación sin declarar quién es el autor del hecho
    Cuando "Ana Zelaya" digita un arribo sin declarar quién lo ejecutó
    Entonces el sistema rechaza la digitación
    Y muestra "Declare quién ejecutó el hecho. Usted lo está digitando; el autor es otra persona."

  Escenario: Se rechaza deducir un odómetro que el papel no trae
    Dado un arribo a "Puesto Fronterizo El Amatillo" sin odómetro anotado en el original
    Cuando "Ana Zelaya" intenta calcular ese odómetro a partir de las lecturas anterior y posterior
    Entonces el sistema no ofrece ningún cálculo ni autocompletado
    Y registra el valor como "no consignado en el original"
    Y muestra "El original no trae este kilometraje. Se registra como no consignado; no lo deduzca."

  Escenario: La digitación queda declarada como diferida, para siempre
    Cuando "Ana Zelaya" digita la bitácora completa del folio "CHO-2026-000401" con los hechos ocurridos entre el "2026-05-12" y el "2026-05-16"
    Entonces el sistema registra cada hecho con su fecha del hecho tomada del papel
    Y registra la fecha de captura "2026-05-28" como no editable
    Y marca el modo de captura "digitación diferida de papel" en pantalla y en todo reporte
    Y muestra el desfase de "12 días" entre el hecho y su registro

  Escenario: El motivo del diferimiento se imputa a la causa correcta
    Cuando "Ana Zelaya" declara el motivo del diferimiento "la delegación no tiene cobertura de datos"
    Entonces el sistema imputa el diferimiento a "condición de la delegación"
    Y no lo imputa al motorista "José Martínez"
    Y el indicador de oportunidad de registro de "José Martínez" no se ve afectado

  Escenario: Lo digitado contradice lo constatado en el portón
    Dado un odómetro de retorno constatado de "93610" con fotografía del tablero
    Cuando "Ana Zelaya" digita del papel un odómetro de retorno de "93061"
    Entonces el sistema conserva ambas versiones íntegras
    Y abre un conflicto para resolución humana con ambas versiones y sus adjuntos
    Y muestra "El papel dice 93,061 y en el portón se leyó 93,610 con foto del tablero. Alguien tiene que decidir cuál describe lo que pasó."
    Y ninguna versión sobrescribe a la otra

  Escenario: Se digita una misión anterior a otras ya registradas
    Dada una misión del "2026-05-03" al "2026-05-07" digitada después de la del "2026-05-12" al "2026-05-16"
    Cuando "Ana Zelaya" completa la digitación de la misión anterior
    Entonces el sistema evalúa la continuidad del odómetro sobre la serie ordenada por fecha del hecho
    Y reabre la validación de todos los registros posteriores
    Y las incoherencias que aparezcan entran a la cola de resolución humana, sin corregirse solas

  Escenario: La hoja de bitácora se perdió y no se puede digitar
    Dado que la hoja folio "CHO-2026-000401" se extravió en la delegación
    Cuando "Ana Zelaya" declara la pérdida del original con causa tipificada
    Entonces el sistema reconstruye lo que exista con descargo alternativo
    Y declara lo no recuperado como "perdido", nunca como vacío
    Y la misión solo puede cerrarse con hallazgo
```

## Fuera de alcance

- La resolución del conflicto abierto entre papel y tablero — es [HU-068](HU-068-cola-de-conflictos-dos-versiones-lado-a-lado.md)
- La emisión de la hoja de bitácora en el despacho — es [HU-056](HU-056-hoja-de-bitacora-impresa-como-respaldo.md)
- La liquidación posterior de la misión — es de M-13

## Notas y pendientes

- `[C]` Plazo máximo de digitación diferida y desde cuándo corre el plazo de liquidación — insumo #32 y [CE-09](../casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md) D-1 y D-2
- `[C]` ¿Puede digitar quien después liquida esa misma misión? — insumo #47, a Auditoría Interna. Si la respuesta es no, se agrega una incompatibilidad nueva a la matriz de la [tabla de actores](../../01-negocio/actores-y-roles.md)
- `[C]` Formatos en papel vigentes, campo por campo, para asegurar la paridad — insumo #2
