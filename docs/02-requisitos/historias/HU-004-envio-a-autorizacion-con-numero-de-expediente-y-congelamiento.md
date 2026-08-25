# HU-004 — Enviar la solicitud a autorización congelando su contenido

| Campo | Valor |
|---|---|
| **Módulo** | M-06 Solicitudes de Transporte |
| **Actor** | ACT-02 Solicitante |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | **Borrador — reabierta por `HB34-03`**: bloqueaba el envío donde `RN-50` prohíbe bloquear. Corregida abajo; vuelve a refinamiento |

## Nota de corrección — hallazgo `HB34-03`

> **Qué estaba mal.** Esta historia **bloqueaba** el envío a autorización cuando el espejo de la jerarquía de ARGOS superaba un umbral de 72 horas, **y citaba [`RN-50`](../../01-negocio/reglas/RN-50-degradacion-por-sincronizacion-detenida.md) como fundamento del bloqueo que `RN-50` niega**.
>
> `RN-50` es la autoridad en esta materia y su enunciado no admite lectura: *«Superado el umbral de advertencia, toda operación sensible debe mostrar la antigüedad del dato antes de continuar, exigir acuse y registrar la advertencia. **La operación no se impide: se marca.**»* Su ficha declara **Tipo: Advertencia con acuse registrado** y añade expresamente que *«el bloqueo por desincronización no está decidido»*. La regla ya había sido corregida por los hallazgos `HB1-10` y `HB1-13` precisamente por este error; la historia se escribió con la versión anterior.
>
> **El caso que lo hace grave, y es el caso de diseño del producto, no un borde.** La Delegación Choluteca lleva cuatro días sin enlace con la sede —96 horas, por encima de 72—. Con esta historia en el código, **ningún solicitante de esa delegación puede enviar una solicitud a autorización** hasta que vuelva el enlace. La premisa rectora 5 es *offline-first, no «con soporte offline»*, y [HU-007](HU-007-captura-sin-conectividad-y-digitacion-diferida.md) existe justamente para capturar y enviar la solicitud sin conectividad. Esta historia la anulaba a las 72 horas.
>
> **Qué se corrige.** El escenario de bloqueo pasa a **advertencia con acuse nominativo, marca en el expediente y marca en el impreso**, alineado con [HU-026](HU-026-disponibilidad-del-motorista-contra-el-espejo.md), que implementa la misma regla correctamente. Desaparece el "umbral de bloqueo"; queda un solo umbral, el de advertencia.
>
> **Lo que sigue bloqueando y está bien que bloquee:** la cadena de autorización que **no se puede resolver** —una dependencia sin puesto superior configurado—. Ese bloqueo es de [`RN-02`](../../01-negocio/reglas/RN-02-escalamiento-de-autorizacion.md), no de `RN-50`, y no depende de la antigüedad del espejo sino de que el dato no exista.
>
> **Si el PO quiere un escalón de bloqueo**, se decide en `RN-50` —que lo tiene abierto como pendiente, con tres preguntas sin responder: a qué umbral, quién autoriza la operación degradada y bajo qué par `I-nn` se controla esa facultad— y baja de ahí a las historias. No se inventa aquí.
>
> **Contaminación aguas arriba, reportada y no corregible desde esta historia:** `RNF-07` lista un umbral que *«bloquea operaciones sensibles»* y los casos límite de `RN-10` afirman que *«superado el umbral, la asignación de motoristas se bloquea»*. Ambos contradicen a `RN-50`, que es la autoridad. Queda como hallazgo hacia esos dos artefactos.

## Historia

**Como** Solicitante
**quiero** enviar la solicitud a autorización y que el sistema le asigne su número de expediente institucional y congele el contenido con su huella
**para** que quien autorice después autorice **ese** contenido concreto, y no una versión editada más tarde que nadie podría distinguir

## Contexto

En papel, el expediente que firma la jefatura es el que tiene enfrente. En un sistema sin congelamiento, el solicitante puede cambiar el destino, la carga o la fecha después de que la firma quedó registrada, y la autorización pasa a amparar algo que nunca se autorizó. Ese es exactamente el hallazgo que un auditor busca.

El número de expediente es **correlativo por delegación y año, y no se recicla**: ni siquiera cuando el expediente se rechaza o se anula. Un correlativo con huecos es normal; un correlativo reutilizado es un expediente que sustituye a otro.

## Reglas que la gobiernan

