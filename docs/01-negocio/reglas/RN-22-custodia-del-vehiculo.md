# RN-22 — Todo vehículo tiene custodio vigente, y el despacho traslada la custodia al motorista con constancia

| Campo | Valor |
|---|---|
| **Módulos** | M-03, M-07, M-08, M-15 |
| **Origen** | Norma [NRM-02](../normativa/NRM-02-bienes-del-estado.md) — tarjeta de responsabilidad y acta de entrega-recepción |
| **Verificación** | `[P]` la exigencia de tarjeta de responsabilidad (Manual de Propiedad Estatal, articulado no extraído) — `[C]` los formatos vigentes |
| **Tipo** | Bloqueo duro |
| **Configurable** | No |

## Enunciado

Todo vehículo de la flota **debe** tener, en todo momento, un **custodio responsable vigente** (ACT-13), registrado con acta de entrega-recepción y fecha de inicio de la custodia.

El despacho de una misión **debe** registrar el **traslado temporal de custodia** al motorista: fecha y hora, odómetro, nivel de combustible, accesorios y herramientas entregadas, estado de la unidad y constancia de recepción. El retorno **debe** registrar la devolución con los mismos elementos.

Un vehículo **sin custodio vigente no debe** poder ser despachado.

## Justificación

[NRM-02](../normativa/NRM-02-bienes-del-estado.md) exige soportar *"tarjeta de responsabilidad / asignación de custodio, con acta de entrega-recepción firmada, y trazar cada cambio de custodio"*, y registrar el ciclo completo del bien incluidas pérdidas y siniestros.

La pregunta que resuelve esta regla es la que aparece cuando algo falta o algo se daña: **¿quién tenía el vehículo en ese momento?** Sin cadena de custodia, la deducción de responsabilidad no tiene sobre quién recaer, y el hallazgo del TSC queda sin responsable identificado — lo que agrava, no atenúa.

## Condiciones de aplicación

Aplica a todo vehículo, en cualquier régimen de tenencia.

Aplica también a los movimientos internos: entrega al taller (ACT-11), préstamo entre dependencias, y resguardo por operativo.

**No aplica** a vehículos dados de baja y entregados a disposición final, cuya custodia sale del sistema con el acta correspondiente.

## Comportamiento esperado

1. La custodia permanente y la custodia temporal por misión son **dos registros distintos**: la primera no se interrumpe, la segunda se superpone durante la misión y se extingue al retorno.
2. La constancia de recepción usa el esquema interno de autorización de [DP-001 D-04](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md): usuario autenticado o código gestionado por el sistema, con registro completo. Además se imprime para firma manuscrita cuando el motorista no tenga dispositivo ([NRM-08](../normativa/NRM-08-firma-electronica.md)).
3. El sistema conserva el **historial completo de custodias** por vehículo, consultable por rango de fechas: en cualquier momento del pasado se puede decir quién respondía por la unidad.
4. Al cambiar el custodio permanente — rotación de personal — se exige acta de entrega-recepción con estado y odómetro. El sistema soporta **traspaso masivo** de custodias, como exige [NRM-09](../normativa/NRM-09-realidad-operativa.md).
5. Toda diferencia entre lo entregado y lo devuelto (herramienta faltante, daño nuevo) genera un registro de novedad vinculado al expediente de M-12.

## Casos límite

- **Relevo de motoristas en ruta.** Cada relevo es un traspaso de custodia con odómetro y estado. Sin eso, un daño ocurrido en el segundo tramo se le atribuye al primer motorista.
- **Custodio permanente que sale de vacaciones.** La custodia permanente **no se transfiere automáticamente**: o se traspasa con acta a otro servidor, o se mantiene y el sistema advierte que el custodio está ausente. `[C]` confirmar el criterio de la institución; ambas prácticas existen.
- **Vehículo asignado a una delegación sin custodio designado.** Bloqueo del despacho. Es incómodo y es correcto: un vehículo del Estado sin responsable identificado es un hallazgo esperando ocurrir.
- **Custodio que cesa en el cargo dejando el vehículo asignado.** El espejo de Talento Humano lo detecta y el sistema marca el vehículo como *custodia vacante*, con alerta al Jefe de Transporte y bloqueo de despacho tras un plazo configurable. `[C]` el plazo.
- **Vehículo siniestrado que queda en poder de la aseguradora, el taller o las autoridades.** La custodia se traslada al tercero registrado, con acta y fecha. El vehículo no queda "sin custodio" ni sigue formalmente bajo el motorista.
- **Motorista que devuelve el vehículo fuera de horario y no hay quien reciba.** Se registra la devolución con la evidencia disponible (foto del odómetro, ubicación, hora) y queda **pendiente de recepción**; el receptor confirma después con su propia marca de tiempo. La custodia no queda en el aire, pero tampoco se finge una recepción que no ocurrió.
- **Despacho en campo sin conectividad.** La constancia se captura localmente y sincroniza después ([RN-43](RN-43-captura-de-campo-sin-conectividad.md)); el documento impreso con folio pre-asignado viaja con el motorista ([RN-44](RN-44-identificadores-y-folios-en-el-cliente.md)).

## Trazabilidad

- Normas: [NRM-02](../normativa/NRM-02-bienes-del-estado.md), [NRM-08](../normativa/NRM-08-firma-electronica.md), [NRM-09](../normativa/NRM-09-realidad-operativa.md)
- Reglas relacionadas: [RN-14](RN-14-sustitucion-de-motorista.md), [RN-19](RN-19-vehiculo-no-operativo-no-se-asigna.md), [RN-31](RN-31-odometro-de-retorno.md), [RN-43](RN-43-captura-de-campo-sin-conectividad.md)
- Actores: ACT-05, ACT-06, ACT-11, ACT-13
- Historias y casos especiales: pendientes — Bloque 2
