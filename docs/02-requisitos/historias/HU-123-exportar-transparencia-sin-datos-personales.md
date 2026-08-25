# HU-123 — Exportar el reporte de transparencia desde la vista de gestión pública, sin datos personales

| Campo | Valor |
|---|---|
| **Módulo** | M-17 Traslado de Personas Externas · M-14 Reportes, Indicadores y Auditoría |
| **Actor** | ACT-08 Gerencia Administrativa · `[C]` Oficial de Información Pública — actor no catalogado |
| **Prioridad** | Media |
| **Sprint** | sin asignar |
| **Estado** | Borrador — falta saber qué publica hoy la institución y en qué formato |

## Historia

**Como** Gerencia Administrativa
**quiero** generar el reporte de flota y traslados listo para publicar en el Portal Único de Transparencia, sin ningún dato personal
**para** cumplir la obligación de publicación sin que nadie tenga que borrar nombres a mano en una hoja de cálculo antes de subirla

## Contexto

`[V]` La Ley de Transparencia y Acceso a la Información Pública (Decreto 170-2006) obliga a publicar a través del **Oficial de Información Pública**, que recopila información de todas las unidades administrativas y la publica en los plazos de la Ley, en el **Portal Único de Transparencia** ([NRM-07](../../01-negocio/normativa/NRM-07-transparencia-y-datos-personales.md)).

Hoy esa publicación se prepara depurando archivos a mano, y **la depuración manual falla**: se olvida una columna, se deja una pestaña oculta, se sube el archivo equivocado. Una vez publicado en el Portal, el dato personal no se recoge.

Por eso [RN-51](../../01-negocio/reglas/RN-51-minimizacion-de-datos-de-personas-externas.md) numeral 3 no dice "filtrar": dice que los reportes públicos se generan desde la vista de gestión pública, **sin acceso técnico** a los campos personales — *"no por filtrado en el reporte, sino por separación de origen"*. La diferencia importa: un filtro se puede desactivar por error; una vista que no tiene los campos no los puede exponer.

## Reglas que la gobiernan

- [RN-51](../../01-negocio/reglas/RN-51-minimizacion-de-datos-de-personas-externas.md) — **Regla rectora**: separación estructural; el reporte público se genera **desde la vista de gestión pública**
- [RN-52](../../01-negocio/reglas/RN-52-registro-de-consultas-a-manifiestos.md) — La consulta agregada o anonimizada **no** entra al registro de consultas, porque por construcción no expone a nadie
- [RN-94](../../01-negocio/reglas/RN-94-fecha-de-corte-de-conocimiento-en-todo-reporte.md) — Todo reporte declara su fecha de corte y es reproducible a esa fecha

## Casos especiales que la afectan

> Sección incorporada por el hallazgo `HB34-13`: faltaba, y el `DoR` exige identificar los `CE-xx` que afectan a la historia **o dejar constancia explícita de que no hay ninguno**.

- [CE-27](../casos-especiales/CE-27-cierre-de-ejercicio-fiscal-con-hallazgo-abierto.md) — La publicación de transparencia de un ejercicio con hallazgos abiertos se emite con su **fecha de corte** (`RN-94`) y no se retiene a la espera de que los hallazgos se resuelvan: no publicar por prudencia también es una decisión que hay que poder defender
- [CE-28](../casos-especiales/CE-28-hallazgo-posterior-sobre-mision-cerrada.md) — Un hallazgo posterior **no modifica** una publicación ya emitida; se publica el ajuste como un dato nuevo con su propia fecha de corte
- Los 26 `CE-xx` restantes **no tocan este flujo**. Constancia dejada

## Requisitos no funcionales relacionados

- [RNF-17](../no-funcionales/RNF-17-retencion-y-depuracion-diferenciada.md) — Lo que sobrevive a la depuración es exactamente lo que este reporte publica: conteo, condición agregada, origen, destino, vehículo, misión y costos
- [RNF-06](../no-funcionales/RNF-06-reproducibilidad-historica-de-reportes.md) — Reproducibilidad histórica

## Criterios de aceptación

> Los nombres de personas externas de estos escenarios son **ficticios de prueba**.

