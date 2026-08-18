# HU-124 — Depurar los datos personales de los manifiestos en su plazo sin romper la cadena de auditoría

| Campo | Valor |
|---|---|
| **Módulo** | M-17 Traslado de Personas Externas · M-14 Reportes, Indicadores y Auditoría · M-16 Sincronización y Operación Desconectada |
| **Actor** | ACT-01 Administrador del Sistema · ACT-12 Auditor Interno (verifica) |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — bloqueada por el insumo #71, y correctamente |

## Historia

**Como** Administrador del Sistema
**quiero** que los datos personales de las personas trasladadas se depuren al vencer su plazo, conservando intacto el expediente contable
**para** que la institución no acumule indefinidamente identidades que solo necesitaba para controlar un traslado, y siga pudiendo probar cada asiento ante el Tribunal Superior de Cuentas

## Contexto

Dos normas del mismo Estado piden cosas opuestas ([RNF-17](../no-funcionales/RNF-17-retencion-y-depuracion-diferenciada.md)): [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md) exige conservar todo por el plazo de prescripción, sin borrado físico; [NRM-07](../../01-negocio/normativa/NRM-07-transparencia-y-datos-personales.md) exige retención **más corta** para los datos personales de pasajeros.

Las dos formas de fallar están escritas y son igual de malas:

- **Si se resuelve borrando**, se rompe la cadena de hash y el acervo de auditoría pierde su verificabilidad. Se cumple [NRM-07](../../01-negocio/normativa/NRM-07-transparencia-y-datos-personales.md) destruyendo [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md), que es el peor intercambio posible: **el TSC fiscaliza todos los años; el hábeas data llega rara vez**.
- **Si no se depura nada**, la institución acumula identidades desde el día uno, y ante una fuga el daño es proporcional a todo lo acumulado.

La depuración **no es un borrado**: es un asiento nuevo que declara qué se depuró, cuándo, por qué plazo y con qué autoridad. El registro histórico conserva estructura, conteos y montos; pierde la identidad de la persona.

Y hay una consecuencia que no se puede posponer: **el costo de separar el segmento de datos personales del asiento de auditoría después de tener años de cadena construida es rehacer la cadena, y una cadena rehecha no prueba nada.**

## Reglas que la gobiernan

- [RN-51](../../01-negocio/reglas/RN-51-minimizacion-de-datos-de-personas-externas.md) — Políticas de retención diferenciadas; los datos capturados de más se **seudonimizan**, no se borran
- [RN-52](../../01-negocio/reglas/RN-52-registro-de-consultas-a-manifiestos.md) — El registro de consultas **sobrevive** a la depuración, referenciando el identificador seudonimizado
- [RN-04](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md) — Nada se borra físicamente; la depuración es un asiento
- [RN-39](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md) — Los plazos de retención son parámetros con vigencia, **nunca constantes en el código**
- [RN-94](../../01-negocio/reglas/RN-94-fecha-de-corte-de-conocimiento-en-todo-reporte.md) — Un reporte reproducido a su fecha de corte no cambia por una depuración posterior

## Requisitos no funcionales relacionados

- [RNF-17](../no-funcionales/RNF-17-retencion-y-depuracion-diferenciada.md) — **Requisito rector de esta historia**
- [RNF-04](../no-funcionales/RNF-04-bitacora-append-only-con-hash-encadenado.md) — **0** rupturas de la cadena provocadas por una depuración
- [RNF-06](../no-funcionales/RNF-06-reproducibilidad-historica-de-reportes.md) — El reporte no cambia
- [RNF-13](../no-funcionales/RNF-13-cifrado-en-transito-y-en-reposo.md) — La depuración alcanza respaldos, adjuntos, registros técnicos y dispositivos de campo

## Criterios de aceptación

> Los nombres de personas externas de estos escenarios son **ficticios de prueba** y los plazos son valores de ejemplo, no valores fijados.

