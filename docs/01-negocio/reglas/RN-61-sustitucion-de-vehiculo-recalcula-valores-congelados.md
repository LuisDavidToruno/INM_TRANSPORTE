# RN-61 — La sustitución de vehículo recalcula y vuelve a congelar todo valor derivado del vehículo, con asiento de diferencia

| Campo | Valor |
|---|---|
| **Módulos** | M-07, M-18, M-09, M-15, M-13 |
| **Origen** | Caso especial [CE-16](../../02-requisitos/casos-especiales/CE-16-vehiculo-a-taller-con-misiones-programadas.md) · Normas [NRM-10](../normativa/NRM-10-peajes.md) y [NRM-01](../normativa/NRM-01-control-interno-tsc.md) |
| **Verificación** | `[P]` la exigencia de registros confiables y de tarifas por categoría. `[I]` el recongelamiento como mecanismo: implicación de requerimiento del equipo |
| **Tipo** | Bloqueo duro + cálculo |
| **Configurable** | No |

## Enunciado

Toda sustitución de vehículo sobre una Orden de Misión ya `PROGRAMADA` o posterior **debe** recalcular y volver a congelar **todo valor derivado del vehículo**, dejando **asiento de diferencia** contra el congelamiento anterior:

| Valor derivado | Efecto de la sustitución |
|---|---|
| Categoría de peaje y estimado por punto | Se recalcula con la tabla vigente **a la fecha del hecho** y se vuelve a congelar |
| Rendimiento esperado galonaje–kilometraje | Se recalcula con el del vehículo entrante |
| Habilitación del motorista (matriz licencia ↔ vehículo) | Se revalida ([`RN-09`](RN-09-matriz-licencia-vehiculo.md), [`RN-10`](RN-10-licencia-vigente-en-todo-el-rango.md)) |
| Compatibilidad y capacidad | Se revalidan por tramo ([`RN-68`](RN-68-compatibilidad-y-capacidad-por-tramo.md)) |
| Documentación del vehículo y estado operativo | Se revalidan ([`RN-16`](RN-16-seguro-y-revision-mecanica.md), [`RN-19`](RN-19-vehiculo-no-operativo-no-se-asigna.md)) |
| Custodia | Se traslada con constancia ([`RN-22`](RN-22-custodia-del-vehiculo.md)) |
| Salvoconducto de día u hora inhábil | Se **anula el anterior y se emite uno nuevo** ([`RN-25`](RN-25-salvoconducto-con-folio-y-qr.md)) |
| Tipo de combustible y folios de vale emitidos | Se anulan folio por folio y se re-emiten si el tipo cambia ([`RN-27`](RN-27-asignacion-de-combustible-con-folio.md)) |
| Paquete de identificación en carretera | Se re-emite si el vehículo entrante no tiene lámina ([`RN-65`](RN-65-sin-lamina-respaldo-y-paquete-de-identificacion.md)) |

El asiento anterior **no se sobrescribe**: la asignación original se conserva junto a la sustituta ([`RN-04`](RN-04-anulacion-como-asiento-reverso.md)).

## Nota de corrección — hallazgo `HB61-01`, 2026-08-30

> **«`PROGRAMADA` o posterior» no es alcanzable hoy, y la regla es la que dice de más.**
>
> El enunciado contempla la sustitución sobre una Orden de Misión ya `PROGRAMADA` **o**
> **posterior**. La [máquina de estados](../../03-arquitectura/estados/orden-de-mision.md)
> sólo admite `T-10` de `PROGRAMADA` a `PROGRAMADA`: después de `T-12` despachar no hay
> transición de reasignación.
>
> **Consecuencia concreta y medida:** el efecto sobre la **custodia** —«se traslada con
> constancia»— **no puede dispararse nunca**. El acta de entrega-recepción de
> [`RN-22`](RN-22-custodia-del-vehiculo.md) se levanta al despachar, que es después del único
> punto donde `T-10` existe. Un acta y una reasignación no pueden coexistir.
>
> La comprobación está escrita en `EfectosDeLaSustitucion` y es correcta si el acta existiera
> por cualquier vía — pero hoy es código inalcanzable, y se dice en vez de contarse como
> hecho.
>
> **La autoridad sobre transiciones es la máquina de estados**, no esta regla. O `§10.2` gana
> una transición de relevo en ruta —que `RN-71` ya contempla con acta y corte de odómetro—, o
> este enunciado se acota a `PROGRAMADA`. **No se resuelve desde acá.**

## Justificación

[`RN-14`](RN-14-sustitucion-de-motorista.md) exige revalidar las habilitaciones al sustituir motorista o vehículo, pero **no dice nada de los valores económicos congelados** por [`RN-41`](RN-41-congelamiento-del-valor-al-autorizar.md). El efecto es que una misión que se despacha con un camión de tres ejes en lugar del pickup programado sale con el estimado de peaje de categoría 2 y va a pagar categoría 4 en cada caseta: seis discrepancias falsas ([`RN-36`](RN-36-discrepancia-de-clasificacion-en-caseta.md)) y una liquidación que no cuadra por una razón que nadie va a encontrar.

