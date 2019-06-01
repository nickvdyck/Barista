using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cronos;

namespace Barista.Core.Jobs
{
    public class Schedule
    {
        public DateTime NextRun { get; internal set; }

        public string Name { get; internal set; } = Guid.NewGuid().ToString();

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

        public Schedule AndThen(IJob job)
        {
            if (job == null)
                throw new ArgumentNullException(nameof(job));

            Jobs.Add(job);
            return this;
        }

        public Schedule ToRunNow()
        {
            Immediate = true;
            return this;
        }

        public Schedule ToRunAt(CronExpression cron)
        {
            CalculateNextRun = now => cron.GetNextOccurrence(now, true) ?? DateTime.MaxValue;
            return this;
        }

        public Schedule ToRunOnce()
        {
            PendingRunOnce = true;
            return this;
        }

        public Schedule WithName(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                Name = name;
            }
            return this;
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
