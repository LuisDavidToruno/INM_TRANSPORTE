# HU-147 — Resolver todo parámetro a la fecha del hecho, y bloquear cuando no hay vigencia aprobada para esa fecha

| Campo | Valor |
|---|---|
| **Módulo** | M-02 Catálogos Maestros |
| **Actor** | ACT-04 Jefe de Transporte · ACT-07 Encargado de Combustible · ACT-10 Encargado de Delegación |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — depende de [HU-144](HU-144-cargar-parametro-normativo-con-vigencia.md) y [HU-145](HU-145-aprobar-la-puesta-en-vigencia-doble-control.md) |

## Historia

**Como** Jefe de Transporte
**quiero** que todo cálculo use el parámetro vigente **a la fecha del hecho** —no a la de captura ni a la de consulta— y que cuando no exista vigencia aprobada para esa fecha el sistema bloquee diciendo exactamente qué falta
**para** que una digitación diferida de hace tres semanas no se calcule con la tarifa de hoy, y para que nadie tenga que adivinar por qué el sistema no le deja continuar

## Contexto

Es `RN-40` y es la mitad operativa de [`RNF-05`](../no-funcionales/RNF-05-temporalidad-normativa.md). El escenario que hay que sostener: se carga la tarifa de un punto con vigencia hasta el 30 de junio y otra distinta desde el 1 de julio; **en agosto** se registra una misión cuya **fecha del hecho** es el 15 de junio. El cálculo debe usar la tarifa de junio, el asiento debe citar la versión que la produjo, y el documento impreso debe mostrar la de junio.

En operación desconectada el asunto se agudiza: el dispositivo lleva días sin red y calcula contra el **paquete normativo congelado** que recibió en el despacho, no contra la tabla actual del servidor ([`RN-41`](../../01-negocio/reglas/RN-41-congelamiento-del-valor-al-autorizar.md), [HU-046](HU-046-operar-la-mision-sin-conectividad.md)).

Y el comportamiento ante el hueco es tajante: **se bloquea, no se extrapola.** `RNF-05`: *"Cálculos con fecha del hecho anterior a la vigencia más antigua cargada: se **bloquean** con mensaje explícito. No se extrapola hacia atrás ni se usa la vigencia más cercana."* Usar la vigencia más cercana es el defecto que produce un número plausible y falso, y nadie lo detecta.

## Reglas que la gobiernan

- [RN-40](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md) — **Regla rectora**: todo cálculo usa el parámetro vigente a la fecha del hecho
- [RN-39](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md) — Un parámetro sin valor vigente **y aprobado** a la fecha del hecho bloquea el cálculo con mensaje accionable
- [RN-41](../../01-negocio/reglas/RN-41-congelamiento-del-valor-al-autorizar.md) — El valor se congela al autorizar, junto con el identificador de la tabla usada
- [RN-46](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md) — Fecha del hecho y fecha de captura son campos distintos y ambos obligatorios
- [RN-34](../../01-negocio/reglas/RN-34-tarifa-de-peaje-por-punto-categoria-vigencia.md) — La tarifa se resuelve por punto × categoría × vigencia

## Casos especiales que la afectan

- [CE-09](../casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md) — La digitación diferida es donde la distinción entre las dos fechas se paga
- [CE-24](../casos-especiales/CE-24-cobro-en-categoria-de-peaje-equivocada.md) — La discrepancia se mide contra la tarifa vigente el día del paso
- [CE-27](../casos-especiales/CE-27-cierre-de-ejercicio-fiscal-con-hallazgo-abierto.md) — Un hecho del ejercicio anterior se calcula con la tabla de ese ejercicio

## Criterios de aceptación

