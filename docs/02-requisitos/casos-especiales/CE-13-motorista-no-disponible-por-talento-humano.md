# CE-13 — El motorista no está disponible: permiso, vacaciones o incapacidad registrados en Talento Humano

| Campo | Valor |
|---|---|
| **Módulos** | M-05 Motoristas y Habilitación, M-07 Programación y Despacho, M-20 Integraciones, M-16 Sincronización |
| **Estados afectados** | `APROBADA → PROGRAMADA` (`T-08`), `PROGRAMADA` (`T-10`), `PROGRAMADA → DESPACHADA` (`T-12`), `EN_RUTA` (`T-17`) |
| **Frecuencia** | Frecuente — la ausencia sobrevenida es semanal en un padrón de más de diez motoristas |
| **Impacto** | Operativo, legal y de auditoría |
| **Resolución** | Definida · `[C]` en tres puntos: catálogo de tipos de ausencia, personal por contrato y baja con misión en curso |

## La situación

Lunes 6 de abril, 5:50 de la mañana. El despachador tiene lista la Orden de Misión N.º `TGU-2026-0412`: pickup `INM-0034`, motorista Óscar Mejía, salida hacia Danlí a las 6:30 con dos técnicos de la Dirección de Delegaciones.

Mejía **no llega**. A las 6:20 llama su esposa: se cayó el sábado, tiene incapacidad del IHSS por siete días y el papel ya lo entregó en Talento Humano el domingo por la noche.

El sistema espejó la incapacidad a las 2:00 de la madrugada. La Orden de Misión, sin embargo, ya está `PROGRAMADA` desde el jueves, con el fondo de combustible propuesto y la ruta calculada.

Las variantes que se ven en la misma semana:

1. **Vacaciones aprobadas en Talento Humano que Transporte no vio.** Se programa a alguien que empieza vacaciones el miércoles y la misión retorna el viernes.
2. **Permiso de dos horas**, de 8:00 a 10:00, dentro de una misión de tres días a Santa Rosa de Copán. ¿Bloquea o no?
3. **El espejo no sincroniza desde hace seis días** porque el servicio de Talento Humano está caído. Nadie sabe si Mejía está de vacaciones.
4. **Se registra la incapacidad cuando el motorista ya está en Juticalpa**, en misión, con la bitácora abierta.
5. **Baja del motorista** — renuncia o traslado — con dos misiones futuras ya programadas a su nombre.
6. **Motorista que Talento Humano no conoce**: personal por contrato o apoyo de otra institución.

## Qué se hace hoy sin sistema

`[C]` No confirmado con la institución — insumo #2 e insumo #17 (contrato de API de Talento Humano).

Práctica común observada `[I]`:

- La disponibilidad de motoristas vive en **una pizarra o un cuaderno en Transporte**. Talento Humano vive en otro edificio y en otro sistema.
- La ausencia se entera **el día de la salida**, cuando el motorista no llega. La reacción es tomar "al que esté" y salir.
- **El que esté** es la parte peligrosa: se despacha a un motorista que no fue verificado contra la categoría del vehículo, porque la urgencia manda. Ahí es donde se juntan este caso y `CE-11`.
- El cambio de motorista **no se anota en la orden impresa**. La orden dice Mejía, conduce otro, y la bitácora la firma quien conduce. El expediente queda con dos nombres que no concuerdan.
- Cuando la incapacidad aparece después, nadie corrige nada.

La regla que nadie escribió: *"si el motorista no llega, sale otro y el papel se arregla después"*. Y el papel casi nunca se arregla.

## Por qué el flujo normal no lo cubre

[`RN-12`](../../01-negocio/reglas/RN-12-disponibilidad-del-motorista.md) y `BD-10` bloquean la asignación contra el espejo de Talento Humano. Eso resuelve el caso limpio: al programar, el sistema ve la ausencia y no deja asignar.

Lo que el flujo feliz no cubre:

- **La ausencia sobrevenida.** La misión ya está `PROGRAMADA` — o peor, `DESPACHADA` con el fondo entregado — cuando aparece el dato. El bloqueo llega tarde.
- **El espejo desactualizado.** [`RN-48`](../../01-negocio/reglas/RN-48-datos-espejo-de-solo-lectura.md) prohíbe editar el espejo desde SIGTI y [`RN-50`](../../01-negocio/reglas/RN-50-degradacion-por-sincronizacion-detenida.md) degrada al superarse el umbral. Pero la operación no se detiene porque un servicio esté caído: el vehículo tiene que salir a Danlí.
- **La ausencia en ruta.** `EN_RUTA` no admite anulación. El motorista está en Juticalpa.
- **La sustitución de urgencia** es el momento de máximo riesgo de saltarse `BD-02` (licencia) y `BD-06` (segregación), porque se hace con el motor encendido.

