# RN-73 — El acto ejecutado sin autorización previa se convalida dentro de un plazo, y la cronología se declara tal como ocurrió

| Campo | Valor |
|---|---|
| **Módulos** | M-06, M-07, M-08, M-14, M-15 |
| **Origen** | Casos especiales [CE-01](../../02-requisitos/casos-especiales/CE-01-salida-de-emergencia-convalidada.md), [CE-07](../../02-requisitos/casos-especiales/CE-07-retorno-anticipado-la-mision-se-aborta.md), [CE-10](../../02-requisitos/casos-especiales/CE-10-motorista-incapacitado-en-ruta.md), [CE-05](../../02-requisitos/casos-especiales/CE-05-cambio-de-motorista-con-la-mision-en-curso.md) · Norma [NRM-01](../normativa/NRM-01-control-interno-tsc.md) |
| **Verificación** | `[P]` la exigencia de autorización previa de toda transacción — TSC-NOGECI V-07, [NRM-01](../normativa/NRM-01-control-interno-tsc.md) `[P]`. `[C]` qué puesto convalida y en qué plazo — insumos #32 y #50 |
| **Tipo** | Bloqueo duro + advertencia con hallazgo |
| **Configurable** | Sí — parámetro `plazo_convalidacion` con vigencia por rango de fechas |

## Enunciado

Todo acto ejecutado sin la autorización previa que le corresponde —salida en régimen de emergencia, retorno anticipado decidido por el conductor ante riesgo, incorporación de un conductor no habilitado por incapacidad del titular, desenlace de una interrupción decidido sin poder consultar— **debe** convalidarse dentro de un **plazo configurable con vigencia**.

**Vencido el plazo, la convalidación no se rechaza**: se registra igual, y la misión **cierra con hallazgo**. Negarse a registrar un hecho ocurrido solo lo hace invisible.

Y la **cronología se declara tal como ocurrió**: cuando la marca de tiempo del hecho de una transición es **posterior** a la de la transición que le sigue en la máquina de estados, el expediente lo declara explícitamente y **lo imprime**. **Ningún acto se presenta como previo si fue posterior.**

## Justificación

Ninguna de las 54 reglas originales gobierna la convalidación, y sin embargo la salida sin autorización previa ocurre: a las 03:15 de un domingo, con una persona que hay que trasladar, no hay a quién pedirle firma. Lo que la institución necesita no es impedir esa salida —no puede— sino que quede registrada con su hora real y con la convalidación que la respalda.

La cláusula de cronología es la que evita el fraude más silencioso de todos: capturar la autorización con fecha anterior al hecho para que el expediente "se vea bien". El sistema tiene ambos datos —fecha del hecho y fecha de captura, [`RN-46`](RN-46-fecha-del-hecho-y-fecha-de-captura.md)— y por tanto **puede** decir la verdad. Si no la dice, es porque alguien decidió que no la dijera.

Un expediente que muestra *"salida 03:15 del domingo, convalidación 08:40 del lunes, intervalo 29 h 25 min, plazo configurado 24 h — excedido"* es defendible. Uno que muestra la autorización fechada el sábado por la tarde no lo es, y es peor: es un documento falso producido por un sistema.

## Condiciones de aplicación

Aplica a todo acto del ciclo de la Orden de Misión que requiera autorización y se haya ejecutado sin ella.

**No aplica** a los bloqueos duros que no admiten excepción: [`RN-01`](RN-01-segregacion-de-funciones.md) segregación de funciones y [`RN-09`](RN-09-matriz-licencia-vehiculo.md) / [`RN-10`](RN-10-licencia-vigente-en-todo-el-rango.md) habilitación. Un acto que los viola **no se convalida**: se registra como hecho y como hallazgo, y su tratamiento es el de la infracción, no el de la formalidad pendiente.

## Comportamiento esperado

