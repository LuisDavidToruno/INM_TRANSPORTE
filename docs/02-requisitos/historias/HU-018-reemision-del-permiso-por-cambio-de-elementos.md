# HU-018 — Reemitir el permiso cuando cambia vehículo, motorista, ruta o ventana

| Campo | Valor |
|---|---|
| **Módulo** | M-04 Documentación y Cumplimiento Vehicular (con M-15 Formatos Oficiales e Impresión) |
| **Actor** | ACT-04 Jefe de Transporte |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Refinada |

## Historia

**Como** Jefe de Transporte
**quiero** que el sistema detecte que un permiso ya firmado dejó de cubrir la misión cuando cambia el vehículo, el motorista, la ruta o la ventana, anule el salvoconducto anterior y exija un permiso nuevo con firma nueva
**para** que el motorista no salga amparado en un papel que ya no corresponde, que es el error más fácil de cometer y el que un operativo del TSC detecta de inmediato

## Contexto

El permiso es **específico**: ampara ese vehículo, esa ruta y esa ventana. Es la redacción más exigente de las tres que conviven en el diseño, y se adopta a propósito porque es la conservadora ante un operativo — resolución `HCU-04` de [CU-03](../casos-de-uso/CU-03-permiso-de-circulacion-en-dia-inhabil.md).

Los disparadores son cotidianos: el vehículo entra a taller la víspera, el motorista aparece con incapacidad, la misión se reprograma para el fin de semana siguiente. En papel, nadie vuelve a mirar el salvoconducto ya firmado: se sale con él y se confía en que nadie compare.

**La firma anterior no se arrastra.** Un permiso nuevo requiere firma nueva de la máxima autoridad. Y el folio anulado **no se recicla**: la página de verificación debe reflejar el cambio de inmediato, para que un papel anulado no pase un control.

Excepción deliberada: **un relevo de motorista documentado en ruta no invalida el permiso de la misión ya iniciada** — se registra el traspaso con acta y corte de odómetro, y el permiso se reemite para el tramo restante cuando la circulación en franja inhábil continúa.

## Reglas que la gobiernan

- [RN-23](../../01-negocio/reglas/RN-23-permiso-de-circulacion-en-dia-inhabil.md) — El permiso ampara vehículo, ruta y ventana determinados
- [RN-25](../../01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md) — El salvoconducto anulado se refleja de inmediato en la verificación; el folio no se recicla
- [RN-61](../../01-negocio/reglas/RN-61-sustitucion-de-vehiculo-recalcula-valores-congelados.md) — La sustitución de vehículo recalcula y vuelve a congelar todo valor derivado, con asiento de diferencia
- [RN-14](../../01-negocio/reglas/RN-14-sustitucion-de-motorista.md) — La sustitución de motorista o vehículo revalida todas las habilitaciones
- [RN-71](../../01-negocio/reglas/RN-71-traspaso-en-ruta-con-acta-y-corte-de-odometro.md) — Todo traspaso en ruta consta en acta con odómetro
- [RN-04](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md) — La anulación del salvoconducto es asiento reverso con motivo y autor, con referencia cruzada

## Casos especiales que la afectan

- [CE-16](../casos-especiales/CE-16-vehiculo-a-taller-con-misiones-programadas.md) — El vehículo entra a taller con misiones ya programadas
- [CE-13](../casos-especiales/CE-13-motorista-no-disponible-por-talento-humano.md) — Motorista no disponible por permiso, vacaciones o incapacidad
- [CE-05](../casos-especiales/CE-05-cambio-de-motorista-con-la-mision-en-curso.md) — Relevo de motorista con la misión en curso

## Criterios de aceptación

