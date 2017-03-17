using System;

using Terraria;

using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

using dotNetMT;

namespace MrBlueSLPlugins
{
    [PluginTag("Additional lights", "MrBlueSL & Co.", @"
        |Activate additional lights:
        |
        | Alt + NumPadSubtract - flashlight under mouse cursor (hold ALT to activate)
        | Control + NumPadSubtract - light on player (invisible torch)        
        | Shift + NumPadSubtract - remove darkness (full bright)
    ")]

    public class LightingEx : PluginBase
    {
        private bool _flashlight = false, _torch = false, _fullbright = false;

        public LightingEx()
        {
            string section = GetType().Name; 

            HotkeyCore.RegisterHotkey(() =>
            {
                _flashlight = !_flashlight;
                CORE.Print("Flashlight: " + (_flashlight ? "Enabled" : "Disabled"));
            }, DNMT.Config.Get(section, "Flashlight", new Hotkey(Keys.Subtract, alt: true), true));
            HotkeyCore.RegisterHotkey(() =>
            {
                _torch = !_torch;
                CORE.Print("Torch: " + (_torch ? "Enabled" : "Disabled"));
            }, DNMT.Config.Get(section, "Torch", new Hotkey(Keys.Subtract, control: true), true));
            HotkeyCore.RegisterHotkey(() =>
            {
                _fullbright = !_fullbright;
                CORE.Print("Fullbright: " + (_fullbright ? "Enabled" : "Disabled"));
            }, DNMT.Config.Get(section, "Fullbright", new Hotkey(Keys.Subtract, shift: true), true));
        }

        public override void OnPlayerUpdate(Player player)
        {
            // 0.7f, 1f, 0.8f - glowstick
            // 1f, 0.95f, 0.8f - torch
            // 1f, 1f, 1f - white

            if (HotkeyCore.IsAltModifierKeyDown() && _flashlight)
                Lighting.AddLight((int)((Main.mouseX + Main.screenPosition.X) / 16), (int)((Main.mouseY + Main.screenPosition.Y) / 16), 1f, 0.95f, 0.8f);

            if (_torch)
                Lighting.AddLight((int)((player.position.X + ((float)player.width / 2)) / 16), (int)((player.position.Y + ((float)player.height / 2)) / 16), 1f, 0.95f, 0.8f);
        }

        public override bool OnLightingGetColor(int x, int y, out Color color)
        {
            color = Color.White;
            return (_fullbright);
        }
    }
}