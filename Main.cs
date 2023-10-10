using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.UI;
using DynamicDB2.DefHelpers;
using DynamicDB2.Useful;
using Microsoft.CodeDom.Providers.DotNetCompilerPlatform;
using RestSharp;

namespace DynamicDB2;

public abstract class Definition
{
    public bool IsUpdated => Db2Data.Count > 0;
    public Dictionary<int, object> Db2Data { get; set; } = new();

    public virtual void Sort(string column) { }
    public virtual bool Update(bool classic) => false;
    public virtual object Getter(int id, string column, bool classic) => null;
    public virtual void Setter(int id, string column, object value, bool classic) { }
}

internal class Program
{
    private static WowBuild _build;
    private static string[] _resources;
    private static CSharpCodeProvider _provider;

    private static readonly Dictionary<WowDatabase, DBConverter> Definitions = new();

    private static void Main(string[] args)
    {
        var success = 0;
        Task.Run(() =>
        {
            var files = WowDatabaseHelper.GetDbdNames();
            var url = @"https://raw.githubusercontent.com/wowdev/WoWDBDefs/master/definitions/{FILE}";

            Parallel.For(0, files.Length, i =>
            {
                var client = new RestClient(url);
                var request = new RestRequest()
                    .AddUrlSegment("FILE", files[i]);

                var path = $"{Storages.Definitions}\\{files[i]}";
                using var writer = File.Open(path, FileMode.OpenOrCreate);
                request.ResponseWriter = responseStream =>
                {
                    responseStream.CopyTo(writer);
                    return writer;
                };
                client.DownloadStream(request);
                Console.WriteLine($"Streamed in {files[i]}.");
                writer.Flush();

                if (File.Exists(path))
                    success++;
            });
        }).Wait(30 * 1000);
        Console.WriteLine($"Updated {success} DBD's.");


        Console.WriteLine("Press any key to exit.");
        Console.ReadKey(true);
    }
    

    internal static string LastChars(string source, int tail_length)
        => tail_length >= source.Length ? source : source.Substring(source.Length - tail_length);
}