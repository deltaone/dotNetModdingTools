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
    [PluginTag("Tweak-DayLength", "d1", "Add 'Increased day length' tweak (changed to x2)!")]
    public class TweakDayLength
    {
        [PluginPatch("Stardew Valley.exe")]
        public static void Patch(ModuleDefinition module, List<string> log)
        {
            float multiplier = DNMT.Config.Get<float>("Tweak-DayLength", "multiplier", 2f, true);

            var fn = IL.GetMethodDefinition(IL.GetTypeDefinition(module, "Game1"), "UpdateGameClock");
            int spot = IL.ScanForOpcodePattern(fn,
                (i, instruction) =>
                {
                    var fieldReference = fn.Body.Instructions[i].Operand as FieldReference;
                    return (fieldReference != null && fieldReference.Name == "gameTimeInterval");
                },
                OpCodes.Ldsfld,
                OpCodes.Ldc_I4,
                OpCodes.Ldsfld
            );

            if (spot == -1)
            {
                log.Add("Can't find patch location!");
                return;
            }

            fn.Body.Instructions[spot + 1].Operand = (int)(7000 * multiplier);
        }
    }
}
