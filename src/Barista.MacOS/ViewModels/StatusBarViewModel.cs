using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AppKit;
using Barista.Common;
using Barista.Data;
using Barista.Data.Events;
using Barista.Data.Queries;
using Barista.MacOS.Views.Preferences;
using Foundation;

namespace Barista.MacOS.ViewModels
{
    public class StatusBarViewModel : NSObject, IEventHandler<PluginsUpdatedEvent>, IEventHandler<PluginExecutedEvent>
    {
        public IReadOnlyCollection<Plugin> Plugins
        {
            get => _pluginManager.ListPlugins().Where(plugin => !plugin.Disabled).ToList();
        }

        private readonly IPluginManager _pluginManager;
        private readonly PreferencesWindowFactory _preferencesWindowFactory;
        private readonly IMediator _mediator;
        public ObservableCollection<StatusItemViewModel> StatusItems = new ObservableCollection<StatusItemViewModel>();

        public StatusBarViewModel(IPluginManager pluginManager, PreferencesWindowFactory preferencesWindowFactory, IMediator mediator)
        {
            _pluginManager = pluginManager;
            _preferencesWindowFactory = preferencesWindowFactory;
            _mediator = mediator;
        }

        async Task IEventHandler<PluginExecutedEvent>.Handle(PluginExecutedEvent @event, CancellationToken cancellationToken)
        {
            var item = StatusItems.FirstOrDefault(s => s.Plugin.Name == @event.Name);
            var plugin = await _mediator.Send(new GetPluginQuery { Name = @event.Name });
            item.Plugin = plugin;

            if (@event.Success == true)
            {
                var titleItem = @event.Items.FirstOrDefault().FirstOrDefault();

                item.IconAndTitle = titleItem.Title;
                item.Color = titleItem.Color;
                item.LastExecution = @event.LastExecution;
                item.Items = @event.Items.Skip(1).ToList();
            }
            else
            {
                item.IconAndTitle = "⚠️";
                item.LastExecution = DateTime.Now;
                item.Items = @event.Items;
            }
        }

        async Task IEventHandler<PluginsUpdatedEvent>.Handle(PluginsUpdatedEvent @event, CancellationToken cancellationToken)
        {
            foreach (var removed in @event.Removed)
            {
                var item = StatusItems.FirstOrDefault(i => i.Plugin.Name == removed.Name);
                StatusItems.Remove(item);
            }

            var plugins = await _mediator.Send(new GetPluginsQuery());

            foreach (var plugin in plugins.Where(p => !p.Disabled))
            {
                var item = StatusItems.FirstOrDefault(i => i.Plugin.Name == plugin.Name);

                if (item == null)
                {
                    StatusItems.Add(new StatusItemViewModel(_pluginManager, _preferencesWindowFactory)
                    {
                        Plugin = plugin,
                    });
                }
            }

            foreach (var plugin in plugins.Where(p => p.Disabled))
            {
                var item = StatusItems.FirstOrDefault(i => i.Plugin.Name == plugin.Name);
                if (item != null) StatusItems.Remove(item);
            }
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
