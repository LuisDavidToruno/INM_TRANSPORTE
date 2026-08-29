using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.M09_Combustible;
using Sigti.Dominio.M14_Auditoria;
using Sigti.Dominio.Organizacion;

namespace Sigti.Aplicacion.M14_Auditoria;

/// <summary>
/// Arma la cadena que `ACT-12` revisa — <b>`PT-089`</b>.
///
/// ── Dónde vive cada eslabón, y por qué esto existe ──────────────────────────
/// La ficha de `ACT-12` los enumera en una línea —<i>«solicitud → autorización → orden de misión
/// → bitácora → vale → comprobante → liquidación»</i>— pero viven en cuatro módulos: los cuatro
/// primeros en el diario de la Orden de Misión, y los tres siguientes en `M-09`. Esta clase es
/// el único lugar que los junta, y por eso está en la capa de aplicación y no en el dominio.
///
/// ── Lo que decide, y no es cosmético ────────────────────────────────────────
/// <b>Si cada eslabón correspondía.</b> Es el dato que separa el hueco de lo que no aplicaba, y
/// equivocarlo produce los dos daños: alarma falsa sobre una misión sin combustible, o silencio
/// sobre una bitácora que sí faltaba.
/// </summary>
public static class ArmadoDeLaCadena
{
    /// <summary>
    /// La cadena de un expediente.
    /// </summary>
    /// <param name="vales">
    /// Los vales de la misión. <b>Vacío significa que no llevó fondo</b> —y entonces el vale y el
    /// comprobante no aplican—, no que falten.
    /// </param>
    public static CadenaDelExpediente De(
        OrdenDeMision expediente,
        string folio,
        IReadOnlyList<AsignacionDeCombustible> vales)
    {
        var diario = expediente.Diario;
        var estado = expediente.Estado;

        // Hasta dónde llegó el expediente. **Es lo que separa «falta» de «todavía no toca»**: una
        // misión programada sin liquidación no es un hallazgo.
        var llegoADespacho = diario.Any(t => t.Id is "T-12");

        // **Dos caminos a RETORNADA**: `T-18` es el retorno normal y `T-16` es el retorno sin
        // vehículo —el bien queda resguardado en sitio—. Mirar sólo uno haría que el segundo
        // pareciera una misión que nunca volvió.
        var llegoARetorno = diario.Any(t => t.Id is "T-18" or "T-16");

        var llegoALiquidar = estado is EstadoDeMision.Liquidada or EstadoDeMision.Cerrada
            or EstadoDeMision.CerradaConHallazgo;

        // **Sin vales, el vale y el comprobante NO aplican.** Una misión sin combustible asignado
        // no tiene por qué tenerlos, y marcarlos ausentes llenaría la pista de alarmas falsas.
        var llevoFondo = vales.Count > 0;

        var entregado = vales.FirstOrDefault(v => v.QuienHizo("V-02") is not null);
        var conConsumo = vales.FirstOrDefault(v => v.TuvoConsumo);

        return new CadenaDelExpediente(expediente.Id.ToString(), folio,
        [
            // `T-02` — el envío a autorización es el acto de solicitar. **No `T-01`**: la captura
            // del borrador puede hacerla un asistente.
            Eslabon(diario, "T-02", Dominio.M14_Auditoria.Eslabon.Solicitud, folio,
                corresponde: true, alcanzado: true),

            Eslabon(diario, "T-05", Dominio.M14_Auditoria.Eslabon.Autorizacion, folio,
                corresponde: true, alcanzado: diario.Any(t => t.Id is "T-02")),

            // Programar es emitir la Orden de Misión.
            Eslabon(diario, "T-08", Dominio.M14_Auditoria.Eslabon.OrdenDeMision, folio,
                corresponde: true, alcanzado: diario.Any(t => t.Id is "T-05")),

            // La bitácora se abre en `T-14` al iniciar la ruta, no al despachar.
            //
            // ⚠️ **Se juzga contra el RETORNO y no contra el despacho.** Una misión despachada
            // que todavía no salió del predio no tiene bitácora y eso no es un hueco: es que no
            // le toca. Medido en vivo, juzgarlo contra el despacho marcaba como hallazgo a toda
            // misión en ese estado — y un falso positivo en una pista de auditoría es lo que
            // hace que se deje de mirar.
            Eslabon(diario, "T-14", Dominio.M14_Auditoria.Eslabon.Bitacora, folio,
                corresponde: true, alcanzado: llegoARetorno),

            ReglasDeLaCadena.Resolver(
                Dominio.M14_Auditoria.Eslabon.Vale,
                corresponde: llevoFondo,
                alcanzado: llegoADespacho,
                referencia: entregado?.Folio,
                quien: entregado?.QuienHizo("V-02"),
                fecha: null,
                porQueNoCorresponde: "La misión no llevó fondo de combustible asignado."),

            ReglasDeLaCadena.Resolver(
                Dominio.M14_Auditoria.Eslabon.Comprobante,
                corresponde: llevoFondo,
                alcanzado: llegoARetorno,
                referencia: conConsumo?.Folio,
                quien: null,
                fecha: null,
                porQueNoCorresponde: "Sin fondo asignado no hay comprobante que rendir."),

            // ⚠️ **Liquidar es `T-19`, no `T-20`.** Medido en vivo: una misión LIQUIDADA salía
            // con la liquidación marcada como hueco, porque se buscaba una transición que no
            // existe. El estado decía una cosa y la cadena la contradecía.
            Eslabon(diario, "T-19", Dominio.M14_Auditoria.Eslabon.Liquidacion, folio,
                corresponde: true, alcanzado: llegoALiquidar || llegoARetorno),
        ]);
    }

    /// <summary>
    /// Un eslabón que sale del diario de la misión.
    ///
    /// <b>Se busca la transición, no el estado.</b> `P-1`: el estado es la proyección del
    /// diario, y preguntarle al estado si hubo autorización no diría quién la firmó ni cuándo.
    /// </summary>
    private static EslabonResuelto Eslabon(
        IReadOnlyList<Transicion> diario,
        string transicion,
        Eslabon eslabon,
        string folio,
        bool corresponde,
        bool alcanzado)
    {
        var asiento = diario.FirstOrDefault(t => t.Id == transicion);

        return ReglasDeLaCadena.Resolver(
            eslabon,
            corresponde,
            alcanzado,
            asiento is null ? null : $"{transicion} de {folio}",
            asiento?.Ejecuta,
            asiento is null ? null : DateOnly.FromDateTime(asiento.Momento.Date));
    }
}
