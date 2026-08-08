# RN-50 — Si la sincronización lleva detenida más del umbral, el sistema degrada explícitamente antes de permitir operaciones sensibles

| Campo | Valor |
|---|---|
| **Módulos** | M-20, M-16, M-07 |
| **Origen** | [ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md) — mitigación obligatoria 5 |
| **Verificación** | Sin nivel normativo: es **decisión de arquitectura** ([ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md), mitigación 5) y se cita por su identificador (`HN1-20`). `[C]` los umbrales, con el PO y con Talento Humano |
| **Tipo** | Advertencia con acuse registrado. **El bloqueo por desincronización no está decidido** — ver la nota de hallazgo abierta |
| **Configurable** | Sí — `umbral_advertencia_desincronizacion` por conjunto de datos y por delegación |

## Nota de corrección — hallazgos `HB1-10` y `HB1-13`

> **Qué estaba mal — dos cosas.**
>
> 1. **Bloqueo donde la autoridad dice advertencia.** Esta regla convertía en **bloqueo duro** lo que la máquina de estados resuelve como advertencia: `T-05` — *"el sistema **advierte antes de permitir** y registra la advertencia en el diario"* — y `T-08` — *"advertencia registrada y visible en el documento impreso"*. El propio [ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md), mitigación 5, usa la misma palabra: *"el sistema **advierte** antes de permitir operaciones sensibles"*. Por la precedencia de `CLAUDE.md`, manda la máquina de estados en materia de bloqueos duros: se corrige esta regla. El escalón de bloqueo y su "autorización degradada" se retiran del enunciado.
> 2. **Licencia.** El enunciado apoyaba el bloqueo de *asignar motorista* en la desactualización del espejo, cuando el dato decisivo de esa asignación —la licencia— **ya no es espejo**: es propio de SIGTI ([ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md), [RN-48](RN-48-datos-espejo-de-solo-lectura.md)). Lo que sí depende del espejo en esa asignación es la **disponibilidad** del motorista — vacaciones, permiso, incapacidad, alta y baja ([RN-12](RN-12-disponibilidad-del-motorista.md)) —, y así queda dicho.
>
> **Nota de hallazgo abierta — decisión del PO.** Si la institución quiere un **escalón de bloqueo** por desincronización, hay que decidir tres cosas que hoy no constan en ninguna matriz: a qué umbral, **quién** puede autorizar la operación degradada, y bajo qué par `I-nn` se controla esa facultad. Mientras no se decida, el sistema **advierte y registra**, que es lo que la autoridad prescribe. No se inventa aquí una facultad que `actores-y-roles.md` no otorga a nadie.

## Enunciado

El sistema **debe** medir, por conjunto de datos espejeado y por dispositivo de campo, el tiempo transcurrido desde la última sincronización confirmada.

Superado el **umbral de advertencia**, toda operación sensible **debe** mostrar la antigüedad del dato **antes** de continuar, exigir **acuse** y registrar la advertencia en el diario del expediente. La operación **no se impide**: se marca.

La marca de operación sobre espejo desactualizado **debe** viajar al expediente y al **documento impreso** de la Orden de Misión, tal como lo exige `T-08`.

Operaciones sensibles, como mínimo: **asignar motorista**, **autorizar una orden de misión**, **aprobar un fondo de combustible** y **liquidar**.

Lo que se degrada es la **confianza en el dato espejeado** — disponibilidad del motorista, jerarquía de autorización, estructura presupuestaria —, no los datos propios de SIGTI. La licencia de conducir, por ser dato propio, **no se degrada** por desincronización ([RN-48](RN-48-datos-espejo-de-solo-lectura.md)).

## Justificación

[ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md), mitigación 5: *"Degradación explícita: si la sincronización está detenida más allá de un umbral, el sistema advierte antes de permitir operaciones sensibles — como asignar un motorista."*

Y el riesgo que la motiva, en las mismas palabras del ADR: *"Un motorista que Talento Humano dio de baja pero que SIGTI sigue asignando a misiones no es un problema técnico: es un problema legal."*

La palabra clave es **explícita**. Un sistema que sigue operando normalmente con datos de hace dos semanas está mintiendo por omisión: quien asigna cree estar decidiendo con información actual.

## Condiciones de aplicación

Aplica al espejo de ARGOS y Talento Humano, y al estado de sincronización de cada dispositivo de campo (M-16).

Los umbrales son **por conjunto de datos**: el catálogo de estructura presupuestaria tolera más desactualización que el registro de incapacidades. `[C]` los valores con el PO y con Talento Humano.

