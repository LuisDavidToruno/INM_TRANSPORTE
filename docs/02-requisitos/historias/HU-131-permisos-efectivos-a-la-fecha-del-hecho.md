# HU-131 — Resolver los permisos efectivos a la fecha del hecho, y dejar sin permisos a quien no tiene puesto vigente

| Campo | Valor |
|---|---|
| **Módulo** | M-01 Organización y Seguridad |
| **Actor** | ACT-01 Administrador del Sistema · todos los actores como sujetos |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — necesita la regla candidata de permisos por puesto vigente, que hoy no existe |

## Historia

**Como** Administrador del Sistema
**quiero** que los permisos efectivos de un usuario se calculen como la unión de los roles de todos sus puestos **vigentes a la fecha del hecho**, y que quien no tenga ningún puesto vigente quede sin ninguna facultad
**para** que la revocación al causar baja sea automática y no dependa de que alguien se acuerde de desactivar la cuenta

## Contexto

Es la consecuencia directa de [HU-129](HU-129-otorgar-rol-al-puesto-con-alcance-y-vigencia.md), y es donde el modelo se gana o se pierde. [actores-y-roles §2.2](../../01-negocio/actores-y-roles.md) —autoridad— lo enuncia así: *"Los permisos efectivos de un usuario, en una fecha dada, son la unión de los roles de todos los puestos que esa persona ocupa vigentes a esa fecha. No hay permisos otorgados directamente a una persona. Sin excepción."*

Y hay un matiz que decide la operación en campo: **la fecha del hecho no es la fecha de captura**. Una bitácora digitada en diferido el 20 de septiembre sobre un hecho del 5 de septiembre debe validarse contra los permisos que el digitador tenía **el 5**, no contra los de hoy. Es la misma distinción de [`RN-46`](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md) llevada al control de acceso.

Y **una persona sin puesto vigente no se borra**: sus actos históricos la referencian.

## Reglas que la gobiernan

- [RN-46](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md) — Fecha del hecho y fecha de captura son campos distintos; la validación usa la del hecho
- [RN-01](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md) — La matriz se evalúa contra la identidad de quien **actuó**, congelada en el momento del acto, no contra el puesto que ocupa hoy
- [RN-03](../../01-negocio/reglas/RN-03-registro-inmutable-de-autorizacion.md) — El acto registra la competencia con que se ejerció
- [RN-05](../../01-negocio/reglas/RN-05-registro-cerrado-no-se-edita.md) — Los registros anteriores a la asignación son consultables dentro del alcance, nunca editables si el expediente está cerrado
- [RN-48](../../01-negocio/reglas/RN-48-datos-espejo-de-solo-lectura.md) — La baja de la persona llega del espejo de Talento Humano y no se declara en SIGTI

## Casos especiales que la afectan

- [CE-09](../casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md) — La digitación diferida obliga a validar contra los permisos de la fecha del hecho
- [CE-05](../casos-especiales/CE-05-cambio-de-motorista-con-la-mision-en-curso.md) — El relevo ocurre con la misión en curso y la asignación del saliente puede cerrarse antes del retorno

## Criterios de aceptación

