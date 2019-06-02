using System;
using System.Collections.Generic;
using Barista.Core.Data;
using Barista.Core.Plugins.Events;

namespace Barista.Core
{
    public interface IPluginManager : IDisposable
    {
        IReadOnlyCollection<Plugin> ListPlugins();
        void Start();
        void Stop();
        void Execute(Plugin plugin);
        void Execute(Item item);
        IObservable<IPluginEvent> Monitor();
    }
}
