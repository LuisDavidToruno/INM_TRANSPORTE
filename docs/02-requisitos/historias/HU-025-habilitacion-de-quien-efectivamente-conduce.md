# HU-025 — Verificar la habilitación de todos los que van a conducir: titular, relevos y quien no es motorista de padrón

| Campo | Valor |
|---|---|
| **Módulo** | M-07 Programación y Despacho · M-05 Motoristas y Habilitación |
| **Actor** | ACT-04 Jefe de Transporte · ACT-10 Encargado de Delegación en su ámbito |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Refinada |
| **Deriva de** | [CU-04](../casos-de-uso/CU-04-programar-mision-asignar-vehiculo-y-motorista.md) pasos 7 y 8, A4, E1 · `T-08` · `BD-02` · `INV-12` |

## Historia

**Como** Jefe de Transporte
**quiero** que el sistema verifique licencia habilitante, vigencia en todo el rango y restricciones médicas sobre **cada persona declarada para conducir** —motorista titular, motoristas de relevo y quien no pertenece al padrón—
**para** que ningún régimen de uso, jerarquía ni condición de "solo va a manejar un tramo" deje a alguien conduciendo un vehículo del Estado sin habilitación, y para que la responsabilidad no recaiga sobre quien autorizó la misión

## Contexto

El modelo tradicional supone que quien conduce es siempre un motorista de planilla del pool. En la operación real no es así: el funcionario con vehículo asignado conduce él mismo, el técnico de la delegación maneja cuando el motorista se enferma, y en una misión de tres días sale un relevo declarado para que nadie conduzca doce horas seguidas. **La verificación se hace sobre quien conduce, no sobre el puesto** ([`RN-57`](../../01-negocio/reglas/RN-57-habilitacion-de-quien-efectivamente-conduce.md)) — ese es exactamente el hueco por el que se escapaba el caso más frecuente.

Es la validación de mayor valor legal del sistema, junto con `HU-012`, que cubre el bloqueo base sobre el motorista titular del padrón. Esta historia lo extiende a **todos los conductores declarados** y agrega el padrón de relevo al paquete de misión, para que el relevo en carretera se pueda validar sin conectividad.

**Bloqueo duro sin excepción configurable**: una excepción registrada en el sistema sería evidencia en contra ante un siniestro ([DP-001 D-12](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md)).

## Reglas que la gobiernan

- [RN-57](../../01-negocio/reglas/RN-57-habilitacion-de-quien-efectivamente-conduce.md) — **Regla rectora**: la habilitación se verifica sobre quien efectivamente conduce, cualquiera sea su puesto
- [RN-09](../../01-negocio/reglas/RN-09-matriz-licencia-vehiculo.md) — La categoría debe habilitar el tipo, el peso bruto y la capacidad del vehículo asignado
- [RN-10](../../01-negocio/reglas/RN-10-licencia-vigente-en-todo-el-rango.md) — Vigente durante **todo** el rango, incluida la holgura posterior
- [RN-11](../../01-negocio/reglas/RN-11-restricciones-medicas-del-motorista.md) — Las restricciones médicas de la licencia deben ser compatibles con las condiciones de la misión
- [RN-14](../../01-negocio/reglas/RN-14-sustitucion-de-motorista.md) — Toda incorporación o cambio de conductor revalida las habilitaciones
- [RN-59](../../01-negocio/reglas/RN-59-todo-uso-se-ampara-en-orden-de-mision.md) — Todo uso se ampara en Orden de Misión, cualquiera sea el régimen del vehículo
- [RN-01](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md) — **Segregación de funciones al declarar al conductor.** La regla exige la comprobación *«antes de registrar cualquier acto de control **y antes de asignar o sustituir al motorista**»*. `I-11` —conducir × autorizar, despachar, entregar el fondo o liquidar la misma misión— es **núcleo irreductible**: bloqueo duro que no se levanta por régimen de excepción, delegación, emergencia ni resolución de la máxima autoridad. Incorporada por `HB34-02`

La matriz de incompatibilidades es autoridad de [`actores-y-roles.md §5.2`](../../01-negocio/actores-y-roles.md): esta historia **no la copia**, la aplica.

> **Nota de corrección — `HB34-02`.** Ninguna de las historias que asignan, reservan, sustituyen o relevan al motorista comprobaba `I-11`. El caso: quien autoriza la orden el lunes se declara conductor de esa misma orden el martes, pasa las cinco verificaciones de habilitación —licencia vigente, categoría habilitante, sin restricciones— y la misión sale con el autorizador al volante. Se detectaba en la verificación de cierre de `RN-01` n.º 5, es decir **después de que ocurrió**, y el único efecto era `CERRADA_CON_HALLAZGO`. [HU-039](HU-039-segregacion-de-funciones-al-despachar.md) cubría la mitad simétrica —el motorista que pretende despachar—; `RN-01` dice que **el sistema bloquea el segundo acto, sea cual sea el orden**, y esta era la mitad que faltaba.

## Casos especiales que la afectan

