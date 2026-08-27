/**
 * Cómo se compara texto que escribió una persona.
 *
 * ── Por qué existe ───────────────────────────────────────────────────────────
 *
 * Estaba escrita **cinco veces** —la paleta de comandos, los dos selectores buscables, la
 * tabla y el clasificador de tonos—, ninguna exportada. Las cinco hacían lo mismo, así que
 * todavía no habían divergido; ese es justamente el momento de unificarlas, porque el día que
 * una gane una regla (la ñ, los guiones, los números romanos) las otras cuatro se quedan sin
 * ella y **nadie se entera**: una búsqueda que no encuentra algo no falla, devuelve vacío.
 *
 * Es el mismo desenlace que este repositorio ya documentó con las siete copias del limpiador
 * de HTML y con las veinticuatro formas de la nota teñida.
 *
 * ── Qué hace, y qué NO ───────────────────────────────────────────────────────
 *
 * Baja la caja y quita los diacríticos, así «Corrección» encuentra a «correccion» y al revés.
 * **No** quita espacios ni puntuación: son parte de lo que se busca, y recortarlos haría que
 * «tipo de gira» y «tipodegira» se traten igual sin que nadie lo haya pedido.
 */
export function normalizarTexto(texto: string): string {
  return texto
    .toLowerCase()
    .normalize('NFD')
    // El rango de los diacríticos combinantes, que es lo que `NFD` separa de su letra.
    //
    // ⚠️ Va ESCAPADO y no con los caracteres literales —de las cinco copias, sólo una lo hacía
    // así y tenía razón—: escritos literalmente, el archivo depende de que nadie lo vuelva a
    // guardar con otra codificación, y si eso pasa la expresión deja de quitar acentos **sin
    // que nada falle**. Este repositorio ya tiene el precedente con el `\b` que se convirtió en
    // un retroceso al escribir C# desde Python.
    .replace(/[\u0300-\u036f]/g, '');
}
