using System;
using System.Collections.Generic;
using System.Linq;
using DroSleep.Core;
using NUnit.Framework;

namespace DroSleep.Tests
{
    public class MonitorRowTests
    {
        [Test]
        public void ParseRowTest()
        {
            const string line = @"92526	14 Jun 18	14:39:00	1	0	0	0	0	0	1	4	4	2	1	7	0	2	4	2	0	0	0	0	0	2	0	0	0	0	0	3	0	3	1	0	1	0	0	0	0	0	0";
            //                    ^id^^ ^date^^^^   ^time^^^    ^^^^unknown stuff^^^^   ^light(on/off)?
            //                                                                              ^ data ...
            MonitorRow sut = new MonitorRow(line);

            Assert.That(sut.Id, Is.EqualTo(92526), "id");
            Assert.That(sut.TimeStamp, Is.EqualTo(new DateTimeOffset(2018, 06, 14, 14, 39, 00, DateTimeOffset.Now.Offset)), "timestamp");
            Assert.That(sut.TimeStamp.Hour, Is.EqualTo(14), "timestamp Hour (timezone)");
            Assert.That(sut.IsLightOn, Is.True, "light on");

            int[] expectedCrossings = {4,4,2,1,7,0,2,4,2,0,0,0,0,0,2,0,0,0,0,0,3,0,3,1,0,1,0,0,0,0,0,0};
            Assert.That(sut.BeamCrossings, Is.EqualTo(expectedCrossings), "beam crossings");
            Assert.That(sut.BeamCrossings, Has.Length.EqualTo(32));
        }
    }
}