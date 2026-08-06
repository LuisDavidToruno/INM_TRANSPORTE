# Visión de producto — SIGTI

| Campo | Valor |
|---|---|
| **Versión** | 1.0 |
| **Fecha** | 2026-08-06 |
| **Sprint / Bloque** | Sprint 0 / Bloque 1 |
| **Autor** | Product Owner |

## La frase

> Así como Talento Humano cuida de todo lo referente a los empleados, **SIGTI cuida de todo lo referente a los vehículos** — motos, buses, pickups, camiones.

Esa analogía no es un eslogan: es la decisión de diseño más importante del producto. Talento Humano no es un listado de empleados; es un expediente vivo con historial, documentos, vencimientos, permisos y responsabilidades. **El vehículo merece exactamente lo mismo**, y hoy no lo tiene.

## El problema

Una institución pública hondureña mueve recursos todos los días. Personal a una delegación, equipo de cómputo a una oficina regional, insumos a una brigada, personas bajo atención institucional entre puntos del país. Cada una de esas movilizaciones consume un vehículo, un motorista, combustible, peajes y tiempo — y **todo eso se controla hoy en papel, en hojas de cálculo dispersas y en la memoria del encargado de transporte**.

Las consecuencias son concretas:

**No se sabe dónde está la flota.** Quién anda en qué vehículo, con quién, hacia dónde, y cuándo vuelve. El encargado lo sabe de memoria hasta que se enferma o lo rotan.

**El expediente del vehículo no existe.** Los datos están repartidos entre Bienes Nacionales, el taller, la aseguradora y un fólder. Cuando vence una matrícula, nadie se entera hasta que un agente lo detiene en carretera.

**El consumo no se puede defender.** El auditor del Tribunal Superior de Cuentas no pide facturas: pide **correlación entre combustible consumido, kilómetros recorridos y misión autorizada**. Reconstruir eso a mano, meses después, con bitácoras en papel, es imposible — y el hallazgo se levanta.

**La segregación de funciones no se hace cumplir.** El MARCI exige que quien solicita no autorice, y que quien despacha no liquide. En papel eso depende de que alguien se acuerde.

**La asignación traslada responsabilidad legal.** Si se asigna un motorista cuya licencia no habilita la categoría del vehículo y ocurre un accidente, la responsabilidad recae sobre quien autorizó. Hoy esa verificación depende de la memoria de una persona con cuarenta motoristas a cargo.

**Nada de esto funciona sin internet.** Y más de 2 millones de personas del área rural hondureña no tienen acceso a internet (INE, EPHPM julio 2025). El control de campo ocurre exactamente donde no hay señal.

## Qué es SIGTI

Un sistema de gestión de transporte y flota vehicular para **instituciones públicas hondureñas**, desplegado on-premise en los servidores internos de cada institución.

**No gestiona "viajes de personas". Gestiona movilizaciones de recursos institucionales.** Lo trasladado puede ser personal, personas externas, carga — equipos, herramientas, insumos, materiales — o una combinación. La unidad de control administrativo-contable es la **Orden de Misión**, y el **tipo de vehículo** es el eje que conecta lo que hay que mover con la flota disponible.

## Para quién

| Usuario | Qué gana |
|---|---|
| **Solicitante** | Pide un vehículo sin perseguir firmas, y sabe en qué va su solicitud |
| **Jefatura inmediata** | Autoriza en dos toques desde el celular, viendo lo que necesita para decidir |
| **Jefe de Transporte** | Deja de cargar la operación en la memoria. Ve la flota completa y su estado |
| **Encargado de Despacho** | Asigna sin conflictos, y el sistema le impide los errores que trasladan responsabilidad legal |
| **Motorista** | Registra su misión desde el celular, sin señal, con menos fricción que el papel |
| **Gerencia Administrativa** | Sabe en qué se gastó el fondo de combustible y contra qué kilometraje |
| **Auditoría Interna y TSC** | Reciben la cadena de evidencia completa, exportable, sin reconstruirla a mano |

## Qué lo hace distinto

**1. El expediente del vehículo es una entidad de primera clase.** Con ciclo de vida completo: documentación y vencimientos, seguro, revisión, mantenimiento, fallas, incidentes, especificaciones técnicas, custodios y asignaciones. No es un catálogo.

**2. Offline-first, no "con soporte offline".** La ausencia de red es el estado normal esperado en campo. El motorista registra salida, bitácora, consumo, peajes, incidentes y fotos sin ninguna conectividad, durante días, y sincroniza al volver. **Cero pérdida de datos, cero sobrescritura silenciosa.**

**3. Híbrido digital-papel por diseño, no por parche.** El control en carretera es físico. Todo documento tiene versión imprimible con folio, QR de verificación, espacio de firma y sello. Y los formularios en pantalla reproducen los formatos en papel campo por campo — el operador que lleva años llenándolos debe reconocerlos.

