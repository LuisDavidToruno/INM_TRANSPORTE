# NRM-04 — Presupuesto y finanzas (SEFIN / SIAFI)

| Campo | Valor |
|---|---|
| **Ámbito** | Imputación presupuestaria, cuotas de compromiso, conciliación con el sistema financiero del Estado |
| **Módulos afectados** | M-09, M-10, M-11, M-13, M-14 |
| **Última verificación** | 2026-08-06 |
| **Riesgo de cambio** | Medio — las Disposiciones Generales cambian cada ejercicio |

## Marco normativo

| Norma / sistema | Referencia | Vigencia | Verificación |
|---|---|---|---|
| SIAFI — Sistema de Administración Financiera Integrada | — | Vigente | `[V]` |
| SIAFI GES — plataforma web con SSO | Manual de Usuario v3, 22/06/2023 | Vigente | `[V]` |
| Manual de Clasificadores Presupuestarios | versión 2018 | Vigente | `[V]` |
| Presupuesto General 2026 | Decreto D-62-2026 | Ejercicio 2026 | `[V]` |
| Reglamento de las Disposiciones Generales del Presupuesto 2026 | Acuerdo No. 360-2026, 12/06/2026 | Ejercicio 2026 | `[V]` |

Según la propia FAQ de SEFIN, **SIAFI GES** se usa en formulación presupuestaria y evaluación física-financiera; otros módulos permanecen en **SIAFI II**. `[V]`

El **Manual de Clasificadores** define cinco clasificaciones: por objeto, por funciones, por fuente de financiamiento, por organismo financiador y geográfica. `[V]`

**No se pudieron verificar los códigos numéricos del objeto del gasto** para combustibles y lubricantes, mantenimiento de vehículos y viáticos. **No se inventan.** Deben tomarse del manual vigente. `[C]`

## Cuotas trimestrales de compromiso `[V]`

El módulo de programación financiera de SIAFI asigna **cuotas trimestrales de compromiso** por Gerencia Administrativa, Unidad Ejecutora, clase de gasto y fuente de financiamiento.

Esto es determinante y suele pasarse por alto: **el gasto en combustible y viáticos no está limitado solo por el presupuesto anual, sino por la cuota del trimestre**. Un sistema que solo controla contra el presupuesto anual permitirá comprometer gasto que la institución no puede ejecutar.

`[C]` Las Disposiciones Generales del Presupuesto suelen incluir restricciones y topes anuales específicos sobre combustible, vehículos y viáticos. Obtener los artículos aplicables del Acuerdo 360-2026.

## Implicaciones de requerimiento

- **El sistema debe** modelar la **estructura presupuestaria completa** como catálogo configurable: institución, gerencia administrativa, unidad ejecutora, programa/actividad-obra, objeto del gasto, fuente de financiamiento, organismo financiador y ubicación geográfica.
- **El sistema debe** exigir la **imputación presupuestaria al autorizar**, no al liquidar: toda orden de misión, requisición de combustible y anticipo de viáticos nace con su estructura de imputación.
- **El sistema debe** llevar control de disponibilidad en **tres niveles**: presupuesto aprobado, **cuota trimestral de compromiso**, y saldo comprometido / devengado / pagado. Debe advertir o bloquear al exceder la cuota del trimestre — configurable.
- **El sistema debe** generar un **archivo de conciliación con SIAFI** por período, con los campos que permitan casar cada gasto local contra el registro en SIAFI: número de preventivo o compromiso, documento F-01 o equivalente, monto, objeto del gasto, unidad ejecutora, proveedor y fecha. `[C]` el formato exacto con la Gerencia Administrativa.
- **El sistema debe** ser **no autoritativo frente a SIAFI**. SIAFI es el sistema de registro oficial; SIGTI es gestión operativa que produce insumos y concilia. **Nunca lo sustituye ni lo "corrige".**
- **El sistema debe** diseñar la integración con SIAFI como **archivo y conciliación primero, API después**. Asumir integración en línea es riesgo alto: la evidencia sugiere que SEFIN no habilita interfaces para terceros de forma general. `[C]`
- **El sistema debe** manejar el **cierre y apertura de ejercicio fiscal**: misiones que cruzan el 31 de diciembre, anticipos no liquidados al cierre, y reversión de compromisos.
- **El sistema debe** producir reportes de **ejecución por objeto del gasto y por vehículo**, para que la Gerencia Administrativa justifique reprogramaciones.

## Zonas grises y pendientes

- `[C]` Códigos del objeto del gasto para combustible, mantenimiento y viáticos, tomados del Manual de Clasificadores vigente.
- `[C]` Artículos del Acuerdo 360-2026 sobre combustible, vehículos y viáticos.
- `[C]` ¿Existe alguna interfaz o mecanismo de intercambio con SIAFI disponible para la institución? Determina si la conciliación es por archivo o automatizable.
- `[C]` Estructura presupuestaria concreta que usa la institución piloto.

## Fuentes

- [SEFIN — Administración Financiera SIAFI](https://www.sefin.gob.hn/administracion-financiera-siafi/) — consultado 2026-08-06
- [SEFIN — diferencia entre SIAFI GES y SIAFI](https://www.sefin.gob.hn/faq-items/que-diferencia-principal-existe-entre-siafi-ges-y-siafi/) — consultado 2026-08-06
- [SEFIN — Manual de Clasificadores Presupuestarios](https://www.sefin.gob.hn/wp-content/uploads/SAMI/docs/CLASIFICADORES/Manual-de-Clasifiadores-Presupuestarios.pdf) — consultado 2026-08-06
