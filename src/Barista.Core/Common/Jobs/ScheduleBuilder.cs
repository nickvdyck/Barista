using System;
using System.Collections.Generic;
using Cronos;

namespace Barista.Common.Jobs
{
    internal class ScheduleBuilder
    {
        private readonly Action<Schedule> _buildSchedule;
        private readonly List<IJob> _jobs = new List<IJob>();

        private string Name { get; set; } = Guid.NewGuid().ToString();

        private Func<DateTime, DateTime> CalculateNextRun { get; set; }

        private bool PendingRunOnce { get; set; }

        private bool Immediate { get; set; }
        private bool Disabled { get; set; }

        public ScheduleBuilder(Action<Schedule> buildSchedule)
        {
            _buildSchedule = buildSchedule;
        }

        public ScheduleBuilder AndThen(IJob job)
        {
            if (job == null)
                throw new ArgumentNullException(nameof(job));

            _jobs.Add(job);
            return this;
        }

        public ScheduleBuilder ToRunNow()
        {
            Immediate = true;
            return this;
        }

        public ScheduleBuilder ToRunAt(CronExpression cron)
        {
            CalculateNextRun = now => cron.GetNextOccurrence(now, true) ?? DateTime.MaxValue;
            return this;
        }

        public ScheduleBuilder ToRunOnce()
        {
            PendingRunOnce = true;
            return this;
        }

        public ScheduleBuilder WithName(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                Name = name;
            }
            return this;
        }

        public ScheduleBuilder SetDisabled(bool disabled = false)
        {
            Disabled = disabled;
            return this;
        }

        public void Start()
        {
            var schedule = new Schedule(_jobs)
            {
                Name = Name,
                CalculateNextRun = CalculateNextRun,
                PendingRunOnce = PendingRunOnce,
                Immediate = Immediate
            };

            if (Disabled)
            {
                schedule.Disable();
            }

            _buildSchedule(schedule);
        }
    }
}
