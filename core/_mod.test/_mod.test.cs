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
        public const string assemblyTitle = ".Net Modding Tools (_mod.test)";
        public const string assemblyDescription = "_mod tester ...";
        public const string assemblyCopyright = "Written by de1ta0ne";
        public const string assemblyGUID = "4C125771-06AF-43C7-B243-39FCE3492BD9";
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

        public static void Initialize()
        {
            print("Initialized!");
        }

        public static void Update()
        {
            print("Updated!");
        }

        static void Main(string[] args)
        {
            Console.WindowWidth = 120;
            Console.WindowHeight = 42;
            print(assemblyTitle + "\n" + assemblyCopyright + "\n");

            HotkeyCore.RegisterHotkey(() =>
            {
                Console.WriteLine("HELLO!");
            }, new Hotkey(Keys.Delete));

            Initialize();
            print("[press 'ctrl+c' to stop!]");
            while (true)
            {
                Update();
                HotkeyCore.Process();
                System.Threading.Thread.Sleep(250);
                if (Keyboard.IsKeyDown(Keys.LeftAlt)) break;
                if (Keyboard.IsKeyDown(Keys.Space)) break;
            }           

            print("---");
            PluginTest.DoTest();
            print("---");
            StructTest.DoTest();                    

            print("\nDone!");
            if (isPauseAfterExit) 
                Console.ReadKey(true);
        }
    }
}
