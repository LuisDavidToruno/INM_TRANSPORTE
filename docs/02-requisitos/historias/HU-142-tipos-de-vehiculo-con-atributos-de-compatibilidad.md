# HU-142 — Definir el tipo de vehículo con los atributos que resuelven la compatibilidad, no como una lista de etiquetas

| Campo | Valor |
|---|---|
| **Módulo** | M-02 Catálogos Maestros |
| **Actor** | ACT-01 Administrador del Sistema (carga) · ACT-08 Gerencia Administrativa (aprueba) |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — la matriz licencia↔vehículo definitiva sigue `[C]` (insumo #20) y sin ella el tipo no se puede cerrar |

## Historia

**Como** Administrador del Sistema
**quiero** definir cada tipo de vehículo con los atributos que el sistema necesita para decidir —categoría de licencia habilitante, rango de peso bruto, capacidad de pasajeros, capacidad de carga, número de ejes y categoría de peaje derivada—
**para** que la asignación vehículo↔motorista y la compatibilidad con el objeto del traslado se resuelvan contra datos y no contra el criterio del despachador

## Contexto

**El tipo de vehículo es el eje de compatibilidad** del producto (premisa rectora 2 de `CLAUDE.md`): *"Toda asignación se resuelve contra esa compatibilidad."* Un catálogo de tipos que sea solo una lista de nombres —*Pickup, Camión, Motocicleta, Bus*— **no resuelve nada**: es exactamente lo que hoy obliga al Encargado de Despacho a recordar de memoria qué licencia habilita qué vehículo.

Cinco reglas vigentes leen atributos de este catálogo y **ninguna historia lo crea**:

| Regla | Qué atributo lee |
|---|---|
| [RN-09](../../01-negocio/reglas/RN-09-matriz-licencia-vehiculo.md) | Categoría de licencia habilitante por tipo, peso bruto y capacidad |
| [RN-20](../../01-negocio/reglas/RN-20-compatibilidad-vehiculo-objeto-del-traslado.md) | Qué objeto del traslado admite este tipo |
| [RN-21](../../01-negocio/reglas/RN-21-capacidad-de-pasajeros-y-carga.md) | Capacidad de pasajeros y de carga |
| [RN-33](../../01-negocio/reglas/RN-33-categoria-de-peaje-derivada-de-ficha-tecnica.md) | Categoría de peaje, **derivada de la ficha técnica, no del número de ejes por sí solo** |
| [RN-19](../../01-negocio/reglas/RN-19-vehiculo-no-operativo-no-se-asigna.md) | El tipo condiciona qué se puede asignar |

Y una consecuencia que hay que sostener: **un tipo de vehículo con atributos incompletos no puede usarse**. Si se admite, el primer vehículo que se dé de alta con ese tipo pasará todas las verificaciones sin que ninguna se ejecute realmente.

## Reglas que la gobiernan

- [RN-39](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md) — El catálogo de tipos y la matriz licencia↔vehículo son parámetros con vigencia y doble control
- [RN-09](../../01-negocio/reglas/RN-09-matriz-licencia-vehiculo.md) — La categoría de licencia debe habilitar tipo, peso bruto y capacidad
- [RN-20](../../01-negocio/reglas/RN-20-compatibilidad-vehiculo-objeto-del-traslado.md) — El tipo asignado debe ser compatible con el objeto del traslado declarado
- [RN-21](../../01-negocio/reglas/RN-21-capacidad-de-pasajeros-y-carga.md) — No se exceden las capacidades de la ficha técnica
- [RN-33](../../01-negocio/reglas/RN-33-categoria-de-peaje-derivada-de-ficha-tecnica.md) — La categoría de peaje se deriva de la ficha técnica
- [RN-40](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md) — La habilitación se resuelve contra la matriz vigente a la fecha del hecho

## Casos especiales que la afectan

- [CE-18](../casos-especiales/CE-18-carga-y-pasajeros-en-la-misma-mision.md) — Carga y pasajeros en el mismo vehículo: la capacidad se evalúa por configuración real
- [CE-11](../casos-especiales/CE-11-licencia-vence-durante-la-mision.md) — La habilitación depende de la matriz vigente en todo el rango
- [CE-24](../casos-especiales/CE-24-cobro-en-categoria-de-peaje-equivocada.md) — La caseta cobra una categoría distinta de la derivada del tipo

## Criterios de aceptación

```gherkin
# language: es
Característica: Tipo de vehículo con atributos de compatibilidad

  Antecedentes:
    Dado un catálogo "Tipos de vehículo" vacío
    Y un catálogo "Categorías de licencia" con las categorías "A", "B", "B1", "C1", "C", "D1", "D" y "CE" vigentes

  Escenario: Se rechaza un tipo de vehículo sin categoría de licencia habilitante
    Cuando el Administrador del Sistema crea el tipo "Camión" sin declarar la categoría de licencia habilitante
    Entonces el sistema rechaza la creación
    Y muestra "Declare la categoría de licencia que habilita este tipo. Sin ella, RN-09 no puede bloquear ninguna asignación y el control quedaría inoperante."

  Escenario: Se rechaza un tipo sin rango de peso bruto
    Cuando el Administrador del Sistema crea el tipo "Camión" con licencia habilitante "C" y sin rango de peso bruto
    Entonces el sistema rechaza la creación
    Y muestra "Declare el rango de peso bruto del tipo. La habilitación de la licencia se resuelve por tipo y por peso bruto, no solo por el nombre del tipo."

  Escenario: Se rechaza un tipo sin categoría de peaje resuelta
    Cuando el Administrador del Sistema crea el tipo "Camión" sin categoría de peaje
    Entonces el sistema rechaza la creación
    Y muestra "Declare la categoría de peaje del tipo. Sin ella no se puede estimar el costo de peajes de ninguna misión que use este vehículo."

  Escenario: Se rechaza usar un tipo con atributos incompletos al dar de alta un vehículo
    Dado un tipo "Motocicleta" cargado y pendiente de aprobación
    Cuando el Jefe de Transporte da de alta un vehículo con tipo "Motocicleta"
    Entonces el sistema rechaza el alta
    Y muestra "El tipo Motocicleta está pendiente de aprobación de la Gerencia Administrativa. Un tipo no aprobado no habilita ninguna asignación."

  Escenario: Se rechaza un rango de peso bruto que solapa con otro tipo de la misma categoría de licencia
    Dado un tipo "Camión liviano" con peso bruto de "3,501" a "7,500" kg habilitado por "C1"
    Cuando el Administrador del Sistema crea "Camión mediano" con peso bruto de "6,000" a "12,000" kg habilitado por "C"
    Entonces el sistema advierte "Los rangos de peso bruto de Camión liviano y Camión mediano se solapan entre 6,000 y 7,500 kg con categorías de licencia distintas. Un vehículo de 7,000 kg tendría dos habilitaciones posibles."
    Y exige resolver el solape o justificarlo con motivo escrito

  Escenario: Se crea el tipo completo y queda pendiente de aprobación
    Cuando el Administrador del Sistema crea el tipo "Pickup" con licencia habilitante "B", peso bruto de "1,500" a "3,500" kg, capacidad de "5" pasajeros, capacidad de carga de "1,000" kg, "2" ejes y categoría de peaje "liviana", con vigencia desde el "2026-10-01"
    Entonces el sistema registra el tipo en estado "PENDIENTE DE APROBACIÓN"
    Y no se ofrece para dar de alta vehículos

  Escenario: La aprobación habilita el tipo para toda la operación
    Dado el tipo "Pickup" pendiente de aprobación
    Cuando la Gerencia Administrativa lo aprueba el "2026-09-25"
    Entonces el tipo queda vigente desde el "2026-10-01"
    Y los vehículos que se den de alta con él resuelven licencia habilitante, capacidades y categoría de peaje automáticamente

  Escenario: La categoría de peaje no se deriva solo del número de ejes
    Dado un tipo "Pickup" con "2" ejes y categoría de peaje "liviana"
    Y un tipo "Camión mediano" con "2" ejes y categoría de peaje "pesada"
    Cuando el sistema estima los peajes de una misión con cada uno
    Entonces aplica tarifas distintas pese a que ambos tienen 2 ejes
    Y el desglose muestra la categoría aplicada por cada vehículo

  Escenario: El cambio de la matriz no altera las asignaciones ya verificadas
    Dada una asignación verificada el "2026-10-05" contra el tipo "Pickup" habilitado por "B"
    Cuando la matriz se corrige el "2026-11-02" para exigir "B1" en ese tipo
    Entonces la asignación del "2026-10-05" conserva su verificación original y su referencia a la versión de matriz usada
    Y las asignaciones nuevas usan la matriz corregida
```

## Fuera de alcance

- La ficha técnica del vehículo concreto — es [HU-098](HU-098-completar-la-ficha-tecnica-que-habilita.md)
- La verificación de la licencia del motorista al asignar — es [HU-025](HU-025-habilitacion-de-quien-efectivamente-conduce.md)
- La matriz de compatibilidad objeto × objeto — es [HU-143](HU-143-matriz-de-compatibilidad-como-catalogo-mantenible.md)
- Las tarifas de peaje por punto — es [HU-144](HU-144-cargar-parametro-normativo-con-vigencia.md)

## Notas y pendientes

- `[V]` El esquema de ocho categorías de licencia (`A`, `B`, `B1`, `C1`, `C`, `D1`, `D`, `CE`) está verificado; **`BE` aparece en `[P]`** y los umbrales literales de peso por categoría **no**. Los "7,500" y "12,000" kg de los criterios son **datos de prueba**, no la matriz — insumo **#20**
- `[C]` La matriz licencia↔vehículo se carga **vacía** hasta obtener el Acuerdo 1012-2021. Con ella vacía, `RN-09` bloquea toda asignación con mensaje que identifica el parámetro faltante — [`RNF-19`](../no-funcionales/RNF-19-configurabilidad-multi-institucion.md)
- `[C]` Tipos de vehículo reales de la flota de la institución — **no se espera inventario previo** (insumo #5 descartado); el catálogo se levanta con la institución
- `[C]` Qué tipos de carga exigen peso cierto y cuáles admiten estimación por rango — insumo **#63**
- `[P]` La derivación de la categoría de peaje se apoya en [NRM-10](../../01-negocio/normativa/NRM-10-peajes.md); el texto del Art. 51 de la Ley de Tránsito no se pudo extraer — insumo **#23**
- **Regla candidata:** *Un tipo de vehículo sin sus atributos de compatibilidad completos y aprobados no habilita el alta de ningún vehículo ni ninguna asignación.* `RN-09`, `RN-20`, `RN-21` y `RN-33` **leen** estos atributos; ninguna exige que el catálogo los tenga
