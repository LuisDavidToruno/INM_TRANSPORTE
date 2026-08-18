# Diccionario de datos — entidades núcleo

Complemento de [`README.md`](README.md). Detalle campo por campo de las **entidades núcleo**: las que sostienen los seis invariantes estructurales y las que un auditor recorrería para reconstruir una misión.

**No hay tipos físicos.** Los tipos son **lógicos** y el Sprint 2 los materializa contra el stack elegido ([`ADR-000`](../adr/ADR-000-diferir-seleccion-de-stack.md)).

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
| `id_folio` | `referencia` a `folio` | Ob. | Del rango de la delegación emisora | `RN-44`, `RNF-21` | No se puede imprimir ni despachar |
| `id_solicitud_transporte` | `referencia` | Op. | Nulo cuando la orden nace de convalidación de acto sin autorización previa | `RN-73` | Nulo obliga a expediente de convalidación con cronología declarada tal como ocurrió |
| `id_unidad_organizativa_requirente` | `referencia` | Ob. | Unidad que origina la necesidad | actores §3 | No se guarda: define alcance de datos e imputación |
| `id_delegacion_emisora` | `referencia` | Ob. | Determina el rango de folios usado | `RNF-21` | No se guarda |
| `motivo_de_viaje` | `enumerado_configurable` | Ob. | Catálogo institucional | `RN-39` | No se guarda |
| `objeto_principal` | `referencia` a `objeto_del_traslado` | Ob. | Declara cuál manda para el orden de reducción de capacidad | `RN-67`, `RN-21` | Bloqueo: sin objeto principal no se sabe qué se reduce si hay exceso |
| `id_vinculacion_argos` | `texto_corto` | Op. | Clave con la que SIGTI expone hechos a ARGOS | `RN-81` | Nulo: la misión no se expone hasta que exista |
| `id_dispositivo_portador` | `referencia` | Cd. | Designado al despachar. Único cuya cadena se aplica automáticamente | máq. estados §6.3 regla 4 | Sin portador designado, toda cadena entrante es "de dispositivo no portador" y no se aplica sola |
| `estado_corriente` | `derivado` | Dv. | Proyección del diario de `transicion_orden_mision` | máq. estados P-1, `RN-06` | **Nunca se escribe.** Si se escribiera, dos dispositivos producirían última-escritura-gana (decisión `D-03`) |
| `tiene_divergencia_pendiente` | `derivado` | Dv. | Verdadero si existe `conflicto_de_sincronizacion` abierto | `BD-08` | Impide liquidar mientras sea verdadero |
| `BTT` | — | Ob. | Bloque completo | `RN-46` | Ver §0.3 |

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
| `id_acta_traspaso_apertura` | `referencia` | Cd. | Obligatorio si `causa_de_apertura ≠ INICIO_DE_MISION` | `RN-71` | **Bloqueo.** Sin acta no hay corte de imputación y el kilometraje se atribuye mal |
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
| `vigencia` | `rango_de_fechas` | Ob. | **Sin solape ni hueco con las demás versiones de la misma tabla.** Se impide al cargar, no se detecta después | `RNF-05` | No se guarda |
| `id_acto_de_aprobacion` | `referencia` | Ob. | Acuerdo, resolución o acta institucional | `RN-39` | Bloqueo |
| `fuente` | `texto` | Ob. | De dónde salió el dato | `RN-34` | Bloqueo |
| `fecha_de_verificacion` | `fecha` | Ob. | El sistema alerta a los 12 meses sin revisar | `RN-34` | Bloqueo |
| `nivel_de_verificacion` | `enumerado_configurable` | Ob. | `[V]` · `[P]` · `[C]` · `[I]` | CLAUDE.md | Bloqueo. **El nivel nunca sube al bajar de abstracción** |
| `id_autoria_cargó` | `referencia` | Ob. | Quien la cargó | actores §4.3 | Bloqueo |
| `id_autoria_aprobó` | `referencia` | Ob. | **Doble control: distinta de quien cargó** | actores §4.3, `RN-01` | Bloqueo duro |

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
| `concepto` | `enumerado_configurable` | Ob. | `ESTIMACION_PEAJE` · `HABILITACION_LICENCIA` · `RENDIMIENTO_ESPERADO` · `UMBRAL_APLICADO` · `TARIFA_POR_PUNTO` · … | `RN-41` | No se guarda |
| `valor` | `magnitud` o `valor_copiado` | Ob. | El resultado | `RN-41` | No se guarda |
| `id_version_tabla_parametrica` | `referencia` | Ob. | **La versión que lo produjo** | `RN-41` | **Bloqueo.** Sin ella una consulta futura recalcularía con los parámetros actuales y mostraría un valor que nunca se autorizó |
| `valores_unitarios` | `valor_copiado` | Ob. | Tarifa por punto, número de cruces, umbral aplicado — los componentes | `RN-41` | El total no se puede explicar ni defender |
| `fecha_del_hecho_usada` | `marca_de_tiempo` | Ob. | Con la que se resolvió la vigencia | `RN-40` | Bloqueo |
| `base_de_derivacion` | `enumerado_configurable` | Cd. | `POR_VEHICULO` · `POR_TIPO_DE_VEHICULO_ESTIMATIVA`. Obligatorio en estimaciones de peaje | `RN-33` | **Un estimado que no dice sobre qué base se calculó no se puede defender ante quien lo autorizó** |
| `id_acto_que_congela` | `referencia` | Ob. | Transición o autorización que lo fijó | `RN-41` | No se guarda |

