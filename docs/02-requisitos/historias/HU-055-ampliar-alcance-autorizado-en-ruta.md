# HU-055 — Ampliar el alcance autorizado en ruta con código de autorización fuera de línea

| Campo | Valor |
|---|---|
| **Módulo** | M-07 Programación y Despacho · M-08 Ejecución y Bitácora · M-16 Sincronización y Operación Desconectada |
| **Actor** | ACT-06 Motorista solicita y registra · ACT-04 Jefe de Transporte genera el código |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — falta la magnitud de la extensión que escala el nivel autorizante (insumo #49) y quién convalida un acto ejecutado sin autorización previa y en qué plazo (insumo #32). Sin lo segundo, la ampliación en ruta no tiene desenlace administrativo |

## Historia

**Como** Motorista
**quiero** poder ampliar los días, los destinos o los kilómetros autorizados de mi misión con un código que me dicta el Jefe de Transporte por radio o por teléfono
**para** seguir operando dentro del amparo de la Orden de Misión cuando la necesidad cambia en carretera y no tengo datos para que el sistema consulte nada

## Contexto

La misión que se extiende es la excepción más frecuente de la operación real: un destino más, dos días más, una vía cerrada que obliga a rodear ([CE-06](../casos-especiales/CE-06-la-mision-se-extiende-mas-dias-destinos-o-kilometros.md)). Hoy se resuelve por teléfono y no queda en ningún papel; el kilometraje de más aparece en la liquidación sin explicación.

**Cada extensión produce una versión del alcance autorizado.** No se sobrescribe la ventana original: el expediente muestra la original y la ampliada, y toda validación posterior usa la vigente a la fecha del hecho ([RN-77](../../01-negocio/reglas/RN-77-versionado-del-alcance-autorizado.md)).

**Prorrogar no puede ser la puerta trasera que evita un bloqueo duro.** Si la licencia del motorista vence dentro de la ventana ampliada, la prórroga se bloquea: la salida es el relevo o el retorno anticipado.

Y cuando no hay radio, ni teléfono, ni un punto con señal, **no se puede exigir una autorización que físicamente no se puede pedir** — pero tampoco fingir que existió: el hecho se registra con justificación obligatoria y se convalida en la liquidación ([RN-73](../../01-negocio/reglas/RN-73-convalidacion-de-actos-sin-autorizacion-previa.md)).

## Reglas que la gobiernan

- [RN-77](../../01-negocio/reglas/RN-77-versionado-del-alcance-autorizado.md) — **Regla rectora**: cada extensión produce una versión del alcance; la validación usa la vigente a la fecha del hecho
- [RN-73](../../01-negocio/reglas/RN-73-convalidacion-de-actos-sin-autorizacion-previa.md) — El acto sin autorización previa se convalida en plazo y la cronología se declara tal como ocurrió
- [RN-55](../../01-negocio/reglas/RN-55-habilitacion-vencida-durante-la-mision.md) — La habilitación que vence en ruta no detiene la ejecución, pero cierra con hallazgo
- [RN-40](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md) · [RN-41](../../01-negocio/reglas/RN-41-congelamiento-del-valor-al-autorizar.md) — El estimado del tramo nuevo se recalcula con el paquete congelado
- [RN-01](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md) — Quien conduce no puede autorizarse a sí mismo la extensión

## Casos especiales que la afectan

- [CE-06](../casos-especiales/CE-06-la-mision-se-extiende-mas-dias-destinos-o-kilometros.md) — La misión se extiende más días, destinos o kilómetros
- [CE-11](../casos-especiales/CE-11-licencia-vence-durante-la-mision.md) — La licencia vence dentro de la ventana ampliada
- [CE-01](../casos-especiales/CE-01-salida-de-emergencia-convalidada.md) — El acto ejecutado sin autorización previa y convalidado después

## Criterios de aceptación

```gherkin
# language: es
Característica: Extensión del alcance autorizado con código fuera de línea

  Antecedentes:
    Dada la Orden de Misión "OM-2026-0451" en estado "EN_RUTA" con ventana autorizada del "2026-05-12" al "2026-05-16"
    Y un motorista "José Martínez" con licencia categoría "C" vigente hasta el "2026-05-17"
    Y un paquete normativo congelado el "2026-05-12" en el dispositivo
    Y que el dispositivo lleva 3 días sin conectividad

  Escenario: Se rechaza la prórroga que deja al motorista sin licencia vigente
    Cuando "José Martínez" solicita ampliar la ventana hasta el "2026-05-20" con un código válido
    Entonces el sistema rechaza la prórroga
    Y muestra "La licencia de José Martínez vence el 17/05/2026, antes del nuevo retorno previsto el 20/05/2026. La prórroga no procede: gestione un relevo o el retorno anticipado."
    Y registra el intento en la bitácora del dispositivo

  Escenario: Se rechaza un código de autorización ya utilizado
    Dado un código de autorización fuera de línea ya consumido para "OM-2026-0451"
    Cuando "José Martínez" ingresa nuevamente ese código
    Entonces el sistema rechaza la autorización
    Y muestra "Ese código ya se usó. Solicite uno nuevo al Jefe de Transporte."

  Escenario: Se rechaza un código vencido
    Dado un código de autorización fuera de línea emitido a las "10:00" con ventana de validez de "30" minutos
    Cuando "José Martínez" lo ingresa a las "10:45"
    Entonces el sistema rechaza la autorización
    Y muestra "Ese código venció. Solicite uno nuevo al Jefe de Transporte."

  Escenario: Prórroga autorizada por radio, verificada sin conectividad
    Cuando "José Martínez" ingresa el código de autorización dictado por el Jefe de Transporte para ampliar la ventana hasta el "2026-05-17"
    Entonces el dispositivo verifica el código sin ninguna conectividad
    Y crea la versión 2 del alcance autorizado, con ventana del "2026-05-12" al "2026-05-17"
    Y conserva visible la ventana original en el expediente
    Y revalida la licencia del motorista contra la nueva fecha de fin con el paquete congelado

  Escenario: Destino adicional autorizado recalcula el estimado de peajes con el paquete congelado
    Cuando "José Martínez" agrega el destino "Delegación de Nacaome" con código de autorización válido
    Entonces el sistema crea una nueva versión del alcance autorizado
    Y recalcula el estimado de peajes del tramo agregado con la tabla del paquete congelado al "2026-05-12"
    Y no consulta la tabla de tarifas actual del servidor

  Escenario: No hay forma de obtener el código y la extensión ocurre igual
    Dado que no hay señal, ni radio, ni teléfono para alcanzar al Jefe de Transporte
    Cuando "José Martínez" registra el arribo al destino no previsto "Delegación de Nacaome" con la justificación "instrucción verbal del Encargado de Delegación, sin forma de obtener código"
    Entonces el sistema registra el arribo sin impedirlo
    Y lo marca como "extensión sin autorización previa, pendiente de convalidación"
    Y muestra "Registrado. Deberá convalidarse al liquidar; si no se convalida, la misión cierra con hallazgo."

  Escenario: La extensión hace circular en día inhábil no cubierto por el salvoconducto
    Dado un salvoconducto vigente que ampara circulación hasta el "2026-05-16"
    Cuando "José Martínez" registra circulación el "2026-05-17", que es día inhábil, sin código de autorización
    Entonces el sistema registra la circulación con justificación obligatoria
    Y la marca como "fuera del amparo del permiso vigente"
    Y genera el hallazgo "H-05" al liquidar si no se justifica
```

## Fuera de alcance

- La generación del código por parte del Jefe de Transporte desde la sede — es de M-07
- El escalamiento del nivel autorizante según la magnitud de la extensión — depende del insumo #49
- La emisión de un salvoconducto nuevo por día inhábil sobrevenido — requiere firma de la máxima autoridad y no se resuelve en carretera

## Notas y pendientes

- `[C]` Magnitud de la extensión que escala el nivel autorizante — insumo #49
- `[C]` Quién convalida un acto sin autorización previa y en qué plazo — insumo #32
- `[I]` El código de un solo uso con ventana corta es mecanismo de control interno, no firma electrónica certificada: en Honduras no la hay disponible para este uso
