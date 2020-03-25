using AppKit;
using System.Collections.Generic;

namespace Barista.Views.Preferences
{
    public class PreferencesWindowFactory
    {
        private readonly List<IPreferencesTab> _tabControllers = new List<IPreferencesTab>();
        private static PreferencesWindowController _preferencesWindowController;

        private void InitializeTabs()
        {
            if (_tabControllers.Count == 0)
            {
                var general = new GeneralViewController();
                var plugins = new PluginViewController();

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