**Invariante.** Una corrección posterior de la tabla **nunca reescribe** un valor congelado: genera `asiento_de_diferencia`, imputado al período corriente con referencia al período afectado (`RN-42`, `RN-93`).

---

## 13. `asiento_auditoria` y `segmento_dato_personal`

**Módulo M-14.** Invariante `M-02`. La resolución estructural de `RNF-04` × `RNF-17`.

### `asiento_auditoria`

| Campo | Tipo lógico | Ob. | Dominio | Regla | Qué pasa si falta |
|---|---|---|---|---|---|
| `id_asiento` | `identificador` | Ob. | Generado en el origen | `RN-44` | No se guarda |
| `id_asiento_anterior` | `referencia` | Cd. | Nulo solo en el primero de la cadena | `RNF-04` | La cadena no cierra: se detecta como ruptura |
| `entidad_afectada` | `valor_copiado` | Ob. | Tipo e identificador del objeto | `RNF-04` | No se guarda |
| `operacion` | `enumerado_configurable` | Ob. | `ALTA` · `MODIFICACION` · `TRANSICION` · `CONSULTA` · `EMISION` · `ANULACION` · `REVERSO` · `DEPURACION` · `FUSION` | `RN-04` | No se guarda |
| `id_autoria_congelada` | `referencia` | Ob. | Ver §11 | `RNF-15`, `RNF-04` | **No se guarda.** Es uno de los seis campos obligatorios del `RNF-04` |
| `valor_anterior` | `valor_copiado` | Cd. | **De los campos NO personales.** Obligatorio en `MODIFICACION` y `REVERSO`, incluso si es nulo | `RNF-04`, `RN-04` | No se guarda |
| `valor_nuevo` | `valor_copiado` | Cd. | Ídem | `RNF-04` | No se guarda |
| `referencia_segmento_personal` | `referencia` | Cd. | Obligatorio si la operación tocó dato personal | `RNF-17` | El dato personal quedaría en claro dentro del asiento y la depuración rompería la cadena |
| `huella_segmento_personal` | `huella` | Cd. | Calculada **con la sal que vive dentro del segmento** | `RNF-17`, decisión `D-04` | Sin sal la huella sería reversible por fuerza bruta sobre un dominio enumerable |
| `id_version_tabla_parametrica_usada` | `referencia` | Cd. | Si la operación usó tabla paramétrica | `RN-41` | La operación no es reproducible |
| `huella_contenido` | `huella` | Ob. | Sobre representación canónica del asiento | `RNF-04` | No se guarda |
| `huella_anterior` | `huella` | Cd. | Del asiento anterior | `RNF-04` | La cadena no cierra |
| `id_sello_de_cadena` | `referencia` | Dv. | Asignado al sellar el período | `RNF-04` | Se asigna |
| `BTT` | — | Ob. | — | `RN-46` | Ver §0.3 |

**Prohibiciones estructurales.** Ninguna funcionalidad del sistema modifica ni borra un asiento. **Ni el Administrador del Sistema (`ACT-01`)**, y su ausencia de esa capacidad debe ser demostrable (`RNF-04`, máq. estados §9.3). La propiedad que se promete es **detectabilidad con anclaje externo**, no inmutabilidad absoluta: prometer lo segundo ante el TSC costaría más que declarar la limitación.

### `segmento_dato_personal`

