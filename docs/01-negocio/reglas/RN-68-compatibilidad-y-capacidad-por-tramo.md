# RN-68 — La compatibilidad y la capacidad se evalúan por tramo, sobre la configuración real de cada tramo

| Campo | Valor |
|---|---|
| **Módulos** | M-06, M-07, M-08, M-17 |
| **Origen** | Caso especial [CE-18](../../02-requisitos/casos-especiales/CE-18-carga-y-pasajeros-en-la-misma-mision.md) · Norma [NRM-06](../normativa/NRM-06-transito-y-licencias.md) |
| **Verificación** | `[P]` los límites de peso y de plazas — [NRM-06](../normativa/NRM-06-transito-y-licencias.md). `[I]` la evaluación por tramo: implicación de requerimiento del equipo |
| **Tipo** | Bloqueo duro |
| **Configurable** | Sí — la tolerancia de [`RN-21`](RN-21-capacidad-de-pasajeros-y-carga.md), no la evaluación por tramo |

## Enunciado

En toda misión con más de un destino, la compatibilidad ([`RN-20`](RN-20-compatibilidad-vehiculo-objeto-del-traslado.md), [`RN-67`](RN-67-matriz-de-compatibilidad-objeto-objeto.md)) y la capacidad de pasajeros y de carga ([`RN-21`](RN-21-capacidad-de-pasajeros-y-carga.md)) **deben** evaluarse **tramo por tramo**, sobre la configuración efectiva de cada tramo, incluyendo:

- la carga y las personas que **se entregan o descienden** en un destino intermedio, que liberan capacidad
- la carga y las personas que **se recogen o abordan** en un destino intermedio, que la consumen

Ningún tramo **debe** exceder la capacidad ni contener un par incompatible, aunque el total de la misión y el tramo inicial sí cumplan.

## Justificación

[`RN-21`](RN-21-capacidad-de-pasajeros-y-carga.md) contempla la carga que **se entrega** y libera capacidad; no contempla la que **se recoge**. Es la mitad del problema, y la mitad que no cubre es la peligrosa: una misión que sale con capacidad holgada y va acumulando en cada parada llega al último tramo sobrecargada, y ninguna validación lo vio porque todas miraron la salida.

En la operación real de reparto y recolección institucional —entregar insumos en tres delegaciones y traer equipo dado de baja de vuelta— **la configuración del último tramo no se parece a la del primero**. Evaluar solo el despacho es evaluar el tramo que menos riesgo tiene.

## Condiciones de aplicación

Aplica a toda Orden de Misión con dos o más destinos declarados, y a toda misión de un solo destino con retorno cargado.

Aplica también cuando el **vehículo cambia** en medio de la misión ([`RN-61`](RN-61-sustitucion-de-vehiculo-recalcula-valores-congelados.md)): los tramos posteriores se evalúan contra la ficha técnica del vehículo sustituto, no del original.

**No aplica** a la misión de un solo destino sin recogida en ruta, donde la evaluación al despacho ya cubre toda la ejecución.

## Comportamiento esperado

1. La solicitud declara, por destino: qué se entrega, qué se recoge, cuántas personas descienden y cuántas abordan. Sin esos datos, el sistema evalúa el tramo con la configuración del anterior y **marca el tramo como no declarado**, que es dato de auditoría, no un supuesto silencioso.
2. El sistema construye la secuencia de tramos y evalúa cada uno. Al bloquear indica **el tramo concreto**: *"Tramo 3, Comayagua → Tegucigalpa: excede capacidad de carga en 180 kg tras recoger \<objeto\>"*.
3. El resultado por tramo se congela con la versión de la matriz y de la ficha técnica ([`RN-41`](RN-41-congelamiento-del-valor-al-autorizar.md)) y se imprime en el documento de despacho, para que el motorista sepa qué puede recoger y qué no.
4. Al despachar se capturan el **peso y la ocupación efectivos**, y su desviación contra lo declarado se acumula como indicador por dependencia solicitante ([`RN-82`](RN-82-indicadores-de-calidad-de-la-programacion.md)).
5. Toda incorporación no prevista en ruta se registra como novedad con hora, lugar, quién la ordenó y quién la aceptó, y **reevalúa los tramos restantes**.
6. Si la reevaluación en ruta resulta en exceso o incompatibilidad, el sistema no puede impedir el hecho físico: lo registra, alerta a ACT-04 en cuanto haya señal y produce hallazgo.

## Casos límite

- **Reordenamiento de destinos en ruta.** Cambia la secuencia de tramos y por tanto la evaluación. Se registra con motivo y se reevalúa; no constituye desviación de ruta si la secuencia sigue siendo geográfica y temporalmente coherente ([`RN-37`](RN-37-coherencia-de-la-secuencia-de-casetas.md)).
- **Carga que se recoge sin haberse declarado**, porque en el destino apareció. Es el caso frecuente. Se registra como novedad y se reevalúa; si el tramo restante ya no admite, la salida es dejar la carga con acta y programar su retiro, no seguir cargado.
- **Personas que abordan en ruta sin estar en el manifiesto.** El manifiesto está cerrado al despacho ([`RN-53`](RN-53-cierre-del-manifiesto-al-despacho.md)): se registra como novedad, nunca como edición, y consume plaza en la evaluación del tramo.
- **Tramo con capacidad excedida solo por el peso del combustible** en tanque lleno. La ficha técnica declara el peso en orden de marcha; el cálculo usa ese valor, no el peso en vacío.
- **Misión que se divide en hermanas** porque ningún ordenamiento de tramos cumple. Se vinculan por folio y cada una se evalúa por separado ([`RN-67`](RN-67-matriz-de-compatibilidad-objeto-objeto.md)).

## Trazabilidad

- Norma: [NRM-06](../normativa/NRM-06-transito-y-licencias.md) `[P]`
- Reglas relacionadas: [RN-20](RN-20-compatibilidad-vehiculo-objeto-del-traslado.md), [RN-21](RN-21-capacidad-de-pasajeros-y-carga.md), [RN-37](RN-37-coherencia-de-la-secuencia-de-casetas.md), [RN-53](RN-53-cierre-del-manifiesto-al-despacho.md), [RN-61](RN-61-sustitucion-de-vehiculo-recalcula-valores-congelados.md), [RN-67](RN-67-matriz-de-compatibilidad-objeto-objeto.md), [RN-69](RN-69-inventario-de-la-carga-y-acta-de-entrega.md)
- Casos especiales: [CE-18](../../02-requisitos/casos-especiales/CE-18-carga-y-pasajeros-en-la-misma-mision.md) — candidatas `RN-C18b`, `RN-C18e` · [CE-08](../../02-requisitos/casos-especiales/CE-08-multi-destino-con-esperas-prolongadas-en-sitio.md)
- Actores: ACT-02 declara · ACT-05 despacha y captura efectivos · ACT-06 registra novedades en ruta
