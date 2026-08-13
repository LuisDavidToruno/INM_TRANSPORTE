# RNF-14 — Los permisos se asignan a puestos, el alcance de datos se verifica en cada consulta, y toda consulta a datos personales queda registrada

| Campo | Valor |
|---|---|
| **Categoría** | Seguridad / Auditoría |
| **Prioridad** | Crítico |
| **Origen** | [`actores-y-roles.md`](../../01-negocio/actores-y-roles.md) — autoridad sobre alcance de datos; [NRM-07](../../01-negocio/normativa/NRM-07-transparencia-y-datos-personales.md) — registro de consultas exigible por el MARCI; [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md) — segregación de funciones como bloqueo duro |
| **Afecta arquitectura** | **Sí** — el alcance de datos verificado en cada consulta no se resuelve con filtros en la pantalla |

## Enunciado

El sistema **debe** asignar permisos a **puestos**, nunca a personas. Una persona tiene permisos porque ocupa un puesto; al dejarlo, los pierde el mismo día.

Toda consulta **debe** verificarse contra el **alcance de datos** del puesto —propio, dependencia, delegación o institución— definido en la tabla de [`actores-y-roles.md`](../../01-negocio/actores-y-roles.md) §alcance. La verificación ocurre en la resolución de la consulta, no en la construcción de la pantalla: si el filtro está solo en la interfaz, basta conocer un identificador para saltarlo.

Toda consulta a **manifiestos de personas externas** deja registro: quién vio qué lista, cuándo y desde dónde ([`RN-52`](../../01-negocio/reglas/RN-52-registro-de-consultas-a-manifiestos.md)). Este registro no se borra y no es una opción de configuración.

Y la **segregación de funciones** del MARCI es un bloqueo duro, no una advertencia: quien solicita ≠ quien autoriza ≠ quien despacha ≠ quien entrega combustible ≠ quien liquida.

## Métrica y umbral

| Métrica | Umbral |
|---|---|
| Permisos asignados directamente a una persona | **0.** El modelo no ofrece la operación |
| Consultas que devuelven datos fuera del alcance del puesto | **0**, incluido el acceso directo por identificador |
| Cobertura de la matriz de pruebas puesto × operación | **100 %.** Cada celda *permitido* y cada celda *denegado* tiene su prueba automatizada |
| Consultas a manifiestos de personas externas sin registro | **0** |
| Campos del registro de consulta | Quién (usuario y puesto), qué manifiesto, cuándo, desde dónde, con qué motivo si el puesto lo requiere |
| Registro de consultas modificable o borrable | **0** — forma parte de la cadena del [`RNF-04`](RNF-04-bitacora-append-only-con-hash-encadenado.md) |
| Violaciones de segregación de funciones que el sistema permita ejecutar | **0.** Se bloquea antes de la operación, no se reporta después |
| Tiempo entre la baja o traslado de un servidor y la revocación efectiva de su acceso | ≤ 1 día hábil `[C]` insumo #27 — el disparador es el evento espejeado de Talento Humano ([`RNF-07`](RNF-07-sincronizacion-del-espejo-local.md)) |
| Cuentas activas sin puesto vigente asociado | **0**, detectado por revisión automática diaria y expuesto en la pantalla de estado |
| Cuentas genéricas o compartidas entre personas | **0.** Una cuenta compartida anula la autoría de todo asiento que produce |
| Sobrecosto de la verificación de alcance en el tiempo de respuesta | ≤ 15 % sobre los umbrales de [`RNF-01`](RNF-01-rendimiento-de-consulta-y-operacion.md) |
| Régimen de excepción a la segregación en delegaciones pequeñas | **No implementado hasta que el insumo #26 tenga pronunciamiento de Auditoría Interna.** Ver más abajo |

## Cómo se verifica

