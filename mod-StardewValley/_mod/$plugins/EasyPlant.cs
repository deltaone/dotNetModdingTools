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
    [PluginTag("EasyPlant", "d1", @"
    |Add 'Easy plant' tweak!
    | 
    |   Note: work only with new planted crops!")]
    public class EasyPlant
    {
        public static void ProcessPlant1x1(StardewValley.GameLocation gameLocation, int x, int y, StardewValley.Farmer who)
        {
            xTile.Dimensions.Location tileLocation = new xTile.Dimensions.Location(x, y);
            Vector2 vector = new Vector2((float)tileLocation.X, (float)tileLocation.Y);

            if (gameLocation.terrainFeatures.ContainsKey(vector)
                && gameLocation.terrainFeatures[vector].GetType() == typeof(StardewValley.TerrainFeatures.HoeDirt)
                && who.ActiveObject != null && (who.ActiveObject.Category == -74 || who.ActiveObject.Category == -19)
                && ((StardewValley.TerrainFeatures.HoeDirt)gameLocation.terrainFeatures[vector]).canPlantThisSeedHere(who.ActiveObject.ParentSheetIndex,
                                tileLocation.X, tileLocation.Y, who.ActiveObject.Category == -19))
            {
                if (((StardewValley.TerrainFeatures.HoeDirt)gameLocation.terrainFeatures[vector]).plant(
                    who.ActiveObject.ParentSheetIndex, tileLocation.X, tileLocation.Y, who, who.ActiveObject.Category == -19) && who.IsMainPlayer)
                {
                    who.reduceActiveItemByOne();
                }
            }
        }

        public static void ProcessPlant(StardewValley.GameLocation gameLocation, xTile.Dimensions.Location tileLocation, StardewValley.Farmer who)
        {
            //for (int x = who.getTileX() - 1; x <= who.getTileX() + 1; x++)
            //{
            //    for (int y = who.getTileY() - 1; y <= who.getTileY() + 1; y++)
            //    {
            //        ProcessPlant1x1(gameLocation, x, y, who);
            //    }
            //}
            for (int x = tileLocation.X - 1; x <= tileLocation.X + 1; x++)
            {
                for (int y = tileLocation.Y - 1; y <= tileLocation.Y + 1; y++)
                {
                    if (tileLocation.X == x && tileLocation.Y == y)
                        continue;
                    ProcessPlant1x1(gameLocation, x, y, who);
                }
            }
        }

        [PluginPatch("Stardew Valley.exe")]
        public static void Patch(ModuleDefinition module, List<string> log)
        {
            var fn = IL.GetMethodDefinition(IL.GetTypeDefinition(module, "GameLocation"), "checkAction");
            int spot = IL.ScanForOpcodePattern(fn,
                (i, instruction) =>
                {
                    var reference = fn.Body.Instructions[i + 3].Operand as FieldReference;
                    return (reference != null && reference.Name == "haltAfterCheck");
                },
                OpCodes.Ldarg_3,
                OpCodes.Callvirt,
                OpCodes.Ldc_I4_0,
                OpCodes.Stsfld
            );
            if (spot == -1)
            {
                log.Add("Can't find patch location!");
                return;
            }

            spot += 4;
            var callType = module.Import(typeof(dotNetMT.EasyPlant)).Resolve();
            var callMethod = module.Import(IL.GetMethodDefinition(callType, "ProcessPlant"));
           
            IL.MethodAppend(fn, spot, 0, new[]
                {
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldarg_S, fn.Parameters.FirstOrDefault(def => def.Name == "tileLocation")),
                    Instruction.Create(OpCodes.Ldarg_S, fn.Parameters.FirstOrDefault(def => def.Name == "who")),
                    Instruction.Create(OpCodes.Call, callMethod),
                });
        }
    }
}