# RN-97 — Los expedientes no terminales y los hallazgos abiertos al corte constituyen el saldo de apertura de control interno del ejercicio siguiente

| Campo | Valor |
|---|---|
| **Módulos** | M-14, M-13, M-12, M-03 |
| **Origen** | Casos especiales [CE-27](../../02-requisitos/casos-especiales/CE-27-cierre-de-ejercicio-fiscal-con-hallazgo-abierto.md) y [CE-02](../../02-requisitos/casos-especiales/CE-02-averia-mecanica-en-ruta.md) · Normas [NRM-01](../normativa/NRM-01-control-interno-tsc.md) y [NRM-04](../normativa/NRM-04-presupuesto-siafi.md) |
| **Verificación** | `[P]` la exigencia de seguimiento de los hallazgos y observaciones — [NRM-01](../normativa/NRM-01-control-interno-tsc.md). `[I]` el saldo de apertura como instrumento: implicación de requerimiento del equipo |
| **Tipo** | Bloqueo duro |
| **Configurable** | No |

## Enunciado

Todo expediente **no terminal** y todo **hallazgo abierto** al corte del ejercicio —Órdenes de Misión sin cerrar, interrupciones sin desenlace, préstamos vencidos, obligaciones de reintegro, expedientes de M-12, reclamos de peaje sin resolver, imputaciones externas no resueltas, misiones con bitácora pendiente de digitación— **debe** integrar el **saldo de apertura de control interno del ejercicio siguiente**, con:

1. **Responsable nominado**
2. **Causa tipificada**
3. **Antigüedad contada desde el hecho original**, que **no se reinicia con el cambio de ejercicio**

El saldo de apertura **debe coincidir renglón por renglón** con el inventario de expedientes no terminales al corte ([`RN-96`](RN-96-cierre-de-ejercicio-como-corte-de-imputacion.md)).

**Ningún período se cierra con préstamos vencidos ni con interrupciones sin desenlace.**

## Justificación

Esta es la regla que impide el abandono, y **no existe**. [`RN-08`](RN-08-cadena-de-trazabilidad-para-cierre.md) resuelve el expediente individual —cuándo se puede cerrar y cuándo se cierra con hallazgo— pero **nada resuelve el inventario de lo que queda vivo al cambiar de año**.

Sin saldo de apertura, el mecanismo de olvido es automático y no requiere mala fe: llega enero, el sistema arranca con reportes en cero, y una misión interrumpida en noviembre, un préstamo vencido en agosto y una obligación de reintegro de mayo simplemente dejan de aparecer en ninguna pantalla. Nadie decidió abandonarlos: se abandonaron solos.

Contar la antigüedad desde el hecho original y no desde el corte es la parte que hace incómoda a la regla, y por eso mismo es la que sirve: un expediente que llega al tercer ejercicio con 800 días de antigüedad **no se puede presentar como pendiente reciente**.

## Condiciones de aplicación

Aplica al cierre de cada ejercicio fiscal y a cualquier corte periódico de control interno que la institución defina.

**No aplica** a los expedientes cerrados, aunque hayan cerrado con hallazgo: ahí el hallazgo es marca de seguimiento del expediente cerrado y su vida sigue por [`RN-93`](RN-93-expediente-de-hallazgo-posterior.md) si reaparece.

## Comportamiento esperado

1. El saldo de apertura se produce como **documento con folio**, junto al acta de cierre, y ambos se conservan.
2. Cada renglón lleva: tipo de expediente, referencia, fecha del hecho original, antigüedad en días, causa tipificada, responsable nominado y estado.
3. La **antigüedad se acumula entre ejercicios**. Un renglón que aparece en tres saldos de apertura consecutivos es visible como tal.
4. El sistema **impide el cierre del período** con préstamos vencidos ([`RN-63`](RN-63-prestamo-de-vehiculo-como-expediente-del-bien.md)) y con misiones marcadas como interrumpidas sin desenlace ([`RN-70`](RN-70-interrupcion-en-ruta-con-desenlace-obligatorio.md)): se listan con responsable y plazo, y hay que resolverlos o declararlos explícitamente.
5. El saldo de apertura se **reporta a la Gerencia Administrativa y a Auditoría Interna** al inicio del ejercicio, con su serie histórica.
6. Todo renglón que se resuelve durante el ejercicio se marca con su fecha de resolución; el **residuo** al cierre siguiente es el nuevo saldo.

## Casos límite

- **Renglón cuyo responsable ya no trabaja en la institución.** No se borra ni se deja sin responsable: se reasigna a la jefatura que corresponde, con constancia del motivo del cambio. Un expediente sin responsable es un expediente muerto.
- **Renglón que no depende de la institución** — un proceso judicial, una resolución de otra entidad. Se mantiene con causa tipificada *fuera del control institucional* y su antigüedad sigue corriendo. Que no dependa de nosotros no lo hace inexistente.
- **Vehículo sustraído o retenido sin recuperar.** Permanece en el saldo hasta su recuperación o su descargo formal ([`RN-75`](RN-75-bien-retenido-o-sustraido-no-sale-del-registro.md)).
- **Saldo de apertura muy grande en el primer ejercicio** tras el despliegue del sistema. Es esperable: es la primera vez que la institución ve todo junto. Se declara como **saldo inicial de implantación**, con su fecha, y a partir de ahí se compara contra sí mismo.
- **Presión por vaciar el saldo antes del cierre.** Vaciar el saldo cerrando expedientes en bloque está impedido por [`RN-96`](RN-96-cierre-de-ejercicio-como-corte-de-imputacion.md) y por la evaluación individual de [`RN-08`](RN-08-cadena-de-trazabilidad-para-cierre.md); y el reporte de cambios de parámetros en la ventana de cierre expone la otra vía.

## Trazabilidad

- Normas: [NRM-01](../normativa/NRM-01-control-interno-tsc.md) `[P]` · [NRM-04](../normativa/NRM-04-presupuesto-siafi.md) `[I]` en cuanto al cierre de ejercicio
- Reglas relacionadas: [RN-08](RN-08-cadena-de-trazabilidad-para-cierre.md), [RN-63](RN-63-prestamo-de-vehiculo-como-expediente-del-bien.md), [RN-66](RN-66-imputacion-externa-por-jerarquia-de-anclas.md), [RN-70](RN-70-interrupcion-en-ruta-con-desenlace-obligatorio.md), [RN-75](RN-75-bien-retenido-o-sustraido-no-sale-del-registro.md), [RN-79](RN-79-el-retorno-constatado-libera-al-vehiculo.md), [RN-86](RN-86-obligacion-de-reintegro-por-saldo-no-devuelto.md), [RN-92](RN-92-reclamo-por-discrepancia-de-peaje.md), [RN-93](RN-93-expediente-de-hallazgo-posterior.md), [RN-96](RN-96-cierre-de-ejercicio-como-corte-de-imputacion.md)
- Casos especiales: [CE-27](../../02-requisitos/casos-especiales/CE-27-cierre-de-ejercicio-fiscal-con-hallazgo-abierto.md) `RN-C27d` · [CE-02](../../02-requisitos/casos-especiales/CE-02-averia-mecanica-en-ruta.md) `RN-c:mision-interrumpida-no-cierra-ejercicio` · [CE-14](../../02-requisitos/casos-especiales/CE-14-vehiculo-prestado-entre-dependencias-o-instituciones.md) `RN-c:prestamo-vencido-no-devuelto`
