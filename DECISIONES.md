# Decisiones

Índice consolidado de las decisiones del proyecto, con **fecha, qué se decidió y por qué**.

Cada entrada enlaza al documento completo. **Los identificadores no se reciclan.** Una decisión que se revierte no se edita: se escribe una nueva que la supersede, y la anterior queda marcada.

| Prefijo | Tipo | Dónde vive |
|---|---|---|
| `ADR-xxx` | Decisión de arquitectura | [`docs/03-arquitectura/adr/`](docs/03-arquitectura/adr/) |
| `DP-xxx` | Decisión de producto y alcance | [`docs/07-gestion/decisiones-de-producto/`](docs/07-gestion/decisiones-de-producto/) |

---

## Decisiones de arquitectura

### ADR-000 — Diferir la selección del stack tecnológico al Sprint 2

**2026-08-06 · Vigente**

**Qué se decidió:** no nombrar lenguaje, framework ni motor de base de datos hasta el Sprint 2. Toda pregunta de tecnología se responde en términos de *capacidades requeridas*.

**Por qué:** las restricciones que iban a decidir el stack todavía se estaban descubriendo. En el primer bloque aparecieron cuatro que no eran obvias — operación offline durante días, bitácora append-only exigida por control interno, documentos imprimibles con verificación, y parámetros normativos con vigencia. Elegir antes de conocerlas produce una arquitectura que pelea contra el problema.

**Consecuencia aceptada:** no hay código ejecutable hasta el Sprint 2. Si alguien mide avance por líneas de código, el proyecto parece detenido dos sprints.

**Revisión prevista, y hoy pertinente:** *se reconsidera si la institución impone un stack por política de TI antes del Sprint 2. En ese caso el ADR documenta la restricción como dada y evalúa qué capacidades quedan comprometidas y cómo se compensan.*

→ [ADR-000](docs/03-arquitectura/adr/ADR-000-diferir-seleccion-de-stack.md)

### ADR-001 — Integración con ARGOS y Talento Humano por espejo local con webhooks

**2026-08-06 · Vigente, con una corrección posterior**

**Qué se decidió:** los datos que viven en ARGOS y en Talento Humano no se consultan en cada operación. Carga inicial completa, copia local de solo lectura, y webhooks que propagan los cambios. SIGTI opera contra su copia.

**Por qué:** el cliente de campo debe funcionar sin conectividad durante días. Consultar en línea acopla la disponibilidad de SIGTI a la de dos sistemas ajenos, y esos datos cambian poco.

**Riesgo declarado:** los webhooks se pierden, y el espejo diverge **en silencio** — que es la peor forma de fallar. Por eso el ADR obliga a reconciliación periódica completa, cola con reintento, marca de última sincronización visible y degradación explícita.

**Corrección del mismo día:** la **licencia de conducir pasó a ser dato propio de SIGTI**, no espejo. El bloqueo duro de habilitación necesita categoría, vigencia y restricciones médicas — datos que a Talento Humano no le sirven y no hay razón para asumir que mantenga. Un control de esa criticidad legal no puede depender del modelo de datos de un sistema ajeno.

→ [ADR-001](docs/03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md)

---

## Decisiones de producto y alcance

### DP-001 — Fronteras del sistema y relación con sistemas existentes

**2026-08-06 · Vigente**

**Principio rector:** *no replicamos lo que otro sistema ya hace.*

Catorce decisiones. Las que más movieron el alcance:

| | Qué se decidió | Por qué |
|---|---|---|
| **D-01** | **Los viáticos salen del alcance.** Los maneja ARGOS | Ya existe y funciona. SIGTI solo conserva la clave para vincular una Orden de Misión con sus viáticos |
| **D-02** | **Entran los peajes** — puntos, tarifas y clasificación vehicular | Requisito del PO. Ningún sistema lo cubre |
| **D-03** | **El combustible se reencuadra:** no hay contratos que gestionar | Administración aprueba un monto u órdenes de pago que Transporte solicita. SIGTI gestiona la asignación de ese fondo y su consumo |
| **D-04** | **Sin firma electrónica certificada.** Autorización interna por usuario autenticado o código gestionado por el sistema | Decisión del PO. Se conserva el registro completo de quién autorizó, cuándo y sobre qué contenido |
| **D-06** | **Seguimiento en ruta como capacidad central** — ubicación, multi-destino, tiempos de espera en sitio | El volumen operativo es alto y la institución necesita saber qué pasa con cada vehículo |
| **D-11** | *Así como Talento Humano cuida de todo lo referente a los empleados, SIGTI cuida de todo lo referente a los vehículos* | Formulación del PO. Es la mejor definición del producto que tenemos |
| **D-12** | **La matriz licencia↔vehículo se implementa con bloqueo duro, sin excepción** | *Nos tenemos que proteger con la ley también.* Una excepción registrada sería evidencia en contra ante un siniestro |
| **D-14** | **No se diseña para anticipar la ley de datos personales** pendiente en el Congreso | Se conserva control de acceso por rol y registro de consultas, que el MARCI exige igual |

