using System.Collections.Specialized;
using Barista.MacOS.ViewModels;
using Foundation;

namespace Barista.MacOS.Views.SystemStatusBar
{
    public class SystemStatusBar : NSObject
    {
        private readonly StatusBarViewModel ViewModel;

        public SystemStatusBar(StatusBarViewModel viewModel)
        {
            ViewModel = viewModel;

            ((INotifyCollectionChanged)ViewModel.Plugins).CollectionChanged += OnPluginCollectionChanged;
        }

        public void OnPluginCollectionChanged(object sender, NotifyCollectionChangedEventArgs ev)
        {
            if (ev.Action == NotifyCollectionChangedAction.Remove)
            {

                //ev.
            }

        }

        public void Show()
        {
            foreach (var execution in ViewModel.PluginExecutions)
            {
                var item = new StatusBarMenuItem(execution, ViewModel);

                item.Show();
            }
        }
    }
}
