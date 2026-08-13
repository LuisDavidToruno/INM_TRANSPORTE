# RN-60 — Toda indisponibilidad sobrevenida del vehículo exige causa tipificada, ventana estimada y desenlace explícito de cada reserva afectada

| Campo | Valor |
|---|---|
| **Módulos** | M-03, M-11, M-07, M-14 |
| **Origen** | Caso especial [CE-16](../../02-requisitos/casos-especiales/CE-16-vehiculo-a-taller-con-misiones-programadas.md) · Norma [NRM-02](../normativa/NRM-02-bienes-del-estado.md) · [DP-001 D-08](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) |
| **Verificación** | `[P]` el deber de mantener operativos y controlados los bienes — [NRM-02](../normativa/NRM-02-bienes-del-estado.md). `[I]` el efecto sobre reservas constituidas: implicación de requerimiento del equipo |
| **Tipo** | Bloqueo duro |
| **Configurable** | Sí — catálogo `causa_indisponibilidad` y `horizonte_reservas_afectadas` |

## Enunciado

Toda transición del vehículo a un estado que no habilita asignación — `EN_TALLER`, `NO_DISPONIBLE` y sus equivalentes de [orden-de-mision.md §10.2](../../03-arquitectura/estados/orden-de-mision.md) — **debe** exigir:

1. **Causa tipificada** del catálogo configurable
2. **Ventana de indisponibilidad estimada**, con fecha de fin
3. **Acuse expreso de quien la ejecuta sobre la lista de reservas afectadas**, mostrada tal como se le presentó y conservada con su marca de tiempo

Toda Orden de Misión ya `PROGRAMADA` o `DESPACHADA` sobre ese vehículo **debe** marcarse **en conflicto**. La marca:

- **impide el despacho** mientras subsista
- **obliga a un desenlace explícito registrado antes del inicio de la ventana de la misión**: sustituir vehículo, reprogramar, anular o levantar la indisponibilidad

Una reserva en conflicto **no expira en silencio** ni se resuelve por el paso del tiempo.

## Justificación

[`RN-19`](RN-19-vehiculo-no-operativo-no-se-asigna.md) gobierna el **acto de asignar**: impide que se programe un vehículo que ya está en taller. **Nada gobierna el efecto del cambio de estado sobre las reservas ya constituidas.** El vehículo entra al taller el jueves con cuatro misiones programadas para la semana siguiente, y el sistema no le dice nada a nadie.

El resultado observable es que el conflicto se descubre en `T-12`, la mañana de la salida, con el personal esperando y la orden ya impresa — y se resuelve tachando el papel, que es la forma en que el registro deja de corresponder a la realidad.

El acuse del ejecutante no es una formalidad: es **la defensa de quien mandó el vehículo al taller**. Ante la pregunta de por qué se paralizó una misión, la respuesta documentada es que la indisponibilidad se declaró con la lista de afectadas a la vista y con responsable identificado.

## Condiciones de aplicación

Aplica a toda indisponibilidad, programada o sobrevenida, de cualquier origen: mantenimiento preventivo, correctivo, siniestro, robo, decomiso, préstamo, fin de tenencia.

Aplica al vehículo `EN_MISION` que se avería en ruta, con la particularidad de que ahí el desenlace lo gobierna [`RN-70`](RN-70-interrupcion-en-ruta-con-desenlace-obligatorio.md).

**No aplica** a la indisponibilidad de duración menor al parámetro `duracion_minima_indisponibilidad_notificable` — un lavado, un cambio de llanta en el predio — que se registra sin disparar el circuito de reservas.

## Comportamiento esperado

