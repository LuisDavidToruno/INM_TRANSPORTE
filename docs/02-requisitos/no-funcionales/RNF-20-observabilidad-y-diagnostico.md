# RNF-20 — Una sola pantalla le dice a alguien sin especialización qué está mal y qué hacer

| Campo | Valor |
|---|---|
| **Categoría** | Operabilidad |
| **Prioridad** | Alto |
| **Origen** | [NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md): no habrá equipo de TI dedicado; mitigación 4 del [`ADR-001`](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md) (bitácora de sincronización) |
| **Afecta arquitectura** | **Sí** — la observabilidad orientada al operador no técnico no se obtiene añadiendo registros técnicos al final |

## Enunciado

El sistema **debe** tener una **pantalla de estado** que una persona sin formación técnica pueda leer y entender, y que responda una sola pregunta: *¿está todo bien, y si no, qué hago?*

No es un tablero de métricas. No muestra uso de memoria ni gráficas de latencia. Muestra las cosas que se rompen en este sistema, en el lenguaje de quien las tiene que resolver:

- ¿Está el espejo de ARGOS y Talento Humano al día? ([`RNF-07`](RNF-07-sincronizacion-del-espejo-local.md))
- ¿Se verificó el último respaldo? ¿Cuándo fue el último simulacro de restauración? ([`RNF-09`](RNF-09-instalacion-respaldo-y-restauracion.md))
- ¿Verificó la cadena de auditoría anoche? ([`RNF-04`](RNF-04-bitacora-append-only-con-hash-encadenado.md))
- ¿Qué dispositivos de campo llevan días sin sincronizar, y de quién son?
- ¿Cuánto espacio queda y para cuántos días alcanza? ([`RNF-02`](RNF-02-volumen-y-crecimiento-del-acervo.md))
- ¿Hay eventos en la bandeja de fallidos? ¿Hay conflictos esperando resolución humana?
- ¿Qué parámetros normativos están vencidos o vacíos? ([`RNF-19`](RNF-19-configurabilidad-multi-institucion.md))

## Métrica y umbral

| Métrica | Umbral |
|---|---|
| Indicadores de la pantalla de estado que exijan conocimiento técnico para interpretarse | **0** |
| Indicadores en estado anómalo sin una acción concreta sugerida | **0.** Cada uno dice qué hacer o a quién avisar |
| Tiempo para que una persona no especializada identifique la causa de una falla usando solo la pantalla | ≤ 5 min |
| Cobertura de la pantalla sobre los modos de falla conocidos del sistema | 100 % de la lista documentada de modos de falla |
| Correlación entre un error visto por el usuario y el registro técnico | Todo mensaje de error lleva un código; con ese código se encuentra el detalle técnico completo ([`RNF-16`](RNF-16-idioma-accesibilidad-y-mensajes.md)) |
| Retención de registros técnicos | ≥ 90 días `[C]` — suficientes para investigar un hallazgo que se detecta al cierre del trimestre |
| Datos personales en registros técnicos | **0** ([`RNF-13`](RNF-13-cifrado-en-transito-y-en-reposo.md), [`RNF-17`](RNF-17-retencion-y-depuracion-diferenciada.md)) |
| Aviso proactivo de condiciones críticas | Por el canal que la institución ya usa `[C]` insumo #73. Condiciones críticas: respaldo fallido, cadena de auditoría rota, espejo detenido más allá del umbral, disco por agotarse, dispositivo sin sincronizar más de 10 días `[C]` |
| Latencia entre la ocurrencia de una condición crítica y su aparición en la pantalla | ≤ 15 min |
| Falsos avisos críticos | Cada aviso que resulte falso se investiga y se ajusta. Un tablero que avisa de más se ignora, y entonces no avisa de nada |
| Paquete de diagnóstico exportable para enviar a quien dé soporte | Una operación, sin datos personales, con los registros del período y el estado del sistema |
| Acceso del administrador (`ACT-01`) a contenido de negocio para diagnosticar | Registrado siempre como acceso excepcional ([`RNF-14`](RNF-14-control-de-acceso-por-puesto-y-registro-de-consultas.md)) |

## Cómo se verifica

1. **Prueba de las cinco fallas** — la verificación central. Se provocan, una por una y sin avisar cuál:
   1. Disco al 95 % de ocupación.
   2. Sincronización del espejo detenida 30 h.
   3. Respaldo de anoche fallido.
   4. Un dispositivo de campo 12 días sin sincronizar.
   5. Un webhook rechazado por esquema inesperado.

   Una persona sin especialización, usando **solo** la pantalla de estado, debe nombrar el problema y decir qué va a hacer. Se cronometra cada caso. **Cada falla que no pueda diagnosticar es un defecto de la pantalla.**
2. **Prueba de la cadena rota**: se altera un asiento de auditoría por fuera del sistema. La verificación nocturna debe fallar y la pantalla debe mostrarlo al día siguiente, con el asiento afectado y el último sello íntegro.
3. **Prueba de correlación**: un usuario reporta un código de error; con ese código se localiza el registro técnico completo en menos de 2 min.
4. **Prueba de fuga en registros**: se opera una jornada con datos personales de prueba y se buscan esos nombres en los registros técnicos. Resultado esperado: cero.
5. **Prueba del paquete de diagnóstico**: se genera y se revisa manualmente que no contiene datos personales antes de darlo por válido.
6. **Prueba del aviso proactivo**: se provocan las condiciones críticas fuera del horario de oficina y se verifica que el aviso llega por el canal acordado.

## Consecuencia de no cumplirlo

El sistema falla en silencio. El respaldo lleva tres semanas sin ejecutarse, el espejo lleva cinco días detenido y se están asignando motoristas dados de baja, y nadie lo sabe porque no hay quien lea registros técnicos en la delegación.

El daño no aparece cuando la falla ocurre: aparece cuando alguien la necesita resuelta —el día que hay que restaurar, o el día que el auditor pregunta por una asignación indebida—. Para entonces lleva semanas acumulándose, y las consecuencias de [`RNF-07`](RNF-07-sincronizacion-del-espejo-local.md) y [`RNF-09`](RNF-09-instalacion-respaldo-y-restauracion.md) se materializan completas.

Un sistema que no se puede diagnosticar en una delegación sin personal técnico es, en la práctica, un sistema que solo funciona mientras nada falle.

## Trazabilidad

- Módulos: M-01, M-16, M-20
- Reglas: [`RN-49`](../../01-negocio/reglas/RN-49-reconciliacion-periodica-del-espejo.md), [`RN-50`](../../01-negocio/reglas/RN-50-degradacion-por-sincronizacion-detenida.md), [`RN-45`](../../01-negocio/reglas/RN-45-cero-sobrescritura-silenciosa.md)
- Decisiones: [`ADR-001`](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md), mitigaciones 3, 4 y 5
- Normativa: [NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md)
- Requisitos relacionados: [`RNF-04`](RNF-04-bitacora-append-only-con-hash-encadenado.md), [`RNF-07`](RNF-07-sincronizacion-del-espejo-local.md), [`RNF-09`](RNF-09-instalacion-respaldo-y-restauracion.md), [`RNF-16`](RNF-16-idioma-accesibilidad-y-mensajes.md)
- Insumos: #73 (quién opera el servidor y por qué canal se le avisa)
