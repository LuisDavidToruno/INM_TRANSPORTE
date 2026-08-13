# RN-58 — El régimen de uso es atributo del vehículo, con acto que lo confiere, beneficiario y vigencia acotada

| Campo | Valor |
|---|---|
| **Módulos** | M-03, M-07, M-04, M-14 |
| **Origen** | Caso especial [CE-19](../../02-requisitos/casos-especiales/CE-19-vehiculo-asignado-a-funcionario-frente-al-pool.md) · Norma [NRM-02](../normativa/NRM-02-bienes-del-estado.md) |
| **Verificación** | `[P]` el deber de custodia y control de los bienes del Estado — [NRM-02](../normativa/NRM-02-bienes-del-estado.md). `[I]` el modelado del régimen de uso: implicación de requerimiento del equipo |
| **Tipo** | Bloqueo duro + derivación |
| **Configurable** | Sí — catálogo `regimen_de_uso` y parámetro `vigencia_maxima_asignacion_permanente` |

## Enunciado

Todo vehículo de la flota **debe** tener un **régimen de uso** vigente, tomado de un catálogo configurable cuyos valores iniciales son:

| Régimen | Significado |
|---|---|
| `POOL` | Asignable a cualquier Orden de Misión de su ámbito |
| `ASIGNADO_A_FUNCIONARIO` | Afectado al uso de un servidor determinado por acto formal |
| `AFECTO_A_OPERACION` | Afectado a una operación o programa con vigencia |
| `EN_TENENCIA_AJENA` | Prestado o cedido — ver [`RN-63`](RN-63-prestamo-de-vehiculo-como-expediente-del-bien.md) |

Todo régimen distinto de `POOL` **debe** constar con: **acto que lo confiere** (folio, autoridad emisora, documento adjunto), **fundamento**, **beneficiario nominado**, **vigencia con fecha de fin**, y declaración expresa de si autoriza **resguardo domiciliario** y de si autoriza que el **beneficiario conduzca**.

Un vehículo en régimen distinto de `POOL` **sale del conjunto asignable** con causa tipificada. Retirarlo del régimen para asignarlo a una misión es un **acto registrado de autoridad competente**, no una decisión de programación.

Vencida la vigencia sin renovación, el vehículo **vuelve automáticamente a `POOL`** y el hecho se reporta.

## Justificación

Ninguna de las 54 reglas originales distingue el vehículo de pool del vehículo asignado permanentemente a un funcionario. **Para el modelo actual, todos los vehículos son de pool.** Eso produce dos fallas simultáneas: la programación cree disponible un vehículo que en la práctica nunca lo está, y el vehículo asignado queda fuera de todo control porque no pasa por el circuito de solicitud, autorización y despacho.

El segundo efecto es el que produce hallazgos. [NRM-02](../normativa/NRM-02-bienes-del-estado.md) exige que todo bien del Estado tenga responsable identificable y uso justificado; un vehículo asignado sin acto que lo confiera, o con acto caducado, es exactamente la observación que el auditor levanta — y el listado que la evita es barato de producir **antes** de que lo pidan.

La vigencia acotada existe porque la asignación indefinida es la forma en que un bien público deja de comportarse como público sin que nadie tome nunca la decisión de que así sea.

## Condiciones de aplicación

Aplica a todo vehículo de la flota, incluidos los de tenencia ajena ([`RN-62`](RN-62-titulo-de-tenencia-con-vigencia-y-rubros.md)).

**No aplica** a la asignación **operativa** de un vehículo a una delegación o dependencia, que es alcance de datos y se rige por [`actores-y-roles.md`](../actores-y-roles.md) — autoridad en la materia. Régimen de uso y ámbito de asignación son dos cosas distintas y un vehículo tiene ambas.

## Comportamiento esperado

