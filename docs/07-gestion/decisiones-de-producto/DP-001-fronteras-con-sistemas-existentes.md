# DP-001 — Fronteras del sistema y relación con sistemas existentes

| Campo | Valor |
|---|---|
| **Fecha** | 2026-08-06 |
| **Decide** | Product Owner |
| **Sprint / Bloque** | Sprint 0 / revisión del Bloque 0 |
| **Estado** | Vigente |

## Principio rector

> **No replicamos lo que otro sistema ya hace.** Si una capacidad ya existe en un sistema de la institución, SIGTI se integra con él en lugar de reimplementarlo.

Esto redefine el alcance del Bloque 0 y obliga a corregir varios artefactos ya escritos.

## El ecosistema

| Sistema | Qué posee | Relación con SIGTI |
|---|---|---|
| **ARGOS** | Viáticos y gastos de viaje. Estructura presupuestaria. Niveles de autorización. Componente de mapas. | Sistema hermano, administrado por el mismo PO. Integración por **API y webhooks**. |
| **Talento Humano** | Expediente del empleado. Permisos, vacaciones, incapacidades. Calendario de feriados. | Integración por API. Fuente de verdad del personal. |
| **Almacén** | Inventario de insumos y materiales. | Integración **diferida**; no se inicia con ella. |
| **SIGTI** | Flota, motoristas, solicitudes, despacho, ejecución y seguimiento en ruta, combustible, peajes, mantenimiento, incidentes. | Este sistema. |

## Decisiones

### D-01 — Viáticos salen del alcance

**M-10 Viáticos y Gastos de Viaje deja de ser un módulo de SIGTI.** ARGOS ya lo maneja y lo hace bien.

SIGTI conserva únicamente el **vínculo**: una Orden de Misión puede tener viáticos asociados en ARGOS, y ambos sistemas comparten una clave para poder cruzarlos. SIGTI no calcula tarifas, no gestiona anticipos ni liquidaciones de viático, y no necesita la tabla de zonas y categorías.

**Consecuencia:** [NRM-03](../../01-negocio/normativa/NRM-03-viaticos.md) pasa a ser informativa. El Acuerdo 401-2026 deja de ser el riesgo #1 del proyecto — es problema de ARGOS.

**Lo que SÍ queda en SIGTI**, y no debe confundirse con viáticos: la **liquidación de los gastos operativos del viaje** que el motorista ejecuta con fondos entregados por la institución — combustible y peajes. Eso es control de flota, no viático del servidor.

### D-02 — Entran los peajes

**Nuevo módulo.** SIGTI debe manejar:

- Todos los **puntos de peaje de Honduras**, con su ubicación y operador
- Sus **tarifas vigentes**, con vigencia por rango de fechas
- La **clasificación vehicular por número de ejes** que usan los peajes hondureños
- La correspondencia entre cada vehículo de la flota y su categoría de peaje

Uso esperado: **estimar** el costo de peajes de una ruta antes de aprobar la solicitud, **registrar** el peaje efectivamente pagado durante el viaje, y **conciliar** estimado contra pagado.

Ficha normativa: `NRM-10` (en elaboración).

### D-03 — El combustible se reencuadra

SIGTI **no compra combustible ni gestiona contratos de suministro**. El mecanismo real es otro:

1. El **Jefe de Transporte** indica cuánto dinero en efectivo u cuántas órdenes de pago necesita
2. **Administración lo aprueba** y entrega el monto o las órdenes
3. Transporte **asigna** ese fondo a las misiones y motoristas
4. El motorista **consume** y comprueba
5. Transporte **liquida y concilia** contra el kilometraje recorrido

**Consecuencia:** M-09 deja de modelar proveedores, contratos, convenios marco y saldos contractuales. Modela un **fondo asignado y su consumo**: solicitud de fondo → aprobación de Administración → asignación a misión o motorista → consumo con comprobante → liquidación → conciliación con kilometraje.

