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

using System.Windows.Forms;
using System.Diagnostics;
using System.CodeDom.Compiler;
using Microsoft.CSharp;

[assembly: AssemblyTitle(dotNetMT.Core.assemblyTitle)]
[assembly: AssemblyDescription(dotNetMT.Core.assemblyDescription)]
[assembly: AssemblyCopyright(dotNetMT.Core.assemblyCopyright)]
[assembly: ComVisible(false)]
[assembly: Guid(dotNetMT.Core.assemblyGUID)]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyVersion("1.0.*")]

namespace dotNetMT
{
    enum ModEntryType
    {
        Patch,
        Hook,
    }

    class ModEntry
    {
        public ModEntryType entryType;
        public string pluginTag;
        public string assembly;        
        public string type;
        public string method;
        public Type type_;
        public MethodInfo method_;
        public string type2hook;
        public string method2hook;
        public bool hookOnBegin;
        public int parameterCount;
    }

    class PluginEntry
    {
        public string pluginTag;
        public string pluginName;
        public string pluginAuthor;
        public string pluginNote;
        public string pluginRequired;
        public bool active;

        public override string ToString()
        {
            return (pluginName);
        }
    }

    class Core
    {
//#if DEBUG 
        public static bool isPauseAfterExit = true;
//#else
//        public static bool isPauseAfterExit = false;
//#endif        
        public const string assemblyTitle = ".Net Modding Tools (setup)";
        public const string assemblyDescription = "Setup user plugins ...";
        public const string assemblyCopyright = "Written by de1ta0ne";
        public const string assemblyGUID = "00000000-06AF-43C7-B243-39FCE3492BD6";
        private static Version _assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
        public static readonly string assemblyVersion = _assemblyVersion.Major.ToString() + "." + _assemblyVersion.Minor.ToString() + " build " + _assemblyVersion.Build;
        public static readonly string assemblyDate = (new DateTime(2000, 1, 1).AddDays(_assemblyVersion.Build).AddSeconds(_assemblyVersion.Revision * 2)).ToString("dd.MM.yyyy");
        public static readonly string version = "v" + assemblyVersion + " [" + assemblyDate + "]";

        public static readonly string assemblyFile = Assembly.GetExecutingAssembly().Location;
        public static readonly string assemblyFolder = Path.GetFullPath(Path.GetDirectoryName(assemblyFile) + Path.DirectorySeparatorChar);
        public static readonly string assemblyStartupFolder = Path.GetFullPath(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar);

        public static readonly string modPluginsFile = "_mod.plugins.dll";
        public static readonly string modCoreFile = "_mod.core.dll";

        public static string modPluginsFilePath = Path.Combine(DNMT.modRootFolder, modPluginsFile);
        public static string modPluginsFilePathBAK = Path.Combine(DNMT.modRootFolder, modPluginsFile + ".bak");
        public static string modCoreFilePath = Path.Combine(assemblyFolder, modCoreFile);

        public static bool modInstalled = false;
        public static bool modQuickSetupMode = false;
        public static int modSetupAction = 0;

        public static Dictionary<string, PluginEntry> modPluginList;  
        public static Dictionary<string, List<ModEntry>> modEntryList;

        public static void print(string message)
        {
            Console.Write(message + "\n");
        }

        public static void printf(string format, params object[] paramList)
        {
            Console.Write(String.Format(format, paramList) + "\n");
        }

        //-------------------------------------------------------------------------------------------------------------
        
