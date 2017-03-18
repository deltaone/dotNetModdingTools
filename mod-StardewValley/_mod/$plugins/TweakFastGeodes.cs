using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using Mono.Cecil;
using Mono.Cecil.Cil;

using StardewValley;
using StardewValley.Menus;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace dotNetMT
{
    [PluginTag("Tweak-FastGeodes", "de1ta0ne", @"Add 'Fast open geode' tweak!")]
    public class TweakFastGeodes
    {
        public static List<FarmerSprite.AnimationFrame> Adjust(List<FarmerSprite.AnimationFrame> frames, GeodeMenu menu)
        {
            menu.geodeAnimationTimer = 50;
            for (int i = 0; i < frames.Count; i++)
            {   // http://stackoverflow.com/questions/414981/directly-modifying-listt-elements
                var frame = frames[i];
                frame.milliseconds = 50;
                frames[i] = frame;
                menu.geodeAnimationTimer += 50; 
            }

            return (frames);
        }       

        [PluginPatch("Stardew Valley.exe")]
        public static void Patch(ModuleDefinition module, List<string> log)
        {
            var target = IL.GetMethodDefinition(module, "GeodeMenu", "receiveLeftClick"); 
            if (target == null)
            {
                log.Add("Can't find target method!");
                return;
            }
			
			// ---
			
            int spot = IL.ScanForOpcodePattern(target, (i, instruction) =>
                {
                    var reference = target.Body.Instructions[i + 3].Operand as FieldReference;
                    return (reference != null && reference.Name == "clint");
                },
                OpCodes.Callvirt,
                OpCodes.Callvirt,
                OpCodes.Ldarg_0,
                OpCodes.Ldfld
            );
            if (spot == -1)
            {
                log.Add("Can't find patch location!");
                return;            
			}
			
            // ---
			
            var type = module.ImportReference(MethodBase.GetCurrentMethod().DeclaringType).Resolve(); // typeof(dotNetMT.EasyPlant)
            var method = module.ImportReference(IL.GetMethodDefinition(type, "Adjust"));
            IL.MethodAppend(target, spot + 1, 0, new[] {
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Call, method),
                });            
        }
        //-------------------------------------------------------------------------------------------------------------
    }
}