```gherkin
# language: es
Característica: Reemisión del permiso de circulación por cambio de sus elementos
  Como Jefe de Transporte
  quiero que el permiso deje de cubrir la misión cuando cambia lo que ampara
  para que nadie circule con un papel que ya no corresponde

  Antecedentes:
    Dado un permiso "PC-2026-0009" firmado por "Doris Cruz", vigente para el vehículo "VH-0142", el motorista "José Martínez", la ruta "Tegucigalpa–Choluteca" y la ventana del "2026-03-20 07:00" al "2026-03-21 17:00"
    Y un salvoconducto "CHO-SC-2026-0011" impreso para ese permiso

  Escenario: Se bloquea el despacho con el permiso que dejó de cubrir por cambio de vehículo
    Dado un vehículo "VH-0142" que pasó a estado "En taller" el "2026-03-19"
    Y una sustitución por el vehículo "VH-0155"
    Cuando el Encargado de Despacho intenta despachar la misión amparada en "PC-2026-0009"
    Entonces el sistema no ejecuta el despacho
    Y muestra "El permiso PC-2026-0009 ampara el vehículo VH-0142 y la misión se ejecutará con VH-0155. El permiso debe reemitirse y firmarse de nuevo (RN-23)."

  Escenario: El cambio de vehículo anula el salvoconducto anterior con referencia cruzada
    Dada una sustitución del vehículo "VH-0142" por "VH-0155"
    Cuando el Jefe de Transporte registra la sustitución
    Entonces el salvoconducto "CHO-SC-2026-0011" pasa a estado "ANULADO"
    Y el registro de anulación indica el motivo "Sustitución de vehículo VH-0142 por VH-0155" y su autor
    Y el folio "CHO-SC-2026-0011" no vuelve a asignarse

  Escenario: El cambio de motorista antes de la salida obliga a reemitir
    Dado un motorista "José Martínez" con incapacidad registrada desde el "2026-03-19"
    Y una sustitución por el motorista "Wilmer Andino"
    Cuando el Jefe de Transporte registra la sustitución antes del despacho
    Entonces el permiso "PC-2026-0009" deja de cubrir la misión
    Y el sistema exige un permiso nuevo con firma de la máxima autoridad
    Y muestra "El permiso es nominativo sobre el motorista. La firma anterior no se arrastra."

  Escenario: La reprogramación de fecha no arrastra el permiso
    Dada una reprogramación de la ventana al "2026-03-27 07:00" hasta el "2026-03-28 17:00"
    Cuando el Jefe de Transporte registra la reprogramación
    Entonces el permiso "PC-2026-0009" deja de cubrir la misión
    Y muestra "El permiso ampara del 20/03/2026 al 21/03/2026 y la misión se ejecutará del 27/03/2026 al 28/03/2026. Reemita el permiso; la vigencia no se traslada."

  Escenario: El permiso nuevo requiere firma nueva, no la firma anterior
    Dado un expediente de permiso reemitido para el vehículo "VH-0155"
    Cuando el sistema genera el permiso nuevo
    Entonces el permiso nuevo queda sin firma
    Y no hereda la firma de "Doris Cruz" del permiso "PC-2026-0009"
    Y el salvoconducto nuevo recibe un folio distinto de "CHO-SC-2026-0011"

  Escenario: La sustitución de vehículo recalcula los valores congelados
    Dado un estimado de peajes congelado de "L 150.00" con categoría "Liviano" para el vehículo "VH-0142"
    Y un vehículo sustituto "VH-0155" cuya ficha técnica resuelve la categoría de peaje "Camión 2 ejes"
    Cuando el Jefe de Transporte registra la sustitución
    Entonces el sistema recalcula el estimado de peajes con la categoría "Camión 2 ejes"
    Y registra el asiento de diferencia entre el valor anterior y el nuevo
    Y vuelve a congelar el valor recalculado

  Escenario: El relevo documentado en ruta no invalida el permiso de la misión ya iniciada
    Dada una misión en estado "EN_RUTA" amparada por "PC-2026-0009"
    Y un traspaso registrado con acta y corte de odómetro de "84960" km al motorista "Wilmer Andino"
    Cuando el motorista relevado registra el traspaso
    Entonces el permiso "PC-2026-0009" conserva su vigencia sobre la misión iniciada
    Y el sistema señala que la circulación en franja inhábil posterior al traspaso requiere permiso reemitido
    Y no ajusta ninguna hora registrada para que la misión quepa en horario hábil
```

## Fuera de alcance

- La **decisión** de sustituir vehículo o motorista y la revalidación de habilitaciones: son de [CU-04](../casos-de-uso/CU-04-programar-mision-asignar-vehiculo-y-motorista.md) y M-07. Aquí solo se resuelve el efecto sobre el permiso
- El **permiso sobreviniente** por prórroga en ruta que empuja la misión a franja inhábil no cubierta: **no es bloqueable** porque el vehículo ya está en carretera. Se registra con justificación obligatoria y, si no se emite, la misión cierra con el hallazgo `H-05`. Es de M-08
- La verificación por QR del salvoconducto anulado — es [HU-019](HU-019-verificacion-del-salvoconducto-en-carretera.md)

## Notas y pendientes

- **Hallazgo abierto y adoptado aquí:** `BD-04` dice *"vigente para esa ventana y ese vehículo"*, `PC-03` dice *"vehículo, motorista y ventana"* y [`RN-23`](../../01-negocio/reglas/RN-23-permiso-de-circulacion-en-dia-inhabil.md) dice *"vehículo, ruta y ventana"*. Esta historia adopta la redacción **más exigente** por ser la conservadora y porque la regla es autoridad en materia de negocio. `BD-04` debe alinearse — autoridad: [máquina de estados](../../03-arquitectura/estados/orden-de-mision.md)
- `[C]` Si un relevo de motorista documentado **invalida** el permiso o solo obliga a reemitir para el tramo restante: la postura adoptada es la segunda. Confirmar con la institución — insumo #1
- `[V]` La exigencia del permiso por vehículo y ventana consta en [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md); que alcance también a motorista y ruta es `[I]`, adoptado por criterio conservador
- Trazabilidad: [CU-03](../casos-de-uso/CU-03-permiso-de-circulacion-en-dia-inhabil.md) excepciones E2, E3 y E5; nota de hallazgo `HCU-04`

---

## Nota de alineación con la autoridad

> Una versión anterior de esta historia describía el salvoconducto como amparando **vehículo, motorista, ruta y ventana**, y exigía reemisión tras un relevo de motorista.
>
> **`BD-04` de la [máquina de estados](../../03-arquitectura/estados/orden-de-mision.md) — autoridad en precondiciones — dice que ampara vehículo, ruta y ventana.** El motorista figura impreso en el documento, pero **un relevo documentado no invalida el permiso** y no exige reemisión.
>
> La razón es operativa: si el relevo invalidara el permiso, un motorista incapacitado un domingo en carretera dejaría el vehículo varado esperando otra firma de la máxima autoridad — un bien del Estado abandonado en la vía es peor que el riesgo que el permiso controla. Ver hallazgo `HB3-07`.
>
> `[C]` Pendiente de confirmar con Auditoría Interna: `NRM-02` no precisa el alcance del permiso. Si la institución exige que sea nominativo por motorista, se revierte y hay que diseñar la salida para el relevo en día inhábil.
