using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace dotNetMT
{
    public class CheatExploreGalaxy
    {
        private static bool enabled = MOD.Config.Get("modules.CheatExploreGalaxy", true);

        [RuntimeHook("Assembly-CSharp.dll", "GalaxyController", "Update")]
        public static object Hook(object rv, object obj, params object[] args)
        {
            if (!enabled) return (null);
            var this_ = (GalaxyController)obj;

            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyUp(KeyCode.Keypad0))
            {
                foreach (KeyValuePair<Vector2, StrategyTile> keyValuePair in this_.TilesDict)
                {
                    keyValuePair.Value.SetExploredByPlayer();
                    keyValuePair.Value.SetMode(TileMode.ExploredSensors);
                }

                foreach (GameObject solarSystemList in this_.SolarSystemList)
                {
                    solarSystemList.GetComponent<SolarSystem>().ExploredDict[this_.PlayerEmpire] = true;
                }

                NotificationManager.instance.AddPlainTextNotice("Systems explored !", true);
            }

            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyUp(KeyCode.Keypad1))
            {
                foreach (KeyValuePair<string, Empire> empireDict in GalaxyController.EmpireDict)
                {
                    if (empireDict.Value.RelationsDict == null)
                    {
                        continue;
                    }
                    foreach (KeyValuePair<string, Relationship> relationsDict in empireDict.Value.RelationsDict)
                    {
                        relationsDict.Value.Known = true;
                    }
                }

                NotificationManager.instance.AddPlainTextNotice("All races explored !", true);
            }

            return (null);
        }
    }
}
