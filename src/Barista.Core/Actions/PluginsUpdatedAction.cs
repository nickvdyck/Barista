using System.Collections.Immutable;
using Barista.Common.Redux;
using Barista.Data;

namespace Barista.Actions
{
    public class PluginsUpdatedAction : IAction
    {
        public ImmutableList<Plugin> Plugins { get; set; }
    }
}
