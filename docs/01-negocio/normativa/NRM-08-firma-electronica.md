# NRM-08 — Firma electrónica y validez documental

| Campo | Valor |
|---|---|
| **Ámbito** | Validez de aprobaciones electrónicas, documentos que siguen requiriendo firma manuscrita |
| **Módulos afectados** | M-15 y todos los flujos de aprobación |
| **Última verificación** | 2026-08-06 |
| **Riesgo de cambio** | Bajo |

## Marco normativo

| Norma | Referencia | Vigencia | Verificación |
|---|---|---|---|
| Ley sobre Firmas Electrónicas | Decreto No. 149-2013 | 30/07/2013 | `[V]` |
| Reglamento de la Ley sobre Firmas Electrónicas | `[C]` referencia exacta | Vigente | `[V]` que existe |
| FIEL — Firma Electrónica Avanzada operada por SEFIN | — | Vigente | `[V]` |

`[C]` Una fuente identifica el reglamento como "Decreto 41-2014"; probablemente sea Acuerdo Ejecutivo, no Decreto legislativo. Confirmar la referencia exacta.

## Qué habilita la ley

**Equivalencia funcional** `[V]`: los actos y contratos suscritos mediante firma electrónica son válidos de la misma manera y producen los mismos efectos que los celebrados por escrito en papel, y se consideran como escritos en los casos en que la ley exija esa forma.

**Habilitación expresa del sector público** `[V]`: los Poderes Legislativo, Ejecutivo y Judicial, el Tribunal Supremo Electoral, todas las instituciones públicas descentralizadas, entes públicos no estatales y cualquier dependencia del sector público están autorizados a usar firma electrónica en documentos electrónicos, en sus relaciones internas, entre sí y con particulares.

**SEFIN opera FIEL (Firma Electrónica Avanzada)** `[V]`. Hay infraestructura de firma avanzada ya operando en el ámbito financiero público — no hay que construirla desde cero.

## Qué seguirá requiriendo papel `[I]`

Inferencia razonada, no norma. Estos documentos deben existir impresos aunque el flujo sea digital:

- **Salvoconducto o permiso de circulación en día inhábil** que el motorista porta: el control es físico en carretera y el agente del TSC o de la DNVT espera un papel con firma y sello.
- **Actas de entrega-recepción de bienes** y **tarjeta de responsabilidad**: práctica arraigada de Bienes Nacionales.
- **Actas de descargo o baja** y actas de comisión de constatación física.
- **Constancias o declaraciones juradas** de gastos sin factura, en zonas rurales.
- **Recibos de entrega de vales o cupones de combustible** firmados por el motorista.
- **Documentos que salen a terceros** (talleres, estaciones de servicio, otras instituciones) que no tienen infraestructura de verificación de firma electrónica.

## Implicaciones de requerimiento

- **El sistema debe** soportar **tres niveles de firma**, seleccionables por tipo de documento:
  1. **Aprobación electrónica interna** con autenticación fuerte y sello de tiempo
  2. **Firma electrónica avanzada con certificado**, cuando el documento tenga efectos externos o financieros
  3. **Firma manuscrita sobre impresión**, con el documento firmado escaneado y adjuntado al expediente electrónico
- **El sistema debe** ser **híbrido por diseño, no por parche**: todo documento generado tiene versión imprimible con folio único, código QR de verificación, espacio para firma y sello, y pie con el hash del documento electrónico.
- **El sistema debe** ofrecer una **página pública de verificación por QR o folio** que confirme autenticidad y estado del documento (vigente / anulado) **sin exponer datos personales**.
- **El sistema debe** registrar, para cada aprobación electrónica: identidad del firmante, cargo, rol, método de autenticación, marca de tiempo, dirección IP o dispositivo, y hash del contenido firmado.
- **El sistema debe** permitir **delegación de firma** con vigencia acotada, dejando constancia de que se actuó por delegación y del acto que la confiere. Esto es esencial dada la rotación de personal.
- **El sistema debe** ser **agnóstico al proveedor de certificados** y permitir integrar FIEL u otra autoridad certificadora cuando la institución la adopte.

## Zonas grises y pendientes

- `[C]` Referencia exacta del reglamento de la Ley de Firmas Electrónicas.
- `[C]` ¿La institución ya tiene certificados emitidos? ¿De qué autoridad certificadora?
- `[C]` ¿Qué documentos exige firmados en papel la Auditoría Interna de la institución, independientemente de lo que permita la ley? En la práctica esto pesa más que la norma.

## Fuentes

- [Ley sobre Firmas Electrónicas, Decreto 149-2013](https://www.sefin.gob.hn/wp-content/uploads/2020/11/Ley_firmas_electronicas_2013.pdf) — consultado 2026-08-06
- [SEFIN — Firma Electrónica Avanzada (FIEL)](https://www.sefin.gob.hn/firma-electronica/) — consultado 2026-08-06
