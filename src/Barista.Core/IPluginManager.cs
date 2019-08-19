using System;
using System.Collections.Generic;
using Barista.Data;

namespace Barista
{
    public interface IPluginManager : IDisposable
    {
        IReadOnlyCollection<Plugin> ListPlugins();
        void Start();
        void Stop();
        void Execute(Plugin plugin);
        void Execute(Item item);
    }
}
