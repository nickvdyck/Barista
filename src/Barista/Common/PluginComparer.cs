using System;
using System.Collections.Generic;
using Barista.State;

namespace Barista.Common
{
    public class PluginComparer : IEqualityComparer<Plugin>
    {
        public bool Equals(Plugin x, Plugin y) => x.Metadata.Name == y.Metadata.Name;

        public int GetHashCode(Plugin obj) => obj.Metadata.Name.GetHashCode();
    }
}
