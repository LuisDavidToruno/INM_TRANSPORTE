# Diccionario de datos — entidades núcleo

Complemento de [`README.md`](README.md). Detalle campo por campo de las **entidades núcleo**: las que sostienen los seis invariantes estructurales y las que un auditor recorrería para reconstruir una misión.

**No hay tipos físicos.** Los tipos son **lógicos** y el Sprint 2 los materializa contra el stack elegido ([`ADR-000`](../adr/ADR-000-diferir-seleccion-de-stack.md)).

**Revisión aplicada.** Incorpora las correcciones de `HB34-50` a `HB34-64` de [`H-B34-002`](../../05-calidad/hallazgos/H-B34-002-revision-arquitectura-bloque-4.md). El registro completo, con qué se corrigió y dónde, está en el [`README`](README.md) — cada corrección lleva además su nota visible en el punto donde se aplicó. Secciones nuevas: **§14** `subrango_de_folio`, **§15** `compromiso_proyectado`, **§19** `adjunto`, **§20** `solicitud_transporte`, **§21** `cumplimiento_de_objeto` y `constatacion_al_despachar`, **§22** `conflicto_de_recurso`, **§23** `ejercicio`, **§24** `expediente_convalidacion`. El resumen de entidades pasó a **§25** y la trazabilidad a **§26**.

---

## 0. Convenciones del diccionario

### 0.1 Tipos lógicos

| Tipo lógico | Significa |
|---|---|
| `identificador` | Opaco, generado en el cliente, sin significado de negocio, tipo UUID (`RN-44`) |
| `referencia` | Apunta a otra entidad por su `identificador` |
| `valor_copiado` | Copia literal tomada al momento del hecho. **No es referencia**: no cambia si el origen cambia (`RNF-15`) |
| `texto` / `texto_corto` | Cadena libre / cadena acotada |
| `entero`, `decimal`, `monto`, `magnitud` | Numéricos. `magnitud` lleva **unidad declarada** junto al número |
| `marca_de_tiempo`, `fecha`, `rango_de_fechas` | Temporales. `rango_de_fechas` lleva inicio y fin, fin nulo = vigente |
| `booleano` | Sí / no. **Nunca se usa para modelar un estado con historia** |
| `enumerado_configurable` | Valor de `catalogo_tipificado`. **Nunca una constante en código** (`RN-39`) |
| `huella` | Resumen criptográfico. No reversible |
| `adjunto` | Referencia a archivo con su propia huella y metadatos |
| `derivado` | **No se captura ni se edita**: se calcula. Se documenta su fórmula y su fuente |

### 0.2 Obligatoriedad

| Marca | Significa |
|---|---|
| **Ob.** | Obligatorio siempre. Sin él, el registro no se guarda |
| **Cd.** | Obligatorio bajo condición declarada en la columna de dominio |
| **Op.** | Opcional. Su ausencia es un estado válido, no un dato pendiente |
| **Dv.** | Derivado. No se captura |

### 0.3 El bloque de trazabilidad temporal `BTT`

Para no repetirlo veintiséis veces: **toda entidad de hecho** —no las de catálogo— incluye estos campos. Cuando en el diccionario aparece la fila `BTT`, significa este bloque completo. Materializa el invariante `M-04` (`RN-46`, `RN-40`, `RN-47`, máquina de estados §6.4).

| Campo | Tipo lógico | Ob. | Dominio | Regla | Qué pasa si falta |
|---|---|---|---|---|---|
| `ocurrido_en` | `marca_de_tiempo` | Ob. | Declarado por el actor o tomado del reloj en captura inmediata. `≤ capturado_en` con tolerancia configurable | `RN-46`, `RN-40` | **No se guarda.** Sin él ningún cálculo normativo se puede resolver: no se sabe qué tarifa ni qué matriz aplicaba |
| `capturado_en` | `marca_de_tiempo` | Ob. | Reloj del dispositivo. **No editable** | `RN-46` | No se guarda. Es lo que permite medir la reconstrucción posterior |
| `recibido_en` | `marca_de_tiempo` | Cd. | Reloj del servidor. Nulo mientras el registro no ha sincronizado | `RN-43` | Nulo es válido y significa "aún en el dispositivo" |
| `zona_horaria` | `texto_corto` | Ob. | Del dispositivo al capturar | máq. estados §9.1 | No se guarda |
| `desfase_reloj_medido` | `magnitud` | Dv. | Medido por el servidor al sincronizar | máq. estados §6.4 | Se calcula; permite auditar después un dispositivo con el reloj corrido **sin corregir el dato** |
| `modo_de_captura` | `enumerado_configurable` | Ob. | `EN_LINEA` · `DESCONECTADA_SINCRONIZADA` · `DIGITACION_DIFERIDA_DE_PAPEL` · `CORRECCION_POSTERIOR` | `RN-47`, máq. estados §6.4 | No se guarda |
| `digitado_por` | `referencia` a `persona` | Cd. | Obligatorio si `modo_de_captura = DIGITACION_DIFERIDA_DE_PAPEL` | `RN-47` | Bloqueo: la digitación diferida sin digitante identificado no acredita nada |
| `id_adjunto_original` | `adjunto` | Cd. | Escaneo o fotografía del papel. Obligatorio en digitación diferida | `RN-47` | Bloqueo |
| `motivo_del_diferimiento` | `enumerado_configurable` | Cd. | Obligatorio si el desfase `capturado_en − ocurrido_en` supera el umbral configurado | `RN-46` | El registro se marca **diferido** y exige motivo antes de aplicarse |
| `id_dispositivo` | `referencia` | Ob. | Dispositivo emisor | máq. estados §6.2 | No se guarda: sin dispositivo no hay cadena por dispositivo |
| `secuencia_dispositivo` | `entero` | Ob. | **Monotónico por dispositivo.** Define el orden de aplicación, no el reloj | máq. estados §6.2 | No se guarda. Sin ella el servidor no puede detectar huecos ni ordenar |
| `origen_de_red` | `texto_corto` | Cd. | El "desde dónde" exigido por NRM-01. Nulo en captura desconectada | `RN-03` | Nulo solo admisible en modo desconectado |

---

## 1. `orden_mision`

**Módulo M-07.** La unidad de control administrativo-contable. **No tiene vehículo ni motorista** — los tiene `tramo_mision` (decisión `D-01`).

| Campo | Tipo lógico | Ob. | Dominio | Regla | Qué pasa si falta |
|---|---|---|---|---|---|
| `id_orden_mision` | `identificador` | Ob. | Generado en el cliente | `RN-44` | No se guarda |
| `id_delegacion_emisora` | `referencia` | Ob. | Determina el rango de folios que se usará | `RNF-21` | No se guarda |
| `id_solicitud_transporte` | `referencia` | Op. | Nulo cuando la orden nace de convalidación de acto sin autorización previa | `RN-73` | Nulo obliga a expediente de convalidación con cronología declarada tal como ocurrió |
| `id_unidad_organizativa_requirente` | `referencia` | Ob. | Unidad que origina la necesidad | actores §3 | No se guarda: define alcance de datos e imputación |
| `motivo_de_viaje` | `enumerado_configurable` | Ob. | Catálogo institucional | `RN-39` | No se guarda |
| `objeto_principal` | `referencia` a `objeto_del_traslado` | Ob. | Declara cuál manda para el orden de reducción de capacidad | `RN-67`, `RN-21` | Bloqueo: sin objeto principal no se sabe qué se reduce si hay exceso |
| `id_vinculacion_argos` | `texto_corto` | Op. | Clave con la que SIGTI expone hechos a ARGOS | `RN-81` | Nulo: la misión no se expone hasta que exista |
| `id_dispositivo_portador` | `referencia` | Cd. | Designado al despachar. Único cuya cadena se aplica automáticamente | máq. estados §6.3 regla 4 | Sin portador designado, toda cadena entrante es "de dispositivo no portador" y no se aplica sola |
| `estado_corriente` | `derivado` | Dv. | Proyección del diario de `transicion_orden_mision` | máq. estados P-1, `RN-06` | **Nunca se escribe.** Si se escribiera, dos dispositivos producirían última-escritura-gana (decisión `D-03`) |
| `tiene_divergencia_pendiente` | `derivado` | Dv. | Verdadero si existe `conflicto_de_sincronizacion` abierto | `BD-08` | Impide liquidar mientras sea verdadero |
| `BTT` | — | Ob. | Bloque completo | `RN-46` | Ver §0.3 |

> **Corrección — hallazgo `HB34-57`.** `id_folio` estaba declarado **`Ob.`**, y según la convención del §0.2 eso significa *«sin él, el registro no se guarda»*. Dos cosas rompía:
>
> 1. `EF-02` —**autoridad**— reserva el folio en `T-08` programar y lo consume en `T-12` despachar. Una orden en `BORRADOR` existe desde `T-01`, y `T-01` se ejecuta **sin conectividad**: exigir folio para guardar hacía imposible crear una solicitud en campo. La propia columna «Qué pasa si falta» lo admitía sin darse cuenta —decía *«no se puede imprimir ni despachar»*, que es un bloqueo de `T-12`, no de creación.
> 2. `EF-02` también dice que al **desprogramar** (`T-11`) el folio reservado **se anula** y al reprogramar se toma uno nuevo. Un campo escalar **no puede guardar los dos**: al reprogramar se sobrescribe y el folio anulado queda como hueco sin explicación — el `0` que `RNF-21` exige (*«huecos en la numeración sin explicación registrada: 0»*).
>
> El campo se **elimina**. La estructura correcta ya existía y es la que manda: `ORDEN_MISION ||--o{ DOCUMENTO_EMITIDO`, `FOLIO ||--o| DOCUMENTO_EMITIDO` y `FOLIO ||--o{ EVENTO_ESTADO_FOLIO` para el ciclo `RESERVADO → ANULADO`. Los folios de una orden —el reservado y anulado en la desprogramación, y el consumido al reprogramar— se leen por ahí, con su motivo cada uno.

**Invariantes.** No se cierra sin cadena de trazabilidad completa (`RN-08`); si está incompleta, cierra como `CERRADA_CON_HALLAZGO`. Ninguna ventana de tramo puede exceder la vigencia del `titulo_de_tenencia` del vehículo usado (`RN-62`). Todo uso de un vehículo del Estado se ampara en una orden, **cualquiera sea su régimen de uso** (`RN-59`).

---

## 2. `transicion_orden_mision`

**Módulo M-14.** El diario. **Es lo que sincroniza el cliente, no el estado.**

| Campo | Tipo lógico | Ob. | Dominio | Regla | Qué pasa si falta |
|---|---|---|---|---|---|
| `id_transicion` | `identificador` | Ob. | Generado en el origen. **Llave de idempotencia** | `RN-44` | No se guarda. Sin ella el reenvío tras un corte de red duplicaría la transición |
| `id_orden_mision` | `referencia` | Ob. | — | `RN-06` | No se guarda |
| `id_transicion_anterior` | `referencia` | Cd. | Nulo solo en la primera de la misión | `RNF-04` | Rompe el encadenamiento: la transición queda en conflicto |
| `codigo_transicion` | `enumerado_configurable` | Ob. | `T-01` a `T-22` de la máquina de estados | `RN-06` | No se guarda |
| `estado_origen_esperado` | `valor_copiado` | Ob. | **El que tenía quien la ejecutó**, que puede diferir del servidor | máq. estados §6.3 regla 3 | No se guarda. Es lo que permite detectar el conflicto sin descartar el hecho |
| `estado_destino` | `enumerado_configurable` | Ob. | De la lista cerrada de estados | `RN-06` | No se guarda |
| `tipo_de_transicion` | `enumerado_configurable` | Ob. | `AVANCE` · `RAMA` · `CORRECCION` · `AUTORIZACION_DE_NIVEL` | máq. estados §9.1 | No se guarda |
| `id_autoria_congelada` | `referencia` | Ob. | Persona **y puesto** congelados | `RNF-15`, `RN-03` | No se guarda. Un asiento sin competencia declarada no acredita nada ante el TSC |
| `id_codigo_autorizacion_fuera_de_linea` | `referencia` | Cd. | Obligatorio si la transición se autorizó sin red | máq. estados §6.6 | La transición queda sin autorizador válido y va a conflicto |
| `motivo_tipificado` | `enumerado_configurable` | Cd. | Obligatorio donde la tabla 3.1 de la máquina lo exige | `RN-06` | Bloqueo duro en esas transiciones |
| `motivo_texto` | `texto` | Op. | Complemento del tipificado, nunca sustituto | `RN-06` | Se acepta vacío |
| `version_de_aplicacion` | `valor_copiado` | Ob. | Permite explicar comportamientos históricos | máq. estados §9.1 | No se guarda |
| `ubicacion_aproximada` | `magnitud` | Op. | Solo si el dispositivo la tiene y la institución lo autoriza. `[C]` insumo #1 | máq. estados §9.1 | Nulo válido |
| `estado_de_sincronizacion` | `enumerado_configurable` | Ob. | `PENDIENTE_DE_ENVIO` · `ENVIADA` · `APLICADA` · `EN_ESPERA_DE_PREDECESOR` · `EN_CONFLICTO` · `DUPLICADA_IGNORADA` · `RESUELTA_APLICADA` · `RESUELTA_DESCARTADA` | `RN-45` | No se guarda |
| `es_de_dispositivo_portador` | `booleano` | Ob. | Falso = legítima pero no automática | máq. estados §6.3 regla 4 | No se guarda |
| `huella_contenido` | `huella` | Ob. | Sobre representación canónica | `RNF-04` | No se guarda: la cadena no cierra |
| `huella_anterior` | `huella` | Cd. | Nula solo en la primera | `RNF-04` | Ídem |
| `BTT` | — | Ob. | Bloque completo | `RN-46` | Ver §0.3 |

**Entidad dependiente — `resultado_verificacion`** (1 : 0..N). Un registro **por cada bloqueo duro evaluado**:

> **Corrección — hallazgo `HB34-55`.** El diagrama de la Vista 5 declaraba esta misma relación como `1..N` y **contradecía a este diccionario**. Manda el `0..N`: `T-01` crear borrador no evalúa ningún `BD-nn`. El diagrama quedó corregido.

