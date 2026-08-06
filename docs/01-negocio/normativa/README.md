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

**Estado actualizado tras la revisión del PO del 2026-08-06** — ver [DP-001](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md).

| ID | Tema | Estado | Módulos |
|---|---|---|---|
| [NRM-01](NRM-01-control-interno-tsc.md) | Control interno y auditoría — TSC, MARCI, ONADICI | ✅ **Vigente, transversal** | Todos |
| [NRM-02](NRM-02-bienes-del-estado.md) | Bienes del Estado, uso y circulación de vehículos oficiales | ✅ Vigente | M-03, M-04, M-07, M-12 |
| [NRM-03](NRM-03-viaticos.md) | Viáticos y gastos de viaje | ⛔ **Fuera de alcance** — lo maneja ARGOS | — |
| [NRM-04](NRM-04-presupuesto-siafi.md) | Presupuesto, SEFIN y SIAFI | ⚠️ Reducida — la estructura la define ARGOS; SIAFI diferido | M-09, M-11, M-13 |
| [NRM-05](NRM-05-contrataciones-oncae.md) | Compras y contrataciones — ONCAE, HonduCompras | ⛔ **Fuera de alcance** — SIGTI no compra | — |
| [NRM-06](NRM-06-transito-y-licencias.md) | Tránsito, licencias, matrícula, seguro, siniestros | ✅ **Núcleo del sistema** | M-03, M-04, M-05, M-07, M-12 |
| [NRM-07](NRM-07-transparencia-y-datos-personales.md) | Transparencia (IAIP) y datos personales | ⚠️ Reducida en la parte de datos personales | M-14, M-17 |
| [NRM-08](NRM-08-firma-electronica.md) | Firma electrónica y validez documental | ⚠️ Reducida — autorización interna, sin certificados | M-15, flujos de aprobación |
| [NRM-09](NRM-09-realidad-operativa.md) | Conectividad, feriados, horarios, prácticas de control | ✅ Vigente | M-08, M-09, M-16, M-19 |
| [NRM-10](NRM-10-peajes.md) | **Peajes**: puntos, tarifas y clasificación vehicular | ✅ **Vigente, núcleo de M-18** | M-18, M-03, M-06, M-08, M-13 |

**Las fichas fuera de alcance no se borran.** Se conservan como referencia: documentan qué le va a pedir SIGTI a otro sistema, y evitan que alguien redescubra en seis meses una investigación que ya se hizo.

## Cómo usar estas fichas

1. **No cites la norma en el código.** Cita la ficha desde la regla de negocio `RN-xx`, y la regla desde el código.
2. **Toda tarifa, plazo, umbral o categoría que venga de una norma es un parámetro configurable con vigencia por fecha.** Sin excepción.
3. **Si una ficha dice `[C]`, no lo resuelvas inventando.** Está registrado en [`insumos-pendientes.md`](../../07-gestion/insumos-pendientes.md).
4. Al obtener un documento pendiente, **actualiza la ficha y su nivel de verificación**, y anota la fecha.