```gherkin
# language: es
Característica: Depuración diferenciada de datos personales de manifiestos

  Antecedentes:
    Dado "412" Órdenes de Misión del ejercicio "2026" con manifiestos de personas externas
    Y una cadena de asientos de auditoría encadenados y un sello emitido el "2027-01-15"

  Escenario: Sin plazo configurado, el sistema no depura nada
    Dado que el parámetro "plazo de depuración de datos personales de pasajeros" no está configurado
    Cuando llega la fecha de ejecución programada de la depuración
    Entonces el sistema no depura ningún registro
    Y muestra en la pantalla de estado "Depuración no ejecutada: el plazo de depuración de datos personales no está configurado. Acuerde el plazo con Auditoría Interna y el Oficial de Información Pública."
    Y no aplica ningún plazo por defecto

  Escenario: Se rechaza depurar registros financieros o de bienes
    Dado un plazo de conservación de registros financieros de "10" años, configurado como ejemplo
    Cuando el Administrador del Sistema intenta incluir liquidaciones y vales de combustible del ejercicio "2026" en la depuración
    Entonces el sistema rechaza la inclusión
    Y muestra "Los registros financieros y de bienes se conservan por el plazo de fiscalización y no se depuran. La depuración alcanza únicamente el segmento de datos personales."

  Escenario: Se rechaza ejecutar la depuración sin aviso previo
    Dado una depuración programada para el "2029-01-15"
    Cuando el Administrador del Sistema intenta ejecutarla el mismo día sin que se haya emitido el aviso previo
    Entonces el sistema rechaza la ejecución
    Y muestra "La depuración se anuncia con antelación al responsable y queda en la pantalla de estado. No se ejecuta sin aviso previo."

  Escenario: La depuración conserva lo que el expediente necesita y pierde la identidad
    Dado un plazo de depuración de datos personales de "2" años, configurado como ejemplo
    Cuando el Administrador del Sistema ejecuta la depuración del ejercicio "2026" el "2029-01-15"
    Entonces las fichas conservan conteo de pasajeros, condición agregada, origen, destino, vehículo, misión y costos
    Y no conservan nombre, contacto ni número de documento
    Y cada ficha depurada queda referenciada por un identificador seudonimizado estable

  Escenario: La depuración es un asiento, no un borrado
    Cuando el Administrador del Sistema ejecuta la depuración del ejercicio "2026"
    Entonces el sistema crea un asiento de depuración con alcance "412 manifiestos, 1.038 personas", plazo aplicado "2 años", autoridad, autor y fecha
    Y ese asiento es consultable por el Auditor Interno

  Escenario: La cadena de auditoría verifica sin ruptura después de depurar
    Cuando el Auditor Interno ejecuta el verificador de la cadena sobre el ejercicio "2026" después de la depuración
    Entonces la cadena verifica sin una sola ruptura
    Y el sello emitido el "2027-01-15", antes de la depuración, sigue siendo válido

  Escenario: El reporte reproducido no cambia por la depuración
    Dado un reporte de traslados del ejercicio "2026" generado con fecha de corte "2026-12-31"
    Cuando se regenera después de la depuración con la misma fecha de corte
    Entonces los conteos, los costos y la estructura son idénticos
    Y las identidades aparecen seudonimizadas

  Escenario: El registro de consultas sobrevive a la depuración
    Dado "26" registros de consulta sobre manifiestos del ejercicio "2026"
    Cuando el Administrador del Sistema ejecuta la depuración de ese ejercicio
    Entonces los "26" registros de consulta se conservan
    Y referencian el identificador seudonimizado de la persona consultada
    Y siguen permitiendo demostrar quién vio esos datos y cuándo

  Escenario: La depuración alcanza los dispositivos de campo
    Dado "9" dispositivos de campo con manifiestos del ejercicio "2026" en su almacén local
    Cuando el Administrador del Sistema ejecuta la depuración el "2029-01-15"
    Entonces el sistema muestra cuáles dispositivos ya la aplicaron y cuáles siguen pendientes
    Y aplica la depuración en cada dispositivo al siguiente contacto con el servidor
```

## Fuera de alcance

- La rectificación por hábeas data — es [HU-122](HU-122-rectificar-por-habeas-data-sin-destruir-el-asiento.md)
- El reporte público, que se genera desde la vista de gestión pública — es [HU-123](HU-123-exportar-transparencia-sin-datos-personales.md)
- El plazo de conservación de los registros financieros: es materia de [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md) y no lo fija esta historia
- El diseño del segmento separado de datos personales: es **determinante de arquitectura**, del Sprint 2 ([RNF-17](../no-funcionales/RNF-17-retencion-y-depuracion-diferenciada.md))

## Notas y pendientes

- `[C]` **Plazos de conservación y de depuración** — insumo #71, con Auditoría Interna y el Oficial de Información Pública. **Es lo que mantiene la historia en borrador, y es correcto que la mantenga**: [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md) exige expresamente que el plazo **no se cablee**, y el escenario de plazo no configurado es el comportamiento válido mientras tanto
- `[C]` Quién es la **autoridad** que ordena una depuración en la institución: ¿la Gerencia Administrativa, Auditoría Interna, el OIP? Hoy no hay actor con esa atribución
- `[C]` Si la depuración debe alcanzar los **respaldos históricos** ya escritos, y con qué procedimiento. [RNF-17](../no-funcionales/RNF-17-retencion-y-depuracion-diferenciada.md) lo exige; el procedimiento operativo no está definido
- `[I]` Que la seudonimización use un identificador estable —para que el registro de consultas siga sirviendo— es derivación de [RN-52](../../01-negocio/reglas/RN-52-registro-de-consultas-a-manifiestos.md), no una exigencia literal de norma
