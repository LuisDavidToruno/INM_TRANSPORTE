# HU-144 — Cargar un parámetro normativo con su rango de vigencia, su fuente y su respaldo documental, sin solapes ni huecos

| Campo | Valor |
|---|---|
| **Módulo** | M-02 Catálogos Maestros |
| **Actor** | ACT-01 Administrador del Sistema |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — los valores reales de casi todos los parámetros están `[C]` y **no se inventan** |

## Historia

**Como** Administrador del Sistema
**quiero** cargar una tarifa, un umbral, un plazo o un feriado con su rango de vigencia, la fuente de la que lo tomé y el respaldo documental adjunto, en menos de diez minutos y sin desplegar una versión
**para** que el día que suba la tarifa de peaje el sistema pueda calcular bien esa misma tarde, en vez de esperar semanas a un despliegue que nadie va a priorizar

## Contexto

Es el corazón de [`RNF-05`](../no-funcionales/RNF-05-temporalidad-normativa.md) y no tiene ninguna historia. La evidencia de por qué importa está en `RN-39` y es abrumadora: la tarifa de peaje se revisó **tres veces en 2026 y se revirtió** `[V]`; la Ley de Tránsito se reformó en 2025 en las categorías `CD` y `CE` `[V]`; la legislación de feriados de octubre **no se pudo verificar** `[C]`.

Dos exigencias que se validan **al cargar** y no después ([`RNF-05`](../no-funcionales/RNF-05-temporalidad-normativa.md)): *"Solapamiento o hueco entre vigencias del mismo parámetro: **0**. El sistema lo impide al cargar, no lo detecta después."* Un hueco de tres días entre dos vigencias es una fecha del hecho sin tabla resoluble, y eso se descubre meses más tarde cuando alguien digita en diferido.

Y una que se valida al operar: **cargar no es poner en vigencia**. El parámetro cargado existe y **no se aplica** hasta que `ACT-08` lo apruebe ([HU-145](HU-145-aprobar-la-puesta-en-vigencia-doble-control.md)).

## Reglas que la gobiernan

- [RN-39](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md) — **Regla rectora**: todo dato normativo es parámetro con vigencia, con fuente, respaldo adjunto y doble control
- [RN-40](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md) — El cálculo usa la versión vigente a la fecha del hecho
- [RN-42](../../01-negocio/reglas/RN-42-correccion-retroactiva-con-asiento-de-diferencia.md) — La carga con vigencia retroactiva dispara el análisis de impacto
- [RN-34](../../01-negocio/reglas/RN-34-tarifa-de-peaje-por-punto-categoria-vigencia.md) — La tarifa se resuelve por punto × categoría × vigencia
- [RN-04](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md) — La corrección de un parámetro mal cargado es asiento reverso, no edición

## Casos especiales que la afectan

- [CE-24](../casos-especiales/CE-24-cobro-en-categoria-de-peaje-equivocada.md) — La discrepancia se evalúa contra la tarifa vigente a la fecha del paso
- [CE-11](../casos-especiales/CE-11-licencia-vence-durante-la-mision.md) — La matriz licencia↔vehículo es uno de estos parámetros

## Criterios de aceptación

