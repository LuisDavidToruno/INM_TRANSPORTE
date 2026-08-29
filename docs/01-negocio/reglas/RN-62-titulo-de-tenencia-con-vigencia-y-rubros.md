# RN-62 — Todo vehículo de la flota tiene título de tenencia con régimen, titular, vigencia y rubros asumidos; ninguna misión excede esa vigencia

| Campo | Valor |
|---|---|
| **Módulos** | M-03, M-04, M-11, M-13, M-07 |
| **Origen** | Caso especial [CE-15](../../02-requisitos/casos-especiales/CE-15-vehiculo-en-comodato-o-alquilado.md) · Norma [NRM-02](../normativa/NRM-02-bienes-del-estado.md) |
| **Verificación** | `[P]` el deber de registrar y custodiar los bienes en tenencia del Estado — [NRM-02](../normativa/NRM-02-bienes-del-estado.md), que además deja abierto qué régimen aplica a comodato y alquiler. `[I]` el modelado del título: implicación de requerimiento del equipo |
| **Tipo** | Bloqueo duro |
| **Configurable** | Sí — catálogo `regimen_de_tenencia` y los parámetros de bloqueo por régimen |

## Enunciado

Todo vehículo de la flota **debe** tener un **título de tenencia** con:

- **Régimen** del catálogo configurable: propiedad, comodato, alquiler, donación en trámite, asignación por otra institución
- **Titular** — quién es el propietario o cedente
- **Documento adjunto** — convenio de comodato, contrato de alquiler, acta de donación, resolución
- **Rango de vigencia** con fecha de fin, salvo el régimen de propiedad
- **Rubros asumidos**: quién paga combustible, mantenimiento, llantas, seguro, peajes, multas y daños

**Sin título vigente el vehículo no se habilita en la flota**, y **ninguna misión se programa ni se despacha si su ventana excede la vigencia del título** — bloqueo duro, con el mismo patrón de [`RN-10`](RN-10-licencia-vigente-en-todo-el-rango.md).

## Justificación

Un vehículo en comodato o alquilado se opera igual que uno propio, pero **su expediente no es igual**: tiene una fecha en la que deja de ser nuestro, un tercero que responde por ciertos rubros, y un costo de tenencia que hoy no aparece en ningún cálculo de costo por kilómetro.

Sin el título, el sistema dirige órdenes de trabajo al presupuesto de la institución por rubros que el contrato cubría, y produce un costo por vehículo que subestima sistemáticamente el de la flota alquilada frente a la propia — que es justo la comparación que una institución necesita para decidir si le conviene alquilar.

Y sin bloqueo por vigencia, se despacha una misión de cinco días con un contrato que vence en tres: el vehículo hay que devolverlo a mitad de misión, o se retiene un bien ajeno sin título.

## Condiciones de aplicación

Aplica a todo vehículo de la flota, incluidos los propios — cuyo título es la propiedad, con documento y sin fecha de fin.

Aplica al **préstamo entre dependencias de la misma institución** solo en cuanto a la tenencia operativa; el instrumento es el de [`RN-63`](RN-63-prestamo-de-vehiculo-como-expediente-del-bien.md), no un título nuevo.

## Comportamiento esperado

1. La ficha del vehículo muestra régimen, titular, vigencia, días restantes y la matriz de rubros asumidos.
2. Al programar o despachar, el sistema compara la ventana de la misión —con la holgura de retorno configurada `[C]` insumo #1— contra la vigencia del título y bloquea con la fecha concreta.
3. M-11 dirige la **orden de trabajo** y M-13 el **cargo** según el rubro: lo que cubre el contrato no se imputa al presupuesto de la institución, y el sistema deja constancia de esa derivación.
4. El **canon de alquiler y los costos asociados al comodato** se prorratean e integran al costo total por vehículo y por kilómetro, **distinguiendo régimen** en todo reporte comparativo.
5. Las alertas de vencimiento ([`RN-17`](RN-17-alertas-de-vencimiento-documental.md)) cubren el título y la póliza del titular, con destinatario y acuse.
6. El **parámetro de bloqueo por póliza vencida** ([`RN-16`](RN-16-seguro-y-revision-mecanica.md)) admite valor distinto **según régimen de tenencia**: la institución puede exigir póliza vigente a un vehículo alquilado y no a uno propio, porque el contrato normalmente la obliga.
7. La salida del vehículo de la flota se registra como **fin de tenencia** con acta de devolución y odómetro obligatorio, **nunca como descargo ni como baja patrimonial**: declarar *dado de baja* un bien ajeno es un asiento falso. Todo el historial —bitácoras, consumos, incidentes y costos del período— **se conserva**: no se va con el vehículo.

