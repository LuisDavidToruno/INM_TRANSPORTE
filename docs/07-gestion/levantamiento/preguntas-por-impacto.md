# Las 12 preguntas, ordenadas por impacto

**Ordenadas por cuánto cambia el diseño la respuesta.** Las cuatro primeras mueven sprints enteros. Si la sesión se corta, hay que haber llegado a la cuatro.

Cada una lleva su número de insumo, para registrar la respuesta después.

---

## Las cuatro que mueven sprints

### 1. ¿La institución tiene almacenamiento propio de combustible — cisterna o bidones? · insumo #36

**Si es sí, el módulo de combustible cambia de circuito completo.** Deja de ser "fondo aprobado y consumo" y aparece control de existencias, mermas, despacho interno y arqueo de tanque.

Es una pregunta de treinta segundos que puede mover el Sprint 6 entero.

### 2. ¿Cuántas personas tiene la delegación más pequeña? · insumo #27

El control interno exige **cinco funciones en personas distintas**. Con tres empleados no se cumple por aritmética.

Preguntar además: **¿qué puesto de la sede podría ejercer remotamente las funciones que la delegación no puede segregar?** Autorizar, aprobar fondo y cerrar no requieren presencia física.

### 3. ¿Quién autoriza la misión de la máxima autoridad? · insumo #28

Es un hueco real del modelo. Cuando el titular de la institución pide un vehículo, **no hay nivel superior al que escalar** — y el flujo se cae en el caso más visible de todos.

Preguntar también por el **autorizador alterno** cuando el titular está ausente.

### 4. ¿El peaje se paga con el viático o es gasto de misión aparte? · insumo #25

Si va en el viático, **es de ARGOS y no nuestro**, y el módulo de peajes se solapa con un sistema que ya existe.

Preguntar además: **¿el motorista paga de su bolsillo y liquida después, o se le entrega efectivo por adelantado?** ¿Tienen tags de telepeaje?

---

## Las cuatro que definen parámetros duros

### 5. ¿Cuál es el horario hábil oficial, y qué feriados aplican? · insumo #32

Determina cuándo hace falta permiso de la máxima autoridad para circular. Preguntar por los **feriados de octubre** en particular: hay legislación posterior que no se pudo verificar.

### 6. ¿Cuál es el plazo para liquidar una misión, y qué pasa si no se liquida? · insumo #32

¿Se bloquean nuevas solicitudes al que debe? ¿Se descuenta por planilla?

### 7. ¿Cómo se resuelve hoy cuando dos áreas piden el mismo vehículo el mismo día? · insumo #31

**Escuchar la respuesta real, no la reglamentaria.** Si es "lo decide quien tiene más jerarquía", eso es justamente lo que el sistema debería evitar — y hay que diseñar un criterio explícito.

### 8. ¿El correlativo del vehículo es único en la institución, o se compone por delegación? · insumo #34

Afecta la identidad del vehículo en todo el sistema. Recordar que **la placa no sirve como identificador**: hay desabastecimiento nacional y vehículos circulando años sin lámina.

---

## Las cuatro que abren o cierran alcance

### 9. ¿Trasladan personas ajenas a la institución? ¿Bajo custodia? ¿Menores? · insumo #39

Cambia la cadena de custodia y el tratamiento de datos personales.

### 10. ¿Hay rutas fijas donde suben y bajan personas en el camino? · insumo #40

Si las hay, **el manifiesto cerrado es impracticable** y hay que modelar conteo por punto de abordaje.

### 11. ¿Movilizan carga peligrosa o especializada? · insumo #38

Agrega requisitos de vehículo, motorista y documentación.

### 12. ¿Qué sistemas existen hoy, y quién administra el servidor? · insumos #16, #17, #9

Contrato de API de ARGOS y de Talento Humano, y **quién es el responsable real de la infraestructura**. Sin eso, la integración es especulación.

---

## Una pregunta que no está en la lista y conviene hacer

> **¿Tienen informes de auditoría —internos o del TSC— sobre flota, combustible o uso de vehículos?**

Si existen, **valen más que toda esta lista junta**. Cada hallazgo describe algo que ya salió mal en la operación real: son requisitos disfrazados, y vienen con la autoridad de quien los levantó. Es el insumo #19.
