using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

using Terraria;
using Terraria.GameContent.Achievements;

using Microsoft.Xna.Framework.Input;

namespace dotNetMT
{
    public static class COMMANDS
    {
        public static void Startup()
        {
            CORE.CommandRegister(".craft", _craft);  // lets you craft anything
            CORE.CommandRegister(".invsave", _invsave); // saves your current inventory to a file in the Terraria folder
            CORE.CommandRegister(".invload", _invload); // loads the inventory from the given file and overwrites your current one
            CORE.CommandRegister(".flare", _flare); // flashlight on mouse cursor
            CORE.CommandRegister(".torch", _torch); // light on player (invisible torch)
            CORE.CommandRegister(".range", _range); // infinite building range
            CORE.CommandRegister(".ruler", _ruler); // building grid
            CORE.CommandRegister(".meteor", _meteor); // force spawn meteor
            CORE.CommandRegister(".bloodmoon", _bloodmoon); // starting bloodmoon
            CORE.CommandRegister(".invasion", _invasion); // starting invasion // Main.player[i].statLifeMax >= 200
            CORE.CommandRegister(".eclipse", _eclipse); // starting eclipse
        }

        [RuntimeHook("Terraria.exe", "Terraria.Recipe", "FindRecipes", false)]
        public static object HookRecipeFindRecipes(object rv, object obj, params object[] args)
        {
            if (!CORE.CommandGetState(".torch")) return (null);

            Main.numAvailableRecipes = 0;
            for (int l = 0; l < Recipe.maxRecipes; l++)
            {
                int num7 = l - 1;
                if (num7 < 1)
                {
                    num7 = 20;
                }
                int num8 = l - 2;
                if (num8 < 2)
                {
                    num8 = 20;
                }
                bool flag4 = false;
                if (Main.recipe[num8].createItem.name == Main.recipe[l].createItem.name)
                {
                    flag4 = true;
                }
                if (Main.recipe[num7].createItem.name == Main.recipe[l].createItem.name)
                {
                    flag4 = true;
                }
                if (!flag4)
                {
                    Main.availableRecipe[Main.numAvailableRecipes] = l;
                    Main.numAvailableRecipes++;
                }
            }

            return (null);
        }

        public static void _craft(string command, string args, bool state)
        {
            CORE.Print("[dotNetMT] " + command + " = " + state.ToString());
        }

        public static void _invsave(string command, string args, bool state)
        {
            string filename = args == "" ? "default" : args;
            string result = "";
            for (int i = 0; i < Main.player[Main.myPlayer].inventory.Length; i++)
            {
                result = string.Concat(new object[]
						{
							result,
							Main.player[Main.myPlayer].inventory[i].type,
							"c",
							Main.player[Main.myPlayer].inventory[i].stack,
							"b",
							Main.player[Main.myPlayer].inventory[i].prefix,
							"a"
						});
            }
            result += Environment.NewLine;
            for (int i = 0; i < Main.player[Main.myPlayer].armor.Length; i++)
            {
                result = string.Concat(new object[]
						{
							result,
							Main.player[Main.myPlayer].armor[i].type,
							"b",
							Main.player[Main.myPlayer].armor[i].prefix,
							"a"
						});
            }
            result += Environment.NewLine;
            for (int i = 0; i < Main.player[Main.myPlayer].dye.Length; i++)
            {
                result = result + Main.player[Main.myPlayer].dye[i].type + "a";
            }
            string currentDirectory = Environment.CurrentDirectory;
                
            if (File.Exists(currentDirectory + "\\" + filename + ".inv")) File.Delete(currentDirectory + "\\" + filename + ".inv");

            using (StreamWriter streamWriter = new StreamWriter(currentDirectory + "\\" + filename + ".inv", true))
            {
                streamWriter.Write(result);
            }

            CORE.Print("[dotNetMT] " + command + " - command done ...");
        }

        public static void _invload(string command, string args, bool state)
        {
            string filename = args == "" ? "default" : args;
            string currentDirectory = Environment.CurrentDirectory;
            if (File.Exists(currentDirectory + "\\" + filename + ".inv"))
            {
                string[] array = File.ReadAllLines(currentDirectory + "\\" + filename + ".inv");
                for (int i = 0; i < Main.player[Main.myPlayer].inventory.Length; i++)
                {
                    Item item2 = new Item();
                    item2.SetDefaults(Convert.ToInt32(array[0].Substring(0, array[0].IndexOf("c"))), false); // , Convert.ToByte(array[0].Substring(array[0].IndexOf("b") + 1, array[0].IndexOf("a") - (array[0].IndexOf("b") + 1)))
                    item2.netDefaults(Convert.ToInt32(array[0].Substring(0, array[0].IndexOf("c"))));
                    item2.Prefix((int)Convert.ToByte(array[0].Substring(array[0].IndexOf("b") + 1, array[0].IndexOf("a") - (array[0].IndexOf("b") + 1))));
                    item2.stack = Convert.ToInt32(array[0].Substring(array[0].IndexOf("c") + 1, array[0].IndexOf("b") - (array[0].IndexOf("c") + 1)));
                    Main.showItemText = true;
                    Main.player[Main.myPlayer].inventory[i] = item2;
                    array[0] = array[0].Substring(array[0].IndexOf("a") + 1);
                }
                for (int i = 0; i < Main.player[Main.myPlayer].armor.Length; i++)
                {
                    Item item2 = new Item();
                    item2.SetDefaults(Convert.ToInt32(array[1].Substring(0, array[1].IndexOf("b"))), false); // , Convert.ToByte(array[1].Substring(array[1].IndexOf("b") + 1, array[1].IndexOf("a") - (array[1].IndexOf("b") + 1)))
                    item2.netDefaults(Convert.ToInt32(array[1].Substring(0, array[1].IndexOf("b"))));
                    item2.Prefix((int)Convert.ToByte(array[1].Substring(array[1].IndexOf("b") + 1, array[1].IndexOf("a") - (array[1].IndexOf("b") + 1))));
                    Main.showItemText = true;
                    Main.player[Main.myPlayer].armor[i] = item2;
                    array[1] = array[1].Substring(array[1].IndexOf("a") + 1);
                }
                for (int i = 0; i < Main.player[Main.myPlayer].dye.Length; i++)
                {
                    Item item2 = new Item();
                    item2.SetDefaults(Convert.ToInt32(array[2].Substring(0, array[2].IndexOf("a"))), false);
                    item2.netDefaults(Convert.ToInt32(array[2].Substring(0, array[2].IndexOf("a"))));
                    Main.showItemText = true;
                    Main.player[Main.myPlayer].dye[i] = item2;
                    array[2] = array[2].Substring(array[2].IndexOf("a") + 1);
                }
            }

            CORE.Print("[dotNetMT] " + command + " - command done ...");
        }

