# RNF-16 — Todo está en español del dominio, y ningún mensaje de bloqueo deja al usuario sin saber qué hacer

| Campo | Valor |
|---|---|
| **Categoría** | Usabilidad / Accesibilidad |
| **Prioridad** | Alto |
| **Origen** | Convención de idioma y vocabulario de [CLAUDE.md](../../../CLAUDE.md); paridad pantalla↔papel de [NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md) |
| **Afecta arquitectura** | No |

## Enunciado

Toda la interfaz, los mensajes, los reportes y los documentos impresos están **en español**, con el **vocabulario real del dominio hondureño**: orden de misión, vale de combustible, bitácora, dependencia, jefatura inmediata, motorista, salvoconducto, descargo, requisición, unidad ejecutora, objeto del gasto.

El personal debe reconocer en la pantalla los términos de sus formatos en papel. Un sistema que llama *"viaje"* a lo que la institución llama *"misión"*, o *"conductor"* a lo que llama *"motorista"*, obliga a traducir mentalmente en cada uso — y esa fricción se paga en adopción, no en comodidad.

Y ningún mensaje de error muestra una traza de excepción, un nombre de tabla o un código interno. **Todo bloqueo responde tres preguntas: qué pasó, por qué, y qué hacer ahora.** El sistema bloquea mucho por diseño —la segregación de funciones y la matriz licencia↔vehículo son bloqueos duros—, así que la calidad de los mensajes de bloqueo no es un adorno: es la mitad de la experiencia.

## Métrica y umbral

| Métrica | Umbral |
|---|---|
| Texto visible al usuario en idioma distinto del español | **0**, incluidos mensajes de bibliotecas y componentes de terceros |
| Términos del dominio que difieran del glosario del proyecto | **0.** El glosario es la fuente; la interfaz no acuña vocabulario |
| Mensajes que expongan traza de excepción, nombre de tabla, columna o identificador interno | **0** |
| Mensajes de bloqueo que no digan **qué hacer ahora** | **0** |
| Mensajes de bloqueo derivados de una regla de negocio que no citen su `RN-xx` | **0.** El usuario no necesita leerla, pero quien lo atienda sí |
| Mensajes de bloqueo que no indiquen **quién sí puede** ejecutar la operación cuando el motivo es de permisos o segregación | **0** |
| Longitud de un mensaje de bloqueo | ≤ 3 líneas en pantalla de campo; el detalle se despliega |
| Etiquetas de campo distintas entre pantalla y formato impreso equivalente | **0** ([`RNF-11`](RNF-11-formatos-oficiales-imprimibles-y-verificables.md)) |
| Escalado de tamaño de fuente sin pérdida de función | Hasta 200 % |
| Contraste mínimo en pantallas de oficina | ≥ 4.5:1; en pantallas de campo ≥ 7:1 ([`RNF-12`](RNF-12-uso-en-campo.md)) |
| Operaciones de oficina completables únicamente con teclado | 100 % de las de captura y aprobación |
| Información transmitida solo por color | **0** |
| Uso de abreviaturas no definidas en el glosario | **0** |
| Bloqueos que un usuario del puesto correspondiente entiende y resuelve sin ayuda externa | ≥ 80 % `[C]` — medido con usuarios reales |

## Cómo se verifica

1. **Catálogo de mensajes**: todos los mensajes del sistema viven en un catálogo revisable. Se recorre entero verificando idioma, vocabulario y las tres preguntas. Un mensaje nuevo que no pase la revisión no entra a la entrega.
2. **Prueba de bloqueos con usuarios reales** — la que decide:
   - Se provocan los 10 bloqueos más frecuentes: licencia vencida, licencia que no habilita el tipo de vehículo, motorista no disponible, vehículo no operativo, segregación de funciones, fondo agotado, espejo desactualizado, documentación vencida, odómetro inconsistente, día inhábil sin salvoconducto.
   - Un usuario del puesto correspondiente, sin ayuda, debe decir qué pasó y qué va a hacer.
   - **Cada bloqueo que el usuario no sepa resolver es un defecto del mensaje, no del usuario.**
3. **Barrido de idioma**: búsqueda automatizada de cadenas visibles en inglés y de los términos prohibidos (*driver*, *trip*, *request*, *booking*). Corre en cada entrega.
4. **Prueba de fuga técnica**: se fuerzan fallas de infraestructura —base no disponible, disco lleno, tiempo de espera agotado, esquema inesperado del webhook— y se verifica que el usuario ve un mensaje comprensible con un código correlacionable, y el detalle técnico va al registro del [`RNF-20`](RNF-20-observabilidad-y-diagnostico.md).
5. **Prueba de paridad de vocabulario**: se compara el formato en papel de la institución (insumo #2) con la pantalla equivalente, etiqueta por etiqueta.
6. **Prueba de escalado y teclado**: se recorre el flujo de solicitud→aprobación al 200 % de fuente y solo con teclado.

## Consecuencia de no cumplirlo

Un mensaje que dice *"error al procesar la solicitud"* obliga a llamar por teléfono. En una delegación sin personal técnico, esa llamada va al desarrollador o al PO, y cada bloqueo mal explicado se convierte en una interrupción. Cuando esas interrupciones se acumulan, la delegación deja de intentar y resuelve por fuera del sistema: se emite el vale a mano, se sale sin orden de misión, se anota en el cuaderno.

Con vocabulario ajeno el efecto es más lento pero igual de sólido: el personal no reconoce sus formatos, la capacitación cuesta el doble, y el sistema se percibe como algo que vino de afuera a complicar el trabajo.

## Trazabilidad

- Módulos: transversal
- Normativa: [NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md) (paridad pantalla↔papel)
- Reglas: todas las que producen bloqueo duro — [`RN-01`](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md), [`RN-09`](../../01-negocio/reglas/RN-09-matriz-licencia-vehiculo.md), [`RN-10`](../../01-negocio/reglas/RN-10-licencia-vigente-en-todo-el-rango.md), [`RN-19`](../../01-negocio/reglas/RN-19-vehiculo-no-operativo-no-se-asigna.md), [`RN-50`](../../01-negocio/reglas/RN-50-degradacion-por-sincronizacion-detenida.md)
- Requisitos relacionados: [`RNF-11`](RNF-11-formatos-oficiales-imprimibles-y-verificables.md), [`RNF-12`](RNF-12-uso-en-campo.md), [`RNF-20`](RNF-20-observabilidad-y-diagnostico.md)
- Insumos: #2 (formatos en papel vigentes)