## Regla de resolución

### 1. Bloqueo al asignar y revalidación al despachar

[`RN-12`](../../01-negocio/reglas/RN-12-disponibilidad-del-motorista.md) con `BD-10` en `T-08`, y **revalidación completa en `T-12`** — igual que la licencia. Entre programar el jueves y despachar el lunes se registró una incapacidad, y el despacho es el último punto donde el sistema puede evitar el daño.

El mensaje **no expone el motivo médico**: *"El motorista Óscar Mejía registra ausencia de tipo incapacidad del 04/04 al 10/04. La misión va del 06/04 al 06/04."* El diagnóstico no es asunto de Transporte.

### 2. El efecto de cada tipo de ausencia es catálogo, no código

El catálogo `tipo_ausencia` (M-02) define, **por tipo y con vigencia**, si la ausencia bloquea, advierte o es indiferente — [`RN-39`](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md). El permiso de dos horas de la variante 2 se resuelve ahí, no discutiendo caso por caso.

`[C]` El catálogo lo define la institución. Lo que **no** es configurable es el bloqueo cuando el tipo está marcado como bloqueante.

Lo que sí fija el análisis: **una ausencia de un solo día dentro de una misión de cinco bloquea igual** — [`RN-12`](../../01-negocio/reglas/RN-12-disponibilidad-del-motorista.md), casos límite. La misión requiere al motorista todos los días.

### 3. Ausencia sobrevenida sobre misión `PROGRAMADA` o `DESPACHADA`

| Estado al aparecer la ausencia | Qué hace el sistema |
|---|---|
| `PROGRAMADA` | La asignación se marca **en conflicto**, se notifica a ACT-04 y ACT-05, y **el despacho queda bloqueado** hasta resolver. La salida es `T-10` sustitución de motorista, que **revalida todas las habilitaciones del entrante** — [`RN-14`](../../01-negocio/reglas/RN-14-sustitucion-de-motorista.md) — o `T-11` liberación de recursos si no hay a quién asignar |
| `DESPACHADA`, sin salida | No se puede sustituir sin revertir. El camino es devolver lo entregado y volver a `PROGRAMADA`: acta de devolución del fondo firmada por ACT-06 y ACT-07, devolución de la custodia del vehículo con odómetro, y **anulación de los folios emitidos** — no se reciclan. Reasignado el motorista, se despacha de nuevo con folio nuevo. Si hubo consumo, `T-15` no está disponible y el camino es `T-16` |
| `EN_RUTA` | **No se cancela la misión desde el escritorio.** Se notifica a ACT-04 y al motorista, se registra el conflicto en el expediente y se decide operativamente: relevo por `T-17` con acta de traspaso de custodia, o retorno anticipado por `T-18`. El expediente **conserva la contradicción**: es información de auditoría, no un error a esconder |

La sustitución **conserva la trazabilidad de la asignación original** — [DP-001 D-07](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md). El diario muestra a quién se había asignado, por qué se cambió y a quién se asignó. Nunca se sobrescribe el nombre en la orden.

### 4. Sustitución de urgencia: rápida, pero con los mismos bloqueos

El sistema debe ofrecer, desde la misma pantalla del bloqueo, la **lista de motoristas elegibles** para esa misión concreta: habilitados por categoría para ese vehículo (`BD-02`), sin ausencia en el rango (`BD-10`), sin misión traslapada ([`RN-13`](../../01-negocio/reglas/RN-13-sin-doble-asignacion.md)) y compatibles con la segregación de la misión ([`RN-01`](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md)).

Que la sustitución sea de dos clics es un requisito de control, no de comodidad: **si el camino correcto es lento, el despachador usa el camino incorrecto** — sacar al vehículo con quien esté y arreglar el papel después.

`BD-02` y `BD-06` **no se levantan por urgencia**. No hay excepción configurable — [DP-001 D-12](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md).

### 5. Espejo caído

Se aplica [`RN-50`](../../01-negocio/reglas/RN-50-degradacion-por-sincronizacion-detenida.md): superado el umbral configurable, la asignación se bloquea con mensaje explícito y con la **marca de última sincronización** visible. Es preferible detener el despacho a asignar contra datos de personal que ya no reflejan la realidad — [`RN-12`](../../01-negocio/reglas/RN-12-disponibilidad-del-motorista.md), casos límite.

Cuando la institución habilite la operación degradada, la advertencia se registra en el diario y **se imprime en la Orden de Misión**, para que quede en el documento físico que el dato de disponibilidad no estaba verificado.

