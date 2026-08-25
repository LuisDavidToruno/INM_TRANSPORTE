# HU-083 — Declarar la fuente de todo ingreso de combustible al tanque, tenga folio o no

| Campo | Valor |
|---|---|
| **Módulo** | M-09 Combustible · M-08 Ejecución y Bitácora |
| **Actor** | ACT-06 Motorista |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — falta si la institución reintegra combustible pagado de peculio propio y bajo qué circuito (insumo #37), si existe control de cisterna institucional y con qué documento se despacha desde ella (insumo #1), y si el fondo de combustible puede absorber un gasto en ruta como una grúa o una llanta (insumo #1) |

## Historia

**Como** Motorista
**quiero** poder registrar el combustible que entró al tanque aunque no venga del fondo de esta misión —prestado de otra dependencia, cargado de la cisterna, o pagado de mi bolsillo—
**para** que la conciliación de la misión no me señale con un rendimiento imposiblemente bueno por unos galones que sí existieron pero que ningún folio respalda

## Contexto

Cuando el fondo se agota, el combustible aparece igual: prestado, de cisterna, o del bolsillo del motorista. Ese es el **préstamo invisible** que describe [CE-23](../casos-especiales/CE-23-fondo-agotado-con-misiones-programadas.md), y su síntoma aguas abajo es el que detecta [CE-21](../casos-especiales/CE-21-galonaje-que-no-cuadra-con-kilometraje.md): un rendimiento por encima del esperado, que casi siempre significa un despacho que nadie anotó.

Si el galón que el motorista pagó de su bolsillo no entra al numerador, la misión aparece con un rendimiento excelente que no significa lo que parece — y el sistema produce un hallazgo falso sobre un hecho real que él sí declaró.

**La práctica ocurre con o sin regla.** Lo que decide si el dato existe es que declararla no le cueste nada al motorista.

## Reglas que la gobiernan

- [RN-83](../../01-negocio/reglas/RN-83-todo-ingreso-de-combustible-es-un-abastecimiento.md) — Todo ingreso al tanque es abastecimiento con **fuente declarada**; nivel de tanque a la salida y al retorno como dato obligatorio de bitácora
- [RN-30](../../01-negocio/reglas/RN-30-conciliacion-galonaje-kilometraje.md) — Entra al numerador **todo** abastecimiento, cualquiera sea su fuente
- [RN-86](../../01-negocio/reglas/RN-86-obligacion-de-reintegro-por-saldo-no-devuelto.md) — El peculio del servidor genera obligación de reintegro **a su favor**, sin afectar el cuadre del fondo
- [RN-87](../../01-negocio/reglas/RN-87-gasto-imprevisto-en-ruta.md) — El gasto imprevisto se registra con su tipo, **no se disfraza de combustible**
- [RN-74](../../01-negocio/reglas/RN-74-sin-atribucion-de-responsabilidad-en-campo.md) — Declarar una fuente irregular no atribuye responsabilidad a quien la declara

## Casos especiales que la afectan

- [CE-23](../casos-especiales/CE-23-fondo-agotado-con-misiones-programadas.md) — La causa
- [CE-21](../casos-especiales/CE-21-galonaje-que-no-cuadra-con-kilometraje.md) — El síntoma
- [CE-02](../casos-especiales/CE-02-averia-mecanica-en-ruta.md) — El gasto imprevisto que acompaña a una avería

## Criterios de aceptación

```gherkin
# language: es
Característica: Fuente declarada de todo ingreso de combustible al tanque

  Antecedentes:
    Dado una Orden de Misión "OM-2026-0512" en estado "EN_RUTA"
    Y un vehículo "TR-0045" con rendimiento esperado de "12.0" km por galón
    Y una asignación de fondo "ASG-2026-00812" agotada

  Escenario: Se rechaza el abastecimiento sin fuente declarada
    Cuando el motorista registra "10.0" galones por "L 1,300.00" sin declarar la fuente
    Entonces el sistema rechaza el registro
    Y muestra "Declare la fuente del combustible: fondo de esta misión, otro fondo, cisterna institucional, otra dependencia, donación o peculio del servidor."

  Escenario: Se registra combustible de cisterna institucional sin folio de este fondo
    Cuando el motorista registra "15.0" galones con fuente "cisterna institucional", odómetro "84,980" km y fotografía del vale de despacho interno
    Entonces el sistema acepta el registro
    Y los 15.0 galones entran al numerador de la conciliación de rendimiento
    Y el registro queda marcado como "sin folio de este fondo", sin producir bloqueo

  Escenario: Se registra combustible prestado por otra dependencia
    Cuando el motorista registra "8.0" galones con fuente "otra dependencia" indicando "Delegación Comayagua"
    Entonces el sistema acepta el registro
    Y genera un pendiente de conciliación entre dependencias visible para el Jefe de Transporte

  Escenario: El peculio del servidor genera obligación a su favor y no altera el cuadre del fondo
    Cuando el motorista registra "9.0" galones por "L 1,170.00" con fuente "peculio del servidor" y fotografía de la factura
    Entonces el sistema acepta el registro
    Y crea una obligación de reintegro de "L 1,170.00" a favor de "Wilmer Cáceres"
    Y esa obligación no afecta el cuadre de la asignación "ASG-2026-00812"
    Y muestra "Registrado. Se generó obligación de reintegro a su favor por L 1,170.00."

  Escenario: Declarar una fuente irregular no imputa responsabilidad al motorista
    Cuando el motorista registra un abastecimiento con fuente "peculio del servidor"
    Entonces el sistema no genera ninguna marca de falta, sanción ni observación contra el motorista
    Y no exige autorización previa para consumar el registro

  Escenario: Se rechaza registrar un gasto que no es combustible como abastecimiento
    Cuando el motorista intenta registrar "L 2,400.00" de servicio de grúa como abastecimiento de combustible
    Entonces el sistema rechaza el registro
    Y muestra "Esto es un gasto imprevisto en ruta, no un abastecimiento. Regístrelo con su tipo, factura y autorización: disfrazarlo de combustible destruye la conciliación de rendimiento."

  Escenario: El nivel de tanque a la salida y al retorno es obligatorio
    Cuando el Encargado de Despacho intenta cerrar el acta de salida sin registrar el nivel del tanque
    Entonces el sistema rechaza el cierre del acta
    Y muestra "Registre el nivel del tanque a la salida. Sin él, un vehículo que sale lleno y retorna vacío produce una conciliación que no significa nada."

  Escenario: Los galones de todas las fuentes entran al mismo numerador
    Dado abastecimientos de "12.0" galones del fondo, "15.0" de cisterna y "9.0" de peculio
    Y un recorrido de "432" km
    Cuando el sistema calcula el rendimiento observado
    Entonces usa "36.0" galones como denominador de galones consumidos
    Y el rendimiento observado es "12.00" km por galón
```

## Fuera de alcance

- La captura general del abastecimiento y su comprobante — es [HU-082](HU-082-registrar-abastecimiento-sin-conectividad.md)
- El cálculo y la clasificación de la desviación — es [HU-088](HU-088-conciliar-galonaje-contra-kilometraje.md)
- El circuito de reintegro al servidor: SIGTI registra la obligación; el pago se gestiona en Administración
- La conciliación entre dependencias por combustible prestado: se genera el pendiente; su liquidación es acto administrativo fuera de esta historia

## Notas y pendientes

- `[C]` **¿La institución reintegra combustible pagado de peculio propio, y bajo qué circuito?** — insumo **#37**. **La práctica ocurre con o sin regla**: el registro del hecho no espera la confirmación
- `[C]` Catálogo `fuente_de_abastecimiento`: se entrega con los valores conocidos y es ampliable sin cambio de código
- `[C]` `admite_gasto_en_ruta_contra_fondo_de_combustible`: si el fondo de combustible puede absorber una grúa o una llanta — insumo **#1**
- `[C]` ¿Existe control de cisterna institucional y con qué documento se despacha desde ella? — insumo **#1**
