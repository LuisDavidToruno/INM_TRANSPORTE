# HU-019 — Verificar el salvoconducto en carretera sin autenticación y con el mínimo dato

| Campo | Valor |
|---|---|
| **Módulo** | M-15 Formatos Oficiales e Impresión (con M-04 y M-14) |
| **Actor** | ACT-15 Verificador en Carretera — **actor no autenticado** |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — bloqueada por el pendiente G (exposición de punto público con despliegue on-premise) |

## Historia

**Como** verificador en carretera —agente de tránsito, fiscalizador del Tribunal Superior de Cuentas o autoridad de control—
**quiero** escanear el QR del salvoconducto y obtener de inmediato si el documento está vigente, anulado, vencido o desactualizado, con el vehículo y la ventana que ampara
**para** resolver la verificación en el punto de control sin llamar a la institución y sin necesitar una cuenta en un sistema que no es mío

## Contexto

Este es el único actor del sistema que **no se autentica y no verá nunca el expediente**. Diseñar la verificación como si fuera una pantalla interna sería inutilizarla: el fiscalizador no va a pedir usuario a la institución que está fiscalizando.

La respuesta es el **mínimo verificable**: folio, tipo de documento, institución, estado, vehículo y ventana autorizada, más la huella del documento. **Nunca** el expediente, ni nombres de personas trasladadas, ni montos.

El estado tiene cuatro valores, y el cuarto es el que casi siempre se olvida: **desactualizado**. Una delegación emite el salvoconducto por anticipado, la misión cambia después, y el papel que el motorista lleva ya no corresponde. Si la verificación solo pudiera responder *vigente* o *anulado*, ese caso quedaría mal clasificado.

**Cada consulta se registra, y cada verificación fallida también.** Un patrón de folios inexistentes consultados es información valiosa: alguien está fabricando salvoconductos.

## Reglas que la gobiernan

- [RN-25](../../01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md) — QR verificable; la verificación devuelve vigente, anulado, vencido o desactualizado
- [RN-51](../../01-negocio/reglas/RN-51-minimizacion-de-datos-de-personas-externas.md) — La verificación pública **nunca** expone nombres de personas trasladadas
- [RN-52](../../01-negocio/reglas/RN-52-registro-de-consultas-a-manifiestos.md) — Toda consulta se registra: quién consultó, qué y cuándo
- [RN-15](../../01-negocio/reglas/RN-15-identidad-del-vehiculo-y-placa.md) — El vehículo se identifica por correlativo institucional; la placa puede no existir
- [RN-04](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md) — El folio anulado sigue siendo consultable y devuelve su estado

## Casos especiales que la afectan

- [CE-17](../casos-especiales/CE-17-vehiculo-sin-placa-metalica.md) — El vehículo sin lámina se verifica por correlativo institucional
- [CE-09](../casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md) — Emisión anticipada en delegación sin señal: origen del estado *desactualizado*

## Criterios de aceptación

