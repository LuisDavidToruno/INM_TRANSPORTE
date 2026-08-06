# RN-09 — La categoría de licencia del motorista debe habilitar el tipo, el peso bruto y la capacidad del vehículo asignado

| Campo | Valor |
|---|---|
| **Módulos** | M-05, M-07, M-03 |
| **Origen** | Norma [NRM-06](../normativa/NRM-06-transito-y-licencias.md) — Ley de Tránsito, Decreto 205-2005; decisión [DP-001 D-12](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) |
| **Verificación** | `[V]` las ocho categorías por fuentes concordantes — `[C]` contraste con el texto oficial y con la reforma al Art. 48 (2025) |
| **Tipo** | Bloqueo duro |
| **Configurable** | **No se puede desactivar.** La **matriz** sí es catálogo con vigencia (parámetro `matriz_licencia_vehiculo`) |

## Enunciado

El sistema **no debe** permitir asignar un motorista a un vehículo si ninguna de las categorías vigentes de su licencia habilita ese vehículo según la matriz licencia ↔ vehículo vigente **a la fecha de inicio de la misión**.

La habilitación se resuelve contra los atributos de la ficha técnica del vehículo: **tipo, peso bruto vehicular en kilogramos, capacidad de pasajeros y condición de articulado**. Nunca contra el nombre comercial del modelo ni contra una clasificación manual suelta.

Categorías vigentes conocidas `[V]`:

| Categoría | Habilita |
|---|---|
| A | Ciclomotores y motocicletas |
| B | Automóviles livianos no comprendidos en A ni B1 |
| B1 | Triciclos y cuadriciclos de motor |
| C1 | Carga de hasta 7,500 kg |
| C | Carga superior a 7,500 kg, no articulados |
| D1 | Autobuses de hasta 25 pasajeros |
| D | Autobuses |
| CE | Furgón de carga pesada articulado |

## Justificación

[NRM-06](../normativa/NRM-06-transito-y-licencias.md) la califica como *la validación de mayor valor legal del sistema*. Asignar un motorista sin licencia habilitante **traslada responsabilidad directa a quien autorizó**: ante un siniestro, la institución y el servidor que aprobó responden.

El PO lo confirmó explícitamente en [DP-001 D-12](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md): *"nos tenemos que proteger con la ley también"*, **sin excepción configurable** — porque una excepción registrada sería evidencia en contra ante un siniestro.

## Condiciones de aplicación

Aplica a toda asignación de motorista a vehículo: programación inicial, sustitución en ruta ([RN-14](RN-14-sustitucion-de-motorista.md)), traslado de vehículo a taller, y movimientos internos dentro del predio si implican circulación en vía pública.

**No aplica** al traslado de un vehículo dentro de un predio cerrado por personal de taller. `[C]` confirmar si la institución quiere registrar esos movimientos.

## Comportamiento esperado

1. El sistema evalúa el conjunto de categorías de la licencia contra los atributos técnicos del vehículo y responde con habilitado / no habilitado, mostrando **qué atributo la excluye**: *"La licencia categoría C1 habilita carga de hasta 7,500 kg. El vehículo <correlativo> tiene peso bruto de 9,200 kg y requiere categoría C."*
2. El resultado se **congela en el expediente** con el identificador de la versión de la matriz usada ([RN-41](RN-41-congelamiento-del-valor-al-autorizar.md)).
3. Si al vehículo le falta **peso bruto vehicular** o la condición de articulado, la evaluación **no se puede hacer**: el sistema bloquea la asignación e indica qué dato de la ficha técnica completar. Nunca asume el valor faltante.
4. El resultado no habilitado **no admite anulación por ningún rol**, ni siquiera ACT-09 Máxima Autoridad. No existe pantalla de excepción.
5. El sistema ofrece el listado de motoristas habilitados para un vehículo dado, y de vehículos habilitados para un motorista dado, para que el despacho no trabaje por ensayo y error.

## Casos límite

- **La matriz definitiva aún no está fijada.** El texto de la reforma al Art. 48 (2025) sobre categorías CD y CE está pendiente — insumo #20 y #23. `[C]` Mientras no se cargue una matriz vigente completa, el sistema **bloquea la asignación** e indica que falta el parámetro. No opera con matriz parcial silenciosa.
- **Vehículo en zona gris de clasificación** — panel tipo H-100 o K2700, microbús Sprinter. Es el mismo universo que la SAPP tuvo que resolver para peajes ([NRM-10](../normativa/NRM-10-peajes.md)). La ficha técnica manda: peso bruto y pasajeros. Si el vehículo queda en un límite exacto (por ejemplo 7,500 kg justos), la matriz debe definir el operador de comparación explícitamente; **no se deja al criterio de la implementación**. `[C]` con el texto oficial.
- **Motorista con varias categorías, una de ellas vencida.** Solo se consideran las categorías vigentes en todo el rango ([RN-10](RN-10-licencia-vigente-en-todo-el-rango.md)). Una categoría vencida no habilita nada.
- **Licencia extranjera o permiso provisional.** No está en la matriz. `[C]` confirmar tratamiento; hasta entonces, bloqueo.
- **Vehículo con remolque acoplado ocasionalmente.** El acople cambia el peso bruto y puede volverlo articulado. La evaluación se hace sobre la **configuración de la misión**, no sobre el vehículo en vacío. Si la misión declara remolque, se reevalúa.
- **Cambio de la matriz entre la aprobación y la ejecución.** Se congela la evaluación de la aprobación, pero si la matriz nueva vuelve **no habilitante** al motorista antes de la salida, el sistema alerta al despacho y bloquea el despacho. Salir con una habilitación caduca es exactamente el riesgo legal que la regla evita.
- **Motorista contratado por servicios o de otra institución.** Si conduce un vehículo de la flota, la regla aplica igual. Su licencia debe estar registrada aunque su expediente no venga de Talento Humano. `[C]` confirmar si existe esta figura.

## Trazabilidad

- Norma: [NRM-06 — Tránsito, licencias, matrícula y siniestros](../normativa/NRM-06-transito-y-licencias.md)
- Decisión: [DP-001, D-12](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md)
- Reglas relacionadas: [RN-10](RN-10-licencia-vigente-en-todo-el-rango.md), [RN-11](RN-11-restricciones-medicas-del-motorista.md), [RN-14](RN-14-sustitucion-de-motorista.md), [RN-33](RN-33-categoria-de-peaje-derivada-de-ficha-tecnica.md), [RN-39](RN-39-parametros-normativos-con-vigencia.md)
- Actores: ACT-04, ACT-05, ACT-06
- Historias y casos especiales: pendientes — Bloque 2
