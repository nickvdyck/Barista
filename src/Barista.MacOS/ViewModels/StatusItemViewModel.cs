using System;
using System.Collections.Generic;
using AppKit;
using Barista.Core;
using Barista.Core.Data;
using Barista.MacOS.Utils;
using Barista.MacOS.Views.Preferences;
using Foundation;

namespace Barista.MacOS.ViewModels
{
    [Register("StatusItemViewModel")]
    public class StatusItemViewModel : NSObject
    {
        private string _iconAndTitle = "...";

        [Export("IconAndTitle")]
        public string IconAndTitle
        {
            get => _iconAndTitle;
            set
            {
                if (_iconAndTitle == value) return;

                WillChangeValue("IconAndTitle");
                _iconAndTitle = value;
                DidChangeValue("IconAndTitle");
            }
        }

        public string ExecutedTimeAgo
        {
            get
            {
                return $"Updated {TimeAgo.Format(LastExecution)}";
            }
        }

        public DateTime LastExecution { get; set; }
        public IReadOnlyCollection<IReadOnlyCollection<Item>> Items { get; set; }
        public Plugin Plugin { get; set; }

        private readonly IPluginManager _pluginManager;
        private readonly PreferencesWindowFactory _preferencesWindowFactory;

        public StatusItemViewModel(IPluginManager pluginManager, PreferencesWindowFactory preferencesWindowFactory)
        {
            _pluginManager = pluginManager;
            _preferencesWindowFactory = preferencesWindowFactory;
        }

        public void OnStatusItemClicked(Item item)
        {
            _pluginManager.Execute(item);
        }

        public void OnOpenPreferences()
        {
            var controller = _preferencesWindowFactory.Create();
            controller.Show();

            NSApplication.SharedApplication.ActivateIgnoringOtherApps(true);
            controller.Window.MakeKeyAndOrderFront(this);
        }

        public void OnRefreshAll()
        {
            foreach (var plugin in _pluginManager.ListPlugins())
            {
                _pluginManager.Execute(plugin);
            }
        }

        public void OnExit() => NSApplication.SharedApplication.Terminate(NSApplication.SharedApplication);
    }
}
