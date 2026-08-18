# HU-010 — Impedir que quien solicita autorice, y escalar al nivel superior

| Campo | Valor |
|---|---|
| **Módulo** | M-06 Solicitudes de Transporte |
| **Actor** | ACT-03 Jefatura Inmediata |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Refinada |

## Historia

**Como** institución sujeta al control interno del Estado
**quiero** que el sistema impida ejecutar la autorización cuando quien autoriza es el **solicitante de derecho, el capturador o el remitente** del expediente, y escale automáticamente al nivel inmediato superior dejando el motivo asentado
**para** que la segregación de funciones no dependa de que alguien se acuerde de respetarla, y para que el escalamiento sea legible por quien reciba la orden en carretera

## Contexto

Es la validación de mayor valor de control interno del sistema. Quien solicita ≠ quien autoriza es **bloqueo duro por mandato del control interno del Estado** `[P]` [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md): no hay confirmación con advertencia, no hay "autorizar de todos modos", no hay excepción configurable.

Y hay una trampa concreta: `BD-01`, tal como está escrita, compara al autorizador contra **quien creó** y **quien envió**. En el escenario más frecuente de la operación real —la asistente captura para su jefe y el jefe autoriza— **no bloquearía**, aunque la incompatibilidad `I-01` sí se estaría violando. Es el hallazgo [`HB3-01`](../../05-calidad/hallazgos/H-B3-001-hallazgos-de-casos-de-uso.md), y esta historia implementa la resolución adoptada por los casos de uso: **comparar contra las tres identidades**.

La comparación es **por identidad de persona, no por identificador de usuario**: un mismo servidor con dos cuentas sigue siendo la misma persona.

**El régimen de excepción para delegaciones con personal insuficiente no existe.** Quedó suspendido por [DP-002](../../07-gestion/decisiones-de-producto/DP-002-segregacion-en-delegaciones-pequenas.md). La vía es el escalamiento a sede.

## Reglas que la gobiernan

- [RN-01](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md) — Un mismo servidor no ejerce dos funciones de control sobre la misma Orden de Misión. **Bloqueo duro, no desactivable**
- [RN-02](../../01-negocio/reglas/RN-02-escalamiento-de-autorizacion.md) — Cuando el autorizador natural es el solicitante, la autorización escala al nivel inmediato superior
- [RN-03](../../01-negocio/reglas/RN-03-registro-inmutable-de-autorizacion.md) — El intento bloqueado se registra con el par de incompatibilidad detectado
- [RN-07](../../01-negocio/reglas/RN-07-delegacion-de-autorizacion.md) — La delegación de firma **no rompe la segregación**
- [RN-06](../../01-negocio/reglas/RN-06-transiciones-de-estado-de-la-orden.md) — El escalamiento no es un estado nuevo: es un asiento sobre el mismo expediente

## Casos especiales que la afectan

- Ninguno de los 28 `CE-xx` cubre este flujo. La constancia queda dejada: lo que lo motiva es el hallazgo [`HB3-01`](../../05-calidad/hallazgos/H-B3-001-hallazgos-de-casos-de-uso.md) y la decisión [DP-002](../../07-gestion/decisiones-de-producto/DP-002-segregacion-en-delegaciones-pequenas.md)

## Criterios de aceptación

