# HU-072 — Aprobar el fondo verificando la cuota trimestral de compromiso, no solo el presupuesto anual

| Campo | Valor |
|---|---|
| **Módulo** | M-09 Combustible |
| **Actor** | ACT-08 Gerencia Administrativa |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador |

## Historia

**Como** Gerencia Administrativa
**quiero** ver, antes de aprobar el fondo, el presupuesto anual de la partida **y** la cuota de compromiso del trimestre en que cae el acto
**para** no comprometer contra una partida que tiene saldo anual pero cuya cuota trimestral ya está copada, que es una gestión que después nadie puede destrabar en SIGTI

## Contexto

Tener saldo en la partida anual no significa que el compromiso quepa en el trimestre: SIAFI asigna cuotas trimestrales de compromiso y ese es el límite que efectivamente frena la ejecución. El error clásico es aprobar contra el anual, comprometer, y descubrir semanas después que la cuota no daba — momento en que la solución ya no está en la institución sino en una reprogramación ante SIAFI.

El trimestre aplicable se determina por la **fecha del hecho** que genera el compromiso, nunca por la fecha en que alguien capturó el registro.

## Reglas que la gobiernan

- [RN-54](../../01-negocio/reglas/RN-54-cuota-trimestral-de-compromiso.md) — Doble límite: presupuesto anual **y** cuota trimestral; la verificación se guarda con todos sus insumos
- [RN-26](../../01-negocio/reglas/RN-26-fondo-de-combustible-aprobado.md) — La aprobación registra monto, fecha, aprobador, partida, período y ámbito
- [RN-40](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md) — El trimestre se resuelve por la fecha del hecho
- [RN-46](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md) — Fecha del hecho ≠ fecha de captura, ambas obligatorias
- [RN-49](../../01-negocio/reglas/RN-49-reconciliacion-periodica-del-espejo.md) · [RN-50](../../01-negocio/reglas/RN-50-degradacion-por-sincronizacion-detenida.md) — Un saldo sin fecha de sincronización no es un saldo
- [RN-03](../../01-negocio/reglas/RN-03-registro-inmutable-de-autorizacion.md) — La aprobación es registro inmutable con autor, puesto, momento y huella
- [RN-39](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md) — `control_cuota_trimestral` es parámetro con vigencia y doble control

## Casos especiales que la afectan

- [CE-23](../casos-especiales/CE-23-fondo-agotado-con-misiones-programadas.md) — Distinguir el fondo agotado de la cuota copada: son problemas distintos con salidas distintas
- [CE-27](../casos-especiales/CE-27-cierre-de-ejercicio-fiscal-con-hallazgo-abierto.md) — Actos que cruzan el corte de trimestre o de ejercicio

## Criterios de aceptación

