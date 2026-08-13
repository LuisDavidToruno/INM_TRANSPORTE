# RNF-10 — La caída del servidor institucional no detiene la operación de campo

| Campo | Valor |
|---|---|
| **Categoría** | Disponibilidad |
| **Prioridad** | Alto |
| **Origen** | Despliegue on-premise sin equipo de TI ([NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md)) combinado con la exigencia de registro oportuno de [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md) |
| **Afecta arquitectura** | **Sí** — obliga a que el cliente de campo sea autónomo y no un cliente delgado del servidor |

## Enunciado

El sistema **debe** estar disponible durante el horario hábil de la institución, y **debe** degradarse de forma que una caída del servidor central **no impida capturar en campo**. La captura desconectada del [`RNF-03`](RNF-03-operacion-sin-conectividad.md) no distingue entre "no hay red" y "el servidor está caído": ambos son el mismo estado para el motorista.

**Lo que este requisito no promete, deliberadamente:** alta disponibilidad con redundancia, conmutación automática ni cifras de cuatro nueves. El servidor está en la institución, depende de su energía eléctrica y de su enlace, y no hay quien lo atienda de madrugada. Prometer 99.9 % sería comprometer a alguien que no existe. Se promete lo que se puede sostener y se declara con claridad lo que no.

## Métrica y umbral

| Métrica | Umbral |
|---|---|
| Disponibilidad del servidor durante el horario hábil declarado | ≥ 99 % mensual `[C]` insumo #32 (horario hábil) y #72 — equivale a ≈ 1.7 h de interrupción al mes sobre 176 h hábiles |
| Disponibilidad comprometida fuera de horario hábil | **Ninguna.** Se declara explícitamente que no hay compromiso, y la ventana de mantenimiento vive ahí |
| Funciones de captura de campo que dejan de operar con el servidor caído | **0** |
| Funciones que sí dejan de operar con el servidor caído | Autorización de nuevas solicitudes, consulta de datos no precargados, emisión de documentos con folio fuera del rango ya asignado a la delegación. **La lista es cerrada, está escrita y el usuario la ve en el mensaje de degradación** |
| Objetivo de tiempo de recuperación (RTO) tras falla del servidor | ≤ 4 h en horario hábil ([`RNF-09`](RNF-09-instalacion-respaldo-y-restauracion.md)) |
| Objetivo de punto de recuperación (RPO) del servidor | ≤ 24 h con respaldo diario; **≤ 1 h** si se habilita respaldo incremental `[C]` insumo #72 |
| Pérdida de datos capturados en campo ante caída total del servidor | **0.** Viven en el dispositivo hasta confirmarse la sincronización |
| Confirmación de sincronización antes de liberar espacio en el dispositivo | Obligatoria. El dispositivo **no borra** un registro local hasta que el servidor confirma su recepción y su asiento |
| Aviso al usuario cuando la aplicación no puede alcanzar el servidor | ≤ 10 s, con mensaje que distinga *"sin conexión"* de *"servidor no responde"* — son problemas distintos y quien los resuelve es distinto |
| Reintento automático de reconexión | Con espera creciente, indefinido, sin intervención del usuario |
| Corte de energía durante una escritura que deje la base inconsistente | **0.** Se verifica con prueba de corte abrupto |

## Cómo se verifica

1. **Prueba de caída en jornada**: se apaga el servidor a media mañana. Se ejecuta el guion completo de campo en tres dispositivos durante 4 h. Se enciende el servidor y se verifica sincronización íntegra, sin duplicados y sin pérdida.
2. **Prueba de corte abrupto**: se corta la energía del servidor durante una escritura de alto volumen (cierre de misión con adjuntos). Al reiniciar, la base debe quedar consistente y la cadena de auditoría del [`RNF-04`](RNF-04-bitacora-append-only-con-hash-encadenado.md) debe verificar. Se repite 10 veces.
3. **Prueba de mensajes distinguibles**: se simulan por separado (a) dispositivo sin red, (b) servidor apagado, (c) servidor lento. Los tres mensajes deben ser distintos y accionables ([`RNF-16`](RNF-16-idioma-accesibilidad-y-mensajes.md)).
4. **Prueba de no-borrado prematuro**: se fuerza una sincronización que el servidor acepta parcialmente. Se verifica que el dispositivo conserva íntegro lo no confirmado.
5. **Registro de disponibilidad**: la pantalla de estado del [`RNF-20`](RNF-20-observabilidad-y-diagnostico.md) acumula el tiempo de indisponibilidad del mes y lo muestra. La métrica se mide con el propio sistema, no con una hoja aparte.

## Consecuencia de no cumplirlo

Si el cliente de campo depende del servidor, cada caída del servidor —o cada corte de energía en la sede, que en Honduras no es un evento raro— deja sin registrar la operación de todas las delegaciones simultáneamente. Se acumulan días de captura pendiente que después se digitan desde memoria, y la digitación diferida masiva es exactamente el patrón que produce el hallazgo por registro no oportuno de TSC-NOGECI V-10.

Si en cambio se promete una disponibilidad que la institución no puede sostener, el incumplimiento se vuelve el argumento con el que se cuestiona todo el proyecto la primera vez que el servidor se cae un lunes.

## Trazabilidad

- Módulos: M-16, transversal
- Reglas: [`RN-43`](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md), [`RN-44`](../../01-negocio/reglas/RN-44-identificadores-y-folios-en-el-cliente.md)
- Normativa: [NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md), [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md)
- Requisitos relacionados: [`RNF-03`](RNF-03-operacion-sin-conectividad.md), [`RNF-09`](RNF-09-instalacion-respaldo-y-restauracion.md), [`RNF-20`](RNF-20-observabilidad-y-diagnostico.md), [`RNF-21`](RNF-21-integridad-de-folios-y-correlativos.md)
- Insumos: #32 (horario hábil), #72 (ventana de mantenimiento y tolerancia de indisponibilidad)
