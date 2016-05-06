using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Reflection;

namespace dotNetMT
{
    public class Template
    {
        //private static bool enabled = MOD.Config.Get("modules.Template", true);

        [RuntimeHook("_mod.injector-test.exe", "dotNetMT.Core", "Test0")]
        [RuntimeHook("_mod.injector-test.exe", "dotNetMT.Core", "Test0")]
        [RuntimeHook("_mod.injector-test.exe", "dotNetMT.Core", "Test0", false)]
        [RuntimeHook("_mod.injector-test.exe", "dotNetMT.Core", "Test0", false)]
        public static object Hook(object rv, object obj, params object[] args)
        {
            //if (!enabled) return (null);
            //var this_ = (GalaxyController)obj;

            Console.WriteLine("hook-test");
            return (null);
        }
    }
}
