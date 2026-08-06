# RN-33 — La categoría de peaje se deriva de la ficha técnica del vehículo, no del número de ejes por sí solo

| Campo | Valor |
|---|---|
| **Módulos** | M-18, M-03, M-02 |
| **Origen** | Norma [NRM-10](../normativa/NRM-10-peajes.md) — comunicado SAPP 17/09/2025, Artículo 51 Ley de Tránsito |
| **Verificación** | `[V]` que la clasificación es combinada y no solo por ejes — `[C]` el texto literal del Artículo 51 (insumo #23, requiere OCR) |
| **Tipo** | Derivación |
| **Configurable** | Sí — catálogo `categoria_peaje` como **tabla abierta**, y matriz `derivacion_categoria_peaje` con vigencia |

## Enunciado

A cada vehículo de la flota **debe** asignársele una **categoría de peaje** derivada de los atributos de su ficha técnica: **tipo de vehículo, peso bruto vehicular, número de ejes, capacidad de pasajeros y condición de articulado**.

El sistema **no debe** resolver la categoría usando el número de ejes como única llave.

La categoría asignada es un **atributo con vigencia y fundamento registrado**: quién la asignó, con qué criterio y desde cuándo rige.

El catálogo de categorías **debe** ser tabla abierta, capaz de admitir "Liviano/Turismo", "Vehículo de N Ejes" hasta 9, montacargas y categorías futuras **sin cambio de código**.

## Justificación

[NRM-10](../normativa/NRM-10-peajes.md) corrigió el supuesto de partida con evidencia: *"Un vehículo liviano tiene 2 ejes y paga L. 22. Un 'Vehículo de 2 Ejes' paga L. 90. Ambos tienen dos ejes."* `[V]`

La consecuencia está escrita en la ficha: *"cualquier modelo que use `numero_ejes` como única llave para resolver la tarifa está mal y va a cobrar cuatro veces de más a cada pickup de la flota."*

El criterio legal es el **Artículo 51 de la Ley de Tránsito**, invocado por la SAPP el 17/09/2025 al ordenar a COVI-H suspender el cobro reclasificado a Hyundai H-100, Kia K2700 y Mercedes-Benz Sprinter `[V]`. Y esos son exactamente los vehículos de una flota institucional hondureña: la zona gris es el caso normal, no la excepción.

Los atributos requeridos son **los mismos** que ya exige [RN-09](RN-09-matriz-licencia-vehiculo.md). No hay modelo nuevo que inventar.

## Condiciones de aplicación

Aplica a todo vehículo que pueda circular por un punto de peaje.

**No sustituye** la categoría con que efectivamente cobre la caseta: eso se registra como hecho y, si difiere, como discrepancia ([RN-36](RN-36-discrepancia-de-clasificacion-en-caseta.md)).

## Comportamiento esperado

1. La derivación se ejecuta al dar de alta el vehículo y **cada vez que cambia** un atributo técnico relevante, generando una nueva vigencia de categoría, no una sobrescritura.
2. El resultado muestra **qué atributos lo determinaron**. Una categoría sin explicación no se puede defender ante la SAPP ni ante un auditor.
3. Si falta un atributo necesario, el sistema **no adivina**: deja la categoría como *no resuelta* y bloquea la estimación de peajes de [RN-35](RN-35-estimacion-de-peajes-antes-de-aprobar.md) para ese vehículo, indicando el dato faltante.
4. La categoría puede **corregirse manualmente** por ACT-01 o ACT-04, exigiendo fundamento y adjunto — típicamente una resolución de la SAPP. La corrección manual queda marcada como tal y no se pierde al recalcular.
5. La progresión de tarifas por eje **no se implementa como fórmula**: [NRM-10](../normativa/NRM-10-peajes.md) advierte que *"una fórmula inferida se vuelve falsa al primer ajuste asimétrico"* `[I]`.

## Casos límite

- **Artículo 51 no transcrito.** `[C]` El PDF oficial es un escaneo sin capa de texto (insumo #23). **La matriz de derivación no se puede fijar definitivamente hasta obtenerlo.** Mientras tanto, el sistema opera con la matriz que cargue la institución, marcada como provisional, y muestra esa condición al usuario. No se inventa ningún criterio de corte.
- **Panel H-100 o K2700 y microbús Sprinter.** Son precisamente los que la SAPP tuvo que reclasificar. La matriz debe resolverlos como **liviano** conforme a la resolución de la SAPP `[V]`, y el fundamento debe quedar registrado — porque volverán a cobrarlos mal.
- **Vehículo con remolque acoplado.** El acople cambia ejes y peso: la categoría de la **configuración de la misión** puede diferir de la del vehículo en vacío. La estimación usa la configuración declarada.
- **Categoría no dependiente de ejes** — montacargas. Por eso el catálogo es tabla abierta. Un enumerado de 2 a 9 ejes deja fuera dos categorías publicadas por la SAPP `[V]`.
- **La SAPP reclasifica por resolución.** Ya ocurrió y volverá a ocurrir. La reclasificación **no reescribe el pasado**: abre una nueva vigencia. Los pasos anteriores conservan la categoría que regía entonces ([RN-40](RN-40-calculo-a-la-fecha-del-hecho.md)).
- **Vehículo cuya categoría de peaje y categoría de licencia parecen contradecirse** — liviano para peaje, pero requiere licencia C1 por peso. No es contradicción: son dos matrices distintas sobre los mismos atributos, con criterios de corte propios. El sistema no debe forzar coherencia entre ellas.

## Trazabilidad

- Norma: [NRM-10 — Peajes](../normativa/NRM-10-peajes.md); relación con [NRM-06](../normativa/NRM-06-transito-y-licencias.md)
- Decisión: [DP-001, D-02](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md)
- Reglas relacionadas: [RN-34](RN-34-tarifa-de-peaje-por-punto-categoria-vigencia.md), [RN-35](RN-35-estimacion-de-peajes-antes-de-aprobar.md), [RN-36](RN-36-discrepancia-de-clasificacion-en-caseta.md), [RN-09](RN-09-matriz-licencia-vehiculo.md)
- Actores: ACT-01, ACT-04
- Historias y casos especiales: pendientes — Bloque 2
