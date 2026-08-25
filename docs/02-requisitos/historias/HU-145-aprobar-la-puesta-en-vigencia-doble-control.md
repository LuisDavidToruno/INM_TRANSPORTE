# HU-145 — Aprobar la puesta en vigencia de un parámetro, que es lo único que lo hace aplicable

| Campo | Valor |
|---|---|
| **Módulo** | M-02 Catálogos Maestros |
| **Actor** | ACT-08 Gerencia Administrativa |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — el circuito está definido por `RN-39`; falta confirmar con la institución quién ejerce `ACT-08` y si acepta el plazo máximo de aprobación |

## Historia

**Como** Gerencia Administrativa
**quiero** ver los parámetros cargados con su valor anterior, su valor nuevo, su fuente, su respaldo adjunto y el impacto que producirán, y aprobar o rechazar su puesta en vigencia
**para** que ninguna tarifa, ningún umbral y ningún plazo entre a regir sin que un segundo par lo haya visto — porque cambiarlos es cambiar dinero

## Contexto

Es el hallazgo `HB34-67`: **el doble control sobre parámetros no tenía ni historia ni pantalla**, siendo `I-13` núcleo irreductible.

Lo que está en juego, dicho sin adornos: **un `ACT-01` que pudiera subir por sí solo el umbral de desviación de consumo haría desaparecer los hallazgos de auditoría sin tocar un solo dato operativo.** Las misiones serían las mismas, los galones los mismos, los kilómetros los mismos — y los hallazgos, cero. Es la forma más limpia de desmontar un control interno que existe.

El circuito ([RN-39](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md), y [actores-y-roles §4.3](../../01-negocio/actores-y-roles.md) como autoridad):

- `ACT-01` **carga** el parámetro, su vigencia y su respaldo documental.
- `ACT-08` **aprueba** su puesta en vigencia. **Sin la aprobación, el parámetro existe y no se aplica.** Ningún cálculo lo resuelve, en ninguna fecha.
- `ACT-12` ve el histórico completo como objeto de auditoría de primera clase.

Y una consecuencia que evita la parálisis: si `ACT-08` no está disponible, **rige la tarifa anterior**, que sí está aprobada. Nadie queda sin tabla. Lo que **no** se admite es aplicar la nueva *"provisionalmente"* y regularizar después.

## Reglas que la gobiernan

- [RN-39](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md) — **Regla rectora**: ningún parámetro entra en vigencia con la sola acción de una persona
- [RN-03](../../01-negocio/reglas/RN-03-registro-inmutable-de-autorizacion.md) — La aprobación se registra con identidad, momento y huella del contenido aprobado
- [RN-40](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md) — El parámetro pendiente de aprobación no participa de ninguna resolución
- [RN-01](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md) — La aprobación es un acto de control con las incompatibilidades que le corresponden
- [RN-42](../../01-negocio/reglas/RN-42-correccion-retroactiva-con-asiento-de-diferencia.md) — Aprobar una vigencia retroactiva dispara los asientos de diferencia

## Casos especiales que la afectan

- [CE-24](../casos-especiales/CE-24-cobro-en-categoria-de-peaje-equivocada.md) — Una tarifa aprobada tarde deja discrepancias que no lo eran
- [CE-28](../casos-especiales/CE-28-hallazgo-posterior-sobre-mision-cerrada.md) — El cambio de un umbral no borra los hallazgos ya emitidos

## Criterios de aceptación

