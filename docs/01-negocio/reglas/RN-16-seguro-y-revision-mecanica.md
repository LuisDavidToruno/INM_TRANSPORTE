# RN-16 — Póliza de seguro y revisión mecánica son rastreables y alertables, con bloqueo configurable apagado por defecto

| Campo | Valor |
|---|---|
| **Módulos** | M-04, M-03, M-07 |
| **Origen** | Norma [NRM-06](../normativa/NRM-06-transito-y-licencias.md); decisión [DP-001 D-13](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) |
| **Verificación** | `[V]` que ni el seguro ni la revisión mecánica son obligatorios por ley vigente en Honduras — `[C]` si las Disposiciones Generales del Presupuesto exigen asegurar la flota |
| **Tipo** | Advertencia por defecto; bloqueo duro si se activa el parámetro |
| **Configurable** | **Sí** — parámetros `bloqueo_por_poliza_vencida` y `bloqueo_por_revision_vencida`, **ambos apagados por defecto** |

## Enunciado

El sistema **debe** registrar y alertar la vigencia de la póliza de seguro y de la revisión mecánica de cada vehículo, pero **no debe** bloquear la asignación ni el despacho por su ausencia o vencimiento **mientras el parámetro correspondiente esté apagado**.

Cuando la institución **active** cualquiera de los dos parámetros, el sistema **debe** bloquear con la misma dureza que la matriz licencia ↔ vehículo, **a partir de la fecha de activación** y sin efecto retroactivo sobre misiones ya aprobadas.

## Justificación

[NRM-06](../normativa/NRM-06-transito-y-licencias.md) verificó que **el seguro de responsabilidad civil vehicular no es obligatorio en Honduras** — la iniciativa de la DNVT de marzo 2026 es un anteproyecto en fase técnica — y que **la revisión técnica obligatoria tampoco está vigente**.

[DP-001 D-13](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) decide gestionarlos igual, como parte del cuidado integral del vehículo, con el bloqueo *"listo para activarse si se aprueba alguna de las leyes en trámite, pero apagado por defecto"*.

Bloquear hoy por algo que la ley no exige detendría la operación de una institución cuyo trámite de renovación de póliza está en curso — un daño real por una obligación inexistente.

## Condiciones de aplicación

Aplica a todo vehículo de la flota, incluidos comodato y alquiler, donde la póliza puede estar a nombre de un tercero. En esos casos se registra el titular de la póliza.

El parámetro es **por institución**, no por vehículo ni por dependencia: una institución no puede exigir póliza a unos vehículos y no a otros sin dejar el criterio escrito. `[C]` confirmar si se requiere granularidad por tipo de vehículo.

**Excepción admitida: granularidad por régimen de tenencia.** `bloqueo_por_poliza_vencida` admite valor distinto según el régimen de [RN-62](RN-62-titulo-de-tenencia-con-vigencia-y-rubros.md) — propiedad, comodato, alquiler. No es una excepción por vehículo: es un criterio escrito y uniforme dentro de cada régimen, y responde a un hecho concreto — el contrato de alquiler normalmente **obliga** a mantener la póliza vigente, de modo que ahí el bloqueo no deriva de la ley sino del contrato.

## Comportamiento esperado

1. Con el parámetro apagado: al asignar o despachar un vehículo con póliza o revisión vencida o ausente, el sistema **advierte**, exige acuse del despachador y **registra** quién continuó pese a la advertencia.
2. Con el parámetro encendido: **bloquea**, con mensaje que identifica el documento vencido y su fecha.
3. La activación del parámetro es un acto registrado: quién lo activó, cuándo, y con qué fundamento — típicamente la publicación de una ley. Ese fundamento se guarda como texto y adjunto.
4. Las advertencias acumuladas por vehículo alimentan un **reporte de exposición**: vehículos sin cobertura vigente que estuvieron en misión, con kilómetros recorridos en ese estado. Es lo que Gerencia Administrativa necesita para decidir si asegura.
5. La vigencia de póliza y revisión alimenta las alertas anticipadas de [RN-17](RN-17-alertas-de-vencimiento-documental.md).

## Casos límite

- **Se aprueba la ley de seguro obligatorio a mitad de ejercicio.** El parámetro se activa con fecha de vigencia. Las misiones ya aprobadas no se invalidan retroactivamente; las nuevas se bloquean. Esa distinción es la razón de que el parámetro tenga fecha y no solo un interruptor. Ver [RN-39](RN-39-parametros-normativos-con-vigencia.md).
- **Póliza vigente pero con cobertura que excluye el uso previsto** — traslado de carga, circulación fuera del país. El sistema registra la cobertura como texto; no la interpreta. Produce advertencia informativa al despachar si hay observaciones registradas. `[C]` confirmar si vale la pena tipificar coberturas.
- **Vehículo asegurado bajo póliza colectiva institucional.** Se registra una póliza y se vincula a múltiples vehículos; la vigencia es de la póliza, no del vehículo. Al renovarse, se renueva para todos.
- **Revisión mecánica interna del taller de la institución**, no una revisión oficial. Es un dato distinto: se registra como *revisión interna* en M-11, y no satisface el parámetro de revisión oficial si algún día se activa.
- **Póliza vencida durante una misión en curso.** No se interrumpe la misión desde el escritorio. Se alerta al Jefe de Transporte y se registra la exposición en el expediente. Se cierra con observación.
- **Institución que activa el bloqueo sin tener las pólizas cargadas.** Paraliza la flota completa. El sistema debe exigir, antes de activar, un **reporte de impacto**: cuántos vehículos quedarían bloqueados hoy. Activar a ciegas es el modo más probable de que el parámetro se apague al día siguiente y nadie vuelva a confiar en él.

## Trazabilidad

- Norma: [NRM-06 — Tránsito, licencias, matrícula y siniestros](../normativa/NRM-06-transito-y-licencias.md)
- Decisión: [DP-001, D-13](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md)
- Reglas relacionadas: [RN-17](RN-17-alertas-de-vencimiento-documental.md), [RN-19](RN-19-vehiculo-no-operativo-no-se-asigna.md), [RN-39](RN-39-parametros-normativos-con-vigencia.md)
- Actores: ACT-01, ACT-04, ACT-08, ACT-11
- Historias y casos especiales: pendientes — Bloque 2
