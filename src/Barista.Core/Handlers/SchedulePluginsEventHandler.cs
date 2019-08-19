using System.Threading;
using System.Threading.Tasks;
using Barista.Common;
using Barista.Data.Events;
using Barista.Common.Jobs;
using Barista.Data.Commands;

namespace Barista.Handlers
{
    internal class SchedulePluginsEventHandler : IEventHandler<PluginsUpdatedEvent>
    {
        private readonly JobScheduler _scheduler;
        private readonly IPluginStore _store;
        private readonly IMediator _mediator;

        public SchedulePluginsEventHandler(JobScheduler scheduler, IPluginStore store, IMediator mediator)
        {
            _store = store;
            _scheduler = scheduler;
            _mediator = mediator;
        }

        public Task Handle(PluginsUpdatedEvent @event, CancellationToken cancellationToken = default)
        {
            // Removed schedule for deleted plugins
            foreach (var plugin in @event.Removed) _scheduler.RemoveSchedule(plugin.Name);

            foreach (var plugin in _store.Plugins)
            {
                // Update plugin schedule
                if (_scheduler.TryGetSchedule(plugin.Name, out var schedule))
                {
                    if (schedule.Disabled != plugin.Disabled)
                    {
                        schedule.Disabled = plugin.Disabled;
                        _scheduler.RunSchedule(schedule);
                    }
                }
                // Register new schedule
                else
                {
                    _scheduler.Schedule(() => _mediator.Send(new ExecutePluginCommand { PluginName = plugin.Name } ))
                        .WithName(plugin.Name)
                        .ToRunNow()
                        .ToRunAt(plugin.Cron)
                        .SetDisabled(plugin.Disabled)
                        .Start();
                }
            }

            return Task.CompletedTask;
        }
    }
}
