# HU-044 — Revertir una misión `DESPACHADA` que no salió: devolución íntegra o liquidación si hubo consumo

| Campo | Valor |
|---|---|
| **Módulo** | M-07 Programación y Despacho · M-09 Combustible · M-13 Liquidación y Cierre |
| **Actor** | ACT-05 Encargado de Despacho · ACT-07 Encargado de Combustible · ACT-06 Motorista · ACT-04 Jefe de Transporte |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Refinada |
| **Deriva de** | [CU-07](../casos-de-uso/CU-07-sustituir-vehiculo-o-motorista.md) A4 · [CU-05](../casos-de-uso/CU-05-emitir-orden-de-mision-y-documentos.md) E6 · [CU-06](../casos-de-uso/CU-06-despachar-y-registrar-salida.md) E6 · `T-15`, `T-16` · `EF-06` |

## Historia

**Como** Encargado de Despacho
**quiero** que cuando una misión ya despachada se caiga antes de salir, el sistema exija la devolución íntegra de lo entregado —fondo, custodia y documentos impresos— o, si hubo cualquier consumo, me obligue a liquidarla
**para** que ninguna misión desaparezca del expediente con dinero público entregado y sin descargo

## Contexto

Entre `DESPACHADA` y `EN_RUTA` hay bienes y dinero entregados sin ejecución que los justifique. Si la misión se cae ahí —el funcionario canceló la reunión, el vehículo no arrancó, cambió la prioridad— **no se puede simplemente cambiar de vehículo**: hay folio consumido, documentos emitidos y fondo entregado.

Hay exactamente dos caminos y la frontera entre ellos es una sola pregunta: **¿hubo algún consumo?**

- **Ninguno**: devolución íntegra con actas, los folios pasan a `ANULADO` y no se reciclan, y la necesidad se vuelve a programar con **folio nuevo**.
- **Alguno, aunque sea parcial** —se llenó el tanque la tarde anterior—: la anulación **no está disponible**. Hubo movimiento de fondos públicos y tiene que liquidarse, aunque el kilometraje sea cero.

Y mientras la devolución no esté completa, **la misión sigue en `DESPACHADA`** con la marca de anulación en trámite y la lista de pendientes visible. No se inventa un estado intermedio para que el tablero se vea limpio.

## Reglas que la gobiernan

- [RN-04](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md) — Toda anulación es asiento reverso con motivo y autor; nada se borra
- [RN-05](../../01-negocio/reglas/RN-05-registro-cerrado-no-se-edita.md) — Los documentos emitidos no se editan
- [RN-06](../../01-negocio/reglas/RN-06-transiciones-de-estado-de-la-orden.md) — Cada transición registra actor, rol, momento y motivo
- [RN-22](../../01-negocio/reglas/RN-22-custodia-del-vehiculo.md) — La devolución de la custodia consta en acta con odómetro
- [RN-27](../../01-negocio/reglas/RN-27-asignacion-de-combustible-con-folio.md) · [RN-29](../../01-negocio/reglas/RN-29-liquidacion-de-combustible.md) — La asignación entregada se devuelve con acta o se liquida
- [RN-25](../../01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md) — La anulación se refleja de inmediato en la verificación pública
- [RN-14](../../01-negocio/reglas/RN-14-sustitucion-de-motorista.md) — La misión nueva revalida íntegramente al recurso entrante

## Casos especiales que la afectan

- [CE-20](../casos-especiales/CE-20-mision-cancelada-con-combustible-ya-entregado.md) — **Caso rector**: misión cancelada con el combustible ya entregado
- [CE-26](../casos-especiales/CE-26-sobrante-o-faltante-al-liquidar.md) — La devolución incompleta se convierte en obligación de reintegro
- [CE-16](../casos-especiales/CE-16-vehiculo-a-taller-con-misiones-programadas.md) — La falla del vehículo en el predio es la causa más frecuente

## Criterios de aceptación

