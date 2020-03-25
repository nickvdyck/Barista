using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Barista.Common;
using Barista.Domain;
using Barista.Scheduler;
using Stateless;

namespace Barista.State
{
    public class Plugin : IObservable<PluginExecutionResult>
    {
        private enum Trigger
        {
            Schedule,
            Ready,
            Execute,
            Stop,
        }

        private enum State
        {
            Initialized,
            Scheduling,
            Ready,
            Executing,
            Stopped,
        }

        private readonly JobScheduler _scheduler;
        private readonly StateMachine<State, Trigger> _machine;
        private readonly ReplaySubject<PluginExecutionResult> _results = new ReplaySubject<PluginExecutionResult>(1);

        public Plugin(PluginMetadata metadata, JobScheduler scheduler)
        {
            Metadata = metadata;

            _scheduler = scheduler;

            _machine = new StateMachine<State, Trigger>(State.Initialized);


            _machine.Configure(State.Initialized)
                .Permit(Trigger.Schedule, State.Scheduling);

            _machine.Configure(State.Scheduling)
                .Permit(Trigger.Ready, State.Ready)
                .Permit(Trigger.Stop, State.Stopped)
                .OnEntryAsync(OnSchedulePlugin);

            _machine.Configure(State.Ready)
                .Permit(Trigger.Execute, State.Executing)
                .Permit(Trigger.Stop, State.Stopped);

            _machine.Configure(State.Executing)
                .Permit(Trigger.Ready, State.Ready)
                .Permit(Trigger.Stop, State.Stopped)
                .OnEntryAsync(OnExecute);

            _machine.Configure(State.Stopped)
                .OnEntry(OnStop);

            Uuid = Guid.NewGuid();
        }

        public PluginMetadata Metadata { get; }

        public Guid Uuid { get; private set; }

        public bool IsActive => !Metadata.Disabled && !(Metadata.Runtime == PluginRuntime.Unknown);

        public bool IsExecuting => _machine.IsInState(State.Executing);

        public void Start()
        {
            _ = _machine.FireAsync(Trigger.Schedule);
        }

        public Task Execute() => _machine.FireAsync(Trigger.Execute);
        public void Stop()
        {
            if (!_machine.IsInState(State.Stopped))
            {
                _machine.Fire(Trigger.Stop);
            }
        }

        public IDisposable Subscribe(IObserver<PluginExecutionResult> observer) => _results.Subscribe(observer);

        private async Task OnSchedulePlugin()
        {
            if (!IsActive)
            {
                _machine.Fire(Trigger.Stop);
                return;
            }

            var schedule = _scheduler.Schedule(async () =>
                {
                    try
                    {

                        await _machine.FireAsync(Trigger.Execute);
                    }
                    catch
                    {
                        // Probably not the best idea, technically we should never get here
                        _machine.Fire(Trigger.Ready);
                        System.Diagnostics.Debug.WriteLine("something gone wrong");
                    }
                })
                .WithName(Metadata.Name)
                .ToRunNow()
                .ToRunAt(Metadata.Cron)
                .SetDisabled(Metadata.Disabled);

            await _machine.FireAsync(Trigger.Ready);
            schedule.Start();
        }

        private async Task OnExecute()
        {
            System.Diagnostics.Debug.WriteLine($"Executing plugin {Metadata.Name}");

            var info = new ProcessStartInfo
            {
                FileName = Metadata.FilePath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8,
            };

            (string Data, string Error) result;

            try
            {
                using var process = Process.Start(info);
                var data = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();
                process.WaitForExit();

                result = (Data: data, Error: error);
            }
            catch (Exception ex)
            {
                result = (Data: "", Error: ex.ToString());
            }

            var execution = new PluginExecutionResult
            {
                LastExecution = DateTime.UtcNow,
            };

            if (!string.IsNullOrEmpty(result.Data))
            {
                var items = PluginParser.ParseExecution(result.Data, Metadata.Name);
                execution.Items = items;
                execution.Success = true;
            }
            else
            {
                execution.Items = ImmutableList.CreateBuilder<ImmutableList<Item>>().ToImmutableList();
                execution.Success = false;
            }

            System.Diagnostics.Debug.WriteLine($"Executed plugin {Metadata.Name}, Items: {execution.Items.Count}, Success: {execution.Success}");

            _results.OnNext(execution);

            _machine.Fire(Trigger.Ready);
        }

        private void OnStop()
        {
            _scheduler.RemoveSchedule(Metadata.Name);
            _results.OnCompleted();
        }
    }
}
