using System.IO;
using Barista.Data;
using Barista.Common.Jobs;
using Cronos;

namespace Barista.Tests.Builders
{
    public class PluginBuilder
    {
        private string FilePath { get; set; }
        private string Name { get; set; }
        private bool Disabled { get; set; } = false;
        private PluginType Type { get; set; } = PluginType.Unknown;
        private string Schedule { get; set; } = "1m";
        private CronExpression Cron { get; set; } = Barista.Common.Jobs.Cron.Minutely();

        public PluginBuilder SetFilePath(string path)
        {
            FilePath = path;
            Name = Path.GetFileNameWithoutExtension(path);
            return this;
        }

        public PluginBuilder Disable()
        {
            Disabled = true;
            return this;
        }

        public Plugin Build()
        {
            return new Plugin
            {
                FilePath = FilePath,
                Name = Name,
                Type = Type,
                Disabled = Disabled,
                Schedule = Schedule,
                Cron = Cron,
            };
        }
    }
}
