# Reglas de negocio — SIGTI

Una regla de negocio es una afirmación **verificable**: se puede escribir una prueba que determine si el sistema la cumple. Si no se puede probar, no es una regla — es un principio de diseño y va en otro lado.

**Un archivo por regla.** Los IDs son estables y **nunca se reciclan**: si una regla se descarta, su ID queda marcado como obsoleto pero no se reutiliza.

Plantilla: [`docs/plantillas/regla-de-negocio.md`](../../plantillas/regla-de-negocio.md).

## Cómo leer la tabla

| Columna | Significado |
|---|---|
| **Tipo** | `Bloqueo duro` impide la operación · `Advertencia` deja continuar con acuse registrado · `Cálculo` produce un valor · `Derivación` resuelve un atributo a partir de otros |
| **Cfg.** | Qué se puede configurar. **`No`** = nada; ni el bloqueo ni su alcance se desactivan. **`Sí`** = hay parámetro, catálogo o umbral configurable, **pero el bloqueo en sí no se apaga** — es el valor de la mayoría de las filas. **`Sí*`** = **el bloqueo mismo es configurable** y se puede dejar apagado. Hoy son **dos, con el valor por defecto opuesto**: [RN-16](RN-16-seguro-y-revision-mecanica.md) nace **apagado** —póliza y revisión no son obligatorias por ley `[V]`— y [RN-103](RN-103-matricula-vigente-para-despachar.md) nace **encendido** —la matrícula sí es un trámite que la institución puede resolver, aunque el bloqueo sea `[I]`— |
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
| [RN-73](RN-73-convalidacion-de-actos-sin-autorizacion-previa.md) | El acto ejecutado sin autorización previa se convalida en plazo, y la cronología se declara tal como ocurrió | M-06, M-07, M-08, M-14, M-15 | Bloqueo + hallazgo | Sí | NRM-01, CE-01 |
| [RN-74](RN-74-sin-atribucion-de-responsabilidad-en-campo.md) | El registro de campo no captura atribución de responsabilidad; se determina en el expediente | M-12, M-08, M-16, M-17 | Bloqueo duro | No | NRM-01, NRM-07, CE-03 |
| [RN-100](RN-100-permisos-por-puesto-no-por-persona.md) | Los permisos se conceden al **puesto**, nunca a la persona; la autoría histórica es de la persona y no se reasigna | M-01, M-14, M-03, M-07 | Bloqueo duro | No | NRM-09, `HN1-18` |
| [RN-101](RN-101-cierre-de-asignacion-de-puesto.md) | Una asignación de puesto **no se cierra con custodias físicas activas**; lo demás pasa al puesto | M-01, M-03, M-09, M-13, M-14 | Bloqueo duro | No | NRM-02, NRM-09, `HN1-18` |

## Habilitación del motorista

| ID | Enunciado | Módulos | Tipo | Cfg. | Origen |
|---|---|---|---|---|---|
| [RN-09](RN-09-matriz-licencia-vehiculo.md) | La categoría de licencia debe habilitar el tipo, el peso bruto y la capacidad del vehículo asignado | M-05, M-07, M-03 | Bloqueo duro | No† | NRM-06, DP-001 D-12 |
| [RN-10](RN-10-licencia-vigente-en-todo-el-rango.md) | La licencia debe estar vigente durante todo el rango de la misión, no solo el día de salida | M-05, M-07 | Bloqueo duro | No | NRM-06, DP-001 D-12 |
| [RN-11](RN-11-restricciones-medicas-del-motorista.md) | Las restricciones médicas de la licencia deben ser compatibles con las condiciones de la misión | M-05, M-07 | Bloqueo / advertencia | Sí | NRM-06 |
| [RN-12](RN-12-disponibilidad-del-motorista.md) | No se asigna un motorista con permiso, vacaciones o incapacidad vigente según el espejo de Talento Humano | M-05, M-07, M-20 | Bloqueo duro | Sí | DP-001 D-07, ADR-001 |
| [RN-13](RN-13-sin-doble-asignacion.md) | Un motorista y un vehículo no pueden estar asignados a dos misiones con ventanas traslapadas | M-07, M-03, M-05 | Bloqueo duro | Sí | NRM-01 |
| [RN-14](RN-14-sustitucion-de-motorista.md) | La sustitución de motorista o vehículo revalida todas las habilitaciones y conserva la asignación original | M-07, M-08, M-05, M-03 | Bloqueo duro | No | DP-001 D-07, NRM-06 |
| [RN-55](RN-55-habilitacion-vencida-durante-la-mision.md) | La habilitación que vence con la misión en ruta no detiene la ejecución, pero cierra el expediente con hallazgo | M-05, M-08, M-07, M-13 | Bloqueo duro | No | NRM-06, CE-11 |
| [RN-57](RN-57-habilitacion-de-quien-efectivamente-conduce.md) | La habilitación se verifica sobre quien efectivamente conduce, cualquiera sea su puesto | M-05, M-07, M-08, M-03 | Bloqueo duro | No | NRM-06, CE-19 |

