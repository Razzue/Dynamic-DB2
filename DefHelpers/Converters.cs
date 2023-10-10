using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using DynamicDB2.Useful;
using DynamicDB2.DefStructs;

namespace DynamicDB2.DefHelpers;

internal struct DBConverter
{
    internal string Name;
    internal LayoutDefinition[] Layouts;
    internal readonly bool Validated;
    internal readonly WowBuild Build;
    internal Dictionary<string, ColumnDefinition> Columns;

    internal DBConverter(WowDatabase def, WowBuild build)
    {
        Name = string.Empty;
        Layouts = Array.Empty<LayoutDefinition>();
        Columns = new Dictionary<string, ColumnDefinition>();

        try
        {
            Name = $"{def}";
            if (def is WowDatabase.Unknown)
            {
                throw new ArgumentException("Could not determine def type or stream.");
            }

            var lines = File.ReadAllLines($"{Storages.Definitions}\\{Name}.dbd");
            if (string.IsNullOrEmpty(Name) || lines.Length <= 0)
            {
                throw new ArgumentException($"Could not load lines from {Name}.dbd.");
            }

            var lineNumber = 1;
            if (lines[0].StartsWith("COLUMNS"))
            {
                var line = lines[lineNumber];
                while (!string.IsNullOrWhiteSpace(line))
                {
                    var column = new ColumnDefinition(line);
                    if (!column.IsValid)
                    {
                        break;
                    }

                    if (!Columns.ContainsKey(column.Name))
                    {
                        Columns.Add(column.Name, column);
                    }
                    lineNumber++;
                }
            }

            lineNumber++;
            var layouts = new List<LayoutDefinition>();
            while (lineNumber < lines.Length)
            {
                var layout = new LayoutDefinition(lineNumber, lines);
                if (!layout.IsValid)
                {
                    lineNumber++;
                }
                else
                {
                    lineNumber = layout.End + 1;
                    layouts.Add(layout);
                }
            }
            Layouts = layouts.ToArray();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        Validated = true;
        for (var i = 0; i < Layouts.Length; i++)
        {
            if (Validated && !Layouts[i].Validate(Columns))
            {
                Console.WriteLine($"Could not validate layout {Layouts[i].Hashes.FirstOrDefault()}");
                Validated = false;
            }
        }

        Build = build;
    }

    internal bool IsValid => Validated;

    internal LayoutDefinition? Layout
    {
        get
        {
            try
            {
                if (Validated && Build != null && Layouts.Length > 0)
                {
                    for (var i = 0; i < Layouts.Length; i++)
                    {
                        if (Layouts[i].ContainsBuild(Build))
                        {
                            return Layouts[i];
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return null;
        }
    }

    public override string ToString()
    {
        var result = string.Empty;
        try
        {
            var layout = Layout;
            if (!Validated || layout == null)
            {
                return result;
            }

            result = $@"
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using wShadow.Casc.Useful;
using wShadow.Casc.Storage;
using wShadow.Casc.Bindings;

public class C_{Name}
{{
{layout}
}}

public class {Name} : DBDefinition // {layout.Value.Hashes.FirstOrDefault()}
{{
    public override bool Update()
    {{
        try {{ }}
        catch (Exception e) {{ Console.WriteLine(e); }}
        return IsUpdated;
    }}

    public override object Getter(int id, string property)
    {{
        try
        {{
            if (!Db2Data.ContainsKey(id) && !Update()) {{ return null; }}
            if (!Db2Data.ContainsKey(id)) {{ return null; }}
            var definition = (C_{Name})Db2Data[id];

            return (from prop in definition.GetType().GetFields()
                    where prop.Name.Contains(property)
                    select prop.GetValue(definition))
                    .FirstOrDefault();
        }}
        catch (Exception e) {{ Console.WriteLine(e); }}
        return null;
    }}

    public override void Setter(int id, string property, object value, bool classic)
    {{
        try
        {{
            if (!Db2Data.ContainsKey(id) && !Update(classic)) {{ return; }}

            var definition = (C_{Name})Db2Data[id];
            foreach (var prop in definition.GetType().GetFields())
            {{
                if (prop.Name.Contains(property))
                    prop.SetValue(definition, value);
            }}
        }}
        catch (Exception e) {{ Console.WriteLine(e); }}
    }}
}}";
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        return result;
    }
}