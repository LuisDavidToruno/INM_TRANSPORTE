# HU-045 — Relevar al motorista con la misión `EN_RUTA`, con acta de traspaso y corte de odómetro

| Campo | Valor |
|---|---|
| **Módulo** | M-08 Ejecución y Bitácora · M-07 Programación y Despacho · M-16 Sincronización y Operación Desconectada |
| **Actor** | ACT-06 Motorista saliente y entrante · ACT-04 Jefe de Transporte (autoriza) |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Refinada |
| **Deriva de** | [CU-07](../casos-de-uso/CU-07-sustituir-vehiculo-o-motorista.md) A5, A6, E3 · `T-17` · `BD-02` |

## Historia

**Como** Motorista entrante en un relevo
**quiero** recibir la custodia del vehículo en carretera con acta de traspaso —hora, lugar, odómetro, identidad de quien entrega y de quien recibe y motivo tipificado— validable desde el dispositivo sin conectividad
**para** que el kilometraje, el combustible y los peajes de cada tramo queden imputados a quien realmente condujo, y para que la responsabilidad del tramo anterior no se me transfiera

## Contexto

El relevo en carretera ocurre por razones muy concretas: la jornada de conducción se agotó, el motorista se enfermó a mitad de ruta, o la misión se extendió más allá de lo previsto. Hoy se resuelve por teléfono y el registro queda como si hubiera conducido una sola persona todo el viaje. Cuando aparece un consumo anómalo o una multa, no hay forma de saber quién iba manejando.

**El odómetro del acta es el corte de imputación.** Antes de ese número, todo se imputa al tramo del saliente; después, al del entrante. Sin ese corte, la conciliación galonaje–kilometraje de una misión con relevo no significa nada.

Dos cosas que la operación real obliga a resolver: el relevo tiene que poder ejecutarse **sin red** —por eso el padrón de relevo viaja en el paquete de misión y existe el código de autorización fuera de línea—, y el saliente **puede estar impedido de firmar**, porque a veces está inconsciente en una ambulancia. La custodia se cierra igual, con dos personas presentes que firman y con el impedimento constando.

## Reglas que la gobiernan

- [RN-71](../../01-negocio/reglas/RN-71-traspaso-en-ruta-con-acta-y-corte-de-odometro.md) — **Regla rectora**: todo traspaso en ruta consta en acta con odómetro, y ese odómetro es el corte de imputación
- [RN-72](../../01-negocio/reglas/RN-72-imputacion-por-tramo-de-vehiculo-y-motorista.md) — Con más de un conductor, kilometraje, combustible y peajes se imputan por tramo
- [RN-57](../../01-negocio/reglas/RN-57-habilitacion-de-quien-efectivamente-conduce.md) · [RN-09](../../01-negocio/reglas/RN-09-matriz-licencia-vehiculo.md) · [RN-10](../../01-negocio/reglas/RN-10-licencia-vigente-en-todo-el-rango.md) — `BD-02` sobre el entrante, evaluado contra el **paquete normativo congelado**
- [RN-14](../../01-negocio/reglas/RN-14-sustitucion-de-motorista.md) — La sustitución revalida y conserva la asignación original
- [RN-22](../../01-negocio/reglas/RN-22-custodia-del-vehiculo.md) — El traslado de custodia consta con constancia
- [RN-74](../../01-negocio/reglas/RN-74-sin-atribucion-de-responsabilidad-en-campo.md) — El registro de campo **no** captura atribución de responsabilidad
- [RN-55](../../01-negocio/reglas/RN-55-habilitacion-vencida-durante-la-mision.md) — La habilitación que vence en ruta no detiene la ejecución, pero cierra el expediente con hallazgo

## Casos especiales que la afectan

- [CE-05](../casos-especiales/CE-05-cambio-de-motorista-con-la-mision-en-curso.md) — **Caso rector**: relevo de motorista con la misión en curso
- [CE-10](../casos-especiales/CE-10-motorista-incapacitado-en-ruta.md) — El saliente no puede firmar el acta
- [CE-11](../casos-especiales/CE-11-licencia-vence-durante-la-mision.md) — El relevo es la salida cuando la habilitación vence en ruta
- [CE-02](../casos-especiales/CE-02-averia-mecanica-en-ruta.md) — La avería es el otro camino de traspaso, y **no está resuelto para el vehículo**

## Criterios de aceptación

