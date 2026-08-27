# RN-01 — Un mismo servidor no puede ejercer dos funciones de control sobre la misma Orden de Misión, ni conducirla si ejerció alguna de ellas

| Campo | Valor |
|---|---|
| **Módulos** | M-01, M-06, M-07, M-08, M-09, M-13 |
| **Origen** | Norma [NRM-01](../normativa/NRM-01-control-interno-tsc.md) — MARCI, norma de segregación de funciones incompatibles. Tabla de incompatibilidades `I-01` a `I-11` de [actores-y-roles §5.2](../actores-y-roles.md) — **artefacto autoridad en materia de actores e incompatibilidades** |
| **Verificación** | `[C]` la numeración de la norma MARCI — ver la nota de abajo. **No** invocar TSC-NOGECI V-07 para esta regla. `[I]` la incorporación del par motorista × función de control (`I-11`): es práctica de control interno recogida por `actores-y-roles`, no articulado citable |
| **Tipo** | Bloqueo duro |
| **Configurable** | **No.** Es mandato de control interno del Estado |

## Nota de corrección — hallazgo `HB1-02`

> **Qué estaba mal.** La versión anterior excluía la conducción de las funciones de control y su caso límite decía que el motorista podía ser también quien autorizó (*"conducir no es función de control"*). [actores-y-roles §5.2](../actores-y-roles.md) declara `I-11` — **motorista sobre su propia misión** — **bloqueo duro del núcleo irreductible que "no se levanta nunca"**, y esta regla no lo implementaba. El caso que pasaba sin bloqueo: el encargado autoriza el lunes, el motorista titular se incapacita el martes, y él se asigna a sí mismo como motorista.
>
> **Qué manda.** Por la precedencia entre artefactos de `CLAUDE.md`, `actores-y-roles.md` es la autoridad en incompatibilidades. Se corrige esta regla, no la autoridad. La corrección espejo en [RN-14](RN-14-sustitucion-de-motorista.md) elimina el `[C]` que degradaba el mismo caso a advertencia.
>
> **Nota de hallazgo abierta, no resuelta aquí.** La matriz §3.3 de [orden-de-mision.md](../../03-arquitectura/estados/orden-de-mision.md) marca además `Conduce × Programa` y `Conduce × Cierra` como incompatibles, pares que **no existen** en la tabla `I-01` a `I-17`. Esta regla implementa `I-11` tal como la escribe la autoridad — autorizar, despachar, entregar fondo y liquidar — y **no** incorpora esos dos pares. Queda como divergencia a resolver entre ambos artefactos.

## Enunciado

Sobre una misma Orden de Misión, **ninguna persona puede ejercer más de una** de las siguientes funciones de control:

| # | Función | Actor típico | Par `I-nn` que la involucra |
|---|---|---|---|
| 1 | **Solicitar** | ACT-02 Solicitante | `I-01`, `I-02`, `I-03`, `I-04` |
| 2 | **Autorizar** | ACT-03 Jefatura Inmediata / ACT-08 Gerencia Administrativa | `I-01`, `I-05`, `I-06`, `I-07` |
| 3 | **Despachar** | ACT-05 Encargado de Despacho | `I-02`, `I-05`, `I-08`, `I-09` |
| 4 | **Entregar combustible** | ACT-07 Encargado de Combustible | `I-03`, `I-06`, `I-08`, `I-10` |
| 5 | **Liquidar** | ACT-04 Jefe de Transporte / ACT-08 Gerencia Administrativa | `I-04`, `I-07`, `I-09`, `I-10` |

**Y una sexta relación, de sentido único:** quien **conduce** la misión (ACT-06 Motorista, o cualquier persona que la conduzca) **no puede haber ejercido, sobre esa misma misión**, ninguna de las funciones 2 a 5 — autorizar, despachar, entregar el combustible o liquidar. Es el par `I-11`, **núcleo irreductible**: bloqueo duro que **no se levanta por régimen de excepción, ni por delegación, ni por emergencia, ni por resolución de la máxima autoridad**.

La incompatibilidad es **simétrica en el tiempo**: da igual si primero autorizó y después se asignó como motorista, o si primero condujo y después pretende liquidar. El sistema bloquea el segundo acto, sea cual sea.

