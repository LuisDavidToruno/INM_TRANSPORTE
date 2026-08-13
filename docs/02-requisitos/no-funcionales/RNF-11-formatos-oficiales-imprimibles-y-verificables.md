# RNF-11 — Todo documento oficial se imprime en blanco y negro, en tamaño carta, en la impresora que la delegación ya tiene, y se puede verificar

| Campo | Valor |
|---|---|
| **Categoría** | Usabilidad / Operabilidad / Auditoría |
| **Prioridad** | Crítico |
| **Origen** | Premisa rectora 4 de [CLAUDE.md](../../../CLAUDE.md) — híbrido digital-papel por diseño; [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md); [`RN-25`](../../01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md) |
| **Afecta arquitectura** | **Sí** — exige un motor de generación de documentos con salida estable y verificación offline. No determinante de stack |

## Enunciado

El control en carretera es físico. Un agente de la DNVT en un retén no consulta un sistema: mira un papel. Por tanto todo documento oficial de SIGTI —Orden de Misión, salvoconducto, vale de combustible, acta de entrega, manifiesto, paquete de identificación del vehículo— **debe** tener una versión imprimible con **folio, QR de verificación, código alfanumérico de verificación, hash del documento electrónico, espacio de firma y sello**, legible en la impresora que la delegación ya tiene.

**El documento impreso debe funcionar sin color y sin conectividad de quien lo revisa.** Si el color transporta información, el documento fotocopiado pierde información. Si la verificación solo funciona escaneando el QR contra un servidor accesible, el documento no sirve en un retén de Olancho.

## Métrica y umbral

| Métrica | Umbral |
|---|---|
| Tamaño de papel | Carta (216 × 279 mm). **Ninguna otra medida** `[C]` insumo #70 |
| Información transportada exclusivamente por color | **0.** Todo estado y toda advertencia se expresan además con texto o símbolo |
| Impresoras soportadas | Matricial de 9 y de 24 agujas, e impresora láser monocroma común `[C]` insumo #70 — el parque real de la institución no se conoce |
| Legibilidad del texto impreso en matricial de 9 agujas con cinta en su último tercio de vida | 100 % de los campos legibles a simple vista |
| Lado mínimo del QR impreso | ≥ 25 mm, con nivel de corrección de errores alto `[I]` |
| Lecturas exitosas del QR al primer intento, impreso en matricial y escaneado con celular de gama baja | ≥ 90 % `[C]` — si no se alcanza, **el código alfanumérico es la vía primaria y el QR pasa a ser conveniencia** |
| Código alfanumérico de verificación legible y dictable por teléfono | Siempre presente junto al QR. Longitud ≤ 12 caracteres, sin caracteres ambiguos (0/O, 1/I/l) |
| Prefijo del hash del documento electrónico impreso en el pie | ≥ 12 caracteres |
| Verificación del documento sin conectividad del verificador | Posible por el código alfanumérico llamando a la delegación o a la sede |
| Paridad campo a campo entre el formulario en pantalla y el formato impreso | **100 %**: mismos campos, mismos nombres, mismo orden ([NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md)) |
| Tiempo de generación del documento imprimible | < 5 s (ver [`RNF-01`](RNF-01-rendimiento-de-consulta-y-operacion.md)) |
| Tamaño del archivo generado | < 500 KB, para que se envíe por el mensajero que la delegación ya usa |
| Emisión anticipada de documentos con folio pre-asignado, antes de salir a zona sin cobertura | Soportada ([`RNF-21`](RNF-21-integridad-de-folios-y-correlativos.md)) |
| Diferencia entre dos generaciones del mismo documento sin cambios de datos | **0** — mismo hash. Un documento cuyo hash cambia solo por reimprimirse no es verificable |
| Documento impreso que no declare si es original o reimpresión, con su marca de tiempo | **0** |

## Cómo se verifica

1. **Prueba del parque real de impresoras**: se imprime el juego completo de documentos en una matricial de 9 agujas con cinta gastada, una de 24 agujas y una láser monocroma. Se revisa campo por campo con el formato en papel de la institución al lado (insumo #2).
2. **Prueba del QR degradado**: cada documento impreso en las tres impresoras se escanea con **tres celulares de gama baja distintos**, cinco intentos cada uno, bajo luz de oficina y a pleno sol. Se mide la tasa de éxito al primer intento. **Si no llega al 90 %, se cambia el diseño, no el umbral.**
3. **Prueba de la fotocopia**: se fotocopia cada documento dos veces en cadena y se verifica que sigue siendo legible y que no se perdió ninguna información — es lo que realmente ocurre con un salvoconducto que pasa por tres manos.
4. **Prueba del retén**: se simula un control en carretera. Alguien que no conoce el sistema recibe la Orden de Misión impresa y debe poder responder: qué vehículo, qué motorista, a dónde va, hasta cuándo es válida, y cómo verificar que no es falsa. Sin ayuda y sin internet.
5. **Prueba de paridad**: se comparan lado a lado la pantalla de captura y el formato impreso. Todo campo que aparezca en uno y no en el otro, o con distinto nombre u orden, es un defecto.
6. **Prueba de estabilidad del hash**: se genera el mismo documento diez veces y se comparan los hashes.
7. **Prueba de reimpresión**: se reimprime un documento ya emitido y se verifica que aparece marcado como reimpresión, con fecha, hora y autor, y que quedó asiento de auditoría.

## Consecuencia de no cumplirlo

El motorista llega al retén con un papel que el agente no puede verificar, o que salió cortado de la impresora de la delegación. La consecuencia inmediata es un vehículo detenido en carretera con la misión en curso. La consecuencia de fondo es peor: la delegación deja de imprimir desde el sistema y vuelve al talonario preimpreso, con lo cual la numeración del sistema y la del papel divergen, y la conciliación se pierde.

El caso [`CE-17`](../casos-especiales/CE-17-vehiculo-sin-placa-metalica.md) lo hace crítico: con el desabastecimiento nacional de placas, el paquete de identificación impreso **es** lo que acredita al vehículo en carretera. Si no se imprime bien, no hay defensa.

## Trazabilidad

- Módulos: M-15, M-07, M-09, M-17
- Reglas: [`RN-25`](../../01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md), [`RN-27`](../../01-negocio/reglas/RN-27-asignacion-de-combustible-con-folio.md), [`RN-15`](../../01-negocio/reglas/RN-15-identidad-del-vehiculo-y-placa.md), [`RN-23`](../../01-negocio/reglas/RN-23-permiso-de-circulacion-en-dia-inhabil.md)
- Normativa: [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md), [NRM-06](../../01-negocio/normativa/NRM-06-transito-y-licencias.md), [NRM-08](../../01-negocio/normativa/NRM-08-firma-electronica.md), [NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md)
- Casos especiales: [`CE-17`](../casos-especiales/CE-17-vehiculo-sin-placa-metalica.md), [`CE-24`](../casos-especiales/CE-24-cobro-en-categoria-de-peaje-equivocada.md), [`CE-09`](../casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md)
- Requisitos relacionados: [`RNF-21`](RNF-21-integridad-de-folios-y-correlativos.md), [`RNF-16`](RNF-16-idioma-accesibilidad-y-mensajes.md), [`RNF-06`](RNF-06-reproducibilidad-historica-de-reportes.md)
- Insumos: #2 (formatos en papel vigentes), #46 (folio del talonario preimpreso), #70 (parque de impresoras)
