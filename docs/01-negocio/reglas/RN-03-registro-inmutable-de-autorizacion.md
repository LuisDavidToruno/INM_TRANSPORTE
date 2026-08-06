# RN-03 — Toda autorización se registra de forma inmutable con identidad, rol, momento, origen y huella del contenido autorizado

| Campo | Valor |
|---|---|
| **Módulos** | M-01, M-14, M-15 |
| **Origen** | Norma [NRM-01](../normativa/NRM-01-control-interno-tsc.md) y [NRM-08](../normativa/NRM-08-firma-electronica.md); decisión [DP-001 D-04](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) |
| **Verificación** | `[V]` la exigencia de pista de auditoría — `[V]` que no se usa firma electrónica certificada (decisión del PO) |
| **Tipo** | Bloqueo duro |
| **Configurable** | No |

## Enunciado

Cada acto de autorización — aprobación de solicitud, permiso de circulación, aprobación de fondo de combustible, autorización de desviación, cierre de liquidación — **debe** producir un asiento que contenga, como mínimo:

1. Identidad del autorizador y **cargo y rol vigentes en ese momento**
2. Método de autenticación empleado (usuario autenticado o código gestionado por el sistema)
3. Marca de tiempo del servidor y, si el acto se originó en campo, también la del dispositivo
4. Origen del acto: dispositivo, dirección de red o delegación
5. **Huella (hash) del contenido exacto autorizado**
6. Si se actuó por delegación, el acto de delegación que la confiere

Ese asiento **no debe** poder modificarse ni eliminarse por ningún rol, incluido ACT-01 Administrador del Sistema.

## Justificación

[NRM-01](../normativa/NRM-01-control-interno-tsc.md) exige pista de auditoría **append-only** de toda transacción, con valor anterior y valor nuevo. [DP-001 D-04](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) descartó la firma electrónica certificada: si no hay certificado que vincule al firmante con el documento, **el registro es la única prueba** de que la autorización existió y sobre qué contenido recayó.

La huella del contenido resuelve la pregunta que el auditor hace siempre: *¿el jefe autorizó este viaje, o autorizó otro que después alguien editó?*

## Condiciones de aplicación

Aplica a todo acto de autorización, aprobación, visto bueno, rechazo y anulación. **Un rechazo se registra con el mismo rigor que una aprobación** — en este sistema los rechazos son los que tienen consecuencia legal.

No aplica a consultas de solo lectura, que se registran bajo [RN-52](RN-52-registro-de-consultas-a-manifiestos.md) cuando recaen sobre datos de personas.

## Comportamiento esperado

1. El sistema calcula la huella sobre el **contenido presentado al autorizador**, no sobre el registro completo: si después cambia un campo no autorizado, la huella debe seguir verificando.
2. Si al momento del acto no se puede resolver el cargo y rol del autorizador, el sistema **bloquea** la autorización. Una autorización sin cargo registrado es inutilizable ante auditoría.
3. Cualquier modificación posterior de un campo cubierto por la huella **invalida la autorización** y devuelve la orden al estado anterior, exigiendo nueva autorización. El sistema lo notifica al autorizador original.
4. El documento impreso ([RN-25](RN-25-salvoconducto-con-folio-y-qr.md), M-15) reproduce folio, autorizador, cargo, fecha y **la huella en el pie**, verificable por QR.
5. El asiento se conserva por el plazo de retención configurado. `[C]` plazo exacto con Auditoría Interna — [NRM-01](../normativa/NRM-01-control-interno-tsc.md).

## Casos límite

- **Autorización emitida en campo sin conectividad.** Se registra con marca de tiempo del dispositivo y se sella con la del servidor al sincronizar. **Ambas se conservan**; la discrepancia entre ellas es dato de auditoría, no error a corregir. Ver [RN-46](RN-46-fecha-del-hecho-y-fecha-de-captura.md).
- **Reloj del dispositivo desajustado.** No se corrige silenciosamente el valor capturado. Se registra la desviación observada al sincronizar y se marca el asiento como *hora de dispositivo no confiable*.
- **El autorizador cambia de cargo después del acto.** El asiento conserva el cargo del momento. Consultar el expediente hoy debe mostrar quién era entonces, no quién es ahora.
- **Autorización verbal o telefónica en emergencia.** No existe como autorización: existe como **registro de instrucción recibida**, capturado por quien la recibió, y exige ratificación posterior del autorizador dentro de un plazo configurable. Sin ratificación, la orden se cierra con hallazgo. `[C]` confirmar si la institución admite este canal — [NRM-09](../normativa/NRM-09-realidad-operativa.md) lo plantea como posible canal degradado.
- **Autorización masiva** (aprobar veinte solicitudes de una vez). Genera **un asiento por solicitud**, cada uno con su propia huella. Un asiento agregado no permite demostrar qué contenido se aprobó en cada caso.
- **Corrección de un error tipográfico** en un campo cubierto por la huella. No hay atajo: invalida y reautoriza. Si el campo es irrelevante para el control, la solución correcta es sacarlo del alcance de la huella al diseñar el documento, no relajar la regla.

## Trazabilidad

- Normas: [NRM-01](../normativa/NRM-01-control-interno-tsc.md), [NRM-08](../normativa/NRM-08-firma-electronica.md)
- Decisión: [DP-001, D-04](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md)
- Reglas relacionadas: [RN-04](RN-04-anulacion-como-asiento-reverso.md), [RN-05](RN-05-registro-cerrado-no-se-edita.md), [RN-07](RN-07-delegacion-de-autorizacion.md), [RN-46](RN-46-fecha-del-hecho-y-fecha-de-captura.md)
- Actores: ACT-03, ACT-04, ACT-08, ACT-09, ACT-12
- Historias y casos especiales: pendientes — Bloque 2
