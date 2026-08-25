# HU-137 — Cerrar la asignación de quien se fue y no entregó, por acta unilateral con hallazgo abierto

| Campo | Valor |
|---|---|
| **Módulo** | M-01 Organización y Seguridad |
| **Actor** | ACT-03 Jefatura Inmediata · ACT-14 Encargado de Bienes Institucionales |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — la deducción de responsabilidad depende del reglamento interno de la institución (insumo #1) |

## Historia

**Como** Jefatura Inmediata
**quiero** cerrar la asignación de un servidor que dejó el puesto sin entregar sus custodias, levantando un acta unilateral con comisión de al menos dos servidores y el inventario de lo no entregado
**para** que sus permisos se extingan hoy y no dentro de seis meses, y para que lo no entregado quede como hallazgo abierto con responsable identificado en lugar de desaparecer

## Contexto

Ocurre. Alguien renuncia el viernes y el lunes no aparece; alguien es removido y no vuelve a la oficina. [actores-y-roles §2.4](../../01-negocio/actores-y-roles.md) —autoridad— lo llama *"el caso feo"* y es tajante sobre por qué hay que resolverlo: **"lo que no se puede es dejar la asignación abierta indefinidamente, porque entonces el saliente conserva permisos."**

El diseño tiene que sostener dos cosas que tiran en direcciones opuestas:

- **Cerrar el acceso ya.** Un servidor que se fue y sigue pudiendo autorizar es una brecha, no un trámite pendiente.
- **No dar por entregado lo que no se entregó.** Cerrar la asignación **no** libera a nadie de la responsabilidad patrimonial sobre el bien.

La salida es que el cierre **produce un hallazgo**, no que el cierre lo tape. Los bienes no entregados quedan marcados como *pendientes de deducción de responsabilidad*, y el vehículo **no queda sin custodio**: pasa a depositario transitorio.

## Reglas que la gobiernan

- [RN-22](../../01-negocio/reglas/RN-22-custodia-del-vehiculo.md) — Todo vehículo tiene custodio vigente; ninguno queda sin él
- [RN-93](../../01-negocio/reglas/RN-93-expediente-de-hallazgo-posterior.md) — El hallazgo es expediente con ciclo propio que no altera el estado del objeto vinculado
- [RN-86](../../01-negocio/reglas/RN-86-obligacion-de-reintegro-por-saldo-no-devuelto.md) — El saldo no devuelto es obligación con responsable y ciclo propio que sobrevive al cierre
- [RN-75](../../01-negocio/reglas/RN-75-bien-retenido-o-sustraido-no-sale-del-registro.md) — El bien no recuperado permanece en el registro hasta su recuperación o descargo
- [RN-04](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md) — Nada se borra; el acta unilateral es asiento con autor, comisión y motivo
- [RN-97](../../01-negocio/reglas/RN-97-saldo-de-apertura-de-control-interno.md) — Lo no terminal al corte constituye saldo de apertura, con antigüedad desde el hecho

## Casos especiales que la afectan

- [CE-26](../casos-especiales/CE-26-sobrante-o-faltante-al-liquidar.md) — El faltante del fondo que nadie devolvió
- [CE-27](../casos-especiales/CE-27-cierre-de-ejercicio-fiscal-con-hallazgo-abierto.md) — El hallazgo abierto atraviesa el cierre de ejercicio
- [CE-04](../casos-especiales/CE-04-robo-de-vehiculo-o-de-carga-en-mision.md) — Un bien no entregado y no localizado deja de ser un problema administrativo

## Criterios de aceptación

```gherkin
# language: es
Característica: Cierre de asignación por entrega unilateral

  Antecedentes:
    Dada una persona "Ramón Cáceres" con asignación abierta al puesto "Jefe de Transporte"
    Y 40 vehículos bajo su tarjeta de responsabilidad
    Y 2 vales de combustible emitidos y no canjeados a su nombre por L 3,500.00
    Y que "Ramón Cáceres" no se presenta desde el "2026-09-15"

  Escenario: Se rechaza el acta unilateral con un solo servidor en la comisión
    Cuando la Jefatura Inmediata levanta el acta unilateral con ella misma como única integrante
    Entonces el sistema rechaza el acta
    Y muestra "El acta de entrega unilateral exige comisión de al menos dos servidores además de quien la levanta. Agregue los integrantes."

  Escenario: Se rechaza el acta unilateral sin inventario de lo no entregado
    Cuando la Jefatura Inmediata levanta el acta sin detallar los bienes no entregados
    Entonces el sistema rechaza el acta
    Y muestra "Detalle uno por uno los 42 bienes y valores no entregados. Un acta sin inventario no acredita nada."

  Escenario: Se rechaza el acta unilateral sin depositario transitorio de los vehículos
    Cuando la Jefatura Inmediata levanta el acta sin indicar quién queda como custodio de los 40 vehículos
    Entonces el sistema rechaza el acta
    Y muestra "Ningún vehículo puede quedar sin custodio. Indique el depositario transitorio de los 40 vehículos."

  Escenario: Se rechaza el acta unilateral cuando el servidor sí está disponible
    Dado que "Ramón Cáceres" figura con asistencia registrada en Talento Humano el "2026-09-16"
    Cuando la Jefatura Inmediata intenta levantar el acta unilateral
    Entonces el sistema advierte "Ramón Cáceres figura activo en Talento Humano al 16/09/2026. La entrega unilateral es para quien no está disponible; use el acta de entrega-recepción."
    Y exige motivo escrito para continuar

  Escenario: El acta unilateral cierra la asignación y abre el hallazgo
    Cuando la Jefatura Inmediata levanta el acta con comisión de 3 servidores, inventario de 42 bienes y depositario transitorio identificado
    Entonces el sistema cierra la asignación de "Ramón Cáceres" al "2026-09-30"
    Y abre un expediente de hallazgo por bienes no entregados
    Y marca los 42 bienes como "PENDIENTE DE DEDUCCIÓN DE RESPONSABILIDAD" a nombre de "Ramón Cáceres"
    Y notifica al Encargado de Bienes Institucionales y al Auditor Interno

  Escenario: El cierre revoca el acceso de inmediato
    Cuando "Ramón Cáceres" intenta autenticarse el "2026-10-01"
    Entonces el sistema rechaza el acceso
    Y muestra "Usted no ocupa ningún puesto vigente al 01/10/2026."

  Escenario: El cierre no extingue la responsabilidad patrimonial
    Dado el hallazgo abierto por 42 bienes y L 3,500.00 en vales
    Cuando la Gerencia Administrativa consulta el expediente de "Ramón Cáceres"
    Entonces el hallazgo figura abierto con su antigüedad desde el "2026-09-30"
    Y el sistema muestra "Cierre de asignación por entrega unilateral. La responsabilidad patrimonial no se extingue con el cierre."
    Y el hallazgo sobrevive al cierre del ejercicio como saldo de apertura

  Escenario: La entrega tardía cierra el hallazgo sin borrar el acta
    Dado el hallazgo abierto
    Cuando "Ramón Cáceres" entrega 40 vehículos el "2026-11-08" y la comisión los constata
    Entonces el sistema registra la entrega parcial con fecha y constatación
    Y el hallazgo permanece abierto por los 2 vales no canjeados
    Y el acta unilateral del "2026-09-30" no se modifica ni se anula
```

## Fuera de alcance

- El cierre ordinario con acta de entrega-recepción — es [HU-136](HU-136-cerrar-asignacion-de-puesto-con-acta.md)
- El procedimiento de deducción de responsabilidad: es competencia de la institución y del TSC; SIGTI registra el hallazgo y aporta la evidencia
- El descargo o baja del bien no recuperado — es [HU-103](HU-103-descargar-el-bien-propio.md)

## Notas y pendientes

- `[C]` **Reglamento interno de uso de vehículos** de la institución: cómo se levanta el acta unilateral, quién integra la comisión y qué formalidad exige — insumo **#1**
- `[C]` **Responsabilidad patrimonial por el bien bajo custodia no entregado**: quién la determina y en qué plazo — insumo **#47**
- `[C]` Si existe unidad de Bienes separada o la función la absorbe la Gerencia Administrativa. Si la absorbe, el acta la levanta y la recibe la misma unidad y hay que activar un control compensatorio — insumo pendiente de [actores-y-roles §9 F](../../01-negocio/actores-y-roles.md)
- `[P]` La exigencia de acta y de constatación física proviene de [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md); el articulado no se pudo extraer
- `[I]` La comisión de *"al menos dos servidores"* la propone [actores-y-roles §2.4](../../01-negocio/actores-y-roles.md); no consta como exigencia normativa
