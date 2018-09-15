using System;
using System.Collections.Generic;
using System.Linq;

namespace DroSleep.Core
{
    public class MonitorRow
    {
        public MonitorRow(string line)
        {
            //92526	14 Jun 18	14:39:00	1	0	0	0	0	0	1	4	4	2	1	7	0	2	4	2	0	0	0	0	0	2	0	0	0	0	0	3	0	3	1	0	1	0	0	0	0	0	0
            if (string.IsNullOrWhiteSpace(line))
            {
                throw new ArgumentException($"empty line: '{line}'");
            }
            string[] columns = line.Split('\t');
            if (columns.Length < 11)
            {
                throw new ArgumentException($"invalid line with {columns.Length} columns only: '{line}'");
            }
            this.Id = long.Parse(columns[0]);
            this.TimeStamp = DateTimeOffset.Parse(columns[1] + ' ' + columns[2]);
            // skip data[3..8] ?
            this.IsLightOn = columns[9] == "1";
            // rest: data, usually 32 but we support other sizes as well.
            List<int> data = new List<int>(32);
            for (int i = 10; i < columns.Length; i++)
            {
                data.Add(int.Parse(columns[i]));
            }
            this.BeamCrossings = data.ToArray();
        }

        /// <Summary>Gets the id of this row.</Summary>
        public long Id { get; }

        /// <Summary>Gets the date and time for this row.</Summary>
        public DateTimeOffset TimeStamp { get; }

        /// <Summary>Indicates that the light is on.</Summary>
        public bool IsLightOn { get; }
        
        /// <Summary>Gets the number of beam crossings since last row.</Summary>
        public int[] BeamCrossings { get; }
        
        public override string ToString()
        {
            return $"{this.Id,7} {this.TimeStamp:yyyy-MM-dd HH:mm} {(this.IsLightOn ? "ON " : "off")} [{string.Join(" ", this.BeamCrossings)}]";
        }
    }
}
