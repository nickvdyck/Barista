using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Barista.Core.Jobs
{
    internal sealed class JobScheduler
    {
        private const uint MAXTIMERINTERVAL = (uint)0xfffffffe;

        private readonly Timer _timer;
        private readonly ConcurrentDictionary<string, Schedule> _schedules = new ConcurrentDictionary<string, Schedule>();
        private readonly ConcurrentDictionary<Schedule, Task> _running = new ConcurrentDictionary<Schedule, Task>();

        public event Action<JobExceptionInfo> JobException;
        public static event Action<JobStartInfo> JobStart;
        public static event Action<JobEndInfo> JobEnd;

        public JobScheduler()
        {
            _timer = new Timer(state => ScheduleJobs(), null, Timeout.Infinite, Timeout.Infinite);
        }

        public void Start()
        {
            ScheduleJobs();
        }

        public void Stop()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public async Task StopAsync()
        {
            Stop();

            // TODO: improve, this is clearly not the best way
            foreach (var runner in _running)
            {
                await runner.Value;
            }
        }

        public void StopAndBlock()
        {
            Stop();

            var tasks = new Task[0];

            // Even though Stop() was just called, a scheduling may be happening right now, that's why the loop.
            // Simply waiting for the tasks inside the lock causes a deadlock (a task may try to remove itself from
            // running, but it can't access the collection, it's blocked by the wait).
            do
            {
                tasks = _running.Select(t => t.Value).ToArray();
                Task.WaitAll(tasks);
            } while (tasks.Any());
        }

        public IReadOnlyCollection<Schedule> RunningSchedules
        {
            get
            {
                return _running.Select(t => t.Key).ToImmutableList();
            }
        }

        public IReadOnlyCollection<Schedule> AllSchedules
        {
            get
            {
                // returning a shallow copy
                return _schedules.Values.ToImmutableList();
            }
        }

        public ScheduleBuilder Schedule(IJob job) =>
            new ScheduleBuilder((Schedule schedule) =>
            {
                CalculateNextRun(new Schedule[] { schedule }).ToList().ForEach(this.ExecuteSchedule);
                ScheduleJobs();
            })
            .AndThen(job);

        public MultiScheduleBuilder ScheduleMany() =>
            new MultiScheduleBuilder((List<Schedule> schedules) =>
            {
                CalculateNextRun(schedules).ToList().ForEach(ExecuteSchedule);
            });

        public Schedule GetSchedule(string name)
        {
            if (_schedules.TryGetValue(name, out var schedule))
            {
                return schedule;
            }

            return null;
        }

        public void RemoveJob(string name)
        {
            _schedules.TryRemove(name, out var _);
        }

        private IEnumerable<Schedule> CalculateNextRun(IEnumerable<Schedule> schedules)
        {
            foreach (var schedule in schedules)
            {
                if (schedule.CalculateNextRun == null)
                {
                    yield return schedule;
                }
                else
                {
                    if (schedule.Immediate)
                    {
                        schedule.NextRun = DateTime.MinValue;
                    }
                    else
                    {
                        schedule.NextRun = schedule.CalculateNextRun(DateTime.UtcNow);
                    }
                    _schedules.TryAdd(schedule.Name, schedule);
                }
            }
        }

        private void ExecuteSchedule(Schedule schedule)
        {
            if (schedule.Disabled)
            {
                System.Diagnostics.Debug.WriteLine($"Disabled schedule {schedule.Name}");
                return;
            }

            // Add flag to schedule to allow some of them to run in parallel
            if (_running.TryGetValue(schedule, out var _))
                return;

            var task = RunJob(schedule);

            _running.TryAdd(schedule, task);
        }

        private async Task RunJob(Schedule schedule)
        {
            var start = DateTime.UtcNow;
            var stopWatch = new Stopwatch();

            JobStart?.Invoke(new JobStartInfo
            {
                Name = schedule.Name,
                StartTime = start,
            });

            try
            {
                stopWatch.Start();

                foreach (var job in schedule.Jobs.ToImmutableList())
                {
                    try
                    {
                        await job.Execute();
                    }
                    finally
                    {
                        DisposeIfNeeded(job);
                    }
                }
            }
            catch (Exception e)
            {
                if (JobException != null)
                {
                    if (e is AggregateException aggregate && aggregate.InnerExceptions.Count == 1)
                        e = aggregate.InnerExceptions.Single();

                    JobException(
                       new JobExceptionInfo
                       {
                           Name = schedule.Name,
                           Exception = e,
                       }
                   );
                }
            }
            finally
            {
                _running.TryRemove(schedule, out var _);

                JobEnd?.Invoke(new JobEndInfo
                {
                    Name = schedule.Name,
                    StartTime = start,
                    Duration = stopWatch.Elapsed,
                    NextRun = schedule.NextRun,
                });
            }

        }

        private void ScheduleJobs()
        {
            var schedules = _schedules
                                .Values
                                .ToImmutableList()
                                .Sort((x, y) => DateTime.Compare(x.NextRun, y.NextRun));

            if (!_schedules.Any())
                return;

            var firstJob = schedules.First();
            if (firstJob.NextRun <= DateTime.UtcNow)
            {
                ExecuteSchedule(firstJob);
                // TODO: it should not be possible for this to be null
                if (firstJob.CalculateNextRun != null)
                {
                    firstJob.NextRun = firstJob.CalculateNextRun(DateTime.UtcNow.AddMilliseconds(1));
                }

                if (firstJob.NextRun <= DateTime.UtcNow || firstJob.PendingRunOnce)
                {
                    // TODO: Add some error handling when removing failed
                    _schedules.TryRemove(firstJob.Name, out var removed);
                }

                firstJob.PendingRunOnce = false;
                ScheduleJobs();
                return;
            }

            var interval = firstJob.NextRun - DateTime.UtcNow;

            if (interval <= TimeSpan.Zero)
            {
                ScheduleJobs();
                return;
            }
            else
            {
                if (interval.TotalMilliseconds > MAXTIMERINTERVAL)
                    interval = TimeSpan.FromMilliseconds(MAXTIMERINTERVAL);

                _timer.Change(interval, interval);
            }
        }

        private void DisposeIfNeeded(IJob job)
        {
            if (job is IDisposable disposable) disposable.Dispose();
        }
    }
}
