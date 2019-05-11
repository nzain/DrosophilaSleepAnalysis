namespace DroSleep.App.Functions
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
}