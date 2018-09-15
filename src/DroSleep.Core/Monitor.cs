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
            using (var r = new StreamReader(filename))
            {
                string line;
                while (!string.IsNullOrWhiteSpace(line = r.ReadLine()))
                {
                    MonitorRow row = new MonitorRow(line); // might throw
                    this.Data.Add(row);
                }
            }
        }

        /// <Summary>Gets the data of this monitor.</Summary>
        public List<MonitorRow> Data { get; }
    }
}