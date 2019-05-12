using System.Collections.Immutable;
using Barista.Core.Data;

namespace Barista.Core.Providers
{
    internal interface IPluginProvider
    {
        ImmutableList<Plugin> ListPlugins();
    }
}
