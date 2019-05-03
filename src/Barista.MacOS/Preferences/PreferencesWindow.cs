using System;

using Foundation;
using AppKit;
using CoreGraphics;

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

        public PreferencesWindow(CGRect contentRect, NSWindowStyle aStyle, NSBackingStore bufferingType, bool deferCreation) : base(contentRect, aStyle, bufferingType, deferCreation)
        {
            ContentView = new NSView(Frame);
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();
        }
    }
}

