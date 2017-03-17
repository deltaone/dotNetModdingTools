using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

using Terraria;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

using Mono.Cecil;
using Mono.Cecil.Cil;

[assembly: AssemblyTitle("_mod.plugins")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

namespace dotNetMT
{
    public static class CORE
    {
        private static int _cooldown = 0;
        internal static readonly IDictionary<string, Texture2D> _textures = new Dictionary<string, Texture2D>();
        internal static readonly IDictionary<string, SoundEffect> _sounds = new Dictionary<string, SoundEffect>();

        public static void Print(string message)
        {
            Print(message, 200, 200, 200);
        }

        public static void Print(string message, Color color)
        {
            Main.NewText(message, color.R, color.G, color.B);
        }

        public static void Print(string message, byte r, byte g, byte b)
        {
            Main.NewText(message, r, g, b);
        }

        public static SoundEffect GetSound(string name)
        {
            if (Main.dedServ) return (null);

            if (_sounds.ContainsKey(name))
                return (_sounds[name]);

            string path = Path.Combine(DNMT.modRootFolder, name + @".wav");
            if (!File.Exists(path))
            {
                DNMT.LogError("Error loading sound effect '" + path + "' - file not exisit!");
                return (null);
            }

            try
            {
                using (FileStream fileStream = new FileStream(path, FileMode.Open))
                {
                    _sounds[name] = SoundEffect.FromStream(fileStream);
                }
            }
            catch (System.Exception ex)
            {
                DNMT.LogError("Error loading sound effect '" + path + "' - " + ex.Message + "!");
                _sounds[name] = null;                
            }

            return (_sounds[name]);
        }

        public static Texture2D GetTexture(string name)
        {
            if (Main.dedServ) return (null);

            if (_textures.ContainsKey(name))
                return (_textures[name]);

            string path = Path.Combine(DNMT.modRootFolder, name + @".png");
            if (!File.Exists(path))
            {
                DNMT.LogError("Error loading texture '" + path + "' - file not exisit!");
                return (null);
            }

            try
            {
                using (FileStream fileStream = new FileStream(path, FileMode.Open))
                {
                    _textures[name] = Texture2D.FromStream(Main.instance.GraphicsDevice, fileStream);
                }
            }
            catch (System.Exception ex)
            {
                DNMT.LogError("Error loading texture '" + path + "' - " + ex.Message + "!");
                _textures[name] = null;
            }

            return (_textures[name]);
        }

        public static bool IsCanUseHotKeys()
        {
            if (Main.blockInput || Main.drawingPlayerChat || Main.editSign || Main.editChest) return (false);
            return(true);            
        }

        public static void BeginCooldown(int frames)
        {
            _cooldown = frames;
        }

        public static bool IsOnCooldown()
        {
            return (_cooldown > 0);
        }

        [PluginHook("Terraria.exe", "Terraria.Main", "DrawFPS")]
        public static object OnDrawFPS(object rv, object obj, params object[] args)
        {
            if (_cooldown > 0) _cooldown--;
            return (null);
        }

        [PluginPatch("Terraria.exe")]
        public static void PatchEnableChat(ModuleDefinition module, List<string> log)
        {   // patch chat to allow on singleplayer // Main.clrInput();
            var main = IL.GetTypeDefinition(module, "Main");
            var update = IL.GetMethodDefinition(main, "DoUpdate");

            int spot = IL.ScanForOpcodePattern(update, (i, instruction) =>
            {
                var f0 = instruction.Operand as FieldReference;
                var result0 = f0 != null && f0.Name == "netMode";
                var f33 = update.Body.Instructions[i + 3].Operand as FieldReference;
                var result33 = f33 != null && f33.Name == "keyState";
                return (result0 && result33);
            },
                OpCodes.Ldsfld,
                OpCodes.Ldc_I4_1,
                OpCodes.Bne_Un
            );

            if (spot == -1)
            {
                log.Add("[ERROR] Can't apply path, pattern not found!");
                return;
            }

            update.Body.Instructions[spot + 0].OpCode = OpCodes.Nop;
            update.Body.Instructions[spot + 1].OpCode = OpCodes.Nop;
            update.Body.Instructions[spot + 2].OpCode = OpCodes.Nop;
        }
    }
}