- [RN-03](../../01-negocio/reglas/RN-03-registro-inmutable-de-autorizacion.md) — Registro inmutable con identidad, rol, momento, origen y **huella del contenido**
- [RN-41](../../01-negocio/reglas/RN-41-congelamiento-del-valor-al-autorizar.md) — El valor calculado se congela junto con el identificador de la tabla usada
- [RN-44](../../01-negocio/reglas/RN-44-identificadores-y-folios-en-el-cliente.md) — Folios y correlativos se asignan de rangos por delegación
- [RN-06](../../01-negocio/reglas/RN-06-transiciones-de-estado-de-la-orden.md) — `T-02` es la única vía de `BORRADOR` a `SOLICITADA`, con actor, rol, momento y motivo
- [RN-02](../../01-negocio/reglas/RN-02-escalamiento-de-autorizacion.md) — La cadena de autorización se resuelve al enviar, contra el espejo de ARGOS
- [RN-50](../../01-negocio/reglas/RN-50-degradacion-por-sincronizacion-detenida.md) — Si el espejo de la jerarquía lleva detenido más del umbral, el sistema **degrada explícitamente: advierte, exige acuse nominativo y registra. La operación no se impide, se marca.** El envío **nunca** se bloquea por antigüedad del espejo (corregido por `HB34-03`)
- [RN-05](../../01-negocio/reglas/RN-05-registro-cerrado-no-se-edita.md) — Enviado el expediente, el contenido sustantivo no se edita

## Casos especiales que la afectan

- Ninguno de los 28 `CE-xx` se materializa en el envío. Constancia dejada: [CE-11](../casos-especiales/CE-11-licencia-vence-durante-la-mision.md), [CE-12](../casos-especiales/CE-12-dos-solicitudes-compiten-por-el-mismo-vehiculo.md), [CE-13](../casos-especiales/CE-13-motorista-no-disponible-por-talento-humano.md) y [CE-16](../casos-especiales/CE-16-vehiculo-a-taller-con-misiones-programadas.md) quedan **descartados explícitamente**: en `SOLICITADA` no hay recursos asignados

## Criterios de aceptación

