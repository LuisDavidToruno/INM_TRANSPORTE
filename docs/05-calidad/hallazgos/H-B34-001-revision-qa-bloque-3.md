# H-B34-001 — Revisión adversarial del Bloque 3

| Campo | Valor |
|---|---|
| **Alcance** | 125 historias `HU-001`–`HU-125`, 18 casos de uso, 21 `RNF-xx`, 97 `RN-xx` en lo que las historias las citan, `docs/07-gestion/backlog.md` |
| **Fecha** | 2026-08-24 |
| **Ambiente** | Sprint 0 — documentación. No hay código, no hay datos reales, no hay producción. La severidad mide **la consecuencia si el artefacto llega a código tal como está**, no un incidente |
| **Estado** | **Los 21 llevan nota de corrección en su artefacto.** Verificados por muestreo, no uno por uno — ver la sección siguiente |
| **Verificación de cierre** | 2026-08-26, contra los artefactos vivos |
| **No repite** | Los 19 hallazgos de [`H-B3-001`](H-B3-001-hallazgos-de-casos-de-uso.md) |


## Estado de corrección — verificado el 2026-08-26

**Los 21 hallazgos aparecen citados por su identificador en el artefacto que corrigen**, casi siempre bajo una *«Nota de corrección»* que explica qué estaba mal y qué manda. Ochenta y cuatro archivos de `docs/` llevan hoy una referencia de ese tipo.

**Lo que se verificó y lo que no.** Se comprobó uno por uno que cada identificador esté citado, y se abrieron a lectura los cuatro de mayor consecuencia. **No se releyeron los veintiuno completos.** Quien necesite certeza sobre uno concreto tiene que abrir su artículo, no fiarse de esta línea.

| Verificado a fondo | Qué se comprobó |
|---|---|
| `HB34-01` | [`HU-061`](../../02-requisitos/historias/HU-061-relevo-de-motorista-en-ruta.md) abre con la nota de corrección: era la misma historia que [`HU-045`](../../02-requisitos/historias/HU-045-relevo-de-motorista-en-ruta.md) y resolvían distinto el mismo caso. Zanjado por [`RN-10`](../../01-negocio/reglas/RN-10-licencia-vigente-en-todo-el-rango.md), que aplica expresamente a la sustitución en ruta. **Es la regla que el código implementa hoy** |
| `HB34-02` | Cubierto en la misma nota: `I-11` bloquea ahora en el momento en que se asigna el motorista |
| `HB34-05` | M-01 y M-02 ya tienen historias — 34 archivos las citan |
| `HB34-07` | [`HU-088`](../../02-requisitos/historias/HU-088-conciliar-galonaje-contra-kilometraje.md) |

Los restantes —`HB34-03`, `HB34-04`, `HB34-06`, `HB34-08` a `HB34-21`— están citados en sus historias y en [`backlog.md`](../../07-gestion/backlog.md); no se auditó el contenido de cada corrección.
## Lo que la revisión encontró bien, para no perder tiempo buscándolo otra vez

Estas comprobaciones se corrieron sobre las 125 historias y **salieron limpias**. No hay hallazgo en ellas:

- **Trazabilidad `RN`:** las 125 citan al menos una regla en su sección *Reglas que la gobiernan*. **Cero** referencias a reglas inexistentes. **Cero** reglas huérfanas: las 97, incluidas las nuevas `RN-55` a `RN-97`, están citadas por al menos una historia.
- **Módulo:** las 125 lo declaran.
- **Casos especiales:** los 28 `CE-xx` están cubiertos. El menos cubierto es `CE-03` con 3 historias.
- **Un `Cuando` por escenario:** 919 escenarios revisados, **cero** con dos `Cuando` y cero con ninguno.
- **Alcance / `DP-001`:** no hay fuga. Ninguna historia calcula viáticos, ninguna gestiona contratos con proveedores ni tarjetas de flota, ninguna asume firma electrónica certificada — nueve historias la descartan explícitamente citando `NRM-08`.
- **Números normativos cableados:** no encontré ninguno. Los valores que aparecen en los mensajes (umbral 20 %, 72 h, 3 días, 180 días) están declarados como parámetro en los `Antecedentes` del mismo escenario. La afirmación del `README` se sostiene, con **una excepción**: `HB34-07`.

Lo que sigue es lo que no salió limpio.

---

# Críticos

## `HB34-01` — `HU-045` y `HU-061` son la misma historia y se contradicen en el bloqueo de licencia

| | |
|---|---|
| **Artefactos** | `HU-045-relevar-al-motorista...`, `HU-061-relevo-de-motorista-en-ruta`, `README.md` de historias, `backlog.md` |
| **Autoridad aplicable** | `RN-10`, `RN-55` |

Las dos historias tienen **título equivalente**, el mismo actor, la misma regla rectora (`RN-71`), los mismos casos especiales (`CE-05`, `CE-10`, `CE-11`) y el mismo objeto: el acta de traspaso en ruta con corte de odómetro. **No están en la tabla de solapamientos del `README`.** Ninguna de las dos se declara fuera del alcance de la otra.

Y no son copias: **resuelven distinto el mismo caso**, en un área de rigor máximo.

**El caso concreto.** Misión `OM-2026-0451` `EN_RUTA`, retorno previsto el día 17. El relevo declarado tiene licencia habilitante que **vence el día 16**, el mismo día del traspaso.

| | `HU-061` | `HU-045` |
|---|---|---|
| Escenario | *Se rechaza el relevo con licencia vencida dentro de la ventana autorizada* | *La licencia del conductor del tramo vence durante la misión* |
| Resultado | **Rechaza el traspaso.** *"La licencia de Marvin Cruz vence el 15/05/2026, antes del retorno previsto el 16/05/2026."* | **Acepta.** *"el sistema no detiene la ejecución"*, se marca para cerrar con hallazgo |

`RN-10` es la autoridad y dice, en *Condiciones de aplicación*: **«Aplica a toda asignación, despacho, sustitución en ruta y extensión de misión.»** El relevo es sustitución en ruta. `RN-55` gobierna el vencimiento **sobrevenido** —el que ocurre con el conductor ya al volante— y ella misma delimita: *«No aplica antes de la salida: ahí manda `RN-10`»*, y en sus casos límite manda el relevo *«con revalidación completa del entrante»*.