1. **Matriz automatizada puesto × operación**: se genera un usuario por cada `ACT-xx` y se intenta cada operación del sistema. El resultado se compara con la tabla de [`actores-y-roles.md`](../../01-negocio/actores-y-roles.md). Toda discrepancia es defecto bloqueante, en ambos sentidos: permitir de más y denegar de más.
2. **Prueba de escalada horizontal**: un usuario con alcance en la delegación A obtiene el identificador de una misión de la delegación B y lo pide directamente, sin pasar por la pantalla. Debe recibir denegación, no un resultado vacío ni un error técnico.
3. **Prueba de segregación**: se intenta ejecutar, con la misma persona, dos funciones incompatibles sobre la misma misión —solicitar y autorizar, despachar y liquidar—. Debe bloquearse indicando qué incompatibilidad se viola y quién sí puede hacerlo.
4. **Prueba de rotación**: se da de baja a un servidor en Talento Humano. Se cronometra hasta que su acceso queda revocado, y se verifica que sus asientos históricos conservan su autoría y su puesto de entonces ([`RNF-15`](RNF-15-continuidad-ante-rotacion-de-personal.md)).
5. **Prueba del registro de consultas**: se consultan 10 manifiestos con distintos puestos. Se verifica que los 10 aparecen en el registro, que el registro es consultable por el responsable de control interno, y que ningún puesto —incluido el administrador— puede borrar una entrada.
6. **Prueba del administrador curioso**: el usuario administrador del sistema (`ACT-01`) intenta abrir un manifiesto de pasajeros. Debe quedar registrado como acceso de diagnóstico y aparecer en el reporte de accesos excepcionales.

## La restricción que hay que decir de frente

El MARCI exige cinco funciones incompatibles, lo que implica **un mínimo de cinco personas por misión**. Una delegación de tres personas no puede cumplirlo por aritmética, y eso no lo arregla el software.

Este `RNF` **no inventa un régimen de excepción**. Mientras el insumo #26 no tenga pronunciamiento formal de Auditoría Interna, el sistema bloquea, y las delegaciones pequeñas operan con funciones respaldadas desde la sede — lo que exige el mapa de dotación real del insumo #27.

**La consecuencia práctica hay que aceptarla o resolverla, no ignorarla:** si ninguno de los dos insumos llega, las delegaciones pequeñas no van a poder despachar una misión dentro del sistema. Ese es un riesgo de despliegue, no un detalle de configuración.

## Consecuencia de no cumplirlo

- **Si los permisos son por persona**: tras el cambio de administración —Honduras lo tuvo en enero de 2026 `[V]`— nadie sabe quién tiene qué. Los permisos se acumulan, se heredan al reemplazo "para que pueda trabajar", y en un año el alcance de datos no significa nada.
- **Si el alcance no se verifica en la consulta**: una delegación ve los manifiestos de personas externas de otra. Ahí ya no hay un defecto técnico, hay una fuga.
- **Si la segregación es advertencia y no bloqueo**: se vuelve un aviso que todos aprenden a cerrar, y el hallazgo del TSC es idéntico al de no tener sistema — con el agravante de que el sistema documenta que la violación se advirtió y se ejecutó igual.

## Trazabilidad

- Módulos: M-01, M-14, M-17
- Autoridad: [`actores-y-roles.md`](../../01-negocio/actores-y-roles.md) (actores, incompatibilidades, matriz de permisos, alcance de datos)
- Reglas: [`RN-01`](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md), [`RN-02`](../../01-negocio/reglas/RN-02-escalamiento-de-autorizacion.md), [`RN-07`](../../01-negocio/reglas/RN-07-delegacion-de-autorizacion.md), [`RN-51`](../../01-negocio/reglas/RN-51-minimizacion-de-datos-de-personas-externas.md), [`RN-52`](../../01-negocio/reglas/RN-52-registro-de-consultas-a-manifiestos.md)
- Normativa: [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md), [NRM-07](../../01-negocio/normativa/NRM-07-transparencia-y-datos-personales.md)
- Requisitos relacionados: [`RNF-04`](RNF-04-bitacora-append-only-con-hash-encadenado.md), [`RNF-13`](RNF-13-cifrado-en-transito-y-en-reposo.md), [`RNF-15`](RNF-15-continuidad-ante-rotacion-de-personal.md)
- Insumos: #26 (excepción a segregación con controles compensatorios), #27 (dotación real de delegaciones), #30 (conflicto de interés)
