using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace dotNetMT
{
    public class ChangedTraits
    {
        private static bool enabled = MOD.Config.Get("modules.ChangedTraits", true);
        private static bool intercept = true;
        
        [RuntimeHook("Assembly-CSharp.dll", "RaceSelectionMenu", "Start")]
        public static object Hook(object rv, object obj, params object[] args)
        {
            if (!enabled) return (null);
            var this_ = (RaceSelectionMenu)obj;
            
            if (!intercept) return (null);
            intercept = false;
            
            MethodInfo method = this_.GetType().GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(this_, new object[] { });

            // this_.TraitsDict["cyber"].cost = "7";            
            var cfg = new ConfigFile(MOD.assemblyFolder + "_mod.ChangedTraits.ini");         
            var keys = cfg.GetKeys("traits");
            foreach (var k in keys) this_.TraitsDict[k].cost = cfg.Get("traits." + k, "1");

            return (1);
        }
    }
}
