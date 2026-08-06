# RN-18 — La rotulación e identificación del vehículo del Estado se verifica con fecha y fotografía, y su constatación caduca

| Campo | Valor |
|---|---|
| **Módulos** | M-03, M-04, M-14 |
| **Origen** | Norma [NRM-02](../normativa/NRM-02-bienes-del-estado.md) — Acuerdo No. 303 (1981) |
| **Verificación** | `[V]` los elementos de identificación obligatoria — `[C]` la vigencia del Acuerdo 303 en su redacción original |
| **Tipo** | Advertencia con constatación fechada (derivación del estado) |
| **Configurable** | Sí — parámetro `vigencia_constatacion_rotulacion` (días) |

## Enunciado

El expediente de cada vehículo **debe** registrar el estado de sus elementos de identificación obligatoria:

- Tres franjas horizontales de 10 cm, azul–blanco–azul, en puertas laterales `[V]`
- Leyenda **"PROPIEDAD DEL ESTADO DE HONDURAS"** en letras de 2.54 cm `[V]`
- Siglas o nombre de la institución `[V]`
- Numeración consecutiva institucional `[V]`
- Placas nacionales, con la salvedad de [RN-15](RN-15-identidad-del-vehiculo-y-placa.md)

Cada elemento se registra como **constatado** con **fecha de constatación, fotografía y servidor que constató**. Transcurrido el parámetro de vigencia, la constatación **caduca** y el vehículo pasa a estado *identificación no constatada*, lo que genera advertencia al despachar.

Una constatación sin fotografía **no debe** aceptarse.

## Justificación

[NRM-02](../normativa/NRM-02-bienes-del-estado.md): la identificación es obligatoria, *"es un hallazgo de auditoría frecuente y se verifica físicamente en operativos"*. El TSC realiza operativos vehiculares de fiscalización **en Semana Santa**, de forma recurrente y predecible — informes E-001-2015-DFBN, E-007-2015-FBN, 002-2023-DFBN.

Un campo booleano "rotulado: sí" cargado una vez en el alta y nunca revisado no prueba nada: la pintura se despinta, las calcomanías se caen, el vehículo se repinta tras un golpe. Por eso la constatación caduca y exige foto.

## Condiciones de aplicación

Aplica a todo vehículo propiedad del Estado en la flota.

`[C]` [NRM-02](../normativa/NRM-02-bienes-del-estado.md) deja abierto si la rotulación aplica a vehículos en comodato o alquilados. Hasta confirmarlo, se registra el estado igual, marcando el régimen de tenencia, y la advertencia se emite con esa aclaración.

**No aplica** a vehículos con excepción de rotulación autorizada — unidades de investigación o seguridad. `[C]` confirmar si la institución tiene esa figura y quién la autoriza; de existir, es atributo del vehículo con fundamento y vigencia, análogo a [RN-24](RN-24-vehiculo-de-servicio-exceptuado.md).

## Comportamiento esperado

1. La constatación se captura desde el móvil, **sin conectividad**, con fotografía por elemento y ubicación. Es el caso de uso de constatación física que [NRM-02](../normativa/NRM-02-bienes-del-estado.md) exige para conciliar contra el registro de bienes.
2. Al despachar un vehículo con identificación no constatada o con algún elemento marcado como **ausente**, el sistema advierte con acuse registrado.
3. El sistema genera el **reporte previo a Semana Santa** que [NRM-02](../normativa/NRM-02-bienes-del-estado.md) exige: vehículos autorizados a circular con su permiso, vehículos que deben estar resguardados con confirmación de resguardo, y estado de identificación de cada uno.
4. La corrección de un elemento ausente genera un registro de **subsanación** con fecha, foto posterior y responsable, no una simple edición del estado.
5. El historial de constataciones es consultable por vehículo: cuándo se verificó, quién, qué encontró.

## Casos límite

- **Vehículo recién adquirido aún sin rotular.** Estado *pendiente de rotulación* con fecha de alta. Genera alerta desde el primer día y advertencia al despachar. `[C]` confirmar si la institución acepta operar un vehículo sin rotular y por cuánto tiempo.
- **Rotulación parcial** — tiene franjas pero le falta la leyenda. Se registra por elemento, no como un todo. El reporte de auditoría debe poder decir exactamente qué falta.
- **Vehículo repintado tras un siniestro.** La constatación anterior queda obsoleta aunque no haya caducado. El cierre de una orden de trabajo de carrocería en M-11 **debe invalidar** la constatación de rotulación y exigir una nueva.
- **Fotografía tomada de un vehículo distinto.** El sistema no puede detectarlo automáticamente. Mitigación: la captura exige que la foto se tome en el momento (no de galería) `[C]` y registre ubicación; y la constatación la firma un servidor identificado que responde por ella.
- **Motocicletas.** Las medidas del Acuerdo 303 — franjas de 10 cm, letras de 2.54 cm — están pensadas para puertas laterales que una moto no tiene. `[C]` cómo se identifica una motocicleta del Estado. **No se infiere**: mientras no se confirme, el sistema registra los elementos aplicables y marca los no aplicables con fundamento.
- **Vehículo en delegación remota** que lleva un año sin constatar por falta de personal. La caducidad producirá advertencia crónica. Es el resultado correcto: la advertencia crónica es exactamente lo que hay que ver antes de que lo vea el TSC.

## Trazabilidad

- Norma: [NRM-02 — Bienes del Estado](../normativa/NRM-02-bienes-del-estado.md)
- Reglas relacionadas: [RN-15](RN-15-identidad-del-vehiculo-y-placa.md), [RN-17](RN-17-alertas-de-vencimiento-documental.md), [RN-23](RN-23-permiso-de-circulacion-en-dia-inhabil.md), [RN-43](RN-43-captura-de-campo-sin-conectividad.md)
- Actores: ACT-04, ACT-10, ACT-12, ACT-13
- Historias y casos especiales: pendientes — Bloque 2
