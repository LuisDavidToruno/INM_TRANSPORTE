# HU-139 — Que el asiento de hace tres años siga diciendo el nombre y el puesto de entonces, pase lo que pase

| Campo | Valor |
|---|---|
| **Módulo** | M-01 Organización y Seguridad · M-14 Reportes, Indicadores y Auditoría |
| **Actor** | ACT-12 Auditor Interno |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — necesita la regla candidata de congelamiento de persona y puesto en el asiento |

## Historia

**Como** Auditor Interno
**quiero** que cada asiento muestre **la persona que actuó y el puesto que ocupaba en ese momento**, congelados, y que ningún ascenso, traslado, reorganización o baja los modifique
**para** poder responder *"¿quién autorizó esto y con qué competencia?"* con un dato que no cambia según cuándo se haga la pregunta

## Contexto

Es el punto 4 de [`RNF-15`](../no-funcionales/RNF-15-continuidad-ante-rotacion-de-personal.md), **el que se rompe con más frecuencia y el más caro**: *"si el nombre del autor se resuelve mirando el puesto actual de esa persona, el día que alguien asciende cambia la historia entera de sus asientos."*

Por qué se guardan **los dos** y no solo el nombre ([actores-y-roles §2.4](../../01-negocio/actores-y-roles.md), autoridad): *"cuando el auditor pregunta '¿quién autorizó esto y con qué competencia?', el nombre solo no responde. La competencia estaba en el puesto, y el puesto pudo haber cambiado de titular tres veces desde entonces."*

El daño de no cumplirlo es **irreversible**: un registro de auditoría que cambia con el organigrama no acredita nada, y no hay forma de reconstruirlo hacia atrás. En ese punto el sistema deja de ser la defensa de la institución ante el TSC y se convierte en el instrumento del hallazgo.

Y hay una consecuencia práctica que hay que sostener: **el usuario nunca se elimina**. Se desactiva. Un asiento firmado por un identificador vacío es un asiento sin autor.

## Reglas que la gobiernan

- [RN-03](../../01-negocio/reglas/RN-03-registro-inmutable-de-autorizacion.md) — Toda autorización se registra de forma inmutable con identidad, rol, momento, origen y huella del contenido
- [RN-04](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md) — Nada se borra; toda corrección es asiento reverso con motivo y autor
- [RN-05](../../01-negocio/reglas/RN-05-registro-cerrado-no-se-edita.md) — Un registro cerrado no se edita, ni siquiera por el Administrador del Sistema
- [RN-94](../../01-negocio/reglas/RN-94-fecha-de-corte-de-conocimiento-en-todo-reporte.md) — Todo reporte declara su fecha de corte y es reproducible a esa fecha
- [RN-01](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md) — La matriz de segregación se evalúa contra la identidad congelada de quien actuó

## Casos especiales que la afectan

- [CE-28](../casos-especiales/CE-28-hallazgo-posterior-sobre-mision-cerrada.md) — El hallazgo posterior interroga un expediente terminal y necesita la autoría intacta
- [CE-27](../casos-especiales/CE-27-cierre-de-ejercicio-fiscal-con-hallazgo-abierto.md) — El reporte del ejercicio anterior debe reproducirse idéntico

## Criterios de aceptación

