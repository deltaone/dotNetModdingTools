using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using Terraria;

using dotNetMT;

namespace ZeromaruPlugins
{
    [PluginTag("Infinite flight", "Zeromaru", @"
		|Infinite flight:
        |
        |   I - toggle mode
    ")]
    public class InfiniteFlight : PluginBase
    {
        private bool _flight = false;

        public InfiniteFlight()
        {
            HotkeyCore.RegisterHotkey(() =>
            {
                _flight = !_flight;
                CORE.Print("Infinite Flight: " + (_flight ? "Enabled" : "Disabled"), Color.Green);
            }, DNMT.Config.Get("InfiniteFlight", "Key", new Hotkey(Keys.I), true));
        }

        public override void OnUpdate()
        {
            if (_flight)
            {
                var player = Main.player[Main.myPlayer];
                player.rocketTime = 1;
                player.carpetTime = 1;
                player.wingTime = 1f;
            }
        }
    }
}