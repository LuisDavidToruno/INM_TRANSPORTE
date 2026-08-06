# 06 — Operación

Cómo se instala, se respalda, se recupera y se usa el sistema en producción.

## Estructura

| Ruta | Estado | Contenido |
|---|---|---|
| `instalacion.md` | Sprint 2+ | Requisitos de servidor, procedimiento de instalación paso a paso |
| `respaldo-y-restauracion.md` | Sprint 2+ | Política de respaldo, procedimiento de restauración y su prueba periódica |
| `actualizacion.md` | Sprint 8 | Cómo aplicar una nueva versión sin perder datos ni interrumpir la operación |
| `manual-administrador.md` | Sprint 8 | Configuración de catálogos, usuarios, roles, parámetros normativos |
| `manual-usuario.md` | Sprint 8 | Guías por rol: solicitante, jefatura, despacho, motorista, delegación |
| `guia-motorista-offline.md` | Sprint 7 | Guía de bolsillo: qué hacer sin señal, y qué hacer ante un accidente |

## Supuesto operativo central

**No habrá un equipo de TI dedicado.** La institución tiene una unidad de informática con carga alta y las delegaciones regionales no tienen personal técnico. Todo procedimiento de operación debe poder ejecutarlo alguien con conocimientos generales siguiendo un documento — sin improvisar.

Consecuencias que arrastran hasta la arquitectura:

- La instalación no puede requerir orquestación compleja ni ajustes manuales de configuración dispersos.
- El respaldo debe ser automático por defecto, no una tarea que alguien recuerde ejecutar.
- **La restauración debe estar probada, no solo documentada.** Un respaldo que nunca se restauró no es un respaldo.
- Los errores deben producir mensajes accionables en español, no trazas de excepción.

## Rotación de personal

Honduras tuvo cambio de gobierno en enero de 2026 y la rotación en el sector público es alta. El sistema debe absorberla:

- Roles y permisos se asignan **por puesto, no por persona**.
- Debe existir traspaso masivo de custodias de vehículos con acta.
- Los expedientes abiertos de alguien que se va deben poder reasignarse sin perder trazabilidad de quién hizo qué antes.
- La inducción de un usuario nuevo debe estar dentro del sistema, no depender de que alguien le enseñe.