```gherkin
# language: es
Característica: Resolución del parámetro a la fecha del hecho

  Antecedentes:
    Dada una tarifa del punto "Zambrano", categoría "liviana", de L 22.00 vigente del "2026-01-01" al "2026-06-30", aprobada
    Y una tarifa del mismo punto y categoría de L 25.00 vigente desde el "2026-07-01", aprobada
    Y la fecha del sistema del "2026-08-14"

  Escenario: Se bloquea el cálculo cuando la fecha del hecho no tiene vigencia cargada
    Dado un paso por caseta digitado con fecha del hecho del "2025-11-20"
    Cuando el Encargado de Delegación registra el paso
    Entonces el sistema bloquea el cálculo
    Y muestra "No hay tarifa cargada y aprobada para el punto Zambrano, categoría liviana, al 20/11/2025. La vigencia más antigua cargada empieza el 01/01/2026. Cargue la tarifa de esa fecha antes de digitar."
    Y no usa la vigencia más cercana ni calcula cero

  Escenario: Se bloquea cuando solo existe una carga pendiente de aprobación para esa fecha
    Dada una tarifa de L 28.00 desde el "2026-08-01" cargada y **no** aprobada
    Cuando el motorista registra un paso con fecha del hecho del "2026-08-10"
    Entonces el sistema aplica L 25.00
    Y muestra "Tarifa aplicada: L 25.00, vigente y aprobada. La carga de L 28.00 del 01/08/2026 está pendiente de aprobación y no se aplica."

  Escenario: El hecho de junio digitado en agosto usa la tarifa de junio
    Dado un paso por caseta con fecha del hecho del "2026-06-15" digitado el "2026-08-14"
    Cuando el Encargado de Delegación lo registra
    Entonces el sistema calcula con L 22.00
    Y el asiento cita el identificador de la versión de tarifa vigente el "2026-06-15"
    Y muestra "Tarifa aplicada: L 22.00, vigente del 01/01/2026 al 30/06/2026, a la fecha del hecho 15/06/2026."

  Escenario: El documento impreso muestra la tarifa de la fecha del hecho
    Dado el paso del "2026-06-15" ya registrado
    Cuando el Jefe de Transporte reimprime el descargo de la misión el "2026-08-20"
    Entonces el documento muestra L 22.00
    Y no muestra L 25.00

  Escenario: El valor congelado al autorizar no cambia por una carga posterior
    Dada una misión autorizada el "2026-06-20" con estimación de peajes por L 66.00 calculada con la tarifa de junio
    Cuando se aprueba una tarifa nueva el "2026-07-01"
    Entonces el monto autorizado sigue siendo L 66.00
    Y el asiento conserva el identificador de la versión usada
    Y el sistema no recalcula nada de forma automática

  Escenario: En ruta sin conectividad se usa el paquete normativo congelado
    Dado un paquete de misión entregado el "2026-06-12" con la tarifa de junio
    Y un dispositivo sin conectividad desde el "2026-06-12"
    Cuando el motorista registra el paso por "Zambrano" el "2026-06-15"
    Entonces el dispositivo calcula con L 22.00 del paquete congelado
    Y no intenta consultar el servidor
    Y al sincronizar, el servidor confirma que la versión usada es la vigente a la fecha del hecho

  Escenario: El feriado corregido no reclasifica misiones ya autorizadas
    Dada una misión autorizada el "2026-10-01" clasificada como día hábil
    Cuando el calendario de feriados se corrige el "2026-11-10" declarando feriado ese día
    Entonces la misión conserva su clasificación original de día hábil
    Y las misiones nuevas usan el calendario corregido
    Y el sistema lista las misiones alcanzadas para que la Gerencia Administrativa decida

  Escenario: El usuario puede consultar qué valor tenía un parámetro en una fecha
    Cuando el Jefe de Transporte consulta la tarifa del punto "Zambrano", categoría "liviana", al "2026-03-08"
    Entonces el sistema responde L 22.00 con su rango de vigencia, su fuente y quién la aprobó
```

## Fuera de alcance

- La carga y la aprobación del parámetro — es [HU-144](HU-144-cargar-parametro-normativo-con-vigencia.md) y [HU-145](HU-145-aprobar-la-puesta-en-vigencia-doble-control.md)
- Los asientos de diferencia de una corrección retroactiva — es [HU-148](HU-148-correccion-retroactiva-con-asiento-de-diferencia.md)
- El paquete normativo congelado que viaja al dispositivo — es [HU-046](HU-046-operar-la-mision-sin-conectividad.md)
- La estimación de peajes en la solicitud — es [HU-005](HU-005-estimado-de-peajes-desglosado-por-punto.md)

## Notas y pendientes

- `[V]` Que las tarifas de peaje cambian y pueden aplicarse retroactivamente — [NRM-10](../../01-negocio/normativa/NRM-10-peajes.md). `[P]` la tarifa concreta de L 22.00 — insumo **#21**
- `[C]` **Plazo máximo de digitación diferida en días hábiles.** Sin él, una delegación sin red puede acumular hechos cuya tabla ya nadie recuerda — insumo **#45**
- `[C]` Legislación de feriados de octubre — [NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md)
- `[C]` Criterio de imputación entre ejercicios fiscales cuando el hecho es de un ejercicio y la captura de otro — [`RN-96`](../../01-negocio/reglas/RN-96-cierre-de-ejercicio-como-corte-de-imputacion.md)
