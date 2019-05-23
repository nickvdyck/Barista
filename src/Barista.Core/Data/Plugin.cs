using System;
using Cronos;

namespace Barista.Core.Data
{
    public class Plugin
    {
        public string FilePath { get; internal set; }
        public string Name { get; internal set; }
        public string Schedule { get; internal set; }
        public PluginType Type { get; internal set; }
        public DateTime LastExecution { get; internal set; } = DateTime.MinValue;
        public bool Enabled { get; set; } = true;
        internal int Interval { get; set; }
        internal CronExpression Cron { get; set; }
    }
}
