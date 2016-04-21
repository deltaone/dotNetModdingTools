using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace dotNetMT
{
    public class CheatDestroyUnit
    {
        private static bool enabled = MOD.Config.Get("modules.CheatDestroyUnit", true);

        [RuntimeHook("Assembly-CSharp.dll", "StrategicGUI", "Update")]
        public static object DestroyFleet(RuntimeMethodHandle rmh, object obj, params object[] args)
        {
            if (!enabled) return (null);
            var this_ = (StrategicGUI)obj;

            if (this_.SelectedFleet != null && Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyUp(KeyCode.KeypadMinus))
            {
                this_.DestroyFleetStrategyMap(this_.SelectedFleet, false);
                NotificationManager.instance.AddPlainTextNotice("Fleet destroyed !", true);
            }
			
            return (null);
        }

        [RuntimeHook("Assembly-CSharp.dll", "GameController", "Update")]
        public static object DestroyGroundCombatant(RuntimeMethodHandle rmh, object obj, params object[] args)
        {
            if (!enabled) return (null);
            var this_ = (GameController)obj;

            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyUp(KeyCode.KeypadMinus))
            {
                this_.SelectedUnit.GetComponent<Combatant>().OnCombatantDeath(null);
                NotificationManager.instance.AddPlainTextNotice("Combatant destroyed !", true);
            }
			
            return (null);
        }
		
        [RuntimeHook("Assembly-CSharp.dll", "ArenaController", "Update")]
        public static object DestroyArenaShip(RuntimeMethodHandle rmh, object obj, params object[] args)
        {
            if (!enabled) return (null);
            var this_ = (ArenaController)obj;
			
            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyUp(KeyCode.KeypadMinus) && this_.SelectedUnits.Count > 0)
            {
                if (this_.SelectedUnits[0].GetComponent<FighterWing>() != null)
                {
                    this_.SelectedUnits[0].GetComponent<FighterWing>().members[0].GetComponent<LiveShipData>().Die(null, null);
                }
                else
                {
                    this_.SelectedUnits[0].GetComponent<LiveShipData>().Die(null, null);
                }                
            }           
			
            return (null);
        }		
    }
}
