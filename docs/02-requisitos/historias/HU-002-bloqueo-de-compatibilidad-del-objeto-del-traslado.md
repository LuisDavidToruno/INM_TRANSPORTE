# HU-002 — Impedir el envío cuando lo declarado no se puede trasladar

| Campo | Valor |
|---|---|
| **Módulo** | M-06 Solicitudes de Transporte |
| **Actor** | ACT-02 Solicitante |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Refinada |

## Historia

**Como** Solicitante
**quiero** que el sistema me impida enviar a autorización una solicitud cuyo tipo de vehículo requerido no puede mover lo que declaré, o cuyos objetos declarados no pueden viajar juntos
**para** que el error se corrija cuando cuesta un minuto y no el día de la salida, cuando el motorista ya está en el predio y hay que devolver personal o carga

## Contexto

Hoy este control lo hace el criterio del Encargado de Despacho el día del viaje: llega un pickup para trasladar quince personas, o alguien pretende llevar bidones de combustible en el mismo microbús que el personal. El costo del error no es administrativo, es operativo y a veces de seguridad.

La compatibilidad se resuelve **contra la matriz del catálogo de M-02, no contra el criterio del solicitante**. Y se evalúa en dos ejes: vehículo × objeto ([`RN-20`](../../01-negocio/reglas/RN-20-compatibilidad-vehiculo-objeto-del-traslado.md), [`RN-21`](../../01-negocio/reglas/RN-21-capacidad-de-pasajeros-y-carga.md)) y objeto × objeto par a par ([`RN-67`](../../01-negocio/reglas/RN-67-matriz-de-compatibilidad-objeto-objeto.md)).

**La ausencia de entrada en la matriz bloquea.** No se interpreta como compatible: un par que nadie evaluó es un par que nadie autorizó.

## Reglas que la gobiernan

- [RN-20](../../01-negocio/reglas/RN-20-compatibilidad-vehiculo-objeto-del-traslado.md) — El tipo de vehículo debe ser compatible con el objeto del traslado declarado — bloqueo `BD-09`
- [RN-21](../../01-negocio/reglas/RN-21-capacidad-de-pasajeros-y-carga.md) — No se excede la capacidad de pasajeros ni la capacidad de carga de la ficha técnica
- [RN-67](../../01-negocio/reglas/RN-67-matriz-de-compatibilidad-objeto-objeto.md) — Matriz de compatibilidad objeto × objeto evaluada par a par; la ausencia de entrada bloquea
- [RN-68](../../01-negocio/reglas/RN-68-compatibilidad-y-capacidad-por-tramo.md) — Compatibilidad y capacidad se evalúan por tramo, sobre la configuración real de cada tramo
- [RN-39](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md) — La matriz es catálogo con vigencia por rango de fechas, nunca constante en el código

## Casos especiales que la afectan

- [CE-18](../casos-especiales/CE-18-carga-y-pasajeros-en-la-misma-mision.md) — Carga y pasajeros en la misma misión, con requisitos que compiten

## Criterios de aceptación

