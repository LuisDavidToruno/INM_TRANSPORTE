# RNF-19 — Una segunda institución se pone en marcha cargando catálogos, sin tocar el código

| Campo | Valor |
|---|---|
| **Categoría** | Portabilidad / Operabilidad |
| **Prioridad** | Alto |
| **Origen** | [CLAUDE.md](../../../CLAUDE.md): SIGTI es un **sistema genérico para instituciones públicas hondureñas**, no un sistema del Instituto Nacional de Migración |
| **Afecta arquitectura** | **Sí** — obliga a que todo lo institucional-específico sea dato, y a que el artefacto de despliegue sea el mismo para todas las instituciones |

## Enunciado

SIGTI se despliega **una instancia por institución**, on-premise, con múltiples dependencias y delegaciones dentro de ella. El repositorio se llama `INM_TRANSPORTE` por su origen, pero **el producto no es específico del Instituto Nacional de Migración**.

Por tanto: **nada institucional-específico vive en el código**. Ni el nombre de la institución, ni sus siglas, ni su organigrama, ni sus tipos de vehículo, ni sus motivos de viaje, ni sus zonas, ni su horario hábil, ni sus umbrales, ni el formato de su correlativo vehicular. Todo eso es **catálogo configurable**, y con [`RNF-05`](RNF-05-temporalidad-normativa.md), catálogo **con vigencia**.

La prueba de que se cumplió no es una declaración de intención: es poner en marcha una segunda institución con el mismo artefacto de despliegue y ver si funciona.

## Métrica y umbral

| Métrica | Umbral |
|---|---|
| Apariciones del nombre o las siglas de la institución piloto en el código o en la configuración base | **0** (se admiten en datos de ejemplo claramente identificados y en documentación de origen) |
| Reglas de negocio condicionadas a una institución concreta dentro del código | **0** |
| Catálogos que exijan intervención de desarrollo para poblarse | **0.** Todo catálogo se carga desde la interfaz de administración o por importación documentada |
| Parámetros de comportamiento configurables sin desplegar versión | 100 % de los listados en el **catálogo de parámetros**, que es un artefacto del proyecto y se mantiene al día |
| Tiempo de puesta en marcha de una segunda institución, con sus catálogos ya levantados | ≤ 1 jornada `[C]` |
| Diferencia entre el artefacto de despliegue de dos instituciones | **0.** Mismo artefacto, distinta configuración |
| Aislamiento entre instancias | Total. Una instancia **no ve ni comparte** datos con otra; no hay multi-institución dentro de una misma base |
| Elementos de identidad institucional configurables | Nombre, siglas, escudo, formato del correlativo vehicular, leyenda de rotulación, pie de los documentos oficiales, horario hábil, calendario de feriados |
| Textos legales y de documentos oficiales cableados en el código | **0.** Viven como plantilla editable con vigencia |
| Cobertura del catálogo de parámetros | Todo umbral, plazo, tarifa, categoría y regla configurable citada en cualquier `RN-xx` o `RNF-xx` aparece en él |
| Parámetros sin valor definido al instalar | Se instalan **vacíos y bloqueantes**, no con un valor por defecto inventado. Ver abajo |

## El valor por defecto inventado es el enemigo

Un catálogo que se instala con una tarifa de peaje de ejemplo, un feriado supuesto o un plazo de liquidación "razonable" produce el peor resultado posible: la institución opera meses sobre valores que nadie confirmó, y esos valores terminan en documentos oficiales y en descargos ante el TSC.

**Regla:** todo parámetro cuyo valor real esté `[C]` se instala vacío. El sistema bloquea la operación que lo necesita con un mensaje que dice qué parámetro falta y quién debe proveerlo — no lo estima.

Hoy están en esa condición, entre otros: tarifas de peaje (insumo #21), exoneraciones (#22), feriados de octubre ([NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md) `[C]`), matriz licencia↔vehículo definitiva (#20), horario hábil y plazos (#32), umbrales de desviación de consumo (#32), plazos de retención (#71).

## Cómo se verifica

1. **Prueba de la segunda institución**: se instala una segunda instancia desde el mismo artefacto con un juego de catálogos distinto —otro nombre, otras siglas, otras dependencias, otros tipos de vehículo, otro horario hábil— y se ejecuta el guion completo de una misión. Debe funcionar sin una sola modificación de código.
2. **Barrido de nombres**: búsqueda automatizada del nombre y las siglas de la institución piloto en todo el repositorio. Toda coincidencia fuera de documentación de origen y datos de ejemplo se corrige.
3. **Auditoría del catálogo de parámetros**: se recorren todas las reglas `RN-xx` y todos los `RNF-xx` extrayendo cada valor configurable citado, y se verifica que aparece en el catálogo de parámetros. Un valor citado en una regla y ausente del catálogo está cableado en alguna parte.
4. **Prueba del parámetro vacío**: se instala una instancia limpia sin cargar tarifas de peaje y se intenta estimar los peajes de una misión. Debe bloquear con mensaje explícito, **no** calcular cero ni usar un valor de ejemplo.
5. **Prueba de aislamiento**: con dos instancias activas, se verifica que ninguna consulta, reporte ni exportación de una alcanza datos de la otra.
6. **Prueba de identidad institucional**: se cambia nombre, siglas y escudo, y se reimprimen todos los documentos oficiales verificando que el cambio se refleja en los 100 % de ellos.

## Consecuencia de no cumplirlo

El producto deja de ser un producto y se convierte en un desarrollo a la medida de una institución. La segunda institución que lo quiera exige un proyecto nuevo, y a partir de ahí existen dos versiones del código que divergen: una corrección hecha en una no llega a la otra, y en dos años hay dos sistemas distintos con el mismo nombre.

Y en el corto plazo, dentro de la propia institución piloto: cada cambio de tarifa, de feriado o de horario exige un despliegue. Como desplegar cuesta, no se hace, y el sistema empieza a calcular con valores vencidos — que es precisamente lo que el [`RNF-05`](RNF-05-temporalidad-normativa.md) existe para impedir.

## Trazabilidad

- Módulos: M-01, M-02
- Reglas: [`RN-39`](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md), [`RN-16`](../../01-negocio/reglas/RN-16-seguro-y-revision-mecanica.md) (bloqueo configurable), [`RN-18`](../../01-negocio/reglas/RN-18-rotulacion-del-vehiculo-del-estado.md), [`RN-24`](../../01-negocio/reglas/RN-24-vehiculo-de-servicio-exceptuado.md)
- Normativa: [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md), [NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md)
- Requisitos relacionados: [`RNF-05`](RNF-05-temporalidad-normativa.md), [`RNF-09`](RNF-09-instalacion-respaldo-y-restauracion.md), [`RNF-16`](RNF-16-idioma-accesibilidad-y-mensajes.md)
- Insumos: #1, #20, #21, #22, #32, #34, #43, #44, #71