† La matriz es catálogo configurable con vigencia; el **bloqueo** no se puede desactivar.

## Programación y recursos escasos

| ID | Enunciado | Módulos | Tipo | Cfg. | Origen |
|---|---|---|---|---|---|
| [RN-56](RN-56-prelacion-entre-solicitudes-que-compiten.md) | La adjudicación de un recurso escaso aplica el criterio de prelación parametrizado y deja constancia de las desplazadas | M-07, M-06, M-09, M-14 | Derivación + bloqueo | Sí | NRM-01, CE-12 |

`RN-56` fusiona las dos candidatas de [CE-12](../../02-requisitos/casos-especiales/CE-12-dos-solicitudes-compiten-por-el-mismo-vehiculo.md) —prelación y constancia de desplazamiento— y la del dinero de [CE-23](../../02-requisitos/casos-especiales/CE-23-fondo-agotado-con-misiones-programadas.md). El `RN-57` que `CE-12` proponía **no ocupa ese número**: su contenido vive dentro de `RN-56`.

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
| [RN-60](RN-60-indisponibilidad-sobrevenida-y-reservas.md) | La indisponibilidad sobrevenida exige causa, ventana estimada y desenlace explícito de cada reserva afectada | M-03, M-11, M-07, M-14 | Bloqueo duro | Sí | NRM-02, CE-16 |
| [RN-61](RN-61-sustitucion-de-vehiculo-recalcula-valores-congelados.md) | La sustitución de vehículo recalcula y vuelve a congelar todo valor derivado, con asiento de diferencia | M-07, M-18, M-09, M-15, M-13 | Bloqueo + cálculo | No | NRM-10, CE-16 |
| [RN-64](RN-64-estado-de-la-placa-tipificado.md) | El estado de la placa es dato tipificado con historial y vigencia, distinto del número asignado | M-03, M-04, M-14 | Bloqueo + derivación | Sí | NRM-02, NRM-06, CE-17 |
| [RN-65](RN-65-sin-lamina-respaldo-y-paquete-de-identificacion.md) | Sin lámina: respaldo vigente en todo el rango y paquete de identificación impreso y acusado | M-04, M-15, M-07, M-03 | Bloqueo duro | Sí | NRM-02, NRM-06, CE-17 |
| [RN-66](RN-66-imputacion-externa-por-jerarquia-de-anclas.md) | Toda imputación externa se resuelve por jerarquía de anclas a la fecha del hecho, con la placa en último lugar | M-12, M-18, M-03, M-14, M-09 | Derivación + bloqueo | Sí | NRM-01, CE-17 |
| [RN-89](RN-89-kilometraje-acumulado-invariante-del-expediente.md) | El kilometraje acumulado es atributo del expediente del vehículo, independiente de la lectura del instrumento | M-03, M-08, M-11, M-09 | Bloqueo + derivación | No | NRM-01, CE-22 |
| [RN-90](RN-90-intervencion-del-instrumento-de-medicion.md) | Toda intervención del odómetro es evento con orden de trabajo y autorización nominativa | M-03, M-11, M-08, M-07 | Bloqueo duro | Sí | NRM-01, CE-22 |
| [RN-99](RN-99-constatacion-fisica-de-la-flota.md) | La flota se **constata físicamente** con acta y comisión, y se concilia contra el registro de bienes | M-03, M-14, M-04, M-16 | Capacidad con efecto en estado operativo | Sí | NRM-01, NRM-02, `HN1-18` |
| [RN-103](RN-103-matricula-vigente-para-despachar.md) | La **matrícula** debe estar vigente durante todo el rango de la misión, no solo el día de salida | M-04, M-03, M-07 | Bloqueo duro | Sí* | NRM-06, `HN1-11` |

## Régimen de uso, tenencia y préstamo

