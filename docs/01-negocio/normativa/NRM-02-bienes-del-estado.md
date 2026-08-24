# NRM-02 — Bienes del Estado: uso, circulación y control de vehículos oficiales

| Campo | Valor |
|---|---|
| **Ámbito** | Administración, registro, identificación, uso y baja de vehículos propiedad del Estado |
| **Módulos afectados** | M-03, M-04, M-07, M-12, M-15 |
| **Última verificación** | **2026-08-24** (previa: 2026-08-06) |
| **Riesgo de cambio** | Medio |

## Marco normativo

| Norma | Referencia | Vigencia | Verificación |
|---|---|---|---|
| Reglamento para el funcionamiento, uso, circulación y control de automotores propiedad del Estado | Acuerdo No. 303 | 24/04/1981 | `[V]` |
| Creación de la Dirección General de Bienes Nacionales | Decreto Legislativo No. 274-2010 | 13/01/2011 | `[V]` |
| Conversión a Dirección Nacional de Bienes del Estado, adscrita a SEFIN | PCM-047-2015 | 2015 | `[V]` |
| Prohibición de uso privado de vehículos del Estado | Decreto 135-94 y Decreto 48 (1981) | Vigente | `[P]` |
| Uso indebido de vehículos | Circular STLCC-ONADICI No. 022-03-2024 | 03/2024 | `[V]` |
| Uso y circulación de vehículos del Estado | Circular 003-2025-Presidencia-TSC | 2025 | `[V]` |
| Manual de Propiedad Estatal (DGBN) | versión mayo 2011 | `[C]` | `[P]` |

`[C]` La denominación y adscripción vigentes del órgano rector en 2026. `[C]` La cita completa del Decreto 48.

## Identificación obligatoria del vehículo `[V]`

Todo vehículo del Estado debe portar:

- Placas nacionales
- **Tres franjas horizontales de 10 cm cada una, azul–blanco–azul**, como distintivo de pertenencia al Gobierno de la República
- Leyenda **"PROPIEDAD DEL ESTADO DE HONDURAS"** en letras de **2.54 cm**, sobre la franja central
- Siglas o nombre de la institución
- **Numeración consecutiva** institucional del vehículo

Es un hallazgo de auditoría frecuente y se verifica físicamente en operativos.

> ### Revisión del 2026-08-24 — dónde van las franjas, y qué pasa con las motos
>
> **Se corrigió "puertas laterales" por la fórmula sin sujeto.** `[P]` Las fuentes secundarias consultadas el 2026-08-24 dicen **"en las partes laterales"** del vehículo, no "en las puertas". Las medidas — 10 cm de franja, 2.54 cm de letra — se **confirman `[P]`** por fuente concordante.
>
> **Es una diferencia con consecuencias.** *"Puertas laterales"* excluye por construcción a la motocicleta y convierte el insumo #43 en un vacío normativo. *"Partes laterales"* no la excluye: un tanque de combustible y un carenado **son** partes laterales, y la rotulación sería exigible con la misma regla.
>
> **La contradicción no se resuelve aquí.** No se pudo leer el texto del Acuerdo 303 ni el de la Circular 003-2025-Presidencia-TSC — esta última es un **escaneo de imagen real** (JPEG embebido, digitalizado en Canon iR1643i II), a diferencia de otros PDF del TSC que sí tienen capa de texto. **Es el único documento del lote que efectivamente requiere OCR.**
>
> **Fiabilidad relativa:** ambas formulaciones vienen de fuentes secundarias. Ninguna es el texto del Acuerdo 303. **La atribución "puertas laterales" es la que hay que probar**, porque es la más restrictiva y la que hoy sostiene un insumo abierto.
>
> **Postura provisional para el diseño, sin cambiar ninguna regla:** el estado de rotulación se modela como **campo verificable con fecha, fotografía y observación libre**, exactamente como ya lo pide la implicación de requerimiento correspondiente. Ese modelo **funciona igual** con moto o con pickup, y no depende de resolver la contradicción. Lo que sí conviene evitar es cablear una lista de ubicaciones esperadas por tipo de vehículo.

## Prohibición de uso privado y permisos `[V]`

- Prohibido el uso de vehículos del Estado en **días y horas inhábiles**, y para tareas ajenas a la función — incluido el traslado de funcionarios, empleados y sus familias a residencias o asuntos personales.
- Para circular en días u horas inhábiles se requiere **permiso firmado por la máxima autoridad** de la institución (Secretario del ramo, o Presidente/Gerente en descentralizadas).
- **Exceptuados**: servicios públicos esenciales, emergencia, seguridad, defensa, salud, e integrantes de CONAPREMM.
- `[P]` Se reportan multas de **L 5,000 a L 50,000** más posible decomiso, según operativos del TSC en Semana Santa 2026. `[C]` la base legal exacta del rango.

**El TSC realiza operativos vehiculares de fiscalización en Semana Santa** `[V]` (informes E-001-2015-DFBN, E-007-2015-FBN, 002-2023-DFBN, comunicados 2026). Es un evento operativo recurrente y **predecible** — el sistema puede prepararse para él.

## Descargo, baja y tarjeta de responsabilidad `[P]`

El *Manual de Propiedad Estatal* de la Dirección General de Bienes Nacionales regula reporte de movimientos de inventario, descargo de bienes y pérdidas. No se pudo extraer el articulado. `[C]` obtener el manual y los formatos vigentes con la unidad de Bienes de la institución.

