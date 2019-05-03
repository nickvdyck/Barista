using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using AppKit;

namespace Barista.MacOS.Preferences.Tabs
{
    public partial class GeneralView : AppKit.NSView
    {
        #region Constructors

        // Called when created from unmanaged code
        public GeneralView(IntPtr handle) : base(handle)
        {
            Initialize();
        }

        // Called when created directly from a XIB file
        [Export("initWithCoder:")]
        public GeneralView(NSCoder coder) : base(coder)
        {
            Initialize();
        }

        // Shared initialization code
        void Initialize()
        {
        }

        #endregion
    }
}

