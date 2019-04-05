using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DroSleep.Core
{
    public class GenericInterval : IEquatable<GenericInterval>
    {
        public static IEnumerable<GenericInterval> EnumerateIntervals(IList<MonitorRow> data, TimeSpan intervalLength)
        {
            MonitorRow start = null;
            int i0 = -1;
            MonitorRow end = null;
            for (int i = 0; i < data.Count; i++)
            {
                MonitorRow row = data[i];
                end = row;
                if (start == null)
                {
                    start = row;
                    i0 = i;
                }
                else if (row.TimeStamp - start.TimeStamp >= intervalLength)
                {
                    yield return new GenericInterval(i0, start.Id, start.TimeStamp, i, end.Id, end.TimeStamp);
                    start = row;
                    i0 = i;
                    //start = null;
                }
            }

            // TODO last interval falls short.. what to do?
            // if (start != null && start.Id < end.Id)
            // {
            //     yield return new GenericInterval(startIndex, start.Id, monitor.Data.Count - 1, end.Id);
            // }
        }

        public GenericInterval(int startIndex, long startId, DateTimeOffset startTime, int endIndex, long endId, DateTimeOffset endTime)
        {
            this.StartIndex = startIndex;
            this.StartId = startId;
            this.StartTime = startTime;
            this.EndIndex = endIndex;
            this.EndId = endId;
            this.EndTime = endTime;
        }

        public int StartIndex { get; }

        public long StartId { get; }

        public DateTimeOffset StartTime { get; }

        public int EndIndex { get; }

        public long EndId { get; }

        public DateTimeOffset EndTime { get; }

        public bool Contains(MonitorRow row)
        {
            return this.StartId <= row.Id && row.Id < this.EndId;
        }

        public bool IntersectsSleep(DroSleepCycle cycle)
        {
            long indexLow = Math.Max(cycle.SleepStart.Id, this.StartId);
            long indexHi = Math.Min(cycle.CycleEnd.Id, this.EndId);
            return indexLow < indexHi; // something is inside
        }

        public bool Equals(GenericInterval other)
        {
            return other != null
                && other.StartId == this.StartId
                && other.StartIndex == this.StartIndex
                && other.EndId == this.EndId
                && other.EndIndex == this.StartIndex;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as GenericInterval);
        }

        public override int GetHashCode()
        {
            unchecked { return (int)((this.StartId * 397) ^ this.EndId); }
        }

        public override string ToString()
        {
            return $"from {this.StartId} to {this.EndId}";
        }
    }
}