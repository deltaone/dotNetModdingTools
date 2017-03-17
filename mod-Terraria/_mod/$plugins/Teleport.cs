using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Reflection;

using Terraria;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace dotNetMT
{
    [PluginTag("Teleport", "de1ta0ne & Co.", @"
		|Add teleport!
        |
        |Game mode usage:
        |
        |   T - teleport to mouse cursor
		|   ALT+NumPad(0-3) - store position
        |   NumPad(0-3) - teleport to stored position
        |
        |Map mode usage:
        |
        |   ALT+RightClick - teleport to clicked position
    ")]
    public class Teleport : PluginBase
    {       
        private Keys[] _slotHotKeys = new Keys[] { Keys.NumPad0, Keys.NumPad1, Keys.NumPad2, Keys.NumPad3 };
        private static Vector2[] _positions = null;

        public Teleport()
        {
            HotkeyCore.RegisterHotkey(() =>
            {
                if (CORE.IsOnCooldown()) return;
                DoTeleport(Main.player[Main.myPlayer], Main.mouseX + Main.screenPosition.X, Main.mouseY + Main.screenPosition.Y);
                CORE.BeginCooldown(30);
            }, DNMT.Config.Get("Teleport", "Key", new Hotkey(Keys.T), true));
        }

        private static void DoTeleport(Player player, float x, float y)
        {
            var vector = new Vector2(x, y);
            player.Teleport(vector, 4, 0);
            player.velocity = Vector2.Zero;
            NetMessage.SendData(65, -1, -1, "", 0, player.whoAmI, vector.X, vector.Y, 5, 0, 0);
        }

        public override void OnUpdate()
        {
            if (!CORE.IsCanUseHotKeys() || CORE.IsOnCooldown()) return;
            Player player = Main.player[Main.myPlayer];
            
            if (Main.mapFullscreen && Main.mouseRight && HotkeyCore.IsAltModifierKeyDown()) //  Main.keyState.IsKeyDown(Keys.LeftAlt)
            {
                int num = Main.maxTilesX * 16;
                int num2 = Main.maxTilesY * 16;
                Vector2 vector = new Vector2((float)Main.mouseX, (float)Main.mouseY);
                vector.X -= (float)(Main.screenWidth / 2);
                vector.Y -= (float)(Main.screenHeight / 2);
                Vector2 mapFullscreenPos = Main.mapFullscreenPos;
                Vector2 vector2 = mapFullscreenPos;
                vector /= 16f;
                vector *= 16f / Main.mapFullscreenScale;
                vector2 += vector;
                vector2 *= 16f;
                vector2.Y -= (float)player.height;
                if (vector2.X < 0f)
                {
                    vector2.X = 0f;
                }
                else if (vector2.X + (float)player.width > (float)num)
                {
                    vector2.X = (float)(num - player.width);
                }
                if (vector2.Y < 0f)
                {
                    vector2.Y = 0f;
                }
                else if (vector2.Y + (float)player.height > (float)num2)
                {
                    vector2.Y = (float)(num2 - player.height);
                }

                DoTeleport(player, vector2.X, vector2.Y);
                CORE.BeginCooldown(30);
                return;
            }

            int slotActive = -1;
            for (int i = 0; i < _slotHotKeys.Length; i++)
                if (Main.keyState.IsKeyDown(_slotHotKeys[i]))
                {
                    slotActive = i;
                    break;
                }
            if (slotActive == -1) return;

            if (_positions == null)
            {
                _positions = new Vector2[_slotHotKeys.Length];
                for (int i = 0; i < _slotHotKeys.Length; i++)
                {
                    float x, y;
                    if (DNMT.Config.TryGet("Teleport", "Slot-" + i.ToString() + "-X", out x) &&
                        DNMT.Config.TryGet("Teleport", "Slot-" + i.ToString() + "-Y", out y))
                        _positions[i] = new Vector2(x, y);
                    else
                        _positions[i] = new Vector2(player.position.X, player.position.Y);
                }
            }

            if (HotkeyCore.IsAltModifierKeyDown())
            {
                _positions[slotActive] = new Vector2(player.position.X, player.position.Y);
                CORE.Print("Teleport position stored! [" + slotActive + "](" +
                          player.position.X.ToString() + ", " + player.position.Y.ToString() + ")");
                DNMT.Config.Set("Teleport", "Slot-" + slotActive.ToString() + "-X", _positions[slotActive].X);
                DNMT.Config.Set("Teleport", "Slot-" + slotActive.ToString() + "-Y", _positions[slotActive].Y);
            }
            else
            {
                CORE.Print("Teleport to slot [" + slotActive + "]!");
                DoTeleport(player, _positions[slotActive].X, _positions[slotActive].Y);
            }
            CORE.BeginCooldown(30);
        }
    }
}
