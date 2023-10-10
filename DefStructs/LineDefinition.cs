using DynamicDB2.Useful;
using System;
using System.Collections.Generic;

namespace DynamicDB2.DefStructs;

internal struct LineDefinition
{
    public int Size;
    public int Length;
    public bool Signed;
    public string Name;
    public ColumnType Type;

    public bool IsArray => Length > 0;
    public string TypeString => $"{GetTypeFromSize()}";
    public bool HasValidSize => Size > 0 && Size <= 64;

    public LineDefinition(string line)
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

            ParseLine(line);
        }
        catch (Exception e)
        {
            // Handle the exception appropriately, e.g., log or rethrow
            Console.WriteLine(e);
        }
    }

    private void ParseLine(string line)
    {
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

        // Remove comment line if it exists
        if (line.Contains("//")) line = line.Remove(line.IndexOf("//")).Trim();

        // What's left of the line should be the field's name!
        Name = line;
    }

    public bool Validate(Dictionary<string, ColumnDefinition> columns)
    {
        try
        {
            if (columns is { Count: > 0 } && columns.ContainsKey(Name))
                Type = columns[Name].Type;
        }
        catch (Exception e)
        {
            // Handle the exception appropriately, e.g., log or rethrow
            Console.WriteLine(e);
        }
        return Type != ColumnType.Unknown;
    }

    public string GetTypeFromSize()
    {
        return Size switch
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
    }

    public override string ToString()
    {
        var type = GetTypeFromSize();
        if (string.IsNullOrEmpty(type)) return string.Empty;

        var isArray = IsArray;
        return $"public {GetTypeFromSize()}{(isArray ? "[]" : "")} {Name};";
    }
}