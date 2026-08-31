using Microsoft.EntityFrameworkCore;

using Sigti.Aplicacion.M03_Flota;
using Sigti.Aplicacion.M07_ProgramacionYDespacho;
using Sigti.Datos;
using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.M07_ProgramacionYDespacho;

namespace Sigti.Aplicacion.M15_Formatos;

/// <summary>
/// `RN-65` — el <b>paquete de identificación en carretera</b> del vehículo sin lámina.
///
/// ── Por qué existe este papel ───────────────────────────────────────────────
/// Un vehículo del Estado sin lámina metálica que un agente detiene <b>no tiene cómo
/// identificarse</b>. La lámina es lo primero que se pide y lo único que normalmente hace
/// falta; sin ella, lo que queda es este paquete o la palabra del motorista.
///
/// Y no es un caso raro: <b>hay desabastecimiento nacional</b>. La flota real circula así.
///
/// ── Se arma, no se guarda ───────────────────────────────────────────────────
/// A diferencia del salvoconducto —que <b>congela</b> lo que ampara porque materializa una
/// firma—, éste no ampara nada: <b>describe</b>. Congelarlo produciría un papel que dice que la
/// rotulación se constató en marzo cuando en junio se volvió a constatar y faltaba la leyenda.
/// Lo que el agente necesita es el estado de hoy.
/// </summary>
public sealed class PaqueteDeIdentificacion(
    SigtiDbContext contexto,
    ServicioDeRespaldoDePlaca respaldos,
    ServicioDeRotulacion rotulacion)
{
    /// <summary>
    /// Arma el paquete de una misión.
    ///
    /// <b>Nulo cuando la misión no tiene vehículo reservado</b>: no hay de qué vehículo hablar,
    /// y eso no es un fallo — es que todavía no se programó.
    /// </summary>
    public async Task<Paquete?> DeLaMisionAsync(
        Ulid mision, CancellationToken cancelacion = default)
    {
        var expediente = await contexto.Expedientes
            .AsNoTracking()
            .Include(e => e.Transiciones)
            .SingleOrDefaultAsync(e => e.Id == mision, cancelacion);

        if (expediente is null) return null;

        var reserva = ServicioDePermisos.Reserva(expediente);
        if (reserva.Vehiculo is not { } idVehiculo) return null;

        var vehiculo = await contexto.Vehiculos
            .AsNoTracking()
            .Include(v => v.RespaldosDePlaca)
            .SingleOrDefaultAsync(v => v.Id == idVehiculo, cancelacion);

        if (vehiculo is null) return null;

        var ventana = new VentanaDeMision(
            expediente.Salida, expediente.Retorno, expediente.HolguraDias,
            expediente.HoraDeSalida, expediente.HoraDeRetorno);

        var motorista = reserva.Motorista is not { } idMotorista
            ? null
            : await contexto.Conductores
                .AsNoTracking()
                .Where(c => c.Id == idMotorista)
                .Select(c => c.Nombre)
                .SingleOrDefaultAsync(cancelacion);

        var historial = await respaldos.HistorialAsync(
            idVehiculo, ventana.Salida, ventana.FinDelRango, cancelacion);

        // El que cubre la ventana. **Nulo es que ninguno la cubre**, y el documento lo dice en
        // vez de mostrar el más reciente como si sirviera.
        var respaldo = historial.FirstOrDefault(r => r.Cubre);

        var identificacion = await rotulacion.EvaluarAsync(
            idVehiculo, ventana.Salida, cancelacion);

        // ⚠️ **El quinto contenido de `RN-65`**: <i>«fotografía vigente del vehículo con su
        // rotulación»</i>. Se toma la de la constatación más reciente — es obligatoria por
        // `RN-18`, así que si hay constatación hay foto.
        //
        // Nula cuando nunca se constató, y el documento lo dice: un paquete sin foto no
        // identifica al vehículo, sólo lo describe.
        var fotografia = (await rotulacion.HistorialAsync(idVehiculo, cancelacion))
            .OrderByDescending(c => c.ConstatadoEl)
            .FirstOrDefault()
            ?.Fotografia;

        return new Paquete(
            // ⚠️ **El correlativo institucional primero.** `RN-15`: la identidad del vehículo
            // del Estado es el correlativo, no la placa — y en este documento eso deja de ser
            // una preferencia de diseño y pasa a ser lo único que hay.
            vehiculo.CorrelativoInstitucional ?? vehiculo.Siglas,
            vehiculo.Siglas,
            vehiculo.Chasis,
            vehiculo.Motor,
            vehiculo.BienDelInventario,

            // Nula cuando no hay número asignado. Distinto de «no tiene lámina»: son los dos
            // datos que `RN-64` separa.
            vehiculo.Placa,
            vehiculo.EstadoDePlaca,

            respaldo?.Respaldo,
            expediente.Dependencia,
            motorista,
            ventana.Salida,
            ventana.FinDelRango,
            expediente.Destino,
            identificacion,
            fotografia);
    }
}

/// <param name="Correlativo">
/// La identidad del vehículo del Estado — `RN-15`. En este documento es <b>lo único que hay</b>:
/// sin lámina, el correlativo es contra lo que el agente compara la calcomanía.
/// </param>
/// <param name="Placa">
/// El número asignado en el registro, si existe. <b>Nulo no es «sin lámina»</b>: son los dos
/// datos que `RN-64` separa, y un vehículo puede tener número y no tener lámina.
/// </param>
/// <param name="Respaldo">
/// El documento que cubre la ventana de la misión. <b>Nulo es que ninguno la cubre</b> — y el
/// papel lo dice, en vez de imprimir el más reciente como si sirviera.
/// </param>
/// <param name="Identificacion">
/// El estado de la rotulación a la fecha de salida — `RN-18`. <b>Nulo es que no se pudo
/// evaluar</b>, que es distinto de «está bien».
/// </param>
/// <param name="Fotografia">
/// La foto de la constatación más reciente — <b>el quinto contenido de `RN-65`</b>.
///
/// <b>Nula cuando nunca se constató.</b> Un paquete sin foto no identifica al vehículo: lo
/// describe, que es menos.
/// </param>
public sealed record Paquete(
    string Correlativo,
    string Siglas,
    string? Chasis,
    string? Motor,
    string? BienDelInventario,
    string? Placa,
    EstadoDePlaca EstadoDePlaca,
    RespaldoDePlaca? Respaldo,
    string Dependencia,
    string? Motorista,
    DateOnly Desde,
    DateOnly Hasta,
    string Destino,
    IdentificacionDelVehiculo? Identificacion,
    Ulid? Fotografia)
{
    /// <summary>
    /// Si el vehículo <b>necesita</b> este paquete. Con lámina puesta, no.
    ///
    /// Se dice para que el documento no se imprima por costumbre: un paquete emitido a un
    /// vehículo que lleva su lámina es papel que nadie va a comparar con nada.
    /// </summary>
    public bool HaceFalta => EstadoDePlaca != EstadoDePlaca.ConLamina;
}
