# RN-59 — Todo uso de un vehículo del Estado se ampara en una Orden de Misión, cualquiera sea su régimen

| Campo | Valor |
|---|---|
| **Módulos** | M-06, M-07, M-08, M-03, M-09 |
| **Origen** | Caso especial [CE-19](../../02-requisitos/casos-especiales/CE-19-vehiculo-asignado-a-funcionario-frente-al-pool.md) · Norma [NRM-02](../normativa/NRM-02-bienes-del-estado.md) · Premisa rectora 1 |
| **Verificación** | `[P]` la obligación de justificar el uso y la custodia del bien del Estado — [NRM-02](../normativa/NRM-02-bienes-del-estado.md). `[I]` la Orden de Misión como instrumento único: decisión de producto del equipo |
| **Tipo** | Bloqueo duro |
| **Configurable** | No |

## Enunciado

Ningún vehículo de la flota **debe** circular sin estar amparado por una Orden de Misión vigente. Ningún régimen de uso ([`RN-58`](RN-58-regimen-de-uso-del-vehiculo.md)), jerarquía del usuario ni práctica institucional exime de:

1. **Bitácora con odómetro** de inicio y fin de jornada, y conductor registrado ([`RN-57`](RN-57-habilitacion-de-quien-efectivamente-conduce.md))
2. **Permiso de circulación** en día u hora inhábil ([`RN-23`](RN-23-permiso-de-circulacion-en-dia-inhabil.md))
3. **Imputación del combustible a una misión** ([`RN-32`](RN-32-entrega-de-combustible-contra-orden-de-mision.md))

Para el vehículo en régimen `ASIGNADO_A_FUNCIONARIO` o `AFECTO_A_OPERACION`, la Orden de Misión que ampara el uso ordinario **es una Orden de Misión de vigencia extendida**, con folio, objeto declarado, ámbito geográfico y fecha de fin — no una ausencia de orden. Todo viaje **fuera del ámbito declarado** exige Orden de Misión ordinaria propia.

## Justificación

Esta es la regla que hoy se incumple sin que ningún artefacto la enuncie. [`RN-32`](RN-32-entrega-de-combustible-contra-orden-de-mision.md) la implica para el combustible — no se entrega vale sin orden — pero **nadie la enuncia para el uso**. El resultado observable es que el vehículo asignado a un funcionario circula durante meses sin un solo registro, y cuando el TSC pide la bitácora del período, no existe.

La consecuencia práctica del hueco es doble: el kilometraje del vehículo deja de ser conciliable con su consumo, y el permiso de circulación en día inhábil no tiene contra qué contrastarse. **El control no es el permiso: es el contraste entre los permisos emitidos y los días en que el vehículo efectivamente se movió según la bitácora.** Sin bitácora no hay contraste, y sin contraste el permiso es papel.

La Orden de Misión de vigencia extendida resuelve el problema real —nadie va a levantar una solicitud cada mañana para ir a la oficina— sin abrir la puerta a la excepción sin registro.

## Condiciones de aplicación

Aplica a todo vehículo de la flota y a todo desplazamiento en vía pública.

**No aplica** al desplazamiento dentro del predio institucional, ni al traslado a taller amparado por una orden de trabajo ([M-11](../../../CLAUDE.md)), que es su propio documento con odómetro de ingreso y de salida.

**No aplica** al vehículo en `EN_TENENCIA_AJENA`, cuyo uso lo ampara el acta de préstamo ([`RN-63`](RN-63-prestamo-de-vehiculo-como-expediente-del-bien.md)) y cuya bitácora la lleva el tenedor.

## Comportamiento esperado

1. La Orden de Misión de vigencia extendida se emite con el mismo circuito de autorización que cualquier otra, con su segregación intacta ([`RN-01`](RN-01-segregacion-de-funciones.md)) y su vigencia acotada a la del régimen que la motiva.
2. La bitácora de un vehículo bajo orden extendida se registra **por jornada**: fecha, conductor, odómetro de inicio, odómetro de fin, destinos y novedades. Capturada con **fecha del hecho** ([`RN-46`](RN-46-fecha-del-hecho-y-fecha-de-captura.md)), no reconstruida a fin de mes.
3. Todo vale o consumo de combustible se imputa a una misión concreta. El sistema produce el listado de **consumos huérfanos** — sin misión — que debe estar en cero.
4. El sistema contrasta, por vehículo y período: días con kilometraje registrado contra días con permiso de circulación en día inhábil, y expone las diferencias. Un día inhábil con kilometraje y sin permiso es hallazgo.
5. Un vehículo cuyo acumulado de odómetro crece sin bitácora que lo explique genera alerta al custodio y a ACT-08, con los kilómetros no justificados.

## Casos límite

- **Emergencia de madrugada del funcionario asignatario.** La orden extendida ya lo ampara si el desplazamiento cae en su ámbito; si no, se registra la salida y se convalida después ([`RN-73`](RN-73-convalidacion-de-actos-sin-autorizacion-previa.md)).
- **Vehículo con resguardo domiciliario autorizado.** El traslado casa–oficina–casa está dentro del ámbito de la orden extendida si el acto lo autoriza expresamente. Si el acto no lo dice, no está autorizado, y el sistema lo trata como uso fuera de ámbito.
- **Fin de semana con el vehículo estacionado.** Bitácora con jornada sin movimiento no es obligatoria; lo que el sistema exige es que **no haya kilometraje sin jornada**, no que haya jornada sin kilometraje.
- **Orden extendida vencida** con el vehículo circulando. Es el mismo caso del salvoconducto vencido: el hecho se registra, el vehículo no se detiene retroactivamente, y la misión de ese período cierra con hallazgo.
- **Institución que no acepta emitir órdenes extendidas** y quiere una orden por viaje. Es admisible: la regla exige amparo, no una forma concreta. La orden extendida es la salida operativa, no una obligación.

## Trazabilidad

- Norma: [NRM-02](../normativa/NRM-02-bienes-del-estado.md) `[P]` · Premisa rectora 1
- Reglas relacionadas: [RN-01](RN-01-segregacion-de-funciones.md), [RN-23](RN-23-permiso-de-circulacion-en-dia-inhabil.md), [RN-32](RN-32-entrega-de-combustible-contra-orden-de-mision.md), [RN-57](RN-57-habilitacion-de-quien-efectivamente-conduce.md), [RN-58](RN-58-regimen-de-uso-del-vehiculo.md), [RN-89](RN-89-kilometraje-acumulado-invariante-del-expediente.md)
- Casos especiales: [CE-19](../../02-requisitos/casos-especiales/CE-19-vehiculo-asignado-a-funcionario-frente-al-pool.md) — candidata `RN-C19c`
- Actores: ACT-08 autoriza la orden extendida · ACT-04 controla el contraste · ACT-12 audita
