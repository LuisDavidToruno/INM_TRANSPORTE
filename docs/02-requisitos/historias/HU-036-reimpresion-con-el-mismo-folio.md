# HU-036 — Reimprimir un documento con el mismo folio, y nunca con contenido distinto

| Campo | Valor |
|---|---|
| **Módulo** | M-15 Formatos Oficiales e Impresión |
| **Actor** | ACT-05 Encargado de Despacho · ACT-10 Encargado de Delegación en su ámbito |
| **Prioridad** | Media |
| **Sprint** | sin asignar |
| **Estado** | Refinada |
| **Deriva de** | [CU-05](../casos-de-uso/CU-05-emitir-orden-de-mision-y-documentos.md) A2 y E7 · `EF-02` |

## Historia

**Como** Encargado de Despacho
**quiero** reimprimir un documento emitido tantas veces como haga falta, siempre con el mismo folio y el mismo contenido, dejando registro de cada reimpresión con su motivo
**para** resolver un extravío o una impresión ilegible sin generar un segundo folio que rompa la conciliación

## Contexto

El papel se moja, se rompe, se queda en la caseta o se pierde en el hotel. La reacción natural del encargado es emitir "otro": y en el momento en que existen dos folios para un mismo permiso, la conciliación deja de cerrar y el auditor encuentra dos órdenes para un mismo viaje.

La regla es simple y no admite matiz: **la reimpresión conserva folio y contenido**. Si el contenido cambia, ya no es una reimpresión: es un documento nuevo, con folio nuevo, que declara expresamente *"sustituye al folio X"*, y el anterior queda anulado.

El conteo de impresiones es dato de auditoría, no una estadística: cinco reimpresiones de un mismo salvoconducto es una pregunta legítima.

## Reglas que la gobiernan

- [RN-05](../../01-negocio/reglas/RN-05-registro-cerrado-no-se-edita.md) — El documento emitido no se edita
- [RN-04](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md) — El documento sustituido se anula con asiento, no se borra ni se recicla
- [RN-25](../../01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md) — Folio único en la institución; la huella impresa debe corresponder al contenido electrónico
- [RN-44](../../01-negocio/reglas/RN-44-identificadores-y-folios-en-el-cliente.md) — El folio nuevo se toma del rango de la delegación
- [RN-03](../../01-negocio/reglas/RN-03-registro-inmutable-de-autorizacion.md) — Cada reimpresión registra actor, momento y motivo

## Casos especiales que la afectan

- [CE-09](../casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md) — En delegación sin red, la reimpresión debe funcionar con el paquete local
- [CE-20](../casos-especiales/CE-20-mision-cancelada-con-combustible-ya-entregado.md) — Los folios anulados no se reciclan

## Criterios de aceptación

```gherkin
# language: es
Característica: Reimpresión y sustitución de documentos emitidos

  Antecedentes:
    Dada una Orden de Misión con folio "OM-CHO-2026-0143" emitida el "2026-09-15 05:40"
    Y un salvoconducto con folio "SC-2026-0087" emitido para la misma misión

  Escenario: Se rechaza reimprimir con contenido distinto
    Cuando el Encargado de Despacho intenta reimprimir "OM-CHO-2026-0143"
      cambiando el motorista asignado
    Entonces el sistema rechaza la reimpresión
    Y muestra "No se reimprime con contenido distinto. Emita un documento nuevo, con folio nuevo, que declare que sustituye al folio OM-CHO-2026-0143."

  Escenario: Se rechaza emitir un folio nuevo por un documento extraviado en ruta
    Dado que el motorista reporta el extravío del salvoconducto "SC-2026-0087" en ruta
    Cuando el Encargado de Despacho intenta emitir un salvoconducto con folio nuevo
      para la misma misión y la misma ventana
    Entonces el sistema rechaza la emisión
    Y muestra "Ya existe el salvoconducto SC-2026-0087 vigente para esta misión y esta ventana. Reimprímalo con el mismo folio: dos folios para un mismo permiso rompen la conciliación."

  Escenario: Se reimprime el documento extraviado con el mismo folio y motivo registrado
    Cuando el Encargado de Despacho reimprime "SC-2026-0087"
      con motivo "extravío en ruta reportado por el motorista"
    Entonces el documento reimpreso conserva el folio "SC-2026-0087", su contenido y su huella
    Y el conteo de impresiones de ese folio pasa a "2"
    Y se registra la reimpresión con el Encargado de Despacho, el momento y el motivo
    Y la verificación por QR sigue devolviendo estado "VIGENTE"

  Escenario: El cambio de contenido produce documento nuevo que declara a cuál sustituye
    Dado que se sustituyó el vehículo de la misión antes de la salida
    Cuando el Encargado de Despacho emite la Orden de Misión con el vehículo sustituto
    Entonces se emite la Orden con folio "OM-CHO-2026-0144"
    Y el documento impreso declara "Sustituye al folio OM-CHO-2026-0143"
    Y el folio "OM-CHO-2026-0143" pasa al estado "ANULADO" con motivo y autor
    Y la verificación por QR de "OM-CHO-2026-0143" devuelve estado "ANULADO"

  Escenario: El conteo de impresiones es consultable por auditoría
    Dado que "SC-2026-0087" se ha impreso "3" veces
    Cuando el Auditor Interno consulta el documento
    Entonces ve las tres impresiones con su autor, momento y motivo
    Y la consulta queda registrada
```

## Fuera de alcance

- La emisión inicial del juego documental — es [HU-031](HU-031-consumo-del-folio-y-emision-del-juego-documental.md)
- La sustitución del recurso que motiva el documento nuevo — es [HU-043](HU-043-sustituir-vehiculo-o-motorista-en-programada.md) y [HU-044](HU-044-sustituir-con-la-mision-despachada.md)
- La reposición de un documento extraviado **después** del retorno, ya en liquidación — es de M-13

## Notas y pendientes

- `[C]` **Formatos en papel vigentes** — insumo #2: define si la reimpresión debe llevar una marca visible de "reimpresión N.º 2" o si el documento debe ser idéntico al original. La postura provisional `[I]` es imprimirlo **idéntico**, porque una marca podría hacer que un control lo rechace, y dejar el conteo únicamente en el sistema.
