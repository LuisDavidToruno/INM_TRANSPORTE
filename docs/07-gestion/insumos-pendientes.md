# Insumos pendientes de la institución piloto

Documentos y datos que se necesitan para que el análisis se apoye en la realidad y no en suposiciones. **Los bloqueantes no se suplen con inferencias**: mientras falten, el módulo correspondiente queda con parámetros abiertos marcados `[C]`.

**Actualizado 2026-08-06** tras la revisión del PO. Ver [DP-001](decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md).

## Abiertos — bloqueantes

| # | Insumo | Para qué | Bloquea |
|---|---|---|---|
| 1 | **Reglamento interno de uso de vehículos** de la institución | Reglas de uso, autorizaciones, responsabilidades y sanciones propias | M-03, M-04, M-12 |
| 2 | **Formatos en papel vigentes**: bitácora, requisición de vehículo, salida, orden de misión, acta de entrega, control de combustible | Paridad pantalla↔papel; son el diseño de las pantallas | M-08, M-09, M-15, todo el Bloque 4 |

## Abiertos — nuevos, derivados de las decisiones del PO

| # | Insumo | Para qué | Bloquea |
|---|---|---|---|
| 16 | **Contrato de API y webhooks de ARGOS**: endpoints, autenticación, esquema de datos, eventos que emite | Es el corazón de la integración. Sin esto, M-20 es especulación | M-20, y el espejo local de autorizaciones y presupuesto |
| 17 | **Contrato de API de Talento Humano**: expediente del empleado, licencias, permisos, vacaciones, incapacidades, calendario | Padrón de motoristas y disponibilidad para asignación | M-05, M-07, M-20 |
| 18 | **Componente de mapas de ARGOS**: cuál es, cómo se reutiliza, qué licencia o servicio usa | M-19 Seguimiento en Ruta se apoya en él | M-19 |
| 19 | **Informes de Auditoría Interna o del TSC** sobre flota, combustible o uso de vehículos, si existen | Cada hallazgo describe algo que salió mal en la operación real: **son requisitos disfrazados** y valen más que cualquier entrevista | Bloque 2 |
| 20 | **Texto de la reforma al Art. 48 de la Ley de Tránsito** (2025), categorías CD y CE | Fijar la matriz licencia↔vehículo definitiva | M-05, M-07 |
| 21 | **Tarifa de peaje efectivamente vigente hoy**, confirmada con COVI-H o la SAPP | Hay contradicción entre el comunicado de la SIT del 28/02/2026 y fuentes comerciales. **No se carga ninguna tarifa sin esto** | M-18 |
| 22 | **Lista oficial de exoneraciones de peaje** — cláusula del contrato de concesión o consulta a COVI-H | Decide si un vehículo administrativo del Estado paga o no. **Es lo que define cómo se construye M-18** | M-18 |
| 23 | **OCR de dos PDF oficiales**: Ley de Tránsito (Arts. 48 y 51) y tabla de tarifas de la SAPP | Un solo trabajo resuelve la matriz licencia↔vehículo y el criterio de clasificación de peaje | M-05, M-07, M-18 |
| 24 | **¿La institución tiene tags CoviPass?** ¿A nombre de quién? ¿COVI-H emite factura fiscal en caseta o estado de cuenta empresarial? | Determina si el descargo de peajes ante el TSC es defendible. `covih.com` bloquea la consulta automatizada | M-18, M-13 |
| 25 | **¿El peaje se financia con el viático o es gasto de misión separado?** | Si va en el viático, es de ARGOS y M-18 se solapa. **Resolver antes de escribir historias de M-18** | M-18, M-20 |

## Resueltos en la revisión del 2026-08-06

