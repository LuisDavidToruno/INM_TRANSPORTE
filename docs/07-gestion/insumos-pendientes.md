# Insumos pendientes de la institución piloto

Documentos y datos que se necesitan para que el análisis se apoye en la realidad y no en suposiciones. **Los bloqueantes no se suplen con inferencias**: mientras falten, el módulo correspondiente queda con parámetros abiertos marcados `[C]`.

**Actualizado 2026-08-06** tras la revisión del PO. Ver [DP-001](decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md).

**Actualizado 2026-08-24** tras una ronda de investigación pública sobre los insumos que **no dependen de la institución piloto** — #20, #21, #22, #23, #42, #43 y los códigos presupuestarios. Ninguno se cierra por completo; seis se reducen o se reencuadran, y se abren los insumos #79 a #82. Ver la nota metodológica de [riesgos-normativos.md](../01-negocio/normativa/riesgos-normativos.md).

## Abiertos — bloqueantes

| # | Insumo | Para qué | Bloquea |
|---|---|---|---|
| 1 | **Reglamento interno de uso de vehículos** de la institución | Reglas de uso, autorizaciones, responsabilidades y sanciones propias | M-03, M-04, M-12 |
| 2 | **Formatos en papel vigentes**: bitácora, requisición de vehículo, salida, orden de misión, acta de entrega, control de combustible | Paridad pantalla↔papel; son el diseño de las pantallas | M-08, M-09, M-15, todo el Bloque 4 |

## Abiertos — nuevos, derivados de las decisiones del PO

| # | Insumo | Para qué | Bloquea |
|---|---|---|---|
| 16 | **Contrato de API y webhooks de ARGOS**: endpoints, autenticación, esquema de datos, eventos que emite | Es el corazón de la integración. Sin esto, M-20 es especulación | M-20, y el espejo local de autorizaciones y presupuesto |
| 17 | **Contrato de API de Talento Humano**: expediente del empleado, licencias, permisos, vacaciones, incapacidades, calendario | Padrón de motoristas y disponibilidad para asignación | M-05, M-07, M-20 |
| 18 | **Componente de mapas de ARGOS**: cuál es, cómo se reutiliza, qué licencia o servicio usa | M-19 Seguimiento en Ruta se apoya en él | M-19 |
| 19 | **Informes de Auditoría Interna o del TSC** sobre flota, combustible o uso de vehículos, si existen | Cada hallazgo describe algo que salió mal en la operación real: **son requisitos disfrazados** y valen más que cualquier entrevista | Bloque 2 |
| 20 | ~~**Texto de la reforma al Art. 48**~~ → **`Acuerdo No. 1012-2021`, Reglamento Especial en Materia de Permisos de Conducir**. Reencuadrado el 2026-08-24: **el Art. 48 regula requisitos de obtención, no la matriz** | Fijar la matriz licencia↔vehículo definitiva. **El esquema de ocho categorías ya está `[V]`, y `BE` aparece en `[P]`; faltan los umbrales literales y confirmar `BE`**. El Decreto 51-2025 se despega como pendiente aparte, y toca M-05, no M-07 | M-05, M-07 |
| 21 | ~~Contradicción entre el comunicado de la SIT y fuentes comerciales~~ **Resuelta el 2026-08-24 en contra de la fuente comercial, por cinco fuentes.** Queda: **confirmar con COVI-H o la SAPP que el congelamiento sigue vigente** — es condicional, sin plazo, y no hay evidencia entre marzo y agosto de 2026 | Ya hay tabla de trabajo `[P]` respaldada por el regulador. **Se puede diseñar M-18; no se promueve a producción sin esto** | M-18 |
| 22 | **Lista oficial de exoneraciones de peaje.** **Cuatro vías web agotadas el 2026-08-24**: contrato en el portal PPP (403), `covih.com` (403 en todas las rutas), sitio de la SAPP (404 tras migración de dominio), búsqueda de resoluciones (sin resultado) | ~~Define cómo se construye M-18~~ **Ya no.** La exoneración se modela como dato del vehículo y el diseño es el mismo pague o no. Define **cuánto estima** el sistema. `[V]` un pick-up es categoría liviana — L. 22 | M-18 |
| 23 | ✅ **CERRADO el 2026-08-26.** Los cuatro PDF están descargados en [`docs/01-negocio/normativa/fuentes/`](../01-negocio/normativa/fuentes/): Acuerdo 1012-2021, Ley de Tránsito, Decreto 51-2025 y Objetos del Gasto de SEFIN. Los cuatro tienen capa de texto. **Queda abierta solo la Circular 003-2025-Presidencia-TSC**, que sí requiere OCR real | La matriz licencia↔vehículo quedó `[V]` y cargada. Falta explotar el Decreto 51-2025 y el clasificador de SEFIN | M-05, M-07, M-18, M-03 |
| 24 | **¿La institución tiene tags CoviPass?** ¿A nombre de quién? ¿COVI-H emite factura fiscal en caseta o estado de cuenta empresarial? | Determina si el descargo de peajes ante el TSC es defendible. `covih.com` bloquea la consulta automatizada | M-18, M-13 |
| 25 | **¿El peaje se financia con el viático o es gasto de misión separado?** | Si va en el viático, es de ARGOS y M-18 se solapa. **Resolver antes de escribir historias de M-18** | M-18, M-20 |

