# NRM-07 — Transparencia y datos personales

| Campo | Valor |
|---|---|
| **Ámbito** | Publicación obligatoria de información, protección de datos de personas trasladadas |
| **Módulos afectados** | M-14, M-17, M-01 |
| **Última verificación** | 2026-08-06 |
| **Riesgo de cambio** | Bajo tras la decisión del PO |

> ## ⚠️ ALCANCE REDUCIDO EN LA PARTE DE DATOS PERSONALES
>
> Decisión del PO del 2026-08-06 — ver [DP-001, decisión D-14](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md).
>
> **No se diseña para anticipar la Ley de Protección de Datos Personales** pendiente en el Congreso desde 2018.
>
> **Lo que sí se conserva**, porque el MARCI lo exige de todas formas y no cuesta más:
> - Control de acceso por rol y por necesidad de conocer sobre listas de pasajeros
> - Registro de cada consulta: quién vio qué y cuándo
> - Separación entre datos de gestión pública y datos personales, para poder publicar en transparencia sin depuración manual
>
> **Lo que se descarta:** registro de consentimiento, catálogo de finalidades y registro de actividades de tratamiento.
>
> La sección de **transparencia** de esta ficha sigue vigente sin cambios.

## Transparencia

| Norma / entidad | Referencia | Vigencia | Verificación |
|---|---|---|---|
| Ley de Transparencia y Acceso a la Información Pública (LTAIP) | Decreto No. 170-2006 | Vigente | `[V]` |
| Reglamento de la LTAIP | — | Vigente | `[V]` |
| IAIP — Instituto de Acceso a la Información Pública | órgano desconcentrado | — | `[V]` |
| Portal Único de Transparencia | `portalunico.iaip.gob.hn` | Vigente | `[V]` |
| SINAIP — Sistema Nacional de Información Pública | — | Vigente | `[V]` |

Cada institución obligada publica a través de un **Oficial de Información Pública (OIP)**, que recopila información de todas las unidades administrativas y la publica en los plazos de la Ley. `[V]`

Por observación directa del Portal Único: las instituciones publican efectivamente reglamentos de viáticos, licitaciones y documentos de gestión de bienes. **Varios reglamentos de vehículos institucionales están publicados ahí** `[V]` — es una fuente útil para el Bloque 1.

`[C]` El numeral exacto del artículo de información de oficio que cubre inventario de bienes, viáticos y contrataciones. Debe leerse el articulado con el OIP institucional.

## Datos personales — situación real

**No existe ley general de protección de datos personales vigente en Honduras.** `[V]`

- El anteproyecto de *Ley de Protección de Datos Personales y Acción de Hábeas Data*, elaborado por el IAIP, está **pendiente en el Congreso Nacional desde 2018** (tercer debate suspendido). El IAIP lo ha vuelto a remitir recientemente con asistencia técnica de IDLO. `[V]`
- `[C]` **Contradicción no resuelta:** una fuente menciona una "Ley de Protección de Datos Personales en Posesión de Sujetos Obligados aprobada en 2013". No se encontró confirmación independiente y el nombre coincide con la ley mexicana homónima. **Trátese como no verificado.**

**Lo que sí está vigente:**

- **Hábeas data — Artículo 182 de la Constitución** (reforma de 2013): toda persona tiene derecho a acceder de forma expedita y no onerosa a la información sobre sí misma o sus bienes contenida en bases de datos o registros públicos o privados y, en su caso, actualizarla, rectificarla y/o suprimirla. **Solo el titular puede interponer la acción.** `[V]`
- Se regula conforme a la **Ley sobre Justicia Constitucional** y el **Artículo 23 de la LTAIP**, que reconoce el hábeas data y regula la sistematización de archivos personales y su acceso. `[V]`

## Implicaciones de requerimiento

- **El sistema debe** distinguir estructuralmente entre **datos de gestión pública** (vehículo, ruta, costo, unidad ejecutora, objeto del viaje) y **datos personales** (nombre, identidad, teléfono, dirección, datos de salud de pasajeros), y permitir exportar lo primero sin lo segundo.
- **El sistema debe** producir un **reporte público de flota y viajes agregado o anonimizado**, listo para publicar en el Portal Único de Transparencia sin trabajo manual de depuración.
- **El sistema debe** aplicar **minimización de datos en M-17**: capturar solo lo necesario para el control (identificación del pasajero, institución o condición, origen, destino). **Evitar campos de salud, etnia, situación migratoria o condición de vulnerabilidad** salvo que exista base legal expresa y necesidad operativa documentada.
- **El sistema debe** implementar **control de acceso por necesidad de conocer** sobre listas de pasajeros, con **registro de cada consulta**: quién vio qué lista y cuándo. Aun sin ley de datos, esto es exigible por el MARCI y protege ante un hábeas data.
- **El sistema debe** soportar el **ejercicio del hábeas data**: buscar todos los registros de una persona identificada, exportarlos y rectificarlos — **dejando traza de la rectificación sin destruir el registro contable original**.
- **El sistema debe** definir **políticas de retención diferenciadas**: los datos financieros y de bienes se conservan por el plazo de fiscalización; los datos personales de pasajeros deben tener plazo de depuración o seudonimización más corto. `[C]` los plazos con Auditoría Interna y el OIP.
- **El sistema debe** cifrar los datos personales en reposo, y cifrar en tránsito toda comunicación, incluida la de las delegaciones.
- ~~**El sistema debe** anticipar la ley en trámite: registro de consentimiento, catálogo de finalidades y registro de actividades de tratamiento.~~ **Descartado por decisión D-14.**

## Zonas grises y pendientes

- `[C]` Artículo y numeral de información de oficio aplicable a flota, viáticos y contrataciones.
- `[C]` Plazos de retención de datos personales, acordados con Auditoría Interna y el OIP.
- `[C]` Qué información de flota publica hoy la institución en el Portal Único, y en qué formato.
- **Vigilar** el avance de la Ley de Protección de Datos Personales en el Congreso.

## Fuentes

- [Ley de Transparencia y Acceso a la Información Pública, Decreto 170-2006](https://www.oas.org/es/sla/ddi/docs/H2%20LeyDeTransparencia.pdf) — consultado 2026-08-06
- [IAIP — Portal Único de Transparencia](https://portalunico.iaip.gob.hn/preguntas_frecuentes) — consultado 2026-08-06
- [El hábeas data en la Constitución de Honduras (Art. 182)](https://dlcarballo.com/2015/02/24/el-habeas-data-en-la-constitucion-de-honduras/) — consultado 2026-08-06
- [IAIP remite anteproyecto de Ley de Protección de Datos Personales al Congreso](https://www.elheraldo.hn/honduras/iaip-destaca-beneficios-honduras-ley-proteccion-datos-personales-BP26294115) — consultado 2026-08-06
