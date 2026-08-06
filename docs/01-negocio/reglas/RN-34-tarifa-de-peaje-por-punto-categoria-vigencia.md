# RN-34 — La tarifa de peaje se resuelve por punto × categoría × vigencia, a la fecha del hecho

| Campo | Valor |
|---|---|
| **Módulos** | M-18, M-02, M-13 |
| **Origen** | Norma [NRM-10](../normativa/NRM-10-peajes.md); premisa rectora 6 de `CLAUDE.md` |
| **Verificación** | `[V]` la matriz de categorías publicada por la SAPP — `[C]` **la tarifa efectivamente vigente hoy** (insumo #21) |
| **Tipo** | Cálculo |
| **Configurable** | Sí — tabla `tarifa_peaje (punto, categoría, vigencia_desde, vigencia_hasta, monto, fuente, fecha_verificacion)` |

## Enunciado

El monto de un peaje **debe** resolverse con la terna **(punto de peaje × categoría del vehículo × fecha del hecho)** contra la tabla de tarifas vigente, **nunca** con una constante en código ni con una fórmula derivada del número de ejes.

Si no existe tarifa vigente para esa combinación en esa fecha, el sistema **no debe** calcular un valor por defecto: **bloquea la estimación** e indica qué falta.

Cada tarifa cargada **debe** registrar su **fuente y fecha de verificación**, y el sistema **debe** alertar cuando una tarifa lleve más de 12 meses sin revisar.

## Justificación

[NRM-10](../normativa/NRM-10-peajes.md) documenta que la tarifa cambia **al menos una vez al año, en enero, con alta probabilidad de aplicación retroactiva o de reversión a mitad de proceso**: en 2026 hubo anuncio el 08/01, suspensión hacia el 15/01, prórroga al 15/02, nuevo anuncio el 27/02 y confirmación de la SIT el 28/02 de que **no habría incremento**.

Además: *"La tarifa que ve el usuario es política, no contractual"* `[V]`. El Estado debe más de L 364 millones a COVI por el subsidio de 2024 y 2025 para mantenerla congelada; cuando el subsidio se corte, la tarifa salta de golpe.

La instrucción de la ficha es explícita: **no cargar ninguna tarifa hasta confirmarla con COVI-H o la SAPP** — hay contradicción abierta entre el comunicado de la SIT y un agregador comercial.

## Condiciones de aplicación

Aplica a la estimación previa ([RN-35](RN-35-estimacion-de-peajes-antes-de-aprobar.md)), al registro del paso durante la ejecución y a la conciliación en la liquidación.

El **punto de peaje** también tiene estado operativo con vigencia — activo, suspendido, cerrado. [NRM-10](../normativa/NRM-10-peajes.md): *"Sin el estado con vigencia no se puede recalcular un viaje pasado por una caseta que ya no existe."*

## Comportamiento esperado

1. La resolución devuelve monto, identificador de la fila de tarifa usada y su vigencia. Ese identificador se congela en el expediente ([RN-41](RN-41-congelamiento-del-valor-al-autorizar.md)).
2. Ausencia de tarifa vigente produce mensaje accionable: *"No hay tarifa vigente para el punto <Zambrano>, categoría <Liviano/Turismo>, a la fecha <fecha>. Solicite a la Gerencia Administrativa que registre la tabla vigente."*
3. La carga de una tarifa exige **fuente** (SAPP, COVI-H, contrato, comunicado de la SIT) y **fecha de verificación**. Una tarifa sin fuente no se guarda.
4. La corrección retroactiva de una tarifa ya aplicada sigue [RN-42](RN-42-correccion-retroactiva-con-asiento-de-diferencia.md): genera asiento de diferencia y **nunca** sobrescribe el valor histórico.
5. Los puntos de peaje son catálogo ampliable en producción, **sin cambio de código** — [NRM-10](../normativa/NRM-10-peajes.md) advierte que hay proyectos en cartera.

## Casos límite

- **Tarifa vigente hoy no confirmada.** `[C]` Es el pendiente #1 de [NRM-10](../normativa/NRM-10-peajes.md) e insumo #21. **El sistema arranca sin tarifas cargadas**, bloqueando la estimación con mensaje claro, antes que arrancar con un número inventado que se convertiría en verdad institucional en una semana.
- **Aumento anunciado, aplicado y luego revertido** — exactamente lo ocurrido en 2026. La tabla debe admitir vigencias cortas y **cierre anticipado** de una vigencia ya abierta, con los pasos ya valorados corregidos por asiento de diferencia.
- **Aumento retroactivo** — COVI anunció uno *"incluyendo subsidios pendientes de 2024 y 2025"* `[V]`. La tabla admite vigencia con fecha de inicio anterior a la fecha de carga; los pasos afectados se recalculan y se registra la diferencia, sin tocar el valor original.
- **Punto de peaje que deja de cobrar** — Canal Seco `[V]`, Corredor Turístico en terminación anticipada `[V]`. El estado del punto es el que gobierna: un paso por un punto sin cobro vigente estima cero, con el fundamento visible.
- **Paso registrado en un punto que no está en el catálogo.** No se descarta: se registra como **punto no catalogado** con ubicación y monto pagado, marcado para depuración del catálogo. `[C]` [NRM-10](../normativa/NRM-10-peajes.md) menciona casetas antiguas en San Pedro Sula sin verificar si operan.
- **Cambio de tarifa entre la aprobación y la ejecución.** El estimado congelado y el pagado difieren legítimamente. La conciliación de [RN-37](RN-37-coherencia-de-la-secuencia-de-casetas.md) tipifica esa causa y no la trata como hallazgo.
- **Tentación de calcular la tarifa por eje.** La progresión de 2 a 9 ejes es casi lineal (~L 45 por eje) `[I]`, lo que hará que alguien proponga una fórmula. [NRM-10](../normativa/NRM-10-peajes.md) lo prohíbe expresamente: es una tabla publicada.

## Trazabilidad

- Norma: [NRM-10 — Peajes](../normativa/NRM-10-peajes.md)
- Decisión: [DP-001, D-02](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md)
- Reglas relacionadas: [RN-33](RN-33-categoria-de-peaje-derivada-de-ficha-tecnica.md), [RN-35](RN-35-estimacion-de-peajes-antes-de-aprobar.md), [RN-39](RN-39-parametros-normativos-con-vigencia.md), [RN-40](RN-40-calculo-a-la-fecha-del-hecho.md), [RN-42](RN-42-correccion-retroactiva-con-asiento-de-diferencia.md)
- Actores: ACT-01, ACT-04, ACT-08
- Historias y casos especiales: pendientes — Bloque 2
