using Barista.MacOS.Models;
using Barista.MacOS.Services;
using Foundation;

namespace Barista.MacOS.ViewModels
{
    [Register("GeneralPreferencesViewModel")]
    public class GeneralPreferencesViewModel : NSObject
    {
        private readonly ISettingsService _settingsService;
        private readonly AppSettings _settings;

        public GeneralPreferencesViewModel(ISettingsService settingsService)
        {
            _settingsService = settingsService;

            _settings = _settingsService.GetSettings();
        }

        [Export("PluginDirectory")]
        public string PluginDirectory
        {
            get => _settings.PluginDirectory;
            set
            {
                WillChangeValue("PluginDirectory");
                _settings.PluginDirectory = value;
                var _ = _settingsService.Save(_settings);
                DidChangeValue("PluginDirectory");
            }
        }
    }
}
