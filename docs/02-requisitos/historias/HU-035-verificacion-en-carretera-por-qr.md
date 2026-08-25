# HU-035 — Verificar un documento en carretera por QR, sin autenticarse y sin exponer el expediente

| Campo | Valor |
|---|---|
| **Módulo** | M-15 Formatos Oficiales e Impresión · M-14 Reportes, Indicadores y Auditoría |
| **Actor** | ACT-15 Verificador en Carretera (**no autenticado**) |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | **Borrador — bajada por `HB34-06`**: bloqueada por el pendiente G (exposición de punto público con despliegue on-premise), el mismo `[C]` por el que [HU-019](HU-019-verificacion-del-salvoconducto-en-carretera.md) está en borrador. El mismo insumo abierto no puede producir dos veredictos opuestos |
| **Deriva de** | [CU-05](../casos-de-uso/CU-05-emitir-orden-de-mision-y-documentos.md) paso 15 y A3 |

## Nota de corrección — hallazgo `HB34-06`

> **Duplicación con [HU-019](HU-019-verificacion-del-salvoconducto-en-carretera.md).** Mismo actor no autenticado, mismo módulo, misma regla rectora `RN-25`, mismos cuatro estados, mismo mínimo verificable. Ninguna se excluía de la otra.
>
> **Delimitación adoptada:** `HU-035` manda en **el mecanismo genérico** —contrato de respuesta mínima, registro de consultas, minimización, un solo punto de verificación para Orden de Misión, vale de combustible, hoja de bitácora y manifiesto—. `HU-019` manda en **el salvoconducto**: qué lo invalida, qué estados devuelve y qué se muestra de él. Los escenarios de salvoconducto de esta historia se conservan como **casos de ejercicio del mecanismo**, y su semántica la fija `HU-019`.
>
> **Qué produce `DESACTUALIZADO`.** Esta historia decía *«cambió de motorista»*; `HU-019` decía *«cambió la ruta»*; `HU-045` seguía a `BD-04` y concluía que el relevo no invalidaba nada. `HB3-07` ya había adoptado **la lectura más exigente**, la de [`RN-23`](../../01-negocio/reglas/RN-23-permiso-de-circulacion-en-dia-inhabil.md): **vehículo, motorista, ruta y ventana**. Se aplica: cualquiera de los cuatro que cambie después de la impresión desactualiza el documento. El motorista sigue siendo uno de ellos, así que el escenario de esta historia era correcto pero **incompleto**.
>
> **Mensaje de folio inexistente.** Esta historia respondía *«Folio no encontrado. Este documento no fue emitido por la institución.»* — una afirmación sobre el emisor que `HU-019` evita a propósito. **Manda la redacción de `HU-019`**: se responde *«Folio no encontrado»* y **no se revela si el rango de folios existe**. Un punto público que confirma qué rangos son válidos enseña a fabricar folios verosímiles.
>
> **Veredicto DoR.** Esta historia declaraba **el mismo** `[C]` bloqueante que `HU-019` —pendiente G— y estaba `Refinada` mientras `HU-019` estaba en borrador. Sin esa decisión el QR no apunta a nada: el `[C]` **es** la lógica, no un parámetro. Baja a borrador.

## Historia

**Como** Verificador en Carretera —autoridad de tránsito o personal de la institución receptora—
**quiero** escanear el QR de un documento y saber si está vigente, anulado, vencido o desactualizado, sin tener que autenticarme
**para** confirmar en el momento que el papel que me presentan es auténtico y sigue amparando ese vehículo en esa ventana, sin acceder a datos que no me corresponden

## Contexto

El documento impreso circula fuera de la institución y su destinatario **no es un usuario del sistema**. Un salvoconducto falsificado o un documento anulado que sigue pasando controles anula todo el esfuerzo de trazabilidad aguas arriba.

La verificación devuelve **el mínimo verificable y nada más**: nunca los nombres de las personas trasladadas, nunca montos, nunca el expediente. Un manifiesto de personas externas se verifica como auténtico **sin exponer identidades**.

Y hay un cuarto estado que casi siempre se olvida: **desactualizado**. Cuando la misión se modificó después de imprimir —una delegación que emitió antes de salir a zona sin cobertura—, el papel que porta el motorista dejó de corresponder al expediente. Devolver *vigente* en ese caso sería mentir; devolver *anulado* también.

## Reglas que la gobiernan

- [RN-25](../../01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md) — **Regla rectora**: todo documento de control en carretera lleva QR verificable y estado consultable
- [RN-51](../../01-negocio/reglas/RN-51-minimizacion-de-datos-de-personas-externas.md) — Minimización: la verificación pública no expone identidades
- [RN-52](../../01-negocio/reglas/RN-52-registro-de-consultas-a-manifiestos.md) — Toda consulta se registra: quién vio qué y cuándo
- [RN-04](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md) — La anulación se refleja de inmediato en la verificación
- [RN-23](../../01-negocio/reglas/RN-23-permiso-de-circulacion-en-dia-inhabil.md) — Para el salvoconducto, lo amparado es **vehículo, motorista, ruta y ventana**: los cuatro producen `DESACTUALIZADO`. La semántica la fija [HU-019](HU-019-verificacion-del-salvoconducto-en-carretera.md); aquí solo se ejercita el mecanismo (`HB34-06`)

