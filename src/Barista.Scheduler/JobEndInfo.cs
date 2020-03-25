using System;

namespace Barista.Scheduler
{
    public class JobEndInfo
    {
        public string Name { get; set; }
        public DateTime StartTime { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTime NextRun { get; set; }
    }
}
