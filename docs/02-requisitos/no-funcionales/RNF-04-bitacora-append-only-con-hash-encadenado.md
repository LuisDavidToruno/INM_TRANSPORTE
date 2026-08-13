# RNF-04 — La bitácora de auditoría es append-only y detecta su propia alteración, incluso hecha por quien administra el servidor

| Campo | Valor |
|---|---|
| **Categoría** | Auditoría / Seguridad |
| **Prioridad** | Crítico |
| **Origen** | [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md) — pista de auditoría append-only exigida por el control interno del Estado |
| **Afecta arquitectura** | **Sí — determinante para la selección de stack (Sprint 2).** Condiciona el modelo de persistencia y obliga a un mecanismo de anclaje externo |

## Enunciado

Toda transacción del sistema **debe** dejar un asiento de auditoría con **quién, qué, cuándo, desde dónde, valor anterior y valor nuevo**. Los asientos **solo se agregan**: no se modifican ni se eliminan. Toda anulación es un **asiento reverso** con motivo y autor, conforme a [`RN-04`](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md).

Cada asiento incorpora el **hash de su contenido y el hash del asiento anterior**, formando una cadena. Alterar un asiento pasado rompe la cadena desde ese punto en adelante, y el sistema **debe** detectarlo.

### La parte incómoda: el administrador del servidor

Este requisito es exigente por una razón concreta. El despliegue es **on-premise** y el servidor pertenece a la institución. Quien administra la base de datos tiene, por definición, permiso para escribir en cualquier tabla. Una bitácora "inmutable" que vive únicamente dentro de esa base es inmutable frente al usuario de la aplicación y frente a nadie más.

**Sea explícito lo que se puede y lo que no se puede prometer:**

| Se puede garantizar | No se puede garantizar |
|---|---|
| Que ninguna ruta del sistema modifique o borre un asiento | Que nadie con acceso al motor de datos pueda intentarlo |
| Que una alteración posterior sea **detectable** y **datable** | Que sea **imposible** |
| Que el estado íntegro previo quede probado por un sello anclado fuera del alcance de ese administrador | Que el dato alterado se recupere sin restaurar respaldo |

La propiedad alcanzable es **detectabilidad con anclaje externo**, no inmutabilidad absoluta. Prometer lo segundo sería falso, y ante el TSC una promesa falsa cuesta más que una limitación declarada. Sin [firma electrónica certificada](../../01-negocio/normativa/NRM-08-firma-electronica.md) —descartada por decisión del PO— el anclaje es la única defensa real.

## Métrica y umbral

| Métrica | Umbral |
|---|---|
| Transacciones que producen asiento de auditoría | **100 %.** Ninguna operación de negocio escribe sin dejar asiento |
| Campos obligatorios por asiento | Los seis: autor (usuario y **puesto** al momento del hecho), entidad y operación, marca de tiempo del hecho y de captura, origen (dispositivo, dirección de red, delegación), valor anterior, valor nuevo |
| Asientos modificables o borrables desde cualquier funcionalidad del sistema | **0** |
| Asientos sin hash propio y sin hash del anterior | **0** |
| Frecuencia del sello de la cadena (hash raíz del período) | Diaria `[C]` — a confirmar con Auditoría Interna, insumo #71 |
| Destinos del sello, obligatoriamente **fuera del control del administrador de la base** | ≥ 2 de: constancia impresa firmada por el responsable de control interno, envío al sistema ARGOS, copia en el respaldo externo sellado |
| Tiempo de verificación completa de 1 año de cadena (≈ 800,000 asientos de `JDR-1`) | < 10 min |
| Detección de una alteración de un solo asiento en 4,000,000 | **100 %**, con identificación del asiento y del sello íntegro más reciente |
| Retraso máximo entre una alteración y su detección | ≤ 24 h (la verificación corre automáticamente a diario y su resultado aparece en la pantalla de estado del [`RNF-20`](RNF-20-observabilidad-y-diagnostico.md)) |
| Asientos escritos por operaciones ocurridas sin conectividad | 100 %, generados en el cliente y encadenados al integrarse al servidor sin reordenar los ya sellados |

