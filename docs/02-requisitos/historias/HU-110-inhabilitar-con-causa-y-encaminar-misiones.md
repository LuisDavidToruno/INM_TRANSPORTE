# HU-110 — Inhabilitar al motorista con causa tipificada y encaminar las misiones programadas afectadas

| Campo | Valor |
|---|---|
| **Módulo** | M-05 Motoristas y Habilitación |
| **Actor** | ACT-04 Jefe de Transporte |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador |

## Historia

**Como** Jefe de Transporte
**quiero** inhabilitar a un motorista con causa tipificada y vigencia, y que el sistema me liste de inmediato las misiones programadas afectadas para reasignarlas
**para** que el bloqueo no aparezca por sorpresa en el despacho de la mañana, y para que el expediente del servidor se conserve íntegro pese a la inhabilitación

## Contexto

Un motorista deja de estar habilitado por causas muy distintas: vencimiento no renovado, suspensión derivada de un expediente de incidentes, restricción médica incompatible, decisión administrativa o cese del servidor. Cada una tiene consecuencias distintas y una vigencia distinta —definitiva o hasta una fecha—, y por eso la causa es dato tipificado obligatorio.

Y hay una distinción que el despacho necesita: **habilitado no es lo mismo que disponible.** La habilitación es dato propio de SIGTI; la disponibilidad —permisos, vacaciones, incapacidades— viene del espejo de Talento Humano. Un motorista habilitado puede estar de vacaciones, y un motorista presente puede estar no habilitado.

**La inhabilitación no borra el expediente**: el historial de conducción, incidentes y habilitaciones pasadas se conserva íntegro.

## Reglas que la gobiernan

- [RN-14](../../01-negocio/reglas/RN-14-sustitucion-de-motorista.md) — La sustitución revalida todas las habilitaciones y conserva la asignación original
- [RN-12](../../01-negocio/reglas/RN-12-disponibilidad-del-motorista.md) — Disponibilidad desde el espejo de Talento Humano, distinta de la habilitación
- [RN-55](../../01-negocio/reglas/RN-55-habilitacion-vencida-durante-la-mision.md) — El vencimiento sobrevenido en ruta no detiene la misión
- [RN-05](../../01-negocio/reglas/RN-05-registro-cerrado-no-se-edita.md) — La inhabilitación cierra el rango; el historial se conserva
- [RN-48](../../01-negocio/reglas/RN-48-datos-espejo-de-solo-lectura.md) — El cese del servidor lo notifica el espejo, no se captura a mano
- [RN-50](../../01-negocio/reglas/RN-50-degradacion-por-sincronizacion-detenida.md) — Con el espejo desactualizado, la disponibilidad se marca *no confirmada*
- [RN-11](../../01-negocio/reglas/RN-11-restricciones-medicas-del-motorista.md) — La restricción médica incompatible es causa de inhabilitación

## Casos especiales que la afectan

- [CE-13](../casos-especiales/CE-13-motorista-no-disponible-por-talento-humano.md) — Eje de la historia
- [CE-11](../casos-especiales/CE-11-licencia-vence-durante-la-mision.md) — Inhabilitación por vencimiento con misión en curso
- [CE-10](../casos-especiales/CE-10-motorista-incapacitado-en-ruta.md) — Incapacidad sobrevenida
- [CE-05](../casos-especiales/CE-05-cambio-de-motorista-con-la-mision-en-curso.md) — Sustitución de motorista

## Criterios de aceptación

