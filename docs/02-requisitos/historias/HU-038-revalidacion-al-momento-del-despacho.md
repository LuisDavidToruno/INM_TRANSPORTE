# HU-038 — Revalidar al momento del despacho lo que se verificó al programar

| Campo | Valor |
|---|---|
| **Módulo** | M-07 Programación y Despacho |
| **Actor** | ACT-05 Encargado de Despacho · ACT-10 Encargado de Delegación en su ámbito |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Refinada |
| **Deriva de** | [CU-06](../casos-de-uso/CU-06-despachar-y-registrar-salida.md) pasos 2 y 3, E1, E2, E3 · `T-12` · `BD-02`, `BD-03`, `BD-04`, `BD-10` |

## Historia

**Como** Encargado de Despacho
**quiero** que el sistema vuelva a verificar licencia, documentación del vehículo, estado operativo, disponibilidad del motorista y permiso de circulación **contra el momento del despacho**, sin dar por buena la verificación de la programación
**para** que no salga a carretera un vehículo o un motorista que dejó de estar habilitado en los días que pasaron entre programar y salir

## Contexto

Entre la programación y la salida pueden pasar días. En esos días una licencia vence, un vehículo entra a taller por una falla, una incapacidad ingresa al sistema de Talento Humano o el permiso de circulación en día inhábil queda sin firmar. **Pasa, y pasa seguido.**

Dar por buena la verificación de la programación es exactamente el error que produce el hallazgo: el expediente muestra "verificado" con fecha de hace nueve días y el siniestro ocurre con la licencia vencida hace dos.

**No hay forma de forzarlo.** Ni por urgencia, ni por autorización superior, ni por régimen de delegación. El camino es sustituir el recurso o devolver la misión a la cola.

## Reglas que la gobiernan

- [RN-09](../../01-negocio/reglas/RN-09-matriz-licencia-vehiculo.md) · [RN-10](../../01-negocio/reglas/RN-10-licencia-vigente-en-todo-el-rango.md) · [RN-11](../../01-negocio/reglas/RN-11-restricciones-medicas-del-motorista.md) — `BD-02` revalidado al despachar
- [RN-19](../../01-negocio/reglas/RN-19-vehiculo-no-operativo-no-se-asigna.md) — El vehículo no operativo no se despacha
- [RN-16](../../01-negocio/reglas/RN-16-seguro-y-revision-mecanica.md) · [RN-62](../../01-negocio/reglas/RN-62-titulo-de-tenencia-con-vigencia-y-rubros.md) — `BD-03` al momento del despacho
- [RN-12](../../01-negocio/reglas/RN-12-disponibilidad-del-motorista.md) — `BD-10` contra el espejo, con su antigüedad registrada
- [RN-23](../../01-negocio/reglas/RN-23-permiso-de-circulacion-en-dia-inhabil.md) · [RN-24](../../01-negocio/reglas/RN-24-vehiculo-de-servicio-exceptuado.md) — `BD-04`: sin permiso vigente no hay emisión ni salida
- [RN-57](../../01-negocio/reglas/RN-57-habilitacion-de-quien-efectivamente-conduce.md) — Se revalida sobre quien efectivamente va a conducir ese día

## Casos especiales que la afectan

- [CE-11](../casos-especiales/CE-11-licencia-vence-durante-la-mision.md) — La licencia venció entre la programación y el despacho
- [CE-16](../casos-especiales/CE-16-vehiculo-a-taller-con-misiones-programadas.md) — El vehículo pasó a `EN_TALLER` después de programar
- [CE-13](../casos-especiales/CE-13-motorista-no-disponible-por-talento-humano.md) — La incapacidad ingresó después de programar
- [CE-01](../casos-especiales/CE-01-salida-de-emergencia-convalidada.md) — Ni la emergencia levanta estos bloqueos: se convalida el acto, no la habilitación

## Criterios de aceptación

