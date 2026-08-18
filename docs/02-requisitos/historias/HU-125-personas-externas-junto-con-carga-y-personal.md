# HU-125 — Evaluar la combinación de personas externas con personal de la institución y con carga, tramo por tramo

| Campo | Valor |
|---|---|
| **Módulo** | M-17 Traslado de Personas Externas · M-07 Programación y Despacho · M-06 Solicitudes de Transporte |
| **Actor** | ACT-04 Jefe de Transporte |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — la matriz objeto × objeto no está poblada y el insumo #39 sigue abierto |

## Historia

**Como** Jefe de Transporte
**quiero** que el sistema evalúe par a par y tramo a tramo la combinación de personas externas con personal de la institución y con carga
**para** no autorizar una configuración que en el papel cabe y en la carretera produce lesionados o expone datos que no debían mezclarse

## Contexto

Es el caso ordinario, no el raro: seis personas y el mobiliario de la delegación, y en el vehículo solo cabe una de las dos cosas ([CE-18](../casos-especiales/CE-18-carga-y-pasajeros-en-la-misma-mision.md)). La variante que toca a M-17 es la cuarta del caso: *"en el retorno viajan dos personas ajenas a la institución. Son **otro objeto de traslado**, con manifiesto, minimización de datos y registro de consultas propios"*.

Dos cosas que ninguna regla anterior a `RN-67` podía expresar:

- **La incompatibilidad por naturaleza.** *Personas junto a bidones de combustible* es el ejemplo que la propia [RN-20](../../01-negocio/reglas/RN-20-compatibilidad-vehiculo-objeto-del-traslado.md) usa para justificarse — y su matriz no lo puede decir, porque solo cruza vehículo contra objeto.
- **El tramo.** El mobiliario se entrega en el destino y libera la paila; de regreso suben dos personas externas que no iban en la ida. **La ida cumple y el retorno no.**

Y hay un límite que no se negocia: la configuración **nunca** se resuelve trasladando personas fuera de plazas homologadas. La paila no es capacidad de pasajeros ([RN-21](../../01-negocio/reglas/RN-21-capacidad-de-pasajeros-y-carga.md)), y no existe parámetro, autorización jerárquica ni emergencia que lo levante.

## Reglas que la gobiernan

- [RN-67](../../01-negocio/reglas/RN-67-matriz-de-compatibilidad-objeto-objeto.md) — **Regla rectora**: matriz objeto × objeto evaluada par a par; **la ausencia de entrada bloquea**
- [RN-68](../../01-negocio/reglas/RN-68-compatibilidad-y-capacidad-por-tramo.md) — Compatibilidad y capacidad se evalúan por tramo, sobre la configuración real de cada tramo
- [RN-21](../../01-negocio/reglas/RN-21-capacidad-de-pasajeros-y-carga.md) — No se excede la capacidad; el conteo incluye al motorista; la paila no es capacidad de pasajeros
- [RN-20](../../01-negocio/reglas/RN-20-compatibilidad-vehiculo-objeto-del-traslado.md) — El tipo de vehículo debe ser compatible con cada objeto declarado
- [RN-51](../../01-negocio/reglas/RN-51-minimizacion-de-datos-de-personas-externas.md) — Las personas externas se registran con el catálogo mínimo, separadas del personal de la institución
- [RN-53](../../01-negocio/reglas/RN-53-cierre-del-manifiesto-al-despacho.md) — El manifiesto distingue personal de la institución de personas externas
- [RN-39](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md) · [RN-40](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md) — La matriz es catálogo con vigencia y se aplica la vigente a la fecha del hecho

## Casos especiales que la afectan

- [CE-18](../casos-especiales/CE-18-carga-y-pasajeros-en-la-misma-mision.md) — **El caso que origina esta historia**
- [CE-02](../casos-especiales/CE-02-averia-mecanica-en-ruta.md) — Transbordo con personas externas a bordo

## Criterios de aceptación

> Los nombres de personas externas de estos escenarios son **ficticios de prueba**.

