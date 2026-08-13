# RN-76 — El estado en ruta lo declara el motorista, el sistema nunca lo infiere, y la espera improductiva se tipifica y se atribuye

| Campo | Valor |
|---|---|
| **Módulos** | M-19, M-08, M-16, M-14 |
| **Origen** | Caso especial [CE-08](../../02-requisitos/casos-especiales/CE-08-multi-destino-con-esperas-prolongadas-en-sitio.md) · Norma [NRM-09](../normativa/NRM-09-realidad-operativa.md) |
| **Verificación** | `[V]` la falta de conectividad en amplias zonas del país — [NRM-09](../normativa/NRM-09-realidad-operativa.md), INE EPHPM julio 2025. `[I]` la prohibición de inferir el estado: decisión de producto del equipo |
| **Tipo** | Bloqueo duro + derivación |
| **Configurable** | Sí — catálogo `estado_en_ruta`, catálogo `causa_de_espera`, umbral `espera_notificable` |

## Enunciado

El estado de un vehículo en ruta **debe** ser **declarado por el conductor** desde un catálogo cerrado, con **un toque y sin conectividad**.

**El sistema nunca infiere el estado a partir de la ausencia de movimiento ni de la ausencia de señal.** Todo dato de ubicación o estado mostrado **debe** exhibir su **antigüedad**.

El **tiempo en sitio** se deriva de los eventos de **arribo** y **salida** por destino, con el reloj del dispositivo. **Nunca se pide al conductor que lo cronometre ni que lo digite.**

La espera en que el vehículo **no puede operar** se **tipifica por causa** y se **atribuye al destino y a la dependencia responsable**. Solo esa cuenta como **espera improductiva** en los indicadores.

## Justificación

Un vehículo detenido tres horas en una bodega de Choluteca y un vehículo detenido tres horas porque el dispositivo perdió señal son dos hechos completamente distintos, y un sistema que los infiere del mismo silencio va a confundirlos siempre. La inferencia produce, además, un daño peor que la ignorancia: un tablero que dice *"detenido"* con confianza cuando en realidad no sabe nada.

La antigüedad visible es la forma honesta de mostrar un dato de campo. *"Última actualización hace 4 h 20 min"* le dice al Jefe de Transporte exactamente lo que puede concluir; el mismo dato sin antigüedad le dice algo falso.

Y la distinción entre espera y espera improductiva es la que hace útil al indicador: el tiempo de carga y descarga es operación normal; las tres horas esperando a que aparezca quien recibe son un costo atribuible a alguien, y por primera vez se puede decir a quién con evidencia.

## Condiciones de aplicación

Aplica a toda misión, con énfasis en las multi-destino y de reparto.

**No aplica** a los eventos de interrupción, que tienen su propio circuito ([`RN-70`](RN-70-interrupcion-en-ruta-con-desenlace-obligatorio.md)).

## Comportamiento esperado

1. El cliente de campo ofrece los estados del catálogo —en marcha, en espera, cargando o descargando, en descanso, detenido por causa— **con un toque**, sin formulario, y los almacena para sincronizar cuando haya señal ([`RN-43`](RN-43-captura-de-campo-sin-conectividad.md)).
2. Arribo y salida por destino se registran como eventos con **fecha del hecho** ([`RN-46`](RN-46-fecha-del-hecho-y-fecha-de-captura.md)); el tiempo en sitio es **derivado**, no capturado.
3. La espera se tipifica al declararla o al salir del sitio, con causa del catálogo y con la dependencia o el destino al que se atribuye.
4. El **motor encendido durante la espera** se registra con un toque y entra como **variable en la conciliación galonaje–kilometraje**: una desviación de consumo con espera prolongada registrada **no produce hallazgo por sí sola** ([`RN-30`](RN-30-conciliacion-galonaje-kilometraje.md)).
5. La espera improductiva que supera `espera_notificable` **notifica a ACT-04 y a la dependencia solicitante en cuanto haya señal**, sin perderse si no la hay.
6. Los tiempos en sitio históricos por destino y tipo de operación **alimentan el estimado de duración** de las misiones siguientes. Programar cuatro entregas en un día deja de ser un acto de fe.
7. El costo de la espera se cuantifica en **horas de vehículo inmovilizado y horas de conductor**; `[C]` el costo-hora de vehículo es dato de la institución. Sin él, el indicador se expresa en horas, que ya es infinitamente más de lo que hay hoy.

