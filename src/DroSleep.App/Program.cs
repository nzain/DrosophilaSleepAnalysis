using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NLog.Conditions;
using NLog.Targets;
using DroSleep.Core;
using DroSleep.App.Functions;
using System.Globalization;

namespace DroSleep.App
{
    class Program
    {
        public const string ConfigFile = "DroSleep.ini";

        private static readonly ILogger Logger;

        static Program()
        {
            // Initialize Logging (NLog)
            var config = new NLog.Config.LoggingConfiguration();
            var logconsole = new ColoredConsoleTarget("logconsole");
            logconsole.RowHighlightingRules.Clear();
            logconsole.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(ConditionParser.ParseExpression("level == LogLevel.Debug"), ConsoleOutputColor.Gray, ConsoleOutputColor.Black));
            logconsole.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(ConditionParser.ParseExpression("level == LogLevel.Info"), ConsoleOutputColor.White, ConsoleOutputColor.Black));
            logconsole.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(ConditionParser.ParseExpression("level == LogLevel.Warn"), ConsoleOutputColor.Yellow, ConsoleOutputColor.Black));
            logconsole.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(ConditionParser.ParseExpression("level == LogLevel.Error"), ConsoleOutputColor.Red, ConsoleOutputColor.Black));
            //logconsole.Layout = "${time} ${level:format=FirstCharacter} ${logger:shortName=True}| ${message} ${exception}";
            logconsole.Layout = "${time} | ${message} ${exception}";
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logconsole);
            LogManager.Configuration = config;
            Logger = LogManager.GetLogger("App");
        }

        public static DroSleepConfiguration Config { get; private set; }

        static void Main(string[] args)
        {
#if Debug
            if (args.Length == 0 && File.Exists(@"..\..\samples\Monitor10.txt"))
            {
                args = new string[]{ @"..\..\samples\Monitor10.txt" };
            }
#endif
            if (args.Length == 0)
            {
                Console.Error.WriteLine("Usage: drag & drop a file onto this exe");
                Console.ReadLine();
                return;
            }

            // Read DroSleep.ini file
            if (DroSleepConfiguration.TryLoad(ConfigFile, out DroSleepConfiguration cfg))
            {
                Config = cfg;
                // apply decimal separator globally to simplify all ToString() calls
                var cul = new CultureInfo(CultureInfo.CurrentCulture.LCID);
                cul.NumberFormat.NumberDecimalSeparator = cfg.DecimalSeparator;
                CultureInfo.CurrentCulture = cul;
                CultureInfo.CurrentUICulture = cul;
            }
            else
            {
                Console.ReadLine();
                return;
            }

            foreach (string filename in args)
            {
                FileInfo fi = new FileInfo(filename);
                if (!fi.Exists)
                {
                    Logger.Error($"File not found: '{filename}'");
                    continue;
                }
                Analyze(fi);
            }
            Console.WriteLine("press 'enter' to exit");
            Console.ReadLine();
        }

        private static void Analyze(FileInfo file)
        {
            Logger.Info($"Loading '{file.Name}'...");
            Monitor monitor = new Monitor(file.FullName);
            string outfile = file.FullName.Replace(file.Extension, "-analysis.csv");
            Logger.Info($"Output: '{outfile}'");
            using (StreamWriter w = new StreamWriter(outfile))
            {
                foreach (IAnalysis analysis in EnumerateActiveAnalysis())
                {
                    Logger.Info($"Computing {analysis.Name}...");
                    string line = string.Join(Config.ColumnSeparator, analysis.Header(monitor));
                    w.WriteLine(line);
                    foreach (string[] columns in analysis.Run(monitor))
                    {
                        line = string.Join(Config.ColumnSeparator, columns);
                        w.WriteLine(line);
                    }
                    w.WriteLine(); // empty line to separate multiple analysis
                }
            }
            Logger.Info("done");
        }

        private static IEnumerable<IAnalysis> EnumerateActiveAnalysis()
        {
            if (Config.AnalyzeTotalSleep)
            {
                yield return new SleepBoutAnalysis(SleepBoutAnalysis.AnalysisMode.TotalSleep);
            }
            if (Config.AnalyzeSleepBoutCount)
            {
                yield return new SleepBoutAnalysis(SleepBoutAnalysis.AnalysisMode.SleepBoutCount);
            }
            if (Config.AnalyzeSleepBoutDurationAvg)
            {
                yield return new SleepBoutAnalysis(SleepBoutAnalysis.AnalysisMode.SleepBoutDurationAvg);
            }
            if (Config.AnalyzeSleepBoutDurationMedian)
            {
                yield return new SleepBoutAnalysis(SleepBoutAnalysis.AnalysisMode.SleepBoutDurationMedian);
            }
        }
    }
}
