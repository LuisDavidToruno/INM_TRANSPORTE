# RN-17 — Todo documento con fecha de vencimiento genera alerta anticipada según umbrales configurables

| Campo | Valor |
|---|---|
| **Módulos** | M-04, M-05, M-03, M-14 |
| **Origen** | Norma [NRM-06](../normativa/NRM-06-transito-y-licencias.md); decisión [DP-001 D-11](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) |
| **Verificación** | `[V]` la exigencia de alertar vencimientos — `[C]` los umbrales que quiere la institución |
| **Tipo** | Advertencia (proceso programado) |
| **Configurable** | Sí — parámetro `umbrales_alerta_vencimiento` por tipo de documento, valor de referencia 60 / 30 / 15 días |

## Enunciado

El sistema **debe** emitir alertas anticipadas del vencimiento de: licencia de conducir y cada una de sus categorías, matrícula, póliza de seguro, revisión mecánica, permisos, salvoconductos, constancia de trámite de placa, tarjetas de responsabilidad y cualquier otro documento con fecha de expiración registrada en el expediente del vehículo o del motorista.

Los umbrales de anticipación **deben** ser parámetros por tipo de documento, no constantes. El valor de referencia es 60, 30 y 15 días `[C]`.

Cada alerta **debe** tener destinatario por **puesto**, no por persona, y quedar registrada como emitida, vista y atendida.

## Justificación

[NRM-06](../normativa/NRM-06-transito-y-licencias.md) lo exige: *"alertar con anticipación configurable (60 / 30 / 15 días) el vencimiento de licencias, matrícula, permisos y pólizas"*.

Los bloqueos duros de [RN-09](RN-09-matriz-licencia-vehiculo.md), [RN-10](RN-10-licencia-vigente-en-todo-el-rango.md) y [RN-16](RN-16-seguro-y-revision-mecanica.md) son la última línea de defensa. Si un motorista se entera de que su licencia venció el día que iba a salir de misión, el sistema cumplió la ley y falló a la operación. La alerta convierte un bloqueo en una gestión.

[NRM-09](../normativa/NRM-09-realidad-operativa.md) advierte que la rotación es alta: por eso el destinatario es el **puesto**. Una alerta dirigida a una persona que ya no está en el cargo no llega a nadie.

## Condiciones de aplicación

Aplica a todo documento con fecha de vencimiento registrada. Los documentos **sin** fecha de vencimiento registrada generan su propia alerta: *documento sin vigencia registrada*, porque la ausencia del dato es tan riesgosa como el vencimiento.

**No aplica** a vehículos dados de baja ni a motoristas cesados, cuyos documentos dejan de alertar sin borrarse.

## Comportamiento esperado

1. Un proceso programado evalúa diariamente los vencimientos contra los umbrales vigentes y genera las alertas pendientes.
2. La alerta identifica documento, titular, fecha de vencimiento, días restantes y **el impacto operativo**: cuántas misiones programadas caen dentro del rango afectado. Esa última parte es la que hace que alguien actúe.
3. Al vencerse el documento, la alerta cambia de estado a **vencido** y permanece hasta que se registre la renovación o la baja del recurso.
4. Existe un **tablero de vencimientos** por dependencia y delegación, con filtro por tipo de documento y por estado.
5. Las alertas se acumulan también en el cliente de campo cuando afectan a un vehículo o motorista de la delegación, para que se vean sin conectividad.

## Casos límite

- **Delegación sin conectividad** que no recibe la alerta a tiempo. Las alertas se replican al cliente de campo en la última sincronización, con la marca de fecha de esa sincronización. Ver [RN-50](RN-50-degradacion-por-sincronizacion-detenida.md).
- **Documento renovado pero no registrado en el sistema.** La alerta seguirá activa y el bloqueo se aplicará. Es correcto: SIGTI decide con lo que consta, no con lo que se dice. Lo que el sistema debe facilitar es el registro rápido con adjunto fotográfico desde el móvil.
- **Umbral mayor que el plazo real de renovación del trámite.** Alertar a 60 días de un trámite que toma 5 no ayuda; alertar a 15 días de uno que toma 90 llega tarde. Por eso el umbral es **por tipo de documento**. `[C]` levantar con la institución los plazos reales de cada trámite.
- **Licencia con varias categorías que vencen en fechas distintas.** Se alerta **por categoría**, no por licencia. Perder la categoría C y conservar la B es una pérdida de habilitación parcial que hay que ver.
- **Vehículo cuyo trámite de placa lleva años sin resolverse** por el desabastecimiento nacional ([RN-15](RN-15-identidad-del-vehiculo-y-placa.md)). La alerta se volverá crónica y ruidosa. Se admite marcarla como **reconocida con fundamento** por un período configurable, tras el cual reaparece. Silenciarla para siempre no está disponible.
- **Alerta emitida y nunca vista** por vacante del puesto destinatario. Escala al puesto superior tras un plazo configurable. Una alerta sin dueño no es una alerta.

## Trazabilidad

- Normas: [NRM-06](../normativa/NRM-06-transito-y-licencias.md), [NRM-09](../normativa/NRM-09-realidad-operativa.md)
- Decisión: [DP-001, D-11](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md)
- Reglas relacionadas: [RN-10](RN-10-licencia-vigente-en-todo-el-rango.md), [RN-15](RN-15-identidad-del-vehiculo-y-placa.md), [RN-16](RN-16-seguro-y-revision-mecanica.md), [RN-18](RN-18-rotulacion-del-vehiculo-del-estado.md)
- Actores: ACT-04, ACT-10, ACT-11, ACT-13
- Historias y casos especiales: pendientes — Bloque 2
