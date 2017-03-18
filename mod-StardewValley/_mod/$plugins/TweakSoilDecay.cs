using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using Mono.Cecil;
using Mono.Cecil.Cil;

using StardewValley;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace dotNetMT
{
    [PluginTag("Tweak-SoilDecay", "de1ta0ne", @"
    |Add 'Adjust soil decay chance' tweak (changed from 0.10 to 0.001)!
    |
    |Note: can be changed in _mod.core.ini under 'Tweak-SoilDecay' section
    |")]

    public class TweakSoilDecay
    {
        [PluginPatch("Stardew Valley.exe")]
        public static void Patch(ModuleDefinition module, List<string> log)
        {
            double chance = DNMT.Config.Get<double>("Tweak-SoilDecay", "chance", 0.001d, true);

            var fn = IL.GetMethodDefinition(IL.GetTypeDefinition(module, "Farm"), "DayUpdate");
            int spot = IL.ScanForOpcodePattern(fn,
                (i, instruction) =>
                {
                    var fieldReference = fn.Body.Instructions[i].Operand as FieldReference;
                    return (fieldReference != null && fieldReference.Name == "random");
                },
                OpCodes.Ldsfld,
                OpCodes.Callvirt,
                OpCodes.Ldc_R8,
                OpCodes.Bgt_Un_S
            );

            if (spot == -1)
            {
                log.Add("Can't find patch location!");
                return;
            }

            //log.Add("spot: " + spot.ToString());
            fn.Body.Instructions[spot + 2].Operand = chance;
        }
    }
}
