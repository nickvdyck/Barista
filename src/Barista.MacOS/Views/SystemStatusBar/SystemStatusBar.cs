using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Barista.MacOS.ViewModels;
using Foundation;

namespace Barista.MacOS.Views.SystemStatusBar
{
    public class SystemStatusBar : NSObject
    {
        private readonly StatusBarViewModel ViewModel;

        private readonly List<StatusBarMenuItem> _menuItems = new List<StatusBarMenuItem>();

        public SystemStatusBar(StatusBarViewModel viewModel)
        {
            ViewModel = viewModel;
            ViewModel.StatusItems.CollectionChanged += OnPluginCollectionChanged;
        }

        public void OnPluginCollectionChanged(object sender, NotifyCollectionChangedEventArgs ev)
        {
            if (ev.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in ev.OldItems)
                {
                    var removed = _menuItems.FirstOrDefault(menu => menu.ViewModel == item);

                    if (removed != null)
                    {
                        removed.Dispose();
                        _menuItems.Remove(removed);
                    }
                }
            }

            if (ev.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var itemViewModel in ev.NewItems)
                {
                    var item = new StatusBarMenuItem((Barista.MacOS.ViewModels.StatusItemViewModel)itemViewModel);
                    _menuItems.Add(item);
                    item.Show();

                }
            }
        }

        public void Show()
        {
            ViewModel.Load();
        }
    }
}
