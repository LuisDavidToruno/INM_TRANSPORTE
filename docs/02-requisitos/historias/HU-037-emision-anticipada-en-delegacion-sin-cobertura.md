# HU-037 — Emitir e imprimir los documentos en la delegación, sin conectividad, antes de salir a zona sin cobertura

| Campo | Valor |
|---|---|
| **Módulo** | M-15 Formatos Oficiales e Impresión · M-16 Sincronización y Operación Desconectada |
| **Actor** | ACT-10 Encargado de Delegación |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | **Borrador — bloqueada por decisión de producto pendiente** |
| **Deriva de** | [CU-05](../casos-de-uso/CU-05-emitir-orden-de-mision-y-documentos.md) A1 y nota `HCU-09` · [CU-06](../casos-de-uso/CU-06-despachar-y-registrar-salida.md) A2 · `T-12` · `RN-44` |

## Historia

**Como** Encargado de Delegación
**quiero** ejecutar el despacho e imprimir el juego documental en mi delegación aunque no tenga conectividad, tomando el folio del rango local
**para** que la misión salga con papel válido hacia zonas donde no hay señal, sin depender de que la sede esté en línea a las cinco de la mañana

## Contexto

[NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md) `[V]` exige poder emitir documentos por adelantado para las delegaciones que salen a zona sin cobertura. Por eso el folio se **reserva** al programar y el rango vive localmente.

Pero "anticipada" admite dos lecturas y **solo una es compatible con el modelo vigente**:

1. **Ejecutar el despacho sin conectividad**, con folio del rango local, e imprimir en la delegación antes de salir. El folio se consume, la revalidación ocurre, el paquete normativo se congela contra las tablas que el dispositivo tenga sincronizadas y **esa condición se imprime en el documento**. Esta es la lectura que adopta la historia.
2. **Imprimir un documento válido días antes, con la misión aún `PROGRAMADA`.** Eso `INV-15` y la [máquina de estados](../../03-arquitectura/estados/orden-de-mision.md) no lo permiten, porque implicaría emitir un documento oficial **antes de la revalidación del despacho** — y esa revalidación es la que detecta la licencia que venció en el ínterin.

Si lo que la institución necesita es lo segundo, **hace falta una decisión de producto explícita** (`HB3-14`): no se resuelve en esta historia.

## Reglas que la gobiernan

- [RN-44](../../01-negocio/reglas/RN-44-identificadores-y-folios-en-el-cliente.md) — Folios de rangos por delegación: es lo que hace posible emitir sin red
- [RN-43](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md) — La captura se completa sin ninguna conectividad y nunca se pierde
- [RN-50](../../01-negocio/reglas/RN-50-degradacion-por-sincronizacion-detenida.md) — La antigüedad de los datos con que se emitió se imprime en el documento
- [RN-41](../../01-negocio/reglas/RN-41-congelamiento-del-valor-al-autorizar.md) — El paquete normativo se congela con las tablas disponibles, identificadas
- [RN-25](../../01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md) — El documento emitido sin red cumple los mismos requisitos de folio, QR y huella
- [RN-45](../../01-negocio/reglas/RN-45-cero-sobrescritura-silenciosa.md) — Al reconectar, ningún conflicto se resuelve por sobrescritura

## Casos especiales que la afectan

- [CE-09](../casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md) — Es el caso que decide la adopción del sistema
- [CE-11](../casos-especiales/CE-11-licencia-vence-durante-la-mision.md) — La revalidación sin red se hace contra el paquete local, y la divergencia posterior abre hallazgo

## Criterios de aceptación

