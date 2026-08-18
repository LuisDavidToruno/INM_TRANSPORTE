# HU-012 — Registrar la autorización de varios niveles sin inventar estados

| Campo | Valor |
|---|---|
| **Módulo** | M-06 Solicitudes de Transporte |
| **Actor** | ACT-03 Jefatura Inmediata y ACT-08 Gerencia Administrativa |
| **Prioridad** | Media |
| **Sprint** | sin asignar |
| **Estado** | Borrador — bloqueada por insumo #16 (esquema de niveles de ARGOS) |

## Historia

**Como** Jefatura Inmediata de un nivel intermedio de la cadena
**quiero** registrar mi autorización sobre un expediente que requiere más de una firma, y ver cuáles niveles faltan
**para** que el expediente no se dé por aprobado antes de tiempo ni quede detenido porque nadie sabe a quién le toca firmar

## Contexto

Una misión puede requerir **una o varias** autorizaciones según monto, destino, duración o tipo de recurso movilizado. La matriz de niveles es **propiedad de ARGOS**, no de SIGTI ([DP-001](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) D-05): aquí se consume del espejo, no se reimplementa.

La decisión de modelado que hay que sostener: **no se inventan estados intermedios por nivel**. Un `APROBADA_NIVEL_1` sería una máquina de estados que cambia cada vez que la institución reordena su matriz de firmas. Cada autorización parcial es un **asiento en el diario del expediente**, y el estado cambia una sola vez, al registrarse la última firma requerida.

Y `BD-01` se evalúa **en cada nivel**, no solo en el primero.

## Reglas que la gobiernan

- [RN-02](../../01-negocio/reglas/RN-02-escalamiento-de-autorizacion.md) — La cadena y los niveles se resuelven contra el espejo de ARGOS
- [RN-01](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md) — La segregación se evalúa en cada nivel
- [RN-03](../../01-negocio/reglas/RN-03-registro-inmutable-de-autorizacion.md) — Cada autorización parcial se registra con identidad, rol ejercido, momento, origen y huella
- [RN-06](../../01-negocio/reglas/RN-06-transiciones-de-estado-de-la-orden.md) — El expediente permanece en `SOLICITADA` hasta la última firma; no hay estados por nivel
- [RN-48](../../01-negocio/reglas/RN-48-datos-espejo-de-solo-lectura.md) — La matriz de niveles es dato espejo de solo lectura
- [RN-54](../../01-negocio/reglas/RN-54-cuota-trimestral-de-compromiso.md) — El compromiso de gasto se valida contra la cuota trimestral, no solo contra el presupuesto anual

## Casos especiales que la afectan

- Ninguno de los 28 `CE-xx` toca este flujo. Constancia dejada

## Criterios de aceptación

```gherkin
# language: es
Característica: Autorización de varios niveles sobre el mismo expediente
  Como autorizador de un nivel de la cadena
  quiero registrar mi firma y ver qué niveles faltan
  para que el expediente avance sin darse por aprobado antes de tiempo

  Antecedentes:
    Dado un expediente "CHO-2026-00087" en estado "SOLICITADA"
    Y una matriz de niveles espejada de ARGOS que exige 2 autorizaciones para misiones fuera del departamento sede
    Y un destino "San Pedro Sula", fuera del departamento sede
    Y un nivel 1 a cargo de "Rolando Discua", Subgerente de Operaciones
    Y un nivel 2 a cargo de "Elsa Maradiaga", Gerente Administrativa

  Escenario: Se bloquea el nivel 2 cuando su titular es el solicitante de derecho
    Dado un expediente cuyo solicitante de derecho es "Elsa Maradiaga"
    Y una autorización de nivel 1 ya registrada por "Rolando Discua"
    Cuando "Elsa Maradiaga" intenta registrar la autorización de nivel 2
    Entonces el sistema no ejecuta la autorización
    Y muestra "Usted figura como solicitante de derecho de este expediente. Quien solicita no autoriza, tampoco en un nivel superior (RN-01). El nivel 2 se escaló."

  Escenario: Se rechaza registrar el nivel 2 antes que el nivel 1
    Dado un expediente sin autorizaciones registradas
    Cuando "Elsa Maradiaga" intenta registrar la autorización de nivel 2
    Entonces el sistema no ejecuta la autorización
    Y muestra "Falta la autorización de nivel 1 a cargo de la Subgerencia de Operaciones. Los niveles se registran en orden."

  Escenario: La autorización parcial no cambia el estado del expediente
    Cuando "Rolando Discua" registra la autorización de nivel 1
    Entonces el sistema registra el asiento "Autorización de nivel 1 — Rolando Discua, Subgerente de Operaciones"
    Y el expediente permanece en estado "SOLICITADA"
    Y muestra "Falta 1 autorización: nivel 2, Gerencia Administrativa."

  Escenario: La última autorización requerida es la que aprueba
    Dada una autorización de nivel 1 ya registrada por "Rolando Discua"
    Cuando "Elsa Maradiaga" registra la autorización de nivel 2
    Entonces el expediente pasa a estado "APROBADA"
    Y el expediente conserva los 2 asientos de autorización con su actor, rol ejercido y momento

  Escenario: Devolver a borrador invalida todas las autorizaciones parciales
    Dada una autorización de nivel 1 ya registrada por "Rolando Discua"
    Cuando "Elsa Maradiaga" devuelve el expediente a borrador con motivo "Destino mal consignado"
    Entonces el sistema invalida la autorización de nivel 1
    Y advierte antes de confirmar "Devolver invalidará 1 autorización de nivel ya registrada. Al reenviar, la cadena vuelve a empezar."
    Y conserva el asiento invalidado en el diario, sin borrarlo

  Escenario: El expediente muestra en todo momento qué niveles faltan
    Dada una autorización de nivel 1 ya registrada
    Cuando el Solicitante consulta el estado de su expediente
    Entonces el sistema muestra "Autorizado nivel 1 de 2. Pendiente: Gerencia Administrativa."
```

## Fuera de alcance

- La **definición** de la matriz de niveles y sus disparadores por monto, destino, duración o tipo de recurso: es propiedad de ARGOS. SIGTI la consume y no la edita ([`RN-48`](../../01-negocio/reglas/RN-48-datos-espejo-de-solo-lectura.md))
- La validación presupuestaria contra la cuota trimestral: se enuncia aquí como regla concurrente, pero su implementación es de M-20 con ARGOS
- El nivel adicional que exige una solicitud marcada **urgente** por antelación mínima incumplida: depende del insumo #32

## Notas y pendientes

- `[C]` **Esquema exacto de niveles y sus disparadores** — insumo #16. **Es el dato que bloquea esta historia**: aquí el `[C]` *es* la lógica. Por la [DoR](../../plantillas/definition-of-ready.md) no entra a sprint sin él, y **no se cablea ningún umbral**
- `[C]` Contrato de API y webhooks de ARGOS — insumo #16
- `[P]` La exigencia de autorización por servidor competente proviene de [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md), verificada parcialmente
- Trazabilidad: [CU-02](../casos-de-uso/CU-02-autorizar-solicitud-de-transporte.md) flujo alterno A1; nota de hallazgo `HCU-02` sobre la diferencia entre *puesto superior* y *nivel inmediato superior de la cadena*