Lo mismo ocurre con el rendimiento: conciliar el consumo del camión contra el rendimiento esperado del pickup produce una desviación grave y falsa, que consume el tiempo de quien la investiga y desacredita al detector ([`RN-30`](RN-30-conciliacion-galonaje-kilometraje.md)).

## Condiciones de aplicación

Aplica a toda sustitución de vehículo, antes o durante la ejecución.

Aplica a la **unidad sustituta entregada por el arrendador** en un vehículo alquilado ([`RN-62`](RN-62-titulo-de-tenencia-con-vigencia-y-rubros.md)): es un vehículo nuevo, con serie de odómetro propia, y todas las misiones programadas sobre la unidad anterior se revalidan.

**No aplica** a la sustitución de motorista, que rige por [`RN-14`](RN-14-sustitucion-de-motorista.md) y [`RN-71`](RN-71-traspaso-en-ruta-con-acta-y-corte-de-odometro.md).

## Comportamiento esperado

1. La sustitución no se confirma hasta que **todas** las revalidaciones pasen. Si alguna falla —el motorista no tiene categoría para el vehículo entrante, la póliza está vencida con bloqueo activo— la sustitución se bloquea y el sistema indica el dato concreto.
2. Cada recálculo genera **asiento de diferencia** con: valor anterior, valor nuevo, identificador de la tabla usada en cada uno, fecha del hecho y autor ([`RN-42`](RN-42-correccion-retroactiva-con-asiento-de-diferencia.md)).
3. Los documentos impresos anteriores se **anulan uno por uno** con referencia cruzada, y los nuevos se emiten con folio nuevo. Ambos se conservan. Ningún folio se reutiliza.
4. Si la sustitución ocurre con la misión **en ruta**, el recálculo se hace contra el paquete normativo congelado que lleva el dispositivo, y se marca como evaluación con paquete congelado; al sincronizar se revalida contra el servidor.
5. La liquidación presenta el **desglose por vehículo**: kilometraje, combustible y peajes de cada tramo con su propio vehículo ([`RN-72`](RN-72-imputacion-por-tramo-de-vehiculo-y-motorista.md)). El rendimiento **nunca** se calcula sobre la misión completa cuando hubo más de un vehículo.

## Casos límite

- **Sustitución con la misión `EN_RUTA`.** La [máquina de estados](../../03-arquitectura/estados/orden-de-mision.md) es la autoridad en transiciones y `T-17` cubre hoy prórroga, destino adicional y relevo de motorista, **no cambio de vehículo**. Se reportó como ampliación necesaria desde [CE-02](../../02-requisitos/casos-especiales/CE-02-averia-mecanica-en-ruta.md). Mientras la ampliación no exista, esta regla describe qué debe recalcularse cuando la transición se habilite; **no la habilita por sí sola**.
- **Vehículo entrante con exoneración de peaje y saliente sin ella**, o al revés. La exoneración no se hereda: exige fundamento y vigencia registrados para **ese** vehículo ([`RN-38`](RN-38-exoneracion-de-peaje.md)). Sin ellos, paga.
- **Vale de combustible ya entregado al motorista.** El vale sigue al motorista, no al vehículo ([`RN-32`](RN-32-entrega-de-combustible-contra-orden-de-mision.md)). Solo se anula y re-emite si el **tipo de combustible** difiere.
- **Sustitución que no cambia la categoría de peaje ni el rendimiento** — dos pickups iguales. El recálculo se ejecuta igual y produce asientos con diferencia cero. Es más barato que decidir cuándo saltárselo.
- **Sustitución revertida** porque el vehículo original salió del taller antes. Es otra sustitución, con sus propios asientos. No se "deshace" la anterior.

## Trazabilidad

- Autoridad: [orden-de-mision.md](../../03-arquitectura/estados/orden-de-mision.md) — `EF-03` congelamiento del paquete normativo, `T-10`, `T-17`
- Normas: [NRM-10](../normativa/NRM-10-peajes.md) `[P]`, [NRM-01](../normativa/NRM-01-control-interno-tsc.md) `[P]`
- Reglas relacionadas: [RN-04](RN-04-anulacion-como-asiento-reverso.md), [RN-14](RN-14-sustitucion-de-motorista.md), [RN-33](RN-33-categoria-de-peaje-derivada-de-ficha-tecnica.md), [RN-38](RN-38-exoneracion-de-peaje.md), [RN-41](RN-41-congelamiento-del-valor-al-autorizar.md), [RN-42](RN-42-correccion-retroactiva-con-asiento-de-diferencia.md), [RN-60](RN-60-indisponibilidad-sobrevenida-y-reservas.md), [RN-72](RN-72-imputacion-por-tramo-de-vehiculo-y-motorista.md), [RN-91](RN-91-categoria-y-tarifa-de-peaje-impresas-en-la-orden.md)
- Casos especiales: [CE-16](../../02-requisitos/casos-especiales/CE-16-vehiculo-a-taller-con-misiones-programadas.md) `RN-C16c` · [CE-02](../../02-requisitos/casos-especiales/CE-02-averia-mecanica-en-ruta.md) · [CE-15](../../02-requisitos/casos-especiales/CE-15-vehiculo-en-comodato-o-alquilado.md) `RN-c:sustitucion-de-unidad-por-el-arrendador`
