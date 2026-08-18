# HU-114 — Cerrar el manifiesto al despachar y entregar la lista de abordo impresa con folio y QR

| Campo | Valor |
|---|---|
| **Módulo** | M-17 Traslado de Personas Externas · M-07 Programación y Despacho · M-15 Formatos Oficiales e Impresión |
| **Actor** | ACT-05 Encargado de Despacho |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — depende del insumo #40 (rutas de lista abierta) |

## Historia

**Como** Encargado de Despacho
**quiero** que al despachar la misión el manifiesto quede cerrado y se imprima la lista de abordo con folio y QR
**para** que exista una declaración firme de para qué y con quiénes salió el vehículo, contra la cual se pueda comparar lo que efectivamente ocurrió

## Contexto

**El manifiesto es la declaración de para qué salió el vehículo.** Si se puede editar después, deja de ser una declaración y pasa a ser un resumen ajustado a lo que ocurrió — que es exactamente lo contrario de un control ([RN-53](../../01-negocio/reglas/RN-53-cierre-del-manifiesto-al-despacho.md)).

Esto importa por una razón concreta: [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md) `[V]` prohíbe usar vehículos del Estado para tareas ajenas a la función, *incluido el traslado de funcionarios, empleados y sus familias a residencias o asuntos personales*, y la Circular STLCC-ONADICI 022-03-2024 sobre uso indebido de vehículos `[V]` es reciente y específica. **La única forma de detectar ese uso es comparar lo autorizado contra lo ocurrido.** Sin manifiesto cerrado no hay contra qué comparar.

La lista impresa es lo que el motorista porta. [RN-51](../../01-negocio/reglas/RN-51-minimizacion-de-datos-de-personas-externas.md) advierte que **el papel sale del control técnico del sistema**: por eso lleva los datos mínimos indispensables para el control en carretera, no el manifiesto completo.

## Reglas que la gobiernan

- [RN-53](../../01-negocio/reglas/RN-53-cierre-del-manifiesto-al-despacho.md) — **Regla rectora**: al despachar se cierra; después solo hay novedades
- [RN-41](../../01-negocio/reglas/RN-41-congelamiento-del-valor-al-autorizar.md) — El manifiesto cerrado se congela junto con el resto de valores autorizados
- [RN-25](../../01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md) — Todo documento de control en carretera se emite impreso, con folio único y QR verificable
- [RN-51](../../01-negocio/reglas/RN-51-minimizacion-de-datos-de-personas-externas.md) — La versión impresa lleva **los mínimos indispensables**, no el manifiesto completo
- [RN-52](../../01-negocio/reglas/RN-52-registro-de-consultas-a-manifiestos.md) — Imprimir el manifiesto **es un acceso** y se registra como consulta con impresión
- [RN-05](../../01-negocio/reglas/RN-05-registro-cerrado-no-se-edita.md) — Un registro cerrado no se edita

## Casos especiales que la afectan

- [CE-18](../casos-especiales/CE-18-carga-y-pasajeros-en-la-misma-mision.md) — La carga y las personas que aparecen en el predio a las cinco de la mañana

## Criterios de aceptación

> Todos los nombres de estos escenarios son **ficticios de prueba**.

