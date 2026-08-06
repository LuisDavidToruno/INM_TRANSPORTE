# Plantilla — Criterios de aceptación en Gherkin español

Archivo: `docs/05-calidad/features/<slug>.feature`

Se escriben **en español** porque los va a leer el personal de la institución durante la validación, no solo el desarrollador. Un criterio que la Gerencia Administrativa no puede leer no sirve para validar nada.

## Palabras clave

| Español | Uso |
|---|---|
| `Característica:` | Qué capacidad se describe |
| `Antecedentes:` | Contexto común a todos los escenarios del archivo |
| `Escenario:` | Un caso concreto |
| `Esquema del escenario:` | Escenario parametrizado |
| `Ejemplos:` | Tabla de datos del esquema |
| `Dado` | Estado previo |
| `Cuando` | La acción que se evalúa |
| `Entonces` | El resultado esperado |
| `Y` / `Pero` | Continuación del paso anterior |

## Cómo escribir escenarios que sirvan

**Un `Cuando` por escenario.** Si hay dos acciones, son dos escenarios. Un escenario con tres `Cuando` no dice qué falló cuando falla.

**Datos concretos, no descripciones.** `Dado un vehículo con peso bruto de 12000 kg` — no `Dado un vehículo pesado`. El número es lo que se prueba.

**Lenguaje del negocio, no de la interfaz.** `Cuando el Encargado de Despacho asigna el motorista` — no `Cuando hace clic en el botón Asignar`. Los botones cambian; la regla no.

**El mensaje de error es parte del criterio.** Un rechazo silencioso o con un mensaje genérico no cumple. En este dominio el usuario debe entender *por qué* el sistema lo bloqueó, porque muchas veces tendrá que resolverlo con una gestión administrativa.

**Cubre el camino infeliz primero.** En este sistema, los escenarios de rechazo son los que tienen consecuencia legal. El camino feliz casi siempre es el escenario más aburrido del archivo.

## Ejemplo completo

```gherkin
# language: es
Característica: Conciliación de combustible contra kilometraje recorrido
  Como Encargado de Combustible
  quiero que el sistema detecte desviaciones entre el galonaje despachado y el
  kilometraje recorrido
  para responder ante el hallazgo de auditoría antes de que lo levante el TSC

  Antecedentes:
    Dado un vehículo "Pickup Hilux" con placa "PAA-1234"
    Y un rendimiento esperado registrado de "12.0" km por galón
    Y un umbral de desviación tolerada del "15" por ciento

  Escenario: Consumo dentro del rango tolerado
    Dado una Orden de Misión "OM-2026-0451" en estado "RETORNADA"
    Y un odómetro de salida de "84500" km y de retorno de "84860" km
    Y un despacho de combustible de "31.0" galones
    Cuando el Encargado de Combustible ejecuta la conciliación de la misión
    Entonces el rendimiento calculado es "11.61" km por galón
    Y la desviación es del "3.2" por ciento
    Y la misión se marca como "CONCILIADA"

  Escenario: Consumo excede el umbral y genera hallazgo
    Dado una Orden de Misión "OM-2026-0452" en estado "RETORNADA"
    Y un odómetro de salida de "84860" km y de retorno de "85100" km
    Y un despacho de combustible de "40.0" galones
    Cuando el Encargado de Combustible ejecuta la conciliación de la misión
    Entonces el rendimiento calculado es "6.00" km por galón
    Y la desviación es del "50.0" por ciento
    Y el sistema genera un hallazgo de tipo "CONSUMO_EXCEDIDO"
    Y la Orden de Misión no puede pasar a "CERRADA" mientras el hallazgo esté abierto
    Y notifica al Encargado de Transporte y a la Gerencia Administrativa

  Escenario: Odómetro de retorno menor al de salida
    Dado una Orden de Misión "OM-2026-0453" en estado "EN_RUTA"
    Y un odómetro de salida de "85100" km
    Cuando el motorista registra un odómetro de retorno de "84900" km
    Entonces el sistema rechaza el registro
    Y muestra "El kilometraje de retorno (84,900) es menor al de salida (85,100). Verifique la lectura."
    Y permite registrar una justificación de "sustitución de odómetro" con adjunto de respaldo

  Esquema del escenario: Clasificación de la desviación
    Dado una Orden de Misión con "<km>" kilómetros recorridos
    Y un despacho de "<galones>" galones
    Cuando se ejecuta la conciliación
    Entonces la clasificación es "<clasificacion>"

    Ejemplos:
      | km  | galones | clasificacion      |
      | 360 | 31.0    | CONFORME           |
      | 360 | 36.0    | REVISAR            |
      | 240 | 40.0    | CONSUMO_EXCEDIDO   |
      | 600 | 20.0    | REVISAR            |
```

## Sobre el último escenario del esquema

`600 km con 20 galones` da 30 km/galón — muy por encima de lo esperado. **También es una desviación y también se revisa.** Un rendimiento imposiblemente bueno normalmente significa que alguien no registró un despacho, no que el vehículo mejoró. Los criterios de aceptación deben cubrir la desviación en ambas direcciones.
