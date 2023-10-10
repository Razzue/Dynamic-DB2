using System;
using System.Collections.Generic;

namespace DynamicDB2.DefStructs;
using DefHelpers;

internal struct LineDefinition
{
    internal int Size;
    internal int Length;
    internal bool Signed;
    internal string Name;
    internal ColumnType Type;
    internal bool IsArray
        => Length > 0;
    internal string TypeString
        => $"{GetTypeFromSize()}";
    internal bool HasValidSize
        => Size is > 0 and <= 64;

    internal LineDefinition(string line)
    {
        Size = 0;
        Length = 0;
        Signed = true;
        Name = string.Empty;
        Type = ColumnType.Unknown;

        try
        {
            if (string.IsNullOrWhiteSpace(line)) return;
            if (line.StartsWith("COMMENT")) return;
            if (line.StartsWith("LAYOUT")) return;
            if (line.StartsWith("BUILD")) return;

            // Ignore annotations for now
            if (line.Contains("$"))
            {
                var annotationStart = line.IndexOf("$");
                var annotationEnd = line.IndexOf("$", 1);

                // Remove annotations from line.
                line = line.Remove(annotationStart, annotationEnd + 1);
            }

            // Handle field size and signed status parsing.
            if (line.Contains("<"))
            {
                var size = line.Substring(line.IndexOf('<') + 1, line.IndexOf('>') - line.IndexOf('<') - 1);
                int.TryParse(size[0] == 'u' ? size.Replace("u", "") : size, out Size);

                // u = Unsigned, lack of = Signed
                Signed = size[0] != 'u';

                // Remove size field from line
                line = line.Remove(line.IndexOf('<'), line.IndexOf('>') - line.IndexOf('<') + 1);
            }

            // Handle field arrays
            if (line.Contains("["))
            {
                int.TryParse(line.Substring(line.IndexOf('[') + 1, line.IndexOf(']') - line.IndexOf('[') - 1), out Length);

                // Remove array value
                line = line.Remove(line.IndexOf('['), line.IndexOf(']') - line.IndexOf('[') + 1);
            }

            // remove comment line if it exists
            if (line.Contains("//")) line = line.Remove(line.IndexOf("//")).Trim();

            // What's left of the line should be the fields name!
            Name = line;
        }
        catch (Exception e) { Console.WriteLine(e); }
    }
    internal bool Validate(Dictionary<string, ColumnDefinition> columns)
    {
        try
        {
            if (columns is { Count: > 0 } && columns.ContainsKey(Name))
                Type = columns[Name].Type;
        }
        catch (Exception e) { Console.WriteLine(e); }
        return Type != ColumnType.Unknown;
    }

    internal string GetTypeFromSize()
        => Size switch
        {
            64 => Signed ? "long" : "ulong",
            32 => Signed ? "int" : "uint",
            16 => Signed ? "short" : "ushort",
            08 => Signed ? "sbyte" : "byte",
            _ => Type switch
            {
                ColumnType.String => "string",
                ColumnType.Single => "float",
                _ => string.Empty
            }
        };
    public override string ToString()
    {
        var result = string.Empty;
        try
        {
            var type = GetTypeFromSize();
            if (string.IsNullOrEmpty(type)) return result;

            var isArray = IsArray;
            result = $"public {GetTypeFromSize()}{(isArray ? "[]" : "")} {Name};";
        }
        catch (Exception e) { Console.WriteLine(e); }
        return result;
    }
}