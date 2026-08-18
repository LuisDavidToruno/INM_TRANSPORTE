# HU-017 — Emitir e imprimir el salvoconducto con folio, QR y vigencia explícita

| Campo | Valor |
|---|---|
| **Módulo** | M-15 Formatos Oficiales e Impresión (con M-04 Documentación y Cumplimiento Vehicular) |
| **Actor** | ACT-04 Jefe de Transporte; ACT-10 Encargado de Delegación |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Refinada |

## Historia

**Como** Jefe de Transporte
**quiero** emitir e imprimir el salvoconducto del permiso firmado, con folio único, QR de verificación, espacio de firma y sello, huella del documento electrónico y **vigencia explícita desde–hasta**
**para** que el motorista lleve en la mano un documento que un verificador en carretera pueda comprobar sin acceso al sistema, que es como funciona el control real en la carretera hondureña

## Contexto

El control en carretera es **físico**. El destinatario del papel no se autentica, no tiene usuario y no verá nunca el expediente. Por eso todo documento de control lleva versión imprimible con folio, QR, espacio de firma y sello, y hash del documento electrónico (premisa rectora 4 de `CLAUDE.md`).

El folio se toma del **rango de la delegación**, para que una delegación sin conectividad pueda emitir e imprimir antes de salir. Y un rango que se agota estando desconectado es un incidente previsible: el sistema alerta por consumo del rango con anticipación.

Dos exigencias que parecen detalle y son las que decide un operativo: la **vigencia impresa desde–hasta** —porque lo primero que compara un fiscalizador es la fecha del papel con la del control—, y la **identificación del vehículo por correlativo institucional**, porque hay vehículos del Estado circulando sin lámina metálica por el desabastecimiento nacional.

## Reglas que la gobiernan

- [RN-25](../../01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md) — El salvoconducto se emite impreso, con folio único y QR verificable
- [RN-44](../../01-negocio/reglas/RN-44-identificadores-y-folios-en-el-cliente.md) — Folios de rangos por delegación, para emisión anticipada sin conectividad
- [RN-15](../../01-negocio/reglas/RN-15-identidad-del-vehiculo-y-placa.md) — La identidad del vehículo es el correlativo institucional; la placa no es obligatoria ni única
- [RN-65](../../01-negocio/reglas/RN-65-sin-lamina-respaldo-y-paquete-de-identificacion.md) — Sin lámina: respaldo vigente y paquete de identificación impreso y acusado
- [RN-23](../../01-negocio/reglas/RN-23-permiso-de-circulacion-en-dia-inhabil.md) — El salvoconducto materializa el permiso firmado
- [RN-04](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md) — El folio no se recicla; la reimpresión no genera folio nuevo

## Casos especiales que la afectan

- [CE-17](../casos-especiales/CE-17-vehiculo-sin-placa-metalica.md) — Vehículo sin placa metálica: el salvoconducto lo identifica por correlativo institucional
- [CE-09](../casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md) — Delegación sin señal: emisión e impresión anticipadas con folio del rango local

## Criterios de aceptación

