# Casos de uso `CU-xx`

18 recorridos paso a paso de la operación de transporte institucional, con sus flujos alternos y de excepción.

Plantilla: [`docs/plantillas/caso-de-uso.md`](../../plantillas/caso-de-uso.md). Los IDs son estables y **nunca se reciclan**.

## Por qué hay casos de uso y no solo historias

Una historia de usuario expresa **una necesidad**. Un caso de uso detalla **el recorrido**, incluidos los caminos que no llevan al final feliz. Escribir los dos para todo sería duplicar trabajo, así que el criterio de la plantilla es el que manda:

> Se usan para los flujos donde **el orden de los pasos y las precondiciones importan legalmente** — autorizaciones, despacho, liquidación. Para el resto, la historia de usuario basta.

En SIGTI eso significa algo muy concreto. Cuando el Tribunal Superior de Cuentas o Auditoría Interna revisan un expediente, no preguntan si el usuario logró hacer lo que quería: preguntan **en qué orden ocurrieron los actos, quién tenía competencia en cada uno, y qué se verificó antes de consumar el siguiente**. Autorizar después de despachar no es el mismo hecho que autorizar antes; entregar el fondo a una misión que todavía no salió no es lo mismo que entregarlo al despacharla. Esa diferencia no cabe en un "Como… quiero… para…".

Por eso los 18 casos cubren la cadena de control —`solicitud → autorización → orden de misión → despacho → bitácora → combustible → liquidación → cierre`— más los dos habilitantes cuya ausencia deja sin fuente a un bloqueo duro: el **expediente del vehículo** y la **habilitación del motorista**. Todo lo demás —consultas, tableros, reportes, mantenimiento de catálogos— se especifica con historias.

## Los 18 casos, por fase del proceso

**Enlaces resueltos.** Los 18 casos están escritos y enlazados. Módulo y actor principal se toman de [`mapa-de-procesos.md`](../../01-negocio/mapa-de-procesos.md) y [`actores-y-roles.md`](../../01-negocio/actores-y-roles.md).

### Habilitantes — lo que debe estar resuelto antes de que se pueda mover un vehículo

| ID | Caso de uso | Módulo | Actor principal | Archivo |
|---|---|---|---|---|
| `CU-17` | Dar de alta y mantener el expediente del vehículo | M-03, M-04 | `ACT-14` Encargado de Bienes Institucionales | [CU-17-alta-y-mantenimiento-del-expediente-del-vehiculo.md](CU-17-alta-y-mantenimiento-del-expediente-del-vehiculo.md) |
| `CU-18` | Registrar y mantener la habilitación del motorista | M-05 | `ACT-04` Jefe de Transporte | [CU-18-registrar-y-mantener-la-habilitacion-del-motorista.md](CU-18-registrar-y-mantener-la-habilitacion-del-motorista.md) |
| `CU-12` | Solicitar y aprobar el fondo de combustible | M-09 | `ACT-04` solicita · `ACT-08` Gerencia Administrativa aprueba | [ficha](CU-12-solicitar-y-aprobar-fondo-de-combustible.md) |
| `CU-03` | Solicitar y emitir permiso de circulación en día u hora inhábil | M-04, M-15 | `ACT-09` Máxima Autoridad | [ficha](CU-03-permiso-de-circulacion-en-dia-inhabil.md) |

`CU-03` se dispara desde una solicitud ya autorizada, pero se agrupa aquí porque su producto —el **salvoconducto impreso con folio y QR**— es precondición dura del despacho (`PC-03`, `BD-04`), no un trámite posterior.

### Solicitud y autorización

| ID | Caso de uso | Módulo | Actor principal | Archivo |
|---|---|---|---|---|
| `CU-01` | Registrar solicitud de transporte | M-06 | `ACT-02` Solicitante | [ficha](CU-01-registrar-solicitud-de-transporte.md) |
| `CU-02` | Autorizar solicitud | M-06 | `ACT-03` Jefatura Inmediata | [ficha](CU-02-autorizar-solicitud-de-transporte.md) |

