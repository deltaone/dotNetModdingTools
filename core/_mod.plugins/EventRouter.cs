using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dotNetMT
{
    #region PluginEvent
    enum PluginEvent
    {   // don't assign numbers manually (used as array indexes)
        OnInitialize,
        OnPreUpdate,
        OnUpdate,
        OnChatCommand,
    }
    #endregion

    #region PluginBase
    public abstract class PluginBase // : MarshalByRefObject
    {
        public virtual void OnInitialize() { throw new NotImplementedException(); }
        public virtual void OnPreUpdate() { throw new NotImplementedException(); }
        public virtual void OnUpdate() { throw new NotImplementedException(); }
        public virtual void OnChatCommand(string command, string[] args) { throw new NotImplementedException(); }
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

        [PluginHook("_mod.test.exe", "dotNetMT.Core", "Initialize", false)]
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

        [PluginHook("_mod.test.exe", "dotNetMT.Core", "Update", true)]
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

        [PluginHook("_mod.test.exe", "dotNetMT.Core", "Update", false)]
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
    }
    #endregion
}
