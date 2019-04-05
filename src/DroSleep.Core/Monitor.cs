using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DroSleep.Core
{
    public class Monitor
    {
        public Monitor(string filename)
        {
            if (!File.Exists(filename))
            {
                throw new FileNotFoundException(filename);
            }
            this.Data = new List<MonitorRow>();
            this.LightIntervals = new List<LightInterval>();
            using (var r = new StreamReader(filename))
            {
                string line;
                LightInterval interval = null;
                int index = 0;
                while ((line = r.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }
                    MonitorRow row = new MonitorRow(line); // might throw
                    if (interval == null)
                    {
                        interval = new LightInterval(row.IsLightOn, index, row.Id, row.TimeStamp, -1, -1L, default(DateTimeOffset));
                    }
                    else if (interval.IsLightOn != row.IsLightOn)
                    {
                        // finish the interval
                        int prevIndex = this.Data.Count - 1;
                        MonitorRow prevRow = this.Data[prevIndex];
                        this.LightIntervals.Add(new LightInterval(interval.IsLightOn, 
                            interval.StartIndex, interval.StartId, interval.StartTime,
                            prevIndex, prevRow.Id, prevRow.TimeStamp));
                        // and start next
                        interval = new LightInterval(row.IsLightOn, index, row.Id, row.TimeStamp, -1, -1L, default(DateTimeOffset));
                    }
                    this.Data.Add(row);
                    index++;
                }
                if (this.Data.Count > 1) 
                {
                    this.DataColumns = this.Data[0].BeamCrossings.Length;
                    // finish the last interval
                    int endIndex = this.Data.Count - 1;
                    MonitorRow endRow = this.Data[endIndex];
                    this.LightIntervals.Add(new LightInterval(interval.IsLightOn, 
                        interval.StartIndex, interval.StartId, interval.StartTime,
                        endIndex, endRow.Id, endRow.TimeStamp));
                }
            }
        }

        /// <Summary>Gets the data of this monitor.</Summary>
        public List<MonitorRow> Data { get; }

        /// <Summary>Gets the number of data columns.</Summary>
        public int DataColumns { get; }

        /// <Summary>Gets the light on/off intervals.</Summary>
        public List<LightInterval> LightIntervals { get; }
    }
}