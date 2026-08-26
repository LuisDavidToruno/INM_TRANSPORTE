# ADR-002 — Adoptar el stack tecnológico en el Sprint 0

| Campo | Valor |
|---|---|
| **Estado** | Aceptada |
| **Fecha** | 2026-08-26 |
| **Decide** | Product Owner, sobre la [designación de LOKI del 2026-08-26](../../07-gestion/designaciones/2026-08-26-stack-y-arranque.md) |
| **Sprint** | 0 |

**Supersede a [`ADR-000`](ADR-000-diferir-seleccion-de-stack.md)**, que difería esta decisión al Sprint 2.

## Contexto

`ADR-000` no difirió la selección por prudencia genérica. La difirió porque **las restricciones que iban a decidirla todavía se estaban descubriendo**, y escribió para sí mismo una cláusula de revisión explícita:

> *Se reconsidera si la institución impone un stack por política de TI antes del Sprint 2.*

Las dos condiciones se cumplieron, y por caminos distintos:

**La condición implícita — que las restricciones estuvieran sobre la mesa.** Los 21 requisitos no funcionales están escritos, con umbrales verificables. La matriz de evaluación que `ADR-000` prometía existe.

**La condición explícita — que la institución impusiera el motor.** El Product Owner confirmó que la institución tiene licencia de **SQL Server 2014 Standard** y que **no hay presupuesto para adquirir otra**. Eso no es una preferencia técnica que se pueda discutir con argumentos técnicos: es una restricción presupuestaria dada.

Conviene decirlo sin eufemismo, porque el `ADR` es lo que va a leer un auditor dentro de tres años: **no elegimos SQL Server 2014. Nos lo encontramos.**

## Requisitos que la condicionan

- [`RNF-02`](../../02-requisitos/no-funcionales/RNF-02-volumen-y-crecimiento-del-acervo.md) — ≈8 GB/año de relacional que nunca se borra. Descalifica Express por aritmética: 10 GB por base
- [`RNF-03`](../../02-requisitos/no-funcionales/RNF-03-operacion-sin-conectividad.md) — 7 días sin red, 0 registros perdidos. Obliga a almacenamiento local real en campo
- [`RNF-09`](../../02-requisitos/no-funcionales/RNF-09-instalacion-respaldo-y-restauracion.md) — restauración probada por no especialista, ≤ 2 h. Es **filtro de elegibilidad**, no meta de calidad
- [`RNF-12`](../../02-requisitos/no-funcionales/RNF-12-uso-en-campo.md) — ≤ 25 % de batería en 8 h con seguimiento activo, en gama baja
- [`RNF-13`](../../02-requisitos/no-funcionales/RNF-13-cifrado-en-transito-y-en-reposo.md) — cifrado en reposo sin modo de compatibilidad
- [`RNF-19`](../../02-requisitos/no-funcionales/RNF-19-configurabilidad-multi-institucion.md) — SIGTI no es del INM: una instancia por institución

## Decisión

| Capa | Qué | Qué lo fuerza |
|---|---|---|
| **Campo** | React Native + SQLite cifrado (SQLCipher) | `RNF-03` · `RNF-08` · `RNF-12` · `RNF-13` · `RNF-15` |
| **Oficina** | React 19 + Vite + TypeScript + Tailwind, desde la plantilla `diseno/` de LOKI (contrato **0.3.3**) | Módulo C de LOKI |
| **Backend** | .NET 10 + ASP.NET Core + EF Core, con `UseCompatibilityLevel(120)` | `RNF-09` · `RNF-19` |
| **Base** | **SQL Server 2014 Standard** — restricción institucional dada | Licencia existente · `RNF-02` |
| **Cifrado** | Respaldo nativo + cifrado por columna en aplicación + BitLocker | `RNF-13` |
| **Adjuntos** | Sistema de archivos, ruta y hash en la base — ver [`ADR-004`](ADR-004-adjuntos-fuera-de-la-base.md) | `RNF-02` · `RNF-09` |

### Qué no tiene 2014, y con qué se reemplaza

| Función | Disponible desde | Reemplazo adoptado |
|---|---|---|
| **TDE** (cifrado transparente) | Standard 2019 | Cifrado de respaldo nativo —que **sí** existe en 2014— + `ValueConverter` por columna + BitLocker en el volumen |
| **Temporal tables** | 2016 | Los dos ejes se modelan a mano — ver [`ADR-006`](ADR-006-temporalidad-bitemporal.md) |
| **Particionado de tablas** | Standard 2016 SP1 | Filegroups de solo lectura para el histórico frío |
| **Compresión de datos** | Standard 2016 SP1 | Nada. Los ≈8 GB/año ocupan más en disco |

### Se desarrolla sobre SQL Server moderno y se genera el script para 2014

Programar diez años sobre un motor que no recibe parches es peor que mantener compatibilidad. La decisión del PO es desarrollar sobre versión moderna, con tres palancas:

1. `UseCompatibilityLevel(120)` en EF Core — **vale 150 por omisión**, y sin esto EF emite SQL que 2014 no entiende `[V]`
2. Base de desarrollo en `COMPATIBILITY_LEVEL = 120` — restaura el estimador de cardinalidad de 2014
3. **El script generado se aplica contra una instancia 2014 real antes de cada entrega**, como paso de integración continua

