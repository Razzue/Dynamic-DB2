using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using RestSharp;
using System.Reflection;
using System.CodeDom.Compiler;
using Microsoft.CodeDom.Providers.DotNetCompilerPlatform;

using DynamicDB2.Helpers;
using DynamicDB2.DBUtility;

namespace DynamicDB2
{
    public class Manager : IDisposable
    {
        private WowBuild _build;
        private string _basePath;
        private string _filePath;
        internal bool IsValid
            => !string.IsNullOrEmpty(_build?.ToString());

        public Manager(WowBuild build)
        {
            _build = build;
            _basePath = Environment.CurrentDirectory;
            _filePath = Path.Combine(_basePath, "Templates");
            if(!Directory.Exists(_filePath))
                Directory.CreateDirectory(_filePath);
        }

        public void ParseTemplates()
        {
            for (var i = 0; i < (int)WowDbd.Maximum; i++)
            {
                var template = new DBTemplate((WowDbd)i, _build);
                if (!template.Validated) return;

                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {(WowDbd)i} was parsed.");
                var text = template.ToString();
                if (string.IsNullOrEmpty(text))
                {
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {(WowDbd)i} does not have a template for {_build}.");
                    continue;
                }

                var file = $"{_filePath}\\{(WowDbd)i}.cs";
                File.WriteAllText(file, template.ToString());
            }
        }

        #region Disposable support

        ~Manager() => Dispose(true);
        private bool _disposedValue;
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (_disposedValue) return;
            _build = null;
            _disposedValue = true;
        }

        #endregion
    }

    public class Updater : IDisposable
    {
        private string _url;
        private string _basePath;
        private string _filePath;

        public Updater()
        {
            _basePath = Environment.CurrentDirectory;
            _filePath = Path.Combine(_basePath, "Definitions");

            if (!Directory.Exists(_filePath))
                Directory.CreateDirectory(_filePath);

            _url = @"https://raw.githubusercontent.com/wowdev/WoWDBDefs/master/definitions/{DEFNAME}";
        }

        public void Download(bool replace)
        {
            // Get an array of definition names from WowDbdHelper
            var dbds = WowDbdHelper.GetDbdNames();

            // Iterate over the array of definition names in parallel
            Parallel.For(0, dbds.Length, i =>
            {

                // Initialize a new RestClient and RestRequest
                var client = new RestClient(_url);
                var request = new RestRequest()
                    .AddUrlSegment("DEFNAME", dbds[i]);

                // Determine the file path for the .dbd file
                var path = Path.Combine(_filePath, dbds[i]);

                // Check if the file already exists and replacement is disabled; if so, skip this .dbd
                if (File.Exists(path))
                    if (!replace)
                    {
                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {dbds[i]} already exists.");
                        return;
                    }

                // Set the response output file stream
                using var writer = File.Open(path, FileMode.OpenOrCreate);
                request.ResponseWriter = responseStream =>
                {
                    // Copy the response stream to the file writer
                    responseStream.CopyTo(writer);
                    return writer;
                };

                // Download the stream from the server and save it to the file
                client.DownloadStream(request);

                // Print a message indicating that the .dbd file has been successfully streamed (optional)
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Streamed in {dbds[i]}.");

                // Flush the writer to ensure all data is written to the file
                writer.Flush();
            });
        }

        #region Disposable support

        ~Updater() => Dispose(true);
        private bool _disposedValue;
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (_disposedValue) return;
            _url = null;
            _basePath = null;
            _filePath = null;
            _disposedValue = true;
        }

        #endregion
    }

    public class Compiler
    {
        private string[] _files;
        private readonly string[] _resources;
        private readonly CSharpCodeProvider _provider;

        public Compiler()
        {
            var resource = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            var strs = resource.Where(rec => LastChars(rec, 4) == ".dll").ToList();
            strs.AddRange(new[]
            {
                "System.dll",
                "DynamicDB2.dll",
                typeof(System.Linq.Enumerable).Assembly.Location,
            });

            _provider = new(new ProviderOptions($"{Environment.CurrentDirectory}\\roslyn\\csc.exe", -1));
            _resources = strs.ToArray();

            var path = $"{Environment.CurrentDirectory}\\Templates";
            if(!Directory.Exists(path)) return;

            var info = new DirectoryInfo(path);
            _files = info.GetFiles().Select(x => x.FullName).ToArray();
        }

        public void Compile(int expansion)
        {
            try
            {
                var name = expansion switch
                {
                    10 => "Retail",
                    3 => "Classic",
                    1 => "Vanilla",
                    _ => string.Empty
                };
                if (string.IsNullOrEmpty(name))
                    throw new ArgumentException("Could not determine expansion to compile for.");
                name = $"{Environment.CurrentDirectory}\\{name}.dll";
                var parameters = new CompilerParameters(_resources)
                {
                    OutputAssembly = name,
                    GenerateInMemory = false,
                    GenerateExecutable = false,
                    TreatWarningsAsErrors = true,
                    IncludeDebugInformation = true,
                    CompilerOptions = "/unsafe /optimize",
                };

                var result = _provider.CompileAssemblyFromFile(parameters, _files);
                if (result.Errors.HasErrors)
                    foreach (CompilerError error in result.Errors) Console.WriteLine(error);
            }
            catch (Exception e) { Console.WriteLine(e); }
        }

        public string LastChars(string source, int tail_length)
            => tail_length >= source.Length ? source : source.Substring(source.Length - tail_length);
    }
}
