# HU-071 — Solicitar el fondo de combustible del período con el arqueo del fondo anterior a la vista

| Campo | Valor |
|---|---|
| **Módulo** | M-09 Combustible |
| **Actor** | ACT-04 Jefe de Transporte |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador |

## Historia

**Como** Jefe de Transporte
**quiero** solicitar el fondo de combustible del período viendo el arqueo del fondo anterior y el detalle por persona de lo que sigue afuera
**para** no pedir dinero nuevo sobre un fondo cuyo saldo real nadie conoce, que es el hallazgo más fácil de levantar para el Tribunal Superior de Cuentas

## Contexto

Hoy la solicitud del fondo se hace por memorando y el estado del fondo anterior se reconstruye a mano cuando alguien lo pregunta. El resultado conocido es que se pide un fondo nuevo mientras siguen sin liquidarse vales del anterior, y nadie puede decir cuánto está afuera ni en manos de quién.

**SIGTI no compra combustible.** Administración aprueba un monto en efectivo o una cantidad de órdenes de pago que Transporte solicita ([`PROP-01`](../../07-gestion/insumos-pendientes.md)). Lo que esta historia captura es la petición de dinero disponible, no una compra ni un contrato con proveedor.

## Reglas que la gobiernan

- [RN-26](../../01-negocio/reglas/RN-26-fondo-de-combustible-aprobado.md) — Circuito solicitud → aprobación → entrega; el fondo tiene período, ámbito y saldo propios
- [RN-86](../../01-negocio/reglas/RN-86-obligacion-de-reintegro-por-saldo-no-devuelto.md) — Arqueo del fondo con detalle por persona de lo no devuelto ni comprobado
- [RN-88](../../01-negocio/reglas/RN-88-saldo-proyectado-del-fondo.md) — El saldo que importa es el proyectado, no el contable
- [RN-29](../../01-negocio/reglas/RN-29-liquidacion-de-combustible.md) — El cierre del fondo anterior exige todas sus asignaciones liquidadas o anuladas
- [RN-48](../../01-negocio/reglas/RN-48-datos-espejo-de-solo-lectura.md) — La partida presupuestaria se toma del espejo de ARGOS, no se captura a mano

## Casos especiales que la afectan

- [CE-23](../casos-especiales/CE-23-fondo-agotado-con-misiones-programadas.md) — El fondo se agota con misiones ya programadas
- [CE-26](../casos-especiales/CE-26-sobrante-o-faltante-al-liquidar.md) — Sobrante o faltante que queda afuera al cerrar el período

## Criterios de aceptación

```gherkin
# language: es
Característica: Solicitud del fondo de combustible del período

  Antecedentes:
    Dado un fondo vigente "FND-2026-08-003" del ámbito "Gerencia Administrativa"
    Y un monto aprobado de "L 180,000.00" con "L 12,400.00" de saldo contable
    Y asignaciones sin liquidar por "L 8,600.00" a nombre de "Wilmer Cáceres" y "Denis Fúnez"

  Escenario: Se impide solicitar sin ver el arqueo del fondo vigente
    Cuando el Jefe de Transporte abre la solicitud de fondo del período "2026-09"
    Entonces el sistema presenta el arqueo de "FND-2026-08-003" con aprobado, ampliaciones, asignado, consumido, comprobado, devuelto y saldo
    Y presenta el detalle por persona: "Wilmer Cáceres L 5,200.00" y "Denis Fúnez L 3,400.00"
    Y muestra "El fondo FND-2026-08-003 tiene L 8,600.00 sin liquidar a cargo de 2 servidores."

  Escenario: Se advierte que el saldo contable no es el saldo disponible
    Dado 6 misiones en estado "APROBADA" o "PROGRAMADA" sin asignación emitida, con estimado de combustible y peajes de "L 20,000.00"
    Cuando el Jefe de Transporte abre la solicitud de fondo del período "2026-09"
    Entonces el sistema muestra un saldo proyectado de "L -7,600.00"
    Y muestra "Saldo contable L 12,400.00. Comprometido por misiones sin asignación emitida L 20,000.00. Saldo proyectado L -7,600.00."

  Escenario: Se rechaza la solicitud sin composición del instrumento
    Cuando el Jefe de Transporte registra una solicitud de "L 200,000.00" sin indicar si es efectivo, órdenes de pago o ambos
    Entonces el sistema rechaza el registro
    Y muestra "Indique la composición del instrumento: efectivo, órdenes de pago o ambos. Cada uno se concilia distinto."

  Escenario: Se rechaza la solicitud sin justificación operativa del período
    Cuando el Jefe de Transporte registra una solicitud de "L 200,000.00" sin justificación operativa
    Entonces el sistema rechaza el registro
    Y muestra "La justificación operativa del período es obligatoria: es lo que sustenta el monto ante Gerencia Administrativa."

  Escenario: Se registra la solicitud y queda encaminada
    Cuando el Jefe de Transporte registra una solicitud de "L 200,000.00" en efectivo, con justificación operativa y la partida "[C] tomada del espejo de ARGOS"
    Entonces la solicitud queda con folio "FND-2026-09-004" en estado "SOLICITADO"
    Y se encamina a Gerencia Administrativa
    Y se registra persona, puesto, rol, marca de tiempo, origen y huella del contenido

  Escenario: Un fondo anterior sin cerrar no impide solicitar el siguiente
    Dado que "FND-2026-08-003" no está cerrado
    Cuando el Jefe de Transporte registra la solicitud del período "2026-09"
    Entonces el sistema acepta la solicitud
    Y adjunta el arqueo de "FND-2026-08-003" a la solicitud, con los saldos por persona
```

## Fuera de alcance

- La aprobación del fondo y la verificación de cuota trimestral — es [HU-072](HU-072-aprobar-fondo-verificando-cuota-trimestral.md)
- La entrega física del efectivo o de las órdenes de pago — es [HU-074](HU-074-entregar-el-fondo-y-registrar-su-custodia.md)
- La compra de combustible, los contratos con proveedores y las tarjetas de flota: **fuera del producto** ([DP-001 D-03](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md))
- La estructura presupuestaria: la define ARGOS y aquí solo se lee ([DP-001 D-09](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md))

## Notas y pendientes

- `[C]` **Periodicidad del fondo** — ¿mensual, trimestral o por misión? Parámetro `periodicidad_fondo`. Insumo **#7 / `PROP-01`**
- `[C]` **¿Un motorista puede arrastrar saldo entre misiones o liquida cada una?** Insumo **#7 / `PROP-01`**
- `[C]` **¿La orden de pago tiene folio preimpreso o lo genera el sistema?** Insumo **#7 / `PROP-01`**
- `[C]` Correspondencia entre delegaciones y unidades ejecutoras, para el fondo de ámbito delegación — insumo **#27**
- `[P]` El control de fondos entregados a servidores públicos proviene de [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md); la norma existe, no se pudo extraer el articulado. **No se eleva el nivel.**
