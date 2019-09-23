using System.Collections.Immutable;
using Barista.Data;
using MediatR;

namespace Barista.Events
{
    public class SchedulePlugins : INotification
    {
        public ImmutableList<Plugin> Add { get; set; }
        public ImmutableList<Plugin> Remove { get; set; }
    }
}
