# RN-25 — El salvoconducto y todo documento de control en carretera se emiten impresos, con folio único y QR verificable

| Campo | Valor |
|---|---|
| **Módulos** | M-15, M-04, M-07 |
| **Origen** | Norma [NRM-02](../normativa/NRM-02-bienes-del-estado.md) y [NRM-08](../normativa/NRM-08-firma-electronica.md); premisa rectora 4 de `CLAUDE.md` |
| **Verificación** | `[V]` la exigencia de **permiso portable y control físico en carretera** — [NRM-02](../normativa/NRM-02-bienes-del-estado.md). `[V]` que **no hay firma electrónica certificada** en el país y la autorización es interna — [NRM-08](../normativa/NRM-08-firma-electronica.md). `[I]` **el formato concreto** —folio, QR, hash, espacio de firma—: es diseño del equipo derivado de la premisa rectora 4 de [`CLAUDE.md`](../../../CLAUDE.md), que es premisa de proyecto y no norma. `[C]` **si la institución acepta exponer un punto de verificación público en internet**, siendo el despliegue on-premise — pendiente **G** |
| **Tipo** | Bloqueo duro |
| **Configurable** | No el documento ni sus seis elementos. **Sí el alcance del punto de verificación** — `alcance_verificacion` con valores *público en internet / interno de la institución* — y su valor lo decide el pendiente **G**, no esta regla |

## Enunciado

Todo documento destinado al control físico en carretera — salvoconducto de circulación en día u hora inhábil, orden de misión, constancia de asignación de combustible, manifiesto — **debe** tener versión imprimible que incluya:

1. **Folio único** dentro de la institución
2. **Código QR** que resuelva a un **punto de verificación**, cuyo alcance —público en internet o interno de la institución— es **configuración**, no parte del bloqueo. Ver la nota de `HN1-17`
3. Espacio para **firma y sello**
4. **Huella (hash)** del documento electrónico en el pie
5. Identificación del vehículo por correlativo institucional y placa si existe
6. Vigencia explícita: desde cuándo y hasta cuándo ampara

La página de verificación **debe** informar autenticidad y estado — vigente, anulado, vencido — **sin exponer datos personales**.

El despacho de una misión que requiere salvoconducto **debe** bloquearse si el salvoconducto no ha sido emitido.

## Nota de corrección — hallazgo `HN1-17`

> **Qué estaba mal.** Esta regla exigía, como **bloqueo duro sin configuración**, que el QR resolviera a *«una página pública de verificación»* — y que esa página exista. Pero [`actores-y-roles`](../actores-y-roles.md), en `ACT-15`, deja `[C]` la pregunta de fondo: *«si la institución acepta exponer un punto de verificación público en internet, siendo el despliegue on-premise»* — pendiente **G**.
>
> **Una regla no configurable que depende de una decisión institucional no tomada es una regla que se va a incumplir o a desactivar.** Y el supuesto de fondo —que hay internet publicable desde el servidor on-premise de la institución— es de los que este proyecto advierte que no deben darse por seguros.
>
> **Qué se corrigió, y qué no.** Se separó lo que está decidido de lo que no:
>
> | | Estado |
> |---|---|
> | Que el documento lleve **folio, QR, hash, firma, sello, vigencia e identificación** | **Bloqueo duro, sin configuración.** No cambia |
> | Que exista un **punto de verificación** al que el QR resuelva | **Bloqueo duro.** Tampoco cambia |
> | Que ese punto sea **público en internet** o **interno de la institución** | **Configuración** — `alcance_verificacion`, pendiente **G** |
>
> Lo que se retira no es el control: es la **suposición** de que el control solo puede ejercerse por una vía. Un QR que resuelve contra un punto interno sigue verificando el documento — lo que cambia es quién puede consultarlo, y eso es exactamente lo que la institución tiene que decidir.
>
> **El realismo que ya tenía esta regla, aplicado al otro lado.** Sus casos límite resuelven bien al verificador **sin señal en carretera** —código corto legible más contraste visual del hash—, que es el escenario real hondureño. Lo que faltaba era el mismo realismo del lado del servidor: no dar por hecho que hay un servidor publicable.
>
> **Corregido de paso el nivel de verificación.** Decía `[V]` a secas sobre un enunciado cuyo origen incluye *«la premisa rectora 4 de `CLAUDE.md`»*, que es premisa de proyecto y no norma. Lo `[V]` es la exigencia de permiso portable y la ausencia de firma electrónica certificada; **el formato concreto es `[I]`**.

