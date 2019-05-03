using System;

using Foundation;
using AppKit;
using System.Collections.Generic;
using Barista.MacOS.Preferences.Tabs;
using System.Linq;
using CoreGraphics;

namespace Barista.MacOS.Preferences
{
    public partial class PreferencesWindowController : NSWindowController
    {
        public PreferencesWindowController(IntPtr handle) : base(handle)
        {
        }

        [Export("initWithCoder:")]
        public PreferencesWindowController(NSCoder coder) : base(coder)
        {
        }

        public PreferencesWindowController() : base("PreferencesWindow")
        {
            tabs.Add(new GeneralViewController());
            tabs.Add(new PluginViewController());

            base.Window = new PreferencesWindow(
                tabs.First().View.Bounds,
                NSWindowStyle.Titled | NSWindowStyle.Closable,
                NSBackingStore.Buffered, false
            );

            Window.AwakeFromNib();
        }

        private readonly List<IPreferencesTab> tabs = new List<IPreferencesTab>();
        private PreferencesToolbarDelegate toolbarDelegate;

        public void Show()
        {
            InitializeToolbar();
            Window.Center();

            ShowWindow(this);
        }

        private void InitializeToolbar()
        {
            toolbarDelegate = new PreferencesToolbarDelegate(tabs);
            toolbarDelegate.SelectionChanged += HandleSelectionChanged;

            Window.Toolbar = CreateToolbar();

            HandleSelectionChanged(this, null);
        }

        private NSToolbar CreateToolbar()
        {
            var tb = new NSToolbar("PreferencesToolbar")
            {
                AllowsUserCustomization = false,
                Delegate = toolbarDelegate,
                SelectedItemIdentifier = tabs.First().Name
            };
            return tb;
        }

        private void HandleSelectionChanged(object sender, EventArgs e)
        {
            var selectedTab = tabs.Single(s => s.Name.Equals(Window.Toolbar.SelectedItemIdentifier));
            Window.Title = selectedTab.Name;
            ShowSelectedTab(selectedTab);
        }

        private void ShowSelectedTab(IPreferencesTab selectedTab)
        {
            var contentSize = (selectedTab as NSViewController).View.Bounds.Size;
            var newWindowSize = Window.FrameRectFor(new CGRect(CGPoint.Empty, contentSize)).Size;
            var frame = Window.Frame;

            frame.Y += frame.Height - newWindowSize.Height;
            frame.Size = newWindowSize;

            var horizontalDiff = (Window.Frame.Width - newWindowSize.Width) / 2;
            frame.X += horizontalDiff;

            RemoveCurrentTabView();
            Window.SetFrame(frame, false, true);
            Window.ContentView.AddSubview(selectedTab.View);
        }

        private void RemoveCurrentTabView()
        {
            if (Window.ContentView.Subviews.Any())
                Window.ContentView.Subviews.Single().RemoveFromSuperview();
        }

        public new PreferencesWindow Window
        {
            get { return (PreferencesWindow)base.Window; }
        }
    }
}

