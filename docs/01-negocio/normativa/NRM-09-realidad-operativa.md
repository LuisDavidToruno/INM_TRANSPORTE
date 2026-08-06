# NRM-09 — Realidad operativa: conectividad, feriados, horarios y prácticas de control

| Campo | Valor |
|---|---|
| **Ámbito** | Condiciones de campo que restringen el diseño tanto o más que la norma escrita |
| **Módulos afectados** | M-08, M-09, M-10, M-15, M-16 |
| **Última verificación** | 2026-08-06 |
| **Riesgo de cambio** | Medio |

Esta ficha no describe una norma sino el **terreno**. En este proyecto la conectividad no es una variable de riesgo: es una restricción de arquitectura.

## Conectividad — datos verificados

- **INE, EPHPM julio 2025** `[V]`: el acceso a internet en el área urbana es del **64.7%**. **Más de 2 millones** de personas de 5 años y más del área rural (de una población rural de 5+ años superior a 3.9 millones) **no tienen acceso a internet**.
- **Brecha de dispositivos** `[V]`: radio o equipo de sonido 63% urbano vs. 37% rural; televisor 68.4% urbano vs. 31.6% rural. La brecha no es solo de red, también de equipamiento.
- `[P]` Cobertura 4G en distritos aislados alrededor del **50.7%** (fuente comercial).
- Honduras figura entre los países de la región con mayores retos en cobertura rural. `[V]`

## Feriados nacionales `[V]`

**Artículo 339 del Código del Trabajo**: 1 de enero, 14 de abril, 1 de mayo, 15 de septiembre, 3 de octubre, 12 de octubre, 21 de octubre y 25 de diciembre — aunque caigan en domingo — más **jueves, viernes y sábado de Semana Santa**. Aplica a sector público y privado. Si se labora, se paga con el duplo del salario ordinario.

`[C]` Existe legislación posterior (comúnmente llamada "feriado morazánico") que reagrupó los feriados de octubre en un bloque único. **No se pudo verificar.** Confirmar antes de codificar cualquier calendario.

## Semana Santa como evento de control `[V]`

El TSC realiza operativos de fiscalización vehicular específicamente en Semana Santa (informes E-001-2015-DFBN, E-007-2015-FBN, comunicados 2026). Es el **pico anual de riesgo de hallazgo por uso indebido** — y es predecible, así que el sistema puede prepararse.

## Control de combustible

- Las instituciones licitan el suministro de combustible `[V]` (ejemplos verificados: Secretaría de Relaciones Exteriores, Secretaría de Gobernación).
- El Decreto 157-2022 permitió compra directa de combustible para la flota estatal en 2023 `[V]`.
- `[I]` El mecanismo predominante sigue siendo el **vale o cupón físico** entregado al motorista contra requisición, canjeado en estación de servicio del proveedor contratado. Los sistemas de tarjeta de flota electrónica existen comercialmente, pero `[C]` su adopción en el sector público hondureño.

## Otras condiciones `[I]`

- **Horario de la administración pública**: típicamente 8:00–16:00 de lunes a viernes. Esto define qué es "hora inhábil" para el permiso de circulación. `[C]` el horario oficial vigente de la institución, incluidos horarios especiales.
- **Rotación de personal**: alta, especialmente tras cambios de administración. Honduras celebró elecciones generales en noviembre de 2025 con cambio de gobierno en enero de 2026 — la institución probablemente atraviesa una rotación reciente y significativa.
- **Prácticas en papel**: los formatos preimpresos de bitácora, requisición y salida de vehículo son la norma. El sistema **no debe exigir que desaparezcan de inmediato**.

## Implicaciones de requerimiento