**`HU-061` es correcta. `HU-045` invocó `RN-55` para un acto de asignación y con eso convirtió un bloqueo duro en un hallazgo posterior.** Es exactamente el patrón que `RN-10` describe como el error clásico.

Diferencias adicionales entre ambas, todas sin resolver:

| Punto | `HU-045` | `HU-061` |
|---|---|---|
| Código de autorización fuera de línea | **Obligatorio.** Escenario propio que rechaza el relevo sin él | **No lo menciona.** Su camino feliz ocurre a 3 días sin conectividad y sin código |
| Relevo no declarado en la programación | No lo evalúa | Lo rechaza con mensaje propio |
| Módulo | M-08 · M-07 · M-16 | M-08 · M-05 |
| Estado DoR | Refinada | Borrador |
| Sprint (backlog) | **5** | **7** |

**Corregir:** delimitar el par en el `README`, decidir cuál manda, y alinear el tratamiento de la licencia del entrante con `RN-10` — bloqueo duro. Consolidar en un solo sprint: hoy el relevo se construye dos veces, con dos reglas distintas, con dos sprints de separación.

## `HB34-02` — `I-11`, el «núcleo irreductible», no tiene historia que lo bloquee en el momento en que se asigna el motorista

| | |
|---|---|
| **Artefactos** | `HU-025`, `HU-027`, `HU-043`, `HU-045`, `HU-061` |
| **Autoridad aplicable** | `RN-01`, comportamiento esperado n.º 1 |

`RN-01` dice literalmente: *«Antes de registrar cualquier acto de control **y antes de asignar o sustituir al motorista**, el sistema compara la identidad de la persona actuante o entrante contra las identidades ya registradas en las demás funciones de la misma Orden de Misión.»* Y califica `I-11` —conducir × autorizar/despachar/entregar combustible/liquidar— de *«núcleo irreductible: bloqueo duro que no se levanta por régimen de excepción, ni por delegación, ni por emergencia, ni por resolución de la máxima autoridad»*.

**Ninguna de las cinco historias que asignan, reservan, sustituyen o relevan al motorista menciona `RN-01`, `I-11` ni la palabra segregación.** Verificado por búsqueda directa: `HU-025`, `HU-027`, `HU-043`, `HU-045` y `HU-061` no la citan en ninguna sección.

Lo que sí existe: `HU-010` (al autorizar), `HU-039` (al despachar), `HU-073` y `HU-079` (fondo), `HU-091` (al liquidar). Todos ellos disparan **por el acto de control**, no por la asignación.

**El caso concreto.** Carlos Rodríguez, `ACT-03`, autoriza `OM-2026-0451` el lunes. Tiene licencia categoría B vigente y figura en el padrón. El martes el Jefe de Transporte lo asigna como motorista de esa misma misión.

1. `HU-025` verifica su habilitación → **pasa**, la licencia cubre el vehículo y el rango.
2. `HU-027` reserva vehículo y motorista → **pasa**, no hay conflicto de franja.
3. `HU-043`, si hubiera sustitución, revalida licencia, categoría de peaje, reserva y ámbito → **pasa**; su lista de revalidaciones no incluye segregación.
4. `HU-039` no se dispara: quien despacha es Mario Fúnez.

La misión sale con el autorizador al volante. Se detecta en `RN-01` n.º 5 —verificación de la matriz al cerrar— es decir, **después de que ocurrió**, y el único efecto es `CERRADA_CON_HALLAZGO`. El bloqueo declarado sin excepción posible resulta ser, en las historias escritas, una detección posterior.

Nota: `HU-039` sí cubre el caso inverso *(«José Martínez es el motorista de esta misión. El motorista no puede despachar…»)*. Es la mitad simétrica. `RN-01` dice explícitamente que **el sistema bloquea el segundo acto, sea cual sea el orden** — y la otra mitad no está escrita.

**Corregir:** `HU-025` y `HU-043` deben citar `RN-01` y tener escenario de rechazo por `I-11` con mensaje; `HU-045`/`HU-061` deben evaluarlo sobre el entrante.

## `HB34-03` — `HU-004` y `HU-009` bloquean donde `RN-50` prohíbe bloquear, y paralizan la delegación sin cobertura

| | |
|---|---|
| **Artefactos** | `HU-004`, `HU-009` frente a `HU-026`; `RNF-07`; casos límite de `RN-10` |
| **Autoridad aplicable** | `RN-50` |

`RN-50` es la regla de la materia y su enunciado no admite lectura: *«Superado el umbral de advertencia, toda operación sensible debe mostrar la antigüedad del dato antes de continuar, exigir acuse y registrar la advertencia. **La operación no se impide: se marca.**»* Su ficha declara **Tipo: Advertencia con acuse registrado**, y añade: *«El bloqueo por desincronización **no está decidido**»*. Las operaciones sensibles que nombra son, textualmente, *asignar motorista, autorizar una orden de misión, aprobar un fondo y liquidar*.

Tres historias implementan la misma regla de tres maneras:

| Historia | Umbral | Comportamiento |
|---|---|---|
| `HU-026` asignar motorista | espejo de 9 días, umbral 3 | **Advierte, exige acuse, la advertencia se imprime en la Orden.** Correcto |
| `HU-004` enviar a autorización | 98 h, umbral 72 | **Bloquea.** *"…el umbral de **bloqueo** es de 72. No se encamina un expediente contra una jerarquía que puede ya no existir (RN-50)."* |
| `HU-009` autorizar | 98 h, umbral 72 | **Bloquea.** Mismo mensaje |

Las dos que bloquean **citan `RN-50` como fundamento del bloqueo que `RN-50` niega**.

**El caso concreto, y es el caso de diseño del producto, no un borde.** La Delegación Choluteca lleva cuatro días sin enlace con la sede — 96 horas, por encima de 72. Con `HU-004`/`HU-009` en el código, **ningún solicitante de esa delegación puede enviar una solicitud a autorización y ninguna jefatura puede autorizarla**, hasta que vuelva el enlace. La premisa rectora 5 es *offline-first, no «con soporte offline»*, y `HU-007` existe precisamente para capturar y enviar la solicitud sin conectividad. `HU-004` la anula a las 72 horas.

Contaminación aguas arriba y abajo: `RNF-07` lista *«Antigüedad del espejo que **bloquea** operaciones sensibles (asignar motorista, aprobar contra estructura de autorización) → > 72 h»*, y los casos límite de `RN-10` afirman *«superado el umbral, **la asignación de motoristas se bloquea**»*. Ambos contradicen a `RN-50`, que es la autoridad en esta materia. Es escalada por copia: la regla dice advertencia, el `RNF` dice bloqueo, dos historias lo codifican como bloqueo.

