using System;
using System.Collections.Generic;

namespace DynamicDB2.DBModels;

using DBUtility;

internal struct LineDefinition
{
    private readonly int _size;
    private readonly int _length;
    private readonly bool _signed;
    private ColumnType _type;


    internal string Name { get; }

    internal bool IsArray
        => _length > 0;
    internal string TypeString
        => $"{GetTypeFromSize()}";
    internal bool HasValidSize
        => _size is > 0 and <= 64;
    
    internal LineDefinition(string line)
    {
        _size = 0;
        _length = 0;
        _signed = true;
        Name = string.Empty;
        _type = ColumnType.Unknown;

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
                int.TryParse(size[0] == 'u' ? size.Replace("u", "") : size, out _size);

                // u = Unsigned, lack of = Signed
                _signed = size[0] != 'u';

                // Remove size field from line
                line = line.Remove(line.IndexOf('<'), line.IndexOf('>') - line.IndexOf('<') + 1);
            }

            // Handle field arrays
            if (line.Contains("["))
            {
                int.TryParse(line.Substring(line.IndexOf('[') + 1, line.IndexOf(']') - line.IndexOf('[') - 1), out _length);

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
                _type = columns[Name].Type;
        }
        catch (Exception e) { Console.WriteLine(e); }
        return _type != ColumnType.Unknown;
    }

    internal string GetTypeFromSize()
        => _size switch
        {
            64 => _signed ? "long" : "ulong",
            32 => _signed ? "int" : "uint",
            16 => _signed ? "short" : "ushort",
            08 => _signed ? "sbyte" : "byte",
            _ => _type switch
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