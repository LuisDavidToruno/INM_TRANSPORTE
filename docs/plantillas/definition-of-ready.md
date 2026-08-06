# Definition of Ready

Una historia **no entra a un sprint** si no cumple todos estos puntos. No es burocracia: cada punto que falta se convierte en una interrupción a mitad del desarrollo, y en este dominio esa interrupción suele ser "¿y qué pasa cuando…?" — es decir, un caso especial que nadie pensó.

## Lista de verificación

### Definición

- [ ] Tiene un `HU-xxx` asignado y está en el archivo que le corresponde
- [ ] El actor es un rol del [glosario](../00-vision/glosario.md), no una descripción genérica como "el usuario"
- [ ] El "para" expresa un beneficio verificable, no una repetición del "quiero"
- [ ] Está asignada a un módulo `M-xx`

### Reglas y excepciones

- [ ] Referencia al menos una regla de negocio `RN-xx` que la gobierna
- [ ] Las reglas referenciadas existen y están escritas — no son un enlace a un archivo que no se ha creado
- [ ] Se identificaron los casos especiales `CE-xx` que la afectan, o se dejó constancia explícita de que no hay ninguno
- [ ] Si la historia toca viáticos, combustible, autorizaciones o datos personales, tiene su ficha `NRM-xx` enlazada

### Verificabilidad

- [ ] Tiene criterios de aceptación en Gherkin español
- [ ] Los criterios cubren al menos un camino de rechazo, no solo el camino feliz
- [ ] Cada criterio es observable: alguien externo puede determinar si se cumple sin preguntarle al desarrollador
- [ ] Los mensajes que ve el usuario están especificados, no dejados a criterio de implementación

### Alcance y dependencias

- [ ] El "fuera de alcance" está escrito y descarta lo que razonablemente se podría asumir incluido
- [ ] Las historias de las que depende están terminadas o programadas antes en el mismo sprint
- [ ] No hay `[C]` pendientes que impidan implementarla — si los hay, están marcados y el PO aceptó la asunción por escrito

### Datos y diseño

- [ ] Las entidades que toca están en el modelo de datos, o la historia incluye su definición
- [ ] Si genera o modifica un documento oficial impreso, el formato está diseñado
- [ ] Si opera en campo, está claro qué parte funciona sin conectividad

## Qué hacer cuando algo no se cumple

**No se negocia el DoR para meter la historia igual.** Se hace una de dos cosas:

1. **Se completa lo que falta** — normalmente son 15 minutos de refinamiento.
2. **Se divide la historia**: la parte que sí está lista entra, la que depende de un `[C]` se separa como historia aparte y espera el insumo.

La segunda opción es casi siempre mejor que esperar. Divide.

## El único `[C]` aceptable

Se permite entrar con un `[C]` pendiente cuando: no bloquea la implementación del núcleo, el PO registró por escrito la asunción con la que se procede, y existe una tarea abierta en [`insumos-pendientes.md`](../07-gestion/insumos-pendientes.md) para resolverlo.

Ejemplo válido: implementar el cálculo de viáticos con la estructura de tarifas parametrizada, aunque las tarifas concretas del Acuerdo 401-2026 todavía no se tengan. La lógica no depende de los valores.

Ejemplo inválido: implementar el flujo de aprobación sin saber quién aprueba qué. Ahí el dato *es* la lógica.
