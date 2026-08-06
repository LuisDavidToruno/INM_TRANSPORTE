# Normativa hondureña aplicable

Marco legal y administrativo que condiciona el diseño de SIGTI. Cada ficha traduce **norma → requisito de sistema**.

**Fecha de investigación: 2026-08-06.** La normativa de este dominio cambia con frecuencia. Revisa [riesgos-normativos.md](riesgos-normativos.md) antes de tomar cualquier ficha como definitiva.

## Leyenda de verificación

| Marca | Significado |
|---|---|
| `[V]` | Verificado con fuente oficial o fuentes concordantes |
| `[P]` | Parcialmente verificado — la norma existe y se confirmó numeración y vigencia, pero no se pudo extraer el articulado |
| `[C]` | Por confirmar con la institución |
| `[I]` | Inferencia o práctica común, no norma |

**Advertencia metodológica:** muchos PDF oficiales de `tsc.gob.hn`, `onadici.gob.hn` y `sefin.gob.hn` son escaneos sin capa de texto. Se verificó existencia, numeración y vigencia; en varios casos no fue posible transcribir el articulado. **No se inventó ningún número de artículo, tarifa ni código presupuestario.**

## Fichas

| ID | Tema | Módulos afectados | Riesgo de cambio |
|---|---|---|---|
| [NRM-01](NRM-01-control-interno-tsc.md) | Control interno y auditoría — TSC, MARCI, ONADICI | Todos | Bajo |
| [NRM-02](NRM-02-bienes-del-estado.md) | Bienes del Estado, uso y circulación de vehículos oficiales | M-03, M-04, M-07, M-12 | Medio |
| [NRM-03](NRM-03-viaticos.md) | Viáticos y gastos de viaje | M-10 | **Alto** |
| [NRM-04](NRM-04-presupuesto-siafi.md) | Presupuesto, SEFIN y SIAFI | M-09, M-10, M-11, M-13 | Medio |
| [NRM-05](NRM-05-contrataciones-oncae.md) | Compras y contrataciones — ONCAE, HonduCompras | M-09, M-11 | **Alto** |
| [NRM-06](NRM-06-transito-y-licencias.md) | Tránsito, licencias, matrícula, seguro, siniestros | M-03, M-04, M-05, M-07, M-12 | **Alto** |
| [NRM-07](NRM-07-transparencia-y-datos-personales.md) | Transparencia (IAIP) y datos personales | M-14, M-17 | Medio |
| [NRM-08](NRM-08-firma-electronica.md) | Firma electrónica y validez documental | M-15, todos los flujos de aprobación | Bajo |
| [NRM-09](NRM-09-realidad-operativa.md) | Conectividad, feriados, horarios, prácticas de control | M-08, M-10, M-16 | Medio |

## Cómo usar estas fichas

1. **No cites la norma en el código.** Cita la ficha desde la regla de negocio `RN-xx`, y la regla desde el código.
2. **Toda tarifa, plazo, umbral o categoría que venga de una norma es un parámetro configurable con vigencia por fecha.** Sin excepción.
3. **Si una ficha dice `[C]`, no lo resuelvas inventando.** Está registrado en [`insumos-pendientes.md`](../../07-gestion/insumos-pendientes.md).
4. Al obtener un documento pendiente, **actualiza la ficha y su nivel de verificación**, y anota la fecha.
