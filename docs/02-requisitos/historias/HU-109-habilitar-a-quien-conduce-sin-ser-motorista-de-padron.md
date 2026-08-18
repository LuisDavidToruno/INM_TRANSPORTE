# HU-109 — Habilitar con el mismo rigor a quien conduce sin ser motorista de padrón

| Campo | Valor |
|---|---|
| **Módulo** | M-05 Motoristas y Habilitación |
| **Actor** | ACT-04 Jefe de Transporte |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador |

## Historia

**Como** Jefe de Transporte
**quiero** exigir exactamente los mismos datos y el mismo rigor a un funcionario asignatario, a un servidor de otra dependencia o a un conductor eventual que a un motorista de padrón
**para** que la habilitación se verifique sobre **quien efectivamente conduce**, que es quien va a estar al volante si ocurre un siniestro

## Contexto

El vehículo asignado a un funcionario lo conduce el funcionario. El motorista que se incapacita en ruta deja el volante a alguien. El servidor de otra dependencia que lleva el pickup a una diligencia también conduce.

**Ningún régimen de uso, jerarquía ni excepción operativa exime de esta verificación.** La responsabilidad ante un siniestro no distingue entre un motorista de planilla y un director que conducía el vehículo institucional.

Desde el momento en que queda habilitado, le aplican **las mismas incompatibilidades** que a cualquier conductor de misión: no entrega su propio fondo, no liquida su propia misión.

## Reglas que la gobiernan

- [RN-57](../../01-negocio/reglas/RN-57-habilitacion-de-quien-efectivamente-conduce.md) — La habilitación se verifica sobre quien conduce, cualquiera sea su puesto. **No configurable**
- [RN-09](../../01-negocio/reglas/RN-09-matriz-licencia-vehiculo.md) — Bloqueo duro sin excepción, también aquí
- [RN-10](../../01-negocio/reglas/RN-10-licencia-vigente-en-todo-el-rango.md) — Vigencia durante todo el rango, también aquí
- [RN-58](../../01-negocio/reglas/RN-58-regimen-de-uso-del-vehiculo.md) — El régimen de uso es atributo del vehículo, no una exención de habilitación
- [RN-59](../../01-negocio/reglas/RN-59-todo-uso-se-ampara-en-orden-de-mision.md) — Todo uso del vehículo se ampara en una Orden de Misión
- [RN-01](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md) — Las incompatibilidades del conductor aplican desde la habilitación
- [RN-14](../../01-negocio/reglas/RN-14-sustitucion-de-motorista.md) — El relevo revalida todas las habilitaciones

## Casos especiales que la afectan

- [CE-19](../casos-especiales/CE-19-vehiculo-asignado-a-funcionario-frente-al-pool.md) — Funcionario que conduce su vehículo asignado
- [CE-10](../casos-especiales/CE-10-motorista-incapacitado-en-ruta.md) — Conductor eventual incorporado por incapacidad
- [CE-05](../casos-especiales/CE-05-cambio-de-motorista-con-la-mision-en-curso.md) — Relevo con la misión en curso

## Criterios de aceptación

