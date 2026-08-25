# HU-102 — Habilitar el vehículo en flota solo con la lista de comprobación completa

| Campo | Valor |
|---|---|
| **Módulo** | M-03 Flota Vehicular |
| **Actor** | ACT-04 Jefe de Transporte |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — falta el catálogo `estado_operativo_vehiculo` y qué estados habilitan la asignación (insumo #1): es la definición misma de *habilitado en flota*, no un parámetro. Faltan también el `horizonte_reservas_afectadas` al inhabilitar (insumo #1) y la modalidad de alquiler con sus condiciones de sustitución de unidad (insumo #57) |

## Historia

**Como** Jefe de Transporte
**quiero** que la habilitación en flota sea un acto separado del alta, condicionado a una lista de comprobación completa, y que el estado `NO_DISPONIBLE` muestre siempre su causa tipificada
**para** que ningún vehículo con la ficha a medio llenar aparezca asignable el mismo día en que ingresó

## Contexto

**El alta ingresa siempre a `NO_DISPONIBLE`.** Habilitar es un acto separado y esa separación es deliberada.

Lo que la lista de comprobación protege no es la prolijidad del registro: son los bloqueos duros que dependen de ella. Sin peso bruto no se evalúa la licencia del motorista; sin categoría de peaje resuelta no se estima ni se concilia el peaje; sin custodio vigente no hay responsable identificado del bien.

Y el estado **nunca queda vacío**: un vehículo `NO_DISPONIBLE` sin causa visible es un vehículo que nadie sabe cómo recuperar.

## Reglas que la gobiernan

- [RN-19](../../01-negocio/reglas/RN-19-vehiculo-no-operativo-no-se-asigna.md) — Solo se asigna desde `DISPONIBLE`; el catálogo de estados marca cuáles habilitan asignación
- [RN-62](../../01-negocio/reglas/RN-62-titulo-de-tenencia-con-vigencia-y-rubros.md) — Sin título vigente no se habilita
- [RN-22](../../01-negocio/reglas/RN-22-custodia-del-vehiculo.md) — Sin custodio vigente no se habilita
- [RN-33](../../01-negocio/reglas/RN-33-categoria-de-peaje-derivada-de-ficha-tecnica.md) — Sin categoría de peaje resuelta el vehículo no es asignable
- [RN-09](../../01-negocio/reglas/RN-09-matriz-licencia-vehiculo.md) — Sin los atributos de la ficha técnica la habilitación licencia↔vehículo no se puede evaluar
- [RN-18](../../01-negocio/reglas/RN-18-rotulacion-del-vehiculo-del-estado.md) — La identificación constatada forma parte de la lista
- [RN-60](../../01-negocio/reglas/RN-60-indisponibilidad-sobrevenida-y-reservas.md) — La indisponibilidad sobrevenida afecta las reservas dentro del horizonte configurado

## Casos especiales que la afectan

- [CE-16](../casos-especiales/CE-16-vehiculo-a-taller-con-misiones-programadas.md) — Inhabilitación con misiones ya programadas
- [CE-15](../casos-especiales/CE-15-vehiculo-en-comodato-o-alquilado.md) — Sustitución de unidad por el arrendador
- [CE-12](../casos-especiales/CE-12-dos-solicitudes-compiten-por-el-mismo-vehiculo.md) — Un vehículo menos en la flota disponible cambia la competencia por el recurso

## Criterios de aceptación

```gherkin
# language: es
Característica: Habilitación del vehículo en la flota

  Antecedentes:
    Dado un vehículo "TR-0092" recién dado de alta, en estado "NO_DISPONIBLE"

  Escenario: El estado NO_DISPONIBLE siempre muestra su causa tipificada
    Cuando el Jefe de Transporte consulta "TR-0092"
    Entonces el sistema muestra la causa "expediente incompleto"
    Y detalla qué falta: ficha técnica evaluable, custodio vigente, identificación constatada
    Y el estado nunca aparece vacío

  Escenario: Se rechaza habilitar sin título de tenencia vigente
    Dado un título de tenencia vencido el "2026-08-31"
    Cuando el Jefe de Transporte solicita la habilitación de "TR-0092"
    Entonces el sistema rechaza la habilitación
    Y muestra "El título de tenencia de TR-0092 venció el 31/08/2026. Sin título vigente el vehículo no se habilita en la flota."

  Escenario: Se rechaza habilitar sin custodio vigente
    Cuando el Jefe de Transporte solicita la habilitación de "TR-0092" sin tarjeta de responsabilidad emitida
    Entonces el sistema rechaza la habilitación
    Y muestra "TR-0092 no tiene custodio vigente. Emita la tarjeta de responsabilidad antes de habilitar."

  Escenario: Se rechaza habilitar sin categoría de peaje resuelta
    Cuando el Jefe de Transporte solicita la habilitación de "TR-0092" sin peso bruto vehicular en la ficha
    Entonces el sistema rechaza la habilitación
    Y muestra "TR-0092 no tiene categoría de peaje resuelta: falta el peso bruto vehicular. Sin ese dato tampoco se puede evaluar la matriz licencia↔vehículo."

  Escenario: Se rechaza habilitar sin identificación constatada
    Cuando el Jefe de Transporte solicita la habilitación de "TR-0092" sin constatación de identificación
    Entonces el sistema rechaza la habilitación
    Y muestra "TR-0092 no tiene constatación de identificación institucional con fotografía."

  Escenario: Se habilita con la lista de comprobación completa
    Dado un título vigente, correlativo asignado, ficha técnica evaluable, categoría de peaje resuelta, custodio vigente, documentación registrada e identificación constatada
    Cuando el Jefe de Transporte solicita la habilitación de "TR-0092"
    Entonces el vehículo pasa a estado "DISPONIBLE"
    Y queda asignable en la programación
    Y el acto queda registrado con autor, puesto, momento y huella del contenido

  Escenario: El alta y la habilitación no ocurren en el mismo acto
    Cuando el Encargado de Bienes da de alta un vehículo con el expediente completo
    Entonces el vehículo queda en "NO_DISPONIBLE"
    Y el sistema no lo habilita automáticamente
    Y muestra "Habilitar en flota es un acto separado, a cargo del Jefe de Transporte."

  Escenario: Inhabilitar lista las misiones programadas afectadas
    Dado un vehículo "TR-0045" en "DISPONIBLE" con 3 misiones programadas dentro de los próximos 10 días
    Cuando el Jefe de Transporte inhabilita "TR-0045" por causa "ingreso a taller"
    Entonces el vehículo pasa a "NO_DISPONIBLE" con causa tipificada
    Y el sistema lista las 3 misiones programadas afectadas con su fecha
    Y no cancela ninguna misión automáticamente

  Escenario: La reserva sobre un vehículo inhabilitado se marca, no se borra
    Cuando el Jefe de Transporte inhabilita "TR-0045"
    Entonces las reservas dentro del horizonte configurado quedan marcadas como afectadas
    Y siguen siendo consultables con su asignación original

  Escenario: Sustitución de unidad por el arrendador
    Dado un vehículo "TR-0092" en régimen de alquiler que el arrendador retira
    Cuando el Encargado de Bienes da de alta la unidad entrante bajo el mismo título de tenencia
    Entonces la unidad entrante recibe su propio correlativo institucional
    Y comienza con su propia serie de odómetro, sin arrastrar el kilometraje de la saliente
    Y las misiones programadas sobre la unidad saliente se revalidan, recalculando y congelando de nuevo todo valor derivado
```

## Fuera de alcance

- El alta patrimonial — es [HU-096](HU-096-dar-de-alta-el-vehiculo-con-titulo-de-tenencia.md)
- La ficha técnica — es [HU-098](HU-098-completar-la-ficha-tecnica-que-habilita.md)
- La programación y la reasignación de misiones: pertenecen a M-07
- El ingreso y salida de taller: pertenecen a M-11
- El descargo del bien — es [HU-103](HU-103-descargar-el-bien-propio.md)

## Notas y pendientes

- `[C]` Catálogo `estado_operativo_vehiculo` y qué estados habilitan asignación — insumo **#1**
- `[C]` `horizonte_reservas_afectadas` al inhabilitar — insumo **#1**
- `[C]` Modalidad de alquiler y condiciones de sustitución de unidad — insumo **#57**
- La **autoridad en transiciones del vehículo** es [`docs/03-arquitectura/estados/`](../../03-arquitectura/estados/orden-de-mision.md). Esta historia consume esas transiciones, no las define