- **El sistema debe** ser **offline-first en el cliente de campo**, no "con soporte offline". El motorista y el encargado de delegación deben registrar salida, bitácora, consumo, incidentes y fotos **sin ninguna conectividad**, y sincronizar cuando haya red.
- **El sistema debe** resolver conflictos de sincronización con **reglas deterministas y sin pérdida de datos**: identificadores generados en el cliente (UUID), marca de tiempo del dispositivo y del servidor, y **cola de conflictos para resolución humana** en lugar de sobrescritura silenciosa.
- **El sistema debe** permitir **emisión anticipada de documentos** (orden de misión, salvoconducto, vale) con folio pre-asignado del rango de la delegación, para imprimirlos antes de salir a zona sin cobertura.
- **El sistema debe** mantener **paridad entre el formulario en pantalla y el formato impreso** — mismos campos, mismos nombres, mismo orden. Esto reduce el costo de adopción más que cualquier funcionalidad.
- **El sistema debe** permitir **digitación diferida de formularios en papel** por un encargado de delegación, con constancia de quién digitó, cuándo, y adjunto del original escaneado o fotografiado, distinguiendo la fecha del hecho de la fecha de captura.
- **El sistema debe** llevar un **calendario configurable de días hábiles, feriados y horario laboral** por institución y por delegación, usado para calcular noches de viático, determinar si una salida requiere permiso de día inhábil, y calcular plazos de liquidación. **Nunca cablear los feriados.**
- **El sistema debe** manejar **vales o cupones de combustible como objetos con folio, estado y ciclo de vida**: emitido → entregado al motorista con firma → canjeado con factura del proveedor → conciliado; o bien anulado o extraviado con acta. Debe soportar tanto vale físico como, eventualmente, tarjeta de flota.
- **El sistema debe** registrar **odómetro de salida y de retorno** obligatoriamente, calcular rendimiento km/galón por vehículo, y detectar lecturas inconsistentes: retroceso de odómetro, saltos imposibles, y rendimientos anómalos **en ambas direcciones**.
- **El sistema debe** absorber la **rotación de personal**: roles y permisos **por puesto, no por persona**; traspaso masivo de custodias; reasignación de expedientes abiertos; e inducción guiada dentro del propio sistema.
- **El sistema debe** anticipar la **fiscalización de Semana Santa** con un reporte específico: vehículos autorizados a circular con su permiso, y vehículos que deben estar resguardados con confirmación de resguardo.
- **El sistema debe** desplegarse **on-premise con requisitos modestos**, respaldo automatizado local y procedimiento de restauración documentado y **probado** — asumiendo que no habrá equipo de TI dedicado en las delegaciones.
- `[C]` **El sistema podría** ofrecer un canal degradado por SMS o llamada para autorizaciones urgentes en ruta, con registro posterior en el expediente. Confirmar viabilidad con la institución.

## Zonas grises y pendientes

- `[C]` Verificar la legislación de feriados de octubre antes de codificar el calendario.
- `[C]` Horario oficial y días hábiles de la institución piloto.
- `[C]` Mapa de delegaciones y su situación real de conectividad.
- `[C]` Mecanismo real de control de combustible que usa la institución hoy.

## Fuentes

- [INE — Conectividad digital en Honduras, julio 2025](https://ine.gob.hn/wp-content/uploads/2025/12/Conectividad-digital-en-Honduras-julio-2025.pdf) — consultado 2026-08-06
- [Dos millones de hondureños sin acceso a internet en zonas rurales](https://www.elheraldo.hn/honduras/dos-millones-hondurenos-area-rural-no-tienen-acceso-a-internet-BD22742113) — consultado 2026-08-06
- [Días feriados en Honduras — Art. 339 Código del Trabajo](https://central-law.com/honduras-laboral-dias-feriados-navidenos-y-obligaciones-patronales-conforme-al-codigo-de-trabajo/) — consultado 2026-08-06
- [TSC — Informe 002-2023-DFBN, operativos de fiscalización vehicular](https://www.tsc.gob.hn/wp-content/uploads/002-2023-DFBN-1.pdf) — consultado 2026-08-06