| Campo | Tipo lógico | Ob. | Dominio | Regla | Qué pasa si falta |
|---|---|---|---|---|---|
| `id_segmento` | `identificador` | Ob. | Lo que el asiento referencia | `RNF-17` | No se guarda |
| `contenido` | `valor_copiado` | Cd. | Los campos del **catálogo autorizado** y ninguno más | `RN-51` | Nulo tras la depuración: es el estado esperado |
| `sal_de_huella` | `valor_copiado` | Ob. | **Vive con el dato y se elimina con él** | decisión `D-04` | Sin ella la huella es invertible y la depuración sería cosmética |
| `categoria_de_dato` | `enumerado_configurable` | Ob. | `IDENTIFICACION` · `CONTACTO` · `DOCUMENTO` · `SALUD` (solo con base legal expresa) | `RN-51` | No se guarda |
| `base_legal_del_campo` | `texto` + `adjunto` | Cd. | **Obligatorio** para salud, etnia, situación migratoria o condición de vulnerabilidad | `RN-51`, `RNF-17` | **El campo no se ofrece.** El umbral de `RNF-17` es cero |
| `vigencia_de_retencion` | `rango_de_fechas` | Ob. | Plazo **menor** que el de los registros contables. `[C]` insumo #71 | `RNF-17` | **Sin plazo configurado el sistema no depura nada** y lo declara en la pantalla de estado. No se inventa un plazo por defecto |
| `id_evento_depuracion` | `referencia` | Cd. | No nulo = ya depurado | `RNF-17` | — |

**Qué sobrevive a la depuración.** Conteo de pasajeros, condición agregada, origen, destino, vehículo, misión y costos. **No** identidad, contacto ni documento. El reporte regenerado con la misma fecha de corte debe dar **conteos, costos y estructura idénticos** (`RNF-06`); solo las identidades aparecen seudonimizadas.

---

## 14. `folio` y `rango_de_folio`

**Módulo M-15.** Invariante `M-03`. `RN-44`, `RNF-21`. **Distinto del identificador interno.**

### `rango_de_folio`

| Campo | Tipo lógico | Ob. | Dominio | Regla | Qué pasa si falta |
|---|---|---|---|---|---|
| `id_rango` | `identificador` | Ob. | — | — | No se guarda |
| `id_delegacion` | `referencia` | Ob. | **Los rangos no se solapan entre delegaciones** | `RN-44` | Colisión de folios entre delegaciones desconectadas |
| `id_tipo_documento` | `referencia` | Ob. | Orden de misión, salvoconducto, vale, acta | `RNF-21` | No se guarda |
| `desde`, `hasta` | `texto_corto` | Ob. | Correlativo legible | `RNF-21` | No se guarda |
| `umbral_de_alerta` | `entero` | Ob. | Aviso al quedar bajo el umbral. `[C]` insumo #34, referencia 20 % | `RNF-21` | El rango se agota sin aviso, y la reposición **sí requiere red** |
| `saldo_disponible` | `derivado` | Dv. | — | `RNF-21` | Se calcula |

### `folio`

| Campo | Tipo lógico | Ob. | Dominio | Regla | Qué pasa si falta |
|---|---|---|---|---|---|
| `id_folio` | `identificador` | Ob. | Interno, opaco | `RN-44` | No se guarda |
| `numero` | `texto_corto` | Ob. | **Único en la institución. Nunca se recicla**, ni el de un anulado | `RNF-21` | Un folio duplicado es hallazgo directo de auditoría; uno reciclado es indistinguible de una alteración deliberada |
| `id_rango` | `referencia` | Ob. | — | `RN-44` | No se guarda |
| `estado` | `derivado` de `evento_estado_folio` | Dv. | `RESERVADO` · `EMITIDO` · `ANULADO` · `EXTRAVIADO` | `RNF-21` | — |
| `id_documento_emitido` | `referencia` | Cd. | 0..1. **Nunca dos documentos por folio** | `RNF-21` | — |
| `motivo_de_anulacion` | `enumerado_configurable` + `texto` | Cd. | Obligatorio al anular | `RN-04`, `RNF-21` | **Hueco sin explicar** — exactamente lo que el auditor busca |
| `numero_talonario_preimpreso` | `texto_corto` | Op. | Si el talonario físico trae folio propio. `[C]` insumo #46 | `RNF-21` | Nulo válido |
| `BTT` | — | Ob. | — | `RN-46` | Ver §0.3 |

**Invariante de cierre de ejercicio.** Todo folio reservado y no consumido al corte **se anula con acta**; ni el folio ni el compromiso se arrastran al ejercicio siguiente (`RN-96`).

---

## 15. `asignacion_combustible` y `consumo_combustible`

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

## 19. Resumen de entidades por módulo

