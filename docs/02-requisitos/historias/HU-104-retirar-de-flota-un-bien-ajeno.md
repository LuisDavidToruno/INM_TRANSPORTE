# HU-104 — Registrar el fin de tenencia de un bien ajeno como retiro de flota, no como baja patrimonial

| Campo | Valor |
|---|---|
| **Módulo** | M-03 Flota Vehicular |
| **Actor** | ACT-14 Encargado de Bienes Institucionales |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — bloqueada por un hallazgo abierto: **el estado terminal `RETIRADO_DE_FLOTA` no existe todavía en la máquina de estados del vehículo**, y sin él la devolución al comodante no tiene desenlace válido. Falta además cómo se registra hoy esa devolución (insumo #58) y la modalidad de alquiler vigente con sus condiciones de sustitución (insumo #57) |

## Historia

**Como** Encargado de Bienes Institucionales
**quiero** registrar la devolución de un vehículo al comodante o al arrendador como **retiro de flota**, con acta, odómetro y liquidación de daños, y no como descargo
**para** no asentar una baja patrimonial que nunca ocurrió sobre un bien que nunca estuvo en el inventario de la institución

## Contexto

Declarar *dado de baja* un pickup que se devolvió al comodante registra un descargo que nunca ocurrió sobre un bien que nunca estuvo en el inventario nacional: **es un asiento falso**, y es exactamente el tipo de asiento que el TSC encuentra cruzando el inventario de bienes contra el padrón de flota.

**Fin de tenencia ≠ descargo.** El bien no se descargó, se devolvió.

Y todo el historial del período de tenencia —bitácoras, consumos, incidentes, costos— **se conserva íntegro: no se va con el vehículo**. La institución respondió por ese vehículo mientras lo tuvo, y esa responsabilidad no desaparece cuando lo entrega.

## Reglas que la gobiernan

- [RN-62](../../01-negocio/reglas/RN-62-titulo-de-tenencia-con-vigencia-y-rubros.md) — Fin de tenencia ≠ descargo; los rubros asumidos definen la liquidación de daños
- [RN-63](../../01-negocio/reglas/RN-63-prestamo-de-vehiculo-como-expediente-del-bien.md) — El préstamo y la cesión son expedientes del bien, con receptor, fecha comprometida y actas
- [RN-22](../../01-negocio/reglas/RN-22-custodia-del-vehiculo.md) — La devolución exige acta con odómetro y novedades
- [RN-89](../../01-negocio/reglas/RN-89-kilometraje-acumulado-invariante-del-expediente.md) — El kilometraje del período de tenencia queda asentado y no se arrastra a otra unidad
- [RN-05](../../01-negocio/reglas/RN-05-registro-cerrado-no-se-edita.md) — El historial del período de tenencia se conserva íntegro
- [RN-15](../../01-negocio/reglas/RN-15-identidad-del-vehiculo-y-placa.md) — El correlativo institucional queda ocupado permanentemente

## Casos especiales que la afectan

- [CE-15](../casos-especiales/CE-15-vehiculo-en-comodato-o-alquilado.md) — Eje de la historia
- [CE-14](../casos-especiales/CE-14-vehiculo-prestado-entre-dependencias-o-instituciones.md) — El préstamo que sí regresa, frente al fin de tenencia definitivo

## Criterios de aceptación

```gherkin
# language: es
Característica: Fin de tenencia de un bien ajeno

  Antecedentes:
    Dado un vehículo "TR-0092" en régimen "comodato", titular "Secretaría de Salud", vigencia hasta el "2027-01-14"
    Y un kilometraje acumulado del expediente de "38,910" km

  Escenario: Se rechaza dar de baja un bien que no es de la institución
    Cuando el Encargado de Bienes intenta instruir el descargo de "TR-0092"
    Entonces el sistema rechaza la acción
    Y muestra "TR-0092 está en régimen comodato, titular Secretaría de Salud. Un bien ajeno no se descarga: se retira de flota al terminar la tenencia."

  Escenario: Se rechaza la devolución sin acta con odómetro
    Cuando el Encargado de Bienes registra la devolución al comodante sin odómetro
    Entonces el sistema rechaza el registro
    Y muestra "El acta de devolución exige odómetro, novedades y liquidación de daños."

  Escenario: Se registra el retiro de flota y el vehículo queda en estado terminal propio
    Cuando el Encargado de Bienes registra la devolución a "Secretaría de Salud" con acta, odómetro "38,910" km, novedades y liquidación de daños
    Entonces el vehículo pasa a estado "RETIRADO_DE_FLOTA"
    Y no pasa a "DADO_DE_BAJA"
    Y el título de tenencia queda cerrado con su fecha de fin real

  Escenario: El historial del período de tenencia se conserva
    Dado que "TR-0092" está en "RETIRADO_DE_FLOTA"
    Cuando el Auditor Interno consulta las bitácoras, consumos, incidentes y costos del período de tenencia
    Entonces todos siguen siendo consultables y exportables
    Y el historial no se elimina ni se transfiere al titular del bien

  Escenario: Se rechaza el retiro con misiones abiertas
    Dado 1 misión "OM-2026-0488" de "TR-0092" en estado "RETORNADA"
    Cuando el Encargado de Bienes intenta registrar la devolución
    Entonces el sistema rechaza la acción
    Y muestra "TR-0092 tiene 1 misión sin terminar: OM-2026-0488, en estado RETORNADA desde el 12/12/2026. Liquídela y ciérrela antes de devolver el vehículo al comodante."
    Y la lista de misiones no terminales indica, por cada una, su folio, su estado y su fecha

  Escenario: La liquidación de daños se evalúa contra los rubros asumidos
    Dado un título de comodato con rubros de combustible y peajes a cargo de la institución, y daños a cargo del comodante
    Cuando el Encargado de Bienes registra una novedad de daño en la devolución
    Entonces el sistema muestra "El rubro daños está a cargo de Secretaría de Salud según el título de tenencia vigente del 15/01/2026."
    Y no genera obligación a cargo de la institución por ese rubro

  Escenario: El correlativo institucional no se recicla tras el retiro
    Cuando el Encargado de Bienes intenta usar el correlativo "TR-0092" para un vehículo nuevo
    Entonces el sistema rechaza el alta
    Y muestra "El correlativo TR-0092 quedó ocupado permanentemente por un vehículo retirado de flota el 14/01/2027."

  Escenario: El kilometraje del período no se arrastra a la unidad sustituta
    Dado que el arrendador entrega una unidad sustituta bajo el mismo título
    Cuando el Encargado de Bienes da de alta la unidad entrante
    Entonces la unidad entrante inicia su propia serie de odómetro
    Y el acumulado de "38,910" km queda asentado únicamente en el expediente de "TR-0092"

  Escenario: Un préstamo a otra dependencia no es un fin de tenencia
    Cuando el Encargado de Bienes cede temporalmente "TR-0045" a la Delegación Choluteca
    Entonces el sistema registra un expediente de préstamo con receptor, fecha comprometida de devolución y actas
    Y el vehículo no cambia de titular ni de régimen de tenencia
    Y el kilometraje recorrido bajo tenencia ajena queda asentado con las dos lecturas del acta y excluido de la conciliación de rendimiento
```

## Fuera de alcance

- El descargo del bien propio — es [HU-103](HU-103-descargar-el-bien-propio.md)
- El alta de la unidad sustituta — es [HU-096](HU-096-dar-de-alta-el-vehiculo-con-titulo-de-tenencia.md)
- La negociación del convenio de comodato o del contrato de alquiler: fuera de SIGTI
- El cobro de daños al comodante o al arrendador: se registra la liquidación, la gestión es administrativa

## Notas y pendientes

- ⚠️ **Hallazgo abierto: el estado terminal `RETIRADO_DE_FLOTA` no existe todavía en la máquina de estados del vehículo.** Hoy el único terminal es `DADO_DE_BAJA`, alcanzable solo por descargo, y ambas transiciones suponen que el bien es de la institución. La **autoridad en transiciones** es [`docs/03-arquitectura/estados/`](../../03-arquitectura/estados/orden-de-mision.md): **esta historia no crea el estado**, lo requiere. Sin él, la devolución al comodante no tiene desenlace válido. Hallazgo ya abierto en [CE-15](../casos-especiales/CE-15-vehiculo-en-comodato-o-alquilado.md) y en el índice de reglas
- `[C]` **Cómo se registra hoy la devolución al comodante** — insumo **#58**
- `[C]` Modalidad de alquiler vigente y condiciones de sustitución de unidad — insumo **#57**
- `[C]` Formato en papel del acta de devolución — insumo **#2**
- Corregido por `HB34-19`: el rechazo por misiones abiertas no tenía mensaje especificado. Es un bloqueo que el Encargado de Bienes resuelve con una gestión administrativa, y necesita saber **cuál** misión y **en qué estado**; sin eso el bloqueo es una pared sin puerta.
