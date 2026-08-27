# RN-101 — Una asignación de puesto no se cierra con custodias físicas activas; lo demás pasa al puesto y la autoría no se toca

| Campo | Valor |
|---|---|
| **Módulos** | M-01, M-03, M-09, M-13, M-14 |
| **Origen** | Norma [NRM-02](../normativa/NRM-02-bienes-del-estado.md) — tarjeta de responsabilidad y acta de entrega-recepción; [NRM-09](../normativa/NRM-09-realidad-operativa.md) — rotación alta. Diseño de [actores-y-roles.md §2.4](../actores-y-roles.md), **artefacto autoridad en actores**. Regla candidata 2 de su §8. Hallazgo `HN1-18` de [H-B1-002](../../05-calidad/hallazgos/H-B1-002-revision-normativa-bloque-1.md) |
| **Verificación** | `[P]` que el *Manual de Propiedad Estatal* de la Dirección General de Bienes Nacionales regule el reporte de movimientos de inventario, el descargo y las pérdidas — [NRM-02](../normativa/NRM-02-bienes-del-estado.md), articulado no extraído. `[V]` la rotación de personal alta tras el cambio de administración de enero de 2026 — [NRM-09](../normativa/NRM-09-realidad-operativa.md). `[I]` el acta de cierre de asignación y su clasificación en cuatro tipos: diseño del equipo. `[C]` los formatos vigentes de acta de entrega-recepción y tarjeta de responsabilidad |
| **Tipo** | Bloqueo duro sobre la custodia física; tratamiento reglado para lo demás |
| **Configurable** | No el bloqueo. Sí el plazo de solape en coocupación |

## Por qué existe esta regla — hallazgo `HN1-18`

[actores-y-roles.md §2.4](../actores-y-roles.md) lo llama *«el escenario que la rotación produce todos los meses y el que más daño hace si el sistema no lo previó»*, y lo dejó **sin regla que lo obligara**. [`RN-22`](RN-22-custodia-del-vehiculo.md) menciona la custodia vacante en un caso límite, pero no cubre ni el cierre de la asignación ni la entrega unilateral.

## Enunciado

Al cerrar la asignación de un puesto, el sistema produce un **acta de cierre de asignación** que enumera y clasifica todo lo que queda abierto, y le da a cada clase el tratamiento que le corresponde:

| Tipo de pendiente | Ejemplos | Tratamiento |
|---|---|---|
| **Custodia física** | Vehículos bajo tarjeta de responsabilidad, vales emitidos sin canjear, efectivo u órdenes de pago del fondo, llaves | **Bloqueo duro.** La asignación **no se cierra** sin acta de entrega-recepción firmada, o sin **acta de entrega unilateral con hallazgo abierto** |
| **Actos pendientes de decisión** | Solicitudes sin autorizar, fondos sin aprobar, liquidaciones sin cerrar | Quedan atribuidos **al puesto**. Quien lo ocupe los ve al entrar. Si el puesto queda vacante, escalan |
| **Misiones en ejecución** | Misiones `DESPACHADA` o `EN_RUTA` programadas por el saliente | **No se interrumpen.** Continúan bajo el puesto. Si el saliente era el motorista, es sustitución de motorista y no cierre de asignación |
| **Autoría histórica** | Todo lo firmado, autorizado, despachado o liquidado | **No se toca.** Persona y puesto quedan congelados en el asiento ([`RN-100`](RN-100-permisos-por-puesto-no-por-persona.md)) |

## El acta de entrega unilateral

**La persona puede haberse ido ya.** Renuncia sin previo aviso, traslado inmediato, fallecimiento, despido. Un diseño que exige la firma del saliente para cerrar deja la asignación abierta para siempre y con ella la custodia, que es peor que cerrarla mal.

Por eso existe la salida: **el acta de entrega unilateral, levantada por la comisión que corresponda, con hallazgo abierto**. No cierra el problema — lo **nombra**, le pone responsable de seguimiento y lo deja donde la auditoría lo va a encontrar. La custodia se reasigna; el hallazgo sigue vivo hasta que alguien lo resuelva.

## Justificación

Sin bloqueo sobre la custodia física, lo que ocurre es lo conocido: el saliente se va, el vehículo sigue a su nombre en la tarjeta de responsabilidad, y cuando aparece un daño o una multa **no hay a quién imputarla** porque la persona ya no está y nadie recibió formalmente el bien. La institución pierde la deducción de responsabilidad por un trámite que no se hizo.