### Programación y despacho

| ID | Caso de uso | Módulo | Actor principal | Archivo |
|---|---|---|---|---|
| `CU-04` | Programar la misión: asignar vehículo y motorista | M-07 | `ACT-04` Jefe de Transporte | [ficha](CU-04-programar-mision-asignar-vehiculo-y-motorista.md) |
| `CU-05` | Emitir la Orden de Misión y documentos imprimibles | M-07, M-15 | `ACT-04` Jefe de Transporte | [ficha](CU-05-emitir-orden-de-mision-y-documentos.md) |
| `CU-13` | Emitir y entregar la asignación de combustible | M-09 | `ACT-07` Encargado de Combustible | [ficha](CU-13-emitir-y-entregar-asignacion-de-combustible.md) |
| `CU-06` | Despachar y registrar la salida | M-07, M-08 | `ACT-05` Encargado de Despacho | [ficha](CU-06-despachar-y-registrar-salida.md) |
| `CU-07` | Sustituir vehículo o motorista | M-07, M-03, M-05 | `ACT-04` Jefe de Transporte | [ficha](CU-07-sustituir-vehiculo-o-motorista.md) |

### Ejecución en ruta

| ID | Caso de uso | Módulo | Actor principal | Archivo |
|---|---|---|---|---|
| `CU-08` | Registrar la ejecución en ruta sin conectividad | M-08, M-16, M-19 | `ACT-06` Motorista | [ficha](CU-08-ejecucion-en-ruta-sin-conectividad.md) |
| `CU-09` | Registrar interrupción en ruta y resolver su desenlace | M-08, M-12 | `ACT-06` reporta · `ACT-04` resuelve | [ficha](CU-09-interrupcion-en-ruta-y-desenlace.md) |
| `CU-14` | Registrar consumo de combustible y paso por peaje | M-09, M-18 | `ACT-06` Motorista | [ficha](CU-14-registrar-consumo-de-combustible-y-peaje.md) |
| `CU-10` | Registrar el retorno y cerrar la bitácora | M-08 | `ACT-05` Encargado de Despacho | [ficha](CU-10-registrar-retorno-y-cerrar-bitacora.md) |
| `CU-11` | Sincronizar el cliente de campo y resolver conflictos | M-16 | `ACT-10` Encargado de Delegación · `ACT-01` en la cola técnica | [ficha](CU-11-sincronizar-y-resolver-conflictos.md) |

### Liquidación y cierre

| ID | Caso de uso | Módulo | Actor principal | Archivo |
|---|---|---|---|---|
| `CU-15` | Liquidar la misión y conciliar | M-13, M-09, M-18 | `ACT-04` Jefe de Transporte | [ficha](CU-15-liquidar-la-mision-y-conciliar.md) |
| `CU-16` | Cerrar el expediente de la misión | M-13, M-14 | `ACT-08` Gerencia Administrativa | [ficha](CU-16-cerrar-el-expediente-de-la-mision.md) |

**Quien liquida no es quien despachó ni quien entregó el fondo, y quien cierra no es quien liquidó.** `CU-15` y `CU-16` son dos casos de uso y no uno por esa razón — `I-09`, `I-10` y `BD-06`, bloqueo duro.

## Cómo se anclan estos casos de uso

Un caso de uso aquí no inventa reglas: **las invoca**. Cada paso se ancla a artefactos que ya existen, y cuando un ancla falta, se dice.