```gherkin
# language: es
Característica: Aprobación de la puesta en vigencia de un parámetro

  Antecedentes:
    Dado un parámetro "umbral_desviacion_consumo" con valor "15" por ciento vigente y aprobado
    Y una carga pendiente de "25" por ciento desde el "2026-10-01", hecha por "Carlos Fúnez" el "2026-09-18"
    Y la Gerencia Administrativa ejercida por "Rolando Discua"

  Escenario: El parámetro cargado y no aprobado no se aplica en ningún cálculo
    Cuando el Encargado de Combustible concilia una misión con desviación del "20" por ciento el "2026-10-05"
    Entonces el sistema aplica el umbral de "15" por ciento
    Y genera el hallazgo de consumo excedido
    Y muestra "Umbral aplicado: 15 %, vigente y aprobado. Existe una carga de 25 % pendiente de aprobación que no se aplica."

  Escenario: Se rechaza la aprobación sin haber visto el respaldo documental
    Dada una carga sin respaldo documental adjunto
    Cuando "Rolando Discua" intenta aprobarla
    Entonces el sistema rechaza la aprobación
    Y muestra "La carga no tiene respaldo documental adjunto. Devuelva al Administrador del Sistema para que lo agregue."

  Escenario: La pantalla de aprobación muestra el valor anterior, el nuevo y el impacto
    Cuando "Rolando Discua" abre la carga pendiente
    Entonces ve el valor anterior "15" por ciento y el nuevo "25" por ciento
    Y ve la fuente, la fecha de verificación, el respaldo adjunto y quién cargó
    Y ve el impacto estimado: "Con 25 % dejarían de generarse 34 de los 41 hallazgos de consumo del último trimestre."
    Y ve la fecha de inicio de vigencia declarada

  Escenario: El rechazo de la aprobación exige motivo y devuelve la carga
    Cuando "Rolando Discua" rechaza la carga con motivo "el respaldo no acredita la fuente del 25 %"
    Entonces el parámetro queda en estado "RECHAZADO" con el motivo
    Y sigue rigiendo el "15" por ciento
    Y el rechazo se notifica a quien cargó y queda en la pista de auditoría

  Escenario: La aprobación pone el parámetro en vigencia desde su fecha declarada
    Cuando "Rolando Discua" aprueba la carga el "2026-09-20"
    Entonces el parámetro de "25" por ciento rige desde el "2026-10-01"
    Y el sistema registra carga el "2026-09-18" por "Carlos Fúnez" y aprobación el "2026-09-20" por "Rolando Discua", como dos actos separados
    Y la aprobación queda en la pista append-only y no puede ser alterada ni borrada, tampoco por el Administrador del Sistema

  Escenario: Los hechos anteriores a la vigencia siguen usando el umbral anterior
    Dado el parámetro de "25" por ciento vigente desde el "2026-10-01"
    Cuando el Encargado de Combustible concilia una misión ocurrida el "2026-09-28" con desviación del "20" por ciento
    Entonces el sistema aplica el umbral de "15" por ciento
    Y genera el hallazgo
    Y muestra "Umbral aplicado: 15 %, vigente a la fecha del hecho 28/09/2026."

  Escenario: La ausencia de la Gerencia Administrativa no detiene la operación
    Dada una carga pendiente de aprobación desde hace 6 días
    Cuando cualquier cálculo necesita el parámetro
    Entonces el sistema usa el valor anterior, aprobado y vigente
    Y no aplica la carga pendiente ni siquiera de forma provisional
    Y muestra en el tablero de la Gerencia Administrativa "1 parámetro pendiente de aprobación desde hace 6 días."

  Escenario: El histórico de cambios es consultable por el Auditor Interno
    Cuando el Auditor Interno consulta el parámetro "umbral_desviacion_consumo"
    Entonces ve todas sus vigencias con valor, fuente, respaldo, quién cargó, quién aprobó y desde cuándo rigió
    Y ve qué cálculos usaron cada versión
    Y puede exportarlo como paquete de evidencia
```

## Fuera de alcance

- La carga del parámetro — es [HU-144](HU-144-cargar-parametro-normativo-con-vigencia.md)
- El bloqueo de la autoaprobación — es [HU-146](HU-146-bloquear-que-quien-carga-apruebe-su-propia-carga.md)
- Los asientos de diferencia de una vigencia retroactiva — es [HU-148](HU-148-correccion-retroactiva-con-asiento-de-diferencia.md)
- La aprobación de catálogos operativos, que sigue el mismo circuito — es [HU-141](HU-141-mantener-catalogos-simples-con-vigencia.md)

## Notas y pendientes

- `[I]` El doble control **carga ↔ aprobación** es diseño de control interno recogido por [actores-y-roles §4.3](../../01-negocio/actores-y-roles.md) y por `RN-39`; **no es articulado citable**. La exigencia general de segregación está en [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md) como implicación de requerimiento `[P]`, con su numeración `[C]` — insumo **#23**
- `[C]` **Quién ejerce `ACT-08`** en la institución y si hay más de un puesto con esa facultad — insumo **#27**
- `[C]` **¿Quiere la institución un plazo máximo de aprobación con alerta de vencimiento?** `RN-39` lo deja abierto
- `[C]` El impacto que muestra la pantalla de aprobación exige que el sistema sepa cuántos hallazgos cambiarían. Confirmar con Auditoría Interna si quiere ver ese número **antes** de aprobar o si prefiere no verlo, para no condicionar la decisión