**4. Trazabilidad inmutable.** Nada se borra. Toda anulación es un asiento reverso con motivo y autor. La cadena solicitud → autorización → orden de misión → bitácora → combustible → peajes → liquidación queda completa y exportable.

**5. Nada normativo se cablea.** Tarifas de peaje, categorías vehiculares, feriados, horario hábil, plazos y matriz licencia↔vehículo son parámetros con vigencia por rango de fechas. Todo cálculo usa la tabla vigente **a la fecha del hecho**. Cuando la tarifa de peaje suba en enero —y va a subir— se carga la tabla nueva sin tocar código, y los viajes de diciembre siguen valorados como corresponde.

**6. No replicamos lo que otro sistema ya hace.** ARGOS posee viáticos, presupuesto, niveles de autorización y mapas. Talento Humano posee el expediente del empleado. SIGTI se integra con ellos. Ver [DP-001](../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md).

**7. Genérico, no de una institución.** Todo lo institucional-específico vive en catálogos configurables. Una instancia por institución, con sus dependencias y delegaciones dentro.

## Objetivos medibles

Las líneas base están pendientes de la institución piloto. **No se inventan**: se miden antes de desplegar.

| # | Objetivo | Métrica | Meta | Base |
|---|---|---|---|---|
| 1 | Eliminar asignaciones con motorista no habilitado | Asignaciones con licencia vencida o categoría insuficiente | **Cero.** Es bloqueo duro | `[C]` |
| 2 | Hacer cumplir la segregación de funciones | Misiones donde solicitante = autorizador | **Cero** | `[C]` |
| 3 | Reducir el tiempo entre solicitud y despacho | Horas desde `SOLICITADA` hasta `DESPACHADA` | −50% | `[C]` medir hoy |
| 4 | Cerrar el ciclo de conciliación de combustible | Misiones conciliadas galonaje–kilometraje | ≥ 95% de las cerradas | 0% hoy |
| 5 | Detectar desviación de consumo antes que el auditor | Días entre el hecho y la detección | ≤ 30 días | `[C]` |
| 6 | Producir evidencia de auditoría sin trabajo manual | Horas para armar un expediente por período | De días a minutos | `[C]` |
| 7 | Que el registro de campo no dependa de la señal | Misiones registradas íntegramente offline y sincronizadas sin pérdida | 100% | — |
| 8 | Adopción real en campo | Motoristas que registran desde el sistema en lugar de papel | ≥ 80% a los 3 meses | 0% hoy |
| 9 | Evitar vencimientos sorpresa | Documentos vehiculares vencidos sin alerta previa | **Cero** | `[C]` |
| 10 | Saber dónde está la flota | Vehículos en misión con estado y ubicación actualizados | ≥ 90% | 0% hoy |

El objetivo 8 es el que decide si el proyecto sirvió. Los demás son consecuencia de él: si el motorista vuelve al papel, ninguna otra métrica se cumple.

## Fuera de alcance

Explícito, para que no haya discusión después:

| No hace | Quién lo hace |
|---|---|
| Viáticos y gastos de viaje del servidor | **ARGOS** |
| Estructura presupuestaria y niveles de autorización | **ARGOS** (SIGTI los espeja) |
| Expediente del empleado, permisos, vacaciones, feriados | **Talento Humano** (SIGTI los espeja) |
| Compra de combustible, contratos de suministro, proveedores | Otros sistemas. SIGTI gestiona el **fondo asignado y su consumo** |
| Registro contable oficial | SIAFI. La integración queda **diferida** |
| Inventario de insumos y materiales | **Almacén**. Integración diferida |
| Firma electrónica certificada | No se usa. Autorización interna por usuario o código del sistema |
| Rastreo GPS automático del vehículo | El estado lo actualiza **el propio motorista**. Un dispositivo GPS podría integrarse después |

## Restricciones que condicionan todo

1. **On-premise**, una instancia por institución, en servidores internos.
2. **Sin equipo de TI dedicado.** Instalación, respaldo y restauración deben poder ejecutarlos alguien con conocimientos generales siguiendo un documento.
3. **Conectividad intermitente o nula** en delegaciones y en carretera.
4. **Rotación de personal alta.** Roles por puesto, no por persona.
5. **Todo puede ser requerido por el TSC.** La trazabilidad prevalece sobre la comodidad en los puntos críticos.
6. **El stack tecnológico está diferido al Sprint 2** — [ADR-000](../03-arquitectura/adr/ADR-000-diferir-seleccion-de-stack.md).

## Cómo sabremos que funcionó

No por las pantallas entregadas. Por esto:

- El encargado de transporte se va de vacaciones y **la operación no se detiene**.
- Llega una solicitud de Auditoría Interna sobre un vehículo y **se responde el mismo día**.
- Un motorista pasa cuatro días en La Mosquitia sin señal y **su bitácora llega completa**.
- Sube la tarifa de peaje y **alguien de la institución carga la tabla nueva sin llamar al desarrollador**.
- Alguien intenta asignar un motorista sin licencia habilitante y **el sistema no lo deja**.
