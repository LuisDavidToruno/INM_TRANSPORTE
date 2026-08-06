# Plantilla — Requisito no funcional

Archivo: `docs/02-requisitos/no-funcionales/RNF-xx-slug-corto.md`

Un requisito no funcional que no se puede medir es una aspiración. **Todo `RNF-xx` lleva un número y una forma de comprobarlo.** "El sistema debe ser rápido" no es un requisito; "toda pantalla de consulta responde en menos de 2 segundos con 50,000 órdenes de misión en base" sí lo es.

---

## Esqueleto

```markdown
# RNF-xx — <Enunciado en una línea>

| Campo | Valor |
|---|---|
| **Categoría** | Disponibilidad / Rendimiento / Seguridad / Auditoría / Usabilidad / Operabilidad / Portabilidad |
| **Prioridad** | Crítico / Alto / Medio |
| **Origen** | <norma, restricción operativa o decisión> |
| **Afecta arquitectura** | Sí / No |

## Enunciado
## Métrica y umbral
## Cómo se verifica
## Consecuencia de no cumplirlo
## Trazabilidad
```

---

## Ejemplo completo

# RNF-03 — El cliente de campo opera sin conectividad durante toda una misión

| Campo | Valor |
|---|---|
| **Categoría** | Disponibilidad / Operabilidad |
| **Prioridad** | Crítico |
| **Origen** | Realidad de conectividad rural hondureña — ver [NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md) |
| **Afecta arquitectura** | **Sí** — es determinante para el `ADR-001` de selección de stack |

## Enunciado

El cliente de campo **debe** permitir al motorista y al encargado de delegación registrar la operación completa de una misión —salida, odómetros, paradas, arribos, entregas, consumo de combustible, incidentes y fotografías— **sin ninguna conectividad**, durante al menos **7 días continuos**, y sincronizar sin pérdida de datos al reconectar.

Esto es **offline-first**, no "soporte offline": la ausencia de red es el estado normal esperado en operación de campo, no una degradación.

## Métrica y umbral

| Métrica | Umbral |
|---|---|
| Duración máxima sin conectividad conservando plena funcionalidad de captura | ≥ 7 días |
| Órdenes de misión almacenables localmente sin sincronizar | ≥ 20 |
| Fotografías almacenables localmente | ≥ 200, con compresión automática |
| Pérdida de datos tras sincronización | **0 registros. Sin excepción.** |
| Sobrescritura silenciosa de un dato en conflicto | **0. Todo conflicto va a cola de resolución humana.** |
| Tiempo de sincronización de una misión completa con 20 fotos, en 3G | < 3 minutos |
| Funciones que requieren conectividad obligatoria | Solo autorización de nuevas solicitudes y consulta de datos que no estaban precargados |

## Cómo se verifica

1. **Prueba de campo simulada**: se configura un dispositivo con una misión de 5 días y 4 destinos, se pone en modo avión, se registra la operación completa incluidas 30 fotos, y se reconecta. Se compara registro a registro contra lo capturado.
2. **Prueba de conflicto**: el mismo vehículo recibe registros desde dos dispositivos distintos sin conexión entre sí. Se verifica que ambos lleguen al servidor, que ninguno se pierda, y que el conflicto aparezca en la cola de resolución con ambas versiones visibles.
3. **Prueba de interrupción**: se corta la red a mitad de una sincronización. Se verifica que al reintentar no se dupliquen registros ni se pierdan los que ya habían subido.
4. **Prueba de volumen**: 20 misiones acumuladas sin sincronizar; se mide tiempo y consumo de almacenamiento.

## Consecuencia de no cumplirlo

El sistema no se usa en campo. El motorista vuelve al papel, el encargado de delegación digita a destiempo desde memoria, y el registro pierde el valor probatorio que exige la norma TSC-NOGECI V-10 sobre registro oportuno — que es precisamente la razón de existir del sistema.

Este es el requisito que más probabilidades tiene de decidir el fracaso del proyecto si se subestima.

## Trazabilidad

- Módulos: M-08, M-16
- Historias: `HU-060` a `HU-068`
- Casos especiales: `CE-18` (bitácora en papel por falta de señal), `CE-07` (avería en ruta sin señal)
- Decide: `ADR-001` (stack), `ADR-004` (estrategia de sincronización), `ADR-005` (identificadores generados en cliente)
