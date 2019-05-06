using System;
using System.Threading.Tasks;
using Barista.MacOS.Models;
using Foundation;

namespace Barista.MacOS.Services
{
    public class DefaultsService : ISettingsService
    {
        private readonly NSUserDefaults _defaults;
        public DefaultsService()
        {
            _defaults = NSUserDefaults.StandardUserDefaults;
        }

        public AppSettings GetSettings()
        {
            return new AppSettings
            {
                PluginDirectory = _defaults.StringForKey("pluginDirectory"),
            };
        }

        public Task Save(AppSettings settings)
        {
            _defaults.SetString(settings.PluginDirectory, "pluginDirectory");

            return Task.CompletedTask;
        }
    }
}
