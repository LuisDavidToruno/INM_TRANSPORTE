# CE-11 — La licencia del motorista vence durante el rango de una misión ya programada

| Campo | Valor |
|---|---|
| **Módulos** | M-05 Motoristas y Habilitación, M-07 Programación y Despacho, M-08 Ejecución, M-04 Documentación |
| **Estados afectados** | `APROBADA → PROGRAMADA` (`T-08`), `PROGRAMADA → DESPACHADA` (`T-12`), `EN_RUTA` (`T-17`) |
| **Frecuencia** | Frecuente — la licencia vence en una fecha fija y las misiones largas cruzan esa fecha varias veces al año |
| **Impacto** | Legal, operativo y de auditoría |
| **Resolución** | Definida para programación y despacho · `[C]` para el vencimiento sobrevenido en ruta |

## La situación

El Jefe de Transporte programa el 26 de febrero una misión de la Dirección de Delegaciones: sale de Tegucigalpa el **3 de marzo** hacia Puerto Lempira, con retorno previsto el **11 de marzo**. Asigna el pickup 4x4 correlativo `INM-0087` y al motorista José Antonio Nolasco, categoría **C1**.

La licencia de Nolasco **vence el 7 de marzo**. El día de la salida está vigente. El día del retorno no.

Nadie lo nota, porque el fólder del motorista dice "licencia vigente" y el despachador mira la fecha de hoy contra el vencimiento, no la fecha de retorno.

El 7 de marzo, Nolasco está en La Mosquitia. Lo que sigue puede ser cualquiera de estas cosas:

- Un retén de la DNVT en la carretera de Tocoa lo detiene el 9 de marzo con licencia vencida. Multa al motorista, y el vehículo del Estado en el acta.
- Nada pasa, retorna el 11, y el hecho aparece nueve meses después cuando el TSC cruza el padrón de licencias contra las órdenes de misión del período.
- Hay un accidente el 10 de marzo. La aseguradora — si hay póliza — objeta la cobertura, y la responsabilidad se traslada a **quien autorizó la asignación**, no al motorista.

Hay tres variantes más, todas reales:

1. **Vence entre programar y despachar.** Se programó el 26 de febrero con licencia vigente hasta el 1 de marzo; el despacho es el 3 de marzo. En la programación la verificación pasó; en el despacho ya no debe pasar.
2. **Vence durante una prórroga en ruta.** La misión iba a retornar el 5 de marzo, se prorroga a pedido de la delegación hasta el 12, y esa prórroga es la que cruza el vencimiento.
3. **Renovó pero el sistema no lo sabe.** Nolasco renovó el 1 de marzo y la DNVT le entregó **comprobante de trámite**, no la licencia física. El expediente en SIGTI sigue mostrando la fecha vieja.

## Qué se hace hoy sin sistema

`[C]` La práctica de la institución no está confirmada — insumo #2 (formatos vigentes) e insumo #20.

Lo que se observa como práctica común en instituciones públicas hondureñas `[I]`:

- La fotocopia de la licencia está en el fólder del motorista, en Transporte. Se revisa **al contratar** y cuando alguien se acuerda.
- El control efectivo es **el vencimiento contra el día de la salida**, no contra el rango completo. Nadie hace la resta de la fecha de retorno.
- Cuando la licencia vence estando el motorista en ruta, se resuelve al regreso: renueva y se archiva la fotocopia nueva. **No queda constancia de que hubo días de conducción con licencia vencida.**
- El comprobante de trámite de renovación se acepta de hecho, sin política escrita que diga si habilita o no.

Ese "no queda constancia" es el punto exacto. El hecho no se oculta por mala fe: es que no hay dónde registrarlo.

## Por qué el flujo normal no lo cubre

[`RN-10`](../../01-negocio/reglas/RN-10-licencia-vigente-en-todo-el-rango.md) resuelve limpiamente la programación y el despacho: bloqueo duro si la licencia no está vigente **todo el rango**. Eso ya está.

Lo que el flujo feliz no cubre es el **vencimiento sobrevenido cuando la misión ya salió**:

- `EN_RUTA` no admite anulación (§3.4 de la máquina de estados). El vehículo está en Puerto Lempira; no hay transición que "deshaga" la misión.
- El principio **P-2** de la máquina de estados es explícito: no se bloquean los hechos consumados. El motorista va a conducir de regreso con o sin sistema, porque no hay otro modo de que el vehículo del Estado vuelva de La Mosquitia.
- La prórroga `T-17` **sí es un acto de autorización**, y ahí sí se puede bloquear — pero hoy ninguna regla lo dice.

## Regla de resolución

### 1. Antes de salir — bloqueo duro, sin excepción

En `T-08` y **revalidado íntegramente** en `T-12`, la licencia debe estar vigente el **último día del rango de la misión más la holgura de retorno**, no el día de la salida. Es [`RN-10`](../../01-negocio/reglas/RN-10-licencia-vigente-en-todo-el-rango.md) con `BD-02`, y `PC-04` de [`PR-01`](../../01-negocio/procesos/PR-01-movilizacion-institucional.md) lo aplica en los dos momentos. No hay excepción configurable — [DP-001 D-12](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md).

El bloqueo no dice "licencia vencida". Dice el dato concreto: *"La licencia N.º &lt;número&gt;, categoría C1, del motorista &lt;nombre&gt; vence el 07/03. La misión retorna el 11/03. No habilita los últimos 4 días (RN-10)."*

La salida es sustituir motorista por `T-10`, conservando la trazabilidad de la asignación original — [`RN-14`](../../01-negocio/reglas/RN-14-sustitucion-de-motorista.md).

### 2. Antes de que llegue el problema — alerta, no bloqueo

[`RN-17`](../../01-negocio/reglas/RN-17-alertas-de-vencimiento-documental.md) genera la alerta anticipada con umbrales configurables — la referencia de [NRM-06](../../01-negocio/normativa/NRM-06-transito-y-licencias.md) es 60/30/15 días `[V]`. La alerta va al motorista, al Jefe de Transporte y al Encargado de Delegación del ámbito.

Además, el sistema debe listar **las misiones ya programadas cuyo rango cruza un vencimiento próximo**, que es la lista que hoy nadie puede armar.

### 3. Durante la misión — prórroga bloqueada, hecho consumado registrado

| Situación | Qué hace el sistema |
|---|---|
| **Prórroga `T-17` que extiende el rango más allá del vencimiento** | **Se rechaza la prórroga con ese motorista.** Es un acto de autorización, no un hecho consumado: autorizar la extensión sería autorizar conducción sin habilitación. La prórroga solo procede con **relevo** de motorista habilitado, con acta de traspaso de custodia y odómetro |
| **La licencia vence en ruta dentro del rango ya autorizado** — no debería ocurrir por (1), salvo error de dato o dato corregido en ruta | Se registra el evento **`HABILITACION_VENCIDA_EN_RUTA`** en la bitácora, capturable sin conectividad: fecha del hecho, ubicación, odómetro. **No detiene la misión**, pero marca el expediente para revisión obligatoria |
| **El motorista renovó y tiene comprobante de trámite** | Se registra la renovación con adjunto fotográfico del comprobante y queda como **habilitación provisional**, con fecha límite de sustitución por el documento definitivo. `[C]` — ver abajo |

### 4. Al cerrar — no se cierra en silencio

Si el diario de la misión contiene un evento `HABILITACION_VENCIDA_EN_RUTA`, la Orden de Misión **no puede cerrar por `T-21`**: el camino es `T-22` `CERRADA_CON_HALLAZGO`, con hallazgo tipificado. [`RN-08`](../../01-negocio/reglas/RN-08-cadena-de-trazabilidad-para-cierre.md) sostiene el bloqueo de cierre.

Cerrar limpio un expediente donde hubo conducción sin habilitación es exactamente lo que convierte al sistema en el instrumento del hallazgo en vez de su defensa.

### Lo que hay que confirmar

