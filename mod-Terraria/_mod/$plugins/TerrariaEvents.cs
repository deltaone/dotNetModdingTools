using System;
using System.Reflection;

using Terraria;
using Terraria.GameContent.Achievements;
using Keys = Microsoft.Xna.Framework.Input.Keys;

using dotNetMT;

namespace TranscendPlugins
{
    [PluginTag("Terraria events", "TerrariaPatcher", @"
        |Initiate terraria event:
        |
        |   Ctrl+Shift+` - Moon Lord
        |   Ctrl+Shift+1 - Meteor
        |   Ctrl+Shift+2 - Blood Moon
        |   Ctrl+Shift+3 - Goblin Army
        |   Ctrl+Shift+4 - Frost Legion
        |   Ctrl+Shift+5 - Pirate Invasion
        |   Ctrl+Shift+6 - Solar Eclipse
        |   Ctrl+Shift+7 - Pumpkin Moon
        |   Ctrl+Shift+8 - Frost Moon
        |   Ctrl+Shift+9 - Martian Madness
        |   Ctrl+Shift+0 - Lunar Apocalypse
    ")]
    public class TerrariaEvents : PluginBase
    {
        private MethodInfo _triggerLunarApocalypse;
        private FieldInfo _spawnMeteor;
        private MethodInfo _dropMeteor;
        private bool SpawnMeteor
        {
            get { return (bool)_spawnMeteor.GetValue(null); }
            set { _spawnMeteor.SetValue(null, value); }
        }

        public TerrariaEvents()
        {
            var worldGen = Assembly.GetEntryAssembly().GetType("Terraria.WorldGen");
            _triggerLunarApocalypse = worldGen.GetMethod("TriggerLunarApocalypse");
            _spawnMeteor = worldGen.GetField("spawnMeteor");
            _dropMeteor = worldGen.GetMethod("dropMeteor");

            string section = GetType().Name;
            bool control = true, shift = true, alt = false;

            HotkeyCore.RegisterHotkey(() =>
            {
                CORE.Print("EVENT: Goblin Army");
                if (Main.invasionType > 0)
                    Main.invasionSize = 0;
                else
                    Main.StartInvasion(1);
            }, DNMT.Config.Get(section, "GoblinArmy", new Hotkey(Keys.D3, control, shift, alt), true));           
            HotkeyCore.RegisterHotkey(() =>
            {
                CORE.Print("EVENT: Frost Legion");
                if (Main.invasionType > 0)
                    Main.invasionSize = 0;
                else
                    Main.StartInvasion(2);
            }, DNMT.Config.Get(section, "FrostLegion", new Hotkey(Keys.D4, control, shift, alt), true));
            HotkeyCore.RegisterHotkey(() =>
            {
                CORE.Print("EVENT: Pirate Invasion");
                if (Main.invasionType > 0)
                    Main.invasionSize = 0;
                else
                    Main.StartInvasion(3);
            }, DNMT.Config.Get(section, "PirateInvasion", new Hotkey(Keys.D5, control, shift, alt), true));
            HotkeyCore.RegisterHotkey(() =>
            {
                CORE.Print("EVENT: Martian Madness");
                if (Main.invasionType > 0)
                    Main.invasionSize = 0;
                else
                    Main.StartInvasion(4);
            }, DNMT.Config.Get(section, "MartianMadness", new Hotkey(Keys.D9, control, shift, alt), true));
            HotkeyCore.RegisterHotkey(() =>
            {
                CORE.Print("EVENT: Pumpkin Moon");
                if (Main.pumpkinMoon)
                    Main.stopMoonEvent();
                else
                    Main.startPumpkinMoon();
            }, DNMT.Config.Get(section, "PumpkinMoon", new Hotkey(Keys.D7, control, shift, alt), true));
            HotkeyCore.RegisterHotkey(() =>
            {
                CORE.Print("EVENT: Frost Moon");
                if (Main.snowMoon)
                    Main.stopMoonEvent();
                else
                    Main.startSnowMoon();
            }, DNMT.Config.Get(section, "FrostMoon", new Hotkey(Keys.D8, control, shift, alt), true));
            HotkeyCore.RegisterHotkey(() =>
            {
                CORE.Print("EVENT: Lunar Apocalypse");
                if (Terraria.NPC.LunarApocalypseIsUp || Terraria.NPC.AnyNPCs(398))
                    StopLunarEvent();
                else
                    TriggerLunarApocalypse();
            }, DNMT.Config.Get(section, "LunarApocalypse", new Hotkey(Keys.D0, control, shift, alt), true));
            HotkeyCore.RegisterHotkey(() =>
            {
                CORE.Print("EVENT: Moon Lord");
                if (Terraria.NPC.LunarApocalypseIsUp || Terraria.NPC.AnyNPCs(398))
                    StopLunarEvent();
                else
                    SpawnMoonLord();
            }, DNMT.Config.Get(section, "MoonLord", new Hotkey(Keys.OemTilde, control, shift, alt), true));
            HotkeyCore.RegisterHotkey(() =>
            {
                CORE.Print("EVENT: Blood Moon");
                if (Main.bloodMoon)
                    Main.bloodMoon = false;
                else
                    TriggerBloodMoon();
            }, DNMT.Config.Get(section, "BloodMoon", new Hotkey(Keys.D2, control, shift, alt), true));
            HotkeyCore.RegisterHotkey(() =>
            {
                CORE.Print("EVENT: Solar Eclipse");
                if (Main.eclipse)
                    Main.eclipse = false;
                else
                    TriggerEclipse();
            }, DNMT.Config.Get(section, "SolarEclipse", new Hotkey(Keys.D6, control, shift, alt), true));            
            HotkeyCore.RegisterHotkey(() =>
            {
                CORE.Print("EVENT: Meteor");
                SpawnMeteor = false;
                DropMeteor();
            }, DNMT.Config.Get(section, "Meteor", new Hotkey(Keys.D1, control, shift, alt), true));
        }
        
        private void DropMeteor()
        {
            _dropMeteor.Invoke(null, null);
        }

        private void TriggerLunarApocalypse()
        {
            _triggerLunarApocalypse.Invoke(null, null);
        }

        private void TriggerEclipse()
        {
            Main.eclipse = true;
            AchievementsHelper.NotifyProgressionEvent(2);
            if (Main.netMode == 0)
            {
                Main.NewText(Lang.misc[20], 50, 255, 130, false);
            }
            else if (Main.netMode == 2)
            {
                NetMessage.SendData(25, -1, -1, Lang.misc[20], 255, 50f, 255f, 130f, 0, 0, 0);
                NetMessage.SendData(7, -1, -1, "", 0, 0f, 0f, 0f, 0, 0, 0);
            }
        }

        private void TriggerBloodMoon()
        {
            Main.bloodMoon = true;
            AchievementsHelper.NotifyProgressionEvent(4);
            if (Main.netMode == 0)
            {
                Main.NewText(Lang.misc[8], 50, 255, 130, false);
            }
            else if (Main.netMode == 2)
            {
                NetMessage.SendData(25, -1, -1, Lang.misc[8], 255, 50f, 255f, 130f, 0, 0, 0);
            }
        }

        private void SpawnMoonLord()
        {
            Terraria.NPC.MoonLordCountdown = 3600;
            NetMessage.SendData(103, -1, -1, "", Terraria.NPC.MoonLordCountdown, 0f, 0f, 0f, 0, 0, 0);
            if (Main.netMode == 0)
            {
                Main.NewText(Lang.misc[52], 50, 255, 130, false);
                return;
            }
            if (Main.netMode == 2)
            {
                NetMessage.SendData(25, -1, -1, Lang.misc[52], 255, 50f, 255f, 130f, 0, 0, 0);
            }
        }

        private void StopLunarEvent()
        {
            Main.NewText("Stopped lunar event!", 50, 255, 130, false);
            Terraria.NPC.LunarApocalypseIsUp = false;
            for (int i = 0; i < 200; i++)
            {
                if (Main.npc[i].active)
                {
                    switch (Main.npc[i].type)
                    {
                        case 398: // Moon Lord
                        case 517: // Tower
                        case 422: // Tower
                        case 507: // Tower
                        case 493: // Tower
                            Main.npc[i].life = 0;
                            break;
                    }
                }
            }
        }
    }
}
