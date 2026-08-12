# CE-03 — Colisión en la CA-5 con un motociclista lesionado y el vehículo retenido por la DNVT

| Campo | Valor |
|---|---|
| **Módulos** | M-12 Incidentes y Siniestros, M-08 Ejecución y Bitácora, M-03 Flota, M-04 Documentación, M-17 Traslado de Personas Externas, M-13 Liquidación, M-14 Auditoría, M-16 Operación Desconectada |
| **Estados afectados** | `EN_RUTA`, `RETORNADA` con subtipo retorno sin vehículo, y bloqueo de `CERRADA` |
| **Frecuencia** | Raro pero grave |
| **Impacto** | Legal antes que nada; luego operativo, financiero y de auditoría |
| **Resolución** | Definida en lo operativo. `[C]` en tres puntos: protocolo institucional, condicionamiento del cierre y aviso al asegurador |

## La situación

Miércoles, 16:40. Un microbús de la institución vuelve de San Pedro Sula con siete servidores a bordo. Sobre la CA-5, a la altura de Zambrano, un motociclista particular se le atraviesa al rebasar y hay colisión. El motociclista queda en el suelo con la pierna fracturada. Ninguno de los ocupantes del microbús está herido de gravedad; dos tienen golpes.

El motorista, que lleva once años manejando, hace lo que sabe: no mueve el vehículo, llama al 911, pone los triángulos. Llega la Policía Nacional de Tránsito. Se levanta el parte, se pide la licencia y la matrícula, y el agente informa que **el vehículo queda retenido** hasta que se resuelva la situación. Hay señal de voz intermitente; datos, no.

Al microbús le faltaban 68 km para llegar a Tegucigalpa. Los siete servidores quedan en la carretera. El vehículo no va a volver hoy, y quizá no vuelva esta semana. La póliza de seguro de ese microbús venció en marzo y no se renovó, porque el seguro no es obligatorio por ley vigente y la institución no activó el bloqueo.

## Qué se hace hoy sin sistema

El motorista llama al Jefe de Transporte. La institución manda otro vehículo a recoger a la gente. Se saca copia del parte policial cuando lo entregan —que puede tardar días— y se abre un expediente que vive en una carpeta física en la Gerencia Administrativa.

Lo que casi nunca queda registrado el mismo día: el odómetro al momento del choque, las fotografías del lugar antes de que se mueva nada, la identidad y el estado de las personas que iban a bordo, y quién dio la instrucción de dejar el vehículo en el predio de la DNVT.

`[C]` **Si la institución tiene un protocolo escrito de actuación en accidente y si exige prueba de alcoholemia** — no se inventa. Ver insumo nuevo #45.

## Por qué el flujo normal no lo cubre

El flujo normal registra eventos de una misión que avanza. Aquí la misión se detiene, hay una persona en el suelo, y **lo último que debe hacer el motorista es abrir una aplicación**. Además:

- El vehículo puede no volver nunca a la institución. `T-18` tiene subtipo "retorno sin vehículo", pero el odómetro final es entonces una estimación y el sistema tiene que aceptarla marcada como tal.
- Hay un tercero con datos personales que la institución necesita registrar y que **no es una persona trasladada**: `RN-51` no lo cubre.
- Hay pasajeros que quedaron en carretera y que hay que mover en otro vehículo, con otra orden de misión, sin que eso rompa el manifiesto cerrado de la original (`RN-53`).
- Lo que el motorista declare hoy puede leerse en un juzgado dentro de dos años.

## Regla de resolución

**1. La primera pantalla no pide datos: muestra qué hacer.** El paquete de misión que el dispositivo recibe al despachar ya incluye la **guía de actuación en accidente** (`EF-03` de la máquina de estados, `NRM-06`). Al declarar un accidente, el cliente de campo despliega primero la guía —atender personas, no mover el vehículo, avisar a la autoridad, avisar a la institución— y **solo después** ofrece el formulario. El registro es importante; la persona en el suelo lo es más.

**2. El registro mínimo se captura sin señal, en este orden.** Hora del hecho, ubicación descrita, odómetro, hay o no lesionados, autoridad presente y número de parte si ya lo hay, fotografías. Todo lo demás puede completarse después. `RN-43`: la captura se completa sin ninguna conectividad y no se pierde.

**3. El sistema no captura culpa.** Los campos son de **hecho observable**: qué pasó, dónde, a qué hora, quién estaba presente, qué se ve en la fotografía. **No existe un campo "responsable del accidente" ni "el motorista tuvo la culpa"** que el propio motorista pueda llenar en la carretera, bajo estrés, sin asesoría. La determinación de responsabilidad es resultado de la investigación de M-12 y, en su caso, de la autoridad judicial. Un sistema que le pide a un servidor público autoinculparse en el minuto cinco produce evidencia en contra de su propia institución.

**4. Los datos del tercero son mínimos y su consulta se registra.** Nombre, documento de identidad, teléfono, datos del vehículo y de su seguro, y estado declarado —lesionado o no—. Nada más. El acceso posterior a ese dato queda registrado igual que el de un manifiesto (`RN-52`). Si hay lesionados, el dato de salud se limita a "hay lesionados: sí/no" y al destino de traslado; el sistema **no registra diagnóstico**.

**5. El vehículo sale de la flota disponible en el acto.** El evento lo lleva a `NO_DISPONIBLE` con causa tipificada "incidente bajo investigación" (`W-08`, `RN-19`), aunque físicamente esté en un predio de la DNVT y nadie de la institución lo tenga. Se registra **dónde está y bajo custodia de quién** — un vehículo del Estado retenido sigue siendo un bien del Estado del que hay que responder (`NRM-02`).

