# 04 — Diseño

Cómo se ve y cómo se recorre el sistema.

## Estructura

| Ruta | Estado | Contenido |
|---|---|---|
| `mapa-de-navegacion.md` | ✅ Bloque 4 | Estructura de pantallas por rol y los caminos entre ellas, en los dos clientes |
| `inventario-de-pantallas.md` | ✅ Bloque 4 | Las 126 pantallas con su trazabilidad, si funcionan sin red y si replican papel |
| [`brief-para-diseno.md`](brief-para-diseno.md) | ✅ Bloque 4 | **Paquete de entrega para el diseñador.** Qué leer, en qué orden, y qué se puede empezar hoy |
| `wireframes/` | Delegado a diseño | Bocetos de baja fidelidad. 91 pantallas se pueden hacer ya; 27 esperan los formatos en papel |
| `formatos-impresos/` | Bloque 4 | Diseño de los documentos oficiales: orden de misión, vale de combustible, bitácora, salvoconducto, acta de entrega-recepción |

## Principio rector: paridad pantalla ↔ papel

El operador que lleva años llenando un formato preimpreso debe encontrar **los mismos campos, con los mismos nombres, en el mismo orden** en la pantalla. Esto reduce el costo de adopción más que cualquier funcionalidad nueva.

Consecuencia práctica: los formatos impresos no se diseñan *después* de las pantallas. Se diseñan **junto con ellas**, tomando como punto de partida el formato en papel que la institución usa hoy.

## Contexto de uso real

Estos son usuarios reales en condiciones reales, no perfiles de laboratorio:

- El **motorista** opera desde el celular, a veces con guantes, a pleno sol, sin conectividad, y a menudo con el vehículo detenido en carretera. Botones grandes, poco texto, cero dependencia de red.
- El **encargado de despacho** trabaja desde escritorio, con muchas solicitudes simultáneas y presión de tiempo. Necesita densidad de información y acciones rápidas en lote.
- La **jefatura que aprueba** entra pocas veces al día, quiere ver lo pendiente y decidir en dos clics. A menudo desde el celular.
- El **encargado de delegación** digita formularios que llegaron en papel. Necesita capturar rápido, adjuntar la foto del original, y que el sistema no le estorbe con validaciones que el papel no tenía.
- El **auditor** busca evidencia. Necesita filtrar, rastrear y exportar; no necesita crear nada.

## Impresión: requisitos que aplican a todo formato

Todo documento oficial generado lleva folio único, código QR de verificación, espacio para firma y sello, y el hash del documento electrónico en el pie. Debe imprimirse legible en impresora matricial o láser común, en tamaño carta, y ser útil en blanco y negro.
