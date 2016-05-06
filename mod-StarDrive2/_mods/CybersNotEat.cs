using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace dotNetMT
{
    public class CybersNotEat
    {
        private static bool enabled = MOD.Config.Get("modules.CybersNotEat", true);

        [RuntimeHook("Assembly-CSharp.dll", "Planet", "CalculateFoodConsumption")]
        public static object Hook(object rv, object obj, params object[] args)
        {
            if (!enabled) return (null);
            var this_ = (Planet)obj;
			
            float num = 0f;
            foreach (EmpireData current in this_.CitizensByRace.Keys)
            {
                if (!current.Cybernetic)
                {
                    if (current.FoodConsumption > 0f)
                    {
                        num += current.FoodConsumption * (float)this_.CitizensByRace[current];
                    }
                }
            }
            return (num);
        }
    }
}
