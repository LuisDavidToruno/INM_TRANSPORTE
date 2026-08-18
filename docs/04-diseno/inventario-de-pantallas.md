# Inventario de pantallas

| Campo | Valor |
|---|---|
| **Ámbito** | Toda pantalla del sistema, con su cliente, sus roles, su trazabilidad y las dos condiciones que deciden si se puede diseñar hoy |
| **Para quién está escrito** | El diseñador externo que va a producir los mockups. Esta tabla es su plan de trabajo |
| **Artefacto hermano** | [`mapa-de-navegacion.md`](mapa-de-navegacion.md) — cómo se recorren y por qué |
| **Total** | **126 pantallas** · 102 administrativo · 23 campo · 1 pública |
| **Última actualización** | 2026-08-18 |

---

## 1. Cómo se lee esta tabla

**`PT-xxx` es un identificador nuevo, estable y no reciclable**, igual que el resto de los IDs del proyecto. Si una pantalla se descarta, su ID queda obsoleto y no se reutiliza.

| Columna | Qué significa |
|---|---|
| **Cli** | `A` cliente administrativo · `C` cliente de campo · `P` superficie pública sin sesión. Ver [mapa §0.2](mapa-de-navegacion.md) — **son dos productos distintos, no uno responsive** |
| **Rol** | Actor `ACT-xx` principal. Otros roles pueden consultarla dentro de su alcance de datos |
| **CU** | Caso de uso que la recorre |
| **HU** | Historias que la implementan. Una pantalla puede implementarse en varias entregas |
| **Sin red** | `Sí` funciona con el dispositivo totalmente desconectado · `No` exige conexión · `Deg.` funciona degradada, declarando qué no puede verificar |
| **Papel** | **La columna que divide el trabajo en dos.** `Sí` = replica un formato preimpreso, **bloqueada por el insumo #2** · `Parc.` = una sección replica papel y el resto no · `No` = sin equivalente en papel, **se diseña ya** |

### La regla que gobierna la columna «Papel»

> El operador que lleva años llenando un formato preimpreso debe encontrar **los mismos campos, con los mismos nombres, en el mismo orden** en la pantalla.

Las pantallas marcadas `Sí` **no se diseñan libremente**. Se toma el formato que la institución usa hoy y se reproduce. Si alguien propone "mejorar" el orden de los campos, **la respuesta por defecto es no**, y quien lo proponga debe justificar por qué el costo de reaprendizaje vale la pena.

Las marcadas `Parc.` sí tienen trabajo disponible hoy: la estructura, los controles nuevos que el papel no tenía y los mensajes de bloqueo. Lo que queda en espera es el bloque de campos que reproduce el formato.

---

## 2. Cliente administrativo

### 2.1 Transversales

| ID | Pantalla | Cli | Rol | CU | HU | Sin red | Papel |
|---|---|---|---|---|---|---|---|
| PT-001 | Ingreso y selección de puesto vigente | A | todos | — | — | No | No |
| PT-002 | Inicio del puesto: pendientes, alertas y accesos | A | todos | — | — | No | No |
| PT-003 | Bandeja de tareas escaladas por segregación de funciones | A | `ACT-03` `ACT-04` `ACT-08` | CU-02, CU-06 | HU-010 | No | No |
| PT-004 | Patrón de pantalla de bloqueo duro: qué se impidió, por qué, cómo salir | A/C | todos | CU-02, CU-04, CU-06, CU-15 | HU-010, HU-025, HU-039, HU-077, HU-078, HU-091, HU-108 | Sí | No |
| PT-005 | Buscador de expedientes con alcance de datos aplicado | A | todos | — | — | No | No |

### 2.2 M-06 Solicitud de transporte

