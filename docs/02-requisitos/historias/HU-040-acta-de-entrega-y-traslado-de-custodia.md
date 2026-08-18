# HU-040 — Levantar el acta de entrega del vehículo y trasladar la custodia de la misión al motorista

| Campo | Valor |
|---|---|
| **Módulo** | M-08 Ejecución y Bitácora · M-07 Programación y Despacho · M-03 Flota Vehicular |
| **Actor** | ACT-05 Encargado de Despacho · ACT-06 Motorista · ACT-13 Custodio del Vehículo |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Refinada |
| **Deriva de** | [CU-06](../casos-de-uso/CU-06-despachar-y-registrar-salida.md) pasos 5, 10 y 11, A5, E10 · `T-12` · `INV-22` |

## Historia

**Como** Encargado de Despacho
**quiero** levantar junto con el motorista el acta de entrega del vehículo —odómetro con fotografía, nivel de tanque, llantas, herramientas, extintor, documentos a bordo, daños preexistentes y constatación de la identificación institucional— y trasladarle la custodia de la misión contra firma
**para** que al retorno se pueda determinar sin discusión qué daño existía antes y qué ocurrió durante la misión

## Contexto

Hoy la entrega se hace de palabra y con las llaves. Cuando el vehículo vuelve con un golpe en la puerta, no hay forma de saber si venía de antes, y la discusión termina en el peor lugar posible: la responsabilidad patrimonial decidida por quién insiste más.

El acta con fotografías cierra esa discusión antes de que exista. Y la **constatación de la identificación institucional** —franjas azul-blanco-azul, leyenda, siglas y correlativo— es hallazgo frecuente de auditoría `[V]` [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md): constatarla con fecha y foto en cada despacho es lo que permite responder con evidencia.

**La custodia se desdobla:** el Custodio del Vehículo conserva la custodia patrimonial permanente del bien; lo que se traslada al motorista es la **custodia de la misión**, temporal, y se registra aparte. Confundirlas es lo que hace que nadie sepa quién responde.

## Reglas que la gobiernan

- [RN-22](../../01-negocio/reglas/RN-22-custodia-del-vehiculo.md) — **Regla rectora**: todo vehículo tiene custodio vigente, y el despacho traslada la custodia al motorista con constancia
- [RN-18](../../01-negocio/reglas/RN-18-rotulacion-del-vehiculo-del-estado.md) — La identificación institucional se constata con fecha y fotografía
- [RN-64](../../01-negocio/reglas/RN-64-estado-de-la-placa-tipificado.md) · [RN-65](../../01-negocio/reglas/RN-65-sin-lamina-respaldo-y-paquete-de-identificacion.md) — Sin lámina: paquete de identificación impreso y acusado
- [RN-89](../../01-negocio/reglas/RN-89-kilometraje-acumulado-invariante-del-expediente.md) — El kilometraje acumulado es atributo del expediente, independiente de la lectura del instrumento
- [RN-31](../../01-negocio/reglas/RN-31-odometro-de-retorno.md) — Coherencia del odómetro de salida contra la última lectura conocida
- [RN-74](../../01-negocio/reglas/RN-74-sin-atribucion-de-responsabilidad-en-campo.md) — El acta registra el hecho, **no atribuye responsabilidad**: eso se determina en el expediente

## Casos especiales que la afectan

- [CE-17](../casos-especiales/CE-17-vehiculo-sin-placa-metalica.md) — Sin lámina, el paquete de identificación se entrega y se acusa en este acto
- [CE-22](../casos-especiales/CE-22-odometro-inconsistente.md) — La lectura se registra tal como se ve, y la inconsistencia se declara
- [CE-19](../casos-especiales/CE-19-vehiculo-asignado-a-funcionario-frente-al-pool.md) — El custodio que autoriza la salida de su propio vehículo: advertencia con motivo escrito

## Criterios de aceptación

