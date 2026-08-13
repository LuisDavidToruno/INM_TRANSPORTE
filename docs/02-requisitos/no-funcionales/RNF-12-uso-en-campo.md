# RNF-12 — El cliente de campo se usa a pleno sol, con guantes, en un celular de gama baja y con la batería contada

| Campo | Valor |
|---|---|
| **Categoría** | Usabilidad / Rendimiento |
| **Prioridad** | Crítico |
| **Origen** | [NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md): brecha de equipamiento verificada `[V]` además de la brecha de red |
| **Afecta arquitectura** | **Sí — determinante para la selección de stack (Sprint 2).** El perfil del dispositivo de campo condiciona la tecnología del cliente tanto como la operación desconectada |

## Enunciado

El cliente de campo se usa **de pie, junto al vehículo, al mediodía, con las manos sucias o con guantes, en un celular que no es de gama alta y sin dónde cargarlo**. No se usa sentado en una oficina.

El sistema **debe** ser operable en esas condiciones. Un formulario que exige precisión de dedo, una pantalla que a pleno sol no se lee, o una aplicación que consume la batería antes del mediodía, no es un problema de comodidad: es la causa por la que el motorista anota en el papel y "lo mete después".

La [brecha de equipamiento del área rural está verificada](../../01-negocio/normativa/NRM-09-realidad-operativa.md) `[V]`. Diseñar para el dispositivo del equipo de desarrollo es diseñar para nadie.

## Métrica y umbral

| Métrica | Umbral |
|---|---|
| Dispositivo de referencia | Gama baja, definido explícitamente `[C]` insumo #69 — hasta conocerlo, se toma el equipo más modesto que el equipo pueda conseguir y se declara |
| Contraste de texto sobre fondo en las pantallas de campo | ≥ 7:1 `[I]` (nivel AAA). El texto crítico —odómetro, folio, placa— en tamaño grande |
| Tamaño mínimo del área tocable de un control | ≥ 12 mm de lado `[I]` — el estándar habitual de 9 mm supone dedo desnudo; con guante de trabajo no alcanza |
| Separación mínima entre controles de acción contraria (guardar / anular) | ≥ 8 mm, y **nunca adyacentes** |
| Campos numéricos que abran teclado alfanumérico | **0** |
| Toques para registrar el evento más frecuente en ruta (arribo o salida de parada) | ≤ 3 |
| Toques para registrar un consumo de combustible con foto del comprobante | ≤ 8 |
| Campos obligatorios de escritura libre en pantallas de campo | Los mínimos; toda selección posible se hace de lista precargada |
| Consumo de batería en jornada de 8 h con seguimiento activo y 20 capturas | ≤ 25 % del total `[C]`, de los cuales ≤ 10 % atribuibles al seguimiento ([`RNF-08`](RNF-08-seguimiento-en-ruta.md)) |
| Descarga inicial de la aplicación | ≤ 25 MB `[C]` — se instala una vez, muchas veces con datos móviles pagados por el propio motorista |
| Descarga de los datos precargados de una misión (catálogos, ruta, manifiesto) | ≤ 5 MB |
| Consumo de datos de una sincronización completa de misión con 20 fotos | ≤ 8 MB, con compresión ([`RNF-03`](RNF-03-operacion-sin-conectividad.md)) |
| Tamaño de una fotografía tras compresión automática | ≤ 300 KB, **conservando legibilidad del monto, la fecha y el establecimiento** en un comprobante de estación |
| Almacenamiento local ocupado con 20 misiones y 200 fotos sin sincronizar | ≤ 2 GB |
| Tiempo de arranque de la aplicación en el dispositivo de referencia, sin red | ≤ 5 s |
| Tiempo de respuesta de una pantalla de captura en el dispositivo de referencia | < 1 s |
| Pérdida de lo capturado si la aplicación se cierra o el dispositivo se apaga a media captura | **0.** Se guarda a medida que se escribe, no al pulsar guardar |
| Funciones que exijan GPS de alta precisión para poder capturar | **0.** La posición acompaña al registro; no lo condiciona ([`RNF-08`](RNF-08-seguimiento-en-ruta.md)) |

## Cómo se verifica

1. **Prueba de mediodía** — se ejecuta afuera, entre 11:00 y 13:00, con sol directo: se registra una salida completa (odómetro, foto del tablero, manifiesto) en el dispositivo de referencia. Si hay que buscar sombra para leer la pantalla, no cumple.
2. **Prueba con guantes**: la misma secuencia con guantes de trabajo puestos. Se cuentan los toques errados. Más de un error por cada 20 toques es un defecto.
3. **Prueba de batería**: dispositivo cargado al 100 % a las 7:00, jornada de 8 h con seguimiento activo y 20 capturas, sin cargador. Se lee el porcentaje restante a las 15:00.
4. **Prueba de la foto útil**: se fotografía un comprobante real de estación de servicio, se comprime, y **una persona distinta** debe leer monto, fecha y establecimiento en la imagen almacenada. Si no puede, la compresión es excesiva por más que el archivo pese poco.
5. **Prueba de muerte súbita**: se apaga el dispositivo a la fuerza a mitad de una captura de consumo. Al reabrir, debe estar todo lo escrito hasta ese punto.
6. **Prueba del motorista real**: un motorista de la institución que no participó en el diseño registra una misión completa sin instrucción previa más allá de la inducción del sistema. Se cronometra y se anotan los puntos donde se detiene a pensar.
7. **Prueba de gama baja**: toda la batería anterior se ejecuta **únicamente** en el dispositivo de referencia. Las mediciones en equipos del equipo de desarrollo no cuentan como evidencia.

## Consecuencia de no cumplirlo

El motorista usa el papel. No por resistencia al cambio, sino porque el papel se lee al sol, no se queda sin batería y no exige precisión de dedo. Y desde el momento en que el registro nace en papel, el sistema pasa a ser un digitador tardío: la fecha del hecho y la de captura se separan, se disparan los plazos de [`CE-09`](../casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md), y la trazabilidad que el TSC exige queda sostenida por la memoria de una persona.

El índice de casos especiales lo dice sin rodeos: [`CE-09`](../casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md) decide la adopción del sistema. Este `RNF` es la mitad de ese caso — la otra mitad es el [`RNF-03`](RNF-03-operacion-sin-conectividad.md).

## Trazabilidad

- Módulos: M-08, M-09, M-16, M-19
- Reglas: [`RN-43`](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md), [`RN-28`](../../01-negocio/reglas/RN-28-comprobacion-del-consumo-de-combustible.md), [`RN-31`](../../01-negocio/reglas/RN-31-odometro-de-retorno.md)
- Normativa: [NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md)
- Casos especiales: [`CE-09`](../casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md), [`CE-25`](../casos-especiales/CE-25-comprobante-perdido-o-estacion-sin-factura.md), [`CE-22`](../casos-especiales/CE-22-odometro-inconsistente.md)
- Requisitos relacionados: [`RNF-03`](RNF-03-operacion-sin-conectividad.md), [`RNF-08`](RNF-08-seguimiento-en-ruta.md), [`RNF-13`](RNF-13-cifrado-en-transito-y-en-reposo.md), [`RNF-16`](RNF-16-idioma-accesibilidad-y-mensajes.md)
- Insumos: #69 (inventario de dispositivos de campo y quién los provee)
