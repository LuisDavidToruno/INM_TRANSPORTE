# HU-140 — Consultar el padrón de personas como espejo de Talento Humano, sin poder editarlo y sabiendo cuán viejo está

| Campo | Valor |
|---|---|
| **Módulo** | M-01 Organización y Seguridad · M-20 Integraciones |
| **Actor** | ACT-01 Administrador del Sistema |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — sin el contrato de API de Talento Humano (insumo #17) los criterios de latencia y de eventos son provisionales |

## Historia

**Como** Administrador del Sistema
**quiero** consultar las personas de la institución como espejo de Talento Humano, sin ninguna pantalla que permita editarlas, viendo en cada una cuándo se sincronizó por última vez
**para** no crear un segundo padrón que diverja del oficial, y para que nadie corrija en SIGTI un dato que mañana la sincronización va a sobrescribir

## Contexto

[ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md) fija la frontera: **el empleado pertenece a Talento Humano; su rol como motorista dentro de la flota pertenece a SIGTI.** SIGTI **no crea personas**: las espeja.

Y hay una corrección del propio ADR que hay que respetar aquí, porque es contraintuitiva: **la licencia de conducir es dato PROPIO de SIGTI, no espejo.** La razón es que sobre ella se sostiene un bloqueo duro de valor legal, y *"un control de esta criticidad no puede depender del modelo de datos de un sistema ajeno que no tiene motivo para mantenerlo"*. Consecuencia operativa que hay que decir de frente: alguien de la institución **tiene que capturar y mantener las licencias dentro de SIGTI**.

El riesgo real de este patrón no es que se caiga: es que **diverja en silencio**. *"Un motorista que Talento Humano dio de baja pero que SIGTI sigue asignando a misiones no es un problema técnico: es un problema legal."*

## Reglas que la gobiernan

- [RN-48](../../01-negocio/reglas/RN-48-datos-espejo-de-solo-lectura.md) — Los datos de ARGOS y Talento Humano son espejo de solo lectura y no se editan desde SIGTI
- [RN-49](../../01-negocio/reglas/RN-49-reconciliacion-periodica-del-espejo.md) — El espejo se reconcilia periódicamente y cada entidad muestra su última sincronización
- [RN-50](../../01-negocio/reglas/RN-50-degradacion-por-sincronizacion-detenida.md) — Si la sincronización lleva detenida más del umbral, el sistema degrada explícitamente antes de operar
- [RN-12](../../01-negocio/reglas/RN-12-disponibilidad-del-motorista.md) — Permisos, vacaciones e incapacidades se leen del espejo, no se declaran en SIGTI
- [RN-45](../../01-negocio/reglas/RN-45-cero-sobrescritura-silenciosa.md) — Ningún conflicto se resuelve por sobrescritura silenciosa

## Casos especiales que la afectan

- [CE-13](../casos-especiales/CE-13-motorista-no-disponible-por-talento-humano.md) — La no disponibilidad llega del espejo y bloquea la asignación
- [CE-10](../casos-especiales/CE-10-motorista-incapacitado-en-ruta.md) — La incapacidad ocurre con la misión en curso

## Criterios de aceptación

```gherkin
# language: es
Característica: Espejo de personas de Talento Humano, de solo lectura

  Antecedentes:
    Dada una persona "María López" espejada de Talento Humano con última sincronización el "2026-09-14 03:10"
    Y un parámetro "umbral_degradacion_sincronizacion_talento_humano" de "48" horas vigente y aprobado

  Escenario: Se rechaza editar cualquier dato espejado
    Cuando el Administrador del Sistema intenta corregir el nombre de "María López"
    Entonces el sistema rechaza la edición
    Y muestra "El nombre de María López pertenece a Talento Humano y no se edita desde SIGTI. Corrija en el sistema origen; el cambio llegará por sincronización."

  Escenario: Se rechaza dar de alta una persona que no existe en el espejo
    Cuando el Administrador del Sistema intenta crear a la persona "Carlos Fúnez"
    Entonces el sistema rechaza el alta
    Y muestra "SIGTI no crea personas. Carlos Fúnez debe existir en Talento Humano. Si ya tiene alta, solicite la reconciliación del espejo."

  Escenario: Se rechaza declarar en SIGTI una incapacidad o unas vacaciones
    Cuando el Administrador del Sistema intenta registrar una incapacidad de "María López" del "2026-10-01" al "2026-10-14"
    Entonces el sistema rechaza el registro
    Y muestra "Permisos, vacaciones e incapacidades pertenecen a Talento Humano. Regístrelo allí; SIGTI lo consumirá por sincronización."

  Escenario: La sincronización detenida más del umbral degrada antes de operar
    Dada una última sincronización el "2026-09-14 03:10"
    Cuando el Jefe de Transporte intenta asignar un motorista el "2026-09-17 08:00"
    Entonces el sistema advierte "La sincronización con Talento Humano lleva 76 horas detenida y el umbral es de 48. Una baja, una incapacidad o unas vacaciones registradas después del 14/09/2026 03:10 no se ven aquí."
    Y exige acuse explícito antes de permitir la asignación
    Y registra el acuse con usuario, puesto y momento

  Escenario: La licencia de conducir sí se captura en SIGTI
    Cuando el Jefe de Transporte registra la licencia de "José Martínez" categoría "C1" con vencimiento "2027-03-15" y escaneo adjunto
    Entonces el sistema acepta el registro
    Y marca el dato como propio de SIGTI, no espejado
    Y muestra "La licencia es dato propio de SIGTI. Su mantenimiento y su alerta de vencimiento son responsabilidad de la institución."

  Escenario: Cada persona muestra la antigüedad de su sincronización
    Cuando el Administrador del Sistema consulta el padrón
    Entonces cada persona muestra la fecha y hora de su última sincronización
    Y las que superan el umbral aparecen señaladas
    Y el sistema no presenta ninguna persona como al día sin decir de cuándo es el dato

  Escenario: La divergencia detectada en la reconciliación va a cola de resolución, no se sobrescribe
    Dada una reconciliación completa ejecutada el "2026-09-15 02:00"
    Cuando detecta que "Ramón Cáceres" figura de baja en Talento Humano y activo en el espejo
    Entonces el sistema registra la divergencia en la cola de resolución
    Y notifica al Administrador del Sistema y a la Gerencia Administrativa
    Y muestra "Ramón Cáceres: baja en Talento Humano el 31/08/2026, activo en el espejo con 2 misiones programadas. Resuelva antes de despachar."
    Y no cierra las asignaciones de puesto de forma automática y silenciosa
```

## Fuera de alcance

- El mecanismo de sincronización y la cola de conflictos — es [HU-069](HU-069-el-espejo-nunca-diverge-en-silencio.md) y [HU-068](HU-068-cola-de-conflictos-dos-versiones-lado-a-lado.md)
- El expediente de habilitación del motorista — pertenece a M-05, [CU-18](../casos-de-uso/CU-18-registrar-y-mantener-la-habilitacion-del-motorista.md)
- La estructura organizativa espejada de ARGOS — es [HU-126](HU-126-estructura-institucional-con-vigencia.md)
- El stack y el protocolo de integración: `ADR-000` difiere la tecnología al Sprint 2

## Notas y pendientes

- `[C]` **Contrato de API de Talento Humano**: qué campos entrega, qué eventos emite y con qué latencia — insumo **#17**. Sin él, `umbral_degradacion_sincronizacion_talento_humano` no tiene valor defendible
- `[C]` **¿Talento Humano administra la licencia de conducir?** Si al obtener el contrato resulta que mantiene la categoría con el detalle requerido, [ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md) admite reconsiderar y espejearla. **Hasta entonces es propia** — insumo **#I de `actores-y-roles`**
- `[C]` El "48" horas del criterio es dato de prueba, no umbral confirmado — insumo **#68**
- `[I]` Que el acuse de degradación sea explícito y registrado es mitigación obligatoria de [ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md), no exigencia normativa
