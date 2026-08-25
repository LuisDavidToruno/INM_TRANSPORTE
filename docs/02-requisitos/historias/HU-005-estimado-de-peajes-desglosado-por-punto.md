# HU-005 — Estimar el peaje desglosado punto por punto antes de autorizar

| Campo | Valor |
|---|---|
| **Módulo** | M-18 Peajes (con M-06 Solicitudes de Transporte) |
| **Actor** | ACT-02 Solicitante; consumido por ACT-03 Jefatura Inmediata |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Refinada |

## Historia

**Como** Solicitante
**quiero** que el sistema estime el costo de peajes de la ruta, punto por punto, con la categoría del tipo de vehículo requerido y la tarifa vigente a la fecha prevista de cada paso
**para** que quien autorice pueda verificar el cálculo en lugar de aceptar un total que nadie puede reconstruir

## Contexto

El estimado de peajes no existe para adornar la pantalla: existe para que la jefatura autorice conociendo el gasto que compromete. Un total opaco no sirve — quien autoriza tiene que poder ver **qué caseta, qué categoría y qué tarifa** produjo cada lempira.

La categoría **no se deriva del número de ejes por sí solo**: se deriva de la ficha técnica ([`RN-33`](../../01-negocio/reglas/RN-33-categoria-de-peaje-derivada-de-ficha-tecnica.md)). Y la tarifa se resuelve por punto × categoría × vigencia **a la fecha del hecho**, no a la fecha de captura: una solicitud registrada hoy para un viaje de abril usa la tarifa de abril si ya está publicada.

Cuando el dato no es confiable, el sistema **lo dice**. Nunca sustituye una tarifa faltante por una cifra inventada.

## Reglas que la gobiernan

- [RN-33](../../01-negocio/reglas/RN-33-categoria-de-peaje-derivada-de-ficha-tecnica.md) — La categoría de peaje se deriva de la ficha técnica, no del número de ejes por sí solo
- [RN-34](../../01-negocio/reglas/RN-34-tarifa-de-peaje-por-punto-categoria-vigencia.md) — La tarifa se resuelve por punto × categoría × vigencia
- [RN-35](../../01-negocio/reglas/RN-35-estimacion-de-peajes-antes-de-aprobar.md) — El costo de peajes se estima desglosado por punto **antes** de aprobar
- [RN-40](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md) — Todo cálculo usa el parámetro vigente a la fecha del hecho
- [RN-41](../../01-negocio/reglas/RN-41-congelamiento-del-valor-al-autorizar.md) — El estimado se congela con el identificador de la tabla usada
- [RN-38](../../01-negocio/reglas/RN-38-exoneracion-de-peaje.md) — La exoneración es dato por vehículo, punto, fundamento y vigencia; nunca una constante
- [RN-50](../../01-negocio/reglas/RN-50-degradacion-por-sincronizacion-detenida.md) — La antigüedad de la tabla se declara **antes** de mostrar el número

## Casos especiales que la afectan

- [CE-24](../casos-especiales/CE-24-cobro-en-categoria-de-peaje-equivocada.md) — Cobro en categoría distinta a la asignada: aquí se produce la **categoría esperada** contra la que después se detecta la discrepancia

## Criterios de aceptación

