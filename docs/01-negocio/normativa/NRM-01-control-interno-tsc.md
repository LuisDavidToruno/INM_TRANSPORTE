# NRM-01 — Control interno y auditoría (TSC / MARCI / ONADICI)

| Campo | Valor |
|---|---|
| **Ámbito** | Control interno de los recursos públicos, rendición de cuentas, evidencia de auditoría |
| **Módulos afectados** | Todos, especialmente M-13 y M-14 |
| **Última verificación** | 2026-08-06 |
| **Riesgo de cambio** | Bajo — es marco estructural, no coyuntural |

Esta es la ficha **transversal** del proyecto. Las demás fichas regulan un módulo; ésta condiciona cómo se diseña el sistema entero.

## Marco normativo

| Norma | Referencia | Vigencia | Verificación |
|---|---|---|---|
| Ley Orgánica del Tribunal Superior de Cuentas | Decreto No. 10-2002-E | 05/12/2002 | `[V]` |
| Marco Rector del Control Interno Institucional (MARCI) | Acuerdo Administrativo 001-2008, compilación 2009 | Vigente | `[V]` |
| Guías para la Implementación del Control Interno Institucional (ONADICI) | 2ª ed. 2021 | Vigente | `[V]` |
| Guía General para la Implementación del MARCI (ONADICI) | 3ª ed., enero 2023 | Vigente | `[V]` |
| Circular CGR-010-2026 — conciliación de bienes ejercicio 2026 | Contaduría General de la República, 04/06/2026 | Vigente | `[V]` |

`[C]` En el sitio del TSC existe un archivo `LOTSC_2024.pdf`, lo que sugiere texto consolidado o reformado en 2024. Confirmar el alcance de esas reformas.

El TSC es el ente rector del control de los recursos públicos por mandato constitucional. El MARCI es obligatorio para todos los sujetos del Artículo 5 de la LOTSC y está construido sobre COSO y normas del IIA. `[V]`

## Normas NOGECI directamente aplicables a flota

Verificadas por citas en informes de auditoría del propio TSC. `[P]`

| Código | Título | Por qué importa aquí |
|---|---|---|
| TSC-NOGECI V-07 | Autorización y Aprobación de Transacciones y Operaciones | Toda salida de vehículo, orden de misión y anticipo debe estar autorizada por servidor competente |
| TSC-NOGECI V-10 | Registro Oportuno | La bitácora y el consumo se registran **en el momento del hecho**, no reconstruidos después |
| TSC-NOGECI V-14 | Conciliación Periódica de Registros | Bitácoras vs. vales vs. facturas del proveedor vs. SIAFI |
| TSC-NOGECI V-15 | Inventarios Periódicos / constatación física | Verificación física de la flota contra el registro de bienes |

`[C]` El MARCI contiene además normas sobre segregación de funciones, documentación de procesos y transacciones, acceso restringido a activos y registros, y controles sobre TI. Los códigos y títulos exactos deben tomarse del MARCI impreso que tenga la institución.

## Qué busca realmente el auditor

Los informes públicos del TSC revelan el patrón del hallazgo típico en flota: **incremento de consumo de combustible sin relación con el uso habitual de la flota** — por ejemplo, en meses cercanos a un proceso electoral. `[V]`

Esto es determinante para el diseño: **el auditor no busca comprobantes, busca correlación entre consumo, kilometraje y misión autorizada.** Un sistema que solo archiva facturas no responde a lo que se le va a preguntar.

## Implicaciones de requerimiento

- **El sistema debe** mantener una **pista de auditoría append-only** de toda transacción: quién, qué, cuándo, desde dónde, valor anterior y valor nuevo. Sin borrado físico: toda anulación es un asiento reverso con motivo y autor.
- **El sistema debe** vincular en cadena trazable: `solicitud → autorización → orden de misión → bitácora → vale de combustible → factura del proveedor → liquidación de viáticos → afectación presupuestaria`. Cada eslabón con su documento y su firmante.
- **El sistema debe** implementar **segregación de funciones** por rol, como bloqueo duro y no como advertencia: quien solicita ≠ quien autoriza ≠ quien despacha ≠ quien entrega combustible ≠ quien liquida.
- **El sistema debe** producir un **reporte de conciliación periódica** que cruce galones despachados por vale, galones facturados por el proveedor, kilómetros recorridos según bitácora, y rendimiento esperado por vehículo — con desviaciones marcadas por umbral configurable, en ambas direcciones.
- **El sistema debe** soportar **constatación física de inventario de flota**: acta de verificación, fecha, comisión verificadora, hallazgos, y conciliación contra el registro de bienes.
- **El sistema debe** exportar **paquetes de evidencia por período o por vehículo** en formato entregable a auditoría: PDF con índice y sello de tiempo, anexos, y hoja de cálculo.
- **El sistema debe** registrar el **momento real del hecho** distinguiéndolo del momento de captura, para satisfacer TSC-NOGECI V-10 cuando el registro se hizo en papel y se digitó después. La digitación diferida debe quedar identificada como tal, con quién digitó y el adjunto del original.
- **El sistema debe** conservar los registros por el plazo de prescripción de responsabilidad administrativa y civil. `[C]` el plazo exacto con Auditoría Interna. **Diseñar la retención como parámetro configurable, no cableada.**
- **El sistema debe** impedir que un rol operativo (motorista) edite bitácoras cerradas o modifique autorizaciones.

## Zonas grises y pendientes

- `[C]` Obtener el MARCI impreso de la institución y extraer el catálogo completo de normas NOGECI con su numeración exacta.
- `[C]` Plazo de retención documental según Auditoría Interna de la institución.
- `[C]` Si la institución tiene hallazgos abiertos del TSC relacionados con flota, combustible o viáticos. **Son requisitos disfrazados** y valen más que cualquier entrevista.

## Fuentes

- [TSC — Marco Rector del Control Interno (MARCI)](https://www.tsc.gob.hn/index.php/marco-rector-del-control-interno-marci/) — consultado 2026-08-06
- [MARCI, Acuerdo Administrativo 001-2008](https://www.tsc.gob.hn/wp-content/uploads/MARCI-2009.pdf) — consultado 2026-08-06
- [ONADICI — Guías para la Implementación del Control Interno Institucional, 2ª ed.](https://www.onadici.gob.hn/wp-content/uploads/2021/12/Guias-Para-la-Implementacion-del-Control-Interno-Institucional-ONADICI-2da-edicion.pdf) — consultado 2026-08-06
- [Ley Orgánica del TSC, Decreto 10-2002-E](https://www.oas.org/juridico/spanish/mesicic2_hnd_anexo13.pdf) — consultado 2026-08-06
- [TSC — Informe 002-2023-DFBN, verificación de vehículos del Estado](https://www.tsc.gob.hn/wp-content/uploads/002-2023-DFBN-1.pdf) — consultado 2026-08-06