        static void InjectHook(ModuleDefinition module, MethodDefinition method, ModEntry entry)
        {
            if (entry.entryType != ModEntryType.Hook) return;

            var hookTypeRef = module.ImportReference(entry.type_);
            var hookMethodRef = module.ImportReference(entry.method_);

            // object[] interceptedArgs;
            // object hookResult;            
            var interceptedArgs = new VariableDefinition(method.Module.TypeSystem.Object.MakeArrayType());
            method.Body.Variables.Add(interceptedArgs);
            var hookResult = new VariableDefinition(method.Module.TypeSystem.Object);
            method.Body.Variables.Add(hookResult);

            var numArgs = method.Parameters.Count;
            var hook = new List<Instruction>();

            // interceptedArgs = new object[numArgs];            
            hook.Add(Instruction.Create(OpCodes.Ldc_I4, numArgs));
            hook.Add(Instruction.Create(OpCodes.Newarr, method.Module.TypeSystem.Object));
            hook.Add(Instruction.Create(OpCodes.Stloc, interceptedArgs));

            // rmh = methodof(this).MethodHandle;
            // hook.Add(Instruction.Create(OpCodes.Ldtoken, method));

            if (entry.hookOnBegin || method.ReturnType.FullName.EndsWith("Void")) 
                hook.Add(Instruction.Create(OpCodes.Ldnull));
            else
            {
                hook.Add(Instruction.Create(OpCodes.Dup));
                if (method.ReturnType.IsByReference)
                {   // if the arg is a reference type, it must be copied and boxed                    
                    var refType = (ByReferenceType)method.ReturnType;
                    hook.Add(Instruction.Create(OpCodes.Ldobj, refType.ElementType));
                    hook.Add(Instruction.Create(OpCodes.Box, refType.ElementType));
                }
                else if (method.ReturnType.IsValueType)
                {   // if the arg descends from ValueType, it must be boxed to be converted to an object:                    
                    hook.Add(Instruction.Create(OpCodes.Box, method.ReturnType));
                }               
            }

            // thisObj = static ? null : this;
            if (!method.IsStatic) 
                hook.Add(Instruction.Create(OpCodes.Ldarg_0));
            else 
                hook.Add(Instruction.Create(OpCodes.Ldnull));

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
                    // if the arg descends from ValueType, it must be boxed to be converted to an object:
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

            if (entry.hookOnBegin) 
                hook.Add(Instruction.Create(OpCodes.Brtrue_S, method.Body.Instructions.First()));
            else
            {   // do not replace last instruction, as result you can get branches bug (note - change is possible)
                method.Body.Instructions[method.Body.Instructions.Count - 1].OpCode = OpCodes.Nop;
                method.Body.Instructions[method.Body.Instructions.Count - 1].Operand = null;
                method.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
                hook.Add(Instruction.Create(OpCodes.Brtrue_S, method.Body.Instructions.Last()));                
            }

            if (!method.ReturnType.FullName.EndsWith("Void"))
            {
                if (!entry.hookOnBegin) hook.Add(Instruction.Create(OpCodes.Pop));
                hook.Add(Instruction.Create(OpCodes.Ldloc, hookResult));
                hook.Add(Instruction.Create(OpCodes.Castclass, method.ReturnType));
                hook.Add(Instruction.Create(OpCodes.Unbox_Any, method.ReturnType));
            }

            hook.Add(Instruction.Create(OpCodes.Ret));
            hook.Reverse();

            method.Body.SimplifyMacros();
            int index = entry.hookOnBegin ? 0 : method.Body.Instructions.Count - 1;
            foreach (var instruction in hook) method.Body.Instructions.Insert(index, instruction);
            method.Body.OptimizeMacros();
        }

