using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DroSleep.Core;
using NLog;
using NLog.Conditions;
using NLog.Targets;

namespace DroSleep.App.Functions
{
    public class SleepBoutAnalysis : IAnalysis
    {
        public enum AnalysisMode
        {
            /// <Summary>Count the number of sleep cycles per interval.</Summary>
            SleepBoutCount,

            /// <Summary>Sum [minutes] of the sleep cycles per interval.</Summary>
            TotalSleep,
            
            /// <Summary>Average sleep duration [minutes] per interval.</Summary>
            SleepBoutDurationAvg,
            
            /// <Summary>Median sleep duration [minutes] per interval.</Summary>
            SleepBoutDurationMedian,
        }

        private static DroSleepConfiguration Cfg => Program.Config;

        public SleepBoutAnalysis(AnalysisMode mode)
        {
            this.Mode = mode;
        }

        public AnalysisMode Mode { get; }

        public string Name => this.Mode.ToString();

        public IEnumerable<string> Header(Monitor monitor)
        {
            yield return "FromID";
            yield return "ToID";
            if (Cfg.AnaylsisIntervalByLight)
            {
                yield return "Light";
            }
            int cols = monitor.DataColumns;
            for (int col = 0; col < cols; col++)
            {
                switch (this.Mode)
                {
                    case AnalysisMode.SleepBoutCount:
                        yield return $"Bouts-{col}";
                        break;
                    case AnalysisMode.TotalSleep:
                        yield return $"TotalSleep[min]-{col}";
                        break;
                    case AnalysisMode.SleepBoutDurationAvg:
                        yield return $"SleepBoutAvg[min]-{col}";
                        break;
                    case AnalysisMode.SleepBoutDurationMedian:
                        yield return $"SleepBoutMedian[min]-{col}";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(this.Mode.ToString());
                }
            }
        }

        public IEnumerable<string[]> Run(Monitor monitor)
        {
            int cols = monitor.DataColumns;
            TimeSpan inactivityThreshold = TimeSpan.FromMinutes(Cfg.SleepIndicatorDurationMin);
            List<MonitorRow> data = monitor.Data
                .Where(w => w.Id >= Cfg.FirstIdToAnalyze)
                .ToList();

            // // 1. compute all cycles
            DroSleepCycle[][] cycles = new DroSleepCycle[cols][];
            for (int col = 0; col < cols; col++)
            {
                cycles[col] = DroSleepCycle.Enumerate(data, col, inactivityThreshold).ToArray();
            }

            // 2. group by interval and compute output
            TimeSpan intervalLength = Cfg.AnalysisIntervalHours > 0
                ? TimeSpan.FromHours(Cfg.AnalysisIntervalHours)
                : TimeSpan.MaxValue;
            GenericInterval[] intervals = Cfg.AnaylsisIntervalByLight
                ? monitor.LightIntervals.ToArray()
                : GenericInterval.EnumerateIntervals(data, intervalLength).ToArray();
            foreach (GenericInterval interval in intervals)
            {
                yield return this.CreateRow(cycles, interval).ToArray();
            }
        }

        private IEnumerable<string> CreateRow(DroSleepCycle[][] cycles, GenericInterval interval)
        {
            yield return interval.StartId.ToString();
            yield return interval.EndId.ToString();
            if (interval is LightInterval li)
            {
                yield return li.ToString();
            }

            Func<DroSleepCycle, double> selector = s => 
            {
                // Charlotte: don't count partial intervals, if cross-border sleep less than threshold
                double partialSleep = s.SleepDurationPartial(interval).TotalMinutes;
                return partialSleep >= Program.Config.SleepIndicatorDurationMin
                    ? partialSleep 
                    : 0;
            };

            for (int col = 0; col < cycles.Length; col++)
            {
                DroSleepCycle[] items = cycles[col]
                    .Where(w => interval.IntersectsSleep(w))
                    .Where(w => selector(w) > 0)
                    .ToArray();
                switch (this.Mode)
                {
                    case AnalysisMode.SleepBoutCount:
                        yield return items.Length.ToString();
                        break;
                    case AnalysisMode.TotalSleep:
                        yield return items.Sum(selector).ToString();
                        break;
                    case AnalysisMode.SleepBoutDurationAvg:
                        yield return items.Length > 0 
                            ? items.Average(selector).ToString("F2")
                            : "N/A";
                        break;
                    case AnalysisMode.SleepBoutDurationMedian:
                        yield return items.Length > 0 
                            ? items.Median(selector).ToString() 
                            : "N/A";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(this.Mode.ToString());
                }
            }
        }
    }
}