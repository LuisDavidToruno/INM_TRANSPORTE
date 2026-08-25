# HU-129 — Otorgar un rol al puesto —nunca a la persona— con alcance de datos y vigencia

| Campo | Valor |
|---|---|
| **Módulo** | M-01 Organización y Seguridad |
| **Actor** | ACT-01 Administrador del Sistema · ACT-08 Gerencia Administrativa (aprueba) |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — la aprobación de segundo par está `[I]` y falta el pronunciamiento de la institución (insumo #26) |

## Historia

**Como** Administrador del Sistema
**quiero** otorgar un rol `ACT-xx` a un **puesto**, declarando su alcance de datos y su rango de vigencia, sin que exista en ninguna parte del sistema la operación de dar un permiso a una persona
**para** que la rotación de personal sea el cambio de una fila y no un proyecto de reconstrucción manual de permisos

## Contexto

Es el requisito central de [`RNF-14`](../no-funcionales/RNF-14-control-de-acceso-por-puesto-y-registro-de-consultas.md): *"Permisos asignados directamente a una persona: **0**. El modelo no ofrece la operación."*

Dos cosas que se otorgan juntas y que la gente confunde:

- **El rol** dice *qué puede hacer* — `ACT-04` Jefe de Transporte programa y asigna.
- **El alcance de datos** dice *sobre qué registros* — propio, dependencia, delegación o institución. **El alcance se otorga en la relación puesto↔rol, no en el rol** ([actores-y-roles §3.1](../../01-negocio/actores-y-roles.md), autoridad). El mismo `ACT-04` tiene alcance INSTITUCIÓN si el puesto es de sede, y DELEGACIÓN si el puesto es regional.

Y hay un tercer eje que un modelo puramente jerárquico no resuelve: **el alcance por tipo de objeto**. `ACT-11` Encargado de Mantenimiento ve cualquier vehículo que entre al taller, sea de la dependencia que sea, y **no ve** solicitudes ni fondos.

## Reglas que la gobiernan

- [RN-01](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md) — El rol se otorga al puesto; la incompatibilidad se evalúa sobre la persona que lo ocupa
- [RN-03](../../01-negocio/reglas/RN-03-registro-inmutable-de-autorizacion.md) — El otorgamiento y su aprobación se registran de forma inmutable con identidad, momento y huella del contenido
- [RN-52](../../01-negocio/reglas/RN-52-registro-de-consultas-a-manifiestos.md) — El alcance sobre datos de personas externas se rige por necesidad de conocer y toda consulta se registra
- [RN-39](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md) — La relación puesto↔rol lleva rango de vigencia como cualquier dato institucional
- [RN-04](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md) — Un otorgamiento no se borra: se cierra su vigencia con motivo y autor

## Casos especiales que la afectan

- Ninguno de los 28 vigentes. **Caso especial candidato:** *rol otorgado con alcance mayor al que el puesto necesita* — es el hallazgo de auditoría más frecuente en control de accesos y hoy nada lo detecta

## Criterios de aceptación

```gherkin
# language: es
Característica: Otorgamiento de rol a un puesto con alcance de datos y vigencia

  Antecedentes:
    Dado un puesto "Encargado de Transporte de la Delegación de Choluteca" en la unidad "Oficina Regional de Trámites"
    Y una persona "María López" ocupando ese puesto desde el "2026-09-01"

  Escenario: No existe la operación de otorgar un permiso a una persona
    Cuando el Administrador del Sistema busca la forma de otorgar el rol "ACT-04 Jefe de Transporte" a la persona "María López"
    Entonces el sistema no ofrece ninguna operación de otorgamiento a persona
    Y muestra "Los permisos se otorgan al puesto. María López ocupa el puesto Encargado de Transporte de la Delegación de Choluteca; otórguelo ahí."

  Escenario: Se rechaza el otorgamiento sin alcance de datos declarado
    Cuando el Administrador del Sistema otorga el rol "ACT-04 Jefe de Transporte" al puesto sin declarar alcance de datos
    Entonces el sistema rechaza el otorgamiento
    Y muestra "Declare el alcance de datos: propio, dependencia, delegación o institución. Sin alcance no se puede resolver ninguna consulta."

  Escenario: Se rechaza un alcance superior al permitido por defecto sin aprobación
    Cuando el Administrador del Sistema otorga el rol "ACT-04 Jefe de Transporte" con alcance "INSTITUCIÓN" a un puesto de la delegación
    Entonces el sistema rechaza el otorgamiento
    Y muestra "El alcance por defecto de ACT-04 en un puesto de delegación es DELEGACIÓN. Un alcance INSTITUCIÓN requiere aprobación de la Gerencia Administrativa con motivo escrito."

  Escenario: Se rechaza el otorgamiento sin fecha de inicio de vigencia
    Cuando el Administrador del Sistema otorga el rol "ACT-05 Encargado de Despacho" al puesto sin fecha de inicio
    Entonces el sistema rechaza el otorgamiento
    Y muestra "El otorgamiento de rol exige fecha de inicio de vigencia. Los permisos se resuelven a la fecha del hecho."

  Escenario: El rol con facultad de autorizar queda pendiente de aprobación y no surte efecto
    Cuando el Administrador del Sistema otorga el rol "ACT-03 Jefatura Inmediata" al puesto con alcance "DEPENDENCIA"
    Entonces el sistema registra el otorgamiento en estado "PENDIENTE DE APROBACIÓN"
    Y "María López" no puede autorizar ninguna solicitud
    Y el otorgamiento aparece en el tablero de la Gerencia Administrativa y en el del Auditor Interno

  Escenario: La aprobación de la Gerencia Administrativa pone el rol en vigencia
    Dado un otorgamiento del rol "ACT-03 Jefatura Inmediata" en estado "PENDIENTE DE APROBACIÓN"
    Cuando la Gerencia Administrativa lo aprueba el "2026-09-02"
    Entonces el rol queda vigente desde la fecha de inicio declarada
    Y el sistema registra carga y aprobación como dos actos fechados por separado, con autor distinto en cada uno

  Escenario: El alcance por objeto se otorga por separado
    Cuando el Administrador del Sistema otorga el rol "ACT-11 Encargado de Mantenimiento" con alcance "INSTITUCIÓN" sobre el objeto "vehículo" y sin alcance sobre el objeto "orden de misión"
    Entonces el sistema acepta el otorgamiento
    Y el ocupante ve todos los vehículos que ingresan al taller
    Y al intentar abrir una solicitud de transporte el sistema muestra "Su puesto no tiene alcance sobre solicitudes de transporte."

  Escenario: El cierre de vigencia del rol no invalida los actos ya ejecutados
    Dado un rol "ACT-04 Jefe de Transporte" vigente y 12 misiones programadas bajo él
    Cuando el Administrador del Sistema cierra su vigencia el "2026-10-31" con motivo "reorganización de la delegación"
    Entonces las 12 misiones conservan su validez y su autoría
    Y el ocupante deja de poder programar a partir del "2026-11-01"
    Y el cierre queda registrado con autor, momento y motivo
```

## Fuera de alcance

- El rechazo del otorgamiento por incompatibilidad absoluta — es [HU-130](HU-130-rechazar-acumulacion-incompatible-de-roles.md)
- La resolución de los permisos efectivos en el momento de operar — es [HU-131](HU-131-permisos-efectivos-a-la-fecha-del-hecho.md)
- La verificación del alcance en cada consulta — es [HU-132](HU-132-alcance-de-datos-verificado-en-cada-consulta.md)
- El catálogo de roles `ACT-xx` en sí: es catálogo cerrado del producto, no configurable por institución

## Notas y pendientes

- `[I]` **La aprobación de segundo par** para roles con facultad de autorizar, aprobar fondos o administrar parámetros la propone [actores-y-roles §4.1 nota 13](../../01-negocio/actores-y-roles.md) marcada `[I]`, y la propia nota deja `[C]` si la institución la acepta. **Esta historia la implementa; si la institución la rechaza, el escenario de aprobación se retira** — insumo **#26**
- `[C]` Qué alcance por defecto corresponde a cada puesto real de la institución — insumo **#27**
- `[C]` Si existe régimen formal de excusa por conflicto de interés que agregue una incompatibilidad más — insumo **#30**
- **Regla candidata:** *El permiso se otorga exclusivamente a un puesto, con alcance de datos por tipo de objeto y rango de vigencia; el sistema no ofrece la operación de otorgar permiso a una persona.* Ninguna de las 97 lo enuncia — `RN-01` gobierna la incompatibilidad, no el modelo de otorgamiento