**Corregir:** `HU-004` y `HU-009` pasan a advertencia con acuse nominativo y marca impresa, alineadas con `HU-026`; `RNF-07` y los casos límite de `RN-10` citan `RN-50` en lugar de reescribirla. Si el PO quiere el escalón de bloqueo, se decide en `RN-50` —que ya lo tiene abierto como pendiente— y baja de ahí.

## `HB34-04` — `HU-041` está en el Sprint 5 y su precondición completa está en el Sprint 6

| | |
|---|---|
| **Artefactos** | `backlog.md`, `HU-041`, `HU-071`–`HU-075`, `HU-077` |

`HU-041` cita `RN-26` —*«Sin fondo vigente aprobado no hay asignación»*— y `RN-88` —saldo proyectado—, y tiene escenario propio: *«Se rechaza emitir la asignación sin fondo vigente aprobado»*, más otro que advierte *«el comprometido proyectado sube a 16,000.00 lempiras y supera el saldo disponible de 15,000.00»*.

El backlog pone `HU-041` en el **Sprint 5** (dentro del rango `HU-038`–`HU-045`) y el ciclo entero del fondo —solicitarlo `HU-071`, aprobarlo `HU-072`, entregarlo y custodiarlo `HU-074`, ampliarlo `HU-075`— en el **Sprint 6**.

**El caso concreto:** al final del Sprint 5 no existe ninguna entidad *fondo*, ningún saldo y ningún comprometido proyectado. Los dos escenarios citados de `HU-041` no se pueden ejecutar, y su escenario feliz —emitir la asignación al programar— solo se puede implementar inventando un fondo ficticio que en el Sprint 6 hay que reemplazar.

Agravante: el propio backlog declara que el insumo **#7 / `PROP-01`** (periodicidad del fondo) *«condiciona el Sprint 6 completo»* y lo lista entre los dos únicos insumos que bloquean construcción. `HU-041` cita ese mismo insumo en sus notas — y está en el Sprint 5.

**Corregir:** mover `HU-041` al Sprint 6 junto a `HU-076`/`HU-079`, o adelantar el núcleo mínimo del fondo (`HU-071`, `HU-072`, `HU-074`) al Sprint 5. La segunda opción arrastra `PROP-01` un sprint antes.

## `HB34-05` — M-01 y M-02 no tienen ni una sola historia, y el Sprint 3 se llama «Catálogos, flota y motoristas»

| | |
|---|---|
| **Artefactos** | `backlog.md` Sprint 3, `README.md` de historias, `RNF-15`, `RNF-19` |

Conteo por módulo primario sobre las 125:

| Módulo | Historias con ese módulo primario |
|---|---|
| **M-01 Organización y Seguridad** | **0** (aparece como módulo secundario en 3) |
| **M-02 Catálogos Maestros** | **0** (secundario en 4) |
| M-11 Mantenimiento y Taller | 0 |
| M-12 Incidentes, Siniestros y Sanciones | 0 |

M-11 y M-12 quedan fuera legítimamente: ningún caso de uso del Bloque 3 los desarrolló. **M-01 y M-02 no.** El backlog los declara cimiento —criterio de priorización n.º 1: *«Lo que nada funciona sin ello va primero — organización, catálogos, flota y motoristas»*— y después asigna al Sprint 3 únicamente `HU-096`–`HU-104` (flota) y `HU-105`–`HU-110` (motoristas). **Los catálogos y la organización no tienen historia que asignar.**

**Los casos concretos, todos tomados de `Antecedentes` de historias ya escritas:**

1. `HU-001` (Sprint 4) empieza con *«un catálogo de motivos de viaje vigente al 2026-03-14»* y *«un catálogo de tipos de vehículo vigente al 2026-03-14»*. Ninguna historia los da de alta ni les pone vigencia.
2. `HU-001` exige *«un Solicitante con rol vigente sobre la dependencia Subgerencia de Operaciones»* y su último escenario prueba que otro solicitante de la misma dependencia **no ve** el borrador. Eso es alcance de datos de M-01: no hay historia que cree institución, dependencia, delegación, usuario, rol ni ámbito.
3. `HU-043` rechaza *«Esta misión no pertenece al ámbito de la Delegación Choluteca»*. El ámbito de competencia no tiene historia.
4. `HU-005` (**Sprint 4**) estima el peaje punto por punto con tarifas vigentes. La única historia que toca la carga y puesta en vigencia del catálogo de tarifas es `HU-086` —*«ACT-01 Administrador del Sistema (carga) · ACT-08 Gerencia Administrativa (pone en vigencia)»*—, y está en el **Sprint 6**. `HU-005` consume dos sprints antes lo que `HU-086` produce.
5. **`RNF-15` queda sin cobertura funcional en todo el Bloque 3.** Exige suplencia con vigencia por rango de fechas, traspaso masivo de ≥ 50 custodias en una operación, baja de usuario que no deja expediente huérfano, y cero cuentas compartidas. `HU-099` traspasa custodia de a un vehículo; `HU-074` traspasa el fondo por rotación. Nada más. Es un `RNF` de prioridad **Alto** motivado por una realidad verificada `[V]` —cambio de gobierno en enero de 2026— sin una sola historia que lo haga verificable.
6. **`RNF-19` configurabilidad multi-institución** — el producto es genérico por definición y no hay historia que lo ejercite.

**Corregir:** escribir el lote de M-01 y M-02 antes de cerrar el Sprint 0, y reordenar el Sprint 3. Sin esto, el Sprint 3 no puede arrancar: `HU-096` da de alta un vehículo dentro de una institución, una dependencia y un tipo de vehículo que ninguna historia crea.

---

# Altos

## `HB34-06` — `HU-019` y `HU-035` duplican la verificación por QR, y entre las dos hay tres lecturas de qué invalida un salvoconducto

| | |
|---|---|
| **Artefactos** | `HU-019`, `HU-035`, `HU-045`; `README.md` de historias |