| # | Insumo original | Resolución |
|---|---|---|
| 3 | Reglamento de viáticos y Acuerdo 401-2026 | **Fuera de alcance.** Lo maneja ARGOS. SIGTI solo comparte la clave para vincular una Orden de Misión con sus viáticos |
| 4 | Organigrama y niveles de autorización | Se obtienen de **ARGOS por API**, con espejo local actualizado por webhooks |
| 5 | Inventario actual de flota | **No se espera.** El catálogo se diseña a partir de cómo se registran habitualmente los vehículos de instituciones públicas hondureñas |
| 6 | Padrón de motoristas y licencias | Viene del **sistema de Talento Humano por API** |
| 7 | Contratos de combustible y mecanismo de control | **Reencuadrado.** No hay contratos que gestionar: Administración aprueba un monto en efectivo u órdenes de pago que el Jefe de Transporte solicita. Ver `PROP-01` abajo |
| 8 | Estructura presupuestaria | Se usa **la que define ARGOS** |
| 9 | Servidor on-premise y quién administra | Desarrollo **local** más **servidor de prueba** disponible, con credenciales gestionadas por el PO |
| 10 | Volumen operativo | **Alto flujo.** Genera requisitos, no solo dimensionamiento — ver M-19 abajo |
| 11 | Delegaciones y conectividad | Se reutiliza el **componente de mapas de ARGOS** |
| 12 | Informes de auditoría | Reformulado como insumo #19, con la aclaración de a qué se refiere |
| 13 | Sistemas con los que integrar | **Talento Humano, ARGOS (viáticos), Almacén.** Almacén queda diferido |
| 14 | Calendario de días hábiles y feriados | Se maneja **junto con Talento Humano** |
| 15 | Certificados de firma electrónica | **No se usa firma electrónica certificada.** Autorización interna por usuario autenticado o código gestionado por el sistema |

## PROP-01 — Propuesta para el control de combustible

El PO pidió una propuesta "práctica y segura" para el insumo 7. Se propone lo siguiente, a validar en el Bloque 1:

**Modelo: fondo asignado con trazabilidad de tres puntas.**

1. **Solicitud de fondo** — el Jefe de Transporte solicita a Administración un monto en efectivo o una cantidad de órdenes de pago, con la justificación operativa del período.
2. **Aprobación** — Administración aprueba y entrega. Queda registrado el monto, la fecha, quién aprobó y contra qué partida.
3. **Asignación** — Transporte asigna porciones del fondo a misiones o a motoristas concretos. Cada asignación tiene folio, monto, responsable y misión vinculada. El motorista **firma la recepción**.
4. **Consumo** — el motorista registra el consumo desde el campo, con galones, monto, estación, odómetro y **fotografía del comprobante**. Funciona sin conectividad.
5. **Liquidación** — al cerrar la misión se concilian: monto asignado vs. monto consumido vs. comprobantes vs. saldo devuelto.
6. **Conciliación con kilometraje** — galones consumidos vs. kilómetros recorridos vs. rendimiento esperado del vehículo, con desviación marcada **en ambas direcciones**.

**Por qué es seguro:** el punto de fuga clásico es el efectivo sin trazabilidad. Aquí ningún lempira se mueve sin quedar atado a un folio, un responsable, una misión y un odómetro. La conciliación automática con kilometraje es exactamente lo que busca el auditor del TSC.

**Por qué es práctico:** no exige contratos, ni integración con proveedores, ni tarjetas de flota. Funciona con el mecanismo que la institución ya usa hoy — solo lo registra.

**Decisiones abiertas de PROP-01:**
- `[C]` ¿El fondo se asigna por período (mensual) o por misión?
- `[C]` ¿Un motorista puede tener saldo acumulado entre misiones, o liquida cada una?
- `[C]` ¿Qué pasa con el sobrante: se devuelve o se arrastra?
- `[C]` ¿La orden de pago es un documento con folio preimpreso, o la genera el sistema?

## Cómo levantar los insumos abiertos

Los insumos 1, 2 y 19 salen de **una sesión de dos horas** con Gerencia Administrativa, el Encargado de Transporte, un motorista con años en el puesto, y Auditoría Interna. Lleva los formatos en papel a la mesa y recórrelos campo por campo: ahí aparecen las reglas que nadie escribió nunca.

Los insumos 16, 17 y 18 dependen del PO, que administra ARGOS.