        static void ProcessModEntry(ModuleDefinition module, ModEntry entry)
        {
            if (entry.pluginTag != "" && (!modPluginList.ContainsKey(entry.pluginTag) || !modPluginList[entry.pluginTag].active))
            {
                //print("  skipping disabled " + entry.entryType.ToString().ToLower() + ": [" + entry.pluginTag + "] " + entry.type + "." + entry.method + "()");
                return;
            }
            else if (entry.entryType == ModEntryType.Patch)
            {
                print("   patch: [" + entry.pluginTag + "] " + entry.type + "." + entry.method + "()");
                List<string> log = new List<string>();
                entry.method_.Invoke(null, new object[] { module, log });
                foreach (var e in log) print("      " + e);
                return;
            }
            else if (entry.entryType != ModEntryType.Hook) return;

            print("   hook: [" + entry.pluginTag + "] [" + (entry.hookOnBegin ? "begin" : "end") + "] " + entry.type2hook + "." + entry.method2hook + " => " + entry.type + "." + entry.method + "()");

            var type = (from TypeDefinition t in module.Types where t.FullName == entry.type2hook select t).FirstOrDefault();
            if (type == null)
            {
                print("      ERROR: Type '" + entry.type2hook + "' not found in assembly!");
                return;
            }

            var method = (from MethodDefinition m in type.Methods where m.Name == entry.method2hook && 
                            (entry.parameterCount == -1 || m.Parameters.Count + m.GenericParameters.Count == entry.parameterCount) select m).FirstOrDefault();
            
            if (method == null) print("      ERROR: '" + entry.type2hook + "." + entry.method2hook + "' not found in assembly!");
            else if (!method.HasBody) print("      ERROR: Can't hook method '" + entry.method2hook + "', method without body!");
            else if (method.HasGenericParameters) print("      ERROR: Can't hook method '" + entry.method2hook + "', generic parameters not supported!");
            else InjectHook(module, method, entry);
        }

        static bool ReadPluginsData(string pluginFile)
        {
            modEntryList = new Dictionary<string, List<ModEntry>>();
            modPluginList = new Dictionary<string, PluginEntry>();

            Assembly pluginAssembly;
            print("[i] read plugin file: '" + pluginFile.Replace(assemblyFolder, ".\\") + "' ...");            
            try
            {
                pluginAssembly = Assembly.LoadFrom(pluginFile); // var hooksAssembly = typeof(GM).Assembly;
            }
            catch (System.Exception ex)
            {
                print("   ERROR: Can't load assembly (" + ex.Message + ")!");
                return(false);
            }
                        
            foreach (Type type in pluginAssembly.GetTypes())
            {
                string pluginTag = "";
                var pluginTagAttributes = Attribute.GetCustomAttributes(type)
                                                    .Where(e => e.GetType().FullName == "dotNetMT.PluginTagAttribute").ToArray();
                if (pluginTagAttributes.Length > 0)
                {
                    pluginTag = type.FullName;
                    var pta = (PluginTagAttribute)pluginTagAttributes[0];
                    if (pta.pluginName != "")
                    {
                        var entry = new PluginEntry
                        {
                            pluginTag = pluginTag,
                            pluginName = pta.pluginName,
                            pluginAuthor = pta.pluginAuthor,
                            pluginNote = pta.pluginNote,
                            pluginRequired = pta.pluginRequired,
                            active = true
                        };
                        modPluginList[pluginTag] = entry;
                    }                    
                }
                
                foreach (MethodInfo method in type.GetMethods())
                {
                    foreach (var pma in Attribute.GetCustomAttributes(method))
                    {
                        ModEntry entry;
                        if (pma.GetType().FullName == "dotNetMT.PluginHookAttribute")
                        {
                            var a = (PluginHookAttribute)pma;
                            entry = new ModEntry
                            {
                                entryType = ModEntryType.Hook,
                                method = method.Name,
                                type = type.Name,
                                method_ = method,
                                type_ = type,
                                assembly = a.assemblyName,
                                type2hook = a.typeName,
                                method2hook = a.methodName,
                                hookOnBegin = a.hookOnBegin,
                                parameterCount = a.parameterCount
                            };
                        }
                        else if (pma.GetType().FullName == "dotNetMT.PluginPatchAttribute")
                        {
                            var a = (PluginPatchAttribute)pma;
                            entry = new ModEntry
                            {
                                entryType = ModEntryType.Patch,
                                method = method.Name,
                                type = type.Name,
                                method_ = method,
                                type_ = type,
                                assembly = a.assemblyName
                            };
                        }
                        else continue;
                        
                        entry.pluginTag = pluginTag;
                        
                        if (!modEntryList.ContainsKey(entry.assembly)) modEntryList.Add(entry.assembly, new List<ModEntry>());
                        modEntryList[entry.assembly].Add(entry);                       
                    }
                }           
            }

            foreach (var k in modPluginList.Keys) modPluginList[k].active = DNMT.Config.Get("plugins", k, true);

            return (true);
        }

