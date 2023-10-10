using System;

namespace DynamicDB2.DefHelpers;

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
        => source switch
        {
            "locstring" => ColumnType.String,
            "string" => ColumnType.String,
            "float" => ColumnType.Single,
            "int" => ColumnType.Number,
            _ => ColumnType.Unknown,
        };
}