using System;
using System.Linq;
using System.Collections.Generic;

namespace DynamicDB2.DefStructs;
using DefHelpers;

internal struct LayoutDefinition
{
    internal int End { get; private set; }
    internal string Comment { get; private set; }
    internal string[] Hashes { get; private set; }
    internal WowBuild[] Builds { get; private set; }
    internal WowBuildRange[] Ranges { get; private set; }
    internal LineDefinition[] Fields { get; private set; }

    public LayoutDefinition(int index, string[] lines)
    {
        End = 0;
        Comment = string.Empty;
        Hashes = Array.Empty<string>();
        Builds = Array.Empty<WowBuild>();
        Ranges = Array.Empty<WowBuildRange>();
        Fields = Array.Empty<LineDefinition>();

        ParseLines(index, lines);
    }

    private void ParseLines(int index, string[] lines)
    {
        var builds = new List<WowBuild>();
        var ranges = new List<WowBuildRange>();
        var fields = new List<LineDefinition>();

        try
        {
            var lineNumber = index;
            while (lineNumber < lines.Length)
            {
                var line = lines[lineNumber];
                if (string.IsNullOrWhiteSpace(line)) break;

                if (line.StartsWith("COMMENT"))
                {
                    Comment = line.Substring(7).Trim();
                }
                else if (line.StartsWith("LAYOUT"))
                {
                    Hashes = line.Remove(0, 7).Split(new[] { ", " }, StringSplitOptions.None);
                }
                else if (line.StartsWith("BUILD"))
                {
                    var splitBuilds = line.Remove(0, 6).Split(new string[] { ", " }, StringSplitOptions.None);
                    foreach (var splitBuild in splitBuilds)
                    {
                        if (!splitBuild.Contains("-"))
                            builds.Add(new WowBuild(splitBuild));
                        else
                        {
                            var splitRange = splitBuild.Split('-');
                            ranges.Add(new WowBuildRange(
                                new WowBuild(splitRange[0]),    // Minimum
                                new WowBuild(splitRange[1])));  // Maximum
                        }
                    }
                }
                else
                {
                    fields.Add(new LineDefinition(line));
                }

                lineNumber++;
            }

            Fields = fields.ToArray();
            Builds = builds.ToArray();
            Ranges = ranges.ToArray();
            End = lineNumber;
        }
        catch (Exception e)
        {
            // Handle the exception appropriately, e.g., log or rethrow
            Console.WriteLine(e);
        }
    }

    public bool Validate(Dictionary<string, ColumnDefinition> columns)
    {
        if (!IsValid) return false;
        return Fields.All(field => field.Validate(columns));
    }
    internal bool ContainsBuild(WowBuild build)
        => Builds.Contains(build) || Ranges.Any(x => x.Contains(build));
    public override string ToString()
    {
        if (!IsValid) return string.Empty;
        return string.Join("\n", Fields.Select(field => $"   {field}"));
    }

    public bool HasLayouts => Hashes.Length > 0;
    public bool HasBuilds => Builds.Length > 0 || Ranges.Length > 0;
    public bool HasFields => Fields.Length > 0;
    public bool IsValid => HasLayouts && HasBuilds && HasFields;
}