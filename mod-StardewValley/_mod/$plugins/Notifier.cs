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
    [PluginTag("Notifier", "de1ta0ne", @"Add 'Notifier' tweak!")]
    public class TweakNotifier
    {
        public static void PlayNotifySound()
        {
            Game1.playSound("phone"); // "fishBite" "FishHit" "crow" "achievement" "questcomplete"
        }

        [PluginPatch("Stardew Valley.exe")]
        public static void PatchBuffUpdate(ModuleDefinition module, List<string> log)
        {
            var target = IL.GetMethodDefinition(module, "Buff", "update");
            if (target == null)
            {
                log.Add("Can't find target method!");
                return;
            }

            int spot = IL.ScanForOpcodePattern(target, (i, instruction) =>
            {
                var reference = target.Body.Instructions[i + 0].Operand as FieldReference;
                return (reference != null && reference.Name == "millisecondsDuration");
            },
                OpCodes.Ldfld,
                OpCodes.Ldc_I4_0,
                OpCodes.Bgt_S,
                OpCodes.Ldc_I4_1,
                OpCodes.Ret
            );
            if (spot == -1)
            {
                log.Add("Can't find patch location!");
                return;
            }

            var type = module.ImportReference(MethodBase.GetCurrentMethod().DeclaringType).Resolve(); // typeof(dotNetMT.EasyPlant)
            var method = module.ImportReference(IL.GetMethodDefinition(type, "PlayNotifySound"));
            IL.MethodAppend(target, spot + 3, 0, new[] { Instruction.Create(OpCodes.Call, method) });
        }
        
        [PluginPatch("Stardew Valley.exe")]
        public static void PatchFishingRodDoneFishing(ModuleDefinition module, List<string> log)
        {
            var target = IL.GetMethodDefinition(module, "FishingRod", "doneFishing");
            if (target == null)
            {
                log.Add("Can't find target method!");
                return;
            }

            var type = module.ImportReference(MethodBase.GetCurrentMethod().DeclaringType).Resolve(); // typeof(dotNetMT.EasyPlant)
            var method = module.ImportReference(IL.GetMethodDefinition(type, "PlayNotifySound"));


            int spot;
            
            spot = IL.ScanForOpcodePattern(target, (i, instruction) =>
            {
                var reference = target.Body.Instructions[i + 1].Operand as FieldReference;
                return (reference != null && reference.Name == "attachments");
            },
                OpCodes.Ldarg_0,
                OpCodes.Ldfld,
                OpCodes.Ldc_I4_0,
                OpCodes.Ldnull
            );
            if (spot == -1)
            {
                log.Add("Can't find patch location!");
                return;
            }

            IL.MethodAppend(target, spot + 0, 0, new[] { Instruction.Create(OpCodes.Call, method) });
            
            spot = IL.ScanForOpcodePattern(target, (i, instruction) =>
            {
                var reference = target.Body.Instructions[i + 1].Operand as FieldReference;
                return (reference != null && reference.Name == "attachments");
            },
                OpCodes.Ldarg_0,
                OpCodes.Ldfld,
                OpCodes.Ldc_I4_1,
                OpCodes.Ldnull
            );
            if (spot == -1)
            {
                log.Add("Can't find patch location!");
                return;
            }

            IL.MethodAppend(target, spot + 0, 0, new[] { Instruction.Create(OpCodes.Call, method) });
        }
        //-------------------------------------------------------------------------------------------------------------
    }
}
