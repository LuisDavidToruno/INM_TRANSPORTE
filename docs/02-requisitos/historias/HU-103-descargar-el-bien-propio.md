# HU-103 — Instruir el descargo del bien propio, con quien lo propone impedido de aprobarlo

| Campo | Valor |
|---|---|
| **Módulo** | M-03 Flota Vehicular |
| **Actor** | ACT-14 Encargado de Bienes Institucionales (propone) · ACT-08 Gerencia Administrativa (aprueba) |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador |

## Historia

**Como** Encargado de Bienes Institucionales
**quiero** instruir el expediente de descargo de un vehículo propio con su causa, avalúo, actas y fotografías, sabiendo que yo no puedo aprobarlo
**para** que la baja patrimonial de un bien del Estado quede documentada y con segregación, sin que el expediente histórico del vehículo se pierda con él

## Contexto

El descargo es una **baja patrimonial**, no una eliminación de registro. Quien la propone no puede aprobarla: es bloqueo duro por `I-17`.

Y el expediente histórico **se conserva íntegro**: las misiones cerradas del vehículo siguen siendo consultables y auditables después de la baja. El correlativo institucional **queda ocupado permanentemente**.

Un detalle que evita un asiento falso: el descargo aplica al **bien propio**. Un vehículo devuelto al comodante o al arrendador no se descarga — se retira de flota, y eso es [HU-104](HU-104-retirar-de-flota-un-bien-ajeno.md).

## Reglas que la gobiernan

- [RN-01](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md) — `I-17`: propone el descargo ≠ aprueba el descargo. **Bloqueo duro**
- [RN-75](../../01-negocio/reglas/RN-75-bien-retenido-o-sustraido-no-sale-del-registro.md) — El bien retenido o sustraído permanece en el registro hasta su recuperación o su descargo formal
- [RN-15](../../01-negocio/reglas/RN-15-identidad-del-vehiculo-y-placa.md) — El correlativo institucional queda ocupado permanentemente
- [RN-05](../../01-negocio/reglas/RN-05-registro-cerrado-no-se-edita.md) — El expediente histórico se conserva íntegro
- [RN-22](../../01-negocio/reglas/RN-22-custodia-del-vehiculo.md) — No quedan custodias vivas sobre un bien descargado
- [RN-03](../../01-negocio/reglas/RN-03-registro-inmutable-de-autorizacion.md) — El intento bloqueado se registra con el par detectado

## Casos especiales que la afectan

- [CE-04](../casos-especiales/CE-04-robo-de-vehiculo-o-de-carga-en-mision.md) — Pérdida no recuperada como causa de descargo
- [CE-03](../casos-especiales/CE-03-accidente-de-transito-en-mision.md) — Siniestro total como causa de descargo
- [CE-16](../casos-especiales/CE-16-vehiculo-a-taller-con-misiones-programadas.md) — Descargo desde taller por irreparable

## Criterios de aceptación

