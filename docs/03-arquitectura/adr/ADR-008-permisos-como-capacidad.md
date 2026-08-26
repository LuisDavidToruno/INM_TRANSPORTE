# ADR-008 — Los permisos se publican como capacidad, nunca como rol

| Campo | Valor |
|---|---|
| **Estado** | Aceptada |
| **Fecha** | 2026-08-26 |
| **Decide** | Product Owner, sobre la [designación de LOKI del 2026-08-26](../../07-gestion/designaciones/2026-08-26-stack-y-arranque.md) |
| **Sprint** | 0 |

## Contexto

SIGTI tiene 17 actores, 40 delegaciones y una regla que no admite excepción: **segregación de funciones**. Quien solicita ≠ quien autoriza ≠ quien despacha ≠ quien entrega combustible ≠ quien liquida. Es bloqueo duro, no advertencia — y su autoridad es [`docs/01-negocio/actores-y-roles.md`](../../01-negocio/actores-y-roles.md), no este documento.

La forma ingenua de llevar eso al cliente es mandarle el rol: *«este usuario es Jefe de Transporte»*, y que la interfaz decida qué mostrar. Funciona hasta que aparece la primera excepción — y en este dominio las excepciones son el dominio. En una delegación pequeña una persona cubre dos puestos; hay suplencias con vigencia acotada; hay delegación de firma; hay un motorista que no puede autorizar su propia misión aunque su puesto lo permitiría en general.

Cada una de esas reglas, si el cliente deriva permisos del rol, **hay que reimplementarla en el cliente**. Y hay dos clientes.

## Requisitos que la condicionan

- [`RNF-14`](../../02-requisitos/no-funcionales/RNF-14-control-de-acceso-por-puesto-y-registro-de-consultas.md) — control de acceso por puesto y registro de consultas
- [`RNF-19`](../../02-requisitos/no-funcionales/RNF-19-configurabilidad-multi-institucion.md) — configurabilidad multi-institución: los puestos no se llaman igual en dos instituciones
- [`RNF-15`](../../02-requisitos/no-funcionales/RNF-15-continuidad-ante-rotacion-de-personal.md) — continuidad ante rotación de personal

## Decisión

**El servidor publica qué puede hacer el usuario, no qué es.**

```
✅  { "puedeAutorizarMision": true, "puedeDespachar": false, ... }
❌  { "rol": "JEFE_TRANSPORTE" }
❌  { "esAdministrador": true }
```

- **Nunca un indicador `esAdministrador`.** Obligaría al cliente a implementar la regla del bypass, y esa regla viviría en dos lados. La regla del bypass es del servidor; el cliente solo recibe el resultado.
- **La capacidad se resuelve para el objeto concreto cuando corresponde.** *«¿Puede autorizar?»* no es una propiedad del usuario: es una propiedad del par usuario–misión. Una misión que él mismo solicitó da `false` aunque su puesto diga otra cosa (incompatibilidad `I-11`).
- **El servidor vuelve a verificar siempre.** La capacidad publicada es para que el cliente sepa qué ofrecer, **no** es la autorización. Un cliente manipulado no obtiene nada que el servidor no conceda por su cuenta.
- **Los roles siguen existiendo** como mecanismo de administración: es como se asignan capacidades a las personas sin configurarlas una por una. Lo que no cruza la frontera hacia el cliente es el rol; cruza su resultado.

### Por qué pesa el doble acá

**Hay dos clientes** — oficina y campo. Si cada uno deriva permisos de roles, **divergen**. Y la divergencia es del peor tipo: invisible.

> **Un botón ofrecido que el servidor rechaza se lee como falla del sistema, no como regla.**

El motorista no concluye *«no tengo permiso»*. Concluye *«el sistema no sirve»*, y con eso llega a la próxima reunión. En un sistema que reemplaza formatos en papel que sí funcionaban, esa percepción es cara.

Además, con 40 delegaciones **los roles se multiplican y las capacidades no**. Cada delegación con su combinación de suplencias y puestos cubiertos genera una variante de rol; la lista de capacidades sigue siendo la misma lista.

## Alternativas consideradas

| Alternativa | A favor | En contra | Por qué se descartó |
|---|---|---|---|
| **Publicar el rol y que el cliente derive** | Simple; carga pequeña; patrón conocido | La regla de permisos vive en tres lugares —servidor y dos clientes— y diverge; las excepciones del dominio hay que reimplementarlas en cada uno | Es exactamente el fallo que este ADR existe para evitar |
| **Publicar rol + lista de excepciones** | Menos verboso que capacidades completas | El cliente sigue teniendo que implementar cómo se combinan rol y excepción, que es la parte con reglas | Traslada la lógica difícil al cliente y deja la fácil en el servidor |
| **No publicar nada; el cliente intenta y el servidor rechaza** | Imposible que diverjan; una sola fuente | Interfaz llena de acciones que fallan al presionarlas. Es precisamente lo que se lee como sistema roto | Correcto en seguridad, inaceptable en experiencia de uso |
| **Capacidades globales, sin resolver por objeto** | Una sola consulta por sesión, cacheable | No puede expresar *«puede autorizar, pero no esta misión»*, que es donde vive la segregación de funciones | No alcanza para el requisito central |

## Consecuencias

**Positivas**

- La regla de permisos vive **en un solo lugar**, y los dos clientes no pueden divergir porque no deciden nada
- Una excepción nueva —una suplencia, una delegación de firma, una incompatibilidad— se implementa una vez y aparece en los dos clientes sin tocarlos
- `RNF-19` se cumple sin esfuerzo: una institución que llama distinto a sus puestos configura roles distintos; las capacidades son las mismas
- El registro de consultas de `RNF-14` es natural, porque cada resolución pasa por el servidor

**Negativas**

- **La carga útil crece**, y resolver capacidades por objeto significa resolverlas para cada elemento de una lista. Es un costo de rendimiento real que hay que medir contra `RNF-01`
- **El cliente de campo trabaja hasta 7 días sin servidor.** Tiene que llevar consigo capacidades resueltas y **decidir con información que puede haber envejecido**. Ver deuda aceptada
- Nombrar capacidades es un trabajo de diseño continuo. Una lista mal nombrada se vuelve tan opaca como los roles que reemplaza

**Deuda aceptada**

- **Las capacidades en campo se resuelven contra el estado de la última sincronización.** Si a un motorista se le retira una habilitación mientras está sin señal, el dispositivo va a seguir ofreciéndole acciones hasta que reconecte. **El servidor las rechaza al ingresar los datos**, así que no hay violación real — pero sí hay trabajo hecho en campo que se cae al sincronizar, y eso hay que mostrarlo en la cola de conflictos, no esconderlo
- La resolución por objeto en listas grandes puede necesitar un mecanismo de agrupación que hoy no está diseñado

## Revisión

- **La medición contra `RNF-01`** muestra que resolver capacidades por objeto no rinde en las listas reales
- **Aparece un caso donde el cliente de campo necesita decidir algo que el servidor no puede anticipar** al sincronizar
- **La lista de capacidades supera el punto en que un administrador la puede entender** — señal de que hacen falta agrupaciones, no de que el enfoque esté mal
