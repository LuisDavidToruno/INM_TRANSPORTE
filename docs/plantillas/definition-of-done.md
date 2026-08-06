# Definition of Done

Una historia está terminada cuando cumple **todos** estos puntos. "Terminada" significa que puede ir a producción en la institución piloto, no que el código compila.

## Sprint 0 y 1 — artefactos de análisis

Estos sprints no producen código. La DoD es documental.

- [ ] El artefacto está en su carpeta, con su ID y siguiendo su plantilla
- [ ] La trazabilidad hacia arriba está completa: historia→regla→norma, o caso especial→regla
- [ ] Toda afirmación normativa lleva su nivel de verificación `[V]` `[P]` `[C]` `[I]`
- [ ] Los diagramas Mermaid renderizan sin error
- [ ] Los enlaces relativos funcionan
- [ ] Fue revisado por una especialidad distinta a la que lo produjo, y los hallazgos se resolvieron o se registraron en `docs/05-calidad/hallazgos/`
- [ ] Los datos que faltan están marcados `[C]` y registrados en `insumos-pendientes.md` — no rellenados con suposiciones
- [ ] El PO lo revisó y lo dio por bueno

## Sprint 2 en adelante — historias con código

Todo lo anterior, cuando aplique, más:

### Funcionalidad

- [ ] Todos los criterios de aceptación pasan, incluidos los de rechazo
- [ ] Los mensajes al usuario son los especificados, en español, y son accionables
- [ ] Los casos especiales `CE-xx` enlazados a la historia están implementados o explícitamente diferidos con el acuerdo del PO

### Calidad del código

- [ ] Las reglas de negocio están implementadas donde corresponde, no dispersas en la interfaz
- [ ] Pruebas automatizadas de las reglas `RN-xx` que la historia implementa
- [ ] Revisión de código hecha por una especialidad distinta a quien lo escribió
- [ ] Sin credenciales, rutas ni valores normativos cableados en el código

### Control interno y auditoría

Esto no es opcional en este sistema. Una historia que toca datos operativos y no lo cumple, **no está terminada**:

- [ ] Toda operación que crea, modifica o anula un registro deja traza en la bitácora de auditoría: quién, qué, cuándo, valor anterior y nuevo
- [ ] Nada se borra físicamente; las anulaciones son asientos reversos con motivo y autor
- [ ] Si la historia involucra autorización, la segregación de funciones está verificada con prueba automatizada
- [ ] Si la historia maneja parámetros normativos, usan la vigencia por fecha del hecho y hay prueba que lo demuestra

### Campo y operación

- [ ] Si la funcionalidad se usa en campo, se probó **con el dispositivo en modo avión** y sincronizando después
- [ ] Si genera un documento oficial, se imprimió realmente y es legible en blanco y negro, tamaño carta
- [ ] Si el QR de verificación aplica, se escaneó desde un teléfono y resuelve correctamente

### Datos personales

- [ ] Si la historia registra datos de personas, se aplicó minimización: solo lo necesario para el control
- [ ] El acceso a esos datos queda registrado (quién consultó qué y cuándo)
- [ ] Los datos personales quedan separables de los datos de gestión pública para efectos de publicación en transparencia

### Documentación

- [ ] El manual de usuario del rol afectado está actualizado
- [ ] Si cambió el modelo de datos, el diccionario de datos está actualizado
- [ ] Si se tomó una decisión de arquitectura, existe su `ADR-xxx`

## Lo que NO cuenta como terminado

- "Funciona en mi máquina"
- "Falta solo la validación de X"
- "El caso especial lo vemos después" — sin que el PO lo haya acordado y quedado escrito
- "La prueba offline la hacemos al final" — es exactamente donde aparecen los problemas irrecuperables
- Pruebas que pasan porque se ajustaron a lo que hace el código
