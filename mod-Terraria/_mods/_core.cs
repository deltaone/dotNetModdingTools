using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Linq;

using Terraria;
using Microsoft.Xna.Framework.Input;

[assembly: AssemblyTitle("Modules")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

namespace dotNetMT
{
    public static class CORE
    {
        private static int _cooldown = 0;

        private static string _lastCommand = "";

        public delegate void CommandCallback(string command, string args, bool state);

        private static Dictionary<string, CommandCallback> _commands = new Dictionary<string, CommandCallback>();
        private static Dictionary<string, bool> _commandStates = new Dictionary<string, bool>();


        static CORE()
        {
            var list = typeof(dotNetMT.CORE).Assembly.GetTypes()
                            .SelectMany(a => a.GetMethods()).Where(x => x.Name == "Startup");
            foreach (var e in list) e.Invoke(null, null);
        }


        public static bool CommandGetState(string command)
        {
            bool value;
            if(_commandStates.TryGetValue(command, out value)) return(value);
            return (false);
        }

        public static void CommandSetState(string command, bool state)
        {
            if (!_commandStates.ContainsKey(command)) return;
            _commandStates[command] = state;
        }

        public static bool CommandProcess(string command)
        {
            if (command.Length <= 1 || command[0] != '.') return (false);
            
            var parts = command.Split(new char[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);

            if (!_commandStates.ContainsKey(parts[0])) return (false);
            
            _commandStates[parts[0]] = !_commandStates[parts[0]];

            try
            {
                if (_commands[parts[0]] != null)
                    _commands[parts[0]](parts[0], parts.Length == 1 ? "" : parts[1], _commandStates[parts[0]]);
            }
            catch (System.Exception ex)
            {
                CORE.Print("Error [" + parts[0] + "]:" + ex.Message);
                MOD.Log("\nEXCEPTION:\n" + ex.Message + "\n" + ex.StackTrace);
            }

            return (true);
        }

        public static void CommandRegister(string command, CommandCallback method)
        {            
            MOD.Log("Command registered: " + command);
            _commands[command] = method;
            _commandStates[command] = false;
        }


        public static void BeginCooldown(int frames)
        {
            _cooldown = frames;
        }

        public static bool IsOnCooldown()
        {
            return (_cooldown > 0);
        }


        public static void Print(string message, byte r = 200, byte g = 200, byte b = 200)
        {
            Main.NewText(message, r, g, b);
        }


        [RuntimeHook("Terraria.exe", "Terraria.Main", "DrawFPS")]
        public static object HookDrawFPS(object rv, object obj, params object[] args)
        {
            if (_cooldown > 0) _cooldown--;
            if (IsOnCooldown()) return (null);

            if (Main.hasFocus && Main.chatMode && Main.keyState.IsKeyDown(Keys.RightControl) && Main.keyState.IsKeyDown(Keys.Enter))
                if (CommandProcess(Main.chatText))
                {
                    _lastCommand = Main.chatText;
                    Main.chatText = "";
                    Main.chatMode = false;
                    Main.chatRelease = false;
                    Main.PlaySound(11, -1, -1, 1);
                    BeginCooldown(30);
                    return (null);
                }

            if (Main.hasFocus && Main.chatMode && Main.keyState.IsKeyDown(Keys.Up) && Main.keyState.IsKeyDown(Keys.LeftControl))
            {
                Main.chatText = _lastCommand;
                BeginCooldown(30);
                return (null);
            }

            if (Main.keyState.IsKeyDown(Keys.Enter) && Main.netMode == 0 && !Main.keyState.IsKeyDown(Keys.LeftAlt) && !Main.keyState.IsKeyDown(Keys.RightAlt) && Main.hasFocus)
            {
                if (Main.chatRelease && !Main.chatMode && !Main.editSign && !Main.editChest && !Main.gameMenu && !Main.keyState.IsKeyDown(Keys.Escape))
                {                    
                    Main.chatMode = true;
                    Main.clrInput();
                    Main.chatText = "";
                    Main.PlaySound(10, -1, -1, 1);                    
                }
                Main.chatRelease = false;
                BeginCooldown(30);
            } else Main.chatRelease = true;
            
            return (null);
        }
    }
}