| Campo | Tipo lógico | Ob. | Dominio | Regla | Qué pasa si falta |
|---|---|---|---|---|---|
| `codigo_bloqueo` | `enumerado_configurable` | Ob. | `BD-01` a `BD-11` | máq. estados §4 | No se guarda |
| `resultado` | `enumerado_configurable` | Ob. | `CUMPLE` · `NO_CUMPLE` · `ADVERTENCIA` · `NO_EVALUABLE` | `RN-03` | No se guarda |
| `insumos_usados` | `valor_copiado` | Ob. | Los **datos concretos** con que se verificó: número de licencia, categorías, vencimiento leído, atributos del vehículo, fecha de fin de rango evaluada | máq. estados §9.2 | **Bloqueo.** "Licencia verificada: sí" no defiende a nadie el día del siniestro |
| `id_version_tabla_parametrica` | `referencia` | Cd. | Obligatorio si la verificación usó tabla paramétrica | `RN-41` | No se puede reproducir la verificación |
| `antiguedad_espejo` | `magnitud` | Cd. | Obligatorio si la verificación usó dato espejo | `RN-50` | No se sabe si se autorizó contra datos viejos |
| `se_continuo_pese_a_advertencia` | `booleano` | Cd. | Obligatorio si `resultado = ADVERTENCIA` | `RN-11`, `RN-16` | No queda constancia de que alguien vio la advertencia y siguió |

---

## 3. `version_alcance_autorizado`

**Módulo M-07.** `RN-77`. La entidad que evita que toda prórroga legítima se convierta en hallazgo.

| Campo | Tipo lógico | Ob. | Dominio | Regla | Qué pasa si falta |
|---|---|---|---|---|---|
| `id_version_alcance` | `identificador` | Ob. | — | `RN-44` | No se guarda |
| `id_orden_mision` | `referencia` | Ob. | — | `RN-77` | No se guarda |
| `numero_de_version` | `entero` | Ob. | 1 nace con la aprobación; crece con cada extensión | `RN-77` | No se guarda |
| `vigencia_de_la_version` | `rango_de_fechas` | Ob. | **Desde cuándo rige esta versión**, distinto de la ventana de la misión | `RN-77` | Sin ella no se puede saber qué alcance regía a la fecha de cada hecho |
| `ventana_autorizada` | `rango_de_fechas` | Ob. | Inicio y fin previstos de la misión | `RN-10`, `RN-62` | Bloqueo: la licencia y el título de tenencia se validan contra todo el rango |
| `kilometraje_estimado` | `magnitud` | Cd. | Obligatorio si la institución lo exige `[C]` | `RN-30` | Sin él la conciliación no tiene contra qué comparar |
| `id_autoria_congelada_autorizador` | `referencia` | Ob. | **No puede ser quien lo pidió** | `RN-01`, `RN-77` | Bloqueo duro |
| `id_dependencia_requirente_de_la_extension` | `referencia` | Cd. | Obligatorio si la versión agrega destino en ruta | `RN-77` | Bloqueo |
| `motivo_de_la_extension` | `enumerado_configurable` | Cd. | Obligatorio desde la versión 2 | `RN-77` | Bloqueo |
| `BTT` | — | Ob. | — | `RN-46` | Ver §0.3 |

**Entidad dependiente — `destino_autorizado`** (1 : 1..N): secuencia, ubicación, zona, propósito, tiempo previsto en sitio, dependencia requirente. **Multi-destino es la norma, no la excepción** (glosario).

**Invariante.** La coherencia de casetas (`RN-37`), el kilometraje y la ruta se evalúan contra **la versión vigente a la fecha del hecho que se está validando**, no contra la última. Un paso amparado por una extensión autorizada **no es hallazgo**.

---

## 4. `objeto_del_traslado`

**Módulo M-06.** Supertipo. Lo que se moviliza: personal, personas externas, carga, o combinación (decisión `D-09`).

| Campo | Tipo lógico | Ob. | Dominio | Regla | Qué pasa si falta |
|---|---|---|---|---|---|
| `id_objeto_traslado` | `identificador` | Ob. | — | `RN-44` | No se guarda |
| `id_solicitud_transporte` | `referencia` | Ob. | — | `RN-20` | No se guarda |
| `id_tipo_objeto_traslado` | `referencia` a catálogo | Ob. | `PERSONAL_INSTITUCIONAL` · `PERSONA_EXTERNA` · `CARGA` y los que la institución agregue | `RN-20`, `RN-39` | **Bloqueo:** sin tipo no se resuelve compatibilidad con el tipo de vehículo |
| `naturaleza` | `enumerado_configurable` | Ob. | `PERSONAS` · `CARGA`. Determina qué subtipo aplica | `RN-20` | No se guarda |
| `cantidad_personas` | `entero` | Cd. | Obligatorio si `naturaleza = PERSONAS`. **Incluye al motorista en la evaluación de capacidad** | `RN-21` | Bloqueo: no se puede validar capacidad de pasajeros |
| `peso_declarado` | `magnitud` | Cd. | Obligatorio si `naturaleza = CARGA` | `RN-21` | Bloqueo |
| `volumen_declarado` | `magnitud` | Op. | Se valida solo cuando existe también en la ficha técnica | `RN-21` | Se omite esa validación y se deja constancia |
| `requiere_condiciones_especiales` | `enumerado_configurable` | Op. | Refrigeración, materiales peligrosos, custodia armada — catálogo abierto | `RN-67` | Nulo válido |
| `id_manifiesto_persona_externa` | `referencia` | Cd. | Obligatorio si `tipo = PERSONA_EXTERNA` | `RN-51`, `RN-53` | Bloqueo |
| `id_inventario_de_carga` | `referencia` | Cd. | Obligatorio si la carga es **bien inventariable** | `RN-69` | Bloqueo: no se puede acreditar la entrega ni declarar faltante |
| `BTT` | — | Ob. | — | `RN-46` | Ver §0.3 |

**Nota de minimización.** El objeto del traslado **no contiene identidades**. Para personas externas, la identidad vive en `segmento_dato_personal` referenciado desde `linea_manifiesto` (`RN-51`, `RNF-17`). Es lo que permite exportar el dato de gestión —vehículo, ruta, costo, unidad ejecutora— sin el dato personal.

---

## 5. `tramo_mision`

**Módulo M-08.** Donde se imputa **todo** (decisión `D-01`, `RN-72`).

| Campo | Tipo lógico | Ob. | Dominio | Regla | Qué pasa si falta |
|---|---|---|---|---|---|
| `id_tramo` | `identificador` | Ob. | — | `RN-44` | No se guarda |
| `id_orden_mision` | `referencia` | Ob. | — | `RN-72` | No se guarda |
| `numero_de_tramo` | `entero` | Ob. | Secuencial dentro de la misión | `RN-72` | No se guarda |
| `id_vehiculo` | `referencia` | Ob. | Estado operativo debe ser `DISPONIBLE` al programar | `RN-19`, `BD-07` | Bloqueo duro |
| `id_motorista` | `referencia` | Ob. | **Quien efectivamente conduce**, cualquiera sea su puesto | `RN-57` | Bloqueo duro |
| `causa_de_apertura` | `enumerado_configurable` | Ob. | `INICIO_DE_MISION` · `RELEVO_DE_CONDUCTOR` · `SUSTITUCION_DE_VEHICULO` · `TRASPASO_EN_RUTA` | `RN-14`, `RN-61`, `RN-71` | No se guarda |
| `id_acta_traspaso_apertura` | `referencia` a `acta` | Cd. | Obligatorio si `causa_de_apertura ≠ INICIO_DE_MISION`. Es un `acta` con `tipo_acta = TRASPASO_EN_RUTA` (decisión `D-19`), no una entidad aparte | `RN-71` | **Bloqueo.** Sin acta no hay corte de imputación y el kilometraje se atribuye mal |
| `odometro_apertura` | `magnitud` | Ob. | Del acta de traspaso o de la salida | `RN-71`, `RN-31` | Bloqueo |
| `odometro_cierre` | `magnitud` | Cd. | Obligatorio al cerrar el tramo. **No puede ser menor al de apertura** | `RN-31` | Bloqueo; todo retroceso o salto exige justificación con respaldo |
| `id_evaluacion_habilitacion_congelada` | `referencia` | Ob. | Resultado de `RN-09` y `RN-11` con sus insumos | `RN-14` | Bloqueo: la sustitución **revalida todas las habilitaciones** |
| `kilometraje_del_tramo` | `derivado` | Dv. | `odometro_cierre − odometro_apertura`, normalizado a la unidad de la serie | `RN-89` | Se calcula |
| `BTT` | — | Ob. | — | `RN-46` | Ver §0.3 |

**Invariante.** Cuando la misión tiene más de un tramo con vehículos distintos, **el rendimiento galonaje–kilometraje nunca se calcula sobre la misión completa** (`RN-72`). Cada vehículo se concilia contra su propio rendimiento esperado y su propio tramo. La asignación original **se conserva**, no se reemplaza (`RN-14`).

---

## 6. `vehiculo`

**Módulo M-03.** El expediente, no un catálogo. Es la entidad que la frase del producto pone al centro.

| Campo | Tipo lógico | Ob. | Dominio | Regla | Qué pasa si falta |
|---|---|---|---|---|---|
| `id_vehiculo` | `identificador` | Ob. | Generado en el cliente | `RN-44` | No se guarda |
| `correlativo_institucional` | `texto_corto` | **Ob.** | **Único en la institución.** Formato `[C]` insumo #34 | `RN-15` | **Bloqueo duro.** Es la identidad operativa: sin él el vehículo no aparece en órdenes, bitácoras ni vales |
| `numero_de_bien_inventario` | `texto_corto` | Cd. | Obligatorio si `regimen = PROPIEDAD`. Primer ancla de imputación externa | `RN-66`, NRM-02 | El vehículo no se puede cruzar contra el inventario institucional |
| `chasis_vin` | `texto_corto` | Op. | Segundo ancla de imputación externa | `RN-66` | Se pierde un ancla; se cae al siguiente de la jerarquía |
| `numero_de_motor` | `texto_corto` | Op. | Tercer ancla | `RN-66` | Ídem |
| `id_unidad_organizativa_adscrita` | `referencia` | Ob. | Determina alcance de datos | actores §3 | No se guarda |
| `id_delegacion_base` | `referencia` | Op. | Nulo para flota de sede | actores §3 | Nulo válido |
| `id_expediente_superviviente` | `referencia` a `vehiculo` | Op. | No nulo = **expediente absorbido** por fusión de duplicados | decisión `D-11`, `RN-45` | Nulo es lo normal. No nulo redirige toda consulta al superviviente **sin reasignar historial** |
| `estado_operativo_corriente` | `derivado` | Dv. | Proyección del diario `evento_estado_operativo` | máq. estados §10.2 | **Nunca se escribe.** `ASIGNADO` y `EN_MISION` los fija el sistema, no una persona |
| `kilometraje_acumulado` | `derivado` | Dv. | Ver §9. **No decrece nunca** | `RN-89` | Se calcula |
| `BTT` | — | Ob. | Alta del expediente | `RN-46` | Ver §0.3 |

**Lo que NO es campo de `vehiculo`, y por qué:**

| No es campo | Dónde vive | Por qué |
|---|---|---|
| `placa` | `asignacion_de_placa` (§7) | Ni obligatoria ni única, e historizada (`RN-15`, `RN-64`) |
| `marca`, `modelo`, `anio`, `peso_bruto`, `ejes`, `capacidad` | `version_ficha_tecnica` | La ficha cambia y su cambio no puede reescribir habilitaciones pasadas |
| `regimen_de_tenencia` | `titulo_de_tenencia` | Tiene vigencia, titular, documento y reparto de rubros (`RN-62`) |
| `custodio` | `custodia_vehiculo` | Tiene acta, vigencia e historial (`RN-22`) |
| `categoria_peaje` | `asignacion_categoria_peaje` | Tiene vigencia y fundamento registrado (`RN-33`) |
| `estado_operativo` | `evento_estado_operativo` | Es un diario, no una columna (decisión `D-03`) |
| `odometro` | `lectura_odometro` sobre `serie_instrumento_medicion` | Decisión `D-06` |

---

## 7. `asignacion_de_placa` y `historial_estado_placa`

**Módulo M-03/M-04.** `RN-15`, `RN-64`. **Dos datos distintos y no intercambiables** (decisión `D-05`).

### `asignacion_de_placa` — el número asignado en el registro

| Campo | Tipo lógico | Ob. | Dominio | Regla | Qué pasa si falta |
|---|---|---|---|---|---|
| `id_asignacion_placa` | `identificador` | Ob. | — | `RN-44` | No se guarda |
| `id_vehiculo` | `referencia` | Ob. | — | `RN-64` | No se guarda |
| `numero_de_placa` | `texto_corto` | **Op.** | **Ni obligatorio ni único** en el sistema. Puede existir aunque la lámina no | `RN-15` | **Nada.** Cero asignaciones es un estado válido y operable: el vehículo se asigna, despacha y liquida igual |
| `vigencia` | `rango_de_fechas` | Ob. | **Nunca se sobrescribe**: se cierra el rango anterior y se abre uno nuevo | `RN-64` | Bloqueo |
| `id_documento_sustitutivo` | `adjunto` | Cd. | Constancia del Instituto de la Propiedad, cuando no hay lámina | `RN-15`, `RN-65` | Sin lámina y sin respaldo vigente, **no se despacha** (`RN-65`) |

### `historial_estado_placa` — la lámina física

| Campo | Tipo lógico | Ob. | Dominio | Regla | Qué pasa si falta |
|---|---|---|---|---|---|
| `estado_de_placa` | `enumerado_configurable` | Ob. | `CON_LAMINA` · `NUMERO_ASIGNADO_SIN_LAMINA` · `SIN_NUMERO_ASIGNADO` · `LAMINA_EXTRAVIADA` · `LAMINA_RETENIDA_POR_AUTORIDAD` · `EN_TRAMITE_DE_REPOSICION` | `RN-64` | No se guarda |
| `vigencia` | `rango_de_fechas` | Ob. | Historial completo | `RN-64` | No se guarda |
| `fundamento` | `adjunto` | Cd. | Obligatorio en `LAMINA_RETENIDA_POR_AUTORIDAD` y `EN_TRAMITE_DE_REPOSICION` | `RN-65` | El estado no se acredita |
| `id_paquete_identificacion_impreso` | `referencia` a `documento_emitido` | Cd. | Obligatorio para despachar sin lámina | `RN-65` | **Bloqueo duro de despacho.** El control en carretera es físico |

**Por qué dos entidades y no un campo.** Un vehículo con número asignado y sin lámina es distinto de uno sin número; y uno con la lámina retenida por autoridad es distinto de uno que la extravió. Todos circulan. Un solo campo `placa` nulo o no nulo habría colapsado seis situaciones operativamente distintas en dos.

---

## 8. `titulo_de_tenencia`

