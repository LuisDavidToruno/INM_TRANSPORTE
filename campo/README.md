# Cliente de campo — núcleo offline

Lo que el motorista lleva a La Mosquitia. Aquí vive **el núcleo**, no la aplicación.

## Qué es esto, y qué no

**Es** la lógica que `RNF-03` no perdona: capturar sin red, sincronizar sin perder nada, y **no sobrescribir nunca en silencio**. TypeScript puro, sin dependencias de plataforma, con pruebas que corren en cualquier máquina con Node.

**No es** la aplicación Android. [`ADR-003`](../docs/03-arquitectura/adr/ADR-003-cliente-de-campo-instalado.md) decide **React Native con SQLite cifrado (SQLCipher) como fuente de verdad local** — no como caché. Esa cáscara no está escrita.

### Por qué se separó así

Porque **el núcleo se puede probar y la cáscara no**, al menos no aquí: esta máquina no tiene Android SDK, ni emulador, ni Java. Escribir la app entera sin poder ejecutarla habría producido cientos de líneas que nadie vio funcionar.

Y hay una razón mejor que la circunstancia: **la regla de qué se captura, qué queda pendiente y qué es conflicto es la misma con o sin disco**. Separarla del almacenamiento la vuelve comprobable en 70 milisegundos en vez de en un dispositivo.

```bash
cd campo && npm run verificar
```

## Lo que hay

| Pieza | Qué defiende |
|---|---|
| [`DiarioLocal`](nucleo/DiarioLocal.ts) | `P-1` — el dispositivo manda **transiciones**, nunca «el estado». Y `RNF-03`: lo que el servidor no acusó **sigue pendiente**, así que una sincronización cortada a la mitad no pierde nada |
| [`Conciliacion`](nucleo/Conciliacion.ts) | [`RN-45`](../docs/01-negocio/reglas/RN-45-cero-sobrescritura-silenciosa.md) — **cero sobrescritura silenciosa**. Dos versiones distintas del mismo hecho conservan las dos y van a cola humana |
| [`SubrangoDeFolios`](nucleo/Folios.ts) | [`RN-44`](../docs/01-negocio/reglas/RN-44-identificadores-y-folios-en-el-cliente.md) y `RNF-21` — el folio se toma **sin consultar al servidor**, y el subrango es **del dispositivo, no de la delegación** |
| [`AlmacenSqlite`](nucleo/AlmacenSqlite.ts) | [`ADR-003`](../docs/03-arquitectura/adr/ADR-003-cliente-de-campo-instalado.md) — **fuente de verdad local, no caché**. Lo capturado sobrevive a que Android mate el proceso, que en gama baja ocurre sin avisar |
| [`SalidaYRetorno`](nucleo/SalidaYRetorno.ts) | `T-14` y `T-18` con **nivel de tanque** ([`RN-83`](../docs/01-negocio/reglas/RN-83-todo-ingreso-de-combustible-es-un-abastecimiento.md)). *«No lo leí»* y *«marcaba cero»* son cosas opuestas, y un campo numérico vacío no las distingue: `RN-80` manda declarar el campo no consignado y **no estimarlo** |
| [`AbastecimientoEnRuta`](nucleo/AbastecimientoEnRuta.ts) | [`RN-83`](../docs/01-negocio/reglas/RN-83-todo-ingreso-de-combustible-es-un-abastecimiento.md) — el galón que **no salió del vale**. El motorista que llena de una donación camino a La Mosquitia, o que pone de su bolsillo porque el vale no alcanzó, no tenía dónde anotarlo: ese galón no llegaba al denominador de `RN-30`, y su ausencia se lee como rendimiento imposiblemente bueno |
| [`ConsumoEnRuta`](nucleo/ConsumoEnRuta.ts) | §10.1 — **`V-04` se ejecuta sin conectividad**. La estación camino a La Mosquitia no tiene señal, y un consumo capturado de memoria tres días después llega sin odómetro, que es el dato con el que `RN-30` sabe *dónde* se fue la diferencia |
| [`ColaDeAdjuntos`](nucleo/ColaDeAdjuntos.ts) | [`RN-43`](../docs/01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md) y [`ADR-004`](../docs/03-arquitectura/adr/ADR-004-adjuntos-fuera-de-la-base.md) — los adjuntos van en **su propia cola** y no retienen al hecho que respaldan |

