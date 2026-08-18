# HU-034 — Emitir la hoja de bitácora en papel, con folio y paridad exacta con la pantalla de digitación

| Campo | Valor |
|---|---|
| **Módulo** | M-15 Formatos Oficiales e Impresión · M-08 Ejecución y Bitácora · M-16 Sincronización y Operación Desconectada |
| **Actor** | ACT-05 Encargado de Despacho (emite) · ACT-06 Motorista (la llena en ruta) |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Refinada |
| **Deriva de** | [CU-05](../casos-de-uso/CU-05-emitir-orden-de-mision-y-documentos.md) paso 11 · [CU-06](../casos-de-uso/CU-06-despachar-y-registrar-salida.md) E9 · `T-12` |

## Historia

**Como** Motorista que va a zona sin señal
**quiero** salir con la hoja de bitácora impresa, con folio y con las mismas casillas, en el mismo orden y con los mismos nombres que la pantalla donde después se digita
**para** poder registrar el viaje completo en papel sin inventar formato, y para que quien digite después no tenga que interpretar nada

## Contexto

Más de dos millones de personas del área rural hondureña no tienen acceso a internet `[V]` (INE, EPHPM julio 2025). El papel no es el plan B: es parte del diseño. Si la hoja impresa pide los datos en un orden distinto al de la pantalla, la digitación se convierte en un ejercicio de traducción, y **cada traducción es un error de kilometraje o de galonaje esperando**.

La paridad exacta es lo que hace que el motorista reconozca su formato de siempre y que el digitador transcriba casilla por casilla sin decidir nada. Es también lo que decide la adopción: si el sistema le complica el registro al motorista, el motorista vuelve al cuaderno y todo lo demás da igual ([CE-09](../casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md)).

## Reglas que la gobiernan

- [RN-80](../../01-negocio/reglas/RN-80-hoja-de-bitacora-impresa-con-folio.md) — **Regla rectora**: el despacho emite la hoja de bitácora con folio, QR y paridad exacta con la pantalla de digitación
- [RN-25](../../01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md) — Folio único, QR verificable, firma y sello, huella
- [RN-47](../../01-negocio/reglas/RN-47-digitacion-diferida-desde-papel.md) — La digitación diferida deja constancia de quién digitó y del original escaneado
- [RN-46](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md) — Fecha del hecho y fecha de captura son campos distintos, ambos obligatorios
- [RN-43](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md) — Toda captura de campo se completa sin ninguna conectividad

## Casos especiales que la afectan

- [CE-09](../casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md) — Zona sin señal: bitácora en papel, digitada días después
- [CE-22](../casos-especiales/CE-22-odometro-inconsistente.md) — La hoja debe permitir anotar la lectura tal como se leyó, aunque sea inconsistente
- [CE-25](../casos-especiales/CE-25-comprobante-perdido-o-estacion-sin-factura.md) — La hoja incluye el registro de consumo con número de comprobante

## Criterios de aceptación

```gherkin
# language: es
Característica: Hoja de bitácora impresa con folio y paridad con la pantalla

  Antecedentes:
    Dada una Orden de Misión "OM-CHO-2026-0143" que se despacha el "2026-09-15"
    Y un vehículo "Pickup Toyota Hilux" con correlativo "INS-P-014" y placa "PAA-1234"
    Y un motorista "José Martínez"

  Escenario: Se rechaza el despacho si no se emite la hoja de bitácora
    Cuando el Encargado de Despacho intenta emitir la Orden de Misión sin la hoja de bitácora
    Entonces el sistema rechaza la emisión
    Y muestra "La hoja de bitácora impresa es parte obligatoria del juego documental de toda misión."

  Escenario: La hoja se emite con folio propio y datos de la misión preimpresos
    Cuando el Encargado de Despacho emite el juego documental de "OM-CHO-2026-0143"
    Entonces la hoja de bitácora lleva folio propio, código QR de verificación,
      espacio de firma y sello y huella del contenido electrónico
    Y trae preimpresos el folio de la Orden de Misión "OM-CHO-2026-0143",
      el correlativo del vehículo "INS-P-014", el nombre del motorista "José Martínez",
      la ruta autorizada con sus destinos en orden y la ventana temporal

  Escenario: Las casillas del papel coinciden en nombre y orden con la pantalla de digitación
    Cuando el Encargado de Despacho emite la hoja de bitácora
    Entonces las casillas impresas aparecen en el mismo orden y con el mismo nombre
      que los campos de la pantalla de digitación diferida
    Y una casilla que exista en el papel y no en la pantalla impide publicar el formato
    Y una casilla que exista en la pantalla y no en el papel impide publicar el formato

  Escenario: La hoja distingue la fecha del hecho de la fecha de captura
    Cuando el Encargado de Despacho emite la hoja de bitácora
    Entonces cada evento tiene casilla de "fecha y hora del hecho"
    Y la pantalla de digitación exige además la fecha de captura, que registra el sistema

  Escenario: La misión operada en papel queda marcada desde el despacho
    Dado que no hay dispositivo de campo disponible para "OM-CHO-2026-0143"
    Cuando el Encargado de Despacho confirma el despacho
    Entonces el sistema marca la misión como "operada en papel"
    Y muestra "Sin dispositivo de campo. La captura será en la hoja de bitácora impresa y la digitación diferida se hará al retorno."
    Y la ausencia de dispositivo se imputa como condición institucional,
      no como falta del motorista, en el indicador de oportunidad de registro

  Escenario: La reimpresión de la hoja conserva el mismo folio
    Dado que el motorista extravió la hoja de bitácora antes de salir
    Cuando el Encargado de Despacho reimprime la hoja
    Entonces la hoja reimpresa conserva el folio original
    Y la reimpresión queda registrada con autor, momento y motivo "extravío antes de la salida"
```

## Fuera de alcance

- La pantalla de digitación diferida y el plazo para digitar — son de M-16
- El registro de eventos en el dispositivo de campo — es de M-08 y del caso de uso de ejecución en ruta
- La conciliación galonaje–kilometraje construida sobre lo digitado — es de M-09 y M-13

## Notas y pendientes

- `[C]` **Formatos en papel vigentes de la institución**, en especial la bitácora — insumo #2. **La paridad se define contra el formato real**; hasta tenerlo, el conjunto de casillas es propuesta.
- `[C]` **¿El talonario preimpreso de bitácora trae folio propio?** Si se conserva, hay dos numeraciones que cruzar — insumo #46.
- `[C]` **Plazo máximo de digitación diferida en días hábiles** y desde cuándo corre el plazo de liquidación cuando el retorno se registra días después — insumo #45.
- `[C]` **¿Puede digitar la hoja de papel quien después liquida esa misma misión?** En una delegación de tres personas es la misma persona — insumo #47.
- `[V]` La exigencia de operar sin conectividad y con paridad papel-pantalla proviene de [NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md).
