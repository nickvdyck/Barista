using System;
using Cronos;

namespace Barista.Data
{
    public class Plugin : IEquatable<Plugin>
    {
        // TODO: At the moment name should be unique, but this is not very flexible or extensible.
        // It can be very easy to create collisions, to make it more flexible I could use FilePath
        // But at the moment I support a feature were you can prefix your filename with and underscore to disable it
        // I like this feature and want to keep it in. Thus I need to find another to identify a plugin that allows for more flexibility.
        public string Name { get; internal set; }
        public string FilePath { get; internal set; }
        public string Schedule { get; internal set; }
        internal CronExpression Cron { get; set; }
        public PluginType Type { get; internal set; }
        public bool Disabled { get; internal set; }

        public bool Equals(Plugin other)
        {
            if (other is null) return false;

            return Name == other.Name;
        }

        public override bool Equals(object obj) => Equals(obj as Plugin);
        public override int GetHashCode() => (Name).GetHashCode();
    }
}
