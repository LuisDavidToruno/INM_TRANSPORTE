# HU-020 — Firmar en lote los permisos del feriado largo con el reporte previo de resguardo

| Campo | Valor |
|---|---|
| **Módulo** | M-04 Documentación y Cumplimiento Vehicular (con M-14 Reportes e Indicadores) |
| **Actor** | ACT-09 Máxima Autoridad; ACT-14 Encargado de Bienes Institucionales (prepara el reporte) |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Refinada |

## Historia

**Como** Máxima Autoridad
**quiero** recibir, antes de Semana Santa y de cada feriado largo, un reporte con los vehículos que se propone autorizar a circular y los que deben quedar resguardados con su confirmación, y poder firmar los permisos en lote
**para** resolver en una sesión con contexto lo que hoy llega expediente por expediente el viernes a última hora, cuando la única alternativa práctica es prestar la clave

## Contexto

El Tribunal Superior de Cuentas realiza **operativos de fiscalización vehicular específicamente en Semana Santa** `[V]` [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md), [NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md). Es el pico anual de riesgo de la institución — y es **predecible**, lo que lo convierte en el caso más fácil de resolver bien y el más caro de resolver mal.

La carga sobre la máxima autoridad es baja en frecuencia pero crítica, y **se concentra justo antes de fines de semana, feriados y Semana Santa**. Un flujo que le exige abrir veinte expedientes uno por uno a las cinco de la tarde del jueves santo produce, en la práctica, una de dos cosas: permisos que no se firman y misiones que salen sin amparo, o la clave prestada a un asistente.

El reporte tiene **dos mitades y ambas importan**: los vehículos autorizados a circular, y los vehículos que deben estar resguardados **con confirmación de resguardo**. Un vehículo del que nadie confirmó dónde está es exactamente lo que un operativo encuentra.

## Reglas que la gobiernan

- [RN-23](../../01-negocio/reglas/RN-23-permiso-de-circulacion-en-dia-inhabil.md) — Cada permiso conserva su individualidad aunque la firma se ejecute en lote
- [RN-03](../../01-negocio/reglas/RN-03-registro-inmutable-de-autorizacion.md) — Cada firma se registra individualmente con identidad, rol ejercido, momento, origen y huella
- [RN-18](../../01-negocio/reglas/RN-18-rotulacion-del-vehiculo-del-estado.md) — La constatación de rotulación caduca; el resguardo sin evidencia figura como **no confirmado**
- [RN-39](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md), [RN-40](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md) — El período se resuelve contra el calendario vigente a esas fechas
- [RN-94](../../01-negocio/reglas/RN-94-fecha-de-corte-de-conocimiento-en-todo-reporte.md) — El reporte declara su fecha de corte de conocimiento y es reproducible a esa fecha
- [RN-24](../../01-negocio/reglas/RN-24-vehiculo-de-servicio-exceptuado.md) — Los vehículos de servicio exceptuado se listan aparte, sin permiso a firmar

## Casos especiales que la afectan

- [CE-17](../casos-especiales/CE-17-vehiculo-sin-placa-metalica.md) — Los vehículos sin lámina metálica se listan por correlativo institucional y con su respaldo vigente

## Criterios de aceptación

