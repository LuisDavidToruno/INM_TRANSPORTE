# HU-130 — Rechazar de entrada la acumulación de roles absolutamente incompatible, y vigilar la que solo lo es por misión

| Campo | Valor |
|---|---|
| **Módulo** | M-01 Organización y Seguridad |
| **Actor** | ACT-01 Administrador del Sistema |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — `I-18` e `I-19` están `[C]` y `RN-01` solo implementa hasta `I-11`; falta regla que gobierne el control preventivo |

## Historia

**Como** Administrador del Sistema
**quiero** que el sistema me impida otorgar a un puesto un rol que produzca en su ocupante una acumulación **absolutamente** incompatible, y que marque como *acumulación vigilada* la que solo es incompatible misión por misión
**para** no construir con dos clics el punto único de falla del control interno: la persona que puede otorgarse a sí misma cualquier facultad y borrar el rastro

## Contexto

El control de segregación opera en **dos momentos y los dos son necesarios** ([actores-y-roles §5.3](../../01-negocio/actores-y-roles.md), autoridad en incompatibilidades):

- **Preventivo, al asignar el rol.** Las incompatibilidades **absolutas y permanentes** —`I-12` auditor × cualquier rol ejecutor, `I-13` administrador × cualquier rol con facultad de autorizar, aprobar fondo o liquidar— **se rechazan aquí**, porque no dependen de ninguna misión concreta.
- **Bloqueante, al ejecutar el acto** sobre una misión. Es donde viven `I-01` a `I-11`, y lo cubre [HU-010](HU-010-bloqueo-de-segregacion-y-escalamiento.md).

No se pueden confundir. Prohibir de entrada que el Encargado de Delegación sea también Solicitante sería inoperante: casi todo puesto de la institución ostenta `ACT-02`. Pero permitir que el Administrador del Sistema tenga facultad de aprobar fondos no es un riesgo operativo, es la anulación del modelo entero: podría otorgarse cualquier cosa y borrar el rastro.

**Hoy nada implementa el control preventivo.** `RN-01` declara expresamente que implementa `I-01` a `I-11` — los pares por misión — y deja fuera `I-12` e `I-13`, que son los absolutos.

## Reglas que la gobiernan

- [RN-01](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md) — Segregación como bloqueo duro, no configurable. Implementa `I-01` a `I-11`; el control preventivo de `I-12` e `I-13` es el hueco que esta historia expone
- [RN-39](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md) — Enuncia `I-13` para el caso de parámetros: quien carga no aprueba, y `ACT-01` **no puede en ningún caso** ostentar la facultad de aprobar
- [RN-03](../../01-negocio/reglas/RN-03-registro-inmutable-de-autorizacion.md) — El intento rechazado se registra con identidad, momento y contenido
- [RN-04](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md) — El rechazo no borra nada: deja asiento del intento

## Casos especiales que la afectan

- Ninguno de los 28 vigentes. **Caso especial candidato:** *la delegación de tres personas donde el único que sabe de licencias es también el que conduce* — [actores-y-roles §5.2](../../01-negocio/actores-y-roles.md) lo señala como consecuencia operativa de `I-18` y lo deja sin resolver salvo por escalamiento a sede

## Criterios de aceptación

