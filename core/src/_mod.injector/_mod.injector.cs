using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Reflection;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;


[assembly: AssemblyTitle(dotNetMT.Core.assemblyTitle)]
[assembly: AssemblyDescription(dotNetMT.Core.assemblyDescription)]
[assembly: AssemblyCopyright(dotNetMT.Core.assemblyCopyright)]
[assembly: ComVisible(false)]
[assembly: Guid(dotNetMT.Core.assemblyGUID)]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyVersion("1.0.*")]

namespace dotNetMT
{
    class HookEntry
    {
        public string assembly;
        public string type;
        public string method;
        public Type type_;
        public MethodInfo method_;
        public string type2hook;
        public string method2hook;
        public bool hookOnBegin;
    }

    class Core
    {
        public static bool isPauseAfterExit = false;
        public const string assemblyTitle = ".Net Modding Tools (injector)";
        public const string assemblyDescription = "Inject user mods to assembly ...";
        public const string assemblyCopyright = "Written by de1ta0ne / @HearthSim";
        public const string assemblyGUID = "4C125770-06AF-43C7-B243-39FCE3492BD6";
        private static Version _assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
        public static readonly string assemblyVersion = _assemblyVersion.Major.ToString() + "." + _assemblyVersion.Minor.ToString() + " build " + _assemblyVersion.Build;
        public static readonly string assemblyDate = (new DateTime(2000, 1, 1).AddDays(_assemblyVersion.Build).AddSeconds(_assemblyVersion.Revision * 2)).ToString("dd.MM.yyyy");
        public static readonly string version = "v" + assemblyVersion + " [" + assemblyDate + "]";

        public static readonly string assemblyFile = Assembly.GetExecutingAssembly().Location;
        public static readonly string assemblyFolder = Path.GetDirectoryName(assemblyFile) + Path.DirectorySeparatorChar;
        public static readonly string startupFolder = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar;