```gherkin
# language: es
Característica: Compatibilidad entre lo declarado y el tipo de vehículo requerido
  Como Solicitante
  quiero que el sistema no me deje enviar una solicitud que no se puede ejecutar
  para no descubrir el problema el día de la salida

  Antecedentes:
    Dado un tipo de vehículo "Pickup doble cabina" con "5" plazas y capacidad de carga de "1000" kg
    Y un tipo de vehículo "Microbús" con "15" plazas y capacidad de carga de "300" kg
    Y un tipo de vehículo "Camión de estacas" con "3" plazas y capacidad de carga de "5000" kg
    Y una matriz de compatibilidad objeto × objeto vigente al "2026-03-14"
    Y una entrada en esa matriz que declara "Personas" y "Combustible en envase" como incompatibles

  Escenario: Se bloquea el envío por plazas insuficientes
    Dado una solicitud con objeto del traslado "Personal de la institución" y "9" pasajeros
    Y un tipo de vehículo requerido "Pickup doble cabina"
    Cuando el Solicitante intenta enviar la solicitud a autorización
    Entonces el sistema no ejecuta el envío
    Y muestra "El tipo Pickup doble cabina tiene 5 plazas y se declararon 9 pasajeros. Tipos que sí lo cubren: Microbús (15 plazas)."
    Y el expediente permanece en estado "BORRADOR"

  Escenario: Se bloquea el envío por capacidad de carga excedida
    Dado una solicitud con objeto del traslado "Carga" y "1800" kg declarados
    Y un tipo de vehículo requerido "Microbús"
    Cuando el Solicitante intenta enviar la solicitud a autorización
    Entonces el sistema no ejecuta el envío
    Y muestra "El tipo Microbús admite 300 kg de carga y se declararon 1,800 kg. Tipos que sí lo cubren: Camión de estacas (5,000 kg)."

  Escenario: Se bloquea el envío por incompatibilidad entre objetos declarados
    Dado una solicitud con objeto del traslado "Mixto"
    Y "6" pasajeros declarados
    Y una carga de "4" bidones de "5" galones de combustible
    Cuando el Solicitante intenta enviar la solicitud a autorización
    Entonces el sistema no ejecuta el envío
    Y muestra "Personas y combustible en envase no pueden trasladarse juntos. Separe el traslado en dos solicitudes o declare la configuración por tramo (RN-67, RN-68)."

  Escenario: Se bloquea el envío cuando el par no está en la matriz
    Dado una solicitud con objeto del traslado "Mixto"
    Y "2" pasajeros declarados
    Y una carga declarada como "Cilindro de oxígeno medicinal"
    Y ninguna entrada en la matriz para el par "Personas" y "Gas comprimido"
    Cuando el Solicitante intenta enviar la solicitud a autorización
    Entonces el sistema no ejecuta el envío
    Y muestra "No existe entrada en la matriz de compatibilidad para Personas junto a Gas comprimido. La ausencia de entrada bloquea: solicite a Catálogos Maestros que evalúe el par."

  Escenario: Se acepta el envío declarando la configuración por tramo
    Dado una solicitud con objeto del traslado "Mixto"
    Y un tramo 1 "Tegucigalpa–Comayagua" con "6" pasajeros y sin carga
    Y un tramo 2 "Comayagua–Siguatepeque" con "4" bidones de combustible y sin pasajeros
    Y un tipo de vehículo requerido "Pickup doble cabina"
    Cuando el Solicitante envía la solicitud a autorización
    Entonces el sistema ejecuta el envío
    Y el expediente pasa a estado "SOLICITADA"
    Y conserva la configuración declarada de cada tramo

  Escenario: La matriz que se aplica es la vigente a la fecha prevista de salida
    Dado una matriz que declara "Personas" y "Herramienta manual" incompatibles a partir del "2026-04-01"
    Y una solicitud con salida prevista el "2026-03-14" que declara "2" pasajeros y "1" caja de herramienta manual
    Cuando el Solicitante envía la solicitud a autorización
    Entonces el sistema ejecuta el envío
    Y deja constancia del identificador de la versión de matriz aplicada
```

## Fuera de alcance

- La verificación contra el **vehículo concreto** y su ficha técnica: aquí se evalúa contra el **tipo de vehículo requerido**. El vehículo se asigna en [CU-04](../casos-de-uso/CU-04-programar-mision-asignar-vehiculo-y-motorista.md)
- La habilitación del motorista ([`RN-09`](../../01-negocio/reglas/RN-09-matriz-licencia-vehiculo.md), [`RN-10`](../../01-negocio/reglas/RN-10-licencia-vigente-en-todo-el-rango.md)): no hay motorista asignado en `SOLICITADA`
- El mantenimiento del catálogo de tipos de vehículo y de la matriz — es M-02
- La reducción de carga por objeto principal cuando el peso efectivo excede lo declarado: ocurre en ejecución, no aquí

## Notas y pendientes

- `[C]` Contenido inicial de la matriz objeto × objeto. La institución debe declarar los pares que su operación real produce — insumos #1 y #39
- `[C]` Si la institución realiza traslados de **personas bajo custodia o de menores**, el par correspondiente exige tratamiento reforzado — insumo #39
- `[I]` El ejemplo canónico "personas junto a bidones de combustible" proviene de la propia [`RN-20`](../../01-negocio/reglas/RN-20-compatibilidad-vehiculo-objeto-del-traslado.md), no de norma citada
- Trazabilidad: [CU-01](../casos-de-uso/CU-01-registrar-solicitud-de-transporte.md) paso 6, excepciones E1 y E2; bloqueo `BD-09`
