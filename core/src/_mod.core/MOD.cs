using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

[assembly: AssemblyTitle("_mod.core")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

namespace dotNetMT
{
    public static class MOD
    {
        public static readonly string assemblyFile = Assembly.GetExecutingAssembly().Location;
        public static readonly string assemblyFolder = Path.GetDirectoryName(assemblyFile) + Path.DirectorySeparatorChar;
        public static readonly string startupFolder = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar;

        public static ConfigFile Config = new ConfigFile(Path.ChangeExtension(assemblyFile, ".ini"));

        static MOD()
        {
            var logName = Path.ChangeExtension(assemblyFile, ".log");
            File.Delete(logName);
            Logger.SetLogPath(logName);
            MOD.Log("Log started: " + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff"));

            AppDomain.CurrentDomain.ProcessExit += ClassDestructor;
        }

        static void ClassDestructor(object sender, EventArgs e)
        {
            Config.Save();
        }

        public static void Log(string message)
        {
            Logger.Write(message);
        }

        public static void LogWarning(string message)
        {
            Logger.Write("WARNING: " + message);
        }

        public static void LogError(string message)
        {
            Logger.Write("ERROR: " + message);
        }

        public static bool TryParseBool(string strVal, out bool boolVal)
        {
            if (bool.TryParse(strVal, out boolVal))
            {
                return true;
            }
            string a = strVal.ToLowerInvariant().Trim();
            if (a == "off" || a == "0" || a == "false")
            {
                boolVal = false;
                return true;
            }
            if (a == "on" || a == "1" || a == "true")
            {
                boolVal = true;
                return true;
            }
            boolVal = false;
            return false;
        }

        public static bool ForceBool(string strVal)
        {
            if (string.IsNullOrEmpty(strVal))
            {
                return false;
            }
            string a = strVal.ToLowerInvariant().Trim();
            return a == "on" || a == "1" || a == "true";
        }

        public static bool TryParseInt(string str, out int val)
        {
            return int.TryParse(str, NumberStyles.Any, null, out val);
        }

        public static int ForceInt(string str)
        {
            int result = 0;
            MOD.TryParseInt(str, out result);
            return result;
        }

        public static bool TryParseLong(string str, out long val)
        {
            return long.TryParse(str, NumberStyles.Any, null, out val);
        }

        public static long ForceLong(string str)
        {
            long result = 0L;
            MOD.TryParseLong(str, out result);
            return result;
        }

        public static bool TryParseULong(string str, out ulong val)
        {
            return ulong.TryParse(str, NumberStyles.Any, null, out val);
        }

        public static ulong ForceULong(string str)
        {
            ulong result = 0uL;
            MOD.TryParseULong(str, out result);
            return result;
        }

        public static bool TryParseFloat(string str, out float val)
        {
            return float.TryParse(str, NumberStyles.Any, null, out val);
        }

        public static float ForceFloat(string str)
        {
            float result = 0f;
            MOD.TryParseFloat(str, out result);
            return result;
        }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public sealed class RuntimeHookAttribute : Attribute
    {
        public string assemblyName;
        public string typeName;
        public string methodName;
        public bool hookOnBegin;

        public RuntimeHookAttribute(string assemblyName, string typeName, string methodName, bool hookOnBegin = true)
        {
            this.assemblyName = assemblyName;
            this.typeName = typeName;
            this.methodName = methodName;
            this.hookOnBegin = hookOnBegin;
        }
    }
}