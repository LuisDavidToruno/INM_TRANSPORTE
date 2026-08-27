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

### La decisión que más costará entender dentro de un año

**No hay «gana el más reciente».** Es la regla automática obvia y es la peor: el reloj del dispositivo se puede alterar, y el hecho capturado después no es el que ocurrió después.

Lo que está en conflicto en este dominio son **odómetros, galones y montos**. Una sobrescritura automática destruye el término de una conciliación de auditoría **y nadie se entera** — [`ADR-001`](../docs/03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md) lo llama *«la peor forma de fallar»*.

El caso concreto que esto atrapa: el motorista registra el retorno con 84.320 km; el encargado lo digita del papel con 84.302, una transposición al leer una hoja mojada. **Nadie notaría los 18 km de diferencia**, y esos 18 km son el denominador de [`RN-30`](../docs/01-negocio/reglas/RN-30-conciliacion-galonaje-kilometraje.md).

## Lo que falta

| Qué | Dónde |
|---|---|
| **La aplicación React Native** — pantallas, cámara, GPS en segundo plano | Necesita máquina con Android SDK y Java |
| **La persistencia SQLite cifrada** | Módulo nativo. El `DiarioLocal` de hoy guarda **en memoria** y lo dice en su propia documentación |
| **Folios pre-asignados por dispositivo** | [`RN-44`](../docs/01-negocio/reglas/RN-44-identificadores-y-folios-en-el-cliente.md) · `RNF-21` — **dos dispositivos de la misma delegación sin red colisionan** (`HB34-52`) |
| **Adjuntos diferidos** — ≥ 200 fotografías por dispositivo | [`ADR-004`](../docs/03-arquitectura/adr/ADR-004-adjuntos-fuera-de-la-base.md) |
| **El endpoint de sincronización en el servidor** | El núcleo produce el lote; nadie lo recibe todavía |
| **`RNF-12`** — ≤ 25 % de batería en 8 h, gama baja | **No se ha medido nada.** Es el requisito más ajustado del sistema |

## Por qué Node corre esto sin compilar

Node 24 quita los tipos de TypeScript de forma nativa, así que `node --test` ejecuta los `.prueba.ts` directamente. **Sin runner, sin transpilador, sin configuración de build** — lo que importa en un módulo cuya gracia es no depender de la plataforma.

Por eso `tsconfig.json` lleva `erasableSyntaxOnly`: prohíbe la sintaxis de TypeScript que **no** se puede borrar sin compilar —`enum`, `namespace`, parámetros con `private`—, y así el compilador impide de antemano que alguien escriba algo que Node no podría ejecutar.
