# RNF-17 — Los datos personales se depuran en su plazo sin romper la cadena de auditoría del expediente contable

| Campo | Valor |
|---|---|
| **Categoría** | Seguridad / Auditoría |
| **Prioridad** | Alto |
| **Origen** | [NRM-07](../../01-negocio/normativa/NRM-07-transparencia-y-datos-personales.md) (retención diferenciada y hábeas data) frente a [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md) (conservación por el plazo de prescripción, sin borrado físico) |
| **Afecta arquitectura** | **Sí — determinante para la selección de stack y para el modelo de datos (Sprint 2).** Es la restricción que obliga a separar el dato personal del asiento que lo referencia |

## Enunciado

Dos normas del mismo Estado piden cosas opuestas, y el diseño tiene que satisfacer las dos:

| Norma | Exige |
|---|---|
| [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md) | Conservar todo por el plazo de prescripción. **Nada se borra físicamente.** Cadena de auditoría verificable |
| [NRM-07](../../01-negocio/normativa/NRM-07-transparencia-y-datos-personales.md) | Retención **más corta** para datos personales de pasajeros, con depuración o seudonimización; y hábeas data con derecho de rectificación |

La resolución es estructural, no procedimental: **el dato personal se almacena en un segmento separado, y la cadena de auditoría encadena una referencia y una huella, no el contenido en claro.** Así la depuración del dato personal no invalida ningún hash y el expediente contable conserva su integridad.

Y la depuración **no es un borrado**: es un asiento nuevo que declara qué se depuró, cuándo, por qué plazo y con qué autoridad. El registro histórico conserva estructura, conteos y montos; pierde la identidad de la persona.

## Métrica y umbral

| Métrica | Umbral |
|---|---|
| Plazo de conservación de registros financieros, de bienes y de auditoría | Parámetro configurable con vigencia. `[C]` insumo #71 — [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md) exige expresamente que **no se cablee** |
| Plazo de depuración o seudonimización de datos personales de pasajeros externos | Parámetro configurable, **menor** que el anterior. `[C]` insumo #71, con Auditoría Interna y el OIP |
| Registros financieros, de bienes o asientos de auditoría eliminados físicamente | **0** |
| Asientos de auditoría cuyo hash contenga datos personales en claro | **0.** El asiento encadena referencia y huella |
| Rupturas de la cadena del [`RNF-04`](RNF-04-bitacora-append-only-con-hash-encadenado.md) provocadas por una depuración | **0** |
| Depuraciones ejecutadas sin asiento propio con autoridad, plazo aplicado y alcance | **0** |
| Depuraciones automáticas sin aviso previo al responsable | **0.** Se anuncian con antelación y quedan en la pantalla de estado |
| Datos que sobreviven a la depuración de un manifiesto | Conteo de pasajeros, condición agregada, origen, destino, vehículo, misión, costos. **No** identidad, contacto ni documento |
| Tiempo para localizar todos los registros de una persona identificada (hábeas data) | ≤ 5 min desde la interfaz, sin intervención de desarrollo |
| Rectificaciones de hábeas data aplicadas sobre el registro original | **0.** Se registra la rectificación conservando el registro contable original ([NRM-07](../../01-negocio/normativa/NRM-07-transparencia-y-datos-personales.md)) |
| Datos personales que sobrevivan la depuración en respaldos, adjuntos, registros técnicos o dispositivos de campo | **0.** La depuración alcanza los cuatro, y el procedimiento lo documenta |
| Campos de salud, etnia, situación migratoria o condición de vulnerabilidad en M-17 | **0**, salvo base legal expresa documentada ([`RN-51`](../../01-negocio/reglas/RN-51-minimizacion-de-datos-de-personas-externas.md)) |

## Cómo se verifica

1. **Prueba de depuración con cadena intacta** — la verificación que define el requisito:
   - Se cargan manifiestos con pasajeros de prueba y se genera un año de asientos encadenados, con su sello emitido.
   - Se ejecuta la depuración del período correspondiente.
   - Se corre el verificador de la cadena. **Debe verificar sin una sola ruptura**, y el sello emitido antes de la depuración debe seguir siendo válido.
   - Se verifica que los nombres depurados no aparecen ni en la base, ni en los adjuntos, ni en los registros técnicos, ni en el respaldo posterior.
2. **Prueba del reporte que no cambia**: se genera un reporte de M-17 antes de la depuración y se regenera después con la misma fecha de corte. Los conteos, costos y estructura deben ser idénticos ([`RNF-06`](RNF-06-reproducibilidad-historica-de-reportes.md)); solo las identidades deben aparecer seudonimizadas.
3. **Prueba de hábeas data**: se solicita el ejercicio del derecho sobre una persona con registros en 12 misiones. Se cronometra la localización, se exporta, se rectifica un dato, y se verifica que el registro contable original sigue intacto y que la rectificación quedó como asiento.
4. **Prueba de retención diferenciada**: se configuran dos plazos distintos y se avanza el reloj del sistema. Debe depurarse lo personal y conservarse lo contable, no al revés.
5. **Prueba del dispositivo de campo**: se verifica que la depuración alcanza los almacenes locales de los dispositivos al siguiente contacto, y que queda constancia de cuáles ya la aplicaron y cuáles no ([`RNF-13`](RNF-13-cifrado-en-transito-y-en-reposo.md)).
6. **Prueba del plazo no configurado**: con el insumo #71 aún abierto, el sistema **no depura nada** y muestra el pendiente en la pantalla de estado. No se inventa un plazo por defecto.

## Consecuencia de no cumplirlo

- **Si se resuelve borrando**: se rompe la cadena de hash y el acervo de auditoría pierde su verificabilidad completa. Se cumple [NRM-07](../../01-negocio/normativa/NRM-07-transparencia-y-datos-personales.md) destruyendo [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md), que es el peor intercambio posible: el TSC fiscaliza todos los años; el hábeas data llega rara vez.
- **Si no se depura nada**: la institución acumula indefinidamente identidades de personas externas que solo necesitaba para controlar un traslado. Ante un hábeas data no tiene respuesta, y ante una fuga el daño es proporcional a todo lo acumulado desde el día uno.
- **Si se pospone la decisión**: el costo de separar el segmento de datos personales del asiento de auditoría **después** de tener años de cadena construida es rehacer la cadena, y una cadena rehecha no prueba nada. Por eso este requisito es determinante de arquitectura y no un ajuste posterior.

## Trazabilidad

- Módulos: M-17, M-14, M-01
- Reglas: [`RN-51`](../../01-negocio/reglas/RN-51-minimizacion-de-datos-de-personas-externas.md), [`RN-52`](../../01-negocio/reglas/RN-52-registro-de-consultas-a-manifiestos.md), [`RN-53`](../../01-negocio/reglas/RN-53-cierre-del-manifiesto-al-despacho.md), [`RN-05`](../../01-negocio/reglas/RN-05-registro-cerrado-no-se-edita.md)
- Normativa: [NRM-07](../../01-negocio/normativa/NRM-07-transparencia-y-datos-personales.md), [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md)
- Requisitos relacionados: [`RNF-04`](RNF-04-bitacora-append-only-con-hash-encadenado.md), [`RNF-06`](RNF-06-reproducibilidad-historica-de-reportes.md), [`RNF-13`](RNF-13-cifrado-en-transito-y-en-reposo.md), [`RNF-02`](RNF-02-volumen-y-crecimiento-del-acervo.md)
- Insumos: #71 (plazos de conservación y de depuración, con Auditoría Interna y el OIP), #39 (traslados de personas bajo custodia o menores)
