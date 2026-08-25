# HU-105 — Capturar la licencia de conducir como dato propio de SIGTI, con el documento físico adjunto

| Campo | Valor |
|---|---|
| **Módulo** | M-05 Motoristas y Habilitación |
| **Actor** | ACT-04 Jefe de Transporte |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — falta el catálogo oficial de restricciones médicas de la DNVT, que se entrega **vacío** porque no se inventan valores (insumo #42), y el contrato de API de Talento Humano (insumo #17): si resultara mantener la categoría de licencia con el detalle requerido, se reconsidera de quién es el dato |

## Historia

**Como** Jefe de Transporte
**quiero** capturar la licencia de conducir del servidor dentro de SIGTI —número, autoridad emisora, cada categoría con su propio vencimiento, restricciones médicas y fotografía del documento físico—
**para** que el bloqueo de licencia tenga de dónde leer, con un dato que la institución controla y que puede sostener ante un siniestro

## Contexto

**Este es el dato que sostiene el control de mayor valor legal del sistema.** El bloqueo de programación y despacho contra la licencia no tiene excepción configurable porque *"nos tenemos que proteger con la ley también"* — y **sin esta captura, ese bloqueo no tiene de dónde leer**.

**La licencia es dato propio de SIGTI, no espejo de Talento Humano.** Un control de esta criticidad legal no puede depender del modelo de datos de un sistema ajeno que no tiene motivo para mantenerlo. Alguien de la institución tiene que capturarlo y mantenerlo dentro de SIGTI. **Es trabajo adicional real y hay que decirlo de frente: es el precio de que el bloqueo sea defendible.**

La persona sí viene del espejo: **SIGTI no crea personas**.

## Reglas que la gobiernan

- [RN-48](../../01-negocio/reglas/RN-48-datos-espejo-de-solo-lectura.md) — La persona viene del espejo de Talento Humano; la licencia es dato propio
- [RN-50](../../01-negocio/reglas/RN-50-degradacion-por-sincronizacion-detenida.md) — El espejo desactualizado degrada explícitamente antes de operar
- [RN-05](../../01-negocio/reglas/RN-05-registro-cerrado-no-se-edita.md) — La renovación cierra el rango anterior y abre uno nuevo; no sobrescribe
- [RN-11](../../01-negocio/reglas/RN-11-restricciones-medicas-del-motorista.md) — Las restricciones médicas se registran del catálogo, con su efecto
- [RN-43](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md) · [RN-46](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md) · [RN-47](../../01-negocio/reglas/RN-47-digitacion-diferida-desde-papel.md) — Captura en delegación sin red, con fechas distintas y original adjunto
- [RN-03](../../01-negocio/reglas/RN-03-registro-inmutable-de-autorizacion.md) — Autor, puesto, momento y huella del contenido

## Casos especiales que la afectan

- [CE-11](../casos-especiales/CE-11-licencia-vence-durante-la-mision.md) — El dato que se captura aquí es el que decide ese caso
- [CE-13](../casos-especiales/CE-13-motorista-no-disponible-por-talento-humano.md) — Lo que sí depende del espejo es la disponibilidad, no la licencia

## Criterios de aceptación

```gherkin
# language: es
Característica: Captura de la licencia de conducir como dato propio

  Antecedentes:
    Dado un espejo de Talento Humano con la persona "José Martínez", identidad "0801-1985-04521", puesto vigente
    Y un catálogo de categorías de licencia vigente con los valores "A", "B", "B1", "C1", "C", "D1", "D" y "CE"

  Escenario: SIGTI no crea personas
    Cuando el Jefe de Transporte busca a "Mario Rivera" y no existe en el espejo de Talento Humano
    Entonces el sistema no permite crear la persona
    Y muestra "Mario Rivera no existe en el espejo de Talento Humano. SIGTI no crea personas: corrija el sistema origen."
    Y genera una incidencia de espejo dirigida al Administrador

  Escenario: Se rechaza la captura sin fotografía o escaneo de la licencia física
    Cuando el Jefe de Transporte captura la licencia de "José Martínez" sin adjunto del documento
    Entonces el sistema no consuma la habilitación
    Y muestra "Adjunte la fotografía o el escaneo de la licencia física. Sin ella, ante un siniestro solo queda la palabra de quien capturó."

  Escenario: Se rechaza la captura sin vencimiento por categoría
    Cuando el Jefe de Transporte captura las categorías "B" y "C1" con un único vencimiento común
    Entonces el sistema rechaza el registro
    Y muestra "Cada categoría lleva su propia fecha de vencimiento: perder una categoría no es perder la licencia completa."

  Escenario: Se captura la licencia completa
    Cuando el Jefe de Transporte captura número "0801198504521", autoridad emisora, fecha de emisión, categoría "B" vigente hasta el "2028-05-12", categoría "C1" vigente hasta el "2027-03-15", restricción "usa corrección visual" y fotografía del documento
    Entonces el sistema registra la licencia como dato propio de SIGTI
    Y registra autor, puesto, marca de tiempo y huella del contenido

  Escenario: La licencia no se edita: se cierra el rango y se abre otro
    Dado una categoría "C1" vigente hasta el "2027-03-15"
    Cuando el Jefe de Transporte registra la renovación de "C1" hasta el "2032-03-15" con adjunto
    Entonces el sistema cierra el rango anterior y abre uno nuevo
    Y el expediente puede decir qué licencia amparaba una misión de hace ocho meses

  Escenario: La promesa de renovación no levanta el bloqueo
    Dado una categoría "C1" vencida el "2027-03-15"
    Cuando el Jefe de Transporte declara que la renovación está en trámite, sin adjunto del documento renovado
    Entonces el bloqueo sigue vigente
    Y muestra "Mientras el dato renovado no conste con adjunto, la categoría C1 sigue vencida para efectos de asignación."

  Escenario: El motorista no captura ni modifica su propia licencia
    Cuando "José Martínez" intenta capturar o modificar su licencia
    Entonces el sistema rechaza la acción
    Y muestra "El motorista aporta el documento físico; no registra su propia habilitación."
    Y el intento queda en la pista de auditoría

  Escenario: El espejo desactualizado degrada, pero la licencia se puede capturar igual
    Dado un espejo de Talento Humano con "23" días sin sincronizar
    Cuando el Jefe de Transporte abre el expediente de "José Martínez"
    Entonces el sistema advierte "Datos de Talento Humano sincronizados hace 23 días. La disponibilidad del servidor —permisos, vacaciones, incapacidades— y la vigencia de su puesto figuran como no confirmadas."
    Y permite capturar y mantener la licencia, porque es dato propio

  Escenario: La captura en delegación se propone, no se consuma
    Cuando el Encargado de Delegación captura la licencia sin conectividad, con fecha del hecho y fecha de captura
    Entonces el registro queda como propuesta de habilitación
    Y la consuma el Jefe de Transporte al sincronizar
    Y ningún conflicto de sincronización se resuelve por sobrescritura

  Escenario: Dos servidores con el mismo número de licencia advierte, no bloquea
    Dado una licencia "0801198504521" ya registrada a nombre de "José Martínez"
    Cuando el Jefe de Transporte registra el mismo número a nombre de "Óscar Banegas"
    Entonces el sistema advierte e indica con quién colisiona
    Y exige verificación contra el documento físico y motivo registrado antes de guardar
```

## Fuera de alcance

- La derivación de los tipos de vehículo habilitados — es [HU-106](HU-106-derivar-los-tipos-de-vehiculo-habilitados.md)
- La vigencia de la habilitación y sus alertas — es [HU-107](HU-107-calcular-la-vigencia-de-la-habilitacion-y-alertar.md)
- La validación contra el registro de la DNVT: **no hay integración disponible**; el dato es el que capturó la institución
- El expediente del empleado, permisos, vacaciones e incapacidades: son de Talento Humano ([DP-001 D-07](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md))

## Notas y pendientes

- `[C]` Si el contrato de API de Talento Humano resulta mantener la categoría de licencia con el detalle requerido, se reconsidera la propiedad del dato. Hasta entonces, es propio de SIGTI ([ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md)) — insumo **#17**
- `[C]` `umbral_advertencia_desincronizacion` del espejo de Talento Humano — insumo **#17**
- `[C]` **Catálogo oficial de restricciones médicas de la DNVT**: no se tiene. El catálogo se entrega **vacío y configurable**; **no se inventan valores** — insumo **#42**
- `[C]` ¿Debe la duplicidad de número de licencia ser bloqueo duro en lugar de advertencia? Casi siempre es error de digitación, y bloquear sin poder corregir deja al motorista fuera del padrón por un dígito — insumo **#1**
- `[I]` La exigencia del adjunto de la licencia es implicación de requerimiento del equipo derivada de [NRM-06](../../01-negocio/normativa/NRM-06-transito-y-licencias.md), **no articulado citable**. Por eso el bloqueo es configurable, encendido por defecto. `[C]` confirmar con Auditoría Interna
- `[C]` ¿Qué ocurre con un empleado dado de baja en Talento Humano que tiene misiones abiertas en SIGTI? Pendiente expreso de [ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md)
