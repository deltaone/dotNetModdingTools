using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Reflection;

using Terraria;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace dotNetMT
{
    public class Teleport
    {
        private static bool enabled = MOD.Config.Get("modules.Teleport", true);
        private static Vector2[] positions = null;

        private static void _Teleport(Player player, float x, float y)
        {
            var vector = new Vector2(x, y);
            player.Teleport(vector, 4, 0);
            player.velocity = Vector2.Zero;
            NetMessage.SendData(65, -1, -1, "", 0, player.whoAmI, vector.X, vector.Y, 5, 0, 0);
        }

        [RuntimeHook("Terraria.exe", "Terraria.Player", "Update", false)]
        public static object HookPlayerUpdate(object rv, object obj, params object[] args)
        {
            if (!enabled) return (null);
            var this_ = (Player)obj;

            if ((int)args[0] != Main.myPlayer) return (null);

            if (CORE.IsOnCooldown()) return (null);

            if (!Main.hasFocus || Main.chatMode || Main.editSign || Main.editChest ||
                    Main.blockInput || Main.playerInventory || Main.mapFullscreen) return (null);

            if (Main.keyState.IsKeyDown(Keys.F))
            {                
                float x = Main.mouseX + Main.screenPosition.X - (this_.width / 2);
                float y = Main.mouseY + Main.screenPosition.Y - (this_.height / 2);
                if (this_.gravDir == -1f) y = Main.screenPosition.Y + Main.screenHeight - Main.mouseY - (this_.height / 2);

                CORE.Print("[dotNetMT] Teleport!");
                _Teleport(this_, x, y);
                CORE.BeginCooldown(30);
                return (null);
            }

            int slotActive = -1;
            var slotHotKeys = new Keys[10] { Keys.NumPad0, Keys.NumPad1, Keys.NumPad2, Keys.NumPad3, Keys.NumPad4, 
                                                Keys.NumPad5, Keys.NumPad6, Keys.NumPad7, Keys.NumPad8, Keys.NumPad9};            
            for (int i = 0; i < slotHotKeys.Length; i++)
                if (Main.keyState.IsKeyDown(slotHotKeys[i]))
                {
                    slotActive = i;
                    break;
                }

            if (slotActive == -1) return (null);

            if (positions == null)
            {
                positions = new Vector2[10];
                for (int i = 0; i < 10; i++)
                {   //default(Vector2)
                    float x, y;
                    string _x = MOD.Config.Get("Teleport.Slot-" + i.ToString() + ".X", "");
                    string _y = MOD.Config.Get("Teleport.Slot-" + i.ToString() + ".Y", "");
                    if (MOD.TryParseFloat(_x, out x) && MOD.TryParseFloat(_y, out y)) positions[i] = new Vector2(x, y);
                    else positions[i] = new Vector2(this_.position.X, this_.position.Y);
                }
            }

            if(Main.keyState.IsKeyDown(Keys.LeftAlt) || Main.keyState.IsKeyDown(Keys.RightAlt)) 
            {
                positions[slotActive] = new Vector2(this_.position.X, this_.position.Y);
                CORE.Print("[dotNetMT] Teleport positions stored! [" + slotActive + "](" + 
                            this_.position.X.ToString() + ", " + this_.position.Y.ToString() + ")");
                MOD.Config.Set("Teleport.Slot-" + slotActive.ToString() + ".X", positions[slotActive].X.ToString());
                MOD.Config.Set("Teleport.Slot-" + slotActive.ToString() + ".Y", positions[slotActive].Y.ToString());
                MOD.Config.Save();
            }
            else
            {
                CORE.Print("[dotNetMT] Teleport to slot [" + slotActive + "]!");
                _Teleport(this_, positions[slotActive].X, positions[slotActive].Y);
            }
            
            CORE.BeginCooldown(30);
            return (null);
        }
    }
}
