# Plantilla — Regla de negocio

Archivo: `docs/01-negocio/reglas/RN-xx-slug-corto.md`

Una regla de negocio es una afirmación **verificable**: se puede escribir una prueba que determine si el sistema la cumple o no. Si no se puede probar, no es una regla — es un principio de diseño, y va en otro lado.

---

## Esqueleto

```markdown
# RN-xx — <Enunciado en una línea, en presente e imperativo>

| Campo | Valor |
|---|---|
| **Módulos** | M-xx, M-xx |
| **Origen** | Norma `NRM-xx` / Decisión de producto / Práctica de la institución |
| **Verificación** | `[V]` `[P]` `[C]` `[I]` |
| **Tipo** | Bloqueo duro / Advertencia / Cálculo / Derivación |
| **Configurable** | Sí (parámetro `<nombre>`) / No |

## Enunciado

<La regla, escrita de forma que no admita interpretación. Usa "debe" / "no debe".>

## Justificación

<Por qué existe. Si viene de una norma, cita la ficha y qué exige. Si viene de la
práctica, di qué problema real evita.>

## Condiciones de aplicación

<Cuándo aplica y cuándo no. Sé explícito con las excepciones.>

## Comportamiento esperado

<Qué hace el sistema exactamente: qué bloquea, qué calcula, qué mensaje muestra,
qué registra.>

## Casos límite

<Los bordes donde la regla se vuelve ambigua, y cómo se resuelven.>

## Trazabilidad

- Norma: [NRM-xx](../normativa/NRM-xx-slug.md)
- Historias: [HU-xxx](../../02-requisitos/historias/HU-xxx-slug.md)
- Casos especiales: [CE-xx](../../02-requisitos/casos-especiales/CE-xx-slug.md)
```

---

## Ejemplo completo

# RN-14 — El viático se calcula con la tabla de tarifas vigente a la fecha del viaje

| Campo | Valor |
|---|---|
| **Módulos** | M-10 Viáticos y Gastos de Viaje |
| **Origen** | Norma [NRM-03](../01-negocio/normativa/NRM-03-viaticos.md) |
| **Verificación** | `[V]` la existencia del reglamento — `[C]` las tarifas concretas |
| **Tipo** | Cálculo |
| **Configurable** | Sí — tabla `tarifa_viatico` con vigencia por rango de fechas |

## Enunciado

El monto de viático de una Orden de Misión **debe** calcularse con la tabla de tarifas vigente en la **fecha de inicio del viaje**, y no con la vigente en la fecha en que se captura, autoriza o liquida la solicitud.

Cuando una misión cruza un cambio de vigencia de tarifas, **cada noche se valora con la tarifa vigente esa noche**.

## Justificación

El Reglamento de Viáticos del Poder Ejecutivo fue actualizado por **Acuerdo No. 401-2026 del 23 de julio de 2026** `[V]`. Esto no es un evento aislado: el reglamento se ha reformado varias veces y volverá a hacerlo.

Si las tarifas se cablean en el código o se guardan sin vigencia, tres cosas fallan a la vez: los viajes históricos se recalculan mal al consultarlos, la liquidación de un viaje anterior al cambio se hace con la tarifa nueva, y ante una auditoría el sistema no puede justificar el monto que pagó.

## Condiciones de aplicación

Aplica a todo cálculo de viático: propuesta, anticipo, liquidación y recálculo por extensión de misión.

**No aplica** a los gastos conexos con comprobante (combustible, peaje, hospedaje facturado), que se liquidan por su valor real, no por tarifa.

## Comportamiento esperado

1. Al calcular, el sistema resuelve la tarifa con `zona destino` + `categoría del servidor` + `fecha de la noche`.
2. Si no existe tarifa vigente para esa combinación en esa fecha, **no calcula un valor por defecto**: bloquea con el mensaje "No hay tarifa vigente para la zona <X>, categoría <Y>, a la fecha <fecha>. Solicite a la Gerencia Administrativa que registre la tabla vigente."
3. El monto calculado se **congela** al autorizar y se guarda junto con el identificador de la tabla de tarifas usada. Una consulta posterior muestra el monto histórico, no un recálculo.
4. Todo cálculo deja registro de: tabla de tarifas usada, tarifa unitaria, número de noches y desgloses aplicados.

## Casos límite

- **Misión que cruza el cambio de vigencia**: cada noche con su tarifa. El desglose se muestra al usuario, no solo el total.
- **Tarifa modificada retroactivamente** por un acuerdo publicado después: **no se recalcula automáticamente** ninguna misión ya liquidada. Se genera un reporte de misiones afectadas y la Gerencia Administrativa decide. Un recálculo silencioso alteraría registros contables cerrados.
- **Viaje sin pernocta**: si el reglamento vigente contempla un porcentaje por viaje sin pernocta, es otro parámetro con vigencia, no una constante. `[C]` confirmar tratamiento en el Acuerdo 401-2026.
- **Extensión de misión** que agrega noches: las noches nuevas se valoran con la tarifa vigente en cada una de ellas, no con la de la fecha original de inicio.

## Trazabilidad

- Norma: [NRM-03 — Viáticos y gastos de viaje](../01-negocio/normativa/NRM-03-viaticos.md)
- Reglas relacionadas: `RN-15` (plazo de liquidación), `RN-16` (bloqueo por liquidación vencida), `RN-17` (tope máximo no superable)
- Historias: `HU-041`, `HU-043`
- Casos especiales: `CE-16` (misión que se extiende), `CE-22` (cambio de reglamento a mitad de ejercicio)
