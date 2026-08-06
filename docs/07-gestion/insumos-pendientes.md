# Insumos pendientes de la institución piloto

Documentos y datos que se necesitan para que el análisis se apoye en la realidad y no en suposiciones. **Los marcados como bloqueantes no deben suplirse con inferencias**: mientras falten, el módulo correspondiente queda con parámetros abiertos marcados `[C]`.

Actualiza el estado de cada fila cuando el insumo se reciba, e indica dónde quedó archivado.

## Bloqueantes

| # | Insumo | Para qué | Bloquea | Estado |
|---|---|---|---|---|
| 1 | **Reglamento interno de uso de vehículos** de la institución | Reglas de uso, autorizaciones, responsabilidades y sanciones propias | M-03, M-04, M-12 | Pendiente |
| 2 | **Formatos en papel vigentes**: bitácora, requisición de vehículo, salida, vale de combustible, orden de misión, acta de entrega | Paridad pantalla↔papel; son el diseño de las pantallas | M-08, M-09, M-15, todo el Bloque 4 | Pendiente |
| 3 | **Reglamento de viáticos homologado** de la institución + **Acuerdo 401-2026** con sus tablas de zonas, categorías y tarifas | Cálculo de viáticos. El acuerdo es de julio de 2026; no se pueden usar tarifas anteriores | M-10 | Pendiente |
| 4 | **Organigrama y niveles de autorización**: quién aprueba qué según destino, monto y jerarquía | Flujo de aprobaciones y matriz de permisos | M-01, M-06 | Pendiente |

## Importantes

| # | Insumo | Para qué | Estado |
|---|---|---|---|
| 5 | Inventario actual de flota, aunque sea en hoja de cálculo | Modelo de datos del vehículo, volumen real, tipos presentes | Pendiente |
| 6 | Padrón de motoristas con categorías de licencia y vencimientos | Matriz licencia↔vehículo, dimensionamiento | Pendiente |
| 7 | Contratos vigentes de combustible y mantenimiento, y el mecanismo real de control (vale físico, cupón, tarjeta, requisición) | M-09 y M-11; determina si se modela vale, tarjeta o ambos | Pendiente |
| 8 | Estructura presupuestaria que usan: gerencia administrativa, unidades ejecutoras, objetos del gasto de combustible / mantenimiento / viáticos | Imputación presupuestaria y conciliación con SIAFI | Pendiente |
| 9 | Capacidades del servidor on-premise disponible y quién administra la infraestructura | ADR de stack y despliegue (Sprint 2) | Pendiente |

## Útiles

| # | Insumo | Para qué | Estado |
|---|---|---|---|
| 10 | Volumen operativo mensual: cuántas solicitudes, viajes, vehículos, delegaciones | Dimensionamiento y priorización | Pendiente |
| 11 | Mapa de delegaciones regionales y su situación de conectividad real | Alcance del modo offline | Pendiente |
| 12 | Últimos informes de auditoría interna o del TSC sobre flota, combustible o viáticos | Los hallazgos reales son requisitos disfrazados | Pendiente |
| 13 | Sistemas institucionales existentes con los que habría que integrar (RRHH, almacén, marcaje, GPS) | Alcance de integraciones | Pendiente |
| 14 | Calendario oficial de días hábiles y horario laboral de la institución, incluidos horarios especiales | Cálculo de viáticos, permisos de día inhábil, plazos | Pendiente |
| 15 | Certificados de firma electrónica: ¿la institución ya tiene? ¿de qué autoridad certificadora? | Diseño del esquema de firma | Pendiente |

## Cómo levantarlos

La forma más eficiente no es pedir la lista completa por correo. Es una sesión de dos horas con:

- **Gerencia Administrativa o quien maneje la flota** — insumos 1, 4, 7, 8
- **Encargado de transporte / despacho** — insumos 2, 5, 6, 10, 11
- **Un motorista con años en el puesto** — es quien conoce los casos especiales reales del Bloque 2
- **Auditoría Interna** — insumo 12, y los plazos de retención documental

Lleva los formatos en papel a la sesión y recórrelos campo por campo. Ahí aparecen las reglas que nadie escribió nunca.