1. La salida en régimen de emergencia exige **causal tipificada** del catálogo, quién la declaró, quién ordenó verbalmente la movilización y **por qué canal**, declarado por quien recibió la orden.
2. El sistema mide el **intervalo entre el hecho y la convalidación** y lo muestra en el expediente y en el impreso, junto al plazo vigente aplicado.
3. Vencido el plazo, la convalidación se registra igual, con la marca de extemporánea, y la misión se excluye del cierre limpio.
4. El acto de convalidación cumple [`RN-03`](RN-03-registro-inmutable-de-autorizacion.md): identidad, rol, momento, origen y huella del contenido convalidado — incluido el código gestionado por el sistema si se hizo fuera de línea.
5. El expediente y su impreso muestran **hora del hecho y hora de captura visiblemente distintas** en toda transición donde difieran.
6. La **frecuencia de la emergencia es un indicador**, no una nota al pie: salidas en régimen de emergencia por dependencia, delegación y mes, expuestas en el reporte de control interno ([`RN-82`](RN-82-indicadores-de-calidad-de-la-programacion.md)). Si esta variante se vuelve la vía normal para saltarse la autorización, el control desapareció y hay que poder verlo en un número.

## Casos límite

- **`[C]` Qué puesto convalida y en qué plazo máximo** — insumos #32 y #50. Opciones y costo:

  | Opción | Costo |
  |---|---|
  | Convalida la jefatura inmediata del solicitante | La más natural, pero un domingo puede estar tan incomunicada como al momento del hecho, y el plazo se consume |
  | Convalida un puesto de turno de sede central designado | Resuelve la disponibilidad, pero exige que la institución tenga ese turno definido y que ARGOS lo refleje |
  | Convalida siempre la Gerencia Administrativa | Concentra la carga y la vuelve cuello de botella, pero es inequívoco |

- **`[C]` La única persona disponible a las 03:15 es el propio conductor.** La incompatibilidad de [`RN-01`](RN-01-segregacion-de-funciones.md) no se levanta ni en emergencia, de modo que hoy ese caso **no tiene salida escrita**. Es un hueco real, no un detalle, y está escalado al PO.
- **Retorno anticipado decidido por el conductor ante riesgo** —vía cerrada, clima, seguridad. La facultad de detener la misión no puede depender de una autorización que no se puede pedir. Se registra con causa tipificada y se convalida después; el control real es la auditoría de las convalidaciones.
- **Permiso de circulación en día inhábil no obtenido** antes de la salida de emergencia. Se convalida por la misma vía; si no se convalida, es hallazgo propio y separado.
- **Convalidación registrada sin conectividad** con código gestionado por el sistema. Vale, con el registro completo de quién, cuándo, desde dónde y sobre qué contenido — no hay firma electrónica certificada en el país y la autorización es interna.

## Trazabilidad

- Autoridad: [orden-de-mision.md](../../03-arquitectura/estados/orden-de-mision.md) · [`PR-01`](../procesos/PR-01-movilizacion-institucional.md) §9 y `PC-18`
- Norma: [NRM-01](../normativa/NRM-01-control-interno-tsc.md) `[P]` · [NRM-08](../normativa/NRM-08-firma-electronica.md) `[P]`
- Reglas relacionadas: [RN-01](RN-01-segregacion-de-funciones.md), [RN-02](RN-02-escalamiento-de-autorizacion.md), [RN-03](RN-03-registro-inmutable-de-autorizacion.md), [RN-07](RN-07-delegacion-de-autorizacion.md), [RN-46](RN-46-fecha-del-hecho-y-fecha-de-captura.md), [RN-70](RN-70-interrupcion-en-ruta-con-desenlace-obligatorio.md), [RN-82](RN-82-indicadores-de-calidad-de-la-programacion.md)
- Casos especiales: [CE-01](../../02-requisitos/casos-especiales/CE-01-salida-de-emergencia-convalidada.md) `RN-c:convalidacion-con-plazo-maximo`, `RN-c:cronologia-invertida-declarada` · [CE-07](../../02-requisitos/casos-especiales/CE-07-retorno-anticipado-la-mision-se-aborta.md) `RN-c:autoria-de-la-decision-de-abortar` · [CE-05](../../02-requisitos/casos-especiales/CE-05-cambio-de-motorista-con-la-mision-en-curso.md) `RN-c:habilitacion-no-verificable-en-campo`
- Insumos pendientes: #32 quién convalida y en qué plazo · #50 quién puede ordenar el retorno anticipado