| ID | Enunciado | Módulos | Tipo | Cfg. | Origen |
|---|---|---|---|---|---|
| [RN-58](RN-58-regimen-de-uso-del-vehiculo.md) | El régimen de uso es atributo del vehículo, con acto, beneficiario y vigencia acotada | M-03, M-07, M-04, M-14 | Bloqueo + derivación | Sí | NRM-02, CE-19 |
| [RN-59](RN-59-todo-uso-se-ampara-en-orden-de-mision.md) | Todo uso de un vehículo del Estado se ampara en una Orden de Misión, cualquiera sea su régimen | M-06, M-07, M-08, M-03, M-09 | Bloqueo duro | No | NRM-02, CE-19 |
| [RN-62](RN-62-titulo-de-tenencia-con-vigencia-y-rubros.md) | Todo vehículo tiene título de tenencia con régimen, vigencia y rubros; ninguna misión excede esa vigencia | M-03, M-04, M-11, M-13, M-07 | Bloqueo duro | Sí | NRM-02, CE-15 |
| [RN-63](RN-63-prestamo-de-vehiculo-como-expediente-del-bien.md) | El préstamo es expediente del bien con receptor, fecha comprometida y actas; nunca una Orden de Misión | M-03, M-04, M-12, M-14 | Bloqueo duro | Sí | NRM-02, CE-14 |

## Objeto del traslado y carga

| ID | Enunciado | Módulos | Tipo | Cfg. | Origen |
|---|---|---|---|---|---|
| [RN-67](RN-67-matriz-de-compatibilidad-objeto-objeto.md) | Existe matriz de compatibilidad objeto × objeto, evaluada par a par; la ausencia de entrada bloquea | M-06, M-07, M-02, M-17 | Bloqueo duro | Sí | NRM-06, CE-18 |
| [RN-68](RN-68-compatibilidad-y-capacidad-por-tramo.md) | Compatibilidad y capacidad se evalúan por tramo, sobre la configuración real de cada tramo | M-06, M-07, M-08, M-17 | Bloqueo duro | Sí | NRM-06, CE-18 |
| [RN-69](RN-69-inventario-de-la-carga-y-acta-de-entrega.md) | La carga se declara con inventario, se entrega con acta, y toda diferencia se declara como faltante | M-06, M-08, M-12, M-17, M-15 | Bloqueo duro | Sí | NRM-06, NRM-02, CE-04 |

## Ejecución en ruta

| ID | Enunciado | Módulos | Tipo | Cfg. | Origen |
|---|---|---|---|---|---|
| [RN-70](RN-70-interrupcion-en-ruta-con-desenlace-obligatorio.md) | La interrupción en ruta es evento tipificado que marca la misión sin cambiarle el estado y exige desenlace | M-08, M-12, M-16, M-03, M-07 | Bloqueo duro | Sí | NRM-09, CE-02 |
| [RN-71](RN-71-traspaso-en-ruta-con-acta-y-corte-de-odometro.md) | Todo traspaso en ruta consta en acta con odómetro; ese odómetro es el corte de imputación | M-08, M-09, M-05, M-03, M-15 | Bloqueo duro | No | NRM-02, NRM-01, CE-05 |
| [RN-72](RN-72-imputacion-por-tramo-de-vehiculo-y-motorista.md) | Con más de un vehículo o conductor, kilometraje, combustible y peajes se imputan por tramo | M-13, M-09, M-18, M-08, M-14 | Cálculo + bloqueo | No | NRM-01, CE-02 |
| [RN-75](RN-75-bien-retenido-o-sustraido-no-sale-del-registro.md) | El bien retenido, sustraído o no recuperado permanece en el registro hasta su recuperación o descargo | M-03, M-12, M-15, M-14 | Bloqueo duro | Sí | NRM-02, CE-04 |
| [RN-76](RN-76-estado-en-ruta-declarado-por-el-motorista.md) | El estado en ruta lo declara el motorista, nunca se infiere, y la espera improductiva se tipifica y atribuye | M-19, M-08, M-16, M-14 | Bloqueo + derivación | Sí | NRM-09, CE-08 |
| [RN-77](RN-77-versionado-del-alcance-autorizado.md) | Cada extensión produce una versión del alcance autorizado; toda validación usa la vigente a la fecha del hecho | M-07, M-08, M-13, M-18, M-14 | Bloqueo + derivación | Sí | NRM-01, CE-06 |
| [RN-78](RN-78-grado-de-cumplimiento-del-objeto.md) | Toda misión cierra declarando el grado de cumplimiento de su objeto, por destino y consolidado | M-13, M-08, M-14 | Bloqueo duro | Sí | NRM-01, CE-07 |
| [RN-79](RN-79-el-retorno-constatado-libera-al-vehiculo.md) | El retorno físico constatado libera vehículo y motorista sin esperar la digitación de la bitácora | M-08, M-07, M-03, M-16, M-13 | Bloqueo + derivación | Sí | NRM-09, CE-09 |

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
| [RN-83](RN-83-todo-ingreso-de-combustible-es-un-abastecimiento.md) | Todo ingreso de combustible se registra como abastecimiento con fuente declarada; el nivel de tanque es dato de bitácora | M-09, M-08, M-13, M-16 | Bloqueo duro | Sí | NRM-01, CE-21 |
| [RN-84](RN-84-unicidad-del-comprobante-en-la-institucion.md) | Todo comprobante es único en la institución por emisor y número; su reutilización se bloquea al registrarlo | M-09, M-18, M-13, M-14, M-16 | Bloqueo duro | No | NRM-01, CE-28 |
| [RN-85](RN-85-ausencia-de-comprobante-causa-y-descargo-alternativo.md) | La ausencia de comprobante lleva causa tipificada y suficiencia probatoria, y admite descargo alternativo con folio | M-09, M-13, M-15, M-18 | Bloqueo + derivación | Sí | NRM-01, NRM-03, CE-25 |
| [RN-86](RN-86-obligacion-de-reintegro-por-saldo-no-devuelto.md) | El saldo no devuelto es obligación de reintegro con responsable y ciclo propio que sobrevive al cierre | M-09, M-13, M-12, M-14 | Bloqueo duro | Sí | NRM-01, CE-26 |
| [RN-87](RN-87-gasto-imprevisto-en-ruta.md) | El gasto imprevisto en ruta distinto de combustible se registra con tipo, factura y autorización del acto | M-09, M-13, M-11, M-08 | Bloqueo duro | Sí | NRM-01, CE-26 |
| [RN-88](RN-88-saldo-proyectado-del-fondo.md) | El saldo del fondo se presenta con el comprometido proyectado, y la alerta se dispara sobre el proyectado | M-09, M-18, M-13, M-20 | Cálculo + advertencia | Sí | NRM-01, NRM-04, CE-23 |

