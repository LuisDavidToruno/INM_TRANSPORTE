# RN-07 — La delegación de autorización tiene vigencia acotada, consta en el expediente y no puede romper la segregación de funciones

| Campo | Valor |
|---|---|
| **Módulos** | M-01, M-06, M-20 |
| **Origen** | Norma [NRM-08](../normativa/NRM-08-firma-electronica.md) y [NRM-01](../normativa/NRM-01-control-interno-tsc.md); realidad de rotación de personal en [NRM-09](../normativa/NRM-09-realidad-operativa.md) |
| **Verificación** | `[V]` la necesidad de delegación registrada — `[C]` el instrumento formal que usa la institución |
| **Tipo** | Bloqueo duro |
| **Configurable** | Sí — parámetro `vigencia_maxima_delegacion`, y catálogo de facultades delegables |

## Enunciado

Una facultad de autorización **puede** delegarse en otro servidor únicamente si la delegación:

1. Tiene **fecha de inicio y fecha de fin** explícitas, dentro del máximo configurado
2. Identifica al delegante, al delegado y **las facultades concretas** delegadas
3. Referencia el **acto administrativo** que la sustenta, con adjunto `[C]`
4. Fue registrada por el propio delegante o por quien lo suceda formalmente

El delegado **no debe** poder subdelegar, y **no debe** poder ejercer una facultad que, por [RN-01](RN-01-segregacion-de-funciones.md), le esté vedada sobre esa Orden de Misión concreta.

Toda actuación por delegación **debe** constar como tal en el expediente y en el documento impreso: *"por delegación de <delegante>, acto <referencia>"*.

## Justificación

[NRM-08](../normativa/NRM-08-firma-electronica.md) lo exige explícitamente: delegación con vigencia acotada, dejando constancia de que se actuó por delegación y del acto que la confiere — *"esencial dada la rotación de personal"*.

[NRM-09](../normativa/NRM-09-realidad-operativa.md) documenta que la rotación es alta, especialmente tras cambios de administración. Sin delegación formal, la operación se detiene cada vez que un jefe sale de vacaciones — y lo que ocurre en la práctica es que alguien usa la clave de otro. Esa es la falla de control que esta regla previene.

## Condiciones de aplicación

Aplica a facultades de autorización y aprobación. **No aplica** a funciones operativas (despacho, entrega de combustible, captura de bitácora), que se resuelven por reasignación de rol, no por delegación.

**No aplica** a la aprobación del permiso de circulación en día u hora inhábil salvo que la institución confirme que la máxima autoridad puede delegarla. `[C]` [NRM-02](../normativa/NRM-02-bienes-del-estado.md) exige *permiso firmado por la máxima autoridad*; si la delegación es admisible es cuestión abierta y no se asume.

## Comportamiento esperado

1. Fuera de la ventana de vigencia, la facultad delegada **no existe**: el sistema la trata como si nunca se hubiera otorgado, sin período de gracia.
2. Antes de aplicar la delegación, el sistema evalúa [RN-01](RN-01-segregacion-de-funciones.md) contra la identidad del **delegado**. Si el delegado es el solicitante, se bloquea y escala por [RN-02](RN-02-escalamiento-de-autorizacion.md).
3. Toda autorización por delegación genera el asiento de [RN-03](RN-03-registro-inmutable-de-autorizacion.md) incluyendo la referencia a la delegación.
4. El sistema alerta al delegante y a ACT-01 cuando una delegación está por vencer, y **no la renueva automáticamente**.
5. Existe reporte de **delegaciones vigentes y actuaciones ejercidas bajo cada una**, para ACT-12 Auditor Interno.

## Casos límite

- **Delegación vencida a mitad de un flujo de aprobación.** La autorización ya ejercida dentro de la vigencia **es válida y permanece**; los actos posteriores requieren facultad vigente. La vigencia se evalúa contra la fecha del acto, no contra la fecha de consulta.
- **El delegante cesa en el cargo.** La delegación **queda sin efecto de inmediato**, aunque su fecha de fin no haya llegado: nadie puede delegar una facultad que ya no tiene. El sistema lo detecta por el espejo de Talento Humano ([RN-48](RN-48-datos-espejo-de-solo-lectura.md)) y notifica a los delegados afectados.
- **Delegación registrada estando el delegante ausente** — vacaciones ya iniciadas, delegación no hecha a tiempo. No hay solución dentro de la regla: la registra el superior del delegante como **designación**, no como delegación, con su propio fundamento. `[C]` confirmar si la institución admite esa figura.
- **Delegación sin acto administrativo adjunto.** Se permite registrarla marcando el adjunto como pendiente, con plazo configurable; vencido el plazo, la delegación se suspende y se alerta. Bloquear de entrada haría que se opere fuera del sistema.
- **Delegación cruzada** — A delega en B y B delega en A las mismas facultades. Se bloquea la segunda delegación: produce un circuito en el que ninguna autorización tiene control efectivo.
- **Delegación que cubre a un delegado que también es custodio del vehículo asignado.** No hay conflicto por sí mismo: la custodia no es función de control de [RN-01](RN-01-segregacion-de-funciones.md). Pero si el custodio es además el solicitante, se bloquea por la vía ordinaria.

## Trazabilidad

- Normas: [NRM-08](../normativa/NRM-08-firma-electronica.md), [NRM-01](../normativa/NRM-01-control-interno-tsc.md), [NRM-09](../normativa/NRM-09-realidad-operativa.md)
- Decisión: [DP-001, D-04](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md)
- Reglas relacionadas: [RN-01](RN-01-segregacion-de-funciones.md), [RN-02](RN-02-escalamiento-de-autorizacion.md), [RN-03](RN-03-registro-inmutable-de-autorizacion.md), [RN-23](RN-23-permiso-de-circulacion-en-dia-inhabil.md)
- Actores: ACT-01, ACT-03, ACT-08, ACT-09, ACT-12
- Historias y casos especiales: pendientes — Bloque 2