| ID | Pantalla | Cli | Rol | CU | HU | Sin red | Papel |
|---|---|---|---|---|---|---|---|
| PT-006 | Mis solicitudes | A | `ACT-02` | CU-01 | HU-001 | No | No |
| PT-007 | **Requisición de vehículo** (solicitud de transporte) | A | `ACT-02` | CU-01 | HU-001, HU-003 | No | **Sí** |
| PT-008 | Objeto del traslado: personas, carga o mixto | A | `ACT-02` | CU-01 | HU-001, HU-002 | No | Parc. |
| PT-009 | Estimado de peajes desglosado por punto | A | `ACT-02` `ACT-03` | CU-01 | HU-005 | No | No |
| PT-010 | Señalamiento de tramos inhábiles, sin bloquear | A | `ACT-02` | CU-01, CU-03 | HU-006 | No | No |
| PT-011 | Envío a autorización con número de expediente y congelamiento | A | `ACT-02` | CU-01 | HU-004 | No | No |
| PT-012 | Registro de salida de emergencia para convalidación posterior | A | `ACT-02` `ACT-10` | CU-01 | HU-008 | No | Parc. |

### 2.3 M-06 Autorización

| ID | Pantalla | Cli | Rol | CU | HU | Sin red | Papel |
|---|---|---|---|---|---|---|---|
| PT-013 | **Bandeja de autorización** — [difícil §7.2](mapa-de-navegacion.md) | A | `ACT-03` | CU-02 | HU-009, HU-012 | No | No |
| PT-014 | Expediente en decisión, en una sola pantalla | A | `ACT-03` | CU-02 | HU-009, HU-011 | No | No |
| PT-015 | Autorizar con constancia inmutable | A | `ACT-03` | CU-02 | HU-011, HU-015 | No | No |
| PT-016 | Rechazar con motivo y solicitud vinculada | A | `ACT-03` | CU-02 | HU-014 | No | No |
| PT-017 | Devolver para corrección con versionado | A | `ACT-03` | CU-02 | HU-013 | No | No |
| PT-018 | Escalamiento de autorización por nivel o umbral | A | `ACT-03` `ACT-08` | CU-02 | HU-012 | No | No |
| PT-019 | Autorización por delegación de firma vigente | A | `ACT-03` | CU-02 | HU-015 | No | No |

### 2.4 M-04 / M-15 Permiso de circulación en día u hora inhábil

| ID | Pantalla | Cli | Rol | CU | HU | Sin red | Papel |
|---|---|---|---|---|---|---|---|
| PT-020 | Trámite del permiso de circulación en día u hora inhábil | A | `ACT-04` `ACT-10` | CU-03 | HU-016 | No | **Sí** |
| PT-021 | Firma del permiso por la máxima autoridad (dos toques, celular) | A | `ACT-09` | CU-03 | HU-016 | No | No |
| PT-022 | Firma en lote de feriado largo con reporte previo | A | `ACT-09` | CU-03 | HU-020 | No | No |
| PT-023 | Emisión e impresión del **salvoconducto** | A | `ACT-04` `ACT-10` | CU-03, CU-05 | HU-017 | No | **Sí** |
| PT-024 | Reemisión del permiso por cambio de elementos amparados | A | `ACT-04` | CU-03, CU-07 | HU-018 | No | **Sí** |

### 2.5 M-07 Programación y asignación

| ID | Pantalla | Cli | Rol | CU | HU | Sin red | Papel |
|---|---|---|---|---|---|---|---|
| PT-025 | Cola de programación con caducidad de la aprobación | A | `ACT-04` `ACT-10` | CU-04 | HU-021 | No | No |
| PT-026 | Asignación de vehículo: compatibilidad, documentación y estado | A | `ACT-04` | CU-04 | HU-022, HU-023, HU-024 | No | No |
| PT-027 | Declaración de conductores: titular y relevos | A | `ACT-04` | CU-04 | HU-025, HU-026 | No | No |
| PT-028 | **Rechazo por licencia no habilitante** — [difícil §7.5](mapa-de-navegacion.md) | A | `ACT-04` `ACT-10` | CU-04, CU-18 | HU-025, HU-108 | No | No |
| PT-029 | Reserva exclusiva y conflicto con su titular | A | `ACT-04` | CU-04 | HU-027 | No | No |
| PT-030 | Consolidación de solicitudes compatibles | A | `ACT-04` | CU-04 | HU-030 | No | No |
| PT-031 | Constancia probatoria de las verificaciones practicadas | A | `ACT-04` `ACT-12` | CU-04 | HU-028 | No | No |
| PT-032 | Sustitución de vehículo o motorista en `PROGRAMADA` | A | `ACT-04` | CU-07 | HU-043 | No | No |
| PT-033 | Sustitución con la misión ya `DESPACHADA` | A | `ACT-04` | CU-07 | HU-044 | No | No |

