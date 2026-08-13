# RNF-06 — Un reporte regenerado con la misma fecha de corte produce exactamente el mismo resultado, siempre

| Campo | Valor |
|---|---|
| **Categoría** | Auditoría |
| **Prioridad** | Crítico |
| **Origen** | [`CE-28`](../casos-especiales/CE-28-hallazgo-posterior-sobre-mision-cerrada.md) — regla candidata *"fecha de corte de conocimiento en todo reporte"*, señalada por los analistas como una de las de mayor retorno |
| **Afecta arquitectura** | **Sí** — deriva de [`RNF-04`](RNF-04-bitacora-append-only-con-hash-encadenado.md) y [`RNF-05`](RNF-05-temporalidad-normativa.md), y condiciona cómo se construye toda consulta agregada |

## Enunciado

Todo reporte del sistema **debe** llevar una **fecha de corte de conocimiento** explícita e impresa. Regenerar el mismo reporte, con los mismos parámetros y la misma fecha de corte, **debe** producir un resultado idéntico en cualquier momento futuro, sin importar cuántas operaciones hayan ocurrido después.

La razón es directa. [`CE-28`](../casos-especiales/CE-28-hallazgo-posterior-sobre-mision-cerrada.md) establece que una misión `CERRADA` **no se reabre**, ni por auditoría, y que un hallazgo posterior se tramita como expediente aparte con asientos reversos. Esa decisión solo tiene efecto si los reportes son reproducibles: si el reporte de un período cambia porque después se registró un asiento reverso, no reabrir el expediente no sirvió de nada, porque el número que la institución presentó ya no se puede volver a obtener.

**El sistema debe poder responder dos preguntas distintas y no confundirlas:**

| Pregunta | Fecha de corte |
|---|---|
| ¿Qué presentamos al TSC en marzo? | La del reporte original |
| ¿Cuál es la situación de ese período **hoy**, con todo lo que se descubrió después? | Hoy |

Ambas son legítimas. Presentarlas como si fueran la misma es lo que produce el hallazgo.

## Métrica y umbral

| Métrica | Umbral |
|---|---|
| Reportes sin fecha de corte de conocimiento visible e impresa | **0** |
| Diferencia entre dos generaciones del mismo reporte, mismos parámetros y misma fecha de corte | **0.** Se compara por hash del contenido de datos, no de la representación |
| Reportes emitidos que quedan registrados con su hash, autor, parámetros y fecha de corte | **100 %.** Reemitir un reporte que ya se entregó a auditoría es una operación registrada |
| Reporte con fecha de corte actual sobre un período que tiene asientos posteriores | Debe **declarar en el propio documento** cuántos asientos posteriores lo afectan y desde cuándo |
| Diferencia entre el reporte con corte original y el reporte con corte actual | Debe poder obtenerse como **reporte de diferencias**, no reconstruirse a mano |
| Ordenamiento no determinista en cualquier listado o reporte | **0.** Todo orden es total y explícito, incluido el criterio de desempate |
| Uso de "fecha de hoy" implícita dentro de la lógica de un reporte | **0.** La fecha entra siempre como parámetro |

## Cómo se verifica

1. **Prueba de reproducción a ciegas**:
   - Se genera el reporte de conciliación de combustible del mes M, con corte al día 5 del mes siguiente. Se guarda su hash.
   - Se ejecutan 50 operaciones sobre el período, incluidos un asiento reverso, una corrección retroactiva de tarifa y el cierre de dos misiones pendientes.
   - Se regenera el reporte con **la misma fecha de corte**. El hash debe ser idéntico.
   - Se regenera con **corte de hoy**. El hash debe ser distinto, y el documento debe declarar cuántos asientos posteriores lo modificaron.
2. **Prueba del reporte de diferencias**: sobre el escenario anterior, se pide el reporte de diferencias entre ambos cortes. Debe listar exactamente los 50 movimientos, con su motivo y autor.
3. **Prueba de determinismo de orden**: se ejecuta el mismo listado 20 veces sobre datos con valores empatados en el criterio principal. Las 20 salidas deben ser idénticas.
4. **Prueba del hallazgo posterior**: se simula [`CE-28`](../casos-especiales/CE-28-hallazgo-posterior-sobre-mision-cerrada.md) completo — hallazgo descubierto ocho meses después sobre una misión cerrada. Se verifica que la misión no se reabre, que el expediente de hallazgo existe, y que el reporte del ejercicio original sigue devolviendo las cifras originales.
5. **Prueba del cierre fiscal**: se simula [`CE-27`](../casos-especiales/CE-27-cierre-de-ejercicio-fiscal-con-hallazgo-abierto.md) y se verifica que el reporte del ejercicio cerrado no cambia al resolverse el hallazgo el año siguiente.

## Consecuencia de no cumplirlo

La institución pierde la capacidad de sostener lo que ya declaró. El Encargado de Transporte presenta un descargo con las cifras del sistema; seis meses después el auditor pide el mismo reporte y salen otras cifras. No hay explicación técnica que ayude en esa reunión: la conclusión razonable del auditor es que los registros se alteraron.

Y hay un efecto de segundo orden que importa más: si los reportes no son reproducibles, la única forma de "congelar" un resultado es imprimirlo y guardarlo. La institución vuelve al archivo físico, y el sistema pasa a ser un generador de papeles.

## Trazabilidad

- Módulos: M-13, M-14
- Reglas: [`RN-05`](../../01-negocio/reglas/RN-05-registro-cerrado-no-se-edita.md), [`RN-42`](../../01-negocio/reglas/RN-42-correccion-retroactiva-con-asiento-de-diferencia.md), [`RN-46`](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md)
- Normativa: [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md)
- Casos especiales: [`CE-27`](../casos-especiales/CE-27-cierre-de-ejercicio-fiscal-con-hallazgo-abierto.md), [`CE-28`](../casos-especiales/CE-28-hallazgo-posterior-sobre-mision-cerrada.md)
- Requisitos relacionados: [`RNF-04`](RNF-04-bitacora-append-only-con-hash-encadenado.md), [`RNF-05`](RNF-05-temporalidad-normativa.md), [`RNF-18`](RNF-18-paquetes-de-evidencia-para-auditoria.md)
