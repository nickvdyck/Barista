using AppKit;
using CoreGraphics;
using Foundation;
using System;

namespace Barista.MacOS.Views.Preferences
{
    public class PreferencesWindow : NSWindow
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