## Peajes

| ID | Enunciado | Módulos | Tipo | Cfg. | Origen |
|---|---|---|---|---|---|
| [RN-33](RN-33-categoria-de-peaje-derivada-de-ficha-tecnica.md) | La categoría de peaje se deriva de la ficha técnica, **no del número de ejes por sí solo** | M-18, M-03, M-02 | Derivación | Sí | NRM-10 |
| [RN-34](RN-34-tarifa-de-peaje-por-punto-categoria-vigencia.md) | La tarifa se resuelve por punto × categoría × vigencia, a la fecha del hecho | M-18, M-02, M-13 | Cálculo | Sí | NRM-10 |
| [RN-35](RN-35-estimacion-de-peajes-antes-de-aprobar.md) | El costo de peajes se estima desglosado por punto antes de aprobar la solicitud | M-18, M-06, M-07 | Cálculo + bloqueo | Sí | NRM-10, DP-001 D-02 |
| [RN-36](RN-36-discrepancia-de-clasificacion-en-caseta.md) | Un cobro en categoría distinta a la asignada se registra como discrepancia y habilita el reclamo | M-18, M-13 | Derivación + advertencia | No | NRM-10 |
| [RN-37](RN-37-coherencia-de-la-secuencia-de-casetas.md) | La secuencia de casetas debe ser geográfica y temporalmente coherente con la ruta autorizada | M-18, M-08, M-13, M-14 | Advertencia con hallazgo | Sí | NRM-10, NRM-01 |
| [RN-38](RN-38-exoneracion-de-peaje.md) | La exoneración es dato por vehículo, punto, fundamento y vigencia; nunca una constante | M-18, M-03 | Derivación | Sí | NRM-10 |
| [RN-91](RN-91-categoria-y-tarifa-de-peaje-impresas-en-la-orden.md) | La Orden de Misión impresa lleva, por punto, la categoría asignada y la tarifa esperada del paquete congelado | M-18, M-15, M-07, M-08 | Bloqueo duro | No† | NRM-10, CE-24 |
| [RN-92](RN-92-reclamo-por-discrepancia-de-peaje.md) | El reclamo por discrepancia es objeto con estado y resultado económico; las discrepancias no cierran sin él | M-18, M-13, M-14 | Bloqueo duro | Sí | NRM-10, CE-24 |

† No la obligación de imprimir; sí la plantilla del formato.

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
| [RN-80](RN-80-hoja-de-bitacora-impresa-con-folio.md) | El despacho emite la hoja de bitácora en papel, con folio, QR y paridad exacta con la pantalla de digitación | M-15, M-08, M-16, M-07 | Bloqueo duro | Sí | NRM-09, CE-09 |

## Integración con ARGOS y Talento Humano

