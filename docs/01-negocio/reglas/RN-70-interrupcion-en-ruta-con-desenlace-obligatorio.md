# RN-70 — La interrupción en ruta se registra como evento tipificado, marca la misión sin cambiarle el estado, y exige desenlace explícito

| Campo | Valor |
|---|---|
| **Módulos** | M-08, M-12, M-16, M-03, M-07 |
| **Origen** | Casos especiales [CE-02](../../02-requisitos/casos-especiales/CE-02-averia-mecanica-en-ruta.md), [CE-03](../../02-requisitos/casos-especiales/CE-03-accidente-de-transito-en-mision.md), [CE-04](../../02-requisitos/casos-especiales/CE-04-robo-de-vehiculo-o-de-carga-en-mision.md), [CE-10](../../02-requisitos/casos-especiales/CE-10-motorista-incapacitado-en-ruta.md) · Norma [NRM-09](../normativa/NRM-09-realidad-operativa.md) |
| **Verificación** | `[V]` la ausencia de conectividad en el área rural — [NRM-09](../normativa/NRM-09-realidad-operativa.md). `[I]` la marca de interrupción y sus desenlaces: implicación de requerimiento del equipo |
| **Tipo** | Bloqueo duro |
| **Configurable** | Sí — catálogo `causa_interrupcion` |

## Enunciado

Todo hecho que impide continuar la misión según lo autorizado —avería mecánica, accidente, sustracción del vehículo o de la carga, incapacidad del conductor, vía cerrada, condición de seguridad— **debe** registrarse como **evento de interrupción tipificado**, capturable **sin ninguna conectividad**, con: hora del hecho, hora de captura, ubicación, odómetro, causa del catálogo, descripción y fotografías.

El evento **marca la misión como interrumpida** y **no le cambia el estado**. La Orden de Misión sigue `EN_RUTA`: el vehículo salió y hubo consumo real de recursos públicos.

Toda interrupción **debe** resolverse con un **desenlace explícito, tipificado y registrado**:

| Desenlace | Efecto |
|---|---|
| **Continuar** con el mismo vehículo y conductor | Se levanta la marca, con constancia de quién lo autorizó |
| **Continuar con sustitución** de vehículo o de conductor | [`RN-61`](RN-61-sustitucion-de-vehiculo-recalcula-valores-congelados.md) y [`RN-71`](RN-71-traspaso-en-ruta-con-acta-y-corte-de-odometro.md) |
| **Retorno anticipado** | `T-18` subtipo retorno anticipado ([`RN-78`](RN-78-grado-de-cumplimiento-del-objeto.md)) |
| **Retorno sin vehículo**, con la unidad resguardada o retenida en sitio | [`RN-75`](RN-75-bien-retenido-o-sustraido-no-sale-del-registro.md) |

**Ninguna misión con marca de interrupción sin desenlace puede quedar viva al cierre del período** ([`RN-97`](RN-97-saldo-de-apertura-de-control-interno.md)).

## Justificación

Hoy no existe forma de decir *"la misión se detuvo aquí y todavía no sabemos qué va a pasar"*. Las alternativas disponibles son falsear el estado —declararla retornada cuando el vehículo está en un taller de Danlí— o no registrar nada hasta que se resuelva, que es lo que ocurre en la práctica y deja un hueco de días en el expediente.

La marca separa dos cosas que no son lo mismo: **el hecho**, que ocurrió a una hora concreta y hay que registrar de inmediato, y **la decisión**, que puede tardar horas y depende de personas que no están en la carretera. Sin la separación, el registro del hecho queda rehén de la decisión.

`EN_RUTA → ANULADA` está expresamente prohibida por la [máquina de estados](../../03-arquitectura/estados/orden-de-mision.md), y con razón: anular sería borrar un hecho. La interrupción es la forma correcta de representar lo que efectivamente pasó.

## Condiciones de aplicación

Aplica a toda misión `DESPACHADA` o `EN_RUTA`.

**No aplica** a las esperas en sitio, que son parte normal de la operación multi-destino y se rigen por [`RN-76`](RN-76-estado-en-ruta-declarado-por-el-motorista.md).

**No aplica** a las paradas por descanso o alimentación, salvo que superen el umbral configurado y se declaren como tales.

## Comportamiento esperado

