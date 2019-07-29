using AppKit;
using System.Collections.Generic;
using TinyIoC;

namespace Barista.MacOS.Views.Preferences
{
    public class PreferencesWindowFactory
    {
        private readonly TinyIoCContainer _container;
        private readonly List<IPreferencesTab> _tabControllers = new List<IPreferencesTab>();
        private static PreferencesWindowController _preferencesWindowController;

        public PreferencesWindowFactory(TinyIoCContainer container)
        {
            _container = container;
        }

        private void InitializeTabs()
        {
            if (_tabControllers.Count == 0)
            {
                var general = _container.Resolve<GeneralViewController>();
                var plugins = _container.Resolve<PluginViewController>();

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
