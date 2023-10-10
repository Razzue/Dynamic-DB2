using System;

namespace DynamicDB2.DefStructs;
using DefHelpers;

internal struct ColumnDefinition
{
    internal string Name;
    internal string Table;
    internal bool Verified;
    internal ColumnType Type;

    internal ColumnDefinition(string line)
    {
        Name = null;
        Table = null;
        Verified = false;
        Type = ColumnType.Unknown;

        try
        {
            if (line.StartsWith("COLUMNS") || string.IsNullOrWhiteSpace(line)) return;

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
        catch (Exception e) { Console.WriteLine(e); }
    }
    internal bool IsValid
        => !string.IsNullOrEmpty(Name) && Type != ColumnType.Unknown;
}