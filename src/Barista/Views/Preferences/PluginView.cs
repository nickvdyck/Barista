using AppKit;
using Foundation;
using System;

namespace Barista.Views.Preferences
{
    public partial class PluginView : NSView
    {
        #region Constructors

        // Called when created from unmanaged code
        public PluginView(IntPtr handle) : base(handle)
        {
            Initialize();
        }

        // Called when created directly from a XIB file
        [Export("initWithCoder:")]
        public PluginView(NSCoder coder) : base(coder)
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

