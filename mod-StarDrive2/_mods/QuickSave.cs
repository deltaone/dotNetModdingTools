using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace dotNetMT
{
    public class QuickSave
    {
        private static bool enabled = MOD.Config.Get("modules.QuickSave", true);

        [RuntimeHook("Assembly-CSharp.dll", "GalaxyController", "Update")]
        public static object Hook(object rv, object obj, params object[] args)
        {
            if (!enabled) return (null);
            var this_ = (GalaxyController)obj;

            if (Event.current.type == EventType.KeyDown && Input.GetKeyDown(KeyCode.F8))
            {                
                GalaxySaveData.SaveGalaxy("Quicksave", new GalaxySaveData());                
                NotificationManager.instance.AddPlainTextNotice("Quicksave done !", true);
            }

            if (Event.current.type == EventType.KeyDown && Input.GetKeyDown(KeyCode.F9))
            {
                ResourceManager.LoadGame("Quicksave");                
                NotificationManager.instance.AddPlainTextNotice("Quickload done !", true);
            }
            
            return (null);
        }
    }
}
