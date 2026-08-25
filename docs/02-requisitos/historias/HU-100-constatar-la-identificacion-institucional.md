# HU-100 — Constatar la identificación institucional del vehículo con fotografía por elemento, y hacerla caducar

| Campo | Valor |
|---|---|
| **Módulo** | M-04 Documentación y Cumplimiento Vehicular · M-03 Flota Vehicular |
| **Actor** | ACT-13 Custodio del Vehículo · ACT-14 Encargado de Bienes Institucionales |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — falta cómo se rotula una motocicleta del Estado, ya que el acuerdo describe franjas en puertas laterales que una moto no tiene (insumo #43), qué vehículos tienen excepción de rotulación y quién la concede (insumo #44), y si la rotulación aplica a vehículos en comodato y alquiler (insumo #55, zona gris expresa de `NRM-02`) |

## Historia

**Como** Custodio del Vehículo
**quiero** constatar elemento por elemento la identificación del vehículo del Estado con una fotografía de cada uno, sin necesidad de señal, y que la constatación caduque
**para** que la institución tenga evidencia fechada frente al hallazgo de auditoría más frecuente en flota, en lugar de una casilla marcada hace tres años

## Contexto

La identificación del vehículo del Estado —tres franjas azul–blanco–azul, leyenda "PROPIEDAD DEL ESTADO DE HONDURAS", siglas de la institución, numeración consecutiva y placas— es **campo verificable con fecha y foto** `[V]`, y es hallazgo frecuente de auditoría.

**Una constatación sin fotografía no se acepta.** Una casilla marcada no prueba nada; una fotografía con fecha, sí.

Y la constatación **caduca**. Una constatación de hace tres años no dice nada sobre el estado actual de la rotulación de un vehículo que ha estado en carretera todo ese tiempo.

## Reglas que la gobiernan

- [RN-18](../../01-negocio/reglas/RN-18-rotulacion-del-vehiculo-del-estado.md) — Identificación constatada con fecha y fotografía; `vigencia_constatacion_rotulacion` como parámetro
- [RN-64](../../01-negocio/reglas/RN-64-estado-de-la-placa-tipificado.md) — Sin lámina, la rotulación es la única identificación visible
- [RN-43](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md) — La captura funciona sin conectividad
- [RN-46](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md) — Fecha del hecho distinta de la fecha de captura
- [RN-75](../../01-negocio/reglas/RN-75-bien-retenido-o-sustraido-no-sale-del-registro.md) — El vehículo no confirmado en la constatación no se borra: se marca

## Casos especiales que la afectan

- [CE-17](../casos-especiales/CE-17-vehiculo-sin-placa-metalica.md) — La rotulación como única identificación visible
- [CE-15](../casos-especiales/CE-15-vehiculo-en-comodato-o-alquilado.md) — Rotulación de un bien que no es de la institución
- [CE-04](../casos-especiales/CE-04-robo-de-vehiculo-o-de-carga-en-mision.md) — Vehículos no confirmados en la constatación

## Criterios de aceptación

```gherkin
# language: es
Característica: Constatación física de la identificación institucional

  Antecedentes:
    Dado un vehículo "TR-0092" con custodio vigente
    Y un parámetro "vigencia_constatacion_rotulacion" de "365" días
    Y los elementos de identificación: franjas azul–blanco–azul, leyenda "PROPIEDAD DEL ESTADO DE HONDURAS", siglas de la institución, numeración consecutiva y placas

  Escenario: Se rechaza la constatación sin fotografía por elemento
    Cuando el Custodio marca los 5 elementos como presentes sin adjuntar fotografía de cada uno
    Entonces el sistema rechaza la constatación
    Y muestra "Adjunte fotografía por elemento. Una constatación sin fotografía no se acepta."

  Escenario: La captura funciona sin conectividad
    Dado un dispositivo sin señal
    Cuando el Custodio captura la constatación con fotografía por elemento, odómetro y ubicación
    Entonces el sistema guarda la constatación en el dispositivo
    Y la sincroniza cuando haya señal, conservando fecha del hecho y fecha de captura

  Escenario: Se registra la constatación con elementos faltantes
    Cuando el Custodio constata que faltan las siglas de la institución y adjunta la fotografía que lo evidencia
    Entonces el sistema acepta la constatación
    Y registra el elemento "siglas de la institución" como ausente, con su fotografía
    Y genera alerta al Jefe de Transporte y al Encargado de Bienes

  Escenario: La constatación caduca y el vehículo queda con identificación no constatada
    Dado una constatación de "TR-0092" del "2025-06-15"
    Cuando el sistema evalúa el vehículo el "2026-09-24"
    Entonces "TR-0092" queda marcado como "identificación no constatada"
    Y muestra "Última constatación: 15/06/2025, hace 466 días. Vigencia: 365 días."

  Escenario: La identificación no constatada advierte con acuse al despachar
    Dado un vehículo "TR-0092" con identificación no constatada
    Cuando el Encargado de Despacho despacha "TR-0092"
    Entonces el sistema advierte y exige acuse
    Y muestra "TR-0092 tiene la identificación institucional no constatada desde el 15/06/2025. Continúe con acuse o programe la constatación."
    Y el acuse queda registrado con autor y motivo

  Escenario: Vigencia más corta para vehículos sin lámina
    Dado un vehículo "TR-0098" con estado de placa "SIN_LAMINA_EN_TRAMITE"
    Y un parámetro de vigencia de constatación de "180" días para vehículos sin lámina
    Cuando el sistema evalúa "TR-0098" a los "200" días de su última constatación
    Entonces "TR-0098" queda marcado como "identificación no constatada"
    Y muestra "Sin lámina metálica, la rotulación es la única identificación visible del vehículo."

  Escenario: Reporte previo a operativo de resguardo
    Dado la proximidad de Semana Santa y un operativo de resguardo de flota
    Cuando el Encargado de Bienes genera el reporte previo
    Entonces el reporte lista los vehículos autorizados a circular con su permiso
    Y lista los vehículos que deben estar resguardados con responsable, fecha, odómetro y ubicación fotografiada

  Escenario: Sin evidencia, el vehículo figura como no confirmado
    Dado un vehículo del que nadie reportó ubicación ni fotografía en el operativo
    Cuando el Encargado de Bienes cierra el reporte de resguardo
    Entonces el vehículo figura como "no confirmado"
    Y nunca figura como "resguardado"

  Escenario: La constatación se registra con sus dos fechas
    Dado una constatación realizada en campo el "2026-09-20" y sincronizada el "2026-09-24"
    Cuando el registro llega al servidor
    Entonces conserva fecha del hecho "20/09/2026" y fecha de captura "24/09/2026"
    Y la vigencia de la constatación se cuenta desde la fecha del hecho
```

## Fuera de alcance

- La colocación física de la rotulación y su contratación: se registra el hecho, no se gestiona el trabajo
- El estado de la placa metálica — es [HU-097](HU-097-registrar-la-placa-y-el-estado-de-la-lamina.md)
- La conciliación del inventario de bienes contra el padrón de flota: pertenece al proceso de bienes; SIGTI aporta la evidencia
- La habilitación en flota — es [HU-102](HU-102-habilitar-el-vehiculo-en-flota.md)

## Notas y pendientes

- `[V]` Que la identificación del vehículo del Estado es obligatoria y es hallazgo frecuente de auditoría — [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md)
- `[V]` Que el resguardo previo a Semana Santa es evento recurrente y predecible de fiscalización del TSC — [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md)
- `[C]` **Cómo se rotula una motocicleta del Estado** — insumo **#43**
- `[C]` **Vehículos con excepción de rotulación y quién la concede** — insumo **#44**
- `[C]` **Rotulación en vehículos en comodato y alquiler** — insumo **#55**
- `[C]` `vigencia_constatacion_rotulacion` y su valor más corto para vehículos sin lámina — insumo **#1**
