# RNF-08 — El tablero de ruta dice dónde está cada vehículo, o dice honestamente desde cuándo no lo sabe

| Campo | Valor |
|---|---|
| **Categoría** | Rendimiento / Disponibilidad / Usabilidad |
| **Prioridad** | Alto |
| **Origen** | M-19, derivado del insumo #10 resuelto por el PO; [`CE-08`](../casos-especiales/CE-08-multi-destino-con-esperas-prolongadas-en-sitio.md) |
| **Afecta arquitectura** | **Sí** — exige un canal de actualización en vivo y una serie temporal de alto volumen. No es determinante de stack: el componente de mapas se reutiliza de ARGOS (insumo #18) |

## Enunciado

El seguimiento en ruta **debe** mostrar, para cada vehículo con misión activa, su **última posición conocida**, su **destino en curso** dentro de una misión multi-destino, y su **tiempo de espera acumulado en sitio**.

El requisito central no es la precisión: es la **honestidad de la degradación**. En Honduras rural el vehículo pasa horas sin cobertura, y eso es lo normal ([NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md)). Un tablero que muestra un ícono en el último punto conocido sin decir que ese dato tiene cuatro horas hace que el despachador crea que el vehículo está detenido en la carretera. Un tablero que dice *"última posición: hace 4 h 12 min, a 12 km de Catacamas — sin cobertura"* es útil.

**Ninguna posición se pierde por falta de red.** Se encolan en el dispositivo y se suben al reconectar, con su marca de tiempo original — es el mismo mecanismo del [`RNF-03`](RNF-03-operacion-sin-conectividad.md), aplicado a una serie de baja criticidad y alto volumen.

## Métrica y umbral

| Métrica | Umbral |
|---|---|
| Frecuencia de captura de posición con el vehículo en movimiento | Cada 5 min, **parámetro configurable con vigencia** `[C]` insumo #74 |
| Frecuencia de captura con el vehículo detenido | Cada 30 min `[C]` |
| Latencia entre la captura de una posición y su visibilidad en el tablero, con red disponible | `p95` < 60 s |
| Posiciones capturadas sin red que se pierden | **0.** Se encolan y suben con su marca de tiempo original |
| Antigüedad de la última posición mostrada sin declararla en pantalla | **0.** Todo marcador muestra "hace HH:MM" |
| Antigüedad que cambia el estado del marcador a *sin contacto* | > 2 h `[C]` — parámetro por zona, porque en la Mosquitia 2 h es normal y en Tegucigalpa es una alerta |
| Actualización manual de estado por el motorista (arribé, salgo, espero, incidente) | ≤ 3 toques, funciona sin red, y **prevalece sobre la posición automática** |
| Cálculo del tiempo de espera en sitio | Automático desde el arribo declarado o desde la permanencia dentro del radio del destino `[C]` insumo #74; **el motorista puede corregirlo con motivo**, y la corrección queda como asiento |
| Carga del tablero con 30 vehículos activos | < 3 s (ver [`RNF-01`](RNF-01-rendimiento-de-consulta-y-operacion.md)) |
| Consumo de datos por seguimiento en una jornada de 8 h | < 3 MB `[C]` — la institución no paga el plan del motorista, ver [`RNF-12`](RNF-12-uso-en-campo.md) |
| Consumo de batería atribuible al seguimiento en 8 h | ≤ 10 % del total ([`RNF-12`](RNF-12-uso-en-campo.md)) |
| Precisión de posición exigida | **Ninguna como bloqueo.** La posición es información de apoyo; **no** es prueba de nada y **no** condiciona ninguna liquidación |
| Retención de la serie de posiciones a densidad completa | 1 año; después se reduce densidad, nunca se elimina ([`RNF-02`](RNF-02-volumen-y-crecimiento-del-acervo.md)) |

## Cómo se verifica

1. **Recorrido real con corte de cobertura**: se ejecuta una misión de 4 destinos con dos tramos sin cobertura de al menos 90 min. Al final se verifica que la traza reconstruida no tiene huecos, que las marcas de tiempo son las del hecho y no las de subida, y que durante el corte el tablero mostró *sin contacto* con la antigüedad correcta.
2. **Prueba de espera en sitio**: en un destino se permanece 2 h 40 min. Se compara el tiempo calculado por el sistema con el cronómetro. Se prueba también la corrección manual y se verifica su asiento.
3. **Prueba de la declaración del motorista**: el motorista marca *arribé* estando la posición automática 800 m fuera del destino. Se verifica que prevalece la declaración y que ambas quedan registradas, sin conflicto silencioso.
4. **Prueba de consumo**: dos dispositivos, uno con seguimiento activo y otro sin él, en la misma jornada de 8 h. Se compara batería y datos consumidos.
5. **Prueba de volumen del tablero**: 30 vehículos activos simulados reportando a 5 min. Se mide latencia y carga del tablero durante 2 h continuas.
6. **Prueba de reducción de densidad**: se reduce la densidad de la serie del año anterior y se verifica que el recorrido de una misión sigue siendo reconstruible y auditable.

## Consecuencia de no cumplirlo

Dos consecuencias distintas, y la segunda es la grave:

- **Si falta el seguimiento**, el despachador coordina por llamada telefónica, como hoy. Se pierde una mejora, no una función crítica.
- **Si el seguimiento miente por omisión** —muestra una posición vieja sin declararlo—, el despachador toma decisiones sobre un vehículo que cree ubicado y no lo está: reasigna una carga, promete una hora de llegada, o no activa la búsqueda cuando un vehículo lleva medio día sin reportar. Y en [`CE-04`](../casos-especiales/CE-04-robo-de-vehiculo-o-de-carga-en-mision.md) —robo del vehículo— la diferencia entre *"sin contacto desde hace 40 min"* y un ícono quieto en el mapa es la diferencia entre reaccionar y no reaccionar.

Un dato viejo presentado como fresco es peor que ningún dato.

## Trazabilidad

- Módulos: M-19, M-08
- Reglas: [`RN-22`](../../01-negocio/reglas/RN-22-custodia-del-vehiculo.md), [`RN-43`](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md), [`RN-46`](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md)
- Casos especiales: [`CE-08`](../casos-especiales/CE-08-multi-destino-con-esperas-prolongadas-en-sitio.md), [`CE-04`](../casos-especiales/CE-04-robo-de-vehiculo-o-de-carga-en-mision.md), [`CE-02`](../casos-especiales/CE-02-averia-mecanica-en-ruta.md), [`CE-06`](../casos-especiales/CE-06-la-mision-se-extiende-mas-dias-destinos-o-kilometros.md)
- Requisitos relacionados: [`RNF-03`](RNF-03-operacion-sin-conectividad.md), [`RNF-12`](RNF-12-uso-en-campo.md), [`RNF-02`](RNF-02-volumen-y-crecimiento-del-acervo.md)
- Insumos: #18 (componente de mapas de ARGOS), #74 (frecuencia aceptable y quién paga los datos)
