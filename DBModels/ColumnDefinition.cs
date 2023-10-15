using System;

namespace DynamicDB2.DBModels;

using DBUtility;

internal struct ColumnDefinition
{
    internal string Name { get; }
    internal string Table { get; }
    internal bool Verified { get; }
    internal ColumnType Type { get; }


    internal ColumnDefinition(string line)
    {
        Verified = false;
        Name = string.Empty;
        Table = string.Empty;
        Type = ColumnType.Unknown;

        if (line.StartsWith("COLUMNS") || string.IsNullOrWhiteSpace(line))
            return;

        if (!line.Contains(" "))
            throw new ArgumentException($"Column line contains no spaces.");

        var typeString = line.Substring(0, line.IndexOfAny(new[] { ' ', '<' }));
        Type = typeString.GetColumnType();
        if (Type == ColumnType.Unknown)
            throw new Exception($"Line type is {Type}.");

        if (line.StartsWith($"{typeString}<"))
        {
            var foreignKeys = line.Substring(
                    line.IndexOf('<') + 1,
                    line.IndexOf('>') - line.IndexOf('<') - 1)
                .Split(new[] { "::" }, StringSplitOptions.None);

            Table = foreignKeys[0];
        }

        if (line.LastIndexOf(' ') == line.IndexOf(' '))
            Name = line.Substring(line.IndexOf(' ') + 1);
        else
        {
            var start = line.IndexOf(' ');
            var end = line.IndexOf(' ', start + 1) - start - 1;
            Name = line.Substring(start + 1, end);
        }

        Verified = !Name.EndsWith("?");
        Name = Name.Replace("?", "");
    }

    internal bool IsValid
        => !string.IsNullOrEmpty(Name) && Type != ColumnType.Unknown;
}