# HU-138 — Traspasar en bloque las custodias de un puesto a otro, con acta y un asiento por cada bien

| Campo | Valor |
|---|---|
| **Módulo** | M-01 Organización y Seguridad · M-03 Flota Vehicular |
| **Actor** | ACT-14 Encargado de Bienes Institucionales · ACT-04 Jefe de Transporte |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — depende de [HU-099](HU-099-emitir-tarjeta-de-responsabilidad-y-traspasar-custodia.md) para la mecánica de la tarjeta de responsabilidad |

## Historia

**Como** Encargado de Bienes Institucionales
**quiero** traspasar en una sola operación las custodias de un servidor saliente a uno entrante, generando el acta de entrega-recepción y **un asiento individual por cada bien**
**para** que un cambio de administración con cuarenta vehículos se resuelva en minutos y no en cuarenta trámites que nadie termina

## Contexto

`RNF-15` lo pone como umbral medible: *"Traspaso masivo de custodias entre dos motoristas: una sola operación, con acta generada, para ≥ 50 vehículos, en ≤ 5 min."* No es una comodidad: es lo que decide si el traspaso se hace o si el sistema se abandona el día del cambio de administración.

Y hay una tensión que hay que resolver bien: **la operación es en bloque, pero el registro es uno por uno**. Un traspaso masivo que produzca un solo asiento agregado deja al auditor sin poder responder *"¿desde cuándo responde esta persona por este vehículo?"* — y esa es exactamente la pregunta que se hace cuando aparece un daño.

**Nada se traspasa a ciegas.** El entrante debe poder rechazar un bien cuyo estado no corresponde al que declara la tarjeta, y ese rechazo es información, no un obstáculo.

## Reglas que la gobiernan

- [RN-22](../../01-negocio/reglas/RN-22-custodia-del-vehiculo.md) — Todo vehículo tiene custodio vigente, y el cambio se hace con constancia
- [RN-04](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md) — El traspaso no reescribe la custodia anterior: la cierra y abre una nueva
- [RN-18](../../01-negocio/reglas/RN-18-rotulacion-del-vehiculo-del-estado.md) — La constatación de identificación institucional se verifica con fecha y fotografía y caduca
- [RN-03](../../01-negocio/reglas/RN-03-registro-inmutable-de-autorizacion.md) — Cada asiento registra quién entrega, quién recibe, momento y contenido
- [RN-89](../../01-negocio/reglas/RN-89-kilometraje-acumulado-invariante-del-expediente.md) — El odómetro al momento del traspaso es dato del expediente del vehículo

## Casos especiales que la afectan

- [CE-16](../casos-especiales/CE-16-vehiculo-a-taller-con-misiones-programadas.md) — Un vehículo en taller no está físicamente disponible para constatarse en el traspaso
- [CE-04](../casos-especiales/CE-04-robo-de-vehiculo-o-de-carga-en-mision.md) — Un bien no localizable no se traspasa: se declara
- [CE-19](../casos-especiales/CE-19-vehiculo-asignado-a-funcionario-frente-al-pool.md) — El vehículo de asignación permanente tiene custodio distinto del pool

## Criterios de aceptación