| Ancla | Qué es | Dónde vive |
|---|---|---|
| `T-nn` | Transición de la Orden de Misión | [`orden-de-mision.md`](../../03-arquitectura/estados/orden-de-mision.md) §3 |
| `W-nn` | Transición del estado operativo del vehículo | [`orden-de-mision.md`](../../03-arquitectura/estados/orden-de-mision.md) §10.2 |
| `BD-nn` | Precondición de bloqueo duro | [`orden-de-mision.md`](../../03-arquitectura/estados/orden-de-mision.md) §4 |
| `PC-nn` | Punto de control del proceso | [`PR-01`](../../01-negocio/procesos/PR-01-movilizacion-institucional.md) |
| `I-nn` | Par de incompatibilidad de funciones | [`actores-y-roles.md`](../../01-negocio/actores-y-roles.md) §5.2 |
| `RN-xx` | Regla de negocio | [`docs/01-negocio/reglas/`](../../01-negocio/reglas/README.md) |
| `CE-xx` | Caso especial con su regla de resolución | [`docs/02-requisitos/casos-especiales/`](../casos-especiales/README.md) |
| `ACT-xx` | Actor | [`actores-y-roles.md`](../../01-negocio/actores-y-roles.md) |

**Precedencia.** Si un caso de uso se contradice con otro artefacto, manda el que es autoridad sobre esa materia: transiciones y bloqueos duros, la máquina de estados; actores e incompatibilidades, `actores-y-roles.md`; lo demás del negocio, la `RN-xx` correspondiente. Ver [`CLAUDE.md`](../../../CLAUDE.md).

**Las tablas derivadas citan su origen en lugar de copiarlo.** Ningún caso de uso reescribe la tabla de transiciones ni la matriz de permisos: enlaza. Una tabla copiada es una tabla que va a divergir.

## Hallazgos abiertos desde estos casos de uso

Se anotan aquí para que no se pierdan entre los archivos. **No se resuelven en el artefacto que los detecta**: se elevan al que tiene autoridad.

| Hallazgo | Detectado en | Autoridad que debe resolverlo |
|---|---|---|
| Falta el estado terminal **`RETIRADO_DE_FLOTA`**. Devolver un vehículo al comodante o al arrendador solo puede registrarse hoy como `DADO_DE_BAJA`, y **declarar dado de baja un bien ajeno es un asiento falso** | `CU-17` `A8`; ya abierto en [CE-15](../casos-especiales/CE-15-vehiculo-en-comodato-o-alquilado.md) | [`docs/03-arquitectura/estados/`](../../03-arquitectura/estados/orden-de-mision.md) |
| **Quién aprueba el descargo** de un vehículo: la máquina de estados dice `ACT-08`, el mapa de procesos admite *"`ACT-08` o `ACT-09`"*, y [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md) lo deja `[P]` | `CU-17` `A7` | Institución `[C]` + máquina de estados |
| **No existe par `I-nn` que cubra "habilita × es habilitado"**: quien se habilita a sí mismo para conducir se autoriza a sí mismo el control de mayor valor legal del sistema | `CU-18` `E5` | [`actores-y-roles.md`](../../01-negocio/actores-y-roles.md) + Auditoría Interna `[C]` |

## Estado del bloque

**Los 18 casos están escritos.** 3,095 líneas, 172 por caso en promedio. Cada uno ancla sus pasos a las transiciones `T-nn`, los puntos de control `PC-nn`, los bloqueos `BD-nn` y los invariantes `INV-nn`, y enlaza los `CE-xx` que lo tocan como flujo alterno o de excepción — o los descarta explícitamente con su razón.

Los casos que más referencias cruzan son `CU-06` despachar (35 reglas), `CU-04` programar (33) y `CU-15` liquidar (31 reglas, 14 casos especiales). No es casualidad: son los tres puntos donde el control interno concentra sus exigencias.

## Los hallazgos que produjo escribirlos

Escribir los casos de uso obligó a recorrer el diseño paso a paso, y eso destapó contradicciones que ni la revisión adversarial del Bloque 1 ni los 28 casos especiales habían encontrado. **Ninguna se resolvió en silencio**: cada caso de uso siguió al artefacto autoridad y dejó la divergencia anotada.

Se consolidan en [`docs/05-calidad/hallazgos/H-B3-001-hallazgos-de-casos-de-uso.md`](../../05-calidad/hallazgos/H-B3-001-hallazgos-de-casos-de-uso.md).

Los insumos nuevos que abrieron están en [`docs/07-gestion/insumos-pendientes.md`](../../07-gestion/insumos-pendientes.md).