## Implicaciones de requerimiento

- **El sistema debe** mantener una **ficha maestra de vehículo** con: número correlativo institucional, placa, número de motor, chasis/VIN, marca, modelo, año, color, tipo, capacidad (pasajeros / carga en kg y m³), tipo de combustible, número de bien del inventario nacional, valor de adquisición, fuente de financiamiento, fecha de alta, unidad asignada y custodio responsable.
- **El sistema debe** registrar el **estado de rotulación e identificación** (franjas, leyenda, siglas, numeración) como campo verificable con fecha de última constatación y fotografía.
- **El sistema debe** implementar un flujo de **Permiso de circulación en día u hora inhábil**: solicitud con justificación, vehículo, motorista, ruta, ventana temporal y firma de la máxima autoridad. Debe imprimirse un **salvoconducto con folio verificable por QR** que el motorista porte — el control en carretera es físico.
- **El sistema debe** bloquear la aprobación de una salida en día inhábil o feriado si no existe permiso vigente de la máxima autoridad, **salvo** que el vehículo esté marcado como de servicio exceptuado (emergencia, seguridad, salud).
- **El sistema debe** soportar **tarjeta de responsabilidad / asignación de custodio**, con acta de entrega-recepción firmada, y trazar cada cambio de custodio.
- **El sistema debe** soportar el ciclo de vida completo del bien: alta, traslado entre unidades, préstamo interinstitucional, siniestro, desuso, **descargo/baja** y disposición final — con acta y resolución en cada caso.
- **El sistema debe** registrar pérdida, robo o siniestro con denuncia, número de expediente, acta y estado del proceso de deducción de responsabilidad.
- **El sistema debe** permitir **constatación física periódica** con captura móvil (foto, odómetro, ubicación, estado) para conciliar contra el registro de bienes.
- **El sistema debe** producir un **reporte previo a Semana Santa**: vehículos autorizados a circular con su permiso, y vehículos que deben estar resguardados con confirmación de resguardo.

## Zonas grises y pendientes

- `[C]` **Ubicación exigida de las franjas — "puertas laterales" vs. "partes laterales".** Decide si la motocicleta del Estado tiene o no obligación de rotulación (insumo #43). Se cierra leyendo el Acuerdo 303 o la Circular 003-2025-Presidencia-TSC. **Esta última sí requiere OCR real.**
- `[P]` **Categorías exceptuadas del control de circulación en día inhábil:** vehículos destinados a **emergencias, seguridad, defensa y salud**, más los adscritos a la **CONAPREMM**. Fuente periodística sobre actuaciones del TSC, consultada el 2026-08-24. Concuerda con la excepción que la ficha ya asumía; **sube de `[I]` a `[P]`, no a `[V]`.** Confirmar el listado literal contra la circular.
- `[C]` ¿La institución tiene vehículos bajo alguna excepción de circulación (emergencia, salud, seguridad)? Es un atributo del vehículo, no del viaje.
- `[C]` Formatos vigentes de acta de entrega-recepción, tarjeta de responsabilidad y descargo.
- `[C]` Régimen aplicable a vehículos en comodato o alquilados: ¿les aplica la rotulación y la prohibición de día inhábil?

## Fuentes

- [PCM-047-2015 — Dirección Nacional de Bienes del Estado](http://www.sefin.gob.hn/wp-content/uploads/2016/03/PCM-047-2015.pdf) — consultado 2026-08-06
- [ONADICI — Circular STLCC-ONADICI No. 022-03-2024, uso indebido de vehículos](https://www.onadici.gob.hn/wp-content/uploads/2024/03/CIRCULAR-STLCC-ONADICI-No.-022-03-2024-USO-INDEBIDO-DE-VEHICULOS.pdf) — consultado 2026-08-06
- [TSC — Circular 003-2025-Presidencia-TSC](https://www.tsc.gob.hn/wp-content/uploads/Circular_003-025_PRESIDENCIA-TSC.pdf) — consultada **2026-08-24**. **Escaneo de imagen real** (JPEG 1725×2221, Canon iR1643i II, procesado con *Paper Capture*): **requiere OCR**, a diferencia de otros PDF del TSC
- [La Prensa — hasta L. 50,000 de multa por uso indebido de vehículos del Estado](https://www.laprensa.hn/honduras/multa-indebido-vehiculos-estado-tribunal-superior-cuentas-DD29912463) — consultada 2026-08-24. Origen `[P]` de las medidas de rotulación y de las categorías exceptuadas
- [Tiempo — vehículos del Estado circulando sin permisos, 28/03/2026](https://tiempo.hn/honduras/2026/03/28/vehiculos-estado-honduras-sin-permisos-tsc-multas/) — consultada 2026-08-24
- [TSC — Circular 003-2025-Presidencia](https://www.tsc.gob.hn/wp-content/uploads/Circular_003-025_PRESIDENCIA-TSC.pdf) — consultado 2026-08-06
- [TSC advierte multas por uso indebido de vehículos del Estado, marzo 2026](https://tiempo.hn/honduras/2026/03/28/vehiculos-estado-honduras-sin-permisos-tsc-multas/) — consultado 2026-08-06
- [AMHON — Reglamento para el control en el uso de vehículos municipales](https://amhon.hn/documentos/manuales/Reglamento_Vehiculos_Municipalidades.pdf) — consultado 2026-08-06
