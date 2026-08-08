# Reglas de negocio — SIGTI

Una regla de negocio es una afirmación **verificable**: se puede escribir una prueba que determine si el sistema la cumple. Si no se puede probar, no es una regla — es un principio de diseño y va en otro lado.

**Un archivo por regla.** Los IDs son estables y **nunca se reciclan**: si una regla se descarta, su ID queda marcado como obsoleto pero no se reutiliza.

Plantilla: [`docs/plantillas/regla-de-negocio.md`](../../plantillas/regla-de-negocio.md).

## Cómo leer la tabla

| Columna | Significado |
|---|---|
| **Tipo** | `Bloqueo duro` impide la operación · `Advertencia` deja continuar con acuse registrado · `Cálculo` produce un valor · `Derivación` resuelve un atributo a partir de otros |
| **Cfg.** | Si el comportamiento se controla con un parámetro configurable. `Sí*` = el bloqueo es configurable; `No` = no se puede desactivar |
| **Origen** | Ficha normativa `NRM-xx`, decisión de producto `DP-001`, decisión de arquitectura `ADR-001`, o premisa rectora del proyecto |

Nivel de verificación (`[V]` `[P]` `[C]` `[I]`) va dentro de cada regla, marcado en cada afirmación normativa.

---

## Autorización y control interno

| ID | Enunciado | Módulos | Tipo | Cfg. | Origen |
|---|---|---|---|---|---|
| [RN-01](RN-01-segregacion-de-funciones.md) | Un mismo servidor no puede ejercer dos funciones de control sobre la misma Orden de Misión | M-01, M-06, M-07, M-09, M-13 | Bloqueo duro | No | NRM-01 |
| [RN-02](RN-02-escalamiento-de-autorizacion.md) | Cuando el autorizador natural es el solicitante, la autorización escala al nivel inmediato superior | M-01, M-06, M-20 | Derivación + bloqueo | Sí | NRM-01 |
| [RN-03](RN-03-registro-inmutable-de-autorizacion.md) | Toda autorización se registra de forma inmutable con identidad, rol, momento, origen y huella del contenido | M-01, M-14, M-15 | Bloqueo duro | No | NRM-01, NRM-08, DP-001 D-04 |
| [RN-04](RN-04-anulacion-como-asiento-reverso.md) | Ningún registro se borra: toda anulación o corrección es un asiento reverso con motivo y autor | M-14 y todos | Bloqueo duro | No | NRM-01 |
| [RN-05](RN-05-registro-cerrado-no-se-edita.md) | Un registro cerrado no se edita, y ningún rol operativo modifica autorizaciones ni bitácoras cerradas | M-08, M-13, M-14, M-01 | Bloqueo duro | No | NRM-01 |
| [RN-06](RN-06-transiciones-de-estado-de-la-orden.md) | La Orden de Misión solo transita por los estados definidos, y cada transición registra actor, rol, momento y motivo | M-06, M-07, M-08, M-13, M-14 | Bloqueo duro | No | NRM-01 |
| [RN-07](RN-07-delegacion-de-autorizacion.md) | La delegación de autorización tiene vigencia acotada, consta en el expediente y no rompe la segregación | M-01, M-06, M-20 | Bloqueo duro | Sí | NRM-08, NRM-01, NRM-09 |
| [RN-08](RN-08-cadena-de-trazabilidad-para-cierre.md) | Una Orden de Misión solo se cierra con su cadena de trazabilidad completa; incompleta, se cierra con hallazgo | M-13, M-14 | Bloqueo duro | Sí | NRM-01 |

## Habilitación del motorista

