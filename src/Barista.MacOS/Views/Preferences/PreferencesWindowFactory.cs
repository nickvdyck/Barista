using AppKit;
using Barista.MacOS.Utils;
using System.Collections.Generic;

namespace Barista.MacOS.Views.Preferences
{
    public class PreferencesWindowFactory
    {
        private readonly ServiceProvider _provider;
        private readonly List<IPreferencesTab> _tabControllers = new List<IPreferencesTab>();
        private static PreferencesWindowController _preferencesWindowController;

        public PreferencesWindowFactory(ServiceProvider provider)
        {
            _provider = provider;
        }

        private void InitializeTabs()
        {
            if (_tabControllers.Count == 0)
            {
                var general = _provider.GetInstance<GeneralViewController>();
                var plugins = _provider.GetInstance<PluginViewController>();

                _tabControllers.Add(general);
                _tabControllers.Add(plugins);
            }
        }

        class PreferencesWindowDelegate : NSWindowDelegate
        {
            public override void WillClose(Foundation.NSNotification notification)
            {
                _preferencesWindowController.Dispose();
                _preferencesWindowController = null;
            }
        }

        public PreferencesWindowController Create()
        {
            InitializeTabs();

            if (_preferencesWindowController == null)
            {
                _preferencesWindowController = PreferencesWindowController.Create(_tabControllers);
                _preferencesWindowController.Window.Delegate = new PreferencesWindowDelegate();
            }

            return _preferencesWindowController;
        }
    }
}
