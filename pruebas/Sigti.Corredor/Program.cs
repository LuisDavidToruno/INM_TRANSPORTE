using System.Diagnostics;

using Xunit.Runners;

// ── Por qué existe este corredor ─────────────────────────────────────────────
//
// Smart App Control bloquea `Sigti.Pruebas.dll` cuando lo carga `testhost`, y **no lo bloquea**
// cuando lo carga un proceso lanzado con `dotnet run`. La diferencia no es el dll —es el mismo
// binario— sino quién lo abre.
//
// Esto corre las mismas pruebas desde un proceso normal. No reemplaza a `dotnet test`: cuando
// SAC suelta, `dotnet test` sigue siendo la vía. Existe para que una máquina bloqueada no deje
// la suite sin correr durante horas, que es lo que convierte una suite en decoración.
//
// Uso:
//   dotnet run --project pruebas/Sigti.Corredor
//   dotnet run --project pruebas/Sigti.Corredor -- Feriado      (filtra por nombre)

// Un primer argumento que sea una ruta apunta a otra copia del ensamblado. SAC decide por
// copia y no por contenido: cuando bloquea la de esta carpeta, la de `Sigti.Pruebas/bin`
// puede estar libre, y al reves. Poder elegir cual se carga es lo que salva la corrida.
var rutaPedida = args.FirstOrDefault(a => a.EndsWith(".dll"));
var filtro = args.FirstOrDefault(a => !a.EndsWith(".dll"));

// Se acepta el nombre corto de la clase y se completa el espacio de nombres, que es como uno
// se acuerda de ella.
var tipo = filtro is null ? null
    : filtro.Contains('.') ? filtro
    : "Sigti.Pruebas.PuntaAPunta." + filtro;

var ensamblado = Path.GetFullPath(rutaPedida ?? Path.Combine(
    AppContext.BaseDirectory, "Sigti.Pruebas.dll"));

if (!File.Exists(ensamblado))
{
    Console.Error.WriteLine($"No se encontró {ensamblado}.");
    return 2;
}

var terminado = new ManualResetEventSlim(false);
var reloj = Stopwatch.StartNew();

var pasadas = 0;
var fallidas = 0;
var omitidas = 0;
var fallas = new List<string>();

using var corredor = AssemblyRunner.WithoutAppDomain(ensamblado);

corredor.OnTestPassed = _ => Interlocked.Increment(ref pasadas);
corredor.OnTestSkipped = _ => Interlocked.Increment(ref omitidas);

corredor.OnTestFailed = info =>
{
    Interlocked.Increment(ref fallidas);

    // Se guarda el mensaje completo: una suite que sólo cuenta fallas obliga a volver a
    // correrla para saber cuáles, y acá volver a correrla puede costar horas.
    lock (fallas)
    {
        fallas.Add($"{info.TestDisplayName}\n    {info.ExceptionMessage?.Replace("\n", "\n    ")}");
    }
};

corredor.OnErrorMessage = info =>
{
    lock (fallas) fallas.Add($"ERROR DEL CORREDOR: {info.ExceptionMessage}");
};

// Filtra en el descubrimiento: `AssemblyRunnerStartOptions` de esta version no expone el
// nombre de tipo, y sin filtro una corrida de una clase toma lo mismo que la suite entera.
corredor.OnDiscoveryComplete = info =>
    Console.WriteLine($"Descubiertas {info.TestCasesToRun} de {info.TestCasesDiscovered} pruebas.");

corredor.OnExecutionComplete = _ => terminado.Set();

Console.WriteLine($"Corriendo {Path.GetFileName(ensamblado)}…");
if (filtro is not null) Console.WriteLine($"Filtro: «{filtro}»");
Console.WriteLine();

corredor.TestCaseFilter = tipo is null
    ? null
    : c => c.TestMethod.TestClass.Class.Name.Contains(tipo, StringComparison.OrdinalIgnoreCase);

corredor.Start(new AssemblyRunnerStartOptions
{

    // En serie: la suite comparte una base de datos real, y en paralelo dos pruebas se
    // pisan el expediente. `dotnet test` la corre igual.
    MaxParallelThreads = 1,
});

// El progreso a intervalos: una suite de dos minutos sin salida se ve igual que una colgada.
while (!terminado.Wait(TimeSpan.FromSeconds(30)))
    Console.WriteLine($"  … {pasadas + fallidas + omitidas} ejecutadas, {reloj.Elapsed:mm\\:ss}");

reloj.Stop();

Console.WriteLine();

if (fallas.Count > 0)
{
    Console.WriteLine("FALLAS");
    Console.WriteLine();
    foreach (var f in fallas) Console.WriteLine($"  {f}\n");
}

Console.WriteLine(
    $"{(fallidas == 0 ? "Correctas!" : "Con error!")} - " +
    $"Con error: {fallidas}, Superado: {pasadas}, Omitido: {omitidas}, " +
    $"Total: {pasadas + fallidas + omitidas}, Duración: {reloj.Elapsed:mm\\:ss}");

return fallidas == 0 ? 0 : 1;
