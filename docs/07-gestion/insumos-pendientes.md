# Insumos pendientes de la institución piloto

Documentos y datos que se necesitan para que el análisis se apoye en la realidad y no en suposiciones. **Los bloqueantes no se suplen con inferencias**: mientras falten, el módulo correspondiente queda con parámetros abiertos marcados `[C]`.

**Actualizado 2026-08-06** tras la revisión del PO. Ver [DP-001](decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md).

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
| 20 | **Texto de la reforma al Art. 48 de la Ley de Tránsito** (2025), categorías CD y CE | Fijar la matriz licencia↔vehículo definitiva | M-05, M-07 |
| 21 | **Tarifa de peaje efectivamente vigente hoy**, confirmada con COVI-H o la SAPP | Hay contradicción entre el comunicado de la SIT del 28/02/2026 y fuentes comerciales. **No se carga ninguna tarifa sin esto** | M-18 |
| 22 | **Lista oficial de exoneraciones de peaje** — cláusula del contrato de concesión o consulta a COVI-H | Decide si un vehículo administrativo del Estado paga o no. **Es lo que define cómo se construye M-18** | M-18 |
| 23 | **OCR de dos PDF oficiales**: Ley de Tránsito (Arts. 48 y 51) y tabla de tarifas de la SAPP | Un solo trabajo resuelve la matriz licencia↔vehículo y el criterio de clasificación de peaje | M-05, M-07, M-18 |
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
| 42 | Catálogo oficial de **restricciones médicas** de la DNVT |
| 43 | **¿Cómo se rotula una motocicleta del Estado?** El Acuerdo 303 describe franjas en *puertas laterales*, que una moto no tiene |
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