```gherkin
# language: es
Característica: Inhabilitación del motorista y sus efectos

  Antecedentes:
    Dado un motorista "Denis Fúnez" en estado "HABILITADO"
    Y 3 misiones programadas a su nombre en los próximos 10 días

  Escenario: Se rechaza la inhabilitación sin causa tipificada
    Cuando el Jefe de Transporte inhabilita a "Denis Fúnez" sin causa
    Entonces el sistema rechaza la inhabilitación
    Y muestra "Indique la causa: vencimiento no renovado, suspensión derivada de un expediente de incidentes, restricción médica incompatible, decisión administrativa o cese del servidor."

  Escenario: Se rechaza la inhabilitación sin vigencia declarada
    Cuando el Jefe de Transporte inhabilita a "Denis Fúnez" con causa "decisión administrativa" sin declarar si es definitiva o hasta fecha
    Entonces el sistema rechaza la inhabilitación
    Y muestra "Declare la vigencia de la inhabilitación: definitiva o hasta una fecha."

  Escenario: Se inhabilita y se listan las misiones programadas afectadas
    Cuando el Jefe de Transporte inhabilita a "Denis Fúnez" con causa "suspensión derivada de expediente de incidentes" hasta el "2026-12-31"
    Entonces "Denis Fúnez" queda en estado "NO HABILITADO"
    Y el sistema lista las 3 misiones programadas afectadas con su folio y su fecha
    Y las encamina a sustitución de motorista

  Escenario: La inhabilitación no cancela misiones automáticamente
    Cuando el Jefe de Transporte inhabilita a "Denis Fúnez"
    Entonces ninguna de las 3 misiones cambia de estado por sí sola
    Y el sistema muestra "3 misiones requieren sustitución de motorista. El sistema no cancela ninguna."

  Escenario: La sustitución conserva la asignación original en el historial
    Cuando el Jefe de Transporte reasigna "OM-2026-0560" a "Marlon Zelaya"
    Entonces el sistema revalida la licencia, la disponibilidad y las restricciones de "Marlon Zelaya"
    Y conserva en el historial la asignación original de "Denis Fúnez" con su motivo de sustitución

  Escenario: La inhabilitación no borra el expediente
    Dado que "Denis Fúnez" está en "NO HABILITADO"
    Cuando el Auditor Interno consulta su historial de conducción, incidentes y habilitaciones pasadas
    Entonces todo el historial sigue siendo consultable
    Y ninguna misión pasada suya se modifica

  Escenario: El cese en Talento Humano cierra la habilitación con causa cese
    Cuando el espejo de Talento Humano notifica la baja de "Denis Fúnez"
    Entonces la habilitación se cierra con causa "cese"
    Y "Denis Fúnez" deja de aparecer entre los motoristas asignables
    Y sus registros históricos no se tocan

  Escenario: Habilitado y disponible se distinguen en el despacho
    Dado un motorista "José Martínez" habilitado y con vacaciones registradas del "2026-10-01" al "2026-10-15"
    Cuando el Jefe de Transporte intenta programar una misión el "2026-10-08"
    Entonces el sistema rechaza la asignación
    Y muestra "José Martínez está habilitado, pero figura con vacaciones del 01/10/2026 al 15/10/2026 según Talento Humano."

  Escenario: Con el espejo desactualizado la disponibilidad se marca no confirmada
    Dado un espejo de Talento Humano con "23" días sin sincronizar
    Cuando el Jefe de Transporte programa una misión con "José Martínez"
    Entonces el sistema advierte "Disponibilidad no confirmada: datos de Talento Humano sincronizados hace 23 días."
    Y esa marca se imprime en el documento de la misión

  Escenario: Rehabilitar al vencer la vigencia de la inhabilitación
    Dado una inhabilitación de "Denis Fúnez" vigente hasta el "2026-12-31"
    Cuando el sistema evalúa su estado el "2027-01-02" y su licencia sigue vigente
    Entonces el sistema ofrece rehabilitar a "Denis Fúnez"
    Y no lo rehabilita automáticamente sin acto del Jefe de Transporte
```

## Fuera de alcance

- La instrucción del expediente de incidentes que motiva la suspensión: pertenece a M-12
- El registro de permisos, vacaciones e incapacidades: son de Talento Humano ([DP-001 D-07](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md))
- La reprogramación de las misiones afectadas: pertenece a M-07
- El relevo en ruta con acta y corte de odómetro: pertenece a M-08

## Notas y pendientes

- `[C]` **¿Qué ocurre con un empleado dado de baja en Talento Humano que tiene misiones abiertas en SIGTI?** Pendiente expreso de [ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md)
- `[C]` `umbral_advertencia_desincronizacion` del espejo de Talento Humano — insumo **#17**
- `[C]` Catálogo `tipo_ausencia` y su efecto sobre la asignación — insumo **#1**
- `[C]` Reevaluación de aptitud tras un evento de salud en ruta: ¿inhabilitación automática hasta dictamen? — insumo **#50**
- `[C]` ¿Los hallazgos reiterados de un mismo motorista bloquean o advierten? La recomendación es que adviertan: bloquear por un hallazgo no resuelto es sancionar antes de investigar — insumo **#1**
