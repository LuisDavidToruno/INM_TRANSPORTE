# HU-041 — Emitir la asignación de fondo al programar y entregarla al motorista contra firma dentro del despacho

| Campo | Valor |
|---|---|
| **Módulo** | M-09 Combustible · M-07 Programación y Despacho |
| **Actor** | ACT-07 Encargado de Combustible · ACT-06 Motorista (recibe) · ACT-05 Encargado de Despacho |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Refinada |
| **Deriva de** | [CU-04](../casos-de-uso/CU-04-programar-mision-asignar-vehiculo-y-motorista.md) paso 11 · [CU-06](../casos-de-uso/CU-06-despachar-y-registrar-salida.md) paso 9, A1, E5 · `V-01`, `V-02` · `EF-04` |

## Historia

**Como** Encargado de Combustible
**quiero** emitir la asignación de fondo cuando la misión se programa y conservarla bajo mi custodia hasta entregarla al motorista **dentro del acto de despacho, contra firma de recepción**
**para** que no haya dinero público ni vales fuera de la caja antes de que exista un vehículo saliendo, y para que cada lempira entregado tenga un receptor con nombre y firma

## Contexto

La corrección `HB1-06` fijó el momento exacto de la entrega, y no es un detalle burocrático: entre `DESPACHADA` y `EN_RUTA` hay **bienes y dinero público entregados sin ejecución que los justifique**. Es el estado de mayor exposición de todo el ciclo, y hay que hacerlo lo más corto posible.

Por eso los dos momentos están separados: la asignación se **emite** al programar —con folio propio, monto o galonaje y misión vinculada— pero el instrumento **no sale de la custodia del Encargado de Combustible**. Se **entrega** dentro de `T-12`, con el motorista presente, contra firma.

Y si el motorista no se presenta o no firma, **no hay entrega**: la asignación permanece emitida en custodia y la misión no avanza. Si la misión después se cae, el camino es devolverla a la cola o anularla — **no** la anulación con devolución de fondos, porque nada salió de la caja.

## Reglas que la gobiernan

- [RN-32](../../01-negocio/reglas/RN-32-entrega-de-combustible-contra-orden-de-mision.md) — **Regla rectora**: no se entrega combustible sin Orden de Misión, y solo al vehículo y motorista de esa orden
- [RN-27](../../01-negocio/reglas/RN-27-asignacion-de-combustible-con-folio.md) — Folio único, responsable receptor, misión vinculada y constancia de recepción
- [RN-26](../../01-negocio/reglas/RN-26-fondo-de-combustible-aprobado.md) — Sin fondo vigente aprobado no hay asignación
- [RN-88](../../01-negocio/reglas/RN-88-saldo-proyectado-del-fondo.md) — El saldo se presenta con el comprometido proyectado, y la alerta se dispara sobre el proyectado
- [RN-01](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md) — Quien entrega el fondo no es quien despacha, ni el motorista, ni quien liquida (`I-08`, `I-10`, `I-11`)
- [RN-04](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md) — La anulación de una asignación es asiento reverso con motivo y autor

## Casos especiales que la afectan

- [CE-23](../casos-especiales/CE-23-fondo-agotado-con-misiones-programadas.md) — El fondo se agota con misiones ya programadas
- [CE-20](../casos-especiales/CE-20-mision-cancelada-con-combustible-ya-entregado.md) — Cualquier consumo cambia el camino de la anulación
- [CE-26](../casos-especiales/CE-26-sobrante-o-faltante-al-liquidar.md) — El monto entregado congelado es la base de la liquidación

## Criterios de aceptación

