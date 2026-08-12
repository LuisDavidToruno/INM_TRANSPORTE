# CE-21 — Los galones consumidos no cuadran con los kilómetros recorridos

| Campo | Valor |
|---|---|
| **Módulos** | M-09 Combustible, M-13 Liquidación, M-08 Bitácora, M-14 Auditoría, M-19 Seguimiento en Ruta |
| **Estados afectados** | `RETORNADA` (conciliación de `EF-05`), `LIQUIDADA`, y el desenlace `CERRADA` o `CERRADA_CON_HALLAZGO` |
| **Frecuencia** | Frecuente en su forma leve; ocasional en la forma que constituye hallazgo |
| **Impacto** | Financiero y de auditoría — **es el hallazgo típico del TSC en flota** |
| **Resolución** | Definida. Umbrales `[C]` |

## La situación

Son dos situaciones opuestas y las dos importan.

### Consumo excesivo

Pickup Toyota Hilux `INS-PU-014`, rendimiento esperado **11 km/galón**. Misión de cinco días: Tegucigalpa → Siguatepeque → San Pedro Sula → Puerto Cortés y retorno. Odómetro de salida 148,320; de retorno 148,940. **620 km recorridos.** Los comprobantes suman **84 galones**.

620 ÷ 84 = **7.4 km/galón**. Treinta y tres por ciento por debajo de lo esperado. Faltan por explicar unos 28 galones — cerca de **L 3,400** al precio del período.

Las explicaciones legítimas existen y son varias: subida a Siguatepeque con carga completa, el vehículo esperó cuatro horas con el motor encendido y el aire acondicionado puesto en el muelle de Puerto Cortés, el tanque salió lleno y volvió a la mitad. Y la ilegítima también existe, y es la que el TSC busca.

### Rendimiento imposiblemente bueno

Microbús `INS-MB-003`, rendimiento esperado 9 km/galón. La bitácora reporta **980 km recorridos** con **42 galones** comprobados: **23.3 km/galón**. Un microbús no hace eso ni de bajada.

Un rendimiento demasiado bueno **casi nunca es una buena noticia**. Significa una de tres cosas: alguien cargó combustible que nadie anotó — del tanque de la institución, de otra dependencia, de un tercero —, el kilometraje está inflado, o el consumo se cargó a otra fuente. [RN-30](../../01-negocio/reglas/RN-30-conciliacion-galonaje-kilometraje.md) lo dice sin rodeos.

## Qué se hace hoy sin sistema