```gherkin
# language: es
Característica: Compatibilidad objeto × objeto con personas externas, por tramo

  Antecedentes:
    Dado una matriz de compatibilidad objeto × objeto vigente al "2026-09-01"
    Y un vehículo "Pickup doble cabina 4x4" correlativo "VEH-0055" con "5" plazas homologadas incluido el motorista y "800" kg de capacidad de carga
    Y una solicitud "SOL-2026-0912" con los tramos "Tegucigalpa → Danlí", "Danlí → Trojes", "Trojes → Danlí" y "Danlí → Tegucigalpa"

  Escenario: Se bloquea el par sin entrada en la matriz
    Dado que la matriz no tiene entrada para el par "personas externas" × "persona bajo custodia"
    Cuando el Jefe de Transporte evalúa una configuración que declara ambos objetos
    Entonces el sistema bloquea la programación
    Y muestra "La combinación 'personas externas' con 'persona bajo custodia' no está definida en la matriz vigente al 01/09/2026. Una combinación sin definir se bloquea: no se interpreta como permitida."

  Escenario: Se bloquea el par declarado incompatible
    Dado que la matriz declara incompatible el par "personas externas" × "combustible en bidones"
    Cuando el Jefe de Transporte evalúa una configuración con "2" personas externas y "4" bidones de combustible
    Entonces el sistema bloquea la programación
    Y muestra "Personas externas y combustible en bidones son incompatibles en la matriz vigente al 01/09/2026. Divida la misión o difiera uno de los objetos."
    Y presenta las salidas validadas con su costo

  Escenario: Se bloquea la configuración que solo cabe usando la paila para personas
    Cuando el Jefe de Transporte evalúa el tramo "Tegucigalpa → Danlí" con "3" personas externas, "3" servidores, "1" motorista y "400" kg de mobiliario
    Entonces el sistema bloquea la asignación de "VEH-0055"
    Y muestra "El vehículo tiene 5 plazas homologadas y la configuración requiere 7 ocupantes. La paila no es capacidad de pasajeros: no hay autorización que lo permita."
    Y presenta las salidas validadas: otro tipo de vehículo, división en misiones hermanas, reducción del alcance o diferir un objeto

  Escenario: Se bloquea el tramo de retorno aunque la ida sea viable
    Dado que el tramo "Trojes → Danlí" libera los "400" kg de mobiliario entregado
    Y que en el tramo "Danlí → Tegucigalpa" se incorporan "2" personas externas y un generador dañado de "180" kg
    Cuando el Jefe de Transporte evalúa la configuración completa
    Entonces el sistema acepta los tramos "Tegucigalpa → Danlí", "Danlí → Trojes" y "Trojes → Danlí"
    Y bloquea el tramo "Danlí → Tegucigalpa"
    Y muestra "El tramo Danlí → Tegucigalpa requiere 8 ocupantes y el vehículo tiene 5 plazas homologadas. Evalúe cada tramo antes de programar."

  Escenario: El par compatible con condiciones exige manifiesto que distinga y acuse del despachador
    Dado que la matriz declara "personas externas" × "personal de la institución" como compatible con condiciones
    Cuando el Jefe de Transporte programa la misión con "2" personas externas y "2" servidores
    Entonces el sistema acepta la programación
    Y exige que el manifiesto distinga las personas externas del personal de la institución
    Y imprime en la Orden de Misión las condiciones del par
    Y exige el acuse del Encargado de Despacho de haberlas leído antes de despachar

  Escenario: La evaluación se aplica con la matriz vigente a la fecha del hecho
    Dado una misión ejecutada el "2026-09-18" con la matriz vigente al "2026-09-01"
    Y una matriz nueva vigente desde el "2027-01-01"
    Cuando el Auditor Interno revisa la evaluación de esa misión el "2027-03-10"
    Entonces el sistema muestra el resultado calculado con la matriz vigente al "2026-09-18"
    Y muestra el identificador de la versión de matriz aplicada

  Escenario: Queda constancia del bloqueo, de las salidas ofrecidas y de la elegida
    Dado un bloqueo por capacidad en el tramo "Tegucigalpa → Danlí"
    Cuando el Jefe de Transporte elige la salida "división en dos Órdenes de Misión hermanas"
    Entonces el sistema registra el bloqueo con su dato concreto, las salidas ofrecidas, la elegida, quién la eligió y cuándo
    Y crea las dos Órdenes de Misión vinculadas explícitamente entre sí
    Y muestra el costo adicional estimado de combustible y peajes antes de confirmar
```

## Fuera de alcance

- El registro del manifiesto en sí — es [HU-111](HU-111-registrar-manifiesto-de-personas-externas.md)
- El acta de entrega de la **carga** con inventario y consignatario — la gobierna [RN-69](../../01-negocio/reglas/RN-69-inventario-de-la-carga-y-acta-de-entrega.md)
- La carga que aparece en el predio a las cinco de la mañana — es materia de [CU-06](../casos-de-uso/CU-06-despachar-y-registrar-salida.md) y de [CE-18](../casos-especiales/CE-18-carga-y-pasajeros-en-la-misma-mision.md) sección 6
- El peso y la ocupación efectivos capturados al despachar y su indicador por dependencia — es de M-07 y [RN-21](../../01-negocio/reglas/RN-21-capacidad-de-pasajeros-y-carga.md)
- El convoy bajo una misma Orden de Misión: `[C]` insumo #62, escalado al PO

## Notas y pendientes

- `[C]` **¿Traslada la institución personas bajo custodia o menores?** — insumo #39. Es el par que más cambia la matriz y los requisitos de M-17. Mientras siga abierto, el par **no tiene entrada y por lo tanto bloquea**, que es el comportamiento correcto de [RN-67](../../01-negocio/reglas/RN-67-matriz-de-compatibilidad-objeto-objeto.md)
- `[C]` **¿Moviliza la institución carga peligrosa o especializada?** — insumo #38. No se infiere ninguna regla de manejo de carga peligrosa
- `[C]` **Los pares de la matriz que la institución debe definir**, con su fundamento: personas externas × combustible en bidones, × material químico, × personal de la institución, × carga suelta sin sujeción. Ninguno se predefine aquí
- `[C]` Qué tipos de carga exigen peso cierto y cuáles admiten rango — insumo #63
- `[I]` Que el bloqueo deba presentar salidas validadas con su costo es la regla candidata `RN-C18c` de [CE-18](../casos-especiales/CE-18-carga-y-pasajeros-en-la-misma-mision.md), **no una regla vigente**. Los escenarios que la ejercitan quedan condicionados a que se escriba
