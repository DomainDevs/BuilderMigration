using System.Text.RegularExpressions;

namespace DataToolkit.MigrationBuilder.Infrastructure.Migration;

public static class SqlScriptValidator
{
    public static void Validate(
        string script)
    {
        ArgumentNullException.ThrowIfNull(script);

        ValidateDoubleQuotedStrings(script);
    }

    private static void ValidateDoubleQuotedStrings(
        string script)
    {
        var matches = Regex.Matches(
            script,
            "\"[^\"]+\"");

        foreach (Match match in matches)
        {
            throw new InvalidOperationException(
$"""
Se detectó un posible literal de texto entre comillas dobles.

Valor encontrado:
    {match.Value}

SQL Server utiliza comillas simples para representar
literales de texto.

Incorrecto:
    "PRUEBAS"

Correcto:
    'PRUEBAS'

Revise el archivo SQL y vuelva a ejecutar la migración.
""");
        }
    }
}