→ [DP-001](docs/07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md)

### DP-002 — Segregación de funciones en delegaciones con personal insuficiente

**2026-08-06 · Provisional. Requiere ratificación del PO y pronunciamiento de Auditoría Interna**

**El problema:** el control interno exige cinco funciones en personas distintas. Una delegación con tres empleados no puede cumplirlo **por aritmética**.

**Qué se decidió:** se adopta el **escalamiento a sede** — la función incompatible la ejerce remotamente alguien de la sede central. El **régimen de excepción queda diseñado pero suspendido**, junto con las dos acciones de la matriz de permisos que lo habilitaban.

**Por qué:** es la dirección conservadora y reversible. Construir el escalamiento y descubrir que hacía falta el régimen de excepción cuesta un sprint; construir el régimen y que el TSC no lo acepte cuesta el hallazgo. Además es lo único con respaldo en las guías del MARCI: no se encontró nada que avale levantar incompatibilidades a cambio de controles compensatorios.

**Qué falta para cerrarla:** el pronunciamiento de Auditoría Interna, insumo #26. En la práctica, lo que esa unidad acepte pesa más que lo que diga la guía.

→ [DP-002](docs/07-gestion/decisiones-de-producto/DP-002-segregacion-en-delegaciones-pequenas.md)

---

## Decisiones de método

No tienen `ADR` ni `DP` propio porque viven en [`CLAUDE.md`](CLAUDE.md), pero cambiaron cómo se trabaja.

### Precedencia entre artefactos

**2026-08-06** · Incorporada tras los hallazgos `HB1-01` a `HB1-05`.

**Qué se decidió:** cuando dos artefactos se contradicen, manda el que es autoridad sobre esa materia. Estados manda en transiciones e invariantes; actores manda en incompatibilidades; la regla `RN-xx` en el resto del negocio.

**Por qué:** cuatro especialidades escribiendo en paralelo produjeron cuatro respuestas distintas a la misma pregunta, y nada decía cuál mandaba.

**Corolario:** las tablas derivadas **citan su origen en lugar de reescribirlo**. Una tabla copiada es una tabla que va a divergir — y ya había divergido en tres celdas cuando se detectó.

### El nivel de verificación nunca sube al bajar de nivel de abstracción

**2026-08-06** · Incorporada tras el hallazgo `HN1-03`.

**Qué se decidió:** ningún artefacto puede declarar un nivel de verificación superior al de la ficha normativa que cita. Y no se marca `[V]` una implicación de requerimiento escrita por el propio equipo.

**Por qué:** el patrón a evitar es la escalada silenciosa — la ficha marca `[C]`, el análisis lo repite como `[P]`, la regla lo declara `[V]`, y el código lo implementa como obligación legal. Nadie mintió en ningún paso, y el resultado es falso.

**Ya se aplicó tres veces contra nuestro propio trabajo:** en `RN-10`, en `RN-21`, y el 2026-08-24 dentro de `NRM-10`, que atribuía `[V]` un fundamento legal que ninguna fuente sostenía.

### Los mockups los produce diseño, no el equipo de documentación

**2026-08-18**

**Qué se decidió:** el Bloque 4 entrega un **brief ejecutable** en vez de wireframes. Los mockups los hace un diseñador con ese material.

**Por qué:** decisión del PO. Y funcionó: se produjeron 41 pantallas correctas sin que el diseñador leyera los 357 documentos, y devolvió 10 hallazgos sobre la documentación que ninguna revisión interna había encontrado.
