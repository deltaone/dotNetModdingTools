using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

[assembly: AssemblyTitle("_mod.core")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

namespace dotNetMT
{
    public static class DNMT
    {
        private static readonly string _modFolder = @"_mod";

        public static readonly string assemblyFile = Assembly.GetExecutingAssembly().Location;
        public static readonly string assemblyFolder = Path.GetFullPath(Path.GetDirectoryName(assemblyFile) + Path.DirectorySeparatorChar);
        public static readonly string assemblyStartupFolder = Path.GetFullPath(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar);
        public static readonly string modRootFolder;

        public static LogFile Logger = new LogFile(Path.ChangeExtension(assemblyFile, ".log"), true);
        public static PrivateProfile Config = new PrivateProfile(Path.ChangeExtension(assemblyFile, ".ini"));

        static DNMT()
        {
            List<string> paths = new List<string>() 
            {
                Config.Get("main", "root", null),
                Path.GetFullPath(Path.Combine(assemblyFolder, _modFolder) + Path.DirectorySeparatorChar),
                File.Exists(Path.Combine(assemblyFolder, "UnityEngine.dll")) ? 
                    Path.GetFullPath(Path.Combine(assemblyFolder, @"..\..\" + _modFolder) + Path.DirectorySeparatorChar) : null,
                Path.GetFullPath(Path.Combine(assemblyStartupFolder, _modFolder) + Path.DirectorySeparatorChar),
            };

            foreach (string path in paths)
            {
                if (String.IsNullOrEmpty(path) || !Directory.Exists(path)) continue;
                modRootFolder = path;
                break;
            }

            if (modRootFolder == null)
            {
                modRootFolder = assemblyFolder;
                LogWarning("Can't find mod root folder, using '" + assemblyFolder + "'!");
            }
           
            AppDomain.CurrentDomain.ProcessExit += ClassDestructor; // DomainUnload
        }

        static void ClassDestructor(object sender, EventArgs e)
        {
        }

        //---------------------------------------------------------------------

        public static void Log(string message)
        {
            Logger.Write(message);
        }

        public static void LogWarning(string message, int callerLevel = 0)
        {
            var i = GetCallerInfo(2 + (callerLevel > 0 ? callerLevel : checked(-callerLevel)));
            Log("[WARNING] " + message + String.Format(" <{0}/{1}()[{2}]>", i["file"], i["method"], i["line"]));
            Logger.FlushLog();
        }

        public static void LogError(string message, int callerLevel = 0)
        {
            var i = GetCallerInfo(2 + (callerLevel > 0 ? callerLevel : checked(-callerLevel)));
            Log("[ERROR] " + message + String.Format(" <{0}/{1}()[{2}]>", i["file"], i["method"], i["line"]));
            foreach (var e in GetStackTrace(2)) Log("[ERROR] " + e);
            Logger.FlushLog();
        }

        public static void LogDebug(string message, int callerLevel = 0)
        {
            var i = GetCallerInfo(2 + (callerLevel > 0 ? callerLevel : checked(-callerLevel)));
            Log(String.Format("[DEBUG] {0}(): {1}", i["method"], message));
            Logger.FlushLog();
        }

        //---------------------------------------------------------------------

        public static Dictionary<string, string> GetCallerInfo(int frame = 1)
        {   // MethodBase.GetCurrentMethod().Name
            var f = new StackFrame(frame, true);
            return (new Dictionary<string, string> { { "method", f.GetMethod().Name }, 
                    { "file", Path.GetFileName(f.GetFileName()) },  { "line", f.GetFileLineNumber().ToString() }});
        }

        public static List<string> GetStackTrace(int skip = 2)
        {
            var result = new List<string>();
            var stack = new System.Diagnostics.StackTrace(true).ToString();
            var trace = new List<string>(stack.Split('\n'));

            trace.RemoveRange(0, skip);
            trace.Reverse();

            bool found = false;
            foreach (string e in trace)
            {
                if (!found && e.Contains("Main")) found = true;
                if (found) result.Add(e.Replace('\n', ' ').Replace("\r", "").Replace('\t', ' '));
            }
            result.Reverse();
            return (result);
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class PluginTagAttribute : Attribute
    {
        public string pluginName;
        public string pluginAuthor;
        public string pluginNote;
        public string pluginRequired;

        public PluginTagAttribute(string pluginName) : this(pluginName, "", "", "") { }

        public PluginTagAttribute(string pluginName, string pluginAuthor) : this(pluginName, pluginAuthor, "", "") { }

        public PluginTagAttribute(string pluginName = "", string pluginAuthor = "", string pluginNote = "", string pluginRequired = "")
        {
            this.pluginName = pluginName;
            this.pluginAuthor = pluginAuthor;
            this.pluginNote = Regex.Replace(pluginNote, "^\\s+\\|", "", RegexOptions.Multiline).Trim();
            this.pluginNote = this.pluginNote.Replace("\r", string.Empty);
            this.pluginNote = this.pluginNote.Replace("\n", "\r\n");
            this.pluginRequired = pluginRequired;
        }
    }
    
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public sealed class PluginHookAttribute : Attribute
    {
        public string assemblyName;
        public string typeName;
        public string methodName;
        public bool hookOnBegin;
        public int parameterCount;

        public PluginHookAttribute(string assemblyName, string typeName, string methodName) : this(assemblyName, typeName, methodName, true, -1) { }

        public PluginHookAttribute(string assemblyName, string typeName, string methodName, bool hookOnBegin) : this(assemblyName, typeName, methodName, hookOnBegin, -1) { }

        public PluginHookAttribute(string assemblyName, string typeName, string methodName, bool hookOnBegin = true, int parameterCount = -1)
        {
            this.assemblyName = assemblyName;
            this.typeName = typeName;
            this.methodName = methodName;
            this.hookOnBegin = hookOnBegin;
            this.parameterCount = parameterCount;
        }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class PluginPatchAttribute : Attribute
    {
        public string assemblyName;

        public PluginPatchAttribute(string assemblyName)
        {
             this.assemblyName = assemblyName;
        }
    }
}