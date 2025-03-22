using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace RKSoftware.DllVersionOverride
{
    public class Addon : RKSoftware.IPUtilities.AddOns.IAddon
    {
        public string Name => "DllVersionOverride";
        public string Command => "dllversionoverride";
        public string Description => "Overrides the version of a DLL file.";
        public string Version => "1.0.0";

        public void Initialize()
        {
        }

        public void Execute(string[] args)
        {
            var (result, application, libraries) = Validation(args);
            if (!result) return;

            var (applicationDlls, librariesDlls) = Dlls(application, libraries);
            var overrides = Orverrides(applicationDlls, librariesDlls);

            var backup = System.IO.Path.Combine(Environment.CurrentDirectory, "dllsbackup");
            if (!System.IO.Directory.Exists(backup)) System.IO.Directory.CreateDirectory(backup);

            foreach (var (applicationDll, libraryDll) in overrides)
            {
                Console.WriteLine($"Overriding {applicationDll} with {libraryDll}");
                System.IO.File.Copy(applicationDll, System.IO.Path.Combine(backup, System.IO.Path.GetFileName(applicationDll)), true);
                System.IO.File.Copy(libraryDll, applicationDll, true);
            }
        }

        (bool, string, string[]) Validation(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: PUtilities dllversionoverride <application dll directory> <library directory> <library directory> ...");
                return (false, string.Empty, Array.Empty<string>());
            }
            var application = args[0];
            var libraries = args.Skip(1).ToArray();
            if (!System.IO.Directory.Exists(application))
            {
                Console.WriteLine("Application directory not found.");
                return (false, string.Empty, Array.Empty<string>());
            }
            foreach (var library in libraries)
            {
                if (!System.IO.Directory.Exists(library))
                {
                    Console.WriteLine($"Library directory {library} not found.");
                    return (false, string.Empty, Array.Empty<string>());
                }
            }
            return (true, application, libraries);
        }

        static (string[], (string, string, FileVersionInfo)[]) Dlls(string application, string[] libraries)
        {
            var applicationDlls = System.IO.Directory.GetFiles(application, "*.dll");
            var librariesDlls = libraries.SelectMany(x => System.IO.Directory.GetFiles(x, "*.dll"))
                .Select(x => (Path.GetFileName(x), x, System.Diagnostics.FileVersionInfo.GetVersionInfo(x)));
            var newestDlls = librariesDlls.GroupBy(x => x.Item1).Select(x => x.OrderBy(xx => xx.Item3).FirstOrDefault()).ToArray();

            return (applicationDlls, newestDlls);
        }

        static (string, string)[] Orverrides(string[] applications, (string, string, FileVersionInfo)[] libraries)
        {
            List<(string, string)> overrides = new();

            var applicationDlls = applications.Select(x => (Path.GetFileName(x), x)).ToArray();
            foreach (var libraryDll in libraries)
                foreach (var applicationDll in applicationDlls)
                {
                    if (string.Compare(applicationDll.Item1, libraryDll.Item1, true) != 0) continue;
                    var version = System.Diagnostics.FileVersionInfo.GetVersionInfo(applicationDll.x);
                    if (new Version(version.FileVersion!).CompareTo(new Version(libraryDll.Item3.FileVersion!)) < 0)
                        overrides.Add((applicationDll.Item2, libraryDll.Item2));
                }
            return overrides.ToArray();
        }
    }
}
