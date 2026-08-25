# HU-133 — Revocar un rol con efecto inmediato sin invalidar ni un solo acto ya ejecutado

| Campo | Valor |
|---|---|
| **Módulo** | M-01 Organización y Seguridad |
| **Actor** | ACT-01 Administrador del Sistema |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — depende de [HU-129](HU-129-otorgar-rol-al-puesto-con-alcance-y-vigencia.md) |

## Historia

**Como** Administrador del Sistema
**quiero** revocar un rol de un puesto con efecto inmediato, dejando asiento del motivo, y que ningún acto ejecutado bajo ese rol pierda validez
**para** poder cortar un acceso el mismo día que lo pide Auditoría Interna sin que la revocación levante sospecha sobre 300 autorizaciones anteriores

## Contexto

La revocación es el acto que más miedo da ejecutar, y por una razón razonable: nadie sabe qué pasa con lo firmado. La respuesta está escrita en [actores-y-roles §7.1](../../01-negocio/actores-y-roles.md) para la delegación y aplica igual al rol: *"Revocable en cualquier momento. Efecto inmediato. **Los actos ya ejecutados bajo la delegación no se invalidan** — se ejecutaron con facultad vigente."*

Lo que sí hay que resolver es **qué pasa con lo que quedó a medias**: el acto iniciado y no consumado, y los expedientes pendientes de decisión en ese puesto. Un expediente pendiente no queda huérfano: **queda atribuido al puesto**, y quien lo ocupe lo ve al entrar ([actores-y-roles §2.4](../../01-negocio/actores-y-roles.md)).

Y hay un caso que produce hallazgo si no se previó: la revocación con una misión que ese puesto ya autorizó y que sigue `EN_RUTA`. **La misión no se detiene.** La operación en carretera no se interrumpe por un cambio administrativo en la sede.

## Reglas que la gobiernan

- [RN-03](../../01-negocio/reglas/RN-03-registro-inmutable-de-autorizacion.md) — La autorización ejecutada con facultad vigente es inmutable; la revocación posterior no la toca
- [RN-04](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md) — La revocación es asiento con motivo y autor, no un borrado
- [RN-05](../../01-negocio/reglas/RN-05-registro-cerrado-no-se-edita.md) — Ningún registro cerrado se altera por un cambio de permisos
- [RN-06](../../01-negocio/reglas/RN-06-transiciones-de-estado-de-la-orden.md) — Una misión no cambia de estado por un evento de administración de accesos
- [RN-02](../../01-negocio/reglas/RN-02-escalamiento-de-autorizacion.md) — Los actos pendientes escalan al puesto superior cuando el puesto queda sin facultad

## Casos especiales que la afectan

- [CE-28](../casos-especiales/CE-28-hallazgo-posterior-sobre-mision-cerrada.md) — Un hallazgo posterior puede motivar la revocación, y no por eso se reabre nada
- **Caso especial candidato:** *revocación de rol solicitada por Auditoría Interna con misiones en ruta autorizadas por ese puesto*

## Criterios de aceptación

```gherkin
# language: es
Característica: Revocación de un rol otorgado a un puesto

  Antecedentes:
    Dado un puesto "Jefe de Transporte" con el rol "ACT-04 Jefe de Transporte" vigente desde el "2026-01-01"
    Y una persona "María López" ocupando ese puesto
    Y 312 misiones programadas por ese puesto entre el "2026-01-01" y el "2026-09-14"

  Escenario: Se rechaza la revocación sin motivo escrito
    Cuando el Administrador del Sistema revoca el rol "ACT-04" del puesto sin motivo
    Entonces el sistema rechaza la revocación
    Y muestra "Toda revocación de rol exige motivo escrito. Queda en la pista de auditoría con su autor."

  Escenario: La revocación no invalida los actos ya ejecutados
    Cuando el Administrador del Sistema revoca el rol "ACT-04" el "2026-09-15" con motivo "traslado a otra unidad"
    Entonces las 312 misiones conservan su validez, su autoría y su puesto de autoría
    Y ninguna cambia de estado
    Y el sistema muestra "Rol ACT-04 revocado del puesto Jefe de Transporte el 15/09/2026. 312 actos anteriores conservan plena validez."

  Escenario: El efecto es inmediato sobre las operaciones nuevas
    Dada la revocación registrada el "2026-09-15" a las "10:00"
    Cuando "María López" intenta programar una misión el "2026-09-15" a las "10:04"
    Entonces el sistema rechaza la operación
    Y muestra "El rol ACT-04 fue revocado del puesto Jefe de Transporte el 15/09/2026 a las 10:00. Su puesto ya no faculta programar misiones."

  Escenario: Las misiones en ruta no se interrumpen
    Dadas 3 misiones en estado "EN_RUTA" programadas por ese puesto
    Cuando el Administrador del Sistema revoca el rol "ACT-04"
    Entonces las 3 misiones siguen en "EN_RUTA"
    Y se marcan para seguimiento por el puesto superior
    Y el sistema muestra "3 misiones EN_RUTA quedan atribuidas al puesto y visibles para el puesto superior. No se interrumpe ninguna operación en carretera."

  Escenario: Los actos pendientes de decisión quedan atribuidos al puesto
    Dadas 5 solicitudes de fondo pendientes de firma en ese puesto
    Cuando el Administrador del Sistema revoca el rol "ACT-04"
    Entonces las 5 solicitudes quedan atribuidas al puesto, no a "María López"
    Y quien ocupe el puesto con el rol restituido las ve al entrar
    Y si el puesto permanece sin el rol más allá del plazo parametrizado, escalan al puesto superior con registro diferenciado

  Escenario: La revocación por hallazgo notifica a Auditoría Interna
    Cuando el Administrador del Sistema revoca el rol con motivo tipificado "solicitud de Auditoría Interna"
    Entonces el sistema notifica al Auditor Interno y a la Gerencia Administrativa
    Y el asiento de revocación queda en la pista append-only y no puede ser alterado por el propio Administrador del Sistema

  Escenario: Un rol revocado se puede volver a otorgar y la vigencia queda partida
    Dado el rol "ACT-04" revocado el "2026-09-15"
    Cuando el Administrador del Sistema lo otorga de nuevo al mismo puesto desde el "2026-11-01"
    Entonces el sistema registra dos rangos de vigencia distintos y consultables
    Y no fusiona los rangos
    Y los actos del período intermedio no adquieren facultad retroactiva
```

## Fuera de alcance

- El cierre de la asignación de la persona al puesto — es [HU-136](HU-136-cerrar-asignacion-de-puesto-con-acta.md)
- La revocación de una delegación de firma — es [HU-135](HU-135-constituir-delegacion-de-firma-con-vigencia.md)
- El traspaso de custodias que la baja arrastra — es [HU-138](HU-138-traspaso-masivo-de-custodias-con-acta.md)

## Notas y pendientes

- `[C]` `plazo_escalamiento_por_puesto_sin_facultad` — parámetro con vigencia; no hay valor confirmado — insumo **#32**
- `[I]` Que la revocación no invalide lo ejecutado se toma de [actores-y-roles §7.1](../../01-negocio/actores-y-roles.md), que lo enuncia para la delegación de autoridad. **Extenderlo al otorgamiento de rol es inferencia del equipo**, no texto de la autoridad — conviene que el PO lo confirme
- `[P]` La inmutabilidad de lo autorizado se apoya en [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md); el articulado exacto no se pudo extraer — insumo **#23**
