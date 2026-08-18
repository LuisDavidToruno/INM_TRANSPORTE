# HU-068 — Resolver la cola de conflictos viendo ambas versiones lado a lado

| Campo | Valor |
|---|---|
| **Módulo** | M-16 Sincronización y Operación Desconectada · M-14 Reportes, Indicadores y Auditoría |
| **Actor** | ACT-04 Jefe de Transporte · ACT-10 Encargado de Delegación en su ámbito |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador |

## Historia

**Como** Jefe de Transporte
**quiero** ver las dos versiones de un registro en conflicto lado a lado, campo por campo, con sus fotografías y en palabras de la operación
**para** decidir cuál describe lo que realmente pasó sin tener que entender nada de sincronización

## Contexto

**Es la pantalla más difícil del sistema y la que nadie diseña hasta que duele.** Quien resuelve no entiende de sincronización, y no tiene por qué. La pantalla debe decir *"el motorista registró la salida el lunes a las 5:40, sin señal"*, no *"conflicto de versión en la entidad transición, secuencia 1, hash divergente"*.

En este dominio los datos en conflicto son **odómetros, galones y montos**: una sobrescritura automática destruye el término de una conciliación de auditoría, y nadie se entera hasta que el Tribunal Superior de Cuentas pregunta.

**La versión descartada no se borra.** Queda con su contenido íntegro y consultable, vinculada como asiento a la decisión que la descartó ([RN-04](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md)).

**Una cola sin dueño se convierte en un basurero**, y su acumulación es el efecto deseado: bloquea liquidaciones, que es donde el control importa.

## Reglas que la gobiernan

- [RN-45](../../01-negocio/reglas/RN-45-cero-sobrescritura-silenciosa.md) — **Regla rectora**: ningún conflicto se resuelve por sobrescritura; todo va a cola de resolución humana
- [RN-04](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md) — La versión descartada se conserva como asiento vinculado a la decisión
- [RN-03](../../01-negocio/reglas/RN-03-registro-inmutable-de-autorizacion.md) — La resolución es un acto identificado: quién, cuándo, con qué autoridad y por qué
- [RN-89](../../01-negocio/reglas/RN-89-kilometraje-acumulado-invariante-del-expediente.md) — Los conflictos de odómetro son de alto impacto y se resuelven uno por uno
- [RN-84](../../01-negocio/reglas/RN-84-unicidad-del-comprobante-en-la-institucion.md) — El mismo comprobante no sostiene dos consumos; el posible duplicado lo resuelve una persona
- [RN-79](../../01-negocio/reglas/RN-79-el-retorno-constatado-libera-al-vehiculo.md) — La conciliación de odómetros entre lo constatado y lo digitado

## Casos especiales que la afectan

- [CE-09](../casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md) — El papel contradice lo constatado en el portón
- [CE-22](../casos-especiales/CE-22-odometro-inconsistente.md) — Odómetro inconsistente entre versiones
- [CE-05](../casos-especiales/CE-05-cambio-de-motorista-con-la-mision-en-curso.md) — Dos motoristas registran el mismo paso por caseta desde dispositivos distintos
- [CE-02](../casos-especiales/CE-02-averia-mecanica-en-ruta.md) — La oficina gestiona otra cosa sobre una misión que ya se interrumpió sin que nadie lo supiera

## Criterios de aceptación

