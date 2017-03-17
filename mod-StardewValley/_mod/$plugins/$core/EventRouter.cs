using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using StardewValley;

namespace dotNetMT
{
    #region PluginEvent
    enum PluginEvent
    {   // don't assign numbers manually (used as array indexes)
        OnInitialize,
        OnPreUpdate,
        OnUpdate,
        OnUpdateControlInput,
    }
    #endregion

    #region PluginBase
    public abstract class PluginBase // : MarshalByRefObject
    {
        public virtual void OnInitialize() { throw new NotImplementedException(); }
        public virtual void OnPreUpdate() { throw new NotImplementedException(); }
        public virtual void OnUpdate() { throw new NotImplementedException(); }
        public virtual void OnUpdateControlInput(KeyboardState stateKeyboard, MouseState stateMouse, GamePadState stateGamePad) { throw new NotImplementedException(); }
    }
    #endregion

    #region EventRouter
    public static class EventRouter
    {
        private static List<PluginBase> _pluginList = null;
        private static List<PluginBase>[] _pluginEventMap = null;

        static EventRouter()
        {
            // LoadPlugins
            _pluginList = new List<PluginBase>();
            var pluginBaseType = typeof(PluginBase);
            var pluginTypeList = pluginBaseType.Assembly.GetTypes()
                .Where(t => pluginBaseType.IsAssignableFrom(t) && pluginBaseType != t);

            Array events = Enum.GetValues(typeof(PluginEvent)); // string[] = Enum.GetNames(typeof(PluginEvent));
            _pluginEventMap = new List<PluginBase>[events.Length];
            for (int i = 0; i < events.Length; i++)
                _pluginEventMap[i] = new List<PluginBase>();

            foreach (var type in pluginTypeList)
            {
                if (!DNMT.Config.Get("plugins", type.FullName, true)) continue;
                PluginBase instance = null;
                try
                {
                    instance = (PluginBase)Activator.CreateInstance(type);
                }
                catch (System.Exception ex)
                {
                    DNMT.LogError("Error loading plugin: " + type.FullName + " - " + ex.Message + "!");
                    continue;
                }
                _pluginList.Add(instance);
                foreach (PluginEvent e in events)
                {
                    var method = type.GetMethod(e.ToString());
                    if (method == null || method.DeclaringType == pluginBaseType) continue;
                    _pluginEventMap[(int)e].Add(instance);
                }
            }
        }

        [PluginHook("Stardew Valley.exe", "StardewValley.Game1", "Initialize", false)]
        public static object OnInitialize(object rv, object obj, params object[] args)
        {
            try
            {
                foreach (var e in _pluginEventMap[(int)PluginEvent.OnInitialize])
                    e.OnInitialize();
            }
            catch (System.Exception ex)
            {
                DNMT.LogWarning(ex.Message);                
            }
            return (null);
        }

        [PluginHook("Stardew Valley.exe", "StardewValley.Game1", "Update", true)]
        public static object OnPreUpdate(object rv, object obj, params object[] args)
        {
            try
            {
                foreach (var e in _pluginEventMap[(int)PluginEvent.OnPreUpdate])
                    e.OnPreUpdate();
            }
            catch (System.Exception ex)
            {
                DNMT.LogWarning(ex.Message);
            }
            return (null);
        }

        [PluginHook("Stardew Valley.exe", "StardewValley.Game1", "Update", false)]
        public static object OnUpdate(object rv, object obj, params object[] args)
        {
            try
            {
                foreach (var e in _pluginEventMap[(int)PluginEvent.OnUpdate])
                    e.OnUpdate();
            }
            catch (System.Exception ex)
            {
                DNMT.LogWarning(ex.Message);
            }
            return (null);
        }

        [PluginPatch("Stardew Valley.exe")]
        public static void PathOnUpdateControlInput(ModuleDefinition module, List<string> log)
        {
            var fn = IL.GetMethodDefinition(module, "Game1", "UpdateControlInput");
            if (fn == null)
            {
                log.Add("Can't find target method!");
                return;
            }            

            int spot = IL.ScanForOpcodePattern(fn,
                (i, instruction) =>
                {
                    var reference = fn.Body.Instructions[i + 2].Operand as MethodReference;
                    return (reference != null && reference.Name == "checkForRunButton");
                },
                OpCodes.Ldloc_0,
                OpCodes.Ldc_I4_0,
                OpCodes.Call
            );
            if (spot == -1)
            {
                log.Add("Can't find patch location!");
                return;
            }

            spot += 3;
            var callType = module.Import(typeof(dotNetMT.EventRouter)).Resolve();
            var callMethod = module.Import(IL.GetMethodDefinition(callType, "OnUpdateControlInput"));
            IL.MethodAppend(fn, spot, 0, new[] {
                    Instruction.Create(OpCodes.Call, callMethod),
                });
        }

        public static void OnUpdateControlInput()
        {
            try
            {
                KeyboardState stateKeyboard = Keyboard.GetState();
                MouseState stateMouse = Mouse.GetState();
                GamePadState stateGamePad = GamePad.GetState(Game1.playerOneIndex);

                foreach (var e in _pluginEventMap[(int)PluginEvent.OnUpdateControlInput])
                    e.OnUpdateControlInput(stateKeyboard, stateMouse, stateGamePad);
            }
            catch (System.Exception ex)
            {
                DNMT.LogWarning(ex.Message);
            }
        }
    }
    #endregion
}
