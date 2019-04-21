using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DroSleep.Core
{
    public class LightInterval : GenericInterval
    {
        public LightInterval(bool isLightOn, int startIndex, long startId, DateTimeOffset startTime, int endIndex, long endId, DateTimeOffset endTime)
            : base(startIndex, startId, startTime, endIndex, endId, endTime)
        {
            this.IsLightOn = isLightOn;
        }

        public bool IsLightOn { get; }

        public bool IsPartial => (this.EndId - this.StartId) < 710;

        public bool IsExcess => (this.EndId - this.StartId) > 730;

        public override string ToString()
        {
            if (this.IsPartial)
            {
                return $"{(this.IsLightOn ? "ON" : "OFF")}(partial)";
            }
            if (this.IsExcess)
            {
                return $"{(this.IsLightOn ? "ON" : "OFF")}(excess)";
            }
            return $"{(this.IsLightOn ? "ON" : "OFF")}({this.StartId}:{this.EndId})";
        }
    }
}