**Consecuencia:** [NRM-05 Contrataciones/ONCAE](../../01-negocio/normativa/NRM-05-contrataciones-oncae.md) sale del alcance. La Ley de Contratación del Estado es problema de otros sistemas.

### D-04 — No hay firma electrónica certificada

Se descarta la firma electrónica avanzada con certificado. En su lugar, **esquema interno de autorización**: basta con la identificación del usuario autenticado o un **código especial gestionado por el sistema**.

Se conserva del diseño original: el registro completo de quién autorizó, cuándo, desde qué dispositivo y sobre qué contenido — eso sigue siendo necesario para control interno. Lo que se elimina es la infraestructura de certificados y autoridades certificadoras.

**Consecuencia:** [NRM-08](../../01-negocio/normativa/NRM-08-firma-electronica.md) se reduce al esquema interno y a la impresión con folio y verificación.

### D-05 — Patrón de integración: espejo local + webhooks

Los datos que viven en ARGOS o en Talento Humano **no se consultan en cada operación**. El patrón es:

1. **Carga inicial** completa por API
2. **Copia local** en la base de datos de SIGTI, marcada como espejo (no editable desde SIGTI)
3. **Webhooks** del sistema origen propagan los cambios cuando ocurren
4. SIGTI opera contra su copia local

Razón: estos datos cambian poco, y depender de una llamada remota en cada operación haría al sistema frágil — especialmente con el requisito de operación desconectada en campo.

Se formaliza en [ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md).

### D-06 — Seguimiento en ruta como capacidad central

El volumen operativo es alto y la institución necesita **saber en todo momento qué está pasando con cada vehículo**. Esto deja de ser un subproducto de la bitácora y pasa a ser una capacidad propia:

- Ubicación actual de cada vehículo
- Qué está haciendo y con quién anda
- A dónde va, con **uno o varios destinos**
- Cuándo se espera que termine
- **Tiempos de espera en sitio** — los períodos en que el vehículo solo está esperando, medidos y visibles
- **El motorista actualiza su propio estado**: indica cuando se movió, cuando llegó, cuando quedó en espera
- Cálculo automático de tiempos, distancias y costos derivados

Se reutiliza el **componente de mapas de ARGOS** en lugar de construir uno nuevo.

### D-07 — Disponibilidad de personal desde Talento Humano

Para asignar motoristas hay que saber quién está disponible. **Permisos, vacaciones, incapacidades y feriados vienen de Talento Humano** por API, con el patrón de espejo local.

Cuando un motorista no está disponible, el sistema debe permitir **cubrir la misión con otro** sin perder la trazabilidad de la asignación original.

### D-08 — El estado del vehículo lo registran los propios motoristas

Mantenimiento, fallas, novedades y seguimiento del estado del vehículo se capturan **por el motorista desde el campo**, no solo por el taller o la oficina. Esto refuerza el requisito de captura móvil sin conectividad.

Las **especificaciones técnicas del vehículo** se modelan a partir de cómo se registran habitualmente los vehículos de instituciones públicas hondureñas — no hay que esperar el inventario de la institución para diseñar el catálogo.

### D-09 — SIAFI queda diferido

La integración con SIAFI **no se inicia** en esta etapa. [NRM-04](../../01-negocio/normativa/NRM-04-presupuesto-siafi.md) se reduce: la estructura presupuestaria que SIGTI use es **la que defina ARGOS**.

### D-10 — Infraestructura de trabajo

Desarrollo en **local**, más un **servidor de prueba** ya disponible con credenciales gestionadas por el PO. Esto desbloquea el insumo #9 y adelanta la posibilidad de despliegue temprano.

### D-11 — SIGTI es a los vehículos lo que Talento Humano es a los empleados

Formulación del PO, y es la mejor definición del producto que tenemos:

> Así como Talento Humano cuida de todo lo referente a los empleados, SIGTI cuida de todo lo referente a los vehículos — motos, buses, pickups, camiones.

**Consecuencia:** el expediente del vehículo es una entidad de primera clase con ciclo de vida completo, no un catálogo. Incluye documentación y vencimientos, seguro, revisión, mantenimiento, fallas, historial de incidentes, especificaciones técnicas, custodios y asignaciones.

