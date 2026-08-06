# RN-20 — El tipo de vehículo asignado debe ser compatible con el objeto del traslado declarado

| Campo | Valor |
|---|---|
| **Módulos** | M-07, M-06, M-02, M-03 |
| **Origen** | Premisa rectora 2 de `CLAUDE.md`; norma [NRM-06](../normativa/NRM-06-transito-y-licencias.md) — registro del lado de la carga |
| **Verificación** | `[I]` regla de producto — `[V]` la exigencia de registrar tipo de carga, origen y destino |
| **Tipo** | Bloqueo duro |
| **Configurable** | Sí — matriz `compatibilidad_tipo_vehiculo_objeto_traslado`, catálogo abierto con vigencia |

## Enunciado

Toda solicitud de transporte **debe** declarar el **objeto del traslado**: personal de la institución, personas externas, carga, o una combinación, con su tipo según el catálogo.

El sistema **no debe** permitir asignar un vehículo cuyo tipo no esté declarado compatible con **todos** los objetos del traslado de la misión, según la matriz de compatibilidad vigente a la fecha de inicio.

Una combinación de personas y carga **debe** evaluarse contra ambas compatibilidades a la vez, no contra la predominante.

## Justificación

La premisa rectora 2 del proyecto lo establece: *"El tipo de vehículo es el eje de compatibilidad entre lo que se necesita mover y la flota disponible. Toda asignación se resuelve contra esa compatibilidad."*

El daño de no aplicarla no es teórico: trasladar personas en la paila de un pickup junto con combustible o herramienta es una práctica real que produce lesionados, y el expediente de la institución debe poder demostrar que el sistema no lo permitió — o registrar quién lo autorizó pese a todo.

[NRM-06](../normativa/NRM-06-transito-y-licencias.md) además exige registrar del lado de la carga tipo, peso, origen, destino, remitente y consignatario, *"por trazabilidad operativa"*.

## Condiciones de aplicación

Aplica a la programación y a toda sustitución de vehículo ([RN-14](RN-14-sustitucion-de-motorista.md)).

**No aplica** cuando el objeto del traslado es únicamente el propio vehículo — traslado a taller, entrega entre delegaciones.

## Comportamiento esperado

1. La matriz define, por par (tipo de vehículo × tipo de objeto de traslado), el resultado: compatible, compatible con condiciones, o incompatible. Las **condiciones** se muestran al despachar y se imprimen en la orden.
2. El bloqueo explica el par que falla: *"El tipo de vehículo <motocicleta> no es compatible con el objeto de traslado <carga: mobiliario>."*
3. El sistema propone los **tipos de vehículo compatibles** y los vehículos disponibles de esos tipos, en el mismo acto del bloqueo.
4. Un cambio del objeto del traslado después de aprobada la orden **reevalúa** la compatibilidad y puede invalidar la asignación, exigiendo reprogramación.
5. La combinación personas + carga se evalúa además contra [RN-21](RN-21-capacidad-de-pasajeros-y-carga.md), que gobierna los límites cuantitativos.

## Casos límite

- **Carga que aparece a última hora**, no declarada en la solicitud. Se registra como **novedad de despacho** y dispara la reevaluación. Si resulta incompatible, no sale: la orden se reformula. El caso más frecuente y el que más presión operativa genera.
- **Personas externas junto con personal de la institución.** Son dos objetos de traslado distintos con implicaciones distintas ([RN-51](RN-51-minimizacion-de-datos-de-personas-externas.md), M-17). La compatibilidad se evalúa para ambos, y el manifiesto los distingue.
- **Carga peligrosa o especializada** — combustible en bidones, cilindros, material químico. `[C]` la institución debe declarar si moviliza este tipo de carga y bajo qué régimen. **No se infiere ninguna regla de manejo de carga peligrosa**: mientras no se confirme, el catálogo la marca como *requiere autorización especial* y bloquea hasta que exista una autorización registrada.
- **Traslado de un detenido o persona bajo custodia.** Es objeto de traslado con requisitos propios de M-17 y compatibilidad restringida a vehículos habilitados. `[C]` confirmar si la institución realiza estos traslados y con qué vehículos.
- **Matriz sin la combinación evaluada.** Ausencia de entrada **no significa compatible**. El sistema bloquea e indica que falta la definición, igual que hace con la matriz de licencias ([RN-09](RN-09-matriz-licencia-vehiculo.md)). Interpretar el vacío como permiso es cómo se cuelan las asignaciones peligrosas.
- **Vehículo compatible pero con adecuación ausente** — pickup con carrocería sin barandas para carga suelta. Es *compatible con condiciones*; la condición se imprime y el despachador acusa haberla leído.

## Trazabilidad

- Norma: [NRM-06](../normativa/NRM-06-transito-y-licencias.md)
- Premisa rectora 2 de [CLAUDE.md](../../../CLAUDE.md)
- Reglas relacionadas: [RN-21](RN-21-capacidad-de-pasajeros-y-carga.md), [RN-14](RN-14-sustitucion-de-motorista.md), [RN-51](RN-51-minimizacion-de-datos-de-personas-externas.md), [RN-53](RN-53-cierre-del-manifiesto-al-despacho.md)
- Actores: ACT-02, ACT-04, ACT-05
- Historias y casos especiales: pendientes — Bloque 2