```gherkin
# language: es
Característica: Verificación pública del salvoconducto por QR
  Como Verificador en Carretera
  quiero comprobar el documento sin autenticarme
  para resolver el control en el punto donde ocurre

  Antecedentes:
    Dado un salvoconducto "CHO-SC-2026-0011" emitido para el vehículo "VH-0142", placa "PAA-1234", con ventana del "2026-03-20 07:00" al "2026-03-21 17:00"
    Y una consulta realizada sin ninguna credencial

  Escenario: Un folio inexistente devuelve no encontrado y queda registrado
    Cuando se consulta el folio "CHO-SC-2026-9999"
    Entonces el sistema responde "Folio no encontrado"
    Y no revela si el rango de folios existe
    Y registra la consulta fallida con fecha, hora y origen

  Escenario: Un salvoconducto anulado se refleja de inmediato
    Dado un salvoconducto "CHO-SC-2026-0011" anulado el "2026-03-19 18:20" por sustitución de vehículo
    Cuando se consulta el folio "CHO-SC-2026-0011"
    Entonces el sistema responde estado "ANULADO"
    Y muestra la fecha de anulación "19/03/2026 18:20"
    Y no muestra el motivo interno de la anulación

  Escenario: Un salvoconducto vencido se distingue de uno anulado
    Dada una fecha de consulta del "2026-03-23 10:00"
    Cuando se consulta el folio "CHO-SC-2026-0011"
    Entonces el sistema responde estado "VENCIDO"
    Y muestra la ventana amparada "20/03/2026 07:00 a 21/03/2026 17:00"

  Escenario: Un salvoconducto emitido por anticipado y superado por un cambio devuelve desactualizado
    Dado un salvoconducto "CHO-SC-2026-0012" emitido por anticipado en delegación sin conectividad
    Y un cambio posterior de la ruta amparada, sincronizado el "2026-03-20 05:00"
    Cuando se consulta el folio "CHO-SC-2026-0012" el "2026-03-20 09:00"
    Entonces el sistema responde estado "DESACTUALIZADO"
    Y muestra "El documento impreso no corresponde a la versión vigente del permiso."

  Escenario: La verificación no expone datos del expediente ni de las personas trasladadas
    Dado un salvoconducto de una misión que traslada 4 personas externas
    Cuando se consulta el folio "CHO-SC-2026-0011"
    Entonces la respuesta contiene folio, tipo de documento, institución, estado, vehículo y ventana amparada
    Y contiene la huella del documento electrónico
    Y no contiene nombres de personas trasladadas
    Y no contiene montos ni el número de expediente de la solicitud

  Escenario: El vehículo sin lámina metálica se verifica por correlativo institucional
    Dado un salvoconducto emitido para el vehículo "VH-0187" sin lámina metálica
    Cuando se consulta su folio
    Entonces la respuesta identifica el vehículo como "VH-0187 — sin lámina metálica"
    Y no muestra ningún número de placa

  Escenario: Cada consulta exitosa queda registrada
    Cuando se consulta el folio "CHO-SC-2026-0011"
    Entonces el sistema registra la consulta con fecha, hora y origen
    Y ese registro queda disponible para Auditoría Interna

  Escenario: Sin datos móviles quedan el contraste visual y el código corto
    Dado un verificador sin acceso a datos móviles
    Cuando compara la huella impresa en el documento con el código de verificación corto del mismo documento
    Entonces ambos elementos constan impresos en el salvoconducto
    Y el documento indica el teléfono institucional de consulta
```

## Fuera de alcance

- La emisión e impresión del salvoconducto — es [HU-017](HU-017-emision-e-impresion-del-salvoconducto.md)
- La verificación de **otros** documentos con QR (Orden de Misión, vale de combustible, hoja de bitácora): comparten el mecanismo pero son historias propias de M-15
- El diseño técnico del punto de verificación: el stack está diferido al Sprint 2 por [ADR-000](../../03-arquitectura/adr/ADR-000-diferir-seleccion-de-stack.md). Aquí se describen capacidades requeridas
- Cualquier interacción del verificador con el expediente: **no la tiene y no la tendrá**

## Notas y pendientes

- `[C]` **Si la institución acepta exponer un punto de verificación público siendo el despliegue on-premise** — pendiente G de [`actores-y-roles.md`](../../01-negocio/actores-y-roles.md). **Es lo que bloquea esta historia**: sin esa decisión, el QR no tiene a dónde apuntar. La alternativa degradada —contraste visual, código corto y consulta telefónica— sí es implementable de inmediato y podría separarse como historia propia
- `[I]` Que el contraste visual y la consulta telefónica sirvan como respaldo sin datos móviles es práctica común declarada en [CU-03](../casos-de-uso/CU-03-permiso-de-circulacion-en-dia-inhabil.md), no norma
- `[V]` Que el control en carretera es físico y que el TSC realiza operativos consta en [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md) y [NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md)
- El control de acceso por rol y el registro de consultas se conservan por exigencia del MARCI; **no** se diseña para anticipar la ley de datos personales pendiente en el Congreso — [NRM-07](../../01-negocio/normativa/NRM-07-transparencia-y-datos-personales.md)
- Trazabilidad: [CU-03](../casos-de-uso/CU-03-permiso-de-circulacion-en-dia-inhabil.md) paso 9 y flujo alterno A5