```gherkin
# language: es
Característica: Emisión e impresión del salvoconducto
  Como Jefe de Transporte
  quiero imprimir el salvoconducto con folio y QR
  para que el control en carretera pueda verificarlo sin acceso al sistema

  Antecedentes:
    Dado un permiso "PC-2026-0009" firmado por la máxima autoridad "Doris Cruz" el "2026-03-19 16:40"
    Y un vehículo "Pickup Hilux" con correlativo institucional "VH-0142" y placa "PAA-1234"
    Y un motorista "José Martínez"
    Y una ruta "Tegucigalpa–Choluteca" y una ventana del "2026-03-20 07:00" al "2026-03-21 17:00"
    Y una delegación "Choluteca" con rango de folios "CHO-SC-2026-0010" a "CHO-SC-2026-0040"

  Escenario: Se bloquea la emisión sin permiso firmado
    Dado un expediente de permiso sin firma de la máxima autoridad
    Cuando el Jefe de Transporte intenta emitir el salvoconducto
    Entonces el sistema no emite el documento
    Y muestra "No existe permiso firmado por la máxima autoridad. El salvoconducto materializa un permiso; sin firma no hay documento que emitir."

  Escenario: Se bloquea la emisión con el rango de folios agotado
    Dado un rango de folios de la delegación con el último número "CHO-SC-2026-0040" ya consumido
    Cuando el Jefe de Transporte intenta emitir el salvoconducto
    Entonces el sistema no emite el documento
    Y muestra "El rango de folios de salvoconducto de la delegación Choluteca está agotado (CHO-SC-2026-0010 a CHO-SC-2026-0040). Solicite ampliación de rango antes de emitir."

  Escenario: Se alerta por consumo del rango antes de agotarlo
    Dado un umbral de alerta de consumo de rango del "80" por ciento
    Y un rango de 31 folios con 25 consumidos
    Cuando el Jefe de Transporte emite el salvoconducto "CHO-SC-2026-0036"
    Entonces el sistema emite el documento
    Y muestra "Rango de folios al 84 por ciento de consumo: quedan 5 de 31. Solicite ampliación."

  Escenario: El salvoconducto impreso lleva todos los elementos verificables
    Cuando el Jefe de Transporte emite el salvoconducto del permiso "PC-2026-0009"
    Entonces el documento recibe el folio "CHO-SC-2026-0011"
    Y contiene el QR de verificación
    Y contiene el espacio de firma y sello
    Y contiene la huella del documento electrónico
    Y contiene la vigencia explícita "desde 20/03/2026 07:00 hasta 21/03/2026 17:00"
    Y contiene el vehículo identificado como "VH-0142 / placa PAA-1234", el motorista "José Martínez" y la ruta "Tegucigalpa–Choluteca"

  Escenario: El vehículo sin lámina metálica se identifica por correlativo institucional
    Dado un vehículo "Pickup Mazda BT-50" con correlativo institucional "VH-0187" y estado de placa "sin lámina, en trámite"
    Y un respaldo documental vigente de ese estado durante toda la ventana
    Cuando el Jefe de Transporte emite el salvoconducto para ese vehículo
    Entonces el documento identifica el vehículo como "VH-0187 / sin lámina metálica — respaldo vigente"
    Y no muestra ningún número de placa

  Escenario: Se emite e imprime por anticipado desde una delegación sin conectividad
    Dado un dispositivo sin ninguna conectividad en la delegación "Choluteca"
    Cuando el Encargado de Delegación emite el salvoconducto del permiso "PC-2026-0009"
    Entonces el documento recibe el folio "CHO-SC-2026-0012" del rango local
    Y se imprime completo con QR y huella
    Y queda encolado para sincronizar

  Escenario: La reimpresión conserva el mismo folio y deja constancia
    Dado un salvoconducto "CHO-SC-2026-0011" ya impreso y extraviado en ruta
    Cuando el Jefe de Transporte reimprime el salvoconducto
    Entonces el documento reimpreso conserva el folio "CHO-SC-2026-0011"
    Y conserva el mismo contenido y la misma huella
    Y el sistema registra quién reimprimió, cuándo y por qué
    Y el conteo de impresiones del folio pasa a "2"

  Escenario: Sin capacidad de impresión no se despacha en día inhábil
    Dada una delegación sin impresora operativa
    Cuando el Encargado de Delegación intenta cerrar la emisión sin imprimir el salvoconducto
    Entonces el sistema no da por emitido el documento
    Y muestra "Sin salvoconducto impreso no se despacha en día u hora inhábil. No hay excepción: es requisito de despliegue de la delegación."
```

## Fuera de alcance

- La **verificación** del salvoconducto por el QR en carretera — es [HU-019](HU-019-verificacion-del-salvoconducto-en-carretera.md)
- La **entrega** del salvoconducto impreso al motorista junto con la Orden de Misión: ocurre dentro del despacho (`T-12`), en [CU-05](../casos-de-uso/CU-05-emitir-orden-de-mision-y-documentos.md)
- La anulación y reemisión cuando cambia vehículo, motorista, ruta o ventana — es [HU-018](HU-018-reemision-del-permiso-por-cambio-de-elementos.md)
- Los demás formatos oficiales impresos (Orden de Misión, hoja de bitácora, vale de combustible) — son historias propias de M-15

## Notas y pendientes

- `[C]` **Formatos en papel vigentes de la institución** — insumo #2. El diseño del salvoconducto debe partir del formato real y recorrerlo campo por campo antes de codificarlo. La [DoR](../../plantillas/definition-of-ready.md) exige que el formato esté diseñado antes de entrar a sprint
- `[C]` Verificar que **toda delegación tenga capacidad de impresión** — insumo #27. Es requisito de despliegue, no excepción a la regla
- `[C]` Procedimiento de ampliación de rango de folios sin conectividad — insumo #1
- `[V]` La exigencia del documento físico de control en carretera consta en [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md); la ausencia de firma electrónica certificada, en [NRM-08](../../01-negocio/normativa/NRM-08-firma-electronica.md)
- Trazabilidad: [CU-03](../casos-de-uso/CU-03-permiso-de-circulacion-en-dia-inhabil.md) pasos 8 a 10, flujos A3 y A6, excepción E6; invariante `INV-19`