## Casos límite

- **Estado terminal `RETIRADO_DE_FLOTA`.** Se reportó desde [CE-15](../../02-requisitos/casos-especiales/CE-15-vehiculo-en-comodato-o-alquilado.md) como ampliación necesaria cuando la [máquina de estados](../../03-arquitectura/estados/orden-de-mision.md) todavía no lo tenía. **Ya existe**: §10.2 lo publica como terminal alcanzable desde `NO_DISPONIBLE` (`W-16b`), con acta de devolución obligatoria, y la corrección `HB3-17` de ese mismo documento fija cuál de los dos terminales corresponde — el descargo es de bienes propios y el retiro, de ajenos. Esta regla es la que aporta el dato con el que esa verificación juzga. **Hallazgo cerrado.**
- **Unidad sustituta entregada por el arrendador.** Se da de alta como **vehículo nuevo bajo el mismo título**, con serie de odómetro propia y acta de sustitución; las misiones programadas sobre la unidad anterior se revalidan por [`RN-61`](RN-61-sustitucion-de-vehiculo-recalcula-valores-congelados.md).
- **Comodato prorrogado verbalmente.** No existe para el sistema. La vigencia es la del documento; sin adenda adjunta, el título vence y el bloqueo opera. Es incómodo y es correcto.
- **Vehículo alquilado sin franjas ni leyenda del Estado.** `[C]` [NRM-02](../normativa/NRM-02-bienes-del-estado.md) deja abierto si le aplica el régimen de rotulación e identificación. Mientras no se confirme, la constatación de [`RN-18`](RN-18-rotulacion-del-vehiculo-del-estado.md) se registra con el resultado real y sin bloqueo.
- **Exoneración de peaje del titular.** No se hereda: exige fundamento y vigencia registrados para **ese** vehículo ([`RN-38`](RN-38-exoneracion-de-peaje.md)). Sin ellos, paga.

## Trazabilidad

- Norma: [NRM-02](../normativa/NRM-02-bienes-del-estado.md) `[P]` — con la zona gris de comodato y alquiler explícitamente abierta
- Reglas relacionadas: [RN-10](RN-10-licencia-vigente-en-todo-el-rango.md), [RN-16](RN-16-seguro-y-revision-mecanica.md), [RN-17](RN-17-alertas-de-vencimiento-documental.md), [RN-18](RN-18-rotulacion-del-vehiculo-del-estado.md), [RN-38](RN-38-exoneracion-de-peaje.md), [RN-58](RN-58-regimen-de-uso-del-vehiculo.md), [RN-61](RN-61-sustitucion-de-vehiculo-recalcula-valores-congelados.md), [RN-63](RN-63-prestamo-de-vehiculo-como-expediente-del-bien.md)
- Casos especiales: [CE-15](../../02-requisitos/casos-especiales/CE-15-vehiculo-en-comodato-o-alquilado.md) — candidatas `RN-c:titulo-de-tenencia-con-vigencia`, `RN-c:mision-dentro-de-la-vigencia-del-titulo`, `RN-c:responsabilidad-economica-por-rubro-segun-tenencia`, `RN-c:costo-de-tenencia-en-el-costo-por-kilometro`, `RN-c:fin-de-tenencia-no-es-descargo`, `RN-c:bloqueo-de-seguro-configurable-por-regimen`
- Insumos pendientes: #1 holgura de retorno · régimen de rotulación de vehículos ajenos
