# RN-98 — La evidencia de auditoría se entrega también por vehículo y por período, no solo por misión

| Campo | Valor |
|---|---|
| **Módulos** | M-14, M-03, M-13, M-09, M-18, M-15 |
| **Origen** | Norma [NRM-01](../normativa/NRM-01-control-interno-tsc.md) — implicación de requerimiento: *«exportar paquetes de evidencia por período o por vehículo en formato entregable a auditoría»*. Hallazgo `HN1-09` de [H-B1-002](../../05-calidad/hallazgos/H-B1-002-revision-normativa-bloque-1.md) |
| **Verificación** | `[V]` que la Ley Orgánica del TSC y el MARCI están vigentes y que el TSC es el ente rector del control de los recursos públicos — [NRM-01](../normativa/NRM-01-control-interno-tsc.md). `[I]` **el formato y los dos ejes de agregación**: es implicación de requerimiento escrita por el equipo, no articulado citable. `[C]` el plazo de conservación exacto, con Auditoría Interna |
| **Tipo** | Capacidad obligatoria del sistema — no es bloqueo de transición |
| **Configurable** | No la capacidad. Sí la composición del paquete por tipo de requerimiento |

## Por qué existe esta regla — hallazgo `HN1-09`

El Bloque 1 produce la evidencia y **no la ensambla en la unidad en que se la van a pedir**.

| Lo que existe | Unidad que entrega |
|---|---|
| [`RN-08`](RN-08-cadena-de-trazabilidad-para-cierre.md) | El expediente de **una misión** |
| [`RN-30`](RN-30-conciliacion-galonaje-kilometraje.md) | El reporte de conciliación de combustible del **período** |
| [`RN-37`](RN-37-coherencia-de-la-secuencia-de-casetas.md) | El reporte de peajes por vehículo, motorista, dependencia y período |

**Un requerimiento del TSC no llega por misión.** Llega como *«entrégueme todo lo del vehículo 042 del ejercicio 2026»*. Sin esta regla la institución exporta misión por misión y arma el paquete a mano — que es el trabajo manual que el sistema existe para evitar, y el que produce omisiones cuando hay prisa.

## Enunciado

El sistema **debe** producir un **paquete de evidencia** sobre un alcance definido por **dos ejes combinables**:

- **Vehículo** — uno o varios, identificados por su correlativo institucional y no por placa ([`RN-15`](RN-15-identidad-del-vehiculo-y-placa.md): la placa cambia, y puede no existir).
- **Período** — rango de fechas, resuelto por **fecha del hecho** y nunca por fecha de captura ([`RN-46`](RN-46-fecha-del-hecho-y-fecha-de-captura.md)).

Ambos ejes se usan solos o combinados. Un tercer eje opcional acota por **dependencia, delegación o motorista**, y no sustituye a los dos anteriores.

El paquete **debe** ensamblar, para ese alcance:

| Componente | De dónde sale |
|---|---|
| Los expedientes de misión completos, cada uno con su cadena de trazabilidad | [`RN-08`](RN-08-cadena-de-trazabilidad-para-cierre.md) |
| Las conciliaciones galonaje–kilometraje del período | [`RN-30`](RN-30-conciliacion-galonaje-kilometraje.md) |
| Las conciliaciones de peaje y sus discrepancias, resueltas o abiertas | [`RN-36`](RN-36-discrepancia-de-clasificacion-en-caseta.md), [`RN-37`](RN-37-coherencia-de-la-secuencia-de-casetas.md) |
| Las misiones cerradas con hallazgo, con el criterio `H-nn` que lo produjo | [`orden-de-mision.md` §7](../../03-arquitectura/estados/orden-de-mision.md) |
| Las constataciones físicas y de rotulación del vehículo en el alcance | [`RN-99`](RN-99-constatacion-fisica-de-la-flota.md), [`RN-18`](RN-18-rotulacion-del-vehiculo-del-estado.md) |
| Los cambios de parámetro normativo **vigentes durante el alcance**, con quién los cargó y quién los aprobó | [`RN-39`](RN-39-parametros-normativos-con-vigencia.md) |
| Los intentos bloqueados por segregación y por habilitación dentro del alcance | [`RN-01`](RN-01-segregacion-de-funciones.md), [`RN-09`](RN-09-matriz-licencia-vehiculo.md) |
| El registro de consultas a datos personales, cuando el alcance los contiene | [`RN-52`](RN-52-registro-de-consultas-a-manifiestos.md) |

## Justificación

El patrón del hallazgo típico del TSC en flota, según [NRM-01](../normativa/NRM-01-control-interno-tsc.md), es el **incremento de consumo sin relación con el uso habitual de la flota**. Ese patrón **no se ve dentro de una misión**: se ve comparando un vehículo consigo mismo a lo largo de un período. Entregar la evidencia por misión obliga al auditor a hacer la agregación que el sistema pudo hacer, y deja a la institución en la peor posición posible — la de quien entrega cajas.

