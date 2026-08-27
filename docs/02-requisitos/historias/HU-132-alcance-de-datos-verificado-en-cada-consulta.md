# HU-132 — Ver únicamente lo que el alcance del puesto permite, verificado en la consulta y no en la pantalla

| Campo | Valor |
|---|---|
| **Módulo** | M-01 Organización y Seguridad |
| **Actor** | ACT-10 Encargado de Delegación · ACT-06 Motorista · ACT-11 Encargado de Mantenimiento |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — falta el mapa real de delegaciones (insumo #27) para probar el corte territorial |

## Historia

**Como** Encargado de Delegación
**quiero** ver todos los registros de mi delegación —atravesando las dependencias que operan en ella— y ninguno de otra, con la verificación hecha al resolver la consulta y no al pintar la pantalla
**para** que conocer el número de un expediente ajeno no alcance para abrirlo

## Contexto

El alcance de datos tiene **cuatro niveles y dos ejes que coexisten** ([actores-y-roles §3](../../01-negocio/actores-y-roles.md), autoridad):

| Nivel | Cómo se resuelve |
|---|---|
| PROPIO | Autor, solicitante, motorista asignado o custodio. Es lo que hace que un motorista no vea las misiones de sus compañeros |
| DEPENDENCIA | Por **descendencia jerárquica** de la unidad del puesto |
| DELEGACIÓN | Por **corte territorial**, atravesando dependencias |
| INSTITUCIÓN | Todo. Reservado a `ACT-08`, `ACT-09` y `ACT-12` |

Y dos ajustes que un modelo puramente jerárquico no resuelve: **el alcance por tipo de objeto** —`ACT-11` ve cualquier vehículo que entre al taller y no ve solicitudes ni fondos— y el **alcance sobre datos de personas externas**, que se rige por **necesidad de conocer** y deja registro de cada consulta.

[`RNF-14`](../no-funcionales/RNF-14-control-de-acceso-por-puesto-y-registro-de-consultas.md) es tajante sobre dónde va la verificación: *"si el filtro está solo en la interfaz, basta conocer un identificador para saltarlo."*

## Reglas que la gobiernan

- [RN-52](../../01-negocio/reglas/RN-52-registro-de-consultas-a-manifiestos.md) — Toda consulta a manifiestos y listas de pasajeros se registra: quién vio qué y cuándo
- [RN-51](../../01-negocio/reglas/RN-51-minimizacion-de-datos-de-personas-externas.md) — Solo los datos mínimos del catálogo autorizado, y solo a quien tiene necesidad de conocer
- [RN-01](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md) — El alcance no habilita a ejecutar: ver un expediente no es poder actuar sobre él
- [RN-05](../../01-negocio/reglas/RN-05-registro-cerrado-no-se-edita.md) — Lo anterior a la asignación del puesto es consultable dentro del alcance, nunca editable si está cerrado
- [RN-94](../../01-negocio/reglas/RN-94-fecha-de-corte-de-conocimiento-en-todo-reporte.md) — Un reporte declara su corte; el alcance no se puede eludir exportando

## Casos especiales que la afectan

- [CE-14](../casos-especiales/CE-14-vehiculo-prestado-entre-dependencias-o-instituciones.md) — El vehículo prestado queda bajo tenencia de otra dependencia y alguien debe verlo sin tener alcance sobre ella
- [CE-16](../casos-especiales/CE-16-vehiculo-a-taller-con-misiones-programadas.md) — El taller ve el vehículo de otra dependencia por alcance de objeto

## Criterios de aceptación

```gherkin
# language: es
Característica: Verificación del alcance de datos en cada consulta

  Antecedentes:
    Dado un puesto "Encargado de Transporte de la Delegación de Choluteca" con alcance "DELEGACIÓN" sobre "orden de misión"
    Y una persona "María López" ocupando ese puesto
    Y una Orden de Misión "OM-2026-0451" originada en la "Delegación de Choluteca"
    Y una Orden de Misión "OM-2026-0602" originada en la "Delegación de Danlí"

  Escenario: Se rechaza el acceso directo por identificador a un expediente fuera del alcance
    Cuando "María López" solicita directamente la Orden de Misión "OM-2026-0602" sin pasar por ninguna pantalla de listado
    Entonces el sistema deniega el acceso
    Y muestra "La Orden de Misión OM-2026-0602 pertenece a la Delegación de Danlí. Su puesto tiene alcance sobre la Delegación de Choluteca."
    Y no devuelve un resultado vacío ni un error técnico
    Y registra el intento con persona, puesto, identificador solicitado y momento

  Escenario: El motorista no ve las misiones de sus compañeros
    Dado un puesto "Motorista" con alcance "PROPIO" ocupado por "José Martínez"
    Y una Orden de Misión "OM-2026-0451" cuyo motorista asignado es "Pedro Rivera"
    Cuando "José Martínez" solicita la Orden de Misión "OM-2026-0451"
    Entonces el sistema deniega el acceso
    Y muestra "Usted solo puede consultar las misiones en las que figura como motorista asignado."

  Escenario: Se rechaza la exportación que excede el alcance
    Cuando "María López" solicita el reporte de misiones de toda la institución del "2026-01-01" al "2026-09-30"
    Entonces el sistema emite el reporte restringido a la "Delegación de Choluteca"
    Y muestra "Reporte emitido dentro de su alcance: Delegación de Choluteca. 148 misiones. Se excluyeron las de otras delegaciones."
    Y el reporte declara el alcance aplicado junto a su fecha de corte

  Escenario: El corte territorial atraviesa dependencias
    Dada una Orden de Misión "OM-2026-0470" originada por una unidad de la "Gerencia de Operaciones" que opera en la "Delegación de Choluteca"
    Cuando "María López" consulta el listado de misiones de su delegación
    Entonces "OM-2026-0470" aparece en el listado
    Y aparece aunque la "Gerencia de Operaciones" no sea la dependencia del puesto de "María López"

  Escenario: El alcance por objeto permite el vehículo y deniega la misión
    Dado un puesto "Jefe de Taller" con alcance "INSTITUCIÓN" sobre "vehículo" y sin alcance sobre "orden de misión"
    Y una persona "Óscar Banegas" ocupando ese puesto
    Cuando "Óscar Banegas" consulta el expediente del vehículo "TR-0092", adscrito a otra dependencia
    Entonces el sistema devuelve el expediente del vehículo
    Y al intentar abrir la Orden de Misión "OM-2026-0451" muestra "Su puesto no tiene alcance sobre órdenes de misión."

  Escenario: La consulta a un manifiesto de personas externas queda registrada
    Dada una Orden de Misión "OM-2026-0451" con manifiesto de 12 personas externas
    Cuando "María López" abre el manifiesto el "2026-09-14" a las "09:32"
    Entonces el sistema entrega el manifiesto
    Y registra la consulta con usuario, puesto, manifiesto, momento y origen
    Y el registro no puede ser borrado ni modificado por ningún puesto, incluido el Administrador del Sistema

  Escenario: El auditor interno ve todo y sus consultas también se registran
    Dado un puesto "Auditor Interno" con alcance "INSTITUCIÓN" y solo lectura
    Cuando "Ana Zelaya" consulta la Orden de Misión "OM-2026-0602"
    Entonces el sistema entrega el expediente completo en modo consulta
    Y registra la consulta
    Y no ofrece ninguna operación de edición sobre el expediente
```

## Fuera de alcance

- El otorgamiento del alcance en la relación puesto↔rol — es [HU-129](HU-129-otorgar-rol-al-puesto-con-alcance-y-vigencia.md)
- El contenido mínimo del manifiesto de personas externas — pertenece a M-17
- El alcance temporal que produce un préstamo entre dependencias: [`RN-63`](../../01-negocio/reglas/RN-63-prestamo-de-vehiculo-como-expediente-del-bien.md) lo deja como materia de `actores-y-roles` y **sigue sin resolver**

## Notas y pendientes

- `[V]` El **hábeas data del Artículo 182** está vigente y sostiene el registro de consultas — [NRM-07](../../01-negocio/normativa/NRM-07-transparencia-y-datos-personales.md). `[C]` que además lo exija el MARCI — [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md) tiene esa familia por confirmar. Ver [RN-52](../../01-negocio/reglas/RN-52-registro-de-consultas-a-manifiestos.md) y el hallazgo `HN1-14`
- `[C]` Mapa real de delegaciones y qué unidades de qué dependencias operan en cada una — insumo **#27**
- `[C]` **Alcance de datos temporal durante un préstamo de vehículo**: quién de la dependencia receptora debe ver el expediente del bien mientras lo tiene, y hasta cuándo. Lo dejó abierto el índice de reglas y **nadie lo ha resuelto** — insumo nuevo a registrar
- **Regla candidata:** *El alcance de datos se resuelve por tipo de objeto y se verifica en la resolución de la consulta, incluido el acceso directo por identificador.* Es la candidata 3 de [actores-y-roles §8](../../01-negocio/actores-y-roles.md); ninguna de las 97 reglas la recoge
