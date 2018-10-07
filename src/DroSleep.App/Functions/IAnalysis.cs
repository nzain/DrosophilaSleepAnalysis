using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DroSleep.Core;

namespace DroSleep.App.Functions
{
    public interface IAnalysis
    {
        string Name { get; }
        
        IEnumerable<string> Header(Monitor monitor);
        
        IEnumerable<string[]> Run(Monitor monitor);
    }
}