Mismo actor `ACT-15` no autenticado, mismo módulo M-15, misma regla rectora `RN-25`, mismos cuatro estados —vigente, anulado, vencido, **desactualizado**—, mismo mínimo verificable, mismo registro de consulta fallida. Ambas usan un salvoconducto como antecedente. **No están en la tabla de solapamientos** y ninguna se excluye de la otra: el *Fuera de alcance* de `HU-019` remite la verificación de otros documentos a *«historias propias de M-15»* sin nombrar a `HU-035`, que ya existe y cubre también el salvoconducto.

**Primera divergencia — qué produce `DESACTUALIZADO`.** Es la pregunta que decide qué pasa en un retén.

| Artefacto | Qué invalida el papel |
|---|---|
| `HU-019` | *«un cambio posterior de la **ruta** amparada»* |
| `HU-035` | *«la Orden de Misión vinculada cambió de **motorista**»* |
| `HU-045` | **Ninguno de los dos.** *«Se sigue a `BD-04`»* — vehículo y ventana |

**El caso concreto.** Domingo 14:30, misión con salvoconducto impreso `SC-2026-0087`. Se registra el relevo de motorista en Comayagua conforme a `HU-045`, que anota expresamente que **no** exige reemisión del permiso. A las 16:00 un agente escanea el QR:

- Si el sistema se construyó con `HU-035` → responde **`DESACTUALIZADO`**: *«El expediente de esta misión se modificó después de la impresión. Solicite el documento vigente.»* El vehículo queda detenido en carretera con un permiso que su propia historia de relevo consideró válido.
- Si se construyó con `HU-019` o con `HU-045` → responde **`VIGENTE`**.

`HB3-07` ya había detectado las tres redacciones del alcance del salvoconducto (`BD-04` / `PC-03` / `RN-23`) y registró *«Resolución adoptada: la más exigente»*. **Las historias no aplicaron esa resolución**: `HU-045` sigue expresamente a `BD-04`, que es la menos exigente de las tres.

**Segunda divergencia — el mensaje de folio inexistente.** `HU-019` responde *"Folio no encontrado"* y añade el criterio *«no revela si el rango de folios existe»*. `HU-035` responde *"Folio no encontrado. Este documento no fue emitido por la institución."* Dos mensajes para el mismo evento, y el segundo afirma algo sobre el emisor que el primero evita a propósito.

**Tercera divergencia — el veredicto DoR.** Ambas declaran **el mismo** `[C]` bloqueante: si la institución acepta exponer un punto de verificación público con despliegue on-premise (pendiente G). `HU-019` está **Borrador — bloqueada por** ese pendiente. `HU-035` está **Refinada**. El mismo insumo abierto produce dos estados opuestos.

## `HB34-07` — `HU-088`: la tabla de ejemplos contradice los umbrales de sus propios antecedentes

| | |
|---|---|
| **Artefactos** | `HU-088-conciliar-galonaje-contra-kilometraje` |

Área de rigor máximo n.º 4. La historia es de las mejores del lote —cubre la desviación en ambas direcciones, excluye el kilometraje bajo tenencia ajena, no concluye con odómetro averiado, concilia por vehículo cuando hubo traspaso—. Y tiene un defecto que hace **indeterminable** su criterio central.

`Antecedentes`:

```
Y un umbral de desviación superior tolerada del "15" por ciento
Y un umbral de desviación inferior tolerada del "20" por ciento
```

`Esquema del escenario — Clasificación de la desviación`, con rendimiento esperado 12.0 km/gal:

| km | galones | observado | desviación | clasificación declarada |
|---|---|---|---|---|
| 360 | 31.0 | 11.61 | 3.2 % por debajo | CONFORME |
| 360 | 36.0 | **10.00** | **16.7 % por debajo** | **REVISAR** |
| 240 | 40.0 | 6.00 | 50 % por debajo | CONSUMO_EXCEDIDO |
| 600 | 20.0 | 30.00 | 150 % por encima | RENDIMIENTO_ANOMALO_SUPERIOR |

La segunda fila, **16.7 % por debajo, está dentro del 20 % de tolerancia inferior declarado en los antecedentes**. Con los umbrales que la propia historia fija, ese caso es `CONFORME`. La tabla dice `REVISAR`.

`REVISAR` es una tercera clasificación **sin ningún umbral que la defina**. Un revisor externo no puede determinar si `360 km / 36 galones` cumple o no cumple — que es justo lo que el DoR exige de un criterio observable, y sobre el control que más mira el auditor.

Origen probable: la tabla se heredó de la plantilla `criterios-aceptacion-gherkin.md`, que usa un solo umbral del 15 %. Al desdoblar el umbral en superior e inferior, la tabla no se recalculó.

**Corregir:** declarar los tres umbrales —tolerancia, banda de revisión, hallazgo— en ambas direcciones, y recalcular la tabla. Ojo con no hacer lo contrario: ajustar el umbral para que la tabla pase. Las notas de la historia ya advierten que los valores son ilustrativos; lo que falta es que sean **coherentes** entre sí.

## `HB34-08` — El código de autorización fuera de línea se consume en el Sprint 5 y se genera en el Sprint 7

| | |
|---|---|
| **Artefactos** | `backlog.md`; `HU-055` frente a `HU-037`, `HU-039`, `HU-045` |

`HU-055` —*«ACT-04 Jefe de Transporte **genera** el código»*, *«un código que me dicta el Jefe de Transporte por radio o por teléfono»*, *«código de un solo uso con ventana corta»*— es la única historia que define el mecanismo. Está en el **Sprint 7**.

Lo consumen, en el **Sprint 5**:

- `HU-045`, con un **bloqueo duro**: *«Se rechaza el relevo sin código de autorización fuera de línea → "El relevo sin conectividad requiere el código de autorización fuera de línea de esta misión."»*
- `HU-037`, emisión anticipada en delegación sin cobertura.
- `HU-039` y `HU-011`, que lo mencionan en el marco de la autorización.

**El caso concreto:** en el Sprint 5 se implementa un rechazo contra un código que ningún componente sabe emitir ni validar. El resultado práctico es que el relevo sin conectividad —el escenario que la historia existe para resolver— queda imposible durante dos sprints, o se implementa un código provisional que hay que rehacer.

## `HB34-09` — La sincronización se entrega en el Sprint 5 sin el protocolo de huecos ni la cola de conflictos

| | |
|---|---|
| **Artefactos** | `backlog.md`; `HU-066` frente a `HU-067`, `HU-068`; `RN-45`, `RNF-03` |