## Justificación

Premisa rectora 4 del proyecto: híbrido digital-papel **por diseño, no por parche**. [NRM-02](../normativa/NRM-02-bienes-del-estado.md) es explícita: debe imprimirse un salvoconducto con folio verificable por QR que el motorista porte, *"el control en carretera es físico"*.

El agente del TSC o de la DNVT en un operativo de Semana Santa no va a consultar un sistema: va a pedir un papel. Y si ese papel no se puede verificar, tiene el mismo valor que uno falsificado. El QR es lo que convierte una impresión en un documento comprobable.

[NRM-08](../normativa/NRM-08-firma-electronica.md) confirma la lista de documentos que seguirán requiriendo papel `[I]` y exige la página pública de verificación.

## Condiciones de aplicación

Aplica a los documentos del catálogo `documento_imprimible_control`, que la institución puede ampliar.

**No aplica** a reportes internos y tableros, que no circulan como documento de control.

## Comportamiento esperado

1. El folio se asigna de un **rango por delegación**, lo que permite emisión anticipada sin conectividad ([RN-44](RN-44-identificadores-y-folios-en-el-cliente.md)).
2. La anulación del documento origen ([RN-04](RN-04-anulacion-como-asiento-reverso.md)) cambia el estado en la página de verificación **de inmediato**, para que un papel anulado no pase un control.
3. Toda reimpresión queda registrada: quién, cuándo y por qué. El folio no cambia; el conteo de impresiones sí, y es dato de auditoría.
4. La página pública de verificación registra cada consulta — fecha, hora y origen — sin exigir autenticación y sin revelar nombres de personas trasladadas ([RN-51](RN-51-minimizacion-de-datos-de-personas-externas.md)).
5. El formato impreso mantiene **paridad de campos, nombres y orden** con el formulario en pantalla y con el formato en papel vigente de la institución ([NRM-09](../normativa/NRM-09-realidad-operativa.md)). `[C]` insumo #2 — formatos en papel vigentes.

## Casos límite

- **Emisión anticipada para zona sin cobertura.** El documento se imprime antes de salir con folio pre-asignado. Si después la misión se modifica, el papel que el motorista lleva ya no corresponde: la página de verificación debe reflejar **desactualizado**, no solo vigente/anulado. Es un estado necesario que no aparece en la lista mínima de [NRM-08](../normativa/NRM-08-firma-electronica.md) y que la realidad obliga a agregar.
- **Control en carretera en zona sin señal.** El agente no puede escanear el QR. Mitigación: el documento incluye datos legibles suficientes para el control visual y un **código de verificación corto** consultable después. La verificación en línea no puede ser el único mecanismo en un país con la conectividad que documenta [NRM-09](../normativa/NRM-09-realidad-operativa.md).
- **Documento perdido en ruta.** Se reimprime con el mismo folio, registrando el motivo. No se emite un folio nuevo: dos folios para un mismo permiso rompen la conciliación.
- **Salvoconducto que ampara una ventana y la misión se retrasa.** El salvoconducto vence según su propia vigencia, no según el estado de la misión. Circular fuera de la ventana amparada es circular sin permiso, aunque el papel exista.
- **Impresión no disponible en la delegación.** No hay excepción: sin salvoconducto impreso no se despacha en día inhábil. `[C]` verificar que todas las delegaciones tengan capacidad de impresión; si alguna no la tiene, es un requisito de despliegue, no una excepción a la regla.
- **Falsificación del documento.** El QR y la huella la detectan al verificar. Lo que el sistema debe además registrar es **cada verificación fallida**, porque un patrón de folios inexistentes consultados es información valiosa.

## Trazabilidad

- Normas: [NRM-02](../normativa/NRM-02-bienes-del-estado.md), [NRM-08](../normativa/NRM-08-firma-electronica.md), [NRM-09](../normativa/NRM-09-realidad-operativa.md)
- Decisión: [DP-001, D-04](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md)
- Reglas relacionadas: [RN-23](RN-23-permiso-de-circulacion-en-dia-inhabil.md), [RN-24](RN-24-vehiculo-de-servicio-exceptuado.md), [RN-27](RN-27-asignacion-de-combustible-con-folio.md), [RN-44](RN-44-identificadores-y-folios-en-el-cliente.md)
- Actores: ACT-05, ACT-06, ACT-09, ACT-10
- Historias y casos especiales: pendientes — Bloque 2
