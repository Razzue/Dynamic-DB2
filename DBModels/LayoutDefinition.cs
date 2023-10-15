using System;
using System.Collections.Generic;
using System.Linq;

namespace DynamicDB2.DBModels;

using DBUtility;

internal struct LayoutDefinition
{
    private readonly int _end;
    private readonly string _comment;
    private readonly string[] _hashes;
    private readonly WowBuild[] _builds;
    private readonly WowBuildRange[] _ranges;
    private readonly LineDefinition[] _fields;

    internal string[] Hashes => _hashes;
    internal LayoutDefinition(int index, string[] lines)
    {
        _end = 0;
        _comment = string.Empty;
        _hashes = Array.Empty<string>();
        _builds = Array.Empty<WowBuild>();
        _ranges = Array.Empty<WowBuildRange>();
        _fields = Array.Empty<LineDefinition>();


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
                    _comment = line.Substring(7).Trim();
                    lineNumber++;
                    continue;
                }

                if (line.StartsWith("LAYOUT"))
                {
                    var hashes = line.Remove(0, 7).Split(new[] { ", " }, StringSplitOptions.None);
                    if (hashes is { Length: > 0 }) _hashes = hashes;
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

            _fields = fields.ToArray();
            _builds = builds.ToArray();
            _ranges = ranges.ToArray();
            _end = lineNumber;
        }
        catch (Exception e) { Console.WriteLine(e); }

    }

     internal bool Validate(Dictionary<string, ColumnDefinition> columns)
    {
        try
        {
            if (!IsValid) return false;
            for (var i = 0; i < _fields.Length; i++)
                if (!_fields[i].Validate(columns))
                {
                    Console.WriteLine($"Could not validate field {_fields[i].Name}.");
                    return false;
                }
        }
        catch (Exception e) { Console.WriteLine(e); }
        return true;
    }
    internal bool ContainsBuild(WowBuild build)
        => _builds.Contains(build) || _ranges.Any(x => x.Contains(build));
    public override string ToString()
    {
        var message = string.Empty;
        try
        {
            if (!IsValid) return message;
            for (int i = 0; i < _fields.Length; i++)
                message += $"   {_fields[i]}\n";
        }
        catch (Exception e) { Console.WriteLine(e); }
        return message;
    }

    internal int End => _end;

    internal bool HasLayouts
        => _hashes is { Length: > 0 };
    internal bool HasBuilds
        => _builds is { Length: > 0 } || _ranges is { Length: > 0 };
    internal bool HasFields
        => _fields is { Length: > 0 };
    internal bool IsValid
        => HasLayouts && HasBuilds && HasFields;
}