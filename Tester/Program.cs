using System;
using DynamicDB2;
using DynamicDB2.DBUtility;

namespace Tester
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Change the build to generate a template here.
            // Double check supported build in the .dbd files.
            var build = new WowBuild("10.1.7.51536");

            // Quickly yoink the .dbds from WowDefs github.
            // Realistically this only needs to happen once every patch,
            // and even that's overkill for how often templates actually change.
            using (var updater = new Updater())
                updater.Download(false);

            Console.WriteLine("Press any key to continue.");
            Console.ReadKey(true);

            // Quickly convert the .dbd files to a .cs file if they support the requested build.
            using (var manager = new Manager(build))
                manager.ParseTemplates();

            // Compile the .cs files to a singular .dll file. Check the local directory for output.
            // File can be ran through DnSpy or DotPeek to confirm everything converted successfully.
            new Compiler().Compile(build.Expansion);

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey(true);
        }
    }
}