        static void ProcessPlugins()
        {
            foreach (var assemblyFileName in modEntryList.Keys)
            {
                print("Processing assembly: '" + assemblyFileName + "' ...");

                if (!File.Exists(assemblyFileName))
                {
                    print("   ERROR: Assembly file not found '" + assemblyFileName + "' !");
                    continue;
                }

                if (File.Exists(assemblyFileName + ".bak"))
                {
                    File.Delete(assemblyFileName);
                    File.Copy(assemblyFileName + ".bak", assemblyFileName);
                }
                else 
                    File.Copy(assemblyFileName, assemblyFileName + ".bak");

                try
                {
                    //using (FileStream fileStream = new FileStream(assemblyFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
                    //{
                    //    AssemblyDefinition scriptAssembly = AssemblyDefinition.ReadAssembly(fileStream);
                    //    scriptAssembly.Write(); //scriptAssembly.Write(assemblyFileName);
                    //}
                    AssemblyDefinition scriptAssembly = AssemblyDefinition.ReadAssembly(assemblyFileName, new ReaderParameters() { ReadWrite = true }); // { InMemory = true }
                    foreach (var entry in modEntryList[assemblyFileName].OrderBy(o => o.entryType).ThenByDescending(o => o.hookOnBegin).ToList()) // OrderByDescending
                        ProcessModEntry(scriptAssembly.MainModule, entry); 
                    scriptAssembly.Write();
                    scriptAssembly.Dispose();
                }
                catch (System.Exception ex)
                {
                    print("   ERROR: Can't process assembly - " + ex.Message);
                    continue;
                }
            }
        }

        static bool BuildPlugins(List<string> log)
        {
            log.Add("[i] rebuild " + modPluginsFilePath.Replace(assemblyFolder, ".\\"));

            if (!Directory.Exists(DNMT.modPluginsFolder))
            {
                log.Add("[e] Plugins source folder not found!");
                return (false);
            }

            var sources = Directory.GetFiles(DNMT.modPluginsFolder, "*.cs", SearchOption.AllDirectories);            
            if (sources.Length == 0)
            {
                log.Add("[e] Plugins sources not found!");
                return (false);
            }

            var references = new List<string>();
            var refs = Directory.GetFiles(DNMT.modPluginsFolder, "*.refs", SearchOption.AllDirectories);
            foreach (var r in refs) 
                references.AddRange(File.ReadAllLines(r));

            references.AddRange(DNMT.Config.Get("references", "default", "")
                .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).Where(p => p.Length > 0).ToList());

            List<string> files = DNMT.Config.Get("references", "extract", "")
                .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).Where(p => p.Length > 0).ToList();