```gherkin
# language: es
Característica: Segregación entre solicitante y autorizador
  Como institución sujeta al control interno del Estado
  quiero que quien solicita no pueda autorizar
  para que la autorización sea un control real y no una formalidad

  Antecedentes:
    Dada una servidora "Karla Zelaya", asistente de la "Subgerencia de Operaciones"
    Y un servidor "Rolando Discua", Subgerente de Operaciones
    Y un servidor "Elsa Maradiaga", Gerente Administrativa, nivel inmediato superior de "Rolando Discua"
    Y un expediente "CHO-2026-00087" en estado "SOLICITADA"

  Escenario: Se bloquea cuando el autorizador es el solicitante de derecho, aunque no haya capturado
    Dado un expediente capturado por "Karla Zelaya" por encargo de "Rolando Discua"
    Y un solicitante de derecho registrado "Rolando Discua"
    Cuando "Rolando Discua" intenta autorizar el expediente "CHO-2026-00087"
    Entonces el sistema no ejecuta la autorización
    Y muestra "Usted figura como solicitante de derecho de este expediente. Quien solicita no autoriza (RN-01). El expediente se escaló a Gerencia Administrativa."
    Y registra el intento con el par de incompatibilidad "I-01", la identidad, el momento y el expediente

  Escenario: Se bloquea cuando el autorizador es quien capturó el expediente
    Dado un expediente capturado por "Karla Zelaya" por encargo de "Marvin Cálix"
    Y una servidora "Karla Zelaya" con rol de autorizador vigente sobre esa dependencia
    Cuando "Karla Zelaya" intenta autorizar el expediente "CHO-2026-00087"
    Entonces el sistema no ejecuta la autorización
    Y muestra "Usted capturó este expediente. Quien participa en la solicitud no autoriza (RN-01)."

  Escenario: Se bloquea cuando el autorizador es quien remitió el expediente
    Dado un expediente cuyo solicitante de derecho es "Marvin Cálix" y cuyo remitente es "Rolando Discua"
    Cuando "Rolando Discua" intenta autorizar el expediente "CHO-2026-00087"
    Entonces el sistema no ejecuta la autorización
    Y muestra "Usted envió este expediente a autorización. Quien remite no autoriza (RN-01)."

  Escenario: Se bloquea aunque el autorizador use una segunda cuenta
    Dado un solicitante de derecho "Rolando Discua" con identidad "0801-1985-04512"
    Y un usuario "rdiscua.admin" asociado a la misma identidad "0801-1985-04512"
    Cuando el usuario "rdiscua.admin" intenta autorizar el expediente "CHO-2026-00087"
    Entonces el sistema no ejecuta la autorización
    Y muestra "La cuenta rdiscua.admin corresponde a la misma persona que figura como solicitante de derecho. La comparación es por identidad, no por cuenta."

  Escenario: Se bloquea al delegado que es el solicitante de derecho
    Dado un acto de delegación de autorización vigente de "Elsa Maradiaga" a favor de "Rolando Discua"
    Y un expediente cuyo solicitante de derecho es "Rolando Discua"
    Cuando "Rolando Discua" intenta autorizar por delegación
    Entonces el sistema no ejecuta la autorización
    Y muestra "La delegación de firma no levanta la segregación. Usted es el solicitante de derecho de este expediente (RN-07)."

  Escenario: El escalamiento queda asentado y es legible en el documento impreso
    Dado un expediente escalado por coincidencia entre solicitante y autorizador
    Cuando "Elsa Maradiaga" autoriza el expediente "CHO-2026-00087"
    Entonces el expediente registra el asiento "Escalado a Gerencia Administrativa por coincidencia entre solicitante y autorizador (RN-02)"
    Y ese asiento aparece en la versión impresa de la Orden de Misión

  Escenario: No existe régimen de excepción por delegación con personal insuficiente
    Dada una delegación "Puerto Lempira" con un único servidor con rol de autorizador, que es el solicitante de derecho
    Cuando ese servidor intenta autorizar invocando falta de personal en la delegación
    Entonces el sistema no ejecuta la autorización
    Y muestra "No existe régimen de excepción por personal insuficiente (DP-002). Solicite la autorización a la sede; si no hay conectividad, use el código de autorización fuera de línea."

  Escenario: La cadena agotada bloquea y muestra la ruta evaluada
    Dada una dependencia cuyo único nivel superior es el propio solicitante de derecho
    Cuando el sistema intenta escalar el expediente "CHO-2026-00087"
    Entonces el sistema no encamina el expediente a ninguna bandeja
    Y muestra "No hay autorizador válido. Ruta evaluada: Subgerencia de Operaciones → Gerencia Administrativa (mismo servidor solicitante) → sin nivel superior. Corrija la configuración, no el expediente."
```

## Fuera de alcance

- La captura del **solicitante de derecho** como dato: es el habilitador [HU-003](HU-003-captura-por-encargo-y-solicitante-de-derecho.md), que debe estar terminado antes
- Las demás incompatibilidades del núcleo irreductible (`I-07`, `I-10`, `I-11`): se evalúan en despacho, combustible y liquidación, en sus propias historias
- La detección automática de **parentesco**: SIGTI no conoce los vínculos familiares. Se resuelve con declaración de excusa del autorizador, que dispara el mismo escalamiento
- **Quién autoriza la misión de la máxima autoridad**: hasta que la institución lo defina, el sistema trata el expediente como cualquier otro y escala, exigiendo fundamento documental y marcándolo para Auditoría Interna

## Notas y pendientes

- `[C]` **Quién autoriza la misión de la máxima autoridad**, y el autorizador alterno por dependencia y por delegación — insumo #28. Es hueco real del modelo, no un detalle
- `[C]` Si la institución tiene **régimen formal de excusa** por conflicto de interés — insumo #30
- `[P]` La exigencia de segregación de funciones incompatibles proviene de [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md), verificada parcialmente. Esta historia **no** eleva ese nivel a `[V]`
- **Hallazgo abierto:** `BD-01` debe corregirse para comparar contra las tres identidades. Autoridad: [máquina de estados](../../03-arquitectura/estados/orden-de-mision.md). Registrado como [`HB3-01`](../../05-calidad/hallazgos/H-B3-001-hallazgos-de-casos-de-uso.md)
- Trazabilidad: [CU-02](../casos-de-uso/CU-02-autorizar-solicitud-de-transporte.md) paso 6 y excepciones E1, E2, E3; `BD-01`, `PC-01`, `I-01`, `INV-10`