**Módulo M-03.** `RN-62`. Gobierna cuál de los dos terminales de vehículo aplica (decisión `D-07`).

| Campo | Tipo lógico | Ob. | Dominio | Regla | Qué pasa si falta |
|---|---|---|---|---|---|
| `id_titulo` | `identificador` | Ob. | — | `RN-44` | No se guarda |
| `id_vehiculo` | `referencia` | Ob. | — | `RN-62` | No se guarda |
| `regimen` | `enumerado_configurable` | Ob. | `PROPIEDAD` · `COMODATO` · `ALQUILER` · `DONACION_EN_TRAMITE` · `ASIGNACION_POR_OTRA_INSTITUCION` | `RN-62` | **Sin título vigente el vehículo no se habilita en la flota** |
| `titular` | `texto` | Ob. | Propietario o cedente | `RN-62` | Bloqueo |
| `documento_respaldo` | `adjunto` | Ob. | Convenio, contrato, acta o resolución | `RN-62` | Bloqueo |
| `vigencia` | `rango_de_fechas` | Ob. | **Fecha de fin obligatoria salvo `PROPIEDAD`** | `RN-62` | Bloqueo. Ninguna misión se programa si su ventana excede la vigencia |
| `BTT` | — | Ob. | — | `RN-46` | Ver §0.3 |

**Entidad dependiente — `rubro_asumido`** (1 : 1..N): rubro (`COMBUSTIBLE`, `MANTENIMIENTO`, `LLANTAS`, `SEGURO`, `PEAJES`, `MULTAS`, `DANOS`) y quién lo paga. Sin esto, la liquidación imputa al presupuesto de la institución gastos que contractualmente asume el arrendador.

**Invariante duro.** `evento_estado_operativo` con destino `DADO_DE_BAJA` exige `regimen = PROPIEDAD` a la fecha del hecho. Con cualquier otro régimen, el terminal correcto es `RETIRADO_DE_FLOTA`. **Declarar dado de baja un bien ajeno es un asiento falso**, y se detecta cruzando el inventario institucional contra el padrón de flota.

---

## 9. `serie_instrumento_medicion`, `lectura_odometro`, `kilometraje_acumulado`

**Módulo M-03.** `RN-89`, `RN-90`, `RN-31`. Decisión `D-06`. **Sin esta separación, cada tablero reemplazado corrompe el histórico.**

### `serie_instrumento_medicion`

| Campo | Tipo lógico | Ob. | Dominio | Regla | Qué pasa si falta |
|---|---|---|---|---|---|
| `id_serie` | `identificador` | Ob. | — | `RN-44` | No se guarda |
| `id_vehiculo` | `referencia` | Ob. | — | `RN-89` | No se guarda |
| `unidad_declarada` | `enumerado_configurable` | Ob. | `KILOMETROS` · `MILLAS` | `RN-89` | **Bloqueo.** Una lectura sin unidad no se puede normalizar ni comparar |
| `vigencia` | `rango_de_fechas` | Ob. | Se cierra al reemplazar el instrumento | `RN-90` | Bloqueo |
| `lectura_inicial_de_serie` | `magnitud` | Ob. | Con qué valor arranca el instrumento nuevo | `RN-90` | Bloqueo: no se puede empalmar con el acumulado |
| `causa_de_cierre` | `enumerado_configurable` | Cd. | `REEMPLAZO_FISICO` · `VUELTA_DE_CONTADOR` · `REPARACION` · `RECALIBRACION`. La vuelta de contador es **reemplazo lógico** con motivo propio | `RN-89`, `RN-90` | Bloqueo al cerrar |
| `id_orden_trabajo` | `referencia` | Cd. | Toda intervención del odómetro es evento **con orden de trabajo y autorización nominativa** | `RN-90` | Bloqueo duro |

### `lectura_odometro`

| Campo | Tipo lógico | Ob. | Dominio | Regla | Qué pasa si falta |
|---|---|---|---|---|---|
| `id_lectura` | `identificador` | Ob. | — | `RN-44` | No se guarda |
| `id_serie` | `referencia` | Ob. | — | `RN-89` | No se guarda |
| `valor_leido` | `magnitud` | Ob. | **Conservando la unidad original** | `RN-89` | No se guarda |
| `valor_normalizado` | `derivado` | Dv. | A la unidad canónica del sistema | `RN-89` | Se calcula |
| `contexto` | `enumerado_configurable` | Ob. | `SALIDA` · `RETORNO` · `CARGA_DE_COMBUSTIBLE` · `TRASPASO` · `INGRESO_A_TALLER` · `CONSTATACION_FISICA` | `RN-28`, `RN-71` | No se guarda |
| `justificacion_de_anomalia` | `texto` + `adjunto` | Cd. | Obligatorio ante retroceso o salto respecto de la lectura anterior de la serie | `RN-31` | **Bloqueo.** El odómetro de retorno no puede ser menor al de salida |
| `BTT` | — | Ob. | — | `RN-46` | Ver §0.3 |

### `kilometraje_acumulado` (1:1 con `vehiculo`, derivado)

Suma de los tramos válidos de todas las series, normalizada. **Nunca decrece.** El plan de mantenimiento preventivo se calcula **sobre el acumulado, jamás sobre la lectura** (`RN-89`).

**Regla de reproceso.** La continuidad se evalúa **sobre la serie ordenada por `ocurrido_en`**. Insertar una lectura anterior —digitación diferida de una bitácora en papel— **reabre la validación de todas las posteriores** y puede abrir hallazgo. Ese es exactamente el comportamiento correcto y por eso el orden es por fecha del hecho y no por orden de captura.

---

## 10. `motorista`, `licencia_conducir`, `categoria_en_licencia`

**Módulo M-05.** **La licencia es dato PROPIO de SIGTI**, no espejo — corrección de [`ADR-001`](../adr/ADR-001-integracion-argos-talento-humano.md).

### `motorista`

| Campo | Tipo lógico | Ob. | Dominio | Regla | Qué pasa si falta |
|---|---|---|---|---|---|
| `id_motorista` | `identificador` | Ob. | — | `RN-44` | No se guarda |
| `id_persona` | `referencia` | Ob. | Persona espejeada de Talento Humano | `RN-48`, ADR-001 | No se guarda |
| `estado_de_habilitacion` | `derivado` | Dv. | Proyección de `evento_habilitacion` + licencias vigentes + ausencias espejo | `RN-12` | Nunca se escribe |
| `BTT` | — | Ob. | Alta como recurso de flota | `RN-46` | Ver §0.3 |

**Nota de frontera.** El empleado pertenece a Talento Humano; **su rol como motorista dentro de la flota pertenece a SIGTI**. Historial de conducción, incidentes al volante y vehículos habilitados son datos propios que Talento Humano no conoce.

### `licencia_conducir` — dato propio

| Campo | Tipo lógico | Ob. | Dominio | Regla | Qué pasa si falta |
|---|---|---|---|---|---|
| `id_licencia` | `identificador` | Ob. | — | `RN-44` | No se guarda |
| `id_motorista` | `referencia` | Ob. | — | `RN-09` | No se guarda |
| `numero_de_licencia` | `texto_corto` | Ob. | — | `RN-09` | **Bloqueo duro de asignación.** Sin licencia capturada no se puede asignar el motorista |
| `fecha_de_vencimiento` | `fecha` | Ob. | Se valida contra **todo el rango de la misión**, no solo el día de salida | `RN-10` | Bloqueo duro |
| `escaneo_del_documento` | `adjunto` | Cd. | Exigido por NRM-06 `[C]` si la institución lo hace bloqueante | ADR-001 | Advertencia con acuse registrado |
| `vigencia_del_registro` | `rango_de_fechas` | Ob. | Las renovaciones abren registro nuevo; el anterior se conserva | `RN-04` | Bloqueo |
| `BTT` | — | Ob. | — | `RN-46` | Ver §0.3 |

### `categoria_en_licencia` — **un motorista tiene varias**

| Campo | Tipo lógico | Ob. | Dominio | Regla | Qué pasa si falta |
|---|---|---|---|---|---|
| `id_categoria_licencia` | `referencia` a catálogo | Ob. | `A` · `B` · `B1` · `C1` · `C` · `D1` · `D` · `CE` `[V]`, catálogo abierto | `RN-09` | No se guarda |
| `vigencia` | `rango_de_fechas` | Ob. | **Propia por categoría**: una puede suspenderse sin caer la licencia entera | `RN-10` | Bloqueo |
| `estado` | `enumerado_configurable` | Ob. | `VIGENTE` · `SUSPENDIDA` · `VENCIDA` · `CANCELADA` | `RN-10` | No se guarda |

**Invariante.** La habilitación se resuelve contra la **matriz licencia↔vehículo vigente a la fecha de inicio de la misión** y contra los atributos de la ficha técnica: tipo, peso bruto en kilogramos, capacidad de pasajeros y condición de articulado. **Nunca contra el nombre comercial del modelo** (`RN-09`). La habilitación se verifica **sobre quien efectivamente conduce, cualquiera sea su puesto** (`RN-57`). Si vence con la misión en ruta, **no se detiene la ejecución**: la misión cierra con hallazgo (`RN-55`).

**Entidad dependiente — `restriccion_medica`.** Cuelga de la licencia **y también del motorista** (hay dictámenes posteriores sin reemisión de licencia). Cada tipo declara en catálogo si es **incompatibilizante** para una condición de misión: si lo es, **bloquea**; si no, **advierte y registra que la advertencia fue vista y por quién** (`RN-11`).

> **Corrección — hallazgo `HB34-53`.** La restricción guarda **tipo, efecto y vigencia, nada más**. El diagnóstico, el dictamen y todo contenido clínico van a `segmento_dato_personal` con `categoria_de_dato = SALUD` y `base_legal_del_campo` **obligatoria** (§13, decisión `D-17`). Estaban en claro colgando del motorista, sin plazo de retención propio y fuera del alcance del `evento_depuracion`, siendo la única categoría que este diccionario marca como exigente de base legal. `CE-10` y la ampliación de `RN-51` registrada en el §16 de las reglas lo exigían desde antes.

---

## 11. `persona`, `puesto`, `asignacion_de_puesto`, `autoria_congelada`

**Módulo M-01.** Invariante `M-05`. `RNF-15`.

| Entidad | Campos núcleo | Notas |
|---|---|---|
| `persona` | `id_persona`, `identidad` (espejo, no editable), `nombre_completo`, `sincronizado_en` | Espejo de Talento Humano. **Una persona sin puesto vigente es un usuario sin permisos y no se borra**: sus actos históricos la referencian |
| `puesto` | `id_puesto`, `denominacion`, `id_unidad_organizativa`, `id_puesto_superior`, `id_delegacion` | **Existe aunque esté vacante.** Los actos pendientes de decisión quedan atribuidos al puesto y escalan al superior si la vacancia supera el plazo parametrizable |
| `asignacion_de_puesto` | `id_asignacion`, `id_persona`, `id_puesto`, `vigencia`, `tipo` (`TITULAR` · `INTERINO` · `POR_DELEGACION`) | Solape entre saliente y entrante permitido; máximo en días `[C]` |
| `puesto_rol` | `id_puesto`, `id_rol` (`ACT-xx`), `vigencia` | **Los permisos son del puesto, siempre.** No hay permisos otorgados a personas |
| `alcance_de_datos` | `id_puesto_rol`, `tipo_de_objeto`, `nivel` (`PROPIO` · `DEPENDENCIA` · `DELEGACION` · `INSTITUCION`) | Se acota **por tipo de objeto**: un puesto puede tener `DEPENDENCIA` sobre misiones e `INSTITUCION` sobre vehículos |

### `autoria_congelada` — la entidad que sostiene `RNF-15`

| Campo | Tipo lógico | Ob. | Dominio | Regla | Qué pasa si falta |
|---|---|---|---|---|---|
| `id_autoria` | `identificador` | Ob. | — | — | No se guarda |
| `id_persona` | `referencia` | Ob. | **Identificador de persona, no de cuenta de usuario** | máq. estados §9.1 | No se guarda |
| `id_puesto` | `referencia` | Ob. | El ocupado al momento del hecho | `RNF-15` | No se guarda |
| `denominacion_puesto` | `valor_copiado` | Ob. | **Copia literal.** No cambia si el puesto se renombra o se suprime | `RNF-15` | **La respuesta a "¿con qué competencia?" deja de ser cierta tras la primera reorganización** |
| `rol_ejercido` | `valor_copiado` | Ob. | `ACT-xx` **copiado, no referenciado** | máq. estados §9.1 | Ídem |
| `id_unidad_organizativa` | `valor_copiado` | Ob. | Desde dónde actuó | máq. estados §9.1 | No se guarda |
| `id_delegacion` | `valor_copiado` | Cd. | Si actuó desde delegación | actores §3 | Nulo válido en sede |
| `alcance_aplicado` | `valor_copiado` | Ob. | El nivel con que se resolvió el acceso | `RNF-14` | No se puede auditar si vio lo que podía ver |
| `id_delegacion_de_autoridad` | `referencia` | Cd. | Obligatorio si actuó por delegación de autoridad | `RN-07` | El acto queda sin competencia acreditada |

**Por qué `valor_copiado` y no referencia.** Porque el auditor pregunta *"¿quién autorizó esto y con qué competencia?"*, y el puesto pudo haber cambiado de titular tres veces y de nombre dos. Una referencia respondería con el presente; el asiento tiene que responder con el pasado.

---

## 12. `tabla_parametrica`, `version_tabla_parametrica`, `entrada_parametrica`, `valor_congelado`

**Módulo M-02.** Invariante `M-01`. `RN-39` a `RN-42`, `RNF-05`. **La respuesta completa a "si el reglamento cambia mañana, ¿qué se rompe?".**

### `tabla_parametrica`

| Campo | Tipo lógico | Ob. | Dominio | Regla | Qué pasa si falta |
|---|---|---|---|---|---|
| `id_tabla` | `identificador` | Ob. | — | — | No se guarda |
| `clave` | `texto_corto` | Ob. | `TARIFAS_PEAJE` · `MATRIZ_LICENCIA_VEHICULO` · `CALENDARIO_INHABIL` · `HORARIO_HABIL` · `UMBRALES_DESVIACION` · `COMPATIBILIDAD_VEHICULO_OBJETO` · `COMPATIBILIDAD_OBJETO_OBJETO` · `CATEGORIAS_PEAJE` · `PLAZOS` · … | `RN-39` | No se guarda |
| `dimensiones` | `texto` | Ob. | Qué columnas forman la llave de resolución | `RN-34` | Sin ellas no se sabe cómo resolver una consulta |
| `bloquea_si_no_hay_vigente` | `booleano` | Ob. | Para tarifas, **verdadero**: si no hay tarifa vigente, no se calcula un valor por defecto, **se bloquea la estimación** | `RN-34` | Se calcularía un valor inventado |

