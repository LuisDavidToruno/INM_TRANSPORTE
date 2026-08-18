# HU-115 — Levantar el acta de entrega y recepción de las personas externas en cada destino

| Campo | Valor |
|---|---|
| **Módulo** | M-17 Traslado de Personas Externas · M-08 Ejecución y Bitácora · M-15 Formatos Oficiales e Impresión |
| **Actor** | ACT-06 Motorista |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — el régimen de custodia depende del insumo #39 |

## Historia

**Como** Motorista
**quiero** registrar en el destino quién recibe a las personas externas que trasladé, con su nombre, puesto, institución, lugar y hora
**para** que quede constancia de dónde termina mi responsabilidad sobre ellas, y de que llegaron todas las que salieron

## Contexto

Hoy la responsabilidad del motorista sobre las personas que traslada **termina cuando él dice que terminó**, y no hay ningún documento que lo pruebe. Si una de las personas trasladadas no aparece después, o alega que la dejaron en un lugar distinto al convenido, no hay nada escrito.

[RN-53](../../01-negocio/reglas/RN-53-cierre-del-manifiesto-al-despacho.md) numeral 4 ya exige que **las entregas de carga registren quién recibió, con constancia** — y lo llama expresamente *"el equivalente de la cadena de custodia aplicada a lo transportado"*. Esta historia aplica el mismo criterio a las personas, que es donde el vacío duele más.

El acta **no atribuye responsabilidad**: [RN-74](../../01-negocio/reglas/RN-74-sin-atribucion-de-responsabilidad-en-campo.md) es clara en que el registro de campo consigna hechos, no culpas. El motorista anota lo que ocurrió; quién respondió por qué se determina en el expediente.

## Reglas que la gobiernan

- [RN-53](../../01-negocio/reglas/RN-53-cierre-del-manifiesto-al-despacho.md) — Las entregas registran **quién recibió, con constancia**; toda diferencia contra el manifiesto se declara
- [RN-69](../../01-negocio/reglas/RN-69-inventario-de-la-carga-y-acta-de-entrega.md) — El acta de entrega con constancia del receptor, aplicada por analogía a las personas trasladadas
- [RN-74](../../01-negocio/reglas/RN-74-sin-atribucion-de-responsabilidad-en-campo.md) — El registro de campo **no** captura atribución de responsabilidad
- [RN-25](../../01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md) — El acta impresa lleva folio único y QR verificable
- [RN-43](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md) — El acta se levanta **sin ninguna conectividad**, que es la condición normal en el destino
- [RN-46](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md) — `ocurrido_en` de la entrega y `capturado_en` son campos distintos
- [RN-78](../../01-negocio/reglas/RN-78-grado-de-cumplimiento-del-objeto.md) — El destino no se declara cumplido sin su desenlace

## Casos especiales que la afectan

- [CE-18](../casos-especiales/CE-18-carga-y-pasajeros-en-la-misma-mision.md) — Personas externas junto con carga: dos actas distintas en el mismo destino
- [CE-07](../casos-especiales/CE-07-retorno-anticipado-la-mision-se-aborta.md) — La misión se aborta y las personas quedan en un punto que no es su destino

## Criterios de aceptación

> Todos los nombres de estos escenarios son **ficticios de prueba**.

