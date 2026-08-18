# HU-097 — Registrar la placa como número asignado y estado físico de la lámina, con su respaldo

| Campo | Valor |
|---|---|
| **Módulo** | M-03 Flota Vehicular · M-04 Documentación y Cumplimiento Vehicular |
| **Actor** | ACT-14 Encargado de Bienes Institucionales |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador |

## Historia

**Como** Encargado de Bienes Institucionales
**quiero** registrar por separado el número de placa asignado en el registro y el estado físico de la lámina, cada uno con su rango de vigencia y su documento de respaldo
**para** que un vehículo sin lámina metálica pueda operar amparado en su respaldo, y para que una multa de marzo se impute al vehículo que tenía esa placa en marzo

## Contexto

**Sin placa metálica es un estado válido**: hay desabastecimiento nacional `[V]`. Un campo `placa` obligatorio y único rompe el sistema. Son dos datos distintos: el número que el registro vehicular asignó —que puede existir sin que la lámina haya llegado— y el estado físico de la lámina, que cambia con el tiempo.

Y lo que bloquea el despacho de un vehículo sin lámina **no es la ausencia de placa: es la ausencia de respaldo**. Un vehículo sin lámina y con documento de respaldo vigente circula; uno sin lámina y sin nada, no.

## Reglas que la gobiernan

- [RN-64](../../01-negocio/reglas/RN-64-estado-de-la-placa-tipificado.md) — Número asignado y estado físico son datos distintos, con historial y vigencia
- [RN-65](../../01-negocio/reglas/RN-65-sin-lamina-respaldo-y-paquete-de-identificacion.md) — Lo que bloquea el despacho es la ausencia de respaldo, no la de placa
- [RN-15](../../01-negocio/reglas/RN-15-identidad-del-vehiculo-y-placa.md) — La placa **no es obligatoria ni única**; el identificador es el correlativo institucional
- [RN-66](../../01-negocio/reglas/RN-66-imputacion-externa-por-jerarquia-de-anclas.md) — La imputación de un hecho externo se resuelve por la placa vigente **a la fecha del hecho**
- [RN-46](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md) — El cambio de estado tiene fecha del hecho propia
- [RN-05](../../01-negocio/reglas/RN-05-registro-cerrado-no-se-edita.md) — El rango anterior se cierra, no se edita
- [RN-17](../../01-negocio/reglas/RN-17-alertas-de-vencimiento-documental.md) — La alerta crónica se reconoce con fundamento por un período, nunca se silencia para siempre

## Casos especiales que la afectan

- [CE-17](../casos-especiales/CE-17-vehiculo-sin-placa-metalica.md) — Eje de la historia
- [CE-15](../casos-especiales/CE-15-vehiculo-en-comodato-o-alquilado.md) — La placa del bien ajeno no es de la institución

## Criterios de aceptación

