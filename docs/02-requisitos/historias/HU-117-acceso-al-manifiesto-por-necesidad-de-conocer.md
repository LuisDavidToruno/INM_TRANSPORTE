# HU-117 — Restringir el acceso al manifiesto por rol, por ámbito y por necesidad de conocer

| Campo | Valor |
|---|---|
| **Módulo** | M-17 Traslado de Personas Externas · M-01 Organización y Seguridad |
| **Actor** | ACT-01 Administrador del Sistema · ACT-03 Jefatura Inmediata |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Refinada |

## Historia

**Como** Administrador del Sistema
**quiero** que las listas de pasajeros solo sean visibles para quien las necesita para operar, y dentro de su propio ámbito
**para** que la institución pueda afirmar, con prueba, que los datos de las personas trasladadas no están al alcance de cualquiera con usuario en el sistema

## Contexto

[NRM-07](../../01-negocio/normativa/NRM-07-transparencia-y-datos-personales.md), aun después de la reducción de alcance de [DP-001 D-14](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md), conserva expresamente el **control de acceso por necesidad de conocer** sobre listas de pasajeros. `[V]` que el MARCI lo exige de todas formas, con independencia de la ley de datos personales que sigue pendiente en el Congreso.

El error frecuente es confundir *rol* con *ámbito*. Un Jefe de Transporte de la Delegación de Choluteca tiene el rol correcto para ver manifiestos — pero no los de la Delegación de Danlí. [RN-52](../../01-negocio/reglas/RN-52-registro-de-consultas-a-manifiestos.md) numeral 1 lo dice sin rodeos: *"el acceso se concede por rol **y** por ámbito"*.

Y hay un caso incómodo que la regla resuelve de frente: **ni siquiera el Administrador del Sistema queda fuera.** Su alcance de datos es *"institución, solo metadatos y configuración; sin acceso al contenido de negocio salvo diagnóstico registrado"* ([actores-y-roles.md](../../01-negocio/actores-y-roles.md)).

## Reglas que la gobiernan

- [RN-52](../../01-negocio/reglas/RN-52-registro-de-consultas-a-manifiestos.md) — **Regla rectora**: acceso por rol y por necesidad de conocer; **ningún rol, incluido ACT-01, consulta sin dejar rastro**
- [RN-51](../../01-negocio/reglas/RN-51-minimizacion-de-datos-de-personas-externas.md) — Separación estructural: los datos de gestión pública no están sujetos a esta restricción
- [RN-11](../../01-negocio/reglas/RN-11-restricciones-medicas-del-motorista.md) — Las restricciones médicas del motorista entran al mismo régimen, por ser datos de salud

## Requisitos no funcionales relacionados

- [RNF-14](../no-funcionales/RNF-14-control-de-acceso-por-puesto-y-registro-de-consultas.md) — Control de acceso por puesto y registro de consultas
- [RNF-13](../no-funcionales/RNF-13-cifrado-en-transito-y-en-reposo.md) — Cifrado en reposo de los datos personales

## Casos especiales que la afectan

- [CE-18](../casos-especiales/CE-18-carga-y-pasajeros-en-la-misma-mision.md) — El manifiesto mixto: personal de la institución y personas externas con regímenes de acceso distintos

## Criterios de aceptación

> Los nombres de personas externas de estos escenarios son **ficticios de prueba**.

