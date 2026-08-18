# HU-029 — Reservar el folio al programar, sin habilitar todavía un documento válido para circulación

| Campo | Valor |
|---|---|
| **Módulo** | M-07 Programación y Despacho · M-15 Formatos Oficiales e Impresión |
| **Actor** | ACT-04 Jefe de Transporte · ACT-10 Encargado de Delegación en su ámbito |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Refinada |
| **Deriva de** | [CU-04](../casos-de-uso/CU-04-programar-mision-asignar-vehiculo-y-motorista.md) paso 11 y A3 · [CU-05](../casos-de-uso/CU-05-emitir-orden-de-mision-y-documentos.md) paso 1 · `EF-02` · `INV-15` |

## Historia

**Como** Jefe de Transporte
**quiero** que al programar la misión se reserve el folio de la Orden de Misión del rango de mi delegación, y que hasta el despacho solo pueda imprimir una vista previa marcada como no válida para circulación
**para** poder armar y revisar el contenido de la Orden con anticipación sin que circule por carretera un documento que todavía no pasó la revalidación del despacho

## Contexto

El folio tiene dos momentos distintos y confundirlos rompe la integridad del correlativo: **se reserva al programar y se consume al despachar**. La reserva es lo que hace posible la emisión sin conectividad en las delegaciones, porque el rango ya está apartado localmente ([`RN-44`](../../01-negocio/reglas/RN-44-identificadores-y-folios-en-el-cliente.md)). El consumo es el acto documental, y ocurre dentro de `T-12` ([HU-031](HU-031-consumo-del-folio-y-emision-del-juego-documental.md)).

Entre uno y otro pueden pasar días, y en esos días la licencia puede vencer o el vehículo puede entrar a taller. Por eso en `PROGRAMADA` **no existe la Orden de Misión como documento válido**: existe una vista previa, y tiene que ser imposible confundirla con el documento real en un retén.

Un folio reservado que no llega a consumirse **se anula: no se recicla ni se devuelve al rango**. Dos misiones distintas con el mismo folio destruyen la conciliación.

## Reglas que la gobiernan

- [RN-44](../../01-negocio/reglas/RN-44-identificadores-y-folios-en-el-cliente.md) — Los folios se asignan de rangos por delegación
- [RN-25](../../01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md) — Todo documento de control en carretera lleva folio único, QR verificable, firma, sello y huella
- [RN-04](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md) — El folio reservado que no se consume se anula con asiento, no desaparece
- [RN-17](../../01-negocio/reglas/RN-17-alertas-de-vencimiento-documental.md) — El agotamiento del rango se alerta con anticipación configurable

## Casos especiales que la afectan

- [CE-09](../casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md) — En delegación sin red, el rango local es lo único que permite emitir
- [CE-12](../casos-especiales/CE-12-dos-solicitudes-compiten-por-el-mismo-vehiculo.md) — La misión desplazada pierde su folio reservado, que queda anulado

## Criterios de aceptación

```gherkin
# language: es
Característica: Reserva del folio y vista previa no válida para circulación

  Antecedentes:
    Dada la Delegación Choluteca con rango de folios de Orden de Misión
      de "OM-CHO-2026-0100" a "OM-CHO-2026-0199"
    Y que el último folio reservado de ese rango es "OM-CHO-2026-0142"
    Y un umbral de alerta por consumo del rango del "85" por ciento

  Escenario: Se rechaza imprimir la Orden como documento válido en PROGRAMADA
    Dada una Orden de Misión "OM-CHO-2026-0143" en estado "PROGRAMADA"
    Cuando el Jefe de Transporte intenta emitir la Orden de Misión como documento válido
    Entonces el sistema rechaza la emisión
    Y muestra "La Orden de Misión se emite al despachar. En estado PROGRAMADA solo está disponible la vista previa, marcada como no válida para circulación."

  Escenario: La vista previa impresa no se puede confundir con el documento válido
    Dada una Orden de Misión "OM-CHO-2026-0143" en estado "PROGRAMADA"
    Cuando el Jefe de Transporte imprime la vista previa
    Entonces el impreso lleva la marca de agua "VISTA PREVIA — NO VÁLIDA PARA CIRCULACIÓN"
    Y no lleva código QR resoluble
    Y muestra el folio como "folio reservado OM-CHO-2026-0143 — no emitido"
    Y la impresión queda registrada con autor y momento

  Escenario: El folio se reserva al programar, sin consumirse
    Dada una solicitud "SOL-2026-0360" de la Delegación Choluteca
    Cuando el Jefe de Transporte programa la misión con vehículo y motorista verificados
    Entonces el sistema reserva el folio "OM-CHO-2026-0143"
    Y el folio queda en estado "RESERVADO"
    Y el rango de la delegación no vuelve a ofrecer ese folio a ninguna otra misión

  Escenario: El folio reservado se anula al desprogramar y no se recicla
    Dada una Orden de Misión "OM-CHO-2026-0143" en estado "PROGRAMADA" con folio reservado
    Cuando el Jefe de Transporte desprograma la misión con motivo "vehículo a taller"
    Entonces el folio "OM-CHO-2026-0143" pasa al estado "ANULADO" con su motivo y autor
    Y al reprogramar la misma solicitud se reserva el folio siguiente "OM-CHO-2026-0144"
    Y el folio "OM-CHO-2026-0143" no vuelve a estar disponible

  Escenario: Se alerta el consumo del rango antes de agotarlo
    Dado que se han reservado o consumido "85" folios del rango de "100"
    Cuando el Jefe de Transporte reserva un folio nuevo
    Entonces el sistema muestra la advertencia "La Delegación Choluteca ha consumido el 85% de su rango de folios: quedan 15. Gestione la ampliación del rango."
    Y notifica al Administrador del Sistema

  Escenario: Se rechaza la reserva con el rango agotado
    Dado que se han reservado o consumido los "100" folios del rango
    Cuando el Jefe de Transporte intenta programar una misión en la Delegación Choluteca
    Entonces el sistema rechaza la programación
    Y muestra "El rango de folios de la Delegación Choluteca está agotado (OM-CHO-2026-0100 a OM-CHO-2026-0199). Solicite la ampliación del rango antes de programar."
```

## Fuera de alcance

- El consumo del folio y la emisión del juego documental — es [HU-031](HU-031-consumo-del-folio-y-emision-del-juego-documental.md)
- La reimpresión con el mismo folio — es [HU-036](HU-036-reimpresion-con-el-mismo-folio.md)
- El folio propio de la constancia de asignación de fondo de combustible — es [HU-041](HU-041-emision-y-entrega-del-fondo-de-combustible.md)
- El procedimiento administrativo de asignación de rangos a cada delegación — es de M-01 y de despliegue

## Notas y pendientes

- `[C]` **Procedimiento de ampliación de rango de folios sin conectividad** — insumo #1. Mientras no exista, una delegación con el rango agotado y sin red no puede emitir, y eso es un **requisito de despliegue**, no una excepción a la regla.
- `[C]` **Qué pasa con la vista previa impresa** no está gobernado por ninguna regla vigente (hallazgo `HCU-10`). Esta historia adopta la recomendación —sin folio consumido, sin QR resoluble, con marca de agua y con registro de impresión— y **queda escalada al PO** para que se convierta en regla o se corrija.
- `[C]` Si el correlativo institucional del vehículo y el rango de folios son **únicos por institución o compuestos por delegación** — insumo #34.