1. Ante ciertas causas —accidente con personas, sustracción— el cliente de campo **muestra la guía de actuación antes de cualquier formulario**, y el registro mínimo se puede diferir sin perderse. Primero se atiende; después se captura.
2. El evento se sincroniza en cuanto haya señal y **notifica** a ACT-04, a la jefatura de la delegación y a la dependencia solicitante, sin perderse si no la hay.
3. Según la causa, el evento abre expediente en **M-12** con responsable y plazo, y cambia el estado operativo del vehículo por `W-07` o `W-08` de la [máquina de estados](../../03-arquitectura/estados/orden-de-mision.md), **desde la hora del hecho** — no desde la hora de captura.
4. El desenlace lo decide y lo registra ACT-04, salvo la facultad del conductor de detener la misión por riesgo inmediato, que se convalida después ([`RN-73`](RN-73-convalidacion-de-actos-sin-autorizacion-previa.md)).
5. La liquidación se hace **por lo efectivamente ejecutado**, con imputación por tramo cuando hubo más de un vehículo o conductor ([`RN-72`](RN-72-imputacion-por-tramo-de-vehiculo-y-motorista.md)).
6. Un vehículo que no retorna por falta de conductor habilitado o por avería queda `NO_DISPONIBLE` con causa tipificada, **acta de resguardo** y obligación de recuperación con responsable y plazo.
7. La misión no cierra limpio con el incidente abierto: el camino es `T-22`, cierre con hallazgo, y el hallazgo **no imputa responsabilidad a nadie** — es marca de seguimiento.

## Casos límite

- **Interrupción sin señal durante días.** Es el caso normal y por eso el registro es offline-first ([`RN-43`](RN-43-captura-de-campo-sin-conectividad.md)). El desenlace también se puede registrar en el dispositivo y sincronizar después, con la constancia de que se decidió sin consultar.
- **`[C]` ¿Un incidente abierto impide cerrar la misión?** La máquina de estados deja abierto qué expedientes vinculados condicionan `T-21`. Opciones y costo:

  | Opción | Costo |
  |---|---|
  | Todo incidente abierto impide cerrar | Misiones abiertas durante años por procesos judiciales lentos; la liquidación económica queda rehén de algo que no es económico |
  | Solo impide cerrar el incidente con pérdida del bien o efecto económico no cuantificado | Más operativo, pero exige tipificar la severidad — se cruza con el insumo #35 |
  | Cierra siempre con hallazgo y el incidente sigue su vida propia | Es lo que la máquina ya permite; el riesgo es que *cerrada con hallazgo* se vuelva rutina |

  **Recomendación del análisis, no decisión:** la tercera.
- **Sustitución de vehículo con la misión en curso.** `T-17` cubre hoy prórroga, destino adicional y relevo de motorista, **no cambio de vehículo**. Reportado como ampliación necesaria a la [máquina de estados](../../03-arquitectura/estados/orden-de-mision.md), que es la autoridad. **Nota de hallazgo abierta.**
- **Interrupción que se resuelve sola** — la vía se abre a las dos horas. Se registra el desenlace *continuar* con su hora; la marca queda en el expediente y el tiempo perdido entra en el indicador.
- **Vehículo resguardado fuera de sede que nadie recupera.** La obligación de recuperación tiene responsable y plazo, y su incumplimiento entra al saldo de apertura del ejercicio siguiente ([`RN-97`](RN-97-saldo-de-apertura-de-control-interno.md)).

## Trazabilidad

- Autoridad: [orden-de-mision.md](../../03-arquitectura/estados/orden-de-mision.md) — `T-17`, `T-18` y subtipos, `T-21`, `T-22`, `W-07`, `W-08`; transición prohibida `EN_RUTA → ANULADA`
- Norma: [NRM-09](../normativa/NRM-09-realidad-operativa.md) `[V]`
- Reglas relacionadas: [RN-43](RN-43-captura-de-campo-sin-conectividad.md), [RN-46](RN-46-fecha-del-hecho-y-fecha-de-captura.md), [RN-61](RN-61-sustitucion-de-vehiculo-recalcula-valores-congelados.md), [RN-71](RN-71-traspaso-en-ruta-con-acta-y-corte-de-odometro.md), [RN-72](RN-72-imputacion-por-tramo-de-vehiculo-y-motorista.md), [RN-73](RN-73-convalidacion-de-actos-sin-autorizacion-previa.md), [RN-74](RN-74-sin-atribucion-de-responsabilidad-en-campo.md), [RN-75](RN-75-bien-retenido-o-sustraido-no-sale-del-registro.md), [RN-97](RN-97-saldo-de-apertura-de-control-interno.md)
- Casos especiales: [CE-02](../../02-requisitos/casos-especiales/CE-02-averia-mecanica-en-ruta.md) `RN-c:interrupcion-en-ruta`, `RN-c:mision-interrumpida-no-cierra-ejercicio` · [CE-10](../../02-requisitos/casos-especiales/CE-10-motorista-incapacitado-en-ruta.md) `RN-c:incapacidad-del-motorista-en-ruta`, `RN-c:vehiculo-resguardado-fuera-de-sede` · [CE-03](../../02-requisitos/casos-especiales/CE-03-accidente-de-transito-en-mision.md) · [CE-04](../../02-requisitos/casos-especiales/CE-04-robo-de-vehiculo-o-de-carga-en-mision.md)
- Insumos pendientes: #35 escala de severidad de fallas