### 2.6 M-15 Emisión de documentos

| ID | Pantalla | Cli | Rol | CU | HU | Sin red | Papel |
|---|---|---|---|---|---|---|---|
| PT-034 | Vista previa con folio reservado, marcada «no válida» | A | `ACT-04` | CU-05 | HU-029 | No | **Sí** |
| PT-035 | Emisión del juego documental: **orden de misión**, peajes, advertencias, bitácora | A | `ACT-04` `ACT-10` | CU-05 | HU-031, HU-032, HU-033, HU-034, HU-081 | No | **Sí** |
| PT-036 | Reimpresión con el mismo folio y marca de reimpresión | A | `ACT-04` | CU-05 | HU-036 | No | **Sí** |
| PT-037 | Emisión anticipada para delegación sin cobertura | A/C | `ACT-10` | CU-05 | HU-037 | Sí | **Sí** |

### 2.7 M-07 / M-08 Despacho y retorno

| ID | Pantalla | Cli | Rol | CU | HU | Sin red | Papel |
|---|---|---|---|---|---|---|---|
| PT-038 | Tablero de despacho del día: salidas y retornos previstos | A | `ACT-05` | CU-06, CU-10 | HU-038 | Deg. | No |
| PT-039 | Acto de despacho: revalidación, kilometraje de salida e inspección | A | `ACT-05` | CU-06 | HU-038, HU-039 | Deg. | **Sí** |
| PT-040 | **Acta de entrega y traslado de custodia** | A/C | `ACT-05` `ACT-13` | CU-06 | HU-040 | Sí | **Sí** |
| PT-041 | Entrega del fondo contra firma, dentro del despacho | A/C | `ACT-07` `ACT-05` | CU-06, CU-13 | HU-041, HU-079 | Sí | **Sí** |
| PT-042 | Registro del retorno y cierre de la bitácora | A/C | `ACT-05` | CU-10 | HU-062, HU-063 | Sí | **Sí** |
| PT-043 | Retorno sin vehículo: el bien queda resguardado en sitio | A | `ACT-05` `ACT-04` | CU-10 | HU-065 | Deg. | No |

### 2.8 M-09 Combustible

| ID | Pantalla | Cli | Rol | CU | HU | Sin red | Papel |
|---|---|---|---|---|---|---|---|
| PT-044 | Solicitud del fondo de combustible del período | A | `ACT-04` | CU-12 | HU-071 | No | **Sí** |
| PT-045 | Aprobación del fondo contra cuota y partida | A | `ACT-08` | CU-12 | HU-072, HU-073 | No | No |
| PT-046 | Ampliación del fondo agotado y resolución de la prelación | A | `ACT-04` `ACT-08` | CU-12 | HU-075 | No | No |
| PT-047 | Emisión de la asignación de combustible con folio | A | `ACT-07` | CU-13 | HU-076, HU-077, HU-078 | No | **Sí** |
| PT-048 | Entrega del fondo y registro de su custodia | A | `ACT-07` | CU-13 | HU-074 | No | **Sí** |
| PT-049 | Anulación de la asignación con acta | A | `ACT-07` | CU-13 | HU-080 | No | **Sí** |
| PT-050 | Ciclo de vida del vale y arqueo del fondo | A | `ACT-07` | CU-13, CU-15 | HU-074, HU-079, HU-080 | No | No |
| PT-051 | Declaración de la fuente de todo abastecimiento y unicidad del comprobante | A | `ACT-07` `ACT-04` | CU-14, CU-15 | HU-083, HU-087 | No | No |

### 2.9 M-16 Sincronización y conflictos

