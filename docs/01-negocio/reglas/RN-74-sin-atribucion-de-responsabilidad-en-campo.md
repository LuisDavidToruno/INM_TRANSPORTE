# RN-74 — El registro de campo no captura atribución de responsabilidad; la responsabilidad se determina en el expediente, por quien corresponde

| Campo | Valor |
|---|---|
| **Módulos** | M-12, M-08, M-16, M-17 |
| **Origen** | Casos especiales [CE-03](../../02-requisitos/casos-especiales/CE-03-accidente-de-transito-en-mision.md) y [CE-04](../../02-requisitos/casos-especiales/CE-04-robo-de-vehiculo-o-de-carga-en-mision.md) · Normas [NRM-01](../normativa/NRM-01-control-interno-tsc.md) y [NRM-07](../normativa/NRM-07-transparencia-y-datos-personales.md) |
| **Verificación** | `[P]` la separación entre registro del hecho y determinación de responsabilidad — práctica del debido proceso administrativo. `[I]` la prohibición del campo en la captura: decisión de producto del equipo |
| **Tipo** | Bloqueo duro |
| **Configurable** | No |

## Enunciado

El sistema **no debe** ofrecer a quien registra un hecho en campo —accidente, sustracción, daño, faltante— **ningún campo de atribución de responsabilidad, culpa o dolo**, sea de selección o de texto.

Lo que se captura en campo es **el hecho**: hora, lugar, odómetro, descripción, personas y vehículos involucrados, autoridad interviniente, fotografías.

La responsabilidad se determina **en el expediente de investigación de M-12**, por la instancia que corresponde, con procedimiento, descargo del interesado, resolución y notificación. El sistema **registra** esa determinación cuando existe, con su acto y su autor; **no la produce**.

Ningún hallazgo generado por el sistema imputa responsabilidad a persona alguna: es **marca de seguimiento**.

## Justificación

Un motorista que acaba de tener un accidente, a la orilla de la carretera, con un tercero gritándole, no está en condiciones de calificar jurídicamente lo que pasó — y no le corresponde. Un campo *"¿de quién fue la culpa?"* en esa pantalla produce dos daños: una declaración tomada bajo presión que después pesa en un expediente, y una atribución hecha por quien no tiene competencia para hacerla.

Lo mismo, con más razón, en la sustracción: el motorista asaltado a mano armada no es responsable de nada, y un sistema que le pide declarar responsabilidad lo pone en la posición de acusarse.

La consecuencia práctica es la contraria a la que se busca: si registrar el hecho implica autoinculparse, **el hecho no se registra**. Y un accidente no registrado es peor que cualquier atribución mal hecha.

## Condiciones de aplicación

Aplica a toda captura hecha por el conductor o por personal en el sitio del hecho, con o sin conectividad.

Aplica también al **faltante de carga** ([`RN-69`](RN-69-inventario-de-la-carga-y-acta-de-entrega.md)) y a la **diferencia de fondo** al liquidar ([`RN-86`](RN-86-obligacion-de-reintegro-por-saldo-no-devuelto.md)): se declara la diferencia, no el culpable.

**No aplica** al expediente de M-12, que existe precisamente para determinar responsabilidad, ni al registro de la **resolución** que la determine.

## Comportamiento esperado