        [RuntimeHook("Terraria.exe", "Terraria.Player", "Update", false)]
        public static object HookPlayerUpdate(object rv, object obj, params object[] args)
        {
            var this_ = (Player)obj;
            if ((int)args[0] != Main.myPlayer) return (null);

            // 0.7f, 1f, 0.8f - glowstick
            // 1f, 0.95f, 0.8f - torch

            if (CORE.CommandGetState(".torch"))
                Lighting.AddLight((int)(this_.position.X + (float)(this_.width / 2)) / 16, (int)(this_.position.Y + (float)(this_.height / 2)) / 16, 1f, 0.95f, 0.8f);

            if (CORE.CommandGetState(".flare") && Main.keyState.IsKeyDown(Keys.LeftAlt))
            {
                if (this_.gravDir == 1f)
                    Lighting.AddLight((int)(((float)Main.mouseX + Main.screenPosition.X) / 16f), (int)(((float)Main.mouseY + Main.screenPosition.Y) / 16f), 0.7f, 1f, 0.8f);
                else if (this_.gravDir == -1f)
                    Lighting.AddLight((int)(((float)Main.mouseX + Main.screenPosition.X) / 16f), (int)((float)((int)(Main.screenPosition.Y + (float)Main.screenHeight - (float)Main.mouseY)) / 16f), 0.7f, 1f, 0.8f);
            }

            return (null);
        }

        public static void _flare(string command, string args, bool state)
        {
            CORE.Print("[dotNetMT] " + command + " = " + state.ToString());
        }

        public static void _torch(string command, string args, bool state)
        {
            CORE.Print("[dotNetMT] " + command + " = " + state.ToString());
        }

        [RuntimeHook("Terraria.exe", "Terraria.Player", "ResetEffects", false)]
        public static object HookPlayerResetEffects(object rv, object obj, params object[] args)
        {
            if (CORE.CommandGetState(".range") && Main.keyState.IsKeyDown(Keys.LeftAlt))
            {
                Player.tileRangeX = 999;
                Player.tileRangeY = 999;
            }

            if (CORE.CommandGetState(".ruler")) // && Main.keyState.IsKeyDown(Keys.LeftAlt))
                Main.player[Main.myPlayer].rulerGrid = true;

            return (null);
        }

        public static void _range(string command, string args, bool state)
        {
            CORE.Print("[dotNetMT] " + command + " = " + state.ToString());
        }

        public static void _ruler(string command, string args, bool state)
        {
            CORE.Print("[dotNetMT] " + command + " = " + state.ToString());
        }

        public static void _meteor(string command, string args, bool state)
        {
            var type = Assembly.GetEntryAssembly().GetType("Terraria.WorldGen");

            FieldInfo field = type.GetField("spawnMeteor");
            field.SetValue(null, false); // landed on second part of night 

            MethodInfo method = type.GetMethod("dropMeteor");
            method.Invoke(null, new object[] { });

            CORE.Print("[dotNetMT] " + command + " - command done ...");
        }

        public static void _bloodmoon(string command, string args, bool state)
        {
            Main.bloodMoon = state;
            if (state)
            {
                AchievementsHelper.NotifyProgressionEvent(4);
                if (Main.netMode == 0) Main.NewText(Lang.misc[8], 50, 255, 130, false);
                else if (Main.netMode == 2) NetMessage.SendData(25, -1, -1, Lang.misc[8], 255, 50f, 255f, 130f, 0, 0, 0);
            }
            CORE.Print("[dotNetMT] " + command + " = " + state.ToString());
        }

        public static void _invasion(string command, string args, bool state)
        {
            // 1 - Goblin army
            // 2 - Frost legion
            // 3 - Pirates
            // 4 - Martian madness
                        
            if (state)
            {
                int invasion = 1;
                MOD.TryParseInt(args, out invasion);
                Main.StartInvasion(invasion);
            }
            else if (Main.invasionType > 0) Main.invasionSize = 0;

            CORE.Print("[dotNetMT] " + command + " - command done ...");
        }

        public static void _eclipse(string command, string args, bool state)
        {
            Main.eclipse = state;
            if (state)
            {
                AchievementsHelper.NotifyProgressionEvent(2);
                if (Main.netMode == 0) Main.NewText(Lang.misc[20], 50, 255, 130, false);
                else if (Main.netMode == 2)
                {
                    NetMessage.SendData(25, -1, -1, Lang.misc[20], 255, 50f, 255f, 130f, 0, 0, 0);
                    NetMessage.SendData(7, -1, -1, "", 0, 0f, 0f, 0f, 0, 0, 0);
                }
            }

            CORE.Print("[dotNetMT] " + command + " = " + state.ToString());
        }
    }
}