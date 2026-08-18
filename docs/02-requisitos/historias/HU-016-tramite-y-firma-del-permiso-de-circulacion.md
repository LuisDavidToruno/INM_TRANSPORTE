# HU-016 — Tramitar y firmar el permiso de circulación en día u hora inhábil

| Campo | Valor |
|---|---|
| **Módulo** | M-04 Documentación y Cumplimiento Vehicular |
| **Actor** | ACT-09 Máxima Autoridad (firma); ACT-04 Jefe de Transporte y ACT-10 Encargado de Delegación (proponen) |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Refinada |

## Historia

**Como** Jefe de Transporte
**quiero** preparar el expediente de permiso de circulación en día u hora inhábil con su justificación, ruta, ventana, vehículo y motorista, y encaminarlo a la máxima autoridad para su firma
**para** que la misión pueda despacharse legalmente el sábado o el feriado, en lugar de salir sin amparo y exponer a la institución a un operativo del Tribunal Superior de Cuentas

## Contexto

Circular un vehículo del Estado en día u hora inhábil requiere permiso firmado por la máxima autoridad `[V]` [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md). No es una formalidad interna: el TSC realiza operativos de fiscalización vehicular, con multas reportadas de L 5,000 a L 50,000 y posible decomiso `[P]`.

Dos decisiones de diseño que se sostienen aquí:

**El expediente se abre en `APROBADA`, pero no se firma sin vehículo y motorista resueltos.** [`RN-23`](../../01-negocio/reglas/RN-23-permiso-de-circulacion-en-dia-inhabil.md) dice que el permiso no requiere que la misión esté programada, y a la vez que el permiso es nominativo sobre vehículo, ruta y ventana. Ambas cosas no se cumplen a la vez si no se separa **abrir el trámite** de **firmarlo** — resolución `HCU-05` de [CU-03](../casos-de-uso/CU-03-permiso-de-circulacion-en-dia-inhabil.md).

**La pantalla de firma debe caber en un teléfono y resolverse en dos toques.** Si no, la máxima autoridad delega informalmente su clave un viernes a las seis de la tarde, que es exactamente el riesgo que se quiere evitar `[I]`.

Y hay un caso que **no requiere permiso**: el vehículo con excepción de servicio exceptuado vigente —emergencia, seguridad, salud—. La excepción es **atributo del vehículo, no del viaje**.

## Reglas que la gobiernan

- [RN-23](../../01-negocio/reglas/RN-23-permiso-de-circulacion-en-dia-inhabil.md) — Circular en día u hora inhábil requiere permiso vigente firmado por la máxima autoridad
- [RN-24](../../01-negocio/reglas/RN-24-vehiculo-de-servicio-exceptuado.md) — La excepción es atributo del vehículo, con fundamento y vigencia registrados
- [RN-07](../../01-negocio/reglas/RN-07-delegacion-de-autorizacion.md) — **No habilitada para esta facultad** hasta confirmación institucional
- [RN-03](../../01-negocio/reglas/RN-03-registro-inmutable-de-autorizacion.md) — El acto de firma se registra con identidad, puesto, rol ejercido, momento, origen y huella
- [RN-39](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md), [RN-40](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md) — Calendario y horario hábil como parámetros con vigencia, evaluados a las fechas de la misión

## Casos especiales que la afectan

- [CE-17](../casos-especiales/CE-17-vehiculo-sin-placa-metalica.md) — El permiso identifica al vehículo por correlativo institucional; la placa no es obligatoria
- [CE-06](../casos-especiales/CE-06-la-mision-se-extiende-mas-dias-destinos-o-kilometros.md) — El permiso sobreviniente por prórroga en ruta: **queda fuera** de esta historia y se resuelve en M-08

## Criterios de aceptación

