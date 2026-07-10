namespace DataToolkit.MigrationBuilder.Helpers;

public static class MigrationWarningExtensions
{
    public static string GetMessage(
        this MigrationWarning warning)
    {
        return warning switch
        {
            MigrationWarning.DataTypeMismatch =>
                "Verify source and target data types.",

            MigrationWarning.LengthMismatch =>
                "Verify source and target column lengths.",

            MigrationWarning.PrecisionMismatch =>
                "Verify source and target precision.",

            MigrationWarning.ScaleMismatch =>
                "Verify source and target scale.",

            MigrationWarning.NullableMismatch =>
                "Verify source and target nullability.",

            MigrationWarning.MissingTargetColumn =>
                "Column not found in target table.",

            MigrationWarning.MissingSourceColumn =>
                "Column not found in source table.",

            _ => string.Empty
        };
    }
}