Un paquete armado a mano tiene además un problema de defensa: **no es reproducible**. Dos exportaciones del mismo alcance en fechas distintas deben producir el mismo contenido, y eso solo se sostiene si lo ensambla el sistema con un criterio declarado.

## Condiciones de aplicación

Aplica a todo requerimiento de auditoría interna o externa, y a la preparación voluntaria de expedientes de descargo.

**No aplica** a la consulta operativa del día a día, que se resuelve con los reportes de M-14 sin índice ni sello.

## Comportamiento esperado

1. El paquete lleva **índice**, **sello de tiempo** y el **hash del contenido**, y se entrega en formato imprimible más hoja de cálculo con los datos tabulares.
2. La portada declara el **alcance solicitado, quién lo pidió, quién lo generó y cuándo**, y **qué quedó fuera y por qué** — un expediente abierto, una misión sin liquidar, un adjunto no disponible. Un paquete que calla lo que le falta es peor que uno incompleto declarado.
3. La generación del paquete **es en sí misma un hecho registrado**: quién exportó qué alcance y cuándo. Un requerimiento de auditoría es información de control.
4. El paquete se reconstruye **a la fecha del hecho**, con el paquete normativo congelado de cada misión ([`RN-41`](RN-41-congelamiento-del-valor-al-autorizar.md)) y no con las tablas vigentes al exportar. Por eso dos exportaciones del mismo alcance coinciden.
5. Si el alcance contiene datos personales de personas trasladadas, el paquete respeta la separación de [`RN-51`](RN-51-minimizacion-de-datos-de-personas-externas.md): la versión para transparencia va sin ellos; la versión para auditoría los lleva y **queda registrada como consulta** ([`RN-52`](RN-52-registro-de-consultas-a-manifiestos.md)).

## Casos límite

- **El vehículo cambió de placa dentro del período.** El paquete lo sigue por su correlativo institucional y **declara el historial de placa** ([`RN-15`](RN-15-identidad-del-vehiculo-y-placa.md)). Un paquete indexado por placa parte en dos la historia del mismo bien.
- **El vehículo salió de la flota dentro del período** — descargo, devolución de comodato. El paquete cubre hasta la fecha de salida y lo declara. El bien deja la flota; su evidencia no.
- **Hay misiones del período todavía abiertas.** Se incluyen con su estado real y se listan aparte. No se omiten: una misión abierta dentro del alcance es un dato, y ocultarla convierte el paquete en una afirmación falsa por omisión.
- **El período abarca un cambio de tarifa o de matriz de licencias.** Cada misión conserva el paquete normativo con que se calculó, y el paquete incluye el registro del cambio con sus dos firmas ([`RN-39`](RN-39-parametros-normativos-con-vigencia.md)).
- **El requerimiento pide un plazo mayor al de conservación configurado.** `[C]` El plazo exacto sigue pendiente con Auditoría Interna — lo deja abierto [NRM-01](../normativa/NRM-01-control-interno-tsc.md). Hasta que se defina, el sistema **no depura** y el paquete declara la fecha del registro más antiguo disponible.

## Trazabilidad

- **Norma**: [NRM-01](../normativa/NRM-01-control-interno-tsc.md) — implicación de requerimiento, `[I]`
- **Hallazgo que la origina**: `HN1-09` de [H-B1-002](../../05-calidad/hallazgos/H-B1-002-revision-normativa-bloque-1.md)
- **Reglas que ensambla**: [`RN-08`](RN-08-cadena-de-trazabilidad-para-cierre.md), [`RN-18`](RN-18-rotulacion-del-vehiculo-del-estado.md), [`RN-30`](RN-30-conciliacion-galonaje-kilometraje.md), [`RN-36`](RN-36-discrepancia-de-clasificacion-en-caseta.md), [`RN-37`](RN-37-coherencia-de-la-secuencia-de-casetas.md), [`RN-39`](RN-39-parametros-normativos-con-vigencia.md), [`RN-51`](RN-51-minimizacion-de-datos-de-personas-externas.md), [`RN-52`](RN-52-registro-de-consultas-a-manifiestos.md), [`RN-99`](RN-99-constatacion-fisica-de-la-flota.md)
- **Reglas que la sostienen**: [`RN-15`](RN-15-identidad-del-vehiculo-y-placa.md), [`RN-41`](RN-41-congelamiento-del-valor-al-autorizar.md), [`RN-46`](RN-46-fecha-del-hecho-y-fecha-de-captura.md)
- **Módulo principal**: M-14 Reportes, Indicadores y Auditoría