| ID | Enunciado | Módulos | Tipo | Cfg. | Origen |
|---|---|---|---|---|---|
| [RN-09](RN-09-matriz-licencia-vehiculo.md) | La categoría de licencia debe habilitar el tipo, el peso bruto y la capacidad del vehículo asignado | M-05, M-07, M-03 | Bloqueo duro | No† | NRM-06, DP-001 D-12 |
| [RN-10](RN-10-licencia-vigente-en-todo-el-rango.md) | La licencia debe estar vigente durante todo el rango de la misión, no solo el día de salida | M-05, M-07 | Bloqueo duro | No | NRM-06, DP-001 D-12 |
| [RN-11](RN-11-restricciones-medicas-del-motorista.md) | Las restricciones médicas de la licencia deben ser compatibles con las condiciones de la misión | M-05, M-07 | Bloqueo / advertencia | Sí | NRM-06 |
| [RN-12](RN-12-disponibilidad-del-motorista.md) | No se asigna un motorista con permiso, vacaciones o incapacidad vigente según el espejo de Talento Humano | M-05, M-07, M-20 | Bloqueo duro | Sí* | DP-001 D-07, ADR-001 |
| [RN-13](RN-13-sin-doble-asignacion.md) | Un motorista y un vehículo no pueden estar asignados a dos misiones con ventanas traslapadas | M-07, M-03, M-05 | Bloqueo duro | Sí | NRM-01 |
| [RN-14](RN-14-sustitucion-de-motorista.md) | La sustitución de motorista o vehículo revalida todas las habilitaciones y conserva la asignación original | M-07, M-08, M-05, M-03 | Bloqueo duro | No | DP-001 D-07, NRM-06 |

† La matriz es catálogo configurable con vigencia; el **bloqueo** no se puede desactivar.

## Vehículo y documentación

| ID | Enunciado | Módulos | Tipo | Cfg. | Origen |
|---|---|---|---|---|---|
| [RN-15](RN-15-identidad-del-vehiculo-y-placa.md) | La identidad del vehículo es el correlativo institucional; la placa no es obligatoria ni única | M-03, M-04 | Bloqueo + derivación | No | NRM-06, NRM-02 |
| [RN-16](RN-16-seguro-y-revision-mecanica.md) | Póliza y revisión mecánica son rastreables y alertables, con bloqueo configurable **apagado por defecto** | M-04, M-03, M-07 | Advertencia / bloqueo | Sí* | NRM-06, DP-001 D-13 |
| [RN-17](RN-17-alertas-de-vencimiento-documental.md) | Todo documento con fecha de vencimiento genera alerta anticipada según umbrales configurables | M-04, M-05, M-03, M-14 | Advertencia | Sí | NRM-06, NRM-09 |
| [RN-18](RN-18-rotulacion-del-vehiculo-del-estado.md) | La rotulación del vehículo del Estado se verifica con fecha y fotografía, y su constatación caduca | M-03, M-04, M-14 | Advertencia + derivación | Sí | NRM-02 |
| [RN-19](RN-19-vehiculo-no-operativo-no-se-asigna.md) | Un vehículo cuyo estado operativo no es disponible no puede ser asignado ni despachado | M-03, M-07, M-11 | Bloqueo duro | Sí | NRM-02, DP-001 D-08 |
| [RN-20](RN-20-compatibilidad-vehiculo-objeto-del-traslado.md) | El tipo de vehículo asignado debe ser compatible con el objeto del traslado declarado | M-07, M-06, M-02, M-03 | Bloqueo duro | Sí | Premisa 2, NRM-06 |
| [RN-21](RN-21-capacidad-de-pasajeros-y-carga.md) | No se excede la capacidad de pasajeros ni la capacidad de carga de la ficha técnica | M-07, M-06, M-03, M-17 | Bloqueo duro | Sí | NRM-02, NRM-06 |
| [RN-22](RN-22-custodia-del-vehiculo.md) | Todo vehículo tiene custodio vigente, y el despacho traslada la custodia al motorista con constancia | M-03, M-07, M-08, M-15 | Bloqueo duro | No | NRM-02 |

## Día y hora inhábil

| ID | Enunciado | Módulos | Tipo | Cfg. | Origen |
|---|---|---|---|---|---|
| [RN-23](RN-23-permiso-de-circulacion-en-dia-inhabil.md) | Circular en día u hora inhábil requiere permiso vigente firmado por la máxima autoridad | M-07, M-04, M-15 | Bloqueo duro | Sí | NRM-02, NRM-09 |
| [RN-24](RN-24-vehiculo-de-servicio-exceptuado.md) | La excepción de circulación es atributo del vehículo, con fundamento y vigencia registrados | M-03, M-04, M-07 | Derivación | Sí | NRM-02 |
| [RN-25](RN-25-salvoconducto-con-folio-y-qr.md) | El salvoconducto y todo documento de control en carretera se emiten impresos, con folio único y QR verificable | M-15, M-04, M-07 | Bloqueo duro | No | NRM-02, NRM-08, Premisa 4 |

## Combustible