| ID | Pantalla | Cli | Rol | CU | HU | Sin red | Papel |
|---|---|---|---|---|---|---|---|
| PT-052 | Panel de sincronización de dispositivos | A | `ACT-04` `ACT-10` | CU-11 | HU-066, HU-067 | No | No |
| PT-053 | **Cola de conflictos** — [difícil §7.1](mapa-de-navegacion.md) | A | `ACT-04` `ACT-10` | CU-11 | HU-068 | No | No |
| PT-054 | **Comparador de dos versiones lado a lado** — [difícil §7.1](mapa-de-navegacion.md) | A | `ACT-04` `ACT-10` | CU-11 | HU-068 | No | No |
| PT-055 | Resolución por lote con criterio declarado | A | `ACT-04` | CU-11 | HU-068 | No | No |
| PT-056 | Estado del espejo de ARGOS y Talento Humano | A | `ACT-01` `ACT-04` | CU-11 | HU-069 | No | No |
| PT-057 | Registro de campo que llega después del cierre de la bitácora | A | `ACT-04` | CU-11 | HU-070 | No | No |

### 2.10 M-19 Seguimiento en ruta

| ID | Pantalla | Cli | Rol | CU | HU | Sin red | Papel |
|---|---|---|---|---|---|---|---|
| PT-058 | Tablero de seguimiento en ruta, con antigüedad del dato | A | `ACT-04` | CU-08 | HU-057 | No | No |
| PT-059 | Detalle de la misión en ruta con sus hitos | A | `ACT-04` `ACT-05` | CU-08 | HU-047, HU-055 | No | No |
| PT-060 | Ampliación del alcance autorizado, con versionado | A | `ACT-04` `ACT-03` | CU-08, CU-09 | HU-055 | No | No |
| PT-061 | Recepción de la interrupción y resolución de su desenlace | A | `ACT-04` | CU-09 | HU-058, HU-059, HU-060 | No | No |
| PT-062 | Relevo de motorista en ruta: resolución desde oficina | A | `ACT-04` | CU-09, CU-07 | HU-045, HU-061 | No | Parc. |

### 2.11 M-13 Liquidación y cierre

| ID | Pantalla | Cli | Rol | CU | HU | Sin red | Papel |
|---|---|---|---|---|---|---|---|
| PT-063 | Cola de liquidación, con lo que bloquea cada misión | A | `ACT-04` | CU-15 | HU-091 | No | No |
| PT-064 | **Conciliación galonaje contra kilometraje** — [difícil §7.4](mapa-de-navegacion.md) | A | `ACT-04` | CU-15 | HU-088, HU-084 | No | No |
| PT-065 | Conciliación del fondo: sobrante y faltante tipificados | A | `ACT-04` | CU-15 | HU-089 | No | Parc. |
| PT-066 | Conciliación de peajes punto por punto | A | `ACT-04` | CU-15 | HU-090, HU-086 | No | No |
| PT-067 | Bloqueo de la liquidación por segregación de funciones | A | `ACT-04` `ACT-07` | CU-15 | HU-091 | No | No |
| PT-068 | Cadena de trazabilidad y propuesta de cierre | A | `ACT-04` | CU-15 | HU-092 | No | No |
| PT-069 | Cierre de la misión con la cadena completa | A | `ACT-08` | CU-16 | HU-093 | No | No |
| PT-070 | Cierre con hallazgo tipificado | A | `ACT-08` | CU-16 | HU-094 | No | No |
| PT-071 | Hallazgo posterior sobre misión `CERRADA` — expediente nuevo, sin reapertura | A | `ACT-08` `ACT-12` | CU-16 | HU-095 | No | No |

### 2.12 M-03 / M-04 Flota y expediente del vehículo

