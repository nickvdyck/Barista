using System;
using Cronos;

namespace Barista.Domain
{
    public class PluginMetadata : IEquatable<PluginMetadata>
    {
        public string Name { get; set; }
        public string FilePath { get; set; }
        public string Schedule { get; set; }
        internal CronExpression Cron { get; set; }
        public PluginRuntime Runtime{ get; set; }
        public bool Disabled { get; set; }
        public string Checksum { get; set; }

        public bool Equals(PluginMetadata other)
        {
            if (other is null) return false;

            return (
                Name == other.Name &&
                FilePath == other.FilePath &&
                Schedule == other.Schedule &&
                Runtime == other.Runtime &&
                Disabled == other.Disabled &&
                Checksum == other.Checksum
            );
        }

        public override bool Equals(object obj) => Equals(obj as PluginMetadata);
        public override int GetHashCode() => (Name).GetHashCode();

        public static bool operator ==(PluginMetadata obj1, PluginMetadata obj2)
        {
            if (ReferenceEquals(obj1, obj2))
            {
                return true;
            }

            if (ReferenceEquals(obj1, null))
            {
                return false;
            }

            if (ReferenceEquals(obj2, null))
            {
                return false;
            }

            // Equals handles the case of null on right side.
            return obj1.Equals(obj2);
        }

        public static bool operator !=(PluginMetadata obj1, PluginMetadata obj2)
        {
            return !(obj1 == obj2);
        }
    }
}
