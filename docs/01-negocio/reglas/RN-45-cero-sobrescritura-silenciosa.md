# RN-45 — Ningún conflicto de sincronización se resuelve por sobrescritura: todo va a cola de resolución humana

| Campo | Valor |
|---|---|
| **Módulos** | M-16, M-14 |
| **Origen** | Norma [NRM-09](../normativa/NRM-09-realidad-operativa.md); [ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md) |
| **Verificación** | `[V]` la exigencia de reglas deterministas sin pérdida de datos y cola de conflictos |
| **Tipo** | Bloqueo duro |
| **Configurable** | No |

## Enunciado

Cuando dos versiones de un mismo registro entren en conflicto — por sincronización de un cliente de campo, por captura simultánea en dos dispositivos, o por llegada tardía de un registro sobre otro ya cerrado — el sistema **no debe** aplicar "gana el último", "gana el servidor" ni ninguna otra sobrescritura automática.

Ambas versiones **deben** conservarse, y el conflicto **debe** entrar a una **cola de resolución humana** con: registro afectado, versiones en conflicto, origen y fecha de cada una, y usuario que las capturó.

La resolución **debe** ser un acto identificado y registrado, con motivo, y **debe** conservar la versión descartada.

## Justificación

[NRM-09](../normativa/NRM-09-realidad-operativa.md) lo exige: *"resolver conflictos de sincronización con reglas deterministas y sin pérdida de datos: identificadores generados en el cliente, marca de tiempo del dispositivo y del servidor, y cola de conflictos para resolución humana en lugar de sobrescritura silenciosa."*

[ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md) identifica la divergencia silenciosa como *"la peor forma de fallar"*.

En este dominio los datos en conflicto son odómetros, galones y montos. Una sobrescritura automática destruye el término de una conciliación de auditoría, y nadie se entera hasta que el TSC pregunta.

## Condiciones de aplicación

Aplica a todo registro propio de SIGTI que pueda capturarse en más de un lugar.

**No aplica** a los datos espejo de ARGOS y Talento Humano, que son de solo lectura y cuyo dueño es el sistema origen ([RN-48](RN-48-datos-espejo-de-solo-lectura.md)): ahí el origen sí prevalece, y la divergencia se corrige por reconciliación ([RN-49](RN-49-reconciliacion-periodica-del-espejo.md)).

**No aplica** cuando no hay conflicto real: dos capturas idénticas del mismo registro con el mismo identificador se consideran reenvío y se aplican una sola vez.

## Comportamiento esperado

1. El cliente reintenta la sincronización de forma segura ante reintentos: reenviar el mismo registro no crea duplicados ([RN-44](RN-44-identificadores-y-folios-en-el-cliente.md)).
2. Detectado un conflicto, el registro queda marcado **en conflicto** y visible como tal en todas las pantallas que lo muestran. No se oculta hasta que alguien lo resuelva.
3. La cola de conflictos indica **impacto**: si el conflicto afecta un odómetro, un monto o una autorización, se prioriza y se notifica al responsable.
4. Un registro en conflicto **bloquea la liquidación** de la misión afectada hasta resolverse ([RN-08](RN-08-cadena-de-trazabilidad-para-cierre.md)).
5. La resolución conserva la versión descartada como asiento vinculado ([RN-04](RN-04-anulacion-como-asiento-reverso.md)), con motivo y autor.
6. Existe reporte de conflictos por período, dispositivo y delegación: un dispositivo que genera conflictos con frecuencia es un problema a corregir, no un hecho a tolerar.

## Casos límite

- **Registro de campo que llega después del cierre en oficina.** No se descarta ni se aplica: entra a la cola con su fecha del hecho ([RN-05](RN-05-registro-cerrado-no-se-edita.md)). Es el caso más frecuente y el que más tienta a implementar un descarte automático.
- **Cola de conflictos que nadie atiende.** Se acumula y bloquea liquidaciones — que es el efecto deseado. Debe tener responsable por puesto, antigüedad visible y escalamiento por plazo configurable. Una cola sin dueño se convierte en un basurero.
- **Conflicto en campos distintos del mismo registro** — uno cambió el odómetro y otro la hora de arribo. Técnicamente combinables. **No se combinan automáticamente**: se presentan campo por campo y decide una persona. Una fusión automática puede producir un registro que nadie capturó.
- **Cientos de conflictos tras semanas sin sincronizar.** Debe poder resolverse por lotes con criterio explícito declarado por el operador ("aceptar versión de campo para todos los registros de esta misión"), quedando ese criterio registrado. Resolver de a uno miles de conflictos es inviable; hacerlo sin declarar el criterio es sobrescritura con más pasos.
- **Conflicto sobre un registro ya usado en una liquidación cerrada.** No se modifica lo cerrado: se resuelve por asiento de diferencia ([RN-42](RN-42-correccion-retroactiva-con-asiento-de-diferencia.md)) o se cierra con hallazgo.
- **Sincronización parcial** — llegó el consumo pero no su fotografía. No es conflicto: es adjunto pendiente. El sistema debe distinguir *pendiente* de *ausente* ([RN-08](RN-08-cadena-de-trazabilidad-para-cierre.md)).
- **Dos motoristas que registran el mismo paso por caseta** en una misión con relevo. Ambos registros son válidos y describen el mismo hecho: se detecta como posible duplicado por punto y ventana temporal, y lo resuelve una persona.

## Trazabilidad

- Norma: [NRM-09 — Realidad operativa](../normativa/NRM-09-realidad-operativa.md)
- Decisión: [ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md)
- Reglas relacionadas: [RN-43](RN-43-captura-de-campo-sin-conectividad.md), [RN-44](RN-44-identificadores-y-folios-en-el-cliente.md), [RN-46](RN-46-fecha-del-hecho-y-fecha-de-captura.md), [RN-05](RN-05-registro-cerrado-no-se-edita.md), [RN-49](RN-49-reconciliacion-periodica-del-espejo.md)
- Actores: ACT-01, ACT-04, ACT-10, ACT-12
- Historias y casos especiales: pendientes — Bloque 2
