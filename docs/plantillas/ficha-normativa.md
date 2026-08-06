# Plantilla — Ficha normativa

Archivo: `docs/01-negocio/normativa/NRM-xx-slug-corto.md`

Una ficha normativa traduce **norma legal → requisito de sistema**. No es un resumen de la ley: es la extracción de lo que obliga al software.

## Reglas no negociables

1. **Nunca inventes** números de decreto, artículos, tarifas ni códigos presupuestarios. Si no lo verificaste, márcalo `[C]`.
2. **Marca el nivel de verificación** en cada afirmación, no solo al inicio del documento.
3. **Cita la fuente** con URL cuando exista.
4. **Distingue norma de práctica.** "Así se hace" no es lo mismo que "así lo manda la ley", y la diferencia importa cuando alguien pregunta por qué el sistema bloquea algo.
5. **Registra la fecha de consulta.** La normativa hondureña de este dominio cambia; una ficha sin fecha no se puede auditar.

## Leyenda de verificación

| Marca | Significado |
|---|---|
| `[V]` | Verificado con fuente oficial o fuentes concordantes |
| `[P]` | Parcialmente verificado — la norma existe y se confirmó su numeración y vigencia, pero no se pudo extraer el articulado (muchos PDF oficiales son escaneos sin capa de texto) |
| `[C]` | Por confirmar con la institución |
| `[I]` | Inferencia o práctica común, no norma |

---

## Esqueleto

```markdown
# NRM-xx — <Tema>

| Campo | Valor |
|---|---|
| **Ámbito** | <qué regula> |
| **Módulos afectados** | M-xx |
| **Última verificación** | <fecha> |
| **Riesgo de cambio** | Alto / Medio / Bajo |

## Marco normativo

| Norma | Referencia | Vigencia | Verificación |
|---|---|---|---|
| <nombre> | <decreto / acuerdo / número> | <fecha> | `[V]` |

## Qué exige

<Lo que la norma obliga, en lenguaje claro. Un punto por obligación.>

## Implicaciones de requerimiento

Formato obligatorio: **"El sistema debe…"** — una por línea, accionable.

- **El sistema debe** …

## Zonas grises y pendientes

- `[C]` <lo que hay que confirmar, y con quién>

## Fuentes

- [<título>](<url>) — consultado <fecha>
```

---

## Ejemplo abreviado

# NRM-XX — Ejemplo de estructura

| Campo | Valor |
|---|---|
| **Ámbito** | Uso y circulación de vehículos propiedad del Estado |
| **Módulos afectados** | M-03, M-04, M-07 |
| **Última verificación** | 2026-08-06 |
| **Riesgo de cambio** | Medio |

## Marco normativo

| Norma | Referencia | Vigencia | Verificación |
|---|---|---|---|
| Reglamento para el funcionamiento, uso, circulación y control de automotores propiedad del Estado | Acuerdo No. 303 | 24/04/1981 | `[V]` |

## Qué exige

- Identificación obligatoria del vehículo: tres franjas horizontales de 10 cm cada una, azul–blanco–azul, en las puertas laterales `[V]`
- Leyenda "PROPIEDAD DEL ESTADO DE HONDURAS" en letras de 2.54 cm `[V]`

## Implicaciones de requerimiento

- **El sistema debe** registrar el estado de rotulación e identificación como campo verificable, con fecha de última constatación y fotografía, porque es hallazgo de auditoría frecuente.
- **El sistema debe** incluir la verificación de rotulación en el acta de constatación física de flota.

## Zonas grises y pendientes

- `[C]` Confirmar si la institución piloto tiene alguna exención autorizada para vehículos de investigación o de seguridad.

## Fuentes

- [TSC — Informe 002-2023-DFBN](https://www.tsc.gob.hn/wp-content/uploads/002-2023-DFBN-1.pdf) — consultado 2026-08-06
