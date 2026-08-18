# HU-023 — Bloquear el vehículo no operativo o con documentación que no cubre todo el rango de la misión

| Campo | Valor |
|---|---|
| **Módulo** | M-07 Programación y Despacho · M-04 Documentación y Cumplimiento Vehicular |
| **Actor** | ACT-04 Jefe de Transporte · ACT-10 Encargado de Delegación en su ámbito |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Refinada |
| **Deriva de** | [CU-04](../casos-de-uso/CU-04-programar-mision-asignar-vehiculo-y-motorista.md) pasos 4 y 5, E4, E5, E9 · `T-08` · `BD-03`, `BD-07` |

## Historia

**Como** Jefe de Transporte
**quiero** que el sistema verifique el estado operativo del vehículo y la vigencia de su documentación **durante todo el rango de la misión**, distinguiendo lo que bloquea de lo que solo advierte
**para** no despachar una unidad con la matrícula vencida a mitad de viaje ni una que está inmovilizada en el taller, y para que toda advertencia superada quede con nombre y apellido en el expediente

## Contexto

La documentación se revisa hoy el día de la salida y contra la fecha de ese día. Una misión que sale el 28 de septiembre y retorna el 3 de octubre con la matrícula venciendo el 30 circula tres días sin cobertura documental, y el hallazgo lo levanta el TSC un año después.

El tratamiento **no es uniforme**, y confundirlo es el error caro en las dos direcciones: la matrícula y el título de tenencia bloquean; la póliza de seguro y la revisión mecánica **no son obligatorias por la ley vigente** `[V]` [NRM-06](../../01-negocio/normativa/NRM-06-transito-y-licencias.md) y su bloqueo es configurable **apagado por defecto**; la ausencia de placa metálica es **estado válido** por el desabastecimiento nacional y no bloquea nada. Un sistema que bloquea por falta de placa deja a la institución sin flota.

## Reglas que la gobiernan

- [RN-19](../../01-negocio/reglas/RN-19-vehiculo-no-operativo-no-se-asigna.md) — Un vehículo cuyo estado operativo no es disponible no se asigna ni se despacha
- [RN-15](../../01-negocio/reglas/RN-15-identidad-del-vehiculo-y-placa.md) — La identidad es el correlativo institucional; la placa no es obligatoria ni única
- [RN-62](../../01-negocio/reglas/RN-62-titulo-de-tenencia-con-vigencia-y-rubros.md) — Ninguna misión excede la vigencia del título de tenencia
- [RN-16](../../01-negocio/reglas/RN-16-seguro-y-revision-mecanica.md) — Póliza y revisión: advertencia rastreable, bloqueo configurable **apagado por defecto**, con valor distinto por régimen de tenencia
- [RN-18](../../01-negocio/reglas/RN-18-rotulacion-del-vehiculo-del-estado.md) — La constatación de la identificación institucional caduca y se advierte, con umbral más corto sin lámina
- [RN-64](../../01-negocio/reglas/RN-64-estado-de-la-placa-tipificado.md) · [RN-65](../../01-negocio/reglas/RN-65-sin-lamina-respaldo-y-paquete-de-identificacion.md) — Sin lámina: respaldo vigente en todo el rango y paquete de identificación impreso y acusado
- [RN-17](../../01-negocio/reglas/RN-17-alertas-de-vencimiento-documental.md) — Alertas anticipadas por umbrales configurables, también en kilómetros

## Casos especiales que la afectan

- [CE-16](../casos-especiales/CE-16-vehiculo-a-taller-con-misiones-programadas.md) — El vehículo entra a taller con misiones ya programadas
- [CE-17](../casos-especiales/CE-17-vehiculo-sin-placa-metalica.md) — Sin placa metálica, por el desabastecimiento nacional
- [CE-15](../casos-especiales/CE-15-vehiculo-en-comodato-o-alquilado.md) — El régimen de tenencia cambia el parámetro de bloqueo
- [CE-14](../casos-especiales/CE-14-vehiculo-prestado-entre-dependencias-o-instituciones.md) — El vehículo prestado no está disponible para programar

## Criterios de aceptación

