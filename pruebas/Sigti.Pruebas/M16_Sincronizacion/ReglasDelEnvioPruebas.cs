using Sigti.Dominio.M16_Sincronizacion;

namespace Sigti.Pruebas.M16_Sincronizacion;

/// <summary>
/// `HU-067` — el resultado registro por registro.
///
/// <i>«Un "sincronizado con éxito" que en realidad significa "de 34 registros, 31 se aplicaron,
/// 1 espera a otro que no llegó y 2 están en conflicto" <b>es una mentira operativa</b>. El día
/// que se descubra, el motorista deja de confiar y vuelve al papel.»</i>
/// </summary>
public class ReglasDelEnvioPruebas
{
    [Theory]
    [InlineData(EstadoDelEnvio.Aceptado, "enviado y aceptado")]
    [InlineData(EstadoDelEnvio.YaEstaba, "ya estaba registrado")]
    [InlineData(EstadoDelEnvio.EsperandoUnAnterior, "esperando un registro anterior que no ha llegado")]
    [InlineData(EstadoDelEnvio.NecesitaQueAlguienDecida, "necesita que alguien decida")]
    public void Cada_estado_se_dice_con_la_frase_que_la_historia_enumera(
        EstadoDelEnvio estado, string frase)
    {
        // Son criterio de aceptación **literal**: la historia las escribe una por una.
        Assert.Equal(frase, ReglasDelEnvio.EnPalabras(estado));
    }

    [Fact]
    public void Ninguna_frase_usa_lenguaje_de_datos()
    {
        // «Y ningún texto de la pantalla contiene "merge", "versión del registro", "timestamp"
        // ni "conflicto de escritura"». Quien lo lee es el motorista.
        string[] prohibidas = ["merge", "timestamp", "versión del registro", "conflicto de escritura"];

        foreach (var estado in Enum.GetValues<EstadoDelEnvio>())
            foreach (var palabra in prohibidas)
                Assert.DoesNotContain(palabra, ReglasDelEnvio.EnPalabras(estado),
                    StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Un_envio_con_algo_en_espera_no_termino_limpio()
    {
        // ⚠️ Es la mentira operativa que la historia nombra: decir «sincronizado» cuando uno de
        // los registros todavía espera a otro que no llegó.
        Assert.False(ReglasDelEnvio.TerminoLimpio([
            EstadoDelEnvio.Aceptado,
            EstadoDelEnvio.Aceptado,
            EstadoDelEnvio.EsperandoUnAnterior,
        ]));
    }

    [Fact]
    public void Un_envio_con_algo_en_conflicto_tampoco()
    {
        Assert.False(ReglasDelEnvio.TerminoLimpio([
            EstadoDelEnvio.Aceptado,
            EstadoDelEnvio.NecesitaQueAlguienDecida,
        ]));
    }

    [Fact]
    public void Lo_que_ya_estaba_no_impide_que_termine_limpio()
    {
        // El reintento del dispositivo es el caso normal, no un problema: se corta la señal bajo
        // un puente y reenvía. Contarlo como pendiente haría que ningún envío terminara nunca.
        Assert.True(ReglasDelEnvio.TerminoLimpio([
            EstadoDelEnvio.Aceptado,
            EstadoDelEnvio.YaEstaba,
        ]));
    }

    [Fact]
    public void Un_envio_vacio_termino_limpio()
    {
        Assert.True(ReglasDelEnvio.TerminoLimpio([]));
    }

    [Theory]
    [InlineData(EstadoDelEnvio.EsperandoUnAnterior)]
    [InlineData(EstadoDelEnvio.NecesitaQueAlguienDecida)]
    [InlineData(EstadoDelEnvio.NoSePudoRegistrar)]
    public void Los_que_siguen_abiertos_piden_algo_de_alguien(EstadoDelEnvio estado)
    {
        Assert.True(ReglasDelEnvio.SigueAbierto(estado));
    }

    [Theory]
    [InlineData(EstadoDelEnvio.Aceptado)]
    [InlineData(EstadoDelEnvio.YaEstaba)]
    public void Los_que_entraron_no_piden_nada(EstadoDelEnvio estado)
    {
        Assert.False(ReglasDelEnvio.SigueAbierto(estado));
    }

    [Fact]
    public void Esperar_y_necesitar_decision_son_estados_distintos()
    {
        // Los dos siguen abiertos y **piden cosas distintas**: el que espera se resuelve solo
        // cuando llegue el que falta; el que necesita decisión espera a una persona y no se va a
        // mover hasta que alguien la tome. Juntarlos haría que alguien intentara «resolver» un
        // hueco de orden, que no tiene nada que decidir.
        Assert.NotEqual(EstadoDelEnvio.EsperandoUnAnterior,
                        EstadoDelEnvio.NecesitaQueAlguienDecida);

        Assert.NotEqual(ReglasDelEnvio.EnPalabras(EstadoDelEnvio.EsperandoUnAnterior),
                        ReglasDelEnvio.EnPalabras(EstadoDelEnvio.NecesitaQueAlguienDecida));
    }
}
