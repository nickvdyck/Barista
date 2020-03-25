using AppKit;
using Foundation;
using System;

namespace Barista.Views.Preferences
{
    public partial class GeneralView : NSView
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

