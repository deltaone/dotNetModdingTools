using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using Terraria;
using Terraria.DataStructures;

using dotNetMT;

namespace ZeromaruPlugins
{
    [PluginTag("God mode", "Zeromaru", @"
        |God mode hotkeys:
        |
        |   G - switch mode 
        |   Shift + G - downgrade mode
    ")]
    public class GodMode : PluginBase
    {
        enum Mode
        {
            Off = 0,
            DemiGod = 1,
            God = 2
        }
        private Mode _mode = Mode.Off;

        public GodMode()
        {
            string section = GetType().Name;

            _mode = DNMT.Config.Get(section, "Mode", Mode.Off, true);
            
            Action update = () =>
            {
                DNMT.Config.Set<Mode>("GodMode", "Mode", _mode);
                CORE.Print("God Mode: " + _mode, Color.Green);
            };
            HotkeyCore.RegisterHotkey(() =>
            {
                if (_mode == Mode.God) _mode = Mode.Off;
                else _mode++;
                update();
            }, DNMT.Config.Get(section, "KeySwitch", new Hotkey(Keys.G), true));
            HotkeyCore.RegisterHotkey(() =>
            {
                if (_mode == Mode.Off) _mode = Mode.God;
                else _mode--;
                update();
            }, DNMT.Config.Get(section, "KeyDowngrade", new Hotkey(Keys.G, shift: true), true));
        }

        public override void OnUpdate()
        {
            if (_mode != Mode.God) return;

            var player = Main.player[Main.myPlayer];
            player.statLife = player.statLifeMax2;
            player.statMana = player.statManaMax2;
            player.breath = player.breathMax + 1;
            player.noFallDmg = true;
            player.immune = true;
            player.immuneTime = 10;
            player.immuneAlpha = 0;
        }

        public override bool OnPlayerHurt(Player player, PlayerDeathReason damageSource, int damage, int hitDirection, bool pvp, bool quiet, bool crit, int cooldownCounter, out double result)
        {
            result = 0.0;
            return (_mode == Mode.God);
        }

        public override bool OnPlayerKillMe(Player player, PlayerDeathReason damageSource, double dmg, int hitDirection, bool pvp)
        {
            return (_mode == Mode.God || _mode == Mode.DemiGod);
        }
    }
}