## Abiertos — surgidos del Bloque 1

Consolidados de los cuatro artefactos del Bloque 1: actores y permisos, mapa de procesos y `PR-01`, máquina de estados, y las 53 reglas `RN-xx`.

### Bloqueantes de la operación en delegaciones

| # | Insumo | Para qué | A quién |
|---|---|---|---|
| 26 | **¿Acepta la institución un régimen de excepción a la segregación de funciones con controles compensatorios?** El MARCI exige cinco funciones incompatibles, lo que implica un mínimo de cinco personas por misión. Una delegación de tres no puede cumplirlo por aritmética. Está marcado `[C]`: **no se verificó que el MARCI contemple controles compensatorios de forma expresa** | Sin esto, la operación de toda delegación pequeña queda en suspenso, y el diseño no se puede cerrar | **Auditoría Interna.** Requiere pronunciamiento formal; no se consigue en un día |
| 27 | **Mapa de delegaciones con dotación real de personal**, y qué puesto de la sede respalda a cada una | Determina qué funciones se pueden sacar de la delegación y cuáles no | Talento Humano / Gerencia Administrativa |

### Huecos del modelo de autorización

| # | Insumo | Para qué |
|---|---|---|
| 28 | **¿Quién autoriza la misión de la máxima autoridad?** Y el autorizador alterno por dependencia y por delegación | Es un hueco real del modelo, no un detalle. Sin respuesta, el flujo se cae en el caso más visible de la institución |
| 29 | **¿Es delegable la firma del permiso de circulación en día u hora inhábil?** NRM-02 dice *firmado por la máxima autoridad*. Hasta confirmarlo, el sistema **no lo permite** | Define si el salvoconducto puede emitirse un sábado sin el titular |
| 30 | **¿Existe régimen formal de excusa por conflicto de interés** (parentesco entre solicitante y autorizador)? | Si existe, es una incompatibilidad más en la matriz de segregación |
| 31 | **Criterio de prelación** cuando dos solicitudes aprobadas compiten por el único vehículo compatible | Aparece la primera semana. Sin criterio explícito, lo resuelve quien tenga más jerarquía, que es exactamente lo que el sistema debería evitar |

### Paquete de parámetros operativos

| # | Insumo |
|---|---|
| 32 | Horario hábil oficial de la institución; antelación mínima de solicitud; plazo de convalidación de una emergencia; plazos de liquidación; umbrales de desviación de consumo (superior e inferior, que son independientes) |
| 33 | Criterio de vencimiento de licencia: ¿al inicio o al fin del día? |
| 34 | Correlativo institucional del vehículo: ¿único por institución, o compuesto por delegación? |
| 35 | Escala de severidad de fallas del vehículo — cuál es incapacitante y cuál no |

### Preguntas que cambian el alcance de un módulo

Estas no son parámetros: si la respuesta es sí, hay diseño adicional que hacer.

| # | Pregunta | Qué cambia si es sí |
|---|---|---|
| 36 | **¿La institución tiene almacenamiento propio de combustible** (cisterna, bidones)? | Cambia el **circuito completo de M-09**: deja de ser solo fondo y consumo, y aparece control de existencias |
| 37 | ¿Admite y reembolsa consumo de combustible pagado por el motorista de su bolsillo? | Agrega un circuito de reembolso con su propia comprobación |
| 38 | ¿Moviliza **carga peligrosa o especializada**? ¿Bajo qué régimen? | Requisitos adicionales de vehículo, motorista y documentación |
| 39 | ¿Realiza traslados de **personas bajo custodia o de menores**? | Cadena de custodia y minimización reforzada en M-17 |
| 40 | ¿Opera **rutas de lista abierta** con paradas donde suben y bajan personas? | El manifiesto cerrado sería impracticable; hay que modelar otra cosa |
| 41 | ¿Habilitamos **modo delegación desconectada** — autorizar y despachar sin red? | Deuda declarada por el arquitecto: se podría autorizar contra un espejo viejo. Tiene mitigaciones diseñadas (horizonte de validez, marca impresa, revalidación con hallazgo automático) pero **es decisión del PO** |

### Datos de catálogo por conseguir

| # | Insumo |
|---|---|
| 42 | Catálogo oficial de **restricciones médicas** de la DNVT. **Buscado el 2026-08-24 sin resultado.** Se confirma `[V]` que el trámite exige exámenes general, visual, psicológico y de tipo sanguíneo en centros autorizados por la DNVT, y `[P]` que el Decreto 51-2025 añade toxicológico, glucosa y electrocardiograma para mayores de 40 en categorías C, D y CE — **pero el catálogo de códigos de restricción que se estampan en la licencia no tiene fuente pública.** Es consulta directa a la DNVT, no investigación documental |
| 43 | **¿Cómo se rotula una motocicleta del Estado?** **Reencuadrado el 2026-08-24: puede que no haya vacío.** Las fuentes consultadas dicen *"partes laterales"*, no *"puertas laterales"* — y un tanque o un carenado **son** partes laterales. **Contradicción no resuelta**: ambas formulaciones son secundarias, ninguna es el texto del Acuerdo 303. Se cierra leyendo el Acuerdo 303 o la Circular 003-2025-Presidencia-TSC, **la única que sí requiere OCR**. Ver riesgo #21 |
| 44 | ¿Hay vehículos con **excepción de rotulación** autorizada, y quién la concede? |

