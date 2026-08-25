# HU-045 — Relevar al motorista con la misión `EN_RUTA`, con acta de traspaso y corte de odómetro

| Campo | Valor |
|---|---|
| **Módulo** | M-08 Ejecución y Bitácora · M-07 Programación y Despacho · M-16 Sincronización y Operación Desconectada |
| **Actor** | ACT-06 Motorista saliente y entrante · ACT-04 Jefe de Transporte (autoriza) |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | **Borrador — reabierta por `HB34-01`**: trataba como hallazgo posterior lo que `RN-10` declara bloqueo duro. Corregida abajo; vuelve a refinamiento junto con [HU-061](HU-061-relevo-de-motorista-en-ruta.md) |
| **Deriva de** | [CU-07](../casos-de-uso/CU-07-sustituir-vehiculo-o-motorista.md) A5, A6, E3 · `T-17` · `BD-02` |

## Nota de corrección — hallazgos `HB34-01`, `HB34-02`, `HB34-06` y `HB34-21`

> **`HB34-01` — duplicación con [HU-061](HU-061-relevo-de-motorista-en-ruta.md), y contradicción sobre un bloqueo duro.** Las dos historias describían el mismo acto —el traspaso en ruta con acta y corte de odómetro— y lo resolvían distinto: `HU-061` **rechaza** al relevo cuya licencia vence antes del retorno; esta historia lo **aceptaba** e invocaba [`RN-55`](../../01-negocio/reglas/RN-55-habilitacion-vencida-durante-la-mision.md) para marcarlo como hallazgo.
>
> **Manda `RN-10`**, que en sus condiciones de aplicación dice literalmente: *«Aplica a toda asignación, despacho, **sustitución en ruta** y extensión de misión.»* El relevo es sustitución en ruta. `RN-55` gobierna únicamente el vencimiento **sobrevenido** —el de quien ya va al volante— y ella misma se delimita: *«No aplica antes de la salida: ahí manda `RN-10`»*. Invocar `RN-55` para un acto de asignación convertía un bloqueo duro en un hallazgo posterior, que es exactamente lo que `RN-10` describe como el error clásico. **Se corrige esta historia**, no la regla.
>
> **Delimitación del par, aplicando la regla general del [`README`](README.md):** el lote de flujo manda en el acto y su momento; el lote de expediente manda en el dato y su ciclo de vida.
>
> | | `HU-045` (M-08 · M-07 · M-16) | `HU-061` (M-08 · M-05) |
> |---|---|---|
> | Manda en | **El acto del traspaso**: acta, corte de odómetro, custodia, traspaso del fondo, código de autorización fuera de línea, impedimento de firma | **La revalidación de la habilitación del entrante**: matriz categoría↔vehículo, vigencia en todo el rango, relevo declarado en la programación |
> | No reimplementa | La verificación de licencia del entrante: la referencia a `HU-061` | El acta, el fondo y la custodia: los referencia a `HU-045` |
>
> Ambas se construyen **en el mismo sprint**. Hoy el relevo se construía dos veces, con dos reglas distintas y dos sprints de separación.
>
> **Código de autorización fuera de línea — diferencia zanjada.** `HU-045` lo exigía; `HU-061` no lo mencionaba y su camino feliz ocurría a tres días sin conectividad sin él. **Se adopta la postura de `HU-045`: es obligatorio en todo relevo registrado sin conectividad** — es la única constancia de que la jefatura conoció y consintió el cambio de custodia antes de que ocurriera. `[C]` **reversible**: si el PO decide que el relevo por incapacidad súbita no puede esperar un código dictado por radio, la excepción se acota a ese motivo tipificado y se registra como decisión de producto. El mecanismo del código es de [HU-055](HU-055-ampliar-alcance-autorizado-en-ruta.md).
>
> **`HB34-02` — `I-11` no se evaluaba sobre el entrante.** [`RN-01`](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md) exige la comprobación *«antes de asignar o **sustituir** al motorista»*, y `I-11` es núcleo irreductible. Se agrega la regla y el escenario de rechazo.
>
> **`HB34-06` — el salvoconducto.** La nota anterior seguía a `BD-04` (vehículo y ventana) y concluía que el relevo **no** exige reemisión. `HB3-07` ya había adoptado **la lectura más exigente**, que es la de [`RN-23`](../../01-negocio/reglas/RN-23-permiso-de-circulacion-en-dia-inhabil.md): el permiso ampara **vehículo, motorista, ruta y ventana**. Se aplica esa resolución: el relevo **invalida** el salvoconducto impreso y obliga a reemitirlo.
>
> **`HB34-21` — la historia se contradecía consigo misma.** Los `Antecedentes` declaraban al relevo "Elder Zavala" con licencia vigente hasta el `2027-11-30` y el último escenario decía que vencía el `2026-09-16`. El `Dado` local pisaba el antecedente sin decirlo, en el escenario que decide un bloqueo duro. Corregido: el caso de vencimiento usa una persona distinta, declarada en su propio `Dado`.

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
- [RN-01](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md) — **Segregación de funciones sobre el entrante.** `I-11` —conducir × autorizar, despachar, entregar el fondo o liquidar la misma misión— es **núcleo irreductible**: bloqueo duro que no se levanta por emergencia, delegación ni resolución de la máxima autoridad. La regla exige la comprobación *antes de sustituir al motorista*, y el relevo es una sustitución. Incorporada por `HB34-02`
- [RN-23](../../01-negocio/reglas/RN-23-permiso-de-circulacion-en-dia-inhabil.md) — El salvoconducto ampara **vehículo, motorista, ruta y ventana**: el relevo lo invalida y obliga a reemisión (resolución de `HB3-07`, aplicada por `HB34-06`)
- [RN-55](../../01-negocio/reglas/RN-55-habilitacion-vencida-durante-la-mision.md) — **Alcance acotado por `HB34-01`**: gobierna únicamente el vencimiento **sobrevenido** de quien ya va al volante. **No** ampara la incorporación de un relevo cuya licencia no cubre la ventana: ahí manda `RN-10`, y es bloqueo duro

