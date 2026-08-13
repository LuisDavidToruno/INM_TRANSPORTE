# RN-90 — Toda intervención del instrumento de medición es un evento del expediente del vehículo con orden de trabajo y autorización nominativa

| Campo | Valor |
|---|---|
| **Módulos** | M-03, M-11, M-08, M-07 |
| **Origen** | Caso especial [CE-22](../../02-requisitos/casos-especiales/CE-22-odometro-inconsistente.md) · Norma [NRM-01](../normativa/NRM-01-control-interno-tsc.md) |
| **Verificación** | `[P]` la exigencia de autorización previa y de control sobre los ajustes de registros — [NRM-01](../normativa/NRM-01-control-interno-tsc.md). `[C]` el plazo con odómetro averiado — decisión del PO |
| **Tipo** | Bloqueo duro |
| **Configurable** | Sí — parámetro `dias_max_odometro_averiado` `[C]` |

## Enunciado

La intervención del instrumento de medición del vehículo —**reemplazo, reparación, vuelta de contador, cambio de unidad**— **debe** registrarse como **evento del expediente del vehículo**, con:

1. **Orden de trabajo** con folio
2. **Lecturas antes y después**
3. **Fotografía de ambos tableros**
4. **Autorización nominativa**

**Ningún ajuste de kilometraje puede originarse fuera de ese evento.**

Un vehículo con **odómetro declarado averiado** genera **advertencia** al programar y **bloquea** la programación si la falla lleva más de `dias_max_odometro_averiado` sin reparar `[C]`.

## Justificación

Hoy [`RN-31`](RN-31-odometro-de-retorno.md) permite que un ajuste de kilometraje nazca como **motivo tipificado dentro de una bitácora**. Es decir: **el acto con mayor incentivo de manipulación de toda la flota entra por la puerta de menor control**, la del registro operativo diario, hecho por una sola persona, sin orden de trabajo y sin autorización.

Un tablero reemplazado sin evento es un vehículo que perdió su historia: el mantenimiento preventivo se pospone indefinidamente ([`RN-89`](RN-89-kilometraje-acumulado-invariante-del-expediente.md)), la conciliación galonaje–kilometraje del período se vuelve incomparable, y la falla que venga después es responsabilidad de quien autorizó el vehículo.

Las dos fotografías —del tablero viejo y del nuevo— son la evidencia que un auditor puede verificar por sí mismo, sin creerle a nadie.

## Condiciones de aplicación

Aplica a toda intervención del odómetro y de cualquier instrumento cuya lectura alimente el expediente.

Aplica a la **vuelta de contador**, que no es una intervención física pero sí un cierre de serie con motivo tipificado propio.

**No aplica** a la corrección de un **error de digitación** de una lectura, que es asiento de corrección ([`RN-04`](RN-04-anulacion-como-asiento-reverso.md)) y no toca el instrumento.

## Comportamiento esperado

1. El evento abre orden de trabajo en M-11 y se registra con: motivo tipificado, taller o responsable de la intervención, fecha del hecho, lectura de cierre de la serie anterior, lectura de apertura de la nueva, unidad de cada una y fotografías.
2. La autorización es **nominativa** y no puede ser de quien ejecuta la intervención ni de quien conduce el vehículo ([`RN-01`](RN-01-segregacion-de-funciones.md)).
3. El acumulado **no se toca**: se cierra la serie y se abre la nueva ([`RN-89`](RN-89-kilometraje-acumulado-invariante-del-expediente.md)). Ningún camino del sistema permite escribir el acumulado a mano.
4. El odómetro declarado averiado se registra como **novedad del vehículo con fecha**, y desde esa fecha el sistema advierte al programar: *"Odómetro averiado desde el \<fecha\>; el consumo del período no será conciliable"*.
5. Vencido `dias_max_odometro_averiado`, la programación se bloquea. El levantamiento del bloqueo exige la intervención registrada, no una excepción.
6. El período con odómetro averiado se marca en la conciliación como **no concluyente**, no como cumplido ni como desviado.

## Casos límite

- **`[C]` Odómetro averiado y programación — escalado al PO.** No hay respuesta obvia y la decisión tiene costo en ambos sentidos:

  | Opción | Costo |
  |---|---|
  | **Bloquear** la programación mientras el odómetro esté averiado | Una institución con flota vieja pierde unidades operativas por una falla que no impide rodar. En delegaciones con dos vehículos, paraliza |
  | **Advertir** y dejar programar | El vehículo circula **sin denominador**: su consumo del período no es conciliable y el TSC lo va a ver como período sin control |
  | **Parámetro configurable con plazo** — advierte, y bloquea si la falla lleva más de N días sin reparar | Es la que se propone. Exige fijar N `[C]` y aceptar que durante N días la conciliación queda no concluyente |

  [`RN-19`](RN-19-vehiculo-no-operativo-no-se-asigna.md) bloquea por **estado operativo**, y un odómetro roto **no vuelve al vehículo `NO_DISPONIBLE`**: rueda perfectamente. Por eso hace falta esta regla.
- **Intervención hecha en ruta**, en un taller de pueblo. Se registra como evento con lo que haya —fotografías, comprobante del taller— y la orden de trabajo se abre al retorno, referenciando el evento. Lo que no se admite es que la lectura nueva entre por la bitácora sin evento.
- **Tablero cambiado por el arrendador** en un vehículo alquilado. Mismo evento, con el titular como ejecutante y el contrato como respaldo ([`RN-62`](RN-62-titulo-de-tenencia-con-vigencia-y-rubros.md)).
- **Odómetro que funciona intermitentemente.** Se declara averiado. Un instrumento que a veces mide no es un instrumento que mide.
- **Manipulación deliberada.** Esta regla no la impide; la hace **visible y atribuible**: cada intervención tiene autorizador nombrado y fotografías, y las intervenciones repetidas del mismo vehículo o autorizadas por la misma persona son un patrón que el reporte expone.

## Trazabilidad

- Norma: [NRM-01](../normativa/NRM-01-control-interno-tsc.md) `[P]`
- Reglas relacionadas: [RN-01](RN-01-segregacion-de-funciones.md), [RN-04](RN-04-anulacion-como-asiento-reverso.md), [RN-19](RN-19-vehiculo-no-operativo-no-se-asigna.md), [RN-30](RN-30-conciliacion-galonaje-kilometraje.md), [RN-31](RN-31-odometro-de-retorno.md), [RN-60](RN-60-indisponibilidad-sobrevenida-y-reservas.md), [RN-62](RN-62-titulo-de-tenencia-con-vigencia-y-rubros.md), [RN-89](RN-89-kilometraje-acumulado-invariante-del-expediente.md)
- Casos especiales: [CE-22](../../02-requisitos/casos-especiales/CE-22-odometro-inconsistente.md) — candidatas `RN-C22b`, `RN-C22c`
- Insumos pendientes: plazo máximo con odómetro averiado `[C]`