## Abiertos — surgidos del Bloque 2 · casos especiales

Derivados de `CE-09` (bitácora en papel digitada días después), `CE-10` (motorista incapacitado en ruta), `CE-14` (vehículo prestado), `CE-15` (comodato y alquiler), `CE-16` (vehículo a taller con misiones programadas), `CE-17` (vehículo sin placa metálica), `CE-18` (carga y pasajeros en la misma misión) y `CE-19` (vehículo asignado a funcionario frente al pool). Cada uno está escalado al PO dentro de su caso especial, con opciones y costo.

| # | Insumo | Qué desbloquea | Origen |
|---|---|---|---|
| 45 | **¿Desde cuándo corre el plazo de liquidación** cuando el retorno se registra días después del hecho? Y **¿cuál es el plazo máximo de digitación diferida** en días hábiles? | `RN-46`, `RN-47`, `T-18`, `T-19`. Sin esto, las delegaciones sin red acumulan hallazgos por algo que no controlan | `CE-09` D-1 y D-2 |
| 46 | **¿El talonario preimpreso de bitácora trae folio propio?** Si se conserva, hay dos numeraciones que cruzar con la de `RN-44` | Diseño del formato impreso de M-15 y de la pantalla de digitación. Complementa el insumo #2 | `CE-09` D-3 |
| 47 | **¿Puede digitar formularios en papel quien después liquida esa misma misión?** En una delegación de tres personas es la misma persona | `RN-47` lo deja `[C]`. Decide si es advertencia o incompatibilidad nueva en la matriz `I-xx` | `CE-09` D-4 |
| 48 | **¿Puede conducir un vehículo oficial un servidor que no es motorista de planilla**, si su licencia habilita el tipo de vehículo y está vigente? | Define si existe la figura de motorista eventual en M-05 y si se apoya en el mecanismo de convalidación de `PC-18` | `CE-10` D-1 |
| 49 | **¿Cubre la póliza de seguro a un conductor no registrado como motorista de la institución?** | Puede cerrar la discusión del insumo #48 antes de empezar, aunque el reglamento lo permita | `CE-10` D-2 |
| 50 | **¿Existe reevaluación de aptitud para conducir tras un evento de salud en ruta?** `RN-11` cubre restricciones de la licencia, no la aptitud posterior a un episodio | Requisito de M-05 y bloqueo adicional sobre la reincorporación. Frontera con Talento Humano | `CE-10` D-3 |
| 51 | **¿Qué se hace hoy cuando no hay ningún motorista disponible para relevar en carretera?** Es la pregunta que decide si el vehículo pasa la noche en la vía | Tipifica los receptores válidos de custodia fuera de sede y el subtipo nuevo de `T-18` | `CE-10` |
| 52 | **¿Quién autoriza el préstamo de un vehículo entre dependencias, y quién el préstamo a otra institución?** `NRM-02` exige acta y resolución, pero el articulado no se pudo extraer | El expediente de préstamo de M-03. Requiere el insumo #1 y consulta a la unidad de Bienes | `CE-14` D-1 |
| 53 | **Rubros económicos del préstamo**: quién asume combustible, peajes, mantenimiento, multas y daños durante la tenencia ajena | Campos tipificados obligatorios del acta de préstamo. Hoy se acuerda de palabra | `CE-14` D-2 |
| 54 | **¿Puede prestarse un vehículo con orden de trabajo abierta, incidente en investigación o documentación que vence dentro de la ventana?** | Bloqueos y advertencias sobre la apertura del préstamo | `CE-14` D-3 |
| 55 | **¿Aplica la rotulación del Estado a vehículos en comodato o alquilados?** Zona gris expresa de `NRM-02` | `RN-18`. Hoy la advertencia se emite con aclaración de régimen y **no** bloquea | `CE-15` D-1 |
| 56 | **¿Aplica la prohibición de circular en día u hora inhábil a vehículos en comodato o alquilados?** Zona gris expresa de `NRM-02` | `RN-23`, `RN-24`, `PC-03`. Postura provisional `[I]`: se aplica igual, por asimetría de costo. Consultar a la unidad de Bienes o a ONADICI | `CE-15` D-2 |
| 57 | **Modalidad de alquiler vigente de la institución, contrato tipo y responsabilidades por rubro**; y si el arrendador puede sustituir la unidad a mitad de contrato | Título de tenencia de M-03, dirección de órdenes de trabajo en M-11 y costo por kilómetro en M-13 | `CE-15` |
| 58 | **¿Cómo se registra hoy la devolución al comodante o al arrendador, y quién la autoriza?** | Estado terminal `RETIRADO_DE_FLOTA` y su acta. Complementa las zonas grises de `NRM-02` sobre acta de entrega-recepción | `CE-15` |
| 59 | **¿El mantenimiento preventivo vencido bloquea la asignación o solo advierte?** Y **¿cuál es la ventana de indisponibilidad estimada exigible** al enviar un vehículo a taller | `BD-07` lo deja abierto expresamente. Sin la ventana, el sistema no puede decir qué misiones programadas quedan afectadas. Complementa los insumos #32 y #35 | `CE-16` |
| 60 | **Catálogo de documentos sustitutivos que emite el Instituto de la Propiedad** ante la falta de placa metálica, y la vigencia de cada uno | `NRM-06` los menciona genéricamente como *"documento sustitutivo o constancia del IP"* sin tipificarlos. El catálogo se entrega vacío y **no se inventa** | `CE-17` |
| 61 | **¿Acepta la DNVT el documento sustitutivo del IP en un retén?** | Decide si el paquete de identificación en carretera es defensa efectiva o solo evidencia interna. Postura provisional: se imprime siempre, por asimetría de costo | `CE-17` |
| 62 | **¿Admite el modelo más de un vehículo simultáneo bajo una misma Orden de Misión (convoy)?** | Decisión de producto con impacto en la máquina de estados, el despacho, la bitácora y la conciliación de `RN-30`. `CE-02` ya introduce varios vehículos **en secuencia**; esto es simultáneo | `CE-18` |
| 63 | **¿Qué tipos de carga exigen peso cierto y cuáles admiten estimación por rango?** | `RN-21` lo deja abierto. Define cuándo se bloquea el despacho por peso no declarado | `CE-18` |
| 64 | **¿Existe régimen formal de asignación permanente de vehículo a funcionario?** Quién lo confiere, con qué acto y con qué vigencia | Es el insumo más bloqueante de `CE-19` y depende del insumo #1. Sin él, todos los vehículos se modelan como pool y el control no alcanza a los asignados | `CE-19` |
| 65 | **¿Autoriza la institución el resguardo domiciliario de vehículos asignados, y con qué fundamento?** `NRM-02` prohíbe `[V]` el traslado a residencias; que el vehículo **pernocte** ahí es figura distinta y no consta regulada | Decide si la entrada y salida del domicilio se registran como eventos de bitácora amparados o como uso indebido | `CE-19` |
| 66 | **¿Se acepta la figura de Orden de Misión permanente de período** para el uso ordinario de vehículos asignados? Y **¿puede el asignatario conducir**, con qué política? | Si se acepta, hay que escribirla en la máquina de estados. Si no, el uso ordinario queda sin instrumento de control o produce ~250 expedientes anuales por vehículo | `CE-19` |

