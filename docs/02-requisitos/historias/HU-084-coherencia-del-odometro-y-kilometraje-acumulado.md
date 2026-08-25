# HU-084 — Bloquear el retroceso de odómetro y sostener el kilometraje acumulado del expediente

| Campo | Valor |
|---|---|
| **Módulo** | M-08 Ejecución y Bitácora · M-03 Flota Vehicular |
| **Actor** | ACT-06 Motorista (captura) · ACT-11 Encargado de Mantenimiento (acta de intervención) · ACT-04 Jefe de Transporte (resuelve) |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — faltan `salto_maximo_km_por_dia` y `salto_maximo_km_por_hora` por tipo de vehículo (insumo #1), el plazo máximo de operación con el odómetro averiado (insumo #1) y si hay unidades con tablero en millas en la flota (insumo #5): asumir kilómetros produce un error del 60 % que nadie detecta hasta que la conciliación es absurda |

## Historia

**Como** Jefe de Transporte
**quiero** que el sistema bloquee toda lectura de odómetro que retroceda, admita el acta de intervención del instrumento como única salida, y lleve el kilometraje acumulado del vehículo **independiente de lo que marque el tablero**
**para** que cambiar un tablero deje de corromper el histórico de kilometraje del vehículo, que es el denominador de toda la conciliación de combustible

## Contexto

El odómetro es el denominador de toda la conciliación de combustible y el testigo de la ruta efectivamente recorrida. Es también el dato sobre el que recae el mayor incentivo de manipulación. **Un retroceso silenciosamente aceptado invalida meses de conciliación hacia atrás.**

El caso legítimo por excelencia es el reemplazo del tablero. El kilometraje acumulado del vehículo **no puede** depender de la lectura del instrumento: tiene que existir un acumulado propio del expediente que sobreviva al cambio. Si esto no se modela desde el inicio, cada tablero reemplazado corrompe el histórico.

El bloqueo no es un obstáculo al registro del hecho: es corregir, no ocultar.

## Reglas que la gobiernan

- [RN-31](../../01-negocio/reglas/RN-31-odometro-de-retorno.md) — El retroceso se bloquea; la lectura rechazada se conserva como lectura observada
- [RN-89](../../01-negocio/reglas/RN-89-kilometraje-acumulado-invariante-del-expediente.md) — El kilometraje acumulado es atributo del expediente, no lectura del instrumento
- [RN-90](../../01-negocio/reglas/RN-90-intervencion-del-instrumento-de-medicion.md) — Toda intervención del odómetro es evento con orden de trabajo y autorización nominativa
- [RN-30](../../01-negocio/reglas/RN-30-conciliacion-galonaje-kilometraje.md) — Con odómetro averiado, el rendimiento de esa misión se marca **no concluyente**
- [RN-05](../../01-negocio/reglas/RN-05-registro-cerrado-no-se-edita.md) — El motorista no corrige su propio registro cerrado: solicita corrección con fotografía del tablero

## Casos especiales que la afectan

- [CE-22](../casos-especiales/CE-22-odometro-inconsistente.md) — Eje de la historia
- [CE-21](../casos-especiales/CE-21-galonaje-que-no-cuadra-con-kilometraje.md) — Lo que se corrompe si el odómetro no es confiable
- [CE-02](../casos-especiales/CE-02-averia-mecanica-en-ruta.md) — Traslado en grúa: recorre distancia sin sumar odómetro

## Criterios de aceptación

```gherkin
# language: es
Característica: Coherencia del odómetro y kilometraje acumulado del expediente

  Antecedentes:
    Dado un vehículo "TR-0045" con última lectura conocida de "148,320" km el "2026-09-25", origen "carga de combustible"
    Y un kilometraje acumulado del expediente de "148,320" km
    Y un parámetro "salto_maximo_km_por_dia" de "900" km para el tipo "Pickup"

  Escenario: Se bloquea la captura por retroceso, con el cálculo explícito
    Cuando el motorista registra una lectura de "147,900" km
    Entonces el sistema rechaza la captura
    Y muestra "Última lectura: 148,320 km el 25/09/2026 (carga de combustible). Lectura ingresada: 147,900 km. Retroceso de 420 km."
    Y conserva "147,900" km como lectura observada, sin descartarla

  Escenario: El bloqueo funciona también en el dispositivo sin conectividad
    Dado un dispositivo sin señal con la última lectura conocida en su paquete
    Cuando el motorista registra una lectura de "147,900" km
    Entonces el dispositivo rechaza la captura y muestra el mismo cálculo de retroceso

  Escenario: Un salto grande advierte pero no bloquea
    Cuando el motorista registra una lectura de "149,700" km el mismo día "2026-09-25"
    Entonces el sistema acepta la captura
    Y exige justificación obligatoria del salto de "1,380" km en un día
    Y marca el evento para revisión

  Escenario: El acta de intervención del instrumento es la única salida al bloqueo
    Dado un acta de intervención registrada por el Encargado de Mantenimiento el "2026-09-26"
    Y una lectura del instrumento retirado de "148,900" km y del instrumento instalado de "000,000" km
    Y una orden de trabajo "OT-2026-0177" con autorización nominativa
    Cuando el motorista registra una lectura de "000,150" km el "2026-09-27"
    Entonces el sistema acepta la captura
    Y el kilometraje acumulado del expediente pasa a "149,050" km
    Y el desfase entre instrumento y acumulado queda registrado en "148,900" km

  Escenario: Se rechaza el acta de intervención sin las dos lecturas
    Cuando el Encargado de Mantenimiento registra la sustitución del tablero sin la lectura del instrumento retirado
    Entonces el sistema rechaza el acta
    Y muestra "Registre la lectura del instrumento retirado y la del instalado. Sin ambas, el kilometraje acumulado del vehículo no se puede sostener."

  Escenario: El acumulado del expediente es el que se usa en la conciliación
    Dado un vehículo con acumulado de "149,050" km y lectura de instrumento de "000,150" km
    Cuando el Jefe de Transporte concilia una misión de ese vehículo
    Entonces el cálculo de kilómetros recorridos usa el acumulado del expediente
    Y no usa la lectura cruda del instrumento

  Escenario: Con odómetro averiado el abastecimiento se registra y el rendimiento no concluye
    Dado un odómetro declarado averiado el "2026-09-28"
    Cuando el motorista registra "12.0" galones por "L 1,560.00" sin lectura de odómetro
    Entonces el sistema acepta el registro del abastecimiento
    Y marca el cálculo de rendimiento de esa misión como "no concluyente"
    Y muestra "Odómetro averiado desde el 28/09/2026. El rendimiento de esta misión no será concluyente."

  Escenario: Lectura de retorno igual a la de salida
    Dado un odómetro de salida de "149,050" km
    Cuando el motorista registra un odómetro de retorno de "149,050" km con consumo de combustible registrado
    Entonces el sistema acepta la captura
    Y produce advertencia "El vehículo no registra recorrido y sí registra consumo. Verifique la lectura."

  Escenario: El motorista no corrige su propio registro cerrado
    Cuando el motorista intenta modificar una lectura ya registrada
    Entonces el sistema rechaza la modificación
    Y muestra "Registre una solicitud de corrección con fotografía del tablero. La resuelve el Jefe de Transporte con asiento."
```

## Fuera de alcance

- La captura del abastecimiento en sí — es [HU-082](HU-082-registrar-abastecimiento-sin-conectividad.md)
- El cálculo de la desviación de rendimiento — es [HU-088](HU-088-conciliar-galonaje-contra-kilometraje.md)
- El registro de la orden de trabajo de taller: pertenece a M-11 Mantenimiento y Taller
- La detección de un odómetro alterado de forma consistente: es indetectable por esta vía; se mitiga con fotografía del tablero y cruce contra peajes y ruta

## Notas y pendientes

- `[C]` `salto_maximo_km_por_dia` y `salto_maximo_km_por_hora` por tipo de vehículo. Una motocicleta y un cabezal no tienen el mismo techo diario — insumo **#1**
- `[C]` `dias_max_odometro_averiado`: plazo máximo de operación con el instrumento averiado — insumo **#1**
- `[C]` ¿Hay unidades con tablero en millas en la flota? La ficha debe declarar la unidad; asumir kilómetros produce un error del 60 % que nadie detecta hasta que la conciliación es absurda — insumo **#5**
- `[V]` La exigencia de registrar odómetro de salida y retorno y detectar lecturas inconsistentes — [NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md)