```gherkin
# language: es
Característica: Traspaso masivo de custodias entre puestos

  Antecedentes:
    Dada una persona "Ramón Cáceres" con 40 vehículos bajo su tarjeta de responsabilidad
    Y una persona "María López" que ocupará el puesto "Jefe de Transporte" desde el "2026-10-01"

  Escenario: Se rechaza el traspaso de un vehículo que está en misión
    Dado que 3 de los 40 vehículos están en estado "EN_MISION"
    Cuando el Encargado de Bienes inicia el traspaso de los 40 vehículos
    Entonces el sistema excluye los 3 vehículos en misión
    Y muestra "3 vehículos están EN_MISION y no se pueden constatar: TR-0092, TR-0104, TR-0117. Se traspasan al retorno o se declaran con constatación diferida y motivo."
    Y traspasa los 37 restantes

  Escenario: Se rechaza el traspaso sin receptor con puesto vigente
    Cuando el Encargado de Bienes intenta traspasar las custodias a "María López" el "2026-09-25"
    Entonces el sistema rechaza el traspaso
    Y muestra "María López no ocupa ningún puesto vigente al 25/09/2026. Su asignación al puesto Jefe de Transporte inicia el 01/10/2026."

  Escenario: Se rechaza el traspaso de un bien declarado no localizable
    Dado un vehículo "TR-0055" declarado no localizable el "2026-09-12"
    Cuando el Encargado de Bienes incluye "TR-0055" en el traspaso
    Entonces el sistema rechaza su inclusión
    Y muestra "TR-0055 está declarado no localizable desde el 12/09/2026. Un bien no localizable no se traspasa: permanece en el registro a nombre del custodio anterior hasta su recuperación o descargo."

  Escenario: El receptor rechaza un bien cuyo estado no corresponde
    Dado el traspaso en curso de 37 vehículos
    Cuando "María López" rechaza el vehículo "TR-0088" con motivo "carrocería con daño no registrado en la tarjeta" y fotografía adjunta
    Entonces el sistema excluye "TR-0088" del acta
    Y abre una novedad del bien con la fotografía y el motivo
    Y traspasa los 36 restantes
    Y muestra "36 vehículos traspasados. TR-0088 excluido con novedad abierta."

  Escenario: El traspaso en bloque produce un asiento por cada bien
    Cuando el Encargado de Bienes confirma el traspaso de 36 vehículos el "2026-10-01"
    Entonces el sistema genera 36 asientos individuales de cambio de custodia
    Y cada asiento registra vehículo, custodio saliente, custodio entrante, odómetro, momento y folio del acta
    Y cierra las 36 custodias anteriores sin modificarlas
    Y ninguna custodia queda abierta a nombre de "Ramón Cáceres"

  Escenario: El acta de entrega-recepción se emite con folio y QR
    Cuando el Encargado de Bienes confirma el traspaso
    Entonces el sistema emite el acta de entrega-recepción con folio y QR verificable
    Y el acta enumera los 36 vehículos con su correlativo institucional, su odómetro y su estado
    Y el acta lleva espacio de firma del entregante, del receptor y del Encargado de Bienes

  Escenario: El traspaso parcial no deja ningún vehículo sin custodio
    Dado que 3 vehículos quedaron pendientes por estar en misión
    Cuando el Encargado de Bienes cierra la operación
    Entonces los 3 vehículos conservan a "Ramón Cáceres" como custodio hasta su retorno
    Y el sistema abre una tarea de traspaso pendiente sobre esos 3
    Y bloquea el cierre de la asignación de "Ramón Cáceres" mientras existan
```

## Fuera de alcance

- El cierre de la asignación de puesto que este traspaso desbloquea — es [HU-136](HU-136-cerrar-asignacion-de-puesto-con-acta.md)
- La emisión de la tarjeta de responsabilidad individual — es [HU-099](HU-099-emitir-tarjeta-de-responsabilidad-y-traspasar-custodia.md)
- La constatación física periódica del inventario — pertenece a M-03 y a `ACT-14`
- El traspaso del fondo de combustible y de los vales, que sigue el circuito de M-09

## Notas y pendientes

- `[P]` La tarjeta de responsabilidad y el acta de entrega-recepción provienen de [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md); el articulado no se pudo extraer
- `[C]` **Formato en papel del acta de entrega-recepción vigente en la institución** — insumo **#2**
- `[C]` Si el traspaso requiere constatación física con fotografía de cada bien o basta la declaración del receptor. La postura de esta historia es que el receptor **puede** rechazar con fotografía, no que esté obligado a fotografiar los 36 — confirmar con la unidad de Bienes
- `[C]` Si existe unidad de Bienes separada — insumo pendiente de [actores-y-roles §9 F](../../01-negocio/actores-y-roles.md)
