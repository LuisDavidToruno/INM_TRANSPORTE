# HU-118 — Registrar cada consulta a un manifiesto con su alcance, incluida la búsqueda sin resultados

| Campo | Valor |
|---|---|
| **Módulo** | M-17 Traslado de Personas Externas · M-14 Reportes, Indicadores y Auditoría |
| **Actor** | ACT-12 Auditor Interno |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Refinada |

## Historia

**Como** Auditor Interno
**quiero** que quede registro inmutable de quién vio qué lista de pasajeros, cuándo y con qué alcance
**para** poder responder con prueba cuando una persona pregunte quién accedió a sus datos, en lugar de responder que no se sabe

## Contexto

[NRM-07](../../01-negocio/normativa/NRM-07-transparencia-y-datos-personales.md) conserva esta exigencia después de la reducción de alcance de [DP-001 D-14](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md): *"registro de cada consulta: quién vio qué lista y cuándo. Aun sin ley de datos, esto es exigible por el MARCI y protege ante un hábeas data"*.

El hábeas data del Art. 182 constitucional está vigente `[V]` y **solo el titular puede interponerlo**. Si una persona pregunta quién accedió a sus datos, la única respuesta defendible es el registro de consultas. Sin él, la institución no puede afirmar nada — ni que sí, ni que no.

Tres detalles que se pierden si no se escriben aquí:

- **La búsqueda que no devuelve nada también se registra.** Buscar el nombre de una persona en el sistema revela interés aunque no haya coincidencias.
- **Imprimir es acceder**, y además genera un objeto fuera del control del sistema.
- **El auditor no es excepción.** Es quien más necesita que el registro exista.

## Reglas que la gobiernan

- [RN-52](../../01-negocio/reglas/RN-52-registro-de-consultas-a-manifiestos.md) — **Regla rectora**: identidad, rol, fecha y hora, registro consultado y alcance; registro inmutable
- [RN-03](../../01-negocio/reglas/RN-03-registro-inmutable-de-autorizacion.md) — El registro se hace inmutable con identidad, momento, origen y huella del contenido mostrado
- [RN-04](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md) — Ningún registro de consulta se borra
- [RN-25](../../01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md) — El documento impreso lleva folio, que es lo que ata la consulta al objeto físico

## Casos especiales que la afectan

> Sección incorporada por el hallazgo `HB34-13`: faltaba, y el `DoR` exige identificar los `CE-xx` que afectan a la historia **o dejar constancia explícita de que no hay ninguno**.

- [CE-09](../casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md) — La consulta hecha en campo sin conectividad: el registro se genera en el dispositivo y sincroniza después, y hasta que sincronice la institución no puede responder por ese acceso. El tratamiento lo desarrolla [HU-120](HU-120-consultar-el-manifiesto-sin-conectividad.md)
- [CE-28](../casos-especiales/CE-28-hallazgo-posterior-sobre-mision-cerrada.md) — Cuando se abre un hallazgo sobre una misión ya cerrada, el registro de consultas es la prueba de quién accedió al manifiesto de esa misión. Por eso `RN-52` exige que **sobreviva** a la depuración de los datos consultados ([HU-124](HU-124-depurar-datos-personales-sin-romper-la-cadena.md))
- Los 26 `CE-xx` restantes **no tocan este flujo**: describen incidencias de la operación del vehículo y de la misión, no del acceso a datos. Constancia dejada

## Requisitos no funcionales relacionados

- [RNF-14](../no-funcionales/RNF-14-control-de-acceso-por-puesto-y-registro-de-consultas.md) — Control de acceso por puesto y registro de consultas
- [RNF-04](../no-funcionales/RNF-04-bitacora-append-only-con-hash-encadenado.md) — Bitácora solo de anexión con hash encadenado
- [RNF-02](../no-funcionales/RNF-02-volumen-y-crecimiento-del-acervo.md) — El volumen del registro de consultas superará al de los datos mismos

## Criterios de aceptación

> Los nombres de personas externas de estos escenarios son **ficticios de prueba**.

