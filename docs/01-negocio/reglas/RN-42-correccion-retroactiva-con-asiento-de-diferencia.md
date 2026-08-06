# RN-42 — La corrección retroactiva de un parámetro genera asiento de diferencia; nunca sobrescribe el valor histórico

| Campo | Valor |
|---|---|
| **Módulos** | M-02, M-18, M-13, M-14 |
| **Origen** | Norma [NRM-10](../normativa/NRM-10-peajes.md); [NRM-01](../normativa/NRM-01-control-interno-tsc.md) |
| **Verificación** | `[V]` que las tarifas de peaje pueden aplicarse retroactivamente |
| **Tipo** | Bloqueo duro + cálculo |
| **Configurable** | No |

## Enunciado

Cuando un parámetro se corrige o se carga con vigencia **anterior a la fecha de carga**, y esa vigencia alcanza valores ya calculados o congelados, el sistema **debe**:

1. **Conservar intacto** el valor histórico y su procedencia
2. Producir un **reporte de impacto**: qué expedientes, misiones y montos quedan afectados
3. Generar, para cada uno, un **asiento de diferencia** con el valor anterior, el valor nuevo, la diferencia, el parámetro que la origina y quién autorizó la corrección
4. **No aplicar** el recálculo de forma automática y silenciosa sobre expedientes cerrados

## Justificación

[NRM-10](../normativa/NRM-10-peajes.md) lo exige textualmente: *"El sistema debe soportar corrección retroactiva de una tarifa ya aplicada, recalculando las misiones afectadas y dejando asiento de la diferencia — nunca sobrescribiendo el valor histórico."*

No es hipotético. En enero de 2026 COVI anunció un aumento **retroactivo** que incluía subsidios pendientes de 2024 y 2025 `[V]`. Estuvo a punto de ocurrir, y volverá a intentarse cuando se corte el subsidio del Estado — deuda verificada de más de L 364 millones.

[NRM-01](../normativa/NRM-01-control-interno-tsc.md) prohíbe el borrado y exige asiento reverso con motivo y autor. Un recálculo silencioso sobre registros contables cerrados es indistinguible de una alteración.

## Condiciones de aplicación

Aplica a toda corrección de parámetro con efecto sobre el pasado: tarifas de peaje, rendimiento esperado, umbrales usados en hallazgos ya emitidos, calendario de feriados corregido, matriz de licencias reformada.

**No aplica** a la carga normal de una nueva vigencia hacia el futuro, que no afecta nada calculado.

## Comportamiento esperado

1. Al guardar un parámetro con vigencia retroactiva, el sistema **calcula y muestra el impacto antes de confirmar**: cuántos expedientes, de qué período, por qué monto total. Confirmar es un acto autorizado y registrado.
2. Los expedientes **abiertos** se recalculan y el cambio se refleja con asiento; los **cerrados** no se tocan: se listan y la Gerencia Administrativa decide expediente por expediente.
3. Cada asiento de diferencia es consultable desde el expediente afectado y desde el parámetro que lo originó, en ambas direcciones.
4. El sistema produce el **reporte de correcciones retroactivas** por período: qué se corrigió, por qué, quién lo autorizó y cuál fue el efecto económico agregado.
5. Un asiento de diferencia **nunca** se aplica sobre otro asiento de diferencia sin dejar la cadena visible.

## Casos límite

- **Corrección que abarca cientos de misiones.** Es el escenario probable con un aumento retroactivo de peaje. El proceso debe ser por lotes, con avance visible y **reversible como conjunto**: si se detecta un error a mitad, debe poder revertirse con un asiento por cada uno, no dejando la mitad corregida.
- **Corrección que afecta un ejercicio presupuestario cerrado.** SIGTI genera el asiento y el reporte; la afectación contable pertenece a ARGOS ([DP-001 D-09](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md)). SIGTI **no decide** si el ejercicio se reabre: informa.
- **Parámetro cargado con vigencia retroactiva por error de digitación** — alguien escribió 2025 en vez de 2026. El reporte de impacto previo es lo que lo detiene antes de causar daño. Por eso mostrar el impacto **antes** de confirmar no es una comodidad, es el control.
- **Corrección de un umbral que ya produjo hallazgos.** Los hallazgos emitidos **no desaparecen**: se marcan como *emitidos bajo umbral anterior*, con el nuevo resultado anotado. Borrar un hallazgo por cambio de umbral sería exactamente lo que un auditor busca detectar.
- **Diferencia a favor de la institución** — la tarifa bajó retroactivamente y se pagó de más. Genera derecho de reclamo, no un simple ajuste contable. Se vincula al expediente de reclamo de [RN-36](RN-36-discrepancia-de-clasificacion-en-caseta.md).
- **Feriado corregido retroactivamente.** Si un día que se trató como hábil resulta feriado, misiones pasadas circularon sin permiso de la máxima autoridad. El asiento de diferencia aquí no es económico sino de **cumplimiento**: genera hallazgo retroactivo, y esa consecuencia debe estar prevista antes de que alguien corrija el calendario a la ligera.

## Trazabilidad

- Normas: [NRM-10](../normativa/NRM-10-peajes.md), [NRM-01](../normativa/NRM-01-control-interno-tsc.md)
- Reglas relacionadas: [RN-39](RN-39-parametros-normativos-con-vigencia.md), [RN-40](RN-40-calculo-a-la-fecha-del-hecho.md), [RN-41](RN-41-congelamiento-del-valor-al-autorizar.md), [RN-04](RN-04-anulacion-como-asiento-reverso.md)
- Actores: ACT-01, ACT-08, ACT-12
- Historias y casos especiales: pendientes — Bloque 2
