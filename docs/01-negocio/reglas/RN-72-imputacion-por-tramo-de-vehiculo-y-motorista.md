# RN-72 — Cuando la misión se ejecuta con más de un vehículo o más de un conductor, kilometraje, combustible y peajes se imputan por tramo

| Campo | Valor |
|---|---|
| **Módulos** | M-13, M-09, M-18, M-08, M-14 |
| **Origen** | Casos especiales [CE-02](../../02-requisitos/casos-especiales/CE-02-averia-mecanica-en-ruta.md), [CE-05](../../02-requisitos/casos-especiales/CE-05-cambio-de-motorista-con-la-mision-en-curso.md), [CE-10](../../02-requisitos/casos-especiales/CE-10-motorista-incapacitado-en-ruta.md), [CE-14](../../02-requisitos/casos-especiales/CE-14-vehiculo-prestado-entre-dependencias-o-instituciones.md) · Norma [NRM-01](../normativa/NRM-01-control-interno-tsc.md) |
| **Verificación** | `[P]` la exigencia de correlación entre consumo, uso y autorización — [NRM-01](../normativa/NRM-01-control-interno-tsc.md). `[I]` la imputación por tramo: implicación de requerimiento del equipo |
| **Tipo** | Cálculo + bloqueo duro |
| **Configurable** | No |

## Enunciado

Cuando una Orden de Misión se ejecuta con **más de un vehículo** o con **más de un conductor**, el kilometraje, el combustible, los peajes y los indicadores de conducción **deben** imputarse **por tramo**, delimitados por el odómetro de las actas de traspaso ([`RN-71`](RN-71-traspaso-en-ruta-con-acta-y-corte-de-odometro.md)) y de sustitución ([`RN-61`](RN-61-sustitucion-de-vehiculo-recalcula-valores-congelados.md)).

**El rendimiento galonaje–kilometraje nunca se calcula sobre la misión completa** cuando hubo más de un vehículo. Cada vehículo se concilia contra su propio rendimiento esperado y su propio tramo.

Cada vehículo involucrado **cierra su bitácora con su propio odómetro**. Cada responsable de fondo **liquida su propia asignación**.

## Justificación

El rendimiento esperado es un atributo del vehículo. Conciliar los galones de un camión contra el rendimiento esperado de un pickup produce una desviación grave y **falsa**, que consume el tiempo de quien la investiga y desacredita al detector — el mismo daño que produce un detector de discrepancias montado sobre una tabla de tarifas no verificada.

Con los conductores pasa lo equivalente: promediar la conducción de dos personas en una misión de 900 km produce un indicador que no describe a ninguna de las dos, y hace imposible atribuir un consumo anómalo a quien lo causó.

Y hay un tercer efecto, el que ve el auditor: **la correlación entre lo consumido y lo recorrido** solo cierra si ambos términos hablan del mismo vehículo y del mismo período.

## Condiciones de aplicación

Aplica a toda misión con sustitución de vehículo, relevo de conductor, o ambas.

Aplica al **kilometraje recorrido bajo tenencia ajena** ([`RN-63`](RN-63-prestamo-de-vehiculo-como-expediente-del-bien.md)): se asienta con las dos lecturas del acta y **no entra** en la conciliación galonaje–kilometraje del vehículo, porque no hubo consumo nuestro contra esos kilómetros.

**No aplica** a la misión ejecutada con un solo vehículo y un solo conductor, donde el tramo es la misión.

## Comportamiento esperado

1. El sistema construye la **secuencia de tramos** a partir de los eventos de traspaso y sustitución, con odómetro de inicio y de fin de cada uno.
2. Cada consumo de combustible se imputa al tramo al que pertenece por su **fecha y hora del hecho** y por su odómetro, no por la misión.
3. Cada paso por caseta se imputa al vehículo que lo cruzó, con **su** categoría de peaje y **su** tarifa congelada ([`RN-91`](RN-91-categoria-y-tarifa-de-peaje-impresas-en-la-orden.md)).
4. La liquidación presenta el **desglose por tramo**: vehículo, conductor, kilómetros, galones, rendimiento observado, rendimiento esperado, desviación, peajes y responsable de fondo. El total de la misión es la suma, no un promedio.
5. Un tramo cuyo odómetro de cierre no se conoce se declara **no determinado**; su kilometraje **no se completa con distancia teórica** ([`RN-89`](RN-89-kilometraje-acumulado-invariante-del-expediente.md)) y la conciliación de ese tramo se declara **truncada**.
6. Los indicadores de conducta de manejo, oportunidad de registro y consumo se acumulan por conductor y por vehículo a partir de los tramos, nunca de las misiones.

## Casos límite

- **Carga de combustible que abarca dos tramos** — el motorista saliente llena el tanque antes del relevo. El consumo se imputa al tramo del hecho, que es el de quien cargó; el remanente en tanque se registra en el acta y entra a la conciliación como variable, no como faltante ([`RN-83`](RN-83-todo-ingreso-de-combustible-es-un-abastecimiento.md)).
- **Traspaso sin acta.** No hay corte y por tanto no hay tramos: la misión se liquida como un solo tramo a nombre del responsable original ([`RN-71`](RN-71-traspaso-en-ruta-con-acta-y-corte-de-odometro.md)) y el hecho entra como hallazgo. La regla no fabrica un corte que nadie registró.
- **Tramo de traslado del vehículo averiado al taller**, hecho por una grúa. No es tramo de misión: es un evento de la interrupción, y sus kilómetros no son kilómetros recorridos por el vehículo.
- **Vehículo sustituto que ya venía con combustible de otra asignación.** Se declara el nivel al incorporarse y se separa del fondo de esta misión.
- **Misión que termina con el vehículo original recuperado.** Tres tramos, no dos, y el primero y el tercero son del mismo vehículo con series de odómetro continuas.

## Trazabilidad

- Norma: [NRM-01](../normativa/NRM-01-control-interno-tsc.md) `[P]`
- Reglas relacionadas: [RN-29](RN-29-liquidacion-de-combustible.md), [RN-30](RN-30-conciliacion-galonaje-kilometraje.md), [RN-31](RN-31-odometro-de-retorno.md), [RN-61](RN-61-sustitucion-de-vehiculo-recalcula-valores-congelados.md), [RN-63](RN-63-prestamo-de-vehiculo-como-expediente-del-bien.md), [RN-71](RN-71-traspaso-en-ruta-con-acta-y-corte-de-odometro.md), [RN-83](RN-83-todo-ingreso-de-combustible-es-un-abastecimiento.md), [RN-89](RN-89-kilometraje-acumulado-invariante-del-expediente.md), [RN-91](RN-91-categoria-y-tarifa-de-peaje-impresas-en-la-orden.md)
- Casos especiales: [CE-02](../../02-requisitos/casos-especiales/CE-02-averia-mecanica-en-ruta.md) `RN-c:imputacion-de-consumo-por-tramo` · [CE-05](../../02-requisitos/casos-especiales/CE-05-cambio-de-motorista-con-la-mision-en-curso.md) `RN-c:conciliacion-por-tramo-de-motorista` · [CE-10](../../02-requisitos/casos-especiales/CE-10-motorista-incapacitado-en-ruta.md) · [CE-14](../../02-requisitos/casos-especiales/CE-14-vehiculo-prestado-entre-dependencias-o-instituciones.md) `RN-c:kilometraje-bajo-tenencia-ajena`
