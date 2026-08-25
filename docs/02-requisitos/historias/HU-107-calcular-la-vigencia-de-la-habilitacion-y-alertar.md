# HU-107 — Calcular la vigencia de la habilitación y alertar por categoría, al puesto

| Campo | Valor |
|---|---|
| **Módulo** | M-05 Motoristas y Habilitación |
| **Actor** | ACT-04 Jefe de Transporte |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — falta el `criterio_vencimiento_licencia` —¿inicio o fin del día?— contra el texto de la Ley de Tránsito (insumo #33), y si la institución exige capacitaciones o certificaciones internas como **condición** de la habilitación o son dato informativo: de ello depende si entran en el cálculo de la vigencia |

## Historia

**Como** Jefe de Transporte
**quiero** ver junto al estado del motorista hasta qué fecha está habilitado, y recibir alertas anticipadas **por categoría** dirigidas al puesto
**para** convertir un bloqueo del despacho en una gestión hecha con tiempo, en vez de descubrir el vencimiento la mañana de la salida

## Contexto

La vigencia de la habilitación es la **menor fecha de vencimiento entre las categorías que la sostienen**, y se muestra explícita. Un motorista "habilitado" sin fecha visible es un motorista que va a bloquear un despacho sin que nadie lo haya visto venir.

Las alertas van **por categoría, no por licencia**: perder la `C` y conservar la `B` es perder parte de la habilitación, no toda, y la gestión de renovación es distinta.

Y van al **puesto, no a la persona**: la rotación es alta y una alerta a quien ya no está en el cargo no llega a nadie.

## Reglas que la gobiernan

- [RN-17](../../01-negocio/reglas/RN-17-alertas-de-vencimiento-documental.md) — Alertas por categoría, dirigidas al puesto, con umbrales configurables
- [RN-10](../../01-negocio/reglas/RN-10-licencia-vigente-en-todo-el-rango.md) — La licencia debe estar vigente durante todo el rango de la misión
- [RN-09](../../01-negocio/reglas/RN-09-matriz-licencia-vehiculo.md) — La habilitación parcial se recalcula por categoría
- [RN-55](../../01-negocio/reglas/RN-55-habilitacion-vencida-durante-la-mision.md) — El vencimiento sobrevenido en ruta no detiene la misión, pero el expediente cierra con hallazgo
- [RN-39](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md) — `criterio_vencimiento_licencia` como parámetro con vigencia
- [RN-14](../../01-negocio/reglas/RN-14-sustitucion-de-motorista.md) — La reasignación conserva la trazabilidad de la asignación original

## Casos especiales que la afectan

- [CE-11](../casos-especiales/CE-11-licencia-vence-durante-la-mision.md) — Eje de la historia
- [CE-05](../casos-especiales/CE-05-cambio-de-motorista-con-la-mision-en-curso.md) — Relevo cuando la licencia vence en ruta
- [CE-13](../casos-especiales/CE-13-motorista-no-disponible-por-talento-humano.md) — Alertas de disponibilidad, distintas de las de licencia

## Criterios de aceptación

```gherkin
# language: es
Característica: Vigencia de la habilitación y alertas de vencimiento

  Antecedentes:
    Dado un motorista "José Martínez" con categoría "B" vigente hasta el "2028-05-12" y categoría "C1" vigente hasta el "2027-03-15"
    Y umbrales de alerta de "60", "30" y "15" días

  Escenario: La vigencia de la habilitación es la menor fecha entre las categorías
    Cuando el Jefe de Transporte consulta el estado de "José Martínez"
    Entonces el sistema muestra estado "HABILITADO"
    Y muestra "Habilitación vigente hasta el 15/03/2027, por vencimiento de la categoría C1."

  Escenario: La alerta se emite por categoría, no por licencia
    Cuando el sistema evalúa los vencimientos el "2027-01-14"
    Entonces genera alerta únicamente para la categoría "C1"
    Y muestra "Categoría C1 de José Martínez vence el 15/03/2027, en 60 días. La categoría B sigue vigente hasta el 12/05/2028."

  Escenario: La alerta se dirige al puesto y no a la persona
    Dado que cambió el titular del puesto responsable del padrón de motoristas
    Cuando el sistema genera la alerta de vencimiento
    Entonces la alerta la ve quien ocupe el puesto en ese momento
    Y no queda dirigida al titular anterior

  Escenario: Al habilitar se listan las misiones programadas que el vencimiento bloquea
    Dado 3 misiones programadas de "José Martínez" con ventana posterior al "2027-03-15"
    Cuando el Jefe de Transporte habilita o renueva su expediente
    Entonces el sistema lista las 3 misiones cuya ventana excede la vigencia
    Y señala la fecha exacta del vencimiento en cada una

  Escenario: La licencia que vence exactamente el día de retorno aplica el parámetro
    Dado un parámetro "criterio_vencimiento_licencia" con valor "fin del día"
    Y una misión con retorno previsto el "2027-03-15"
    Cuando el Jefe de Transporte asigna a "José Martínez" a esa misión
    Entonces el sistema permite la asignación
    Y muestra la advertencia visible "La categoría C1 vence el mismo día del retorno, 15/03/2027."

  Escenario: El vencimiento sobrevenido en ruta no detiene la misión
    Dado una misión "EN_RUTA" con retorno previsto el "2027-03-20"
    Cuando la categoría "C1" vence el "2027-03-15" con el vehículo ya en carretera
    Entonces el sistema no interrumpe la misión
    Y abre hallazgo automático
    Y muestra "La categoría C1 de José Martínez venció el 15/03/2027 con la misión en ruta. El expediente cerrará con hallazgo."

  Escenario: La prórroga se bloquea si la licencia vence dentro de la ventana ampliada
    Dado una misión "EN_RUTA" con retorno previsto el "2027-03-12"
    Cuando el motorista solicita prórroga hasta el "2027-03-18"
    Entonces el sistema rechaza la prórroga
    Y muestra "La categoría C1 vence el 15/03/2027, dentro de la ventana ampliada. Las salidas son el relevo o el retorno anticipado."

  Escenario: Recalcular la habilitación tras renovar produce el nuevo horizonte
    Cuando el Jefe de Transporte registra la renovación de la categoría "C1" hasta el "2032-03-15" con adjunto
    Entonces la vigencia de la habilitación pasa a "12/05/2028", por vencimiento de la categoría "B"
    Y el sistema lista las misiones programadas que el nuevo rango desbloquea

  Escenario: La alerta vencida no desaparece hasta que se resuelve
    Dado una categoría "C1" vencida el "2027-03-15" y sin renovar
    Cuando el sistema evalúa el "2027-04-20"
    Entonces la alerta permanece en estado "vencido"
    Y permanece hasta que se registre la renovación o la baja del recurso
```

## Fuera de alcance

- La captura de la licencia — es [HU-105](HU-105-capturar-la-licencia-como-dato-propio-de-sigti.md)
- La derivación de vehículos habilitados — es [HU-106](HU-106-derivar-los-tipos-de-vehiculo-habilitados.md)
- La inhabilitación por causa distinta del vencimiento — es [HU-110](HU-110-inhabilitar-con-causa-y-encaminar-misiones.md)
- El trámite de renovación ante la DNVT: se registra el resultado, no se gestiona

## Notas y pendientes

- `[C]` **`criterio_vencimiento_licencia`: ¿inicio o fin del día?** Confirmar contra el texto de la Ley de Tránsito. **No se cablea ninguna de las dos interpretaciones**; el valor inicial es *fin del día* con advertencia visible — insumo **#33**
- `[C]` `umbrales_alerta_vencimiento` de licencia. El valor de referencia 60 / 30 / 15 días es propuesta, no dato confirmado — insumo **#1**
- `[C]` ¿Exige la institución capacitaciones o certificaciones internas —manejo defensivo, primeros auxilios, carga especializada— como **condición** de la habilitación, o son dato informativo? De ello depende si entran en el cálculo de la vigencia — insumo nuevo registrado en [CU-18](../casos-de-uso/CU-18-registrar-y-mantener-la-habilitacion-del-motorista.md)
- `[C]` Reevaluación de aptitud tras un evento de salud en ruta — insumo **#50**