1. La ficha del vehículo muestra el régimen vigente, su acto, su beneficiario y los días que faltan para vencer. Sin régimen declarado, el vehículo se comporta como `POOL`, y esa omisión aparece en el listado de excepciones.
2. La programación **excluye** del conjunto asignable los vehículos en régimen distinto de `POOL`, indicando la causa: *"Vehículo \<correlativo\> asignado a \<beneficiario\> por acto \<folio\> hasta \<fecha\>"*. No los oculta: los muestra excluidos y con motivo.
3. Retirar temporal o definitivamente un vehículo de su régimen para asignarlo a una misión exige acto registrado de ACT-08 o de la autoridad que confirió el régimen, con motivo y vigencia del retiro.
4. Al vencer la vigencia sin renovación, el sistema cambia el régimen a `POOL`, deja asiento con la causa *vigencia caducada* y notifica a ACT-08 y al beneficiario.
5. El sistema produce el **listado institucional de vehículos por régimen**, con dos columnas de excepción: régimen sin acto que lo confiera, y régimen con vigencia caducada.
6. Ante un **operativo declarado** — la restricción de circulación de Semana Santa u otra franja inhábil ([`RN-23`](RN-23-permiso-de-circulacion-en-dia-inhabil.md)) — el reporte previo lista, por vehículo: permiso vigente, o **confirmación de resguardo con responsable, fecha, odómetro y ubicación fotografiada**, o la marca **no confirmado**. Sin evidencia, el vehículo figura como *no confirmado*, nunca como *resguardado*.

## Casos límite

- **Funcionario que cesa o se traslada** con el vehículo asignado. El régimen se cierra con **acta de cierre**: odómetro, estado, accesorios, documentos y faltantes. Toda diferencia abre expediente en M-12. El vehículo no vuelve a `POOL` sin esa acta.
- **Acto que confiere el régimen sin fecha de fin.** No se admite. Si el acto institucional no la trae, se registra con la vigencia máxima del parámetro `vigencia_maxima_asignacion_permanente` y el hecho se marca como *vigencia derivada*, no como vigencia del acto.
- **Vehículo asignado que se necesita para una emergencia.** Se retira del régimen por acto registrado, aunque sea posterior al hecho ([`RN-73`](RN-73-convalidacion-de-actos-sin-autorizacion-previa.md)). Lo que no se admite es que la programación lo tome como si fuera de pool.
- **Régimen conferido por autoridad que no es la máxima.** Se registra igual con su emisor; el sistema no valida competencias que no conoce. `[C]` insumo #28 — quién puede conferir régimen en esta institución.
- **Vehículo de pool que en la práctica usa siempre la misma persona.** No es un caso del sistema, es un hallazgo: el contraste entre régimen declarado `POOL` y conductor efectivo único aparece en el indicador de uso por conductor ([`RN-82`](RN-82-indicadores-de-calidad-de-la-programacion.md)).

## Trazabilidad

- Norma: [NRM-02](../normativa/NRM-02-bienes-del-estado.md) `[P]`
- Autoridad de alcance de datos: [actores-y-roles.md](../actores-y-roles.md)
- Reglas relacionadas: [RN-19](RN-19-vehiculo-no-operativo-no-se-asigna.md), [RN-22](RN-22-custodia-del-vehiculo.md), [RN-23](RN-23-permiso-de-circulacion-en-dia-inhabil.md), [RN-24](RN-24-vehiculo-de-servicio-exceptuado.md), [RN-57](RN-57-habilitacion-de-quien-efectivamente-conduce.md), [RN-59](RN-59-todo-uso-se-ampara-en-orden-de-mision.md), [RN-62](RN-62-titulo-de-tenencia-con-vigencia-y-rubros.md)
- Casos especiales: [CE-19](../../02-requisitos/casos-especiales/CE-19-vehiculo-asignado-a-funcionario-frente-al-pool.md) — candidatas `RN-C19a`, `RN-C19b`, `RN-C19e`
- Insumos pendientes: #28 quién autoriza la misión y el régimen de un funcionario de alto nivel