```gherkin
# language: es
Característica: Reversión de una misión despachada que no salió

  Antecedentes:
    Dada una Orden de Misión "OM-2026-0451" en estado "DESPACHADA"
    Y un folio consumido "OM-2026-0451" y un salvoconducto emitido "SC-2026-0087"
    Y una asignación de fondo "AC-2026-0233" en estado "ENTREGADA" por "4000.00" lempiras
    Y una custodia de misión registrada a nombre del motorista "José Martínez"
    Y que el vehículo no ha registrado salida

  Escenario: Se rechaza cambiar de vehículo estando DESPACHADA
    Cuando el Jefe de Transporte intenta sustituir el vehículo de "OM-2026-0451"
    Entonces el sistema rechaza la sustitución
    Y muestra "La misión está DESPACHADA: hay folio consumido, documentos emitidos y fondo entregado. Revierta primero con la devolución íntegra de lo entregado."

  Escenario: Se rechaza la anulación mientras la devolución esté incompleta
    Dado que el motorista devolvió el fondo pero no los documentos impresos
    Cuando el Encargado de Despacho intenta completar la anulación
    Entonces el sistema rechaza el cierre de la anulación
    Y muestra "Pendientes de devolución: documentos impresos (Orden de Misión OM-2026-0451, salvoconducto SC-2026-0087)."
    Y la misión permanece en estado "DESPACHADA" con la marca "anulación en trámite"
    Y la lista de pendientes queda visible en el expediente

  Escenario: Se rechaza la anulación cuando hubo consumo parcial del fondo
    Dado que el motorista abasteció "15.0" galones por "1200.00" lempiras la tarde anterior
    Cuando el Encargado de Despacho intenta anular la misión con devolución íntegra
    Entonces el sistema rechaza la anulación
    Y muestra "Hubo consumo de 1,200.00 lempiras contra la asignación AC-2026-0233. La misión no se anula: debe liquidarse aunque su kilometraje sea cero."
    Y ofrece el camino de misión no ejecutada con consumo

  Escenario: Devolución íntegra completa y anulación de todos los folios
    Dado que el motorista devolvió "4000.00" lempiras con acta firmada por él
      y por la Encargada de Combustible
    Y que devolvió la custodia con acta y odómetro "84520" km, coincidente con el de entrega
    Y que devolvió físicamente la Orden de Misión y el salvoconducto impresos
    Cuando el Encargado de Despacho completa la anulación con motivo
      "cancelación de la actividad por la dependencia solicitante"
    Entonces la asignación "AC-2026-0233" se revierte con asiento reverso, con motivo y autor
    Y los folios "OM-2026-0451" y "SC-2026-0087" pasan al estado "ANULADO" y no se reciclan
    Y la verificación pública de "SC-2026-0087" devuelve estado "ANULADO" de inmediato
    Y el vehículo vuelve al estado operativo "DISPONIBLE"

  Escenario: La misión con consumo se liquida con kilometraje cero
    Dado que hubo consumo de "1200.00" lempiras contra la asignación
    Cuando el Encargado de Despacho registra la misión como no ejecutada con consumo
    Entonces la Orden de Misión pasa al estado "RETORNADA"
    Y el kilometraje recorrido registrado es "0" km
    Y la misión entra a liquidación con el consumo y el saldo por devolver
    Y no se emite ninguna anulación de la asignación

  Escenario: La necesidad se atiende con una misión nueva y folio nuevo
    Dado que "OM-2026-0451" quedó anulada con devolución íntegra
    Cuando el Jefe de Transporte programa la misma necesidad con otro vehículo
    Entonces se crea una Orden de Misión nueva con folio nuevo
    Y la misión nueva queda vinculada a la anulada
    Y el recurso entrante se revalida íntegramente

  Escenario: Se rechaza la devolución de custodia con odómetro fuera de tolerancia sin justificación
    Dado un odómetro de entrega de "84520" km y una tolerancia de "5" km
    Cuando el motorista devuelve la custodia declarando un odómetro de "84590" km
    Entonces el sistema exige justificación del recorrido de "70" km
    Y muestra "El odómetro de devolución (84,590) excede en 70 km el de entrega (84,520). Declare el motivo: hubo movimiento del vehículo."
```

## Fuera de alcance

- La liquidación completa de la misión no ejecutada con consumo — es de M-13
- La obligación de reintegro por saldo no devuelto — es de M-13 y [`RN-86`](../../01-negocio/reglas/RN-86-obligacion-de-reintegro-por-saldo-no-devuelto.md)
- La interrupción con la misión ya `EN_RUTA` — es del caso de uso de interrupción en ruta
- La sustitución antes del despacho — es [HU-043](HU-043-sustituir-vehiculo-o-motorista-en-programada.md)

## Notas y pendientes

- `[C]` **¿Exige la institución la devolución física de los documentos impresos o admite constancia de destrucción con acta?** — insumo #1. La historia implementa **ambas** como alternativas configurables, porque la lógica no cambia.
- `[C]` **Tolerancia de odómetro en la devolución de custodia** — insumo #32.
- `[C]` **Plazo de devolución del saldo** cuando la misión no se ejecutó — insumos #7 y #37.
