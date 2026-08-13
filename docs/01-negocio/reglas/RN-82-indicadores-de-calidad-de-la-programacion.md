# RN-82 — Los indicadores de calidad de la programación se acumulan por causa tipificada y se atribuyen a la dependencia responsable

| Campo | Valor |
|---|---|
| **Módulos** | M-14, M-07, M-08, M-13 |
| **Origen** | Casos especiales [CE-06](../../02-requisitos/casos-especiales/CE-06-la-mision-se-extiende-mas-dias-destinos-o-kilometros.md), [CE-07](../../02-requisitos/casos-especiales/CE-07-retorno-anticipado-la-mision-se-aborta.md), [CE-08](../../02-requisitos/casos-especiales/CE-08-multi-destino-con-esperas-prolongadas-en-sitio.md), [CE-09](../../02-requisitos/casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md), [CE-01](../../02-requisitos/casos-especiales/CE-01-salida-de-emergencia-convalidada.md) · Norma [NRM-01](../normativa/NRM-01-control-interno-tsc.md) |
| **Verificación** | `[V]` la exigencia de registro oportuno de las operaciones — TSC-NOGECI V-10, citado en [NRM-01](../normativa/NRM-01-control-interno-tsc.md) con nivel `[P]`; se declara `[P]` por la regla de no escalar el nivel |
| **Tipo** | Derivación |
| **Configurable** | Sí — catálogos de causa y umbrales de alerta por indicador |

## Enunciado

El sistema **debe** producir, por período y con fecha de corte de conocimiento ([`RN-94`](RN-94-fecha-de-corte-de-conocimiento-en-todo-reporte.md)), al menos estos indicadores, **desagregados por causa tipificada** y **atribuidos a la dependencia, delegación o vehículo responsable**:

| Indicador | Qué mide | Origen |
|---|---|---|
| **Extensión de misión** | Frecuencia y magnitud de las prórrogas y destinos agregados | [`RN-77`](RN-77-versionado-del-alcance-autorizado.md) |
| **Misión abortada** | Retornos anticipados, con su causa y su costo atribuido | [`RN-78`](RN-78-grado-de-cumplimiento-del-objeto.md) |
| **Espera improductiva** | Horas de vehículo y de conductor inmovilizados por causa ajena a la operación | [`RN-76`](RN-76-estado-en-ruta-declarado-por-el-motorista.md) |
| **Oportunidad de registro** | Desfase entre fecha del hecho y fecha de captura | [`RN-46`](RN-46-fecha-del-hecho-y-fecha-de-captura.md) |
| **Salida en régimen de emergencia** | Frecuencia de actos convalidados por dependencia y mes | [`RN-73`](RN-73-convalidacion-de-actos-sin-autorizacion-previa.md) |
| **Demanda no atendida y desplazamiento** | Solicitudes desplazadas por falta de recurso | [`RN-56`](RN-56-prelacion-entre-solicitudes-que-compiten.md) |

**El indicador de oportunidad de registro imputa el diferimiento a la delegación, a la institución o al conductor según motivo tipificado; nunca por defecto al conductor.**

## Justificación

Estos seis indicadores tienen algo en común: **la institución no los tiene hoy**, y son los únicos datos con los que una unidad de transporte puede ir a una gestión presupuestaria o a una discusión con otra dependencia **con evidencia propia**.

Sin ellos, una dependencia que aborta seis misiones por trimestre y otra que no aborta ninguna reciben el mismo trato, y la unidad de transporte carga con la fama de ineficiente por una demanda que no controla.

La cláusula sobre la oportunidad de registro no es un detalle: es la diferencia entre un indicador que mide la realidad —la zona no tiene señal, la delegación no tiene quién digite— y uno que castiga al motorista por vivir donde vive. Un indicador que solo puede culpar al eslabón más débil no se usa, o se usa mal.

TSC-NOGECI V-10 exige registro oportuno de las operaciones `[P]`. El indicador de oportunidad **es la respuesta institucional a esa exigencia**: no dice que siempre se registra a tiempo, dice cuánto, dónde y por qué no.

## Condiciones de aplicación

Aplica a todo período de reporte y a toda dependencia y delegación de la institución.

