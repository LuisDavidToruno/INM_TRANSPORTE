# Requisitos no funcionales `RNF-xx`

21 requisitos. **Un requisito no funcional que no se puede medir es una aspiración**: cada uno lleva umbral numérico, forma concreta de comprobarlo, consecuencia real de incumplirlo y trazabilidad.

Plantilla: [`docs/plantillas/requisito-no-funcional.md`](../../plantillas/requisito-no-funcional.md).

## Índice

### Operación desconectada y campo

| ID | Requisito | Prioridad | Arq. |
|---|---|---|---|
| [RNF-03](RNF-03-operacion-sin-conectividad.md) | El cliente de campo opera sin conectividad durante toda una misión | Crítico | **Determinante** |
| [RNF-12](RNF-12-uso-en-campo.md) | Se usa a pleno sol, con guantes, en celular de gama baja y con la batería contada | Crítico | **Determinante** |
| [RNF-08](RNF-08-seguimiento-en-ruta.md) | El tablero dice dónde está cada vehículo, o dice desde cuándo no lo sabe | Alto | Sí |

### Auditoría, temporalidad y prueba

| ID | Requisito | Prioridad | Arq. |
|---|---|---|---|
| [RNF-04](RNF-04-bitacora-append-only-con-hash-encadenado.md) | Bitácora append-only que detecta su alteración incluso por quien administra el servidor | Crítico | **Determinante** |
| [RNF-05](RNF-05-temporalidad-normativa.md) | Parámetros con vigencia; todo cálculo a la fecha del hecho | Crítico | **Determinante** |
| [RNF-06](RNF-06-reproducibilidad-historica-de-reportes.md) | Un reporte regenerado con la misma fecha de corte da el mismo resultado, siempre | Crítico | Sí |
| [RNF-21](RNF-21-integridad-de-folios-y-correlativos.md) | Ningún folio se duplica ni se recicla, aunque se emita sin red | Crítico | **Determinante** |
| [RNF-18](RNF-18-paquetes-de-evidencia-para-auditoria.md) | El expediente que pide el auditor se entrega el mismo día, completo | Alto | No |

### Rendimiento y volumen

| ID | Requisito | Prioridad | Arq. |
|---|---|---|---|
| [RNF-01](RNF-01-rendimiento-de-consulta-y-operacion.md) | Toda pantalla responde bajo umbral con el histórico completo en línea | Alto | Sí |
| [RNF-02](RNF-02-volumen-y-crecimiento-del-acervo.md) | Soporta el crecimiento del acervo sin que nada se borre nunca | Alto | **Determinante** |

### Integración

| ID | Requisito | Prioridad | Arq. |
|---|---|---|---|
| [RNF-07](RNF-07-sincronizacion-del-espejo-local.md) | El espejo de ARGOS y Talento Humano nunca diverge en silencio | Crítico | **Determinante** |

### Operación on-premise sin equipo de TI

| ID | Requisito | Prioridad | Arq. |
|---|---|---|---|
| [RNF-09](RNF-09-instalacion-respaldo-y-restauracion.md) | Instalar, respaldar y restaurar lo hace alguien sin especialización | Crítico | **Determinante** |
| [RNF-10](RNF-10-disponibilidad-y-recuperacion.md) | La caída del servidor no detiene la operación de campo | Alto | Sí |
| [RNF-20](RNF-20-observabilidad-y-diagnostico.md) | Una sola pantalla dice qué está mal y qué hacer | Alto | Sí |
| [RNF-19](RNF-19-configurabilidad-multi-institucion.md) | Una segunda institución se pone en marcha cargando catálogos | Alto | Sí |

### Seguridad y datos personales

| ID | Requisito | Prioridad | Arq. |
|---|---|---|---|
| [RNF-13](RNF-13-cifrado-en-transito-y-en-reposo.md) | Nada personal viaja ni reposa en claro; el celular perdido no es una fuga | Crítico | Sí |
| [RNF-14](RNF-14-control-de-acceso-por-puesto-y-registro-de-consultas.md) | Permisos por puesto, alcance verificado en cada consulta, consultas registradas | Crítico | Sí |
| [RNF-17](RNF-17-retencion-y-depuracion-diferenciada.md) | Los datos personales se depuran sin romper la cadena de auditoría | Alto | **Determinante** |
| [RNF-15](RNF-15-continuidad-ante-rotacion-de-personal.md) | Un cambio de administración no deja expedientes huérfanos ni reescribe la autoría | Alto | Sí |

### Papel y personas

| ID | Requisito | Prioridad | Arq. |
|---|---|---|---|
| [RNF-11](RNF-11-formatos-oficiales-imprimibles-y-verificables.md) | Todo documento oficial se imprime en la impresora que la delegación ya tiene | Crítico | Sí |
| [RNF-16](RNF-16-idioma-accesibilidad-y-mensajes.md) | Español del dominio; ningún bloqueo deja al usuario sin saber qué hacer | Alto | No |

## Los nueve que van a decidir el stack

El [`ADR-000`](../../03-arquitectura/adr/ADR-000-diferir-seleccion-de-stack.md) difiere la selección de stack al Sprint 2 precisamente para que se decida contra restricciones conocidas. **Estos nueve son esas restricciones**, y el ADR de stack debe evaluarse explícitamente contra cada uno:

| ID | Qué restringe |
|---|---|
| [RNF-02](RNF-02-volumen-y-crecimiento-del-acervo.md) | Acervo monótonamente creciente, sin borrado, con archivado consultable |
| [RNF-03](RNF-03-operacion-sin-conectividad.md) | Almacenamiento local persistente y sincronización diferida. Descarta arquitecturas puramente server-rendered |
| [RNF-04](RNF-04-bitacora-append-only-con-hash-encadenado.md) | Persistencia append-only con hash encadenado y anclaje externo del sello |
| [RNF-05](RNF-05-temporalidad-normativa.md) | Bitemporalidad en el modelo de datos completo |
| [RNF-07](RNF-07-sincronizacion-del-espejo-local.md) | Cola persistente con reintento, reconciliación programada y degradación explícita |
| [RNF-09](RNF-09-instalacion-respaldo-y-restauracion.md) | **Descarta directamente todo stack cuya operación exija un especialista** |
| [RNF-12](RNF-12-uso-en-campo.md) | Cliente de campo sobre dispositivo de gama baja, con techo de batería, datos y almacenamiento |
| [RNF-17](RNF-17-retencion-y-depuracion-diferenciada.md) | Segmento de datos personales separable de la cadena de auditoría |
| [RNF-21](RNF-21-integridad-de-folios-y-correlativos.md) | Identificadores generados en cliente y rangos de folio pre-asignados |

Los tres primeros y el `RNF-09` son los que más probabilidades tienen de eliminar candidatos por sí solos.

## Las dos tensiones que el diseño tiene que resolver, no esquivar

**1. [`RNF-04`](RNF-04-bitacora-append-only-con-hash-encadenado.md) contra [`RNF-17`](RNF-17-retencion-y-depuracion-diferenciada.md)** — el control interno exige conservar todo y encadenarlo; la protección de datos exige depurar lo personal antes. La resolución es estructural: la cadena encadena una referencia y una huella, no el contenido personal en claro. **Si esto se decide después de tener años de cadena construida, no se puede corregir.**

**2. [`RNF-14`](RNF-14-control-de-acceso-por-puesto-y-registro-de-consultas.md) contra la aritmética de las delegaciones** — el MARCI exige cinco funciones incompatibles; una delegación de tres personas no puede cumplirlo. El sistema **no inventa** un régimen de excepción: bloquea hasta que Auditoría Interna se pronuncie (insumo #26). La consecuencia práctica es que las delegaciones pequeñas no podrán despachar dentro del sistema mientras ese insumo siga abierto. **Es un riesgo de despliegue, no un detalle de configuración.**

## Umbrales que quedaron `[C]`

Ninguno se inventó. El más determinante es el volumen: el insumo #10 se resolvió como *"alto flujo"* sin cifras, así que el juego de datos de referencia `JDR-1` del [`RNF-01`](RNF-01-rendimiento-de-consulta-y-operacion.md) es una **derivación aritmética `[I]` sobre entradas `[C]`**. Si el insumo #67 devuelve cifras distintas, `JDR-1` se rehace y los umbrales se remiden — no se ajusta el umbral para que la implementación pase.

| Umbral abierto | Requisitos afectados | Insumo |
|---|---|---|
| Volumen operativo cifrado: flota, delegaciones, usuarios, concurrencia, duración máxima de misión | `RNF-01`, `RNF-02`, `RNF-03` | #67 (nuevo) |
| Plazo de conservación y plazo de depuración de datos personales | `RNF-02`, `RNF-04`, `RNF-13`, `RNF-17` | #71 (nuevo) |
| Ventana de mantenimiento, RPO/RTO tolerados, periodicidad de reconciliación | `RNF-07`, `RNF-09`, `RNF-10` | #72 (nuevo) |
| Perfil del responsable del servidor, canal de aviso, custodia de claves | `RNF-09`, `RNF-13`, `RNF-20` | #73 (nuevo) |
| Dispositivo de campo de referencia y quién lo provee | `RNF-12`, `RNF-08`, `RNF-11` | #69 (nuevo) |
| Parque real de impresoras y tamaño de papel | `RNF-11` | #70 (nuevo) |
| Frecuencia de posición aceptable y quién paga los datos móviles | `RNF-08` | #74 (nuevo) |
| Horario hábil, plazos, umbrales de desviación | `RNF-05`, `RNF-10`, `RNF-19` | #32 |
| Formato del correlativo institucional; folio del talonario preimpreso | `RNF-21`, `RNF-11` | #34, #46 |
| Excepción a la segregación de funciones y dotación real de delegaciones | `RNF-14` | #26, #27 |
| Tarifas de peaje, exoneraciones, matriz licencia↔vehículo definitiva | `RNF-05`, `RNF-19` | #20, #21, #22 |

## Cómo se usan estos requisitos

- Los `RNF` **no son un anexo del Sprint 2**. Cinco de ellos —`RNF-03`, `RNF-04`, `RNF-05`, `RNF-17`, `RNF-21`— describen propiedades que **no se pueden agregar después**: o están en el modelo desde el primer día, o hay que rehacerlo.
- Toda historia de usuario del Bloque 3 que toque campo, auditoría, cálculo normativo o impresión **cita el `RNF` que la condiciona**.
- Cada `RNF` trae su batería de verificación redactada para ser ejecutada por alguien. Esas baterías son la base del plan de pruebas, no una lista de buenas intenciones.
- Los umbrales `[C]` **no se rellenan por inferencia** al empezar a construir. Se levantan con la institución o el requisito se construye con el parámetro vacío y bloqueante ([`RNF-19`](RNF-19-configurabilidad-multi-institucion.md)).
