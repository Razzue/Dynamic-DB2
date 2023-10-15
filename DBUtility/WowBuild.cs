using System;

namespace DynamicDB2.DBUtility;

public class WowBuild : IComparable
{
    public readonly int Expansion;
    public readonly int Major;
    public readonly int Minor;
    public readonly int Build;

    public WowBuild(string version)
    {
        try
        {
            if (!version.Contains(".")) return;
            var split = version.Split('.');

            Expansion = short.Parse(split[0]);
            Major = short.Parse(split[1]);
            Minor = short.Parse(split[2]);
            Build = int.Parse(split[3]);
        }
        catch (Exception e) { Console.WriteLine(e); }
    }

    public int CompareTo(object obj)
    {
        var result = 1;
        try
        {
            if (obj is WowBuild build)
            {
                if (Expansion != build.Expansion)
                    result = Expansion.CompareTo(build.Expansion);
                else if (Major != build.Major)
                    result = Major.CompareTo(build.Major);
                else if (Minor != build.Minor)
                    result = Minor.CompareTo(build.Minor);
                else if (Build != build.Build)
                    result = Build.CompareTo(build.Build);
                else result = 0;
            }
            else throw new ArgumentException("Invalid build was passed.");
        }
        catch (Exception ex) { Console.WriteLine(ex); }
        return result;
    }
    public override int GetHashCode()
    {
        if (Build > 0xFFFFF) throw new Exception("Build too large for fake hash code");
        if (Minor > 0xF) throw new Exception("Minor too large for fake hash code");
        if (Major > 0xF) throw new Exception("Major too large for fake hash code");
        if (Expansion > 0xF) throw new Exception("Expansion too large for fake hash code");
        return (int)((uint)Expansion << 28 | (uint)Major << 24 | (uint)Minor << 20 | (uint)Build);
    }
    public override string ToString()
        => $"{Expansion}.{Major}.{Minor}.{Build}";
    public bool Equals(WowBuild build)
    {
        return build != null &&
               build.Expansion == Expansion &&
               build.Major == Major &&
               build.Minor == Minor &&
               build.Build == this.Build;
    }
    public override bool Equals(object obj)
        => obj is WowBuild build && Equals(build);
}

public class WowBuildRange : IComparable
{
    public readonly WowBuild Minimum;
    public readonly WowBuild Maximum;

    public WowBuildRange(WowBuild minimum, WowBuild maximum)
    {
        Minimum = minimum;
        Maximum = maximum;

        if (Minimum.Expansion != Maximum.Expansion)
            throw new Exception("Expansion differs across build range. This is not allowed!");
    }

    public WowBuildRange(string buildRange)
    {
        var split = buildRange.Split('-');
        Minimum = new WowBuild(split[0]);
        Maximum = new WowBuild(split[1]);

        if (Minimum.Expansion != Maximum.Expansion)
            throw new Exception("Expansion differs across build range. This is not allowed!");
    }

    public int CompareTo(object obj)
    {
        var result = 1;
        try
        {
            if (obj is WowBuildRange otherBuildRange)
            {
                if (!Minimum.Equals(otherBuildRange.Minimum))
                    result = Minimum.CompareTo(otherBuildRange.Minimum);
                else if (Maximum.Equals(otherBuildRange.Maximum))
                    result = Maximum.CompareTo(otherBuildRange.Maximum);
                else result = 0;
            }
            else throw new ArgumentException("Invalid build range was passed.");
        }
        catch (Exception e) { Console.WriteLine(e); }
        return result;
    }
    public override int GetHashCode()
        => CombineHashes(CombineHashes(Minimum), Maximum.GetHashCode());
    public override string ToString()
        => $"{Minimum}-{Maximum}";
    public bool Contains(WowBuild build)
    {
        return
            build.Expansion >= Minimum.Expansion && build.Expansion <= Maximum.Expansion &&
            build.Major >= Minimum.Major && build.Major <= Maximum.Major &&
            build.Build >= Minimum.Build && build.Build <= Maximum.Build;
    }
    public bool Equals(WowBuildRange buildRange)
        => Minimum.Equals(buildRange.Minimum) && Maximum.Equals(buildRange.Maximum);
    public override bool Equals(object obj)
    {
        var buildRange = obj as WowBuildRange;
        return buildRange != null && Equals(buildRange);
    }
    public static int CombineHashes(object obj, int current = 0)
        => current ^ obj.GetHashCode() + -1640531527 + (current << 6) + (current >> 2);
}