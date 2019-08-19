using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Barista.MacOS.ViewModels;

namespace Barista.MacOS.Views.StatusBar
{
    public class BaristaStatusBar : IDisposable
    {
        private readonly Dictionary<StatusItemViewModel, StatusBarItem> _items = new Dictionary<StatusItemViewModel, StatusBarItem>();

        public StatusBarViewModel ViewModel { get; private set; }

        public BaristaStatusBar(StatusBarViewModel viewModel)
        {
            ViewModel = viewModel;
            ViewModel.StatusItems.CollectionChanged += OnPluginCollectionChanged;

            foreach (var model in viewModel.StatusItems)
            {
                var item = new BaristaStatusBarItem(model);
                AddItem(model, item);
            }
        }

        public void OnPluginCollectionChanged(object sender, NotifyCollectionChangedEventArgs ev)
        {
            void OnItemRemoved()
            {
                if (ev.NewItems == null || ev.NewItems.Count == 0)
                {
                    foreach (StatusItemViewModel toRemove in ev.OldItems)
                    {
                        RemoveItem(toRemove);
                    }
                }
                else
                {
                    var removed = (ev.OldItems as List<object>).Except(ev.NewItems as List<object>).ToList();
                    foreach (StatusItemViewModel toRemove in removed)
                    {
                        RemoveItem(toRemove);
                    }
                }

            }

            void OnItemAdded()
            {
                foreach (StatusItemViewModel viewModel in ev.NewItems)
                {
                    var item = new BaristaStatusBarItem(viewModel);
                    AddItem(viewModel, item);
                }
            }

            switch (ev.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                    OnItemRemoved();
                    break;

                case NotifyCollectionChangedAction.Add:
                    OnItemAdded();
                    break;
            }
        }

        public void AddItem(StatusItemViewModel viewModel, StatusBarItem item)
        {
            _items.Add(viewModel, item);
        }

        public void RemoveItem(StatusItemViewModel viewModel)
        {
            if (_items.Remove(viewModel, out var item))
            {
                item.Dispose();
            }
        }

        public void Dispose()
        {
            foreach (var item in _items)
            {
                item.Value.Dispose();
                _items.Remove(item.Key);
            }
        }
    }
}
