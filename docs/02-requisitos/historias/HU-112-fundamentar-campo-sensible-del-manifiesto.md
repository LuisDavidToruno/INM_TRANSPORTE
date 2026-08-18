# HU-112 — Exigir base legal y necesidad operativa antes de activar un campo sensible en el manifiesto

| Campo | Valor |
|---|---|
| **Módulo** | M-17 Traslado de Personas Externas · M-02 Catálogos Maestros · M-14 Reportes, Indicadores y Auditoría |
| **Actor** | ACT-01 Administrador del Sistema |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Refinada |

## Historia

**Como** Administrador del Sistema
**quiero** que activar un campo de salud, etnia, situación migratoria o condición de vulnerabilidad en el manifiesto me obligue a registrar base legal y necesidad operativa
**para** que la institución pueda sostener ante el IAIP, ante Auditoría Interna y ante un hábeas data por qué está guardando ese dato — y no descubra dos años después que nadie sabe quién lo pidió

## Contexto

El catálogo de campos del manifiesto es configurable, y esa configurabilidad es la puerta por la que entra todo lo que [RN-51](../../01-negocio/reglas/RN-51-minimizacion-de-datos-de-personas-externas.md) quiere dejar fuera. Alguien con perfil de administrador agrega *"condición médica"* porque una jefatura lo pidió por teléfono, y el campo queda para siempre.

La regla es deliberadamente asimétrica: **el sistema no impide técnicamente crear el campo**, porque impedirlo llevaría a que el dato se escriba en un campo de observaciones — que es peor, porque ahí no se puede separar, ni depurar, ni contar. Lo que hace es dejar el campo marcado como **sin fundamento registrado** y reportarlo a auditoría.

`[V]` que no existe ley general de datos personales vigente en Honduras y `[V]` que el hábeas data del Art. 182 constitucional sí lo está ([NRM-07](../../01-negocio/normativa/NRM-07-transparencia-y-datos-personales.md)). Esta historia no anticipa la ley en trámite: aplica lo que el MARCI ya exige.

## Reglas que la gobiernan

- [RN-51](../../01-negocio/reglas/RN-51-minimizacion-de-datos-de-personas-externas.md) — Campo sensible solo con **base legal expresa y necesidad operativa documentada**, con autor y fecha, visible para el Auditor Interno
- [RN-39](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md) — El catálogo de campos es parámetro con vigencia por rango de fechas
- [RN-40](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md) — Un manifiesto se valida contra el catálogo vigente **a la fecha del hecho**, no contra el actual
- [RN-04](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md) — Desactivar un campo no borra los datos ya capturados con él
- [RN-52](../../01-negocio/reglas/RN-52-registro-de-consultas-a-manifiestos.md) — Un campo sensible activo entra al régimen de registro de consultas

## Casos especiales que la afectan

- [CE-18](../casos-especiales/CE-18-carga-y-pasajeros-en-la-misma-mision.md) — El par *personal de la institución + persona bajo custodia* es el supuesto que más presión pone sobre el catálogo

## Criterios de aceptación