| ID | Pantalla | Cli | Rol | CU | HU | Sin red | Papel |
|---|---|---|---|---|---|---|---|
| PT-072 | Padrón de flota con estado operativo | A | `ACT-04` `ACT-14` | CU-17 | HU-102 | No | No |
| PT-073 | Expediente del vehículo: vista completa del ciclo de vida | A | `ACT-04` `ACT-14` `ACT-11` | CU-17 | HU-096 – HU-104 | No | No |
| PT-074 | Alta del vehículo con título de tenencia | A | `ACT-14` | CU-17 | HU-096 | No | **Sí** |
| PT-075 | Placa y estado de la lámina (sin placa es estado válido) | A | `ACT-14` `ACT-04` | CU-17 | HU-097 | No | No |
| PT-076 | Ficha técnica que habilita: peso bruto, ejes, capacidad | A | `ACT-04` | CU-17 | HU-098 | No | Parc. |
| PT-077 | Tarjeta de responsabilidad y traspaso de custodia | A | `ACT-14` `ACT-13` | CU-17 | HU-099 | No | **Sí** |
| PT-078 | Vencimientos documentales y alertas dirigidas al puesto | A | `ACT-04` | CU-17 | HU-101 | No | No |
| PT-079 | Habilitación del vehículo para operar en flota | A | `ACT-04` | CU-17 | HU-102 | No | No |
| PT-080 | Descargo del bien propio con acta y resolución | A | `ACT-14` | CU-17 | HU-103 | No | **Sí** |
| PT-081 | Retiro de flota de un bien ajeno (comodato, alquiler) | A | `ACT-14` | CU-17 | HU-104 | No | **Sí** |

### 2.13 M-05 Motoristas y habilitación

| ID | Pantalla | Cli | Rol | CU | HU | Sin red | Papel |
|---|---|---|---|---|---|---|---|
| PT-082 | Padrón de motoristas con su habilitación vigente | A | `ACT-04` | CU-18 | HU-105, HU-107 | No | No |
| PT-083 | Captura de la licencia como dato propio de SIGTI, con fotografía | A | `ACT-04` | CU-18 | HU-105 | No | Parc. |
| PT-084 | Tipos de vehículo habilitados, derivados de la categoría | A | `ACT-04` | CU-18 | HU-106 | No | No |
| PT-085 | Vigencia de la habilitación y alertas anticipadas | A | `ACT-04` | CU-18 | HU-107 | No | No |
| PT-086 | Declaración de conductor fuera del padrón, con el mismo rigor | A | `ACT-04` | CU-18, CU-04 | HU-109, HU-108 | No | No |
| PT-087 | Inhabilitación con causa y encaminamiento de las misiones afectadas | A | `ACT-04` | CU-18 | HU-110 | No | No |

### 2.14 M-14 Auditoría y reportes

| ID | Pantalla | Cli | Rol | CU | HU | Sin red | Papel |
|---|---|---|---|---|---|---|---|
| PT-088 | Consulta de la pista de auditoría | A | `ACT-12` | CU-16 | — | No | No |
| PT-089 | Rastro del expediente de extremo a extremo, con sus huecos visibles | A | `ACT-12` | CU-16 | HU-092 | No | No |
| PT-090 | Exportación del paquete de evidencia (PDF con índice, anexos, hoja de cálculo) | A | `ACT-12` `ACT-08` | CU-16 | — | No | No |
| PT-091 | Reporte de intentos bloqueados por segregación de funciones | A | `ACT-12` `ACT-08` | CU-02, CU-06, CU-15 | HU-010, HU-039, HU-091 | No | No |
| PT-092 | Histórico de cambios de parámetros normativos con vigencia | A | `ACT-12` | — | — | No | No |
| PT-093 | Registro de consultas a datos de personas externas | A | `ACT-12` | — | — | No | No |

### 2.15 M-17 Traslado de personas externas

| ID | Pantalla | Cli | Rol | CU | HU | Sin red | Papel |
|---|---|---|---|---|---|---|---|
| PT-094 | Manifiesto de personas trasladadas | A/C | `ACT-04` `ACT-05` | CU-05, CU-06 | HU-031 | Sí | **Sí** |
| PT-095 | Consulta del manifiesto bajo necesidad de conocer, con registro | A/C | `ACT-05` `ACT-06` `ACT-12` | CU-06, CU-08 | — | Sí | No |

### 2.16 M-01 / M-02 Administración y operación

