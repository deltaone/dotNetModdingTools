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
        public const string assemblyTitle = ".Net Modding Tools (_mod.injector tester)";
        public const string assemblyDescription = "_mod.core tester ...";
        public const string assemblyCopyright = "Written by de1ta0ne";
        public const string assemblyGUID = "4C125771-06AF-43C7-B243-39FCE3492BD9";
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

        public struct TST
        {
            public int x;
            public int y;
        }

        public static TST Test0(string message)
        {
            print(message);
            var t = new TST();
            t.x = 10;
            t.y = 15;
            return (t);
        }

        public static void Test1(ref bool message)
        {
            message = true;
        }


        static void Main(string[] args)
        {
            Console.WindowWidth = 120;
            Console.WindowHeight = 42;
            print(assemblyTitle + " " + version + "\n" + assemblyCopyright + "\n");

            Test0("Test+");

            bool bReturn = false;
            Test1(ref bReturn);
            if (bReturn)
            {
                print("\nDone!\n");
            }

            print("\nDone!\n");
            if (isPauseAfterExit) Console.ReadKey(true);
        }
    }
}
