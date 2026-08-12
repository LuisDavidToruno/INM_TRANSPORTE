# Casos especiales `CE-xx`

28 situaciones de la **operación real** que el flujo feliz no contempla. No son errores del usuario ni casos de prueba: son cosas que pasan, que hoy se resuelven con papel y criterio, y que el sistema tiene que absorber sin trabar la operación.

**Ningún caso se cierra sin regla de resolución.** Lo que no se supo resolver está escalado al PO marcado `[C]`, con opciones y su costo — no resuelto por inferencia.

Plantilla: [`docs/plantillas/caso-especial.md`](../../plantillas/caso-especial.md).

## Ejecución en ruta

| ID | Caso |
|---|---|
| [CE-01](CE-01-salida-de-emergencia-convalidada.md) | Salida de emergencia sin autorización previa, convalidada después |
| [CE-02](CE-02-averia-mecanica-en-ruta.md) | Avería mecánica que impide continuar |
| [CE-03](CE-03-accidente-de-transito-en-mision.md) | Accidente de tránsito, con o sin terceros y lesionados |
| [CE-04](CE-04-robo-de-vehiculo-o-de-carga-en-mision.md) | Robo del vehículo o de la carga |
| [CE-05](CE-05-cambio-de-motorista-con-la-mision-en-curso.md) | Relevo de motorista con la misión en curso |
| [CE-06](CE-06-la-mision-se-extiende-mas-dias-destinos-o-kilometros.md) | La misión se extiende: más días, destinos o kilómetros |
| [CE-07](CE-07-retorno-anticipado-la-mision-se-aborta.md) | Retorno anticipado: la misión se aborta |
| [CE-08](CE-08-multi-destino-con-esperas-prolongadas-en-sitio.md) | Multi-destino con esperas prolongadas en sitio |
| [CE-09](CE-09-bitacora-en-papel-digitada-dias-despues.md) | Zona sin señal: bitácora en papel, digitada días después |
| [CE-10](CE-10-motorista-incapacitado-en-ruta.md) | El motorista se incapacita en carretera |

## Recursos, flota y habilitación

| ID | Caso |
|---|---|
| [CE-11](CE-11-licencia-vence-durante-la-mision.md) | La licencia vence dentro del rango de una misión programada |
| [CE-12](CE-12-dos-solicitudes-compiten-por-el-mismo-vehiculo.md) | Dos solicitudes aprobadas compiten por el único vehículo compatible |
| [CE-13](CE-13-motorista-no-disponible-por-talento-humano.md) | Motorista no disponible: permiso, vacaciones o incapacidad |
| [CE-14](CE-14-vehiculo-prestado-entre-dependencias-o-instituciones.md) | Vehículo prestado entre dependencias o instituciones |
| [CE-15](CE-15-vehiculo-en-comodato-o-alquilado.md) | Vehículo en comodato o alquilado |
| [CE-16](CE-16-vehiculo-a-taller-con-misiones-programadas.md) | Entra a taller con misiones ya programadas |
| [CE-17](CE-17-vehiculo-sin-placa-metalica.md) | Sin placa metálica, por el desabastecimiento nacional |
| [CE-18](CE-18-carga-y-pasajeros-en-la-misma-mision.md) | Carga y pasajeros en la misma misión, con requisitos que compiten |
| [CE-19](CE-19-vehiculo-asignado-a-funcionario-frente-al-pool.md) | Vehículo asignado a funcionario, frente al vehículo de pool |

## Combustible, peajes, liquidación y cierre

