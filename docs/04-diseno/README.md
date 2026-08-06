# 04 — Diseño

Cómo se ve y cómo se recorre el sistema.

## Estructura

| Ruta | Estado | Contenido |
|---|---|---|
| `mapa-de-navegacion.md` | Bloque 4 | Estructura de pantallas por rol y los caminos entre ellas |
| `wireframes/` | Bloque 4 | Bocetos de baja fidelidad, uno por pantalla clave |
| `formatos-impresos/` | Bloque 4 | Diseño de los documentos oficiales: orden de misión, vale, bitácora, salvoconducto, planilla de viáticos, acta de entrega |

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