### El corte del cifrado, dicho sin adornos

`ADR-002` y `ADR-003` deciden **SQLCipher**, que es una compilación distinta de SQLite: se abre con `PRAGMA key` y **desde ahí todo el SQL es idéntico**. Node trae SQLite sin cifrar.

Así que lo que está probado es **el esquema, las consultas y la durabilidad** — que es exactamente lo que corre en el dispositivo. Lo que **no** está probado es que el archivo quede ilegible sin la clave. Eso se verifica en el dispositivo, abriéndolo sin clave, y **no está hecho**.

El corte es defendible porque la superficie que el cifrado cambia es una línea. Lo que no sería defendible es dar por probado el cifrado porque las consultas funcionan.

### La decisión que más costará entender dentro de un año

**No hay «gana el más reciente».** Es la regla automática obvia y es la peor: el reloj del dispositivo se puede alterar, y el hecho capturado después no es el que ocurrió después.

Lo que está en conflicto en este dominio son **odómetros, galones y montos**. Una sobrescritura automática destruye el término de una conciliación de auditoría **y nadie se entera** — [`ADR-001`](../docs/03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md) lo llama *«la peor forma de fallar»*.

El caso concreto que esto atrapa: el motorista registra el retorno con 84.320 km; el encargado lo digita del papel con 84.302, una transposición al leer una hoja mojada. **Nadie notaría los 18 km de diferencia**, y esos 18 km son el denominador de [`RN-30`](../docs/01-negocio/reglas/RN-30-conciliacion-galonaje-kilometraje.md).

## El servidor ya recibe

`POST /sincronizacion` existe y es **idempotente**: el dispositivo que no supo si el servidor recibió reenvía, y el servidor lo reconoce en vez de duplicar. La unicidad la impone **la base** con un índice único sobre `IdDeCaptura` — no una comprobación previa, que sería una condición de carrera con dos lotes del mismo dispositivo en vuelo.

**El nivel de tanque viaja con `T-14` y `T-18`**, y hasta hoy no llegaba: la API lo aceptaba,
pero la ruta de sincronización —la única que este cliente usa— construía el odómetro sin él. El
dato se tecleaba en el predio, se sincronizaba, y **no aparecía en ninguna parte**.

> Es el peor de los tres modos de fallar: no hay error, no hay hueco visible, y el reparo de
> [`RN-30`](../docs/01-negocio/reglas/RN-30-conciliacion-galonaje-kilometraje.md) que depende
> del nivel nunca se activaba porque el nivel nunca estaba.

**Y desde ahora también `V-04` y `A-01`.** El primero es el consumo del vale; el segundo, el
combustible de cualquier **otra** fuente.

> ⚠️ **`A-01` no es una transición de ninguna máquina de estados.** Un abastecimiento no mueve
> un expediente ni un vale: es un registro que cuelga del vehículo, y puede llegar **sin
> misión** —el reabastecimiento de rutina en el predio—. Viaja por el mismo canal porque eso
> es lo que da **una sola cola, una sola idempotencia y un solo acuse**; abrirle un endpoint
> propio duplicaría los tres, y son justo los tres que `RNF-03` obliga a que funcionen sin
> fallo.

**Sobre `V-04`:** el consumo del vale. No es una transición más: es de
**otro agregado** —el vale, no la misión—, así que viaja con `idAsignacion` y su carga. Mandar
sólo el expediente obligaría al servidor a adivinar a cuál de los vales de la misión cargarle
el galón, y adivinar sobre dinero es lo que el folio existe para impedir.

