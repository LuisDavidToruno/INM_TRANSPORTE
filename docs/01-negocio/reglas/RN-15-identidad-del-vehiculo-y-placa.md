# RN-15 — La identidad del vehículo es el correlativo institucional; la placa no es obligatoria ni única

| Campo | Valor |
|---|---|
| **Módulos** | M-03, M-04 |
| **Origen** | Norma [NRM-06](../normativa/NRM-06-transito-y-licencias.md) — desabastecimiento de placas; [NRM-02](../normativa/NRM-02-bienes-del-estado.md) — numeración consecutiva institucional |
| **Verificación** | `[V]` el desabastecimiento de placas metálicas — `[V]` la exigencia de numeración consecutiva institucional |
| **Tipo** | Bloqueo duro (sobre el correlativo) + derivación |
| **Configurable** | No |

## Enunciado

Todo vehículo de la flota **debe** tener un **número correlativo institucional** obligatorio y único dentro de la institución. Ese correlativo es el identificador operativo con el que el vehículo aparece en órdenes de misión, bitácoras, vales y reportes.

El campo **placa no debe ser obligatorio ni único**. El sistema **debe** admitir el estado **"sin placa metálica"** como estado válido y operable, con el documento sustitutivo o constancia del Instituto de la Propiedad como adjunto.

Un vehículo sin placa **no debe** quedar bloqueado para asignación, despacho ni liquidación por ese solo hecho.

## Justificación

[NRM-06](../normativa/NRM-06-transito-y-licencias.md) es categórica: *"Un campo `placa` obligatorio y único rompería el sistema en la realidad hondureña actual."* Hay desabastecimiento prolongado de placas metálicas y reportes de miles de vehículos circulando sin placa durante años; en marzo de 2026 el Congreso aprobó la compra directa a través del IP.

[NRM-02](../normativa/NRM-02-bienes-del-estado.md) exige, en cambio, **numeración consecutiva institucional** como parte de la identificación obligatoria del vehículo del Estado. Ese dato sí está bajo control de la institución, y por eso es el que sirve de identidad.

La no unicidad de la placa no es capricho: una placa puede reasignarse, transcribirse mal, o repetirse temporalmente entre un registro histórico y uno nuevo.

## Condiciones de aplicación

Aplica a todo vehículo del expediente de flota, sea propio, en comodato, alquilado o donado.

`[C]` [NRM-02](../normativa/NRM-02-bienes-del-estado.md) deja abierto el régimen de vehículos en comodato o alquilados. Hasta confirmarlo, se les exige correlativo institucional igual, marcando su régimen de tenencia.

## Comportamiento esperado

1. El correlativo institucional es obligatorio en el alta y **no se puede reciclar**: dado de baja un vehículo, su correlativo queda ocupado permanentemente.
2. La placa se captura con su **estado**: vigente, en trámite, sin placa metálica con constancia, o no aplica. El estado *sin placa* exige adjunto y fecha.
3. Si se ingresa una placa que ya existe en otro vehículo, el sistema **advierte** e indica cuál, pero **permite guardar** dejando el motivo. La advertencia queda registrada.
4. Toda búsqueda, reporte y documento impreso identifica al vehículo por **correlativo + placa si existe + marca/modelo**, en ese orden. Nunca solo por placa.
5. El sistema alerta el vencimiento del documento sustitutivo o del trámite de placa ([RN-17](RN-17-alertas-de-vencimiento-documental.md)).

## Casos límite

- **Vehículo con placa nueva que reemplaza a una anterior.** No se sobrescribe: se registra el historial de placas con rangos de vigencia. Un expediente de 2024 debe seguir mostrando la placa que el vehículo tenía en 2024 — importa para conciliar tickets de peaje y multas de tránsito.
- **Dos vehículos con la misma placa por error del registro vehicular.** Ocurre. La advertencia lo señala; el correlativo mantiene la operación funcionando mientras el IP resuelve.
- **Vehículo sin placa detenido en un operativo del TSC o de la DNVT.** Riesgo operativo real. El sistema debe permitir imprimir la **constancia de trámite adjunta** junto con la orden de misión, para que el motorista la porte. Ver [RN-25](RN-25-salvoconducto-con-folio-y-qr.md).
- **Motocicletas sin placa.** Muy frecuente. Se tratan igual: correlativo institucional obligatorio, placa opcional.
- **Correlativo institucional duplicado por dos delegaciones que numeraron por su cuenta.** El correlativo es único **por institución**, no por delegación. La carga inicial debe resolver duplicados antes de operar; el sistema los rechaza. `[C]` confirmar si la institución numera por delegación — si es así, el correlativo se compone de código de delegación + número, y esa composición debe ser el identificador único.
- **Vehículo recuperado tras robo que reingresa a la flota.** Reingresa con su correlativo original, no con uno nuevo: el expediente debe ser continuo, con el período de baja registrado.

## Trazabilidad

- Normas: [NRM-06](../normativa/NRM-06-transito-y-licencias.md), [NRM-02](../normativa/NRM-02-bienes-del-estado.md)
- Reglas relacionadas: [RN-17](RN-17-alertas-de-vencimiento-documental.md), [RN-18](RN-18-rotulacion-del-vehiculo-del-estado.md), [RN-33](RN-33-categoria-de-peaje-derivada-de-ficha-tecnica.md)
- Actores: ACT-01, ACT-04, ACT-13
- Historias y casos especiales: pendientes — Bloque 2