```gherkin
# language: es
Característica: Placa asignada y estado físico de la lámina

  Antecedentes:
    Dado un catálogo "estado_de_placa" vigente con los valores "CON_LAMINA", "SIN_LAMINA_ASIGNADA", "SIN_LAMINA_EN_TRAMITE", "EN_TRAMITE_DE_REPOSICION", "RETENIDA_POR_AUTORIDAD" y "EXTRAVIADA"
    Y un vehículo "TR-0092" recién dado de alta

  Escenario: Se acepta el alta sin número de placa
    Cuando el Encargado de Bienes registra "TR-0092" sin número de placa, con estado "SIN_LAMINA_EN_TRAMITE"
    Entonces el sistema acepta el registro
    Y no exige el número de placa como dato obligatorio

  Escenario: Se rechaza un estado distinto de CON_LAMINA sin documento de respaldo
    Cuando el Encargado de Bienes registra el estado "SIN_LAMINA_EN_TRAMITE" sin documento de respaldo
    Entonces el sistema rechaza el registro
    Y muestra "El estado SIN_LAMINA_EN_TRAMITE exige documento de respaldo con emisor, folio, adjunto y vigencia."

  Escenario: Se bloquea el despacho sin lámina y sin respaldo vigente
    Dado un vehículo "TR-0092" con estado "SIN_LAMINA_EN_TRAMITE" y respaldo vencido el "2026-08-31"
    Cuando el Encargado de Despacho intenta despachar "TR-0092" el "2026-09-24"
    Entonces el sistema rechaza el despacho
    Y muestra "TR-0092 circula sin lámina y su documento de respaldo venció el 31/08/2026. Renueve el respaldo antes de despachar."

  Escenario: Se despacha sin lámina con respaldo vigente
    Dado un vehículo "TR-0092" con estado "SIN_LAMINA_EN_TRAMITE" y respaldo vigente hasta el "2026-12-31"
    Cuando el Encargado de Despacho despacha "TR-0092" el "2026-09-24"
    Entonces el sistema acepta el despacho
    Y el paquete de identificación del vehículo se emite con el documento de respaldo incluido

  Escenario: Placa duplicada advierte pero no bloquea
    Dado un vehículo "TR-0045" con placa "PAA-1234" vigente
    Cuando el Encargado de Bienes registra la placa "PAA-1234" en el vehículo "TR-0092" con rango que se traslapa
    Entonces el sistema advierte y permite guardar exigiendo motivo escrito
    Y muestra "La placa PAA-1234 está registrada en TR-0045 desde el 14/03/2024. Indique el motivo: el correlativo institucional mantiene la operación mientras el registro vehicular resuelve."

  Escenario: Dos vehículos con la misma placa en rangos que no se traslapan no son duplicado
    Dado un vehículo "TR-0045" con placa "PAA-1234" hasta el "2025-06-30"
    Cuando el Encargado de Bienes registra la placa "PAA-1234" en "TR-0092" desde el "2025-08-01"
    Entonces el sistema acepta el registro sin advertencia de duplicado

  Escenario: El cambio de estado cierra el rango anterior con fecha del hecho propia
    Dado un vehículo "TR-0092" con estado "SIN_LAMINA_EN_TRAMITE" desde el "2026-01-15"
    Cuando el Encargado de Bienes registra el estado "CON_LAMINA" con fecha del hecho "2026-09-10" y fecha de captura "2026-09-24"
    Entonces el rango anterior se cierra el "2026-09-09"
    Y se abre un rango nuevo desde el "2026-09-10"
    Y el sistema no edita el registro anterior

  Escenario: Una consulta por placa a una fecha pasada devuelve el vehículo correcto
    Cuando se consulta la placa "PAA-1234" a la fecha "2025-03-12" para imputar una multa
    Entonces el sistema devuelve el vehículo que tenía esa placa el "12/03/2025"
    Y no devuelve el vehículo que la tiene hoy

  Escenario: El estado EN_TRAMITE_DE_REPOSICION exige expediente del trámite
    Cuando el Encargado de Bienes registra el estado "EN_TRAMITE_DE_REPOSICION" sin expediente
    Entonces el sistema rechaza el registro
    Y muestra "Registre el expediente del trámite: fecha de inicio, institución ante la que se gestiona, gestiones realizadas y resultado."

  Escenario: La alerta crónica se reconoce con fundamento, no se silencia para siempre
    Dado una alerta de trámite de placa detenido desde hace "26" meses
    Cuando el Jefe de Transporte la marca como reconocida con fundamento por "180" días
    Entonces la alerta se suprime hasta el vencimiento del período
    Y reaparece al vencerlo
    Y el sistema no ofrece la opción de silenciarla de forma permanente
```

## Fuera de alcance

- La constatación de la rotulación institucional — es [HU-100](HU-100-constatar-la-identificacion-institucional.md)
- El alta del vehículo y su título de tenencia — es [HU-096](HU-096-dar-de-alta-el-vehiculo-con-titulo-de-tenencia.md)
- El trámite de placa ante el registro vehicular: se registra su expediente, no se gestiona desde SIGTI
- La imputación de multas en sí: pertenece a M-12

## Notas y pendientes

- `[V]` El desabastecimiento nacional de placas — [NRM-06](../../01-negocio/normativa/NRM-06-transito-y-licencias.md)
- `[C]` **Catálogo de documentos sustitutivos** que la institución acepta como respaldo por falta de lámina — insumo **#60**
- `[C]` `vigencia_constatacion_rotulacion` más corta para vehículos sin lámina, cuya rotulación es su única identificación visible — insumo **#1**
- `[C]` Qué documento emite hoy la institución para amparar la circulación sin lámina — insumo **#2**