- [CE-11](../casos-especiales/CE-11-licencia-vence-durante-la-mision.md) — La licencia vence dentro del rango de una misión programada
- [CE-19](../casos-especiales/CE-19-vehiculo-asignado-a-funcionario-frente-al-pool.md) — El funcionario asignatario que conduce su propio vehículo
- [CE-05](../casos-especiales/CE-05-cambio-de-motorista-con-la-mision-en-curso.md) — El relevo declarado es lo que permite validar el cambio en ruta sin red
- [CE-10](../casos-especiales/CE-10-motorista-incapacitado-en-ruta.md) — Sin relevo declarado, la incapacidad en carretera no tiene salida validable

## Criterios de aceptación

```gherkin
# language: es
Característica: Habilitación de cada persona declarada para conducir

  Antecedentes:
    Dada una matriz licencia↔vehículo vigente al "2026-09-10"
    Y un vehículo "Camión Isuzu FVR" con correlativo "INS-C-002", tipo "Camión",
      peso bruto vehicular de "12000" kg
    Y un vehículo "Pickup Toyota Hilux" con correlativo "INS-P-014", tipo "Pickup",
      peso bruto vehicular de "2800" kg
    Y una misión con ventana del "2026-09-10" al "2026-09-14" y holgura posterior de "1" día
    Y un motorista "José Martínez" con licencia "01-1985-04321" categoría "C1",
      vigente hasta el "2027-03-15", sin restricciones médicas
    Y una Orden de Misión "OM-2026-0451" autorizada por "Carlos Rodríguez" el "2026-09-07"

  Escenario: Se rechaza declarar como conductor a quien autorizó la misma Orden de Misión
    Dado un servidor "Carlos Rodríguez" con licencia "04-1983-22910" categoría "C",
      vigente hasta el "2028-04-30", sin restricciones médicas
    Cuando el Jefe de Transporte lo declara como conductor titular de "OM-2026-0451"
    Entonces el sistema rechaza la declaración
    Y muestra "Carlos Rodríguez autorizó la Orden de Misión OM-2026-0451 el 07/09/2026. Por incompatibilidad I-11 (RN-01) no puede ser declarado conductor de esa misión. Es núcleo irreductible: no admite excepción."
    Y no ofrece ninguna opción de continuar por urgencia, por escasez de personal ni por resolución superior
    Y registra el intento con el par de incompatibilidad detectado, el usuario, la función pretendida y el momento

  Escenario: Se rechaza declarar como relevo a quien entregó el fondo de esa misión
    Dado una Encargada de Combustible "Delmy Cruz" con licencia "07-1987-13355" categoría "C" vigente hasta el "2029-02-28"
    Y que "Delmy Cruz" registró la entrega del fondo de "OM-2026-0451"
    Cuando el Jefe de Transporte la declara como relevo de esa misión
    Entonces el sistema rechaza la declaración del relevo
    Y muestra "Delmy Cruz entregó el fondo de combustible de la Orden de Misión OM-2026-0451. Por incompatibilidad I-11 (RN-01) no puede conducirla."
    Y la verificación se hace por identidad de persona, no por rol asignado

  Escenario: Conducir y solicitar sí son compatibles
    Dado que "José Martínez" registró la solicitud que dio origen a "OM-2026-0451"
    Cuando el Jefe de Transporte lo declara como conductor titular de esa misión
    Entonces el sistema acepta la declaración
    Y no genera advertencia por segregación: `I-11` no incluye solicitar

  Escenario: Se rechaza al motorista de relevo cuya licencia no habilita el vehículo
    Dado un motorista de relevo "Marvin Discua" con licencia "05-1990-11987" categoría "B",
      vigente hasta el "2028-01-20"
    Cuando el Jefe de Transporte declara a "Marvin Discua" como relevo del "INS-C-002"
    Entonces el sistema rechaza la declaración del relevo
    Y muestra "La licencia categoría B no habilita un vehículo de 12,000 kg de peso bruto. El vehículo INS-C-002 requiere categoría C."
    Y registra el intento con la versión de la matriz licencia↔vehículo evaluada

  Escenario: Se rechaza al relevo cuya licencia vence antes del fin de la ventana efectiva
    Dado un motorista de relevo "Elder Zavala" con licencia "08-1988-77120" categoría "C",
      vigente hasta el "2026-09-14"
    Cuando el Jefe de Transporte declara a "Elder Zavala" como relevo del "INS-C-002"
    Entonces el sistema rechaza la declaración del relevo
    Y muestra "La licencia 08-1988-77120 vence el 14/09/2026 y la ventana efectiva de la misión termina el 15/09/2026, incluida la holgura posterior."

  Escenario: Se rechaza al conductor que no es motorista de padrón por datos de licencia incompletos
    Dado un servidor "Ana Fúnez", jefa de la unidad solicitante, que no pertenece al padrón de motoristas
    Cuando el Jefe de Transporte la declara como conductora del "INS-P-014"
      sin adjuntar la fotografía de la licencia física
    Entonces el sistema rechaza la declaración
    Y muestra "Para declarar un conductor fuera del padrón se exigen identidad, número de licencia, categoría, fecha de vencimiento, restricciones y fotografía de la licencia física."

  Escenario: Se rechaza al conductor fuera de padrón con la misma exigencia que a un motorista
    Dado un servidor "Ana Fúnez" con licencia "01-1992-30014" categoría "B",
      vigente hasta el "2026-09-12", con fotografía de la licencia adjunta
    Cuando el Jefe de Transporte la declara como conductora del "INS-P-014"
    Entonces el sistema rechaza la declaración
    Y muestra "La licencia 01-1992-30014 vence el 12/09/2026 y la ventana efectiva de la misión termina el 15/09/2026."
    Y no ofrece ninguna opción de continuar por jerarquía, urgencia ni régimen de uso del vehículo

  Escenario: Se rechaza por restricción médica incompatible con las condiciones de la misión
    Dado un motorista "Óscar Banegas" con licencia "02-1979-55210" categoría "C" vigente hasta el "2029-05-01"
    Y una restricción registrada "no conducir en horario nocturno"
    Y que la misión declara conducción entre las "19:00" y las "23:00" del "2026-09-10"
    Cuando el Jefe de Transporte intenta asignar a "Óscar Banegas" a esa misión
    Entonces el sistema rechaza la asignación
    Y muestra "La licencia 02-1979-55210 tiene la restricción 'no conducir en horario nocturno' y la misión declara conducción de 19:00 a 23:00."

  Escenario: Se acepta al titular y a un relevo, ambos verificados por separado
    Dado un motorista de relevo "Elder Zavala" con licencia "08-1988-77120" categoría "C",
      vigente hasta el "2027-11-30", sin restricciones médicas
    Cuando el Jefe de Transporte asigna a "José Martínez" como titular del "INS-P-014"
      y declara a "Elder Zavala" como relevo
    Entonces el sistema acepta la asignación
    Y registra la verificación de "José Martínez" y la de "Elder Zavala" por separado,
      cada una con su número de licencia, categoría, vencimiento y fin de rango evaluado
    Y ambos quedan en el padrón de conductores de la misión

  Escenario: El padrón de conductores viaja en el paquete de misión para validar sin red
    Dado que la misión tiene declarados a "José Martínez" como titular y a "Elder Zavala" como relevo
    Cuando el sistema transfiere el paquete de misión al dispositivo portador
    Entonces el paquete incluye el padrón de conductores con la habilitación verificada de cada uno
    Y el dispositivo puede aceptar la autenticación de "Elder Zavala" sin conectividad
    Y rechaza la autenticación de cualquier persona no declarada, mostrando
      "Esta persona no está declarada como conductor de la misión. El camino es la sustitución."
```