Esta frase entra en la visión de producto del Bloque 1.

### D-12 — La matriz licencia ↔ vehículo se mantiene, con bloqueo duro

El PO lo confirma explícitamente: *"nos tenemos que proteger con la ley también"*.

El sistema **bloquea** la asignación de un motorista a un vehículo cuya categoría no cubre su licencia, o cuya licencia vence dentro del rango de la misión. Sin excepción configurable: una excepción registrada sería evidencia en contra ante un siniestro.

Queda pendiente obtener el texto de la reforma al Art. 48 de la Ley de Tránsito (2025) antes de fijar la matriz definitiva de categorías CD y CE. Ver [NRM-06](../../01-negocio/normativa/NRM-06-transito-y-licencias.md).

### D-13 — Seguro y revisión mecánica sí se gestionan

Aunque no sean obligatorios por ley vigente en Honduras, **el sistema los administra igual**, como parte del cuidado integral del vehículo (D-11): póliza, vigencia, aseguradora, revisión, y alertas de vencimiento.

Se mantiene el diseño previsto: rastreables y alertables, con el **bloqueo como regla configurable** — lista para activarse si se aprueba alguna de las leyes en trámite, pero apagada por defecto.

### D-14 — Fuera de alcance: ley de datos personales pendiente y permiso IHTT de carga

- **Ley de Protección de Datos Personales** (pendiente en el Congreso desde 2018): no se diseña para anticiparla. Se conserva únicamente el control de acceso por rol y el registro de consultas, que de todos modos exige el control interno del MARCI.
- **Permiso especial del IHTT para traslado de carga**: el PO confirma que no se requiere. Sale del alcance.

## Impacto en el mapa de módulos

| Antes | Ahora |
|---|---|
| M-10 Viáticos y Gastos de Viaje | **Eliminado** → integración con ARGOS |
| M-09 Combustible (contratos, proveedores, convenios marco) | **Reencuadrado** → fondo asignado y su consumo |
| — | **Nuevo:** Peajes |
| — | **Nuevo:** Seguimiento en Ruta |
| — | **Nuevo:** Integraciones (ARGOS, Talento Humano, Almacén) |
| M-15 Formatos e Impresión con firma electrónica avanzada | **Simplificado** → autorización interna + folio y verificación |

## Pendiente de aclarar

**Insumo #12 — informes de auditoría.** El PO indicó que no entendió a qué se refería. Aclaración: se trata de los **informes de Auditoría Interna de la institución, o del Tribunal Superior de Cuentas, sobre flota, combustible o uso de vehículos**. Si existen, valen más que cualquier entrevista: cada hallazgo describe algo que salió mal en la operación real, y es un requisito disfrazado. Queda pendiente confirmar si la institución tiene alguno.

Los **pasos 3 a 5** de la revisión del Bloque 0 (fichas normativas en detalle, plantillas, subagentes) quedan pendientes.

## Artefactos a corregir

- [x] `CLAUDE.md` — mapa de módulos y restricciones
- [x] `docs/07-gestion/insumos-pendientes.md`
- [x] `docs/01-negocio/normativa/riesgos-normativos.md`
- [x] `NRM-03`, `NRM-04`, `NRM-05`, `NRM-08` — marcadas fuera de alcance o reducidas
- [x] `NRM-06` — se confirma la matriz licencia↔vehículo; sale el permiso IHTT de carga
- [x] `NRM-07` — se reduce la sección de datos personales
- [ ] `NRM-10` Peajes — en elaboración
- [x] `ADR-001` — patrón de integración
- [ ] Plantillas cuyos ejemplos usan viáticos (`regla-de-negocio.md` usa `RN-14` de tarifas de viático) — el ejemplo sigue siendo válido pedagógicamente porque el mismo patrón de vigencia temporal aplica ahora a las **tarifas de peaje**; se reemplazará al escribir las reglas reales del Bloque 1
