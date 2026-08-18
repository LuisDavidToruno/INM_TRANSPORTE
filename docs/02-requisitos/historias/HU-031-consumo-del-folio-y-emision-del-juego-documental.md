# HU-031 — Consumir el folio y emitir el juego de documentos que corresponda al caso

| Campo | Valor |
|---|---|
| **Módulo** | M-15 Formatos Oficiales e Impresión · M-07 Programación y Despacho |
| **Actor** | ACT-05 Encargado de Despacho · ACT-10 Encargado de Delegación en su ámbito |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Refinada |
| **Deriva de** | [CU-05](../casos-de-uso/CU-05-emitir-orden-de-mision-y-documentos.md) pasos 5 a 7 y 12 a 14 · `T-12` · `EF-02`, `EF-03` |

## Historia

**Como** Encargado de Despacho
**quiero** que al despachar el sistema consuma el folio reservado y emita **el juego completo de documentos que corresponda a esa misión concreta**, cada uno con folio, QR, espacio de firma y sello y huella del contenido electrónico
**para** que el motorista salga con todo el papel que el control en carretera y la entrega en destino le van a exigir, y ni un documento de más ni uno de menos

## Contexto

El control en carretera es físico y el papel es la premisa, no el parche. Pero el juego documental **no es fijo**: depende del caso. Una misión en día hábil, con personal propio y sin carga, lleva Orden de Misión y hoja de bitácora. La misma misión un sábado lleva además salvoconducto; con personas externas, manifiesto; con carga, acta de entrega-recepción; con vehículo sin lámina, paquete de identificación; con fondo asignado, constancia de asignación.

Emitir de menos deja al motorista sin defensa en un retén. Emitir de más expone datos que no deben salir —el manifiesto de personas externas no se imprime "por si acaso".

En el mismo acto el sistema **congela el paquete normativo**: todo cálculo posterior de esa misión usa esas tablas, aunque cambien mientras el vehículo está en ruta.

## Reglas que la gobiernan

- [RN-25](../../01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md) — Folio único, QR, firma y sello, huella y vigencia explícita en todo documento de control en carretera
- [RN-44](../../01-negocio/reglas/RN-44-identificadores-y-folios-en-el-cliente.md) — El folio se toma del rango de la delegación
- [RN-59](../../01-negocio/reglas/RN-59-todo-uso-se-ampara-en-orden-de-mision.md) — Ningún vehículo circula sin Orden de Misión, cualquiera sea su régimen
- [RN-23](../../01-negocio/reglas/RN-23-permiso-de-circulacion-en-dia-inhabil.md) — Salvoconducto si la ventana toca día u hora inhábil
- [RN-53](../../01-negocio/reglas/RN-53-cierre-del-manifiesto-al-despacho.md) · [RN-51](../../01-negocio/reglas/RN-51-minimizacion-de-datos-de-personas-externas.md) — Manifiesto cerrado al despachar, con datos mínimos
- [RN-69](../../01-negocio/reglas/RN-69-inventario-de-la-carga-y-acta-de-entrega.md) — Acta de entrega-recepción cuando hay carga
- [RN-65](../../01-negocio/reglas/RN-65-sin-lamina-respaldo-y-paquete-de-identificacion.md) — Paquete de identificación cuando el vehículo no tiene lámina
- [RN-41](../../01-negocio/reglas/RN-41-congelamiento-del-valor-al-autorizar.md) · [RN-39](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md) — Congelamiento del paquete normativo con el identificador de cada tabla

## Casos especiales que la afectan

- [CE-17](../casos-especiales/CE-17-vehiculo-sin-placa-metalica.md) — El paquete de identificación es documento del juego cuando no hay lámina
- [CE-18](../casos-especiales/CE-18-carga-y-pasajeros-en-la-misma-mision.md) — Una misma misión puede exigir manifiesto **y** acta de carga
- [CE-20](../casos-especiales/CE-20-mision-cancelada-con-combustible-ya-entregado.md) — Los folios emitidos y luego anulados no se reciclan

## Criterios de aceptación