- `[C]` **¿El comprobante de trámite de renovación de la DNVT habilita para conducir?** No lo resuelve [NRM-06](../../01-negocio/normativa/NRM-06-transito-y-licencias.md), que deja pendiente incluso el texto reformado del Art. 48 (insumo #20). Mientras no se confirme, el sistema lo registra como **habilitación provisional que no levanta el bloqueo** de `BD-02`: es la opción conservadora, y la contraria — aceptarlo — no se puede sostener ante un siniestro sin norma que la respalde.
- `[C]` **Holgura de retorno** que se suma al rango para la verificación — insumo #1, pendiente 4 de la máquina de estados.

### Regla candidata

**`RN-55` (candidata) — Habilitación vencida sobrevenida en ruta.** Enunciado propuesto:

> Cuando la habilitación de un motorista (licencia, categoría o restricción médica) pierde vigencia mientras la Orden de Misión está `EN_RUTA`, el sistema **no bloquea la ejecución** pero **registra el hecho** como evento de bitácora con fecha del hecho, ubicación y odómetro; **rechaza toda prórroga `T-17` que dependa de ese motorista**, admitiendo únicamente el relevo; y **excluye la Orden de Misión de `T-21`**, forzando el cierre por `T-22` con hallazgo tipificado.

No existe entre las 54 reglas vigentes. [`RN-10`](../../01-negocio/reglas/RN-10-licencia-vigente-en-todo-el-rango.md) gobierna la asignación; nada gobierna el hecho sobrevenido.

## Evidencia que debe quedar

Encadenado a la misma Orden de Misión, la institución debe poder mostrar al auditor del TSC:

1. El **registro de la verificación de `T-08`** con los datos concretos contra los que se evaluó: número de licencia, categoría, fecha de vencimiento consultada, versión de la matriz licencia↔vehículo, y el último día del rango contra el que se comparó.
2. El **registro de la revalidación de `T-12`**, separado del anterior, con su propia marca de tiempo. Es la defensa de quien despachó.
3. Las **alertas de vencimiento emitidas**, a quién y cuándo, con acuse.
4. Si hubo sustitución: el diario de `T-10` con el motorista saliente, el motivo tipificado y las verificaciones del entrante.
5. Si hubo relevo en ruta: el acta de traspaso de custodia con odómetro y hora, y la verificación de habilitación del motorista entrante **contra el paquete normativo congelado** en `EF-03`.
6. Si hubo vencimiento sobrevenido: el evento en bitácora, el hallazgo tipificado y la resolución de `T-22`.
7. El expediente del motorista con el **historial completo de licencias** y sus rangos de vigencia — no solo la vigente. Una licencia sobrescrita hace imposible reconstruir qué estaba vigente el 7 de marzo.

## Trazabilidad

- Reglas: [`RN-10`](../../01-negocio/reglas/RN-10-licencia-vigente-en-todo-el-rango.md), [`RN-09`](../../01-negocio/reglas/RN-09-matriz-licencia-vehiculo.md), [`RN-11`](../../01-negocio/reglas/RN-11-restricciones-medicas-del-motorista.md), [`RN-14`](../../01-negocio/reglas/RN-14-sustitucion-de-motorista.md), [`RN-17`](../../01-negocio/reglas/RN-17-alertas-de-vencimiento-documental.md), [`RN-08`](../../01-negocio/reglas/RN-08-cadena-de-trazabilidad-para-cierre.md), [`RN-43`](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md)
- Regla candidata: `RN-55` — habilitación vencida sobrevenida en ruta
- Normas: [NRM-06](../../01-negocio/normativa/NRM-06-transito-y-licencias.md) `[V]` categorías, `[C]` Art. 48 reformado
- Transiciones: `T-08`, `T-10`, `T-12`, `T-17`, `T-21`, `T-22` · Bloqueos `BD-02` · [orden-de-mision.md](../../03-arquitectura/estados/orden-de-mision.md)
- Puntos de control: `PC-04` de [`PR-01`](../../01-negocio/procesos/PR-01-movilizacion-institucional.md)
- Actores: ACT-04, ACT-05, ACT-06, ACT-10
- Casos especiales relacionados: `CE-13` (motorista no disponible), `CE-16` (vehículo a mantenimiento con misiones programadas)
- Insumos: #1 (holgura), #2 (formatos), #20 (Art. 48 reformado)
