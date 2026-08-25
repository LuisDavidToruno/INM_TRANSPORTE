# HU-058 — Registrar una interrupción en ruta sin conectividad y sin atribuir responsabilidad

| Campo | Valor |
|---|---|
| **Módulo** | M-08 Ejecución y Bitácora · M-12 Incidentes, Siniestros y Sanciones |
| **Actor** | ACT-06 Motorista |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — falta la escala de severidad de fallas del vehículo, cuál es incapacitante y cuál no (insumo #35): es lo que decide el desenlace de la interrupción. Falta también el catálogo mínimo autorizado de datos de terceros de siniestro, que `NRM-07` deja `[C]` |

## Historia

**Como** Motorista
**quiero** registrar de inmediato lo que me impidió seguir la misión —una avería, un accidente, una vía cerrada— con hora, lugar, odómetro y fotos, sin señal
**para** que el hecho quede en el expediente desde el minuto en que ocurrió y no dependa de que alguien en oficina decida antes qué hacer

## Contexto

Este caso separa dos cosas que hoy van juntas y no deberían: **el hecho** ocurrió a una hora concreta y hay que registrarlo ya; **la decisión** puede tardar horas y depende de personas que no están en la carretera.

Sin esa separación, el registro queda rehén de la decisión, y lo que pasa en la práctica es que no se registra nada hasta que se resuelve — dejando un hueco de días en el expediente ([RN-70](../../01-negocio/reglas/RN-70-interrupcion-en-ruta-con-desenlace-obligatorio.md)).

**El registro de campo no pregunta de quién fue la culpa.** Ninguna casilla del formulario admite atribución de responsabilidad: eso se determina en el expediente, no en la carretera y no por el conductor ([RN-74](../../01-negocio/reglas/RN-74-sin-atribucion-de-responsabilidad-en-campo.md)). Una declaración de culpa escrita en el arcén, bajo presión y sin asesoría, es evidencia que después la institución no puede desdecir.

Y cuando hay personas involucradas, **primero se atiende y después se captura**: el cliente muestra la guía de actuación antes de cualquier formulario.

## Reglas que la gobiernan

- [RN-70](../../01-negocio/reglas/RN-70-interrupcion-en-ruta-con-desenlace-obligatorio.md) — **Regla rectora**: la interrupción es evento tipificado que marca la misión sin cambiarle el estado
- [RN-74](../../01-negocio/reglas/RN-74-sin-atribucion-de-responsabilidad-en-campo.md) — El registro de campo no captura atribución de responsabilidad
- [RN-43](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md) — Se registra sin ninguna conectividad
- [RN-46](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md) — La hora del hecho es distinta de la hora de captura, y aquí la diferencia importa
- [RN-51](../../01-negocio/reglas/RN-51-minimizacion-de-datos-de-personas-externas.md) — De terceros y lesionados solo se capturan los datos mínimos del catálogo autorizado

## Casos especiales que la afectan

- [CE-02](../casos-especiales/CE-02-averia-mecanica-en-ruta.md) — Avería mecánica en ruta
- [CE-03](../casos-especiales/CE-03-accidente-de-transito-en-mision.md) — Accidente de tránsito en misión
- [CE-04](../casos-especiales/CE-04-robo-de-vehiculo-o-de-carga-en-mision.md) — Robo del vehículo o de la carga
- [CE-10](../casos-especiales/CE-10-motorista-incapacitado-en-ruta.md) — Motorista incapacitado en ruta

## Criterios de aceptación

```gherkin
# language: es
Característica: Registro de la interrupción en ruta

  Antecedentes:
    Dada la Orden de Misión "OM-2026-0451" en estado "EN_RUTA"
    Y un vehículo "Pickup Hilux" con odómetro conocido de "93280" km
    Y que el dispositivo lleva 3 días sin conectividad

  Escenario: El formulario no ofrece ninguna casilla de atribución de responsabilidad
    Cuando "José Martínez" abre el registro de novedad con causa "accidente de tránsito"
    Entonces el formulario no presenta ninguna casilla de culpa, responsable del hecho ni versión de responsabilidad
    Y muestra "Registre lo que pasó, no de quién fue. La responsabilidad se determina en el expediente."

  Escenario: Se rechaza el registro sin causa tipificada
    Cuando "José Martínez" registra una interrupción con solo la descripción "se dañó"
    Entonces el sistema rechaza el registro
    Y muestra "Seleccione la causa: avería mecánica, accidente, sustracción, vía cerrada, condición de seguridad, incapacidad del conductor o retención por autoridad."

  Escenario: Se rechaza el registro sin odómetro ni hora del hecho
    Cuando "José Martínez" registra una interrupción por "avería mecánica" sin odómetro
    Entonces el sistema rechaza el registro
    Y muestra "Falta el kilometraje al momento del hecho. Es el corte que separa lo recorrido de lo que no se recorrió."

  Escenario: La guía de actuación aparece antes que cualquier formulario
    Cuando "José Martínez" selecciona la causa "accidente de tránsito con personas lesionadas"
    Entonces el sistema muestra primero la guía de actuación del paquete de misión
    Y ofrece diferir el registro con "Atienda primero. Cuando pueda, vuelva: nada de lo que ya registró se pierde."
    Y no exige completar ningún campo para salir de la pantalla

  Escenario: La interrupción marca la misión y no le cambia el estado
    Cuando "José Martínez" registra una interrupción por "avería mecánica" a las "11:40" con odómetro "93280", ubicación "km 84 carretera a El Amatillo" y 3 fotografías
    Entonces la Orden de Misión "OM-2026-0451" permanece en estado "EN_RUTA"
    Y queda con la marca "interrumpida" y la lista de pendientes visible
    Y el evento queda en estado de sincronización "PENDIENTE_DE_ENVIO"

  Escenario: No se ofrece anular una misión que ya salió
    Cuando "José Martínez" o el Jefe de Transporte buscan anular la Orden de Misión "OM-2026-0451"
    Entonces el sistema no ofrece la acción de anular
    Y muestra "Esta misión ya salió del predio y consumió recursos. No se anula: regístrela como retorno anticipado o retorno sin vehículo."

  Escenario: De los terceros solo se capturan los datos mínimos autorizados
    Cuando "José Martínez" registra un accidente con un tercero involucrado
    Entonces el sistema solicita únicamente los campos del catálogo mínimo autorizado
    Y no solicita datos de salud, ni antecedentes, ni ningún dato fuera del catálogo
    Y registra quién consulte después ese dato, cuándo y desde dónde
```

## Fuera de alcance

- El cambio de estado operativo del vehículo por causa de la interrupción — es [HU-059](HU-059-vehiculo-fuera-de-circulacion-desde-la-hora-del-hecho.md)
- El desenlace de la interrupción y su seguimiento — es [HU-060](HU-060-desenlace-obligatorio-de-la-interrupcion.md)
- El expediente de investigación del incidente en M-12 y la orden de trabajo en M-11: se abren desde aquí, se gestionan allá

## Notas y pendientes

- `[C]` Escala de severidad de fallas del vehículo: cuál es incapacitante y cuál no — insumo #35
- `[C]` Catálogo mínimo autorizado de datos de terceros de siniestro — [NRM-07](../../01-negocio/normativa/NRM-07-transparencia-y-datos-personales.md) lo deja `[C]`
- `[P]` La guía de actuación en accidente se apoya en [NRM-06](../../01-negocio/normativa/NRM-06-transito-y-licencias.md); el contenido exacto de la obligación de conducta no se pudo extraer del articulado
