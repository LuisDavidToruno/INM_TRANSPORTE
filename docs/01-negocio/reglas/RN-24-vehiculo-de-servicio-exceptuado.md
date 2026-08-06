# RN-24 — La excepción de circulación en día inhábil es un atributo del vehículo, con fundamento y vigencia registrados

| Campo | Valor |
|---|---|
| **Módulos** | M-03, M-04, M-07 |
| **Origen** | Norma [NRM-02](../normativa/NRM-02-bienes-del-estado.md) — servicios exceptuados |
| **Verificación** | `[V]` que existen servicios exceptuados (emergencia, seguridad, defensa, salud, CONAPREMM) — `[C]` si la institución tiene vehículos bajo alguna excepción |
| **Tipo** | Derivación (habilita la excepción a [RN-23](RN-23-permiso-de-circulacion-en-dia-inhabil.md)) |
| **Configurable** | Sí — catálogo `tipo_servicio_exceptuado` con vigencia; atributo por vehículo |

## Enunciado

Un vehículo **puede** circular en día u hora inhábil sin permiso de la máxima autoridad **únicamente** si tiene registrado un **servicio exceptuado vigente**, con:

1. Tipo de servicio del catálogo — emergencia, seguridad, defensa, salud, u otro reconocido `[V]`
2. **Fundamento documental** de la excepción, con adjunto
3. **Rango de vigencia** explícito
4. Servidor que la registró y quién la autorizó

La excepción es **atributo del vehículo, no del viaje**. No se concede por misión ni por urgencia declarada.

## Justificación

[NRM-02](../normativa/NRM-02-bienes-del-estado.md) `[V]`: están exceptuados los servicios públicos esenciales, emergencia, seguridad, defensa, salud e integrantes de CONAPREMM. Y la propia ficha lo formula como pregunta abierta con la respuesta ya implícita: *"¿La institución tiene vehículos bajo alguna excepción de circulación? Es un atributo del vehículo, no del viaje."*

Si la excepción se declarara por viaje, cualquier misión podría autoexceptuarse alegando urgencia, y el control de [RN-23](RN-23-permiso-de-circulacion-en-dia-inhabil.md) se vaciaría en una semana.

## Condiciones de aplicación

Aplica solo a la excepción del permiso de circulación en día u hora inhábil.

**No exime** de ninguna otra regla: el vehículo exceptuado sigue sujeto a matriz licencia ↔ vehículo, estado operativo, custodia, bitácora, combustible y peajes.

`[C]` La institución debe declarar si tiene vehículos exceptuados y bajo qué fundamento. **Mientras no lo declare, ningún vehículo está exceptuado** — el valor por defecto es *no exceptuado*.

## Comportamiento esperado

1. Al evaluar una ventana inhábil, el sistema comprueba si el vehículo tiene excepción vigente **a la fecha del hecho**. Si la tiene, permite la programación sin permiso y lo deja registrado con el fundamento aplicado.
2. La orden de misión impresa **muestra la excepción invocada y su fundamento**, para que el control en carretera lo pueda leer. Un vehículo exceptuado sin nada que mostrar en un operativo del TSC está en la misma posición práctica que uno sin permiso.
3. La excepción vencida deja de aplicar sin aviso previo al despacho, por eso alimenta las alertas de [RN-17](RN-17-alertas-de-vencimiento-documental.md).
4. El sistema reporta **todas las circulaciones en día inhábil amparadas en excepción**, por vehículo y período, para revisión de ACT-12 Auditor Interno. Una excepción es una autorización permanente: exige vigilancia proporcional.
5. Registrar o modificar una excepción es un acto autorizado por ACT-09 o por quien la institución designe `[C]`, con el asiento inmutable de [RN-03](RN-03-registro-inmutable-de-autorizacion.md).

## Casos límite

- **Vehículo administrativo usado ocasionalmente en emergencia.** No convierte al vehículo en exceptuado. La salida correcta es el permiso sobreviniente de [RN-23](RN-23-permiso-de-circulacion-en-dia-inhabil.md), o el cierre con hallazgo. La tentación de marcar el vehículo como exceptuado "por si acaso" es exactamente lo que hay que impedir.
- **Excepción por institución completa.** Una institución de salud o de seguridad podría alegar que toda su flota está exceptuada. Se modela igual: atributo por vehículo, cargado masivamente, con el fundamento común. Nunca como interruptor global sin fundamento por unidad.
- **Excepción invocada y misión que resulta ser administrativa** — el vehículo de emergencia usado para un trámite en día domingo. La excepción cubre la circulación, pero el **motivo de la misión** sigue sujeto a la prohibición de uso para tareas ajenas a la función `[V]`. El sistema no puede juzgar el motivo, pero sí debe exigirlo declarado y reportarlo. Es un hallazgo típico.
- **Excepción sin fundamento adjunto.** Se admite registrarla con adjunto pendiente y plazo configurable, tras el cual se suspende — mismo tratamiento que la delegación en [RN-07](RN-07-delegacion-de-autorizacion.md).
- **Fundamento que expira por cambio de administración.** Tras rotación, las excepciones deben revalidarse. `[C]` confirmar si la institución quiere caducidad automática al cambio de máxima autoridad; es una salvaguarda razonable pero no es norma.
- **Vehículo exceptuado prestado a otra dependencia** para uso administrativo. La excepción sigue al vehículo, lo que puede producir circulación inhábil legítima para un uso que no lo es. Se mitiga con el reporte de circulaciones amparadas.

## Trazabilidad

- Norma: [NRM-02 — Bienes del Estado](../normativa/NRM-02-bienes-del-estado.md)
- Reglas relacionadas: [RN-23](RN-23-permiso-de-circulacion-en-dia-inhabil.md), [RN-25](RN-25-salvoconducto-con-folio-y-qr.md), [RN-17](RN-17-alertas-de-vencimiento-documental.md), [RN-38](RN-38-exoneracion-de-peaje.md)
- Actores: ACT-01, ACT-04, ACT-09, ACT-12
- Historias y casos especiales: pendientes — Bloque 2
