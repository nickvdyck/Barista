using System;

using Foundation;
using AppKit;
using System.Collections.Generic;
using Barista.MacOS.Preferences.Tabs;
using System.Linq;

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
        }

        readonly List<IPreferencesTab> tabs = new List<IPreferencesTab>();
        PreferencesToolbarDelegate toolbarDelegate;

        // When Preference Window is loaded from a NIB we create a view controller
        // for each tab in preferences window and initialize the toolbar.
        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            tabs.Add(new GeneralViewController());
            tabs.Add(new PluginViewController());

            InitializeToolbar();
        }

        public void Show()
        {
            ShowWindow(this);
        }

        void InitializeToolbar()
        {
            toolbarDelegate = new PreferencesToolbarDelegate(tabs);
            toolbarDelegate.SelectionChanged += HandleSelectionChanged;

            Window.Toolbar = CreateToolbar();

            HandleSelectionChanged(this, null); // Called once when the window is created to make first tab visible
        }

        NSToolbar CreateToolbar()
        {
            var tb = new NSToolbar("PreferencesToolbar")
            {
                AllowsUserCustomization = false,
                Delegate = toolbarDelegate,
                SelectedItemIdentifier = tabs.First().Name
            };
            return tb;
        }

        // Called when user clicks on toolbar item to change the preferences tab.
        void HandleSelectionChanged(object sender, EventArgs e)
        {
            var selectedTab = tabs.Single(s => s.Name.Equals(Window.Toolbar.SelectedItemIdentifier));
            Window.Title = selectedTab.Name;
            ShowSelectedTab(selectedTab);
        }

        // Change preferences tab view to selected one. Animate resizing of the window if the selected
        // tab is different size than the currently visible one.
        void ShowSelectedTab(IPreferencesTab selectedTab)
        {
            var delta = Window.ContentView.Frame.Height - selectedTab.View.Frame.Height; // Delta must be calculated before currect tab view is removed
            RemoveCurrentTabView();
            Window.SetFrame(CalculateNewFrameForWindow(delta), true, true);
            Window.ContentView.AddSubview(selectedTab.View);
        }

        void RemoveCurrentTabView()
        {
            if (Window.ContentView.Subviews.Any())
                Window.ContentView.Subviews.Single().RemoveFromSuperview();
        }

        CoreGraphics.CGRect CalculateNewFrameForWindow(nfloat delta)
        {
            return new CoreGraphics.CGRect(Window.Frame.X, Window.Frame.Y + delta, Window.Frame.Width, Window.Frame.Height - delta);
        }

        public new PreferencesWindow Window
        {
            get { return (PreferencesWindow)base.Window; }
        }
    }
}