## Cómo se verifica

1. **Cobertura**: se ejecuta el guion completo de una misión —solicitud, aprobación, programación, despacho, bitácora, combustible, peaje, retorno, liquidación, cierre— y se cuentan los asientos producidos contra la lista de operaciones ejecutadas. Toda operación sin asiento es un defecto bloqueante.
2. **Prueba de manipulación por el administrador** — la prueba que da sentido a este requisito:
   - Se genera un período completo de asientos y se emite su sello.
   - Un operador con **permisos totales sobre el motor de datos** ejecuta tres ataques distintos: (a) modificar el valor de un asiento intermedio, (b) eliminar un asiento intermedio, (c) insertar un asiento con fecha pasada entre dos existentes.
   - Se corre el verificador. **En los tres casos debe señalar el punto exacto de ruptura** y el último sello íntegro.
   - Se documenta el resultado con capturas. Es evidencia entregable a Auditoría Interna.
3. **Prueba de anulación**: se anula una orden de misión aprobada. Se verifica que el asiento original permanece intacto, que aparece el asiento reverso con motivo y autor, y que la cadena sigue verificando.
4. **Prueba del puesto histórico**: se consulta un asiento de hace tres años cuyo autor ya no trabaja en la institución y cuyo puesto fue reorganizado. Debe mostrar el puesto **que tenía al momento del hecho**, no el actual — ver [`RNF-15`](RNF-15-continuidad-ante-rotacion-de-personal.md).
5. **Prueba de origen desconectado**: se capturan asientos en tres dispositivos en modo avión y se sincronizan en orden inverso al de ocurrencia. La cadena del servidor debe cerrar correctamente y cada asiento conservar su marca de tiempo del hecho distinguida de la de captura.
6. **Prueba del sello**: se verifica que el sello del día está efectivamente en los dos destinos externos y que su valor coincide con el recalculado sobre la base.

## Consecuencia de no cumplirlo

El sistema deja de servir para lo único que lo hace obligatorio. Ante un hallazgo del Tribunal Superior de Cuentas, un registro sin cadena verificable tiene el mismo valor probatorio que una hoja de cálculo: la contraparte puede alegar que se editó después, y no hay forma de refutarlo.

Peor: como el sistema **parece** llevar auditoría, la institución sustituye sus controles en papel por él, y queda con menos defensa que antes de instalarlo. Un control de auditoría que no resiste la prueba es peor que no tenerlo, porque genera confianza que no respalda.

## Trazabilidad

- Módulos: M-14 (bitácora inmutable), transversal a todos
- Reglas: [`RN-03`](../../01-negocio/reglas/RN-03-registro-inmutable-de-autorizacion.md), [`RN-04`](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md), [`RN-05`](../../01-negocio/reglas/RN-05-registro-cerrado-no-se-edita.md), [`RN-08`](../../01-negocio/reglas/RN-08-cadena-de-trazabilidad-para-cierre.md), [`RN-46`](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md)
- Normativa: [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md), [NRM-08](../../01-negocio/normativa/NRM-08-firma-electronica.md)
- Casos especiales: [`CE-28`](../casos-especiales/CE-28-hallazgo-posterior-sobre-mision-cerrada.md), [`CE-27`](../casos-especiales/CE-27-cierre-de-ejercicio-fiscal-con-hallazgo-abierto.md)
- Requisitos relacionados: [`RNF-06`](RNF-06-reproducibilidad-historica-de-reportes.md), [`RNF-17`](RNF-17-retencion-y-depuracion-diferenciada.md), [`RNF-18`](RNF-18-paquetes-de-evidencia-para-auditoria.md), [`RNF-21`](RNF-21-integridad-de-folios-y-correlativos.md)
- Insumos: #71 (plazo de conservación y periodicidad del sello, con Auditoría Interna)