### Lo que hay que confirmar

- `[C]` **Catálogo `tipo_ausencia` y el efecto de cada tipo** — quién lo define y con qué vigencia.
- `[C]` **Motorista que no existe en Talento Humano** — personal por contrato, apoyo de otra institución. Si la figura existe, su disponibilidad se gestiona con registro propio en SIGTI y se marca visiblemente como *disponibilidad no verificada contra Talento Humano*, incluido en el documento impreso. Insumo #17.
- `[C]` **Qué ocurre con un empleado dado de baja con misiones abiertas** — pendiente 13 de la máquina de estados. Lo que sí está fijado: la bitácora y el consumo **ya registrados permanecen a su nombre**; reasignar retroactivamente la ejecución sería falsear el registro.

No hace falta ninguna regla nueva: [`RN-12`](../../01-negocio/reglas/RN-12-disponibilidad-del-motorista.md), [`RN-13`](../../01-negocio/reglas/RN-13-sin-doble-asignacion.md), [`RN-14`](../../01-negocio/reglas/RN-14-sustitucion-de-motorista.md), [`RN-48`](../../01-negocio/reglas/RN-48-datos-espejo-de-solo-lectura.md), [`RN-49`](../../01-negocio/reglas/RN-49-reconciliacion-periodica-del-espejo.md) y [`RN-50`](../../01-negocio/reglas/RN-50-degradacion-por-sincronizacion-detenida.md) cubren el caso completo.

## Evidencia que debe quedar

1. El **registro de la verificación `BD-10`** en `T-08` y su revalidación en `T-12`, con el tipo y rango de ausencia consultados y la **marca de última sincronización del espejo** en ese momento.
2. Si hubo sustitución: el diario de `T-10` con motorista saliente, motivo tipificado, motorista entrante y **las verificaciones completas del entrante** — licencia, categoría, disponibilidad, no traslape.
3. La **Orden de Misión impresa** con el motorista que efectivamente condujo. Si se reimprimió, el folio anulado y el nuevo, con referencia cruzada.
4. Si hubo devolución desde `DESPACHADA`: acta de devolución del fondo firmada, acta de recepción del vehículo con odómetro, y los folios anulados con referencia a la misión — [`RN-04`](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md).
5. Si la ausencia apareció con el motorista en ruta: el registro del conflicto, la notificación, y la decisión operativa con su autor. **La contradicción se conserva**, no se limpia.
6. Si se operó con el espejo degradado: la advertencia registrada y su reflejo en el documento impreso.
7. El **calendario de disponibilidad del padrón** del período: ausencias, misiones asignadas y descansos, para que el auditor pueda cruzar contra el registro de asistencia de Talento Humano sin encontrar contradicciones.

## Trazabilidad

- Reglas: [`RN-12`](../../01-negocio/reglas/RN-12-disponibilidad-del-motorista.md), [`RN-13`](../../01-negocio/reglas/RN-13-sin-doble-asignacion.md), [`RN-14`](../../01-negocio/reglas/RN-14-sustitucion-de-motorista.md), [`RN-09`](../../01-negocio/reglas/RN-09-matriz-licencia-vehiculo.md), [`RN-10`](../../01-negocio/reglas/RN-10-licencia-vigente-en-todo-el-rango.md), [`RN-01`](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md), [`RN-04`](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md), [`RN-48`](../../01-negocio/reglas/RN-48-datos-espejo-de-solo-lectura.md), [`RN-49`](../../01-negocio/reglas/RN-49-reconciliacion-periodica-del-espejo.md), [`RN-50`](../../01-negocio/reglas/RN-50-degradacion-por-sincronizacion-detenida.md), [`RN-39`](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md)
- Reglas candidatas: ninguna — el caso se resuelve con las reglas vigentes
- Decisiones: [DP-001 D-07](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md), [ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md)
- Transiciones: `T-08`, `T-10`, `T-11`, `T-12`, `T-15`, `T-16`, `T-17`, `T-18` · Bloqueos `BD-02`, `BD-06`, `BD-10` · [orden-de-mision.md](../../03-arquitectura/estados/orden-de-mision.md)
- Puntos de control: `PC-10`, `PC-04` de [`PR-01`](../../01-negocio/procesos/PR-01-movilizacion-institucional.md)
- Actores: ACT-04, ACT-05, ACT-06, ACT-10, ACT-17
- Casos especiales relacionados: `CE-11` (licencia vencida), `CE-12` (competencia por flota — un motorista menos agrava la escasez)
- Insumos: #2, #17