```gherkin
# language: es
Característica: Fundamento obligatorio para campos sensibles del manifiesto

  Antecedentes:
    Dado un catálogo "campo_manifiesto_persona_externa" con los campos mínimos vigentes al "2026-09-01"
    Y una lista de clases sensibles: "salud", "etnia", "situación migratoria", "condición de vulnerabilidad"

  Escenario: Se advierte y se marca el campo sensible activado sin fundamento
    Cuando el Administrador del Sistema activa el campo "condición médica" de clase "salud" sin registrar base legal ni necesidad operativa
    Entonces el sistema activa el campo
    Y lo marca como "CAMPO SIN FUNDAMENTO REGISTRADO"
    Y muestra "Activó un campo de clase salud sin base legal ni necesidad operativa. El campo queda marcado y se reporta a Auditoría Interna hasta que registre el fundamento."
    Y lo incluye en el reporte de campos sin fundamento visible para el Auditor Interno

  Escenario: Se rechaza registrar el fundamento sin necesidad operativa
    Dado el campo "condición médica" activo y marcado como "CAMPO SIN FUNDAMENTO REGISTRADO"
    Cuando el Administrador del Sistema registra únicamente la base legal "Convenio interinstitucional de ejemplo" y deja vacía la necesidad operativa
    Entonces el sistema rechaza el fundamento
    Y muestra "El fundamento requiere las dos cosas: la base legal que autoriza el dato y para qué operación del traslado se necesita."
    Y el campo continúa marcado como "CAMPO SIN FUNDAMENTO REGISTRADO"

  Escenario: Se registra el fundamento completo y el campo deja de estar marcado
    Dado el campo "condición médica" activo y marcado como "CAMPO SIN FUNDAMENTO REGISTRADO"
    Cuando el Administrador del Sistema registra la base legal "Convenio interinstitucional de ejemplo, cláusula tercera", la necesidad operativa "asignar vehículo con acceso para camilla" y la vigencia del "2026-10-01" al "2027-09-30"
    Entonces el sistema retira la marca "CAMPO SIN FUNDAMENTO REGISTRADO"
    Y registra el fundamento con el autor "Administrador del Sistema" y la fecha "2026-09-15"
    Y el fundamento queda visible para el Auditor Interno

  Escenario: El campo sensible no aparece en la exportación de transparencia
    Dado el campo "condición médica" activo con fundamento vigente
    Y un manifiesto de la Orden de Misión "OM-2026-0451" con ese campo diligenciado
    Cuando la Gerencia Administrativa genera la exportación de transparencia del período
    Entonces la exportación no contiene la columna "condición médica"
    Y no contiene ningún otro campo del segmento de datos personales

  Escenario: Desactivar el campo no borra lo ya capturado
    Dado el campo "condición médica" con vigencia hasta el "2027-09-30"
    Y "18" manifiestos que lo diligenciaron entre el "2026-10-01" y el "2027-09-30"
    Cuando el Administrador del Sistema cierra la vigencia del campo el "2027-09-30"
    Entonces el sistema deja de ofrecer el campo en manifiestos nuevos
    Y conserva los "18" valores ya capturados hasta que la política de retención los depure
    Y muestra "El campo queda cerrado a partir del 30/09/2027. Los 18 registros existentes no se borran: se depuran según la política de retención."

  Escenario: Un manifiesto viejo se lee con el catálogo vigente a su fecha
    Dado un manifiesto cerrado el "2026-11-20" con el campo "condición médica" diligenciado
    Cuando el Auditor Interno consulta ese manifiesto el "2027-12-01"
    Entonces el sistema muestra el manifiesto con el catálogo vigente al "2026-11-20"
    Y muestra el fundamento que estaba registrado para el campo en esa fecha
```

## Fuera de alcance

- La captura del manifiesto por el Solicitante — es [HU-111](HU-111-registrar-manifiesto-de-personas-externas.md)
- La depuración efectiva de los datos ya capturados — es [HU-124](HU-124-depurar-datos-personales-sin-romper-la-cadena.md)
- El registro de consultas sobre el campo sensible — es [HU-118](HU-118-registrar-cada-consulta-al-manifiesto.md)
- El registro de consentimiento y el catálogo de finalidades: **descartados** por [DP-001 D-14](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md)

## Notas y pendientes

- `[C]` **¿La institución traslada personas bajo custodia o menores?** — insumo #39. Es el supuesto más probable de campo sensible con base legal real, y hasta que se confirme **no se predefine ninguno**
- `[C]` A quién notifica el reporte de campos sin fundamento además del Auditor Interno — el Oficial de Información Pública no está en el catálogo de actores (ver [HU-121](HU-121-atender-habeas-data-buscar-y-exportar.md))
- `[I]` Que el sistema permita crear el campo y lo marque, en vez de impedirlo, es **decisión de diseño derivada de [RN-51](../../01-negocio/reglas/RN-51-minimizacion-de-datos-de-personas-externas.md)** — no una exigencia de norma
