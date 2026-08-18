# HU-001 — Registrar la solicitud declarando qué se traslada

| Campo | Valor |
|---|---|
| **Módulo** | M-06 Solicitudes de Transporte |
| **Actor** | ACT-02 Solicitante |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Refinada |

## Historia

**Como** Solicitante
**quiero** registrar una solicitud de transporte declarando **primero el objeto del traslado** —personal de la institución, personas externas, carga o mixto— con su detalle, el origen, los destinos en su orden previsto y la ventana solicitada
**para** que la necesidad de movilización quede constituida como expediente institucional con el dato que determina el tipo de vehículo compatible, en lugar de un formato en papel que se traspapela y que nadie puede rastrear después

## Contexto

Hoy la solicitud se llena en un formato de papel que viaja de escritorio en escritorio. Cuando la Gerencia Administrativa pregunta por qué salió un vehículo el 14 de marzo, la respuesta depende de que alguien encuentre la hoja.

El sistema pregunta **qué se traslada antes que ninguna otra cosa** porque ese dato decide todo lo que viene después: el tipo de vehículo compatible, los documentos que se emitirán y las validaciones que se aplicarán (premisas rectoras 1 y 2 de `CLAUDE.md`). Un formulario que pregunta "destino" primero y "qué lleva" al final es un formulario que produce asignaciones incompatibles.

El borrador **no compromete nada**: no tiene folio, no reserva vehículo ni motorista, y solo lo ve su creador.

## Reglas que la gobiernan

- [RN-20](../../01-negocio/reglas/RN-20-compatibilidad-vehiculo-objeto-del-traslado.md) — El tipo de vehículo debe ser compatible con el objeto del traslado declarado
- [RN-21](../../01-negocio/reglas/RN-21-capacidad-de-pasajeros-y-carga.md) — No se excede la capacidad de pasajeros ni la de carga de la ficha técnica
- [RN-59](../../01-negocio/reglas/RN-59-todo-uso-se-ampara-en-orden-de-mision.md) — Todo uso de un vehículo del Estado se ampara en una Orden de Misión, cualquiera sea su régimen
- [RN-58](../../01-negocio/reglas/RN-58-regimen-de-uso-del-vehiculo.md) — El régimen de uso es atributo del vehículo, con acto, beneficiario y vigencia
- [RN-06](../../01-negocio/reglas/RN-06-transiciones-de-estado-de-la-orden.md) — La Orden de Misión solo transita por los estados definidos; `T-01` deja el expediente en `BORRADOR`
- [RN-43](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md) — La captura se completa sin ninguna conectividad
- [RN-44](../../01-negocio/reglas/RN-44-identificadores-y-folios-en-el-cliente.md) — El identificador del expediente se genera en el cliente, no en el servidor

## Casos especiales que la afectan

- [CE-19](../casos-especiales/CE-19-vehiculo-asignado-a-funcionario-frente-al-pool.md) — El vehículo asignado permanentemente a un funcionario también empieza por esta solicitud
- [CE-18](../casos-especiales/CE-18-carga-y-pasajeros-en-la-misma-mision.md) — Carga y pasajeros en la misma misión: aquí solo se **declara**; el bloqueo es [HU-002](HU-002-bloqueo-de-compatibilidad-del-objeto-del-traslado.md)

## Criterios de aceptación