### `version_tabla_parametrica`

| Campo | Tipo lógico | Ob. | Dominio | Regla | Qué pasa si falta |
|---|---|---|---|---|---|
| `id_version` | `identificador` | Ob. | **Es el identificador que congela `valor_congelado`** | `RN-41` | No se guarda |
| `id_tabla` | `referencia` | Ob. | — | `RN-39` | No se guarda |
| `vigencia` | `rango_de_fechas` | Ob. | **Tiempo del hecho.** Sin solape ni hueco con las demás versiones **conocidas a la vez** de la misma tabla. Se impide al cargar, no se detecta después | `RNF-05` | No se guarda |
| `conocido_desde` | `marca_de_tiempo` | Ob. | **Tiempo del sistema.** Momento en que esta versión pasó a ser conocida por el sistema. **No editable jamás** | `RNF-05`, `RNF-06` | **No se guarda.** Es el dato que se pierde si no se captura en el acto de cargar, y sin él ningún reporte histórico es reproducible |
| `conocido_hasta` | `marca_de_tiempo` | Cd. | Nulo = versión conocida vigente. Se llena **solo** cuando otra versión la corrige: nunca se borra ni se edita la corregida | `RNF-05`, `RN-42` | Nulo es el estado normal |
| `id_version_corregida` | `referencia` a `version_tabla_parametrica` | Cd. | Obligatorio si esta versión corrige a otra con la **misma `vigencia`** | `RN-42` | No se puede explicar por qué dos versiones comparten vigencia |
| `motivo_de_correccion` | `enumerado_configurable` + `texto` | Cd. | Obligatorio junto con `id_version_corregida` | `RN-42` | Bloqueo. Una corrección retroactiva sin motivo es indistinguible de una alteración |
| `id_acto_de_aprobacion` | `referencia` | Ob. | Acuerdo, resolución o acta institucional | `RN-39` | Bloqueo |
| `fuente` | `texto` | Ob. | De dónde salió el dato | `RN-34` | Bloqueo |
| `fecha_de_verificacion` | `fecha` | Ob. | El sistema alerta a los 12 meses sin revisar | `RN-34` | Bloqueo |
| `nivel_de_verificacion` | `enumerado_configurable` | Ob. | `[V]` · `[P]` · `[C]` · `[I]` | CLAUDE.md | Bloqueo. **El nivel nunca sube al bajar de abstracción** |
| `id_autoria_cargó` | `referencia` | Ob. | Quien la cargó | actores §4.3 | Bloqueo |
| `id_autoria_aprobó` | `referencia` | Ob. | **Doble control: distinta de quien cargó** | actores §4.3, `RN-01` | Bloqueo duro |

> **Corrección — hallazgo `HB34-50`.** La entidad tenía **un solo eje** (`vigencia`), y `RNF-05` pide dos. Los cinco campos de arriba, en conjunto, son la bitemporalidad (decisión `D-13`).
>
> **Regla de resolución.** Toda consulta paramétrica toma **dos fechas**: `fecha_del_hecho` (`RN-40`) y `fecha_de_corte_de_conocimiento`. Se selecciona la versión cuya `vigencia` contiene la primera **y** cuyo intervalo `conocido_desde` / `conocido_hasta` contiene la segunda. El corte por defecto es *ahora*; un reporte reproducible usa el suyo (`RN-94`, `RNF-06`).
>
> **Regla de corrección retroactiva.** Corregir **no es editar**: se cierra `conocido_hasta` de la versión errónea y se inserta una nueva con la **misma `vigencia`**, `conocido_desde` = ahora, `id_version_corregida` y `motivo_de_correccion`. Las dos coexisten para siempre. La diferencia económica se materializa como `asiento_de_diferencia` imputado al período corriente (`RN-42`), no como reescritura.
>
> **Qué pasa si falta.** El caso que lo obligó: tarifa de Zambrano mal cargada para enero–marzo de 2026, corregida en marzo de 2027 con vigencia retroactiva. El reporte de conciliación del primer trimestre de 2026 **con fecha de corte 30 de abril de 2026**, regenerado en 2028 para el TSC, encontraba la versión corregida —que en abril de 2026 no existía— y **cambiaba de hash**. `RNF-06` exige diferencia **0** entre dos generaciones del mismo reporte con los mismos parámetros y la misma fecha de corte.

### `entrada_parametrica`

Filas de la versión. Estructura por dimensiones de la tabla. Ejemplos:

| Tabla | Dimensiones de la entrada | Valor |
|---|---|---|
| `TARIFAS_PEAJE` | punto de peaje × categoría de peaje | monto |
| `MATRIZ_LICENCIA_VEHICULO` | categoría de licencia × tipo de vehículo × rango de peso bruto × rango de pasajeros × articulado | habilita sí/no |
| `COMPATIBILIDAD_OBJETO_OBJETO` | tipo de objeto A × tipo de objeto B | `COMPATIBLE` · `COMPATIBLE_CON_CONDICIONES` + texto de condiciones · `INCOMPATIBLE`. **La ausencia de entrada bloquea** (`RN-67`) |
| `CALENDARIO_INHABIL` | fecha × ámbito territorial | inhábil sí/no + tipo |

### `valor_congelado` — decisión `D-08`

| Campo | Tipo lógico | Ob. | Dominio | Regla | Qué pasa si falta |
|---|---|---|---|---|---|
| `id_valor_congelado` | `identificador` | Ob. | — | — | No se guarda |
| `concepto` | `enumerado_configurable` | Ob. | `ESTIMACION_PEAJE` · `HABILITACION_LICENCIA` · `RENDIMIENTO_ESPERADO` · `UMBRAL_APLICADO` · `TARIFA_POR_PUNTO` · `CALENDARIO_APLICADO` · `HOLGURA_APLICADA` · `PLAZO_APLICADO` · … | `RN-41` | No se guarda |
| `caracter` | `enumerado_configurable` | Ob. | `INDICATIVO` (congelado al **someterse a autorización**, `T-02` — `INV-07`; `T-05` lo **ratifica**, no lo recongela · `HB1-17`) · `VINCULANTE` (congelado al despachar, `T-12` — `EF-03`) · `RECONGELADO_POR_SUSTITUCION` (`T-10` o en ruta — `RN-61`) | `EF-03`, `RN-35`, `RN-61` | **Bloqueo.** Sin él no se sabe cuál de los dos paquetes lleva impreso el papel del motorista, y uno pisaría al otro |
| `id_portador` | `referencia` | Ob. | **Polimórfico** con `tipo_de_portador` ∈ `VERSION_ALCANCE_AUTORIZADO` (indicativo) · `ORDEN_MISION` (vinculante) · `TRAMO_MISION` (recongelado) | `EF-03`, `RN-61` | No se guarda |
| `id_valor_congelado_que_reemplaza` | `referencia` | Cd. | Obligatorio en `RECONGELADO_POR_SUSTITUCION`. **El reemplazado se conserva** | `RN-61`, `RN-04` | No se puede producir el asiento de diferencia de la sustitución |
| `valor` | `magnitud` o `valor_copiado` | Ob. | El resultado | `RN-41` | No se guarda |
| `id_version_tabla_parametrica` | `referencia` | Ob. | **La versión que lo produjo**, identificada en sus dos ejes | `RN-41`, `RNF-05` | **Bloqueo.** Sin ella una consulta futura recalcularía con los parámetros actuales y mostraría un valor que nunca se autorizó |
| `fecha_de_corte_de_conocimiento_usada` | `marca_de_tiempo` | Ob. | Con la que se resolvió el **eje de tiempo del sistema** | `RNF-05`, `RNF-06` | No se puede reproducir la resolución si después se corrigió la tabla hacia atrás |
| `valores_unitarios` | `valor_copiado` | Ob. | Tarifa por punto, número de cruces, umbral aplicado — los componentes | `RN-41` | El total no se puede explicar ni defender |
| `fecha_del_hecho_usada` | `marca_de_tiempo` | Ob. | Con la que se resolvió la vigencia | `RN-40` | Bloqueo |
| `base_de_derivacion` | `enumerado_configurable` | Cd. | `POR_VEHICULO` · `POR_TIPO_DE_VEHICULO_ESTIMATIVA`. Obligatorio en estimaciones de peaje | `RN-33` | **Un estimado que no dice sobre qué base se calculó no se puede defender ante quien lo autorizó** |
| `id_acto_que_congela` | `referencia` | Ob. | Transición o autorización que lo fijó | `RN-41` | No se guarda |

**Invariante.** Una corrección posterior de la tabla **nunca reescribe** un valor congelado: genera `asiento_de_diferencia`, imputado al período corriente con referencia al período afectado (`RN-42`, `RN-93`).

> **Corrección — hallazgo `HB34-59`.** Tres fuentes declaraban tres momentos de congelamiento distintos: `CLAUDE.md` y `RN-41` decían «al autorizar», `EF-03` —**autoridad en efectos de transición**— dice «al pasar a `DESPACHADA`», y el modelo colgaba `valor_congelado` obligatoriamente de `version_alcance_autorizado`, o sea al autorizar. Además, el recongelamiento por sustitución de vehículo de `RN-61` **no tenía dónde alojarse**, porque no cambia el alcance autorizado.
>
> La decisión, en `D-08` y `D-18`: **se congela dos veces con `caracter` distinto y ninguno pisa al otro.** Al autorizar, la estimación **indicativa** que `RN-35` necesita para decidir. Al despachar, el paquete **vinculante** de `EF-03` — y es ese el que lleva impreso la orden que el motorista discute en la caseta (`RN-91`). El caso que lo obligó: aprobada el 5 de enero, programada el 28, despachada el 3 de febrero, con la tarifa cambiando el 20 de enero y `NRM-10` documentando tres reversiones tarifarias en dos meses. Con el paquete de enero, el papel del motorista está mal.
>
> La obligatoriedad de colgar de `version_alcance_autorizado` queda **rota**: el portador es polimórfico.
>
> **Ajuste posterior — hallazgo `HB1-17`, 2026-08-26.** La decisión de los **dos congelamientos con carácter distinto se mantiene íntegra**. Lo que se corrige es *cuándo* ocurre el indicativo: no en `T-05` sino en **`T-02`**, porque [`INV-07`](../estados/orden-de-mision.md) lo exige ya en el estado `SOLICITADA`, que es anterior a toda autorización — y la máquina de estados es autoridad en invariantes. `T-05` **ratifica** el valor congelado, registrando cuál aprobó; no lo vuelve a congelar. El portador polimórfico que esta misma decisión abrió es lo que lo hace posible: el indicativo cuelga de `orden_mision`, no de una `version_alcance_autorizado` que todavía no existe al enviar.

---

## 13. `asiento_auditoria` y `segmento_dato_personal`

**Módulo M-14.** Invariante `M-02`. La resolución estructural de `RNF-04` × `RNF-17`.

### `asiento_auditoria`

| Campo | Tipo lógico | Ob. | Dominio | Regla | Qué pasa si falta |
|---|---|---|---|---|---|
| `id_asiento` | `identificador` | Ob. | Generado en el origen | `RN-44` | No se guarda |
| `ambito_de_cadena` | `enumerado_configurable` | Ob. | `GLOBAL_DE_INSTANCIA`. Todo asiento pertenece a la cadena global, sin excepción por módulo ni por misión | `RNF-04` | No se guarda |
| `id_asiento_anterior` | `referencia` | **Dv.** | **Lo asigna el servidor al integrar.** Nulo **solo en el primer asiento de la instancia** | `RNF-04` | Se asigna. Si el asiento aún no se ha integrado, es nulo y el asiento está `PENDIENTE_DE_EMPALME` |
| `id_asiento_anterior_de_dispositivo` | `referencia` | Cd. | **Lo pone el dispositivo al escribir.** Es la subcadena que sostiene lo nacido sin red. Nulo en el primero de cada tramo de subcadena | `RNF-04`, `RN-43` | La subcadena del dispositivo no cierra y su lote entra en conflicto |
| `orden_de_encadenamiento` | `derivado` | Dv. | `recibido_en`, con desempate determinista por (`id_dispositivo`, `secuencia_dispositivo`, `id_asiento`) | `RNF-04` | Se calcula. **Es distinto del criterio de aplicación de transiciones**, que es `secuencia_dispositivo` (máq. estados §6.2) |
| `entidad_afectada` | `valor_copiado` | Ob. | Tipo e identificador del objeto | `RNF-04` | No se guarda |
| `operacion` | `enumerado_configurable` | Ob. | `ALTA` · `MODIFICACION` · `TRANSICION` · `CONSULTA` · `EMISION` · `ANULACION` · `REVERSO` · `DEPURACION` · `FUSION` | `RN-04` | No se guarda |
| `id_autoria_congelada` | `referencia` | Ob. | Ver §11 | `RNF-15`, `RNF-04` | **No se guarda.** Es uno de los seis campos obligatorios del `RNF-04` |
| `valor_anterior` | `valor_copiado` | Cd. | **De los campos NO personales.** Obligatorio en `MODIFICACION` y `REVERSO`, incluso si es nulo | `RNF-04`, `RN-04` | No se guarda |
| `valor_nuevo` | `valor_copiado` | Cd. | Ídem | `RNF-04` | No se guarda |
| `referencia_segmento_personal` | `referencia` | Cd. | Obligatorio si la operación tocó dato personal | `RNF-17` | El dato personal quedaría en claro dentro del asiento y la depuración rompería la cadena |
| `huella_segmento_personal` | `huella` | Cd. | Calculada **con la sal que vive dentro del segmento** | `RNF-17`, decisión `D-04` | Sin sal la huella sería reversible por fuerza bruta sobre un dominio enumerable |
| `id_version_tabla_parametrica_usada` | `referencia` | Cd. | Si la operación usó tabla paramétrica | `RN-41` | La operación no es reproducible |
| `huella_contenido` | `huella` | Ob. | Sobre representación canónica del asiento | `RNF-04` | No se guarda |
| `huella_anterior` | `huella` | **Dv.** | Del asiento anterior **de la cadena global**. La asigna el servidor en el empalme, no el origen | `RNF-04` | La cadena global no cierra |
| `huella_anterior_de_dispositivo` | `huella` | Cd. | Del asiento anterior de la **subcadena del dispositivo**. La pone el dispositivo | `RNF-04` | La subcadena no cierra |
| `id_sello_de_cadena` | `referencia` | Dv. | Asignado al sellar el período | `RNF-04` | Se asigna |
| `BTT` | — | Ob. | — | `RN-46` | Ver §0.3 |

