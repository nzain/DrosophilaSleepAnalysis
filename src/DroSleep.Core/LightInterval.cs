using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DroSleep.Core
{
    public class LightInterval : GenericInterval
    {
        public LightInterval(bool isLightOn, int startIndex, long startId, int endIndex, long endId)
            : base(startIndex, startId, endIndex, endId)
        {
            this.IsLightOn = isLightOn;
        }
        public bool IsLightOn { get; }

        public override string ToString()
        {
            return $"Light {(this.IsLightOn ? "ON " : "OFF")} from {this.StartId} to {this.EndId}";
        }
    }
}