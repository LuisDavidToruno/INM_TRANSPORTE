# HU-108 — Impedir habilitar con licencia vencida y no crear categorías al vuelo

| Campo | Valor |
|---|---|
| **Módulo** | M-05 Motoristas y Habilitación · M-02 Catálogos Maestros |
| **Actor** | ACT-04 Jefe de Transporte |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador |

## Historia

**Como** Jefe de Transporte
**quiero** que el sistema registre la licencia vencida tal como es pero nunca habilite con ella, y que no me deje crear una categoría escribiéndola en el campo
**para** que ninguna excepción registrada exista como evidencia en contra ante un siniestro, y para que el catálogo de categorías siga siendo un catálogo

## Contexto

**Nunca se habilita con licencia vencida, por ningún rol, ni siquiera la Máxima Autoridad.** El bloqueo no admite excepción configurable y **no existe pantalla de excepción** — porque una excepción registrada sería evidencia en contra ante un siniestro. Esa es la postura que la institución puede sostener.

Al mismo tiempo, **el dato no se falsea**: la licencia vencida se registra tal como es, y el servidor queda `NO HABILITADO` con causa. Registrar el hecho y bloquear el efecto son cosas distintas.

Y un catálogo que se completa escribiendo en el campo deja de ser catálogo: incorporar una categoría nueva pasa por doble control.

## Reglas que la gobiernan

- [RN-09](../../01-negocio/reglas/RN-09-matriz-licencia-vehiculo.md) — **Bloqueo duro sin excepción configurable**
- [RN-10](../../01-negocio/reglas/RN-10-licencia-vigente-en-todo-el-rango.md) — **No configurable**
- [RN-39](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md) — El catálogo de categorías se carga con respaldo documental y se pone en vigencia por otro puesto
- [RN-40](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md) — La categoría se valida contra el catálogo vigente a la fecha del hecho
- [RN-17](../../01-negocio/reglas/RN-17-alertas-de-vencimiento-documental.md) — La alerta se abre en estado *vencido* y permanece
- [RN-01](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md) — Marco del registro del intento bloqueado

## Casos especiales que la afectan

- [CE-11](../casos-especiales/CE-11-licencia-vence-durante-la-mision.md) — Lo que ocurre cuando el vencimiento sobreviene en ruta
- [CE-13](../casos-especiales/CE-13-motorista-no-disponible-por-talento-humano.md) — Habilitado y disponible son cosas distintas
- [CE-19](../casos-especiales/CE-19-vehiculo-asignado-a-funcionario-frente-al-pool.md) — La jerarquía no exime del bloqueo

## Criterios de aceptación

