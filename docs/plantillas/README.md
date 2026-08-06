# Plantillas

Cada plantilla incluye un **ejemplo completo y real del dominio de transporte**, no un `<lorem ipsum>`. Copia el ejemplo y adáptalo; es más rápido y produce artefactos más consistentes que partir del esqueleto vacío.

| Plantilla | Para qué | Prefijo |
|---|---|---|
| [historia-de-usuario.md](historia-de-usuario.md) | Historias de usuario | `HU-xxx` |
| [criterios-aceptacion-gherkin.md](criterios-aceptacion-gherkin.md) | Escenarios de aceptación en Gherkin español | — |
| [regla-de-negocio.md](regla-de-negocio.md) | Reglas de negocio verificables | `RN-xx` |
| [caso-especial.md](caso-especial.md) | Excepciones de la operación real | `CE-xx` |
| [caso-de-uso.md](caso-de-uso.md) | Casos de uso con flujo principal y alternos | `CU-xx` |
| [requisito-no-funcional.md](requisito-no-funcional.md) | Requisitos no funcionales medibles | `RNF-xx` |
| [ficha-normativa.md](ficha-normativa.md) | Fichas del marco legal hondureño | `NRM-xx` |
| [adr.md](adr.md) | Decisiones de arquitectura | `ADR-xxx` |
| [definition-of-ready.md](definition-of-ready.md) | Criterio para que una historia entre a un sprint | — |
| [definition-of-done.md](definition-of-done.md) | Criterio para dar una historia por terminada | — |
| [acta-de-refinamiento.md](acta-de-refinamiento.md) | Registro de sesión de refinamiento | — |

## Reglas comunes a todas

1. **Un artefacto por archivo.** El nombre del archivo empieza por su ID: `RN-07-licencia-habilitante.md`.
2. **Los IDs no se reciclan.** Si un artefacto se descarta, se marca `Obsoleto` y se deja; no se reasigna el número.
3. **Enlaces relativos** entre artefactos, para que funcionen en GitHub y en el editor.
4. **Nivel de verificación** `[V]` `[P]` `[C]` `[I]` en toda afirmación normativa.
5. **Nombres de archivo** en kebab-case, sin tildes ni ñ. El contenido sí lleva tildes correctas.
