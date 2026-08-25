# HU-096 — Dar de alta el vehículo con su título de tenencia y su correlativo institucional

| Campo | Valor |
|---|---|
| **Módulo** | M-03 Flota Vehicular |
| **Actor** | ACT-14 Encargado de Bienes Institucionales |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — falta saber si la institución numera el correlativo institucional **por delegación** (insumo #34): de eso depende cuál es el identificador único del vehículo, que es el eje de la ficha. Faltan también la modalidad de alquiler vigente (insumo #57) y los formatos en papel del acta de recepción y de la tarjeta de responsabilidad (insumo #2) |

## Historia

**Como** Encargado de Bienes Institucionales
**quiero** dar de alta el vehículo con el documento que originó su ingreso, su título de tenencia con vigencia y rubros, y un correlativo institucional único e irreciclable
**para** que la institución pueda decir, para cualquier fecha del pasado, bajo qué título tenía ese vehículo y quién pagaba su combustible, sus llantas y sus multas

## Contexto

El alta del bien no es un formulario de catálogo: es un acto con consecuencia patrimonial. Y el **título de tenencia** es el que decide qué puede hacer la institución con la unidad: un vehículo en comodato con rubro de mantenimiento a cargo del comodante no se manda al taller institucional, y un alquiler que vence el 30 de noviembre no puede sostener una misión que retorna el 3 de diciembre.

**Sin título vigente el vehículo no se habilita en la flota.**

El alta ingresa siempre a `NO_DISPONIBLE`. **Habilitar es un acto separado del alta**, y esa separación es deliberada: es lo que impide que un vehículo entre a la flota con la ficha a medio llenar y aparezca asignable el mismo día.

## Reglas que la gobiernan

- [RN-62](../../01-negocio/reglas/RN-62-titulo-de-tenencia-con-vigencia-y-rubros.md) — Régimen, titular, documento, vigencia y rubros asumidos; sin título vigente no hay habilitación
- [RN-15](../../01-negocio/reglas/RN-15-identidad-del-vehiculo-y-placa.md) — Correlativo institucional obligatorio, único en la institución y **no reciclable**
- [RN-63](../../01-negocio/reglas/RN-63-prestamo-de-vehiculo-como-expediente-del-bien.md) — El préstamo es expediente propio del bien, **nunca una Orden de Misión**
- [RN-19](../../01-negocio/reglas/RN-19-vehiculo-no-operativo-no-se-asigna.md) — Solo se asigna desde `DISPONIBLE`
- [RN-03](../../01-negocio/reglas/RN-03-registro-inmutable-de-autorizacion.md) — Autor, puesto, momento y huella del contenido
- [RN-01](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md) — El alta del bien y el número de inventario son competencia de Bienes, no de Transporte

## Casos especiales que la afectan

- [CE-15](../casos-especiales/CE-15-vehiculo-en-comodato-o-alquilado.md) — Regímenes de tenencia distintos de la propiedad
- [CE-14](../casos-especiales/CE-14-vehiculo-prestado-entre-dependencias-o-instituciones.md) — El préstamo no es un traslado ni una misión
- [CE-19](../casos-especiales/CE-19-vehiculo-asignado-a-funcionario-frente-al-pool.md) — Régimen de uso distinto del régimen de tenencia

## Criterios de aceptación

```gherkin
# language: es
Característica: Alta del vehículo en el registro institucional

  Antecedentes:
    Dado un catálogo "regimen_de_tenencia" vigente con los valores "propiedad", "comodato", "alquiler" y "traslado interinstitucional"
    Y un vehículo existente "TR-0045" en el registro

  Escenario: Se rechaza el alta sin documento de origen
    Cuando el Encargado de Bienes registra un vehículo sin acta de recepción, acta de donación, convenio, contrato ni resolución
    Entonces el sistema rechaza el alta
    Y muestra "Adjunte el documento que origina el ingreso del vehículo: acta de recepción de compra, acta de donación, convenio de comodato, contrato de alquiler o resolución de traslado."

  Escenario: Se rechaza el alta sin título de tenencia
    Cuando el Encargado de Bienes registra un vehículo sin régimen, titular, vigencia ni rubros asumidos
    Entonces el sistema rechaza el alta
    Y muestra "Registre el título de tenencia: régimen, titular, documento adjunto, rango de vigencia y rubros asumidos —combustible, mantenimiento, llantas, seguro, peajes, multas y daños."

  Escenario: Se rechaza el alta con correlativo institucional duplicado
    Cuando el Encargado de Bienes registra un vehículo con correlativo institucional "TR-0045"
    Entonces el sistema rechaza el alta
    Y muestra "El correlativo TR-0045 ya está asignado al Pickup Toyota Hilux dado de alta el 14/03/2024. El correlativo es único en la institución y no se recicla."

  Escenario: Un correlativo de vehículo dado de baja tampoco se reutiliza
    Dado un vehículo "TR-0012" en estado "DADO_DE_BAJA" desde el "2025-11-20"
    Cuando el Encargado de Bienes intenta usar el correlativo "TR-0012" para un vehículo nuevo
    Entonces el sistema rechaza el alta
    Y muestra "El correlativo TR-0012 quedó ocupado permanentemente por un vehículo dado de baja el 20/11/2025."

  Escenario: Un régimen distinto de propiedad exige fecha de fin de vigencia
    Cuando el Encargado de Bienes registra un título de régimen "comodato" sin fecha de fin
    Entonces el sistema rechaza el registro
    Y muestra "El régimen comodato exige fecha de fin de vigencia. Solo el régimen propiedad admite vigencia indefinida."

  Escenario: Se da de alta y el vehículo entra NO_DISPONIBLE
    Cuando el Encargado de Bienes da de alta un vehículo con correlativo "TR-0092", régimen "comodato" del "2026-01-15" al "2027-01-14", rubros de combustible y peajes a cargo de la institución, y convenio adjunto
    Entonces el vehículo queda registrado en estado "NO_DISPONIBLE"
    Y muestra la causa tipificada "expediente incompleto: falta ficha técnica, custodio y constatación de identificación"
    Y el estado nunca queda vacío

  Escenario: El vehículo no es asignable mientras esté NO_DISPONIBLE
    Cuando el Jefe de Transporte intenta programar una misión con "TR-0092"
    Entonces el sistema rechaza la asignación
    Y muestra "TR-0092 está NO_DISPONIBLE: expediente incompleto. Solo se asigna desde DISPONIBLE."

  Escenario: El Jefe de Transporte no ejecuta el alta del bien
    Cuando el Jefe de Transporte intenta dar de alta un vehículo con su número de inventario nacional
    Entonces el sistema rechaza la acción
    Y muestra "El alta del bien y el número de inventario nacional son competencia de la unidad de Bienes. Su competencia cubre ficha técnica, documentación, vencimientos, estado operativo y habilitación en flota."

  Escenario: Se registra el valor de adquisición y la fuente de financiamiento
    Cuando el Encargado de Bienes da de alta un vehículo comprado
    Entonces el sistema exige número de bien del inventario nacional, valor de adquisición y fuente de financiamiento

  Escenario: Un préstamo no se instrumenta como Orden de Misión
    Cuando alguien intenta registrar la cesión temporal del vehículo a otra dependencia como una Orden de Misión
    Entonces el sistema rechaza la acción
    Y muestra "El préstamo del vehículo es un expediente del bien, con receptor, fecha comprometida de devolución y actas. No se instrumenta como Orden de Misión."
```

## Fuera de alcance

- La ficha técnica y la derivación de categoría de peaje — es [HU-098](HU-098-completar-la-ficha-tecnica-que-habilita.md)
- El estado de la placa — es [HU-097](HU-097-registrar-la-placa-y-el-estado-de-la-lamina.md)
- La tarjeta de responsabilidad y la custodia — es [HU-099](HU-099-emitir-tarjeta-de-responsabilidad-y-traspasar-custodia.md)
- La habilitación en flota — es [HU-102](HU-102-habilitar-el-vehiculo-en-flota.md)
- El descargo y el fin de tenencia — es [HU-103](HU-103-descargar-el-bien-propio.md) y [HU-104](HU-104-retirar-de-flota-un-bien-ajeno.md)
- El inventario nacional de bienes en sí: SIGTI registra la referencia, no lo reemplaza

## Notas y pendientes

- `[C]` **¿La institución numera el correlativo por delegación?** Si es así, el identificador único es la composición código de delegación + número — insumo **#34**
- `[C]` Modalidad de alquiler vigente y si contempla sustitución de unidad — insumo **#57**
- `[C]` Cómo se registra hoy la devolución al comodante — insumo **#58**
- `[C]` Régimen de asignación permanente de vehículo a funcionario — insumo **#64**
- `[C]` Formatos en papel del acta de recepción y de la tarjeta de responsabilidad — insumo **#2**
- `[P]` La tarjeta de responsabilidad y el descargo provienen de [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md); el Manual de Propiedad Estatal los regula pero **no se pudo extraer el articulado**. **No se eleva el nivel**