## Abiertos — surgidos de los requisitos no funcionales `RNF-xx`

Un requisito no funcional que no se puede medir es una aspiración. Estos son los umbrales que **no se inventaron**: cada uno bloquea la verificación de al menos un `RNF`. Ver [`docs/02-requisitos/no-funcionales/README.md`](../02-requisitos/no-funcionales/README.md).

| # | Insumo | Qué desbloquea | Origen |
|---|---|---|---|
| 67 | **Volumen operativo cifrado**: cuántos vehículos, cuántas delegaciones y dependencias, cuántos usuarios con cuenta y cuántos concurrentes en hora pico, cuántas misiones al mes, y **cuál es la misión más larga que la institución ejecuta**. El insumo #10 se resolvió como *"alto flujo"*, que no es un número | Fija el juego de datos de referencia `JDR-1`, del que dependen **todos** los umbrales de rendimiento y volumen. Hoy `JDR-1` es una derivación aritmética sobre supuestos | `RNF-01`, `RNF-02`, `RNF-03` |
| 68 | **Enlace real de la sede y de cada delegación**: tipo, ancho de banda, estabilidad, y si el enlace de la delegación es compartido con otras funciones | Umbrales de tiempo de sincronización y periodicidad de reconciliación. Complementa el insumo #11 | `RNF-03`, `RNF-07` |
| 69 | **Dispositivo de campo de referencia**: qué celulares tienen hoy los motoristas, quién los provee —¿la institución o el motorista?— y quién paga el plan de datos | Sin dispositivo de referencia declarado, las mediciones de batería, almacenamiento y respuesta se hacen contra el equipo del desarrollador, que es diseñar para nadie | `RNF-12`, `RNF-08` |
| 70 | **Parque real de impresoras** en sede y delegaciones: matriciales de 9 o 24 agujas, láser, tamaño de papel que usan hoy | Decide si el QR impreso es vía primaria o solo conveniencia, y si el formato es carta o hay excepciones. Complementa el insumo #2 | `RNF-11` |
| 71 | **Plazo de conservación** de registros financieros y de bienes, y **plazo de depuración o seudonimización** de datos personales de pasajeros. [NRM-01](../01-negocio/normativa/NRM-01-control-interno-tsc.md) y [NRM-07](../01-negocio/normativa/NRM-07-transparencia-y-datos-personales.md) los dejan `[C]` expresamente. Además: **periodicidad del sello de la cadena de auditoría** | Es el insumo detrás de la tensión estructural entre conservar todo y depurar lo personal. **A Auditoría Interna y al OIP** | `RNF-02`, `RNF-04`, `RNF-13`, `RNF-17` |
| 72 | **Ventana de mantenimiento aceptable**, tolerancia de indisponibilidad en horario hábil, y cuánta pérdida de datos acepta la institución tras un desastre (RPO) | Sin esto, la disponibilidad se promete a ojo. Y prometer números que nadie podrá sostener es peor que declarar el límite | `RNF-09`, `RNF-10`, `RNF-07` |
| 73 | **¿Quién opera el servidor en producción, con qué perfil técnico, y por qué canal se le avisa** de una condición crítica fuera de horario? También: **quién custodia la clave de cifrado del respaldo**. El insumo #9 resolvió el ambiente de desarrollo, no la operación real | Fija el nivel de simplicidad exigible a la instalación y al respaldo, que es un **filtro de elegibilidad de stack**, no una meta de calidad | `RNF-09`, `RNF-13`, `RNF-20` |
| 74 | **Frecuencia de reporte de posición aceptable** para M-19 y quién asume el consumo de datos que genera | Umbrales de latencia y de consumo del seguimiento en ruta. Depende también del insumo #18 | `RNF-08` |

