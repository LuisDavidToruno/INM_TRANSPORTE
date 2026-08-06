# RN-49 — El espejo se reconcilia periódicamente contra el origen y cada entidad muestra su última sincronización

| Campo | Valor |
|---|---|
| **Módulos** | M-20, M-14 |
| **Origen** | [ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md) — mitigaciones obligatorias; norma [NRM-01](../normativa/NRM-01-control-interno-tsc.md) — TSC-NOGECI V-14 |
| **Verificación** | `[V]` la decisión de arquitectura y la exigencia de conciliación periódica |
| **Tipo** | Bloqueo duro (proceso obligatorio) + advertencia |
| **Configurable** | Sí — `frecuencia_reconciliacion` por sistema origen |

## Enunciado

SIGTI **debe** ejecutar, con la periodicidad configurada, una **reconciliación completa** de cada conjunto de datos espejeados contra su sistema origen, que detecte y corrija divergencias sin depender de los webhooks.

Cada entidad espejeada **debe** exhibir su **marca de última sincronización confirmada**, visible en las pantallas donde se usa.

Toda divergencia detectada **debe** registrarse en la bitácora de sincronización: qué entidad, qué campo, valor local, valor del origen, y qué se hizo.

Los eventos fallidos **deben** encolarse con reintento y quedar revisables por ACT-01 Administrador del Sistema.

## Justificación

[ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md) enumera estas mitigaciones y advierte: *"Estas no son opcionales; sin ellas la decisión es imprudente."* Y describe el riesgo con precisión: *"Los webhooks se pierden. Una caída de red, un reinicio, un despliegue del origen, y un evento no llega. Si el único mecanismo es el webhook, el espejo diverge en silencio, que es la peor forma de fallar."*

TSC-NOGECI V-14 exige **Conciliación Periódica de Registros** `[P]`. La reconciliación del espejo es exactamente eso aplicado a los datos de personal y autorización — los que deciden quién puede conducir y quién puede aprobar.

## Condiciones de aplicación

Aplica a todos los conjuntos espejeados de ARGOS y Talento Humano ([RN-48](RN-48-datos-espejo-de-solo-lectura.md)).

`[C]` La frecuencia depende del contrato de API de cada origen (insumos #16 y #17). [ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md) propone diaria de madrugada como referencia; el valor es parámetro, no constante.

## Comportamiento esperado

1. La reconciliación compara el conjunto completo, no solo lo cambiado, y produce un **resumen**: entidades comparadas, coincidentes, divergentes y corregidas.
2. Una divergencia se corrige **a favor del origen** — es el dueño del dato ([RN-48](RN-48-datos-espejo-de-solo-lectura.md)) — pero se **registra** el valor anterior, porque puede haber servido de base a decisiones ya tomadas.
3. Si una divergencia afecta un dato que respaldó una asignación vigente — licencia, disponibilidad, nivel de autorización — el sistema **alerta sobre las misiones afectadas**, no solo corrige el campo.
4. La marca de última sincronización se muestra junto al dato en las pantallas de asignación y autorización, no escondida en una pantalla de administración.
5. Los eventos fallidos se reintentan con espaciamiento creciente y, agotados los reintentos, quedan en cola revisable con su error.

## Casos límite

- **Origen que no emite webhooks.** [ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md) lo prevé: la reconciliación pasa a ser el mecanismo principal, con mayor frecuencia y con la **ventana de desactualización documentada y aceptada explícitamente por el PO**. No se finge que el espejo está al día.
- **Reconciliación que encuentra cientos de divergencias.** Es señal de que los webhooks no están funcionando, no un dato a corregir en silencio. Superado un umbral de divergencias, el sistema **alerta como incidente**, porque el problema es el canal, no los registros.
- **Divergencia sobre un motorista actualmente en ruta.** Se corrige el espejo y se registra el impacto; la misión en curso no se interrumpe desde el escritorio ([RN-12](RN-12-disponibilidad-del-motorista.md)).
- **Origen caído durante la ventana de reconciliación.** No se marca como reconciliado. La marca de última sincronización conserva la fecha real, que es lo que dispara [RN-50](RN-50-degradacion-por-sincronizacion-detenida.md).
- **Reconciliación parcial** que solo alcanzó a comparar la mitad del conjunto. Se registra como parcial, indicando el alcance. Una reconciliación parcial reportada como completa es peor que ninguna: da falsa confianza.
- **Entidad borrada en el origen.** No se borra en SIGTI: se marca como inactiva conservando el histórico, porque los expedientes que la referencian deben seguir siendo legibles ([RN-04](RN-04-anulacion-como-asiento-reverso.md)).
- **Reloj de los sistemas desalineado** al comparar fechas de modificación. La reconciliación debe basarse en comparación de contenido, no solo en marcas de tiempo del origen.

## Trazabilidad

- Decisión: [ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md); [DP-001, D-05](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md)
- Norma: [NRM-01](../normativa/NRM-01-control-interno-tsc.md) — TSC-NOGECI V-14
- Reglas relacionadas: [RN-48](RN-48-datos-espejo-de-solo-lectura.md), [RN-50](RN-50-degradacion-por-sincronizacion-detenida.md), [RN-12](RN-12-disponibilidad-del-motorista.md)
- Actores: ACT-01, ACT-04, ACT-12
- Historias y casos especiales: pendientes — Bloque 2
