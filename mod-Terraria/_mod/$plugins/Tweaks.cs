using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Reflection;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Terraria;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace dotNetMT
{
    [PluginTag("Tweak-Invulnerable", "TerrariaTweaker", "Add 'Invulnerable' tweak!")]
    public class TweakInvulnerable
    {
        [PluginPatch("Terraria.exe")]
        public static void Patch(ModuleDefinition module, List<string> log)
        {           
            var spot = IL.GetMethodDefinition(IL.GetTypeDefinition(module, "Player"), "Hurt");

            spot.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Ret));
            spot.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Ldc_R8, 0.0));
        }
    }

    [PluginTag("Tweak-Invincible", "TerrariaTweaker", "Add 'Invincible' tweak!")]
    public class TweakInvincible
    {
        [PluginPatch("Terraria.exe")]
        public static void Patch(ModuleDefinition module, List<string> log)
        {
            var spot = IL.GetMethodDefinition(IL.GetTypeDefinition(module, "Player"), "KillMe", -1);
            spot.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Ret));
        }
    }

    [PluginTag("Tweak-OneHitKill", "TerrariaPatcher", "Add 'OneHitKill' tweak!")]
    public class TweakOneHitKill
    {
        [PluginPatch("Terraria.exe")]
        public static void Patch(ModuleDefinition module, List<string> log)
        {
            var npc = IL.GetTypeDefinition(module, "NPC");
            var strikeNPC = IL.GetMethodDefinition(npc, "StrikeNPC");

            int spot = IL.ScanForOpcodePattern(strikeNPC,
                OpCodes.Ldarg_1,
                OpCodes.Conv_R8,
                OpCodes.Stloc_1);

            var life = IL.GetFieldDefinition(npc, "life");
            strikeNPC.Body.Instructions[spot].OpCode = OpCodes.Ldarg_0;
            strikeNPC.Body.Instructions.Insert(spot + 1, Instruction.Create(OpCodes.Ldfld, life));

            int spot2 = IL.ScanForOpcodePattern(strikeNPC,
                (i, instruction) =>
                {
                    var i0 = strikeNPC.Body.Instructions[i].Operand as ParameterReference;
                    return i0 != null && i0.Name == "crit";
                },
                spot,
                OpCodes.Ldarg_S,
                OpCodes.Brfalse_S);

            for (int i = spot + 4; i < spot2; i++)
                strikeNPC.Body.Instructions[i].OpCode = OpCodes.Nop;
        }
    }

    [PluginTag("Tweak-InfiniteAmmo", "TerrariaPatcher", "Add 'InfiniteAmmo' tweak!")]
    public class TweakInfiniteAmmo
    {
        [PluginPatch("Terraria.exe")]
        public static void Patch(ModuleDefinition module, List<string> log)
        {
            var player = IL.GetTypeDefinition(module, "Player");
            var pickAmmo = IL.GetMethodDefinition(player, "PickAmmo");

            int spot = IL.ScanForOpcodePattern(pickAmmo,
                                                (i, instruction) =>
                                                {
                                                    var i1 = instruction.Operand as FieldReference;
                                                    return i1 != null && i1.Name == "stack";
                                                },
                                                OpCodes.Ldfld,
                                                OpCodes.Ldc_I4_1,
                                                OpCodes.Sub,
                                                OpCodes.Stfld);

            pickAmmo.Body.Instructions[spot + 1].OpCode = OpCodes.Ldc_I4_0;
        }
    }

    [PluginTag("Tweak-RemoveManaCost", "TerrariaPatcher", "Add 'RemoveManaCost' tweak!")]
    public class TweakRemoveManaCost
    {
        [PluginPatch("Terraria.exe")]
        public static void Patch(ModuleDefinition module, List<string> log)
        {
            var player = IL.GetTypeDefinition(module, "Player");
            var itemCheck = IL.GetMethodDefinition(player, "ItemCheck");
            var checkMana = IL.GetMethodDefinition(player, "CheckMana");

            int spot = IL.ScanForOpcodePattern(itemCheck, (i, instruction) =>
            {
                var in1 = itemCheck.Body.Instructions[i - 1].OpCode;
                return in1.Name.ToLower().Contains("ldloc") && itemCheck.Body.Instructions[i + 1].Operand as sbyte? == 127;
            },
                OpCodes.Ldfld,
                OpCodes.Ldc_I4_S,
                OpCodes.Bne_Un_S);

            for (int i = -1; i < 5; i++)
                itemCheck.Body.Instructions[spot + i].OpCode = OpCodes.Nop;
            itemCheck.Body.Instructions[spot + 5].OpCode = OpCodes.Br;

            checkMana.Body.ExceptionHandlers.Clear();
            checkMana.Body.Instructions.Clear();
            checkMana.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4_1));
            checkMana.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        }
    }

    [PluginTag("Tweak-RemoveDrowning", "TerrariaPatcher", "Add 'RemoveDrowning' tweak!")]
    public class TweakRemoveDrowning
    {
        [PluginPatch("Terraria.exe")]
        public static void Patch(ModuleDefinition module, List<string> log)
        {
            var player = IL.GetTypeDefinition(module, "Player");
            var checkDrowning = IL.GetMethodDefinition(player, "CheckDrowning");
            checkDrowning.Body.ExceptionHandlers.Clear();
            checkDrowning.Body.Instructions.Clear();
            checkDrowning.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        }
    }

    [PluginTag("Tweak-RemoveDiscordBuff", "TerrariaPatcher", "Add 'RemoveDiscordBuff' tweak!")]
    public class TweakRemoveDiscordBuff
    {
        [PluginPatch("Terraria.exe")]
        public static void Patch(ModuleDefinition module, List<string> log)
        {
            var player = IL.GetTypeDefinition(module, "Player");
            var itemCheck = IL.GetMethodDefinition(player, "ItemCheck");

            int spot = IL.ScanForOpcodePattern(itemCheck, (i, instruction) =>
            {
                var fieldReference = instruction.Operand as FieldReference;
                return fieldReference != null && fieldReference.Name == "chaosState";
            },
                OpCodes.Ldfld,
                OpCodes.Brfalse);

            var target = itemCheck.Body.Instructions[spot + 1].Operand as Instruction;
            bool done = false;
            for (; !done; target = target.Next)
            {
                if (target.OpCode == OpCodes.Call) done = true;

                target.OpCode = OpCodes.Nop;
                target.Operand = null;
            }

            int spot2 = IL.ScanForOpcodePattern(itemCheck, (i, instruction) =>
            {
                var methodReference = instruction.Operand as MethodReference;
                return methodReference != null && methodReference.Name == "SolidCollision";
            },
                OpCodes.Call,
                OpCodes.Brtrue);

            itemCheck.Body.Instructions[spot2 + 1].OpCode = OpCodes.Pop;
        }
    }

    [PluginTag("Tweak-RemovePotionSickness", "TerrariaPatcher", "Add 'RemovePotionSickness' tweak!")]
    public class TweakRemovePotionSickness
    {
        [PluginPatch("Terraria.exe")]
        public static void Patch(ModuleDefinition module, List<string> log)
        {
            var player = IL.GetTypeDefinition(module, "Player");
            var quickHeal = IL.GetMethodDefinition(player, "QuickHeal");
            var quickMana = IL.GetMethodDefinition(player, "QuickMana");
            var itemCheck = IL.GetMethodDefinition(player, "ItemCheck"); // regular potion usage

            // quick heal
            int spot1 = IL.ScanForOpcodePattern(quickHeal,
                (i, instruction) =>
                {
                    var i1 = quickHeal.Body.Instructions[i + 1].Operand as FieldReference;
                    return i1 != null && i1.Name == "potion";
                },
                OpCodes.Ldloc_0,
                OpCodes.Ldfld,
                OpCodes.Brfalse_S);

            for (int i = 0; i < 2; i++)
                quickHeal.Body.Instructions[spot1 + i].OpCode = OpCodes.Nop;
            quickHeal.Body.Instructions[spot1 + 2].OpCode = OpCodes.Br_S;

            // quick mana
            int spot2 = IL.ScanForOpcodePattern(quickMana,
                (i, instruction) =>
                {
                    var i4 = quickMana.Body.Instructions[i + 4].Operand as FieldReference;
                    return i4 != null && i4.Name == "potion";
                },
                OpCodes.Ldarg_0,
                OpCodes.Ldfld,
                OpCodes.Ldloc_0,
                OpCodes.Ldelem_Ref,
                OpCodes.Ldfld,
                OpCodes.Brfalse_S);

            for (int i = 0; i < 5; i++)
                quickMana.Body.Instructions[spot2 + i].OpCode = OpCodes.Nop;
            quickMana.Body.Instructions[spot2 + 5].OpCode = OpCodes.Br_S;

            // health/mana
            int spot3 = IL.ScanForOpcodePattern(itemCheck,
                                               (i, instruction) =>
                                               {
                                                   var i2 = itemCheck.Body.Instructions[i + 2].Operand as FieldReference;
                                                   return i2 != null && i2.Name == "potionDelayTime";
                                               },
                                               OpCodes.Ldarg_0,
                                               OpCodes.Ldarg_0,
                                               OpCodes.Ldfld,
                                               OpCodes.Stfld);

            for (int i = 0; i < 10; i++)
                itemCheck.Body.Instructions[spot3 + i].OpCode = OpCodes.Nop;

            // rejuv
            int spot4 = IL.ScanForOpcodePattern(itemCheck,
                                               (i, instruction) =>
                                               {
                                                   var i2 = itemCheck.Body.Instructions[i + 2].Operand as FieldReference;
                                                   return i2 != null && i2.Name == "restorationDelayTime";
                                               },
                                               OpCodes.Ldarg_0,
                                               OpCodes.Ldarg_0,
                                               OpCodes.Ldfld,
                                               OpCodes.Stfld);

            for (int i = 0; i < 10; i++)
                itemCheck.Body.Instructions[spot4 + i].OpCode = OpCodes.Nop;
        }
    }

    [PluginTag("Tweak-InfiniteCloudJumps", "TerrariaPatcher", "Add 'InfiniteCloudJumps' tweak!")]
    public class TweakInfiniteCloudJumps
    {
        [PluginPatch("Terraria.exe")]
        public static void Patch(ModuleDefinition module, List<string> log)
        {
            var player = IL.GetTypeDefinition(module, "Player");
            var update = IL.GetMethodDefinition(player, "Update");
            var jumpAgain1 = IL.GetFieldDefinition(player, "jumpAgainBlizzard");
            var jumpAgain2 = IL.GetFieldDefinition(player, "jumpAgainCloud");
            var jumpAgain3 = IL.GetFieldDefinition(player, "jumpAgainFart");
            var jumpAgain4 = IL.GetFieldDefinition(player, "jumpAgainSail");
            var jumpAgain5 = IL.GetFieldDefinition(player, "jumpAgainSandstorm");
            var jumpAgain6 = IL.GetFieldDefinition(player, "jumpAgainUnicorn");

            int spot = IL.ScanForOpcodePattern(update,
                                               (i, instruction) =>
                                               {
                                                   var i0 = update.Body.Instructions[i + 1].Operand as FieldReference;
                                                   return i0 != null && i0.Name == "doubleJumpCloud";
                                               },
                                               OpCodes.Ldc_I4_0,
                                               OpCodes.Stfld);

            update.Body.Instructions.Insert(spot + 2, Instruction.Create(OpCodes.Ldarg_0));
            update.Body.Instructions.Insert(spot + 3, Instruction.Create(OpCodes.Ldc_I4_1));
            update.Body.Instructions.Insert(spot + 4, Instruction.Create(OpCodes.Stfld, jumpAgain1));
            update.Body.Instructions.Insert(spot + 2, Instruction.Create(OpCodes.Ldarg_0));
            update.Body.Instructions.Insert(spot + 3, Instruction.Create(OpCodes.Ldc_I4_1));
            update.Body.Instructions.Insert(spot + 4, Instruction.Create(OpCodes.Stfld, jumpAgain2));
            update.Body.Instructions.Insert(spot + 2, Instruction.Create(OpCodes.Ldarg_0));
            update.Body.Instructions.Insert(spot + 3, Instruction.Create(OpCodes.Ldc_I4_1));
            update.Body.Instructions.Insert(spot + 4, Instruction.Create(OpCodes.Stfld, jumpAgain3));
            update.Body.Instructions.Insert(spot + 2, Instruction.Create(OpCodes.Ldarg_0));
            update.Body.Instructions.Insert(spot + 3, Instruction.Create(OpCodes.Ldc_I4_1));
            update.Body.Instructions.Insert(spot + 4, Instruction.Create(OpCodes.Stfld, jumpAgain4));
            update.Body.Instructions.Insert(spot + 2, Instruction.Create(OpCodes.Ldarg_0));
            update.Body.Instructions.Insert(spot + 3, Instruction.Create(OpCodes.Ldc_I4_1));
            update.Body.Instructions.Insert(spot + 4, Instruction.Create(OpCodes.Stfld, jumpAgain5));
            update.Body.Instructions.Insert(spot + 2, Instruction.Create(OpCodes.Ldarg_0));
            update.Body.Instructions.Insert(spot + 3, Instruction.Create(OpCodes.Ldc_I4_1));
            update.Body.Instructions.Insert(spot + 4, Instruction.Create(OpCodes.Stfld, jumpAgain6));
        }
    }

    [PluginTag("Tweak-AddWings", "TerrariaPatcher", "Add 'AddWings' tweak!")]
    public class TweakAddWings
    {
        [PluginPatch("Terraria.exe")]
        public static void Patch(ModuleDefinition module, List<string> log)
        {
            var player = IL.GetTypeDefinition(module, "Player");
            var updatePlayerEquips = IL.GetMethodDefinition(player, "UpdateEquips");
            var wings = IL.GetFieldDefinition(player, "wings");
            var wingsLogic = IL.GetFieldDefinition(player, "wingsLogic");
            var wingTimeMax = IL.GetFieldDefinition(player, "wingTimeMax");

            IL.MethodAppend(updatePlayerEquips, updatePlayerEquips.Body.Instructions.Count - 1, 1, new[]
            {
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldc_I4, 32),
                Instruction.Create(OpCodes.Stfld, wings),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldc_I4, 32),
                Instruction.Create(OpCodes.Stfld, wingsLogic),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldc_I4, int.MaxValue),
                Instruction.Create(OpCodes.Stfld, wingTimeMax),
                Instruction.Create(OpCodes.Ret)
            });
        }
    }

    [PluginTag("Tweak-DisplayTime", "TerrariaPatcher", "Add 'DisplayTime' tweak!")]
    public class TweakDisplayTime
    {
        [PluginPatch("Terraria.exe")]
        public static void Patch(ModuleDefinition module, List<string> log)
        {
            var main = IL.GetTypeDefinition(module, "Main");
            var drawInfoAccs = IL.GetMethodDefinition(main, "DrawInfoAccs");

            int spot = IL.ScanForOpcodePattern(drawInfoAccs, (i, instruction) =>
            {
                var fieldReference = instruction.Operand as FieldReference;
                return fieldReference != null && fieldReference.Name == "accWatch";
            },
                OpCodes.Ldfld,
                OpCodes.Ldc_I4_0,
                OpCodes.Ble
                );

            drawInfoAccs.Body.Instructions[spot + 2].OpCode = OpCodes.Blt;
        }
    }

    [PluginTag("Tweak-MaxCraftingRange", "TerrariaPatcher", "Add 'MaxCraftingRange' tweak!")]
    public class TweakMaxCraftingRange
    {
        [PluginPatch("Terraria.exe")]
        public static void Patch(ModuleDefinition module, List<string> log)
        {
            var player = IL.GetTypeDefinition(module, "Player");
            var adjTiles = IL.GetMethodDefinition(player, "AdjTiles");

            int spot = IL.ScanForOpcodePattern(adjTiles, OpCodes.Ldc_I4_4,
                OpCodes.Stloc_0,
                OpCodes.Ldc_I4_3,
                OpCodes.Stloc_1
                );

            adjTiles.Body.Instructions[spot].OpCode = OpCodes.Ldc_I4;
            adjTiles.Body.Instructions[spot].Operand = 30;
            adjTiles.Body.Instructions[spot + 2].OpCode = OpCodes.Ldc_I4;
            adjTiles.Body.Instructions[spot + 2].Operand = 30;
        }
    }

    [PluginTag("Tweak-RemoveAnglerQuestLimit", "TerrariaPatcher", "Add 'RemoveAnglerQuestLimit' tweak!")]
    public class TweakMaxRemoveAnglerQuestLimit
    {
        [PluginPatch("Terraria.exe")]
        public static void Patch(ModuleDefinition module, List<string> log)
        {
            var main = IL.GetTypeDefinition(module, "Main");
            var guiChatDrawInner = IL.GetMethodDefinition(main, "GUIChatDrawInner");
            var questSwap = IL.GetMethodDefinition(main, "AnglerQuestSwap");

            int spot = IL.ScanForOpcodePattern(guiChatDrawInner,
                                               (i, instruction) =>
                                               {
                                                   var i3 = guiChatDrawInner.Body.Instructions[i + 3].Operand as FieldReference;
                                                   return i3 != null && i3.Name == "anglerQuestFinished";
                                               },
                                               OpCodes.Ldloc_S,
                                               OpCodes.Brfalse,
                                               OpCodes.Ldc_I4_1);

            guiChatDrawInner.Body.Instructions[spot + 2] = Instruction.Create(OpCodes.Call, questSwap);
            for (int i = spot + 3; guiChatDrawInner.Body.Instructions[i].OpCode != OpCodes.Ret; i++)
                guiChatDrawInner.Body.Instructions[i].OpCode = OpCodes.Nop;
        }
    }
}