> ⚠️ **La idempotencia estaba rota, y `V-04` lo destapó.** La comprobación de «esto ya llegó»
> usaba un `Contains` sobre `IdDeCaptura`, que lleva convertidor de valor a `binary(16)`. Con
> `UseCompatibilityLevel(120)` esa traducción **devuelve vacío en vez de fallar**: la consulta
> corría, no encontraba nada, y cada reenvío pasaba por nuevo.
>
> En las transiciones de misión no se notaba porque **la máquina de estados frenaba el
> duplicado** —`T-14` sobre una misión ya en ruta es inválida— y el hecho terminaba en
> `rechazadas`. La prueba que existía contaba transiciones y daba 1, así que pasaba por el
> motivo equivocado. Pero **un hecho rechazado nunca se acusa**, y el dispositivo lo
> reintentaría para siempre: justo lo que `RNF-03` existe para impedir.
>
> Con `V-04` se vio de golpe, porque un vale admite varias cargas y ahí no hay máquina de
> estados que lo frene: el duplicado llegaba hasta el índice único y devolvía un 500.

**El lote no es atómico, a propósito.** Que una transición no entre no puede impedir que las otras seis sí: el dispositivo lleva siete días de trabajo encima, y perderlo todo por un expediente inexistente sería el fallo que este endpoint existe para evitar. La respuesta separa `aplicadas`, `yaConocidas` y `rechazadas`, y las dos primeras son lo que el dispositivo puede sacar de su cola.

`POST /adjuntos` recibe el binario **como formulario, no como JSON**: en base64 crecería un 33 %, y sobre la red de un retén ese tercio se paga en tiempo y en batería. El archivo va al sistema de archivos por año y mes —**fecha del hecho, no de subida**, porque `P-4` manda y siete días sin red no cambian a qué mes pertenece una foto—, y a la base va solo su rastro.

**El hash se verifica al recibir, no solo se guarda.** Guardarlo sin comprobarlo lo volvería decorativo: un archivo truncado por la red de un retén quedaría registrado como íntegro, y el defecto aparecería meses después al armar el paquete de evidencia — cuando ya no se puede volver a tomar la foto. El rechazo devuelve **los dos hashes**, para que se pueda diagnosticar en vez de adivinar.

## Lo que falta

| Qué | Dónde |
|---|---|
| **La aplicación React Native** — pantallas, cámara, GPS en segundo plano | Necesita máquina con Android SDK y Java |
| **El cifrado en reposo** | El esquema, las consultas y la durabilidad están probados; **que el archivo quede ilegible sin la clave, no**. Se verifica en el dispositivo |
| **La asignación de subrangos** — quién los reparte, cuándo se recargan, y el aviso antes de que un dispositivo salga con el saldo bajo | El consumo ya está; **repartirlos es de `M-01`**, que no existe |
| **La compresión automática** que `RNF-03` exige para llegar a ≥ 200 fotografías | Es de la cámara, en el módulo nativo |
| **El respaldo de dos piezas** — base y almacén de archivos, **consistentes entre sí** | `ADR-004` lo exige *«desde el principio, no adaptado después»*, y `RNF-09` da 2 h a personal no especialista. No está escrito |
| **La cola de resolución de conflictos** | El núcleo los **detecta** (`Conciliacion`) y el servidor todavía no los recibe. La cola es de `M-16` |
| **El catálogo de causas sin comprobante** | `RN-85` lo exige y la institución no lo ha entregado. Hoy la causa es **texto libre**: se registra y se conserva, pero no se puede consultar por tipo |
| **`RNF-12`** — ≤ 25 % de batería en 8 h, gama baja | **No se ha medido nada.** Es el requisito más ajustado del sistema |

## Por qué Node corre esto sin compilar

Node 24 quita los tipos de TypeScript de forma nativa, así que `node --test` ejecuta los `.prueba.ts` directamente. **Sin runner, sin transpilador, sin configuración de build** — lo que importa en un módulo cuya gracia es no depender de la plataforma.

Por eso `tsconfig.json` lleva `erasableSyntaxOnly`: prohíbe la sintaxis de TypeScript que **no** se puede borrar sin compilar —`enum`, `namespace`, parámetros con `private`—, y así el compilador impide de antemano que alguien escriba algo que Node no podría ejecutar.