> **Corrección — hallazgo `HB34-51`.** Faltaban las tres cosas que deciden si la cadena sirve, y la única que estaba declarada era la equivocada.
>
> 1. **Alcance.** La cadena es **global por instancia** — decisión `D-14`, §1 `M-02` del [`README`](README.md). El «encadenamiento por misión» del §12 queda **derogado**: con miles de cadenitas de veinte asientos, **borrar íntegra una misión no rompe ninguna cadena**, y la batería de `RNF-04` —detectar la eliminación de un asiento intermedio en 4,000,000, con identificación del punto exacto de ruptura— presupone una cadena, no un archipiélago.
> 2. **Los asientos que no son de ninguna misión.** `ALTA` de un vehículo, carga de una `version_tabla_parametrica`, `CONSULTA` a un manifiesto (`RN-52`), `DEPURACION` y `FUSION` **entran en la misma cadena global**. Con encadenamiento por misión todos habrían sido «primeros de cadena», que era justamente lo que el diccionario declaraba imposible.
> 3. **Quién encadena y en qué orden.** `huella_anterior` **la fija el servidor al integrar** y por eso es `Dv.`, no `Cd.`. El dispositivo desconectado encadena en su **subcadena** y el servidor la empalma **sin reordenar ni reescribir lo ya sellado** (`RNF-04`). El orden de encadenamiento es `recibido_en`; el de aplicación de transiciones es `secuencia_dispositivo`. Son criterios distintos y confundirlos produce una cadena que se reordena — y una cadena que se reordena no prueba nada.

**Prohibiciones estructurales.** Ninguna funcionalidad del sistema modifica ni borra un asiento. **Ni el Administrador del Sistema (`ACT-01`)**, y su ausencia de esa capacidad debe ser demostrable (`RNF-04`, máq. estados §9.3). La propiedad que se promete es **detectabilidad con anclaje externo**, no inmutabilidad absoluta: prometer lo segundo ante el TSC costaría más que declarar la limitación.

### `segmento_dato_personal`

| Campo | Tipo lógico | Ob. | Dominio | Regla | Qué pasa si falta |
|---|---|---|---|---|---|
| `id_segmento` | `identificador` | Ob. | Lo que el asiento referencia | `RNF-17` | No se guarda |
| `tipo_de_objeto_portador` | `enumerado_configurable` | Ob. | `LINEA_MANIFIESTO` · `INVOLUCRADO_EN_INCIDENTE` · `RESTRICCION_MEDICA` · `FIRMANTE_ACTA` · `ADJUNTO` · los que la institución agregue | `RN-51`, `RNF-17` | **Bloqueo.** Sin portador declarado, la depuración no sabe qué alcanza |
| `id_objeto_portador` | `referencia` | Ob. | El objeto concreto. **Polimórfico** con el campo anterior | `RN-51` | Ídem |
| `contenido` | `valor_copiado` | Cd. | Los campos del **catálogo autorizado** y ninguno más | `RN-51` | Nulo tras la depuración: es el estado esperado |
| `sal_de_huella` | `valor_copiado` | Ob. | **Vive con el dato y se elimina con él** | decisión `D-04` | Sin ella la huella es invertible y la depuración sería cosmética |
| `categoria_de_dato` | `enumerado_configurable` | Ob. | `IDENTIFICACION` · `CONTACTO` · `DOCUMENTO` · `SALUD` (solo con base legal expresa) | `RN-51` | No se guarda |
| `base_legal_del_campo` | `texto` + `adjunto` | Cd. | **Obligatorio** para salud, etnia, situación migratoria o condición de vulnerabilidad | `RN-51`, `RNF-17` | **El campo no se ofrece.** El umbral de `RNF-17` es cero |
| `vigencia_de_retencion` | `rango_de_fechas` | Ob. | Plazo **menor** que el de los registros contables. `[C]` insumo #71 | `RNF-17` | **Sin plazo configurado el sistema no depura nada** y lo declara en la pantalla de estado. No se inventa un plazo por defecto |
| `id_evento_depuracion` | `referencia` | Cd. | No nulo = ya depurado | `RNF-17` | — |

**Qué sobrevive a la depuración.** Conteo de pasajeros, condición agregada, origen, destino, vehículo, misión y costos. **No** identidad, contacto ni documento. El reporte regenerado con la misma fecha de corte debe dar **conteos, costos y estructura idénticos** (`RNF-06`); solo las identidades aparecen seudonimizadas.

> **Corrección — hallazgo `HB34-53`, segunda parte.** El dato personal separable existía **solo** para `linea_manifiesto`, mientras el `§16` de las reglas registra que `RN-51` fue ampliada *«a terceros de siniestro y al dato de salud del servidor»* (`CE-03`, `CE-10`). Quedaban en claro y sin plazo de retención propio:
>
> - **El tercero lesionado** de un accidente, que no es empleado y no está en el espejo de Talento Humano. El modelo lo resolvía con `EXPEDIENTE_INCIDENTE }o--o| PERSONA`, que obliga a crear un registro de persona para alguien que la institución no administra. Ahora es `involucrado_en_incidente`, que apunta a `persona` **solo cuando lo es** y en los demás casos aparta su identidad aquí.
> - **La `restriccion_medica` del servidor** — dato de salud, la única categoría que este mismo diccionario marca como exigente de `base_legal_del_campo`, y que estaba en claro colgando del motorista.
> - **El `firmante_acta`** que suscribe sin ser de la institución.
> - **El `adjunto`** con contenido personal — ver §19.
>
> Los cuatro usan la **misma** indirección, el mismo plazo de retención y el mismo `evento_depuracion`. Una lógica de depuración por portador se implementa mal en tres de cada cuatro.

---

## 14. `folio`, `rango_de_folio` y `subrango_de_folio`

**Módulo M-15.** Invariante `M-03`. `RN-44`, `RNF-21`. **Distinto del identificador interno.**

### `rango_de_folio`

| Campo | Tipo lógico | Ob. | Dominio | Regla | Qué pasa si falta |
|---|---|---|---|---|---|
| `id_rango` | `identificador` | Ob. | — | — | No se guarda |
| `id_delegacion` | `referencia` | Ob. | **Los rangos no se solapan entre delegaciones** | `RN-44` | Colisión de folios entre delegaciones desconectadas |
| `id_tipo_documento` | `referencia` | Ob. | Orden de misión, salvoconducto, vale, acta | `RNF-21` | No se guarda |
| `desde`, `hasta` | `texto_corto` | Ob. | Correlativo legible | `RNF-21` | No se guarda |
| `umbral_de_alerta` | `entero` | Ob. | Aviso al quedar bajo el umbral. `[C]` insumo #34, referencia 20 % | `RNF-21` | El rango se agota sin aviso, y la reposición **sí requiere red** |
| `saldo_disponible` | `derivado` | Dv. | Descontando lo repartido en subrangos y lo consumido directamente | `RNF-21` | Se calcula |

### `subrango_de_folio` — el nivel que faltaba

> **Corrección — hallazgo `HB34-52`.** El rango se asignaba **solo a la delegación**, aunque el §1 `M-03` y la nota de la Vista 1 decían que el portador es el dispositivo. Ni el diagrama ni este diccionario lo modelaban. El caso concreto no es hipotético, lo describe el propio inventario de pantallas: en una delegación, `PT-121` emite con folio desde la tableta de la caseta, `PT-037` y `PT-123` desde el equipo del encargado y `PT-114` desde el teléfono del motorista — **cuatro dispositivos, una delegación, un solo rango**. Todos sin red toman el mismo número siguiente, y `RNF-21` exige **0 folios duplicados a nivel institución**.

| Campo | Tipo lógico | Ob. | Dominio | Regla | Qué pasa si falta |
|---|---|---|---|---|---|
| `id_subrango` | `identificador` | Ob. | — | `RN-44` | No se guarda |
| `id_rango` | `referencia` | Ob. | Del rango de su delegación y su tipo de documento | `RNF-21` | No se guarda |
| `id_dispositivo` | `referencia` | Ob. | **El portador.** Un subrango pertenece a un solo dispositivo | `RNF-21`, máq. estados §6.2 | **Bloqueo.** Un subrango sin dispositivo es el rango de delegación otra vez, y la colisión vuelve |
| `desde`, `hasta` | `texto_corto` | Ob. | **Disjuntos de todo otro subrango del mismo rango.** Se verifica al asignar, no después | `RNF-21` | Bloqueo. El solape entre subrangos es la colisión que esto viene a impedir |
| `umbral_de_alerta` | `entero` | Ob. | Propio del subrango, no heredado del rango. `[C]` — ver `README` §15, punto 14 | `RNF-21` | El dispositivo se queda sin folios en campo y **la reposición sí requiere red** |
| `saldo_disponible` | `derivado` | Dv. | — | `RNF-21` | Se calcula |
| `estado` | `derivado` de `evento_estado_subrango` | Dv. | `ASIGNADO` · `EN_USO` · `AGOTADO` · `DEVUELTO` · `ANULADO_POR_CIERRE` | `RNF-21`, `RN-96` | — |
| `id_acta_de_devolucion` | `referencia` a `acta` | Cd. | Obligatorio al devolver el remanente cuando el dispositivo se reincorpora o se da de baja | `RNF-21`, `RN-96` | **Hueco sin explicar** en la numeración |
| `BTT` | — | Ob. | — | `RN-46` | Ver §0.3 |

**Devolución y huecos.** Lo no consumido de un subrango **vuelve al rango de la delegación con acta** y queda registrado como hueco explicado, nunca como salto silencioso. Al cierre de ejercicio, el remanente se anula con acta y no se arrastra (`RN-96`).

### `folio`

| Campo | Tipo lógico | Ob. | Dominio | Regla | Qué pasa si falta |
|---|---|---|---|---|---|
| `id_folio` | `identificador` | Ob. | Interno, opaco | `RN-44` | No se guarda |
| `numero` | `texto_corto` | Ob. | **Único en la institución. Nunca se recicla**, ni el de un anulado | `RNF-21` | Un folio duplicado es hallazgo directo de auditoría; uno reciclado es indistinguible de una alteración deliberada |
| `id_rango` | `referencia` | Ob. | — | `RN-44` | No se guarda |
| `id_subrango` | `referencia` | Cd. | Del que se consumió. **Nulo solo en la emisión desde sede conectada**, que consume del rango y lo declara | `RNF-21` | No se puede atribuir el folio al dispositivo que lo emitió, ni auditar una colisión |
| `estado` | `derivado` de `evento_estado_folio` | Dv. | `RESERVADO` · `EMITIDO` · `ANULADO` · `EXTRAVIADO` | `RNF-21` | — |
| `id_documento_emitido` | `referencia` | Cd. | 0..1. **Nunca dos documentos por folio** | `RNF-21` | — |
| `motivo_de_anulacion` | `enumerado_configurable` + `texto` | Cd. | Obligatorio al anular | `RN-04`, `RNF-21` | **Hueco sin explicar** — exactamente lo que el auditor busca |
| `numero_talonario_preimpreso` | `texto_corto` | Op. | Si el talonario físico trae folio propio. `[C]` insumo #46 | `RNF-21` | Nulo válido |
| `BTT` | — | Ob. | — | `RN-46` | Ver §0.3 |

**Invariante de cierre de ejercicio.** Todo folio reservado y no consumido al corte **se anula con acta**; ni el folio ni el compromiso se arrastran al ejercicio siguiente (`RN-96`).

---

## 15. `asignacion_combustible`, `consumo_combustible` y `compromiso_proyectado`

**Módulo M-09.** El vale. Máquina de estados §10.1.

### `asignacion_combustible`

| Campo | Tipo lógico | Ob. | Dominio | Regla | Qué pasa si falta |
|---|---|---|---|---|---|
| `id_asignacion` | `identificador` | Ob. | — | `RN-44` | No se guarda |
| `id_folio` | `referencia` | Ob. | **Folio único, no reciclable** | `RN-27` | Bloqueo |
| `id_fondo_combustible` | `referencia` | Ob. | Del que se descuenta. **Sin fondo vigente no hay asignación** | `RN-26` | Bloqueo duro |
| `id_orden_mision` | `referencia` | Cd. | Obligatorio en esquema por misión; sustituible por motorista + período en esquema por período `[C]` insumo #7 | `RN-27`, `RN-32` | **No se entrega combustible sin Orden de Misión aprobada**, y solo al vehículo y motorista de esa orden |
| `id_persona_receptora` | `referencia` | Ob. | Motorista o encargado de delegación | `RN-27` | Bloqueo |
| `monto`, `galones` | `magnitud` | Cd. | Al menos uno | `RN-27` | Bloqueo |
| `instrumento` | `enumerado_configurable` | Ob. | `VALE` · `CUPON` · `EFECTIVO` · `ORDEN_DE_PAGO` · `TARJETA_DE_FLOTA` (previsto, no cerrado) | `RN-27` | No se guarda |
| `constancia_de_recepcion` | `adjunto` o firma registrada | Cd. | Obligatoria para pasar a `ENTREGADA` | `RN-27` | Queda **emitida no entregada**: no consumible ni liquidable |
| `estado` | `derivado` de `evento_estado_asignacion` | Dv. | `EMITIDA` · `ENTREGADA` · `CONSUMIDA` · `DEVUELTA` · `EXTRAVIADA` · `LIQUIDADA` · `CONCILIADA` · `CONCILIADA_CON_DESVIACION` · `ANULADA` | máq. estados §10.1 | — |
| `BTT` | — | Ob. | — | `RN-46` | Ver §0.3 |

**Segregación (`RN-01`, `BD-06`).** Emite `ACT-04` ≠ entrega `ACT-07` ≠ consume `ACT-06` ≠ liquida ≠ concilia. Es **bloqueo duro**, no advertencia. Un vale devuelto **sin nominar** vuelve al inventario; **un vale nominado a una misión se anula, no se reutiliza**.

### `consumo_combustible`

