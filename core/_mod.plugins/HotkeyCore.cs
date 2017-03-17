using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace dotNetMT
{
    #region Hotkey
    public class Hotkey : IEquatable<Hotkey>
    {
        public bool Control { get; set; }
        public bool Shift { get; set; }
        public bool Alt { get; set; }

        private bool _ignoreModifierKeys;
        public bool IgnoreModifierKeys
        {
            get
            {   
                if (Key == Keys.LeftControl || Key == Keys.RightControl ||
                    Key == Keys.LeftAlt || Key == Keys.RightAlt ||
                    Key == Keys.LeftShift || Key == Keys.RightShift)
                    return true;
                return _ignoreModifierKeys;
            }
            set { _ignoreModifierKeys = value; }
        }

        public Hotkey() { }

        public Hotkey(Keys key, bool control = false, bool shift = false, bool alt = false, bool ignoreModifierKeys = false)
        {
            Key = key;
            Control = control;
            Shift = shift;
            Alt = alt;
            IgnoreModifierKeys = ignoreModifierKeys;
        }

        public Keys Key { get; set; }

        public Action Action { get; set; }

        public string Tag { get; set; } // If non-null, it stores the chat command associated with this hotkey.

        public override string ToString()
        {
            return (Control ? "Control," : "") + (Shift ? "Shift," : "") + (Alt ? "Alt," : "") + Key + (Tag == null || Tag.Length == 0 ? "" : "," + Tag);
        }

        public static bool TryParseKey(string input, out Keys result)
        {
            try
            {
                result = (Keys)Enum.Parse(typeof(Keys), input, true);
                return (true);
            }
            catch
            {
                result = Keys.None;
                return (false);
            }
        }        

        public static bool TryParse(string input, out Hotkey result)
        {
            var key = Keys.None;
            var control = false;
            var shift = false;
            var alt = false;            
            string tag = null;

            foreach (var keyString in input.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                switch (keyString.ToLower())
                {
                    case "control":
                        control = true;
                        break;
                    case "shift":
                        shift = true;
                        break;
                    case "alt":
                        alt = true;
                        break;
                    default:
                        if (key == Keys.None && TryParseKey(keyString, out key))
                            break;
                        tag = keyString.Trim();
                        break;
                }
            }

            if (key == Keys.None)
            {
                result = null;
                return (false);
            }
            result = new Hotkey() { Key = key, Control = control, Alt = alt, Shift = shift, Tag = tag };
            return (true);
        }

        public static Hotkey Parse(string input, Hotkey defaultValue = null)
        {
            Hotkey result;
            if (TryParse(input, out result)) return (result);
            return (defaultValue);
        }

        public static implicit operator Hotkey(string input)
        {
            return (Parse(input, null));
        }
        
        public bool Equals(Hotkey other)
        {
            if (other == null) return false;

            return this.Key == other.Key &&
                   this.Control == other.Control &&
                   this.Shift == other.Shift &&
                   this.Alt == other.Alt &&
                   this.IgnoreModifierKeys == other.IgnoreModifierKeys;
        }
    }
    #endregion

    #region HotkeyCore
    public static class HotkeyCore
    {
        private static List<Hotkey> _hotkeys = new List<Hotkey>();

        private static bool _control, _shift, _alt;
        private static bool _fresh = true;

        static HotkeyCore()
        {
            // Load hotkey binds
            var result = DNMT.Config.EnumerateKeys("HotkeyBinds");
            foreach (var k in result)
            {
                Hotkey key;
                string command;
                if (DNMT.Config.TryGet("HotkeyBinds", k, out command) && Hotkey.TryParse(k, out key) && command.StartsWith("."))
                    RegisterHotkey(command, key);
                else
                    DNMT.LogWarning("Invalid record in [HotkeyBinds]: " + k + ".");
            }
        }

        public static void RegisterHotkey(string command, Keys key, bool control = false, bool shift = false, bool alt = false, bool ignoreModifierKeys = false)
        {
            RegisterHotkey(command, new Hotkey() { Key = key, Control = control, Shift = shift, Alt = alt, IgnoreModifierKeys = ignoreModifierKeys });
        }

        public static void RegisterHotkey(string command, Hotkey key)
        {
            key.Tag = command;
            key.Action = () =>
            {
                var split = command.Substring(1).Split(new[] { ' ' }, 2);
                var cmd = split[0].ToLower();
                var args = split.Length > 1 ? split[1].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries) : new string[0];
                EventRouter.OnChatCommand(cmd, args);
            };
            RegisterHotkey(key);
        }

        public static void RegisterHotkey(Action action, Keys key, bool control = false, bool shift = false, bool alt = false, bool ignoreModifierKeys = false)
        {
            RegisterHotkey(new Hotkey() { Action = action, Key = key, Control = control, Shift = shift, Alt = alt, IgnoreModifierKeys = ignoreModifierKeys });
        }

        public static void RegisterHotkey(Action action, Hotkey key)
        {
            key.Action = action;
            RegisterHotkey(key);
        }

        public static void RegisterHotkey(Hotkey hotkey)
        {
            _hotkeys.Add(hotkey);
        }

        public static void UnregisterHotkey(Keys key, bool control = false, bool shift = false, bool alt = false, bool ignoreModifierKeys = false)
        {
            UnregisterHotkey(new Hotkey() { Key = key, Control = control, Shift = shift, Alt = alt, IgnoreModifierKeys = ignoreModifierKeys });
        }

        public static void UnregisterHotkey(Hotkey hotkey)
        {
            _hotkeys.RemoveAll(key => key.Equals(hotkey));
        }

        public static ICollection<Hotkey> GetHotkeys()
        {
            return _hotkeys.AsReadOnly();
        }

        public static bool IsAltModifierKeyDown()
        {
            return _alt;
        }

        public static bool IsControlModifierKeyDown()
        {
            return _control;
        }

        public static bool IsShiftModifierKeyDown()
        {
            return _shift;
        }

        public static void Process()
        {
            List<Keys> checkList = new List<Keys>() { Keys.LeftControl, Keys.RightControl, Keys.LeftShift, Keys.RightShift, Keys.LeftAlt, Keys.RightAlt };
            foreach (var hotkey in _hotkeys)
                if (!checkList.Contains(hotkey.Key)) checkList.Add(hotkey.Key);
            for (int i = 0; i < checkList.Count; i++)
                if (!Keyboard.IsKeyDown(checkList[i])) checkList.RemoveAt(i--);            
            Keys[] keysdown = checkList.ToArray();
            
			//Keys[] keysdown = Keyboard.GetState().GetPressedKeys();
            
            _control = keysdown.Contains(Keys.LeftControl) || keysdown.Contains(Keys.RightControl);
            _shift = keysdown.Contains(Keys.LeftShift) || keysdown.Contains(Keys.RightShift);
            _alt = keysdown.Contains(Keys.LeftAlt) || keysdown.Contains(Keys.RightAlt);
                        
            var anyPresses = false;
            foreach (var hotkey in _hotkeys)
            {
                if (keysdown.Contains(hotkey.Key) &&
                    (hotkey.IgnoreModifierKeys || (_control == hotkey.Control && _shift == hotkey.Shift && _alt == hotkey.Alt)))
                {
                    anyPresses = true;
                    if (_fresh) hotkey.Action();
                }
            }
            _fresh = !anyPresses;
        }
    }
    #endregion
}
