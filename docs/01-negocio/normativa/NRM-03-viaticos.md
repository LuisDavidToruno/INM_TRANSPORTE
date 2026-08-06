# NRM-03 — Viáticos y gastos de viaje

> ## ⛔ FUERA DEL ALCANCE DE SIGTI
>
> **Los viáticos los maneja ARGOS.** Decisión del PO del 2026-08-06 — ver [DP-001, decisión D-01](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md).
>
> SIGTI **no** calcula tarifas, **no** gestiona anticipos ni liquidaciones de viático, y **no** necesita la tabla de zonas y categorías. Solo conserva la clave que permite vincular una Orden de Misión con sus viáticos en ARGOS.
>
> **Esta ficha se conserva como referencia**, no como requisito. Es útil para dos cosas: entender qué le va a pedir SIGTI a ARGOS por API, y **detectar doble cobro** — un viático de transporte cobrado cuando el traslado se hizo en vehículo oficial con combustible institucional.
>
> **No confundir con lo que sí queda en SIGTI:** la liquidación de los gastos operativos del viaje que el motorista ejecuta con fondos de la institución — **combustible y peajes**. Eso es control de flota, no viático del servidor.

| Campo | Valor |
|---|---|
| **Ámbito** | Cálculo, autorización, anticipo y liquidación de viáticos del sector público |
| **Módulos afectados** | Ninguno. M-10 fue retirado |
| **Última verificación** | 2026-08-06 |
| **Riesgo de cambio** | Alto, **pero es riesgo de ARGOS, no de SIGTI** |

## Hallazgo crítico

**Existe un Reglamento de Viáticos del Poder Ejecutivo aprobado por Acuerdo No. 401-2026, del 23 de julio de 2026**, publicado en el sitio de SEFIN alrededor del 31 de julio de 2026. `[V]`

**Tiene semanas de vigencia al momento de esta investigación.** No se pudo obtener su texto ni sus tablas.

**No uses tarifas de reglamentos anteriores ni de otras instituciones.** Obtener el Acuerdo 401-2026 es bloqueante para M-10.

## Marco normativo

| Norma | Referencia | Vigencia | Verificación |
|---|---|---|---|
| Reglamento de Viáticos y Otros Gastos de Viaje para Funcionarios y Empleados del Poder Ejecutivo | Acuerdo No. 096 | 27/10/2008 | `[V]` |
| **Reglamento de Viáticos del Poder Ejecutivo** | **Acuerdo No. 401-2026** | **23/07/2026** | `[V]` existencia — `[C]` contenido |

Las instituciones públicas deben **homologar** sus reglamentos internos al del Poder Ejecutivo. `[V]`

`[C] urgente` Confirmar si el Acuerdo 401-2026 deroga o reforma el Acuerdo 096-2008, y obtener las tablas de zonas, categorías y tarifas vigentes.

## Estructura típica `[I]`

De la revisión comparada de reglamentos homologados (Poder Judicial, TSC, ENP, municipalidades, INPREUNAH). **Es patrón observado, no norma específica** — sirve para diseñar la estructura de datos, no para fijar valores:

- Asignación diaria por **cada noche** que el servidor permanece fuera de su sede cumpliendo funciones
- Diferenciación por **zona geográfica** (territorio nacional / exterior) y por **categoría o nivel del funcionario**
- Los montos son **asignaciones máximas**: no pueden aplicarse tarifas superiores
- Existe **anticipo** y posterior **liquidación** con comprobantes
- Existe el mecanismo de **constancia o declaración jurada** cuando no es posible obtener factura en zonas sin comercio formalizado. `[C]` su admisibilidad y forma en el reglamento vigente

`[C]` con la institución: plazo exacto de liquidación, consecuencia por no liquidar (típicamente descuento por planilla y bloqueo de nuevos anticipos), quién autoriza según nivel y destino, y tratamiento de viajes de menos de 24 horas o sin pernocta.

## Implicaciones de requerimiento

- **El sistema debe** tratar **zonas, categorías y tarifas como parámetros con vigencia por rango de fechas**, nunca como constantes en código. Debe poder coexistir la tabla vigente hasta el 22/07/2026 con la vigente desde el 23/07/2026, y **calcular cada viaje con la tabla vigente a la fecha del viaje**, no a la de captura. Ver [RN-14](../reglas/) *(a escribir en el Bloque 1)*.
- **El sistema debe** calcular el viático propuesto en función de zona de destino, categoría del servidor, número de noches, y si hay hospedaje o alimentación provista — y **permitir asignar menos, nunca más** que el máximo, con validación dura.
- **El sistema debe** gestionar el ciclo completo: solicitud → autorización según nivel → **anticipo** con afectación presupuestaria → viaje → **liquidación dentro del plazo** → devolución de saldo o reintegro → cierre.
- **El sistema debe** llevar un **reloj de plazo de liquidación** con alertas escalonadas y **bloqueo automático de nuevas solicitudes** al servidor con liquidaciones vencidas. El bloqueo duro vs. advertencia debe ser parámetro institucional.
- **El sistema debe** aceptar adjuntos de comprobantes con captura por cámara del móvil, y soportar el mecanismo alternativo de **constancia o declaración jurada** cuando no exista factura — marcándolo como tal para el auditor.
- **El sistema debe** vincular cada liquidación a la **Orden de Misión** que la originó y al vehículo y bitácora si el traslado fue en flota institucional, para **detectar doble cobro**: viático de transporte cuando se usó vehículo oficial con combustible institucional.
- **El sistema debe** registrar la **cadena de autorización** con nombre, cargo, fecha y hora de cada aprobador.
- **El sistema debe** congelar el monto al autorizar, guardando el identificador de la tabla de tarifas usada, para que una consulta posterior muestre el monto histórico y no un recálculo.
- **El sistema debe** manejar la **extensión de misión**: noches adicionales valoradas con la tarifa vigente en cada una, y flujo de autorización del monto adicional.

## Zonas grises y pendientes

- `[C] BLOQUEANTE` Texto y tablas del Acuerdo 401-2026.
- `[C]` Reglamento de viáticos homologado de la institución piloto.
- `[C]` Tratamiento de viajes sin pernocta.
- `[C]` Plazo de liquidación y consecuencia por incumplimiento.
- `[C]` Niveles de autorización por destino y monto.
- `[C]` Si un reglamento posterior modifica tarifas retroactivamente, ¿la institución recalcula liquidaciones cerradas? La postura de diseño por defecto es **no recalcular automáticamente** y generar un reporte de misiones afectadas para decisión humana.

## Fuentes

- [SEFIN — sitio oficial, listado de documentos recientes (Acuerdo 401-2026)](https://www.sefin.gob.hn/) — consultado 2026-08-06