## Abiertos — surgidos de la consolidación de reglas

| # | Insumo | Qué desbloquea | Origen |
|---|---|---|---|
| 75 | **¿Los proveedores de combustible y de peaje emiten estado de cuenta consolidado a nombre de la institución?** ¿Con qué periodicidad, en qué formato, y a quién llega? | `RN-95` exige conciliar contra esos estados de cuenta, y hoy no hay ningún insumo que los cubra: #16 y #17 son de ARGOS y Talento Humano. **Sin estado de cuenta, la conciliación de combustible y peajes depende solo de lo que declare el motorista** — que es exactamente lo que el auditor no acepta como control | `RN-95`, `CE-25`, `CE-28` |

## Abiertos — surgidos de los casos de uso `CU-xx`

| # | Insumo | Qué desbloquea | Origen |
|---|---|---|---|
| 76 | **¿Quién es el responsable, por puesto, de la cola de conflictos de sincronización de cada delegación?** Y con ella: **cuánto tiempo retiene el servidor una transición cuya predecesora no ha llegado** antes de escalarla, y **en qué plazo se escala un conflicto sin resolver**. `RN-45` exige que la cola tenga responsable por puesto, antigüedad visible y escalamiento por plazo configurable —*"una cola sin dueño se convierte en un basurero"*—, y la §6.3 Regla 2 de la máquina de estados exige un plazo que hoy no tiene valor | Sin responsable nombrado, los conflictos se acumulan y `BD-08` bloquea liquidaciones que nadie sabe que le tocan. Sin plazo de retención, una transición con hueco de secuencia queda en espera indefinida y la misión no se puede liquidar ni cerrar | [`CU-11`](../02-requisitos/casos-de-uso/CU-11-sincronizar-y-resolver-conflictos.md), `RN-45`, `RNF-03` |
| **93** | **¿Quién aprueba el descargo de un vehículo — `ACT-08` Gerencia Administrativa, o también `ACT-09` Máxima Autoridad?** El [*Manual de Propiedad Estatal*](../01-negocio/normativa/NRM-02-bienes-del-estado.md) regula el descargo pero **no se pudo extraer su articulado** (`NRM-02` `[P]`): la norma no lo zanja, así que lo zanja el PO. Ver `CU-17`, `HU-103` | `PR-02` y el estado terminal del vehículo. Hoy tres artefactos dicen cosas distintas | Hallazgo `HB3-16` |


## Abiertos — surgidos de las historias de usuario

| # | Insumo | Qué desbloquea | Origen |
|---|---|---|---|
| 77 | **Procedimiento y plazo para reclamar un peaje mal cobrado** ante el concesionario o la SAPP: a quién se presenta, en qué forma, con qué plazo de prescripción y qué respuesta cabe esperar | `RN-92` modela el reclamo como objeto con estado y resultado económico, pero **nadie sabe cómo se presenta**. Sin el procedimiento, el sistema registra un reclamo que no se puede tramitar | `HU-050`, `RN-92`, `CE-24` |
| 78 | **Plazo de la obligación de recuperar un vehículo resguardado fuera de sede**, y quién responde por él mientras tanto | Cuando la misión termina con el vehículo resguardado en sitio — avería, incapacidad del motorista, retorno del personal sin la unidad — el bien queda bajo custodia de alguien, en algún lugar, por tiempo indefinido. Es responsabilidad patrimonial sin dueño ni reloj | `HU-059`, `HU-065`, `CE-02`, `CE-10` |

## Abiertos — surgidos de la investigación pública del 2026-08-24

