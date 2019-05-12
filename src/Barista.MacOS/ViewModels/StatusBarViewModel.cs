using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AppKit;
using Barista.Core;
using Barista.Core.Data;
using Barista.Core.Events;
using Barista.Core.Extensions;
using Barista.MacOS.Utils;
using Barista.MacOS.Views.Preferences;
using Foundation;

namespace Barista.MacOS.ViewModels
{
    public class StatusBarViewModel : NSObject
    {
        public IReadOnlyCollection<Plugin> Plugins
        {
            get => _pluginManager.ListPlugins().Where(plugin => plugin.Enabled).ToList();
        }

        private readonly IPluginManager _pluginManager;
        private readonly PreferencesWindowFactory _preferencesWindowFactory;
        public ObservableCollection<StatusItemViewModel> StatusItems = new ObservableCollection<StatusItemViewModel>();

        public StatusBarViewModel(IPluginManager pluginManager, PreferencesWindowFactory preferencesWindowFactory)
        {
            _pluginManager = pluginManager;
            _preferencesWindowFactory = preferencesWindowFactory;

            foreach (var plugin in Plugins)
            {
                StatusItems.Add(new StatusItemViewModel(pluginManager, preferencesWindowFactory)
                {
                    Plugin = plugin
                });
            }

            _pluginManager.Monitor().Subscribe(OnPluginMonitorEvent);
        }

        public void OnPluginMonitorEvent(IPluginEvent e)
        {
            switch(e)
            {
                case PluginExecutedEvent executedEvent:
                    OnPluginExecuted(executedEvent);
                    break;
            }
        }

        public void OnPluginExecuted(PluginExecutedEvent e)
        {
            var item = StatusItems.FirstOrDefault(s => s.Plugin.Name == e.Plugin.Name);

            item.Plugin = e.Plugin;

            var titleItem = e.Execution.Items.FirstOrDefault().FirstOrDefault();

            item.IconAndTitle = titleItem.Title;
            item.LastExecution = $"Updated {TimeAgo.Format(e.Plugin.LastExecution)}";
            item.Items = e.Execution.Items.Skip(1).ToList();
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
            foreach (var plugin in Plugins)
            {
                _pluginManager.Execute(plugin);
            }
        }

        public void OnExit() => NSApplication.SharedApplication.Terminate(NSApplication.SharedApplication);

        public new void Dispose()
        {

            base.Dispose();
        }
    }
}