            List<string> exclude = DNMT.Config.Get("references", "exclude", "")
                .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim().ToLower()).Where(p => p.Length > 0).ToList();

            foreach (string file in files)
            {
                Assembly assembly;
                try
                {
                    assembly = Assembly.ReflectionOnlyLoad(File.ReadAllBytes(file));
                }
                catch (Exception ex)
                {
                    log.Add("[w] Can't load assembly for extracting refs - " + ex.Message);
                    continue;
                }
                AssemblyName[] assemblies = assembly.GetReferencedAssemblies();
                foreach (var a in assemblies)
                {
                    if (a.FullName.StartsWith(Path.GetFileNameWithoutExtension(modPluginsFile)))
                        continue;
                    try
                    {
                        //references.Add(Assembly.ReflectionOnlyLoad(a.FullName).Location);
                        UriBuilder uri = new UriBuilder(Assembly.ReflectionOnlyLoad(a.FullName).CodeBase);
                        references.Add(Uri.UnescapeDataString(uri.Path));                        
                    }
                    catch { }
                }
            }

            references = references.Distinct().ToList(); // var noDupes = new HashSet<T>(withDupes);

            var noDupes = new Dictionary<string, string>();
            foreach (var r in references)
            {
                var file = Path.GetFileName(r).ToLower();
                var path = Path.GetDirectoryName(r);
                
                if (exclude.Contains(file))
                    continue;

                if (!noDupes.ContainsKey(file))
                    noDupes[file] = path;
                else
                    if (noDupes[file] == string.Empty || noDupes[file] == null)
                        noDupes[file] = path;
            }

            references = noDupes.Select(x => x.Value.Length > 0 ? x.Value + Path.DirectorySeparatorChar + x.Key : x.Key).ToList();

            if (references.Count == 0)
                log.Add("[w] Plugins reference list is empty!");

            if (File.Exists(modPluginsFilePath))
            {
                File.Delete(modPluginsFilePathBAK);
                File.Move(modPluginsFilePath, modPluginsFilePathBAK);
            }

            var compilerParams = new CompilerParameters
            {
                OutputAssembly = modPluginsFilePath,
                GenerateInMemory = false,
                GenerateExecutable = false,
                TreatWarningsAsErrors = false,
                CompilerOptions = "/optimize /nostdlib+ /utf8output /platform:x86 /nowarn:1701,1702" // /langversion:5
            };
            compilerParams.ReferencedAssemblies.AddRange(references.ToArray());

            var provider = new CSharpCodeProvider();  // new Dictionary<string, string> { { "CompilerVersion", "v3.5" } }
            var compile = provider.CompileAssemblyFromFile(compilerParams, sources); // compile.CompiledAssembly.Location
                        
            if (compile.Errors.HasErrors)
            {
                log.Add("[e] Compile errors:");
                foreach (CompilerError ce in compile.Errors) log.Add("[e]   " + ce);
            }

            if (File.Exists(modPluginsFilePath))
            {
                File.Delete(modPluginsFilePathBAK);
                return (true);
            }
            
            if (File.Exists(modPluginsFilePathBAK)) File.Move(modPluginsFilePathBAK, modPluginsFilePath);
            return (false);
        }

        static void ModInstall(string installFolder)
        {
            print("[i] installing ...");
            if (!Directory.Exists(DNMT.modRootFolder))
            {
                print("   ERROR: Mod root folder not found!");
                return;
            }

            foreach (var k in modPluginList.Keys) DNMT.Config.Set("plugins", k, modPluginList[k].active);
            
            var toCopy = Directory.GetFiles(DNMT.modRootFolder, "*", SearchOption.TopDirectoryOnly).ToList();

            if (!File.Exists(Path.Combine(installFolder, Path.GetFileName(assemblyFile))))
            {
                var file = Path.Combine(assemblyFolder, Path.ChangeExtension(modCoreFile, ".ini"));
                if (File.Exists(file)) toCopy.Add(file);
                toCopy.Add(modCoreFilePath);
            }

            foreach (var file in toCopy)
            {
                string file_ = Path.Combine(installFolder, Path.GetFileName(file));
                print("\t" + file.Replace(assemblyFolder, ".\\") + " => " + file_.Replace(assemblyFolder, ".\\"));
                File.Delete(file_);
                File.Copy(file, file_);
            }

            Directory.SetCurrentDirectory(installFolder);
            ProcessPlugins();
            Directory.SetCurrentDirectory(assemblyFolder);            
        }

        static void ModUninstall(string installFolder)
        {
            print("[i] uninstalling ...");
            if (!Directory.Exists(DNMT.modRootFolder))
            {
                print("   ERROR: Mod root folder not found!");
                return;
            }
            
            print("[i] update mod .ini files ...");
            var toCopy = Directory.GetFiles(DNMT.modRootFolder, "*.ini").ToList();
            foreach (var file in toCopy)
            {
                string file_ = Path.Combine(installFolder, Path.GetFileName(file));
                if (!File.Exists(file_)) continue;
                print("\t" + file_.Replace(assemblyFolder, ".\\") + " => " + file.Replace(assemblyFolder, ".\\"));
                File.Delete(file);
                File.Copy(file_, file);
            }

            print("[i] delete mod files ...");
            var toDelete = Directory.GetFiles(DNMT.modRootFolder, "*").ToList();

            if (!File.Exists(Path.Combine(installFolder, Path.GetFileName(assemblyFile))))
            {
                toDelete.Add(Path.Combine(installFolder, Path.ChangeExtension(modCoreFile, ".ini")));
                toDelete.Add(Path.Combine(installFolder, modCoreFile));
                toDelete.AddRange(Directory.GetFiles(installFolder, "_mod.*.log"));
            }
                                    
            foreach (var file in toDelete)
            {
                string file_ = Path.Combine(installFolder, Path.GetFileName(file));
                if (!File.Exists(file_)) continue;
                print("\t" + file_.Replace(assemblyFolder, ".\\"));
                  File.Delete(file_);
            }

            print("[i] restore originals in '" + installFolder.Replace(assemblyFolder, ".\\") + "' ...");
            var toRestore = Directory.GetFiles(installFolder, "*.bak").ToList();
            foreach (var file in toRestore)
            {
                string file_ = Path.Combine(installFolder, Path.GetFileNameWithoutExtension(file));
                print("\t" + file.Replace(assemblyFolder, ".\\") + " => " + file_.Replace(assemblyFolder, ".\\"));
                File.Delete(file_);
                File.Move(file, file_);                
            }
        }

        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                Console.WindowWidth = 110;
                Console.WindowHeight = 32;
            }
            catch { }

            print(assemblyTitle + " " + version + "\n" + assemblyCopyright + "\n");
            
            Directory.SetCurrentDirectory(assemblyFolder);

            var installFolder = DNMT.Config.Get("main", "path", null);
            if (installFolder == null || !Directory.Exists(installFolder))
            {
                installFolder = assemblyFolder;
                foreach (string folder in Directory.GetDirectories(installFolder, "*_data"))
                    if (Directory.Exists(Path.Combine(folder, "managed")))
                    {
                        installFolder = Path.Combine(folder, "managed");
                        break;
                    }
            }
            installFolder = Path.GetFullPath(installFolder).ToLower();
            //DNMT.Config.Set("main", "path", installFolder);

            var coreFile = Path.Combine(installFolder, modCoreFile);
            var pluginsFile = Path.Combine(installFolder, modPluginsFile);
            if (File.Exists(coreFile) && File.Exists(pluginsFile)) modInstalled = true;
            else
            {
                coreFile = Path.Combine(assemblyFolder, modCoreFile);
                pluginsFile = Path.Combine(assemblyFolder, modPluginsFile);
                if (File.Exists(coreFile) && File.Exists(pluginsFile))
                {
                    installFolder = assemblyFolder;
                    modInstalled = true;
                }                    
            }

            if (modInstalled && !Directory.Exists(DNMT.modRootFolder)) modQuickSetupMode = true;            
            //if (modInstalled && Directory.GetFiles(installFolder, "*.bak").ToList().Count == 0) modQuickSetupMode = true;

            print("[i] path = " + installFolder);
            print("[i] installed = " + modInstalled.ToString());
            print("[i] quick setup mode = " + modQuickSetupMode.ToString());

            do
            {
                if (modQuickSetupMode)
                {
                    Directory.SetCurrentDirectory(installFolder);
                    if (!ReadPluginsData(pluginsFile)) break;
                    ProcessPlugins();
                    break;
                }

                List<string> log = new List<string>();
                BuildPlugins(log);
                foreach (string s in log)
                    print(s);

                if (!File.Exists(modPluginsFilePath)) break;                
                if (!ReadPluginsData(modPluginsFilePath)) break;

                Application.EnableVisualStyles();
                Application.Run(new FrameSetup());

                if (modSetupAction == 0) print("[i] exitting ...");
                else if (modSetupAction == 1)
                    ModInstall(installFolder);
                else if (modSetupAction == 2)
                    ModUninstall(installFolder);

            } while (false);
           
            print("Done!");
            if (isPauseAfterExit) Console.ReadKey(true);
        }
    }
}