La matriz de incompatibilidades es autoridad de [`actores-y-roles.md §5.2`](../../01-negocio/actores-y-roles.md): esta historia **no la copia**, la aplica. La verificación de licencia, categoría y vigencia del entrante la desarrolla [HU-061](HU-061-relevo-de-motorista-en-ruta.md); esta historia la exige como precondición del acta.

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
    Y un retorno previsto el "2026-09-17"
    Y un paquete normativo congelado al despacho el "2026-09-15"
    Y una asignación de fondo "AC-2026-0233" entregada a "José Martínez"
    Y un salvoconducto impreso "SC-2026-0087" que ampara vehículo, motorista, ruta y ventana
    Y un dispositivo portador sin conectividad

  Escenario: Se rechaza el relevo con una persona no habilitada según el paquete congelado
    Dado un servidor "Marvin Discua" con licencia categoría "B"
    Cuando se intenta registrar el relevo a favor de "Marvin Discua"
    Entonces el dispositivo rechaza el relevo
    Y muestra "La licencia categoría B no habilita el vehículo INS-P-014 según la matriz congelada al despacho."
    Y registra el intento para sincronizarlo después

  Escenario: Se rechaza el relevo del entrante cuya licencia no cubre el retorno previsto
    Dado un motorista de relevo declarado "Marvin Cruz" con licencia "03-1991-40218"
      categoría "C" vigente hasta el "2026-09-16"
    Cuando se intenta registrar el relevo a favor de "Marvin Cruz" el "2026-09-16"
    Entonces el dispositivo rechaza el relevo
    Y muestra "La licencia de Marvin Cruz vence el 16/09/2026, antes del retorno previsto el 17/09/2026. El relevo es una sustitución en ruta y exige licencia vigente en todo el rango (RN-10)."
    Y el bloqueo no admite excepción por urgencia, por jerarquía ni por estar en carretera
    Y registra el intento para sincronizarlo después

  Escenario: Se rechaza el relevo a favor de quien autorizó la misma Orden de Misión
    Dado un servidor "Carlos Rodríguez" con licencia categoría "C" vigente hasta el "2028-04-30"
    Y que "Carlos Rodríguez" autorizó la Orden de Misión "OM-2026-0451" el "2026-09-14"
    Cuando se intenta registrar el relevo a favor de "Carlos Rodríguez"
    Entonces el dispositivo rechaza el relevo
    Y muestra "Carlos Rodríguez autorizó la Orden de Misión OM-2026-0451 el 14/09/2026. Por incompatibilidad I-11 (RN-01) no puede recibir la conducción de esa misión. Es núcleo irreductible: no admite excepción."
    Y registra el intento con el par de incompatibilidad detectado

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

  Escenario: El relevo invalida el salvoconducto impreso y obliga a reemitirlo
    Dado que el relevo a favor de "Elder Zavala" quedó registrado el "2026-09-16 14:30"
    Cuando se consulta el estado del salvoconducto "SC-2026-0087"
    Entonces el sistema responde estado "DESACTUALIZADO"
    Y muestra al Jefe de Transporte "El salvoconducto SC-2026-0087 amparaba a José Martínez. Con el relevo del 16/09/2026 14:30 dejó de corresponder: reemítalo a nombre de Elder Zavala."
    Y el folio "SC-2026-0087" no se recicla en la reemisión

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

  Escenario: La licencia de quien ya iba al volante vence en ruta y el relevo es la salida
    Dado que la licencia de "José Martínez" vence el "2026-09-16" y la misión retorna el "2026-09-17"
    Y que "José Martínez" ya estaba conduciendo cuando la misión salió, con licencia vigente al despacho
    Cuando llega el "2026-09-17" sin que se haya registrado el relevo
    Entonces el sistema no detiene la ejecución en carretera
    Y el expediente se marca para cerrar con hallazgo por habilitación vencida durante la misión (RN-55)
    Y el dispositivo muestra "La licencia de José Martínez venció el 16/09/2026. Registre el relevo a favor de un conductor habilitado."
    Y el relevo que se registre a partir de ese momento se evalúa con RN-10, no con RN-55
