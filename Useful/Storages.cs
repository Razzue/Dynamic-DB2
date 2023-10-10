using System;
using System.IO;

namespace DynamicDB2.Useful;

internal class Storages
{
    /// <summary>
    /// Get the directory of execution.
    /// </summary>
    private static string BaseDirectory => Environment.CurrentDirectory;

    /// <summary>
    /// Get the folder path where we'll store .dbd files.
    /// </summary>
    internal static string Definitions
    {
        get
        {
            var path = Path.Combine(BaseDirectory, "Definitions");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }
    }

    /// <summary>
    /// Get the folder path where we'll store .db2 files.
    /// </summary>
    internal static string Database
    {
        get
        {
            var path = Path.Combine(BaseDirectory, "Database");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }
    }

    /// <summary>
    /// Get the folder path where we'll store .cs files.
    /// </summary>
    internal static string Classes
    {
        get
        {
            var path = Path.Combine(BaseDirectory, "Classes");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }
    }

    /// <summary>
    /// Get the folder path where roslyn's csc.exe is stored.
    /// </summary>
    internal static string Roslyn
    {
        get
        {
            var path = Path.Combine(BaseDirectory, "Roslyn");
            path = Path.Combine(path, "csc.exe");
            return path;
        }
    }
}