| ID | Enunciado | Módulos | Tipo | Cfg. | Origen |
|---|---|---|---|---|---|
| [RN-26](RN-26-fondo-de-combustible-aprobado.md) | El fondo lo solicita el Jefe de Transporte y lo aprueba Gerencia Administrativa; sin fondo vigente no hay asignación | M-09, M-13 | Bloqueo duro | Sí | DP-001 D-03, PROP-01 |
| [RN-27](RN-27-asignacion-de-combustible-con-folio.md) | Toda asignación tiene folio único, responsable receptor, misión vinculada y constancia de recepción | M-09, M-15 | Bloqueo duro | No | PROP-01, NRM-09 |
| [RN-28](RN-28-comprobacion-del-consumo-de-combustible.md) | Todo consumo se registra con galones, monto, estación, odómetro y fotografía del comprobante | M-09, M-08, M-16 | Bloqueo + advertencia | Sí | PROP-01, NRM-01 |
| [RN-29](RN-29-liquidacion-de-combustible.md) | La liquidación concilia asignado contra consumido contra saldo devuelto, y la diferencia debe quedar explicada | M-13, M-09 | Cálculo + bloqueo | Sí | PROP-01, NRM-01 |
| [RN-30](RN-30-conciliacion-galonaje-kilometraje.md) | El rendimiento galonaje–kilometraje se concilia con desviación detectada **en ambas direcciones** | M-09, M-13, M-14 | Cálculo + advertencia | Sí | NRM-01, NRM-09 |
| [RN-31](RN-31-odometro-de-retorno.md) | El odómetro de retorno no puede ser menor al de salida; todo retroceso o salto exige justificación con respaldo | M-08, M-09, M-03 | Bloqueo duro | Sí | NRM-09, NRM-01 |
| [RN-32](RN-32-entrega-de-combustible-contra-orden-de-mision.md) | No se entrega combustible sin Orden de Misión aprobada, y solo al vehículo y motorista de esa orden | M-09, M-07 | Bloqueo duro | Sí | NRM-01, PROP-01 |

## Peajes

| ID | Enunciado | Módulos | Tipo | Cfg. | Origen |
|---|---|---|---|---|---|
| [RN-33](RN-33-categoria-de-peaje-derivada-de-ficha-tecnica.md) | La categoría de peaje se deriva de la ficha técnica, **no del número de ejes por sí solo** | M-18, M-03, M-02 | Derivación | Sí | NRM-10 |
| [RN-34](RN-34-tarifa-de-peaje-por-punto-categoria-vigencia.md) | La tarifa se resuelve por punto × categoría × vigencia, a la fecha del hecho | M-18, M-02, M-13 | Cálculo | Sí | NRM-10 |
| [RN-35](RN-35-estimacion-de-peajes-antes-de-aprobar.md) | El costo de peajes se estima desglosado por punto antes de aprobar la solicitud | M-18, M-06, M-07 | Cálculo + bloqueo | Sí | NRM-10, DP-001 D-02 |
| [RN-36](RN-36-discrepancia-de-clasificacion-en-caseta.md) | Un cobro en categoría distinta a la asignada se registra como discrepancia y habilita el reclamo | M-18, M-13 | Derivación + advertencia | No | NRM-10 |
| [RN-37](RN-37-coherencia-de-la-secuencia-de-casetas.md) | La secuencia de casetas debe ser geográfica y temporalmente coherente con la ruta autorizada | M-18, M-08, M-13, M-14 | Advertencia con hallazgo | Sí | NRM-10, NRM-01 |
| [RN-38](RN-38-exoneracion-de-peaje.md) | La exoneración es dato por vehículo, punto, fundamento y vigencia; nunca una constante | M-18, M-03 | Derivación | Sí | NRM-10 |

## Parámetros normativos

