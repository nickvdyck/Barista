using System.Threading;
using System.Threading.Tasks;
using Barista.Common.Jobs;
using Barista.Common.Redux;
using Barista.Events;
using Barista.State;
using MediatR;

namespace Barista.Handlers
{
    internal class SchedulePluginsHandler : INotificationHandler<SchedulePlugins>
    {
        private readonly JobScheduler _scheduler;
        private readonly IMediator _mediator;

        public SchedulePluginsHandler(JobScheduler scheduler, IMediator mediator)
        {
            _scheduler = scheduler;
            _mediator = mediator;
        }

        public Task Handle(SchedulePlugins @event, CancellationToken cancellationToken)
        {
            foreach (var plugin in @event.Remove) _scheduler.RemoveSchedule(plugin.Name);

            foreach (var plugin in @event.Add)
            {
                _scheduler.Schedule(() =>
                        _mediator.Publish(new ExecutePlugin
                        {
                            Name = plugin.Name,
                            FilePath = plugin.FilePath,
                        })
                    )
                    .WithName(plugin.Name)
                    .ToRunNow()
                    .ToRunAt(plugin.Cron)
                    .SetDisabled(plugin.Disabled)
                    .Start();
            }

            return Task.CompletedTask;
        }
    }
}