```gherkin
# language: es
Característica: Consumo del folio y emisión del juego documental

  Antecedentes:
    Dada una Orden de Misión "OM-CHO-2026-0143" en estado "PROGRAMADA", con folio reservado
    Y un vehículo "Pickup Toyota Hilux" con correlativo institucional "INS-P-014" y placa "PAA-1234"
    Y un motorista "José Martínez" con licencia vigente y verificada
    Y una ventana del "2026-09-15 06:00" al "2026-09-15 18:00", en día hábil

  Escenario: Se rechaza el despacho por falta de capacidad de impresión
    Dado que la Delegación Choluteca no tiene impresora disponible
    Cuando el Encargado de Despacho confirma el despacho de "OM-CHO-2026-0143"
    Entonces el sistema rechaza el despacho
    Y muestra "No hay impresora disponible. El control en carretera es físico: sin documentos impresos no hay salida."
    Y el folio permanece en estado "RESERVADO"

  Escenario: Se rechaza emitir sin manifiesto cuando hay personas externas declaradas
    Dada una misión con objeto del traslado "3 personas externas" sin manifiesto emitido
      ni cadena de custodia registrada
    Cuando el Encargado de Despacho confirma el despacho
    Entonces el sistema rechaza el despacho
    Y muestra "La misión traslada personas externas y no tiene manifiesto emitido ni cadena de custodia registrada."
    Y el folio permanece en estado "RESERVADO"

  Escenario: Se emite el juego mínimo en día hábil, sin carga ni personas externas
    Cuando el Encargado de Despacho confirma el despacho de "OM-CHO-2026-0143"
    Entonces el folio "OM-CHO-2026-0143" pasa al estado "CONSUMIDO"
    Y se emiten los documentos "Orden de Misión" y "Hoja de bitácora"
    Y no se emite salvoconducto, manifiesto ni acta de entrega de carga
    Y cada documento lleva folio único, código QR de verificación, espacio de firma y sello,
      huella del contenido electrónico, correlativo institucional "INS-P-014" con placa "PAA-1234"
      y la vigencia "ampara del 15/09/2026 06:00 al 15/09/2026 18:00"
    Y la Orden de Misión pasa al estado "DESPACHADA"

  Escenario: Se emite el juego ampliado según las condiciones de la misión
    Dada una ventana del "2026-09-19 22:00" al "2026-09-20 14:00", que toca hora inhábil y domingo
    Y un permiso de circulación vigente firmado por la Máxima Autoridad para ese vehículo y esa ventana
    Y un objeto del traslado que incluye "2 servidores" y "carga: 300 kg de insumos"
    Y una asignación de fondo de combustible emitida para la misión
    Cuando el Encargado de Despacho confirma el despacho
    Entonces se emiten los documentos "Orden de Misión", "Salvoconducto", "Hoja de bitácora",
      "Acta de entrega-recepción de la carga" y "Constancia de asignación de fondo de combustible"
    Y cada uno lleva su propio folio único

  Escenario: El paquete normativo queda congelado al emitir
    Cuando el Encargado de Despacho confirma el despacho de "OM-CHO-2026-0143"
    Entonces el sistema congela, con su identificador y vigencia: la tabla de tarifas de peaje,
      la categoría de peaje del vehículo y su fundamento, el calendario de días hábiles de la delegación,
      la matriz licencia↔vehículo, el rendimiento esperado del vehículo, los umbrales de desviación,
      las holguras y los plazos
    Y todo cálculo posterior de esa misión usa el paquete congelado
    Y una modificación posterior de la tabla de tarifas no altera los valores de esa misión

  Escenario: Se rechaza cambiar de vehículo con la misión ya DESPACHADA
    Dada la Orden de Misión "OM-CHO-2026-0143" en estado "DESPACHADA"
    Cuando el Jefe de Transporte intenta cambiar el vehículo asignado
    Entonces el sistema rechaza el cambio
    Y muestra "La misión está DESPACHADA: hay folio consumido y documentos emitidos. Revierta primero con la devolución de lo entregado."
```

## Fuera de alcance

- La sección de peajes del impreso — es [HU-032](HU-032-seccion-de-peajes-en-la-orden-impresa.md)
- Las advertencias superadas impresas en la Orden — es [HU-033](HU-033-advertencias-superadas-impresas-en-la-orden.md)
- El contenido y la paridad de la hoja de bitácora — es [HU-034](HU-034-hoja-de-bitacora-impresa-con-folio.md)
- La verificación física del vehículo, la entrega de llaves y la entrega del fondo — son [HU-040](HU-040-acta-de-entrega-y-traslado-de-custodia.md) y [HU-041](HU-041-emision-y-entrega-del-fondo-de-combustible.md)
- La revalidación de licencia, documentación y disponibilidad al despachar — es [HU-038](HU-038-revalidacion-al-momento-del-despacho.md)

## Notas y pendientes

- `[C]` **Formatos en papel vigentes de la institución** — insumo #2. Cada documento se diseña sobre el formato real; hasta tenerlo, el contenido está definido pero la maquetación no.
- `[C]` **Parque real de impresoras** en sede y delegaciones — insumo #70. Decide si el QR impreso es vía primaria o solo conveniencia.
- `[C]` **Requisitos documentales del traslado de personas externas** según el tipo de institución — insumos #1 y #39. **No se inventan.**
- `[V]` La exigencia de documento portable y verificable en carretera proviene de [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md) y [NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md).
