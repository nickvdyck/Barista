using System;
using System.Collections.Generic;
using System.Linq;
using Cronos;

namespace Barista.Core.Jobs
{
    internal class Schedule
    {
        public DateTime NextRun { get; internal set; }

        public string Name { get; internal set; }

        public bool Disabled { get; private set; }

        internal List<IJob> Jobs { get; private set; }

        internal Func<DateTime, DateTime> CalculateNextRun { get; set; }

        internal bool PendingRunOnce { get; set; }

        internal bool Immediate { get; set; }

        public Schedule(IJob job) : this(new[] { job }) { }

        public Schedule(IEnumerable<IJob> jobs)
        {
            Disabled = false;
            Jobs = jobs.ToList();
        }

        public void Disable()
        {
            Disabled = true;
        }

        public void Enable()
        {
            Disabled = false;
        }
    }
}