```gherkin
# language: es
Característica: Envío de la solicitud a autorización
  Como Solicitante
  quiero enviar la solicitud y que su contenido quede congelado
  para que se autorice exactamente lo que se envió

  Antecedentes:
    Dada una delegación "Choluteca" con rango de correlativos "CHO-2026" disponible
    Y un Solicitante "Ana Bustillo" de la dependencia "Subgerencia de Operaciones"
    Y un espejo de la jerarquía de ARGOS sincronizado el "2026-03-14 06:00"
    Y un umbral configurable de advertencia por sincronización detenida de "24" horas

  Escenario: Se rechaza el envío con contenido mínimo incompleto
    Dado un borrador sin motivo de viaje del catálogo
    Cuando "Ana Bustillo" intenta enviar la solicitud a autorización
    Entonces el sistema no ejecuta el envío
    Y muestra "Falta el motivo de viaje del catálogo. No se puede enviar una solicitud sin motivo institucional."
    Y el expediente permanece en estado "BORRADOR"

  Escenario: Se bloquea el envío cuando la cadena de autorización no se puede resolver
    Dado una dependencia "Unidad de Enlace" cuyo único puesto es el del solicitante de derecho
    Y ningún puesto superior configurado sobre esa dependencia
    Cuando "Ana Bustillo" intenta enviar una solicitud de esa dependencia
    Entonces el sistema no ejecuta el envío
    Y muestra "No se pudo resolver un autorizador competente. Ruta evaluada: Unidad de Enlace → sin puesto superior configurado. Corrija la configuración de la dependencia (RN-02)."
    Y no propone ningún autorizador alterno

  Escenario: La delegación sin enlace durante días sigue pudiendo enviar, con advertencia y acuse
    Dado un espejo de la jerarquía de ARGOS sincronizado por última vez el "2026-03-10 06:00"
    Y una fecha del sistema del "2026-03-14 08:00"
    Cuando "Ana Bustillo" envía la solicitud a autorización
    Entonces el sistema muestra, antes de ejecutar el envío, "La estructura de autorización se sincronizó por última vez hace 98 horas. Un cambio de jefatura posterior no se ve aquí. Si continúa, quedará registrado que usted lo hizo con este dato."
    Y exige el acuse nominativo de "Ana Bustillo" con motivo antes de continuar
    Y ejecuta el envío
    Y asienta en el diario del expediente el acuse con el nombre, el momento y el motivo
    Y la advertencia se imprimirá en la Orden de Misión que derive de este expediente
    Y no ofrece en ningún momento impedir el envío por antigüedad del espejo

  Escenario: Se advierte y se deja constancia con el espejo desactualizado bajo el umbral
    Dado un espejo de la jerarquía de ARGOS sincronizado por última vez el "2026-03-12 06:00"
    Y una fecha del sistema del "2026-03-14 08:00"
    Cuando "Ana Bustillo" envía la solicitud a autorización
    Entonces el sistema ejecuta el envío
    Y muestra "La estructura de autorización tiene 50 horas de antigüedad. Se encamina con esa advertencia registrada."
    Y deja la advertencia asentada en el diario del expediente

  Escenario: La superación sostenida del umbral genera incidente para el Administrador del Sistema
    Dado un espejo de la jerarquía de ARGOS detenido desde el "2026-03-10 06:00"
    Cuando el sistema evalúa el estado de sincronización el "2026-03-14 08:00"
    Entonces genera un incidente dirigido a ACT-01 Administrador del Sistema
    Y muestra "El espejo de la estructura de autorización lleva 98 horas detenido. 12 expedientes se encaminaron con advertencia en ese período."
    Y el incidente no se dirige al solicitante que tropezó con la advertencia

  Escenario: El envío asigna correlativo, congela el contenido y no reserva flota
    Dado un borrador completo con salida prevista el "2026-03-20 07:00"
    Y un último correlativo asignado en la delegación "Choluteca" con número "CHO-2026-00086"
    Cuando "Ana Bustillo" envía la solicitud a autorización
    Entonces el expediente recibe el número "CHO-2026-00087"
    Y el expediente pasa a estado "SOLICITADA"
    Y el sistema calcula y almacena la huella del contenido sustantivo
    Y el estimado de peajes queda congelado con el identificador de la tabla de tarifas usada
    Y no existe reserva de vehículo ni de motorista

  Escenario: El contenido sustantivo no se edita después del envío
    Dado un expediente "CHO-2026-00087" en estado "SOLICITADA"
    Cuando "Ana Bustillo" intenta cambiar el destino de "Comayagua" a "San Pedro Sula"
    Entonces el sistema rechaza el cambio
    Y muestra "El contenido está congelado desde el envío. Solicite a la jefatura que devuelva el expediente a borrador (T-04); el número CHO-2026-00087 se conserva y la versión pasa a 2."

  Escenario: El correlativo del expediente rechazado no se recicla
    Dado un expediente "CHO-2026-00087" en estado "RECHAZADA"
    Cuando otro Solicitante de la delegación "Choluteca" envía una solicitud nueva
    Entonces el expediente nuevo recibe el número "CHO-2026-00088"
    Y el número "CHO-2026-00087" no vuelve a asignarse
```

## Fuera de alcance

- La resolución de **quién** es el autorizador competente por monto, destino o duración: es propiedad de ARGOS ([DP-001](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) D-05). SIGTI la consume del espejo
- El acto de autorizar — es [HU-011](HU-011-registro-inmutable-de-la-autorizacion.md)
- La asignación de correlativos **sin conectividad** desde el rango de la delegación — es [HU-007](HU-007-captura-sin-conectividad-y-digitacion-diferida.md)
- El manifiesto de personas externas como precondición de envío: **diferido** a las historias de M-17. Mientras no exista, la validación de manifiesto no se implementa y así queda declarado

## Notas y pendientes

- `[C]` Formato exacto del número de expediente institucional (prefijo de delegación, año, dígitos) — insumo #2
- `[C]` **Umbral de advertencia** por sincronización detenida, por conjunto de datos y por delegación — insumo #16. Los valores de los criterios son de ejemplo y **son parámetros**, no constantes. **Ya no hay umbral de bloqueo**: `RN-50` lo tiene retirado y su reintroducción es decisión abierta del PO **en la regla**, no en esta historia (`HB34-03`)
- `[C]` Autorizador alterno por dependencia y por delegación — insumo #28. Hasta tenerlo, la cadena agotada **bloquea y muestra la ruta evaluada**; no se inventa un sustituto
- `[I]` Que el algoritmo de huella deba ser resistente a colisiones es implicación de requerimiento, no exigencia normativa citada
- Trazabilidad: [CU-01](../casos-de-uso/CU-01-registrar-solicitud-de-transporte.md) pasos 10 a 13, excepción E7; transición `T-02`; invariantes `INV-05` a `INV-08`