```gherkin
# language: es
Característica: Revalidación de los bloqueos duros al momento del despacho

  Antecedentes:
    Dada una Orden de Misión "OM-2026-0451" en estado "PROGRAMADA", programada el "2026-09-01"
    Y una ventana del "2026-09-15" al "2026-09-17"
    Y un vehículo "Pickup Toyota Hilux" con correlativo "INS-P-014"
    Y un motorista "José Martínez" con licencia "01-1985-04321" categoría "C1"
    Y la fecha del despacho "2026-09-15"

  Escenario: Se rechaza el despacho porque la licencia venció después de programar
    Dado que la licencia "01-1985-04321" fue actualizada y su vencimiento es el "2026-09-14"
    Cuando el Encargado de Despacho inicia el despacho de "OM-2026-0451"
    Entonces el sistema rechaza el despacho
    Y muestra "La licencia 01-1985-04321 venció el 14/09/2026 y la misión retorna el 17/09/2026. Sustituya al motorista o devuelva la misión a la cola."
    Y no se emite ningún documento ni se consume el folio
    Y la misión permanece en estado "PROGRAMADA"
    Y registra el intento con el número de licencia, el vencimiento y el fin de rango evaluado

  Escenario: Se rechaza el despacho porque el vehículo pasó a taller
    Dado que el Encargado de Mantenimiento declaró el "INS-P-014" en estado "EN_TALLER"
      el "2026-09-13", con ventana estimada de indisponibilidad hasta el "2026-09-20"
    Cuando el Encargado de Despacho inicia el despacho de "OM-2026-0451"
    Entonces el sistema rechaza el despacho
    Y muestra "El vehículo INS-P-014 está EN_TALLER desde el 13/09/2026. Sustituya el vehículo o devuelva la misión a la cola."

  Escenario: Se rechaza el despacho porque la matrícula venció después de programar
    Dado que la matrícula del "INS-P-014" venció el "2026-09-10"
    Cuando el Encargado de Despacho inicia el despacho de "OM-2026-0451"
    Entonces el sistema rechaza el despacho
    Y muestra "La matrícula del vehículo INS-P-014 venció el 10/09/2026."

  Escenario: Se rechaza el despacho por permiso de circulación ausente en franja inhábil
    Dada una ventana del "2026-09-19 22:00" al "2026-09-20 14:00", que toca domingo y hora inhábil
    Y que no existe permiso de circulación vigente para ese vehículo y esa ventana
    Cuando el Encargado de Despacho inicia el despacho
    Entonces el sistema rechaza el despacho
    Y muestra "La ventana del 19/09/2026 22:00 al 20/09/2026 14:00 toca día y hora inhábil y no hay permiso vigente. Lo emite la Máxima Autoridad: la autorización de la jefatura no lo sustituye."
    Y no ofrece continuar de todos modos

  Escenario: El vehículo de servicio exceptuado no requiere permiso
    Dada una ventana que toca domingo
    Y que el "INS-P-014" está marcado como vehículo de servicio exceptuado,
      con fundamento y vigencia registrados
    Cuando el Encargado de Despacho inicia el despacho
    Entonces el sistema no exige permiso de circulación
    Y la Orden de Misión imprimirá el fundamento y la vigencia de la excepción

  Escenario: Se revalida sobre el conductor que efectivamente se presenta
    Dado que el motorista titular es "José Martínez" y hay un relevo declarado "Elder Zavala"
    Y que quien se presenta a conducir es "Elder Zavala"
    Cuando el Encargado de Despacho inicia el despacho declarando a "Elder Zavala" como conductor
    Entonces el sistema revalida licencia, vigencia y restricciones de "Elder Zavala"
    Y registra la verificación con sus datos concretos

  Escenario: Revalidación conforme, con registro nuevo
    Dado que licencia, matrícula, estado operativo y disponibilidad están conformes al "2026-09-15"
    Cuando el Encargado de Despacho inicia el despacho de "OM-2026-0451"
    Entonces el sistema autoriza continuar con el despacho
    Y registra una verificación nueva con fecha "2026-09-15", sin sobrescribir la de la programación
```

## Fuera de alcance

- La verificación equivalente al programar — es [HU-023](HU-023-documentacion-y-estado-operativo-del-vehiculo.md), [HU-025](HU-025-habilitacion-de-quien-efectivamente-conduce.md) y [HU-026](HU-026-disponibilidad-del-motorista-contra-el-espejo.md)
- La emisión del permiso de circulación en día inhábil — es del caso de uso [CU-03](../casos-de-uso/CU-03-permiso-de-circulacion-en-dia-inhabil.md)
- La sustitución del recurso que falló la revalidación — es [HU-043](HU-043-sustituir-vehiculo-o-motorista-en-programada.md)
- La segregación de funciones al despachar — es [HU-039](HU-039-segregacion-de-funciones-al-despachar.md)

## Notas y pendientes

- **Divergencia registrada, no resuelta aquí:** `BD-04` de la [máquina de estados](../../03-arquitectura/estados/orden-de-mision.md) exige permiso vigente *"para esa ventana y ese vehículo"*; `PC-03` de `PR-01` exige *"vehículo, motorista y ventana"*. **Esta historia sigue a `BD-04`**, que es la autoridad en precondiciones. Si el salvoconducto amparara también al motorista, un relevo en ruta lo invalidaría — y eso cambia [HU-045](HU-045-relevo-de-motorista-en-ruta.md).
- `[C]` **Horario hábil oficial de la institución** y **legislación de feriados** — insumo #32 y [NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md). El calendario es parámetro con vigencia, no una constante.
- `[C]` **¿Es delegable la firma del permiso de circulación?** — insumo #29. Hasta confirmarlo, **el sistema no lo permite**.
