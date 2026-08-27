# RN-43 — Toda captura de campo debe completarse sin ninguna conectividad y nunca perderse

| Campo | Valor |
|---|---|
| **Módulos** | M-16, M-08, M-09, M-18, M-12 |
| **Origen** | Norma [NRM-09](../normativa/NRM-09-realidad-operativa.md); premisa rectora 5 de `CLAUDE.md` |
| **Verificación** | `[V]` INE EPHPM julio 2025 — más de 2 millones de personas del área rural sin acceso a internet |
| **Tipo** | Bloqueo duro (requisito de comportamiento verificable) |
| **Configurable** | No |

## Enunciado

El cliente de campo **debe** permitir completar y almacenar localmente, **sin ninguna conectividad**, al menos:

- Registro de salida con odómetro y estado del vehículo
- Bitácora: paradas, arribos, eventos en ruta, entregas
- Consumo de combustible con fotografía del comprobante
- Paso por caseta de peaje con fotografía del ticket
- Reporte de falla, incidente o siniestro con fotografías
- Actualización de estado y ubicación para seguimiento en ruta
- Registro de retorno

Ninguna de estas capturas **debe** requerir validación remota para guardarse. Ningún dato capturado **debe** perderse por falta de red, por cierre de la aplicación, por batería agotada o por falla de sincronización.

### Y una capacidad que no es de captura sino de consulta — corrección del hallazgo `HN1-18`

El cliente **debe** llevar a bordo, legible **sin ninguna conectividad**, la **guía de actuación en accidente**: qué hacer, a quién llamar, qué no firmar, qué fotografiar y en qué orden.

[NRM-06](../normativa/NRM-06-transito-y-licencias.md) lo dice literalmente —*«proveer al motorista una guía de actuación en accidente accesible sin conexión desde el móvil, y capturar el reporte inicial offline»*— y la máquina de estados ya la transfiere con el paquete de misión en `T-12` ([orden-de-mision.md](../../03-arquitectura/estados/orden-de-mision.md)). **Faltaba en esta lista**, que es donde se declara qué tiene que funcionar sin red.

Va aparte del resto porque es de naturaleza distinta: las demás son cosas que el motorista **escribe**; ésta es la única que el motorista **lee**, y la lee en el peor momento posible. Una guía que requiere señal para abrirse no existe: los accidentes no ocurren donde hay cobertura, y el minuto en que se necesita es el minuto en que nadie va a esperar a que cargue.

## Justificación

Premisa rectora 5: *"Offline-first, no 'con soporte offline'."*

[NRM-09](../normativa/NRM-09-realidad-operativa.md) aporta el dato duro `[V]`: el acceso a internet urbano es del 64.7%, y **más de 2 millones** de personas del área rural no tienen acceso. La cobertura 4G en distritos aislados ronda el 50.7% `[P]`.

Un sistema que exige red para registrar la salida de un vehículo simplemente no se usa en una delegación rural: el motorista sale, y el registro se reconstruye después de memoria — que es precisamente lo que TSC-NOGECI V-10 prohíbe al exigir **registro oportuno**.

## Condiciones de aplicación

Aplica al cliente de campo usado por ACT-06 Motorista y ACT-10 Encargado de Delegación.

**No aplica** a operaciones que por naturaleza requieren estado global: aprobar una orden con la cadena de autorización, cerrar un fondo, cargar parámetros. Esas se hacen contra el servidor, y el cliente de campo debe **decir claramente** cuáles no están disponibles sin red, en lugar de fallar sin explicación.

## Comportamiento esperado

1. La captura se guarda localmente de forma **duradera** en el momento de confirmar, antes de cualquier intento de envío.
2. Las validaciones locales se ejecutan contra la copia local de catálogos y padrón, marcando el resultado con la **fecha de sincronización** de esos datos ([RN-50](RN-50-degradacion-por-sincronizacion-detenida.md)).
3. Las fotografías se almacenan localmente y se sincronizan como **adjuntos diferidos**, sin bloquear el envío del registro principal.
4. El cliente muestra en todo momento **cuántos registros y adjuntos están pendientes** de sincronizar y desde cuándo.
5. Al reconectar, la sincronización es incremental y reintentable, sin duplicar ni perder ([RN-45](RN-45-cero-sobrescritura-silenciosa.md)).
6. Los documentos que el motorista debe portar se imprimen antes de salir con **folio pre-asignado** ([RN-44](RN-44-identificadores-y-folios-en-el-cliente.md)).

## Casos límite

- **Dispositivo perdido, robado o destruido con datos sin sincronizar.** Los datos se pierden con él. Mitigaciones exigibles: sincronización oportunista en cada ventana de red, y **el respaldo en papel** como red de seguridad ([NRM-09](../normativa/NRM-09-realidad-operativa.md) — el formato preimpreso sigue existiendo). Esta es la razón por la que la paridad pantalla↔papel no es nostalgia.
- **Almacenamiento local agotado** por acumulación de fotografías en una misión larga. El cliente debe alertar con anticipación y permitir reducir la resolución de las fotos antes de impedir la captura. Que un incidente no se pueda registrar por falta de espacio es una falla inaceptable.
- **Motorista sin dispositivo.** Existe y existirá. Se opera en papel y se digita después ([RN-47](RN-47-digitacion-diferida-desde-papel.md)). El sistema no puede asumir un teléfono por motorista.
- **Batería agotada a mitad de captura.** El registro parcial debe conservarse como borrador local recuperable. Perder veinte minutos de captura de un siniestro es la forma más rápida de que el motorista deje de usar la aplicación.
- **Validación imposible sin red** — por ejemplo, comprobar que un fondo tiene saldo. El cliente valida contra lo que sabe y **advierte** que la validación es local. La entrega ocurrida no se revierte al sincronizar ([RN-27](RN-27-asignacion-de-combustible-con-folio.md)).
- **Reloj del dispositivo alterado**, deliberadamente o no. Se registra la marca de tiempo del dispositivo **y** la del servidor al sincronizar, y la desviación queda como dato ([RN-03](RN-03-registro-inmutable-de-autorizacion.md)).
- **Dos dispositivos capturando la misma misión** — motorista y encargado de delegación. Producirá registros paralelos: se resuelven por [RN-45](RN-45-cero-sobrescritura-silenciosa.md), nunca descartando el que llegó segundo.

## Trazabilidad

- Norma: [NRM-09 — Realidad operativa](../normativa/NRM-09-realidad-operativa.md)
- Premisa rectora 5 de [CLAUDE.md](../../../CLAUDE.md)
- Reglas relacionadas: [RN-44](RN-44-identificadores-y-folios-en-el-cliente.md), [RN-45](RN-45-cero-sobrescritura-silenciosa.md), [RN-46](RN-46-fecha-del-hecho-y-fecha-de-captura.md), [RN-47](RN-47-digitacion-diferida-desde-papel.md), [RN-50](RN-50-degradacion-por-sincronizacion-detenida.md)
- Actores: ACT-06, ACT-10
- Historias y casos especiales: pendientes — Bloque 2
