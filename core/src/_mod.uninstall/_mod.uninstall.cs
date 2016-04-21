using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Reflection;

[assembly: AssemblyTitle(dotNetMT.Core.assemblyTitle)]
[assembly: AssemblyDescription(dotNetMT.Core.assemblyDescription)]
[assembly: AssemblyCopyright(dotNetMT.Core.assemblyCopyright)]
[assembly: ComVisible(false)]
[assembly: Guid(dotNetMT.Core.assemblyGUID)]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyVersion("1.0.*")]

namespace dotNetMT
{
    class Core
    {
        public static bool isPauseAfterExit = true;
        public const string assemblyTitle = ".Net Modding Tools (uninstaller)";
        public const string assemblyDescription = "Remove installed mods ...";
        public const string assemblyCopyright = "Written by de1ta0ne / @HearthSim";
        public const string assemblyGUID = "4C125770-06AF-43C7-B243-39FCE3492BD0";
        private static Version _assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
        public static readonly string assemblyVersion = _assemblyVersion.Major.ToString() + "." + _assemblyVersion.Minor.ToString() + " build " + _assemblyVersion.Build;
        public static readonly string assemblyDate = (new DateTime(2000, 1, 1).AddDays(_assemblyVersion.Build).AddSeconds(_assemblyVersion.Revision * 2)).ToString("dd.MM.yyyy");
        public static readonly string version = "v" + assemblyVersion + " [" + assemblyDate + "]";

        public static readonly string assemblyFile = Assembly.GetExecutingAssembly().Location;
        public static readonly string assemblyFolder = Path.GetDirectoryName(assemblyFile) + Path.DirectorySeparatorChar;
        public static readonly string startupFolder = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar;

        public static void print(string message)
        {
            Console.Write(message + "\n");
        }

        public static void printf(string format, params object[] paramList)
        {
            Console.Write(String.Format(format, paramList) + "\n");
        }

        static void Main(string[] args)
        {
            Console.WindowWidth = 120;
            Console.WindowHeight = 42;
            print(assemblyTitle + " " + version + "\n" + assemblyCopyright + "\n\nTask: " + assemblyDescription + "\n");

            Directory.SetCurrentDirectory(assemblyFolder);
            var errors = new List<string>();

            string managedFolder = ".\\";
            string[] dataFolder_ = Directory.GetDirectories(".\\", "*_Data");
            if (dataFolder_.Length > 0) managedFolder = dataFolder_[0] + "\\managed\\"; // errors.Add("GAME_Data folder not found!");

            if (!Directory.Exists("_mods")) errors.Add("Folder '_mods' not found!");
            if (!Directory.Exists("_mods\\managed")) errors.Add("Folder '_mods\\managed' not found!");
            if (managedFolder != "" && !Directory.Exists(managedFolder)) errors.Add("Folder '" + managedFolder + "' not found!");

            var filesToCheck = new Dictionary<string, string>()
            {
                {"_mods\\managed\\_mod.modules.dll", "File '_mods\\managed\\_mod.modules.dll' not found!"},
                {"_mods\\managed\\_mod.core.dll", "File '_mods\\managed\\_mod.core.dll' not found!"},
                {"_mods\\managed\\_mod.injector.exe", "File '_mods\\managed\\_mod.injector.exe' not found!"},
                {"_mods\\managed\\Mono.Cecil.dll", "File '_mods\\managed\\Mono.Cecil.dll' not found!"},
                {"_mods\\managed\\Mono.Cecil.Rocks.dll", "File '_mods\\managed\\Mono.Cecil.Rocks.dll' not found!"},                
            };
            foreach (var k in filesToCheck.Keys) if (!File.Exists(k)) errors.Add(filesToCheck[k]);

            if (errors.Count > 0)
            {
                foreach (var error in errors) print("ERROR: " + error);
                if (isPauseAfterExit) Console.ReadKey(true);
                return;
            }

            print("[i] game managed folder = '" + managedFolder + "' ...");


            print("[i] delete mod files from game ...");
            List<string> toDelete = new List<string>(Directory.GetFiles(".\\_mods\\managed\\", "*"));
            foreach (var file in toDelete)
            {
                string fn = managedFolder + Path.GetFileName(file);
                if (!File.Exists(fn)) continue;
                print("\t" + fn);
                File.Delete(fn);                
            }
            toDelete = new List<string>(Directory.GetFiles(managedFolder, "_mod.*.log"));            
            foreach (var file in toDelete)
            {
                string fn = managedFolder + Path.GetFileName(file);
                if (!File.Exists(fn)) continue;
                print("\t" + fn);
                File.Delete(fn);                
            }             
            
            print("[i] restore originals in '" + managedFolder + "' ...");
            List<string> toRestore = new List<string>(Directory.GetFiles(managedFolder, "*.bak"));
            foreach (var file in toRestore)
            {
                print("\t" + file + " => " + managedFolder + Path.GetFileNameWithoutExtension(file));
                File.Delete(managedFolder + Path.GetFileNameWithoutExtension(file));
                File.Copy(file, managedFolder + Path.GetFileNameWithoutExtension(file));
                File.Delete(file);
            }

            print("Done!\n");
            if (isPauseAfterExit) Console.ReadKey(true);
        }
    }
}