```gherkin
# language: es
Característica: Acceso al manifiesto por rol, ámbito y necesidad de conocer

  Antecedentes:
    Dado una Orden de Misión "OM-2026-0451" originada por la "Delegación de Choluteca"
    Y un manifiesto cerrado con "3" personas externas
    Y que el despacho de esa misión lo atendió el predio de la "Delegación de Choluteca"

  Escenario: Se deniega el acceso a una jefatura de otra dependencia
    Dado una Jefatura Inmediata con ámbito "Delegación de Danlí"
    Cuando esa Jefatura intenta abrir el manifiesto de "OM-2026-0451"
    Entonces el sistema deniega el acceso
    Y muestra "El manifiesto pertenece a la Delegación de Choluteca y su ámbito es la Delegación de Danlí. Si necesita consultarlo, solicítelo con fundamento."
    Y registra el intento denegado con identidad, rol, fecha y hora

  Escenario: Se deniega el acceso a un rol sin necesidad de conocer
    Dado un Encargado de Mantenimiento con ámbito "institución, acotado al objeto vehículo y mantenimiento"
    Cuando el Encargado de Mantenimiento intenta abrir el manifiesto de "OM-2026-0451"
    Entonces el sistema deniega el acceso
    Y muestra "Su puesto no requiere las listas de pasajeros para operar. Puede consultar el vehículo, su estado y su mantenimiento."
    Y registra el intento denegado

  Escenario: Se deniega al Administrador del Sistema el acceso sin motivo de diagnóstico
    Cuando el Administrador del Sistema intenta abrir el manifiesto de "OM-2026-0451"
    Entonces el sistema deniega el acceso
    Y muestra "Su puesto administra puestos, roles, catálogos y parámetros, no contenido de negocio. Para acceder por diagnóstico, registre el motivo: quedará en el registro de consultas y en el reporte del Auditor Interno."

  Escenario: El acceso de diagnóstico queda registrado y acotado
    Cuando el Administrador del Sistema registra el motivo de diagnóstico "incidencia 2026-334: manifiesto no se imprime" y accede al manifiesto de "OM-2026-0451"
    Entonces el sistema concede el acceso
    Y registra la consulta con el motivo, la identidad, el rol, la fecha, la hora y el alcance "COMPLETO"
    Y la incluye en el reporte de accesos de diagnóstico visible para el Auditor Interno

  Escenario: El conteo de pasajeros es visible con un alcance menor
    Dado un Jefe de Transporte con ámbito "institución"
    Cuando el Jefe de Transporte consulta la ocupación de "OM-2026-0451" para programar
    Entonces el sistema muestra "3 personas externas y 2 servidores, 6 ocupantes con el motorista"
    Y no muestra ninguna identidad
    Y registra la consulta con alcance "CONTEO"

  Escenario: El Encargado de Despacho del predio que atiende sí ve el manifiesto completo
    Dado un Encargado de Despacho con ámbito "Delegación de Choluteca, acotado a despachos del predio que atiende"
    Cuando el Encargado de Despacho abre el manifiesto de "OM-2026-0451"
    Entonces el sistema concede el acceso
    Y muestra las "3" fichas con identificación, institución o condición, origen y destino
    Y registra la consulta con alcance "COMPLETO"

  Escenario: Los datos de gestión pública no quedan restringidos por esta regla
    Dado un Solicitante de otra dependencia
    Cuando el Solicitante consulta la ficha pública de "OM-2026-0451"
    Entonces el sistema muestra vehículo, ruta, objeto del viaje, unidad ejecutora y costo
    Y no muestra ninguna identidad de persona trasladada
    Y no registra la consulta en el registro de accesos a manifiestos
```

## Fuera de alcance

- El registro de la consulta propiamente dicho y su contenido — es [HU-118](HU-118-registrar-cada-consulta-al-manifiesto.md)
- El reporte de accesos y la alerta de patrón anómalo — es [HU-119](HU-119-reporte-de-accesos-y-alerta-de-patron-anomalo.md)
- El acceso desde el dispositivo de campo sin conectividad — es [HU-120](HU-120-consultar-el-manifiesto-sin-conectividad.md)
- La matriz general de permisos por puesto: la gobierna [actores-y-roles.md](../../01-negocio/actores-y-roles.md), que es la autoridad en la materia. Esta historia **la aplica, no la redefine**

## Notas y pendientes

- `[C]` Cuál es el **procedimiento institucional** para conceder acceso con fundamento a una jefatura fuera de su ámbito: ¿quién lo autoriza y por cuánto tiempo? Hoy no hay regla que lo cubra
- `[C]` **Quién administra el servidor on-premise** en cada institución — [RN-52](../../01-negocio/reglas/RN-52-registro-de-consultas-a-manifiestos.md) lo declara riesgo residual: el acceso técnico directo a la base de datos escapa a esta historia por construcción, y debe documentarse como tal
- `[I]` El acceso de diagnóstico con motivo registrado es derivación de [RN-52](../../01-negocio/reglas/RN-52-registro-de-consultas-a-manifiestos.md) y del alcance de datos de `ACT-01`, no una figura de norma