| ID | Enunciado | Módulos | Tipo | Cfg. | Origen |
|---|---|---|---|---|---|
| [RN-39](RN-39-parametros-normativos-con-vigencia.md) | Ningún dato normativo se escribe en el código: todo es parámetro con vigencia por rango de fechas | M-02 y todos | Bloqueo duro | No | Premisa 6, NRM-06, NRM-09, NRM-10 |
| [RN-40](RN-40-calculo-a-la-fecha-del-hecho.md) | Todo cálculo usa el parámetro vigente a la **fecha del hecho**, no a la de captura ni a la de consulta | M-02, M-18, M-09, M-13, M-07 | Cálculo | No | Premisa 6, NRM-10 |
| [RN-41](RN-41-congelamiento-del-valor-al-autorizar.md) | El valor calculado se congela al autorizar, junto con el identificador de la tabla usada | M-13, M-18, M-09, M-14 | Cálculo + bloqueo | No | NRM-01, NRM-10 |
| [RN-42](RN-42-correccion-retroactiva-con-asiento-de-diferencia.md) | La corrección retroactiva genera asiento de diferencia; nunca sobrescribe el valor histórico | M-02, M-18, M-13, M-14 | Bloqueo + cálculo | No | NRM-10, NRM-01 |

## Operación desconectada

| ID | Enunciado | Módulos | Tipo | Cfg. | Origen |
|---|---|---|---|---|---|
| [RN-43](RN-43-captura-de-campo-sin-conectividad.md) | Toda captura de campo debe completarse sin ninguna conectividad y nunca perderse | M-16, M-08, M-09, M-18, M-12 | Bloqueo duro | No | NRM-09, Premisa 5 |
| [RN-44](RN-44-identificadores-y-folios-en-el-cliente.md) | Los identificadores se generan en el cliente y los folios se asignan de rangos por delegación | M-16, M-15, M-09 | Bloqueo duro | Sí | NRM-09 |
| [RN-45](RN-45-cero-sobrescritura-silenciosa.md) | Ningún conflicto de sincronización se resuelve por sobrescritura: todo va a cola de resolución humana | M-16, M-14 | Bloqueo duro | No | NRM-09, ADR-001 |
| [RN-46](RN-46-fecha-del-hecho-y-fecha-de-captura.md) | Fecha del hecho y fecha de captura son campos distintos, ambos obligatorios; los cálculos usan la del hecho | M-08, M-09, M-16, M-14 | Bloqueo duro | Sí | NRM-01, NRM-09 |
| [RN-47](RN-47-digitacion-diferida-desde-papel.md) | La digitación diferida desde papel deja constancia de quién digitó y del original escaneado | M-16, M-15, M-08, M-09 | Bloqueo duro | No | NRM-09, NRM-01 |

## Integración con ARGOS y Talento Humano

| ID | Enunciado | Módulos | Tipo | Cfg. | Origen |
|---|---|---|---|---|---|
| [RN-48](RN-48-datos-espejo-de-solo-lectura.md) | Los datos de ARGOS y Talento Humano son espejo de solo lectura y no se editan desde SIGTI | M-20, M-01, M-05 | Bloqueo duro | No | DP-001 D-05, ADR-001 |
| [RN-49](RN-49-reconciliacion-periodica-del-espejo.md) | El espejo se reconcilia periódicamente contra el origen y cada entidad muestra su última sincronización | M-20, M-14 | Bloqueo + advertencia | Sí | ADR-001, NRM-01 |
| [RN-50](RN-50-degradacion-por-sincronizacion-detenida.md) | Si la sincronización lleva detenida más del umbral, el sistema degrada explícitamente antes de operar | M-20, M-16, M-07 | Advertencia → bloqueo | Sí | ADR-001 |

## Traslado de personas y carga

| ID | Enunciado | Módulos | Tipo | Cfg. | Origen |
|---|---|---|---|---|---|
| [RN-51](RN-51-minimizacion-de-datos-de-personas-externas.md) | En el traslado de personas externas solo se capturan los datos mínimos del catálogo autorizado | M-17, M-06, M-14 | Bloqueo duro | Sí | NRM-07, DP-001 D-14 |
| [RN-52](RN-52-registro-de-consultas-a-manifiestos.md) | Toda consulta a manifiestos y listas de pasajeros se registra: quién vio qué y cuándo | M-17, M-14, M-01 | Bloqueo duro | No | NRM-07, NRM-01 |
| [RN-53](RN-53-cierre-del-manifiesto-al-despacho.md) | El manifiesto se cierra al despachar; los cambios en ruta se registran como novedad, no como edición | M-17, M-08, M-07, M-13 | Bloqueo + advertencia | No | NRM-02, NRM-06, NRM-01 |

## Presupuesto

