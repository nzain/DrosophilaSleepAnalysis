using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;

namespace DroSleep.App
{
    public class DroSleepConfiguration
    {
        // DroSleep.ini parameters:
        public string ColumnSeparator { get; } = ";";
        public string DecimalSeparator { get; } = ".";
        public long FirstIdToAnalyze { get; } = -1;
        public int SleepIndicatorDurationMin { get; } = 10;
        public int AnalysisIntervalHours { get; } = -1;
        public bool AnaylsisIntervalByLight { get; } = true;

        public bool AnalyzeTotalSleep { get; } = false;
        public bool AnalyzeSleepBoutCount { get; } = false;
        public bool AnalyzeSleepBoutDurationAvg { get; } = false;
        public bool AnalyzeSleepBoutDurationMedian { get; } = false;


        // class and logic
        private static readonly ILogger Logger = LogManager.GetLogger("Configuration");

        public static bool TryLoad(string filename, out DroSleepConfiguration config)
        {
            if (!File.Exists(filename))
            {
                Logger.Error($"Configuration file '{new FileInfo(filename).FullName}' not found.");
                config = null;
                return false;
            }
            config = new DroSleepConfiguration(filename);
            return true;
        }

        public DroSleepConfiguration(string filename)
        {
            Logger.Info($"--- Loading '{Program.ConfigFile}' ---------------------------------");
            Dictionary<string, string> content = new Dictionary<string, string>();
            using (var r = new StreamReader(filename))
            {
                string line;
                while((line = r.ReadLine()) != null)
                {
                    ReadLine(line.Trim(), content);
                }
            }
            // now consume the dictionary, removing all correct entries
            this.ColumnSeparator = GetString("ColumnSeparator", content, ";");
            this.DecimalSeparator = GetString("DecimalSeparator", content, ".");
            this.FirstIdToAnalyze = GetLong("FirstIdToAnalyze", content);
            this.SleepIndicatorDurationMin = GetInt("SleepIndicatorDurationMin", content, 5);
            
            string analysisInterval = GetString("AnalysisIntervalHours", content, "light");
            if (analysisInterval.Equals("light", StringComparison.OrdinalIgnoreCase))
            {
                this.AnalysisIntervalHours = -1;
                this.AnaylsisIntervalByLight = true;
            }
            else if (int.TryParse(analysisInterval, out int intervalHours))
            {
                this.AnalysisIntervalHours = intervalHours;
                this.AnaylsisIntervalByLight = false;
            }

            this.AnalyzeTotalSleep = GetBool("AnalyzeTotalSleep", content);
            this.AnalyzeSleepBoutCount = GetBool("AnalyzeSleepBoutCount", content);
            this.AnalyzeSleepBoutDurationAvg = GetBool("AnalyzeSleepBoutDurationAvg", content);
            this.AnalyzeSleepBoutDurationMedian = GetBool("AnalyzeSleepBoutDurationMedian", content);

            // Warning about all others (not used/understood)
            foreach (var kvp in content)
            {
                Logger.Warn($" {Program.ConfigFile} unknown key/value: {kvp.Key} = {kvp.Value}");
            }
            Logger.Debug(new string('-', 60));
        }

        private static void ReadLine(string line, Dictionary<string,string> content)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return; // no error
            }
            if (line.StartsWith('#') || line.StartsWith(';'))
            {
                return; // comment, just skip ahead
            }
            string[] split = line.Split('=');
            if (split.Length != 2)
            {
                Logger.Error($" {Program.ConfigFile} invalid line: '{line}'");
                return;
            }
            string key = split[0].Trim();
            string val = split[1].Trim();
            if (string.IsNullOrWhiteSpace(key))
            {
                Logger.Error($" {Program.ConfigFile} empty key: '{line}'");
                return;
            }
            if (content.ContainsKey(key))
            {
                Logger.Error($" {Program.ConfigFile} duplicate key: '{key}'");
                return;
            }
            content.Add(key, val);
        }

        private static string GetString(string key, Dictionary<string, string> content, string defaultValue = "")
        {
            if (content.TryGetValue(key, out string result) && !string.IsNullOrWhiteSpace(result))
            {
                content.Remove(key);
                result = result.Trim('"').Trim('\'');
                Logger.Debug($" Config {key} = '{result}'");
                return result;
            }
            return defaultValue;
        }

        private static bool GetBool(string key, Dictionary<string, string> content, bool defaultValue = false)
        {
            if (content.TryGetValue(key, out string val) && bool.TryParse(val, out bool result))
            {
                content.Remove(key);
                Logger.Debug($" Config {key} = {result}");
                return result;
            }
            return defaultValue;
        }

        private static int GetInt(string key, Dictionary<string, string> content, int defaultValue = -1)
        {
            if (content.TryGetValue(key, out string val) && int.TryParse(val, out int result))
            {
                content.Remove(key);
                Logger.Debug($" Config {key} = {result}");
                return result;
            }
            return defaultValue;
        }

        private static long GetLong(string key, Dictionary<string, string> content, long defaultValue = -1)
        {
            if (content.TryGetValue(key, out string val) && long.TryParse(val, out long result))
            {
                content.Remove(key);
                Logger.Debug($" Config {key} = {result}");
                return result;
            }
            return defaultValue;
        }
    }
}