```gherkin
# language: es
Característica: Carga de un parámetro normativo con vigencia

  Antecedentes:
    Dado un parámetro "tarifa_peaje" del punto "Zambrano", categoría "liviana", con una vigencia de L 22.00 del "2026-01-01" al "2026-06-30", cargada y aprobada
    Y el Administrador del Sistema autenticado

  Escenario: Se rechaza la carga sin respaldo documental adjunto
    Cuando el Administrador del Sistema carga una tarifa de L 25.00 desde el "2026-07-01" sin adjuntar el respaldo
    Entonces el sistema rechaza la carga
    Y muestra "Adjunte el respaldo documental: comunicado, acuerdo o tabla oficial. Un parámetro sin respaldo no se puede sostener ante el Tribunal Superior de Cuentas."

  Escenario: Se rechaza la carga sin declarar la fuente
    Cuando el Administrador del Sistema carga la tarifa con adjunto pero sin declarar la fuente
    Entonces el sistema rechaza la carga
    Y muestra "Declare la fuente del dato y la fecha en que se verificó."

  Escenario: Se rechaza la vigencia que solapa con otra
    Cuando el Administrador del Sistema carga una tarifa de L 25.00 con vigencia desde el "2026-06-15"
    Entonces el sistema rechaza la carga
    Y muestra "La vigencia desde el 15/06/2026 solapa con la tarifa de L 22.00 vigente del 01/01/2026 al 30/06/2026. Cierre la anterior el 14/06/2026 o inicie esta el 01/07/2026."

  Escenario: Se rechaza la vigencia que deja un hueco
    Cuando el Administrador del Sistema carga una tarifa de L 25.00 con vigencia desde el "2026-07-04"
    Entonces el sistema rechaza la carga
    Y muestra "Quedaría un hueco del 01/07/2026 al 03/07/2026 sin tarifa vigente para el punto Zambrano, categoría liviana. Todo hecho de esos tres días quedaría sin poder calcularse."

  Escenario: Se rechaza cargar un valor sin ámbito cuando el parámetro lo exige
    Cuando el Administrador del Sistema carga el parámetro "horario_habil" sin declarar si aplica a la institución o a una delegación
    Entonces el sistema rechaza la carga
    Y muestra "Declare el ámbito del parámetro. La resolución busca del más específico al más general y una delegación puede tener horario distinto."

  Escenario: La carga válida queda pendiente de aprobación y no se aplica
    Cuando el Administrador del Sistema carga una tarifa de L 25.00 del "2026-07-01" en adelante, con fuente "comunicado de la SIT del 28/06/2026" y respaldo adjunto
    Entonces el sistema registra la carga con autor y momento
    Y el parámetro figura en estado "PENDIENTE DE APROBACIÓN"
    Y ningún cálculo lo resuelve, en ninguna fecha
    Y muestra "Cargado. Mientras la Gerencia Administrativa no apruebe su puesta en vigencia, rige la tarifa anterior de L 22.00."

  Escenario: La vigencia abierta hacia el futuro se cierra sola al cargar la siguiente
    Dada una tarifa de L 25.00 vigente desde el "2026-07-01" sin fecha de fin, aprobada
    Cuando el Administrador del Sistema carga una tarifa de L 28.00 desde el "2026-11-01"
    Entonces el sistema cierra la vigencia anterior el "2026-10-31"
    Y deja asiento del cierre con autor y momento
    Y no modifica el valor de la vigencia anterior

  Escenario: La carga con vigencia retroactiva advierte el impacto antes de confirmar
    Dada la fecha del sistema del "2026-09-20"
    Cuando el Administrador del Sistema carga una tarifa con vigencia desde el "2026-03-01"
    Entonces el sistema calcula y muestra el impacto antes de confirmar
    Y muestra "Vigencia retroactiva de 203 días. Alcanza 412 misiones por un efecto estimado de L 8,240.00. Al aprobarse generará asientos de diferencia, no recálculo silencioso."
    Y exige confirmación explícita

  Escenario: La corrección de un parámetro mal cargado no edita el valor
    Dada una tarifa cargada con "2025" en lugar de "2026" por error de digitación
    Cuando el Administrador del Sistema la corrige
    Entonces el sistema registra un asiento reverso con motivo y autor
    Y la corrección exige la misma aprobación de la Gerencia Administrativa que la carga original
    Y el valor erróneo permanece consultable con su marca de reversión
```

## Fuera de alcance

- La aprobación de la puesta en vigencia — es [HU-145](HU-145-aprobar-la-puesta-en-vigencia-doble-control.md)
- El bloqueo de que el cargador apruebe su propia carga — es [HU-146](HU-146-bloquear-que-quien-carga-apruebe-su-propia-carga.md)
- La resolución del parámetro al calcular — es [HU-147](HU-147-resolver-el-parametro-a-la-fecha-del-hecho.md)
- Los asientos de diferencia que produce la vigencia retroactiva — es [HU-148](HU-148-correccion-retroactiva-con-asiento-de-diferencia.md)

## Notas y pendientes

- `[V]` Que las tarifas de peaje se revisan periódicamente y pueden aplicarse retroactivamente — [NRM-10](../../01-negocio/normativa/NRM-10-peajes.md)
- `[P]` La tarifa concreta de L 22.00 para categoría liviana tiene respaldo del regulador; **el congelamiento vigente no está confirmado** — insumo **#21**. Los valores de los criterios son datos de prueba
- `[C]` Umbrales de desviación de consumo, horario hábil, plazos de liquidación y antelación mínima de solicitud — insumo **#32**
- `[C]` Calendario de feriados, incluido el feriado de octubre, cuya legislación no se pudo verificar — [NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md)
- `[C]` Lista oficial de exoneraciones de peaje — insumo **#22**
- `[C]` **¿Quiere la institución un plazo máximo de aprobación con alerta de vencimiento?** `RN-39` lo deja abierto en sus casos límite
