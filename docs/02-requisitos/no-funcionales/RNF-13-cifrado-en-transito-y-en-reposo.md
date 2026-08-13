# RNF-13 — Ningún dato personal viaja ni reposa en claro, y el celular perdido no es una fuga

| Campo | Valor |
|---|---|
| **Categoría** | Seguridad |
| **Prioridad** | Crítico |
| **Origen** | [NRM-07](../../01-negocio/normativa/NRM-07-transparencia-y-datos-personales.md): *"el sistema debe cifrar los datos personales en reposo, y cifrar en tránsito toda comunicación, incluida la de las delegaciones"* |
| **Afecta arquitectura** | **Sí** — el cifrado del almacén local del cliente de campo y la gestión de claves en un despliegue sin equipo de TI son restricciones reales |

## Enunciado

Toda comunicación de SIGTI **debe** viajar cifrada: entre el cliente de campo y el servidor, entre las delegaciones y la sede, y entre SIGTI y los sistemas externos. No existe canal en claro ni modo de compatibilidad que lo permita "mientras tanto".

Los **datos personales** —identidad de pasajeros externos de M-17, datos de contacto, escaneo de licencias— se almacenan cifrados en reposo, y también en el **respaldo** y en el **dispositivo de campo**.

El escenario que gobierna este requisito es concreto y frecuente: **el celular del motorista se pierde o se lo roban**, con un manifiesto de personas externas dentro. Ese dispositivo está fuera del perímetro de la institución desde el momento en que sale de la sede.

## Métrica y umbral

| Métrica | Umbral |
|---|---|
| Tráfico del sistema que viaja sin cifrar | **0**, incluidas las delegaciones y las llamadas a ARGOS y Talento Humano |
| Modos de compatibilidad sin cifrado o con protocolos obsoletos | **0.** Si un componente no soporta el canal cifrado, no se integra |
| Datos personales legibles en un respaldo abierto con un visor de texto | **0.** Se verifica buscando nombres reales de prueba |
| Datos personales legibles en el almacén local del dispositivo extraído del sistema de archivos | **0** |
| Autenticación local obligatoria para abrir el cliente de campo | Sí, incluso sin red |
| Antigüedad sin sincronizar tras la cual el cliente exige reautenticación | ≤ 7 días `[C]` — coincide con la duración máxima de misión del [`RNF-03`](RNF-03-operacion-sin-conectividad.md); no puede ser menor o bloquearía una misión legítima |
| Antigüedad sin sincronizar tras la cual el cliente **bloquea la lectura** de manifiestos ya sincronizados y conserva solo lo pendiente de subir | ≤ 15 días `[C]` insumo #71 |
| Borrado remoto del contenido local al reportar un dispositivo perdido | Soportado; se ejecuta al siguiente contacto del dispositivo con el servidor, y queda asiento |
| Datos pendientes de sincronizar que se pierden por un borrado remoto | Se declara explícitamente como pérdida aceptada y **se avisa a quien ordena el borrado antes de ejecutarlo**, indicando cuántos registros se perderían |
| Claves o secretos en el código fuente o en el repositorio | **0.** Verificado por barrido automático en cada entrega |
| Procedimiento de rotación de claves ejecutable por personal no especializado | Documentado y probado ([`RNF-09`](RNF-09-instalacion-respaldo-y-restauracion.md)) |
| Custodia de la clave de cifrado del respaldo | Fuera del servidor. `[C]` procedimiento a acordar con la institución — insumo #73 |
| Fotografías y adjuntos con datos personales | Mismo tratamiento que el dato estructurado. Una foto de un documento de identidad es un dato personal |

## Cómo se verifica

1. **Captura de tráfico**: se instrumenta la red durante una jornada completa de operación —incluidas sincronizaciones de campo y llamadas a los sistemas externos— y se inspecciona. Cualquier contenido legible es un defecto bloqueante.
2. **Prueba del respaldo abierto**: se toma un respaldo de una base con pasajeros de prueba de nombre conocido y se busca esos nombres con una herramienta de texto plano. Resultado esperado: cero coincidencias.
3. **Prueba del celular robado** — la prueba que define este requisito:
   - Se carga un dispositivo con una misión que incluye manifiesto de personas externas.
   - Se extrae el almacén local por acceso directo al sistema de archivos.
   - Se intenta leer. Debe ser ilegible.
   - Se reporta el dispositivo como perdido, se le da red, y se verifica que el contenido se borra y queda asiento con autor y motivo.
4. **Prueba de expiración**: se mantiene un dispositivo 8 días sin sincronizar y se verifica que exige reautenticación; a los 16 días, que bloquea la lectura de lo ya sincronizado **sin destruir lo pendiente de subir**.
5. **Barrido de secretos**: análisis automático del repositorio y de los artefactos de despliegue buscando claves, contraseñas y certificados. Corre en cada entrega.
6. **Prueba de rotación**: la persona no especializada del [`RNF-09`](RNF-09-instalacion-respaldo-y-restauracion.md) rota la clave de cifrado siguiendo el documento, y se verifica que el sistema sigue operando y que los respaldos anteriores siguen restaurables.

## Consecuencia de no cumplirlo

Un manifiesto de personas externas en manos de terceros es una fuga de datos personales de individuos que no eligieron estar en ese registro. [NRM-07](../../01-negocio/normativa/NRM-07-transparencia-y-datos-personales.md) lo hace exigible por el MARCI incluso sin ley de datos personales, y un hábeas data contra la institución no requiere que exista esa ley.

En sentido operativo: la institución quedaría en la posición de haber creado un riesgo que antes no tenía. En papel, el manifiesto se quedaba en la delegación. Digitalizarlo sin cifrarlo lo pone en 200 celulares en carretera. Si el sistema empeora la protección de datos respecto del papel, no debió construirse así.

## Trazabilidad

- Módulos: M-17, M-16, M-01, transversal
- Reglas: [`RN-51`](../../01-negocio/reglas/RN-51-minimizacion-de-datos-de-personas-externas.md), [`RN-52`](../../01-negocio/reglas/RN-52-registro-de-consultas-a-manifiestos.md), [`RN-53`](../../01-negocio/reglas/RN-53-cierre-del-manifiesto-al-despacho.md)
- Normativa: [NRM-07](../../01-negocio/normativa/NRM-07-transparencia-y-datos-personales.md), [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md)
- Requisitos relacionados: [`RNF-14`](RNF-14-control-de-acceso-por-puesto-y-registro-de-consultas.md), [`RNF-17`](RNF-17-retencion-y-depuracion-diferenciada.md), [`RNF-03`](RNF-03-operacion-sin-conectividad.md), [`RNF-09`](RNF-09-instalacion-respaldo-y-restauracion.md)
- Insumos: #71 (plazos de depuración con Auditoría Interna y el OIP), #73 (custodia de claves)