```gherkin
# language: es
Característica: Control preventivo de acumulación incompatible de roles

  Antecedentes:
    Dado un puesto "Analista de Informática" ocupado por "Carlos Fúnez" con el rol "ACT-01 Administrador del Sistema" vigente
    Y un puesto "Auditor Interno" ocupado por "Ana Zelaya" con el rol "ACT-12 Auditor Interno" vigente
    Y un puesto "Encargado de Transporte de la Delegación de Choluteca" ocupado por "María López"

  Escenario: Se rechaza dar al administrador del sistema facultad de aprobar fondo
    Cuando el Administrador del Sistema otorga el rol "ACT-08 Gerencia Administrativa" al puesto "Analista de Informática"
    Entonces el sistema rechaza el otorgamiento
    Y muestra "Carlos Fúnez ostenta ACT-01 Administrador del Sistema. La incompatibilidad I-13 es núcleo irreductible: el administrador no puede tener facultad de autorizar, aprobar fondo ni liquidar. No admite excepción, delegación ni emergencia."
    Y registra el intento en la pista de auditoría con el par "I-13", la persona, el puesto y el momento

  Escenario: Se rechaza en sentido inverso: dar el rol de administrador a quien ya aprueba fondos
    Dado un puesto "Gerente Administrativo" con el rol "ACT-08 Gerencia Administrativa" vigente
    Cuando el Administrador del Sistema otorga el rol "ACT-01 Administrador del Sistema" a ese puesto
    Entonces el sistema rechaza el otorgamiento
    Y muestra "El ocupante de Gerente Administrativo ostenta ACT-08 con facultad de aprobar fondo. La incompatibilidad I-13 es simétrica y es núcleo irreductible."

  Escenario: Se rechaza dar al auditor interno cualquier rol ejecutor
    Cuando el Administrador del Sistema otorga el rol "ACT-05 Encargado de Despacho" al puesto "Auditor Interno"
    Entonces el sistema rechaza el otorgamiento
    Y muestra "Ana Zelaya ostenta ACT-12 Auditor Interno. La incompatibilidad I-12 es núcleo irreductible: la independencia de la auditoría no admite excepción. Un auditor con capacidad de ejecutar deja de ser auditor."

  Escenario: La incompatibilidad se evalúa sobre la persona aunque los roles cuelguen de puestos distintos
    Dada una asignación vigente de "Carlos Fúnez" al puesto "Analista de Informática" con "ACT-01"
    Cuando el Administrador del Sistema asigna además a "Carlos Fúnez" al puesto "Gerente Administrativo", que ostenta "ACT-08"
    Entonces el sistema rechaza la asignación de puesto
    Y muestra "Carlos Fúnez acumularía ACT-01 y ACT-08 entre dos puestos. La incompatibilidad I-13 se evalúa sobre la persona, no sobre el puesto."

  Escenario: La acumulación incompatible solo por misión se permite y se marca como vigilada
    Cuando el Administrador del Sistema otorga los roles "ACT-02 Solicitante", "ACT-05 Encargado de Despacho" y "ACT-07 Encargado de Combustible" al puesto "Encargado de Transporte de la Delegación de Choluteca"
    Entonces el sistema acepta los otorgamientos
    Y marca el puesto como "DE ACUMULACIÓN VIGILADA" citando los pares "I-02", "I-03" y "I-08"
    Y muestra "Los pares I-02, I-03 e I-08 se evalúan por misión concreta y se bloquearán en el momento del acto, no aquí."
    Y el puesto aparece en el tablero de la Gerencia Administrativa y en el del Auditor Interno

  Escenario: El tablero de acumulación vigilada lista los puestos y sus pares
    Cuando la Gerencia Administrativa abre el tablero de acumulación vigilada
    Entonces ve el puesto "Encargado de Transporte de la Delegación de Choluteca" con sus tres pares
    Y ve la persona que lo ocupa y desde cuándo
    Y puede exportar el tablero como paquete de evidencia

  Escenario: No se ofrece continuar de todos modos ante un bloqueo duro
    Cuando el Administrador del Sistema recibe el rechazo por "I-13"
    Entonces el sistema no ofrece ninguna opción de continuar, forzar ni justificar
    Y la única salida ofrecida es otorgar el rol a un puesto distinto
```

## Fuera de alcance

- El bloqueo en el momento de ejecutar el acto sobre una misión — es [HU-010](HU-010-bloqueo-de-segregacion-y-escalamiento.md) y [HU-039](HU-039-segregacion-de-funciones-al-despachar.md)
- El bloqueo del doble control sobre parámetros — es [HU-146](HU-146-bloquear-que-quien-carga-apruebe-su-propia-carga.md)
- El régimen de excepción para delegaciones pequeñas: **no se implementa** ([DP-002](../../07-gestion/decisiones-de-producto/DP-002-segregacion-en-delegaciones-pequenas.md))

## Notas y pendientes

- `[P]` La exigencia de segregación de funciones está en [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md) como implicación de requerimiento; **su numeración normativa no está verificada** — insumo **#23**. Esta historia no cita ningún artículo
- `[I]` Que `I-12` e `I-13` se controlen **al otorgar** y no solo al ejecutar es diseño de [actores-y-roles §5.3](../../01-negocio/actores-y-roles.md), no articulado
- `[C]` `I-18` (habilita la licencia × es habilitado) e `I-19` (solicita el fondo × aprueba el fondo) están marcadas `[C]` en la autoridad. **Esta historia no las incluye en el control preventivo** hasta que Auditoría Interna se pronuncie — insumo **#26**
- **Regla candidata:** *El otorgamiento de un rol que produzca en una persona una acumulación absolutamente incompatible (`I-12`, `I-13`) se rechaza en el acto de otorgar; la acumulación incompatible solo por misión se admite y marca el puesto como de acumulación vigilada.* `RN-01` declara implementar `I-01` a `I-11` y **deja fuera los dos absolutos**
