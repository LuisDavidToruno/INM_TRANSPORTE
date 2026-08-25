# HU-059 — Sacar el vehículo de circulación desde la hora del hecho, no desde la hora de captura

| Campo | Valor |
|---|---|
| **Módulo** | M-03 Flota Vehicular · M-11 Mantenimiento y Taller · M-12 Incidentes, Siniestros y Sanciones |
| **Actor** | ACT-06 Motorista declara · ACT-11 Encargado de Mantenimiento recibe la orden de trabajo |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — falta la escala de severidad de fallas que decide si el vehículo va a `EN_TALLER`, a `NO_DISPONIBLE` o puede continuar (insumo #35), los receptores válidos de custodia fuera de sede cuando no hay predio institucional cerca (insumo #51) y el plazo de la obligación de recuperación |

## Historia

**Como** Encargado de Mantenimiento
**quiero** que el vehículo que se averió en carretera salga del conjunto asignable desde la hora en que se averió, no desde la hora en que la noticia llegó a la oficina
**para** que nadie programe mañana una misión con una unidad que está varada en el km 84

## Contexto

Entre que un vehículo se avería a las 11:40 en una zona sin cobertura y que la novedad llega al servidor pueden pasar dos días. Si el estado operativo cambia desde la hora de captura, hay una ventana de dos días en la que el sistema ofrece como disponible una unidad que no lo está — y alguien la programa.

**El estado del vehículo lo registran los propios motoristas desde el campo** ([DP-001, D-08](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md)). No hay nadie más en el km 84.

Y `NO_DISPONIBLE` **siempre lleva causa tipificada**: sin tipificación, ese estado se convierte en el cementerio donde se esconde la flota que nadie repara.

## Reglas que la gobiernan

- [RN-19](../../01-negocio/reglas/RN-19-vehiculo-no-operativo-no-se-asigna.md) — **Regla rectora**: el vehículo no operativo no se asigna, y su estado cambia desde la hora del hecho
- [RN-70](../../01-negocio/reglas/RN-70-interrupcion-en-ruta-con-desenlace-obligatorio.md) — El evento de interrupción es lo que dispara el cambio de estado operativo
- [RN-46](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md) — La hora del hecho rige el efecto; la de captura solo documenta cuándo se supo
- [RN-75](../../01-negocio/reglas/RN-75-bien-retenido-o-sustraido-no-sale-del-registro.md) — El bien resguardado fuera de sede permanece en el registro con obligación de recuperación
- [RN-22](../../01-negocio/reglas/RN-22-custodia-del-vehiculo.md) — Siempre hay alguien identificado que responde por el vehículo, aunque esté en un predio ajeno

## Casos especiales que la afectan

- [CE-02](../casos-especiales/CE-02-averia-mecanica-en-ruta.md) — Avería mecánica en ruta
- [CE-16](../casos-especiales/CE-16-vehiculo-a-taller-con-misiones-programadas.md) — El vehículo entra a taller con misiones ya programadas
- [CE-04](../casos-especiales/CE-04-robo-de-vehiculo-o-de-carga-en-mision.md) — El vehículo sustraído que no sale del registro

## Criterios de aceptación

```gherkin
# language: es
Característica: Estado operativo del vehículo desde la hora del hecho

  Antecedentes:
    Dado un vehículo "Pickup Hilux" en estado operativo "EN_MISION" por la Orden de Misión "OM-2026-0451"
    Y una avería declarada por el motorista el "2026-05-14" a las "11:40", capturada a las "11:45"
    Y que el dispositivo estuvo sin conectividad hasta el "2026-05-16" a las "09:00"

  Escenario: Se rechaza asignar el vehículo averiado a una misión nueva
    Dado que la avería ya sincronizó
    Cuando el Encargado de Despacho intenta asignar "Pickup Hilux" a la Orden de Misión "OM-2026-0470"
    Entonces el sistema rechaza la asignación
    Y muestra "Pickup Hilux está EN_TALLER desde el 14/05/2026 a las 11:40 por avería mecánica en ruta. No está disponible."

  Escenario: Se rechaza dejar el vehículo NO_DISPONIBLE sin causa tipificada
    Cuando el Encargado de Mantenimiento intenta poner "Pickup Hilux" en "NO_DISPONIBLE" sin seleccionar causa
    Entonces el sistema rechaza el cambio de estado
    Y muestra "Seleccione la causa. Un vehículo NO_DISPONIBLE sin causa no se puede reportar ni recuperar."

  Escenario: El cambio de estado rige desde la hora del hecho, no desde la de sincronización
    Cuando la avería sincroniza el "2026-05-16" a las "09:00"
    Entonces el sistema registra el cambio a "EN_TALLER" con efecto desde el "2026-05-14" a las "11:40"
    Y el expediente muestra que se supo el "2026-05-16" a las "09:00"
    Y las dos fechas quedan visibles y distintas

  Escenario: Misiones programadas con el vehículo mientras nadie sabía de la avería
    Dada una Orden de Misión "OM-2026-0470" programada el "2026-05-15" con "Pickup Hilux"
    Cuando la avería del "2026-05-14" sincroniza el "2026-05-16"
    Entonces el sistema marca "OM-2026-0470" como programada con un vehículo que no estaba operativo
    Y notifica al Jefe de Transporte "Pickup Hilux estaba averiado desde el 14/05/2026. Revise la Orden de Misión OM-2026-0470."
    Y no revierte ni borra la programación ya ejecutada

  Escenario: La avería abre orden de trabajo correctiva
    Cuando la avería con causa "falla de sistema de frenos" sincroniza
    Entonces el sistema abre una orden de trabajo correctiva en M-11 vinculada al evento
    Y la vincula al expediente del vehículo "Pickup Hilux"

  Escenario: El vehículo queda resguardado en un predio ajeno
    Cuando "José Martínez" registra el acta de resguardo en "Taller Hermanos Cruz, Nacaome", bajo responsabilidad de "Marvin Cruz"
    Entonces el sistema registra dónde quedó el vehículo y bajo responsabilidad de quién
    Y abre la obligación de recuperación con responsable nombrado y fecha límite
    Y el vehículo permanece en el registro de la flota, nunca se declara dado de baja
```

## Fuera de alcance

- El registro del evento de interrupción en sí — es [HU-058](HU-058-registrar-interrupcion-en-ruta.md)
- La gestión de la orden de trabajo correctiva y el ciclo de taller — es de M-11
- La reprogramación de las misiones afectadas — es de M-07 y de [CE-16](../casos-especiales/CE-16-vehiculo-a-taller-con-misiones-programadas.md)

## Notas y pendientes

- `[C]` Escala de severidad de fallas: cuál lleva a `EN_TALLER`, cuál a `NO_DISPONIBLE` y cuál permite continuar — insumo #35
- `[C]` Receptores válidos de custodia fuera de sede cuando no hay predio institucional cerca — insumo #51
- `[C]` Plazo de la obligación de recuperación de un vehículo resguardado fuera de sede