```gherkin
# language: es
Característica: Cola de conflictos y pantalla de resolución

  Antecedentes:
    Dada la Orden de Misión "OM-2026-0451" con un conflicto abierto sobre el odómetro de retorno
    Y una versión de campo con odómetro "93610", capturada por "José Martínez" el "2026-05-16" a las "21:00", sin señal, con fotografía del tablero
    Y una versión digitada del papel con odómetro "93061", capturada por "Ana Zelaya" el "2026-05-28", con fotografía del original

  Escenario: Se rechaza resolver el conflicto sin motivo
    Cuando el Jefe de Transporte elige la versión de campo sin escribir motivo
    Entonces el sistema rechaza la resolución
    Y muestra "Escriba por qué toma esa versión. La decisión queda en el expediente y el auditor la va a leer."

  Escenario: No existe la acción de editar el dato para "arreglar" el conflicto
    Cuando el Jefe de Transporte busca modificar el odómetro directamente
    Entonces el sistema no ofrece ninguna acción de edición
    Y muestra "No se edita un registro. Elija entre las versiones que existen o registre un asiento nuevo."
    Y tampoco el Administrador del Sistema puede alterar la bitácora

  Escenario: Se rechaza liquidar con divergencias pendientes
    Cuando el Jefe de Transporte intenta liquidar "OM-2026-0451"
    Entonces el sistema rechaza la liquidación
    Y muestra "OM-2026-0451 tiene 1 divergencia pendiente sobre el kilometraje de retorno. Resuélvala antes de liquidar."

  Escenario: La pantalla muestra ambas versiones campo por campo, en lenguaje del negocio
    Cuando el Jefe de Transporte abre el conflicto de "OM-2026-0451"
    Entonces la pantalla muestra ambas versiones completas, con la diferencia resaltada
    Y muestra de cada una quién la capturó, cuándo ocurrió el hecho y cuándo se registró
    Y muestra la fotografía del tablero y la del original, ambas visibles
    Y declara el impacto "Esta misión no se puede liquidar hasta resolver esto."
    Y ningún texto de la pantalla contiene "merge", "timestamp" ni "conflicto de escritura"

  Escenario: Conflicto en campos distintos del mismo registro, sin fusión automática
    Dado un conflicto donde una versión cambia el odómetro y la otra la hora de arribo
    Cuando el Jefe de Transporte abre el conflicto
    Entonces el sistema presenta ambos campos por separado
    Y no combina automáticamente las versiones
    Y muestra "Decida campo por campo. Combinar solo produciría un registro que nadie capturó."

  Escenario: La versión descartada se conserva íntegra
    Cuando el Jefe de Transporte elige la versión de campo con el motivo "hay fotografía del tablero tomada al retornar"
    Entonces la versión digitada queda en estado "RESUELTA_DESCARTADA"
    Y su contenido permanece íntegro y consultable desde el expediente
    Y el Auditor Interno puede ver la versión que no se aplicó y la decisión que la descartó

  Escenario: La oficina anuló, el motorista ya había salido
    Dada una anulación registrada en oficina el "2026-05-12" a las "08:15" por "María López, Jefa de Transporte"
    Y una salida registrada por el motorista el "2026-05-12" a las "05:40", sin señal, con odómetro "92480"
    Cuando el Jefe de Transporte abre el conflicto
    Entonces la pantalla muestra ambas versiones con su hora y su autor
    Y pregunta "El vehículo salió antes de que se registrara la anulación. ¿Qué versión describe lo que pasó?"
    Y no ofrece revivir la anulación sobre una misión que ya está EN_RUTA

  Escenario: Dos motoristas registran el mismo paso por caseta
    Dado un relevo en ruta y dos registros del punto "Peaje Jícaro Galán" dentro de la misma ventana temporal, con identificadores distintos
    Cuando el servidor los procesa
    Entonces los detecta como posible duplicado por punto y ventana temporal
    Y no descarta ninguno automáticamente
    Y los envía a la cola con ambas versiones y sus fotografías

  Escenario: Dos dispositivos sobre la misma misión
    Dado un dispositivo portador designado "DEL-CHO-03" y un segundo dispositivo "DEL-CHO-07"
    Cuando "DEL-CHO-07" envía una cadena que declara la misma transición con datos distintos
    Entonces el servidor aplica la primera cadena recibida
    Y conserva íntegra la segunda como cadena divergente
    Y abre el conflicto con ambas versiones lado a lado
    Y marca la misión como "con divergencia pendiente"

  Escenario: Resolución por lotes con criterio declarado
    Dados "180" conflictos de texto de la Orden de Misión "OM-2026-0451" acumulados tras 3 semanas sin sincronizar
    Cuando el Jefe de Transporte resuelve por lote con el criterio "aceptar la versión de campo para todos los registros de esta misión"
    Entonces el sistema registra el criterio con su autor y su alcance
    Y deja fuera del lote los conflictos de odómetro, monto y autorización
    Y muestra "3 conflictos de alto impacto quedan fuera del lote y se resuelven uno por uno."

  Escenario: La cola se ordena por impacto y antigüedad
    Dado un conflicto de monto abierto hace "3" días y uno de texto abierto hoy
    Cuando el Jefe de Transporte abre su cola
    Entonces el conflicto de monto aparece antes que el de texto
    Y cada conflicto muestra su antigüedad y qué queda bloqueado mientras no se resuelva
```

## Fuera de alcance

- La detección del conflicto en el servidor — es [HU-066](HU-066-sincronizar-sola-y-reanudable.md) y [HU-067](HU-067-resultado-registro-por-registro-y-hueco-de-secuencia.md)
- La reasignación del dispositivo portador, que es acto propio del Jefe de Transporte y queda en el diario
- Las divergencias del espejo de ARGOS y Talento Humano, que **no entran a esta cola** — son [HU-069](HU-069-el-espejo-nunca-diverge-en-silencio.md)

## Notas y pendientes

- `[C]` Responsable por puesto de la cola de conflictos de cada delegación, y plazo de escalamiento de un conflicto sin resolver — insumo #76
- `[C]` Volumen esperado de conflictos por período, sin el cual no se puede dimensionar la cola ni la resolución por lotes — insumo #67
- `[I]` La lista de campos considerados de alto impacto —odómetro, monto, autorización— es parámetro configurable con vigencia, no una lista fija en el código
