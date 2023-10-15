namespace DynamicDB2.DBUtility;

public enum ColumnType
{
    Unknown,
    Number,
    Single,
    String,
}

internal static class ColumnTypeExtension
{
    internal static ColumnType GetColumnType(this string source)
        => source.ToLower() switch
        {
            "locstring" => ColumnType.String,
            "string" => ColumnType.String,
            "float" => ColumnType.Single,
            "int" => ColumnType.Number,
            _ => ColumnType.Unknown,
        };
}