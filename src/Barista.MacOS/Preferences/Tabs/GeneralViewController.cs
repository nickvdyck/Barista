using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using AppKit;

namespace Barista.MacOS.Preferences.Tabs
{
    public partial class GeneralViewController : NSViewController, IPreferencesTab
    {
        public string Name
        {
            get
            {
                return "General";
            }
        }

        public NSImage Icon
        {
            get
            {
                return NSImage.ImageNamed("NSGeneralPreferences");
            }
        }

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
            Initialize();
        }

        // Shared initialization code
        void Initialize()
        {
        }

        #endregion

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

