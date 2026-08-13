# RN-71 — Todo traspaso en ruta consta en acta con hora, lugar, odómetro e identidad de ambas partes; el odómetro del acta es el corte de imputación

| Campo | Valor |
|---|---|
| **Módulos** | M-08, M-09, M-05, M-03, M-15 |
| **Origen** | Casos especiales [CE-05](../../02-requisitos/casos-especiales/CE-05-cambio-de-motorista-con-la-mision-en-curso.md), [CE-10](../../02-requisitos/casos-especiales/CE-10-motorista-incapacitado-en-ruta.md), [CE-02](../../02-requisitos/casos-especiales/CE-02-averia-mecanica-en-ruta.md) · Normas [NRM-02](../normativa/NRM-02-bienes-del-estado.md) y [NRM-01](../normativa/NRM-01-control-interno-tsc.md) |
| **Verificación** | `[P]` el deber de custodia continua e identificable del bien y de los fondos. `[I]` el acta como instrumento y el corte de imputación: implicación de requerimiento del equipo |
| **Tipo** | Bloqueo duro |
| **Configurable** | No |

## Enunciado

Todo relevo de conductor con la misión en curso **debe** constar en **acta de traspaso** con: hora, lugar, **odómetro**, identidad de quien entrega y de quien recibe, y **motivo tipificado**. El odómetro del acta es el **corte de imputación** de kilometraje y de consumo entre ambos tramos ([`RN-72`](RN-72-imputacion-por-tramo-de-vehiculo-y-motorista.md)).

El **fondo de combustible** se traspasa únicamente por **acta propia**, con **conteo de folios uno por uno**, saldo enumerado y, cuando el receptor original no puede firmar, **dos testigos presentes**. Sin acta, el fondo **permanece a nombre del receptor original** y la liquidación se hace por asignación, no por persona presente.

La **custodia del vehículo se cierra siempre**, aunque el conductor no pueda firmar: consta el impedimento, y firman dos personas presentes más el receptor tipificado.

Un consumo imputado a un folio **ya traspasado** es **alerta automática**.

## Justificación

El relevo de motorista en carretera existe y es frecuente: una misión de siete horas continuas, un conductor que se incapacita, un apoyo desde la delegación más cercana. Lo que hoy no existe es el instrumento que dice **dónde termina la responsabilidad de uno y empieza la del otro**.

Sin corte de odómetro, el rendimiento y los indicadores de conducción se promedian sobre la misión completa y no dicen nada de ninguno de los dos conductores. Sin acta de fondo, el dinero cambia de mano sin registro y el arqueo pregunta por él a quien ya no lo tiene. Y sin cierre de custodia, hay un tramo en el que nadie responde por el vehículo — que es exactamente el tramo del que va a preguntar la auditoría si algo pasó.

Los dos testigos no son burocracia: son la única forma de cerrar una custodia cuando el custodio está inconsciente en una ambulancia, y esa situación ocurre.

## Condiciones de aplicación

Aplica a todo relevo de conductor, previsto o sobrevenido, y a todo traspaso de fondo entre personas durante la ejecución.

Aplica al **transbordo de la carga** en cuanto a su acta propia, regida por [`RN-69`](RN-69-inventario-de-la-carga-y-acta-de-entrega.md).

**No aplica** a la sustitución de motorista **antes de la salida**, que se resuelve por [`RN-14`](RN-14-sustitucion-de-motorista.md) sin acta de ruta.

## Comportamiento esperado

1. El acta se levanta en el dispositivo **sin conectividad**, con folio del rango de la delegación ([`RN-44`](RN-44-identificadores-y-folios-en-el-cliente.md)), y se imprime o se conserva en el dispositivo con su hash.
2. La habilitación del conductor entrante se revalida contra el paquete normativo congelado ([`RN-57`](RN-57-habilitacion-de-quien-efectivamente-conduce.md)). Si no está en el padrón del paquete, se registra con **evaluación diferida marcada**, foto de la licencia física y revalidación obligatoria al sincronizar; el fallo posterior produce hallazgo.
3. El paquete de misión que viaja en el dispositivo **incluye los datos mínimos de habilitación de los conductores de las delegaciones que toca la ruta**, para que la verificación se pueda hacer en campo sin red.
4. `ACT-07` convalida el traspaso de fondo al liquidar. El saldo y los folios enumerados del acta son la base del arqueo ([`RN-86`](RN-86-obligacion-de-reintegro-por-saldo-no-devuelto.md)).
5. Superado el umbral configurable de duración o distancia, el sistema **propone declarar conductor de relevo en la programación**, con habilitaciones validadas antes de la salida. El relevo previsto es siempre mejor que el improvisado.
6. Rendimiento e indicadores de conducción se calculan **por tramo de conductor**, nunca promediando la misión completa.

