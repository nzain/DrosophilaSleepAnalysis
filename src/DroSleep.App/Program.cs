using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DroSleep.Core;

namespace DroSleep.App
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            if (args.Length == 0)
            {
                args = new string[]{ @"..\..\samples\Monitor10.txt" };
            }

            Console.Write($"Loading '{args[0]}'...");
            Monitor monitor = new Monitor(args[0]);
            System.Console.WriteLine(" [ok]");

            foreach (MonitorRow row in monitor.Data)
            {
                System.Console.WriteLine(row);
            }
        }
    }
}
