# NRM-05 — Compras y contrataciones (ONCAE / HonduCompras)

| Campo | Valor |
|---|---|
| **Ámbito** | Adquisición de combustible, llantas, repuestos, mantenimiento y alquiler de vehículos |
| **Módulos afectados** | M-09, M-11 |
| **Última verificación** | 2026-08-06 |
| **Riesgo de cambio** | **Alto** — hay reformas anunciadas y regímenes de excepción con vigencia incierta |

## Marco normativo

| Norma | Referencia | Vigencia | Verificación |
|---|---|---|---|
| Ley de Contratación del Estado | Decreto No. 74-2001 | 01/06/2001 | `[V]` |
| Reglamento de la Ley de Contratación del Estado | texto revisado a diciembre 2015 | Vigente | `[V]` |
| Ley de Compras Eficientes y Transparentes a través de Medios Electrónicos | Decreto No. 36-2013, publicada 05/08/2014 | Vigente | `[V]` |
| Compra directa de combustible para la flota estatal | Decreto 157-2022 | `[C]` si sigue vigente | `[V]` que existió |

`[P]` En 2024 la Secretaría de Transparencia anunció una **nueva Ley de Contratación del Estado**. No se encontró evidencia de aprobación. `[C]` confirmar con ONCAE si el Decreto 74-2001 sigue vigente.

## Elementos verificados

- El Decreto 36-2013 introduce **Convenios Marco, Compras Conjuntas y Subasta Inversa**, y regula el **Catálogo Electrónico**. `[V]`
- **ONCAE** es el órgano normativo; selecciona proveedores del Catálogo Electrónico e incorpora sus precios y productos. `[V]`
- Duración de cada Convenio Marco: **no menos de un año, no más de dos**. Si se firma a dos años, ONCAE debe licitar de nuevo antes del fin del primer año. `[V]`
- **HonduCompras** es el sistema único de información de contrataciones y el **único medio** por el cual se difunden y gestionan los procedimientos de las entidades cubiertas. Es público y de acceso libre. Incluye **Registro de Proveedores**. `[V]`
- Existe **Catálogo Electrónico de llantas** para vehículos. `[V]` `[C]` si hay convenio marco vigente para combustible, lubricantes, repuestos o mantenimiento.
- El **Decreto 157-2022** autorizó en 2023 la **compra directa de combustible** para la flota estatal sin sujetarse a la Ley de Contratación del Estado. `[V]` Era una medida de excepción. `[C] urgente` si sigue vigente en 2026 — determina el modelo de proveedores.

## Implicaciones de requerimiento

- **El sistema debe** mantener un **catálogo de proveedores** con RTN, razón social, número de registro en el Registro de Proveedores de ONCAE, estado (habilitado / inhabilitado / vencido) y fecha de vencimiento de la inscripción. Debe **advertir al generar una orden contra un proveedor no vigente**.
- **El sistema debe** registrar el **instrumento contractual de origen** de cada compra: contrato de suministro, orden de compra bajo convenio marco, compra menor, o autorización de excepción con su decreto o resolución. **Sin este dato el gasto es indefendible en auditoría.**
- **El sistema debe** modelar **contratos de suministro de combustible** con proveedor, vigencia, monto contratado, saldo disponible, precio o mecanismo de precio, estaciones autorizadas y consumo acumulado. Debe alertar al agotarse el saldo o vencer el contrato.
- **El sistema debe** referenciar el **número de proceso en HonduCompras** en los rubros que lo requieran, para trazabilidad hacia el portal público.
- **El sistema debe** gestionar **órdenes de mantenimiento** con vehículo, tipo (preventivo/correctivo), taller o proveedor, repuestos, mano de obra, kilometraje al ingreso, fechas de ingreso y salida, garantía y costo. Debe acumular **costo total de propiedad por vehículo**.
- **El sistema debe** soportar **alquiler de vehículos** como modalidad alterna, con su contrato, tarifa, período y responsable — aplicándole el mismo control de bitácora y combustible que a la flota propia.
- **El sistema debe** llevar el histórico de **llantas y repuestos por vehículo** (fecha de instalación, kilometraje, marca, posición): el catálogo electrónico de llantas hace de esto un rubro fiscalizado.
- **El sistema debe** producir el insumo para el **Plan Anual de Compras y Contrataciones (PACC)** a partir del consumo histórico de combustible, llantas y mantenimiento.

## Zonas grises y pendientes

- `[C] urgente` Vigencia del Decreto 157-2022 sobre compra directa de combustible.
- `[C]` Si el Decreto 74-2001 sigue siendo la ley aplicable, o fue sustituido.
- `[C]` Convenios marco vigentes aplicables a combustible, lubricantes, repuestos y mantenimiento.
- `[C]` Contratos vigentes de la institución piloto y su mecanismo de control (vale físico, cupón, tarjeta, requisición contra factura).

## Fuentes

- [Ley de Contratación del Estado, Decreto 74-2001](https://www.unodc.org/cld/uploads/res/uncac/LegalLibrary/Honduras/Laws/Ley%20de%20Contrataci%C3%B3n%20del%20Estado%20-%20Decreto%20Nro.%2074%20(2001).pdf) — consultado 2026-08-06
- [Ley de Compras Eficientes y Transparentes, Decreto 36-2013](https://www.tsc.gob.hn/web/leyes/Ley_compras_eficientes_transparentes__medios_electronicos.pdf) — consultado 2026-08-06
- [ONCAE — HonduCompras](https://oncae.gob.hn/honducompras/) — consultado 2026-08-06
- [Gobierno comprará combustible sin licitación, Decreto 157-2022](https://www.laprensa.hn/honduras/gobierno-comprara-combustible-flota-licitacion-honduras-HN11854300) — consultado 2026-08-06
