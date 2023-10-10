using System;
using System.Linq;
using System.Collections.Generic;

namespace DynamicDB2.DefStructs;
using DefHelpers;

internal struct LayoutDefinition
{
    internal int End;
    internal string Comment;
    internal string[] Hashes;
    internal LineDefinition[] Fields;
    internal WowBuild[] Builds;
    internal WowBuildRange[] Ranges;

    internal LayoutDefinition(int index, string[] lines)
    {
        End = 0;
        Comment = string.Empty;
        Hashes = Array.Empty<string>();
        Fields = Array.Empty<LineDefinition>();
        Builds = Array.Empty<WowBuild>();
        Ranges = Array.Empty<WowBuildRange>();


        try
        {
            var fields = new List<LineDefinition>();
            var builds = new List<WowBuild>();
            var ranges = new List<WowBuildRange>();

            var lineNumber = index;
            while (lineNumber < lines.Length)
            {
                var line = lines[lineNumber];
                if (string.IsNullOrWhiteSpace(line)) break;

                if (line.StartsWith("COMMENT"))
                {
                    Comment = line.Substring(7).Trim();
                    lineNumber++;
                    continue;
                }

                if (line.StartsWith("LAYOUT"))
                {
                    var hashes = line.Remove(0, 7).Split(new[] { ", " }, StringSplitOptions.None);
                    if (hashes is { Length: > 0 }) Hashes = hashes;
                    else throw new Exception("Could not parse layout hash set.");

                    lineNumber++;
                    continue;
                }

                if (line.StartsWith("BUILD"))
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

                    lineNumber++;
                    continue;
                }

                fields.Add(new LineDefinition(line));
                lineNumber++;
            }

            Fields = fields.ToArray();
            Builds = builds.ToArray();
            Ranges = ranges.ToArray();
            End = lineNumber;
        }
        catch (Exception e) { Console.WriteLine(e); }

    }

    internal bool Validate(Dictionary<string, ColumnDefinition> columns)
    {
        try
        {
            if (!IsValid) return false;
            for (var i = 0; i < Fields.Length; i++)
                if (!Fields[i].Validate(columns))
                {
                    Console.WriteLine($"Could not validate field {Fields[i].Name}.");
                    return false;
                }
        }
        catch (Exception e) { Console.WriteLine(e); }
        return true;
    }
    internal bool ContainsBuild(WowBuild build)
        => Builds.Contains(build) || Ranges.Any(x => x.Contains(build));
    public override string ToString()
    {
        var message = string.Empty;
        try
        {
            if (!IsValid) return message;
            for (int i = 0; i < Fields.Length; i++)
                message += $"   {Fields[i]}\n";
        }
        catch (Exception e) { Console.WriteLine(e); }
        return message;
    }

    internal bool HasLayouts
        => Hashes is { Length: > 0 };
    internal bool HasBuilds
        => Builds is { Length: > 0 } || Ranges is { Length: > 0 };
    internal bool HasFields
        => Fields is { Length: > 0 };
    internal bool IsValid
        => HasLayouts && HasBuilds && HasFields;
}