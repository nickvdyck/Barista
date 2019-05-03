using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using AppKit;

namespace Barista.MacOS.Preferences.Tabs
{
    public partial class PluginViewController : NSViewController, IPreferencesTab
    {
        public string Name
        {
            get
            {
                return "Plugins";
            }
        }

        public NSImage Icon
        {
            get
            {
                return NSImage.ImageNamed("NSUser");
            }
        }

        #region Constructors

        // Called when created from unmanaged code
        public PluginViewController(IntPtr handle) : base(handle)
        {
            Initialize();
        }

        // Called when created directly from a XIB file
        [Export("initWithCoder:")]
        public PluginViewController(NSCoder coder) : base(coder)
        {
            Initialize();
        }

        // Call to load from the XIB/NIB file
        public PluginViewController() : base("PluginView", NSBundle.MainBundle)
        {
            Initialize();
        }

        // Shared initialization code
        void Initialize()
        {
        }

        #endregion

        //strongly typed view accessor
        public new PluginView View
        {
            get
            {
                return (PluginView)base.View;
            }
        }
    }
}

