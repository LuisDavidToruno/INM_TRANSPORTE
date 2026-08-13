# RNF-05 — Todo parámetro normativo tiene vigencia por rango de fechas y todo cálculo usa la tabla vigente a la fecha del hecho

| Campo | Valor |
|---|---|
| **Categoría** | Auditoría / Portabilidad |
| **Prioridad** | Crítico |
| **Origen** | Premisa rectora 6 de [CLAUDE.md](../../../CLAUDE.md); [NRM-10](../../01-negocio/normativa/NRM-10-peajes.md) (tarifas de peaje revisadas periódicamente); [NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md) (feriados) |
| **Afecta arquitectura** | **Sí — determinante para la selección de stack (Sprint 2).** Es bitemporalidad: condiciona el modelo de datos completo |

## Enunciado

Ningún valor normativo se cablea. Tarifas de peaje, categorías por número de ejes, feriados, horario hábil, plazos de solicitud y de liquidación, umbrales de desviación de consumo, matriz licencia↔vehículo y cualquier otro parámetro que la norma o la institución puedan cambiar, **son datos con vigencia por rango de fechas**.

Todo cálculo **debe** resolverse contra la versión del parámetro **vigente a la fecha del hecho**, nunca a la fecha de captura. Al autorizar, el valor calculado **se congela** junto con el identificador de la versión de tabla que lo produjo. Una corrección posterior de la tabla **no reescribe** el cálculo: genera un **asiento de diferencia**.

Esto tiene dos ejes de tiempo y hay que decirlo con claridad, porque es la fuente de errores más común en sistemas de este tipo:

| Eje | Qué responde |
|---|---|
| **Tiempo del hecho** | ¿Qué tarifa estaba vigente el día que el vehículo cruzó la caseta? |
| **Tiempo del sistema** | ¿Qué sabía el sistema sobre esa tarifa el día que se emitió el reporte? |

Un reporte de agosto sobre hechos de marzo debe poder responder ambas.

## Métrica y umbral

| Métrica | Umbral |
|---|---|
| Parámetros normativos con vigencia por rango de fechas | **100 %.** La lista de parámetros es un catálogo cerrado y auditable, no una convención |
| Valores normativos literales en el código | **0.** Ni tarifas, ni feriados, ni plazos, ni umbrales, ni categorías |
| Cálculos que no registran el identificador de la versión de tabla usada | **0** |
| Montos congelados al autorizar que cambien después por una edición de catálogo | **0** ([`RN-41`](../../01-negocio/reglas/RN-41-congelamiento-del-valor-al-autorizar.md)) |
| Correcciones retroactivas aplicadas sobre el registro original | **0.** Todas producen asiento de diferencia ([`RN-42`](../../01-negocio/reglas/RN-42-correccion-retroactiva-con-asiento-de-diferencia.md)) |
| Solapamiento o hueco entre vigencias del mismo parámetro | **0.** El sistema lo impide al cargar, no lo detecta después |
| Consulta "¿qué valor tenía este parámetro el {fecha}?" disponible para el usuario | Sí, para todo parámetro, en ≤ 2 s |
| Tiempo de alta de una nueva vigencia de tarifa por un usuario administrador | ≤ 10 min, **sin intervención de desarrollo y sin desplegar versión** |
| Cálculos con fecha del hecho anterior a la vigencia más antigua cargada | Se **bloquean** con mensaje explícito. No se extrapola hacia atrás ni se usa la vigencia más cercana |

## Cómo se verifica

1. **Prueba de las dos vigencias** — la prueba central:
   - Se carga la tarifa de un punto de peaje con vigencia hasta el 30 de junio y otra distinta desde el 1 de julio.
   - Se registra, **en agosto**, una misión cuya **fecha del hecho** es el 15 de junio.
   - Se verifica que el cálculo usa la tarifa de junio, que el asiento cita el identificador de esa versión, y que el documento impreso muestra la tarifa de junio.
2. **Prueba de congelamiento**: se autoriza una misión con su estimación de peajes. Después se corrige la tarifa vigente en la fecha del hecho. Se verifica que el monto autorizado **no cambia**, que aparece un asiento de diferencia con motivo y autor, y que el reporte muestra ambos: lo congelado y la diferencia.
3. **Búsqueda de valores cableados**: barrido automatizado del código y de la configuración buscando cifras, listas de fechas y nombres de categorías del dominio. Corre en cada entrega. Toda coincidencia se justifica o se corrige.
4. **Prueba de integridad de vigencias**: se intenta cargar una vigencia que solapa con otra existente y una que deja un hueco de tres días. Ambas deben rechazarse en el momento de la carga, con mensaje que indique con qué vigencia choca.
5. **Prueba del feriado móvil**: se cambia el calendario de feriados —escenario real, porque el "feriado morazánico" de octubre está `[C]` en [NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md)— y se verifica que las misiones ya autorizadas conservan su clasificación de día hábil o inhábil original y que las nuevas usan el calendario nuevo.
6. **Prueba de la fecha anterior al catálogo**: se registra una digitación diferida de un hecho ocurrido antes de la vigencia más antigua cargada. El sistema debe bloquear y decir exactamente qué parámetro le falta y para qué fecha.
7. **Prueba del administrador funcional**: un usuario administrador de la institución —no un desarrollador— carga una nueva tarifa de peaje siguiendo el manual, y se cronometra.

## Consecuencia de no cumplirlo

Es la falla que no se ve hasta que ya destruyó el histórico. Si el catálogo se edita en su lugar en vez de versionarse, el día que suba la tarifa de peaje **todos los reportes de los años anteriores cambian de monto retroactivamente**. Nadie lo nota en el momento; se nota cuando el auditor compara el reporte de hoy con el descargo que la institución presentó hace dos años y los números no coinciden.

En ese punto el daño es irreversible: no hay forma de reconstruir qué valor se usó realmente, porque el valor viejo ya no existe. Y la institución tiene que explicarle al TSC una discrepancia que no cometió.

En sentido inverso, cablear un feriado o un plazo produce el hallazgo barato pero seguro: el sistema clasifica mal un día inhábil, no exige el salvoconducto que [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md) requiere, y el vehículo circula un sábado sin permiso.

## Trazabilidad

- Módulos: M-02 (catálogos maestros), M-18 (peajes), transversal a todo cálculo
- Reglas: [`RN-39`](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md), [`RN-40`](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md), [`RN-41`](../../01-negocio/reglas/RN-41-congelamiento-del-valor-al-autorizar.md), [`RN-42`](../../01-negocio/reglas/RN-42-correccion-retroactiva-con-asiento-de-diferencia.md), [`RN-34`](../../01-negocio/reglas/RN-34-tarifa-de-peaje-por-punto-categoria-vigencia.md), [`RN-46`](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md)
- Normativa: [NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md), [NRM-10](../../01-negocio/normativa/NRM-10-peajes.md), [NRM-06](../../01-negocio/normativa/NRM-06-transito-y-licencias.md)
- Casos especiales: [`CE-24`](../casos-especiales/CE-24-cobro-en-categoria-de-peaje-equivocada.md), [`CE-11`](../casos-especiales/CE-11-licencia-vence-durante-la-mision.md), [`CE-28`](../casos-especiales/CE-28-hallazgo-posterior-sobre-mision-cerrada.md)
- Requisitos relacionados: [`RNF-06`](RNF-06-reproducibilidad-historica-de-reportes.md), [`RNF-19`](RNF-19-configurabilidad-multi-institucion.md)
- Insumos: #20, #21, #22, #32 — ningún parámetro se carga con valor inventado
