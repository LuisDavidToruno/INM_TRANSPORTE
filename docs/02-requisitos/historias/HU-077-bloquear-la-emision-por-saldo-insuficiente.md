# HU-077 — Bloquear la emisión por saldo insuficiente distinguiendo el fondo agotado de la cuota copada

| Campo | Valor |
|---|---|
| **Módulo** | M-09 Combustible |
| **Actor** | ACT-07 Encargado de Combustible |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — falta la decisión sobre si la institución admite despachar una misión **sin fondo asignado** (insumo #1): el punto de control `PC-08` lo deja abierto y de ello depende si el saldo insuficiente bloquea o marca. Falta también si una asignación puede imputarse a un fondo de otro ámbito (insumo #27) |

## Historia

**Como** Encargado de Combustible
**quiero** que el sistema me bloquee la emisión cuando no hay saldo y me diga **cuál de las dos causas** es —fondo agotado o cuota trimestral copada—
**para** que el Jefe de Transporte no pierda una semana pidiendo una ampliación que nadie le puede aprobar

## Contexto

Son dos problemas distintos con dos salidas distintas. El **fondo agotado** se resuelve dentro de la institución con una ampliación. La **cuota trimestral copada** no se resuelve en SIGTI ni en la institución sola: exige reprogramación de cuota ante SIAFI, gestionada por Gerencia Administrativa.

Decirle "fondo agotado" a secas a quien lo que tiene copada es la cuota lo manda a tramitar lo que no corresponde. El mensaje de error es aquí una decisión de diseño, no un detalle de presentación.

Y el bloqueo recae sobre **la emisión, no sobre la misión**: la Orden queda en `PROGRAMADA` con la marca *sin fondo asignado*.

## Reglas que la gobiernan

- [RN-26](../../01-negocio/reglas/RN-26-fondo-de-combustible-aprobado.md) — Sin saldo suficiente no hay emisión; `tolerancia_sobregiro` con valor inicial cero
- [RN-88](../../01-negocio/reglas/RN-88-saldo-proyectado-del-fondo.md) — Saldo contable **y** proyectado a la vista antes de emitir
- [RN-54](../../01-negocio/reglas/RN-54-cuota-trimestral-de-compromiso.md) — La cuota copada es causa distinta y se nombra como tal
- [RN-39](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md) — `tolerancia_sobregiro` no se sube "por esta vez": doble control con vigencia
- [RN-28](../../01-negocio/reglas/RN-28-comprobacion-del-consumo-de-combustible.md) — El consumo que ocurra se registra igual, aunque no haya asignación emitida

## Casos especiales que la afectan

- [CE-23](../casos-especiales/CE-23-fondo-agotado-con-misiones-programadas.md) — Eje de la historia
- [CE-21](../casos-especiales/CE-21-galonaje-que-no-cuadra-con-kilometraje.md) — Lo que ocurre aguas abajo cuando el despacho sale sin fondo

## Criterios de aceptación

```gherkin
# language: es
Característica: Bloqueo de la emisión por saldo insuficiente, con causa distinguida

  Antecedentes:
    Dado un fondo "FND-2026-09-004" en estado "ENTREGADO"
    Y el parámetro "tolerancia_sobregiro" en "L 0.00"
    Y una Orden de Misión "OM-2026-0533" en estado "PROGRAMADA" con estimado de combustible y peajes de "L 6,200.00"

  Escenario: Se bloquea por saldo del fondo y se indica cuánto falta
    Dado un saldo disponible de "L 2,150.00" en el fondo "FND-2026-09-004"
    Cuando el Encargado de Combustible intenta emitir la asignación de "OM-2026-0533" por "L 6,200.00"
    Entonces el sistema rechaza la emisión
    Y muestra "Saldo insuficiente en el fondo FND-2026-09-004: disponible L 2,150.00, requerido L 6,200.00. Faltan L 4,050.00. Se resuelve por ampliación del fondo."
    Y no muestra el texto "fondo agotado" sin indicar la causa

  Escenario: Se bloquea por cuota trimestral copada y se nombra la otra causa
    Dado un saldo disponible de "L 48,000.00" en el fondo "FND-2026-09-004"
    Y una cuota del trimestre "2026-T3" copada para la unidad ejecutora "Dirección Administrativa"
    Cuando el Encargado de Combustible intenta emitir la asignación de "OM-2026-0533" por "L 6,200.00"
    Entonces el sistema rechaza la emisión
    Y muestra "Excede en L 6,200.00 la cuota de compromiso del trimestre 2026-T3 para la unidad ejecutora Dirección Administrativa, según ARGOS al 12/09/2026. Esto no se resuelve con una ampliación de fondo: requiere reprogramación de cuota ante SIAFI, gestionada por Gerencia Administrativa."

  Escenario: Se indica qué fondo sí cubriría la asignación
    Dado un fondo "FND-2026-09-007" del ámbito "Delegación Choluteca" con saldo de "L 31,000.00"
    Cuando el Encargado de Combustible intenta emitir "OM-2026-0533" contra "FND-2026-09-004" sin saldo
    Entonces el sistema muestra "El fondo FND-2026-09-007 (Delegación Choluteca) tiene saldo suficiente, pero la misión pertenece al ámbito Gerencia Administrativa."

  Escenario: El bloqueo recae sobre la emisión, no sobre la misión
    Cuando el sistema bloquea la emisión de "OM-2026-0533"
    Entonces la Orden de Misión "OM-2026-0533" permanece en estado "PROGRAMADA"
    Y queda con la marca "sin fondo asignado" visible
    Y esa marca se arrastra visible hasta la liquidación

  Escenario: No se sube la tolerancia de sobregiro desde esta pantalla
    Cuando el Encargado de Combustible busca una opción de continuar de todos modos
    Entonces el sistema no ofrece ninguna opción de sobregiro
    Y muestra "tolerancia_sobregiro es un parámetro con vigencia: lo carga el Administrador con respaldo documental y lo pone en vigencia Gerencia Administrativa."

  Escenario: El consumo que ocurra sin asignación se registra igual
    Dado que "OM-2026-0533" se despachó con la marca "sin fondo asignado" y responsable nominado
    Cuando el motorista registra un abastecimiento de "9.5" galones por "L 1,235.00"
    Entonces el sistema acepta el registro
    Y lo imputa al fondo que se constituya después
    Y la misión conserva la marca "sin fondo asignado" hasta la liquidación
```

## Fuera de alcance

- La solicitud de ampliación del fondo — es [HU-075](HU-075-ampliar-el-fondo-agotado-y-resolver-la-prelacion.md)
- La aprobación por encima de cuota con acuse — es [HU-072](HU-072-aprobar-fondo-verificando-cuota-trimestral.md)
- La reprogramación de cuota ante SIAFI: se gestiona fuera de SIGTI
- La decisión de despachar o no sin fondo asignado: la toma una persona con responsable nominado, no el sistema

## Notas y pendientes

- `[C]` **¿Admite la institución despachar una misión sin fondo asignado?** El punto de control `PC-08` lo deja abierto. Propuesta escalada al PO en [CE-23](../casos-especiales/CE-23-fondo-agotado-con-misiones-programadas.md): *sí, con responsable nominado, motivo y marca visible hasta la liquidación* — insumo **#1**
- `[C]` ¿Puede una asignación imputarse a un fondo de otro ámbito con autorización de Gerencia Administrativa? — insumo **#27**
- `[I]` Que SIGTI valide contra la cuota trimestral es implicación de requerimiento del equipo ([NRM-04](../../01-negocio/normativa/NRM-04-presupuesto-siafi.md)), no articulado citable
