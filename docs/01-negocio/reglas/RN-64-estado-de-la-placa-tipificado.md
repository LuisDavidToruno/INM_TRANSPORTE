# RN-64 — El estado de la placa es un dato tipificado con historial y vigencia, que distingue el número asignado de la presencia física de la lámina

| Campo | Valor |
|---|---|
| **Módulos** | M-03, M-04, M-14 |
| **Origen** | Caso especial [CE-17](../../02-requisitos/casos-especiales/CE-17-vehiculo-sin-placa-metalica.md) · Normas [NRM-02](../normativa/NRM-02-bienes-del-estado.md) y [NRM-06](../normativa/NRM-06-transito-y-licencias.md) |
| **Verificación** | `[V]` que la ausencia de placa metálica es un estado válido y frecuente — desabastecimiento nacional, [orden-de-mision.md `BD-03`](../../03-arquitectura/estados/orden-de-mision.md) y [NRM-06](../normativa/NRM-06-transito-y-licencias.md). `[I]` la tipificación del estado: implicación de requerimiento del equipo |
| **Tipo** | Bloqueo duro (sobre la captura) + derivación |
| **Configurable** | Sí — catálogo `estado_de_placa` |

## Enunciado

El vehículo **debe** tener dos datos distintos y no intercambiables:

- **Número de placa asignado en el registro** — puede existir aunque la lámina no
- **Estado de la placa física** — dato tipificado del catálogo configurable, cuyos valores iniciales son: `CON_LAMINA`, `NUMERO_ASIGNADO_SIN_LAMINA`, `SIN_NUMERO_ASIGNADO`, `LAMINA_EXTRAVIADA`, `LAMINA_RETENIDA_POR_AUTORIDAD`, `EN_TRAMITE_DE_REPOSICION`

Ambos datos **deben** conservarse como **historial con rangos de vigencia**. El número de placa de un vehículo **nunca se sobrescribe**: se cierra el rango anterior y se abre uno nuevo.

## Justificación

[`RN-15`](RN-15-identidad-del-vehiculo-y-placa.md) admite el estado *sin placa* — la identidad es el correlativo institucional y la placa no es obligatoria ni única — pero **no lo tipifica**. Y esa distinción es la que decide dos cosas concretas: **qué se imprime** en el paquete que viaja con el vehículo ([`RN-65`](RN-65-sin-lamina-respaldo-y-paquete-de-identificacion.md)) y **contra qué se concilia** una imputación externa ([`RN-66`](RN-66-imputacion-externa-por-jerarquia-de-anclas.md)).

Un vehículo con número asignado y sin lámina, uno con la lámina retenida por la DNVT y uno que nunca tuvo número son tres situaciones administrativas distintas con tres tratamientos distintos, y hoy el sistema las representaría todas con el mismo campo vacío.

El historial con vigencia es lo que responde la pregunta que el auditor hace de verdad: *¿a qué vehículo corresponde esta multa de marzo?* Sin historial, un número reasignado hace la respuesta imposible.

## Condiciones de aplicación

Aplica a todo vehículo de la flota, en cualquier régimen de tenencia — incluidos los alquilados, que circulan con placa particular a nombre del arrendador.

**No aplica** al número de bien del inventario nacional ni al correlativo institucional, que son anclas propias y estables ([`RN-15`](RN-15-identidad-del-vehiculo-y-placa.md)).

## Comportamiento esperado

1. La ficha del vehículo muestra el estado de placa vigente, con la fecha desde la cual rige y el documento que lo sustenta cuando corresponda.
2. Todo cambio de estado o de número abre un rango nuevo y cierra el anterior, con autor, fecha del hecho ([`RN-46`](RN-46-fecha-del-hecho-y-fecha-de-captura.md)) y motivo. El sistema **no permite** editar un rango cerrado ([`RN-05`](RN-05-registro-cerrado-no-se-edita.md)).
3. Una consulta por placa a una fecha determinada devuelve el vehículo que la tenía **a esa fecha**, no el que la tiene hoy.
4. El estado `EN_TRAMITE_DE_REPOSICION` exige expediente del trámite: fecha de inicio, institución ante la que se gestiona, gestiones realizadas y resultado. Es lo que responde *¿por qué este vehículo lleva dieciocho meses sin placa?* con una gestión documentada.
5. La advertencia de placa duplicada de [`RN-15`](RN-15-identidad-del-vehiculo-y-placa.md) se evalúa **por rango de vigencia**: dos vehículos con el mismo número en rangos que no se traslapan no son duplicado.
6. El sistema reporta la flota por estado de placa, con la antigüedad de cada estado no `CON_LAMINA`.

## Casos límite

- **Vehículo con lámina delantera y sin trasera.** No es un estado intermedio: es `CON_LAMINA` con novedad registrada en la constatación de rotulación ([`RN-18`](RN-18-rotulacion-del-vehiculo-del-estado.md)). Si se quiere distinguir, es una entrada más del catálogo, no una excepción en el código.
- **Placa retenida por autoridad** durante un operativo o un siniestro. `LAMINA_RETENIDA_POR_AUTORIDAD` exige el mismo respaldo que el bien retenido: autoridad, número de expediente y gestiones ([`RN-75`](RN-75-bien-retenido-o-sustraido-no-sale-del-registro.md)).
- **Número reasignado a otro vehículo** por el registro nacional. Los rangos lo permiten y lo resuelven. Sin rangos, una multa histórica se imputaría al vehículo equivocado.
- **Vehículo alquilado con placa particular.** Se registra igual, con el estado que corresponda y con la advertencia de que la placa no es del Estado. `[C]` [NRM-02](../normativa/NRM-02-bienes-del-estado.md) deja abierto qué régimen de rotulación e identificación aplica a los vehículos en comodato o alquilados.
- **Migración inicial de la flota** sin datos históricos. Se carga el estado corriente con vigencia desde la fecha de carga y se marca *sin historial previo*. Lo que no se sabe se declara, no se inventa.

## Trazabilidad

- Autoridad: [orden-de-mision.md `BD-03`](../../03-arquitectura/estados/orden-de-mision.md) — *"Placa metálica: no bloquea. Sin placa metálica es estado válido"* `[V]`
- Normas: [NRM-02](../normativa/NRM-02-bienes-del-estado.md) `[P]`, [NRM-06](../normativa/NRM-06-transito-y-licencias.md) `[P]`
- Reglas relacionadas: [RN-15](RN-15-identidad-del-vehiculo-y-placa.md), [RN-18](RN-18-rotulacion-del-vehiculo-del-estado.md), [RN-65](RN-65-sin-lamina-respaldo-y-paquete-de-identificacion.md), [RN-66](RN-66-imputacion-externa-por-jerarquia-de-anclas.md)
- Casos especiales: [CE-17](../../02-requisitos/casos-especiales/CE-17-vehiculo-sin-placa-metalica.md) — candidata `RN-C17a`
- Insumos pendientes: #43 rotulación de motocicletas · régimen de vehículos en comodato o alquilados