```gherkin
# language: es
Característica: Estado operativo y documentación del vehículo al programar

  Antecedentes:
    Dada una misión con ventana del "2026-09-28" al "2026-10-03"
    Y un vehículo "Pickup Toyota Hilux" con correlativo institucional "INS-P-014",
      régimen de tenencia "propio", matrícula vigente hasta el "2027-05-30"
      y estado operativo "DISPONIBLE"
    Y el parámetro "bloqueo por póliza vencida" desactivado para el régimen "propio"

  Escenario: Se rechaza por matrícula que vence dentro del rango
    Dado un vehículo "Microbús Toyota Coaster" con correlativo "INS-B-003"
      y matrícula vigente hasta el "2026-09-30"
    Cuando el Jefe de Transporte intenta asignar el "INS-B-003" a esa misión
    Entonces el sistema rechaza la asignación
    Y muestra "La matrícula del vehículo INS-B-003 vence el 30/09/2026, antes del retorno previsto el 03/10/2026."
    Y registra el intento con la fecha de fin de rango evaluada

  Escenario: Se rechaza por vehículo inmovilizado en taller
    Dado un vehículo "Camión Isuzu FVR" con correlativo "INS-C-002" y estado operativo "EN_TALLER"
      declarado por el Encargado de Mantenimiento el "2026-09-20"
    Cuando el Jefe de Transporte intenta asignar el "INS-C-002" a esa misión
    Entonces el sistema rechaza la asignación
    Y muestra "El vehículo INS-C-002 está EN_TALLER desde el 20/09/2026, con ventana estimada de indisponibilidad hasta el 05/10/2026."
    Y el vehículo no aparece entre los candidatos propuestos para esa ventana

  Escenario: Se rechaza por título de tenencia que no cubre el retorno
    Dado un vehículo "Pickup Nissan Frontier" con correlativo "INS-P-021",
      régimen de tenencia "comodato" con vigencia hasta el "2026-09-30"
    Cuando el Jefe de Transporte intenta asignar el "INS-P-021" a esa misión
    Entonces el sistema rechaza la asignación
    Y muestra "El comodato del vehículo INS-P-021 vence el 30/09/2026 y la misión retorna el 03/10/2026. Ninguna misión puede exceder la vigencia del título de tenencia."

  Escenario: La póliza vencida advierte y no bloquea, y la advertencia queda con nombre
    Dado que la póliza de seguro del "INS-P-014" venció el "2026-08-15"
    Cuando el Jefe de Transporte asigna el "INS-P-014" a esa misión
    Entonces el sistema acepta la asignación
    Y muestra la advertencia "La póliza de seguro del vehículo INS-P-014 venció el 15/08/2026. Puede continuar: el bloqueo por póliza está desactivado para el régimen propio."
    Y registra en el expediente "advertencia superada por el Jefe de Transporte el 01/09/2026"
    Y la advertencia se imprimirá en la Orden de Misión

  Escenario: El mismo vencimiento bloquea cuando el régimen de tenencia lo exige
    Dado un vehículo "Pickup Mitsubishi L200" con correlativo "INS-P-030", régimen "alquilado"
    Y el parámetro "bloqueo por póliza vencida" activado para el régimen "alquilado"
    Y que su póliza venció el "2026-08-15"
    Cuando el Jefe de Transporte intenta asignar el "INS-P-030" a esa misión
    Entonces el sistema rechaza la asignación
    Y muestra "La póliza del vehículo alquilado INS-P-030 venció el 15/08/2026 y el bloqueo está activado para el régimen alquilado."

  Escenario: Vehículo sin lámina metálica con respaldo vigente
    Dado un vehículo "Pickup Toyota Hilux" con correlativo "INS-P-045",
      estado de placa "sin lámina asignada" desde el "2026-03-10"
    Y un documento sustitutivo del Instituto de la Propiedad vigente hasta el "2026-12-31"
    Cuando el Jefe de Transporte asigna el "INS-P-045" a esa misión
    Entonces el sistema acepta la asignación
    Y exige que el paquete de identificación del vehículo se imprima y se acuse al despachar
    Y no muestra ningún error referido a la ausencia de placa

  Escenario: Vehículo sin lámina metálica y sin respaldo vigente en todo el rango
    Dado un vehículo "Pickup Toyota Hilux" con correlativo "INS-P-046",
      estado de placa "sin lámina asignada"
    Y un documento sustitutivo del Instituto de la Propiedad vigente hasta el "2026-09-29"
    Cuando el Jefe de Transporte intenta asignar el "INS-P-046" a esa misión
    Entonces el sistema rechaza la asignación
    Y muestra "El documento sustitutivo del vehículo INS-P-046 vence el 29/09/2026, antes del retorno previsto el 03/10/2026."

  Escenario: Constatación de rotulación caducada
    Dado que la última constatación de la identificación institucional del "INS-P-014"
      se registró el "2026-01-10" con fotografía
    Y un umbral de caducidad de la constatación de "180" días
    Cuando el Jefe de Transporte asigna el "INS-P-014" a esa misión
    Entonces el sistema acepta la asignación
    Y muestra la advertencia "La constatación de franjas, leyenda, siglas y correlativo del vehículo INS-P-014 se hizo el 10/01/2026 y caducó. Constátela al despachar."
```

## Fuera de alcance

- La captura de la ficha del vehículo y de sus vencimientos — es de M-03 y M-04
- La declaración del estado operativo `EN_TALLER`, que ejecuta el Encargado de Mantenimiento — es de M-11
- El desenlace de las reservas ya constituidas cuando el vehículo entra a taller — es [HU-043](HU-043-sustituir-vehiculo-o-motorista-en-programada.md)
- La verificación física de la rotulación en el predio con fecha y foto — es [HU-040](HU-040-acta-de-entrega-y-traslado-de-custodia.md)

## Notas y pendientes

- `[C]` **Si el mantenimiento preventivo vencido bloquea la asignación o solo advierte**, y cuál es la ventana de indisponibilidad estimada exigible al enviar un vehículo a taller — insumo #59.
- `[C]` **Catálogo de documentos sustitutivos que emite el Instituto de la Propiedad** y la vigencia de cada uno — insumo #60. El catálogo se entrega **vacío**: no se inventa.
- `[C]` Si la **rotulación del Estado aplica a vehículos en comodato o alquilados** — insumo #55. Zona gris expresa de [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md).
- `[P]` La no obligatoriedad de póliza y revisión mecánica proviene de [NRM-06](../../01-negocio/normativa/NRM-06-transito-y-licencias.md); el articulado concreto no se pudo extraer.
