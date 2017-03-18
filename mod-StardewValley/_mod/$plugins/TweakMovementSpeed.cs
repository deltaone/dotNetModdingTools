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
    [PluginTag("Tweak-MovementSpeed", "de1ta0ne", @"
    |Add 'Movement speed multiplier' tweak (default 0.066f / changed to 0.099f)!
    |
    |Note: can be changed in _mod.core.ini under 'Tweak-MovementSpeed' section
    |")]

    public class TweakMovementSpeed
    {
        [PluginPatch("Stardew Valley.exe")]
        public static void Patch(ModuleDefinition module, List<string> log)
        {
            float multiplier = DNMT.Config.Get<float>("Tweak-MovementSpeed", "multiplier", 0.099f, true);

            var fn = IL.GetMethodDefinition(IL.GetTypeDefinition(module, "Farmer"), "getMovementSpeed");
            int spot = IL.ScanForOpcodePattern(fn,
                (i, instruction) =>
                {
                    var fieldReference = fn.Body.Instructions[i + 2].Operand as FieldReference;
                    return (fieldReference != null && fieldReference.Name == "movementMultiplier");
                },
                OpCodes.Ldarg_0,
                OpCodes.Ldc_R4,
                OpCodes.Stfld
            );

            if (spot == -1)
            {
                log.Add("Can't find patch location!");
                return;
            }

            fn.Body.Instructions[spot + 1].Operand = multiplier;
        }
    }
}