```gherkin
# language: es
Característica: Habilitación de quien conduce sin ser motorista de padrón

  Antecedentes:
    Dado un funcionario "Ana Suazo" con vehículo "TR-0060" asignado en régimen permanente
    Y una matriz licencia↔vehículo vigente

  Escenario: Se rechaza asignar a un funcionario sin licencia registrada
    Cuando el Jefe de Transporte intenta asignar a "Ana Suazo" como conductora de "TR-0060"
    Entonces el sistema rechaza la asignación
    Y muestra "Ana Suazo no tiene licencia registrada en SIGTI. La habilitación se verifica sobre quien conduce, cualquiera sea su puesto."

  Escenario: El régimen de asignación permanente no exime de la verificación
    Cuando el Jefe de Transporte invoca el régimen de asignación permanente para omitir la verificación
    Entonces el sistema rechaza la omisión
    Y muestra "Ningún régimen de uso, jerarquía ni excepción operativa exime de la verificación de licencia."

  Escenario: Se exige el mismo rigor que a un motorista de padrón
    Cuando el Jefe de Transporte registra la habilitación de "Ana Suazo" sin fotografía de la licencia física
    Entonces el sistema no consuma la habilitación
    Y muestra "Se exigen los mismos datos que a un motorista de padrón: identidad, número de licencia, categorías con su vencimiento, restricciones y fotografía de la licencia física."

  Escenario: Se bloquea por categoría insuficiente aunque sea funcionario
    Dado "Ana Suazo" con licencia categoría "B" vigente
    Y un vehículo "TR-0098" tipo "Camión", peso bruto "12,000" kg
    Cuando el Jefe de Transporte intenta asignarla a "TR-0098"
    Entonces el sistema rechaza la asignación
    Y muestra "La licencia categoría B no habilita un vehículo de 12,000 kg. Se requiere categoría C."

  Escenario: Se habilita al conductor eventual por incapacidad del motorista
    Dado un motorista "Wilmer Cáceres" incapacitado en ruta
    Cuando el Jefe de Transporte habilita a "Marlon Zelaya" con su licencia categoría "B" vigente hasta el "2029-08-30" y fotografía del documento
    Entonces el sistema habilita a "Marlon Zelaya" para los vehículos que cubre la categoría "B"
    Y la sustitución revalida todas las habilitaciones y conserva la asignación original en el historial

  Escenario: Las incompatibilidades aplican desde la habilitación
    Dado "Ana Suazo" habilitada como conductora de la misión "OM-2026-0560"
    Cuando "Ana Suazo" intenta liquidar "OM-2026-0560"
    Entonces el sistema rechaza la liquidación
    Y muestra "Ana Suazo condujo OM-2026-0560. Quien conduce no liquida su propia misión."

  Escenario: Se bloquea también la entrega de su propio fondo
    Cuando "Ana Suazo" intenta entregarse el instrumento de combustible de "OM-2026-0560"
    Entonces el sistema rechaza la entrega
    Y muestra "Ana Suazo es la conductora de OM-2026-0560. No puede entregar el fondo de su propia misión."

  Escenario: Todo uso del vehículo se ampara en una Orden de Misión
    Cuando "Ana Suazo" usa "TR-0060" sin Orden de Misión abierta
    Entonces el sistema registra el uso como no amparado
    Y muestra "Todo uso del vehículo se ampara en una Orden de Misión, incluido el régimen de asignación permanente."

  Escenario: Se habilita al conductor eventual y queda con las mismas alertas
    Cuando el Jefe de Transporte habilita a "Marlon Zelaya"
    Entonces el sistema programa las alertas anticipadas por categoría dirigidas al puesto
    Y calcula su vigencia de habilitación como la menor fecha entre sus categorías
```

## Fuera de alcance

- La captura de la licencia en sí — es [HU-105](HU-105-capturar-la-licencia-como-dato-propio-de-sigti.md)
- La derivación de vehículos habilitados — es [HU-106](HU-106-derivar-los-tipos-de-vehiculo-habilitados.md)
- El régimen de asignación permanente del vehículo a un funcionario: es atributo del vehículo (M-03)
- El relevo en ruta con acta y corte de odómetro: pertenece a M-08

## Notas y pendientes

- `[C]` **¿Admite la institución la figura de motorista eventual?** — insumo **#48**
- `[C]` **¿Cubre la póliza a un conductor no registrado como motorista?** — insumo **#49**. Mientras no consten, el sistema exige la licencia y bloquea, que es la posición sostenible ante un siniestro
- `[C]` Régimen de asignación permanente de vehículo a funcionario y sus condiciones — insumo **#64**
- `[C]` Límite de jornada de conducción, para decidir cuándo un relevo es obligatorio — insumo **#48**
- `[I]` La exigencia de verificar sobre quien efectivamente conduce es implicación de requerimiento del equipo derivada de [NRM-06](../../01-negocio/normativa/NRM-06-transito-y-licencias.md), no articulado citable
