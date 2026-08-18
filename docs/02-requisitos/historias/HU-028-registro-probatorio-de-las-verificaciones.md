# HU-028 — Registrar cada verificación con los datos concretos contra los que se evaluó

| Campo | Valor |
|---|---|
| **Módulo** | M-07 Programación y Despacho · M-14 Reportes, Indicadores y Auditoría |
| **Actor** | ACT-04 Jefe de Transporte (produce el registro) · ACT-12 Auditor Interno (lo consulta) |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Refinada |
| **Deriva de** | [CU-04](../casos-de-uso/CU-04-programar-mision-asignar-vehiculo-y-motorista.md) paso 11 · `T-08`, `T-10`, `T-12` · §9.2 de la [máquina de estados](../../03-arquitectura/estados/orden-de-mision.md) |

## Historia

**Como** Jefe de Transporte
**quiero** que el sistema guarde, por cada verificación de la asignación, **los datos concretos contra los que evaluó** —número de licencia, categoría, vencimiento, versión de la matriz, atributos del vehículo, vencimientos consultados y fecha de fin de rango— y no solo el veredicto
**para** poder demostrar dos años después, ante un siniestro o ante el Tribunal Superior de Cuentas, con qué información y con qué tabla vigente se autorizó esa asignación

## Contexto

Guardar `verificado: sí` no defiende a nadie. Cuando ocurre un accidente y la licencia del motorista aparece vencida, la pregunta del auditor no es si el sistema verificó: es **contra qué verificó**. Si la matriz licencia↔vehículo cambió en el ínterin, si el vencimiento que tenía capturado el expediente era otro, si el fin de rango evaluado incluía o no la holgura — todo eso decide si quien autorizó respondió con diligencia o no.

Este registro es, literalmente, **la defensa de quien autorizó la misión**. Y por eso no se edita, no se borra y sobrevive al cierre del expediente ([`RN-05`](../../01-negocio/reglas/RN-05-registro-cerrado-no-se-edita.md)).

Lo mismo aplica al **intento rechazado**: un bloqueo que no deja rastro es un bloqueo que nadie puede acreditar que existió.

## Reglas que la gobiernan

- [RN-03](../../01-negocio/reglas/RN-03-registro-inmutable-de-autorizacion.md) — Toda autorización se registra de forma inmutable con identidad, rol, momento, origen y huella del contenido
- [RN-05](../../01-negocio/reglas/RN-05-registro-cerrado-no-se-edita.md) — Un registro cerrado no se edita
- [RN-04](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md) — Nada se borra: toda corrección es asiento reverso con motivo y autor
- [RN-40](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md) — La evaluación usa el parámetro vigente a la fecha del hecho, y esa versión se registra
- [RN-14](../../01-negocio/reglas/RN-14-sustitucion-de-motorista.md) — La sustitución revalida y **conserva la asignación original** en el diario

## Casos especiales que la afectan

- [CE-11](../casos-especiales/CE-11-licencia-vence-durante-la-mision.md) — Sin registro del fin de rango evaluado no se puede sostener que la verificación fue correcta
- [CE-28](../casos-especiales/CE-28-hallazgo-posterior-sobre-mision-cerrada.md) — El hallazgo llega meses después: el registro debe ser reproducible a la fecha del hecho
- [CE-13](../casos-especiales/CE-13-motorista-no-disponible-por-talento-humano.md) — La antigüedad del espejo consultado forma parte de la evidencia

## Criterios de aceptación