**6. La misión termina por `T-18` subtipo "retorno sin vehículo".** El odómetro se declara **estimado** y se marca como tal; el expediente de incidente de M-12 es obligatorio y se vincula. La bitácora se cierra en el punto del accidente.

**7. Los pasajeros se mueven con una misión nueva vinculada.** No se edita el manifiesto cerrado de la original: se registra la **novedad** en ella (`RN-53`) y se abre una segunda Orden de Misión con vínculo explícito a la primera. Si esa segunda misión sale en régimen de emergencia, aplica `CE-01`.

**8. Documentación y seguro se congelan como estaban en la fecha del hecho.** Si la póliza estaba vencida, el expediente debe mostrar que **lo estaba al momento del accidente**, no al momento de la consulta (`RN-40`, `RN-41`). Es un dato que la institución va a necesitar, le convenga o no. Si hay póliza vigente, el sistema dispara el recordatorio de **aviso al asegurador dentro del plazo del contrato**, que es un parámetro con vigencia, nunca un número fijo (`RN-39`).

**9. La misión no cierra con el incidente abierto.** Criterio `H-06`. Si el incidente no se resuelve, el cierre es `T-22` con hallazgo, y el hallazgo **no imputa responsabilidad a nadie**: es una marca de seguimiento.

### Reglas candidatas

| Candidata | Enunciado |
|---|---|
| `RN-c:guia-de-actuacion-en-accidente-precede-al-registro` | Ante la declaración de accidente, el cliente de campo muestra la guía de actuación antes de cualquier formulario, y el registro mínimo se puede diferir sin perderse |
| `RN-c:sin-campo-de-valoracion-de-culpa` | El sistema no ofrece a quien registra en campo ningún campo de atribución de responsabilidad. La responsabilidad se determina en el expediente de investigación, por quien corresponde |
| `RN-c:datos-minimos-de-terceros-en-siniestro` | De un tercero involucrado se capturan solo los datos del catálogo autorizado, sin diagnóstico médico, y toda consulta posterior queda registrada |
| `RN-c:aviso-al-asegurador-en-plazo-parametrizado` | Si el vehículo tiene póliza vigente a la fecha del hecho, el sistema dispara el aviso al asegurador dentro del plazo contractual, tratado como parámetro con vigencia |
| `RN-c:bien-del-estado-retenido-por-autoridad` | Un vehículo retenido o decomisado por autoridad conserva registro de ubicación, autoridad custodia, número de expediente y gestiones de recuperación, hasta su devolución o su descargo |

## Escalamiento al PO

`[C]` **¿Un incidente con lesionados impide cerrar la misión, o solo la marca?** La máquina de estados deja abierto qué expedientes vinculados condicionan `T-21` (pendiente 9). Opciones y costo:

| Opción | Costo |
|---|---|
| Todo incidente abierto impide cerrar | Misiones abiertas durante años por procesos judiciales lentos; la liquidación económica queda rehén de algo que no es económico |
| Solo impide cerrar el incidente con pérdida del bien o con efecto económico no cuantificado | Más operativo, pero exige tipificar bien la severidad — se cruza con el insumo #35 |
| Cierra siempre con hallazgo `H-06` y el incidente sigue su vida propia | Es lo que la máquina de estados ya permite y lo más consistente con `§7.4`; el riesgo es que "cerrada con hallazgo" se vuelva rutina |

**Recomendación del análisis**, no decisión: la tercera. El expediente de hallazgo tiene su propio ciclo y la misión no debe quedar viva por algo que no depende de ella.

## Evidencia que debe quedar

1. Hora del hecho, ubicación, odómetro y fotografías tomadas en el sitio, con su marca de captura
2. Número de parte policial y autoridad interviniente, cuando exista
3. Manifiesto de la misión con las personas que iban a bordo, y la novedad registrada sobre su traslado posterior
4. Estado de la documentación del vehículo **a la fecha del hecho**: matrícula, póliza, revisión
5. Ubicación del vehículo retenido, autoridad custodia y gestiones de recuperación
6. Expediente de incidente de M-12 con su responsable, su plazo y su resultado
7. Registro de quién consultó los datos del tercero y cuándo
8. Liquidación de la misión por lo efectivamente ejecutado, con el odómetro estimado marcado como tal

## Trazabilidad

- **Reglas**: `RN-04` nada se borra · `RN-16` seguro y revisión · `RN-19` vehículo no operativo · `RN-22` custodia · `RN-31` odómetro · `RN-39`, `RN-40`, `RN-41` parámetros y congelamiento · `RN-43` captura sin conectividad · `RN-46` fecha del hecho · `RN-52` registro de consultas · `RN-53` manifiesto cerrado
- **Reglas candidatas**: las cinco de la sección anterior
- **Transiciones**: `T-18` subtipo retorno sin vehículo · `W-08` del estado operativo del vehículo · `T-22` si el incidente no se resuelve
- **Criterios de hallazgo**: `H-06`
- **Puntos de control**: `PC-11`, `PC-12`, `PC-16`
- **Normativa**: `NRM-02` bienes del Estado · `NRM-06` tránsito y licencias · `NRM-07` datos personales · `NRM-01` control interno
- **Actores**: `ACT-06` registra · `ACT-04` coordina · `ACT-11` recibe el vehículo · `ACT-12` es notificado · `ACT-08` cierra · `ACT-14` responde por el bien
- **Casos especiales relacionados**: `CE-02` avería · `CE-04` robo · `CE-07` retorno anticipado · `CE-10` motorista incapacitado
- **Insumo nuevo**: #45 — protocolo institucional de actuación en accidente y exigencia de prueba de alcoholemia
- **Historias candidatas**: `HU-c:declarar-accidente-desde-el-campo`, `HU-c:registrar-tercero-involucrado-con-datos-minimos`, `HU-c:seguir-un-vehiculo-retenido-por-autoridad`