```gherkin
# language: es
Característica: Permisos efectivos resueltos a la fecha del hecho

  Antecedentes:
    Dada una persona "María López" con asignación al puesto "Encargado de Transporte de la Delegación de Choluteca" del "2026-01-01" al "2026-09-30"
    Y ese puesto con los roles "ACT-02 Solicitante" y "ACT-05 Encargado de Despacho" vigentes en esa ventana
    Y una persona "Ramón Cáceres" sin ninguna asignación de puesto vigente

  Escenario: Se rechaza toda operación a quien no tiene puesto vigente
    Cuando "Ramón Cáceres" intenta abrir el tablero de misiones el "2026-10-05"
    Entonces el sistema rechaza el acceso
    Y muestra "Usted no ocupa ningún puesto vigente al 05/10/2026. Sin puesto vigente no hay permisos. Consulte con la unidad de informática de la institución."
    Y no se elimina su usuario ni su identidad

  Escenario: Se rechaza la operación un día después del fin de la asignación
    Cuando "María López" intenta registrar un despacho el "2026-10-01"
    Entonces el sistema rechaza la operación
    Y muestra "Su asignación al puesto Encargado de Transporte de la Delegación de Choluteca terminó el 30/09/2026. El rol ACT-05 Encargado de Despacho ya no está vigente para usted."

  Escenario: Se rechaza la digitación diferida de un hecho anterior a la asignación
    Dada una asignación de "Carlos Fúnez" a un puesto con rol "ACT-05" desde el "2026-09-15"
    Cuando "Carlos Fúnez" digita el "2026-09-20" un despacho ocurrido el "2026-09-05"
    Entonces el sistema rechaza la digitación
    Y muestra "El 05/09/2026 usted no ocupaba ningún puesto con facultad de despachar. La digitación diferida se valida contra la fecha del hecho, no contra la de captura."

  Escenario: La digitación diferida dentro de la ventana de la asignación se acepta
    Cuando "María López" digita el "2026-09-28" una bitácora de un hecho ocurrido el "2026-09-05"
    Entonces el sistema acepta la digitación
    Y registra "ocurrido_en" el "2026-09-05" y "capturado_en" el "2026-09-28"
    Y el asiento cita el puesto que ella ocupaba el "2026-09-05"

  Escenario: Los permisos son la unión de los roles de todos los puestos vigentes
    Dada una segunda asignación de "María López" al puesto "Custodio de vehículos" con rol "ACT-13 Custodio del Vehículo" del "2026-03-01" al "2026-09-30"
    Cuando "María López" opera el "2026-06-15"
    Entonces sus permisos efectivos son la unión de "ACT-02", "ACT-05" y "ACT-13"
    Y su alcance de datos es el mayor otorgado por cada tipo de objeto, no el mayor global

  Escenario: La baja en Talento Humano revoca el acceso sin borrar la identidad
    Dada una baja de "María López" registrada en Talento Humano el "2026-08-31"
    Cuando el evento llega al espejo y el sistema evalúa sus asignaciones
    Entonces el sistema cierra sus asignaciones de puesto al "2026-08-31"
    Y su acceso queda revocado
    Y sus 214 asientos históricos conservan su nombre y el puesto que ocupaba en cada uno
    Y notifica al puesto responsable de la baja los expedientes que quedan pendientes

  Escenario: El usuario activo sin puesto vigente aparece en la revisión diaria
    Cuando el sistema ejecuta la revisión diaria de accesos el "2026-10-06"
    Entonces "Ramón Cáceres" figura en la lista de cuentas activas sin puesto vigente
    Y la lista es visible en la pantalla de estado del sistema y exportable para auditoría
```

## Fuera de alcance

- La verificación del alcance de datos en la resolución de cada consulta — es [HU-132](HU-132-alcance-de-datos-verificado-en-cada-consulta.md)
- La revocación de un rol antes de que termine la asignación — es [HU-133](HU-133-revocar-rol-sin-invalidar-actos-ejecutados.md)
- El cierre de la asignación con custodias y expedientes abiertos — es [HU-136](HU-136-cerrar-asignacion-de-puesto-con-acta.md)
- La autenticación en el dispositivo de campo sin red — es [HU-046](HU-046-operar-la-mision-sin-conectividad.md)

## Notas y pendientes

- `[C]` **Tiempo máximo entre la baja del servidor y la revocación efectiva de su acceso.** [`RNF-14`](../no-funcionales/RNF-14-control-de-acceso-por-puesto-y-registro-de-consultas.md) propone ≤ 1 día hábil marcado `[C]` — insumo **#27**
- `[C]` **¿Puede digitar formularios en papel quien después liquida esa misma misión?** — insumo **#47**
- `[I]` La resolución de permisos a la fecha del hecho es derivación de [actores-y-roles §2.2](../../01-negocio/actores-y-roles.md) y de `RN-46`, no articulado normativo
- **Regla candidata:** *Los permisos efectivos se calculan como la unión de los roles de los puestos vigentes a la **fecha del hecho**; una persona sin puesto vigente carece de toda facultad y no se elimina.* Es la candidata 1 de [actores-y-roles §8](../../01-negocio/actores-y-roles.md) y **ninguna de las 97 reglas la recoge**