```gherkin
# language: es
Característica: Aprobación del fondo con verificación de cuota trimestral

  Antecedentes:
    Dado una solicitud de fondo "FND-2026-09-004" por "L 200,000.00" del ámbito "Gerencia Administrativa"
    Y una unidad ejecutora "Dirección Administrativa" con presupuesto anual disponible de "L 1,450,000.00"
    Y una cuota de compromiso del trimestre "2026-T3" de "L 400,000.00" con "L 355,000.00" ya comprometidos
    Y el parámetro "control_cuota_trimestral" en valor "advertir"

  Escenario: Se advierte el exceso de cuota nombrando el trimestre y el monto
    Cuando la Gerencia Administrativa abre la solicitud "FND-2026-09-004" para decidir
    Entonces el sistema muestra "Presupuesto anual disponible L 1,450,000.00"
    Y muestra "Excede en L 155,000.00 la cuota de compromiso del trimestre 2026-T3 para la unidad ejecutora Dirección Administrativa, según ARGOS al 12/09/2026 08:15."
    Y nunca muestra el texto "fondo agotado"

  Escenario: Se rechaza continuar sin acuse nominativo ni motivo escrito
    Cuando la Gerencia Administrativa intenta aprobar "FND-2026-09-004" sin motivo escrito
    Entonces el sistema rechaza la aprobación
    Y muestra "Aprobar por encima de la cuota del trimestre 2026-T3 exige acuse nominativo y motivo escrito, que quedan en el expediente del fondo."

  Escenario: Se aprueba por encima de cuota con acuse y el acto entra al reporte
    Cuando la Gerencia Administrativa aprueba "FND-2026-09-004" con acuse nominativo y motivo "Operativo de verificación migratoria del 20 al 30 de septiembre"
    Entonces el fondo queda aprobado por "L 200,000.00"
    Y el acto figura en el reporte por unidad ejecutora y trimestre como "aprobado por encima de cuota"
    Y el reporte muestra cuota "L 400,000.00", comprometido "L 355,000.00" y monto del acto "L 200,000.00"

  Escenario: Se guarda la verificación con todos sus insumos, no solo el resultado
    Cuando la Gerencia Administrativa aprueba "FND-2026-09-004"
    Entonces el expediente del fondo conserva cuota consultada "L 400,000.00", saldo comprometido "L 355,000.00", monto del acto "L 200,000.00", trimestre "2026-T3", fecha de sincronización del espejo "12/09/2026 08:15" y resultado "excede"
    Y no se admite guardar únicamente la palabra "verificado"

  Escenario: El trimestre se determina por la fecha del hecho, no por la de captura
    Dado un acto con fecha del hecho "2026-09-29" capturado el "2026-10-03"
    Cuando la Gerencia Administrativa aprueba ese acto
    Entonces el sistema verifica la cuota del trimestre "2026-T3"
    Y muestra "Trimestre aplicable 2026-T3, por fecha del hecho 29/09/2026. Fecha de captura 03/10/2026."

  Escenario: El dato de cuota no está disponible en el espejo
    Dado que el espejo de ARGOS no expone la cuota del trimestre "2026-T4"
    Cuando la Gerencia Administrativa aprueba un fondo con fecha del hecho "2026-10-15"
    Entonces el sistema registra la verificación como "no realizada" con causa "cuota no disponible en el espejo"
    Y muestra "Verificación de cuota no realizada: el dato no está disponible. Último dato de ARGOS: 12/09/2026 08:15."
    Y el acto continúa

  Escenario: Se aprueba dentro de cuota
    Dado una solicitud de fondo "FND-2026-10-005" por "L 40,000.00"
    Y una cuota del trimestre "2026-T4" de "L 400,000.00" con "L 60,000.00" comprometidos
    Cuando la Gerencia Administrativa aprueba "FND-2026-10-005"
    Entonces el fondo queda aprobado
    Y el resultado de la verificación de cuota se registra como "dentro de cuota"
```

## Fuera de alcance

- La solicitud del fondo — es [HU-071](HU-071-solicitar-fondo-de-combustible-del-periodo.md)
- El bloqueo por segregación solicita × aprueba — es [HU-073](HU-073-impedir-que-quien-solicita-el-fondo-lo-apruebe.md)
- La reprogramación de cuota ante SIAFI: se gestiona fuera de SIGTI; el sistema solo produce el reporte que la sustenta
- El cálculo de la ejecución presupuestaria: es de ARGOS ([DP-001 D-09](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md))

## Notas y pendientes

- `[V]` Que SIAFI asigna cuotas trimestrales de compromiso — [NRM-04](../../01-negocio/normativa/NRM-04-presupuesto-siafi.md)
- `[I]` Que SIGTI deba validar contra esas cuotas es **implicación de requerimiento del equipo**, no articulado citable. No se eleva a `[V]`
- `[C]` **¿ARGOS expone cuota y comprometido del trimestre por unidad ejecutora?** De eso depende que `control_cuota_trimestral` pueda pasar de *advertir* a *bloquear* — insumo **#16**
- `[C]` Correspondencia entre delegaciones y unidades ejecutoras — insumo **#27**
- `control_cuota_trimestral` y `tolerancia_sobregiro` **no se apagan "por esta vez"**: los carga ACT-01 con respaldo y los pone en vigencia ACT-08 ([RN-39](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md))
