using System;
using System.Linq;

using Terraria;

using dotNetMT;

namespace MrBlueSLPlugins
{
    [PluginTag("Command binding", "MrBlueSL", @"
        |Add command binding to hotkey:
        |
        |   press ENTER key and type for info:
        |       .bind help
    ")]
    public class HotkeyBind : PluginBase
    {
        public HotkeyBind()
        {
            // Load hotkey binds
            var result = DNMT.Config.EnumerateKeys("HotkeyBinds");
            foreach (var k in result)
            {
                Hotkey key;
                string command;
                if (DNMT.Config.TryGet("HotkeyBinds", k, out command) && Hotkey.TryParse(k, out key) && command.StartsWith("."))
                    HotkeyCore.RegisterHotkey(command, key);
                else
                    DNMT.LogWarning("Invalid record in [HotkeyBinds]: " + k + ".");
            }
        }

        public override void OnChatCommand(string command, string[] args)
        {
            if (command != "bind" && command != "unbind" && command != "listbinds") return;
            
            if ((command == "bind" && (args.Length <= 1 || args[0] == "help")) ||
                (command == "unbind" && (args.Length <= 0 || args[0] == "help")) ||
                (command == "listbinds" && args.Length > 0 && args[0] == "help"))
            {
                CORE.Print("Usage:");
                CORE.Print("  .bind modifiers,hotkey command");
                CORE.Print("  .unbind modifiers,hotkey");
                CORE.Print("  .listbinds");
                CORE.Print("Example:");
                CORE.Print("  .bind Control,T .time dusk");
                CORE.Print("  .unbind Control,T");
                CORE.Print("  .bind Control,Shift,K .usetime");
                return;
            }
            
            if (command == "bind")
                BindHotkey(args[0], string.Join(" ", args.Skip(1)));
            else if (command == "unbind")
                UnbindHotkey(args[0]);
            else if (command == "listbinds")
            {
                foreach (var hotkey in HotkeyCore.GetHotkeys().Where(hotkey => !string.IsNullOrEmpty(hotkey.Tag)))
                    Main.NewText(hotkey.ToString());
            }
        }

        private void BindHotkey(string hotkey, string cmd)
        {
            var key = Hotkey.Parse(hotkey);

            if (string.IsNullOrEmpty(cmd) || !cmd.StartsWith(".") || key == null)
                Main.NewText("Invalid hotkey binding!");
            else
            {
                DNMT.Config.Set("HotkeyBinds", hotkey, cmd);
                HotkeyCore.RegisterHotkey(cmd, key);
                CORE.Print(hotkey + " set to " + cmd);
            }
        }

        private void UnbindHotkey(string hotkey)
        {
            var key = Hotkey.Parse(hotkey);

            if (key == null)
                Main.NewText("Invalid hotkey binding!");
            else
            {
                DNMT.Config.Set("HotkeyBinds", hotkey, null);
                HotkeyCore.UnregisterHotkey(key);
                CORE.Print("Unbind " + hotkey);
            }
        }
    }
}