```gherkin
# language: es
Característica: Trámite y firma del permiso de circulación en día u hora inhábil
  Como Jefe de Transporte
  quiero encaminar el permiso a la máxima autoridad
  para que la misión pueda despacharse legalmente en franja inhábil

  Antecedentes:
    Dado un expediente "CHO-2026-00087" en estado "APROBADA" con la marca "REQUIERE_PERMISO_MAXIMA_AUTORIDAD"
    Y una ventana del "2026-03-20 07:00" al "2026-03-21 17:00"
    Y un tramo inhábil señalado "sábado 21/03/2026, de 00:00 a 17:00"
    Y una Máxima Autoridad titular "Doris Cruz"
    Y un vehículo "Pickup Hilux" con correlativo institucional "VH-0142"
    Y un motorista "José Martínez"

  Escenario: Se bloquea la firma si el permiso no tiene vehículo y motorista resueltos
    Dado un expediente de permiso sin vehículo ni motorista asignados
    Cuando "Doris Cruz" intenta firmar el permiso
    Entonces el sistema no ejecuta la firma
    Y muestra "El permiso es nominativo sobre vehículo, ruta y ventana. Programe la misión antes de firmar (RN-23)."

  Escenario: Se bloquea la firma ejercida por alguien distinto del titular de la máxima autoridad
    Dado un servidor "Elsa Maradiaga", Gerente Administrativa
    Y un expediente de permiso completo con vehículo "VH-0142" y motorista "José Martínez"
    Cuando "Elsa Maradiaga" intenta firmar el permiso
    Entonces el sistema no ejecuta la firma
    Y muestra "Esta facultad es de la máxima autoridad y se trata como indelegable mientras no se confirme lo contrario. Las salidas son reprogramar la ventana a franja hábil o esperar la firma (RN-23, insumo #29)."
    Y registra el intento con identidad y momento en la pista de auditoría

  Escenario: Se bloquea la firma con delegación registrada a favor de otro servidor
    Dado un acto de delegación "DEL-2026-0031" a favor de "Elsa Maradiaga" para autorizar solicitudes
    Cuando "Elsa Maradiaga" intenta firmar el permiso invocando esa delegación
    Entonces el sistema no ejecuta la firma
    Y muestra "La delegación DEL-2026-0031 no alcanza a la firma del permiso de circulación en día u hora inhábil."

  Escenario: No se genera un permiso duplicado si ya existe uno vigente que cubre lo mismo
    Dado un permiso "PC-2026-0009" vigente para el vehículo "VH-0142", el motorista "José Martínez", la ruta "Tegucigalpa–Choluteca" y la ventana del "2026-03-20 07:00" al "2026-03-21 17:00"
    Cuando el Jefe de Transporte intenta preparar otro permiso para esa misma misión
    Entonces el sistema no crea un permiso nuevo
    Y muestra "Ya existe el permiso PC-2026-0009 vigente para ese vehículo, ruta y ventana. Dos permisos para una misma circulación rompen la conciliación."

  Escenario: El vehículo de servicio exceptuado no requiere permiso
    Dado un vehículo "Ambulancia institucional" con correlativo "VH-0203" y excepción de servicio exceptuado vigente del "2026-01-01" al "2026-12-31", con fundamento registrado
    Cuando el Jefe de Transporte abre el trámite de permiso para una misión con ese vehículo en franja inhábil
    Entonces el sistema no encamina ningún permiso a la máxima autoridad
    Y registra en la Orden de Misión la excepción con su fundamento y su vigencia
    Y muestra "El vehículo VH-0203 tiene excepción de servicio exceptuado vigente hasta el 31/12/2026. No requiere permiso (RN-24)."

  Escenario: La firma extingue la marca y registra el acto
    Dado un expediente de permiso completo con vehículo "VH-0142" y motorista "José Martínez"
    Cuando "Doris Cruz" firma el permiso
    Entonces el sistema registra identidad, puesto, rol ejercido, marca de tiempo del hecho y de captura, dispositivo y huella del contenido firmado
    Y el permiso queda vigente para ese vehículo, ese motorista, esa ruta y esa ventana
    Y la marca "REQUIERE_PERMISO_MAXIMA_AUTORIDAD" queda extinguida

  Escenario: El permiso cubre la ventana completa de una misión de varios días
    Dado un expediente con ventana del "2026-04-01 08:00" al "2026-04-05 16:00"
    Y tramos inhábiles el "2026-04-02" por feriado, el "2026-04-04" sábado y el "2026-04-05" domingo
    Cuando "Doris Cruz" firma el permiso
    Entonces el permiso ampara la ventana completa del "2026-04-01 08:00" al "2026-04-05 16:00"
    Y enumera los 3 tramos inhábiles cubiertos
```

