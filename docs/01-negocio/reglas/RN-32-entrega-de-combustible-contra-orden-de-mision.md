# RN-32 — No se entrega combustible sin Orden de Misión aprobada, y solo al vehículo y motorista de esa orden

| Campo | Valor |
|---|---|
| **Módulos** | M-09, M-07 |
| **Origen** | Norma [NRM-01](../normativa/NRM-01-control-interno-tsc.md) — TSC-NOGECI V-07; `PROP-01` |
| **Verificación** | `[V]` la exigencia de autorización previa de toda transacción |
| **Tipo** | Bloqueo duro |
| **Configurable** | Sí — `estado_minimo_orden_para_asignar_combustible`, con valor inicial `APROBADA` |

## Enunciado

ACT-07 Encargado de Combustible **no debe** poder registrar una asignación de combustible imputada a una misión si:

1. La Orden de Misión no ha alcanzado el estado mínimo configurado (inicialmente `APROBADA`), o
2. El vehículo receptor no es el asignado a esa orden, o
3. El motorista receptor no es el asignado a esa orden ni un encargado de delegación facultado para recibir en su nombre `[C]`

Cuando el esquema de la institución sea **asignación por período** y no por misión, el vínculo obligatorio es motorista + período + fondo, y cada consumo se imputa después a una misión concreta.

## Justificación

TSC-NOGECI V-07 exige que toda transacción esté **autorizada por servidor competente antes de ejecutarse**. El combustible entregado antes de que exista autorización de viaje es un desembolso sin causa: si la misión después no se aprueba, el combustible ya salió y no hay expediente al cual imputarlo.

El control de vehículo y motorista cierra la puerta al desvío más simple de todos: sacar el vale a nombre de una misión real y cargarlo en otro vehículo.

## Condiciones de aplicación

Aplica a toda asignación imputada a misión.

**No aplica** a la carga de combustible en tanques o cisternas institucionales, si existen `[C]`, que tienen circuito propio.

**No aplica** al reabastecimiento de rutina de un vehículo en el predio sin misión asociada, si la institución lo practica. `[C]` confirmar; de existir, requiere imputación a vehículo y período, con la misma trazabilidad de folio y responsable.

## Comportamiento esperado

1. Al registrar la entrega, ACT-07 selecciona la Orden de Misión y el sistema **precarga** vehículo y motorista de la orden. No los captura libremente.
2. Si el receptor presente no es el motorista de la orden, el sistema bloquea e indica quién es el motorista asignado. La sustitución previa ([RN-14](RN-14-sustitucion-de-motorista.md)) es el único camino para cambiarlo.
3. La entrega genera la constancia de recepción de [RN-27](RN-27-asignacion-de-combustible-con-folio.md), y ACT-07 **no puede** ser el receptor ni el liquidador ([RN-01](RN-01-segregacion-de-funciones.md)).
4. Toda entrega queda vinculada al fondo, a la orden, al vehículo, al motorista y al folio del instrumento — los cinco vínculos, sin excepción.
5. El sistema reporta entregas por encargado, período y dependencia, con las órdenes a las que se imputaron.

## Casos límite

- **Emergencia con vehículo que debe salir de inmediato.** No hay entrega sin orden aprobada. La salida operativa es aprobar la orden por la vía rápida ([RN-02](RN-02-escalamiento-de-autorizacion.md) resuelve el autorizador), no entregar primero. Si aun así se entregó, se registra como **entrega sin orden** — que el sistema permite consignar pero marca como hallazgo desde el primer momento, porque negarse a registrarlo solo la haría invisible.
- **Vehículo que se avería y se sustituye después de recibido el combustible.** El vale ya entregado al motorista se conserva; la sustitución de vehículo no invalida la asignación al motorista. Si el vehículo entrante tiene otro tipo de combustible, el vale se anula y se reemite ([RN-27](RN-27-asignacion-de-combustible-con-folio.md)).
- **Tipo de combustible incompatible.** El sistema debe validar que el combustible asignado corresponde al tipo declarado en la ficha del vehículo. Un vale de diésel para un vehículo de gasolina es un error caro y perfectamente evitable.
- **Encargado de delegación que recibe por varios motoristas.** `[C]` confirmar si la institución lo practica. De admitirse, el receptor es el encargado, que a su vez registra las entregas individuales — dos niveles de folio y dos constancias, no una entrega colectiva sin desglose.
- **Orden aprobada que después se anula, con combustible ya entregado.** Ver [RN-27](RN-27-asignacion-de-combustible-con-folio.md): anulación del instrumento con acta y devolución constatada, o registro de extravío. La orden anulada se liquida igual ([RN-29](RN-29-liquidacion-de-combustible.md)).
- **Entrega registrada en delegación sin conectividad** contra una orden que en el servidor fue rechazada. Al sincronizar aparece el conflicto: no se revierte la entrega física ocurrida, se registra como sobregiro o entrega sin orden válida y se eleva ([RN-45](RN-45-cero-sobrescritura-silenciosa.md)).

## Trazabilidad

- Norma: [NRM-01](../normativa/NRM-01-control-interno-tsc.md) — TSC-NOGECI V-07
- Decisión: [DP-001, D-03](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md); `PROP-01`
- Reglas relacionadas: [RN-01](RN-01-segregacion-de-funciones.md), [RN-26](RN-26-fondo-de-combustible-aprobado.md), [RN-27](RN-27-asignacion-de-combustible-con-folio.md), [RN-14](RN-14-sustitucion-de-motorista.md)
- Actores: ACT-04, ACT-05, ACT-06, ACT-07, ACT-10
- Historias y casos especiales: pendientes — Bloque 2
