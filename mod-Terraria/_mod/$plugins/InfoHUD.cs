using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Terraria;

namespace dotNetMT
{
    [PluginTag("Info HUD", "de1ta0ne", @"
		|Show info on HUD:
        |
        |   Alt+Shift+` - Watch (show daytime)
        |   Alt+Shift+1 - Ore Finder
        |   Alt+Shift+2 - Fish Finder
        |   Alt+Shift+3 - Compass and Depth Meter
        |   Alt+Shift+4 - Critter Guide (rare creatures)
        |   Alt+Shift+5 - Third Eye (number of nearby enemies)
        |   Alt+Shift+6 - Weather Radio
        |   Alt+Shift+7 - Calendar (moon phase)
        |   Alt+Shift+8 - Jar Of Souls (kill count)
        |   Alt+Shift+9 - Dream Catcher (dps)
        |   Alt+Shift+0 - Stopwatch (movement speed)
    ")]
    public class InfoHUD : PluginBase
    {
        private int _nfoWatch = 0;
        private bool _nfoOreFinder = false, _nfoFishFinder = false, _nfoCompassAndDepthMeter = false,
            _nfoCritterGuide = false, _nfoThirdEye = false, _nfoWeatherRadio = false, _nfoCalendar = false,
            _nfoJarOfSouls = false, _nfoDreamCatcher = false, _nfoStopwatch = false;

        public InfoHUD()
        {
            string section = GetType().Name;
            bool control = false, shift = true, alt = true;

            HotkeyCore.RegisterHotkey(() =>
            {
                _nfoWatch++;
                if (_nfoWatch > 3) _nfoWatch = 0;
                CORE.Print("Info - watch: " + (_nfoWatch > 0 ? "Enabled - mode " + _nfoWatch.ToString() : "Disabled"), Color.Green);
            }, DNMT.Config.Get(section, "Watch", new Hotkey(Keys.OemTilde, control, shift, alt), true));
            HotkeyCore.RegisterHotkey(() =>
            {
                _nfoOreFinder = !_nfoOreFinder;
                CORE.Print("Info - ore finder: " + (_nfoOreFinder ? "Enabled" : "Disabled"), Color.Green);
            }, DNMT.Config.Get(section, "OreFinder", new Hotkey(Keys.D1, control, shift, alt), true));
            HotkeyCore.RegisterHotkey(() =>
            {
                _nfoFishFinder = !_nfoFishFinder;
                CORE.Print("Info - fish finder: " + (_nfoFishFinder ? "Enabled" : "Disabled"), Color.Green);
            }, DNMT.Config.Get(section, "FishFinder", new Hotkey(Keys.D2, control, shift, alt), true));
            HotkeyCore.RegisterHotkey(() =>
            {
                _nfoCompassAndDepthMeter = !_nfoCompassAndDepthMeter;
                CORE.Print("Info - compass and depth meter: " + (_nfoCompassAndDepthMeter ? "Enabled" : "Disabled"), Color.Green);
            }, DNMT.Config.Get(section, "CompassAndDepthMeter", new Hotkey(Keys.D3, control, shift, alt), true));
            HotkeyCore.RegisterHotkey(() =>
            {
                _nfoCritterGuide = !_nfoCritterGuide;
                CORE.Print("Info - critter guide: " + (_nfoCritterGuide ? "Enabled" : "Disabled"), Color.Green);
            }, DNMT.Config.Get(section, "CritterGuide", new Hotkey(Keys.D4, control, shift, alt), true));
            HotkeyCore.RegisterHotkey(() =>
            {
                _nfoThirdEye = !_nfoThirdEye;
                CORE.Print("Info - third eye: " + (_nfoThirdEye ? "Enabled" : "Disabled"), Color.Green);
            }, DNMT.Config.Get(section, "ThirdEye", new Hotkey(Keys.D5, control, shift, alt), true));                       
            HotkeyCore.RegisterHotkey(() =>
            {
                _nfoWeatherRadio = !_nfoWeatherRadio;
                CORE.Print("Info - weather radio: " + (_nfoWeatherRadio ? "Enabled" : "Disabled"), Color.Green);
            }, DNMT.Config.Get(section, "WeatherRadio", new Hotkey(Keys.D6, control, shift, alt), true));
            HotkeyCore.RegisterHotkey(() =>
            {
                _nfoCalendar = !_nfoCalendar;
                CORE.Print("Info - calendar: " + (_nfoCalendar ? "Enabled" : "Disabled"), Color.Green);
            }, DNMT.Config.Get(section, "Calendar", new Hotkey(Keys.D7, control, shift, alt), true));
            HotkeyCore.RegisterHotkey(() =>
            {
                _nfoJarOfSouls = !_nfoJarOfSouls;
                CORE.Print("Info - jar of souls: " + (_nfoJarOfSouls ? "Enabled" : "Disabled"), Color.Green);
            }, DNMT.Config.Get(section, "JarOfSouls", new Hotkey(Keys.D8, control, shift, alt), true));
            HotkeyCore.RegisterHotkey(() =>
            {
                _nfoDreamCatcher = !_nfoDreamCatcher;
                CORE.Print("Info - dream catcher: " + (_nfoDreamCatcher ? "Enabled" : "Disabled"), Color.Green);
            }, DNMT.Config.Get(section, "DreamCatcher", new Hotkey(Keys.D9, control, shift, alt), true));
            HotkeyCore.RegisterHotkey(() =>
            {
                _nfoStopwatch = !_nfoStopwatch;
                CORE.Print("Info - stopwatch: " + (_nfoStopwatch ? "Enabled" : "Disabled"), Color.Green);
            }, DNMT.Config.Get(section, "Stopwatch", new Hotkey(Keys.D0, control, shift, alt), true));
        }

        public override void OnPlayerUpdateBuffs(Player player)
        {
            if (_nfoWatch >= 0)
                Main.player[Main.myPlayer].accWatch = _nfoWatch;
            if (_nfoOreFinder)           
                Main.player[Main.myPlayer].accOreFinder = true;            
            if (_nfoFishFinder)
                Main.player[Main.myPlayer].accFishFinder = true;
            if (_nfoCompassAndDepthMeter)
            {
                Main.player[Main.myPlayer].accCompass = 1;
                Main.player[Main.myPlayer].accDepthMeter = 1;
            }
            if (_nfoCritterGuide)
                Main.player[Main.myPlayer].accCritterGuide = true;
            if (_nfoThirdEye)
                Main.player[Main.myPlayer].accThirdEye = true;
            if (_nfoWeatherRadio)
                Main.player[Main.myPlayer].accWeatherRadio = true;
            if (_nfoCalendar)
                Main.player[Main.myPlayer].accCalendar = true;
            if (_nfoJarOfSouls)
                Main.player[Main.myPlayer].accJarOfSouls = true;
            if (_nfoDreamCatcher)
                Main.player[Main.myPlayer].accDreamCatcher = true;
            if (_nfoStopwatch)
                Main.player[Main.myPlayer].accStopwatch = true;
        }
    }
}