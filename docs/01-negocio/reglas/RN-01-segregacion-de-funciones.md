# RN-01 — Un mismo servidor no puede ejercer dos funciones de control sobre la misma Orden de Misión

| Campo | Valor |
|---|---|
| **Módulos** | M-01, M-06, M-07, M-09, M-13 |
| **Origen** | Norma [NRM-01](../normativa/NRM-01-control-interno-tsc.md) — MARCI / TSC-NOGECI V-07 |
| **Verificación** | `[V]` la exigencia de segregación de funciones — `[C]` la numeración NOGECI exacta |
| **Tipo** | Bloqueo duro |
| **Configurable** | **No.** Es mandato de control interno del Estado |

## Enunciado

Sobre una misma Orden de Misión, **ninguna persona puede ejercer más de una** de las siguientes funciones de control:

| Función | Actor típico |
|---|---|
| **Solicitar** | ACT-02 Solicitante |
| **Autorizar** | ACT-03 Jefatura Inmediata / ACT-08 Gerencia Administrativa |
| **Despachar** | ACT-05 Encargado de Despacho |
| **Entregar combustible** | ACT-07 Encargado de Combustible |
| **Liquidar** | ACT-04 Jefe de Transporte / ACT-08 Gerencia Administrativa |

La verificación se hace **por persona**, no por rol: que un usuario tenga dos roles asignados no lo habilita a ejercer dos funciones sobre el mismo expediente.

## Justificación

[NRM-01](../normativa/NRM-01-control-interno-tsc.md) establece que el sistema **debe** implementar segregación de funciones por rol **como bloqueo duro y no como advertencia**. Es la defensa estructural contra el fraude de flota: quien puede solicitar, autorizar y liquidar su propio viaje puede fabricar un consumo de combustible completo sin que ningún registro lo contradiga.

Una advertencia que se puede saltar no es un control: ante el TSC, el registro de la advertencia ignorada es prueba de que la institución sabía y aun así permitió la operación.

## Condiciones de aplicación

Aplica a **toda** Orden de Misión, en cualquier dependencia o delegación, sin importar el monto, la distancia ni la urgencia.

**No aplica** entre funciones de control distintas ejercidas sobre **órdenes de misión distintas**: el mismo servidor puede autorizar la misión A y solicitar la misión B.

**No aplica** a las funciones de consulta, registro de bitácora en ruta (ACT-06 Motorista) ni auditoría (ACT-12 Auditor Interno), que no son funciones de control en el sentido de esta regla.

**La emergencia no es excepción.** Si no hay personal para cubrir las funciones, se aplica [RN-02](RN-02-escalamiento-de-autorizacion.md) — escalamiento — no la dispensa.

## Comportamiento esperado

1. Antes de registrar cualquier acto de control, el sistema compara la identidad del usuario actuante contra las identidades ya registradas en las demás funciones de la misma Orden de Misión.
2. Si coincide, **bloquea** con: *"Usted registró la solicitud de esta Orden de Misión N.º <folio>. Por segregación de funciones (RN-01) no puede autorizarla. Corresponde a <nivel superior> conforme a RN-02."*
3. El intento bloqueado **se registra** en la pista de auditoría con usuario, función pretendida, orden, fecha y hora. Un patrón de intentos repetidos es en sí un hallazgo.
4. El sistema expone un reporte de **matriz de segregación por expediente**: qué persona ejerció qué función en cada orden, exportable para auditoría.
5. Al cerrar la orden, se verifica de nuevo la matriz completa. Una violación detectada en el cierre — posible si una persona fue reasignada de puesto — fuerza `CERRADA_CON_HALLAZGO`.

## Casos límite

- **Delegación única con un solo servidor.** En una delegación pequeña puede no existir personal suficiente. La regla **no se relaja**: la función faltante se ejerce por el nivel correspondiente de la dependencia matriz, en línea o por el canal degradado. `[C]` confirmar con la institución cuál es el nivel de reemplazo por delegación. El sistema debe exigir que cada delegación tenga configurado su **suplente de autorización** antes de operar.
- **La misma persona ocupa dos puestos por encargaduría.** Ocurre con frecuencia tras rotación de personal ([NRM-09](../normativa/NRM-09-realidad-operativa.md)). La verificación es por persona, así que bloquea. La salida es [RN-07](RN-07-delegacion-de-autorizacion.md) — delegación acotada a otra persona —, nunca la autoautorización.
- **El motorista es también el solicitante.** Permitido: conducir no es función de control. Pero si además firma la recepción del combustible y la liquidación, se bloquea la liquidación.
- **Reasignación de puesto a mitad del expediente.** La matriz se evalúa contra la identidad de quien **actuó**, congelada en el momento del acto, no contra el puesto que la persona ocupa hoy.
- **Usuario administrador del sistema (ACT-01).** No puede ejercer funciones de control sobre expedientes operativos. Su capacidad es de configuración, no de operación; si necesita operar, se le asigna el rol operativo y queda sujeto a esta regla como cualquiera.
- **Consolidación de dos solicitudes en una misma Orden de Misión.** La matriz se evalúa contra el conjunto de solicitantes de todas las solicitudes consolidadas: si el autorizador es solicitante de **cualquiera** de ellas, se bloquea.
- **Anulación y reemisión.** El asiento reverso de [RN-04](RN-04-anulacion-como-asiento-reverso.md) hereda la matriz de la orden original: no se puede usar la anulación para "limpiar" un conflicto de segregación.

## Trazabilidad

- Norma: [NRM-01 — Control interno y auditoría](../normativa/NRM-01-control-interno-tsc.md)
- Reglas relacionadas: [RN-02](RN-02-escalamiento-de-autorizacion.md), [RN-03](RN-03-registro-inmutable-de-autorizacion.md), [RN-07](RN-07-delegacion-de-autorizacion.md), [RN-32](RN-32-entrega-de-combustible-contra-orden-de-mision.md)
- Actores: ACT-02, ACT-03, ACT-04, ACT-05, ACT-07, ACT-08
- Historias y casos especiales: pendientes — Bloque 2
