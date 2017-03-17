using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using Terraria;
using Terraria.Localization;
using Terraria.Map;

namespace dotNetMT
{
    [PluginTag("Helpful hotkeys", "jopojelly", @"
		|Helpful hotkeys:
        |
        |   F - Quick Use Torch
        |   Q - Quick Use Item in Slot #50
        |   C - Quick Use Item in Slot #49
        |   X - Quick Use Item in Slot #48
        |   Z - Quick Use Item in Slot #47
    ")]
    public class HelpfulHotkeys : PluginBase
    {
        internal int _originalSelectedItem;
        internal bool _autoRevertSelectedItem = false;

        public HelpfulHotkeys()
        {
            string section = GetType().Name;

            HotkeyCore.RegisterHotkey(() =>
            {
                Player player = Main.player[Main.myPlayer];

                if (player.inventory[49].type != 0)
                {
                    _originalSelectedItem = player.selectedItem;
                    _autoRevertSelectedItem = true;
                    player.selectedItem = 49;
                    player.controlUseItem = true;
                    player.ItemCheck(Main.myPlayer);
                }
            }, DNMT.Config.Get(section, "QuickUse50", new Hotkey(Keys.Q), true));

            HotkeyCore.RegisterHotkey(() =>
            {
                Player player = Main.player[Main.myPlayer];

                if (player.inventory[48].type != 0)
                {
                    _originalSelectedItem = player.selectedItem;
                    _autoRevertSelectedItem = true;
                    player.selectedItem = 48;
                    player.controlUseItem = true;
                    player.ItemCheck(Main.myPlayer);
                }
            }, DNMT.Config.Get(section, "QuickUse49", new Hotkey(Keys.C), true));

            HotkeyCore.RegisterHotkey(() =>
            {
                Player player = Main.player[Main.myPlayer];

                if (player.inventory[47].type != 0)
                {
                    _originalSelectedItem = player.selectedItem;
                    _autoRevertSelectedItem = true;
                    player.selectedItem = 47;
                    player.controlUseItem = true;
                    player.ItemCheck(Main.myPlayer);
                }
            }, DNMT.Config.Get(section, "QuickUse48", new Hotkey(Keys.X), true));

            HotkeyCore.RegisterHotkey(() =>
            {
                Player player = Main.player[Main.myPlayer];

                if (player.inventory[46].type != 0)
                {
                    _originalSelectedItem = player.selectedItem;
                    _autoRevertSelectedItem = true;
                    player.selectedItem = 46;
                    player.controlUseItem = true;
                    player.ItemCheck(Main.myPlayer);
                }
            }, DNMT.Config.Get(section, "QuickUse47", new Hotkey(Keys.Z), true));

            HotkeyCore.RegisterHotkey(() =>
            {
                Player player = Main.player[Main.myPlayer];
                for (int i = 0; i < player.inventory.Length; i++)
                {
                    if (player.inventory[i].createTile == 4)
                    {
                        _originalSelectedItem = player.selectedItem;
                        _autoRevertSelectedItem = true;
                        player.selectedItem = i;
                        player.controlUseItem = true;                        
                        Player.tileTargetX = (int)(player.Center.X / 16f);                            
                        Player.tileTargetY = (int)(player.Center.Y / 16f);
                        int stack = player.inventory[player.selectedItem].stack;
                        float distance = -3.40282347E+38f;
                        bool flag;
                        do
                        {
                            float lastDistance = 3.40282347E+38f;
                            flag = false;
                            for (int x = -Player.tileRangeX - player.blockRange + (int)(player.position.X / 16f); 
                                x <= Player.tileRangeX + player.blockRange - 1 + (int)((player.position.X + (float)player.width) / 16f); x++)
                            {
                                for (int y = -Player.tileRangeY - player.blockRange + (int)(player.position.Y / 16f); 
                                    y <= Player.tileRangeY + player.blockRange - 2 + (int)((player.position.Y + (float)player.height) / 16f); y++)
                                {
                                    float currentDistance = Vector2.Distance(Main.MouseWorld, new Vector2((float)(x * 16), (float)(y * 16)));                                                            
                                    if (lastDistance > currentDistance && distance < currentDistance)
                                    {
                                        flag = true;
                                        lastDistance = currentDistance;
                                        Player.tileTargetX = x;
                                        Player.tileTargetY = y;
                                    }
                                }
                            }
                            distance = lastDistance;
                            player.ItemCheck(Main.myPlayer);
                        }
                        while (flag && stack == player.inventory[player.selectedItem].stack);
                        break;
                    }
                }
            }, DNMT.Config.Get(section, "AutoTorch", new Hotkey(Keys.F), true));
        }

        public override void OnUpdate()
        {
            if (!_autoRevertSelectedItem) return;

            Player player = Main.player[Main.myPlayer];
            if (player.itemTime == 0 && player.itemAnimation == 0)
            {
                player.selectedItem = _originalSelectedItem;
                _autoRevertSelectedItem = false;
            }
        }
    }
}