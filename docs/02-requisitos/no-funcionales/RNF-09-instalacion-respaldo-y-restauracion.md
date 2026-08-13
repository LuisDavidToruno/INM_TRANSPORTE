# RNF-09 — Instalar, respaldar y restaurar lo hace alguien sin especialización, siguiendo un documento

| Campo | Valor |
|---|---|
| **Categoría** | Operabilidad |
| **Prioridad** | Crítico |
| **Origen** | [NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md): despliegue on-premise asumiendo que **no habrá equipo de TI dedicado en las delegaciones** |
| **Afecta arquitectura** | **Sí — determinante para la selección de stack (Sprint 2).** Descarta directamente stacks cuya operación exige un especialista |

## Enunciado

La instalación, el respaldo y la **restauración probada** del sistema **deben** poder ejecutarlos una persona con conocimientos generales de informática —no un administrador de bases de datos, no un ingeniero de sistemas— siguiendo un documento paso a paso, sin llamar a nadie.

Esto no es una meta de calidad: es un filtro de elegibilidad. Cualquier arquitectura que requiera afinar un motor de datos, editar archivos de configuración crípticos o interpretar registros técnicos para operar **no es elegible**, por buena que sea en cualquier otro aspecto.

Y el respaldo no vale nada sin la restauración. Un respaldo que nunca se restauró no es un respaldo: es un archivo del que se supone algo.

## Métrica y umbral

| Métrica | Umbral |
|---|---|
| Tiempo de instalación limpia sobre servidor recién preparado, por persona no especializada | ≤ 2 h `[C]` insumo #73 |
| Pasos manuales del procedimiento de instalación | ≤ 15, cada uno con su resultado esperado descrito |
| Decisiones técnicas que el instalador debe tomar por su cuenta | **0.** Todo valor a decidir viene con su valor por defecto y su explicación |
| Comandos que el instalador debe escribir a mano | ≤ 5, o ninguno si el instalador es guiado |
| Ejecución del respaldo | **Automática y diaria**, sin intervención humana `[C]` insumo #72 para la ventana |
| Verificación automática de integridad del respaldo | En cada corrida. Un respaldo que no verifica genera alerta, no un registro que nadie lee |
| Respaldos que salen del servidor a un segundo medio | 100 %. Un respaldo en el mismo disco no protege del modo de falla más común |
| Antigüedad del último respaldo verificado antes de alertar en pantalla | > 48 h ([`RNF-20`](RNF-20-observabilidad-y-diagnostico.md)) |
| Tiempo de restauración completa por persona no especializada | ≤ 4 h |
| Pérdida máxima de datos tras restaurar el último respaldo (RPO) | ≤ 24 h para el servidor; **0 para lo capturado en campo**, que se recupera de los dispositivos ([`RNF-03`](RNF-03-operacion-sin-conectividad.md)) |
| Periodicidad del **simulacro de restauración** | Trimestral, con acta firmada: quién lo ejecutó, cuánto tardó, qué salió mal |
| Simulacros con resultado "no se pudo" que no generen corrección del documento | **0** |
| Actualización de versión ejecutable por la misma persona | ≤ 1 h, con **procedimiento de retroceso documentado y probado** |
| Requisitos de hardware del servidor de referencia | Modestos y explícitos `[C]` insumo #73 — se fijan al cerrar `JDR-1`, y el sistema debe correr en ellos, no en un servidor ideal |

## Cómo se verifica

1. **Prueba de la persona ajena** — es la única verificación que vale para este requisito:
   - Se elige a alguien que **no participó en el desarrollo** y que no es especialista en infraestructura.
   - Se le entrega el documento y un servidor limpio. Nadie lo acompaña ni contesta preguntas.
   - Se cronometra la instalación completa.
   - **Cada pregunta que esa persona necesite hacer y no esté respondida en el documento es un defecto del documento**, y se corrige antes de dar por cumplido el requisito.
2. **Simulacro de desastre**: se apaga el servidor, se borra el volumen de datos, y la misma persona restaura desde el respaldo del día anterior siguiendo el documento. Se verifica que el sistema queda operativo, que la cadena de auditoría del [`RNF-04`](RNF-04-bitacora-append-only-con-hash-encadenado.md) verifica correctamente tras la restauración, y que los dispositivos de campo sincronizan sin duplicar lo que ya habían subido.
3. **Prueba del respaldo corrupto**: se corrompe deliberadamente un respaldo y se verifica que la verificación automática lo detecta **antes** de que alguien lo necesite.
4. **Prueba de retroceso de versión**: se actualiza a la versión siguiente y se retrocede. Se verifica que no se perdió ningún dato capturado entre ambos momentos.
5. **Prueba de hardware de referencia**: la batería de [`RNF-01`](RNF-01-rendimiento-de-consulta-y-operacion.md) se ejecuta sobre el hardware mínimo declarado, no sobre el equipo de desarrollo.

## Consecuencia de no cumplirlo

El sistema se instala una vez, con el equipo de desarrollo presente, y a partir de ahí nadie se atreve a tocarlo. No se actualiza, no se verifica el respaldo, y el día del incidente —disco lleno, corte de energía, servidor que no arranca— la institución descubre que el respaldo no servía. Se pierde la operación y, con ella, el acervo de auditoría completo.

Ese día es el final del proyecto, sin importar la calidad de todo lo demás. Y ocurre en una delegación donde no hay nadie a quien llamar.

## Trazabilidad

- Módulos: transversal
- Normativa: [NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md), [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md)
- Requisitos relacionados: [`RNF-10`](RNF-10-disponibilidad-y-recuperacion.md), [`RNF-19`](RNF-19-configurabilidad-multi-institucion.md), [`RNF-20`](RNF-20-observabilidad-y-diagnostico.md), [`RNF-13`](RNF-13-cifrado-en-transito-y-en-reposo.md)
- Insumos: #72 (ventana de mantenimiento y tolerancia de indisponibilidad), #73 (quién opera el servidor en producción y con qué perfil)
- Decide: `ADR` de stack (Sprint 2), `ADR` de estrategia de despliegue