| ID | Pantalla | Cli | Rol | CU | HU | Sin red | Papel |
|---|---|---|---|---|---|---|---|
| PT-096 | Usuarios, puestos y asignaciones vigentes | A | `ACT-01` | — | — | No | No |
| PT-097 | Asignación puesto↔rol con control de acumulación incompatible | A | `ACT-01` `ACT-08` | — | HU-010 | No | No |
| PT-098 | Catálogos maestros (M-02) | A | `ACT-01` `ACT-08` | — | — | No | No |
| PT-099 | Carga de parámetros normativos con vigencia y respaldo documental | A | `ACT-01` | — | — | No | No |
| PT-100 | Aprobación de la puesta en vigencia — doble control | A | `ACT-08` | — | — | No | No |
| PT-101 | Panel de salud: qué está mal y qué hacer ([RNF-20](../02-requisitos/no-funcionales/RNF-20-observabilidad-y-diagnostico.md)) | A | `ACT-01` | — | — | No | No |
| PT-102 | Respaldo y restauración para alguien sin especialización ([RNF-09](../02-requisitos/no-funcionales/RNF-09-instalacion-respaldo-y-restauracion.md)) | A | `ACT-01` | — | — | No | No |

---

## 3. Cliente de campo

**Todas funcionan sin red. No es una característica: es la condición de operación** ([RNF-03](../02-requisitos/no-funcionales/RNF-03-operacion-sin-conectividad.md), [RNF-12](../02-requisitos/no-funcionales/RNF-12-uso-en-campo.md)).

| ID | Pantalla | Cli | Rol | CU | HU | Sin red | Papel |
|---|---|---|---|---|---|---|---|
| PT-103 | Ingreso sin red contra las credenciales del paquete de misión | C | `ACT-06` `ACT-10` | CU-08 | HU-046 | Sí | No |
| PT-104 | **Mi misión** — raíz única del cliente de campo | C | `ACT-06` `ACT-10` | CU-08 | HU-046 | Sí | No |
| PT-105 | **Registro en ruta: llegué, salí, estoy esperando** — [difícil §7.3](mapa-de-navegacion.md) | C | `ACT-06` | CU-08 | HU-047 | Sí | Parc. |
| PT-106 | Entrega de carga y de personas en ruta, con evidencia | C | `ACT-06` | CU-08 | HU-048 | Sí | **Sí** |
| PT-107 | Paso por caseta de peaje | C | `ACT-06` | CU-14 | HU-049, HU-085 | Sí | No |
| PT-108 | Discrepancia de peaje y reclamo | C | `ACT-06` | CU-14 | HU-050, HU-086 | Sí | No |
| PT-109 | Abastecimiento de combustible con comprobante | C | `ACT-06` | CU-14 | HU-051, HU-082, HU-083 | Sí | Parc. |
| PT-110 | Consumo sin comprobante y gasto imprevisto | C | `ACT-06` | CU-14 | HU-052, HU-087 | Sí | No |
| PT-111 | Aviso de odómetro menor a la última lectura conocida | C | `ACT-06` | CU-08, CU-14 | HU-053 | Sí | No |
| PT-112 | Pendientes de envío y adjuntos en espera | C | `ACT-06` `ACT-10` | CU-11 | HU-054 | Sí | No |
| PT-113 | Solicitud de ampliación del alcance autorizado desde la ruta | C | `ACT-06` | CU-08 | HU-055 | Sí | No |
| PT-114 | Respaldo en papel: hoja de bitácora con folio | C | `ACT-06` `ACT-10` | CU-08 | HU-056 | Sí | **Sí** |
| PT-115 | Actualización de estado y última posición conocida | C | `ACT-06` | CU-08 | HU-057 | Sí | No |
| PT-116 | **Registro de interrupción en ruta** — avería, accidente, robo, otra | C | `ACT-06` | CU-09 | HU-058 | Sí | No |
| PT-117 | Desenlace de la interrupción, comunicado al motorista | C | `ACT-06` | CU-09 | HU-060 | Sí | No |
| PT-118 | Relevo de motorista en ruta con acta y corte de odómetro | C | `ACT-06` | CU-09, CU-07 | HU-045, HU-061 | Sí | **Sí** |
| PT-119 | Retorno y cierre de la bitácora desde el campo | C | `ACT-06` `ACT-10` | CU-10 | HU-062, HU-065 | Sí | Parc. |
| PT-120 | Estado de sincronización del dispositivo | C | `ACT-06` `ACT-10` | CU-11 | HU-066, HU-067 | Sí | No |
| PT-121 | Registro de la salida sin conectividad, en el predio | C | `ACT-05` `ACT-10` | CU-06 | HU-042 | Sí | **Sí** |
| PT-122 | Captura de solicitud en delegación sin red | C | `ACT-10` | CU-01 | HU-007 | Sí | **Sí** |
| PT-123 | **Digitación diferida desde el papel**, con foto del original | C | `ACT-10` | CU-11, CU-10 | HU-064, HU-007 | Sí | **Sí** |
| PT-124 | Constatación de la identificación institucional del vehículo | C | `ACT-14` `ACT-13` | CU-17 | HU-100 | Sí | **Sí** |
| PT-125 | Consulta de mis documentos: orden de misión y salvoconducto | C | `ACT-06` | CU-08, CU-03 | HU-017, HU-019 | Sí | No |

