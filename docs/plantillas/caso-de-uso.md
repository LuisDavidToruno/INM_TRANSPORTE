# Plantilla — Caso de uso

Archivo: `docs/02-requisitos/casos-de-uso/CU-xx-slug-corto.md`

Un caso de uso describe **una interacción completa** entre un actor y el sistema que produce un resultado con valor. A diferencia de la historia de usuario — que expresa una necesidad — el caso de uso detalla el recorrido paso a paso, incluidos los caminos alternos.

En este proyecto se usan para los flujos donde el **orden de los pasos y las precondiciones importan legalmente**: autorizaciones, despacho, liquidación. Para el resto, la historia de usuario basta.

---

## Esqueleto

```markdown
# CU-xx — <Nombre en infinitivo: "Autorizar solicitud de transporte">

| Campo | Valor |
|---|---|
| **Módulo** | M-xx |
| **Actor principal** | ACT-xx |
| **Actores secundarios** | ACT-xx |
| **Precondiciones** | <lo que debe ser cierto antes de empezar> |
| **Postcondiciones** | <lo que es cierto al terminar con éxito> |
| **Disparador** | <qué inicia el caso de uso> |

## Flujo principal

1. …
2. …

## Flujos alternos

**A1 — <nombre>** (desde el paso N)
1. …

## Flujos de excepción

**E1 — <nombre>** (desde el paso N)
1. …

## Reglas aplicables
## Trazabilidad
```

---

## Ejemplo completo

# CU-08 — Autorizar solicitud de transporte

| Campo | Valor |
|---|---|
| **Módulo** | M-06 Solicitudes de Transporte |
| **Actor principal** | ACT-03 Jefatura Inmediata |
| **Actores secundarios** | ACT-02 Solicitante, ACT-05 Encargado de Despacho |
| **Precondiciones** | Existe una solicitud en estado `SOLICITADA`. El autorizador tiene rol vigente sobre la dependencia del solicitante. |
| **Postcondiciones** | La solicitud queda en `APROBADA` o `RECHAZADA`, con actor, motivo y marca de tiempo registrados de forma inmutable. |
| **Disparador** | El solicitante envía la solicitud, o el autorizador entra a su bandeja de pendientes. |

## Flujo principal

1. El autorizador abre su bandeja de solicitudes pendientes de su dependencia.
2. El sistema muestra las solicitudes ordenadas por fecha de salida más próxima, señalando las que salen en menos de 24 horas.
3. El autorizador abre una solicitud y ve: solicitante, motivo, objeto del traslado (personas / carga / mixto), origen, destino, fechas, número de pasajeros o descripción de la carga, y tipo de vehículo requerido.
4. El sistema muestra las **validaciones automáticas ya evaluadas**: disponibilidad estimada de vehículo del tipo requerido, si el viaje cae en día u hora inhábil, si el solicitante tiene liquidaciones de viáticos vencidas, y el viático estimado.
5. El autorizador autoriza.
6. El sistema verifica la **segregación de funciones**: el autorizador no puede ser el solicitante.
7. El sistema registra la autorización con identidad, cargo, rol, marca de tiempo, método de autenticación y hash del contenido autorizado.
8. La solicitud pasa a `APROBADA` y entra a la cola de programación del Encargado de Despacho.
9. El sistema notifica al solicitante.

## Flujos alternos

**A1 — Autorización con modificaciones** (desde el paso 5)
1. El autorizador ajusta fechas, número de pasajeros o tipo de vehículo, y registra el motivo del ajuste.
2. El sistema recalcula el viático estimado y revalida la disponibilidad.
3. El sistema **notifica el cambio al solicitante** y continúa desde el paso 6.
4. La versión original de la solicitud se conserva en el historial; no se sobrescribe.

**A2 — Rechazo** (desde el paso 5)
1. El autorizador rechaza e indica el motivo, que es obligatorio.
2. La solicitud pasa a `RECHAZADA`. Se notifica al solicitante.
3. El solicitante puede duplicarla como nueva solicitud, pero no reabrir la rechazada.

**A3 — Autorización por delegación** (desde el paso 1)
1. El autorizador titular tiene una delegación de firma vigente a favor de otro servidor.
2. El delegado ve la bandeja del titular, marcada como tal.
3. La autorización se registra indicando que se actuó **por delegación**, con el acto que la confiere y su vigencia.

## Flujos de excepción

**E1 — El autorizador es el mismo solicitante** (en el paso 6)
1. El sistema bloquea la autorización.
2. Muestra "No puede autorizar una solicitud que usted mismo generó. La solicitud se escaló a <cargo superior>."
3. La solicitud se escala automáticamente al siguiente nivel jerárquico y se le notifica.
4. Se registra el intento en la bitácora de auditoría.

**E2 — El viaje requiere permiso de día u hora inhábil** (en el paso 4)
1. El sistema advierte que la salida cae en día u hora inhábil y que requiere permiso de la máxima autoridad.
2. La autorización de la jefatura es válida pero **no habilita el despacho**: la solicitud queda `APROBADA` con la marca `REQUIERE_PERMISO_MAXIMA_AUTORIDAD`.
3. Se dispara el flujo de solicitud de permiso ([CU-11](CU-11-solicitar-permiso-dia-inhabil.md)).

**E3 — El solicitante tiene liquidaciones de viáticos vencidas** (en el paso 4)
1. El sistema muestra la advertencia con el detalle de lo vencido.
2. Si la institución configuró el bloqueo como duro, la autorización se impide hasta que liquide.
3. Si es advertencia, el autorizador puede continuar, y su decisión queda registrada con esa advertencia visible en el expediente.

**E4 — La solicitud es de emergencia y ya se ejecutó** (desde el paso 1)
1. La solicitud llega en estado `SOLICITADA` con marca `EMERGENCIA` y fecha de salida en el pasado.
2. El sistema la presenta como **convalidación posterior**, no como autorización previa.
3. La autorización se registra como convalidación, exigiendo motivo, y el expediente queda marcado para revisión de Auditoría Interna. Ver [CE-01](../casos-especiales/CE-01-emergencia-fuera-de-horario.md).

## Reglas aplicables

- `RN-03` — Segregación de funciones: quien solicita no puede autorizar
- `RN-04` — El nivel de autorización depende del destino, la duración y el monto de viático
- `RN-11` — Los viajes en día u hora inhábil requieren permiso de la máxima autoridad
- `RN-16` — Liquidaciones de viáticos vencidas bloquean o advierten según parámetro institucional
- `RN-28` — Toda aprobación registra identidad, cargo, método de autenticación, marca de tiempo y hash

## Trazabilidad

- Historias: `HU-021`, `HU-022`, `HU-024`
- Casos especiales: `CE-01`, `CE-09` (solicitudes que compiten por el mismo vehículo)
- Normativa: [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md) (TSC-NOGECI V-07, autorización de transacciones), [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md) (circulación en día inhábil)