| Campo | Tipo lógico | Ob. | Dominio | Regla | Qué pasa si falta |
|---|---|---|---|---|---|
| `id_consumo` | `identificador` | Ob. | Generado en el cliente, **sin conectividad** | `RN-43`, `RN-44` | No se guarda |
| `id_asignacion` | `referencia` | Ob. | Folio del que se descuenta | `RN-28` | **No se guarda** |
| `galones` | `magnitud` | Ob. | — | `RN-28` | No se guarda |
| `monto` | `monto` | Ob. | — | `RN-28` | No se guarda |
| `id_estacion_servicio` | `referencia` | Ob. | Con su ubicación | `RN-28` | No se guarda |
| `id_lectura_odometro` | `referencia` | Ob. | Al momento de la carga | `RN-28`, `RN-30` | No se guarda: sin ella la conciliación galonaje–kilometraje no existe |
| `id_comprobante` | `referencia` | **Op.** | **Exigible pero no bloqueante** | `RN-28`, `RN-85` | Se registra como observación **que se arrastra a la liquidación**, con causa tipificada y suficiencia probatoria; admite descargo alternativo con folio |
| `fotografia_comprobante` | `adjunto` | Op. | Ídem | `RN-28` | Ídem |
| `id_tramo_mision` | `referencia` | Ob. | **La imputación es por tramo, no por misión** | `RN-72` | El rendimiento se calcularía sobre la misión completa y sería inservible |
| `BTT` | — | Ob. | — | `RN-46` | Ver §0.3 |

**`comprobante` — unicidad institucional.** Terna `tipo + emisor + numero`, **única en la institución**, verificada **al registrar** y **atravesando el alcance de datos** (`RN-84`). Dos delegaciones sin red que invocan el mismo comprobante producen **conflicto a cola humana**, nunca aceptación ni descarte silenciosos. Es la fuga de combustible clásica y detectarla al conciliar, ocho meses después, ya no sirve.

**`abastecimiento` — entidad separada del consumo (`RN-83`).** Todo ingreso de combustible al tanque se registra como abastecimiento con **fuente de financiamiento declarada**: `FONDO_DE_LA_MISION` · `TANQUE_INSTITUCIONAL` · `OTRA_DEPENDENCIA` · `DONACION` · `PECULIO_DEL_SERVIDOR` · `TERCERO_EN_APOYO`. Las fuentes distintas del fondo **entran en el denominador de la conciliación** y **no en el cuadre del fondo**. El nivel de tanque a la salida y al retorno es **dato obligatorio de bitácora**, en la escala que el instrumento permita.

### `compromiso_proyectado` — el saldo proyectado de `RN-88`

> **Corrección — hallazgo `HB34-61`.** `fondo_combustible` tenía aprobado, asignado y saldo contable. `RN-88` exige que el saldo **se presente siempre acompañado del comprometido proyectado** y que **la alerta de agotamiento se dispare sobre el proyectado, no sobre el contable** — que es donde se ve venir el problema con dos semanas de anticipación. Sin entidad, la alerta no tenía sobre qué dispararse y el fondo se descubría agotado el día en que una delegación venía por su vale.

| Campo | Tipo lógico | Ob. | Dominio | Regla | Qué pasa si falta |
|---|---|---|---|---|---|
| `id_compromiso` | `identificador` | Ob. | — | `RN-44` | No se guarda |
| `id_fondo_combustible` | `referencia` | Ob. | El fondo cuyo saldo compromete | `RN-88`, `RN-26` | No se guarda |
| `id_orden_mision` | `referencia` | Ob. | Misión **aprobada o programada sin asignación emitida** | `RN-88` | No se guarda: el proyectado sería un agregado sin cartera consultable |
| `id_valor_congelado` | `referencia` | Ob. | El estimado congelado que lo cuantifica (`RN-35`, `RN-41`) | `RN-88` | El número no se puede explicar ni defender |
| `estado` | `enumerado_configurable` | Ob. | `VIGENTE` · `CONSUMIDO_POR_ASIGNACION` · `LIBERADO_POR_ANULACION` · `LIBERADO_POR_CADUCIDAD` | `RN-88` | El comprometido no se libera y el proyectado queda inflado para siempre |
| `BTT` | — | Ob. | — | `RN-46` | Ver §0.3 |

**Derivados del fondo.** `saldo_contable` = aprobado − asignado + devoluciones (`RN-26`). `saldo_proyectado` = saldo contable − suma de compromisos `VIGENTE`. **Las cuatro cifras se muestran juntas** y la cartera que compone el proyectado es consultable (`RN-88`). El compromiso se valida además contra la **cuota trimestral** (`RN-54`), no solo contra el saldo. Y nada se resuelve apagando el control: `tolerancia_sobregiro` es parámetro con vigencia bajo `RN-39`, no un interruptor de una persona.

---

## 16. `paso_por_caseta`

**Módulo M-18.** `RN-34` a `RN-38`, `RN-91`, `RN-92`.

| Campo | Tipo lógico | Ob. | Dominio | Regla | Qué pasa si falta |
|---|---|---|---|---|---|
| `id_paso` | `identificador` | Ob. | Capturado sin conectividad | `RN-43` | No se guarda |
| `id_tramo_mision` | `referencia` | Ob. | Imputación por tramo | `RN-72` | No se guarda |
| `id_punto_peaje` | `referencia` | Ob. | Del catálogo de puntos con su operador | `RN-34` | No se guarda |
| `categoria_cobrada` | `referencia` a `categoria_peaje` | Ob. | La que efectivamente aplicó la caseta | `RN-36` | No se puede detectar discrepancia |
| `categoria_asignada_al_vehiculo` | `valor_copiado` | Ob. | Del `valor_congelado` de la orden | `RN-33`, `RN-91` | Ídem |
| `monto_pagado` | `monto` | Ob. | — | `RN-34` | No se guarda |
| `medio_de_pago` | `enumerado_configurable` | Ob. | `EFECTIVO` · `COVIPASS` · `EXONERADO` | `RN-38` | No se guarda |
| `id_comprobante` | `referencia` | Op. | Ticket. Sostiene el reclamo | `RN-36`, `RN-85` | Observación arrastrada a la liquidación |
| `id_exoneracion_aplicada` | `referencia` | Cd. | Obligatorio si `medio_de_pago = EXONERADO` | `RN-38` | **El valor por defecto es "paga".** El sistema no asume exoneración por pertenecer al Estado |
| `BTT` | — | Ob. | — | `RN-46` | Ver §0.3 |

**Derivadas.** `discrepancia_clasificacion` cuando `categoria_cobrada ≠ categoria_asignada_al_vehiculo` (`RN-36`); se agrupa en `reclamo_peaje`, que **sobrevive al cierre de la misión** como cuenta por cobrar contra el concesionario, sin marcar a la institución con un hallazgo (`RN-92`). La coherencia geográfica y temporal de la secuencia se evalúa contra el **alcance vigente a la fecha del hecho** (`RN-37`, `RN-77`): un reordenamiento justificado no es desviación.

---

## 17. `evento_bitacora`

**Módulo M-08.** Supertipo. Todo se captura **sin ninguna conectividad** (`RN-43`) y con **paridad exacta** con la hoja de bitácora impresa (`RN-80`).

| Campo | Tipo lógico | Ob. | Dominio | Regla | Qué pasa si falta |
|---|---|---|---|---|---|
| `id_evento` | `identificador` | Ob. | — | `RN-44` | No se guarda |
| `id_tramo_mision` | `referencia` | Ob. | — | `RN-72` | No se guarda |
| `tipo_de_evento` | `enumerado_configurable` | Ob. | `SALIDA` · `PARADA` · `ARRIBO` · `SALIDA_DE_SITIO` · `ESPERA` · `ENTREGA` · `INTERRUPCION` · `RETORNO` · … catálogo abierto | `RN-39` | No se guarda |
| `id_destino_autorizado` | `referencia` | Cd. | Obligatorio en arribo y salida de sitio | `RN-76` | No se puede derivar el tiempo en sitio |
| `id_lectura_odometro` | `referencia` | Cd. | Obligatorio en salida, retorno y traspaso | `RN-31` | Bloqueo duro |
| `estado_declarado_en_ruta` | `enumerado_configurable` | Cd. | **Declarado por el motorista, con un toque.** El sistema **nunca lo infiere** de la ausencia de movimiento ni de señal | `RN-76` | El tablero muestra "sin dato" con su antigüedad, nunca un progreso inferido |
| `causa_de_espera` | `enumerado_configurable` | Cd. | Obligatoria si el vehículo **no puede operar** | `RN-76` | Solo la espera tipificada cuenta como improductiva en los indicadores |
| `id_dependencia_atribuida` | `referencia` | Cd. | A quién se atribuye la espera improductiva | `RN-76`, `RN-82` | El indicador queda sin responsable |
| `desenlace` | `enumerado_configurable` | Cd. | **Obligatorio** en `INTERRUPCION` | `RN-70` | **Bloqueo.** La interrupción marca la misión sin cambiarle el estado y exige desenlace explícito |
| `adjuntos` | `adjunto` | Op. | Fotografías | `RN-28` | Observación |
| `tiempo_en_sitio` | `derivado` | Dv. | De arribo y salida de sitio, con el reloj del dispositivo | `RN-76` | **Nunca se le pide al motorista que lo cronometre ni que lo digite** |
| `BTT` | — | Ob. | — | `RN-46` | Ver §0.3 |

**Nota de campo.** `RN-74`: el registro de campo **no captura atribución de responsabilidad**. Describe el hecho; la responsabilidad se determina después, en el expediente y por otro actor.

---

## 18. `entrada_diario_sincronizacion` y `conflicto_de_sincronizacion`

**Módulo M-16.** Invariante `M-03` y `RN-45`. **Cero sobrescritura silenciosa.**

| Campo | Tipo lógico | Ob. | Dominio | Regla | Qué pasa si falta |
|---|---|---|---|---|---|
| `id_entrada` | `identificador` | Ob. | **Llave de idempotencia.** Un identificador ya aplicado se **ignora y se registra la recepción duplicada** | `RN-44` | Los reenvíos tras corte de red duplicarían hechos |
| `id_dispositivo` | `referencia` | Ob. | — | máq. estados §6.2 | No se guarda |
| `secuencia_dispositivo` | `entero` | Ob. | **Monotónica.** El servidor aplica en este orden, no en orden de llegada | máq. estados §6.2 | Sin ella no se detectan huecos |
| `estado` | `enumerado_configurable` | Ob. | Los ocho de la máquina §6.2 | `RN-45` | No se guarda |
| `carga_util` | `valor_copiado` | Ob. | La transición o el evento íntegro | `RN-45` | No se guarda |
| `huella_contenido`, `huella_anterior` | `huella` | Ob. | Cadena **por dispositivo** | `RNF-04` | La cadena del dispositivo no cierra |

**`conflicto_de_sincronizacion`** (1 : **2..N** `version_divergente`):

| Campo | Tipo lógico | Ob. | Dominio | Regla | Qué pasa si falta |
|---|---|---|---|---|---|
| `tipo_de_conflicto` | `enumerado_configurable` | Ob. | `ESTADO_ORIGEN_INESPERADO` · `CADENAS_DIVERGENTES` · `HUECO_NO_CERRADO` · `COMPROBANTE_DUPLICADO` · `IDENTIDAD_PRESUNTA_DUPLICADA` | `RN-45`, `RN-84` | No se guarda |
| `id_version_divergente[]` | `referencia` | Ob. | **Todas conservadas íntegras** | `RN-45` | Se perdería una versión, que es lo único prohibido |
| `id_resolucion` | `referencia` | Cd. | Nulo mientras esté abierto. **`BD-08` impide liquidar** con conflicto abierto | máq. estados §6.3 | La misión no liquida |
| `id_autoria_congelada_resolutor` | `referencia` | Cd. | La resolución es **acto humano registrado**: qué versión se toma, cuál se descarta, por qué y con qué autoridad | `RN-45` | No hay resolución válida |

**`RESUELTA_DESCARTADA` no significa borrada.** El contenido queda íntegro y consultable, con la decisión humana que lo descartó.

---

## 19. `adjunto`

**Módulos M-08, M-12, M-15.** `RNF-17`, `RN-47`. Decisión `D-16`.

> **Corrección — hallazgo `HB34-53`.** `adjunto` colgaba de `evento_bitacora`, `expediente_incidente`, `licencia_conducir` y del bloque `BTT` (`id_adjunto_original`), y **no tenía ninguna relación con `segmento_dato_personal` ni campo que clasificara su contenido**. No había forma de saber qué adjuntos contienen dato personal ni de alcanzarlos con el `evento_depuracion`.
>
> El caso concreto: `PT-123`, «digitación diferida desde el papel, **con foto del original**», obligatoria por `RN-47` y por el `BTT`. El original digitado es la hoja de manifiesto, con nombres y números de identidad manuscritos. A los cinco años se ejecuta la depuración: se vacía el `contenido` del segmento, se elimina la sal, la cadena sigue verificando — **y los nombres siguen en el JPEG, íntegros y consultables**. `RNF-17` fija el umbral en **0** y nombra los adjuntos expresamente.

| Campo | Tipo lógico | Ob. | Dominio | Regla | Qué pasa si falta |
|---|---|---|---|---|---|
| `id_adjunto` | `identificador` | Ob. | Generado en el cliente, **sin conectividad** | `RN-43`, `RN-44` | No se guarda |
| `tipo_de_objeto_dueno` | `enumerado_configurable` | Ob. | `EVENTO_BITACORA` · `EXPEDIENTE_INCIDENTE` · `LICENCIA_CONDUCIR` · `ACTA` · `TITULO_DE_TENENCIA` · `DOCUMENTO_VEHICULAR` · `REGISTRO_DIGITADO` (el `id_adjunto_original` del `BTT`) · … | `RN-47` | No se guarda |
| `id_objeto_dueno` | `referencia` | Ob. | Polimórfico con el anterior | `RN-47` | No se guarda |
| `huella_contenido` | `huella` | Ob. | Sobre el archivo. **Se conserva aunque el archivo se depure** | `RNF-04`, `RNF-17` | No se guarda: sin huella no se puede demostrar que la constancia de depuración corresponde a este adjunto |
| `clasificacion_de_contenido` | `enumerado_configurable` | Ob. | `SIN_DATO_PERSONAL` · `CON_DATO_PERSONAL_ESTRUCTURADO` · `CON_DATO_PERSONAL_NO_ESTRUCTURADO` · `NO_CLASIFICADO` | `RNF-17`, `RN-51` | **`NO_CLASIFICADO` es el valor por defecto y se trata como si contuviera dato personal.** El valor por defecto no puede ser el permisivo |
| `id_segmento_dato_personal` | `referencia` | Cd. | Obligatorio si la clasificación no es `SIN_DATO_PERSONAL`. Misma indirección que `linea_manifiesto` | `RNF-17`, `RN-51` | El adjunto queda fuera del alcance de la depuración, que es exactamente el defecto que este campo corrige |
| `id_constancia_de_depuracion` | `referencia` | Cd. | No nulo = **ya depurado**. El archivo dejó de ser legible; el expediente sigue mostrando que existió | `RNF-17` | Nulo es lo normal |
| `BTT` | — | Ob. | — | `RN-46` | Ver §0.3 |

