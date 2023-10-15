using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using DynamicDB2.DBModels;
using DynamicDB2.DBUtility;

namespace DynamicDB2.Helpers;

internal struct DBTemplate
{
    internal string Name { get; }
    internal bool Validated { get; }
    internal WowBuild Build { get; }

    internal LayoutDefinition[] Layouts { get; }
    internal Dictionary<string, ColumnDefinition> Columns { get; }

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

    internal DBTemplate(WowDbd dbd, WowBuild build)
    {
        Build = build;
        Layouts = Array.Empty<LayoutDefinition>();
        Columns = new Dictionary<string, ColumnDefinition>();

        Name = $"{dbd}";
        if (dbd is WowDbd.Maximum or WowDbd.Unknown )
            throw new ArgumentException("Could not determine def type or stream.");
        var lines = File.ReadAllLines($"{Environment.CurrentDirectory}\\Definitions\\{dbd.DbdString()}");

        if (string.IsNullOrEmpty(Name) || lines.Length <= 0)
            throw new ArgumentException($"Could not load lines from {dbd.DbdString()}.");

        Validated = false;
        var lineNumber = 0; 
        if (!lines[lineNumber].StartsWith("COLUMNS"))
            throw new ArgumentException("DBD file does not start with columns.");
        
        lineNumber++;
        var columns = ParseColumns(lines, ref lineNumber);
        foreach (var column in columns)
        {
            if (!Columns.ContainsKey(column.Name))
                Columns.Add(column.Name, column);
        }
        Layouts = ParseLayouts(lines, lineNumber);

        Validated = true;
        for (var i = 0; i < Layouts.Length; i++)
        {
            if (Validated && !Layouts[i].Validate(Columns))
            {
                Console.WriteLine($"Could not validate layout {Layouts[i].Hashes.FirstOrDefault()}");
                Validated = false;
            }
        }
    }

    private ColumnDefinition[] ParseColumns(string[] lines, ref int lineNumber)
    {
        var columns = new List<ColumnDefinition>();
        while (lineNumber < lines.Length && !string.IsNullOrWhiteSpace(lines[lineNumber]))
        {
            var column = new ColumnDefinition(lines[lineNumber]);
            if (!column.IsValid) break;
            columns.Add(column);
            lineNumber++;
        }
        return columns.ToArray();
    }

    private LayoutDefinition[] ParseLayouts(string[] lines, int lineNumber)
    {
        var temp = new List<LayoutDefinition>();
        while (lineNumber < lines.Length)
        {
            var layout = new LayoutDefinition(lineNumber, lines);
            if (!layout.IsValid) lineNumber++;
            else
            {
                lineNumber = layout.End + 1;
                temp.Add(layout);
            }
        }
        return temp.ToArray();
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

    public override object GetData(int id, string property)
    {{
        try
        {{
            
        }}
        catch (Exception e) {{ Console.WriteLine(e); }}
        return null;
    }}

    public override void SetData(int id, string property, object value)
    {{
        try
        {{
            
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