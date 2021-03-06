using AppKit;
using Foundation;
using System;
using System.Linq;

namespace Barista.Views.Preferences
{
    public partial class GeneralViewController : NSViewController, IPreferencesTab
    {
        public string Name => "General";
        public NSImage Icon => NSImage.ImageNamed("NSGeneralPreferences");

        [Export("PluginDirectory")]
        private string PluginDirectory { get; set; }

        #region Constructors

        // Called when created from unmanaged code
        public GeneralViewController(IntPtr handle) : base(handle)
        {
            Initialize();
        }

        // Called when created directly from a XIB file
        [Export("initWithCoder:")]
        public GeneralViewController(NSCoder coder) : base(coder)
        {
            Initialize();
        }

        // Call to load from the XIB/NIB file
        public GeneralViewController() : base("GeneralView", NSBundle.MainBundle)
        {
            PluginDirectory = string.Empty;
            Initialize();
        }

        // Shared initialization code
        void Initialize()
        {

        }

        #endregion

        partial void OnBrowsePluginsClicked(NSObject _)
        {
            var panel = NSOpenPanel.OpenPanel;
            panel.FloatingPanel = true;
            panel.CanChooseDirectories = true;
            panel.CanChooseFiles = false;
            panel.AllowsMultipleSelection = false;

            var i = panel.RunModal();

            if (i > 0)
            {
                PluginDirectory = panel.Urls.First().Path;
            }
        }

        //strongly typed view accessor
        public new GeneralView View
        {
            get
            {
                return (GeneralView)base.View;
            }
        }
    }
}

