namespace Sigti.Pruebas.Datos;

/// <summary>
/// Una sola base compartida por todas las pruebas de integración.
///
/// Con IClassFixture cada clase recibía su propia instancia, y dos clases creando y
/// borrando la <b>misma</b> base competían: una la borraba mientras la otra la usaba.
/// ICollectionFixture la construye una vez y la comparte, y de paso xunit deja de
/// correr esas clases en paralelo entre sí.
/// </summary>
[CollectionDefinition(Nombre)]
public sealed class ColeccionDeBaseDeDatos : ICollectionFixture<BaseDePruebas>
{
    public const string Nombre = "Base de datos SQL Server";
}