| # | Insumo | Qué desbloquea | A quién |
|---|---|---|---|
| 79 | ✅ **RESUELTO el 2026-08-26 contra la fuente oficial.** El Artículo 4 del Acuerdo 1012-2021 **crea `BE`** —«automóviles de la categoría B enganchados a un remolque»— y **no existe ninguna `DE`**: el epígrafe de remolques solo contempla `BE` y `CE`. Son **nueve** categorías. Corregidos `CLAUDE.md`, `NRM-06`, `BD-02` y el enumerado del dominio | Cerró el hueco de `BD-02`. **Abrió otro**: `A` y `B1` se definen por clase de vehículo y la matriz no las puede expresar — ver HANDOFF | M-05, M-07 |
| 80 | **¿Se cerró la negociación SIT–COVI del ajuste gradual de peaje a cuatro años?** | Si se cerró, hay **cuatro tramos tarifarios con fecha conocida** que se cargan de una vez como parámetros con vigencia. Es el mejor escenario posible para el modelo de tarifas de M-18 | SIT o SAPP. Complementa el insumo #21 |
| 81 | **¿Habrá lectura RFID de placa en las estaciones de peaje, y será el dato accesible a la institución?** El IP distribuye placas con RFID desde el último trimestre de 2026, con puntos de lectura en retenes, fronteras y casetas | Sería una fuente de paso por caseta **independiente de lo que declare el motorista** — el tipo de evidencia que el auditor prefiere. **No se diseña nada hoy**; se registra para no rehacer el modelo de M-18 y M-19 después | Instituto de la Propiedad. **Vigilancia, no bloqueo** |
| 82 | **¿Quedan los vehículos del Estado incluidos, exceptuados o priorizados en el reemplazo de placas?** | Con **990,000 vehículos sin identificación (27 % del parque)**, define cuánto tiempo más el estado *"sin placa metálica"* seguirá siendo el caso normal y no la excepción | Instituto de la Propiedad / unidad de Bienes |

## Reducidos o reencuadrados en la ronda del 2026-08-24

Ninguno de estos se cierra del todo — se documenta qué se consiguió y qué queda.

| # | Antes | Después |
|---|---|---|
| 20 | Texto de la reforma al Art. 48, para fijar la matriz | **El Art. 48 no contiene la matriz.** La matriz está en el Acuerdo 1012-2021, y su esquema de ocho categorías ya está `[V]`, y `BE` aparece en `[P]` por fuentes concordantes. El Decreto 51-2025 pasa a ser insumo de M-05 |
| 21 | Contradicción abierta sobre la tarifa vigente | **Resuelta en contra de la fuente comercial.** Queda un hueco de seis meses sobre un congelamiento condicional. Hay tabla de trabajo `[P]` para diseñar |
| 22 | Decide cómo se construye M-18 | **Ya no lo decide.** Un pick-up es categoría liviana `[V]` — L. 22. Cuatro vías web agotadas; solo se cierra preguntando |
| 23 | Trabajo de OCR sobre dos PDF | **Cuatro PDF tienen capa de texto y solo hay que abrirlos.** Solo uno requiere OCR real |
| 42 | Catálogo de restricciones médicas | Sin fuente pública. **Se confirma que la vía documental no existe**; es consulta a la DNVT |
| 43 | *"El Acuerdo 303 describe franjas en puertas laterales, que una moto no tiene"* | **La premisa del insumo puede ser falsa.** Las fuentes dicen *"partes laterales"*. Contradicción abierta, ninguna fuente primaria |


## Abiertos — surgidos de la corrección de hallazgos del 2026-08-25

Detectados al corregir los 46 hallazgos de las revisiones de los Bloques 3 y 4. Cada uno está declarado dentro del artefacto que lo espera.

