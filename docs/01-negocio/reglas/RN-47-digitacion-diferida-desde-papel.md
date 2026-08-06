# RN-47 — La digitación diferida desde papel deja constancia de quién digitó y del original escaneado

| Campo | Valor |
|---|---|
| **Módulos** | M-16, M-15, M-08, M-09 |
| **Origen** | Norma [NRM-09](../normativa/NRM-09-realidad-operativa.md); [NRM-01](../normativa/NRM-01-control-interno-tsc.md) |
| **Verificación** | `[V]` la exigencia de digitación diferida con constancia y adjunto del original |
| **Tipo** | Bloqueo duro |
| **Configurable** | No |

## Enunciado

El sistema **debe** permitir que ACT-10 Encargado de Delegación, o el rol facultado, digite formularios llenados en papel — bitácora, consumo de combustible, paso por caseta, incidente — registrando obligatoriamente:

1. **Quién digitó** y cuándo (fecha de captura, [RN-46](RN-46-fecha-del-hecho-y-fecha-de-captura.md))
2. **Quién es el autor del registro original** — el motorista que llenó el papel
3. **Adjunto del original** escaneado o fotografiado
4. La **fecha del hecho** que consta en el papel

Un registro digitado **sin adjunto del original no debe** poder cerrarse como completo: queda con adjunto pendiente y, vencido el plazo configurado, produce hallazgo.

El registro digitado **debe** ser distinguible de uno capturado en el momento, en pantalla y en todo reporte.

## Justificación

[NRM-09](../normativa/NRM-09-realidad-operativa.md): *"El sistema debe permitir digitación diferida de formularios en papel por un encargado de delegación, con constancia de quién digitó, cuándo, y adjunto del original escaneado o fotografiado, distinguiendo la fecha del hecho de la fecha de captura."* Y advierte que *"los formatos preimpresos de bitácora, requisición y salida de vehículo son la norma. El sistema no debe exigir que desaparezcan de inmediato."*

[NRM-01](../normativa/NRM-01-control-interno-tsc.md) exige lo mismo desde el control interno: la digitación diferida debe quedar identificada como tal, con quién digitó y el adjunto del original.

Sin el adjunto, el registro digitado es la afirmación de una persona sobre lo que otra escribió, sin respaldo. Con el adjunto, es una transcripción verificable.

## Condiciones de aplicación

Aplica a todo registro de hecho operativo capturado por alguien distinto de quien lo ejecutó, o capturado con desfase respecto del hecho.

**No aplica** a la captura del propio motorista en su dispositivo, aunque sea diferida: ahí el autor y el capturador coinciden, y basta la marca de registro diferido de [RN-46](RN-46-fecha-del-hecho-y-fecha-de-captura.md).

## Comportamiento esperado

1. El formulario de digitación mantiene **paridad exacta con el formato en papel**: mismos campos, mismos nombres, mismo orden ([NRM-09](../normativa/NRM-09-realidad-operativa.md)). `[C]` insumo #2 — formatos vigentes de la institución.
2. El digitador **no puede** figurar como autor del hecho. La responsabilidad del contenido es del motorista; la de la transcripción, del digitador. Ambas se registran.
3. Las validaciones se aplican igual que en la captura directa — odómetro ([RN-31](RN-31-odometro-de-retorno.md)), saldo de asignación ([RN-27](RN-27-asignacion-de-combustible-con-folio.md)) — y sus advertencias se resuelven contra el papel, no editando el dato para que pase.
4. El adjunto del original queda vinculado al registro y se incluye en el paquete de evidencia de auditoría.
5. Los registros digitados alimentan el indicador de oportunidad de registro por delegación, con su motivo.

## Casos límite

- **El papel está incompleto o ilegible.** No se completa por deducción. Los campos ausentes se registran como **no consignados en el original**, y la liquidación decide si constituyen hallazgo. Rellenar un odómetro que el papel no trae es fabricar el dato más sensible del sistema.
- **El papel contradice un registro ya sincronizado del motorista.** Es un conflicto: entra a la cola de [RN-45](RN-45-cero-sobrescritura-silenciosa.md) con ambas versiones y el adjunto como evidencia. El papel no prevalece automáticamente sobre lo digital, ni al revés.
- **Digitación masiva de un mes de bitácoras.** Admitida, pero cada registro conserva su fecha del hecho y su adjunto. Un solo adjunto para veinte registros solo es válido si el papel efectivamente contiene los veinte — el sistema debe permitir vincular un adjunto a varios registros dejando constancia de esa relación.
- **Sin escáner en la delegación.** La fotografía con el móvil es suficiente. Exigir escáner en una delegación rural es exigir que la regla no se cumpla.
- **El motorista no firmó el papel.** Es una debilidad del original que debe registrarse como observación, no ocultarse. La firma manuscrita sobre impresión es uno de los tres niveles previstos por [NRM-08](../normativa/NRM-08-firma-electronica.md).
- **Digitación por alguien que también autoriza o liquida.** La digitación no es función de control de [RN-01](RN-01-segregacion-de-funciones.md), pero un digitador que además liquida esos mismos registros concentra transcripción y verificación. `[C]` confirmar con Auditoría Interna; hasta entonces, advertencia registrada.
- **Original perdido después de digitar.** Si el adjunto ya está, el original físico es prescindible. Si no está y el papel se perdió, el registro queda permanentemente con hallazgo de falta de respaldo — y esa es la consecuencia correcta.

## Trazabilidad

- Normas: [NRM-09](../normativa/NRM-09-realidad-operativa.md), [NRM-01](../normativa/NRM-01-control-interno-tsc.md), [NRM-08](../normativa/NRM-08-firma-electronica.md)
- Reglas relacionadas: [RN-46](RN-46-fecha-del-hecho-y-fecha-de-captura.md), [RN-43](RN-43-captura-de-campo-sin-conectividad.md), [RN-45](RN-45-cero-sobrescritura-silenciosa.md), [RN-08](RN-08-cadena-de-trazabilidad-para-cierre.md)
- Actores: ACT-06, ACT-10, ACT-04, ACT-12
- Historias y casos especiales: pendientes — Bloque 2
