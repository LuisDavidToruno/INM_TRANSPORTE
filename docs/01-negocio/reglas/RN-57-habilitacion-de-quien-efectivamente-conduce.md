# RN-57 — La habilitación para conducir se verifica sobre la persona que efectivamente conduce, cualquiera sea su puesto

| Campo | Valor |
|---|---|
| **Módulos** | M-05, M-07, M-08, M-03 |
| **Origen** | Casos especiales [CE-19](../../02-requisitos/casos-especiales/CE-19-vehiculo-asignado-a-funcionario-frente-al-pool.md), [CE-10](../../02-requisitos/casos-especiales/CE-10-motorista-incapacitado-en-ruta.md) y [CE-05](../../02-requisitos/casos-especiales/CE-05-cambio-de-motorista-con-la-mision-en-curso.md) · Norma [NRM-06](../normativa/NRM-06-transito-y-licencias.md) · [DP-001 D-12](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) |
| **Verificación** | `[P]` la matriz licencia ↔ vehículo — [NRM-06](../normativa/NRM-06-transito-y-licencias.md) deja pendiente el texto reformado del Art. 48, insumo #20. `[I]` la extensión a quien no es motorista de padrón: implicación de requerimiento del equipo |
| **Tipo** | Bloqueo duro |
| **Configurable** | No |

## Enunciado

[`RN-09`](RN-09-matriz-licencia-vehiculo.md) y [`RN-10`](RN-10-licencia-vigente-en-todo-el-rango.md) **deben** evaluarse sobre **quien conduce**, no sobre quien ostenta el puesto de motorista.

Toda jornada de conducción de un vehículo institucional **debe** tener un **conductor registrado y nominado**. Si esa persona no pertenece al padrón de motoristas — funcionario asignatario, servidor de otra dependencia, motorista de otra institución en un préstamo — el sistema **debe** capturar y evaluar su licencia con **el mismo rigor** que aplicaría a un motorista de padrón, y desde ese momento le aplican las mismas incompatibilidades de [`actores-y-roles.md`](../actores-y-roles.md) que a cualquier conductor de misión.

Ningún régimen de uso, jerarquía ni excepción operativa **debe** eximir de esta verificación.

## Justificación

`RN-09` y `RN-10` están redactadas alrededor del **motorista**: se disparan cuando se asigna un motorista a una misión. El funcionario que conduce el vehículo que tiene asignado no es motorista, nunca se le asigna nada, y por eso **hoy no lo alcanza ninguna verificación de licencia**. Es justo el vehículo que aparece en los operativos del Tribunal Superior de Cuentas y en las inspecciones de la DNVT.

El efecto de la omisión no es teórico: si ese vehículo se ve involucrado en un siniestro y quien conducía no tenía categoría habilitante, la responsabilidad se traslada a quien autorizó el uso del bien del Estado. [DP-001 D-12](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) es explícita en que este bloqueo no admite excepción registrada, porque *"una excepción registrada sería evidencia en contra ante un siniestro"*.

## Condiciones de aplicación

Aplica a toda persona que conduzca un vehículo de la flota, en cualquier régimen de uso ([`RN-58`](RN-58-regimen-de-uso-del-vehiculo.md)) y en cualquier momento del ciclo de la misión.

Aplica al conductor entrante en un relevo en ruta ([`RN-71`](RN-71-traspaso-en-ruta-con-acta-y-corte-de-odometro.md)) y al conductor eventual incorporado por incapacidad del motorista.

**No aplica** a quien traslada el vehículo dentro del predio institucional sin salir a vía pública, si la institución así lo define `[C]` — insumo pendiente.

**No aplica** al vehículo prestado a otra institución con motorista de la receptora ([`RN-63`](RN-63-prestamo-de-vehiculo-como-expediente-del-bien.md)): ahí la habilitación la verifica el tenedor, y el hecho consta en el acta de entrega.

## Comportamiento esperado