| Módulo | Entidades núcleo documentadas aquí | Otras entidades del módulo |
|---|---|---|
| M-01 | `persona`, `puesto`, `asignacion_de_puesto`, `autoria_congelada` | `institucion`, `unidad_organizativa`, `delegacion`, `rol`, `puesto_rol`, `alcance_de_datos`, `usuario`, `dispositivo`, `delegacion_de_autoridad`, `incompatibilidad_detectada`, `acta_cierre_asignacion` |
| M-02 | `tabla_parametrica`, `version_tabla_parametrica`, `entrada_parametrica`, `valor_congelado` | `catalogo_tipificado`, `valor_tipificado`, `tipo_vehiculo`, `tipo_objeto_traslado`, `categoria_licencia`, `categoria_peaje`, `punto_peaje`, `operador_vial`, `zona`, `estacion_servicio`, `calendario_laboral`, `horario_habil` |
| M-03 / M-04 / M-11 | `vehiculo`, `asignacion_de_placa`, `historial_estado_placa`, `titulo_de_tenencia`, `serie_instrumento_medicion`, `lectura_odometro` | `version_ficha_tecnica`, `custodia_vehiculo`, `regimen_de_uso`, `evento_estado_operativo`, `documento_vehicular`, `verificacion_rotulacion`, `asignacion_categoria_peaje`, `exoneracion_peaje`, `prestamo_vehiculo`, `orden_trabajo`, `imputacion_externa`, `fusion_de_expedientes` |
| M-05 | `motorista`, `licencia_conducir`, `categoria_en_licencia` | `restriccion_medica`, `capacitacion`, `evento_habilitacion`, `evaluacion_habilitacion_congelada` |
| M-06 / M-07 | `orden_mision`, `transicion_orden_mision`, `version_alcance_autorizado`, `objeto_del_traslado`, `tramo_mision` | `solicitud_transporte`, `destino_solicitado`, `destino_autorizado`, `reserva_recurso`, `permiso_circulacion_inhabil`, `resultado_verificacion`, `codigo_autorizacion_fuera_de_linea` |
| M-08 / M-12 / M-17 / M-19 | `evento_bitacora` | `acta`, `firmante_acta`, `manifiesto_persona_externa`, `linea_manifiesto`, `novedad_de_manifiesto`, `registro_de_consulta`, `inventario_de_carga`, `linea_inventario`, `diferencia_de_inventario`, `expediente_incidente`, `posicion_reportada` |
| M-09 / M-18 / M-13 | `asignacion_combustible`, `consumo_combustible`, `paso_por_caseta` | `fondo_combustible`, `abastecimiento`, `comprobante`, `gasto_imprevisto`, `obligacion_reintegro`, `estimacion_peaje`, `discrepancia_clasificacion`, `reclamo_peaje`, `liquidacion_mision`, `conciliacion`, `desviacion` |
| M-14 / M-15 / M-16 | `asiento_auditoria`, `segmento_dato_personal`, `folio`, `rango_de_folio`, `entrada_diario_sincronizacion`, `conflicto_de_sincronizacion` | `sello_de_cadena`, `destino_de_anclaje`, `evento_depuracion`, `rectificacion_habeas_data`, `asiento_reverso`, `expediente_hallazgo_posterior`, `documento_emitido`, `impresion`, `verificacion_qr`, `reporte_generado`, `version_divergente`, `resolucion_de_conflicto` |
| M-20 | — | `sistema_origen`, `entidad_espejo`, `evento_sincronizacion`, `estado_de_frescura`, `divergencia_de_espejo`, `empleado_espejo`, `estructura_espejo`, `ausencia_espejo`, `feriado_espejo`, `unidad_ejecutora_espejo`, `objeto_del_gasto_espejo`, `nivel_autorizacion_espejo`, `cuota_trimestral_espejo`, `vinculacion_argos`, `hecho_expuesto` |

---

## 20. Trazabilidad

Ver [`README.md` §16](README.md#16-trazabilidad). Las entidades de este diccionario cubren los cinco requisitos no funcionales que el índice declara imposibles de agregar después: `RNF-03` (bloque `BTT` e identificadores en cliente), `RNF-04` (`asiento_auditoria`), `RNF-05` (`version_tabla_parametrica`, `valor_congelado`), `RNF-17` (`segmento_dato_personal`), `RNF-21` (`folio`, `rango_de_folio`).

**Pendiente del Sprint 1.** Diccionario de las entidades no núcleo listadas en §19, y el modelo lógico completo con claves y restricciones de integridad. **Sigue sin haber DDL ni motor** hasta el Sprint 2 ([`ADR-000`](../adr/ADR-000-diferir-seleccion-de-stack.md)).