```gherkin
# language: es
Característica: Registro del borrador de una solicitud de transporte
  Como Solicitante
  quiero declarar qué se traslada, hacia dónde y cuándo
  para constituir el expediente de la movilización sin comprometer flota

  Antecedentes:
    Dado un Solicitante "Ana Bustillo" con rol vigente sobre la dependencia "Subgerencia de Operaciones"
    Y un catálogo de motivos de viaje vigente al "2026-03-14"
    Y un catálogo de tipos de vehículo vigente al "2026-03-14"

  Escenario: Se rechaza continuar sin declarar el objeto del traslado
    Dado un borrador recién abierto sin objeto del traslado declarado
    Cuando "Ana Bustillo" intenta registrar el origen y los destinos
    Entonces el sistema rechaza el registro
    Y muestra "Declare primero qué se traslada: personal de la institución, personas externas, carga o mixto. De ese dato depende el tipo de vehículo compatible."

  Escenario: Se rechaza la carga declarada sin peso
    Dado un borrador con objeto del traslado "Carga"
    Y una carga descrita como "12 sillas de oficina" sin peso registrado
    Cuando "Ana Bustillo" intenta guardar el detalle de la carga
    Entonces el sistema rechaza el registro
    Y muestra "Declare el peso en kilogramos de la carga. Sin peso no se puede verificar la capacidad del vehículo (RN-21)."

  Escenario: Se rechaza la ventana con retorno anterior a la salida
    Dado un borrador con objeto del traslado "Personal de la institución" y "4" pasajeros
    Y una fecha y hora de salida del "2026-03-14 07:00"
    Cuando "Ana Bustillo" registra una fecha y hora de retorno del "2026-03-13 18:00"
    Entonces el sistema rechaza el registro
    Y muestra "El retorno previsto (13/03/2026 18:00) es anterior a la salida (14/03/2026 07:00)."

  Escenario: El vehículo asignado a un funcionario también requiere solicitud
    Dado un vehículo "Pickup Hilux" con correlativo institucional "VH-0142" bajo régimen de uso "Asignación permanente a funcionario"
    Cuando "Ana Bustillo" abre un borrador declarando ese régimen de uso
    Entonces el sistema acepta el borrador
    Y muestra "Todo uso de un vehículo del Estado se ampara en una Orden de Misión, incluido el de asignación permanente (RN-59)."
    Y exige declarar quién conducirá, para verificar su habilitación al programar

  Escenario: El borrador no es visible para otro solicitante de la misma dependencia
    Dado un borrador creado por "Ana Bustillo" en la dependencia "Subgerencia de Operaciones"
    Cuando el Solicitante "Marvin Cálix", de la misma dependencia, consulta las solicitudes de la dependencia
    Entonces el sistema no incluye el borrador de "Ana Bustillo" en el resultado

  Escenario: Se registra el borrador completo sin comprometer flota
    Dado un borrador con objeto del traslado "Mixto", "3" pasajeros y "180" kg de carga
    Y un origen "Tegucigalpa", un destino 1 "Comayagua" con permanencia de "2" horas y un destino 2 "Siguatepeque" con permanencia de "1" hora
    Y una ventana del "2026-03-14 07:00" al "2026-03-14 19:00"
    Cuando "Ana Bustillo" guarda el borrador
    Entonces el expediente queda en estado "BORRADOR"
    Y no tiene número de expediente institucional asignado
    Y no tiene vehículo ni motorista vinculados
    Y no existe ninguna reserva sobre la ventana solicitada
```

## Fuera de alcance

- La verificación de compatibilidad que impide **enviar** la solicitud — es [HU-002](HU-002-bloqueo-de-compatibilidad-del-objeto-del-traslado.md)
- La asignación del número de expediente y el congelamiento del contenido — ocurren al enviar, en [HU-004](HU-004-envio-a-autorizacion-con-numero-de-expediente-y-congelamiento.md)
- La captura por encargo de otro servidor — es [HU-003](HU-003-captura-por-encargo-y-solicitante-de-derecho.md)
- La captura de datos de **personas externas** con minimización ([`RN-51`](../../01-negocio/reglas/RN-51-minimizacion-de-datos-de-personas-externas.md)) y el manifiesto de M-17: quedan **diferidos** a las historias de M-17. Aquí solo se declara la cantidad y el tipo de objeto
- El viático asociado: SIGTI solo guarda la **clave de vínculo** con ARGOS. No lo calcula ni lo liquida ([DP-001](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) D-01)
- El descarte del borrador (`T-03`): queda como historia aparte del backlog de M-06

## Notas y pendientes

- `[C]` Campos exactos del formato en papel vigente de la institución para la solicitud de transporte — insumo #2. Los campos aquí listados son el mínimo que exige el flujo, no el formato definitivo
- `[C]` Antelación mínima entre la captura y la salida prevista — insumo #32. **No se cablea ningún valor**: si no se cumple, la solicitud se marca urgente y no se bloquea
- `[I]` La distinción entre destinos con orden previsto y permanencia estimada por destino proviene de la variante V-04 de `PR-01`, no de norma
- Trazabilidad: [CU-01](../casos-de-uso/CU-01-registrar-solicitud-de-transporte.md) flujo principal pasos 1 a 5, excepción E6