1. Toda Orden de Misión declara un conductor por jornada. La bitácora registra **quién condujo cada día**, no solo quién salió el primer día.
2. Si el conductor no está en el padrón de motoristas, el sistema exige: identidad, número de licencia, categoría, fecha de vencimiento, restricciones y **fotografía de la licencia física**. Con esos datos evalúa `RN-09` y `RN-10` y bloquea igual que con un motorista.
3. En campo y sin conectividad, la evaluación se hace contra el padrón de habilitación del paquete de misión. Si el conductor no está en el paquete, se registra con **evaluación diferida marcada**, foto de la licencia física, y **revalidación obligatoria al sincronizar**. Si la revalidación falla, se produce hallazgo `H-07` — no se borra el hecho.
4. El conductor registrado es el sujeto de la imputación de infracciones ([`RN-66`](RN-66-imputacion-externa-por-jerarquia-de-anclas.md)), de la custodia ([`RN-22`](RN-22-custodia-del-vehiculo.md)) y de la conciliación de rendimiento por tramo ([`RN-72`](RN-72-imputacion-por-tramo-de-vehiculo-y-motorista.md)).
5. El sistema produce el listado de **jornadas sin conductor registrado** por vehículo y período. Ese listado, en cero, es la prueba de cumplimiento; con renglones, es el trabajo pendiente.

## Casos límite

- **Funcionario de alto nivel que conduce su vehículo asignado.** Le aplica igual. `[C]` insumo #28 — si la institución tiene política escrita sobre si el asignatario puede conducir; hoy ocurre de hecho y no consta política. Mientras no exista, el sistema **exige la licencia y bloquea**, que es la posición conservadora y la única sostenible ante un siniestro.
- **Servidor no perteneciente al padrón que conduce en emergencia**, porque el motorista se incapacitó en carretera. Se registra, se evalúa con lo que haya, y si no se pudo evaluar en el momento, se convalida después ([`RN-73`](RN-73-convalidacion-de-actos-sin-autorizacion-previa.md)). La ausencia de convalidación es hallazgo, no motivo para no registrar el hecho.
- **Licencia en trámite de renovación con comprobante de la DNVT.** `[C]` [NRM-06](../normativa/NRM-06-transito-y-licencias.md) no lo resuelve — insumo #20. Mientras tanto se registra como **habilitación provisional que no levanta el bloqueo**.
- **Dos conductores en la misma jornada** — relevo a media ruta. Se registran ambos con el corte de odómetro del acta ([`RN-71`](RN-71-traspaso-en-ruta-con-acta-y-corte-de-odometro.md)), y cada tramo se imputa a su conductor.
- **Vehículo que se mueve sin conductor declarado** según la bitácora y el kilometraje. No es un caso a resolver por el sistema: es exactamente el hallazgo que el listado de jornadas sin conductor debe hacer visible.

## Trazabilidad

- Norma: [NRM-06](../normativa/NRM-06-transito-y-licencias.md) `[P]` · Decisión: [DP-001 D-12](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md)
- Autoridad de incompatibilidades: [actores-y-roles.md](../actores-y-roles.md)
- Reglas relacionadas: [RN-09](RN-09-matriz-licencia-vehiculo.md), [RN-10](RN-10-licencia-vigente-en-todo-el-rango.md), [RN-11](RN-11-restricciones-medicas-del-motorista.md), [RN-14](RN-14-sustitucion-de-motorista.md), [RN-55](RN-55-habilitacion-vencida-durante-la-mision.md), [RN-58](RN-58-regimen-de-uso-del-vehiculo.md), [RN-59](RN-59-todo-uso-se-ampara-en-orden-de-mision.md)
- Casos especiales: [CE-19](../../02-requisitos/casos-especiales/CE-19-vehiculo-asignado-a-funcionario-frente-al-pool.md) `RN-C19d` · [CE-10](../../02-requisitos/casos-especiales/CE-10-motorista-incapacitado-en-ruta.md) `RN-c:motorista-eventual-habilitado-en-ruta` · [CE-05](../../02-requisitos/casos-especiales/CE-05-cambio-de-motorista-con-la-mision-en-curso.md) `RN-c:padron-de-relevo-en-el-paquete-de-mision`, `RN-c:habilitacion-no-verificable-en-campo`
- Insumos pendientes: #20 texto del Art. 48 reformado · #28 quién autoriza la misión de un funcionario de alto nivel