**Conducir y solicitar sí son compatibles** (`I-11` no incluye solicitar): un motorista puede pedir el traslado que él mismo va a ejecutar.

La verificación se hace **por persona**, no por rol: que un usuario tenga dos roles asignados no lo habilita a ejercer dos funciones sobre el mismo expediente.

## Nota sobre la cita normativa — corregida por hallazgo HN1-02

> Una versión anterior de esta regla atribuía la segregación de funciones a **TSC-NOGECI V-07**. **Es incorrecto.**
>
> [NRM-01](../normativa/NRM-01-control-interno-tsc.md) define V-07 como *"Autorización y Aprobación de Transacciones y Operaciones"* — que sustenta [RN-32](RN-32-entrega-de-combustible-contra-orden-de-mision.md), no esta regla. La misma ficha registra `[C]` que el MARCI contiene una norma de segregación de funciones **distinta**, cuyo código y título exactos deben tomarse del MARCI impreso de la institución. `[P]` V-08 es *"Documentación de Procesos y Transacciones"*, así que tampoco es esa.
>
> **La exigencia de segregación existe** y está recogida en la ficha como implicación de requerimiento. **Lo que no está verificado es su numeración.** Hasta obtener el MARCI impreso (insumo #23, OCR), esta regla se sostiene sobre la exigencia, no sobre un número.
>
> Un número de norma inventado o mal atribuido entra al código y nadie vuelve a cuestionarlo. Si el TSC pregunta bajo qué norma se bloquea, la respuesta tiene que ser correcta.


## Mapa de cobertura de `I-01` a `I-17` — hallazgo `HB1-18`

> **Qué estaba mal.** Ninguna de las reglas referenciaba la tabla `I-01` a `I-17` de [actores-y-roles §5.2](../actores-y-roles.md), que [`mapa-de-procesos` §7](../mapa-de-procesos.md) y [`PR-01`](../procesos/PR-01-movilizacion-institucional.md) declaran **fuente de verdad** en incompatibilidades. El núcleo irreductible que la autoridad dice que no se levanta nunca **no lo implementaba ninguna regla**.
>
> Este mapa lo cierra en un solo lugar y **declara los huecos** en vez de dejarlos implícitos. Si un par no tiene regla, aquí se ve.

| Par | Quién lo implementa |
|---|---|
| `I-01` a `I-10` — los diez pares entre las cinco funciones | **Esta regla.** Es exactamente lo que enumera la tabla de arriba |
| `I-11` — motorista × autoriza / despacha / entrega fondo / liquida su propia misión | **Esta regla**, sexta relación · [`RN-14`](RN-14-sustitucion-de-motorista.md) en la sustitución · [`RN-57`](RN-57-habilitacion-de-quien-efectivamente-conduce.md) sobre quien efectivamente conduce |
| `I-13` — `ACT-01` Administrador × autorizar, aprobar fondo o liquidar | [`RN-39`](RN-39-parametros-normativos-con-vigencia.md), doble control carga↔aprobación |
| **`I-12`** — `ACT-12` Auditor Interno × cualquier rol ejecutor | ⚠️ **Sin regla.** `actores-y-roles` le fija *«solo lectura y exportación, sin excepciones»* como límite absoluto, y ninguna `RN-xx` lo sostiene |
| **`I-14`** — quien emite la Orden de Misión × liquida esa misma misión | ⚠️ **Sin regla.** No es redundante con `I-09`: emitir no es despachar |
| **`I-15`** — `ACT-13` Custodio × autoriza la salida de su propio vehículo | ⚠️ **Sin regla.** [`RN-22`](RN-22-custodia-del-vehiculo.md) define la custodia y no toca esta incompatibilidad |
| **`I-16`** — quien ordena el mantenimiento × recibe conforme el trabajo | ⚠️ **Sin regla.** Es de `M-11`, que no se ha trabajado — ver el [`README`](README.md) |
| **`I-17`** — `ACT-14` Encargado de Bienes × aprueba el descargo del bien | ⚠️ **Sin regla.** Ya lo señalaba `HN1-15`, y sigue abierto |

**Cinco pares sin regla, y no son todos iguales.** `I-16` es una postergación declarada: pertenece a `M-11`. Los otros cuatro son huecos: la incompatibilidad está escrita en la autoridad, nadie la implementa, y **quien programe el bloqueo no va a encontrarla**.

`I-12` es el que más pesa. Un auditor con capacidad de ejecutar deja de ser auditor, y hoy eso solo lo dice un documento de actores — no una regla que alguien pueda probar.

## Justificación

[NRM-01](../normativa/NRM-01-control-interno-tsc.md) establece que el sistema **debe** implementar segregación de funciones por rol **como bloqueo duro y no como advertencia**. Es la defensa estructural contra el fraude de flota: quien puede solicitar, autorizar y liquidar su propio viaje puede fabricar un consumo de combustible completo sin que ningún registro lo contradiga.

**Por qué conducir sí importa.** `I-11` lo nombra por su nombre: *autoliquidación, el vector de fraude clásico en combustible* `[I]`. Quien autorizó la misión y además la conduce es la única persona que declara los kilómetros, el consumo y la ruta de un gasto que él mismo aprobó. No hay segundo par en ninguna parte de la cadena: el odómetro, el galonaje y el destino los aporta la misma mano que firmó la autorización. Lo mismo vale para quien despachó — nadie verificó su salida — y para quien entregó el fondo — nadie constata cuánto se llevó.

Una advertencia que se puede saltar no es un control: ante el TSC, el registro de la advertencia ignorada es prueba de que la institución sabía y aun así permitió la operación.

## Condiciones de aplicación

Aplica a **toda** Orden de Misión, en cualquier dependencia o delegación, sin importar el monto, la distancia ni la urgencia.

**No aplica** entre funciones de control distintas ejercidas sobre **órdenes de misión distintas**: el mismo servidor puede autorizar la misión A y solicitar la misión B.

**No aplica** a las funciones de consulta ni de auditoría (ACT-12 Auditor Interno), que no son funciones de control en el sentido de esta regla.

**El registro de bitácora en ruta no es, por sí solo, función de control.** Lo que la regla bloquea no es registrar la bitácora: es **conducir la misión** habiendo ejercido sobre ella una función de control. Un ACT-10 Encargado de Delegación que digita en diferido la bitácora de otro motorista no incurre en `I-11`; el que se sienta al volante de la misión que autorizó, sí.

**La emergencia no es excepción.** Si no hay personal para cubrir las funciones, se aplica [RN-02](RN-02-escalamiento-de-autorizacion.md) — escalamiento — no la dispensa. Es lo que ratifica [DP-002](../../07-gestion/decisiones-de-producto/DP-002-segregacion-en-delegaciones-pequenas.md): se construye el **Nivel 1, escalamiento a sede**; el régimen de excepción del Nivel 2 **no se implementa**. Y aunque llegara a implementarse, `I-11` seguiría fuera de su alcance: el núcleo irreductible no se levanta en ningún escenario.

## Comportamiento esperado

1. Antes de registrar cualquier acto de control **y antes de asignar o sustituir al motorista**, el sistema compara la identidad de la persona actuante o entrante contra las identidades ya registradas en las demás funciones de la misma Orden de Misión.
2. Si coincide, **bloquea** con: *"Usted registró la solicitud de esta Orden de Misión N.º <folio>. Por segregación de funciones (RN-01) no puede autorizarla. Corresponde a <nivel superior> conforme a RN-02."* Para el par `I-11`: *"Usted autorizó la Orden de Misión N.º <folio> el <fecha>. Por incompatibilidad I-11 (RN-01) no puede ser asignado como motorista de esa misión. Es núcleo irreductible: no admite excepción."*
3. El intento bloqueado **se registra** en la pista de auditoría con usuario, función pretendida, **par `I-nn` detectado**, orden, fecha y hora. Un patrón de intentos repetidos es en sí un hallazgo.
4. El sistema expone un reporte de **matriz de segregación por expediente**: qué persona ejerció qué función en cada orden, exportable para auditoría.
5. Al cerrar la orden, se verifica de nuevo la matriz completa. Una violación detectada en el cierre — posible si una persona fue reasignada de puesto — fuerza `CERRADA_CON_HALLAZGO`.

## Casos límite

- **Delegación única con un solo servidor.** En una delegación pequeña puede no existir personal suficiente. La regla **no se relaja**: la función faltante se ejerce por el nivel correspondiente de la dependencia matriz, en línea o por el canal degradado. `[C]` confirmar con la institución cuál es el nivel de reemplazo por delegación. El sistema debe exigir que cada delegación tenga configurado su **suplente de autorización** antes de operar.
- **La misma persona ocupa dos puestos por encargaduría.** Ocurre con frecuencia tras rotación de personal ([NRM-09](../normativa/NRM-09-realidad-operativa.md)). La verificación es por persona, así que bloquea. La salida es [RN-07](RN-07-delegacion-de-autorizacion.md) — delegación acotada a otra persona —, nunca la autoautorización.
- **El motorista es también el solicitante.** Permitido: `I-11` no incluye solicitar, y la propia norma no lo enumera. Pero si además autoriza, despacha, entrega el fondo o liquida esa misma misión, se bloquea el segundo acto.
- **El autorizador se asigna a sí mismo como motorista tras la baja del titular.** Ocurre de verdad: el encargado autoriza el lunes, el motorista se incapacita el martes a las 05:50, y se pone él. **Bloqueo duro por `I-11`.** No hay confirmación con motivo, no hay advertencia registrada, no hay urgencia que lo habilite. La salida es sustituir por otro motorista habilitado o escalar por [RN-02](RN-02-escalamiento-de-autorizacion.md); si la única persona disponible es la que autorizó, la misión no sale hasta que la autorización la ejerza otro puesto. Ver [RN-14](RN-14-sustitucion-de-motorista.md).
- **El motorista conduce y luego liquida su propia misión.** Bloqueo duro por `I-11` y por `I-10` si además recibió el fondo. Es el vector de autoliquidación, el caso que el núcleo irreductible existe para impedir.
- **El motorista firmó la recepción del fondo.** Firmar la recepción **no es** ejercer la función "entregar combustible": el que entrega es ACT-07. Recibir es propio del motorista y no dispara `I-11`. Lo que sí bloquea es que el motorista **liquide** lo que recibió.
- **Reasignación de puesto a mitad del expediente.** La matriz se evalúa contra la identidad de quien **actuó**, congelada en el momento del acto, no contra el puesto que la persona ocupa hoy.
- **Usuario administrador del sistema (ACT-01).** No puede ejercer funciones de control sobre expedientes operativos. Su capacidad es de configuración, no de operación; si necesita operar, se le asigna el rol operativo y queda sujeto a esta regla como cualquiera.
- **Consolidación de dos solicitudes en una misma Orden de Misión.** La matriz se evalúa contra el conjunto de solicitantes de todas las solicitudes consolidadas: si el autorizador es solicitante de **cualquiera** de ellas, se bloquea.
- **Anulación y reemisión.** El asiento reverso de [RN-04](RN-04-anulacion-como-asiento-reverso.md) hereda la matriz de la orden original: no se puede usar la anulación para "limpiar" un conflicto de segregación.

## Trazabilidad

- Norma: [NRM-01 — Control interno y auditoría](../normativa/NRM-01-control-interno-tsc.md)
- Incompatibilidades implementadas: `I-01` a `I-11` de [actores-y-roles §5.2](../actores-y-roles.md). `I-11` es núcleo irreductible
- Decisión: [DP-002](../../07-gestion/decisiones-de-producto/DP-002-segregacion-en-delegaciones-pequenas.md) — se adopta el escalamiento a sede; el régimen de excepción no se implementa
- Hallazgos que corrigen esta regla: `HN1-02` (cita normativa), `HB1-02` (par `I-11`) de [H-B1-001](../../05-calidad/hallazgos/H-B1-001-revision-qa-bloque-1.md)
- Reglas relacionadas: [RN-02](RN-02-escalamiento-de-autorizacion.md), [RN-03](RN-03-registro-inmutable-de-autorizacion.md), [RN-07](RN-07-delegacion-de-autorizacion.md), [RN-14](RN-14-sustitucion-de-motorista.md), [RN-32](RN-32-entrega-de-combustible-contra-orden-de-mision.md)
- Actores: ACT-02, ACT-03, ACT-04, ACT-05, ACT-06, ACT-07, ACT-08, ACT-10
- Historias y casos especiales: pendientes — Bloque 2
