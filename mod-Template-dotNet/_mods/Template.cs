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
        private static bool enabled = MOD.Config.Get("modules.Template", true);

        // [RuntimeHook("Assembly-CSharp.dll", "GalaxyController", "Update")]
        public static object Hook(RuntimeMethodHandle rmh, object obj, params object[] args)
        {
            if (!enabled) return (null);
            //var this_ = (GalaxyController)obj;
            
            return (null);
        }
    }
}
