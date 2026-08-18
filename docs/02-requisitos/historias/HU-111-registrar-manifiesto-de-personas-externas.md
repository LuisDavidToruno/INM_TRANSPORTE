# HU-111 — Registrar el manifiesto de personas externas al solicitar, con el catálogo mínimo de datos

| Campo | Valor |
|---|---|
| **Módulo** | M-17 Traslado de Personas Externas · M-06 Solicitudes de Transporte |
| **Actor** | ACT-02 Solicitante |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — falta el catálogo mínimo de datos que la institución necesita capturar (insumo nuevo a registrar) |

## Historia

**Como** Solicitante de una dependencia
**quiero** declarar que la movilización incluye personas ajenas a la institución y registrar su manifiesto con los datos mínimos autorizados
**para** que la Orden de Misión ampare a quienes efectivamente van a bordo, sin que la institución acumule datos personales que no necesita para controlar el traslado

## Contexto

La premisa rectora 1 dice que SIGTI gestiona **movilizaciones de recursos institucionales**, y que lo trasladado puede ser personal de la institución, **personas externas**, carga o una combinación. [DP-001 D-01](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) confirmó que el traslado de personas externas es **parte del núcleo**, no un módulo opcional.

Hoy, cuando en un vehículo del Estado viaja alguien que no es empleado, la requisición en papel lo resuelve con una línea de observaciones: *"se traslada a personal de la alcaldía"*. No hay contra qué comparar lo que ocurrió, y ante un accidente nadie puede decir cuántas personas iban ni quiénes eran.

El riesgo del extremo contrario es igual de real: una casilla libre invita a escribir la enfermedad, la nacionalidad o la condición de la persona. **Un dato que no se captura no se puede filtrar, no se puede publicar por error y no se puede pedir por hábeas data** ([RN-51](../../01-negocio/reglas/RN-51-minimizacion-de-datos-de-personas-externas.md)). Esta historia es la que decide qué se captura.

## Reglas que la gobiernan

- [RN-51](../../01-negocio/reglas/RN-51-minimizacion-de-datos-de-personas-externas.md) — **Regla rectora**: solo los campos del catálogo autorizado; separación estructural entre datos de gestión pública y datos personales
- [RN-67](../../01-negocio/reglas/RN-67-matriz-de-compatibilidad-objeto-objeto.md) — Personas externas son un **objeto de traslado** propio, evaluado par a par contra los demás
- [RN-21](../../01-negocio/reglas/RN-21-capacidad-de-pasajeros-y-carga.md) — El conteo de personas incluye siempre al motorista y no excede las plazas homologadas
- [RN-53](../../01-negocio/reglas/RN-53-cierre-del-manifiesto-al-despacho.md) — Antes del despacho el manifiesto es editable; después no
- [RN-39](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md) — El catálogo de campos del manifiesto es parámetro configurable con vigencia, nunca una lista en el código

## Casos especiales que la afectan

- [CE-18](../casos-especiales/CE-18-carga-y-pasajeros-en-la-misma-mision.md) — Personas externas junto con personal de la institución y con carga en la misma misión

## Criterios de aceptación

> Todos los nombres y números de identidad de estos escenarios son **ficticios de prueba**.

