# RNF-07 — El espejo de ARGOS y Talento Humano nunca diverge en silencio

| Campo | Valor |
|---|---|
| **Categoría** | Disponibilidad / Auditoría |
| **Prioridad** | Crítico |
| **Origen** | [`ADR-001`](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md) — las mitigaciones declaradas obligatorias de ese ADR se convierten aquí en umbrales medibles |
| **Afecta arquitectura** | **Sí — determinante para la selección de stack (Sprint 2).** Cola persistente, reintento y reconciliación programada son capacidades, no detalles |

## Enunciado

SIGTI opera contra una **copia local de solo lectura** de los datos que poseen ARGOS y Talento Humano. El `ADR-001` nombra el riesgo real de ese patrón con todas sus letras: **la divergencia silenciosa**. Un motorista que Talento Humano dio de baja pero que SIGTI sigue asignando a misiones no es un problema técnico, es un problema legal.

Por tanto el sistema **debe** garantizar tres cosas medibles, y ninguna es opcional:

1. **Que la divergencia se detecte** — reconciliación completa programada, no solo webhooks.
2. **Que el usuario sepa cuán viejo es el dato que está viendo** — marca de última sincronización visible, no escondida en una pantalla de administración.
3. **Que el sistema se degrade de forma explícita** — si el espejo lleva demasiado tiempo sin confirmarse, se advierte y eventualmente se bloquean las operaciones sensibles, en lugar de fingir que todo está al día.

## Métrica y umbral

| Métrica | Umbral |
|---|---|
| Latencia `p95` entre el evento en el sistema origen y su reflejo en el espejo | < 5 min `[C]` — depende de la garantía de entrega de los webhooks, insumo #16 y #17 |
| Entidades espejeadas cubiertas por la reconciliación completa | **100 %.** Ninguna entidad depende únicamente del webhook |
| Periodicidad de la reconciliación completa | Diaria, en ventana de baja actividad `[C]` insumo #72 |
| Duración de una reconciliación completa a volumen `JDR-1` | < 30 min, sin bloquear la operación |
| Eventos perdidos que la reconciliación no recupera | **0** |
| Reintentos de un evento fallido antes de escalar | Con espera creciente durante ≥ 24 h; luego queda en la bandeja de fallidos, **visible**, nunca descartado |
| Eventos descartados sin registro | **0** |
| Pantallas que usan dato espejo sin mostrar su marca de última sincronización | **0** |
| Antigüedad del espejo que dispara **advertencia** visible | > 24 h sin confirmación `[C]` |
| Antigüedad del espejo que **bloquea** operaciones sensibles (asignar motorista, aprobar contra estructura de autorización) | > 72 h sin confirmación `[C]` — ambos umbrales son parámetros con vigencia, no constantes ([`RNF-05`](RNF-05-temporalidad-normativa.md)) |
| Divergencias corregidas por la reconciliación sin dejar asiento | **0.** Toda corrección se registra: qué entidad, qué campo, valor local, valor del origen, de qué corrida vino |
| Escrituras de SIGTI sobre datos espejo | **0** ([`RN-48`](../../01-negocio/reglas/RN-48-datos-espejo-de-solo-lectura.md)) |
| Operaciones de SIGTI que hagan una llamada en línea al origen en la ruta crítica del usuario | **0** |
| Disponibilidad de SIGTI con ARGOS y Talento Humano caídos | 100 % de las funciones propias |

## Cómo se verifica

1. **Prueba del webhook perdido** — la prueba que justifica todo el requisito:
   - Se detiene el receptor de eventos de SIGTI.
   - En el origen se dan de baja dos empleados y se cambia un nivel de autorización.
   - Se reactiva el receptor. Los eventos ya se perdieron: no hay reintento del lado del origen.
   - Se corre la reconciliación. **Debe detectar las tres divergencias, corregirlas y dejar asiento de cada una.**
2. **Prueba del motorista dado de baja**: se da de baja un motorista en Talento Humano y se intenta asignarlo a una misión antes de que llegue el evento. Se verifica el comportamiento declarado por [`RN-12`](../../01-negocio/reglas/RN-12-disponibilidad-del-motorista.md) y la advertencia de antigüedad del espejo.
3. **Prueba de degradación**: se detiene la sincronización 25 h y se verifica la advertencia; se detiene 73 h y se verifica el bloqueo de operaciones sensibles, con mensaje que diga **qué** está desactualizado, **desde cuándo** y **qué hacer**.
4. **Prueba de origen caído**: se apagan ARGOS y Talento Humano durante una jornada completa. Se ejecuta el guion de un día hábil de SIGTI. Ninguna función propia debe fallar.
5. **Prueba de solo lectura**: se intenta modificar un dato espejo desde cada pantalla que lo muestra y desde la interfaz de administración. Todas deben rechazarlo.
6. **Prueba de la bandeja de fallidos**: se fuerza el rechazo permanente de un evento (esquema inesperado) y se verifica que queda visible, con su contenido, y que un administrador puede reprocesarlo.
7. **Prueba de la marca visible**: recorrido de todas las pantallas que consumen dato espejo verificando que muestran la fecha de última confirmación de ese dato, no la del proceso global.

## Consecuencia de no cumplirlo

Se asigna a una misión un motorista que está de vacaciones, incapacitado o dado de baja. Si ese motorista tiene un accidente, la institución no tiene defensa: el sistema tenía la información disponible, no la usó, y quien autorizó la asignación responde. [NRM-06](../../01-negocio/normativa/NRM-06-transito-y-licencias.md) traslada esa responsabilidad de forma directa.

El fallo por divergencia es peor que el fallo por caída, y esa es la razón de los umbrales de degradación explícita: un sistema caído obliga a resolver por otra vía; un sistema que muestra datos viejos con cara de datos frescos hace que se decida mal con confianza.

## Deuda declarada

El insumo #41 pregunta si se habilita el **modo delegación desconectada** —autorizar y despachar sin red—. Si el PO lo aprueba, se estaría autorizando contra un espejo potencialmente viejo, que es exactamente lo que este `RNF` bloquea a partir de 72 h. Las mitigaciones diseñadas (horizonte de validez, marca impresa, revalidación con hallazgo automático) reducen el riesgo pero no lo eliminan.

**Señal para pagar la deuda:** el primer caso en que una autorización emitida en modo desconectado resulte inválida al revalidarse. Si eso ocurre más de una vez por trimestre, el modo se retira.

## Trazabilidad

- Módulos: M-20 (integraciones), M-05, M-07
- Reglas: [`RN-48`](../../01-negocio/reglas/RN-48-datos-espejo-de-solo-lectura.md), [`RN-49`](../../01-negocio/reglas/RN-49-reconciliacion-periodica-del-espejo.md), [`RN-50`](../../01-negocio/reglas/RN-50-degradacion-por-sincronizacion-detenida.md), [`RN-12`](../../01-negocio/reglas/RN-12-disponibilidad-del-motorista.md)
- Decisiones: [`ADR-001`](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md), [DP-001](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md)
- Casos especiales: [`CE-13`](../casos-especiales/CE-13-motorista-no-disponible-por-talento-humano.md), [`CE-11`](../casos-especiales/CE-11-licencia-vence-durante-la-mision.md)
- Requisitos relacionados: [`RNF-03`](RNF-03-operacion-sin-conectividad.md), [`RNF-20`](RNF-20-observabilidad-y-diagnostico.md)
- Insumos: #16, #17, #41, #72