El backlog adelanta al Sprint 5 `HU-046`, `HU-054` y `HU-066`, con el argumento —correcto— de que offline es propiedad del cliente y no un módulo posterior. Pero deja en el Sprint 7 `HU-067` (resultado registro por registro y retención por hueco de secuencia) y `HU-068` (cola de conflictos con las dos versiones lado a lado).

`HU-066` declara *«El servidor aplica en orden de secuencia, no en orden de llegada»* — y **no dice qué hacer cuando falta un número de la secuencia**. Eso lo define `HU-067`: *«llega la 41 y falta la 40 → el servidor no aplica ni rechaza: retiene la 41 en espera de su predecesor. Nunca aplica una transición saltando una faltante, porque eso produciría una misión `RETORNADA` sin odómetro de salida.»*

**El caso concreto.** El dispositivo `DEL-CHO-03` envía los registros 38, 39 y 41; el 40 —el odómetro de salida— se perdió en el envío. En un Sprint 5 que solo tiene `HU-066`, el comportamiento no está especificado: aplicar el 41 produce la misión `RETORNADA` sin odómetro de salida que `HU-067` describe; descartarlo viola `RN-45` (cero pérdida, cero sobrescritura silenciosa).

Segundo caso, dentro de la propia `HU-066`: su escenario *«Marca de tiempo incoherente»* termina en *«muestra al responsable: "El evento dice que ocurrió después de haberse registrado. Alguien tiene que revisarlo."»*. **La pantalla donde el responsable ve eso es `HU-068`, Sprint 7.** En el Sprint 5 el mensaje no tiene dónde aparecer.

**Corregir:** `HU-067` no es «cola avanzada», es el protocolo de aplicación. Sube al Sprint 5 con `HU-066`, o `HU-066` incorpora la regla de retención. La cola con resolución humana (`HU-068`) sí puede esperar, pero entonces el conflicto tiene que quedar visible en algún lado durante dos sprints.

## `HB34-10` — Los ocho pares delimitados quedaron partidos entre sprints, y en cuatro la historia que «manda» va después

| | |
|---|---|
| **Artefactos** | `README.md` de historias (tabla de delimitación) frente a `backlog.md` |

El `README` fija la regla: *«el lote de flujo manda en el acto y su momento; el lote de expediente manda en el dato y su ciclo de vida»*. El backlog reparte los pares así:

| Par | Quién manda | Sprint del que manda | Sprint del otro |
|---|---|---|---|
| `HU-041` ↔ `HU-076`, `HU-079` | `HU-041` | 5 | 6 |
| `HU-049`, `HU-050` ↔ `HU-085`, `HU-090` | `HU-049`/`HU-050` | **7** | **6** ← invertido |
| `HU-051`, `HU-052` ↔ `HU-082`, `HU-087` | `HU-051`/`HU-052` | **7** | **6** ← invertido |
| `HU-053` ↔ `HU-084` | `HU-053` | **7** | **6** ← invertido |
| `HU-024` ↔ `HU-098` | `HU-024` | 5 | 3 |
| `HU-025` ↔ `HU-109` | `HU-025` | 5 | 3 |
| `HU-034` ↔ `HU-082` | `HU-034` | 5 | 6 |
| `HU-032` ↔ `HU-081` | `HU-032` | 5 | 6 |

En los tres marcados, **el equipo construye primero la historia subordinada y un sprint después la que manda**.

Agravante: la delimitación no coincide con el contenido. El `README` asigna a `HU-051`/`HU-052` *«la captura sin red»* y a `HU-082` *«la comprobación y la unicidad»* — pero `HU-082` se titula literalmente **«Registrar un abastecimiento de combustible en carretera, sin conectividad»**. Lo mismo con `HU-085`, *«Registrar el paso por caseta y marcar la discrepancia»*, frente a `HU-049`, *«Registrar el paso por caseta de peaje contra la tarifa esperada»*.

**El caso concreto:** en el Sprint 6 alguien implementa el registro de un abastecimiento en carretera sin red siguiendo `HU-082`. En el Sprint 7 aparece `HU-051`, que según el `README` es la que manda sobre ese acto, con su propio catálogo de fuente declarada. O se reescribe, o quedan dos caminos de captura para el mismo hecho — y en un módulo donde la unicidad del comprobante (`RN-84`) es un control, dos caminos de captura es un agujero.

**Corregir:** la delimitación se aplica sobre los artefactos, no solo se declara en una tabla; y cada par se agrupa en un solo sprint.

## `HB34-11` — El backlog quedó en 110 historias y nunca incorporó las 15 de M-17

| | |
|---|---|
| **Artefactos** | `backlog.md`, `README.md` de historias |

El backlog abre con *«**110 historias**, `HU-001` a `HU-110`»* y en el Sprint 8 declara: *«**M-17 está sin cubrir en el backlog actual.** […] no se escribieron historias porque los casos de uso no lo desarrollaron. Hay que corregirlo antes de cerrar el Sprint 0.»*

Las historias se escribieron —`HU-111` a `HU-125`, 15 historias de M-17, incluidas las de datos personales, hábeas data, registro de consultas y depuración— **y el backlog no se actualizó**. Quedan sin sprint, sin prioridad relativa y sin dependencias declaradas.

Consecuencia concreta: `HU-114` cierra el manifiesto **dentro del despacho** y `HU-115` levanta el acta de entrega de personas **en cada destino**. Despacho y ejecución están en los Sprints 5 y 7. Si M-17 se construye en el Sprint 8, el despacho del Sprint 5 se escribe sin el punto de extensión del manifiesto y hay que abrirlo después. Y `HU-120` —consultar la lista de abordo sin conectividad— depende del paquete de misión que el cliente de campo arma en el Sprint 5.

Además, tres conteos del backlog y del `README` están desactualizados:

| Afirmación | Real |
|---|---|
| «110 historias» | **125** |
| «41 marcadas `Refinada`, 69 `Borrador`» | **46 refinadas, 79 borradores** |
| `README`: «Cuatro lo dicen en el propio campo de estado» | **14** lo dicen |

## `HB34-12` — El bloqueo por póliza vencida es global en `HU-101` y por régimen de tenencia en `HU-023`

| | |
|---|---|
| **Artefactos** | `HU-023`, `HU-101` |
| **Autoridad aplicable** | `RN-16` |

`RN-16` es explícita: *«**Excepción admitida: granularidad por régimen de tenencia.** `bloqueo_por_poliza_vencida` admite valor distinto según el régimen […] propiedad, comodato, alquiler […] el contrato de alquiler normalmente **obliga** a mantener la póliza vigente.»*