| # | Insumo | Qué desbloquea | Origen |
|---|---|---|---|
| 83 | **Tamaño del subrango de folios por dispositivo**, y procedimiento de ampliación sin conectividad | `subrango_de_folio` existe pero sin tamaño no se puede dimensionar. Complementa al #1, que solo cubre el rango de la delegación | `HB34-52` |
| 84 | **Plazo de retención propio del tercero de siniestro y del dato de salud**, distintos del plazo del manifiesto | El #71 fija el plazo general; falta la diferenciación por categoría de dato, que es lo que `RNF-17` exige | `HB34-53` |
| 85 | **Catálogos `grado_de_cumplimiento` y `causa_de_incumplimiento`**, y si el cumplimiento se declara al retornar o al liquidar | `RN-78` es bloqueo duro para cerrar y la entidad recién existe | `HB34-61` |
| 86 | **Fechas de corte legal y operativa del ejercicio fiscal**, y criterio de imputación entre ejercicios | `RN-96` lo marca dependiente de SIAFI, que está diferido | `HB34-61` |
| 87 | **¿La constatación de peso y ocupación efectivos al despachar bloquea, o solo deja indicador?** | Decide si `T-12` gana una precondición más | `HB34-61` |
| 88 | **Solape máximo en días entre titular saliente y entrante** de un puesto | Hoy solo está citado en `actores-y-roles §2.3`, sin entrada propia. Condiciona el traspaso de custodias | `HU-128` |
| 89 | **Alcance de datos durante un préstamo de vehículo**: qué ve la institución receptora y por cuánto tiempo | El índice de reglas lo remite a `actores-y-roles.md` y ese documento no lo trata. Es un hueco sin dueño | `HU-132` |
| 90 | **Plazo de aprobación de un parámetro cargado**, con alerta al vencerse | Un parámetro cargado y nunca aprobado no se aplica, y hoy nada avisa | `HU-145` |
| 91 | **¿Qué hacer cuando informática y Gerencia Administrativa recaen en la misma persona?** `I-13` es núcleo irreductible y en una institución chica no se puede cumplir por aritmética | Mismo problema que el #26, en otro par. Requiere pronunciamiento | `HU-146` |
| 92 | **Actor no catalogado: el Oficial de Información Pública.** Lo necesitan cuatro historias de M-17 y tres pantallas | Hoy cuelgan del auditor, que es **solo lectura** — y una de ellas rectifica. Es una contradicción con `I-12` | `HB34-66`, `HU-121`–`HU-123` |
| 93 | **Cuántos días antes del corte legal empieza la ventana de cierre** — `cierre.ventana_de_cierre_dias` | Sin este parámetro **no se evalúan los motivos de cierre compartidos (`RN-96` punto 3) ni el indicador de cierre apurado.** El código ya lo lee del catálogo con vigencia y declara su ausencia en el acta, pero mientras nadie lo cargue esos dos controles están apagados. **No tiene valor por omisión a propósito**: la ficha no fija ninguno, y suponer uno mediría el cierre en bloque contra un número que nadie declaró. Va junto al #86 y al #94 | `RN-96` |
| 94 | **Día y mes del corte legal** (`cierre.corte_legal_dia_y_mes`, formato `MM-DD`) y **cuántos días después cae el operativo** (`cierre.corte_operativo_dias_despues`) | Es el #86 aterrizado: el código ya los lee del catálogo con vigencia, y **sin ellos no se arma ni se produce ninguna acta de cierre**. A diferencia de la ventana —que apaga dos reportes— los cortes deciden qué expedientes entran al inventario y a qué ejercicio se imputa cada hecho, así que suponerlos falsearía todo lo demás. El legal se guarda como día y mes porque el parámetro rige para **todos** los ejercicios; el operativo como días después porque cae en el año siguiente | `RN-96` |
| 95 | **Clave de vinculación de la Orden de Misión con ARGOS** — cómo se forma y quién la asigna | `RN-81` punto 1 la exige: se establece al crear la orden y **no cambia** en todo su ciclo. El modelo de datos la nombra y el código no la tiene: el reporte de reversión de compromisos la emite hoy con el ULID de la misión, que sirve dentro de SIGTI y **ARGOS no va a reconocer**. Sin ella el archivo de conciliación no se puede cruzar con el sistema al que va dirigido. Va junto a los #16 y #17 | `RN-81`, `RN-96` |
| 96 | **Catálogo `causa_interrupcion`** — qué causas reconoce la institución para el evento de interrupción en ruta | `RN-70` lo declara configurable. El expediente de M-12 ya lo exige, pero hoy es **texto validado contra no-vacío**: cablear una lista obligaría a un despliegue cada vez que aparezca una causa que nadie previó. Mismo estado que `tipo_de_hallazgo_posterior` de `RN-93` | `RN-70` |
| 97 | **Catálogo `causa_de_no_disponibilidad_del_bien`** | `RN-75` lo declara configurable, y es el que tipifica por qué un vehículo pasa a `NO_DISPONIBLE` desde la hora del hecho. Sin él, el estado operativo no se puede mover con la causa que la máquina de estados §10.2 exige | `RN-75`, `RN-60` |
| 98 | **Catálogo `motivo_de_prestamo`** y umbrales de escalamiento por mora | `RN-63` los declara configurables. El expediente ya exige el motivo, pero es **texto validado**; y el escalamiento diario por mora no tiene umbrales contra los cuales escalar | `RN-63` |
| 99 | **Catálogo `causa_indisponibilidad`** y **`horizonte_reservas_afectadas`** | `RN-60` los declara configurables. La causa es hoy **texto validado**, y el horizonte es la ventana estimada de la propia indisponibilidad — lo defendible sin inventarlo, pero no lo que la regla pide. Tercer catálogo pendiente junto al #96 y el #98 | `RN-60` |
| 100 | **El régimen de tenencia real de cada vehículo de la flota** — quién es el titular, con qué documento, hasta cuándo, y qué rubros cubre | **La estructura ya existe**: `RN-62` está implementada, el título es una serie con vigencia y matriz de rubros, y `HB3-17` **ya juzga en vez de advertir**. Lo que falta es la **carga**: mientras un vehículo no tenga título registrado, el sistema advierte y deja pasar el terminal —frenar el descargo de toda la flota por un dato de alta que nadie llenó sería peor que el asiento que se quiere evitar—, y ninguna misión se contrasta contra la vigencia. **Cada vehículo sin título es un control apagado, uno por uno** | `RN-62`, §10.2 |
| 101 | **¿Cómo se declara indisponible un vehículo que ya está `ASIGNADO`?** | `RN-60` presupone esa transición —habla de reservas ya programadas que quedan en conflicto— y §10.2 no la tiene: sólo `DISPONIBLE → EN_TALLER` (`W-09`) y `NO_DISPONIBLE → EN_TALLER` (`W-12`). Hoy el expediente se registra y el asiento de estado no se pone, declarándolo. **Requiere pronunciamiento sobre §10.2**, que es la autoridad | `RN-60`, §10.2 |

