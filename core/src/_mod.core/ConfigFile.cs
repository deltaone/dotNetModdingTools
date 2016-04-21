using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace dotNetMT
{
    public class ConfigFile
    {
        public enum LineType
        {
            UNKNOWN,
            COMMENT,
            SECTION,
            ENTRY
        }

        public class Line
        {
            public string raw = string.Empty;
            public ConfigFile.LineType type;
            public string section = string.Empty;
            public string key = string.Empty;
            public string fullKey = string.Empty;
            public string value = string.Empty;
            public bool quoteValue;
        }

        private string path;

        private List<ConfigFile.Line> lines = new List<ConfigFile.Line>();

        public ConfigFile()
        {

        }

        public ConfigFile(string path)
        {
            FullLoad(path);
        }

        public string GetPath()
        {
            return (path);
        }

        public bool LightLoad(string path)
        {
            return (Load(path, true));
        }

        public bool FullLoad(string path)
        {
            return (Load(path, false));
        }

        public bool Save(string path = null)
        {
            if (path == null) path = this.path;
            if (path == null)
            {
                MOD.LogError("ConfigFile.Save() - no path given");
                return (false);
            }
            string contents = GenerateText();
            try
            {
                File.WriteAllText(path, contents);
            }
            catch (Exception ex)
            {
                MOD.LogError(string.Format("ConfigFile.Save() - Failed to write file at {0}. Exception={1}", path, ex.Message));
                return (false);
            }
            this.path = path;
            return (true);
        }

        public bool Has(string key)
        {
            ConfigFile.Line line = FindEntry(key);
            return (line != null);
        }

        public bool Delete(string key, bool removeEmptySections = true)
        {
            int num = FindEntryIndex(key);
            if (num < 0) return (false);

            lines.RemoveAt(num);

            if (removeEmptySections)
            {
                int i;
                for (i = num - 1; i >= 0; i--)
                {
                    ConfigFile.Line line = lines[i];
                    if (line.type == ConfigFile.LineType.SECTION) break;
                    string value = line.raw.Trim();
                    if (!string.IsNullOrEmpty(value)) return (true);
                }
                int j;
                for (j = num; j < lines.Count; j++)
                {
                    ConfigFile.Line line2 = lines[j];
                    if (line2.type == ConfigFile.LineType.SECTION) break;
                    string value2 = line2.raw.Trim();
                    if (!string.IsNullOrEmpty(value2)) return (true);
                }
                lines.RemoveRange(i, j - i);
            }
            return (true);
        }

        public void Clear()
        {
            lines.Clear();
        }

        public string Get(string key, string defaultVal = "")
        {
            ConfigFile.Line line = FindEntry(key);
            if (line == null) return (defaultVal);
            return (line.value);
        }

        public bool Get(string key, bool defaultVal = false)
        {
            ConfigFile.Line line = FindEntry(key);
            if (line == null) return (defaultVal);
            return (MOD.ForceBool(line.value));
        }

        public int Get(string key, int defaultVal = 0)
        {
            ConfigFile.Line line = FindEntry(key);
            if (line == null) return (defaultVal);
            return (MOD.ForceInt(line.value));
        }

        public float Get(string key, float defaultVal = 0f)
        {
            ConfigFile.Line line = FindEntry(key);
            if (line == null) return (defaultVal);
            return (MOD.ForceFloat(line.value));
        }

        public bool Set(string key, object val)
        {
            string value = (val != null) ? val.ToString() : string.Empty;
            return (Set(key, value));
        }

        public bool Set(string key, bool val)
        {
            string value = (!val) ? "false" : "true";
            return (Set(key, value));
        }

        public bool Set(string key, string val)
        {
            ConfigFile.Line line = RegisterEntry(key);
            if (line == null) return (false);
            line.value = val;
            return (true);
        }

        public List<ConfigFile.Line> GetLines()
        {
            return (lines);
        }

        public List<string> GetKeys(string sectionName)
        {
            var list = new List<string>();

            int sectionIndex = FindSectionIndex(sectionName);
            if (sectionIndex < 0) return (list);

            for (int i = sectionIndex + 1; i < lines.Count; i++)
            {
                ConfigFile.Line lineCurrent = lines[i];
                if (lineCurrent.type == ConfigFile.LineType.SECTION) break;
                if (lineCurrent.type == ConfigFile.LineType.ENTRY) list.Add(lineCurrent.key);
            }
            return (list);
        }

        public string GenerateText()
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < lines.Count; i++)
            {
                ConfigFile.Line line = lines[i];
                ConfigFile.LineType type = line.type;
                if (type != ConfigFile.LineType.SECTION)
                {
                    if (type != ConfigFile.LineType.ENTRY) stringBuilder.Append(line.raw);
                    else if (line.quoteValue) stringBuilder.AppendFormat("{0} = \"{1}\"", line.key, line.value);
                    else stringBuilder.AppendFormat("{0} = {1}", line.key, line.value);
                }
                else
                {
                    stringBuilder.AppendFormat("[{0}]", line.section);
                }
                stringBuilder.AppendLine();
            }
            return stringBuilder.ToString();
        }

        private bool Load(string path, bool ignoreUselessLines)
        {
            this.path = path; // this.path = null;
            lines.Clear();

            if (!File.Exists(path))
            {
                MOD.LogError("Error loading config file " + path);
                return (false);
            }

            int num = 1;
            using (StreamReader streamReader = File.OpenText(path))
            {
                string section = string.Empty;

                while (streamReader.Peek() != -1)
                {
                    string raw = streamReader.ReadLine();
                    string trimmed = raw.Trim();
                    if (!ignoreUselessLines || trimmed.Length > 0)
                    {
                        bool flag = trimmed.Length > 0 && trimmed[0] == ';';
                        if (!ignoreUselessLines || !flag)
                        {
                            ConfigFile.Line line = new ConfigFile.Line();
                            line.raw = raw;
                            line.section = section;
                            if (flag) line.type = ConfigFile.LineType.COMMENT;
                            else if (trimmed.Length > 0)
                            {
                                if (trimmed[0] == '[')
                                {
                                    if (trimmed.Length < 2 || trimmed[trimmed.Length - 1] != ']')
                                    {
                                        MOD.LogWarning(string.Format("ConfigFile.Load() - invalid section \"{0}\" on line {1} in file {2}", raw, num, path));
                                        if (!ignoreUselessLines) lines.Add(line);
                                        continue;
                                    }
                                    line.type = ConfigFile.LineType.SECTION;
                                    section = (line.section = trimmed.Substring(1, trimmed.Length - 2));
                                    lines.Add(line);
                                    continue;
                                }
                                else
                                {
                                    int delimiter = trimmed.IndexOf('=');
                                    if (delimiter < 0)
                                    {
                                        MOD.LogWarning(string.Format("ConfigFile.Load() - invalid entry \"{0}\" on line {1} in file {2}", raw, num, path));
                                        if (!ignoreUselessLines) lines.Add(line);
                                        continue;
                                    }
                                    string key = trimmed.Substring(0, delimiter).Trim();
                                    string value = trimmed.Substring(delimiter + 1, trimmed.Length - delimiter - 1).Trim();
                                    if (value.Length > 2)
                                    {
                                        int index = value.Length - 1;
                                        if ((value[0] == '"' || value[0] == '“' || value[0] == '”') && (value[index] == '"' || value[index] == '“' || value[index] == '”'))
                                        {
                                            value = value.Substring(1, value.Length - 2);
                                            line.quoteValue = true;
                                        }
                                    }
                                    line.type = ConfigFile.LineType.ENTRY;
                                    line.fullKey = ((!string.IsNullOrEmpty(section)) ? string.Format("{0}.{1}", section, key) : key);
                                    line.key = key;
                                    line.value = value;
                                }
                            }
                            lines.Add(line);
                        }
                    }
                }
            }

            this.path = path;
            return (true);
        }

        private int FindSectionIndex(string sectionName)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                ConfigFile.Line line = lines[i];
                if (line.type == ConfigFile.LineType.SECTION)
                    if (line.section.Equals(sectionName, StringComparison.OrdinalIgnoreCase)) return (i);
            }
            return (-1);
        }

        private ConfigFile.Line FindEntry(string fullKey)
        {
            int dot = fullKey.IndexOf('.');
            if (dot < 0) fullKey = "main." + fullKey;

            int num = FindEntryIndex(fullKey);
            if (num < 0) return (null);
            return (lines[num]);
        }

        private int FindEntryIndex(string fullKey)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                ConfigFile.Line line = lines[i];
                if (line.type == ConfigFile.LineType.ENTRY)
                    if (line.fullKey.Equals(fullKey, StringComparison.OrdinalIgnoreCase)) return (i);
            }
            return (-1);
        }

        private ConfigFile.Line RegisterEntry(string fullKey)
        {
            if (string.IsNullOrEmpty(fullKey)) return (null);

            int dot = fullKey.IndexOf('.');
            if (dot < 0)
            {
                fullKey = "main." + fullKey;
                dot = 4;
            }

            string sectionName = fullKey.Substring(0, dot);
            string lineKey = string.Empty;
            if (fullKey.Length > dot + 1) lineKey = fullKey.Substring(dot + 1, fullKey.Length - dot - 1);

            ConfigFile.Line lineEntry = null;
            int sectionIndex = FindSectionIndex(sectionName);
            if (sectionIndex < 0)
            {
                if (lines.Count > 0)
                {
                    ConfigFile.Line lineEmpty = new ConfigFile.Line();
                    lineEmpty.section = lines[lines.Count - 1].section;
                    lines.Add(lineEmpty);
                }
                ConfigFile.Line lineSection = new ConfigFile.Line();
                lineSection.type = ConfigFile.LineType.SECTION;
                lineSection.section = sectionName;
                lines.Add(lineSection);
                lineEntry = new ConfigFile.Line();
                lineEntry.type = ConfigFile.LineType.ENTRY;
                lineEntry.section = sectionName;
                lineEntry.key = lineKey;
                lineEntry.fullKey = fullKey;
                lines.Add(lineEntry);
            }
            else
            {
                int i;
                for (i = sectionIndex + 1; i < lines.Count; i++)
                {
                    ConfigFile.Line lineCurrent = lines[i];
                    if (lineCurrent.type == ConfigFile.LineType.SECTION) break;
                    if (lineCurrent.type == ConfigFile.LineType.ENTRY)
                        if (lineCurrent.key.Equals(lineKey, StringComparison.OrdinalIgnoreCase))
                        {
                            lineEntry = lineCurrent;
                            break;
                        }
                }
                if (lineEntry == null)
                {
                    lineEntry = new ConfigFile.Line();
                    lineEntry.type = ConfigFile.LineType.ENTRY;
                    lineEntry.section = sectionName;
                    lineEntry.key = lineKey;
                    lineEntry.fullKey = fullKey;
                    lines.Insert(sectionIndex + 1, lineEntry); // lines.Insert(i, lineEntry);
                }
            }
            return (lineEntry);
        }
    }
}