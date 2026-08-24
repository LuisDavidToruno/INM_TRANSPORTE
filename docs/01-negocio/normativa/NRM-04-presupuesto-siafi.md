# NRM-04 — Presupuesto y finanzas (SEFIN / SIAFI)

> ## ⚠️ ALCANCE REDUCIDO
>
> Decisión del PO del 2026-08-06 — ver [DP-001, decisión D-09](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md).
>
> - **La estructura presupuestaria que use SIGTI es la que defina ARGOS.** Llega por API como espejo local, no se modela desde cero.
> - **La integración con SIAFI queda diferida.** No se inicia con ella.
> - Los **códigos del objeto del gasto** se investigan cuando se necesiten; no bloquean.
>
> **Lo que sí queda vigente para SIGTI:** entender que el gasto está sujeto a **cuota trimestral de compromiso**, no solo a presupuesto anual. Ese dato viene de ARGOS, pero SIGTI debe respetarlo al aprobar la asignación de fondos de combustible y peajes.

| Campo | Valor |
|---|---|
| **Ámbito** | Imputación presupuestaria, cuotas de compromiso, conciliación con el sistema financiero del Estado |
| **Módulos afectados** | M-09, M-11, M-13 — a través de la estructura que provee ARGOS |
| **Última verificación** | 2026-08-06 |
| **Riesgo de cambio** | Medio, pero absorbido por ARGOS |

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

### Códigos del objeto del gasto — avance parcial del 2026-08-24

**Combustibles y lubricantes** `[P]`. Grupo localizado en el clasificador de objetos del gasto de SEFIN. **Origen: el índice de un buscador sobre el PDF oficial `Objetos-del-Gasto-2019.pdf`; no se pudo abrir el documento.**

| Código | Descripción |
|---|---|
| **35600** | Combustibles y Lubricantes |
| 35610 | Gasolina |
| 35620 | Diésel |
| 35630 | Kerosén |
| 35640 | Gas LPG |
| **35650** | Aceites y Grasas Lubricantes |
| 35660 | Bunker |

Definición asociada `[P]`: *productos derivados del petróleo — gasolinas, aceites ligeros usados como combustible, kerosén, diésel y grasas lubricantes, gas natural y artificial, líquido de frenos y aceite para equipo de oficina.*

> ### ⚠️ Tres advertencias que hay que leer antes de usar estos códigos
>
> 1. **`[P]`, no `[V]`.** El PDF oficial de SEFIN **sí tiene capa de texto** — los buscadores lo indexan — pero el entorno de investigación no pudo descomprimirlo. Nadie del equipo ha visto la tabla con sus ojos. Ver *Limitación de herramienta* en [NRM-06](NRM-06-transito-y-licencias.md).
> 2. **Puede ser el clasificador municipal, no el de la Administración Central.** Una de las fuentes describe el archivo como *Clasificador de Objetos del Gasto Municipal*. **Si SIGTI se despliega en una institución de la Administración Central, estos códigos podrían no aplicarle.** Contradicción no resuelta.
> 3. **El archivo se llama `Objetos-del-Gasto-2019.pdf`.** El Manual de Clasificadores de referencia en esta ficha es de 2018 y el ejercicio en curso es 2026. **Un clasificador de siete años atrás no se carga a producción sin confirmar vigencia.**

**No verificado y no inventado** `[C]`:

- **Mantenimiento y reparación de vehículos o equipo de transporte** — no se localizó el código.
- **Llantas, neumáticos y repuestos** — no se localizó.
- **Peajes.** No se localizó ningún objeto del gasto de peaje, y **es dudoso que exista uno propio**: el peaje probablemente se imputa a un objeto genérico de servicios, o se financia con el viático — que es precisamente la frontera abierta con ARGOS del insumo #25. `[I]`
- **Seguros** — no se localizó.

**Ninguno de estos códigos se cablea.** Van al catálogo de objeto del gasto con vigencia por fecha, igual que todo lo demás.

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

- `[P]` **Combustibles y lubricantes: grupo 35600 y sus subcódigos.** Falta abrir el PDF oficial y confirmar tres cosas: que aplica a la Administración Central y no solo a municipios, que el clasificador de 2019 sigue vigente en 2026, y la transcripción literal.
- `[C]` **Códigos de mantenimiento y reparación de vehículos, llantas, repuestos y seguros.** Buscados el 2026-08-24 sin resultado.
- `[C]` **¿Existe objeto del gasto propio para peajes?** Probablemente no; ver la nota en la sección de clasificadores. Es la otra cara del insumo #25.
- `[C]` Artículos del Acuerdo 360-2026 sobre combustible, vehículos y viáticos.
- `[C]` ¿Existe alguna interfaz o mecanismo de intercambio con SIAFI disponible para la institución? Determina si la conciliación es por archivo o automatizable.
- `[C]` Estructura presupuestaria concreta que usa la institución piloto.

## Fuentes

- [SEFIN — Administración Financiera SIAFI](https://www.sefin.gob.hn/administracion-financiera-siafi/) — consultado 2026-08-06
- [SEFIN — diferencia entre SIAFI GES y SIAFI](https://www.sefin.gob.hn/faq-items/que-diferencia-principal-existe-entre-siafi-ges-y-siafi/) — consultado 2026-08-06
- [SEFIN — Manual de Clasificadores Presupuestarios](https://www.sefin.gob.hn/wp-content/uploads/SAMI/docs/CLASIFICADORES/Manual-de-Clasifiadores-Presupuestarios.pdf) — consultado 2026-08-06
- [SEFIN — Objetos del Gasto 2019](https://www.sefin.gob.hn/wp-content/uploads/SAMI/docs/CLASIFICADORES/Objetos-del-Gasto-2019.pdf) — consultado **2026-08-24**. **Tiene capa de texto**; no legible con las herramientas disponibles. Origen del grupo 35600
- [SEFIN — Disposiciones Generales del Presupuesto 2026](https://www.sefin.gob.hn/wp-content/uploads/Presupuesto/2026/Proyecto/index_html_files/Disposiciones-Generales-2026.pdf) — localizado el 2026-08-24, **no consultado en detalle**. Es la vía para cerrar los topes sobre combustible y vehículos