```gherkin
# language: es
Característica: Manifiesto de personas externas en la solicitud de transporte

  Antecedentes:
    Dado un catálogo "campo_manifiesto_persona_externa" vigente al "2026-09-01" con los campos
      | campo                              | obligatorio |
      | identificación de la persona       | sí          |
      | institución o condición que motiva  | sí          |
      | origen                             | sí          |
      | destino                            | sí          |
    Y una solicitud de transporte "SOL-2026-0912" en estado "BORRADOR"
    Y que el objeto de traslado declarado incluye "personas externas"

  Escenario: Se rechaza encaminar la solicitud sin el manifiesto de las personas externas
    Dado que la solicitud declara "3" personas externas y ningún registro en el manifiesto
    Cuando el Solicitante intenta encaminar la solicitud "SOL-2026-0912" a la jefatura inmediata
    Entonces el sistema rechaza el encaminamiento
    Y muestra "Declaró 3 personas externas y el manifiesto tiene 0 registros. Registre las 3 antes de encaminar la solicitud."
    Y la solicitud permanece en estado "BORRADOR"

  Escenario: Se rechaza el manifiesto con menos personas que las declaradas
    Dado que la solicitud declara "3" personas externas
    Y que el manifiesto tiene registradas a "Ana de Prueba Uno" y "Beto de Prueba Dos"
    Cuando el Solicitante intenta encaminar la solicitud "SOL-2026-0912" a la jefatura inmediata
    Entonces el sistema rechaza el encaminamiento
    Y muestra "Declaró 3 personas externas y el manifiesto tiene 2 registros. Corrija la cantidad declarada o registre la persona faltante."

  Escenario: El sistema no ofrece ningún campo de salud, etnia, situación migratoria ni vulnerabilidad
    Cuando el Solicitante abre la ficha de una persona externa del manifiesto
    Entonces el sistema muestra únicamente los campos del catálogo vigente al "2026-09-01"
    Y no muestra ningún campo de diagnóstico, enfermedad, etnia, nacionalidad de origen migratorio ni condición de vulnerabilidad
    Y el campo de requerimiento operativo solo admite valores del catálogo "requiere camilla", "requiere acompañante", "requiere asiento accesible"

  Escenario: Se rechaza una necesidad de salud escrita como texto libre
    Cuando el Solicitante registra en el campo de requerimiento operativo el texto "paciente con insuficiencia renal en tratamiento"
    Entonces el sistema rechaza el valor
    Y muestra "El requerimiento operativo se elige del catálogo. No registre diagnósticos ni datos clínicos: SIGTI no es expediente de salud."

  Escenario: Se registra el manifiesto con los datos mínimos
    Cuando el Solicitante registra en el manifiesto de "SOL-2026-0912"
      | identificación   | número          | institución o condición   | origen      | destino |
      | Ana de Prueba Uno| 0000-0000-00001 | Alcaldía Municipal (ej.)  | Tegucigalpa | Danlí   |
      | Beto de Prueba Dos| 0000-0000-00002| Alcaldía Municipal (ej.)  | Tegucigalpa | Danlí   |
      | Carla de Prueba Tres| 0000-0000-00003| Alcaldía Municipal (ej.)| Tegucigalpa | Danlí   |
    Entonces el sistema acepta el manifiesto con "3" personas externas
    Y almacena los datos personales en el segmento separado del expediente de la misión
    Y la vista de gestión pública de "SOL-2026-0912" muestra "3 personas externas" sin ninguna identidad

  Escenario: El conteo de personas externas se suma al de personal y al motorista
    Dado que la solicitud declara además "3" servidores de la institución
    Y un vehículo candidato con "5" plazas homologadas incluido el motorista
    Cuando el Jefe de Transporte evalúa la capacidad de "SOL-2026-0912"
    Entonces el sistema calcula "7" ocupantes: 3 personas externas, 3 servidores y 1 motorista
    Y rechaza la asignación de ese vehículo
    Y muestra "El vehículo tiene 5 plazas homologadas y la misión requiere 7, incluido el motorista."
```

## Fuera de alcance

- El cierre del manifiesto y su impresión al despachar — es [HU-114](HU-114-cerrar-el-manifiesto-al-despachar.md)
- La persona que no porta documento de identidad — es [HU-113](HU-113-persona-sin-documento-de-identidad.md)
- La activación de un campo sensible con base legal — es [HU-112](HU-112-fundamentar-campo-sensible-del-manifiesto.md)
- La evaluación de la combinación con carga — es [HU-125](HU-125-personas-externas-junto-con-carga-y-personal.md)
- Los datos del **personal de la institución**: viven en Talento Humano y se referencian por espejo ([RN-48](../../01-negocio/reglas/RN-48-datos-espejo-de-solo-lectura.md)), no se duplican en el manifiesto

## Notas y pendientes

- `[C]` **El catálogo mínimo de datos que la institución necesita capturar de una persona externa.** [RN-51](../../01-negocio/reglas/RN-51-minimizacion-de-datos-de-personas-externas.md) fija el techo —identificación, institución o condición, origen, destino— pero no hay insumo abierto que lo confirme campo por campo con la Gerencia Administrativa y Auditoría Interna. **Insumo nuevo a registrar.** Sin esa declaración el catálogo queda en el mínimo de la regla
- `[C]` **¿La institución traslada personas bajo custodia o menores?** — insumo #39. Si la respuesta es sí, la minimización se refuerza y el catálogo cambia. **No se diseñan campos específicos hasta confirmarlo**
- `[C]` Qué traslados de personas externas realiza la institución y con qué fundamento — [RN-51](../../01-negocio/reglas/RN-51-minimizacion-de-datos-de-personas-externas.md) lo exige como condición de aplicación
- `[I]` El sistema no captura consentimiento ni finalidad del tratamiento: descartado por [DP-001 D-14](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md)
