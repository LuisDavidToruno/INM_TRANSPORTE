# RN-55 — La habilitación que vence con la misión en ruta no detiene la ejecución, pero cierra el expediente con hallazgo

| Campo | Valor |
|---|---|
| **Módulos** | M-05, M-08, M-07, M-13 |
| **Origen** | Caso especial [CE-11](../../02-requisitos/casos-especiales/CE-11-licencia-vence-durante-la-mision.md) · Norma [NRM-06](../normativa/NRM-06-transito-y-licencias.md) · [DP-001 D-12](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) |
| **Verificación** | `[P]` la exigencia de licencia vigente y habilitante — [NRM-06](../normativa/NRM-06-transito-y-licencias.md), con el texto reformado del Art. 48 pendiente (insumo #20). `[I]` el tratamiento del hecho sobrevenido: implicación de requerimiento del equipo |
| **Tipo** | Bloqueo duro (sobre la prórroga y sobre el cierre limpio) |
| **Configurable** | No |

## Enunciado

Cuando la habilitación de un conductor —licencia, categoría o restricción médica— **pierde vigencia mientras la Orden de Misión está `EN_RUTA`**, el sistema:

1. **No bloquea la ejecución en curso.** El vehículo ya salió y está a distancia de la sede; detenerlo por decisión del sistema no es una salida operativa ni una salida segura
2. **Registra el hecho** como evento de bitácora con fecha del hecho, ubicación y odómetro, funcionando sin conectividad
3. **Rechaza toda prórroga (`T-17`) que dependa de ese conductor**, admitiendo únicamente el **relevo**
4. **Excluye la Orden de Misión del cierre limpio (`T-21`)**, forzando el cierre por `T-22` con hallazgo tipificado

## Justificación

[`RN-10`](RN-10-licencia-vigente-en-todo-el-rango.md) gobierna la **asignación**: exige vigencia durante todo el rango de la misión y bloquea al programar y al despachar. **Nada gobierna el hecho sobrevenido**, y el hecho sobrevenido ocurre: la misión se prorroga, el retorno se atrasa por un derrumbe, o simplemente la licencia vencía el día 3 de una misión de cinco días que se aprobó cuando cubría el rango original.

Detener la ejecución no es una opción real. Cerrar el expediente como si nada hubiera pasado, tampoco: ante un siniestro ocurrido el día 4, el expediente tiene que mostrar que la institución **sabía** que la licencia había vencido, cuándo lo supo y qué hizo. Un cierre limpio sería el peor documento posible.

El bloqueo de la prórroga es la parte preventiva: extender voluntariamente una misión con un conductor sin habilitación es una decisión, no un accidente, y esa sí se puede impedir.

## Condiciones de aplicación

Aplica a todo conductor registrado, sea motorista de padrón o no ([`RN-57`](RN-57-habilitacion-de-quien-efectivamente-conduce.md)).

Aplica también al vencimiento de la **documentación del vehículo** dentro del rango de la misión, con el mismo tratamiento: no detiene, registra, bloquea la prórroga y cierra con hallazgo.

**No aplica** antes de la salida: ahí manda [`RN-10`](RN-10-licencia-vigente-en-todo-el-rango.md) y el bloqueo es duro y previo.

## Comportamiento esperado

1. El paquete normativo congelado que lleva el dispositivo incluye la fecha de vencimiento de la habilitación. El cliente de campo **detecta el vencimiento sin conectividad** y lo registra el día en que ocurre.
2. El evento se notifica al motorista, a ACT-04 y a la jefatura de la delegación en cuanto haya señal, sin perderse si no la hay ([`RN-43`](RN-43-captura-de-campo-sin-conectividad.md)).
3. Toda solicitud de prórroga que mantenga a ese conductor se bloquea con el dato concreto: *"Licencia \<número\>, categoría \<x\>, venció el \<fecha\>. La prórroga exige relevo de conductor."* La [máquina de estados](../../03-arquitectura/estados/orden-de-mision.md) ya revalida `BD-02` y `BD-03` en la prórroga contra la nueva fecha de fin.
4. Al liquidar, el sistema propone el hallazgo tipificado *habilitación vencida durante la ejecución* con los datos concretos; ACT-08 lo clasifica y lo justifica individualmente ([`RN-08`](RN-08-cadena-de-trazabilidad-para-cierre.md)).
5. El expediente del motorista conserva el **historial completo de licencias con sus rangos de vigencia**, no solo la vigente. Una licencia sobrescrita hace imposible reconstruir qué estaba vigente el día del hecho.
6. La alerta anticipada de [`RN-17`](RN-17-alertas-de-vencimiento-documental.md) debe haber existido. Su ausencia, o su emisión sin acuse, es parte de lo que el hallazgo documenta.

## Casos límite

- **Comprobante de trámite de renovación de la DNVT.** `[C]` [NRM-06](../normativa/NRM-06-transito-y-licencias.md) no lo resuelve — insumo #20. Mientras no se confirme, se registra como **habilitación provisional que no levanta el bloqueo**: aceptarlo no se puede sostener ante un siniestro sin norma que lo respalde.
- **Holgura de retorno.** `[C]` insumo #1 — el rango contra el que se verifica la vigencia debe incluir la holgura configurada, no solo la fecha de fin planificada.
- **Vencimiento durante un retorno ya iniciado**, con el vehículo a pocas horas de la sede. Mismo tratamiento: registro, hallazgo, sin detener. La proximidad no cambia el hecho.
- **Relevo disponible en la delegación más cercana.** Es la salida recomendada y la única que permite prorrogar. Se ejecuta por [`RN-71`](RN-71-traspaso-en-ruta-con-acta-y-corte-de-odometro.md) con revalidación completa del entrante.
- **Restricción médica que cambia**, no vence — un dictamen nuevo de Talento Humano con fecha de inicio dentro de la misión. Llega por el espejo ([`RN-48`](RN-48-datos-espejo-de-solo-lectura.md)); si no hay evento en ruta que la explique, es conflicto para resolución humana, no un dato que se aplica retroactivamente.

## Trazabilidad

- Autoridad: [orden-de-mision.md](../../03-arquitectura/estados/orden-de-mision.md) — `BD-02`, `BD-03`, `T-17` con revalidación en prórroga, `T-21`, `T-22`
- Norma: [NRM-06](../normativa/NRM-06-transito-y-licencias.md) `[P]` · Decisión: [DP-001 D-12](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md)
- Reglas relacionadas: [RN-09](RN-09-matriz-licencia-vehiculo.md), [RN-10](RN-10-licencia-vigente-en-todo-el-rango.md), [RN-11](RN-11-restricciones-medicas-del-motorista.md), [RN-17](RN-17-alertas-de-vencimiento-documental.md), [RN-57](RN-57-habilitacion-de-quien-efectivamente-conduce.md), [RN-71](RN-71-traspaso-en-ruta-con-acta-y-corte-de-odometro.md), [RN-77](RN-77-versionado-del-alcance-autorizado.md)
- Casos especiales: [CE-11](../../02-requisitos/casos-especiales/CE-11-licencia-vence-durante-la-mision.md) · [CE-06](../../02-requisitos/casos-especiales/CE-06-la-mision-se-extiende-mas-dias-destinos-o-kilometros.md)
- Insumos pendientes: #1 holgura de retorno · #20 texto del Art. 48 reformado