```gherkin
# language: es
Característica: Reporte previo a feriado largo y firma en lote de permisos
  Como Máxima Autoridad
  quiero resolver los permisos del período en una sola sesión con contexto
  para que ningún vehículo circule sin amparo ni quede sin confirmar su resguardo

  Antecedentes:
    Dado un período de Semana Santa del "2026-03-30" al "2026-04-05" declarado en el calendario institucional
    Y una flota de "18" vehículos operativos
    Y una Máxima Autoridad titular "Doris Cruz"
    Y una fecha del sistema del "2026-03-26 09:00"

  Escenario: Se bloquea la firma en lote de un permiso sin vehículo y motorista resueltos
    Dado un lote con 5 permisos, uno de ellos sin motorista asignado
    Cuando "Doris Cruz" firma el lote
    Entonces el sistema firma los 4 permisos completos
    Y no firma el permiso incompleto
    Y muestra "1 de 5 permisos no se firmó: el expediente CHO-2026-00093 no tiene motorista asignado. El permiso es nominativo (RN-23)."

  Escenario: Se bloquea la firma en lote ejercida por alguien distinto del titular
    Dado un servidor "Elsa Maradiaga", Gerente Administrativa
    Cuando "Elsa Maradiaga" intenta firmar el lote de permisos del período
    Entonces el sistema no firma ningún permiso
    Y muestra "Esta facultad es de la máxima autoridad y se trata como indelegable mientras no se confirme lo contrario (insumo #29)."
    Y registra el intento con identidad y momento

  Escenario: El reporte muestra las dos mitades del período
    Cuando el Encargado de Bienes Institucionales genera el reporte previo del período "30/03/2026 al 05/04/2026"
    Entonces el reporte lista "5" vehículos con permiso propuesto para circular
    Y lista "11" vehículos que deben quedar resguardados
    Y lista "2" vehículos de servicio exceptuado, sin permiso a firmar
    Y la suma de las tres listas es "18"

  Escenario: El vehículo sin confirmación de resguardo figura como no confirmado
    Dado un vehículo "VH-0161" asignado a resguardo en el predio "Sede central" sin evidencia registrada
    Cuando el Encargado de Bienes Institucionales genera el reporte previo
    Entonces el vehículo "VH-0161" figura como "resguardo no confirmado"
    Y el reporte lo destaca por encima de los vehículos con resguardo confirmado

  Escenario: La confirmación de resguardo exige evidencia con fecha
    Dado un vehículo "VH-0161" en estado "resguardo no confirmado"
    Cuando el Encargado de Bienes Institucionales registra la confirmación con fotografía fechada el "2026-03-27"
    Entonces el vehículo pasa a "resguardo confirmado el 27/03/2026"
    Y el reporte refleja el cambio en su siguiente corte

  Escenario: Cada firma del lote se registra individualmente
    Dado un lote con 5 permisos completos
    Cuando "Doris Cruz" firma el lote
    Entonces el sistema registra 5 actos de firma independientes
    Y cada uno lleva su propia huella del contenido firmado
    Y cada permiso queda vigente solo para su vehículo, ruta y ventana

  Escenario: El reporte declara su fecha de corte de conocimiento
    Cuando el Encargado de Bienes Institucionales genera el reporte previo el "2026-03-26 09:00"
    Entonces el reporte declara la fecha de corte "26/03/2026 09:00"
    Y una consulta posterior con esa misma fecha de corte reproduce el mismo resultado

  Escenario: El sistema anticipa el período sin que nadie lo solicite
    Dada una anticipación configurada de "10" días antes del inicio del período
    Cuando el sistema evalúa el calendario el "2026-03-20"
    Entonces notifica al Encargado de Bienes Institucionales y al Jefe de Transporte
    Y muestra "El período inhábil del 30/03/2026 al 05/04/2026 inicia en 10 días. Prepare el reporte previo de circulación y resguardo."
```

## Fuera de alcance

- La emisión e impresión de los salvoconductos de los permisos firmados — es [HU-017](HU-017-emision-e-impresion-del-salvoconducto.md), que se ejecuta después
- La gestión del resguardo físico de los vehículos (predios, custodios, actas): es de M-03 y del proceso `PR-14`
- El bloqueo del despacho por falta de permiso — ocurre al despachar, en [CU-05](../casos-de-uso/CU-05-emitir-orden-de-mision-y-documentos.md)
- La firma en lote de cualquier otro acto de autorización: **solo** aplica a permisos de circulación de un mismo período. No se generaliza

## Notas y pendientes

- `[C]` **Legislación posterior sobre los feriados de octubre** — insumo #14. Un feriado mal cargado produce misiones ilegales o bloqueos infundados: el reporte muestra siempre la **versión y vigencia del calendario** con que se resolvió
- `[C]` Si la institución tiene unidad de Bienes separada o la función la absorbe la Gerencia Administrativa — si la absorbe, se activa el control compensatorio de [`actores-y-roles.md`](../../01-negocio/actores-y-roles.md)
- `[C]` Anticipación con que debe generarse el reporte previo — insumo #32. Los "10 días" del criterio son **parámetro**, no constante
- `[V]` Los operativos de fiscalización del TSC en Semana Santa constan en [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md) y [NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md); `[P]` el rango de multas de L 5,000 a L 50,000, con base legal exacta `[C]`
- `[I]` Que la firma en lote reduce el riesgo de delegación informal de la clave es criterio de análisis declarado en [CU-03](../casos-de-uso/CU-03-permiso-de-circulacion-en-dia-inhabil.md), no norma
- Trazabilidad: [CU-03](../casos-de-uso/CU-03-permiso-de-circulacion-en-dia-inhabil.md) flujo alterno A4; procesos `PR-07` y `PR-14`

---

## Nota de corrección — alcance del salvoconducto

> **El permiso ampara vehículo, motorista, ruta y ventana**, conforme a [`RN-23`](../../01-negocio/reglas/RN-23-permiso-de-circulacion-en-dia-inhabil.md) y a `BD-04` de la [máquina de estados](../../03-arquitectura/estados/orden-de-mision.md). **Un relevo de motorista lo invalida** y obliga a reemitirlo para el tramo restante — ver [`HU-018`](HU-018-reemision-del-permiso-por-cambio-de-elementos.md).
>
> **Una corrección anterior de esta historia adoptó la lectura contraria** —sin el motorista, con el relevo sin invalidar— por temor a dejar el vehículo varado un domingo. **Ese temor era infundado:** el código de autorización fuera de línea permite que la máxima autoridad autorice por teléfono, sin conectividad.
>
> La razón de fondo: **el salvoconducto lo lee un agente en carretera que compara el nombre del papel con quien va al volante.** Si no coinciden, el documento no sirve para lo único que existe.
>
> Hallazgos `HB3-07` y `HB34-06`.
