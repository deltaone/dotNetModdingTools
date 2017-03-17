using System;

using Terraria;

using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace dotNetMT
{
    [PluginTag("Build & Craft", "de1ta0ne & Co.", @"
        |Additional building tweaks:
        |
        |   Alt + NumPadDecimal(dot) - building grid (ruler)
        |   Shift + NumPadDecimal(dot) or Delete - building ruler line
        |   Control + NumPadDecimal(dot) - show wires
        |   Alt + NumPadAdd - infinite building range
        |   Shift + NumPadAdd - long loot range
        |   Control + NumPadAdd - lets you craft anything
    ")]
    public class BuildAndCraft : PluginBase
    {
        private bool _grid = false, _rulerLine = false, _wires = false,
                     _rangeBuild = false, _rangeLoot = false, _craft = false;

        private int _initialTileRangeX = Player.tileRangeX,
                    _initialTileRangeY = Player.tileRangeY,
                    _initialDefaultItemGrabRange = Player.defaultItemGrabRange;

        public BuildAndCraft()
        {
            string section = GetType().Name;

            HotkeyCore.RegisterHotkey(() =>
            {
                _grid = !_grid;
                CORE.Print("Building grid: " + (_grid ? "Enabled" : "Disabled"));
            }, DNMT.Config.Get(section, "BuildingGrid", new Hotkey(Keys.Decimal, alt: true), true));
            HotkeyCore.RegisterHotkey(() =>
            {
                _rulerLine = !_rulerLine;
                CORE.Print("Building ruler line: " + (_rulerLine ? "Enabled" : "Disabled"));
            }, DNMT.Config.Get(section, "RulerLine", new Hotkey(Keys.Delete), true));
            HotkeyCore.RegisterHotkey(() =>
            {
                _wires = !_wires;
                CORE.Print("Show wires: " + (_wires ? "Enabled" : "Disabled"));
            }, DNMT.Config.Get(section, "ShowWires", new Hotkey(Keys.Decimal, control: true), true));
            HotkeyCore.RegisterHotkey(() =>
            {
                _rangeBuild = !_rangeBuild;
                if (!_rangeBuild)
                {
                    Player.tileRangeX = _initialTileRangeX;
                    Player.tileRangeY = _initialTileRangeY;
                }
                CORE.Print("Build range: " + (_rangeBuild ? "Enabled" : "Disabled"));
            }, DNMT.Config.Get(section, "BuildRange", new Hotkey(Keys.Add, alt: true), true));
            HotkeyCore.RegisterHotkey(() =>
            {
                _rangeLoot = !_rangeLoot;
                if (!_rangeLoot)
                    Player.defaultItemGrabRange = _initialDefaultItemGrabRange;
                CORE.Print("Loot range: " + (_rangeLoot ? "Enabled" : "Disabled"));
            }, DNMT.Config.Get(section, "LootRange", new Hotkey(Keys.Add, shift: true), true));
            HotkeyCore.RegisterHotkey(() =>
            {
                _craft = !_craft;
                CORE.Print("Craft anything: " + (_craft ? "Enabled" : "Disabled"));
            }, DNMT.Config.Get(section, "CraftAnything", new Hotkey(Keys.Add, control: true), true));
        }

        public override void OnPlayerUpdateBuffs(Player player)
        {
            if (player.whoAmI != Main.myPlayer) return;

            if (_grid)
                Main.player[Main.myPlayer].rulerGrid = true;

            if (_rulerLine)
                player.rulerLine = true;
            
            if (_wires)
                player.InfoAccMechShowWires = true;

            if (_rangeBuild)
            {
                Player.tileRangeX = 999;
                Player.tileRangeY = 999;                
            }

            if (_rangeLoot)
                Player.defaultItemGrabRange = 700;
        }

        public override void OnRecipeFindRecipes()
        {
            if (!_craft) return;

            Main.numAvailableRecipes = 0;
            for (int i = 0; i < Recipe.maxRecipes; i++)
            {
                int num7 = i - 1;
                if (num7 < 1)
                    num7 = 20;
                int num8 = i - 2;
                if (num8 < 2)
                    num8 = 20;
                bool flag4 = false;
                if (Main.recipe[num8].createItem.name == Main.recipe[i].createItem.name)
                    flag4 = true;
                if (Main.recipe[num7].createItem.name == Main.recipe[i].createItem.name)
                    flag4 = true;
                if (!flag4)
                {
                    Main.availableRecipe[Main.numAvailableRecipes] = i;
                    Main.numAvailableRecipes++;
                }
            }            
        }
    }
}