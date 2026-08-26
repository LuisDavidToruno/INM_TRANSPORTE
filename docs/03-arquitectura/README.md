# 03 — Arquitectura

Cómo se estructura el sistema y por qué.

## Estructura

| Ruta | Estado | Contenido |
|---|---|---|
| `modelo-datos/` | Bloque 4 | Modelo conceptual, lógico y diccionario de datos |
| `estados/` | Bloque 1 | Máquinas de estado: Orden de Misión, vale de combustible, viático, vehículo |
| [`adr/`](adr/README.md) | Sprint 0 | Decisiones de arquitectura `ADR-000` a `ADR-009` |
| [`c4/`](c4/README.md) | Sprint 0 | Contexto y contenedores. Componentes queda para el primer módulo |
| `seguridad/` | Sprint 2 | Autenticación, autorización, auditoría, cifrado, firma electrónica |

## El stack está decidido desde el 2026-08-26

`ADR-000` difirió la selección mientras las restricciones determinantes se descubrían. Las dos condiciones de su cláusula de revisión se cumplieron —los 21 `RNF` están escritos y la institución impuso el motor—, así que **[`ADR-002`](adr/ADR-002-adoptar-el-stack-tecnologico.md) lo supera**.

| Capa | Qué |
|---|---|
| Campo | React Native + SQLite cifrado (SQLCipher) |
| Oficina | React 19 + Vite + TypeScript + Tailwind |
| Backend | .NET 10 + ASP.NET Core + EF Core, `UseCompatibilityLevel(120)` |
| Base | SQL Server 2014 Standard — **restricción institucional dada, no elección** |

El detalle, las funciones que 2014 no tiene, con qué se reemplazan y la deuda aceptada están en `ADR-002`.

## Capacidades identificadas como determinantes

Se registraron a medida que aparecían, y son contra lo que se evaluó el stack en `ADR-002`:

| Capacidad | Origen | Impacto |
|---|---|---|
| Almacenamiento local persistente en cliente de campo y sincronización diferida | `RNF` offline-first | Alto — descarta arquitecturas puramente server-rendered |
| Bitácora append-only inmutable con hash encadenado | Control interno TSC | Alto — condiciona el modelo de persistencia |
| Generación de documentos imprimibles con folio, QR y hash | Marco híbrido papel-digital | Medio |
| Parámetros con vigencia por rango de fechas (temporalidad bitemporal) | Tarifas de peaje y categorías por ejes, que se revisan periódicamente | Alto — condiciona el modelo de datos |
| Espejo local de datos externos con sincronización por eventos y reconciliación | Integración con ARGOS y Talento Humano — ver [ADR-001](adr/ADR-001-integracion-argos-talento-humano.md) | Alto |
| Seguimiento de ubicación y estado en tiempo real, con mapas | M-19; se reutiliza el componente de ARGOS | Medio |
| Instalación y respaldo operables por personal sin especialización | Despliegue on-premise en delegaciones | Alto — descarta stacks con operación compleja |
| Uso desde celular en campo, con cámara y sin conectividad | Realidad rural hondureña | Alto |
| Cifrado en reposo de datos personales | M-17 y hábeas data | Medio |

### Capacidades incorporadas al escribir los `RNF-xx`

Surgieron al fijar umbrales verificables. Ver [`docs/02-requisitos/no-funcionales/`](../02-requisitos/no-funcionales/README.md).

| Capacidad | Origen | Impacto |
|---|---|---|
| **Anclaje externo del sello de la cadena de auditoría** — el sello periódico sale a ≥ 2 destinos fuera del alcance de quien administra la base | [`RNF-04`](../02-requisitos/no-funcionales/RNF-04-bitacora-append-only-con-hash-encadenado.md) | Alto — sin él, la inmutabilidad es solo frente al usuario de la aplicación |
| **Segmento de datos personales separable de la cadena de auditoría** — la cadena encadena referencia y huella, no el contenido en claro | [`RNF-17`](../02-requisitos/no-funcionales/RNF-17-retencion-y-depuracion-diferenciada.md) | Alto — **no se puede agregar después** de tener años de cadena construida |
| **Generación distribuida de folios con rangos pre-asignados por delegación**, distinta de los identificadores internos generados en el cliente | [`RNF-21`](../02-requisitos/no-funcionales/RNF-21-integridad-de-folios-y-correlativos.md) | Alto — condiciona el modelo documental |
| **Reportes reproducibles con fecha de corte de conocimiento** — misma consulta, mismo corte, mismo resultado años después | [`RNF-06`](../02-requisitos/no-funcionales/RNF-06-reproducibilidad-historica-de-reportes.md) | Alto — condiciona toda consulta agregada |
| **Archivado en frío que conserva la consultabilidad**, con reducción de densidad de series y sin eliminación | [`RNF-02`](../02-requisitos/no-funcionales/RNF-02-volumen-y-crecimiento-del-acervo.md) | Alto |
| **Canal de posición en vivo con cola offline y degradación declarada** en el propio marcador del tablero | [`RNF-08`](../02-requisitos/no-funcionales/RNF-08-seguimiento-en-ruta.md) | Medio — el componente de mapas se reutiliza de ARGOS |
| **Almacén local del dispositivo cifrado, con expiración y borrado remoto** | [`RNF-13`](../02-requisitos/no-funcionales/RNF-13-cifrado-en-transito-y-en-reposo.md) | Medio |
| **Pantalla de estado legible por personal no técnico**, con acción sugerida por cada indicador anómalo | [`RNF-20`](../02-requisitos/no-funcionales/RNF-20-observabilidad-y-diagnostico.md) | Medio — no se obtiene añadiendo registros técnicos al final |
| **Autoría histórica inmutable frente a reorganizaciones**: persona, puesto y vigencia separados | [`RNF-15`](../02-requisitos/no-funcionales/RNF-15-continuidad-ante-rotacion-de-personal.md) | Alto — condiciona el modelo de datos |
| **Mismo artefacto de despliegue para toda institución**, con parámetros vacíos y bloqueantes cuando el dato real está `[C]` | [`RNF-19`](../02-requisitos/no-funcionales/RNF-19-configurabilidad-multi-institucion.md) | Alto |

**Los nueve `RNF` determinantes** contra los que se evaluó el stack en [`ADR-002`](adr/ADR-002-adoptar-el-stack-tecnologico.md) están listados en el [índice de requisitos no funcionales](../02-requisitos/no-funcionales/README.md#los-nueve-que-van-a-decidir-el-stack).

## Máquina de estados principal

```mermaid
stateDiagram-v2
    [*] --> BORRADOR
    BORRADOR --> SOLICITADA
    SOLICITADA --> APROBADA
    SOLICITADA --> RECHAZADA
    APROBADA --> PROGRAMADA
    PROGRAMADA --> DESPACHADA
    DESPACHADA --> EN_RUTA
    EN_RUTA --> RETORNADA
    RETORNADA --> LIQUIDADA
    LIQUIDADA --> CERRADA
    LIQUIDADA --> CERRADA_CON_HALLAZGO
    SOLICITADA --> ANULADA
    APROBADA --> ANULADA
    PROGRAMADA --> ANULADA
    RECHAZADA --> [*]
    ANULADA --> [*]
    CERRADA --> [*]
    CERRADA_CON_HALLAZGO --> [*]
```

El detalle de quién puede ejecutar cada transición, qué precondiciones tiene y qué efectos colaterales dispara se documenta en `estados/orden-de-mision.md` (Bloque 1).