Y sin el traspaso reglado de lo demás, los expedientes abiertos del saliente quedan invisibles: nadie los ve porque colgaban de un usuario que ya no entra. **Un fondo sin liquidar que nadie ve es un fondo perdido.**

## Condiciones de aplicación

Aplica al cierre de toda asignación de puesto, cualquiera sea la causa: traslado, renuncia, despido, término de contrato, fallecimiento, reestructuración o supresión del puesto.

**No aplica** a la ausencia temporal —vacaciones, incapacidad, permiso—, que no cierra la asignación y se resuelve por delegación ([`RN-07`](RN-07-delegacion-de-autorizacion.md)) o por escalamiento ([`RN-02`](RN-02-escalamiento-de-autorizacion.md)).

## Comportamiento esperado

1. El acta se genera **antes** de cerrar, y muestra lo que va a bloquear el cierre. Nadie descubre la custodia pendiente al intentar guardar.
2. Cada custodia física entregada produce su **acta de entrega-recepción** con las dos firmas, y actualiza la tarjeta de responsabilidad ([`RN-22`](RN-22-custodia-del-vehiculo.md)).
3. Toda entrega unilateral **abre hallazgo**, con responsable de seguimiento y sin plazo de caducidad automática. Un hallazgo que se cierra solo no es un hallazgo.
4. El acta de cierre es **documento oficial con folio** ([`RN-25`](RN-25-salvoconducto-con-folio-y-qr.md) fija el patrón), imprimible, y forma parte del paquete de evidencia por período ([`RN-98`](RN-98-paquete-de-evidencia-por-vehiculo-o-periodo.md)).
5. Cerrada la asignación, la persona **pierde todos los permisos de ese puesto de inmediato** ([`RN-100`](RN-100-permisos-por-puesto-no-por-persona.md)), y conserva la autoría de todo lo que hizo.

## Casos límite

- **El saliente tiene un vehículo en misión, `EN_RUTA`, el día que se va.** La misión no se interrumpe. La custodia del vehículo está en el motorista mientras dure la misión ([`RN-22`](RN-22-custodia-del-vehiculo.md)); la custodia permanente se entrega al retorno, y hasta entonces la asignación queda **cerrada con custodia diferida declarada**, no cerrada en silencio.
- **No hay a quién entregar** — el puesto queda vacante y no hay sucesor. La custodia se traslada al puesto superior o al Encargado de Bienes (`ACT-14`), nunca al vacío. Un bien sin custodio no se despacha.
- **El saliente era el único con custodia en una delegación de tres personas.** Es el caso de siempre. La entrega la recibe quien quede, y si eso rompe la segregación aplica el escalamiento a sede — no una excepción local ([`DP-002`](../../07-gestion/decisiones-de-producto/DP-002-segregacion-en-delegaciones-pequenas.md)).
- **Aparece una custodia que el registro no tenía.** Se registra en el acta como **hallazgo de constatación** y se concilia por [`RN-99`](RN-99-constatacion-fisica-de-la-flota.md). Un bien que aparece es tan grave como uno que falta: significa que el registro no era fiel.
- **La asignación se cerró y después se descubre una custodia pendiente.** No se reabre el acta ([`RN-05`](RN-05-registro-cerrado-no-se-edita.md)): se levanta un acta nueva que la refiere, con hallazgo. La corrección va siempre hacia adelante.

## Trazabilidad

- **Normas**: [NRM-02](../normativa/NRM-02-bienes-del-estado.md) `[P]` · [NRM-09](../normativa/NRM-09-realidad-operativa.md) `[V]` la rotación
- **Hallazgo que la origina**: `HN1-18` de [H-B1-002](../../05-calidad/hallazgos/H-B1-002-revision-normativa-bloque-1.md)
- **Autoridad del diseño**: [actores-y-roles.md §2.4](../actores-y-roles.md)
- **Reglas relacionadas**: [`RN-02`](RN-02-escalamiento-de-autorizacion.md) · [`RN-05`](RN-05-registro-cerrado-no-se-edita.md) · [`RN-07`](RN-07-delegacion-de-autorizacion.md) · [`RN-22`](RN-22-custodia-del-vehiculo.md) · [`RN-98`](RN-98-paquete-de-evidencia-por-vehiculo-o-periodo.md) · [`RN-99`](RN-99-constatacion-fisica-de-la-flota.md) · [`RN-100`](RN-100-permisos-por-puesto-no-por-persona.md)
- **Actor**: `ACT-14` Encargado de Bienes Institucionales recibe lo que queda sin sucesor
- **Módulo principal**: M-01 Organización y Seguridad