**No aplica** a los datos propios de SIGTI. Una delegación sin red durante semanas sigue pudiendo asignar motorista y vehículo con los datos que le pertenecen; lo que arrastra la advertencia es la verificación de disponibilidad contra el espejo.

## Comportamiento esperado

1. El estado de sincronización es visible de forma permanente, no en una pantalla escondida: cuándo fue la última confirmada, por conjunto.
2. La advertencia dice **cuánto tiempo** y **qué implica**: *"Los datos de permisos y vacaciones se sincronizaron por última vez hace 6 días. Un permiso aprobado después no se verá aquí."*
3. El acuse es **nominativo y con motivo**: queda registrado quién continuó, cuándo y con qué fundamento. Un acuse anónimo no defiende a nadie.
4. La operación realizada sobre datos degradados se **marca en el expediente** de forma permanente y **se imprime en la Orden de Misión** (`T-08`). Si después se descubre que el dato estaba desactualizado, el expediente muestra que se sabía y quién asumió el riesgo.
5. La superación sostenida del umbral de advertencia genera **incidente para ACT-01 Administrador del Sistema**, no solo un mensaje al usuario que tropezó con él: el problema es el canal de sincronización, no la operación que lo encontró.
6. El sistema reporta, por delegación y período, cuántas operaciones sensibles se ejecutaron sobre espejo desactualizado. Es un **indicador de infraestructura**, y es el insumo con el que el PO decidirá si hace falta un escalón de bloqueo.

## Casos límite

- **Delegación permanentemente sin red.** Con el bloqueo retirado, la delegación **sigue operando** con advertencia y acuse — que es lo que exige [NRM-09](../normativa/NRM-09-realidad-operativa.md) y la premisa rectora 5. `[C]` mapa de delegaciones y su situación real de conectividad. El umbral de advertencia debe poder configurarse **por delegación**: una advertencia que salta siempre deja de leerse.
- **Advertencia que se vuelve rutina.** Si todos los días alguien acusa y continúa, el control se vació igual. Por eso el reporte del comportamiento 6 es parte de la regla: es un indicador de infraestructura, no de disciplina.
- **Motorista dado de baja en Talento Humano que el espejo no ha traído.** Es el riesgo que el `ADR-001` nombra por su nombre: *"no es un problema técnico: es un problema legal"*. Con advertencia y acuse, la responsabilidad queda nominada en el expediente y en el impreso. Es lo que hoy prescribe la autoridad; si la institución quiere que además bloquee, es la decisión abierta de la nota de corrección.
- **Sincronización que responde pero devuelve datos vacíos.** Técnicamente "sincronizó". La marca de última sincronización confirmada debe exigir **confirmación de contenido**, no solo respuesta del canal ([RN-49](RN-49-reconciliacion-periodica-del-espejo.md)).
- **Un conjunto sincronizado y otro no.** El estado es por conjunto: se puede asignar vehículo (datos propios de SIGTI) pero no motorista (espejo de Talento Humano desactualizado). El sistema debe ser preciso sobre qué se puede y qué no.
- **Dispositivo de campo que lleva semanas sin sincronizar.** No se le impide capturar — eso violaría [RN-43](RN-43-captura-de-campo-sin-conectividad.md). Lo que se degrada son las **validaciones**, que se marcan como hechas con datos locales de fecha X ([RN-14](RN-14-sustitucion-de-motorista.md)).
- **Umbral mal configurado, demasiado corto.** Producirá bloqueos constantes y presión para desactivarlo. Cambiar el umbral es un acto registrado con fundamento; ponerlo en un valor absurdamente alto equivale a apagar el control y debe verse en el reporte de parámetros.

## Trazabilidad

- Decisión: [ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md) — mitigación 5
- Autoridad en el tratamiento de la desincronización: [orden-de-mision.md](../../03-arquitectura/estados/orden-de-mision.md) `T-05` y `T-08`
- Norma: [NRM-09](../normativa/NRM-09-realidad-operativa.md)
- Hallazgos que corrigen esta regla: `HB1-10` y `HB1-13` de [H-B1-001](../../05-calidad/hallazgos/H-B1-001-revision-qa-bloque-1.md); `HN1-20` de [H-B1-002](../../05-calidad/hallazgos/H-B1-002-revision-normativa-bloque-1.md)
- Reglas relacionadas: [RN-48](RN-48-datos-espejo-de-solo-lectura.md), [RN-49](RN-49-reconciliacion-periodica-del-espejo.md), [RN-12](RN-12-disponibilidad-del-motorista.md), [RN-43](RN-43-captura-de-campo-sin-conectividad.md)
- Actores: ACT-01, ACT-04, ACT-05, ACT-10
- Historias y casos especiales: pendientes — Bloque 2
