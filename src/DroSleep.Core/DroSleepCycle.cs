using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DroSleep.Core
{
    public class DroSleepCycle
    {
        public static IEnumerable<DroSleepCycle> Enumerate(IEnumerable<MonitorRow> rows, int index, TimeSpan inactivityThreshold)
        {
            MonitorRow cycleStart = null;
            int beamCrossingsBeforeSleep = 0;
            MonitorRow sleepStart = null;
            MonitorRow cycleEnd = null;
            foreach (MonitorRow row in rows)
            {
                int activity = row.BeamCrossings[index];
                // 1) check for completion
                if (sleepStart != null && row.TimeStamp - sleepStart.TimeStamp >= inactivityThreshold && activity > 0)
                {
                    // end of interval!
                    yield return new DroSleepCycle(cycleStart, beamCrossingsBeforeSleep, sleepStart, cycleEnd);
                    // start next interval
                    cycleStart = null;
                    beamCrossingsBeforeSleep = 0;
                    sleepStart = null;
                    cycleEnd = null;
                }
                else if (sleepStart != null && activity > 0)
                {
                    sleepStart = null; // reset, not really sleep according to threshold
                }

                // 2) update
                if (cycleStart == null)
                {
                    cycleStart = row;
                }
                beamCrossingsBeforeSleep += activity;
                if (sleepStart == null && activity == 0) // eventually the start of a sleep cycle
                {
                    sleepStart = row;
                }
                cycleEnd = row;
            } // end-for
        }

        public DroSleepCycle(MonitorRow cycleStart, int beamCrossingsBeforeSleep, MonitorRow sleepStart, MonitorRow cycleEnd)
        {
            this.CycleStart = cycleStart ?? throw new ArgumentNullException(nameof(cycleStart));
            this.BeamCrossingsBeforeSleep = beamCrossingsBeforeSleep;
            this.SleepStart = sleepStart ?? throw new ArgumentNullException(nameof(sleepStart));
            this.CycleEnd = cycleEnd ?? throw new ArgumentNullException(nameof(cycleEnd));
        }

        public MonitorRow CycleStart { get; }

        public int BeamCrossingsBeforeSleep { get; }

        public MonitorRow SleepStart { get; }

        public MonitorRow CycleEnd { get; }

        public TimeSpan SleepDuration => this.CycleEnd.TimeStamp - this.SleepStart.TimeStamp;

        public override string ToString() 
        {
            TimeSpan active = this.SleepStart.TimeStamp - this.CycleStart.TimeStamp;
            TimeSpan sleeping = this.CycleEnd.TimeStamp - this.SleepStart.TimeStamp;
            return $"{this.BeamCrossingsBeforeSleep,3} crossings over {active} then sleeping for {sleeping}";
        }
    }
}