```gherkin
# language: es
Característica: Exportación de transparencia sin datos personales

  Antecedentes:
    Dado "412" Órdenes de Misión del período "2026-01-01" al "2026-09-30"
    Y "138" de ellas con manifiesto de personas externas
    Y "1.038" personas externas registradas en esos manifiestos

  Escenario: Se rechaza incluir identidades en la exportación pública
    Cuando la Gerencia Administrativa intenta agregar la columna "nombre de la persona trasladada" a la exportación de transparencia
    Entonces el sistema no ofrece esa columna
    Y muestra "La exportación de transparencia se genera desde la vista de gestión pública, que no contiene datos personales. Para la versión con identidades use la exportación con autorización."

  Escenario: Se rechaza publicar la condición individual de la persona
    Cuando la Gerencia Administrativa intenta agregar la columna "institución o condición" a nivel de persona
    Entonces el sistema no ofrece esa columna a nivel de persona
    Y ofrece en su lugar el conteo agregado por condición declarada
    Y muestra "La condición se publica agregada, no persona por persona: publicada individualmente vuelve a identificar."

  Escenario: La exportación pública contiene solo datos de gestión pública
    Cuando la Gerencia Administrativa genera la exportación de transparencia del período con fecha de corte "2026-10-01"
    Entonces la exportación contiene por misión: folio, fecha, unidad ejecutora, objeto del viaje, origen, destino, vehículo, kilometraje, combustible, peajes y costo total
    Y contiene el conteo de personas trasladadas y el conteo agregado por condición declarada
    Y contiene "0" columnas del segmento de datos personales

  Escenario: La exportación pública no se registra como consulta a manifiestos
    Cuando la Gerencia Administrativa genera la exportación de transparencia del período
    Entonces el sistema no registra ninguna consulta en el registro de accesos a manifiestos
    Y sí registra la generación del reporte con su fecha de corte, autor y folio

  Escenario: La exportación declara su fecha de corte y es reproducible
    Dado una exportación generada el "2026-10-01" con fecha de corte "2026-09-30"
    Cuando se regenera el "2027-02-20" con la misma fecha de corte "2026-09-30"
    Entonces los conteos, los montos y la estructura son idénticos a los de la primera generación

  Escenario: La exportación con identidades es otra cosa, con autorización y registro
    Dado una solicitud de información pública que pide los nombres de las personas trasladadas en "OM-2026-0451"
    Cuando el Auditor Interno genera la exportación con identidades con el expediente "SIP-2026-021"
    Entonces el sistema genera un paquete distinto, con folio propio
    Y registra la consulta con alcance "EXPORTACIÓN CON IDENTIDADES" y el expediente que la motiva
    Y muestra "La decisión de entregar los nombres corresponde al Oficial de Información Pública, no al sistema. Queda registrado qué se entregó."

  Escenario: La exportación pública de un período ya depurado no cambia
    Dado que los datos personales del período "2026-01-01" al "2026-09-30" fueron depurados el "2029-01-15"
    Cuando la Gerencia Administrativa regenera la exportación de transparencia de ese período con fecha de corte "2026-09-30"
    Entonces los conteos, los montos y la estructura son idénticos a los de antes de la depuración
```

## Fuera de alcance

- La decisión de **qué** se publica y en qué plazo: es del Oficial de Información Pública y de la LTAIP, no del sistema
- La carga del archivo al Portal Único de Transparencia: es acto institucional externo
- La depuración por retención — es [HU-124](HU-124-depurar-datos-personales-sin-romper-la-cadena.md)
- La atención de un hábeas data — es [HU-121](HU-121-atender-habeas-data-buscar-y-exportar.md)

## Notas y pendientes

- `[C]` **Qué información de flota publica hoy la institución en el Portal Único, y en qué formato** — [NRM-07](../../01-negocio/normativa/NRM-07-transparencia-y-datos-personales.md) lo deja abierto. Es lo que mantiene la historia en borrador: sin saber qué se publica hoy, las columnas del reporte son una propuesta
- `[C]` **El numeral exacto del artículo de información de oficio** que cubre inventario de bienes, viáticos y contrataciones — [NRM-07](../../01-negocio/normativa/NRM-07-transparencia-y-datos-personales.md) `[C]`: debe leerse el articulado con el OIP institucional. **No se cita ningún número de artículo aquí**
- `[C]` El **Oficial de Información Pública no está catalogado como actor** ([actores-y-roles.md](../../01-negocio/actores-y-roles.md)). Aquí se asignó provisionalmente a `ACT-08`
- `[I]` Que la condición se publique agregada y no persona por persona es derivación de [RN-51](../../01-negocio/reglas/RN-51-minimizacion-de-datos-de-personas-externas.md); ninguna norma consultada lo dice literalmente