```

## Fuera de alcance

- **El cambio de vehículo con la misión `EN_RUTA`**: `T-17` no lo cubre — ver notas
- La interrupción en ruta por avería o accidente y su desenlace — es del caso de uso de interrupción en ruta
- La liquidación por tramo — es de M-13
- La sustitución antes de la salida — es [HU-043](HU-043-sustituir-vehiculo-o-motorista-en-programada.md)
- **La revalidación de la habilitación del entrante** —matriz categoría↔vehículo, vigencia en todo el rango, relevo declarado en la programación— es [HU-061](HU-061-relevo-de-motorista-en-ruta.md). Esta historia la exige como precondición del acta y **no la reimplementa** (delimitación de `HB34-01`)
- La reemisión del salvoconducto invalidado por el relevo — es [HU-018](HU-018-reemision-del-permiso-por-cambio-de-elementos.md); aquí solo se produce el disparador

## Notas y pendientes

- `[C]` **Código de autorización fuera de línea obligatorio en todo relevo sin conectividad.** Postura adoptada por `HB34-01` frente a `HU-061`, que no lo exigía. **Es reversible**: si el PO decide eximir el relevo por incapacidad súbita, la excepción se acota a ese motivo tipificado. El mecanismo lo define [HU-055](HU-055-ampliar-alcance-autorizado-en-ruta.md), que hoy va en un sprint posterior — dependencia invertida reportada en `HB34-08`, se corrige en el backlog, no aquí
- `[C]` **`HB3-12` — no existe transición para cambiar el vehículo con la misión `EN_RUTA`.** `T-17` cubre prórroga, destino adicional y relevo de motorista, no cambio de unidad. El vacío se alcanza por dos caminos —la avería y la simple decisión administrativa— y esta historia **no lo resuelve**: si el PO necesita esa capacidad, hace falta decisión sobre la [máquina de estados](../../03-arquitectura/estados/orden-de-mision.md), que es la autoridad en transiciones. El tratamiento pedido es idéntico al del relevo: revalidación contra el paquete congelado, acta de traspaso con odómetro como corte, conservación de la asignación original y recálculo de los valores derivados del vehículo.
- **Divergencia cerrada (`HB34-06`):** el salvoconducto ampara *vehículo, motorista, ruta y ventana* — es la lectura de [`RN-23`](../../01-negocio/reglas/RN-23-permiso-de-circulacion-en-dia-inhabil.md) y la **resolución más exigente ya adoptada en `HB3-07`** frente a `BD-04` y `PC-03`. Consecuencia aplicada: **el relevo en ruta invalida el permiso impreso y exige reemisión**. La nota anterior de esta historia seguía a `BD-04` y concluía lo contrario; era la lectura menos exigente y no aplicaba la resolución vigente. `BD-04` y `PC-03` quedan como redacciones a corregir en su artefacto de origen.
- `[C]` **Límite de jornada de conducción** — insumo #48: sin él, el motivo "jornada agotada" no tiene umbral verificable.
- `[C]` **¿Qué se hace hoy cuando no hay ningún motorista disponible para relevar en carretera?** — insumo #51. Es la pregunta que decide si el vehículo pasa la noche en la vía.
- `[C]` **¿Existe reevaluación de aptitud para conducir tras un evento de salud en ruta?** — insumo #50.