```gherkin
# language: es
Característica: Emisión y entrega del fondo de combustible de la misión

  Antecedentes:
    Dada una Orden de Misión "OM-2026-0451" en estado "PROGRAMADA"
    Y un motorista asignado "José Martínez"
    Y una Encargada de Combustible "Delmy Cruz"
    Y un fondo de combustible del período aprobado por la Gerencia Administrativa,
      con saldo disponible de "15000.00" lempiras y comprometido proyectado de "12000.00" lempiras

  Escenario: Se rechaza emitir la asignación sin fondo vigente aprobado
    Dado que no hay fondo de combustible vigente aprobado para el período
    Cuando la Encargada de Combustible intenta emitir la asignación de "OM-2026-0451"
    Entonces el sistema rechaza la emisión
    Y muestra "No hay fondo de combustible vigente aprobado por la Gerencia Administrativa para este período."

  Escenario: Se advierte sobre el saldo proyectado antes de emitir
    Dada una asignación propuesta de "4000.00" lempiras
    Cuando la Encargada de Combustible emite la asignación de "OM-2026-0451"
    Entonces el sistema muestra la advertencia "Con esta asignación el comprometido proyectado sube a 16,000.00 lempiras y supera el saldo disponible de 15,000.00 lempiras."
    Y exige acuse antes de confirmar

  Escenario: En PROGRAMADA el instrumento no sale de la custodia del Encargado de Combustible
    Dada una asignación emitida con folio "AC-2026-0233" para "OM-2026-0451"
    Cuando el motorista "José Martínez" solicita retirar los vales antes del día del despacho
    Entonces el sistema rechaza la entrega
    Y muestra "La asignación AC-2026-0233 está EMITIDA y permanece en custodia del Encargado de Combustible. Se entrega dentro del acto de despacho, contra firma."
    Y la asignación permanece en estado "EMITIDA"

  Escenario: Se rechaza la entrega a una persona distinta del motorista de la orden
    Dada una asignación emitida con folio "AC-2026-0233" para "OM-2026-0451"
    Cuando la Encargada de Combustible intenta entregarla a "Marvin Discua",
      que no es motorista de esa orden
    Entonces el sistema rechaza la entrega
    Y muestra "La asignación AC-2026-0233 corresponde a la Orden de Misión OM-2026-0451, cuyo motorista es José Martínez."

  Escenario: Sin firma de recepción no hay entrega y la misión no avanza
    Dado que el motorista "José Martínez" no se presentó al predio
    Cuando la Encargada de Combustible intenta cerrar la entrega sin firma de recepción
    Entonces el sistema rechaza la entrega
    Y muestra "Sin firma de recepción del motorista no hay entrega. La asignación permanece EMITIDA en custodia."
    Y la Orden de Misión no pasa a "DESPACHADA"
    Y no se emiten documentos ni se consume el folio
    Y el hecho queda registrado con su motivo

  Escenario: Se entrega el fondo dentro del despacho, contra firma
    Dado que el despacho de "OM-2026-0451" está en curso con el motorista presente
    Cuando la Encargada de Combustible entrega "4000.00" lempiras contra la firma
      de recepción de "José Martínez"
    Entonces la asignación "AC-2026-0233" pasa al estado "ENTREGADA"
    Y el monto entregado queda congelado con la misión
    Y la constancia de asignación impresa lleva folio "AC-2026-0233", el receptor,
      la misión vinculada y el espacio de firma

  Escenario: Se rechaza que quien despachó entregue el fondo
    Dado que "Delmy Cruz" ejecutó el despacho de "OM-2026-0451"
    Cuando "Delmy Cruz" intenta entregar el fondo de esa misión
    Entonces el sistema rechaza la entrega
    Y muestra "Delmy Cruz despachó esta misión. Quien despacha no puede entregar el fondo."

  Escenario: Se despacha sin fondo asignado como decisión registrada
    Dado que el fondo del período está agotado y no hay asignación para "OM-2026-0451"
    Cuando el Encargado de Despacho continúa el despacho declarando el responsable
      de la decisión y su motivo
    Entonces el sistema acepta el despacho
    Y la Orden de Misión impresa declara "Despachada sin fondo de combustible asignado. Responsable de la decisión: <nombre y cargo>."
    Y el hecho queda visible en el expediente
```

## Fuera de alcance

- La solicitud y la aprobación del fondo del período por la Gerencia Administrativa — es de M-09
- El registro del consumo, los comprobantes y la conciliación galonaje–kilometraje — son de M-09 y M-13
- La devolución del saldo y el sobrante o faltante al liquidar — son de M-13
- El traspaso del fondo entre motoristas en un relevo en ruta — es [HU-045](HU-045-relevo-de-motorista-en-ruta.md)

## Notas y pendientes

- `[C]` **Decisiones abiertas de `PROP-01`** — insumo #7: si el fondo es por período o por misión, qué pasa con el saldo entre misiones, cómo se trata el sobrante y si el vale lleva folio preimpreso. La mecánica de esta historia no depende de esas respuestas, **pero el formato de la constancia sí**.
- `[C]` **¿Admite la institución despachar sin fondo asignado?** — insumo #1 / `PROP-01`. La historia asume que sí, como **decisión registrada con responsable nombrado**.
- `[C]` **¿La institución tiene almacenamiento propio de combustible** (cisterna, bidones)? — insumo #36. Cambiaría el circuito completo de M-09.
- `[C]` **Formatos en papel vigentes** del control de combustible — insumo #2.