## Resueltos en la revisión del 2026-08-06

| # | Insumo original | Resolución |
|---|---|---|
| 3 | Reglamento de viáticos y Acuerdo 401-2026 | **Fuera de alcance.** Lo maneja ARGOS. SIGTI solo comparte la clave para vincular una Orden de Misión con sus viáticos |
| 4 | Organigrama y niveles de autorización | Se obtienen de **ARGOS por API**, con espejo local actualizado por webhooks |
| 5 | Inventario actual de flota | **No se espera.** El catálogo se diseña a partir de cómo se registran habitualmente los vehículos de instituciones públicas hondureñas |
| 6 | Padrón de motoristas y licencias | Viene del **sistema de Talento Humano por API** |
| 7 | Contratos de combustible y mecanismo de control | **Reencuadrado.** No hay contratos que gestionar: Administración aprueba un monto en efectivo u órdenes de pago que el Jefe de Transporte solicita. Ver `PROP-01` abajo |
| 8 | Estructura presupuestaria | Se usa **la que define ARGOS** |
| 9 | Servidor on-premise y quién administra | Desarrollo **local** más **servidor de prueba** disponible, con credenciales gestionadas por el PO |
| 10 | Volumen operativo | **Alto flujo.** Genera requisitos, no solo dimensionamiento — ver M-19 abajo |
| 11 | Delegaciones y conectividad | Se reutiliza el **componente de mapas de ARGOS** |
| 12 | Informes de auditoría | Reformulado como insumo #19, con la aclaración de a qué se refiere |
| 13 | Sistemas con los que integrar | **Talento Humano, ARGOS (viáticos), Almacén.** Almacén queda diferido |
| 14 | Calendario de días hábiles y feriados | Se maneja **junto con Talento Humano** |
| 15 | Certificados de firma electrónica | **No se usa firma electrónica certificada.** Autorización interna por usuario autenticado o código gestionado por el sistema |

## PROP-01 — Propuesta para el control de combustible

El PO pidió una propuesta "práctica y segura" para el insumo 7. Se propone lo siguiente, a validar en el Bloque 1:

**Modelo: fondo asignado con trazabilidad de tres puntas.**

1. **Solicitud de fondo** — el Jefe de Transporte solicita a Administración un monto en efectivo o una cantidad de órdenes de pago, con la justificación operativa del período.
2. **Aprobación** — Administración aprueba y entrega. Queda registrado el monto, la fecha, quién aprobó y contra qué partida.
3. **Asignación** — Transporte asigna porciones del fondo a misiones o a motoristas concretos. Cada asignación tiene folio, monto, responsable y misión vinculada. El motorista **firma la recepción**.
4. **Consumo** — el motorista registra el consumo desde el campo, con galones, monto, estación, odómetro y **fotografía del comprobante**. Funciona sin conectividad.
5. **Liquidación** — al cerrar la misión se concilian: monto asignado vs. monto consumido vs. comprobantes vs. saldo devuelto.
6. **Conciliación con kilometraje** — galones consumidos vs. kilómetros recorridos vs. rendimiento esperado del vehículo, con desviación marcada **en ambas direcciones**.

**Por qué es seguro:** el punto de fuga clásico es el efectivo sin trazabilidad. Aquí ningún lempira se mueve sin quedar atado a un folio, un responsable, una misión y un odómetro. La conciliación automática con kilometraje es exactamente lo que busca el auditor del TSC.

**Por qué es práctico:** no exige contratos, ni integración con proveedores, ni tarjetas de flota. Funciona con el mecanismo que la institución ya usa hoy — solo lo registra.

**Decisiones abiertas de PROP-01:**
- `[C]` ¿El fondo se asigna por período (mensual) o por misión?
- `[C]` ¿Un motorista puede tener saldo acumulado entre misiones, o liquida cada una?
- `[C]` ¿Qué pasa con el sobrante: se devuelve o se arrastra?
- `[C]` ¿La orden de pago es un documento con folio preimpreso, o la genera el sistema?

## Cómo levantar los insumos abiertos

Los insumos 1, 2 y 19 salen de **una sesión de dos horas** con Gerencia Administrativa, el Encargado de Transporte, un motorista con años en el puesto, y Auditoría Interna. Lleva los formatos en papel a la mesa y recórrelos campo por campo: ahí aparecen las reglas que nadie escribió nunca.

Los insumos 16, 17 y 18 dependen del PO, que administra ARGOS.
