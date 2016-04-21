using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;
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
        public const string assemblyTitle = ".Net Modding Tools (installer)";
        public const string assemblyDescription = "Build and install mods ...";
        public const string assemblyCopyright = "Written by de1ta0ne / @HearthSim";
        public const string assemblyGUID = "4C125770-06AF-43C7-B243-39FCE3492BD6";
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

        //Regex reg = new Regex(@"(\d+)\.(\d+)\.(\d+)(\.\d+)*");
        //var dirs = Directory.GetDirectories(path)
        //             .Where(path => reg.IsMatch(path))
        //             .ToList();

        static void InjectModCore(string managedFolder)
        {
            string currentFolder = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(managedFolder);
            string[] rsp = Directory.GetFiles(".", "*.rsp", SearchOption.TopDirectoryOnly);

            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
            p.StartInfo.FileName = "_mod.injector.exe";
            p.StartInfo.Arguments = "";
            p.Start();
            // Do not wait for the child process to exit before reading to the end of its redirected stream.
            // p.WaitForExit();
            // Read the output stream first and then wait.
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            p.Close();
            p.Dispose();
            if (output.Length > 0) print(output);

            Directory.SetCurrentDirectory(currentFolder);            
            return;
        }       

        static bool BuilModCore(string csc)
        {
            Directory.SetCurrentDirectory("_mods");
            string[] rsp = Directory.GetFiles(".", "*.rsp", SearchOption.TopDirectoryOnly);

            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
            p.StartInfo.FileName = csc;
            foreach (string file in rsp) p.StartInfo.Arguments += " @" + file;
            print("[i] executing compiler = '" + p.StartInfo.FileName + p.StartInfo.Arguments + "'");
            p.Start();
            // Do not wait for the child process to exit before reading to the end of its redirected stream.
            // p.WaitForExit();
            // Read the output stream first and then wait.
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            p.Close();
            p.Dispose();
            if (output.Length > 0) print(output);
            
            Directory.SetCurrentDirectory("..");
            if (File.Exists("_mods\\managed\\_mod.modules.dll")) return (true);
            return (false);
        }

        static void Main(string[] args)
        {
            Console.WindowWidth = 120;
            Console.WindowHeight = 42;
            print(assemblyTitle + " " + version + "\n" + assemblyCopyright + "\n\nTask: " + assemblyDescription + "\n");

            Directory.SetCurrentDirectory(assemblyFolder);
            string csc = Environment.GetFolderPath(Environment.SpecialFolder.Windows) + "\\Microsoft.NET\\Framework\\v4.0.30319\\csc.exe";

            var errors = new List<string>();

            string managedFolder = ".\\";
            string[] dataFolder_ = Directory.GetDirectories(".\\", "*_Data");
            if (dataFolder_.Length > 0) managedFolder = dataFolder_[0] + "\\managed\\"; // errors.Add("GAME_Data folder not found!");
            
            if (!Directory.Exists("_mods")) errors.Add("Folder '_mods' not found!");
            if (!Directory.Exists("_mods\\managed")) errors.Add("Folder '_mods\\managed' not found!");
            if (managedFolder != "" && !Directory.Exists(managedFolder)) errors.Add("Folder '"+ managedFolder + "' not found!");

            var filesToCheck = new Dictionary<string, string>()
            {
                {csc, "Microsoft.NET Framework v4.0 compiler not found!"},                
                {"_mods\\_mod.modules.rsp", "Microsoft.NET Framework v4.0 compiler response file ('_mods\\_mod.modules.rsp') not found!"},                
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

            File.Delete("_mods\\managed\\_mod.modules.dll");

            print("[i] game managed folder = '" + managedFolder + "'");
            if (!BuilModCore(csc))
            {
                print("ERROR: Can't compile '_mods\\managed\\_mod.modules.dll', please check mod sources ...");
                if (isPauseAfterExit) Console.ReadKey(true);
                return;
            }
            print("[i] _mods\\managed\\_mod.modules.dll - compiled ok!");
            
            print("[i] copy mod files to game");
            List<string> toCopy = new List<string>(Directory.GetFiles(".\\_mods\\managed\\", "*"));
            foreach (var file in toCopy)
            {
                print("\t" + file + " => " + managedFolder + Path.GetFileName(file));
                File.Delete(managedFolder + Path.GetFileName(file));
                File.Copy(file, managedFolder + Path.GetFileName(file));
            }

            print("[i] injecting '_mods\\managed\\_mod.modules.dll' to game\n");
            InjectModCore(managedFolder);

            List<string> toUpdate = new List<string>(Directory.GetFiles(managedFolder, "*.modded"));
            if (toUpdate.Count == 0)
            {
                print("ERROR: Updated/modded files not found!");
                if (isPauseAfterExit) Console.ReadKey(true);
                return;
            }
            
            print("[i] update game files ...");
            foreach (var file in toUpdate)
            {
                print("\t" + file + " => " + managedFolder + Path.GetFileNameWithoutExtension(file));
                File.Delete(managedFolder + Path.GetFileNameWithoutExtension(file));
                File.Copy(file, managedFolder + Path.GetFileNameWithoutExtension(file));
                File.Delete(file);
            }

            print("\nDone!");
            if (isPauseAfterExit) Console.ReadKey(true);
        }
    }
}