```gherkin
# language: es
Característica: Acta de entrega y recepción de personas externas

  Antecedentes:
    Dado una Orden de Misión "OM-2026-0451" en estado "EN_RUTA"
    Y un manifiesto cerrado con "3" personas externas con destino "Danlí"
    Y un motorista "José Martínez" con el dispositivo sin conectividad

  Escenario: Se rechaza declarar el destino cumplido sin acta de entrega
    Cuando el Motorista intenta declarar cumplido el destino "Danlí"
    Entonces el sistema rechaza la declaración
    Y muestra "En este destino se entregan 3 personas externas. Levante el acta de recepción antes de declarar el destino cumplido."

  Escenario: Se rechaza el acta sin identificar a quien recibe
    Cuando el Motorista registra el acta de entrega en "Danlí" sin nombre, puesto ni institución del receptor
    Entonces el sistema rechaza el acta
    Y muestra "Registre quién recibe: nombre, puesto e institución. Un acta sin receptor identificado no acredita nada."

  Escenario: Se rechaza que el propio motorista figure como receptor
    Cuando el Motorista registra el acta de entrega en "Danlí" declarando como receptor a "José Martínez"
    Entonces el sistema rechaza el acta
    Y muestra "Quien entrega no puede ser quien recibe. Registre al servidor o representante de la institución receptora."

  Escenario: Se rechaza un acta que atribuye responsabilidad
    Cuando el Motorista registra en las observaciones del acta el texto "la persona se retrasó por culpa del encargado de la delegación"
    Entonces el sistema rechaza la observación
    Y muestra "Registre el hecho, no la responsabilidad. Ejemplo: 'la persona abordó a las 07:20, 40 minutos después de la hora prevista'."

  Escenario: Se levanta el acta con menos personas de las que salieron
    Dado que solo "2" de las "3" personas externas llegan al destino "Danlí"
    Cuando el Motorista registra el acta de entrega con "2" personas recibidas
    Entonces el sistema acepta el acta
    Y exige vincular una novedad que explique la diferencia de "1" persona
    Y muestra "Salieron 3 y entrega 2. Registre la novedad de la persona faltante: no abordó, descendió en punto intermedio, u otra causa."

  Escenario: Se levanta el acta completa sin conectividad
    Cuando el Motorista registra el acta de entrega en "Danlí" el "2026-09-18" a las "11:35" con receptor "Delegación de ejemplo, encargada de recepción", "3" personas recibidas y fotografía del acta firmada
    Entonces el sistema guarda el acta con identificador generado en el dispositivo
    Y le asigna folio del rango asignado a la delegación
    Y registra "ocurrido_en" en "2026-09-18 11:35" y "capturado_en" no editable
    Y la deja en estado de sincronización "PENDIENTE_DE_ENVIO"
    Y no solicita en ningún momento conexión de datos

  Escenario: El acta impresa lleva folio y espacio de firma
    Cuando el Motorista emite el acta de entrega de "Danlí" para firma en papel
    Entonces el documento lleva folio único, QR verificable, hash del acta electrónica y espacio de firma y sello del receptor
    Y muestra el conteo de personas recibidas y sus nombres, sin ningún campo sensible
```

## Fuera de alcance

- El acta de entrega de **carga**, con inventario y consignatario — la gobierna [RN-69](../../01-negocio/reglas/RN-69-inventario-de-la-carga-y-acta-de-entrega.md) y le corresponde a M-08
- El traspaso de custodia del **vehículo** entre motoristas — es [RN-71](../../01-negocio/reglas/RN-71-traspaso-en-ruta-con-acta-y-corte-de-odometro.md)
- Las novedades del manifiesto en ruta — es [HU-116](HU-116-registrar-novedades-del-manifiesto-en-ruta.md)
- El envío del acta al servidor y la resolución de conflictos — es [HU-066](HU-066-sincronizar-sola-y-reanudable.md)

## Notas y pendientes

- `[C]` **¿Traslada la institución personas bajo custodia o menores?** — insumo #39. **Es lo que mantiene esta historia en borrador.** Si la respuesta es sí, la cadena de custodia deja de ser un acta de recepción entre instituciones y pasa a ser un régimen con requisitos propios: autoridad que ordena, autoridad que recibe, identificación del acompañante habilitado y probablemente constancia con formato oficial. **No se infiere ninguno de esos requisitos hasta que la institución lo declare**
- `[C]` ¿Existe hoy un formato en papel de acta de recepción de personas? — insumo #2. Si existe, ese formato es el diseño de esta pantalla, campo por campo
- `[C]` Qué hace el sistema si el receptor **se niega a firmar**. Propuesta a validar: se registra la negativa como hecho, con testigo si lo hay, y el acta queda válida marcada "SIN FIRMA DEL RECEPTOR"
- **Regla candidata `RN-C17a`** — *Toda entrega de personas externas en un destino consta en acta con lugar, fecha del hecho, identificación de quien entrega y de quien recibe, y conteo recibido contra manifiesto; el destino no se declara cumplido sin ella.* [RN-53](../../01-negocio/reglas/RN-53-cierre-del-manifiesto-al-despacho.md) y [RN-69](../../01-negocio/reglas/RN-69-inventario-de-la-carga-y-acta-de-entrega.md) lo exigen **para la carga**; **ninguna regla vigente lo dice de las personas**. No darla por escrita
