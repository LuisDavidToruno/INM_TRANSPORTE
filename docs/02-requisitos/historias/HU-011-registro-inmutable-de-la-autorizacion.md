# HU-011 — Registrar la autorización de forma inmutable y aprobar sin comprometer flota

| Campo | Valor |
|---|---|
| **Módulo** | M-06 Solicitudes de Transporte |
| **Actor** | ACT-03 Jefatura Inmediata |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Refinada |

## Historia

**Como** Jefatura Inmediata
**quiero** autorizar la solicitud y que el acto quede registrado con mi identidad, mi puesto, el rol que ejercí en ese momento, la marca de tiempo, el origen y la huella del contenido que autoricé
**para** poder demostrar ante el Tribunal Superior de Cuentas exactamente qué autoricé y cuándo, y que nadie pueda cambiar después lo que quedó amparado por mi firma

## Contexto

No hay firma electrónica certificada en este sistema: la autorización es **interna**, con usuario autenticado y registro completo de quién, cuándo, desde dónde y **sobre qué contenido** ([DP-001](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) D-04). Lo que sustituye a la firma es la calidad del registro.

Dos detalles que parecen menores y no lo son. El **rol ejercido se guarda como copia, no como referencia**: si la persona cambia de puesto el año siguiente, el expediente debe seguir diciendo con qué facultad autorizó ese día. Y la **huella del contenido** es lo que impide que la autorización ampare algo distinto de lo que se envió.

Tercer punto, contraintuitivo pero central: **aprobar no compromete flota**. No se reserva vehículo ni motorista. La aprobación resuelve la procedencia de la necesidad; la disponibilidad de recursos es del Jefe de Transporte al programar. Por eso la aprobación tiene **fecha de caducidad**: una aprobación que nadie programó antes del inicio de la ventana caduca y se anula con motivo tipificado.

## Reglas que la gobiernan

- [RN-03](../../01-negocio/reglas/RN-03-registro-inmutable-de-autorizacion.md) — Identidad, puesto, rol ejercido, momento, origen y huella del contenido autorizado
- [RN-06](../../01-negocio/reglas/RN-06-transiciones-de-estado-de-la-orden.md) — `T-05` es la transición de `SOLICITADA` a `APROBADA`, con actor, rol, momento y motivo
- [RN-04](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md) — Una autorización registrada no se borra: se revierte con asiento reverso
- [RN-05](../../01-negocio/reglas/RN-05-registro-cerrado-no-se-edita.md) — El acto de autorización no se edita
- [RN-41](../../01-negocio/reglas/RN-41-congelamiento-del-valor-al-autorizar.md) — Los valores estimados quedan congelados con el identificador de la tabla usada
- [RN-43](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md) — El acto puede ejecutarse sin conectividad, con código de autorización de un solo uso
- [RN-23](../../01-negocio/reglas/RN-23-permiso-de-circulacion-en-dia-inhabil.md) — La marca de franja inhábil se conserva en `APROBADA` y dispara el trámite del permiso

## Casos especiales que la afectan

- Ninguno se materializa en el acto de autorizar. Constancia dejada: [CE-11](../casos-especiales/CE-11-licencia-vence-durante-la-mision.md), [CE-13](../casos-especiales/CE-13-motorista-no-disponible-por-talento-humano.md) y [CE-16](../casos-especiales/CE-16-vehiculo-a-taller-con-misiones-programadas.md) quedan **descartados explícitamente** porque la aprobación no reserva recursos (`INV-11`)

## Criterios de aceptación