```gherkin
# language: es
Característica: Estimado de peajes de la ruta solicitada
  Como Solicitante
  quiero un estimado desglosado por punto de peaje
  para que quien autoriza pueda verificar el cálculo

  Antecedentes:
    Dado un punto de peaje "Zambrano" con tarifa de "L 40.00" para la categoría "Liviano" vigente desde el "2026-01-01"
    Y un punto de peaje "Zambrano" con tarifa de "L 95.00" para la categoría "Camión 2 ejes" vigente desde el "2026-01-01"
    Y un punto de peaje "Villa de San Antonio" con tarifa de "L 35.00" para la categoría "Liviano" vigente desde el "2026-01-01"
    Y un tipo de vehículo requerido "Pickup doble cabina" cuya ficha técnica resuelve la categoría de peaje "Liviano"
    Y un umbral de antigüedad de la tabla de tarifas de "30" días

  Escenario: Se rechaza producir estimado cuando ningún punto de la ruta tiene tarifa cargada
    Dada una ruta "Tegucigalpa–Puerto Lempira" cuyos puntos de peaje no tienen ninguna tarifa cargada
      para la categoría "Liviano"
    Cuando el Solicitante consulta el estimado de peajes de esa ruta
    Entonces el sistema no produce ningún estimado
    Y muestra "Ningún punto de peaje de esta ruta tiene tarifa cargada. No se estima un gasto que quien autorice no podría verificar. Solicite a Catálogos Maestros la carga de las tarifas."
    Y no muestra un total de "L 0.00" ni ninguna cifra sustitutiva
    Y el expediente queda con el estimado marcado "no disponible", no con valor cero

  Escenario: Se declara la antigüedad de la tabla antes de mostrar cualquier número
    Dada una tabla de tarifas sincronizada por última vez el "2026-01-20"
    Y una fecha del sistema del "2026-03-14"
    Cuando el Solicitante consulta el estimado de peajes de la ruta "Tegucigalpa–Comayagua"
    Entonces el sistema muestra "La tabla de tarifas de peaje tiene 53 días sin sincronizar y el umbral es de 30. El estimado puede no corresponder a la tarifa vigente."
    Y muestra esa advertencia antes del monto estimado
    Y deja la advertencia asentada en el diario del expediente

  Escenario: Un punto sin tarifa cargada se rotula y el total se marca incompleto
    Dado un punto de peaje "Jícaro Galán" sin tarifa cargada para la categoría "Liviano"
    Y una ruta "Tegucigalpa–Choluteca" que pasa por "Zambrano" y por "Jícaro Galán"
    Cuando el Solicitante consulta el estimado de peajes de esa ruta
    Entonces el sistema muestra el punto "Jícaro Galán" rotulado como "sin tarifa disponible"
    Y muestra el total como "L 40.00 — incompleto: 1 de 2 puntos sin tarifa"
    Y no sustituye el punto faltante por ninguna cifra estimada

  Escenario: El estimado se desglosa por punto, categoría y tarifa
    Dada una ruta "Tegucigalpa–Comayagua" que pasa por "Zambrano" y por "Villa de San Antonio", ida y vuelta
    Y una salida prevista el "2026-03-20"
    Cuando el Solicitante consulta el estimado de peajes de esa ruta
    Entonces el sistema muestra 4 líneas, una por paso
    Y cada línea indica el punto, la fecha prevista de paso, la categoría "Liviano" y la tarifa aplicada
    Y el total estimado es "L 150.00"

  Escenario: Se usa la tarifa vigente a la fecha prevista de paso, no a la de captura
    Dada una tarifa de "L 45.00" para "Zambrano" y categoría "Liviano" vigente desde el "2026-04-01"
    Y una captura realizada el "2026-03-14"
    Y una salida prevista el "2026-04-05" con un solo paso por "Zambrano"
    Cuando el Solicitante consulta el estimado de peajes de esa ruta
    Entonces el sistema aplica la tarifa de "L 45.00"
    Y muestra la vigencia de la tarifa aplicada junto al monto

  Escenario: La exoneración vigente se aplica por punto y con su fundamento
    Dado un tipo de vehículo requerido "Ambulancia institucional" con exoneración vigente en "Zambrano" desde el "2026-01-01"
    Y una ruta con un solo paso por "Zambrano"
    Cuando el Solicitante consulta el estimado de peajes de esa ruta
    Entonces el sistema muestra el punto "Zambrano" con monto "L 0.00"
    Y muestra el fundamento y la vigencia de la exoneración aplicada

  Escenario: El estimado se congela al enviar, con el identificador de la tabla usada
    Dado un estimado de "L 150.00" calculado con la tabla de tarifas "TAR-2026-01"
    Cuando el Solicitante envía la solicitud a autorización
    Entonces el estimado queda congelado en "L 150.00"
    Y queda registrado el identificador "TAR-2026-01"
    Y una actualización posterior de la tabla no modifica el estimado congelado
```

## Fuera de alcance

- La conciliación del peaje **efectivamente pagado** contra el estimado: es de M-13 y M-18, en la liquidación
- El reclamo por discrepancia de categoría ([`RN-92`](../../01-negocio/reglas/RN-92-reclamo-por-discrepancia-de-peaje.md)) — se abre en ejecución, no aquí
- La impresión de categoría y tarifa esperada en la Orden de Misión ([`RN-91`](../../01-negocio/reglas/RN-91-categoria-y-tarifa-de-peaje-impresas-en-la-orden.md)) — es de M-15 al emitir la orden
- El trazado de la ruta sobre mapa: el componente de mapas es de ARGOS ([DP-001](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) D-02). SIGTI resuelve los puntos de peaje de la ruta declarada
- El mantenimiento del catálogo de puntos y tarifas — es M-02

## Notas y pendientes

- `[C]` **Tarifa de peaje efectivamente vigente** — insumo #21. Los montos de los criterios son de ejemplo: **no se carga ninguna tarifa real sin este insumo**
- `[C]` **Lista oficial de exoneraciones** — insumo #22. Decide si un vehículo administrativo del Estado paga o no
- `[P]` Que el país clasifique por punto y categoría consta en [NRM-10](../../01-negocio/normativa/NRM-10-peajes.md) con verificación parcial. Esta historia **no** eleva ese nivel
- `[C]` Texto del Art. 51 de la Ley de Tránsito, necesario para fijar la derivación de categoría — insumo #23
- Corregido por `HB34-20`: la historia no tenía **ningún camino de rechazo** y estaba marcada `Refinada`. El `DoR` lo exige sin excepción. El rechazo agregado es el que la propia historia insinuaba en su contexto —*«cuando el dato no es confiable, el sistema lo dice; nunca sustituye una tarifa faltante por una cifra inventada»*— llevado a su extremo: **con la tabla completamente vacía no se emite estimado, y el estimado ausente no es cero**. Un total de `L 0.00` en el expediente es un gasto que la jefatura autoriza creyendo que no existe.
- `[C]` **Si la ausencia total de tarifas debe además impedir el envío de la solicitud.** Se adopta que **no**: el estimado queda *no disponible* y el expediente se encamina, porque bloquear el envío por un catálogo que la institución todavía no cargó paraliza la operación. **Reversible** si el PO decide lo contrario; el bloqueo del envío es materia de [HU-004](HU-004-envio-a-autorizacion-con-numero-de-expediente-y-congelamiento.md) — insumo #21
- Trazabilidad: [CU-01](../casos-de-uso/CU-01-registrar-solicitud-de-transporte.md) paso 8, excepción E5; [CU-02](../casos-de-uso/CU-02-autorizar-solicitud-de-transporte.md) paso 3