| ID | Enunciado | Módulos | Tipo | Cfg. | Origen |
|---|---|---|---|---|---|
| [RN-48](RN-48-datos-espejo-de-solo-lectura.md) | Los datos de ARGOS y Talento Humano son espejo de solo lectura y no se editan desde SIGTI | M-20, M-01, M-05 | Bloqueo duro | No | DP-001 D-05, ADR-001 |
| [RN-49](RN-49-reconciliacion-periodica-del-espejo.md) | El espejo se reconcilia periódicamente contra el origen y cada entidad muestra su última sincronización | M-20, M-14 | Bloqueo + advertencia | Sí | ADR-001, NRM-01 |
| [RN-50](RN-50-degradacion-por-sincronizacion-detenida.md) | Si la sincronización lleva detenida más del umbral, el sistema degrada explícitamente antes de operar | M-20, M-16, M-07 | Advertencia → bloqueo | Sí | ADR-001 |
| [RN-81](RN-81-sigti-expone-hechos-a-argos.md) | SIGTI expone a ARGOS los hechos con la clave de vinculación de la orden, y no escribe en el sistema origen | M-20, M-13, M-09, M-18 | Bloqueo duro | Sí | DP-001 D-01/D-05, CE-20 |

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

## Cierre de ejercicio, hallazgo posterior y reportes

| ID | Enunciado | Módulos | Tipo | Cfg. | Origen |
|---|---|---|---|---|---|
| [RN-82](RN-82-indicadores-de-calidad-de-la-programacion.md) | Los indicadores de calidad de la programación se acumulan por causa tipificada y se atribuyen al responsable | M-14, M-07, M-08, M-13 | Derivación | Sí | NRM-01, CE-06/07/08/09 |
| [RN-93](RN-93-expediente-de-hallazgo-posterior.md) | El hallazgo posterior es expediente con ciclo propio; no altera el estado ni los datos del objeto vinculado | M-14, M-12, M-13 | Bloqueo duro | Sí | NRM-01, CE-28 |
| [RN-94](RN-94-fecha-de-corte-de-conocimiento-en-todo-reporte.md) | Todo reporte declara su fecha de corte de conocimiento y es reproducible a esa fecha | M-14, M-13, M-09, M-18, M-03 | Bloqueo duro | No | NRM-01, CE-28 |
| [RN-95](RN-95-conciliacion-contra-fuentes-externas.md) | El sistema concilia periódicamente contra fuentes externas, y cada diferencia abre expediente de hallazgo posterior | M-14, M-09, M-18, M-12, M-20 | Bloqueo duro | Sí | NRM-01, CE-28 |
| [RN-96](RN-96-cierre-de-ejercicio-como-corte-de-imputacion.md) | El cierre de ejercicio es corte de imputación y de reporte; ningún expediente cambia de estado por una fecha | M-13, M-09, M-18, M-14, M-20 | Bloqueo duro | Sí | NRM-04, CE-27 |
| [RN-97](RN-97-saldo-de-apertura-de-control-interno.md) | Lo no terminal al corte constituye el saldo de apertura del ejercicio siguiente, con antigüedad desde el hecho | M-14, M-13, M-12, M-03 | Bloqueo duro | No | NRM-01, CE-27 |
| [RN-98](RN-98-paquete-de-evidencia-por-vehiculo-o-periodo.md) | La evidencia de auditoría se entrega también **por vehículo y por período**, no solo por misión | M-14, M-03, M-13, M-09, M-18, M-15 | Capacidad obligatoria | Sí | NRM-01, `HN1-09` |
| [RN-102](RN-102-reporte-publico-de-flota.md) | El **reporte público de flota** se produce sin depuración manual, agregado o anonimizado | M-14, M-17, M-03 | Capacidad obligatoria | Sí | NRM-07, DP-001 D-14, `HN1-18` |

---

## Qué cubren estas reglas y qué está deliberadamente diferido

Incorporado al corregir el hallazgo `HN1-18` de [H-B1-002](../../05-calidad/hallazgos/H-B1-002-revision-normativa-bloque-1.md). El problema lo dijo el propio hallazgo:

> *«El `README.md` de reglas no declara qué módulos cubre, así que no hay forma de distinguir un hueco de una postergación — y esa distinción es la que evita que un hueco sobreviva tres bloques.»*

**Un hueco no declarado es indistinguible de una decisión.** Por eso lo que falta se dice aquí, con su motivo.

### Cobertura real por módulo

Los diecinueve módulos vigentes aparecen citados por alguna regla. **Citado no es gobernado**, y la diferencia importa:

| Situación | Módulos |
|---|---|
| **Gobernados** — tienen reglas cuyo módulo principal es ése | M-01, M-02, M-03, M-04, M-05, M-06, M-07, M-08, M-09, M-13, M-14, M-15, M-16, M-17, M-18, M-20 |
| **Tocados de refilón** — otras reglas los citan como módulo secundario, pero casi ninguna es suya | **M-11** (solo desde M-03 y M-09: indisponibilidad, orden de trabajo, intervención del odómetro) · **M-12** (dos reglas propias: [`RN-66`](RN-66-imputacion-externa-por-jerarquia-de-anclas.md) y [`RN-74`](RN-74-sin-atribucion-de-responsabilidad-en-campo.md); el resto lo cita de paso) · **M-19** (una sola regla propia: [`RN-76`](RN-76-estado-en-ruta-declarado-por-el-motorista.md)) |

### Materias diferidas a un bloque posterior

No son huecos. Pertenecen a módulos que todavía no se han trabajado, y se listan para que nadie las busque creyendo que se olvidaron.

| Materia | Módulo | Estado |
|---|---|---|
| **Infracciones y multas de tránsito** asociadas a vehículo y motorista | M-12 | La ficha [NRM-06](../normativa/NRM-06-transito-y-licencias.md) lo exige. La regla se escribe con M-12, no antes |
| **Pérdida, robo o siniestro** con denuncia y deducción de responsabilidad | M-12 | [`RN-75`](RN-75-bien-retenido-o-sustraido-no-sale-del-registro.md) cubre que el bien no salga del registro — es una parte, no el circuito |
| **Mantenimiento preventivo y correctivo**, llantas, repuestos, órdenes de trabajo | M-11 | Las reglas existentes solo tratan su **efecto** sobre la disponibilidad del vehículo, no el taller |

### Materias bloqueadas por un insumo pendiente

Éstas sí se escribirían hoy si hubiera con qué. **Falta el dato de la institución, no la decisión.**

| Materia | Ficha | Insumo que la bloquea |
|---|---|---|
| **TAG prepago** como instrumento institucional con ciclo de vida propio | [NRM-10](../normativa/NRM-10-peajes.md) | **#24** — si la institución usa TAG, con qué modalidad y quién lo administra. Aparece en `EF-04` de la [máquina de estados](../../03-arquitectura/estados/orden-de-mision.md) y en casos límite de [`RN-35`](RN-35-estimacion-de-peajes-antes-de-aprobar.md) y [`RN-36`](RN-36-discrepancia-de-clasificacion-en-caseta.md); **ninguna regla lo modela** |

---

## De dónde salieron `RN-55` a `RN-97`

Los 28 casos especiales del Bloque 2 detectaron **127 reglas candidatas** con tres nomenclaturas distintas (`RN-Cnn<letra>`, `RN-c:slug`, y las numeradas de `CE-11` y `CE-12`). Ninguna se dio por escrita. La consolidación del Bloque 3 las fusionó por materia y descartó las que no son reglas verificables —requisitos de interfaz, decisiones de arquitectura, catálogos disfrazados de regla— o que ya estaban cubiertas por las 54 vigentes.

Cuatro huecos estructurales que estas reglas cierran:

| Hueco | Lo cubren |
|---|---|
| Para el modelo anterior **todos los vehículos eran de pool**, y `RN-09`/`RN-10` se redactaban alrededor del *motorista*: el funcionario que conduce su vehículo asignado no lo alcanzaba ninguna verificación | [RN-57](RN-57-habilitacion-de-quien-efectivamente-conduce.md), [RN-58](RN-58-regimen-de-uso-del-vehiculo.md), [RN-59](RN-59-todo-uso-se-ampara-en-orden-de-mision.md) |
| `RN-20` solo cruzaba vehículo × objeto: *personas junto a bidones de combustible* —el ejemplo que la propia `RN-20` usa— no se podía expresar | [RN-67](RN-67-matriz-de-compatibilidad-objeto-objeto.md), [RN-68](RN-68-compatibilidad-y-capacidad-por-tramo.md) |
| `RN-19` gobierna el acto de asignar; **nada gobernaba el efecto del cambio de estado del vehículo sobre reservas ya constituidas** | [RN-60](RN-60-indisponibilidad-sobrevenida-y-reservas.md), [RN-61](RN-61-sustitucion-de-vehiculo-recalcula-valores-congelados.md) |
| Sin placa, nada resolvía contra qué se imputan peajes, multas y siniestros. **El mundo exterior indexa por placa** | [RN-64](RN-64-estado-de-la-placa-tipificado.md), [RN-65](RN-65-sin-lamina-respaldo-y-paquete-de-identificacion.md), [RN-66](RN-66-imputacion-externa-por-jerarquia-de-anclas.md) |