## Casos límite

- **`[C]` ¿Existe límite de jornada de conducción y quién lo controla?** Insumo #48. Sin respuesta, el sistema **mide y muestra** las horas al volante por tramo pero no bloquea. Opciones:

  | Opción | Costo |
  |---|---|
  | Solo medir y mostrar | No previene el accidente por fatiga, que es real en misiones de siete horas continuas |
  | Advertir al superar un umbral configurable | Exige fijar el umbral sin norma que lo respalde: sería `[I]`, no `[V]` |
  | Bloquear el despacho de rutas que exigen más horas continuas que el umbral, salvo relevo declarado | Produce el comportamiento correcto, pero condiciona la operación con un número que hoy nadie puede verificar |

  **Recomendación del análisis, no decisión:** la segunda hasta que exista el insumo #48, y la tercera después, con el umbral como parámetro con vigencia ([`RN-39`](RN-39-parametros-normativos-con-vigencia.md)).
- **Salvoconducto de día inhábil y relevo.** El permiso ampara **vehículo y ventana**, no motorista: el relevo no lo invalida. Es la [máquina de estados](../../03-arquitectura/estados/orden-de-mision.md) la que manda aquí; `PC-03` de [`PR-01`](../procesos/PR-01-movilizacion-institucional.md) dice otra cosa y debe alinearse. **Nota de hallazgo abierta.**
- **Conductor entrante que no pertenece al padrón** — un servidor de otra dependencia, en emergencia. Se admite con captura y evaluación de licencia, y desde ese momento le aplican las incompatibilidades de conductor de misión.
- **Traspaso de fondo sin testigos disponibles** en un tramo despoblado. Se registra el impedimento; el fondo **no cambia de responsable** y la liquidación se hace contra el receptor original. Es incómodo y es lo correcto: el dinero no cambia de dueño por conveniencia.
- **Incapacidad sobrevenida que llega del espejo** de Talento Humano con fecha de inicio coincidente con una misión ejecutada. Se vincula al evento en ruta correspondiente; sin evento que la explique, es **conflicto para resolución humana** ([`RN-45`](RN-45-cero-sobrescritura-silenciosa.md)).

## Trazabilidad

- Autoridad: [orden-de-mision.md](../../03-arquitectura/estados/orden-de-mision.md) — `T-17` relevo de motorista, `BD-02`, `BD-04`
- Normas: [NRM-02](../normativa/NRM-02-bienes-del-estado.md) `[P]`, [NRM-01](../normativa/NRM-01-control-interno-tsc.md) `[P]`
- Reglas relacionadas: [RN-14](RN-14-sustitucion-de-motorista.md), [RN-22](RN-22-custodia-del-vehiculo.md), [RN-27](RN-27-asignacion-de-combustible-con-folio.md), [RN-44](RN-44-identificadores-y-folios-en-el-cliente.md), [RN-57](RN-57-habilitacion-de-quien-efectivamente-conduce.md), [RN-69](RN-69-inventario-de-la-carga-y-acta-de-entrega.md), [RN-70](RN-70-interrupcion-en-ruta-con-desenlace-obligatorio.md), [RN-72](RN-72-imputacion-por-tramo-de-vehiculo-y-motorista.md), [RN-86](RN-86-obligacion-de-reintegro-por-saldo-no-devuelto.md)
- Casos especiales: [CE-05](../../02-requisitos/casos-especiales/CE-05-cambio-de-motorista-con-la-mision-en-curso.md) — candidatas `RN-c:acta-de-relevo-con-corte-de-odometro`, `RN-c:traspaso-de-fondo-entre-motoristas`, `RN-c:padron-de-relevo-en-el-paquete-de-mision`, `RN-c:relevo-previsto-en-mision-larga` · [CE-10](../../02-requisitos/casos-especiales/CE-10-motorista-incapacitado-en-ruta.md) `RN-c:traspaso-de-custodia-por-incapacidad`, `RN-c:traspaso-de-fondo-por-incapacidad-del-receptor`
- Insumos pendientes: #48 límite de jornada de conducción
