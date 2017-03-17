using System;
using Microsoft.Xna.Framework.Input;

using Terraria;

using dotNetMT;

namespace TranscendPlugins
{
    [PluginTag("Change time", "TerrariaPatcher", @"
        |Change current terraria time:
        |
        |   , - 12:00 AM (midnight)
        |   Ctrl+, - 7:30 PM (dusk), triggers all night time events
        |   . - 12:00 PM (noon)
        |   Ctrl+. - 4:30 AM (dawn), triggers all day time events
    ")]
    public class Time : PluginBase
    {
        public Time()
        {
            string section = GetType().Name;

            HotkeyCore.RegisterHotkey(() => ChangeTime(HotkeyCore.IsControlModifierKeyDown() ? "dusk" : "midnight"),
                DNMT.Config.Get(section, "Night", new Hotkey(Keys.OemComma, ignoreModifierKeys: true), true));
            HotkeyCore.RegisterHotkey(() => ChangeTime(HotkeyCore.IsControlModifierKeyDown() ? "dawn" : "noon"), 
                DNMT.Config.Get(section, "Day", new Hotkey(Keys.OemPeriod, ignoreModifierKeys:true), true));
        }

        private void ChangeTime(string time)
        {
            switch (time.ToLower())
            {
                case "dusk":
                    Main.dayTime = true;
                    Main.time = 54001.0; // 7:30 PM (dusk), triggers all night time events
                    Main.NewText("Time changed to dusk.");
                    break;
                case "midnight":
                    Main.dayTime = false;
                    Main.time = 16200.0; // 12:00 AM (midnight)
                    Main.NewText("Time changed to midnight.");
                    break;
                case "dawn":
                    Main.dayTime = false;
                    Main.time = 32401.0; // 4:30 AM (dawn), triggers all day time events
                    Main.NewText("Time changed to dawn.");
                    break;
                case "noon":
                    Main.dayTime = true;
                    Main.time = 27000.0; // 12:00 PM (noon)
                    Main.NewText("Time changed to noon.");
                    break;
            }
        }

        public override void OnChatCommand(string command, string[] args)
        {
            if (command != "time") return;
            
            if (args.Length < 1 || args.Length > 1 || args[0] == "help")
            {
                Main.NewText("Usage:");
                Main.NewText("   .time dawn");
                Main.NewText("   .time noon");
                Main.NewText("   .time midnight");
                Main.NewText("   .time dusk");
                Main.NewText("   .time help");
                return;
            }

            ChangeTime(args[0]);
        }
    }
}