**Entidad dependiente — `constancia_de_depuracion`.** Conserva `huella_contenido` del original, tipo de archivo, tamaño, fecha del hecho del original y referencia al `evento_depuracion` que lo alcanzó, con su autoridad y plazo aplicado. **La depuración de un adjunto no lo hace desaparecer del expediente**: lo hace ilegible dejando constancia verificable de que estuvo.

**Invariante.** Ningún adjunto `NO_CLASIFICADO` se exporta en un paquete de evidencia (`RNF-18`) ni en una exportación de dato de gestión (`RN-51`) sin clasificarse antes.

---

## 20. `solicitud_transporte`

**Módulo M-06.** `RN-02`, `RN-56`, `BD-01`.

> **Corrección — hallazgo `HB34-60`.** `solicitud_transporte` figuraba en el diagrama de la Vista 5 y en la lista de «otras entidades» **sin diccionario**, y ninguna entidad guardaba al **solicitante de derecho**. `BD-01` fue corregido tras el hallazgo `HB3-01` justamente para bloquear el escenario más cotidiano de todos —la asistente captura la solicitud, el jefe la autoriza— comparando al autorizador contra **tres** personas: quien creó, quien envió y **la persona a cuyo nombre se solicita**. `id_unidad_organizativa_requirente` identifica la unidad, no a la persona, y `autoria_congelada` congela a quien ejecuta el acto, no a aquel por cuenta de quien se ejecuta. El dato sobre el que descansa el bloqueo duro más frecuente del sistema no tenía dónde vivir, y `HU-003` no era implementable.

| Campo | Tipo lógico | Ob. | Dominio | Regla | Qué pasa si falta |
|---|---|---|---|---|---|
| `id_solicitud_transporte` | `identificador` | Ob. | Generado en el cliente | `RN-44` | No se guarda |
| `id_persona_solicitante_de_derecho` | `referencia` a `persona` | **Ob.** | **Identidad de persona, no de cuenta de usuario.** Igual al capturador cuando no hay encargo | `BD-01`, `RN-02` | **Bloqueo duro.** Sin él, `BD-01` compara contra dos de las tres personas y el escenario de captura por encargo pasa el control |
| `id_autoria_congelada_creacion` | `referencia` | Ob. | Quien la creó — la primera de las tres que compara `BD-01` | `BD-01`, `RN-03` | No se guarda |
| `id_autoria_congelada_envio` | `referencia` | Cd. | Quien la envió (`T-02`). La segunda de las tres. Nulo mientras esté en borrador | `BD-01` | Nulo válido en borrador |
| `id_unidad_organizativa_requirente` | `referencia` | Ob. | Unidad que origina la necesidad. **No sustituye al solicitante de derecho** | actores §3 | No se guarda: define alcance de datos e imputación |
| `id_tipo_vehiculo_requerido` | `referencia` | Ob. | El eje de compatibilidad con la flota | `RN-20`, `RN-67` | Bloqueo: no se resuelve compatibilidad |
| `motivo_de_viaje` | `enumerado_configurable` | Ob. | Catálogo institucional | `RN-39` | No se guarda |
| `ventana_solicitada` | `rango_de_fechas` | Ob. | Inicio y fin pretendidos | `RN-10` | Bloqueo |
| `contador_de_desplazamientos` | `derivado` | Dv. | Cuántas veces fue desplazada por prelación | `RN-56`, `RN-82` | Se calcula. La solicitud desplazada **conserva su aprobación** y vuelve a la cola con su marca |
| `estado_corriente` | `derivado` | Dv. | Proyección de su diario de transiciones | `D-03` | **Nunca se escribe** |
| `BTT` | — | Ob. | — | `RN-46` | Ver §0.3 |

**Entidad dependiente — `destino_solicitado`** (1 : 1..N): secuencia, ubicación, zona, propósito y tiempo previsto en sitio. **Multi-destino es la norma, no la excepción.**

**Invariante.** `BD-01` compara la identidad de persona del autorizador contra **las tres**: creador, remitente y solicitante de derecho, *«si fueran distintas entre sí»*. Autoridad: [`estados/orden-de-mision.md` §4](../estados/orden-de-mision.md).

---

## 21. `cumplimiento_de_objeto` y `constatacion_al_despachar`

**Módulos M-13, M-08.** `RN-78`, `RN-21` ampliada por `CE-18`.

### `cumplimiento_de_objeto`

> **Corrección — hallazgo `HB34-61`.** Ni la palabra «cumplimiento» aparecía en el modelo. `liquidacion_mision` tenía `linea_liquidacion`, `conciliacion` y `desviacion`, **todas económicas**, mientras `RN-78` es **bloqueo duro**: *«toda misión cierra declarando el grado de cumplimiento de su objeto, por destino y consolidado»*, y sin él la misión no se liquida. **No se podía ejecutar `T-19` y no había dónde escribir el dato.** Una misión de 600 km que no entregó nada porque la bodega estaba cerrada cerraba limpia.

| Campo | Tipo lógico | Ob. | Dominio | Regla | Qué pasa si falta |
|---|---|---|---|---|---|
| `id_cumplimiento` | `identificador` | Ob. | — | `RN-44` | No se guarda |
| `id_orden_mision` | `referencia` | Ob. | — | `RN-78` | No se guarda |
| `alcance` | `enumerado_configurable` | Ob. | `POR_DESTINO` · `CONSOLIDADO` | `RN-78` | No se guarda |
| `id_destino_autorizado` | `referencia` | Cd. | Obligatorio si `alcance = POR_DESTINO` | `RN-78` | No se puede atribuir el incumplimiento a un destino ni a la dependencia que lo pidió |
| `grado` | `enumerado_configurable` | Ob. | Catálogo `grado_de_cumplimiento`. `[C]` — ver `README` §15, punto 16 | `RN-78`, `RN-39` | **Bloqueo duro de `T-19`.** Es dato de cierre, **no observación de texto libre** |
| `causa_de_incumplimiento` | `enumerado_configurable` | Cd. | Obligatoria cuando el grado no es total. Catálogo `causa_de_incumplimiento` | `RN-78` | Bloqueo. Sin causa tipificada el indicador no se puede agregar por dependencia ni por destino |
| `id_acta` | `referencia` a `acta` | Cd. | Acta de entrega **o constancia de no atención**, por destino | `RN-78`, `RN-69` | No se acredita lo entregado ni lo no atendido |
| `id_autoria_congelada_declarante` | `referencia` | Ob. | Quién lo declaró y con qué competencia | `RN-03` | No se guarda |
| `BTT` | — | Ob. | — | `RN-46` | Ver §0.3 |

**Condiciones.** Aplica a toda orden que llegue a `RETORNADA`, por ejecución normal o por retorno anticipado, y a la **misión no ejecutada con consumo** (`T-16`), cuyo grado es *no ejecutada* con su causa. **No aplica** a la misión anulada antes del despacho, que nunca tuvo ejecución que evaluar — de ahí la cardinalidad `0..N`.

**Invariante.** La desviación de kilometraje y de rendimiento derivada de un **retorno anticipado con causa registrada y aceptada no produce hallazgo por sí sola**: la conciliación se recalcula contra el trayecto efectivamente autorizado hasta el punto de retorno (`RN-30`, `RN-77`).

### `constatacion_al_despachar`

> **Corrección — hallazgo `HB34-61`.** `objeto_del_traslado` solo tenía `peso_declarado` y `cantidad_personas`, **ambos declarados**. `CE-18` amplió `RN-21` a *«peso y ocupación **efectivos** con indicador de desviación»*, y lo efectivo se constata al despachar. No tenía campo.

Cuelga de `objeto_en_tramo`, no de la misión, porque la capacidad se evalúa **por tramo, sobre la configuración real de cada tramo** (`RN-68`).

| Campo | Tipo lógico | Ob. | Dominio | Regla | Qué pasa si falta |
|---|---|---|---|---|---|
| `id_constatacion` | `identificador` | Ob. | — | `RN-44` | No se guarda |
| `id_objeto_en_tramo` | `referencia` | Ob. | — | `RN-68` | No se guarda |
| `peso_efectivo` | `magnitud` | Cd. | Obligatorio si `naturaleza = CARGA` | `RN-21`, `CE-18` | Se registra la ausencia y se arrastra a la liquidación como observación |
| `ocupacion_efectiva` | `entero` | Cd. | Obligatoria si `naturaleza = PERSONAS`. **Incluye al motorista** | `RN-21` | Ídem |
| `desviacion_contra_declarado` | `derivado` | Dv. | Efectivo − declarado, absoluto y porcentual | `RN-21`, `RN-30` | Se calcula |
| `id_autoria_congelada_constata` | `referencia` | Ob. | Quien constató. **No puede ser el motorista del tramo** | `RN-01` | No se guarda |
| `BTT` | — | Ob. | — | `RN-46` | Ver §0.3 |

`[C]` Si la constatación es **bloqueante de `T-12`** o solo deja indicador de desviación — ver `README` §15, punto 20.

---

## 22. `conflicto_de_recurso`, `solicitud_desplazada` y `adjudicacion_de_recurso`

**Módulos M-07, M-14.** `RN-56`, `RN-82`, `EF-01`.

> **Corrección — hallazgo `HB34-61`.** No existía entidad de conflicto de recurso ni de desplazamiento. `RN-13` impide la doble asignación, que es **la consecuencia del conflicto, no su resolución**; `RN-56` exige aplicar el criterio parametrizado y **dejar constancia de las desplazadas**. `EF-01` dice que *«cada conflicto registrado, con su resolución, es la medición del déficit de flota»* y *«uno de los pocos indicadores llevables a una gestión presupuestaria con evidencia»*. Sin entidad, ese indicador no existe — y arrastraba también a `RN-82`.

### `conflicto_de_recurso`

| Campo | Tipo lógico | Ob. | Dominio | Regla | Qué pasa si falta |
|---|---|---|---|---|---|
| `id_conflicto` | `identificador` | Ob. | — | `RN-44` | No se guarda |
| `tipo_de_recurso` | `enumerado_configurable` | Ob. | `VEHICULO` · `MOTORISTA` · `SALDO_DE_FONDO` | `RN-56` | No se guarda |
| `id_recurso` | `referencia` | Cd. | Polimórfico con el anterior. Nulo cuando el conflicto es por saldo agregado | `RN-56` | — |
| `origen_del_conflicto` | `enumerado_configurable` | Ob. | `CONCURRENCIA_DE_SOLICITUDES` · `INDISPONIBILIDAD_SOBREVENIDA` (`RN-60`) · `EXTENSION_QUE_INVADE_RESERVA` (`RN-77`) · `FONDO_INSUFICIENTE` (`RN-26`, `RN-88`) | `RN-56` | No se guarda |
| `consolidacion_evaluada` | `booleano` | Ob. | **Se evalúa siempre**, aunque no proceda | `RN-56` | **Bloqueo.** Es la defensa contra el hallazgo *«se pudo hacer un solo viaje y se hicieron dos»* |
| `resultado_de_consolidacion` | `enumerado_configurable` + `texto` | Cd. | Obligatorio si `consolidacion_evaluada`. `PROCEDE` · `NO_PROCEDE` con motivo | `RN-56` | Queda sin registro la evaluación que la regla obliga a dejar |
| `id_criterio_prelacion_aplicado` | `referencia` a `valor_congelado` | Ob. | El criterio **vigente a la fecha del hecho**, congelado | `RN-56`, `RN-40`, `RN-41` | El orden propuesto no se puede reproducir ni defender |
| `BTT` | — | Ob. | — | `RN-46` | Ver §0.3 |

### `solicitud_desplazada` (1 : 1..N desde el conflicto)

| Campo | Tipo lógico | Ob. | Dominio | Regla | Qué pasa si falta |
|---|---|---|---|---|---|
| `id_solicitud_transporte` | `referencia` | Ob. | La que se quedó sin recurso | `RN-56` | No se guarda |
| `posicion_en_la_cartera` | `entero` | Ob. | El orden que produjo el criterio | `RN-56` | No se guarda |
| `id_notificacion_con_acuse` | `referencia` | Cd. | A la dependencia desplazada | `RN-56` | **Sin acuse, su reclamo no tiene contraparte y el hecho no existe para nadie más que para ella** |

**La solicitud desplazada conserva su aprobación**, vuelve a la cola con marca y contador, y su eventual caducidad se anula con motivo tipificado *sin recurso disponible* — **nunca por vencimiento silencioso** (`RN-56`).

### `adjudicacion_de_recurso` (0..1 por conflicto)

Quién adjudicó, a qué solicitud, con `id_autoria_congelada`, y —si se apartó del orden propuesto— **justificación registrada, que es admisible y obligatoria** (`RN-56`). **El sistema no adjudica por sí solo, no cancela misiones por sí solo y no ordena por jerarquía del solicitante.**

---

## 23. `ejercicio` y `renglon_saldo_apertura`

**Módulos M-13, M-09, M-18, M-14.** `RN-96`, `RN-97`.

> **Corrección — hallazgo `HB34-61`.** No había entidad `ejercicio`. El §14 de `folio` invocaba *«el invariante de cierre de ejercicio»* **contra un objeto que no existía**, y `RN-97` —«lo no terminal al corte constituye el saldo de apertura, con antigüedad desde el hecho»— no tenía ni corte ni saldo. Sin saldo de apertura el mecanismo de olvido es automático y no requiere mala fe: llega enero, los reportes arrancan en cero, y una misión interrumpida en noviembre, un préstamo vencido en agosto y una obligación de reintegro de mayo dejan de aparecer en ninguna pantalla.

### `ejercicio`

| Campo | Tipo lógico | Ob. | Dominio | Regla | Qué pasa si falta |
|---|---|---|---|---|---|
| `id_ejercicio` | `identificador` | Ob. | — | `RN-44` | No se guarda |
| `denominacion` | `texto_corto` | Ob. | Ejercicio fiscal | `RN-96` | No se guarda |
| `fecha_de_corte_legal` | `fecha` | Ob. | Parámetro con vigencia. `[C]` — ver `README` §15, punto 19 | `RN-96`, `RN-39` | Bloqueo |
| `fecha_de_corte_operativa` | `fecha` | Ob. | Ídem | `RN-96` | Bloqueo |
| `id_acta_de_cierre` | `referencia` a `acta` | Cd. | Fechas de corte aplicadas, parámetros vigentes usados, quién lo ejecutó y cuándo | `RN-96` | El cierre no queda acreditado |
| `estado` | `enumerado_configurable` | Ob. | `ABIERTO` · `EN_VENTANA_DE_CIERRE` · `CERRADO` | `RN-96` | No se guarda |

