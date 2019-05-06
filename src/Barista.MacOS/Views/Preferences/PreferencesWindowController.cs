using AppKit;
using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Barista.MacOS.Views.Preferences
{
    public class PreferencesWindowController : NSWindowController
    {
        public static PreferencesWindowController Create(IEnumerable<IPreferencesTab> tabControllers) =>
            new PreferencesWindowController(tabControllers);

        public PreferencesWindowController(IntPtr handle) : base(handle)
        {
        }

        [Export("initWithCoder:")]
        public PreferencesWindowController(NSCoder coder) : base(coder)
        {
        }

        public PreferencesWindowController(IEnumerable<IPreferencesTab> tabControllers) : base("PreferencesWindow")
        {
            _tabControllers = tabControllers;
            base.Window = new PreferencesWindow(
                tabControllers.First().View.Bounds,
                NSWindowStyle.Titled | NSWindowStyle.Closable | NSWindowStyle.Miniaturizable,
                NSBackingStore.Buffered, false
            );

            Window.AwakeFromNib();
        }

        private readonly IEnumerable<IPreferencesTab> _tabControllers;
        private PreferencesToolbarDelegate _toolbarDelegate;

        public void Show()
        {
            if (Window.Toolbar == null)
            {
                InitializeToolbar();
                Window.Center();
            }

            ShowWindow(this);
        }

        private void InitializeToolbar()
        {
            _toolbarDelegate = new PreferencesToolbarDelegate(_tabControllers);
            _toolbarDelegate.SelectionChanged += HandleSelectionChanged;

            Window.Toolbar = CreateToolbar();

            HandleSelectionChanged(this, null);
        }

        private NSToolbar CreateToolbar()
        {
            var tb = new NSToolbar("PreferencesToolbar")
            {
                AllowsUserCustomization = false,
                Delegate = _toolbarDelegate,
                SelectedItemIdentifier = _tabControllers.First().Name
            };
            return tb;
        }

        private void HandleSelectionChanged(object sender, EventArgs e)
        {
            var selectedTab = _tabControllers.Single(s => s.Name.Equals(Window.Toolbar.SelectedItemIdentifier));
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