## Casos especiales que la afectan

- [CE-20](../casos-especiales/CE-20-mision-cancelada-con-combustible-ya-entregado.md) — Un papel anulado no debe pasar un control
- [CE-09](../casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md) — Sin datos del lado del verificador, queda la huella impresa y el código corto

## Criterios de aceptación

```gherkin
# language: es
Característica: Verificación pública de documentos por QR

  Antecedentes:
    Dado un salvoconducto con folio "SC-2026-0087" emitido para el vehículo con correlativo
      "INS-P-014" y placa "PAA-1234", que ampara del "2026-09-19 22:00" al "2026-09-20 14:00"
    Y la fecha y hora actuales "2026-09-19 23:40"

  Escenario: Se rechaza una consulta de folio inexistente y queda registrada
    Cuando un Verificador en Carretera consulta el folio "SC-2026-9999"
    Entonces el sistema responde "Folio no encontrado"
    Y no revela si el rango de folios existe ni afirma nada sobre el emisor
    Y registra la consulta fallida con el folio consultado, el momento y el origen de la consulta

  Escenario: Un documento anulado se reporta como anulado de inmediato
    Dado que el salvoconducto "SC-2026-0087" fue anulado el "2026-09-19 21:00"
      por anulación de la misión
    Cuando un Verificador en Carretera escanea su QR
    Entonces el sistema responde estado "ANULADO"
    Y muestra "Documento anulado el 19/09/2026 21:00. No ampara circulación."

  Escenario: Un documento fuera de su ventana se reporta como vencido
    Dada la fecha y hora actuales "2026-09-20 18:00"
    Cuando un Verificador en Carretera escanea el QR de "SC-2026-0087"
    Entonces el sistema responde estado "VENCIDO"
    Y muestra "Este documento amparó del 19/09/2026 22:00 al 20/09/2026 14:00."

  Esquema del escenario: Cualquier elemento amparado que cambie tras la impresión se reporta como desactualizado
    Dado que la Orden de Misión vinculada cambió "<elemento>" el "2026-09-19 23:00",
      posterior a la impresión del documento
    Cuando un Verificador en Carretera escanea el QR de "SC-2026-0087"
    Entonces el sistema responde estado "DESACTUALIZADO"
    Y muestra "El expediente de esta misión se modificó después de la impresión. Solicite el documento vigente."

    Ejemplos:
      | elemento         |
      | de vehículo      |
      | de motorista     |
      | de ruta amparada |
      | de ventana       |

  Escenario: Un documento vigente devuelve solo el mínimo verificable
    Cuando un Verificador en Carretera escanea el QR de "SC-2026-0087"
    Entonces el sistema responde estado "VIGENTE"
    Y muestra el folio, el tipo de documento, la institución emisora,
      el vehículo por correlativo "INS-P-014" y placa "PAA-1234",
      la ventana temporal amparada y la huella del documento
    Y no muestra nombres de personas trasladadas, montos, ruta detallada ni el expediente

  Escenario: El manifiesto se verifica sin exponer identidades
    Dado un manifiesto de personas externas con folio "MF-2026-0021"
    Cuando un Verificador en Carretera escanea su QR
    Entonces el sistema responde estado "VIGENTE", el folio, la institución y el vehículo
    Y muestra la cantidad de personas amparadas
    Y no muestra ningún nombre, documento de identidad ni dato personal

  Escenario: La verificación no exige autenticación y queda registrada igual
    Cuando un Verificador en Carretera escanea el QR de "SC-2026-0087" sin iniciar sesión
    Entonces el sistema responde sin solicitar credenciales
    Y registra la consulta con folio, momento y origen
```

## Fuera de alcance

- La emisión de los documentos y su contenido — es [HU-031](HU-031-consumo-del-folio-y-emision-del-juego-documental.md)
- **Lo propio del salvoconducto** —qué elementos ampara, qué lo invalida y qué se muestra de él— es [HU-019](HU-019-verificacion-del-salvoconducto-en-carretera.md). Aquí el salvoconducto aparece solo como caso de ejercicio del mecanismo genérico (delimitación de `HB34-06`)
- El acceso al expediente por parte del Auditor Interno, que sí es usuario autenticado — es de M-14
- La verificación telefónica y el contraste visual de la huella cuando el verificador no tiene datos móviles: se documentan como procedimiento, no como funcionalidad `[I]`

## Notas y pendientes

- `[C]` **¿Acepta la institución exponer un punto de verificación público**, siendo el despliegue on-premise? — pendiente G de [insumos-pendientes.md](../../07-gestion/insumos-pendientes.md). Si la respuesta es no, el QR se degrada a código de verificación corto con consulta telefónica y la historia cambia de alcance.
- `[C]` **¿Acepta la DNVT el documento sustitutivo del Instituto de la Propiedad en un retén?** — insumo #61. Decide si la verificación es defensa efectiva o solo evidencia interna.
- `[V]` La exigencia de documento portable y verificable proviene de [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md); la ausencia de firma electrónica certificada, de [NRM-08](../../01-negocio/normativa/NRM-08-firma-electronica.md).