        public static readonly string fileIni = Path.Combine(assemblyFolder, "_mod.core.ini");

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string value, string filePath);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder value, int size, string filePath);

        public static void WriteProfileString(string section, string key, string value)
        {
            WritePrivateProfileString(section, key, value, fileIni);
        }

        public static string GetProfileString(string section, string key, string def = "")
        {
            StringBuilder temp = new StringBuilder(1024);
            int i = GetPrivateProfileString(section, key, def, temp, 1024, fileIni);
            return temp.ToString();
        }

        public static void print(string message)
        {
            Console.Write(message + "\n");
        }

        public static void printf(string format, params object[] paramList)
        {
            Console.Write(String.Format(format, paramList) + "\n");
        }

        static void InjectHook(ModuleDefinition module, MethodDefinition method, HookEntry hook_)
        {
            var hookTypeRef = module.ImportReference(hook_.type_);
            var hookMethodRef = module.ImportReference(hook_.method_);            
            
            // object[] interceptedArgs;
            // object hookResult;
            var interceptedArgs = new VariableDefinition("interceptedArgs", method.Module.TypeSystem.Object.MakeArrayType());
            var hookResult = new VariableDefinition("hookResult", method.Module.TypeSystem.Object);
            method.Body.Variables.Add(interceptedArgs);
            method.Body.Variables.Add(hookResult);
            var numArgs = method.Parameters.Count;
            var hook = new List<Instruction>();

            // interceptedArgs = new object[numArgs];
            hook.Add(Instruction.Create(OpCodes.Ldc_I4, numArgs));
            hook.Add(Instruction.Create(OpCodes.Newarr, method.Module.TypeSystem.Object));
            hook.Add(Instruction.Create(OpCodes.Stloc, interceptedArgs));

            // rmh = methodof(this).MethodHandle;
            hook.Add(Instruction.Create(OpCodes.Ldtoken, method));

            // thisObj = static ? null : this;
            if (!method.IsStatic) hook.Add(Instruction.Create(OpCodes.Ldarg_0));
            else hook.Add(Instruction.Create(OpCodes.Ldnull));

            var i = 0;
            foreach (var param in method.Parameters)
            {
                // interceptedArgs[i] = (object)arg;
                hook.Add(Instruction.Create(OpCodes.Ldloc, interceptedArgs));
                hook.Add(Instruction.Create(OpCodes.Ldc_I4, i));
                hook.Add(Instruction.Create(OpCodes.Ldarg, param));
                if (param.ParameterType.IsByReference)
                {
                    // if the arg is a reference type, it must be copied and boxed
                    var refType = (ByReferenceType)param.ParameterType;
                    hook.Add(Instruction.Create(OpCodes.Ldobj, refType.ElementType));
                    hook.Add(Instruction.Create(OpCodes.Box, refType.ElementType));
                }
                else if (param.ParameterType.IsValueType)
                {
                    // if the arg descends from ValueType, it must be boxed to be
                    // converted to an object:
                    hook.Add(Instruction.Create(OpCodes.Box, param.ParameterType));
                }
                hook.Add(Instruction.Create(OpCodes.Stelem_Ref));
                i++;
            }
            // hookResult = HookRegistry.OnCall(rmh, thisObj, interceptedArgs);
            hook.Add(Instruction.Create(OpCodes.Ldloc, interceptedArgs));
            hook.Add(Instruction.Create(OpCodes.Call, hookMethodRef));
            hook.Add(Instruction.Create(OpCodes.Stloc, hookResult));
            // if (hookResult != null) {
            //     return (ReturnType)hookResult;
            // }
            hook.Add(Instruction.Create(OpCodes.Ldloc, hookResult));
            hook.Add(Instruction.Create(OpCodes.Ldnull));
            hook.Add(Instruction.Create(OpCodes.Ceq));
            if(hook_.hookOnBegin) hook.Add(Instruction.Create(OpCodes.Brtrue_S, method.Body.Instructions.First()));
            else hook.Add(Instruction.Create(OpCodes.Brtrue_S, method.Body.Instructions.Last()));
            
            if (!method.ReturnType.FullName.EndsWith("Void"))
            {
                if (!hook_.hookOnBegin) hook.Add(Instruction.Create(OpCodes.Pop));
                hook.Add(Instruction.Create(OpCodes.Ldloc, hookResult));
                hook.Add(Instruction.Create(OpCodes.Castclass, method.ReturnType));
                hook.Add(Instruction.Create(OpCodes.Unbox_Any, method.ReturnType));
            }
            hook.Add(Instruction.Create(OpCodes.Ret));

            hook.Reverse();

            int adr = hook_.hookOnBegin ? 0 : method.Body.Instructions.Count - 1;
            foreach (var instruction in hook) method.Body.Instructions.Insert(adr, instruction);
            method.Body.OptimizeMacros();
        }

        static void ProcessHook(ModuleDefinition module, HookEntry hook)
        {
            string hookString = "hook [" + (hook.hookOnBegin ? "begin" : "end") + "] " + hook.type2hook + "." + hook.method2hook + " => " + hook.type + "." + hook.method + "(...)";
            
            var type = module.GetType(hook.type2hook);
            if (type == null)
            {
                print("WARNING: Type '" + hook.type2hook + "' not found in assembly!\n\tOn " + hookString);
                return;
            }
            
            bool found = false;
            foreach (MethodDefinition method in type.Methods)
            {
                if (method.Name != hook.method2hook) continue;
                
                found = true;
                print("Inject: " + hookString);

                if (!method.HasBody)
                {
                    print("WARNING: Cannot hook method '" + hook.method2hook + "', method without body!\n\tOn " + hookString);
                    continue;
                }
                if (method.HasGenericParameters)
                {
                    print("WARNING: Cannot hook method '" + hook.method2hook + "', generic parameters not supported!\n\tOn " + hookString);
                    continue;
                }

                InjectHook(module, method, hook);                                
            }
            if (!found) print("WARNING: Method '" + hook.method2hook + "' not found in assembly!\n\tOn " + hookString);
        }

        static void ReadHookList(string hooksAssemblyFileName, Dictionary<string, List<HookEntry>> hooksList)
        {
            Dictionary<string, string> modules = new Dictionary<string, string>();
            
            // var hooksAssembly = typeof(GM).Assembly;
            var hooksAssembly = Assembly.LoadFrom(hooksAssemblyFileName);
            
            foreach (Type type in hooksAssembly.GetTypes())
            {
                foreach (MethodInfo method in type.GetMethods())
                {
                    var attrs = Attribute.GetCustomAttributes(method);
                    foreach (var attr in attrs)
                    {
                        if (attr.GetType().FullName != "dotNetMT.RuntimeHookAttribute") continue;
                        //if (!(attr is RuntimeHookAttribute)) continue;
                        //var a = (RuntimeHookAttribute)attr;
                        var entry = new HookEntry();
                        entry.method = method.Name;
                        entry.type = type.Name;
                        entry.method_ = method;
                        entry.type_ = type;
                        //entry.assembly = a.assemblyName;
                        entry.assembly = (string) attr.GetType().GetField("assemblyName").GetValue(attr);
                        //entry.type2hook = a.typeName;
                        entry.type2hook = (string) attr.GetType().GetField("typeName").GetValue(attr);
                        //entry.method2hook = a.methodName;
                        entry.method2hook = (string) attr.GetType().GetField("methodName").GetValue(attr);
                        //entry.hookOnBegin = a.hookOnBegin;
                        entry.hookOnBegin = (bool) attr.GetType().GetField("hookOnBegin").GetValue(attr);

                        if (!hooksList.ContainsKey(entry.assembly)) hooksList.Add(entry.assembly, new List<HookEntry>());
                        hooksList[entry.assembly].Add(entry);
                        
                        if (!modules.ContainsKey(entry.type)) modules[entry.type] = "";
                    }
                }
            }

            foreach(var k in modules.Keys)
            {
                // var v = GM.Config.Get("modules." + k, "1");
                var v = GetProfileString("modules", k, "1");
                // GM.Config.Set("modules." + k, v);
                WriteProfileString("modules", k, v);
            }
        }

        static void Main(string[] args)
        {
            try
            {
                Console.WindowWidth = 110;
                Console.WindowHeight = 32;
            }
            catch { }

            print(assemblyTitle + " " + version + "\n" + assemblyCopyright + "\n\nTask: " + assemblyDescription + "\n");

            Dictionary<string, List<HookEntry>> hookList = new Dictionary<string, List<HookEntry>>();

            string hooksAssemblyFileName = assemblyFolder + "_mod.modules.dll";
            if (!File.Exists(hooksAssemblyFileName))
            {
                print("ERROR: Hook assembly file not found '" + hooksAssemblyFileName + "' !");
                if (isPauseAfterExit) Console.ReadKey(true);
                return;
            }

            ReadHookList(hooksAssemblyFileName, hookList);

            foreach (var assemblyFileName in hookList.Keys)
            {
                var hooks = hookList[assemblyFileName];

                print("Processing: '" + assemblyFileName + "' ...");
                
                if (!File.Exists(assemblyFileName))
                {
                    print("ERROR: Assembly file not found '" + assemblyFileName + "' !");
                    continue;
                }

                if (File.Exists(assemblyFileName + ".bak"))
                {
                    File.Delete(assemblyFileName);
                    File.Copy(assemblyFileName + ".bak", assemblyFileName);
                }
                else File.Copy(assemblyFileName, assemblyFileName + ".bak");

                using (var inStream = File.Open(assemblyFileName, FileMode.Open, FileAccess.Read))
                {
                    var scriptAssembly = AssemblyDefinition.ReadAssembly(inStream);

                    // scriptAssembly.MainModule.AssemblyReferences.Add(new AssemblyNameReference("_mod.core", new Version(1, 0, 0, 0)));
                    scriptAssembly.MainModule.ImportReference(typeof(System.RuntimeMethodHandle));

                    foreach (var hook in hooks) ProcessHook(scriptAssembly.MainModule, hook);

                    //scriptAssembly.Write(assemblyFileName.Replace(".", ".modded."));
                    scriptAssembly.Write(assemblyFileName + ".modded");
                }
                //File.Delete(assemblyFileName);
                //File.Move(assemblyFileName.Replace(".", ".modded."), assemblyFileName);
            }

            print("Done!");
            if (isPauseAfterExit) Console.ReadKey(true);
        }
    }
}