```gherkin
# language: es
Característica: Emisión de documentos sin conectividad en la delegación

  Antecedentes:
    Dada la Delegación Choluteca con rango de folios de "OM-CHO-2026-0100" a "OM-CHO-2026-0199"
    Y una Orden de Misión "OM-CHO-2026-0143" en estado "PROGRAMADA" con folio reservado
    Y un umbral configurable de antigüedad de sincronización de "3" días
    Y que el dispositivo de la delegación sincronizó por última vez el "2026-09-08"
    Y que la fecha del despacho es "2026-09-15"

  Escenario: Se rechaza emitir sin red cuando el rango local está agotado
    Dado que los "100" folios del rango de la delegación están reservados o consumidos
    Cuando el Encargado de Delegación intenta despachar sin conectividad
    Entonces el sistema rechaza el despacho
    Y muestra "El rango de folios de la Delegación Choluteca está agotado y no hay conectividad para ampliarlo. No se puede emitir."

  Escenario: Se rechaza emitir sin red sin código de autorización fuera de línea
    Cuando el Encargado de Delegación intenta despachar sin conectividad
      y sin presentar el código de autorización fuera de línea
    Entonces el sistema rechaza el despacho
    Y muestra "El despacho sin conectividad requiere el código de autorización fuera de línea de esta misión."

  Escenario: Se emite sin red y la condición se imprime en el documento
    Cuando el Encargado de Delegación despacha "OM-CHO-2026-0143" sin conectividad,
      con el código de autorización fuera de línea válido
    Entonces el folio "OM-CHO-2026-0143" pasa al estado "CONSUMIDO" desde el rango local
    Y se imprimen los documentos del juego que corresponda
    Y la Orden de Misión imprime "Emitida sin conectividad con datos sincronizados hace 7 días (última sincronización: 08/09/2026)"
    Y el paquete normativo congelado registra el identificador de cada tabla local usada

  Escenario: La revalidación sin red se hace contra el paquete local
    Cuando el Encargado de Delegación despacha "OM-CHO-2026-0143" sin conectividad
    Entonces el sistema revalida licencia, documentación del vehículo y estado operativo
      contra los datos del paquete local
    Y registra que la revalidación se hizo sobre datos locales con su fecha de sincronización

  Escenario: Al reconectar, una divergencia en un bloqueo duro abre hallazgo y no revierte el hecho
    Dado que la misión se despachó sin red el "2026-09-15" y el vehículo ya salió
    Cuando el dispositivo sincroniza el "2026-09-17" y la licencia del motorista figuraba
      vencida desde el "2026-09-14" en el servidor
    Entonces el sistema no revierte el despacho
    Y abre un hallazgo automático sobre la misión
    Y notifica al Jefe de Transporte y al Auditor Interno
    Y el hallazgo debe estar resuelto antes de que la misión pueda pasar a "CERRADA"

  Escenario: El documento emitido sin red se reporta como desactualizado si el expediente cambió
    Dado que la misión se modificó en el servidor después de la impresión
    Cuando un Verificador en Carretera escanea el QR del documento
    Entonces el sistema responde estado "DESACTUALIZADO"
```

## Fuera de alcance

- **Imprimir un documento válido con la misión todavía en `PROGRAMADA`** — no lo permite `INV-15`; requiere decisión de producto
- El registro de la salida por el motorista sin conectividad — es [HU-042](HU-042-registro-de-la-salida-sin-conectividad.md)
- El mecanismo de generación y verificación del código de autorización fuera de línea — es de M-16 y de arquitectura
- La reconciliación general del espejo al reconectar — es de M-20

## Notas y pendientes

- `[C]` **`HB3-14` — decisión de producto pendiente.** Si la institución necesita **imprimir con antelación real, estando la misión aún `PROGRAMADA`**, esta historia no la cubre: hay que decidir si se admite emitir un documento oficial antes de la revalidación del despacho, y con qué mitigaciones. Insumos #1 y #41. **La historia no entra a sprint hasta que el PO se pronuncie.**
- `[C]` **¿Habilitamos modo delegación desconectada** —autorizar y despachar sin red—? — insumo #41. Es deuda declarada por el arquitecto y decisión del PO.
- `[C]` **Enlace real de cada delegación** y umbrales de sincronización — insumo #68.
- `[C]` **Procedimiento de ampliación de rango de folios sin conectividad** — insumo #1.