```gherkin
# language: es
Característica: Registro de consultas a manifiestos y listas de pasajeros

  Antecedentes:
    Dado una Orden de Misión "OM-2026-0451" con manifiesto cerrado de "3" personas externas
    Y un Auditor Interno con ámbito "institución, solo lectura"

  Escenario: Se rechaza borrar un registro de consulta
    Dado un registro de consulta del "2026-09-19" a las "10:04" sobre el manifiesto de "OM-2026-0451"
    Cuando el Administrador del Sistema intenta eliminar ese registro de consulta
    Entonces el sistema rechaza la eliminación
    Y muestra "El registro de consultas no se borra ni se edita. Es la única prueba de trazabilidad de acceso a datos personales."

  Escenario: Se rechaza mostrar el manifiesto si el registro de la consulta no se puede escribir
    Dado que el almacén del registro de consultas no está disponible
    Cuando el Encargado de Despacho intenta abrir el manifiesto de "OM-2026-0451"
    Entonces el sistema deniega el acceso
    Y muestra "No se puede registrar la consulta en este momento, así que no se muestra el dato. Intente de nuevo o reporte la incidencia."

  Escenario: La búsqueda por nombre sin resultados también se registra
    Cuando el Auditor Interno busca en manifiestos el nombre "Daniela de Prueba Cuatro" y no hay coincidencias
    Entonces el sistema registra una consulta con alcance "BÚSQUEDA POR IDENTIDAD", resultado "0 coincidencias", identidad, rol, fecha y hora
    Y muestra "Sin coincidencias. La búsqueda quedó registrada."

  Escenario: La consulta del conteo se registra con alcance menor que la del manifiesto completo
    Cuando el Jefe de Transporte consulta el conteo de ocupantes de "OM-2026-0451"
    Entonces el sistema registra una consulta con alcance "CONTEO"
    Y no registra ninguna identidad de persona trasladada en el registro de la consulta

  Escenario: La impresión se registra como consulta con impresión y con folio
    Cuando el Encargado de Despacho imprime la lista de abordo de "OM-2026-0451" con folio "LA-2026-000318"
    Entonces el sistema registra una consulta con alcance "COMPLETO" y modalidad "CON IMPRESIÓN"
    Y asocia el folio "LA-2026-000318" al registro de la consulta

  Escenario: La exportación masiva para auditoría registra volumen y destino
    Cuando el Auditor Interno exporta los manifiestos del período "2026-01-01" al "2026-09-30"
    Entonces el sistema registra una consulta con alcance "EXPORTACIÓN MASIVA"
    Y deja constancia del volumen "412 manifiestos, 1.038 personas" y del destino del archivo
    Y muestra "La exportación quedó registrada: 412 manifiestos, 1.038 personas."

  Escenario: El registro de la consulta conserva la huella de lo que se mostró
    Cuando el Encargado de Despacho abre el manifiesto de "OM-2026-0451"
    Entonces el sistema registra la huella del contenido mostrado
    Y esa huella permite demostrar después qué versión del manifiesto se vio, sin almacenar los datos personales en claro dentro del registro de la consulta
```

## Fuera de alcance

- La decisión de conceder o denegar el acceso — es [HU-117](HU-117-acceso-al-manifiesto-por-necesidad-de-conocer.md)
- El reporte y la alerta sobre esos registros — es [HU-119](HU-119-reporte-de-accesos-y-alerta-de-patron-anomalo.md)
- El registro de consultas generado sin conectividad — es [HU-120](HU-120-consultar-el-manifiesto-sin-conectividad.md)
- La supervivencia del registro a la depuración de los datos consultados — es [HU-124](HU-124-depurar-datos-personales-sin-romper-la-cadena.md)
- El acceso técnico directo a la base de datos: escapa a esta historia por construcción y se documenta como riesgo residual ([RN-52](../../01-negocio/reglas/RN-52-registro-de-consultas-a-manifiestos.md))

## Notas y pendientes

- `[C]` **Plazo de retención del registro de consultas** — insumo #71, con Auditoría Interna y el Oficial de Información Pública. [RN-52](../../01-negocio/reglas/RN-52-registro-de-consultas-a-manifiestos.md) exige que **sobreviva** a la depuración de los datos consultados, así que no puede ser más corto que el de esos datos
- `[C]` Volumen esperado de consultas por período en la institución piloto: determina el dimensionamiento del almacenamiento ([RNF-02](../no-funcionales/RNF-02-volumen-y-crecimiento-del-acervo.md)). **Lo que se dimensiona es el almacenamiento, no se relaja la regla**
- `[I]` Que el sistema deniegue el dato cuando no puede registrar la consulta es aplicación directa del caso límite de [RN-52](../../01-negocio/reglas/RN-52-registro-de-consultas-a-manifiestos.md) sobre el cliente sin conectividad, extendida al servidor
