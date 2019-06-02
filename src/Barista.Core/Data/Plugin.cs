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
        public bool Disabled { get; internal set; }
        internal CronExpression Cron { get; set; }
    }
}
