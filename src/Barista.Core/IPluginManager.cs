using System;
using System.Collections.Generic;
using Barista.Core.Data;
using Barista.Core.Events;

namespace Barista.Core
{
    public interface IPluginManager
    {
        IReadOnlyCollection<Plugin> ListPlugins();
        void Execute(int interval = 500);
        void Execute(Plugin plugin);
        void Execute(Item item);
        IObservable<IPluginEvent> Monitor();
    }
}