| ID | Caso |
|---|---|
| [CE-20](CE-20-mision-cancelada-con-combustible-ya-entregado.md) | Misión cancelada con el combustible ya entregado |
| [CE-21](CE-21-galonaje-que-no-cuadra-con-kilometraje.md) | El galonaje no cuadra con el kilometraje |
| [CE-22](CE-22-odometro-inconsistente.md) | Odómetro inconsistente: retroceso, salto imposible, tablero reemplazado |
| [CE-23](CE-23-fondo-agotado-con-misiones-programadas.md) | El fondo se agota con misiones ya programadas |
| [CE-24](CE-24-cobro-en-categoria-de-peaje-equivocada.md) | En la caseta cobran una categoría de peaje que no corresponde |
| [CE-25](CE-25-comprobante-perdido-o-estacion-sin-factura.md) | Comprobante perdido, o estación que no da factura |
| [CE-26](CE-26-sobrante-o-faltante-al-liquidar.md) | Sobra o falta dinero al liquidar |
| [CE-27](CE-27-cierre-de-ejercicio-fiscal-con-hallazgo-abierto.md) | Cierre de ejercicio fiscal con hallazgo abierto |
| [CE-28](CE-28-hallazgo-posterior-sobre-mision-cerrada.md) | Hallazgo descubierto meses después, sobre una misión ya `CERRADA` |

## Los que hay que leer aunque no se lea el resto

**[CE-09](CE-09-bitacora-en-papel-digitada-dias-despues.md) — decide la adopción del sistema.** Más de 2 millones de personas del área rural hondureña no tienen acceso a internet. Si este caso se resuelve mal, el motorista vuelve al papel y todo lo demás da igual.

**[CE-28](CE-28-hallazgo-posterior-sobre-mision-cerrada.md) — decide si los reportes son reproducibles.** `CERRADA` no se reabre, ni por auditoría. Basta con que la reapertura exista para que se use, y basta con que se use una vez para que ningún reporte histórico vuelva a ser reproducible. La salida es expediente de hallazgo posterior con asientos reversos.

**[CE-12](CE-12-dos-solicitudes-compiten-por-el-mismo-vehiculo.md) — el `[C]` que aparece la primera semana.** El criterio de prelación no está definido (insumo #31). Sin criterio explícito, lo resuelve quien tenga más jerarquía — que es exactamente lo que el sistema debería evitar.

## Lo que este bloque produjo hacia atrás

Escribir los casos obligó a recorrer el diseño del Bloque 1 con situaciones reales en la mano, y ahí aparecieron huecos que la revisión adversarial no había encontrado:

| Hallazgo | Dónde | Corregido |
|---|---|---|
| `T-17` no revalidaba `BD-02` ni `BD-03` en la **prórroga**, solo en el relevo. Una misión prorrogada podía circular con licencia vencida | Máquina de estados | ✅ |
| `actores-y-roles.md` §4.2 daba a `ACT-09` poder de **reabrir una misión `CERRADA`**, contra la máquina de estados | Actores y roles | ✅ |
| `BD-04` vs. `PC-03`: el salvoconducto ampara *"vehículo y ventana"* o *"vehículo, motorista y ventana"*, según el documento | Máquina de estados vs. `PR-01` | ⬜ pendiente |
| Faltan estados de vehículo: `PRESTADO_A_OTRA_INSTITUCION` y `RETIRADO_DE_FLOTA` — declarar *dado de baja* un bien ajeno es un asiento falso | Máquina de estados | ⬜ pendiente |
| `T-18` no tipifica *retorno del personal con vehículo resguardado en sitio* | Máquina de estados | ⬜ pendiente |

## Reglas candidatas

Los casos detectaron **más de 120 reglas candidatas** que las 54 existentes no cubren. **Ninguna se dio por escrita.** Se consolidan y numeran en el Bloque 3, cuando se sepa cuáles sobreviven al contraste entre casos.

Las de mayor retorno según los analistas:

- **Unicidad del comprobante a nivel institución** — un mismo recibo sosteniendo dos consumos en dos delegaciones se detecta al registrarlo, no ocho meses después conciliando a mano.
- **Categoría de peaje y tarifa esperada impresas en la Orden de Misión** — el motorista llega a la caseta con el papel en la mano y resuelve la discrepancia donde ocurre.
- **Fecha de corte de conocimiento en todo reporte** — sin ella, no reabrir el expediente no sirve de nada, porque el reporte cambia igual.
- **El retorno constatado libera al vehículo** sin esperar la digitación — evita que la delegación salga sin orden de misión porque el sistema tiene el vehículo secuestrado por un trámite.