```gherkin
# language: es
Característica: Registro del acto de autorización
  Como Jefatura Inmediata
  quiero que mi autorización quede registrada de forma inmutable
  para poder demostrar qué autoricé y cuándo

  Antecedentes:
    Dado un expediente "CHO-2026-00087" en estado "SOLICITADA" con ventana del "2026-03-20 07:00" al "2026-03-20 19:00"
    Y una Jefatura Inmediata "Rolando Discua", Subgerente de Operaciones, con rol de autorizador vigente
    Y una fecha del sistema del "2026-03-14 09:30"

  Escenario: Se rechaza autorizar un expediente cuyo contenido ya no coincide con su huella
    Dado un expediente cuya huella congelada no coincide con el contenido actual
    Cuando "Rolando Discua" intenta autorizar el expediente "CHO-2026-00087"
    Entonces el sistema no ejecuta la autorización
    Y muestra "El contenido del expediente no coincide con la huella congelada al envío. No se autoriza un contenido alterado; devuelva el expediente a borrador (T-04)."

  Escenario: Se rechaza editar la autorización ya registrada
    Dado un expediente en estado "APROBADA" autorizado por "Rolando Discua" el "2026-03-14 09:30"
    Cuando "Rolando Discua" intenta modificar el motivo registrado en su autorización
    Entonces el sistema rechaza la modificación
    Y muestra "La autorización registrada no se edita. Toda corrección es un asiento reverso con motivo y autor (RN-04)."

  Escenario: Se rechaza el código de autorización fuera de línea usado por segunda vez
    Dado un código de autorización fuera de línea emitido para el expediente "CHO-2026-00087" y la transición "T-05"
    Y ese código ya consumido el "2026-03-14 09:30"
    Cuando el Encargado de Delegación intenta usar el mismo código nuevamente
    Entonces el sistema rechaza el código
    Y muestra "El código ya fue utilizado el 14/03/2026 09:30 sobre este mismo expediente. Es de un solo uso y no transferible."

  Escenario: Se rechaza el código de autorización emitido para otro expediente
    Dado un código de autorización fuera de línea emitido para el expediente "CHO-2026-00090"
    Cuando el Encargado de Delegación intenta usarlo sobre el expediente "CHO-2026-00087"
    Entonces el sistema rechaza el código
    Y muestra "El código corresponde al expediente CHO-2026-00090. No es válido para CHO-2026-00087."

  Escenario: La autorización registra rol ejercido, origen y huella
    Cuando "Rolando Discua" autoriza el expediente "CHO-2026-00087"
    Entonces el sistema registra la identidad "Rolando Discua", el puesto "Subgerente de Operaciones" y el rol ejercido "ACT-03 Jefatura Inmediata" como copia
    Y registra la marca de tiempo del hecho "2026-03-14 09:30" y la de captura
    Y registra el dispositivo desde el que se ejecutó el acto
    Y registra la huella del contenido autorizado

  Escenario: La aprobación no reserva vehículo ni motorista
    Cuando "Rolando Discua" autoriza el expediente "CHO-2026-00087"
    Entonces el expediente pasa a estado "APROBADA"
    Y no existe ninguna reserva de vehículo para la ventana del "2026-03-20"
    Y no existe ninguna reserva de motorista para esa ventana
    Y el expediente entra en la cola de programación del Jefe de Transporte

  Escenario: La aprobación caduca si no se programa antes del inicio de la ventana
    Dado un expediente en estado "APROBADA" con ventana que inicia el "2026-03-20 07:00"
    Y ninguna programación registrada al "2026-03-20 07:01"
    Cuando el sistema evalúa la caducidad de las aprobaciones
    Entonces el expediente queda señalado como aprobación caducada
    Y muestra al Jefe de Transporte "La aprobación de CHO-2026-00087 caducó: la ventana inició el 20/03/2026 07:00 sin programación. Anúlela con motivo tipificado (T-09)."

  Escenario: La marca de franja inhábil sobrevive a la aprobación
    Dado un expediente con la marca "REQUIERE_PERMISO_MAXIMA_AUTORIDAD"
    Cuando "Rolando Discua" autoriza el expediente
    Entonces el expediente pasa a estado "APROBADA"
    Y conserva la marca "REQUIERE_PERMISO_MAXIMA_AUTORIDAD"
    Y la marca queda visible en la bandeja de Transporte
```

## Fuera de alcance

- El bloqueo de segregación y el escalamiento — es [HU-010](HU-010-bloqueo-de-segregacion-y-escalamiento.md), que se evalúa **antes** de registrar nada
- La autorización de varios niveles — es [HU-012](HU-012-autorizacion-de-varios-niveles.md)
- La **generación** del código de autorización fuera de línea por el autorizador competente en la sede y su transmisión: el canal —llamada, radio, mensaje— **no forma parte del sistema**
- La anulación posterior de la aprobación (`T-09`) — es de M-07, al programar

## Notas y pendientes

- `[C]` Canal operativo real, longitud del código de autorización fuera de línea y ventana de validez — insumo #1
- `[C]` Habilitación del **modo delegación desconectada** para autorizar sin red — insumo #41. Es decisión del PO: hasta que se tome, los escenarios de código fuera de línea quedan implementables pero desactivados por parámetro
- `[P]` La exigencia de registrar la autorización con servidor competente e inalterable proviene de [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md); la ausencia de firma electrónica certificada, de [NRM-08](../../01-negocio/normativa/NRM-08-firma-electronica.md)
- Trazabilidad: [CU-02](../casos-de-uso/CU-02-autorizar-solicitud-de-transporte.md) pasos 8 a 11 y flujo alterno A5; transición `T-05`; invariantes `INV-09`, `INV-11`; punto de control `PC-16`
