using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Reflection;

using Mono.Cecil;

[assembly: AssemblyTitle("_mod.plugins")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

namespace dotNetMT
{
    public class Template0
    {
        [PluginPatch("_mod.test.exe")]
        public static void Patch(ModuleDefinition module, List<string> log)
        {
            log.Add("plugins without PluginTag - always enabled and not showed in selection list!");
        }	
    }

    [PluginTag("Test plugin 1", "de1ta0ne-1", @"
        |plugin description line 1 [1]
        |   plugin description line 2 [1]
        |   plugin description line 3 [1]
    ")]
    public class Template1
    {
        [PluginHook("_mod.test.exe", "dotNetMT.StructTest", "Test0")]
        [PluginHook("_mod.test.exe", "dotNetMT.StructTest", "Test0", false)]
        [PluginHook("_mod.test.exe", "dotNetMT.StructTest", "Test0", false)]
        public static object Hook(object rv, object obj, params object[] args)
        {
			// rv - return value of hooked function when hookOnBegin = false, if hookOnBegin is true, then rv = null
			// obj - hooked function class reference
			// args - arguments of hooked function		

            // var this_ = (GalaxyController)obj;
            Console.WriteLine("hook-test");

            // if return (null) - continue executing hooked function
            // if return (object) - immediately return object from hooked function
            return (null);
        }

        [PluginPatch("_mod.test.exe")]
        public static void Patch(ModuleDefinition module, List<string> log)
        {
            log.Add("patches runing on inject stage - modify assembly as you wish!");
        }
    }

    [PluginTag("Test plugin 2", "de1ta0ne-2",
        "plugin description line 1 [2]\r\nplugin description line 2 [2]\r\ntested on: v1.03 [2]", 
        "dotNetMT.Template1")]
    // last argument is tag of required plugin (namespace.class)
    // can't enable plugin in selection window if required plugin disabled
    // when disable plugin in selection window, all dependent plugins will also be disabled
    public class Template2
    {
    }

    [PluginTag("Test plugin 3", "de1ta0ne-3", "plugin description line 1 [3]\r\nplugin description line 2 [3]\r\ntested on: v1.03 [3]")]
    public class Template3: PluginBase
    {
        public override void OnInitialize()
        {
            Console.WriteLine("OnInitialize from event system!");

            HotkeyCore.RegisterHotkey(() =>
            {
                CORE.Print("HEEEEELLLLLOPOO!");
            }, Keys.E);
        }
    }
}