```gherkin
# language: es
Característica: Inmutabilidad de la autoría histórica

  Antecedentes:
    Dada una Orden de Misión "OM-2025-0188" autorizada por "Ramón Cáceres" el "2025-06-12"
    Y que el "2025-06-12" "Ramón Cáceres" ocupaba el puesto "Coordinador de la Unidad de Trámites"
    Y una consulta del asiento realizada el "2025-07-01" que muestra ese nombre y ese puesto

  Escenario: Se rechaza cualquier edición del asiento, incluso por el Administrador del Sistema
    Cuando el Administrador del Sistema intenta corregir el nombre del autor del asiento
    Entonces el sistema rechaza la operación
    Y muestra "La pista de auditoría es append-only. Ningún puesto, incluido el Administrador del Sistema, puede alterarla ni borrarla."
    Y registra el intento

  Escenario: Un ascenso no cambia el asiento anterior
    Dado que "Ramón Cáceres" pasa al puesto "Jefe de Transporte" el "2026-01-15"
    Cuando el Auditor Interno consulta "OM-2025-0188" el "2026-02-01"
    Entonces el asiento muestra "Ramón Cáceres" y el puesto "Coordinador de la Unidad de Trámites"
    Y no muestra "Jefe de Transporte"

  Escenario: La baja del servidor no borra la autoría
    Dada la baja de "Ramón Cáceres" registrada el "2026-09-30"
    Cuando el Auditor Interno consulta "OM-2025-0188" el "2027-03-01"
    Entonces el asiento muestra "Ramón Cáceres" y el puesto "Coordinador de la Unidad de Trámites"
    Y el usuario figura desactivado, no eliminado
    Y el hash de la cadena de la pista no ha cambiado

  Escenario: La reorganización de la unidad no cambia el asiento anterior
    Dado que la unidad "Unidad de Trámites" se fusiona con otra y se cierra el "2026-06-30"
    Cuando el Auditor Interno consulta "OM-2025-0188" el "2027-03-01"
    Entonces el asiento sigue mostrando el puesto "Coordinador de la Unidad de Trámites" y su unidad de entonces
    Y la unidad cerrada sigue siendo consultable desde el asiento

  Escenario: El acto por delegación conserva delegado y delegante
    Dada una autorización ejecutada por "Marvin Cálix" por delegación del puesto "Subgerente de Operaciones", folio "DEL-2026-0012"
    Cuando el Auditor Interno la consulta después de revocada la delegación
    Entonces el asiento muestra a "Marvin Cálix", su puesto propio, el puesto delegante y el folio de la delegación con la vigencia que tenía a la fecha del acto

  Escenario: El asiento reverso no reemplaza al original
    Dada una corrección registrada el "2026-08-14" sobre "OM-2025-0188"
    Cuando el Auditor Interno consulta el expediente
    Entonces ve el asiento original con su autor y su puesto
    Y ve el asiento reverso con su propio autor, puesto, motivo y momento
    Y los dos asientos son consultables por separado y encadenados entre sí

  Escenario: El reporte histórico se reproduce idéntico dos años después
    Dado un reporte de misiones del "2025-06-01" al "2025-06-30" emitido el "2025-07-05"
    Cuando el Auditor Interno lo vuelve a emitir el "2027-07-05" con la misma fecha de corte de conocimiento
    Entonces el resultado es idéntico, autor por autor y puesto por puesto
    Y el reporte declara la fecha de corte aplicada
```

## Fuera de alcance

- La estructura técnica de la bitácora con hash encadenado — es [`RNF-04`](../no-funcionales/RNF-04-bitacora-append-only-con-hash-encadenado.md) y materia del Sprint 2
- El cierre de la asignación de puesto — es [HU-136](HU-136-cerrar-asignacion-de-puesto-con-acta.md)
- La exportación del paquete de evidencia — pertenece a M-14
- La retención documental y su plazo — es [`RNF-17`](../no-funcionales/RNF-17-retencion-y-depuracion-diferenciada.md)

## Notas y pendientes

- `[P]` La prohibición de borrar y la exigencia de asiento reverso con motivo y autor provienen de [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md); la numeración exacta de la norma **no está verificada** — insumo **#23**
- `[C]` Plazo de retención documental que fija hasta cuándo el asiento debe seguir consultable — insumo **#71**
- **Regla candidata:** *La autoría de un asiento registra persona y puesto vigentes a la fecha del hecho, y ninguno de los dos se modifica por un cambio posterior de puesto, de estructura o por la baja de la persona.* Es la candidata 1 de [actores-y-roles §8](../../01-negocio/actores-y-roles.md). `RN-03` exige registrar *"identidad, rol, momento, origen y huella"* — **no dice que el puesto quede congelado**, que es exactamente donde se rompe