```gherkin
# language: es
Característica: Relevo de motorista con la misión en ruta

  Antecedentes:
    Dada una Orden de Misión "OM-2026-0451" en estado "EN_RUTA"
    Y un vehículo "Pickup Toyota Hilux" con correlativo "INS-P-014"
    Y un motorista saliente "José Martínez" con la custodia de la misión
    Y un motorista de relevo declarado "Elder Zavala" con licencia "08-1988-77120"
      categoría "C" vigente hasta el "2027-11-30", registrado en el paquete de misión
    Y un paquete normativo congelado al despacho el "2026-09-15"
    Y una asignación de fondo "AC-2026-0233" entregada a "José Martínez"
    Y un dispositivo portador sin conectividad

  Escenario: Se rechaza el relevo con una persona no habilitada según el paquete congelado
    Dado un servidor "Marvin Discua" con licencia categoría "B"
    Cuando se intenta registrar el relevo a favor de "Marvin Discua"
    Entonces el dispositivo rechaza el relevo
    Y muestra "La licencia categoría B no habilita el vehículo INS-P-014 según la matriz congelada al despacho."
    Y registra el intento para sincronizarlo después

  Escenario: Se rechaza el relevo sin código de autorización fuera de línea
    Cuando se intenta registrar el relevo a favor de "Elder Zavala" sin conectividad
      y sin el código de autorización fuera de línea
    Entonces el dispositivo rechaza el relevo
    Y muestra "El relevo sin conectividad requiere el código de autorización fuera de línea de esta misión."

  Escenario: Se rechaza el acta de traspaso sin odómetro
    Cuando se registra el relevo a favor de "Elder Zavala" sin declarar el odómetro
    Entonces el dispositivo rechaza el registro
    Y muestra "El acta de traspaso exige el odómetro: es el corte de imputación de kilometraje y consumo entre los dos tramos."

  Escenario: Relevo completo con acta y corte de odómetro
    Dado un odómetro de salida de "84520" km
    Cuando se registra el relevo a favor de "Elder Zavala" con hora del hecho "2026-09-16 14:30",
      lugar "Comayagua, gasolinera del desvío", odómetro "84890" km,
      motivo tipificado "jornada de conducción agotada",
      y las firmas de "José Martínez" y "Elder Zavala"
    Entonces el dispositivo acepta el relevo sin conectividad
    Y el tramo de "José Martínez" queda cerrado con "370" km recorridos
    Y el tramo de "Elder Zavala" abre en el odómetro "84890" km
    Y la custodia de la misión pasa a "Elder Zavala"
    Y la misión permanece en estado "EN_RUTA"
    Y la responsabilidad del tramo anterior no se transfiere

  Escenario: El fondo no se traspasa sin acta propia
    Dado que el relevo se registró y la asignación "AC-2026-0233" sigue a nombre de "José Martínez"
    Cuando "Elder Zavala" registra un consumo contra "AC-2026-0233"
    Entonces el sistema acepta el registro del consumo
    Y genera una alerta automática "consumo imputado a una asignación cuyo receptor no es el conductor del tramo"
    Y la liquidación se hace por asignación, no por persona presente

  Escenario: Traspaso del fondo con acta propia y conteo de folios
    Cuando "José Martínez" y "Elder Zavala" registran el acta de traspaso del fondo
      con el conteo de vales uno por uno y el saldo enumerado
    Entonces la asignación "AC-2026-0233" queda a nombre de "Elder Zavala" desde ese momento
    Y un consumo posterior imputado a un folio ya traspasado genera alerta automática

  Escenario: El saliente no puede firmar por incapacidad
    Dado que "José Martínez" está siendo trasladado en ambulancia y no puede firmar
    Cuando se registra el relevo declarando el impedimento
      y con la firma de dos personas presentes más el receptor tipificado
    Entonces el dispositivo acepta el acta
    Y la custodia del vehículo se cierra igual
    Y el acta consta con el impedimento declarado
    Y el registro no admite ninguna anotación de atribución de responsabilidad

  Escenario: La licencia del conductor del tramo vence durante la misión
    Dado que la licencia de "Elder Zavala" vence el "2026-09-16" y la misión retorna el "2026-09-17"
    Cuando "Elder Zavala" continúa conduciendo el "2026-09-17"
    Entonces el sistema no detiene la ejecución
    Y el expediente se marca para cerrar con hallazgo por habilitación vencida durante la misión
```

## Fuera de alcance

- **El cambio de vehículo con la misión `EN_RUTA`**: `T-17` no lo cubre — ver notas
- La interrupción en ruta por avería o accidente y su desenlace — es del caso de uso de interrupción en ruta
- La liquidación por tramo — es de M-13
- La sustitución antes de la salida — es [HU-043](HU-043-sustituir-vehiculo-o-motorista-en-programada.md)

## Notas y pendientes

- `[C]` **`HB3-12` — no existe transición para cambiar el vehículo con la misión `EN_RUTA`.** `T-17` cubre prórroga, destino adicional y relevo de motorista, no cambio de unidad. El vacío se alcanza por dos caminos —la avería y la simple decisión administrativa— y esta historia **no lo resuelve**: si el PO necesita esa capacidad, hace falta decisión sobre la [máquina de estados](../../03-arquitectura/estados/orden-de-mision.md), que es la autoridad en transiciones. El tratamiento pedido es idéntico al del relevo: revalidación contra el paquete congelado, acta de traspaso con odómetro como corte, conservación de la asignación original y recálculo de los valores derivados del vehículo.
- **Divergencia registrada:** si el salvoconducto ampara *vehículo, motorista y ventana* (`PC-03`) en lugar de *vehículo y ventana* (`BD-04`), **un relevo en ruta lo invalidaría** y esta historia tendría que exigir reemisión. Se sigue a `BD-04`, que es la autoridad en precondiciones.
- `[C]` **Límite de jornada de conducción** — insumo #48: sin él, el motivo "jornada agotada" no tiene umbral verificable.
- `[C]` **¿Qué se hace hoy cuando no hay ningún motorista disponible para relevar en carretera?** — insumo #51. Es la pregunta que decide si el vehículo pasa la noche en la vía.
- `[C]` **¿Existe reevaluación de aptitud para conducir tras un evento de salud en ruta?** — insumo #50.
