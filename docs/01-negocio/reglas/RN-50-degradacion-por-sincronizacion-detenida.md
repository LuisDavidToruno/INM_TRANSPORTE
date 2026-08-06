# RN-50 — Si la sincronización lleva detenida más del umbral, el sistema degrada explícitamente antes de permitir operaciones sensibles

| Campo | Valor |
|---|---|
| **Módulos** | M-20, M-16, M-07 |
| **Origen** | [ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md) — mitigación obligatoria 5 |
| **Verificación** | `[V]` la decisión de arquitectura |
| **Tipo** | Advertencia escalonada a bloqueo duro |
| **Configurable** | Sí — `umbral_advertencia_desincronizacion` y `umbral_bloqueo_desincronizacion`, por conjunto de datos |

## Enunciado

El sistema **debe** medir, por conjunto de datos espejeado y por dispositivo de campo, el tiempo transcurrido desde la última sincronización confirmada.

Superado el **umbral de advertencia**, toda operación sensible **debe** mostrar la antigüedad del dato antes de continuar, con acuse registrado.

Superado el **umbral de bloqueo**, la operación sensible **debe** bloquearse hasta que la sincronización se restablezca o hasta que un rol facultado autorice la operación degradada, dejando constancia.

Operaciones sensibles, como mínimo: **asignar motorista**, **autorizar una orden de misión**, **aprobar un fondo de combustible** y **liquidar**.

## Justificación

[ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md), mitigación 5: *"Degradación explícita: si la sincronización está detenida más allá de un umbral, el sistema advierte antes de permitir operaciones sensibles — como asignar un motorista."*

Y el riesgo que la motiva, en las mismas palabras del ADR: *"Un motorista que Talento Humano dio de baja pero que SIGTI sigue asignando a misiones no es un problema técnico: es un problema legal."*

La palabra clave es **explícita**. Un sistema que sigue operando normalmente con datos de hace dos semanas está mintiendo por omisión: quien asigna cree estar decidiendo con información actual.

## Condiciones de aplicación

Aplica al espejo de ARGOS y Talento Humano, y al estado de sincronización de cada dispositivo de campo (M-16).

Los umbrales son **por conjunto de datos**: el catálogo de estructura presupuestaria tolera más desactualización que el registro de incapacidades. `[C]` los valores con el PO y con Talento Humano.

## Comportamiento esperado

1. El estado de sincronización es visible de forma permanente, no en una pantalla escondida: cuándo fue la última confirmada, por conjunto.
2. La advertencia dice **cuánto tiempo** y **qué implica**: *"Los datos de permisos y vacaciones se sincronizaron por última vez hace 6 días. Un permiso aprobado después no se verá aquí."*
3. El bloqueo indica qué operación se detiene y qué hay que restablecer. Ofrece la vía de autorización degradada cuando exista, con motivo obligatorio.
4. La operación realizada en modo degradado se **marca en el expediente** de forma permanente. Si después se descubre que el dato estaba desactualizado, el expediente muestra que se sabía y quién asumió el riesgo.
5. La superación del umbral de bloqueo genera **incidente para ACT-01 Administrador del Sistema**, no solo un mensaje al usuario que tropezó con él.

## Casos límite

- **Delegación permanentemente sin red.** El umbral de bloqueo la dejaría inoperante. Es el conflicto de fondo entre [ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md) y [NRM-09](../normativa/NRM-09-realidad-operativa.md), y hay que resolverlo con datos, no con una regla genérica: `[C]` mapa de delegaciones y su situación real de conectividad (pendiente de [NRM-09](../normativa/NRM-09-realidad-operativa.md)). El umbral debe poder configurarse **por delegación**, y en las de conectividad crónica la degradación se acepta explícitamente con la autorización correspondiente.
- **Autorización degradada que se vuelve rutina.** Si todos los días alguien autoriza operar degradado, el control se vació. El sistema debe **reportar la frecuencia** de autorizaciones degradadas por delegación y período: es un indicador de infraestructura, no de disciplina.
- **Sincronización que responde pero devuelve datos vacíos.** Técnicamente "sincronizó". La marca de última sincronización confirmada debe exigir **confirmación de contenido**, no solo respuesta del canal ([RN-49](RN-49-reconciliacion-periodica-del-espejo.md)).
- **Un conjunto sincronizado y otro no.** El estado es por conjunto: se puede asignar vehículo (datos propios de SIGTI) pero no motorista (espejo de Talento Humano desactualizado). El sistema debe ser preciso sobre qué se puede y qué no.
- **Dispositivo de campo que lleva semanas sin sincronizar.** No se le impide capturar — eso violaría [RN-43](RN-43-captura-de-campo-sin-conectividad.md). Lo que se degrada son las **validaciones**, que se marcan como hechas con datos locales de fecha X ([RN-14](RN-14-sustitucion-de-motorista.md)).
- **Umbral mal configurado, demasiado corto.** Producirá bloqueos constantes y presión para desactivarlo. Cambiar el umbral es un acto registrado con fundamento; ponerlo en un valor absurdamente alto equivale a apagar el control y debe verse en el reporte de parámetros.

## Trazabilidad

- Decisión: [ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md) — mitigación 5
- Norma: [NRM-09](../normativa/NRM-09-realidad-operativa.md)
- Reglas relacionadas: [RN-48](RN-48-datos-espejo-de-solo-lectura.md), [RN-49](RN-49-reconciliacion-periodica-del-espejo.md), [RN-12](RN-12-disponibilidad-del-motorista.md), [RN-43](RN-43-captura-de-campo-sin-conectividad.md)
- Actores: ACT-01, ACT-04, ACT-05, ACT-10
- Historias y casos especiales: pendientes — Bloque 2
