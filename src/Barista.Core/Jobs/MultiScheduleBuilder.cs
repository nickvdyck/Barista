using System;
using System.Collections.Generic;

namespace Barista.Core.Jobs
{
    internal class MultiScheduleBuilder
    {
        private List<Schedule> Schedules { get; set; }
        private readonly Action<List<Schedule>> _buildSchedules;

        public MultiScheduleBuilder(Action<List<Schedule>> buildSchedules)
        {
            _buildSchedules = buildSchedules;
            Schedules = new List<Schedule>();
        }

        public ScheduleBuilder Schedule(IJob job)
        {
            Action<Schedule> completed = (Schedule schedule) => Schedules.Add(schedule);
            var builder = new ScheduleBuilder(completed);
            return builder.AndThen(job);
        }

        public void Build()
        {
            _buildSchedules(Schedules);
        }
    }
}
