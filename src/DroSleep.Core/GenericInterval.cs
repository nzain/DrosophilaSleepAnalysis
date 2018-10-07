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
            int startIndex = -1;
            MonitorRow end = null;
            for (int i = 0; i < data.Count; i++)
            {
                MonitorRow row = data[i];
                end = row;
                if (start == null)
                {
                    start = row;
                    startIndex = i;
                }
                else if (end.TimeStamp - start.TimeStamp >= intervalLength)
                {
                    yield return new GenericInterval(startIndex, start.Id, i, end.Id);
                    start = null;
                }
            }

            // TODO last interval falls short.. what to do?
            // if (start != null && start.Id < end.Id)
            // {
            //     yield return new GenericInterval(startIndex, start.Id, monitor.Data.Count - 1, end.Id);
            // }
        }

        public GenericInterval(int startIndex, long startId, int endIndex, long endId)
        {
            this.StartIndex = startIndex;
            this.StartId = startId;
            this.EndIndex = endIndex;
            this.EndId = endId;
        }

        public int StartIndex { get; }

        public long StartId { get; }

        public int EndIndex { get; }

        public long EndId { get; }

        public bool Contains(MonitorRow row)
        {
            return this.StartId <= row.Id && row.Id <= this.EndId;
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