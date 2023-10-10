using System;
using DynamicDB2.Useful;

namespace DynamicDB2.DefStructs;

internal struct ColumnDefinition
{
    internal bool IsValid
        => !string.IsNullOrEmpty(Name) && Type != ColumnType.Unknown;
    internal string Name { get; private set; }
    internal string Table { get; private set; }
    internal bool Verified { get; private set; }
    internal ColumnType Type { get; private set; }

    internal void ParseLine(string line)
    {
        if (!line.Contains(" "))
            throw new ArgumentException("Column line contains no spaces.");
        var typeString = line.Substring(0, line.IndexOfAny(new[] { ' ', '<' }));

        Type = typeString.GetColumnType();
        if (Type == ColumnType.Unknown)
        {
            throw new InvalidOperationException($"Line type is {Type}.");
        }

        if (line.StartsWith($"{typeString}<"))
        {
            var foreignKeys = line.Substring(
                    line.IndexOf('<') + 1,
                    line.IndexOf('>') - line.IndexOf('<') - 1)
                .Split(new[] { "::" }, StringSplitOptions.None);

            Table = foreignKeys[0];
        }

        if (line.LastIndexOf(' ') == line.IndexOf(' '))
        {
            Name = line.Substring(line.IndexOf(' ') + 1);
        }
        else
        {
            var start = line.IndexOf(' ');
            var end = line.IndexOf(' ', start + 1) - start - 1;
            Name = line.Substring(start + 1, end);
        }

        Verified = !Name.EndsWith("?");
        Name = Name.Replace("?", "");
    }
    internal ColumnDefinition(string line)
    {
        Verified = false;
        Name = string.Empty;
        Table = string.Empty;
        Type = ColumnType.Unknown;

        if (string.IsNullOrWhiteSpace(line) || line.StartsWith("COLUMNS"))
            return;

        ParseLine(line);
    }
}