### Reglas existentes corregidas en la consolidación

Ocho candidatas no produjeron regla nueva porque **solo matizaban una regla vigente**. Se corrigió la vigente:

| Regla | Qué se corrigió | De qué caso |
|---|---|---|
| [RN-10](RN-10-licencia-vigente-en-todo-el-rango.md) | Todo cambio de ventana revalida, no solo la extensión; verificación bajada de `[V]` a `[P]`/`[I]` por la regla de no escalar el nivel | CE-06, CE-11, CE-19 |
| [RN-16](RN-16-seguro-y-revision-mecanica.md) | El bloqueo por póliza vencida admite valor distinto **por régimen de tenencia** | CE-15 |
| [RN-17](RN-17-alertas-de-vencimiento-documental.md) | Umbrales también **en kilómetros**, proyectados sobre la ventana de las misiones programadas | CE-16 |
| [RN-18](RN-18-rotulacion-del-vehiculo-del-estado.md) | Umbral de caducidad más corto sin lámina; el resguardo sin evidencia figura **no confirmado** | CE-17, CE-19 |
| [RN-21](RN-21-capacidad-de-pasajeros-y-carga.md) | Peso y ocupación **efectivos** con indicador de desviación; orden de reducción por objeto principal | CE-18 |
| [RN-30](RN-30-conciliacion-galonaje-kilometraje.md) | Motor encendido como variable; kilometraje bajo tenencia ajena excluido; todo abastecimiento en el numerador | CE-08, CE-14, CE-21 |
| [RN-37](RN-37-coherencia-de-la-secuencia-de-casetas.md) | Se evalúa contra el **alcance vigente a la fecha del hecho**; el reordenamiento justificado no es desviación | CE-06, CE-08 |
| [RN-51](RN-51-minimizacion-de-datos-de-personas-externas.md) | Alcance extendido a terceros de siniestro y al dato de salud del servidor | CE-03, CE-10 |

### Notas de hallazgo abiertas

No se corrigen aquí porque el artefacto autoridad está fuera de esta carpeta:

- **`T-17` no cubre el cambio de vehículo con la misión `EN_RUTA`** — [RN-61](RN-61-sustitucion-de-vehiculo-recalcula-valores-congelados.md), [RN-70](RN-70-interrupcion-en-ruta-con-desenlace-obligatorio.md) describen el recálculo, no habilitan la transición.
- **Falta el estado terminal `RETIRADO_DE_FLOTA`** — [RN-62](RN-62-titulo-de-tenencia-con-vigencia-y-rubros.md), [RN-75](RN-75-bien-retenido-o-sustraido-no-sale-del-registro.md). Declarar *dado de baja* un bien ajeno o solo robado es un asiento falso.
- **`BD-04` vs. `PC-03`**: el salvoconducto ampara *vehículo y ventana* o *vehículo, motorista y ventana* según el documento — [RN-71](RN-71-traspaso-en-ruta-con-acta-y-corte-de-odometro.md) sigue a la máquina de estados y reporta contra `PR-01`.
- **Incompatibilidad propuesta a [`actores-y-roles.md`](../actores-y-roles.md)**: quien autoriza un préstamo no puede ser el receptor — [RN-63](RN-63-prestamo-de-vehiculo-como-expediente-del-bien.md).
- **Alcance de datos temporal por préstamo** — materia de [`actores-y-roles.md`](../actores-y-roles.md), no se resolvió como regla.
- **`[C]` ¿Puede digitar quien después liquida?** — [RN-80](RN-80-hoja-de-bitacora-impresa-con-folio.md), insumo #27, pregunta abierta a Auditoría Interna.

---

## Los bloqueos que no se pueden desactivar

Si alguna vez se propone volverlos configurables, la respuesta está escrita:

1. **[RN-01](RN-01-segregacion-de-funciones.md) Segregación de funciones** — mandato del control interno del Estado ([NRM-01](../normativa/NRM-01-control-interno-tsc.md)).
2. **[RN-09](RN-09-matriz-licencia-vehiculo.md) y [RN-10](RN-10-licencia-vigente-en-todo-el-rango.md) Matriz licencia ↔ vehículo y vigencia** — [DP-001 D-12](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md): *"una excepción registrada sería evidencia en contra ante un siniestro"*.
3. **[RN-04](RN-04-anulacion-como-asiento-reverso.md) Nada se borra** — sin esto, el sistema es el instrumento del hallazgo, no su defensa.
4. **[RN-39](RN-39-parametros-normativos-con-vigencia.md) a [RN-42](RN-42-correccion-retroactiva-con-asiento-de-diferencia.md) Parámetros con vigencia y cálculo a la fecha del hecho** — premisa rectora 6.
5. **[RN-45](RN-45-cero-sobrescritura-silenciosa.md) Cero sobrescritura silenciosa** — [ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md): la divergencia silenciosa *"es la peor forma de fallar"*.
6. **[RN-57](RN-57-habilitacion-de-quien-efectivamente-conduce.md) La habilitación se verifica sobre quien conduce** — es la extensión de `RN-09` y `RN-10` a quien no es motorista de padrón. Si admitiera excepción, la excepción sería siempre la misma persona: el funcionario que conduce su vehículo asignado.
7. **[RN-84](RN-84-unicidad-del-comprobante-en-la-institucion.md) Unicidad del comprobante** — el control barato se ejecuta al registrar; el caro, ocho meses después.
8. **[RN-94](RN-94-fecha-de-corte-de-conocimiento-en-todo-reporte.md) Fecha de corte de conocimiento** — sin ella, no reabrir el expediente terminal no sirve de nada: el reporte cambia igual.

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
| Contratos de API de ARGOS y Talento Humano | [RN-12](RN-12-disponibilidad-del-motorista.md), [RN-48](RN-48-datos-espejo-de-solo-lectura.md), [RN-49](RN-49-reconciliacion-periodica-del-espejo.md), [RN-50](RN-50-degradacion-por-sincronizacion-detenida.md), [RN-81](RN-81-sigti-expone-hechos-a-argos.md) | #16, #17 |
| **Criterio de prelación entre solicitudes y entre misiones por financiar** | [RN-56](RN-56-prelacion-entre-solicitudes-que-compiten.md), [RN-88](RN-88-saldo-proyectado-del-fondo.md) | #31 |
| **Quién convalida un acto de emergencia y en qué plazo; quién puede ordenar el retorno anticipado** | [RN-73](RN-73-convalidacion-de-actos-sin-autorizacion-previa.md), [RN-78](RN-78-grado-de-cumplimiento-del-objeto.md) | #32, #50 |
| **Límite de jornada de conducción** | [RN-71](RN-71-traspaso-en-ruta-con-acta-y-corte-de-odometro.md) | #48 |
| **Magnitud de la extensión que escala el nivel autorizante** | [RN-77](RN-77-versionado-del-alcance-autorizado.md) | #49 |
| **Ventana de atención del destino y costo-hora de vehículo** | [RN-76](RN-76-estado-en-ruta-declarado-por-el-motorista.md), [RN-82](RN-82-indicadores-de-calidad-de-la-programacion.md) | #51 |
| **Reintegro de combustible pagado de peculio propio y plazo de devolución del saldo** | [RN-83](RN-83-todo-ingreso-de-combustible-es-un-abastecimiento.md), [RN-86](RN-86-obligacion-de-reintegro-por-saldo-no-devuelto.md) | #7, #37 |
| **¿Se admite constancia como descargo? ¿Con qué tope y qué umbral de hallazgo?** | [RN-85](RN-85-ausencia-de-comprobante-causa-y-descargo-alternativo.md) | #1, Auditoría Interna |
| **Plazo máximo de operación con odómetro averiado** | [RN-90](RN-90-intervencion-del-instrumento-de-medicion.md) | nuevo |
| **Responsabilidad patrimonial por el bien sustraído bajo custodia de misión** | [RN-74](RN-74-sin-atribucion-de-responsabilidad-en-campo.md), [RN-75](RN-75-bien-retenido-o-sustraido-no-sale-del-registro.md) | #47 |
| **Régimen aplicable a vehículos en comodato o alquilados: rotulación e identificación** | [RN-62](RN-62-titulo-de-tenencia-con-vigencia-y-rubros.md), [RN-64](RN-64-estado-de-la-placa-tipificado.md) | NRM-02 |
| **Traslado de personas bajo custodia o menores** | [RN-67](RN-67-matriz-de-compatibilidad-objeto-objeto.md) | #39 |
| **Criterio de imputación entre ejercicios fiscales y ventana de apertura** | [RN-96](RN-96-cierre-de-ejercicio-como-corte-de-imputacion.md) | NRM-04 |
| **Contratos con proveedores de combustible y de peaje para conciliar** | [RN-95](RN-95-conciliacion-contra-fuentes-externas.md) | **insumo nuevo a registrar** |

Registro completo en [`docs/07-gestion/insumos-pendientes.md`](../../07-gestion/insumos-pendientes.md).
