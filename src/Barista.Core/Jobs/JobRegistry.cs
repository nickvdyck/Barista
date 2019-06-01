using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Barista.Core.Jobs
{
    public class JobRegistry
    {
        internal List<Schedule> Schedules { get; private set; }

        public JobRegistry()
        {
            Schedules = new List<Schedule>();
        }

        public Schedule Schedule(IJob job)
        {
            return Schedule(job, null);
        }

        public Schedule Schedule(Func<Task> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var job = new FuncJob(action);
            return Schedule(job, null);
        }

        private Schedule Schedule(IJob job, string name)
        {
            var schedule = new Schedule(job).WithName(name);

            lock (((ICollection)Schedules).SyncRoot)
            {
                Schedules.Add(schedule);
            }

            return schedule;
        }
    }
}