```gherkin
# language: es
Característica: Cierre del manifiesto al despachar la misión

  Antecedentes:
    Dado una Orden de Misión "OM-2026-0451" en estado "PROGRAMADA"
    Y un manifiesto con "3" personas externas y "2" servidores de la institución
    Y un motorista "José Martínez" declarado titular

  Escenario: Se rechaza el despacho con el manifiesto incompleto
    Dado que la solicitud declaró "4" personas externas y el manifiesto tiene "3" fichas
    Cuando el Encargado de Despacho intenta despachar "OM-2026-0451"
    Entonces el sistema rechaza el despacho
    Y muestra "El manifiesto tiene 3 de las 4 personas externas declaradas. Complete la ficha faltante o corrija la cantidad antes de despachar."
    Y la Orden de Misión permanece en estado "PROGRAMADA"

  Escenario: Se rechaza editar el manifiesto después del despacho
    Dado que "OM-2026-0451" fue despachada el "2026-09-18" a las "05:52"
    Cuando el Encargado de Despacho intenta retirar del manifiesto a "Carla de Prueba Tres"
    Entonces el sistema rechaza la edición
    Y muestra "El manifiesto se cerró el 18/09/2026 a las 05:52. Los cambios posteriores se registran como novedad de ruta, no como edición."
    Y ofrece registrar una novedad de tipo "persona que no abordó"

  Escenario: Se rechaza despachar sin haber emitido la lista de abordo impresa
    Cuando el Encargado de Despacho intenta cerrar el despacho de "OM-2026-0451" sin emitir la lista de abordo
    Entonces el sistema rechaza el cierre del despacho
    Y muestra "La lista de abordo es documento de control en carretera. Emítala e imprímala antes de entregar las llaves."

  Escenario: El manifiesto se cierra y se congela como versión autorizada
    Cuando el Encargado de Despacho despacha "OM-2026-0451" el "2026-09-18" a las "05:52"
    Entonces el sistema congela el manifiesto como versión "1"
    Y registra el cierre con el actor "Encargado de Despacho", la fecha "2026-09-18 05:52" y la huella del contenido
    Y el manifiesto queda en estado "CERRADO"

  Escenario: La lista de abordo impresa lleva folio, QR y solo los datos indispensables
    Cuando el Encargado de Despacho emite la lista de abordo de "OM-2026-0451"
    Entonces el documento lleva folio único y QR verificable
    Entonces el documento muestra por persona externa: nombre, tipo y número de identificación, origen y destino
    Y no muestra ningún campo de clase salud, etnia, situación migratoria ni condición de vulnerabilidad
    Y distingue visualmente a las personas externas del personal de la institución

  Escenario: La impresión de la lista de abordo se registra como consulta
    Cuando el Encargado de Despacho imprime la lista de abordo de "OM-2026-0451"
    Entonces el sistema registra una consulta con alcance "COMPLETO" y modalidad "CON IMPRESIÓN"
    Y deja constancia del folio del documento impreso, del consultante y de la fecha y hora

  Escenario: La reimpresión no genera un folio nuevo ni un manifiesto nuevo
    Dado una lista de abordo ya emitida con folio "LA-2026-000318"
    Cuando el Encargado de Despacho reimprime la lista de abordo de "OM-2026-0451"
    Entonces el documento conserva el folio "LA-2026-000318"
    Y se marca como "REIMPRESIÓN 2"
    Y el sistema registra una consulta nueva con modalidad "CON IMPRESIÓN"
```

## Fuera de alcance

- El registro de las novedades en ruta — es [HU-116](HU-116-registrar-novedades-del-manifiesto-en-ruta.md)
- La comparación manifiesto contra novedades en la liquidación — es de [CU-15](../casos-de-uso/CU-15-liquidar-la-mision-y-conciliar.md)
- El acta de entrega y recepción de las personas en el destino — es [HU-115](HU-115-cadena-de-custodia-de-personas-externas.md)
- La verificación del QR en carretera — es [HU-019](HU-019-verificacion-del-salvoconducto-en-carretera.md), con su `[C]` de punto público aún abierto

## Notas y pendientes

- `[C]` **¿Opera la institución rutas de lista abierta**, con paradas donde suben y bajan personas? — insumo #40. **Si la respuesta es sí, esta historia no aplica tal cual**: [RN-53](../../01-negocio/reglas/RN-53-cierre-del-manifiesto-al-despacho.md) prevé cerrar por tramo o sustituir el manifiesto por conteo con puntos de abordaje, y esa variante **debe modelarse explícitamente en lugar de forzar la regla general**. Es lo que mantiene la historia en borrador
- `[C]` Formato en papel vigente de la lista de pasajeros, si existe — insumo #2. Ese formato es el diseño de este documento
- `[C]` Parque real de impresoras en sede y delegaciones — insumo #70. Decide si el QR es vía primaria o conveniencia
- `[I]` Que la reimpresión conserve folio y se numere es derivación de [RN-25](../../01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md), no cita de norma
