using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using Terraria;
using Terraria.DataStructures;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace dotNetMT
{
    #region PluginEvent
    enum PluginEvent
    {   // don't assign numbers manually (used as array indexes)
        OnInitialize,
        OnPreUpdate,
        OnUpdate,
        OnChatCommand,
        OnPlayerUpdate,
        OnPlayerUpdateBuffs,
        OnPlayerHurt,
        OnPlayerKillMe,
        OnNetMessageSendData,
        OnRecipeFindRecipes,
        OnLightingGetColor,
        OnDrawInterface,
        OnDrawInventory,
    }
    #endregion

    #region PluginBase
    public abstract class PluginBase // : MarshalByRefObject
    {
        public virtual void OnInitialize() { throw new NotImplementedException(); }
        public virtual void OnPreUpdate() { throw new NotImplementedException(); }
        public virtual void OnUpdate() { throw new NotImplementedException(); }
        public virtual void OnChatCommand(string command, string[] args) { throw new NotImplementedException(); }
        public virtual void OnPlayerUpdate(Player player) { throw new NotImplementedException(); }
        public virtual void OnPlayerUpdateBuffs(Player player) { throw new NotImplementedException(); }
        public virtual bool OnPlayerHurt(Player player, PlayerDeathReason damageSource, int damage, int hitDirection, bool pvp, bool quiet, bool crit, int cooldownCounter, out double result) { throw new NotImplementedException(); }
        public virtual bool OnPlayerKillMe(Player player, PlayerDeathReason damageSource, double dmg, int hitDirection, bool pvp) { throw new NotImplementedException(); }
        public virtual bool OnNetMessageSendData(int msgType, int remoteClient, int ignoreClient, string text, int number, float number2, float number3, float number4, int number5, int number6, int number7) { throw new NotImplementedException(); }
        public virtual void OnRecipeFindRecipes() { throw new NotImplementedException(); }
        public virtual bool OnLightingGetColor(int x, int y, out Color color) { throw new NotImplementedException(); }
        public virtual void OnDrawInterface() { throw new NotImplementedException(); }
        public virtual void OnDrawInventory() { throw new NotImplementedException(); }
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

        //public static void OnUpdate_()
        //{
        //    DNMT.LogDebug("OnUpdate-router");
        //}
        //[PluginPatch("Terraria.exe")]
        //public static void PathHookOnUpdate(ModuleDefinition module, List<string> log)
        //{
        //    var main = IL.GetTypeDefinition(module, "Main");
        //    var update = IL.GetMethodDefinition(main, "Update");
        //    var router = module.Import(typeof(dotNetMT.EventRouter)).Resolve();
        //    var onUpdate = module.Import(IL.GetMethodDefinition(router, "OnUpdate_"));
        //    IL.MethodAppend(update, update.Body.Instructions.Count - 1, 1, new[]   // Main.Update post hook
        //        {
        //            Instruction.Create(OpCodes.Call, onUpdate),
        //            Instruction.Create(OpCodes.Ret)
        //        });
        //}

        [PluginHook("Terraria.exe", "Terraria.Main", "Initialize", false)]
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

        [PluginHook("Terraria.exe", "Terraria.Main", "DoUpdate", true)]
        public static object OnPreUpdate(object rv, object obj, params object[] args)
        {
            try
            {
                if (CORE.IsCanUseHotKeys())
                    HotkeyCore.Process();
                foreach (var e in _pluginEventMap[(int)PluginEvent.OnPreUpdate])
                    e.OnPreUpdate();
            }
            catch (System.Exception ex)
            {
                DNMT.LogWarning(ex.Message);
            }
            return (null);
        }

        [PluginHook("Terraria.exe", "Terraria.Main", "DoUpdate", false)]
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

        public static void OnChatCommand(string command, string[] args)
        {
            try
            {
                foreach (var e in _pluginEventMap[(int)PluginEvent.OnChatCommand])
                    e.OnChatCommand(command, args);
            }
            catch (System.Exception ex)
            {
                DNMT.LogWarning(ex.Message);
            }
        }

        [PluginHook("Terraria.exe", "Terraria.Player", "Update", false)]
        public static object OnPlayerUpdate(object rv, object obj, params object[] args)
        {
            try
            {
                foreach (var e in _pluginEventMap[(int)PluginEvent.OnPlayerUpdate])
                    e.OnPlayerUpdate((Player) obj);
            }
            catch (System.Exception ex)
            {
                DNMT.LogWarning(ex.Message);
            }
            return (null);
        }

        [PluginHook("Terraria.exe", "Terraria.Player", "UpdateBuffs", false)]
        public static object OnPlayerUpdateBuffs(object rv, object obj, params object[] args)
        {
            try
            {
                foreach (var e in _pluginEventMap[(int)PluginEvent.OnPlayerUpdateBuffs])
                    e.OnPlayerUpdateBuffs((Player)obj);
            }
            catch (System.Exception ex)
            {
                DNMT.LogWarning(ex.Message);
            }
            return (null);
        }

        [PluginHook("Terraria.exe", "Terraria.Player", "Hurt", true)]
        public static object OnPlayerHurt(object rv, object obj, params object[] args)
        {
            double result = 0.0;
            bool ret = false;
            try
            {
                foreach (var e in _pluginEventMap[(int)PluginEvent.OnPlayerHurt])
                {
                    double temp;
                    if (e.OnPlayerHurt((Player)obj, (PlayerDeathReason)args[0], (int)args[1], (int)args[2], (bool)args[3], (bool)args[4], (bool)args[5], (int)args[6], out temp))
                    {
                        result = temp;
                        ret = true;
                    }
                }
            }
            catch (System.Exception ex)
            {
                DNMT.LogWarning(ex.Message);
            }
            return (ret ? (object)result : null);
        }

        [PluginHook("Terraria.exe", "Terraria.Player", "KillMe", true)]
        public static object OnPlayerKillMe(object rv, object obj, params object[] args)
        {
            bool ret = false;
            try
            {                
                foreach (var e in _pluginEventMap[(int)PluginEvent.OnPlayerKillMe])
                    ret = ret || e.OnPlayerKillMe((Player)obj, (PlayerDeathReason)args[0], (double)args[1], (int)args[2], (bool)args[3]);
            }
            catch (System.Exception ex)
            {
                DNMT.LogWarning(ex.Message);
            }

            return (ret ? (object)true : null);
        }

        [PluginHook("Terraria.exe", "Terraria.NetMessage", "SendData", true)]
        public static object OnNetMessageSendData(object rv, object obj, params object[] args)
        {
            var msgType = (int)args[0];
            var remoteClient = (int) args[1];
            var ignoreClient = (int) args[2];
            var text = (string)args[3];
            var number = (int)args[4];

            bool ret = false;
            try
            {
                foreach (var e in _pluginEventMap[(int)PluginEvent.OnNetMessageSendData])
                    ret = ret || e.OnNetMessageSendData(msgType, remoteClient, ignoreClient, text, number, 
                        (float)args[5], (float)args[6], (float)args[7], (int)args[8], (int)args[9], (int)args[10]);

                if (msgType == 25 && number == Main.myPlayer && !string.IsNullOrEmpty(text) && text[0] == '.')
                {
                    ret = true;
                    var split = text.Substring(1).Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                    var cmd = split[0].ToLower();
                    var cmdArgs = split.Length > 1 ? split[1].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries) : new string[0];
                    if (cmd == "plugins")
                    {
                        CORE.Print("Loaded plugins:", Color.Green);
                        CORE.Print(string.Join(", ", _pluginList.Select(plugin => plugin.GetType().Name)));
                        //pluginList.ForEach(plugin => { CORE.Print("   " + plugin.GetType().Name); });
                    }
                    else
                        foreach (var e in _pluginEventMap[(int)PluginEvent.OnChatCommand])
                            e.OnChatCommand(cmd, cmdArgs);
                }
            }
            catch (System.Exception ex)
            {
                DNMT.LogWarning(ex.Message);
            }
            return (ret ? (object)true : null);
        }

        [PluginHook("Terraria.exe", "Terraria.Recipe", "FindRecipes", false)]
        public static object OnRecipeFindRecipes(object rv, object obj, params object[] args)
        {
            try
            {
                foreach (var e in _pluginEventMap[(int)PluginEvent.OnRecipeFindRecipes])
                    e.OnRecipeFindRecipes();
            }
            catch (System.Exception ex)
            {
                DNMT.LogWarning(ex.Message);
            }
            return (null);
        }        

        [PluginHook("Terraria.exe", "Terraria.Lighting", "GetColor", true, 2)]
        public static object OnLightingGetColor(object rv, object obj, params object[] args)
        {
            bool ret = false;
            var result = Color.White;
            try
            {
                foreach (var e in _pluginEventMap[(int)PluginEvent.OnLightingGetColor])
                {
                    Color temp;
                    if (e.OnLightingGetColor((int)args[0], (int)args[1], out temp))
                    {
                        ret = true;
                        result = temp;
                    }
                }
            }
            catch (System.Exception ex)
            {
                DNMT.LogWarning(ex.Message);
            }
            return (ret ? (object)result : null);
        }

        [PluginHook("Terraria.exe", "Terraria.Main", "DrawInterface", false)]
        public static object OnDrawInterface(object rv, object obj, params object[] args)
        {
            try
            {
                foreach (var e in _pluginEventMap[(int)PluginEvent.OnDrawInterface])
                    e.OnDrawInterface();
            }
            catch (System.Exception ex)
            {
                DNMT.LogWarning(ex.Message);
            }
            return (null);
        } 

        [PluginHook("Terraria.exe", "Terraria.Main", "DrawInventory", false)]
        public static object OnDrawInventory(object rv, object obj, params object[] args)
        {
            try
            {
                foreach (var e in _pluginEventMap[(int)PluginEvent.OnDrawInventory])
                    e.OnDrawInventory();
            }
            catch (System.Exception ex)
            {
                DNMT.LogWarning(ex.Message);
            }
            return (null);
        } 
    }
    #endregion
}
