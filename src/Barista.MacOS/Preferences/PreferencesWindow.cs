using System;

using Foundation;
using AppKit;

namespace Barista.MacOS.Preferences
{
    public partial class PreferencesWindow : NSWindow
    {
        public PreferencesWindow(IntPtr handle) : base(handle)
        {
        }

        [Export("initWithCoder:")]
        public PreferencesWindow(NSCoder coder) : base(coder)
        {
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();
        }
    }
}

