# 04 — Diseño

Cómo se ve y cómo se recorre el sistema.

## Estructura

| Ruta | Estado | Contenido |
|---|---|---|
| [`mapa-de-navegacion.md`](mapa-de-navegacion.md) | ✅ Bloque 4 · corregido | Estructura de pantallas por rol y los caminos entre ellas, en los dos clientes |
| [`inventario-de-pantallas.md`](inventario-de-pantallas.md) | ✅ Bloque 4 · corregido | Las **138 pantallas** con su trazabilidad, si funcionan sin red y si replican papel |
| [`brief-para-diseno.md`](brief-para-diseno.md) | ✅ Alineado | **Paquete de entrega para el diseñador.** Qué leer, en qué orden, y qué se puede empezar hoy |
| [`mockups/`](mockups/README.md) | ✅ Entregado por diseño | Tablero de mockups autocontenido, **41 identificadores `PT` tocados**: 21 completas y 20 esbozadas. Su `§5` devuelve **diez hallazgos sobre nuestra documentación** |
| `wireframes/` | Delegado a diseño | Bocetos de baja fidelidad. **99 pantallas se pueden hacer ya; 29 esperan los formatos en papel y 10 están parcialmente bloqueadas** |
| `formatos-impresos/` | Bloqueado por el insumo #2 | Diseño de los documentos oficiales: orden de misión, vale de combustible, bitácora, salvoconducto, acta de entrega-recepción, lista de abordo |

## Estado del recuento — corrección `HB34-65`

El recuento anterior (27 bloqueadas / 8 parciales / 91 libres = 126) **sumaba pero era incorrecto en las tres celdas**. Recontado columna a columna sobre las mismas 126 filas era **28 / 9 / 89**. Después de inventariar las quince historias de M-17 que no tenían pantalla (`HB34-66`) y de separar la raíz de `ACT-10` de la del motorista (`HB34-70`):

| | Bloqueadas por el insumo #2 | Parciales | Se diseñan ya | **Total** |
|---|---|---|---|---|
| Decía | 27 | 8 | 91 | 126 |
| Era | 28 | 9 | 89 | 126 |
| **Es** | **29** | **10** | **99** | **138** |

**Por cliente:** 103 solo administrativo · 9 duales `A/C` · 25 solo campo · 1 pública. **Las superficies de campo a diseñar son 34, no 25**, porque una pantalla dual hay que diseñarla en los dos clientes: no hay "vista móvil", hay dos productos.

**Lo que queda abierto** está declarado en el [§7 del inventario](inventario-de-pantallas.md): falta un `CU-19` que gobierne el ciclo de vida del parámetro normativo, el Oficial de Información Pública no es un actor catalogado, y la operación de las delegaciones pequeñas depende del insumo #26 y de [`DP-002`](../07-gestion/decisiones-de-producto/DP-002-segregacion-en-delegaciones-pequenas.md).

## Principio rector: paridad pantalla ↔ papel

El operador que lleva años llenando un formato preimpreso debe encontrar **los mismos campos, con los mismos nombres, en el mismo orden** en la pantalla. Esto reduce el costo de adopción más que cualquier funcionalidad nueva.

Consecuencia práctica: los formatos impresos no se diseñan *después* de las pantallas. Se diseñan **junto con ellas**, tomando como punto de partida el formato en papel que la institución usa hoy.

## Contexto de uso real

Estos son usuarios reales en condiciones reales, no perfiles de laboratorio:

- El **motorista** opera desde el celular, a veces con guantes, a pleno sol, sin conectividad, y a menudo con el vehículo detenido en carretera. Botones grandes, poco texto, cero dependencia de red.
- El **encargado de despacho** trabaja desde escritorio, con muchas solicitudes simultáneas y presión de tiempo. Necesita densidad de información y acciones rápidas en lote. **No entrega el fondo**: eso lo hace el encargado de combustible, presente en el mismo acto — `I-08` es bloqueo duro (`HB34-68`).
- La **jefatura que aprueba** entra pocas veces al día, quiere ver lo pendiente y decidir en dos toques. A menudo desde el celular.
- El **encargado de delegación** digita formularios que llegaron en papel. Necesita capturar rápido, adjuntar la foto del original, y que el sistema no le estorbe con validaciones que el papel no tenía. **Hoy no despacha ni entrega el fondo dentro del sistema**: escala a sede, porque `DP-002` suspendió el régimen de excepción (`HB34-69`).
- El **auditor** busca evidencia. Necesita filtrar, rastrear y exportar; no necesita crear nada.

**Vocabulario:** se dice **motorista** para quien está en el padrón, y *«quien conduce»* o *«conductor declarado»* cuando el padrón no aplica —el funcionario con vehículo asignado, o quien releva en una emergencia—. La distinción es de `RN-57` y está en el [glosario](../00-vision/glosario.md); no es intercambiable.

## Impresión: requisitos que aplican a todo formato

Todo documento oficial generado lleva folio único, código QR de verificación, espacio para firma y sello, y el hash del documento electrónico en el pie. Debe imprimirse legible en impresora matricial o láser común, en tamaño carta, y ser útil en blanco y negro.

La lista completa de pantallas que producen documento con folio está en el [§6 del inventario](inventario-de-pantallas.md). **Faltaban `PT-020` y `PT-024`** (`HB34-72`): la reemisión de un permiso no es la edición del vigente, es **un documento nuevo con folio nuevo que declara «sustituye al folio X»**, y el anterior queda anulado con su asiento.

**El salvoconducto es el caso más exigente** y tiene una tensión sin resolver: la paridad con el papel pide reproducir el formato vigente campo por campo, y la legibilidad en carretera pide los cuatro datos que el agente necesita en el tercio superior y en cuerpo grande. Si el formato actual no los pone arriba, **hay que decidir cuál gana antes de dibujarlo**. La postura propuesta —solo para este documento— es que gana la legibilidad, porque su lector no conoce el formato.

## Nota de gestión sobre `HB34-74`

La revisión de arquitectura reportó que `mockups/README.md` no existía. **Sí existe.** Lo que pasó fue que los mockups se habían commiteado en `main` y la rama de la revisión estaba dos commits atrás; ya está sincronizado. Consecuencia práctica: los diez hallazgos que el diseño devolvió en su `§5` **sí están escritos**, y el descuadre del recuento **sí fue autorreportado** por diseño en su `§5.4` antes de que la revisión lo encontrara. Los que tocan estos documentos están resueltos o declarados; los que tocan el glosario, la matriz de permisos, `DP-002` y `RN-xx` corresponden a otros artefactos.