---

## 4. Superficie pública

| ID | Pantalla | Cli | Rol | CU | HU | Sin red | Papel |
|---|---|---|---|---|---|---|---|
| PT-126 | Verificación del documento por QR — **mínimo verificable, nunca el expediente** | P | `ACT-15` sin autenticar | CU-03, CU-05 | HU-019, HU-035 | No | No |

`[C]` Sujeta a que la institución acepte exponer un punto público en internet con despliegue on-premise — insumo abierto. **La vía degradada (huella impresa, código corto, consulta telefónica) sí se diseña hoy** y puede terminar siendo la única.

---

## 5. Recuento: qué se puede diseñar y qué no

| Situación | Pantallas | Qué significa para el diseñador |
|---|---|---|
| **Bloqueadas por el insumo #2** — replican un formato en papel | **27** | No se dibujan hasta tener el formato. Dibujarlas antes es garantizar que hay que rehacerlas |
| **Parcialmente bloqueadas** — una sección replica papel | **8** | Se diseña hoy la estructura, los controles nuevos y los mensajes; se deja el bloque de campos como marco vacío |
| **Sin equivalente en papel** — se diseñan ya | **91** | Trabajo disponible desde el primer día, incluidas **las cinco pantallas difíciles** |
| **Total** | **126** | 102 administrativo · 23 campo · 1 pública |

### 5.1 Por dónde empezar

Las cinco pantallas difíciles no replican papel, son las que más valor destruyen si se diseñan mal, y **todas se pueden diseñar hoy**:

| Orden | Pantalla | Por qué primero |
|---|---|---|
| 1 | `PT-053` / `PT-054` **Cola de conflictos** | La más difícil del sistema. Si se deja para el final, se diseña bajo presión y mal |
| 2 | `PT-105` **Registro en ruta** | Decide la adopción. Si el motorista no la usa, todo lo demás da igual |
| 3 | `PT-013` **Bandeja de autorización** | Es el cuello de botella del proceso completo |
| 4 | `PT-064` **Conciliación galonaje contra kilometraje** | Es lo que el Tribunal Superior de Cuentas va a mirar |
| 5 | `PT-028` **Rechazo por licencia no habilitante** | Es el bloqueo de mayor valor legal, y el usuario no lo resuelve reintentando |

Después, el resto del cliente de campo (`PT-103` a `PT-125`, quitando las cuatro que replican papel), porque comparte sistema de interacción con `PT-105` y porque tiene las restricciones más duras.

### 5.2 Las 27 bloqueadas, y qué formato hay que pedirle a la institución

Esta lista es el contenido del **insumo #2** visto desde el diseño. Sirve para pedirle a la institución exactamente lo que hace falta, en lugar de "los formatos".