## Fuera de alcance

- La **emisión e impresión del salvoconducto** con folio y QR — es [HU-017](HU-017-emision-e-impresion-del-salvoconducto.md)
- El **bloqueo del despacho** por falta de permiso (`BD-04`, `T-12`): ocurre al despachar, en [CU-05](../casos-de-uso/CU-05-emitir-orden-de-mision-y-documentos.md). El retraso del trámite se registra **contra el expediente del permiso, no contra el motorista**
- La reemisión del permiso cuando cambia alguno de sus cuatro elementos — es [HU-018](HU-018-reemision-del-permiso-por-cambio-de-elementos.md)
- El **permiso sobreviniente** cuando una prórroga en ruta empuja la misión a franja inhábil no cubierta: es de M-08 y no se bloquea, se registra con justificación

## Notas y pendientes

- `[C]` **¿Es delegable la firma del permiso?** — insumo #29. Hasta confirmarlo, **el sistema no la permite**. Si la institución confirma que sí, se habilita por [`RN-07`](../../01-negocio/reglas/RN-07-delegacion-de-autorizacion.md) con vigencia acotada y folio del acto
- `[C]` Si la institución tiene **vehículos de servicio exceptuado** y quién declara esa condición — insumo #1
- `[C]` Si el permiso de una misión de varios días debe fraccionarse en permisos diarios — insumo #1. Por defecto **no se fracciona**
- `[V]` La exigencia de permiso firmado por la máxima autoridad consta en [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md); `[P]` el rango de multas; `[C]` la cita completa del decreto. El eslabón débil está declarado en [`RN-23`](../../01-negocio/reglas/RN-23-permiso-de-circulacion-en-dia-inhabil.md), hallazgo `HN1-19`
- **Hallazgo abierto:** `BD-04` no contempla el vehículo de servicio exceptuado y exigiría permiso en todos los casos — una ambulancia con excepción vigente no podría despacharse un domingo. Registrado como `HB1-21`, autoridad: [máquina de estados](../../03-arquitectura/estados/orden-de-mision.md)
- Trazabilidad: [CU-03](../casos-de-uso/CU-03-permiso-de-circulacion-en-dia-inhabil.md) pasos 1 a 7, flujos A1 y A2, excepción E1; notas `HCU-03` y `HCU-05`

---

## Nota de alineación con la autoridad

> Una versión anterior de esta historia describía el salvoconducto como amparando **vehículo, motorista, ruta y ventana**, y exigía reemisión tras un relevo de motorista.
>
> **`BD-04` de la [máquina de estados](../../03-arquitectura/estados/orden-de-mision.md) — autoridad en precondiciones — dice que ampara vehículo, ruta y ventana.** El motorista figura impreso en el documento, pero **un relevo documentado no invalida el permiso** y no exige reemisión.
>
> La razón es operativa: si el relevo invalidara el permiso, un motorista incapacitado un domingo en carretera dejaría el vehículo varado esperando otra firma de la máxima autoridad — un bien del Estado abandonado en la vía es peor que el riesgo que el permiso controla. Ver hallazgo `HB3-07`.
>
> `[C]` Pendiente de confirmar con Auditoría Interna: `NRM-02` no precisa el alcance del permiso. Si la institución exige que sea nominativo por motorista, se revierte y hay que diseñar la salida para el relevo en día inhábil.
