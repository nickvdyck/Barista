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
        }

        public void Show()
        {
            foreach (var plugin in ViewModel.Plugins)
            {
                var item = new StatusBarMenuItem(plugin, ViewModel);

                item.Show();
            }
        }
    }
}
