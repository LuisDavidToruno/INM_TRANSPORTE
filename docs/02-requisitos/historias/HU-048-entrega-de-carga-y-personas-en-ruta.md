# HU-048 — Registrar la entrega o recepción de carga y de personas en ruta

| Campo | Valor |
|---|---|
| **Módulo** | M-08 Ejecución y Bitácora · M-17 Traslado de Personas Externas |
| **Actor** | ACT-06 Motorista |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — falta el catálogo de datos mínimos autorizados de personas externas, que `NRM-07` deja `[C]`, y el formato en papel vigente del acta de entrega (insumo #2), sin el cual la pantalla no puede tener paridad con la casilla del papel |

## Historia

**Como** Motorista
**quiero** levantar en el dispositivo el acta de entrega de lo que traslado, con el inventario y la firma de quien recibe, aunque no tenga señal
**para** que la cadena de custodia del bien del Estado no se corte en el punto donde lo entregué y no responder yo por un faltante que no causé

## Contexto

La institución no traslada "viajes": traslada recursos institucionales — equipos, herramientas, insumos, materiales, personal y personas externas. Lo que se entrega sin acta se convierte, meses después, en un faltante sin dueño.

**Toda diferencia contra el inventario declarado se registra como faltante; el inventario no se ajusta.** Ajustar el inventario para que cuadre es la forma más común de hacer desaparecer un hallazgo, y es exactamente lo que el Tribunal Superior de Cuentas busca ([RN-69](../../01-negocio/reglas/RN-69-inventario-de-la-carga-y-acta-de-entrega.md)).

En el traslado de personas externas, el manifiesto **se cerró al despachar**: los cambios en ruta se registran como novedad, no como edición del manifiesto ([RN-53](../../01-negocio/reglas/RN-53-cierre-del-manifiesto-al-despacho.md)).

## Reglas que la gobiernan

- [RN-69](../../01-negocio/reglas/RN-69-inventario-de-la-carga-y-acta-de-entrega.md) — **Regla rectora**: la carga se entrega con acta y toda diferencia se declara como faltante
- [RN-53](../../01-negocio/reglas/RN-53-cierre-del-manifiesto-al-despacho.md) — El manifiesto se cierra al despachar; en ruta solo se registran novedades
- [RN-51](../../01-negocio/reglas/RN-51-minimizacion-de-datos-de-personas-externas.md) — Solo se capturan los datos mínimos del catálogo autorizado
- [RN-52](../../01-negocio/reglas/RN-52-registro-de-consultas-a-manifiestos.md) — Toda consulta al manifiesto se registra: quién vio qué y cuándo
- [RN-43](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md) — El acta se levanta sin ninguna conectividad
- [RN-22](../../01-negocio/reglas/RN-22-custodia-del-vehiculo.md) — La custodia no se interrumpe: siempre hay alguien identificado que responde

## Casos especiales que la afectan

- [CE-18](../casos-especiales/CE-18-carga-y-pasajeros-en-la-misma-mision.md) — Carga y pasajeros en la misma misión
- [CE-04](../casos-especiales/CE-04-robo-de-vehiculo-o-de-carga-en-mision.md) — Sustracción de la carga durante la misión
- [CE-02](../casos-especiales/CE-02-averia-mecanica-en-ruta.md) — La carga se transborda o se resguarda por avería

## Criterios de aceptación

```gherkin
# language: es
Característica: Acta de entrega de carga y novedades del manifiesto en ruta

  Antecedentes:
    Dada la Orden de Misión "OM-2026-0451" en estado "EN_RUTA"
    Y un inventario declarado al despacho de "12 cajas de formularios" y "3 computadoras portátiles"
    Y un manifiesto de personas externas cerrado al despacho con "4" personas
    Y que el dispositivo lleva 5 días sin conectividad

  Escenario: Se rechaza el ajuste del inventario para que cuadre
    Cuando "José Martínez" intenta modificar el inventario declarado de "3 computadoras portátiles" a "2 computadoras portátiles"
    Entonces el sistema rechaza la modificación
    Y muestra "El inventario declarado al despacho no se edita. Si entrega menos de lo declarado, registre la diferencia como faltante en el acta."

  Escenario: Se rechaza cerrar el acta sin quien recibe
    Cuando "José Martínez" intenta cerrar el acta de entrega en "Delegación de Choluteca" sin registrar quién recibe
    Entonces el sistema rechaza el cierre
    Y muestra "Falta el nombre, el puesto y la firma de quien recibe. Sin receptor identificado la custodia queda abierta y la misión no se puede liquidar."

  Escenario: Se rechaza editar el manifiesto de personas externas en ruta
    Cuando "José Martínez" intenta agregar una persona al manifiesto de "OM-2026-0451"
    Entonces el sistema rechaza la edición
    Y muestra "El manifiesto se cerró al despachar. Registre el cambio como novedad, indicando qué pasó y a qué hora."

  Escenario: La entrega con faltante se registra como faltante, no como ajuste
    Cuando "José Martínez" registra la entrega de "12 cajas de formularios" y "2 computadoras portátiles" con firma del receptor "Ana Zelaya, Encargada de Delegación"
    Entonces el sistema cierra el acta de entrega con folio
    Y registra un faltante de "1 computadora portátil" contra el inventario declarado
    Y el faltante abre expediente en M-12 con responsable y plazo
    Y la Orden de Misión no puede pasar a "CERRADA" mientras el faltante esté abierto

  Escenario: Entrega completa sin conectividad
    Cuando "José Martínez" registra la entrega de "12 cajas de formularios" y "3 computadoras portátiles" con firma del receptor "Ana Zelaya, Encargada de Delegación"
    Entonces el sistema cierra el acta de entrega con folio del rango asignado a la delegación
    Y la deja en estado de sincronización "PENDIENTE_DE_ENVIO"
    Y la custodia de lo entregado pasa a "Ana Zelaya" desde la hora del hecho

  Escenario: Una persona del manifiesto no sube en el punto previsto
    Cuando "José Martínez" registra la novedad "persona del manifiesto no se presentó" para la persona con identificador interno "P-3"
    Entonces el sistema registra la novedad con hora del hecho, sin alterar el manifiesto cerrado
    Y no muestra ningún dato de la persona fuera del catálogo mínimo autorizado
```

## Fuera de alcance

- La conformación del manifiesto y la declaración del inventario al despachar — es de [CU-06](../casos-de-uso/CU-06-despachar-y-registrar-salida.md)
- El expediente de sustracción de la carga — es [HU-058](HU-058-registrar-interrupcion-en-ruta.md) y M-12
- La depuración o seudonimización de datos personales pasado el plazo de conservación — insumo #71

## Notas y pendientes

- `[C]` Catálogo de datos mínimos autorizados de personas externas para la institución — [NRM-07](../../01-negocio/normativa/NRM-07-transparencia-y-datos-personales.md) lo deja `[C]`
- `[C]` Formato en papel vigente del acta de entrega, para que la pantalla tenga paridad exacta con la casilla del papel — insumo #2
- `[I]` La firma en pantalla no es firma electrónica certificada: es constancia interna con registro de quién, cuándo y sobre qué contenido