```gherkin
# language: es
Característica: Acta de entrega del vehículo y traslado de la custodia de misión

  Antecedentes:
    Dada una Orden de Misión "OM-2026-0451" que se despacha el "2026-09-15"
    Y un vehículo "Pickup Toyota Hilux" con correlativo "INS-P-014"
    Y una última lectura de odómetro registrada de "84500" km al retorno de la misión anterior
    Y un motorista "José Martínez"
    Y un Custodio del Vehículo "Rubén Ordóñez"

  Escenario: Se rechaza el despacho sin acta de entrega completa
    Cuando el Encargado de Despacho confirma el despacho sin registrar el odómetro inicial
      ni su fotografía
    Entonces el sistema rechaza el despacho
    Y muestra "El acta de entrega exige odómetro inicial con fotografía. Sin acta no hay traslado de custodia ni salida."

  Escenario: Se rechaza un odómetro inicial menor a la última lectura conocida
    Cuando el Encargado de Despacho registra un odómetro inicial de "84200" km
    Entonces el sistema rechaza el registro
    Y muestra "El odómetro inicial (84,200) es menor a la última lectura registrada del vehículo (84,500). Verifique la lectura o declare intervención del instrumento."
    Y permite declarar "intervención del instrumento de medición" con orden de trabajo y respaldo

  Escenario: Se rechaza el despacho de un vehículo sin lámina cuyo paquete de identificación no se acusó
    Dado un vehículo "Pickup Toyota Hilux" con correlativo "INS-P-045", sin lámina metálica
    Y un documento sustitutivo del Instituto de la Propiedad vigente
    Cuando el Encargado de Despacho confirma el despacho sin el acuse del paquete de identificación
      por parte del motorista
    Entonces el sistema rechaza el despacho
    Y muestra "El vehículo INS-P-045 no tiene lámina metálica: el paquete de identificación debe imprimirse y acusarse antes de la salida."

  Escenario: Se levanta el acta completa con daños preexistentes fotografiados
    Cuando el Encargado de Despacho y el motorista registran juntos el acta con:
      odómetro inicial "84520" km con fotografía, nivel de tanque "3/4",
      estado de llantas y llanta de repuesto, herramientas, extintor, documentos a bordo,
      y un daño preexistente "rayón en puerta trasera derecha" con fotografía
    Entonces el sistema registra el acta con la firma de ambos
    Y el daño preexistente queda vinculado al expediente del vehículo con fecha "2026-09-15"
    Y el acta no admite ninguna anotación de responsabilidad sobre ese daño

  Escenario: Se constata la identificación institucional con fecha y fotografía
    Cuando el Encargado de Despacho registra la constatación de franjas, leyenda, siglas
      y correlativo del "INS-P-014" con fotografía
    Entonces la constatación queda con fecha "2026-09-15" en el expediente del vehículo
    Y la advertencia de constatación caducada deja de mostrarse para las misiones siguientes
      hasta que venza el umbral

  Escenario: El traslado de custodia distingue la patrimonial de la de misión
    Cuando el Encargado de Despacho entrega llaves, documentos impresos y custodia
      al motorista "José Martínez" contra firma
    Entonces la custodia de la misión queda registrada a nombre de "José Martínez",
      con fecha y hora de inicio
    Y la custodia patrimonial permanente del bien sigue registrada a nombre de "Rubén Ordóñez"
    Y el paquete de misión se transfiere al dispositivo portador designado, con marca de tiempo

  Escenario: El custodio que autoriza la salida de su propio vehículo continúa con motivo escrito
    Dado que el Custodio del Vehículo "Rubén Ordóñez" es también quien autoriza la salida
    Cuando "Rubén Ordóñez" autoriza la salida del "INS-P-014"
    Entonces el sistema muestra la advertencia "Rubén Ordóñez es el custodio permanente del vehículo INS-P-014 y autoriza su salida. Registre el motivo."
    Y exige un motivo escrito para continuar
    Y el hecho se lista en el reporte de excepciones
```

## Fuera de alcance

- El registro de la salida por el motorista en su dispositivo — es [HU-042](HU-042-registro-de-la-salida-sin-conectividad.md)
- El acta de retorno y la comparación de estado al final de la misión — son de M-08 y M-13
- La determinación de responsabilidad patrimonial por un daño — es de M-12
- El inventario y el acta de entrega de la carga — es parte del juego documental, [HU-031](HU-031-consumo-del-folio-y-emision-del-juego-documental.md)

## Notas y pendientes

- `[C]` **Formatos en papel vigentes**, en especial el acta de entrega — insumo #2. Cada casilla del formato actual es una regla que nadie escribió.
- `[C]` **Responsabilidad patrimonial por el bien bajo custodia de misión** — insumo #47.
- `[C]` **¿Cómo se rotula una motocicleta del Estado?** El acuerdo describe franjas en puertas laterales, que una moto no tiene — insumo #43.
- `[V]` La obligación de identificación del vehículo del Estado proviene de [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md); la tarjeta de responsabilidad del custodio está `[P]`.