1. Antes de confirmar la indisponibilidad, el sistema muestra las Órdenes de Misión afectadas dentro del `horizonte_reservas_afectadas`: folio, dependencia solicitante, ventana, motorista y objeto. Quien ejecuta acusa.
2. La lista mostrada se **conserva exactamente como se presentó**, con su marca de tiempo. No se reconstruye después.
3. Cada misión afectada queda marcada en conflicto y se **notifica a ACT-04 y a la dependencia solicitante** con acuse.
4. El desenlace se ejecuta con la transición que corresponda —`T-10`, `T-11`, `T-13`, `T-15` o `T-16` según la [máquina de estados](../../03-arquitectura/estados/orden-de-mision.md), que es la autoridad— con actor, rol ejercido, motivo tipificado y marca de tiempo. **La asignación original se conserva junto a la sustituta, nunca sobrescrita.**
5. Si al inicio de la ventana la misión sigue en conflicto sin desenlace, el despacho se bloquea y el hecho entra al reporte de indisponibilidad de flota.
6. Al dar de alta el vehículo, se registra la **fecha real** con la orden de trabajo cerrada y el odómetro de salida, contrastada contra la ventana estimada. La desviación sistemática entre estimado y real es indicador de la gestión del taller.
7. El sistema reporta, para un horizonte configurable, las misiones programadas sobre vehículos con **mantenimiento preventivo por vencer dentro de la ventana de la misión** — por kilometraje o por fecha ([`RN-17`](RN-17-alertas-de-vencimiento-documental.md)).

## Casos límite

- **Falla que no impide rodar** — un odómetro averiado, un aire acondicionado. No es indisponibilidad y no dispara esta regla; rige [`RN-90`](RN-90-intervencion-del-instrumento-de-medicion.md) para el odómetro. `[C]` insumo #35, escala de severidad de fallas: hoy *"entra a taller el lunes"* y *"no se mueve de aquí"* son el mismo `EN_TALLER`.
- **Un solo vehículo sustituto y dos misiones en conflicto.** Es el mismo hueco de [CE-12](../../02-requisitos/casos-especiales/CE-12-dos-solicitudes-compiten-por-el-mismo-vehiculo.md) y se resuelve con [`RN-56`](RN-56-prelacion-entre-solicitudes-que-compiten.md), incluida la constancia de la desplazada.
- **Indisponibilidad que se levanta antes de lo estimado.** Las reservas en conflicto no se restauran automáticamente: quien las desplazó ya tomó decisiones. El sistema ofrece reasignar y registra la decisión.
- **Vehículo que no vuelve** — siniestro total, robo, decomiso. La indisponibilidad no tiene fecha de fin estimada; se declara `indefinida` con expediente de M-12 vinculado ([`RN-75`](RN-75-bien-retenido-o-sustraido-no-sale-del-registro.md)), y todas las reservas se resuelven por sustitución o anulación.
- **Ejecutante que acusa sin leer.** El sistema no puede evitarlo, pero deja constancia de qué se le mostró y cuándo. Es lo máximo que un sistema puede hacer, y es suficiente para que la responsabilidad quede donde corresponde.

## Trazabilidad

- Autoridad: [orden-de-mision.md](../../03-arquitectura/estados/orden-de-mision.md) — transiciones `T-10`, `T-11`, `T-13`, `T-15`, `T-16`; §10.2 estado operativo del vehículo, `W-07` a `W-10`
- Norma: [NRM-02](../normativa/NRM-02-bienes-del-estado.md) `[P]` · Decisión: [DP-001 D-08](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md)
- Reglas relacionadas: [RN-13](RN-13-sin-doble-asignacion.md), [RN-17](RN-17-alertas-de-vencimiento-documental.md), [RN-19](RN-19-vehiculo-no-operativo-no-se-asigna.md), [RN-56](RN-56-prelacion-entre-solicitudes-que-compiten.md), [RN-61](RN-61-sustitucion-de-vehiculo-recalcula-valores-congelados.md)
- Casos especiales: [CE-16](../../02-requisitos/casos-especiales/CE-16-vehiculo-a-taller-con-misiones-programadas.md) — candidatas `RN-C16a`, `RN-C16b`, `RN-C16d`
- Insumos pendientes: #31 criterio de prelación · #35 escala de severidad de fallas
- Actores: ACT-11 declara la indisponibilidad y acusa · ACT-04 resuelve el desenlace · ACT-08 escala prioridad
