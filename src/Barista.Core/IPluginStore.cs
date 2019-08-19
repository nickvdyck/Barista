using System.Collections.Generic;
using Barista.Data;

namespace Barista
{
    internal interface IPluginStore
    {
        IReadOnlyCollection<Plugin> Plugins { get; }

        void Update(IReadOnlyCollection<Plugin> plugins);
    }
}
