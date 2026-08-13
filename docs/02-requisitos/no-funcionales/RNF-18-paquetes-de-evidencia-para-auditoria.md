# RNF-18 — El expediente que pide el auditor se entrega el mismo día, completo y sin trabajo manual

| Campo | Valor |
|---|---|
| **Categoría** | Auditoría / Usabilidad |
| **Prioridad** | Alto |
| **Origen** | [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md): *"exportar paquetes de evidencia por período o por vehículo en formato entregable a auditoría"* |
| **Afecta arquitectura** | No — se apoya en [`RNF-04`](RNF-04-bitacora-append-only-con-hash-encadenado.md) y [`RNF-06`](RNF-06-reproducibilidad-historica-de-reportes.md) |

## Enunciado

Cuando el Tribunal Superior de Cuentas o Auditoría Interna piden el expediente de un vehículo, de una misión o de un período, el sistema **debe** producir un **paquete de evidencia completo** en una operación, sin que nadie tenga que armar carpetas, imprimir pantallas ni cruzar hojas de cálculo a mano.

El paquete es **autocontenido y verificable**: se abre sin software especial, declara su fecha de corte de conocimiento, e incluye el sello de la cadena de auditoría del período para que su integridad se pueda comprobar años después.

Este requisito es lo que convierte todo el trabajo de auditoría interna del sistema en algo que la institución puede efectivamente usar. Una bitácora inmutable que nadie sabe exportar no defiende a nadie.

## Métrica y umbral

| Métrica | Umbral |
|---|---|
| Ejes de exportación disponibles | Por misión, por vehículo, por motorista, por delegación, por período, por fondo de combustible, por incidente |
| Operaciones manuales para producir un paquete | 1: elegir el eje, el rango y confirmar |
| Contenido obligatorio del paquete | Documento con índice navegable, la cadena documental completa (`solicitud → autorización → orden de misión → bitácora → vale → comprobante → liquidación → afectación presupuestaria`), los adjuntos originales, la hoja de cálculo con el detalle, el extracto de asientos de auditoría del período y su **sello** |
| Paquetes sin fecha de corte de conocimiento declarada | **0** ([`RNF-06`](RNF-06-reproducibilidad-historica-de-reportes.md)) |
| Eslabones faltantes de la cadena documental que el paquete no señale | **0.** Un eslabón ausente aparece como *faltante* con su motivo, nunca se omite en silencio ([`RN-08`](../../01-negocio/reglas/RN-08-cadena-de-trazabilidad-para-cierre.md)) |
| Tiempo de generación de un paquete de 1 vehículo × 1 año a volumen `JDR-1` | ≤ 10 min, en segundo plano, con aviso al terminar |
| Tiempo de generación de un paquete de 1 misión | ≤ 60 s |
| Software especial requerido para abrir el paquete | Ninguno: lector de documentos y hoja de cálculo comunes |
| Tamaño de un paquete que no se pueda entregar por el medio habitual | Se parte automáticamente en volúmenes con índice general |
| Emisiones del mismo paquete con los mismos parámetros que difieran entre sí | **0** |
| Registro de cada emisión de paquete | Obligatorio: quién lo generó, para quién, qué alcance, qué fecha de corte, qué hash. Entregar evidencia a auditoría **es** un acto auditable |
| Datos personales incluidos en un paquete destinado a publicación o transparencia | **0** — el paquete de transparencia es un eje distinto, agregado o anonimizado ([NRM-07](../../01-negocio/normativa/NRM-07-transparencia-y-datos-personales.md)) |
| Reporte específico de fiscalización de Semana Santa | Disponible como paquete predefinido ([NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md)) |

## Cómo se verifica

1. **Simulacro de requerimiento real**: se le pide a la persona que ocupa el puesto de control interno que entregue "el expediente completo del vehículo tal del año pasado". Se cronometra desde la solicitud hasta el archivo listo. Si necesita abrir una hoja de cálculo aparte, no cumple.
2. **Prueba de completitud**: se toma una misión con incidencias reales —cambio de motorista, extensión de días, comprobante perdido, sobrante al liquidar— y se compara el paquete contra la lista de artefactos que esa misión debió producir. Toda ausencia no señalada es defecto.
3. **Prueba de verificabilidad diferida**: se genera un paquete, se guarda, se opera el sistema tres meses, y se verifica el sello del paquete contra la cadena actual. Debe seguir validando.
4. **Prueba de eslabón faltante**: se fuerza una misión sin comprobante de consumo ([`CE-25`](../casos-especiales/CE-25-comprobante-perdido-o-estacion-sin-factura.md)) y se verifica que el paquete lo declara como faltante con el acta sustitutiva, en lugar de omitirlo.
5. **Prueba de apertura limpia**: el paquete se abre en un equipo sin ninguna herramienta del proyecto instalada.
6. **Prueba de transparencia**: se genera el paquete público de flota y viajes y se revisa buscando datos personales. Resultado esperado: cero.
7. **Prueba de Semana Santa**: se genera el reporte de fiscalización con vehículos autorizados a circular y vehículos que deben estar resguardados, con su confirmación.

## Consecuencia de no cumplirlo

El requerimiento de auditoría se atiende como se atiende hoy: alguien pasa una semana armando carpetas, imprimiendo pantallas y buscando el vale que falta. Durante esa semana, esa persona no opera la flota.

Y el efecto de fondo es peor: si armar la evidencia es costoso, se arma incompleta y tarde, que es exactamente el patrón que produce el hallazgo. El sistema habría registrado todo correctamente y la institución igual perdería el descargo, por no poder presentarlo.

## Trazabilidad

- Módulos: M-14, M-13
- Reglas: [`RN-08`](../../01-negocio/reglas/RN-08-cadena-de-trazabilidad-para-cierre.md), [`RN-28`](../../01-negocio/reglas/RN-28-comprobacion-del-consumo-de-combustible.md), [`RN-30`](../../01-negocio/reglas/RN-30-conciliacion-galonaje-kilometraje.md)
- Normativa: [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md), [NRM-07](../../01-negocio/normativa/NRM-07-transparencia-y-datos-personales.md), [NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md)
- Casos especiales: [`CE-25`](../casos-especiales/CE-25-comprobante-perdido-o-estacion-sin-factura.md), [`CE-26`](../casos-especiales/CE-26-sobrante-o-faltante-al-liquidar.md), [`CE-28`](../casos-especiales/CE-28-hallazgo-posterior-sobre-mision-cerrada.md)
- Requisitos relacionados: [`RNF-04`](RNF-04-bitacora-append-only-con-hash-encadenado.md), [`RNF-06`](RNF-06-reproducibilidad-historica-de-reportes.md), [`RNF-17`](RNF-17-retencion-y-depuracion-diferenciada.md)
- Insumos: #19 (informes de auditoría previos, que definen qué pide realmente el auditor)
