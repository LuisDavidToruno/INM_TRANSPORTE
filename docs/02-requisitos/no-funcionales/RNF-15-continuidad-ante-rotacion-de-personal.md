# RNF-15 — Un cambio de administración no deja expedientes huérfanos ni reescribe quién hizo qué

| Campo | Valor |
|---|---|
| **Categoría** | Operabilidad / Auditoría |
| **Prioridad** | Alto |
| **Origen** | [NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md): rotación alta, especialmente tras cambio de administración — Honduras tuvo elecciones en noviembre de 2025 y cambio de gobierno en enero de 2026 `[V]` |
| **Afecta arquitectura** | **Sí** — obliga a separar *persona*, *puesto* y *autoría histórica* en el modelo de datos. No determinante de stack |

## Enunciado

El sistema **debe** absorber la rotación de personal sin interrumpir la operación y **sin alterar la autoría histórica**. Cuando un motorista, un encargado de delegación o un jefe de transporte deja su puesto:

1. Sus **permisos se extinguen** con el puesto, no con su persona ([`RNF-14`](RNF-14-control-de-acceso-por-puesto-y-registro-de-consultas.md)).
2. Sus **custodias de vehículos** se traspasan en bloque, con acta y receptor identificado.
3. Sus **expedientes abiertos** —misiones en curso, liquidaciones pendientes, incidentes en investigación— se reasignan sin quedar sin responsable.
4. Su **autoría sobre los registros pasados no cambia jamás**. El asiento de hace tres años sigue diciendo su nombre y el puesto que ocupaba entonces.

El punto 4 es el que se rompe con más frecuencia y el más caro: si el nombre del autor se resuelve mirando el puesto actual de esa persona, el día que alguien asciende **cambia la historia entera de sus asientos**. Y si el usuario se elimina al darse de baja, los asientos quedan firmados por un identificador vacío.

## Métrica y umbral

| Métrica | Umbral |
|---|---|
| Usuarios eliminados físicamente al causar baja | **0.** Se desactivan; su identidad se conserva para la autoría ([`RNF-02`](RNF-02-volumen-y-crecimiento-del-acervo.md)) |
| Asientos históricos cuyo autor o puesto de autor cambie tras una reorganización, ascenso o baja | **0** |
| Puesto mostrado en un asiento antiguo | El vigente **a la fecha del hecho**, resuelto por vigencia ([`RNF-05`](RNF-05-temporalidad-normativa.md)) |
| Traspaso masivo de custodias entre dos motoristas | Una sola operación, con acta generada, para ≥ 50 vehículos, en ≤ 5 min |
| Vehículos que queden sin custodio tras una baja | **0.** El sistema no permite completar la baja sin destino de las custodias |
| Expedientes abiertos que queden sin responsable tras una baja | **0.** Se listan al iniciar la baja y exigen reasignación explícita, uno por uno o en bloque |
| Misiones `EN_RUTA` afectadas por la baja del responsable | Se marcan y escalan al superior del puesto; **no se bloquea la operación en carretera** |
| Puesto vacante que impida operar | **0.** Existe suplencia con vigencia por rango de fechas, receptor identificado y registro — nunca "prestar el usuario" |
| Cuentas compartidas creadas para cubrir una vacante | **0** ([`RNF-14`](RNF-14-control-de-acceso-por-puesto-y-registro-de-consultas.md)) |
| Tiempo de la inducción guiada dentro del sistema para un puesto operativo | ≤ 30 min `[C]`, medido con una persona nueva real |
| Tiempo hasta que un encargado de delegación recién nombrado despacha su primera misión sin acompañamiento | ≤ 1 jornada `[C]` |
| Reasignaciones de custodia o de expediente sin asiento con motivo y autor | **0** |

## Cómo se verifica

1. **Prueba del cambio de administración** — se simula el escenario real, no uno cómodo:
   - Se dan de baja simultáneamente 10 usuarios, entre ellos un jefe de transporte con 3 misiones `EN_RUTA`, 2 liquidaciones pendientes y 40 vehículos bajo custodia.
   - Se verifica: cero expedientes huérfanos, cero vehículos sin custodio, actas generadas, accesos revocados, y las misiones en ruta escaladas sin interrumpirse.
2. **Prueba de la historia inmutable**: se toma un asiento de un usuario, se le cambia el puesto, se le da la baja, y se vuelve a consultar el mismo asiento. Nombre y puesto deben ser idénticos a la primera consulta. Se compara además el hash de la cadena ([`RNF-04`](RNF-04-bitacora-append-only-con-hash-encadenado.md)), que no debe haberse alterado.
3. **Prueba del traspaso masivo**: 50 vehículos de un motorista a otro en una operación. Se cronometra, se revisa el acta generada y se verifica que cada vehículo tiene su asiento individual de cambio de custodia.
4. **Prueba de suplencia**: se declara una suplencia de 15 días con vigencia. Se verifica que el suplente puede operar dentro de la ventana, que **no** puede antes ni después, y que sus actos quedan firmados como suplente, no como el titular.
5. **Prueba de la persona nueva**: alguien que nunca vio el sistema completa la inducción guiada y ejecuta una solicitud, un despacho y una liquidación. Se cronometra y se anota dónde se detiene.
6. **Prueba de baja incompleta**: se intenta dar de baja a un usuario dejando expedientes sin reasignar. El sistema debe impedirlo y decir exactamente qué falta.

## Consecuencia de no cumplirlo

Dos daños distintos:

- **Operativo**: tras el cambio de administración, las delegaciones quedan con vehículos sin custodio registrado y misiones sin responsable. La salida práctica del personal es compartir cuentas o pedir "que me pongan los permisos del anterior", y a partir de ahí el modelo de segregación de funciones es decorativo.
- **Probatorio, y este es irreversible**: si la autoría histórica se resuelve contra los datos actuales, cada ascenso reescribe el pasado. El día que Auditoría pregunte quién autorizó una salida en 2027, el sistema responderá con el puesto que esa persona tiene hoy. Un registro de auditoría que cambia con el organigrama no acredita nada, y no hay forma de reconstruirlo hacia atrás.

## Trazabilidad

- Módulos: M-01, M-03 (custodia), M-05
- Reglas: [`RN-22`](../../01-negocio/reglas/RN-22-custodia-del-vehiculo.md), [`RN-07`](../../01-negocio/reglas/RN-07-delegacion-de-autorizacion.md), [`RN-03`](../../01-negocio/reglas/RN-03-registro-inmutable-de-autorizacion.md), [`RN-14`](../../01-negocio/reglas/RN-14-sustitucion-de-motorista.md)
- Normativa: [NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md), [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md)
- Casos especiales: [`CE-05`](../casos-especiales/CE-05-cambio-de-motorista-con-la-mision-en-curso.md), [`CE-10`](../casos-especiales/CE-10-motorista-incapacitado-en-ruta.md), [`CE-19`](../casos-especiales/CE-19-vehiculo-asignado-a-funcionario-frente-al-pool.md)
- Requisitos relacionados: [`RNF-04`](RNF-04-bitacora-append-only-con-hash-encadenado.md), [`RNF-05`](RNF-05-temporalidad-normativa.md), [`RNF-14`](RNF-14-control-de-acceso-por-puesto-y-registro-de-consultas.md)
- Insumos: #27 (dotación real), #28 (autorizador alterno), #64 (asignación permanente de vehículo a funcionario)
