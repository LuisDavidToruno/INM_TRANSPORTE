# RN-28 — Todo consumo de combustible se registra con galones, monto, estación, odómetro y fotografía del comprobante

| Campo | Valor |
|---|---|
| **Módulos** | M-09, M-08, M-16 |
| **Origen** | `PROP-01` de [insumos-pendientes](../../07-gestion/insumos-pendientes.md); normas [NRM-01](../normativa/NRM-01-control-interno-tsc.md) (TSC-NOGECI V-10) y [NRM-09](../normativa/NRM-09-realidad-operativa.md) |
| **Verificación** | `[V]` la exigencia de registro oportuno y conciliación — `[C]` qué acepta la institución como comprobante |
| **Tipo** | Bloqueo duro sobre los campos; advertencia sobre el comprobante |
| **Configurable** | Sí — `comprobante_obligatorio_por_monto` (umbral), con valor `[C]` |

## Enunciado

Cada consumo de combustible **debe** registrarse en el momento del hecho, desde el campo y **sin necesidad de conectividad**, con:

1. Asignación (folio) de la que se descuenta
2. **Galones** y **monto**
3. **Estación de servicio** y su ubicación
4. **Lectura de odómetro** al momento de la carga
5. Fecha y hora **del hecho**
6. **Fotografía del comprobante**

Los cinco primeros son obligatorios: sin ellos el registro no se guarda. La fotografía del comprobante es **exigible pero no bloqueante**: su ausencia se registra como observación y se arrastra a la liquidación.

## Justificación

`PROP-01`: *"el motorista registra el consumo desde el campo, con galones, monto, estación, odómetro y fotografía del comprobante. Funciona sin conectividad."*

TSC-NOGECI V-10 exige **Registro Oportuno**: la bitácora y el consumo se registran *en el momento del hecho*, no reconstruidos después. El odómetro en cada carga es lo que hace posible la conciliación de [RN-30](RN-30-conciliacion-galonaje-kilometraje.md): sin él solo se puede conciliar la misión completa, y una carga anómala queda escondida en el promedio.

La foto no bloquea porque en zonas rurales el comprobante a veces no existe o es ilegible, y bloquear ahí significa que el consumo no se registra en absoluto — que es peor.

## Condiciones de aplicación

Aplica a todo consumo imputado a una asignación de combustible, en misión o en el predio institucional.

`[C]` La institución debe definir qué acepta como comprobante: factura del proveedor, ticket de la estación, o declaración jurada en zonas sin facturación. [NRM-08](../normativa/NRM-08-firma-electronica.md) prevé *constancias o declaraciones juradas de gastos sin factura, en zonas rurales* `[I]`.

## Comportamiento esperado

1. El formulario de consumo es idéntico en pantalla y en el formato impreso ([NRM-09](../normativa/NRM-09-realidad-operativa.md)) y funciona completo sin red ([RN-43](RN-43-captura-de-campo-sin-conectividad.md)).
2. El sistema valida coherencia inmediata: galones > 0, monto > 0, odómetro no menor al último registrado del vehículo ([RN-31](RN-31-odometro-de-retorno.md)), y galones no superiores a la capacidad del tanque más un margen. La incoherencia produce advertencia con acuse, no pérdida del registro.
3. La suma de consumos no puede exceder el monto de la asignación; el exceso se marca como **sobregiro** y notifica a ACT-04 y ACT-07.
4. La fotografía se guarda con el registro y sincroniza como adjunto diferido si la red no lo permite en el momento.
5. La ausencia de comprobante se tipifica en la liquidación y, si supera el umbral configurado, produce **cierre con hallazgo** ([RN-08](RN-08-cadena-de-trazabilidad-para-cierre.md)).

## Casos límite

- **Estación de servicio no listada en el catálogo.** Se permite capturar como texto libre con ubicación, marcando *estación no catalogada*. Bloquear por esto detendría cargas legítimas en carretera. El dato alimenta la depuración posterior del catálogo.
- **Carga en bidones o en cisterna institucional**, no en estación. Es un tipo de consumo distinto y debe tipificarse; el odómetro sigue siendo obligatorio. `[C]` confirmar si la institución tiene almacenamiento propio de combustible — cambiaría el circuito completo.
- **Comprobante emitido a nombre del motorista y no de la institución.** Se registra la observación; es un defecto de descargo que la liquidación debe señalar. `[C]` confirmar con Auditoría Interna si lo aceptan.
- **Foto tomada de un comprobante de otra carga.** No detectable automáticamente. Mitigación: la captura registra fecha, hora y ubicación del dispositivo, que la conciliación cruza contra la estación declarada. Una foto tomada a 200 km de la estación declarada es una alerta.
- **Registro capturado días después desde papel.** Admitido por [RN-47](RN-47-digitacion-diferida-desde-papel.md), con fecha del hecho distinta de la fecha de captura, constancia del digitador y adjunto del original. Genera observación por incumplimiento de registro oportuno.
- **Odómetro averiado.** Es una falla del vehículo (M-11) y debe reportarse como tal. Mientras esté averiado, el consumo se registra con odómetro marcado *no disponible por falla*, referenciando el reporte. La conciliación de ese período se hace por ruta estimada y se marca como no concluyente. Lo que no se admite es inventar una lectura.
- **Consumo con dispositivo sin batería.** El registro se hace en el formato en papel y se digita después. El papel es el respaldo previsto, no una falla del diseño.

## Trazabilidad

- Normas: [NRM-01](../normativa/NRM-01-control-interno-tsc.md), [NRM-09](../normativa/NRM-09-realidad-operativa.md), [NRM-08](../normativa/NRM-08-firma-electronica.md)
- Decisión: `PROP-01` en [insumos-pendientes](../../07-gestion/insumos-pendientes.md)
- Reglas relacionadas: [RN-27](RN-27-asignacion-de-combustible-con-folio.md), [RN-29](RN-29-liquidacion-de-combustible.md), [RN-30](RN-30-conciliacion-galonaje-kilometraje.md), [RN-31](RN-31-odometro-de-retorno.md), [RN-43](RN-43-captura-de-campo-sin-conectividad.md), [RN-47](RN-47-digitacion-diferida-desde-papel.md)
- Actores: ACT-06, ACT-07, ACT-04
- Historias y casos especiales: pendientes — Bloque 2