- `HU-023` lo modela así: *«el parámetro "bloqueo por póliza vencida" desactivado para el régimen "propio"»*, *«activado para el régimen "alquilado"»*, con dos escenarios que producen resultados opuestos para el mismo vencimiento.
- `HU-101` lo modela como interruptor único: *«los parámetros `bloqueo_por_poliza_vencida` y `bloqueo_por_revision_vencida` en "apagado"»*, sin dimensión de régimen.

**El caso concreto:** vehículo `INS-P-030`, régimen **alquilado**, póliza vencida el 15/08/2026, institución con el parámetro global en *apagado*.

- Implementando `HU-023` → **se rechaza** la asignación: *«La póliza del vehículo alquilado INS-P-030 venció el 15/08/2026 y el bloqueo está activado para el régimen alquilado.»*
- Implementando `HU-101` → **se permite** el despacho con advertencia: *«El seguro no es obligatorio por ley vigente; el bloqueo es configurable y está apagado.»*

`HU-023` es la correcta. `HU-101`, que es la historia dueña del ciclo de vida del dato documental, es la que define el modelo del parámetro — y lo define incompleto.

Segundo punto: `RN-16` exige, con el parámetro apagado, que el sistema *«advierte, **exige acuse del despachador** y registra quién continuó»*. `HU-023` registra *«advertencia superada por el Jefe de Transporte»*; **`HU-101` solo advierte**, sin acuse ni registro de quién continuó. Con la implementación de `HU-101`, el reporte de exposición que pide `RN-16` n.º 4 no tiene de dónde salir.

---

# Medios

## `HB34-13` — Seis historias de M-17 omiten la sección de casos especiales

`HU-118`, `HU-119`, `HU-121`, `HU-122`, `HU-123` y `HU-124` **no tienen sección «Casos especiales que la afectan»**. En su lugar traen «Requisitos no funcionales relacionados», que no está en la plantilla.

El DoR exige: *«Se identificaron los casos especiales `CE-xx` que la afectan, **o se dejó constancia explícita de que no hay ninguno**»*. Las otras seis historias sin `CE` aplicable —`HU-003`, `HU-010`, `HU-012` a `HU-015`— sí dejan la constancia, con la fórmula *«Ninguno de los 28 `CE-xx` toca este flujo. Constancia dejada»*. Estas seis no dejan nada: la sección simplemente no existe.

Es la firma del cuarto analista: es el mismo lote que **sí** cita `RNF-xx` sistemáticamente, cosa que casi nadie más hace (ver `HB34-14`). Ninguna de las dos convenciones es mala; el problema es que sean dos.

`HU-124` es el caso que más incomoda: depura datos personales en su plazo sin romper la cadena de auditoría, y no declara relación con `CE-27` (cierre de ejercicio con hallazgo abierto) ni con `CE-28` (hallazgo posterior sobre misión cerrada). Depurar el manifiesto de una misión sobre la que después se abre un hallazgo es exactamente el cruce que hay que haber pensado.

## `HB34-14` — Nueve `RNF-xx` no los cita ninguna historia, y los `RNF` solo se citan en un lote

Citas a `RNF-xx` desde las 125 historias:

| Sin ninguna cita | Con cita |
|---|---|
| `RNF-01`, `RNF-05`, `RNF-09`, `RNF-10`, `RNF-15`, `RNF-16`, `RNF-19`, `RNF-20`, `RNF-21` | los 12 restantes |

De las 32 citas totales a `RNF-xx`, **21 vienen de `HU-111`–`HU-125`**. Fuera de ese lote los `RNF` prácticamente no se referencian.

Matiz honesto, porque no todo lo no citado está descubierto:

- **`RNF-05` temporalidad** está bien cubierto **funcionalmente**, aunque nadie lo cite: `RN-39` a `RN-42` están citadas por 20, 16, 15 y 5 historias; `HU-005` prueba *«Se usa la tarifa vigente a la fecha prevista de paso, no a la de captura»*; `HU-064` separa fecha del hecho de fecha de captura y prohíbe deducir; `HU-070` hace el asiento de diferencia; `HU-125` reproduce la evaluación con la matriz vigente a la fecha del hecho. **Falta la prueba central del propio `RNF-05`** —cargar dos vigencias, registrar en agosto un hecho de junio y verificar que el impreso muestra la tarifa de junio con su identificador de versión— pero la capacidad está.
- **`RNF-21` folios** está cubierto por `HU-029`, `HU-031`, `HU-036`, `HU-017`, `HU-080`. Falta el *reporte de control de folios por delegación* que el `RNF` exige en línea.
- **`RNF-15` y `RNF-19` no están cubiertos por nada** — ver `HB34-05`.
- `RNF-01`, `RNF-09`, `RNF-10`, `RNF-16`, `RNF-20` son de infraestructura y operación; que no tengan historia es defendible, pero entonces hay que decir **quién** los verifica y **cuándo**, porque hoy no lo dice nadie.

## `HB34-15` — `RN-67` se implementa en tres momentos distintos sin delimitación, y con veredicto DoR opuesto

`HU-002` bloquea el **envío** de la solicitud (M-06, Sprint 4), `HU-022` bloquea la **asignación** (M-07, Sprint 5) y `HU-125` bloquea la **programación** evaluando tramo por tramo (M-17/M-07, sin sprint). No están en la tabla de delimitación.

El comportamiento coincide en lo esencial —ambas tratan la ausencia de entrada en la matriz como bloqueo, no como permiso—, así que no es contradicción. Lo que sí es un problema:

1. `HU-002` ya ofrece *«declarar la configuración por tramo»*, que es el objeto de `RN-68` y de `HU-125`. La frontera entre las dos no está escrita en ninguna de las dos.
2. **`HU-002` está `Refinada`** con `Antecedentes` que exigen *«una matriz de compatibilidad objeto × objeto vigente al 2026-03-14»*, mientras **`HU-125` está `Borrador — la matriz objeto × objeto no está poblada y el insumo #39 sigue abierto»**. Es el mismo insumo faltante produciendo dos veredictos opuestos, igual que en `HB34-06`.

Y nadie es dueño de poblar la matriz: es catálogo de M-02, que no tiene historias (`HB34-05`).

## `HB34-16` — `HU-044` y `HU-080` resuelven el mismo bloqueo en dos sprints con dos mensajes

Ambas cubren `CE-20` y llegan a la misma conclusión correcta —con consumo no se anula, se liquida— con mensajes distintos:

- `HU-044` (Sprint 5): *«Hubo consumo de 1,200.00 lempiras contra la asignación AC-2026-0233. La misión no se anula: debe liquidarse aunque su kilometraje sea cero.»*
- `HU-080` (Sprint 6): *«La misión OM-2026-0512 registra consumo de L 1,040.00 el 23/09/2026. No se anula: se liquida, aunque su kilometraje sea cero.»*

No hay contradicción de comportamiento; hay duplicación de implementación y dos textos para el mismo bloqueo. El par no está en la tabla de delimitación. `HU-080` aporta lo que `HU-044` no tiene —el acta de devolución folio por folio, el no reciclado del folio anulado, el plazo de devolución— así que la delimitación natural es: `HU-044` manda en la reversión de la misión, `HU-080` en el ciclo del instrumento. Escríbanla.

## `HB34-17` — 65 de las 79 historias en borrador no declaran por qué lo están

El `README` afirma: *«Las que están en borrador **no lo están por descuido**: cada una declara qué insumo o decisión le falta.»*

Conteo real sobre el campo **Estado**:

| | |
|---|---|
| Borradores con la razón escrita en el campo Estado | **14** |
| Borradores que dicen únicamente «Borrador» | **65** |

Las 65 tienen notas `[C]` en el pie —entre 1 y 5 cada una—, pero eso no es lo mismo que declarar qué la bloquea: en la mayoría los `[C]` son parámetros que el propio DoR admite como no bloqueantes. La afirmación del `README` no se sostiene, y es la que sustenta la sesión de refinamiento que el backlog propone.

## `HB34-18` — Las 125 historias tienen «Sprint: sin asignar»

El campo **Sprint** de la plantilla está en *sin asignar* en **las 125**, mientras el backlog asigna sprints por rango. Hay una sola fuente y ya diverge de sí misma: `HU-111`–`HU-125` no están en ningún sprint del backlog, y el resto no lo dice en su ficha.

Efecto práctico sobre el DoR: el punto *«Las historias de las que depende están terminadas o programadas antes en el mismo sprint»* **no se puede verificar desde la historia**. Hay que ir al backlog, cruzar rangos y confiar en que está al día — y `HB34-11` muestra que no lo está.

---

# Bajos

## `HB34-19` — Rechazos sin el mensaje especificado

El DoR exige *«Los mensajes que ve el usuario están especificados, no dejados a criterio de implementación»*, y la plantilla Gherkin lo repite: *«El mensaje de error es parte del criterio.»* Sobre 919 escenarios, dos rechazos duros lo incumplen:

- `HU-104`, *Se rechaza el retiro con misiones abiertas*: *«Entonces el sistema rechaza la acción / Y lista la misión no terminal que lo impide»*. No hay texto. Es un bloqueo que el Encargado de Bienes tiene que resolver con una gestión administrativa: necesita saber cuál misión y en qué estado.
- `HU-043`, *Se rechaza el recurso entrante ya reservado en la franja*: *«Y muestra el conflicto con su titular: la misión "OM-2026-0460", la Unidad de Bienes y su franja»*. Es una descripción de contenido, no el mensaje.

## `HB34-20` — Tres historias sin ningún camino de rechazo

`HU-005`, `HU-054` y `HU-066` no tienen un solo escenario de rechazo. Las tres son informativas o de comportamiento en segundo plano, y en los tres casos es defendible — pero el DoR lo pide sin excepción y `HU-005` está marcada **Refinada**. O se añade el rechazo (`HU-005`: qué pasa si no hay ninguna tarifa cargada para ningún punto de la ruta), o el DoR admite la excepción por escrito. Hoy dice que no la admite.

## `HB34-21` — `HU-045` se contradice consigo misma en los antecedentes

`Antecedentes`: *«un motorista de relevo declarado "Elder Zavala" con licencia […] **vigente hasta el "2027-11-30"**»*. Último escenario: *«Dado que la licencia de "Elder Zavala" **vence el "2026-09-16"**»*. El `Dado` local pisa el antecedente sin decirlo, en el escenario que decide un bloqueo duro. Se corrige junto con `HB34-01`.

---

# Definition of Ready — cuántos de los 79 borradores pasarían

Se aplicó la lista escrita, punto por punto, a los 79. Resultado:

**Ninguna de las 79 pasa hoy**, y no por su contenido: por dos puntos del DoR que fallan de forma transversal.

| Punto del DoR | Por qué falla y a cuántas alcanza |
|---|---|
| *«Las historias de las que depende están terminadas o programadas antes en el mismo sprint»* | Falla estructuralmente por `HB34-04`, `HB34-08`, `HB34-09` y `HB34-10`. Alcanza a **las 125**, refinadas incluidas |
| *«Si genera o modifica un documento oficial impreso, el formato está diseñado»* | Insumo **#2** abierto. Alcanza al menos a `HU-017`, `HU-020`, `HU-029`, `HU-031`–`HU-034`, `HU-036`, `HU-040`, `HU-056`, `HU-081`, `HU-099`, `HU-114`, `HU-115` |

Descontados esos dos —que se resuelven arreglando el backlog y consiguiendo el insumo #2, no historia por historia—, **la estimación del backlog es correcta: la gran mayoría pasa sin tocarse.** El conteo:

| | |
|---|---|
| Borradores totales | **79** |
| Legítimamente no refinables — el `[C]` **es** la lógica | **18** |
| No refinables por hallazgo de esta revisión | **6** |
| Pasarían aplicando el criterio escrito | **≈ 55** |

## Las 18 que legítimamente no pasan

| Historia | Por qué el `[C]` es la lógica |
|---|---|
| `HU-008` | Qué puesto convalida una emergencia y en qué plazo — insumo #32. Sin eso no hay flujo |
| `HU-012` | Sin el esquema de niveles de ARGOS no hay umbral que evaluar — insumo #16 |
| `HU-019` **y `HU-035`** | Si no hay punto de verificación público, el QR no apunta a nada. **`HU-035` debe bajar a borrador**: declara el mismo `[C]` y está Refinada (`HB34-06`) |
| `HU-037` | Requiere la decisión de producto de `HB3-14` |
| `HU-071` – `HU-075` | La periodicidad del fondo decide si el objeto es de período o de misión. Es estructural — `PROP-01` / insumo #7 |
| `HU-111`, `HU-113` | El catálogo mínimo de datos de una persona externa **es** el modelo. No es un parámetro |
| `HU-114` | Depende del insumo #40, rutas de lista abierta |
| `HU-115` | El régimen de custodia de personas externas depende del insumo #39 |
| `HU-121`, `HU-122`, `HU-123` | El actor —Oficial de Información Pública— **no está catalogado**. El DoR exige actor del glosario. Falla el primer bloque de la lista, no un `[C]` de detalle |
| `HU-125` | La matriz objeto × objeto no está poblada — insumo #39 |

## Las 6 que dejan de ser refinables por esta revisión

| Historia | Hallazgo |
|---|---|
| `HU-045`, `HU-061` | `HB34-01` — duplicadas y contradictorias sobre un bloqueo duro |
| `HU-004`, `HU-009` | `HB34-03` — bloquean donde `RN-50` prohíbe bloquear. **Ambas estaban Refinadas** |
| `HU-041` | `HB34-04` — su precondición completa está un sprint después. **Estaba Refinada** |
| `HU-088` | `HB34-07` — la clasificación central no es determinable |

## Historias marcadas `Refinada` que no lo están

`HU-004`, `HU-009`, `HU-035`, `HU-041`, `HU-045`. Las cinco tienen defecto sustantivo, no formal.

Y una observación sobre `HU-002` y `HU-096`: el backlog usa `HU-096` como ejemplo de borrador injustificado, y tiene razón —el formato del correlativo es el ejemplo válido de la plantilla—. Pero `HU-002` está Refinada dependiendo de una matriz que `HU-125` declara no poblada. **La sesión de refinamiento de una hora que propone el backlog va a producir un verde falso si se corre antes de arreglar el backlog y de zanjar `HB34-01`, `HB34-03` y `HB34-06`.**

---

# Qué se revisó y qué no

**Revisado a fondo:**
- Las 125 historias: metadatos, trazabilidad `RN`/`CE`/`NRM`, estructura de secciones, 919 escenarios Gherkin (`Cuando` por escenario, camino de rechazo, mensaje especificado, números cableados).
- Cruce completo de las 97 `RN` y los 28 `CE` contra las citas de las historias.
- Cruce de los 21 `RNF` contra las citas y contra la cobertura funcional real de `RNF-03`, `RNF-04`, `RNF-05`, `RNF-07`, `RNF-15`, `RNF-17`, `RNF-19`, `RNF-21`.
- Lectura completa de `RN-01`, `RN-10`, `RN-16`, `RN-50`, `RN-55` para dirimir las contradicciones reportadas.
- `backlog.md`: dependencias declaradas, orden de sprints, conteos.
- Las seis áreas de rigor máximo, una por una.
- Búsqueda de fugas de alcance contra `DP-001`.

**No revisado, o revisado por encima:**
- Los 18 casos de uso **en sí mismos**: se leyó `H-B3-001` para no repetir y se usaron los `CU` como referencia de trazabilidad, pero no se hizo pasada adversarial nueva sobre ellos. Los 19 hallazgos previos siguen abiertos y ocho tocan la máquina de estados.
- El contenido interno de las 97 reglas más allá de las cinco leídas completas. **No verifiqué que cada historia diga exactamente lo que su regla dice**: verifiqué que la regla exista y, donde la historia hacía una afirmación fuerte, la contrasté. Un muestreo dirigido, no censo.
- Los `RNF` de infraestructura (`RNF-01`, `02`, `09`, `10`, `13`, `20`) contra su viabilidad técnica: el stack está diferido al Sprint 2 y no había contra qué contrastarlos.
- Los artefactos del Bloque 4 —modelo de datos, navegación, 126 pantallas—: fuera del encargo.
- El nivel de verificación normativa `[V]`/`[P]`/`[C]`/`[I]` de cada historia contra su ficha `NRM-xx`. Los muestreos que hice (`HU-011`, `HU-017`, `HU-019`, `HU-035`, `HU-088`) no mostraron escalada, pero **no es un censo** y `HN1-03` demostró que la escalada silenciosa es el modo de falla de este proyecto. Merece una pasada de `normativa-honduras`.

# Lo que más me preocupa, en una frase

Que **cinco historias marcadas `Refinada` contradicen la regla que dicen implementar en las dos áreas de mayor consecuencia legal** —la habilitación del conductor (`HB34-01`) y el bloqueo por segregación al asignar (`HB34-02`)— y que en las dos el resultado sea el mismo: un bloqueo duro convertido en hallazgo posterior, que es precisamente la diferencia entre un control y un registro de que el control no existió.

# ¿Está el Bloque 3 listo para que empiece el código?

**No, y no está lejos.**

La materia prima es buena: 919 escenarios con un solo `Cuando`, mensajes de bloqueo redactados en el lenguaje del negocio, trazabilidad `RN`/`CE` completa sin un solo enlace roto, cero fugas de alcance y cero números normativos cableados. Eso no es lo normal y no hay que subestimarlo.

Lo que impide empezar es de otro orden:

1. **El Sprint 3 no puede arrancar.** No hay historias de M-01 ni de M-02, y todo lo demás las presupone en sus antecedentes (`HB34-05`).
2. **El backlog no es un plan ejecutable.** Está en 110 de 125, con cuatro inversiones de dependencia confirmadas (`HB34-04`, `HB34-08`, `HB34-09`, `HB34-10`).
3. **Tres contradicciones de comportamiento tienen que zanjarse antes de escribir la primera prueba**, porque cada una tiene dos respuestas correctas según qué historia se lea: `HB34-01`, `HB34-03` y `HB34-06`.
4. **`HB34-02` es un hueco, no una contradicción**, y es el más barato de cerrar: dos escenarios en `HU-025` y `HU-043`.

Ninguno de los cinco críticos exige rehacer nada. Cuatro se resuelven editando historias y reordenando el backlog; el quinto —M-01 y M-02— es escribir un lote que faltaba. Con eso hecho, la sesión de refinamiento del PO tiene sentido y **≈ 55 historias entran sin tocarse**.

Recomendación de secuencia, por la lección del Bloque 1 —una sola pasada, no parches sucesivos: primero las decisiones del PO (`HB34-01`, `HB34-03`, `HB34-06`), después el lote M-01/M-02, después una única reescritura del backlog, y solo entonces el refinamiento.
