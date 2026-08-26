# ADR-004 — Fotografías y adjuntos fuera de la base de datos

| Campo | Valor |
|---|---|
| **Estado** | Aceptada |
| **Fecha** | 2026-08-26 |
| **Decide** | Product Owner, sobre la [designación de LOKI del 2026-08-26](../../07-gestion/designaciones/2026-08-26-stack-y-arranque.md) |
| **Sprint** | 0 |

## Contexto

SIGTI produce muchas más fotografías de las que parece. `RN-47` obliga a fotografiar el original digitado; el kilometraje de salida y retorno se respalda con foto del odómetro; los incidentes y siniestros se documentan con imágenes; la identificación del vehículo del Estado —franjas, leyenda, siglas— es campo verificable **con fecha y foto**, porque es hallazgo frecuente de auditoría. `RNF-03` fija el piso en **≥ 200 fotografías por dispositivo** acumuladas sin sincronizar.

La aritmética que decide es simple:

| Acervo | Volumen anual estimado |
|---|---|
| Datos relacionales | ≈ 8 GB |
| Adjuntos | ≈ 30 GB |

Los adjuntos son **casi cuatro veces** el resto del sistema. Dónde vivan no es un detalle de implementación: decide el tamaño del respaldo, el tiempo de restauración y si `RNF-09` se cumple o no.

## Requisitos que la condicionan

- [`RNF-02`](../../02-requisitos/no-funcionales/RNF-02-volumen-y-crecimiento-del-acervo.md) — acervo que nunca se borra físicamente
- [`RNF-03`](../../02-requisitos/no-funcionales/RNF-03-operacion-sin-conectividad.md) — ≥ 200 fotografías por dispositivo sin sincronizar
- [`RNF-09`](../../02-requisitos/no-funcionales/RNF-09-instalacion-respaldo-y-restauracion.md) — **restauración probada por personal no especialista, ≤ 2 h**
- [`RNF-17`](../../02-requisitos/no-funcionales/RNF-17-retencion-y-depuracion-diferenciada.md) — datos personales que sobrevivan la depuración **en adjuntos**: 0
- [`RNF-18`](../../02-requisitos/no-funcionales/RNF-18-paquetes-de-evidencia-para-auditoria.md) — paquetes de evidencia para auditoría

## Decisión

**Los adjuntos viven en el sistema de archivos. La base guarda la ruta, el hash, el tipo, el tamaño y la clasificación de contenido.**

- **Sistema de archivos plano**, organizado por institución y fecha. **Nada de `FILESTREAM` ni `FileTable`**: agregan complejidad operativa que `RNF-09` no admite — atan el respaldo del archivo al del motor y convierten la restauración en un procedimiento que solo un DBA entiende.
- **El hash de cada archivo se guarda en la base.** Es lo que permite detectar que un adjunto fue sustituido o se corrompió, y es lo que sostiene los paquetes de evidencia de `RNF-18`.
- **El procedimiento de respaldo y restauración es de dos piezas: base + almacén de archivos, consistentes entre sí.** Se escribe así desde el principio, no se adapta después.
- **La depuración de datos personales alcanza a los adjuntos.** Por la corrección del hallazgo `HB34-53`, `adjunto` lleva `clasificacion_de_contenido` y, cuando corresponde, referencia a su `segmento_dato_personal`. Al depurar, el archivo se sustituye por una **constancia de depuración** que conserva la huella del original, su tipo, su tamaño y el evento que lo alcanzó: **el adjunto no desaparece del expediente, deja de ser legible.**

### Por qué no es diferible

Sacar los blobs de la base después no es una migración de datos: es una migración **más** la reescritura del plan de respaldo, **más** el cambio del procedimiento de restauración que el personal de la delegación ya habría aprendido. Y `RNF-09` dice que en la delegación no hay equipo de TI: cambiarles el procedimiento una vez que lo memorizaron es caro de una forma que no aparece en ninguna estimación.

## Alternativas consideradas

| Alternativa | A favor | En contra | Por qué se descartó |
|---|---|---|---|
| **Blobs en la base (`varbinary(max)`)** | Un solo respaldo; consistencia transaccional entre el registro y su foto; nada que sincronizar entre dos almacenes | Cuadruplica el tamaño del respaldo; saca la restauración de las 2 h de `RNF-09`; sin compresión de datos en 2014 Standard el costo en disco es íntegro | El incumplimiento de `RNF-09` es directo y no tiene compensación |
| **`FILESTREAM` / `FileTable`** | Archivos en disco con consistencia transaccional del motor | Complejidad operativa alta: configuración de instancia, respaldo acoplado, restauración que exige entender el mecanismo | `RNF-09` es filtro de elegibilidad. Un procedimiento que necesita un DBA no es elegible |
| **Almacenamiento de objetos (S3, MinIO)** | Escala sin esfuerzo, replicación incluida | Un servicio más que instalar, operar, respaldar y actualizar en cada institución, on-premise | Cada pieza móvil es una pieza que alguien tiene que entender a las 11 de la noche siguiendo un documento |

## Consecuencias

**Positivas**

- El respaldo diario del motor queda en el orden de los 8 GB anuales, no de los 38
- La restauración de la base es rápida y comprensible; el almacén de archivos se respalda con su propia cadencia
- Los adjuntos se pueden mover a almacenamiento más barato o de solo lectura sin tocar el esquema
- La depuración de un adjunto es borrar un archivo y escribir su constancia — no un `UPDATE` sobre una tabla de 30 GB

**Negativas**

- **Se pierde la consistencia transaccional entre el registro y su archivo.** Un fallo entre el `COMMIT` y la escritura del archivo deja una fila que apunta a nada. Hay que escribir la reconciliación: un verificador que recorra rutas y hashes y reporte huérfanos en las dos direcciones
- **Son dos cosas que respaldar, y una restauración parcial es peor que ninguna.** Restaurar la base a una fecha y el almacén a otra produce un expediente que se ve completo y no lo está
- El procedimiento de `RNF-09` se vuelve de dos pasos, y hay que probarlo entero — no basta con probar el de la base

**Deuda aceptada**

- **La consistencia entre los dos almacenes es responsabilidad del código, no del motor.** Mientras el verificador de reconciliación no exista y no corra periódicamente, un huérfano puede vivir indefinidamente sin que nadie lo note
- El almacén de archivos no tiene control de acceso por fila. La protección es de sistema de archivos y de aplicación; **un archivo con dato personal está protegido por quien tiene acceso al volumen**, y eso vuelve a `RNF-13` y a BitLocker

## Revisión

- **La medición real de volumen de adjuntos se desvía mucho** de los ≈30 GB/año estimados, en cualquier dirección
- **El verificador de reconciliación reporta huérfanos con frecuencia** — señal de que la escritura en dos almacenes necesita un mecanismo más fuerte que el reintento
- **La institución adquiere una versión del motor con compresión y `FILESTREAM` maduro** y el argumento operativo cambia