`[C]` No se sabe con certeza y hay que levantarlo (insumo #1, insumo #19 — informes de Auditoría Interna). Lo que se observa en instituciones comparables:

El Encargado de Transporte compara "a ojo" el consumo del mes contra el del mes anterior. Si el salto es grande, pregunta. Si no, se archiva. **La comparación contra el rendimiento del propio vehículo casi nunca se hace**, porque exige tener el kilometraje y el galonaje en la misma hoja, y están en cuadernos distintos: el kilometraje en la bitácora del vehículo, los galones en los vales.

**Y el rendimiento demasiado bueno no se mira nunca.** Nadie investiga un vehículo que gastó poco. Esa es la regla que nadie escribió, y es por donde se va el combustible.

## Por qué el flujo normal no lo cubre

Porque el flujo feliz de la liquidación **suma comprobantes y cuadra caja**. Ese cálculo puede dar exacto — asignado = consumido + devuelto — y la misión seguir siendo un hallazgo, porque el dinero cuadra y los galones no corresponden al recorrido.

Son dos conciliaciones distintas y **ninguna sustituye a la otra**:

| Conciliación | Pregunta que responde | Regla |
|---|---|---|
| De caja | ¿Volvió como combustible o como efectivo el dinero que salió? | [RN-29](../../01-negocio/reglas/RN-29-liquidacion-de-combustible.md) |
| De rendimiento | ¿Los galones explican los kilómetros de la misión autorizada? | [RN-30](../../01-negocio/reglas/RN-30-conciliacion-galonaje-kilometraje.md) |

[NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md) es explícita: *"el auditor no busca comprobantes, busca correlación entre consumo, kilometraje y misión autorizada"*. Un sistema que solo hace la primera conciliación archiva facturas y no responde a lo que se le va a preguntar.

## Regla de resolución

**La conciliación se dispara sola al retornar**, no a pedido de nadie (`EF-05` de la [máquina de estados](../../03-arquitectura/estados/orden-de-mision.md)). No bloquea el registro del retorno: el motorista que llega a las 9 de la noche no se queda peleando con una pantalla.

**1. Cálculo con dos umbrales independientes.** `rendimiento observado = km recorridos ÷ galones consumidos`, contra el `rendimiento_esperado` del vehículo **vigente a la fecha del hecho** ([RN-40](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md)) y congelado con la misión al despachar (`EF-03`). Los umbrales superior e inferior son distintos y configurables: un exceso del 20% y un ahorro del 20% no significan lo mismo ([RN-30](../../01-negocio/reglas/RN-30-conciliacion-galonaje-kilometraje.md), comportamiento 2).

**2. Se presenta con el desglose que lo sustenta, nunca como un número solo.** Kilómetros por tramo, cada carga con su odómetro y su estación, nivel de tanque a la salida y al retorno, tiempo de motor encendido en espera (M-19), y la ruta autorizada contra la recorrida. Sin el desglose, el liquidador no puede aceptar ni rechazar la justificación: solo puede firmar.

**3. Cuando el cálculo no es concluyente, se dice.** Odómetro averiado, saldo de tanque arrastrado entre misiones, sustitución de vehículo a mitad de misión. El resultado se marca **no concluyente** y alimenta el análisis agregado, que sí es válido ([RN-30](../../01-negocio/reglas/RN-30-conciliacion-galonaje-kilometraje.md), condiciones de aplicación). Un hallazgo falso repetido tres veces hace que nadie vuelva a mirar los hallazgos.

**4. Cada vehículo por separado si hubo sustitución.** Cortes de odómetro propios por vehículo. Un promedio de la misión mezcla dos rendimientos y no significa nada. Es el desenlace "continúa con vehículo sustituto" del caso de avería en ruta ([`CE-02`](CE-02-averia-mecanica-en-ruta.md)).

**5. La desviación se tipifica, no se comenta en texto libre.** Catálogo configurable: ruta de montaña, espera prolongada con motor encendido, carga completa, tráfico, descenso continuo, precio distinto al estimado, y — la que importa — *sin causa identificada*.

**6. Fuera de umbral sin justificación aceptada, la asignación va a `CONCILIADA_CON_DESVIACION` y la misión dispara `H-01`.** Entonces **`T-21` deja de estar disponible** y el único cierre posible es `T-22` → `CERRADA_CON_HALLAZGO` ([§7.2](../../03-arquitectura/estados/orden-de-mision.md)). Quien cierra **no elige** entre cerrar limpio o con hallazgo: el criterio decide y él lo confirma con su justificación. Y no se puede desactivar el criterio para una misión concreta.

**7. El patrón se ve en el agregado, no en la misión.** Desviaciones recurrentes del mismo vehículo, motorista o dependencia generan alerta agregada. El hallazgo del TSC —*incremento de consumo sin relación con el uso habitual de la flota*— es un patrón de meses, no un viaje.

**8. Contra la manipulación consistente del odómetro, esta regla no alcanza.** Se mitiga con fotografía del tablero en salida y retorno y con el cruce contra peajes y ruta ([RN-37](../../01-negocio/reglas/RN-37-coherencia-de-la-secuencia-de-casetas.md)): un vehículo que declara 980 km pero solo cruzó Zambrano dos veces está diciendo dos cosas incompatibles.

### Reglas candidatas — no dar por escritas

| Candidata | Enunciado propuesto | Por qué falta |
|---|---|---|
| `RN-C21a` | *Todo ingreso de combustible al tanque de un vehículo institucional se registra como abastecimiento, cualquiera sea su fuente de financiamiento — fondo de la misión, tanque institucional, otra dependencia, donación o pago del propio servidor.* | Las siete reglas de M-09 modelan el consumo **del fondo**. Un despacho desde el tanque de la institución no pasa por ningún folio y por eso **no existe para el sistema** — y es exactamente lo que produce el rendimiento imposiblemente bueno de `INS-MB-003`. Sin esta regla, la detección de [RN-30](../../01-negocio/reglas/RN-30-conciliacion-galonaje-kilometraje.md) señala un síntoma cuya causa el sistema no puede registrar |
| `RN-C21b` | *El nivel de combustible del tanque a la salida y al retorno es dato obligatorio de la bitácora, en la escala que el instrumento permita.* | [RN-30](../../01-negocio/reglas/RN-30-conciliacion-galonaje-kilometraje.md) lo menciona en un caso límite y lo atribuye a [RN-22](../../01-negocio/reglas/RN-22-custodia-del-vehiculo.md), que trata de custodia. **Ninguna regla lo obliga.** Sin él, "salió lleno y volvió vacío" no se puede distinguir de un faltante |

## Evidencia que debe quedar

Lo que la institución le entrega al auditor del TSC **no son las facturas**. Es esto:

1. El **reporte de conciliación periódica** que exige [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md): galones asignados por folio, galones comprobados, kilómetros según bitácora, rendimiento esperado y observado, y desviación **marcada en ambas direcciones**, por vehículo, motorista, dependencia y período
2. Por cada misión desviada: odómetro de salida y retorno con fotografía del tablero, cada carga con estación, galones, monto, odómetro del momento y comprobante
3. La causa tipificada de la desviación, quién la aceptó o la rechazó, con qué fundamento y cuándo
4. El `rendimiento_esperado` usado, su vigencia y el paquete normativo congelado al despachar (`EF-03`) — para que el cálculo sea **reproducible dos años después**
5. Los tiempos de espera en sitio de M-19, cuando son la explicación
6. El expediente de hallazgo `H-01` con responsable de seguimiento y plazo, si la misión cerró con hallazgo
7. La serie histórica del vehículo, que es donde se ve si esta misión es un caso aislado o el vigésimo del semestre

## Trazabilidad

- **Autoridad de transiciones:** [`EF-05` conciliación al retornar, `H-01` y §7.2](../../03-arquitectura/estados/orden-de-mision.md), [§10.1](../../03-arquitectura/estados/orden-de-mision.md) estados `CONCILIADA` / `CONCILIADA_CON_DESVIACION`
- **Reglas:** [RN-30](../../01-negocio/reglas/RN-30-conciliacion-galonaje-kilometraje.md) (regla eje), [RN-28](../../01-negocio/reglas/RN-28-comprobacion-del-consumo-de-combustible.md), [RN-29](../../01-negocio/reglas/RN-29-liquidacion-de-combustible.md), [RN-31](../../01-negocio/reglas/RN-31-odometro-de-retorno.md), [RN-37](../../01-negocio/reglas/RN-37-coherencia-de-la-secuencia-de-casetas.md), [RN-40](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md), [RN-41](../../01-negocio/reglas/RN-41-congelamiento-del-valor-al-autorizar.md), [RN-08](../../01-negocio/reglas/RN-08-cadena-de-trazabilidad-para-cierre.md)
- **Puntos de control:** `PC-11` (coherencia del odómetro), `PC-13` (segregación de liquidación y cierre) de [PR-01](../../01-negocio/procesos/PR-01-movilizacion-institucional.md)
- **Normas:** [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md) TSC-NOGECI V-10 y V-14, y el patrón de hallazgo en flota `[V]`; [NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md)
- **Actores:** ACT-04, ACT-06, ACT-07, ACT-08, ACT-12
- **Casos relacionados:** [CE-22](CE-22-odometro-inconsistente.md), [CE-25](CE-25-comprobante-perdido-o-estacion-sin-factura.md), [CE-26](CE-26-sobrante-o-faltante-al-liquidar.md), [CE-28](CE-28-hallazgo-posterior-sobre-mision-cerrada.md)
- **Insumos:** #1 (umbrales de desviación por tipo de vehículo, rendimiento esperado), #19 (informes de auditoría: cada hallazgo pasado describe algo que salió mal de verdad)
