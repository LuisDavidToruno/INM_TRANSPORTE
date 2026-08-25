# HU-141 — Mantener los catálogos simples —motivos de viaje, tipos de carga, zonas— con vigencia y sin borrar nunca una entrada usada

| Campo | Valor |
|---|---|
| **Módulo** | M-02 Catálogos Maestros |
| **Actor** | ACT-01 Administrador del Sistema (carga) · ACT-08 Gerencia Administrativa (aprueba) |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — el contenido inicial de cada catálogo está `[C]` y **no se inventa** (insumo #1) |

## Historia

**Como** Administrador del Sistema
**quiero** mantener los catálogos operativos —motivos de viaje, tipos de carga, zonas, tipificaciones de incidente— con rango de vigencia, y que una entrada ya usada nunca se pueda borrar ni renombrar en su lugar
**para** que el catálogo se pueda depurar sin que cambien retroactivamente los 3,000 expedientes que ya citan una de sus entradas

## Contexto

`HU-001` arranca dando por existente *"un catálogo de motivos de viaje vigente"*, y **ninguna historia lo crea**. Ésta es esa historia.

El error clásico de los catálogos operativos no es no tenerlos: es **editarlos en su lugar**. Alguien decide que *"Diligencia administrativa"* debería llamarse *"Gestión administrativa"*, lo renombra, y los 3,000 expedientes históricos que decían lo primero pasan a decir lo segundo. Nadie lo nota, hasta que el auditor compara un descargo de hace dos años con el reporte de hoy.

El tratamiento correcto: **depurar es cerrar una vigencia, no borrar**. La entrada retirada deja de ofrecerse para expedientes nuevos y sigue mostrándose en los antiguos.

Y el catálogo lleva el mismo **doble control** que los parámetros normativos: la matriz de permisos ([actores-y-roles §4.1](../../01-negocio/actores-y-roles.md), acción 16) marca `E` para `ACT-01` y `A` para `ACT-08`.

## Reglas que la gobiernan

- [RN-39](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md) — Todo catálogo es dato con rango de vigencia, mantenible sin cambio de código y con doble control
- [RN-40](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md) — Lo que se resuelve contra el catálogo usa la entrada vigente a la fecha del hecho
- [RN-04](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md) — Una entrada no se borra: se cierra su vigencia con motivo y autor
- [RN-05](../../01-negocio/reglas/RN-05-registro-cerrado-no-se-edita.md) — Un expediente cerrado no cambia porque cambie el catálogo que citaba
- [RN-94](../../01-negocio/reglas/RN-94-fecha-de-corte-de-conocimiento-en-todo-reporte.md) — El reporte se reproduce con el catálogo vigente a su fecha de corte

## Casos especiales que la afectan

- Ninguno de los 28 vigentes. **Caso especial candidato:** *entrada de catálogo retirada con misiones programadas que la citan y aún no ejecutadas* — la misión ya autorizada no puede quedar apuntando a un motivo que dejó de existir

## Criterios de aceptación

```gherkin
# language: es
Característica: Mantenimiento de catálogos operativos con vigencia

  Antecedentes:
    Dado un catálogo "Motivos de viaje" con la entrada "Diligencia administrativa" vigente desde el "2026-01-01"
    Y 3,000 expedientes que la citan
    Y un catálogo "Tipos de carga" vacío

  Escenario: Se rechaza eliminar una entrada usada
    Cuando el Administrador del Sistema intenta eliminar "Diligencia administrativa"
    Entonces el sistema rechaza la eliminación
    Y muestra "Diligencia administrativa está citada por 3,000 expedientes. Una entrada de catálogo no se elimina: cierre su vigencia con motivo."

  Escenario: Se rechaza renombrar una entrada usada
    Cuando el Administrador del Sistema intenta cambiar la denominación a "Gestión administrativa"
    Entonces el sistema rechaza el cambio
    Y muestra "No se renombra una entrada citada por 3,000 expedientes: los reportes históricos cambiarían de contenido. Cierre esta vigencia y cree la nueva denominación con vigencia desde la fecha que corresponda."

  Escenario: Se rechaza una entrada duplicada dentro del mismo rango de vigencia
    Dada la entrada "Traslado de personal" vigente del "2026-01-01" al "2026-12-31"
    Cuando el Administrador del Sistema crea otra entrada "Traslado de personal" con vigencia desde el "2026-06-01"
    Entonces el sistema rechaza la creación
    Y muestra "Ya existe Traslado de personal vigente del 01/01/2026 al 31/12/2026. Dos entradas no pueden solapar su vigencia dentro del mismo catálogo."

  Escenario: La entrada cargada queda pendiente y no se ofrece hasta ser aprobada
    Cuando el Administrador del Sistema carga la entrada "Traslado de insumos médicos" en el catálogo "Tipos de carga"
    Entonces el sistema la registra en estado "PENDIENTE DE APROBACIÓN"
    Y no se ofrece al Solicitante al registrar una solicitud
    Y aparece en el tablero de la Gerencia Administrativa

  Escenario: La aprobación pone la entrada en circulación
    Dada la entrada "Traslado de insumos médicos" pendiente de aprobación
    Cuando la Gerencia Administrativa la aprueba el "2026-09-20"
    Entonces la entrada queda disponible desde su fecha de inicio de vigencia
    Y el sistema registra carga y aprobación como dos actos fechados por separado con autor distinto

  Escenario: El cierre de vigencia retira la entrada sin tocar el histórico
    Cuando el Administrador del Sistema cierra la vigencia de "Diligencia administrativa" al "2026-09-30" con motivo "se sustituye por dos motivos más específicos"
    Entonces la entrada deja de ofrecerse a partir del "2026-10-01"
    Y los 3,000 expedientes anteriores siguen mostrando "Diligencia administrativa"
    Y el cierre queda registrado con autor, momento y motivo

  Escenario: Una misión ya programada conserva la entrada retirada
    Dada una misión programada el "2026-09-25" para ejecutarse el "2026-10-05" con motivo "Diligencia administrativa"
    Cuando se cierra la vigencia de la entrada al "2026-09-30"
    Entonces la misión conserva su motivo y no se bloquea
    Y el sistema muestra al Jefe de Transporte "El motivo Diligencia administrativa se retiró el 30/09/2026. 14 misiones programadas lo citan y no se ven afectadas."

  Escenario: El reporte histórico usa la entrada vigente a la fecha del hecho
    Cuando el Auditor Interno emite el reporte de motivos de viaje del "2026-08-01" al "2026-08-31" con corte al "2027-01-15"
    Entonces las misiones de agosto figuran bajo "Diligencia administrativa"
    Y el reporte declara su fecha de corte de conocimiento
```

## Fuera de alcance

- Los tipos de vehículo, que no son un catálogo simple de etiquetas — es [HU-142](HU-142-tipos-de-vehiculo-con-atributos-de-compatibilidad.md)
- Las matrices de compatibilidad — es [HU-143](HU-143-matriz-de-compatibilidad-como-catalogo-mantenible.md)
- Los parámetros normativos con valor económico o de plazo — es [HU-144](HU-144-cargar-parametro-normativo-con-vigencia.md) y siguientes
- La estructura organizativa, que no es catálogo — es [HU-126](HU-126-estructura-institucional-con-vigencia.md)

## Notas y pendientes

- `[C]` **Contenido inicial de cada catálogo**: los motivos de viaje, los tipos de carga y las zonas reales de la institución. **Los catálogos se entregan vacíos**; los valores de los criterios son datos de prueba — insumo **#1** y **#2** (formatos en papel, que es donde están escritos hoy)
- `[C]` Si el catálogo de zonas se deriva de la división política del país o es propio de la institución
- `[I]` Que el doble control aplique también a los catálogos operativos y no solo a los parámetros normativos se toma de la matriz de permisos acción 16 de [actores-y-roles](../../01-negocio/actores-y-roles.md), que marca `A` para `ACT-08`. `RN-39` lo enuncia para *"todo dato normativo o institucional"*
- **Regla candidata:** *Una entrada de catálogo citada por algún expediente no se elimina ni se renombra; se retira cerrando su vigencia, y toda resolución usa la entrada vigente a la fecha del hecho.* `RN-39` cubre la vigencia; **la prohibición de renombrar en su lugar no está enunciada en ninguna de las 97**