```gherkin
# language: es
Característica: Registro probatorio de las verificaciones de la asignación

  Antecedentes:
    Dada una Orden de Misión "OM-2026-0451" con ventana del "2026-09-10" al "2026-09-12"
      y holgura posterior de "1" día
    Y un vehículo "Camión Isuzu FVR" con correlativo "INS-C-002", peso bruto "12000" kg
    Y un motorista "José Martínez" con licencia "01-1985-04321" categoría "C"
      vigente hasta el "2027-03-15"
    Y una matriz licencia↔vehículo en su versión "MLV-2026-02" vigente desde el "2026-06-01"

  Escenario: El registro de una verificación exitosa contiene los datos evaluados
    Cuando el Jefe de Transporte programa "OM-2026-0451" con ese vehículo y ese motorista
    Entonces el diario de la misión registra la verificación de licencia con:
      | dato                          | valor          |
      | numero_de_licencia            | 01-1985-04321  |
      | categoria                     | C              |
      | vencimiento                   | 2027-03-15     |
      | version_matriz                | MLV-2026-02    |
      | peso_bruto_evaluado_kg        | 12000          |
      | fin_de_rango_evaluado         | 2026-09-13     |
      | resultado                     | HABILITADO     |
    Y registra al Jefe de Transporte como autor, con su rol ejercido y la marca de tiempo
    Y el registro no es editable por ningún rol

  Escenario: El intento rechazado también deja registro con sus datos
    Dado un motorista "Marvin Discua" con licencia "05-1990-11987" categoría "B"
      vigente hasta el "2028-01-20"
    Cuando el Jefe de Transporte intenta asignar a "Marvin Discua" al "INS-C-002"
    Entonces el sistema rechaza la asignación
    Y el diario registra el intento con categoría evaluada "B", peso bruto "12000" kg,
      versión de matriz "MLV-2026-02" y resultado "NO_HABILITADO"
    Y registra el motivo concreto "categoría insuficiente para el peso bruto del vehículo"

  Escenario: Se rechaza cualquier edición del registro de verificación
    Dado el registro de verificación de "OM-2026-0451" ya asentado
    Cuando el Jefe de Transporte intenta corregir el número de licencia registrado
    Entonces el sistema rechaza la modificación
    Y muestra "El registro de verificación no se edita. Si el dato del expediente era incorrecto, corríjalo en el expediente del motorista: la verificación queda como se hizo."

  Escenario: La revalidación al despachar produce un registro nuevo, no reemplaza el anterior
    Dado que "OM-2026-0451" se programó el "2026-09-05"
    Cuando el Encargado de Despacho despacha la misión el "2026-09-10"
    Entonces existen dos registros de verificación de licencia para la misión
    Y cada uno conserva su momento, su autor y la versión de matriz usada
    Y el registro de la programación no se sobrescribe

  Escenario: La sustitución conserva la asignación original en el diario
    Dado que "OM-2026-0451" tenía asignado a "José Martínez"
    Cuando el Jefe de Transporte sustituye al motorista por "Elder Zavala"
      con motivo "motorista no disponible"
    Entonces el diario muestra a "José Martínez" como asignación original,
      el motivo del cambio y a "Elder Zavala" como asignación vigente
    Y conserva el registro de verificación de ambos

  Escenario: El Auditor Interno consulta el registro y la consulta queda asentada
    Cuando el Auditor Interno consulta las verificaciones de "OM-2026-0451"
    Entonces el sistema muestra el registro completo en modo de solo lectura
    Y asienta quién consultó, qué expediente y cuándo
```

## Fuera de alcance

- El diseño del almacenamiento inmutable y del encadenamiento por huella — es [`RNF-04`](../no-funcionales/RNF-04-bitacora-append-only-con-hash-encadenado.md)
- Los reportes de auditoría construidos sobre estos registros — son de M-14
- El expediente de hallazgo posterior sobre una misión cerrada — es de M-14 y [`RN-93`](../../01-negocio/reglas/RN-93-expediente-de-hallazgo-posterior.md)

## Notas y pendientes

- `[P]` La exigencia de conservar los insumos de la verificación se apoya en [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md); el articulado concreto no se pudo extraer.
- `[I]` La lista de campos registrados es la deducida por el equipo a partir de los bloqueos `BD-02`, `BD-03`, `BD-07`, `BD-10` y `BD-11`. **No** proviene de un formato oficial: si el insumo #2 aporta el formato en papel de la institución, la lista se contrasta contra él.