| ID | Enunciado | Módulos | Tipo | Cfg. | Origen |
|---|---|---|---|---|---|
| [RN-54](RN-54-cuota-trimestral-de-compromiso.md) | El compromiso de gasto se valida contra la **cuota trimestral**, no solo contra el presupuesto anual | M-09, M-18, M-13, M-20 | Advertencia → bloqueo | Sí | NRM-04, DP-001 D-09, ADR-001 |

Escrita tras el hallazgo `HN1-07`: `NRM-04` deja la cuota trimestral en alcance explícito y **ninguna de las 53 reglas originales la citaba**. El gasto de combustible y peajes no está limitado solo por el presupuesto anual: está limitado por la cuota del trimestre, y un sistema que solo controla contra el anual deja comprometer gasto que la institución no puede ejecutar.

---

## Los cinco bloqueos que no se pueden desactivar

Si alguna vez se propone volverlos configurables, la respuesta está escrita:

1. **[RN-01](RN-01-segregacion-de-funciones.md) Segregación de funciones** — mandato del control interno del Estado ([NRM-01](../normativa/NRM-01-control-interno-tsc.md)).
2. **[RN-09](RN-09-matriz-licencia-vehiculo.md) y [RN-10](RN-10-licencia-vigente-en-todo-el-rango.md) Matriz licencia ↔ vehículo y vigencia** — [DP-001 D-12](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md): *"una excepción registrada sería evidencia en contra ante un siniestro"*.
3. **[RN-04](RN-04-anulacion-como-asiento-reverso.md) Nada se borra** — sin esto, el sistema es el instrumento del hallazgo, no su defensa.
4. **[RN-39](RN-39-parametros-normativos-con-vigencia.md) a [RN-42](RN-42-correccion-retroactiva-con-asiento-de-diferencia.md) Parámetros con vigencia y cálculo a la fecha del hecho** — premisa rectora 6.
5. **[RN-45](RN-45-cero-sobrescritura-silenciosa.md) Cero sobrescritura silenciosa** — [ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md): la divergencia silenciosa *"es la peor forma de fallar"*.

## Lo que estas reglas todavía no pueden fijar

Marcado `[C]` dentro de cada regla. Los bloqueantes de mayor impacto:

| Falta | Reglas que quedan provisionales | Insumo |
|---|---|---|
| Texto de la reforma al Art. 48 de la Ley de Tránsito (2025) | [RN-09](RN-09-matriz-licencia-vehiculo.md) | #20, #23 |
| Texto del Art. 51 de la Ley de Tránsito | [RN-33](RN-33-categoria-de-peaje-derivada-de-ficha-tecnica.md) | #23 |
| Tarifa de peaje efectivamente vigente | [RN-34](RN-34-tarifa-de-peaje-por-punto-categoria-vigencia.md), [RN-35](RN-35-estimacion-de-peajes-antes-de-aprobar.md) | #21 |
| Lista oficial de exoneraciones de peaje | [RN-38](RN-38-exoneracion-de-peaje.md) | #22 |
| Formatos en papel vigentes de la institución | [RN-25](RN-25-salvoconducto-con-folio-y-qr.md), [RN-27](RN-27-asignacion-de-combustible-con-folio.md), [RN-47](RN-47-digitacion-diferida-desde-papel.md) | #2 |
| Legislación de feriados de octubre | [RN-23](RN-23-permiso-de-circulacion-en-dia-inhabil.md) | NRM-09 |
| Horario hábil oficial de la institución | [RN-23](RN-23-permiso-de-circulacion-en-dia-inhabil.md) | NRM-09 |
| Decisiones abiertas de `PROP-01` (fondo por período o por misión, saldo entre misiones, sobrante, folio preimpreso) | [RN-26](RN-26-fondo-de-combustible-aprobado.md), [RN-27](RN-27-asignacion-de-combustible-con-folio.md), [RN-29](RN-29-liquidacion-de-combustible.md) | #7 |
| Contratos de API de ARGOS y Talento Humano | [RN-12](RN-12-disponibilidad-del-motorista.md), [RN-48](RN-48-datos-espejo-de-solo-lectura.md), [RN-49](RN-49-reconciliacion-periodica-del-espejo.md), [RN-50](RN-50-degradacion-por-sincronizacion-detenida.md) | #16, #17 |

Registro completo en [`docs/07-gestion/insumos-pendientes.md`](../../07-gestion/insumos-pendientes.md).
