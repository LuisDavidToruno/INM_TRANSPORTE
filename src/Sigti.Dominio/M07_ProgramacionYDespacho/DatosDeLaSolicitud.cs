using Sigti.Dominio.M03_Flota;

namespace Sigti.Dominio.M07_ProgramacionYDespacho;

/// <summary>
/// Qué se pidió movilizar, y cuándo.
///
/// <b>El sistema no gestiona «viajes de personas»: gestiona movilizaciones de
/// recursos institucionales.</b> Lo trasladado puede ser personal, personas
/// externas, carga o una combinación — por eso el campo se llama <i>objeto del
/// traslado</i> y no «pasajeros».
///
/// Sin estos datos el expediente sería una máquina de estados sin nada que
/// autorizar, y `BD-09` no tendría contra qué verificar la compatibilidad entre lo
/// solicitado y el tipo de vehículo.
/// </summary>
/// <param name="Ventana">
/// La misma que evalúan `BD-02` y `BD-03`. Vive en la solicitud y no en la
/// asignación porque la declara quien pide, no quien programa: si la pusiera quien
/// programa, podría acortarla para que una licencia alcance.
/// </param>
public sealed record DatosDeLaSolicitud(
    string Dependencia,
    string ObjetoDelTraslado,
    string Destino,
    VentanaDeMision Ventana);
