# HU-061 — Revalidar la habilitación del motorista entrante antes de aceptar el relevo en ruta

| Campo | Valor |
|---|---|
| **Módulo** | M-08 Ejecución y Bitácora · M-05 Motoristas y Habilitación |
| **Actor** | ACT-06 Motorista saliente y motorista entrante |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — falta el límite de jornada de conducción (insumo #48), sin el cual el motivo "jornada cumplida" no tiene umbral verificable, y falta zanjar si la póliza cubre a un conductor no registrado en el padrón (insumo #49) |

## Nota de corrección — hallazgos `HB34-01` y `HB34-02`

> **`HB34-01` — esta historia y [HU-045](HU-045-relevo-de-motorista-en-ruta.md) eran la misma.** Mismo actor, misma regla rectora `RN-71`, mismos casos especiales, mismo objeto. No estaban en la tabla de solapamientos del [`README`](README.md) y **resolvían distinto el mismo caso**: aquí se rechaza al relevo cuya licencia vence antes del retorno; allá se aceptaba invocando `RN-55`.
>
> **Esta historia tenía razón.** [`RN-10`](../../01-negocio/reglas/RN-10-licencia-vigente-en-todo-el-rango.md) aplica *«a toda asignación, despacho, **sustitución en ruta** y extensión de misión»*, y `RN-55` se autolimita al vencimiento **sobrevenido**. `HU-045` quedó corregida.
>
> **Delimitación adoptada**, con la regla general del `README` —el lote de flujo manda en el acto, el lote de expediente manda en el dato—:
>
> | | `HU-061` (M-05) | `HU-045` (M-07 · M-16) |
> |---|---|---|
> | Manda en | **La revalidación del entrante**: matriz categoría↔vehículo, vigencia en todo el rango, restricciones, relevo declarado en la programación, segregación `I-11` | **El acto del traspaso**: acta, corte de odómetro, custodia, traspaso del fondo, código de autorización fuera de línea, impedimento de firma |
>
> Se retiran de aquí los escenarios del fondo, que son de `HU-045`. Ambas van **al mismo sprint**.
>
> **Código de autorización fuera de línea.** El camino feliz de esta historia ocurría a tres días sin conectividad **sin ningún código**. Se adopta la postura de `HU-045`: **es obligatorio**, porque es la única constancia de que la jefatura conoció el cambio de custodia antes de que ocurriera. `[C]` reversible — ver notas.
>
> **`HB34-02` — `I-11` no se evaluaba sobre el entrante.** [`RN-01`](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md) exige comprobar la matriz *«antes de asignar o **sustituir** al motorista»*, y `I-11` es núcleo irreductible: quien autorizó, despachó, entregó el fondo o liquidó la misión no puede recibir su conducción. Se agrega la regla y el escenario.

## Historia

**Como** Motorista
**quiero** entregar el vehículo a mi relevo en carretera con un acta que fije la hora, el lugar y el kilometraje del traspaso, aunque no haya señal
**para** que desde ese kilómetro el combustible, los peajes y lo que ocurra dejen de imputarse a mi tramo y pasen al de quien recibió

## Contexto

El relevo ocurre por jornada de conducción cumplida, por incapacidad del conductor o por decisión del Jefe de Transporte. Sin acta, la misión termina con un solo responsable de todo lo que pasó en 600 km que condujeron dos personas distintas.

**El odómetro del acta es el corte de imputación entre tramos** ([RN-71](../../01-negocio/reglas/RN-71-traspaso-en-ruta-con-acta-y-corte-de-odometro.md)). Sin ese corte, promediar el rendimiento de dos conductores produce un número que no describe a ninguno de los dos.

**La habilitación se verifica sobre quien efectivamente conduce**, cualquiera sea su puesto. No es regla del padrón de motoristas: es regla de quien va al volante ([RN-57](../../01-negocio/reglas/RN-57-habilitacion-de-quien-efectivamente-conduce.md)).

Y la custodia **se cierra siempre**, aunque el conductor saliente no pueda firmar: consta el impedimento y firman dos personas presentes más el receptor tipificado.

## Reglas que la gobiernan

- [RN-71](../../01-negocio/reglas/RN-71-traspaso-en-ruta-con-acta-y-corte-de-odometro.md) — **Regla rectora**: todo traspaso en ruta consta en acta con odómetro, y ese odómetro es el corte de imputación
- [RN-72](../../01-negocio/reglas/RN-72-imputacion-por-tramo-de-vehiculo-y-motorista.md) — Kilometraje, combustible y peajes se imputan por tramo
- [RN-57](../../01-negocio/reglas/RN-57-habilitacion-de-quien-efectivamente-conduce.md) — La habilitación se verifica sobre quien va al volante
- [RN-09](../../01-negocio/reglas/RN-09-matriz-licencia-vehiculo.md) — La categoría debe habilitar el tipo, el peso bruto y la capacidad del vehículo
- [RN-10](../../01-negocio/reglas/RN-10-licencia-vigente-en-todo-el-rango.md) — **Regla que zanja `HB34-01`**: la licencia debe estar vigente en **todo** el rango, y la regla aplica expresamente a la **sustitución en ruta**. Bloqueo duro
- [RN-01](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md) — **Segregación sobre el entrante.** `I-11` es núcleo irreductible y se comprueba *antes de sustituir al motorista*. Incorporada por `HB34-02`
- [RN-14](../../01-negocio/reglas/RN-14-sustitucion-de-motorista.md) — La sustitución exige revalidación completa contra el paquete normativo congelado
- [RN-22](../../01-negocio/reglas/RN-22-custodia-del-vehiculo.md) — La custodia se cierra siempre, con constancia del impedimento si lo hay
- [RN-43](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md) — El acta se levanta sin conectividad

## Casos especiales que la afectan

- [CE-05](../casos-especiales/CE-05-cambio-de-motorista-con-la-mision-en-curso.md) — Cambio de motorista con la misión en curso
- [CE-10](../casos-especiales/CE-10-motorista-incapacitado-en-ruta.md) — El motorista se incapacita y no puede firmar
- [CE-11](../casos-especiales/CE-11-licencia-vence-durante-la-mision.md) — El relevo entra con licencia que vence dentro de la ventana

## Criterios de aceptación

```gherkin
# language: es
Característica: Relevo de motorista en ruta

  Antecedentes:
    Dada la Orden de Misión "OM-2026-0451" en estado "EN_RUTA" con ventana hasta el "2026-05-16"
    Y un vehículo "Camión Isuzu FVR" con peso bruto de "12000" kg
    Y un motorista titular "José Martínez"
    Y un paquete normativo congelado el "2026-05-12" en el dispositivo portador
    Y que el dispositivo lleva 3 días sin conectividad
    Y un código de autorización fuera de línea vigente para "OM-2026-0451", dictado por el Jefe de Transporte

  Escenario: Se rechaza el relevo con licencia de categoría insuficiente
    Dado un relevo declarado "Marvin Cruz" con licencia categoría "C1" vigente
    Cuando "José Martínez" registra el traspaso a "Marvin Cruz"
    Entonces el sistema rechaza el traspaso
    Y muestra "La licencia categoría C1 de Marvin Cruz no habilita un vehículo de 12,000 kg. Se requiere categoría C."
    Y registra el intento en la bitácora del dispositivo

  Escenario: Se rechaza el relevo con licencia vencida dentro de la ventana autorizada
    Dado un relevo declarado "Marvin Cruz" con licencia categoría "C" vigente hasta el "2026-05-15"
    Cuando "José Martínez" registra el traspaso a "Marvin Cruz" el "2026-05-14"
    Entonces el sistema rechaza el traspaso
    Y muestra "La licencia de Marvin Cruz vence el 15/05/2026, antes del retorno previsto el 16/05/2026. El relevo es una sustitución en ruta y exige licencia vigente en todo el rango (RN-10)."
    Y el bloqueo no admite excepción por urgencia, por jerarquía ni por estar en carretera

  Escenario: Se rechaza el traspaso a quien ejerció una función de control sobre la misma misión
    Dado un servidor "Carlos Rodríguez" declarado como relevo, con licencia categoría "C" vigente hasta el "2028-04-30"
    Y que "Carlos Rodríguez" autorizó la Orden de Misión "OM-2026-0451" el "2026-05-11"
    Cuando "José Martínez" registra el traspaso a "Carlos Rodríguez"
    Entonces el sistema rechaza el traspaso
    Y muestra "Carlos Rodríguez autorizó la Orden de Misión OM-2026-0451 el 11/05/2026. Por incompatibilidad I-11 (RN-01) no puede recibir la conducción de esa misión. Es núcleo irreductible: no admite excepción."
    Y registra el intento con el par de incompatibilidad detectado
    Y el bloqueo se aplica igual sin conectividad, con la matriz que viaja en el paquete de misión

  Escenario: Se rechaza el traspaso sin el código de autorización fuera de línea
    Dado un relevo declarado "Marvin Cruz" con licencia categoría "C" vigente hasta el "2027-01-30"
    Cuando "José Martínez" registra el traspaso sin ingresar el código de autorización fuera de línea
    Entonces el sistema rechaza el traspaso
    Y muestra "El relevo sin conectividad requiere el código de autorización fuera de línea de esta misión."

  Escenario: Se rechaza el traspaso a quien no fue declarado como relevo en la programación
    Dado un servidor "Carlos Fúnez" que no figura como relevo declarado de "OM-2026-0451"
    Cuando "José Martínez" registra el traspaso a "Carlos Fúnez"
    Entonces el sistema rechaza el traspaso
    Y muestra "Carlos Fúnez no está declarado como relevo de esta misión ni tiene verificación de licencia registrada."

  Escenario: Se rechaza el acta de traspaso sin odómetro
    Dado un relevo declarado "Marvin Cruz" con licencia categoría "C" vigente hasta el "2027-01-30"
    Cuando "José Martínez" registra el traspaso sin ingresar odómetro
    Entonces el sistema rechaza el acta
    Y muestra "Falta el kilometraje del traspaso. Es el corte que separa su tramo del de Marvin Cruz."

  Escenario: Traspaso válido sin conectividad, con corte de imputación
    Dado un relevo declarado "Marvin Cruz" con licencia categoría "C" vigente hasta el "2027-01-30"
    Cuando "José Martínez" registra el traspaso a "Marvin Cruz" a las "14:30", en "Nacaome", con odómetro "93520",
      el código de autorización fuera de línea y firma de ambos
    Entonces el sistema cierra el tramo de "José Martínez" en "93520" km
    Y abre el tramo de "Marvin Cruz" desde "93520" km
    Y todo evento posterior se imputa al tramo de "Marvin Cruz"
    Y el acta queda en estado de sincronización "PENDIENTE_DE_ENVIO"

  Escenario: El motorista saliente no puede firmar por incapacidad
    Dado que "José Martínez" sufrió un evento de salud y no puede firmar
    Cuando se registra el traspaso a "Marvin Cruz" con constancia del impedimento y firma de dos personas presentes
    Entonces el sistema cierra la custodia con el impedimento declarado
    Y no libera a "José Martínez" como motorista disponible
    Y registra el hecho de salud, no el diagnóstico

  Escenario: La revalidación que esta historia produce es precondición del acta
    Dado un relevo declarado "Marvin Cruz" con licencia categoría "C" vigente hasta el "2027-01-30"
    Cuando el dispositivo va a levantar el acta de traspaso de HU-045
    Entonces exige el resultado positivo de la revalidación de "Marvin Cruz" como precondición
    Y el acta registra el número de licencia, la categoría, el vencimiento y el fin de rango evaluado
    Y no se levanta ninguna acta de traspaso sin ese resultado registrado
```

## Fuera de alcance

- **El acta de traspaso, el corte de odómetro, la custodia y el traspaso del fondo de combustible** — son de [HU-045](HU-045-relevo-de-motorista-en-ruta.md), que manda en el acto. Delimitación de `HB34-01`: esta historia produce la revalidación que aquel acto exige, y **no la duplica**
- La declaración del relevo en la programación y su verificación de licencia previa — es de M-07, [HU-025](HU-025-habilitacion-de-quien-efectivamente-conduce.md)
- La generación y validación del **código de autorización fuera de línea** — es [HU-055](HU-055-ampliar-alcance-autorizado-en-ruta.md); aquí solo se exige
- La sustitución de **vehículo** con la misión `EN_RUTA`: no existe transición que la respalde — hallazgo abierto
- La reevaluación de aptitud para conducir tras un evento de salud — depende del insumo #50

## Notas y pendientes

- `[C]` **Código de autorización fuera de línea obligatorio en todo relevo sin conectividad.** Postura adoptada por `HB34-01`, alineada con `HU-045`. **Es reversible**: si el PO decide eximir el relevo por incapacidad súbita —cuando no hay nadie a quien llamar—, la excepción se acota a ese motivo tipificado y se registra como decisión de producto
- `[C]` Límite de jornada de conducción, sin el cual no se puede alertar el relevo por jornada cumplida — insumo #48
- `[C]` ¿Puede conducir un vehículo oficial un servidor que no es motorista de planilla? — insumo #48 en el índice de casos especiales
- `[C]` ¿Cubre la póliza de seguro a un conductor no registrado como motorista de la institución? — insumo #49
- `[C]` ¿Qué se hace cuando no hay ningún motorista disponible para relevar en carretera? — insumo #51