| Formato en papel que hay que conseguir | Pantallas que desbloquea |
|---|---|
| Requisición o solicitud de vehículo | PT-007, PT-122 (y PT-008, PT-012 parciales) |
| Orden de misión | PT-034, PT-035, PT-036, PT-037 |
| Permiso de circulación en día u hora inhábil y su salvoconducto | PT-020, PT-023, PT-024 |
| Hoja de salida / control de despacho del predio | PT-039, PT-121 |
| Acta de entrega-recepción del vehículo | PT-040 |
| Bitácora de vehículo (talonario, con su folio propio si lo trae — insumo #46) | PT-042, PT-114, PT-123 (y PT-105, PT-119 parciales) |
| Solicitud de fondo de combustible | PT-044 |
| Vale de combustible y su constancia de entrega | PT-047, PT-048, PT-041 |
| Acta de anulación de vale | PT-049 |
| Acta de relevo de motorista | PT-118 |
| Alta de bien / ficha de inventario del vehículo | PT-074 (y PT-076 parcial) |
| Tarjeta de responsabilidad | PT-077 |
| Acta de descargo o baja de bien | PT-080, PT-081 |
| Acta de constatación física | PT-124 |
| Manifiesto de personas trasladadas | PT-094 |
| Constancia de entrega de carga | PT-106 |
| Descargo o liquidación de misión | PT-065 (parcial) |
| Registro de licencia del motorista | PT-083 (parcial) |
| Control de combustible del motorista | PT-109 (parcial) |

Complementan el insumo #2: **#46** (¿el talonario de bitácora trae folio propio? Si se conserva, hay dos numeraciones que cruzar) y **#70** (parque real de impresoras y tamaño de papel, que decide si el QR impreso es vía primaria o solo conveniencia).

---

## 6. Todo formato impreso, sin excepción

Aplica a los documentos que producen PT-023, PT-034 a PT-037, PT-040, PT-042, PT-047 a PT-049, PT-077, PT-080, PT-081, PT-094, PT-106, PT-114, PT-118 y PT-124:

- **Folio único**, que no se duplica ni se recicla, aunque se haya emitido sin red ([RNF-21](../02-requisitos/no-funcionales/RNF-21-integridad-de-folios-y-correlativos.md)).
- **Código QR de verificación**, grande y en posición fija.
- **Espacio para firma y sello**. No hay firma electrónica certificada en el país: la autorización es interna, con registro completo de quién, cuándo, desde dónde y sobre qué contenido.
- **Hash del documento electrónico en el pie**, legible a simple vista para el contraste manual cuando no hay datos móviles.
- **Legible en impresora matricial o láser común, tamaño carta, útil en blanco y negro.** Nada puede depender del color para significar.

**El salvoconducto (`PT-023`) es el caso más exigente del sistema.** Lo va a revisar un agente en carretera, de pie, posiblemente de noche, con luz de linterna. Los cuatro datos que ese agente necesita —vehículo, ventana temporal autorizada, autoridad que firmó y vigencia— van **en el tercio superior, en cuerpo grande**, antes que cualquier otra cosa. Todo lo demás es secundario a eso.

---

## 7. Lo que este inventario no cubre

- **M-11 Mantenimiento y Taller** y **M-12 Incidentes** más allá del registro en ruta: el Bloque 3 no escribió historias para ellos todavía. Las pantallas de `ACT-11` Encargado de Mantenimiento se inventariarán cuando existan sus historias. Lo que sí está cubierto es lo que toca la misión: declarar el vehículo fuera de circulación (`PT-061`) y su estado operativo (`PT-072`, `PT-079`).
- **M-18 Peajes** en su faceta de administración del catálogo de puntos y tarifas: entra en `PT-098` y `PT-099` como catálogo y parámetro, sin pantalla propia hasta que haya historias.
- **El sistema visual** — tipografía, paleta, retícula, componentes. Es del diseñador externo. El stack de interfaz está diferido al Sprint 2 por [`ADR-000`](../03-arquitectura/adr/ADR-000-diferir-seleccion-de-stack.md), así que **los mockups deben ser agnósticos de tecnología**.
- **El orden de los campos de las 27 pantallas bloqueadas.** Lo fija el formato de la institución, no el diseño.