```gherkin
# language: es
Característica: Descargo del vehículo propio

  Antecedentes:
    Dado un vehículo "TR-0012" de régimen "propiedad", en estado "NO_DISPONIBLE"
    Y un expediente de incidente por siniestro vinculado

  Escenario: Se rechaza el descargo con misiones abiertas
    Dado 2 misiones de "TR-0012" en estados no terminales
    Cuando el Encargado de Bienes intenta instruir el descargo
    Entonces el sistema rechaza la acción
    Y lista las 2 misiones no terminales con su estado
    Y muestra "TR-0012 tiene 2 misiones sin estado terminal: OM-2026-0455 (LIQUIDADA) y OM-2026-0462 (RETORNADA). Primero se cierran las misiones, después se descarga el bien."
    Y el sistema no ofrece ninguna pantalla de excepción

  Escenario: Se rechaza el descargo con custodia viva
    Cuando el Encargado de Bienes intenta instruir el descargo con "Karla Ordóñez" como custodia vigente
    Entonces el sistema rechaza la acción
    Y muestra "TR-0012 tiene custodia vigente a nombre de Karla Ordóñez. Resuelva la custodia antes del descargo."

  Escenario: Se rechaza el expediente de descargo sin causa tipificada
    Cuando el Encargado de Bienes instruye el descargo sin causa
    Entonces el sistema rechaza el registro
    Y muestra "Indique la causa del descargo: siniestro total, desuso, obsolescencia o pérdida no recuperada."

  Escenario: Se rechaza el expediente sin actas ni fotografías
    Cuando el Encargado de Bienes instruye el descargo con causa "siniestro total" sin actas ni fotografías
    Entonces el sistema rechaza el registro
    Y muestra "El expediente de descargo exige actas y fotografías, y el avalúo si corresponde."

  Escenario: Quien propone el descargo no puede aprobarlo
    Dado un expediente de descargo instruido por "Elmer Rodríguez"
    Cuando "Elmer Rodríguez" intenta aprobar ese descargo
    Entonces el sistema rechaza la aprobación y no guarda nada
    Y muestra "Elmer Rodríguez instruyó el expediente de descargo de TR-0012 el 20/09/2026. Quien propone la baja de un bien no la aprueba."
    Y indica a qué puesto corresponde la aprobación
    Y el intento queda en la pista de auditoría con el par "I-17"

  Escenario: Se aprueba el descargo y el vehículo queda en estado terminal
    Cuando la Gerencia Administrativa aprueba el descargo de "TR-0012" con acta
    Entonces el vehículo pasa a estado "DADO_DE_BAJA"
    Y ese estado es terminal

  Escenario: El expediente histórico se conserva íntegro
    Dado que "TR-0012" está en "DADO_DE_BAJA"
    Cuando el Auditor Interno consulta las misiones cerradas de "TR-0012"
    Entonces todas siguen siendo consultables y exportables
    Y sus bitácoras, consumos, incidentes y costos permanecen íntegros

  Escenario: El correlativo queda ocupado permanentemente
    Cuando el Encargado de Bienes intenta reutilizar el correlativo "TR-0012" para un vehículo nuevo
    Entonces el sistema rechaza el alta
    Y muestra "El correlativo TR-0012 quedó ocupado permanentemente por un vehículo dado de baja el 25/09/2026."

  Escenario: Un vehículo robado no se borra ni se oculta antes del descargo formal
    Dado un vehículo "TR-0098" robado en misión y no recuperado
    Cuando el Encargado de Bienes registra el robo
    Entonces el vehículo permanece en el registro en estado "NO_DISPONIBLE" con causa tipificada
    Y con expediente de incidente, denuncia y estado del proceso de deducción de responsabilidad
    Y no se marca como "DADO_DE_BAJA" hasta que se apruebe el descargo formal

  Escenario: Un vehículo recuperado reingresa con su correlativo original
    Dado un vehículo "TR-0098" recuperado tras robo
    Cuando el Encargado de Bienes registra el reingreso
    Entonces el vehículo conserva el correlativo "TR-0098"
    Y ingresa a "NO_DISPONIBLE" con causa tipificada
    Y el período de indisponibilidad queda registrado en su expediente
```

## Fuera de alcance

- El fin de tenencia de un bien ajeno — es [HU-104](HU-104-retirar-de-flota-un-bien-ajeno.md)
- La disposición física del bien descargado (subasta, chatarra, donación): fuera de SIGTI
- El proceso de deducción de responsabilidad: se registra su estado, no se instruye desde SIGTI
- La conciliación con el inventario nacional de bienes: SIGTI aporta la referencia

## Notas y pendientes

- ⚠️ `[C]` **Quién aprueba el descargo de un vehículo.** Los artefactos internos divergen: la autoridad en transiciones dice Gerencia Administrativa con acta; el mapa de procesos admite *"Gerencia Administrativa o Máxima Autoridad"*. [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md) lo deja `[P]`: el Manual de Propiedad Estatal regula el descargo pero **no se pudo extraer el articulado**. Esta historia sigue a la autoridad en transiciones y usa Gerencia Administrativa; **la divergencia queda anotada, no resuelta aquí**
- `[C]` **Qué formato de acta de descargo usa la institución** — insumo **#2**
- `[C]` **¿Existe unidad de Bienes separada en la institución?** Si no existe, desaparece la separación proponer/aprobar y se activa el control compensatorio: el expediente se marca como *acumulación vigilada* y se notifica al Auditor Interno — pendiente registrado en la ficha del actor
- `[P]` El descargo proviene de [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md), con articulado no extraído. **No se eleva el nivel**
