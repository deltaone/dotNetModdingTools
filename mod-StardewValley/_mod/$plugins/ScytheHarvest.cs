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
    [PluginTag("ScytheHarvest", "d1", @"Add scythe harvest ...")]
    public class ScytheHarvest
    {
        private static int[] _useScytheWith = new int[] { 
			499, // Ancient Seeds
			745, // Strawberry Seeds
			478, // Rhubarb Seeds
			481, // Blueberry Seeds
			486, // Starfruit Seeds
			493, // Cranberry Seeds
			490, // Pumpkin Seeds
        };

        public static int AdjustHarvestMethod(int defaultMethod, int seedIndex)
        {
            if (Array.IndexOf(_useScytheWith, seedIndex) > -1)
                return (1);
            return (defaultMethod);
        }

        public static void ProcessScytheHarvest(Crop crop, int xTile, int yTile,  
            StardewValley.TerrainFeatures.HoeDirt soil, StardewValley.Characters.JunimoHarvester junimoHarvester)
        {
            Random random = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame);

            // extra luck fix - num *= 2;  
            if (random.NextDouble() < (double)((float)Game1.player.LuckLevel / 1500f) + Game1.dailyLuck / 1200.0 + 9.9999997473787516E-05)
            {                 
                int num = random.Next(1, 3);
                for (int i = 0; i < num; i++)
                {
                    if (junimoHarvester == null)
                        Game1.createObjectDebris(crop.indexOfHarvest, xTile, yTile, -1, random.Next(0, 2), 1f, null);
                    else
                        junimoHarvester.tryToAddItemToHut(new StardewValley.Object(crop.indexOfHarvest, 1, false, -1, 0));
                }
                if (junimoHarvester == null || Utility.isOnScreen(junimoHarvester.getTileLocationPoint(), Game1.tileSize, junimoHarvester.currentLocation))
                    Game1.playSound("dwoop");
            }

            // experience fix
            if (junimoHarvester == null)
            {
                int experienceBase = Convert.ToInt32(Game1.objectInformation[crop.indexOfHarvest].Split(new char[] { '/' })[1]);
                float experience = (float)(16.0 * Math.Log(0.018 * (double)experienceBase + 1.0, 2.7182818284590451));
                Game1.player.gainExperience(0, (int)Math.Round((double)experience));
            }

            // sunflower seeds fix
            if (crop.indexOfHarvest == 421)
            {
                int indexOfHarvest = 431;
                int num = random.Next(1, 3);
                for (int i = 0; i < num; i++)
                {
                    if (junimoHarvester == null)
                        Game1.createObjectDebris(indexOfHarvest, xTile, yTile, -1, 0, 1f, null);
                    else
                        junimoHarvester.tryToAddItemToHut(new StardewValley.Object(indexOfHarvest, 1, false, -1, 0));
                }
            }
        }

        [PluginPatch("Stardew Valley.exe")]
        public static void Patch(ModuleDefinition module, List<string> log)
        {
            var fn = IL.GetMethodDefinition(IL.GetTypeDefinition(module, "Crop"), ".ctor", 3);
            int spot = IL.ScanForOpcodePattern(fn,
                (i, instruction) =>
                {
                    var fieldReference = fn.Body.Instructions[i + 2].Operand as FieldReference;
                    return (fieldReference != null && fieldReference.Name == "harvestMethod");
                },
                OpCodes.Ldelem_Ref,
                OpCodes.Call,
                OpCodes.Stfld
            );
            if (spot == -1)
            {
                log.Add("Can't find patch location!");
                return;
            }

            //spot -= 2;
            //fn.Body.Instructions[spot + 0].OpCode = OpCodes.Nop;
            //fn.Body.Instructions[spot + 1].OpCode = OpCodes.Ldc_I4_1;
            //fn.Body.Instructions[spot + 2].OpCode = OpCodes.Nop;
            //fn.Body.Instructions[spot + 3].OpCode = OpCodes.Nop;

            spot += 2;
            var callType = module.Import(typeof(dotNetMT.ScytheHarvest)).Resolve();
            var callMethod = module.Import(IL.GetMethodDefinition(callType, "AdjustHarvestMethod"));
            IL.MethodAppend(fn, spot, 0, new[]
                {
                    Instruction.Create(OpCodes.Ldarg_1),
                    Instruction.Create(OpCodes.Call, callMethod),
                });

            //---------------------------------------------------------------------------------------------------------

            fn = IL.GetMethodDefinition(IL.GetTypeDefinition(module, "Crop"), "harvest");
            spot = IL.ScanForOpcodePattern(fn,
                (i, instruction) =>
                {
                    var fieldReference = fn.Body.Instructions[i + 1].Operand as FieldReference;
                    return (fieldReference != null && fieldReference.Name == "harvestMethod");
                },
                OpCodes.Ldarg_0,
                OpCodes.Ldfld,
                OpCodes.Ldc_I4_1,
                OpCodes.Bne_Un
            );            
            if (spot == -1)
            {
                log.Add("Can't find patch location!");
                return;
            }

            spot += 4;
            callMethod = module.Import(IL.GetMethodDefinition(callType, "ProcessScytheHarvest"));
            IL.MethodAppend(fn, spot, 0, new[]
                {
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldarg_S, fn.Parameters.FirstOrDefault(def => def.Name == "xTile")),
                    Instruction.Create(OpCodes.Ldarg_S, fn.Parameters.FirstOrDefault(def => def.Name == "yTile")),
                    //Instruction.Create(OpCodes.Ldloc_S, fn.Body.Variables[4]),
                    Instruction.Create(OpCodes.Ldarg_S, fn.Parameters.FirstOrDefault(def => def.Name == "soil")),
                    Instruction.Create(OpCodes.Ldarg_S, fn.Parameters.FirstOrDefault(def => def.Name == "junimoHarvester")),
                    Instruction.Create(OpCodes.Call, callMethod),
                    //Instruction.Create(OpCodes.Stloc_S, fn.Body.Variables[4]),
                });
            
            //---------------------------------------------------------------------------------------------------------
        }
    }
}