**Invariante fundamental (`RN-96`).** El cierre de ejercicio es un **corte de imputación y de reporte**. **No ejecuta ni habilita ninguna transición de la Orden de Misión, y ningún expediente cambia de estado por efecto de una fecha.** La orden que cruza el corte **no se divide**: cada hecho económico se imputa al ejercicio de su fecha del hecho y se valora con la tabla vigente a esa fecha (`RN-40`); la liquidación presenta el desglose por ejercicio. Un cierre masivo por fecha —cincuenta expedientes cerrados el 31 de diciembre a la misma hora con el mismo motivo— **es el hallazgo, no su solución**.

### `renglon_saldo_apertura` (1 : 0..N desde el ejercicio siguiente)

| Campo | Tipo lógico | Ob. | Dominio | Regla | Qué pasa si falta |
|---|---|---|---|---|---|
| `tipo_de_expediente` | `enumerado_configurable` | Ob. | `ORDEN_MISION` · `INTERRUPCION_SIN_DESENLACE` · `PRESTAMO_VENCIDO` · `OBLIGACION_REINTEGRO` · `EXPEDIENTE_INCIDENTE` · `RECLAMO_PEAJE` · `IMPUTACION_EXTERNA` · `BITACORA_PENDIENTE_DE_DIGITACION` | `RN-97` | No se guarda |
| `id_expediente` | `referencia` | Ob. | Polimórfico con el anterior | `RN-97` | No se guarda |
| `fecha_del_hecho_original` | `marca_de_tiempo` | Ob. | **No la del corte** | `RN-97`, `RN-46` | No se guarda |
| `antiguedad_en_dias` | `derivado` | Dv. | Desde el hecho original. **No se reinicia con el cambio de ejercicio** | `RN-97` | Se calcula. Es la parte incómoda de la regla y por eso es la que sirve: un expediente que llega al tercer ejercicio con 800 días **no se puede presentar como pendiente reciente** |
| `causa_tipificada` | `enumerado_configurable` | Ob. | — | `RN-97` | Bloqueo |
| `id_persona_responsable` | `referencia` | Ob. | **Responsable nominado**, no una unidad | `RN-97` | Bloqueo: un pendiente sin responsable es un pendiente abandonado |

**Invariante.** El saldo de apertura **coincide renglón por renglón** con el inventario de expedientes no terminales al corte (`RN-96`, `RN-97`), se emite como **documento con folio** junto al acta de cierre, y ambos se conservan. **Ningún período se cierra con préstamos vencidos ni con interrupciones sin desenlace.**

---

## 24. `expediente_convalidacion`

**Módulos M-06, M-07, M-14.** `RN-73`, `CE-01`, `HU-008`.

> **Corrección — hallazgo `HB34-61`.** `orden_mision.id_solicitud_transporte` nulo *«obliga a expediente de convalidación con cronología declarada»* —así lo dice este mismo diccionario en §1— y **ese expediente no era ninguna entidad**. `CE-01` y `HU-008` dependen de él.

| Campo | Tipo lógico | Ob. | Dominio | Regla | Qué pasa si falta |
|---|---|---|---|---|---|
| `id_expediente_convalidacion` | `identificador` | Ob. | Generado en el cliente | `RN-44` | No se guarda |
| `id_orden_mision` | `referencia` | Ob. | La orden nacida sin autorización previa | `RN-73` | No se guarda |
| `causal_tipificada` | `enumerado_configurable` | Ob. | Salida en régimen de emergencia, retorno anticipado por riesgo, conductor incorporado por incapacidad del titular, desenlace decidido sin poder consultar | `RN-73` | **Bloqueo** |
| `id_autoria_congelada_declarante` | `referencia` | Ob. | Quién declaró la causal | `RN-73`, `RN-03` | Bloqueo |
| `quien_ordeno_verbalmente` | `texto` + `referencia` a `persona` | Ob. | **Declarado por quien recibió la orden** | `RN-73` | Bloqueo |
| `canal_de_la_orden_verbal` | `enumerado_configurable` | Ob. | Por qué medio se recibió | `RN-73` | Bloqueo |
| `ocurrido_en_el_hecho` | `marca_de_tiempo` | Ob. | Hora real del acto ejecutado sin autorización | `RN-46`, `RN-73` | Bloqueo |
| `convalidado_en` | `marca_de_tiempo` | Cd. | Nulo mientras no se convalide | `RN-73` | Nulo válido: el expediente está abierto |
| `id_autoria_congelada_convalidador` | `referencia` | Cd. | Qué puesto convalida: `[C]` insumos #32 y #50 | `RN-73` | El acto queda sin competencia acreditada |
| `intervalo_hasta_convalidacion` | `derivado` | Dv. | `convalidado_en − ocurrido_en_el_hecho`. **Se muestra en el expediente y en el impreso**, junto al plazo vigente aplicado | `RN-73` | Se calcula |
| `id_valor_congelado_plazo_aplicado` | `referencia` | Ob. | `plazo_convalidacion` vigente **a la fecha del hecho** | `RN-73`, `RN-40` | No se puede decir si se excedió |
| `es_extemporanea` | `derivado` | Dv. | Verdadero si el intervalo supera el plazo | `RN-73` | Se calcula. **Vencido el plazo la convalidación no se rechaza**: se registra igual y la misión cierra con hallazgo |
| `BTT` | — | Ob. | — | `RN-46` | Ver §0.3 |

**Invariante de cronología (`RN-73`).** Cuando la marca de tiempo del hecho de una transición es **posterior** a la de la transición que le sigue en la máquina de estados, el expediente **lo declara explícitamente y lo imprime**. **Ningún acto se presenta como previo si fue posterior.** El sistema tiene ambos datos —fecha del hecho y fecha de captura, `RN-46`— y por tanto **puede** decir la verdad; si no la dice, es porque alguien decidió que no la dijera.

**No aplica** a los bloqueos duros que no admiten excepción: `RN-01` segregación y `RN-09` / `RN-10` habilitación. Un acto que los viola **no se convalida**: se registra como hecho y como hallazgo.

---

## 25. Resumen de entidades por módulo

> **Corrección — hallazgo `HB34-63`.** Las nueve vistas del [`README`](README.md) contienen **167 entidades distintas** y este resumen enumeraba **126**: faltaban **41**, y varias eran portadoras de invariantes —`objeto_en_tramo` (`RN-68`), `desenlace_interrupcion` (bloqueo duro de `RN-70`), `hallazgo_de_cierre`, `valor_anterior_y_nuevo` (dos de los seis campos obligatorios de `RNF-04`), `entrada_matriz_licencia_vehiculo` (`BD-02`), los cuatro diarios de estado sobre los que descansa la decisión `D-03`, `linea_liquidacion`, `kilometraje_acumulado`, `tarifa_peaje` y `fecha_corte_conocimiento` (`RNF-06`)—. Como esta es la lista que el Sprint 1 va a usar para saber qué falta documentar, **lo omitido se quedaba sin diccionario dos veces**. Abajo está completa, incluidas las entidades incorporadas por esta revisión, en **negrita** las que ya tienen sección propia.

| Módulo | Entidades núcleo documentadas aquí | Otras entidades del módulo |
|---|---|---|
| M-01 | `persona`, `puesto`, `asignacion_de_puesto`, `autoria_congelada` | `institucion`, `unidad_organizativa`, `delegacion`, `rol`, `puesto_rol`, `rol_permiso`, `alcance_de_datos`, `usuario`, `dispositivo`, `delegacion_de_autoridad`, `incompatibilidad_detectada`, `empleado_espejo` |
| M-02 | `tabla_parametrica`, `version_tabla_parametrica`, `entrada_parametrica`, `valor_congelado` | `acto_de_aprobacion`, `catalogo_tipificado`, `valor_tipificado`, `tipo_vehiculo`, `tipo_objeto_traslado`, `compatibilidad_vehiculo_objeto`, `compatibilidad_objeto_objeto`, `categoria_licencia`, `entrada_matriz_licencia_vehiculo`, `categoria_peaje`, `punto_peaje`, `tarifa_peaje`, `operador_vial`, `zona`, `estacion_servicio`, `calendario_laboral`, `dia_inhabil`, `horario_habil`, `motivo_de_viaje` |
| M-03 / M-04 / M-11 | `vehiculo`, `asignacion_de_placa`, `historial_estado_placa`, `titulo_de_tenencia`, `serie_instrumento_medicion`, `lectura_odometro` | `version_ficha_tecnica`, `rubro_asumido`, `custodia_vehiculo`, `regimen_de_uso`, `evento_estado_operativo`, `documento_vehicular`, `tipo_documento_vehicular`, `verificacion_rotulacion`, `asignacion_categoria_peaje`, `exoneracion_peaje`, `kilometraje_acumulado`, `evento_intervencion_instrumento`, `prestamo_vehiculo`, `orden_trabajo`, `repuesto_aplicado`, `imputacion_externa`, `fusion_de_expedientes` |
| M-05 | `motorista`, `licencia_conducir`, `categoria_en_licencia` | `restriccion_medica`, `tipo_restriccion_medica`, `capacitacion`, `evento_habilitacion`, `evaluacion_habilitacion_congelada`, `ausencia_espejo` |
| M-06 / M-07 | `orden_mision`, `transicion_orden_mision`, `version_alcance_autorizado`, `objeto_del_traslado`, `tramo_mision`, **`solicitud_transporte`** §20, **`constatacion_al_despachar`** §21, **`conflicto_de_recurso`** §22, **`expediente_convalidacion`** §24 | `destino_solicitado`, `destino_autorizado`, `acto_de_autorizacion`, `objeto_en_tramo`, `reserva_recurso`, `solicitud_desplazada`, `adjudicacion_de_recurso`, `permiso_circulacion_inhabil`, `resultado_verificacion`, `codigo_autorizacion_fuera_de_linea`, `hallazgo_de_cierre`, `vinculacion_argos` |
| M-08 / M-12 / M-17 / M-19 | `evento_bitacora`, **`adjunto`** §19 | `tipo_evento_bitacora`, `evento_arribo`, `evento_salida_de_sitio`, `evento_espera`, `evento_interrupcion`, `evento_entrega`, `desenlace_interrupcion`, `dependencia_responsable`, `posicion_reportada`, `acta`, `tipo_acta`, `firmante_acta`, `manifiesto_persona_externa`, `linea_manifiesto`, `novedad_de_manifiesto`, `registro_de_consulta`, `inventario_de_carga`, `linea_inventario`, `diferencia_de_inventario`, `expediente_incidente`, `involucrado_en_incidente`, `evento_estado_incidente` |
| M-09 / M-18 / M-13 | `asignacion_combustible`, `consumo_combustible`, `paso_por_caseta`, **`compromiso_proyectado`** §15, **`cumplimiento_de_objeto`** §21, **`ejercicio`** §23 | `fondo_combustible`, `evento_estado_fondo`, `evento_estado_asignacion`, `abastecimiento`, `fuente_financiamiento`, `comprobante`, `emisor_comprobante`, `gasto_imprevisto`, `obligacion_reintegro`, `evento_estado_reintegro`, `estimacion_peaje`, `discrepancia_clasificacion`, `reclamo_peaje`, `liquidacion_mision`, `linea_liquidacion`, `conciliacion`, `desviacion`, `causa_tipificada`, `renglon_saldo_apertura` |
| M-14 / M-15 / M-16 | `asiento_auditoria`, `segmento_dato_personal`, `folio`, `rango_de_folio`, **`subrango_de_folio`** §14, `entrada_diario_sincronizacion`, `conflicto_de_sincronizacion` | `valor_anterior_y_nuevo`, `sello_de_cadena`, `destino_de_anclaje`, `evento_depuracion`, `constancia_de_depuracion`, `rectificacion_habeas_data`, `asiento_reverso`, `asiento_de_diferencia`, `expediente_hallazgo_posterior`, `evento_estado_folio`, `evento_estado_subrango`, `tipo_documento`, `documento_emitido`, `huella_documento`, `impresion`, `verificacion_qr`, `reporte_generado`, `fecha_corte_conocimiento`, `version_divergente`, `resolucion_de_conflicto` |
| M-20 | — | `sistema_origen`, `entidad_espejo`, `evento_sincronizacion`, `estado_de_frescura`, `divergencia_de_espejo`, `empleado_espejo`, `estructura_espejo`, `ausencia_espejo`, `feriado_espejo`, `unidad_ejecutora_espejo`, `objeto_del_gasto_espejo`, `nivel_autorizacion_espejo`, `cuota_trimestral_espejo`, `vinculacion_argos`, `hecho_expuesto` |

---

## 26. Trazabilidad

Ver [`README.md` §16](README.md#16-trazabilidad), que trae el **estado por requisito** de los cinco `RNF` irreversibles con lo que queda abierto en cada uno.

Las entidades de este diccionario que los sostienen:

| `RNF` | Entidades |
|---|---|
| `RNF-03` operación sin conectividad | Bloque `BTT` (§0.3) e identificadores generados en cliente |
| `RNF-04` bitácora encadenada | `asiento_auditoria` (§13), con su cadena global y su subcadena por dispositivo |
| `RNF-05` temporalidad normativa | `version_tabla_parametrica` **con sus dos ejes** y `valor_congelado` con su corte de conocimiento (§12) |
| `RNF-17` retención y depuración | `segmento_dato_personal` **polimórfico** (§13) y `adjunto` clasificado y depurable (§19) |
| `RNF-21` integridad de folios | `folio`, `rango_de_folio` y **`subrango_de_folio`** (§14) |

> **Corrección — hallazgo `HB34-64`.** Este apartado afirmaba sin matiz que las entidades *«cubren los cinco requisitos»*. Cuatro de los cinco tenían un agujero estructural, señalado en `H-B34-002` y corregido en esta revisión. «Cubren» significa que **la estructura existe**; no significa que esté verificada ni que los datos que necesita estén entregados. El desglose honesto está en el `README` §16.

**Pendiente del Sprint 1.** Diccionario de las entidades no núcleo listadas en §25, y el modelo lógico completo con claves y restricciones de integridad. **Sigue sin haber DDL ni motor** hasta el Sprint 2 ([`ADR-000`](../adr/ADR-000-diferir-seleccion-de-stack.md)).