> **El punto 3 es el que sostiene todo, y hoy no se puede ejecutar.** El nivel de compatibilidad **no apaga las funciones**: en un servidor moderno con la base en nivel 120 se pueden crear temporal tables, particionar y activar TDE igual. 2014 las rechazaría **al aplicar el script, no al desarrollar**. La única red que atrapa eso es la instancia 2014 real — y todavía no se conoce su edición exacta ni su Service Pack (insumo abierto, ver más abajo). **Hasta que esa instancia exista y esté en el pipeline, el punto 3 es una intención, no un control.**

### Lo verificado y lo que falta confirmar

| Afirmación | Nivel |
|---|---|
| La instancia de desarrollo es SQL Server 2025 (17.0.1000.7 RTM, Standard Developer) y **acepta `COMPATIBILITY_LEVEL = 120`** | `[V]` |
| `Microsoft.EntityFrameworkCore.SqlServer` 10.0.10 usa nivel 150 por omisión | `[V]` |
| SQL Server 2014 salió de soporte extendido el 2024-07-09 | `[V]` |
| El cifrado de respaldo nativo existe en 2014 Standard | `[P]` — confirmar en la instancia real |
| **Edición exacta, Service Pack y disponibilidad del cifrado de respaldo** en la instancia de la institución | `[C]` |
| **¿La licencia tiene Software Assurance vigente?** | `[C]` |

## Alternativas consideradas

| Alternativa | A favor | En contra | Por qué se descartó |
|---|---|---|---|
| **Mantener el diferimiento de `ADR-000`** | No compromete nada todavía | Las dos condiciones de su cláusula de revisión ya se cumplieron; diferir más es no decidir, no prudencia | La institución ya impuso el motor. Diferir no cambia ese hecho, solo retrasa el arranque |
| **PostgreSQL** | Costo de licencia **cero**, soportado y parcheado, no multiplica el costo por institución (`RNF-19`), trae particionado y cifrado sin edición premium | Requiere adquirir capacidad operativa que la institución no tiene hoy; el personal conoce SQL Server | **El PO evaluó PostgreSQL y eligió 2014 por la licencia existente.** Se registra así deliberadamente: la alternativa gratuita se vio y se descartó, no pasó desapercibida |
| **SQL Server Express** | Gratuito, mismo motor y mismas herramientas | Límite de **10 GB por base** contra ≈8 GB/año de `RNF-02` | Descalificado por aritmética: se agota en el segundo año |
| **Cliente de campo web (PWA)** | Un solo cliente, sin instalación | Sin geolocalización en segundo plano; cuota de almacenamiento desalojable | Ver [`ADR-003`](ADR-003-cliente-de-campo-instalado.md) |

## Consecuencias

**Positivas**

- El licenciamiento del piloto cuesta cero: la licencia ya está pagada
- El personal de la institución conoce SQL Server y sus herramientas de respaldo, lo que juega directamente a favor de `RNF-09`
- Un solo lenguaje —TypeScript— entre oficina y campo permite compartir las reglas puras de verdad, no transcribirlas. Eso es `RNF-15` hecho código
- .NET 10 y EF Core dan migraciones versionadas, que es lo que hace repetible la actualización on-premise

**Negativas**

- **Cuatro funciones del motor hay que construirlas a mano**: bitemporalidad, particionado, cifrado en reposo y compresión. Cada una es código propio que hay que probar y mantener
- **La cadena de hash necesita serialización explícita** (`sp_getapplock` sobre la cola dentro de la transacción). Sin ella, dos transacciones concurrentes bifurcan la cadena y deja de detectar alteraciones — que es lo único para lo que existe
- **Cada columna cifrada deja de poder buscarse, ordenarse y filtrarse por rango.** El costo se acepta columna por columna y se registra qué consulta se sacrifica en cada caso
- **Dos aplicaciones cliente**, con dos ciclos de publicación
- El desarrollo en versión moderna exige disciplina permanente: el servidor ofrece funciones que el destino no tiene, y solo el paso de CI contra 2014 real las atrapa

**Deuda aceptada**

1. **Motor fuera de soporte extendido desde el 2024-07-09, con datos personales de ciudadanos** bajo `M-17` y control interno del TSC. No recibe parches de seguridad. **La arquitectura compensa funciones que faltan; no compensa parches que no llegan.** Esta deuda requiere **aceptación por escrito del Product Owner**, y hasta que exista queda como insumo abierto.
2. **`RNF-19` dice que SIGTI no es del Instituto Nacional de Migración.** La segunda institución que adopte el sistema y **no** tenga licencia de SQL Server **paga por adoptarlo**. La decisión que abarata el piloto encarece la promesa de reutilización.
3. **El paso de CI contra una instancia 2014 real no existe todavía.** Mientras no exista, nada impide que una migración use una función que el destino rechaza.

## Revisión

Se reconsidera ante cualquiera de estas señales:

- **Aparece Software Assurance vigente** sobre la licencia. Entonces la actualización de versión ya está pagada y buena parte de este ADR sobra: se escribe el ADR que lo supera
- **La institución adquiere una versión soportada** por cualquier vía
- **Se materializa un incidente de seguridad** atribuible a una vulnerabilidad sin parche del motor
- **Una segunda institución sin licencia** entra al alcance y el costo de adopción se vuelve el obstáculo
- **La medición de `RNF-12` en el walking skeleton falla** — eso no toca la base, pero sí la capa de campo. Ver `ADR-003`