**No aplica** como base de sanción por sí solo: los indicadores describen, no imputan responsabilidad ([`RN-74`](RN-74-sin-atribucion-de-responsabilidad-en-campo.md)).

## Comportamiento esperado

1. Cada indicador se calcula sobre hechos con **causa tipificada de catálogo**. Un hecho sin causa tipificada entra al indicador en la categoría *sin causa declarada*, que es en sí misma un dato de gestión.
2. La atribución sigue a la causa, no a quien tuvo el hecho enfrente. Una espera por bodega cerrada se atribuye al destino; una espera porque el motorista llegó tarde, a la institución.
3. Los indicadores se exponen en el **reporte de control interno** de la Gerencia Administrativa y de Auditoría Interna, con su serie histórica.
4. El **costo atribuido** de misiones abortadas y de espera improductiva se calcula con combustible, peajes, kilometraje y días de vehículo; el costo-hora de vehículo es `[C]` dato de la institución. Sin él, se expresa en horas y kilómetros.
5. Superado el umbral configurado de un indicador, el sistema alerta a ACT-08 con el detalle desagregado. La alerta no bloquea nada.
6. Todo indicador declara su **fecha de corte de conocimiento** y es reproducible a esa fecha.

## Casos límite

- **Delegación con incomunicación acreditada.** Su desfase de registro se atribuye a la condición de la zona, no a las personas. `[C]` pendiente D-1 de [CE-09](../../02-requisitos/casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md): si el **plazo de liquidación se suspende** durante ventanas de incomunicación acreditadas y registradas por delegación. Sin esa decisión, el plazo corre y la delegación acumula incumplimientos que no puede evitar.
- **Dependencia que declara siempre la misma causa** para justificar sus abortos. El indicador lo muestra; que sea cierto o no es materia de gestión, no del sistema.
- **Emergencia que se vuelve la vía normal.** Es el uso principal del indicador de salidas en régimen de emergencia: si esta variante reemplaza al circuito de autorización, **el control desapareció y hay que poder verlo en un número**.
- **Período con muy pocas misiones.** El indicador se publica igual con su denominador visible. Un porcentaje sobre tres misiones no se presenta sin decir que son tres.
- **Indicador que alguien quiere usar para sancionar.** No lo impide el sistema; lo acota la regla: la determinación de responsabilidad tiene su procedimiento y no nace en un reporte.

## Trazabilidad

- Norma: [NRM-01](../normativa/NRM-01-control-interno-tsc.md) `[P]` — TSC-NOGECI V-10 registro oportuno
- Reglas relacionadas: [RN-46](RN-46-fecha-del-hecho-y-fecha-de-captura.md), [RN-56](RN-56-prelacion-entre-solicitudes-que-compiten.md), [RN-73](RN-73-convalidacion-de-actos-sin-autorizacion-previa.md), [RN-74](RN-74-sin-atribucion-de-responsabilidad-en-campo.md), [RN-76](RN-76-estado-en-ruta-declarado-por-el-motorista.md), [RN-77](RN-77-versionado-del-alcance-autorizado.md), [RN-78](RN-78-grado-de-cumplimiento-del-objeto.md), [RN-94](RN-94-fecha-de-corte-de-conocimiento-en-todo-reporte.md)
- Casos especiales: [CE-06](../../02-requisitos/casos-especiales/CE-06-la-mision-se-extiende-mas-dias-destinos-o-kilometros.md) `RN-c:indicador-de-extension-recurrente` · [CE-07](../../02-requisitos/casos-especiales/CE-07-retorno-anticipado-la-mision-se-aborta.md) `RN-c:indicador-de-mision-abortada-por-causa` · [CE-09](../../02-requisitos/casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md) `RN-c:imputacion-del-registro-diferido`, `RN-c:plazo-de-liquidacion-y-ventanas-de-incomunicacion` · [CE-08](../../02-requisitos/casos-especiales/CE-08-multi-destino-con-esperas-prolongadas-en-sitio.md) · [CE-01](../../02-requisitos/casos-especiales/CE-01-salida-de-emergencia-convalidada.md)
- Insumos pendientes: costo-hora de vehículo · plazo de liquidación y ventanas de incomunicación