```gherkin
# language: es
Característica: Bloqueo de habilitación con licencia vencida y control del catálogo de categorías

  Antecedentes:
    Dado un catálogo de categorías de licencia vigente con los valores "A", "B", "B1", "C1", "C", "D1", "D" y "CE"
    Y una persona "Óscar Banegas" existente en el espejo de Talento Humano

  Escenario: La licencia vencida se registra tal como es y no habilita
    Cuando el Jefe de Transporte captura una licencia con categoría "B" vencida el "2026-04-30"
    Entonces el sistema registra el dato tal como es, sin alterarlo
    Y deja a "Óscar Banegas" en estado "NO HABILITADO" con causa "licencia vencida"
    Y muestra "Registrado. Óscar Banegas queda NO HABILITADO: la categoría B venció el 30/04/2026."

  Escenario: No existe pantalla de excepción para habilitar con licencia vencida
    Cuando el Jefe de Transporte busca la opción de habilitar de todos modos
    Entonces el sistema no ofrece ninguna opción de excepción, acuse ni autorización superior
    Y muestra "La habilitación con licencia vencida no admite excepción por ningún rol."

  Escenario: Ni la Máxima Autoridad puede habilitar con licencia vencida
    Cuando la Máxima Autoridad intenta habilitar a "Óscar Banegas" con la categoría B vencida
    Entonces el sistema rechaza la habilitación
    Y muestra "La habilitación con licencia vencida no admite excepción, ni por resolución de la máxima autoridad."
    Y el intento queda en la pista de auditoría

  Escenario: La alerta se abre en estado vencido y permanece
    Cuando el sistema registra la licencia vencida de "Óscar Banegas"
    Entonces abre la alerta de vencimiento en estado "vencido"
    Y la alerta permanece hasta que se registre la renovación o la baja del recurso

  Escenario: Se bloquea la captura de una categoría que no existe en el catálogo vigente
    Cuando el Jefe de Transporte intenta capturar la categoría "CD"
    Entonces el sistema bloquea la captura
    Y muestra "La categoría CD no existe en el catálogo vigente a la fecha del hecho. La incorporación de una categoría pasa por el Administrador con respaldo documental y la aprobación de Gerencia Administrativa."
    Y no crea la categoría al vuelo

  Escenario: El Administrador no pone en vigencia lo que él mismo cargó
    Cuando el Administrador carga una categoría nueva con respaldo documental e intenta ponerla en vigencia
    Entonces el sistema rechaza la puesta en vigencia
    Y muestra "La puesta en vigencia corresponde a Gerencia Administrativa. Quien carga no aprueba."

  Escenario: La categoría se valida contra el catálogo vigente a la fecha del hecho
    Dado una categoría "CE" incorporada al catálogo con vigencia desde el "2026-06-01"
    Cuando el Jefe de Transporte captura una licencia con fecha de emisión "2025-11-20" y categoría "CE"
    Entonces el sistema advierte que la categoría no estaba vigente en el catálogo a esa fecha
    Y exige verificación contra el documento físico antes de guardar

  Escenario: El motorista no puede habilitarse a sí mismo desde su perfil
    Cuando "Óscar Banegas" intenta registrar su propia habilitación en el padrón
    Entonces el sistema rechaza la acción
    Y muestra "El motorista aporta el documento físico; no registra su propia habilitación."
    Y el intento queda en la pista de auditoría

  Escenario: Quien habilita y sería habilitado queda marcado como acumulación vigilada
    Dado un servidor que ocupa el puesto con rol de Jefe de Transporte y además conduce
    Cuando habilita su propia licencia en el padrón
    Entonces el sistema permite el acto exigiendo motivo escrito
    Y lo marca como "acumulación vigilada" en el tablero de Gerencia Administrativa y del Auditor Interno
```

## Fuera de alcance

- La captura general de la licencia — es [HU-105](HU-105-capturar-la-licencia-como-dato-propio-de-sigti.md)
- La derivación de vehículos habilitados — es [HU-106](HU-106-derivar-los-tipos-de-vehiculo-habilitados.md)
- Las alertas anticipadas de vencimiento — es [HU-107](HU-107-calcular-la-vigencia-de-la-habilitacion-y-alertar.md)
- La carga completa de la matriz licencia↔vehículo: es acto de catálogo con doble control

## Notas y pendientes

- ⚠️ **Hallazgo abierto: no existe par de incompatibilidad que cubra "habilita × es habilitado".** La tabla de incompatibilidades de [`actores-y-roles.md`](../../01-negocio/actores-y-roles.md) —autoridad en la materia— cubre las funciones de la misión y el descargo del bien, pero **no la autohabilitación para conducir**. Dado que el bloqueo de licencia existe precisamente porque la responsabilidad se traslada a quien autorizó, quien se habilita a sí mismo se autoriza a sí mismo el control. **Esta historia no crea la incompatibilidad**: aplica el tratamiento provisional de *acumulación vigilada* y eleva la propuesta. `[C]` La respuesta la debe dar **Auditoría Interna**
- `[C]` **Texto de la reforma al Art. 48 (2025)** sobre las categorías `CD` y `CE` — insumos **#20** y **#23**
- `[V]` Las ocho categorías conocidas por fuentes concordantes; `[C]` el contraste con el texto oficial — [NRM-06](../../01-negocio/normativa/NRM-06-transito-y-licencias.md). **No se eleva el nivel**
- `[C]` Confirmar con la institución si existe algún supuesto de excepción autorizada. **La postura por defecto es no**, y es la que se implementa — insumo **#1**
