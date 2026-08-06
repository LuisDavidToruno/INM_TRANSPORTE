# RN-38 — La exoneración de peaje es un dato por vehículo, punto, fundamento y vigencia; nunca una constante

| Campo | Valor |
|---|---|
| **Módulos** | M-18, M-03 |
| **Origen** | Norma [NRM-10](../normativa/NRM-10-peajes.md) — sección 5 |
| **Verificación** | `[V]` que existe régimen de exoneración — `[I]` que se perfila funcional (emergencia, rescate) y no institucional — `[C]` la lista oficial (insumo #22) |
| **Tipo** | Derivación |
| **Configurable** | Sí — tabla `exoneracion_peaje (vehículo, punto, fundamento, vigencia_desde, vigencia_hasta)` |

## Enunciado

Un vehículo **puede** estar exonerado del pago de peaje **en puntos determinados**, y esa exoneración se modela como dato con: vehículo, punto (o todos los puntos de un operador), **fundamento** documental, y **rango de vigencia**.

El sistema **no debe** asumir que un vehículo está exonerado por pertenecer al Estado. El valor por defecto es **paga**.

Un vehículo con exoneración vigente en un punto estima **cero** en ese punto, **con el fundamento visible** en el desglose ([RN-35](RN-35-estimacion-de-peajes-antes-de-aprobar.md)).

## Justificación

[NRM-10](../normativa/NRM-10-peajes.md) llega a una conclusión de trabajo explícitamente marcada `[I]` y explícitamente no verificada:

> *"La exoneración se perfila como funcional (emergencia y rescate), no institucional (por ser del Estado). Ninguna fuente exonera a vehículos administrativos de una institución pública... **Un pickup institucional en misión administrativa PAGA peaje.** M-18 se diseña asumiendo que se paga."*

Lo verificado `[V]` es que existe régimen de exoneración: en 2023 pasaron 224,994 vehículos con libre paso — el 2% del tráfico. Las categorías mencionadas son *"ambulancias, policía, etc."*, y la lista completa **no está publicada en ninguna fuente consultable**.

El diseño defensivo que la ficha ordena: modelar *"vehículo exonerado en el punto X con fundamento Y y vigencia Z"* **como dato, no como constante** — porque SIGTI es genérico y una institución con ambulancias o unidades de rescate lo necesita desde el día uno.

## Condiciones de aplicación

Aplica a la estimación y a la conciliación de peajes.

`[C]` **Insumo #22** — lista oficial de exoneraciones y si alcanza a vehículos administrativos del Estado. [NRM-10](../normativa/NRM-10-peajes.md) lo califica como *"lo que decide cómo se construye M-18"*. No se resuelve por inferencia: mientras no se confirme, ninguna exoneración se carga por defecto.

## Comportamiento esperado

1. Registrar una exoneración exige fundamento en texto y adjunto, y es un acto autorizado y registrado ([RN-03](RN-03-registro-inmutable-de-autorizacion.md)).
2. La exoneración se resuelve **a la fecha del hecho** ([RN-40](RN-40-calculo-a-la-fecha-del-hecho.md)): un paso de hace un año se valora con la exoneración vigente entonces.
3. En la liquidación, un **peaje pagado en un punto donde el vehículo estaba exonerado** es una desviación tipificada — pudo ser cobro indebido, y habilita reclamo igual que [RN-36](RN-36-discrepancia-de-clasificacion-en-caseta.md).
4. El sistema reporta las exoneraciones vigentes por vehículo y punto, con su fundamento, para revisión de ACT-12 Auditor Interno. Una exoneración es una excepción permanente al pago: exige vigilancia proporcional.
5. La exoneración **no exime** de registrar el paso por la caseta: el dato de ruta y tiempo se necesita igual para [RN-37](RN-37-coherencia-de-la-secuencia-de-casetas.md).

## Casos límite

- **Institución que asume que su flota está exonerada.** Es la suposición más probable y más costosa. El sistema **no la permite** como configuración global sin fundamento por vehículo. Si la institución tiene un acuerdo, ese acuerdo es el fundamento y se adjunta.
- **Vehículo de emergencia en misión administrativa.** La exoneración puede ser funcional — por el uso, no por el vehículo. `[C]` sin resolver. Si la lista oficial resulta ser funcional, el modelo debe admitir **exoneración condicionada al tipo de misión**, no solo al vehículo. El diseño debe dejar ese eje abierto.
- **Motocicletas.** `[P]` una fuente no oficial las enumera entre los exonerados. No basta para cargarlo. Se registra la duda como `[C]` y la institución decide con evidencia.
- **Caseta que no reconoce la exoneración en el momento.** El motorista paga para no detener la misión. Se registra el pago **y** la exoneración vigente: la contradicción es la base del reclamo.
- **Exoneración vencida sin que nadie lo note.** Alimenta las alertas de [RN-17](RN-17-alertas-de-vencimiento-documental.md). Una estimación que sigue calculando cero con exoneración vencida subestima el costo de la misión y produce faltante de efectivo en ruta.
- **Punto sin cobro por causa distinta a la exoneración** — Canal Seco, Corredor Turístico en terminación anticipada `[V]`. No es exoneración del vehículo: es estado del punto ([RN-34](RN-34-tarifa-de-peaje-por-punto-categoria-vigencia.md)). Confundirlos haría que al reactivarse el cobro el sistema siguiera estimando cero.

## Trazabilidad

- Norma: [NRM-10 — Peajes](../normativa/NRM-10-peajes.md)
- Reglas relacionadas: [RN-34](RN-34-tarifa-de-peaje-por-punto-categoria-vigencia.md), [RN-35](RN-35-estimacion-de-peajes-antes-de-aprobar.md), [RN-36](RN-36-discrepancia-de-clasificacion-en-caseta.md), [RN-24](RN-24-vehiculo-de-servicio-exceptuado.md)
- Actores: ACT-01, ACT-04, ACT-08, ACT-12
- Historias y casos especiales: pendientes — Bloque 2
