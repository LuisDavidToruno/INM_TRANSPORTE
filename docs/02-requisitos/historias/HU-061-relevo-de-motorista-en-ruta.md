# HU-061 — Relevar al motorista en ruta con acta de traspaso y corte de odómetro

| Campo | Valor |
|---|---|
| **Módulo** | M-08 Ejecución y Bitácora · M-05 Motoristas y Habilitación |
| **Actor** | ACT-06 Motorista saliente y motorista entrante |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador |

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
    Y muestra "La licencia de Marvin Cruz vence el 15/05/2026, antes del retorno previsto el 16/05/2026."

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
    Cuando "José Martínez" registra el traspaso a "Marvin Cruz" a las "14:30", en "Nacaome", con odómetro "93520" y firma de ambos
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

  Escenario: El fondo de combustible no se traspasa sin acta propia
    Cuando "José Martínez" traspasa el vehículo a "Marvin Cruz" sin levantar acta de traspaso del fondo
    Entonces el fondo permanece a nombre de "José Martínez"
    Y un consumo posterior imputado a un folio de "José Martínez" genera alerta automática
    Y muestra "El fondo sigue a nombre de José Martínez. Para traspasarlo levante acta con conteo de folios uno por uno."
```

## Fuera de alcance

- La declaración del relevo en la programación y su verificación de licencia previa — es de M-07
- La sustitución de **vehículo** con la misión `EN_RUTA`: no existe transición que la respalde — hallazgo abierto
- La reevaluación de aptitud para conducir tras un evento de salud — depende del insumo #50

## Notas y pendientes

- `[C]` Límite de jornada de conducción, sin el cual no se puede alertar el relevo por jornada cumplida — insumo #48
- `[C]` ¿Puede conducir un vehículo oficial un servidor que no es motorista de planilla? — insumo #48 en el índice de casos especiales
- `[C]` ¿Cubre la póliza de seguro a un conductor no registrado como motorista de la institución? — insumo #49
- `[C]` ¿Qué se hace cuando no hay ningún motorista disponible para relevar en carretera? — insumo #51