## Fuera de alcance

- El bloqueo base sobre el motorista titular del padrón — es `HU-012` (Bloque de solicitud y autorización)
- El expediente del motorista, la captura de licencias y las alertas anticipadas de vencimiento — son de M-05
- El relevo ejecutado con la misión ya en ruta: el acta y el corte de odómetro son de [HU-045](HU-045-relevo-de-motorista-en-ruta.md); la revalidación del entrante en ese momento es de [HU-061](HU-061-relevo-de-motorista-en-ruta.md)
- La segregación de funciones en los demás actos —autorizar, despachar, entregar el fondo, liquidar— es de [HU-010](HU-010-bloqueo-de-segregacion-y-escalamiento.md), [HU-039](HU-039-segregacion-de-funciones-al-despachar.md), [HU-073](HU-073-impedir-que-quien-solicita-el-fondo-lo-apruebe.md) y [HU-091](HU-091-bloquear-la-liquidacion-por-segregacion-de-funciones.md). **Esta historia cubre exclusivamente el disparo por el acto de declarar al conductor**, que era la mitad de `I-11` que faltaba (`HB34-02`)
- La definición y el mantenimiento de la matriz de incompatibilidades — es de [`actores-y-roles.md`](../../01-negocio/actores-y-roles.md) y de M-01
- La validación contra el registro de la DNVT: no hay integración disponible; el dato es el que capturó la institución

## Notas y pendientes

- `[C]` **¿Puede conducir un vehículo oficial un servidor que no es motorista de planilla?** — insumo #48. Esta historia asume que **sí, con el mismo rigor de verificación**; si la institución lo prohíbe, el bloqueo se endurece pero la verificación no cambia.
- `[C]` **¿Cubre la póliza de seguro a un conductor no registrado como motorista?** — insumo #49. Puede cerrar la discusión del #48 antes de empezar.
- `[C]` **Catálogo oficial de restricciones médicas de la DNVT** — insumo #42. Sin él, las restricciones se capturan como texto tipificado por la institución.
- `[C]` **Criterio de vencimiento de la licencia: ¿al inicio o al fin del día?** — insumo #33.
- `[C]` **Límite de jornada de conducción** que justifica exigir relevo — insumo #48 / `RN-71`. Hoy el relevo es opcional; el sistema no lo exige.
- `[P]` La matriz licencia↔vehículo proviene de [NRM-06](../../01-negocio/normativa/NRM-06-transito-y-licencias.md); falta el texto reformado del Art. 48 — insumo #20.