## Casos límite

- **`[C]` ¿El destino compromete una ventana de atención?** Insumo #51 — es la pregunta que decide si el caso se previene o solo se documenta. Opciones y costo:

  | Opción | Costo |
  |---|---|
  | No se modela: el vehículo llega cuando llega | Es lo que hay hoy. La espera se mide y se atribuye, pero nadie la evita |
  | Cada dependencia y bodega registra su **horario de recepción**, y la programación advierte si el arribo estimado cae fuera | Barato — un catálogo con vigencia más — y evita la entrega perdida entera. Alguien tiene que mantenerlo: un horario desactualizado es peor que ninguno |
  | Además del horario, **confirmación del receptor antes del despacho** | Evita también el caso caro, pero agrega un paso que depende de un tercero y puede trabar salidas |

  **Recomendación del análisis, no decisión:** la segunda de entrada, con la tercera como **advertencia no bloqueante** en misiones de reparto. Trabar un despacho porque una bodega no contestó el teléfono es cambiar un problema por otro peor.
- **Dispositivo sin batería durante horas.** No se infiere nada. Al reconectar, los eventos llegan con su fecha del hecho y el hueco queda visible como período sin declaración, que es un dato, no un supuesto.
- **Conductor que olvida declarar la salida del sitio.** El tiempo en sitio se calcula hasta el siguiente evento con su marca, y el registro señala que la salida fue **derivada**, no declarada.
- **Reordenamiento de destinos en ruta.** Se registra con motivo y **no constituye desviación de ruta** si la secuencia sigue siendo geográfica y temporalmente coherente ([`RN-37`](RN-37-coherencia-de-la-secuencia-de-casetas.md)); el estimado de peajes se recalcula con el paquete congelado.
- **Espera por causa del propio equipo** —el motorista llegó tarde. Se tipifica igual y se atribuye a la institución, no al destino. El indicador que solo mide culpas ajenas no lo cree nadie.

## Trazabilidad

- Norma: [NRM-09](../normativa/NRM-09-realidad-operativa.md) `[V]`
- Reglas relacionadas: [RN-30](RN-30-conciliacion-galonaje-kilometraje.md), [RN-37](RN-37-coherencia-de-la-secuencia-de-casetas.md), [RN-43](RN-43-captura-de-campo-sin-conectividad.md), [RN-46](RN-46-fecha-del-hecho-y-fecha-de-captura.md), [RN-70](RN-70-interrupcion-en-ruta-con-desenlace-obligatorio.md), [RN-78](RN-78-grado-de-cumplimiento-del-objeto.md), [RN-82](RN-82-indicadores-de-calidad-de-la-programacion.md)
- Casos especiales: [CE-08](../../02-requisitos/casos-especiales/CE-08-multi-destino-con-esperas-prolongadas-en-sitio.md) — candidatas `RN-c:estado-en-ruta-declarado-por-el-motorista`, `RN-c:tiempo-en-sitio-derivado-de-arribo-y-salida`, `RN-c:espera-improductiva-tipificada-y-atribuida`, `RN-c:motor-encendido-en-espera-como-variable-de-conciliacion`, `RN-c:aviso-por-espera-sobre-umbral`, `RN-c:tiempos-en-sitio-historicos-alimentan-la-programacion`, `RN-c:reordenamiento-de-destinos-justificado`
- Insumos pendientes: #51 ventana de atención del destino · costo-hora de vehículo
