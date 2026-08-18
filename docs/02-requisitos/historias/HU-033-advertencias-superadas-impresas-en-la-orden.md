# HU-033 — Imprimir en la Orden las advertencias que se superaron y quién continuó a pesar de ellas

| Campo | Valor |
|---|---|
| **Módulo** | M-15 Formatos Oficiales e Impresión · M-07 Programación y Despacho |
| **Actor** | ACT-05 Encargado de Despacho (emite) · ACT-15 Verificador en Carretera (destinatario) · ACT-12 Auditor Interno |
| **Prioridad** | Media |
| **Sprint** | sin asignar |
| **Estado** | Refinada |
| **Deriva de** | [CU-05](../casos-de-uso/CU-05-emitir-orden-de-mision-y-documentos.md) paso 10 · `T-12` |

## Historia

**Como** Auditor Interno
**quiero** que la Orden de Misión impresa liste las advertencias que se superaron durante la programación y el despacho, con el nombre de quien decidió continuar
**para** que la excepción quede a la vista de quien recibe el documento en carretera y no escondida en una pantalla que nadie abre

## Contexto

Una advertencia que nadie ve no es un control: es exactamente lo que el auditor pregunta después. Si el vehículo salió con la póliza vencida, con la constatación de rotulación caducada o con la disponibilidad verificada sobre un espejo de once días, eso **es parte del documento**, no una nota interna.

Y hay una razón operativa además de la de control: quien recibe la orden en un retén debe poder ver **por qué firmó quien firmó**. Cuando la autorización escaló porque el autorizador natural era el solicitante, el nombre que aparece en la Orden no es el esperado, y sin la explicación impresa parece una irregularidad cuando es justo lo contrario.

## Reglas que la gobiernan

- [RN-16](../../01-negocio/reglas/RN-16-seguro-y-revision-mecanica.md) — La advertencia por póliza o revisión vencidas queda visible con el nombre de quien continuó
- [RN-18](../../01-negocio/reglas/RN-18-rotulacion-del-vehiculo-del-estado.md) — La constatación de rotulación caducada se advierte con su fecha
- [RN-50](../../01-negocio/reglas/RN-50-degradacion-por-sincronizacion-detenida.md) — La evaluación sobre espejo desactualizado se imprime en el documento
- [RN-02](../../01-negocio/reglas/RN-02-escalamiento-de-autorizacion.md) — El escalamiento de la autorización se imprime con su causa
- [RN-03](../../01-negocio/reglas/RN-03-registro-inmutable-de-autorizacion.md) — Cada acuse de advertencia registra identidad, rol y momento
- [RN-25](../../01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md) — El contenido impreso queda cubierto por la huella del documento electrónico

## Casos especiales que la afectan

- [CE-15](../casos-especiales/CE-15-vehiculo-en-comodato-o-alquilado.md) — Las advertencias de póliza y rotulación cambian según el régimen de tenencia
- [CE-17](../casos-especiales/CE-17-vehiculo-sin-placa-metalica.md) — Sin lámina, el umbral de caducidad de la constatación es más corto
- [CE-13](../casos-especiales/CE-13-motorista-no-disponible-por-talento-humano.md) — La antigüedad del espejo se imprime como condición de la verificación

## Criterios de aceptación

```gherkin
# language: es
Característica: Advertencias superadas impresas en la Orden de Misión

  Antecedentes:
    Dada una Orden de Misión "OM-2026-0451" lista para despachar
    Y un vehículo "Pickup Toyota Hilux" con correlativo "INS-P-014", régimen de tenencia "propio"
    Y un Jefe de Transporte "Carlos Rodríguez" y un Encargado de Despacho "Sandra Paz"

  Escenario: La póliza vencida superada se imprime con nombre y fecha del acuse
    Dado que la póliza del "INS-P-014" venció el "2026-08-15"
    Y que "Carlos Rodríguez" acusó la advertencia el "2026-09-01" al programar
    Cuando el Encargado de Despacho emite la Orden de Misión "OM-2026-0451"
    Entonces la sección de advertencias del impreso contiene
      "Póliza de seguro vencida el 15/08/2026. Continuó Carlos Rodríguez, Jefe de Transporte, el 01/09/2026."

  Escenario: La constatación de rotulación caducada se imprime con su fecha
    Dado que la última constatación de la identificación institucional del "INS-P-014"
      se registró el "2026-01-10"
    Y que el umbral de caducidad es de "180" días
    Cuando el Encargado de Despacho emite la Orden de Misión "OM-2026-0451"
    Entonces la sección de advertencias contiene
      "Constatación de franjas, leyenda, siglas y correlativo con fecha 10/01/2026: caducada."

  Escenario: La verificación sobre espejo desactualizado se imprime con la antigüedad
    Dado que el espejo de Talento Humano se sincronizó por última vez el "2026-08-30"
    Y que el despacho ocurre el "2026-09-10"
    Cuando el Encargado de Despacho emite la Orden de Misión "OM-2026-0451"
    Entonces la sección de advertencias contiene
      "Disponibilidad del motorista verificada con datos sincronizados hace 11 días."

  Escenario: El escalamiento de la autorización se imprime con su causa
    Dado que la autorización de la solicitud escaló al nivel inmediato superior
      porque el autorizador natural era el solicitante
    Cuando el Encargado de Despacho emite la Orden de Misión "OM-2026-0451"
    Entonces la Orden imprime "Autorización escalada al nivel inmediato superior: el autorizador natural era el solicitante."
    Y muestra el nombre y el cargo de quien autorizó finalmente

  Escenario: Sin advertencias, la sección lo declara expresamente
    Dado que no hay advertencias superadas en la programación ni en el despacho
    Cuando el Encargado de Despacho emite la Orden de Misión "OM-2026-0451"
    Entonces la sección de advertencias imprime "Sin advertencias registradas."
    Y la sección no se omite del documento

  Escenario: La sección de advertencias no se puede excluir de la impresión
    Dado que la Orden de Misión "OM-2026-0451" tiene dos advertencias superadas
    Cuando el Encargado de Despacho intenta emitir el documento sin la sección de advertencias
    Entonces el sistema rechaza la emisión
    Y muestra "La sección de advertencias es parte obligatoria de la Orden de Misión."
```

## Fuera de alcance

- La generación de cada advertencia — es de las historias que la producen: [HU-023](HU-023-documentacion-y-estado-operativo-del-vehiculo.md), [HU-026](HU-026-disponibilidad-del-motorista-contra-el-espejo.md)
- El reporte consolidado de excepciones por período — es de M-14
- Las advertencias que surgen en ruta — son de M-08

## Notas y pendientes

- `[C]` **Formatos en papel vigentes** — insumo #2: define dónde cabe la sección de advertencias en la hoja real de la institución.
- `[I]` La redacción concreta de cada línea de advertencia es propuesta del equipo; se valida con la Gerencia Administrativa en la revisión de formatos.