1. Ante la declaración de un accidente o una sustracción, el cliente de campo **muestra la guía de actuación antes de cualquier formulario**: qué hacer, a quién llamar, qué no mover, qué fotografiar. El registro mínimo se puede **diferir sin perderse**.
2. Los formularios de campo contienen hechos observables. La descripción libre existe, pero el sistema no pregunta por causa ni por culpa.
3. De un **tercero involucrado** se capturan solo los datos del catálogo autorizado —identificación, contacto, vehículo, aseguradora— **sin diagnóstico médico ni dato clínico**, y toda consulta posterior queda registrada ([`RN-51`](RN-51-minimizacion-de-datos-de-personas-externas.md), [`RN-52`](RN-52-registro-de-consultas-a-manifiestos.md)).
4. El evento abre expediente en M-12 con responsable de seguimiento y plazo. El expediente **admite adjuntar el acto de determinación de responsabilidad** cuando la instancia competente lo emita.
5. Los hallazgos que el sistema propone al liquidar se clasifican y justifican individualmente ([`RN-08`](RN-08-cadena-de-trazabilidad-para-cierre.md)); ninguno nombra un responsable económico.
6. Si el vehículo tiene **póliza vigente a la fecha del hecho**, el sistema dispara el recordatorio de **aviso al asegurador dentro del plazo contractual**, tratado como parámetro con vigencia ([`RN-39`](RN-39-parametros-normativos-con-vigencia.md)) y nunca como número fijo.

## Casos límite

- **`[C]` ¿Quién responde patrimonialmente por el vehículo sustraído bajo custodia de misión?** Insumo #47. Opciones y costo:

  | Opción | Costo |
  |---|---|
  | El sistema solo registra hechos y deja el expediente a la instancia competente | Consistente con no capturar culpa en campo. Riesgo: la institución esperaba que SIGTI le dijera quién paga, y no se lo va a decir |
  | El sistema registra una **determinación de responsabilidad** como resultado del expediente de M-12, con su acto y su autor | Requiere modelar el procedimiento administrativo, que es materia de Talento Humano y de la Gerencia Administrativa |
  | Se cablea que el custodio de misión responde siempre | **Descartado**: no hay norma verificada que lo sostenga, y el motorista asaltado a mano armada no es responsable de nada |

  **Recomendación del análisis, no decisión:** la primera con el gancho de la segunda — el expediente admite adjuntar el acto cuando exista, sin que SIGTI lo produzca.
- **Parte policial que atribuye responsabilidad.** Se adjunta como documento, con su número y autoridad. Lo que dice el parte es un dato del expediente, no un campo capturado por el motorista.
- **Descripción libre en la que el conductor se autoinculpa.** El sistema no puede impedirlo y no lo censura: conserva el texto tal cual. Lo que evita es **provocarlo** con un campo diseñado para eso.
- **Institución que quiere el campo de culpa** porque su formato en papel lo tiene. El formato en papel es un documento de requisitos, pero no todos sus campos sobreviven al análisis: este es uno que no. Se documenta la decisión y se ofrece, en su lugar, la determinación en el expediente.

## Trazabilidad

- Normas: [NRM-01](../normativa/NRM-01-control-interno-tsc.md) `[P]` · [NRM-07](../normativa/NRM-07-transparencia-y-datos-personales.md) `[P]`
- Reglas relacionadas: [RN-08](RN-08-cadena-de-trazabilidad-para-cierre.md), [RN-39](RN-39-parametros-normativos-con-vigencia.md), [RN-51](RN-51-minimizacion-de-datos-de-personas-externas.md), [RN-52](RN-52-registro-de-consultas-a-manifiestos.md), [RN-69](RN-69-inventario-de-la-carga-y-acta-de-entrega.md), [RN-70](RN-70-interrupcion-en-ruta-con-desenlace-obligatorio.md), [RN-75](RN-75-bien-retenido-o-sustraido-no-sale-del-registro.md), [RN-86](RN-86-obligacion-de-reintegro-por-saldo-no-devuelto.md)
- Casos especiales: [CE-03](../../02-requisitos/casos-especiales/CE-03-accidente-de-transito-en-mision.md) — candidatas `RN-c:guia-de-actuacion-en-accidente-precede-al-registro`, `RN-c:sin-campo-de-valoracion-de-culpa`, `RN-c:datos-minimos-de-terceros-en-siniestro`, `RN-c:aviso-al-asegurador-en-plazo-parametrizado` · [CE-04](../../02-requisitos/casos-especiales/CE-04-robo-de-vehiculo-o-de-carga-en-mision.md)
- Insumos pendientes: #47 responsabilidad patrimonial por el bien sustraído bajo custodia
