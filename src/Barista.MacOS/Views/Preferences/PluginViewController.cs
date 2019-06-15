using AppKit;
using Foundation;
using System;
using System.Collections.Generic;

namespace Barista.MacOS.Views.Preferences
{
    public partial class PluginViewController : NSViewController, IPreferencesTab
    {
        public string Name
        {
            get
            {
                return "Plugins";
            }
        }

        public NSImage Icon
        {
            get
            {
                return NSImage.ImageNamed("NSUser");
            }
        }

        #region Constructors

        // Called when created from unmanaged code
        public PluginViewController(IntPtr handle) : base(handle)
        {
            Initialize();
        }

        // Called when created directly from a XIB file
        [Export("initWithCoder:")]
        public PluginViewController(NSCoder coder) : base(coder)
        {
            Initialize();
        }

        // Call to load from the XIB/NIB file
        public PluginViewController() : base("PluginView", NSBundle.MainBundle)
        {
            Initialize();
        }

        // Shared initialization code
        void Initialize()
        {
        }

        #endregion

        //strongly typed view accessor
        public new PluginView View
        {
            get
            {
                return (PluginView)base.View;
            }
        }

        public class ProductTableDataSource : NSTableViewDataSource
        {
            public List<string> Products = new List<string>();

            public override nint GetRowCount(NSTableView tableView)
            {
                return Products.Count;
            }
        }

        public class ProductTableDelegate : NSTableViewDelegate
        {
            #region Constants
            private const string CellIdentifier = "ProdCell";
            #endregion

            #region Private Variables
            private ProductTableDataSource DataSource;
            #endregion

            #region Constructors
            public ProductTableDelegate(ProductTableDataSource datasource)
            {
                this.DataSource = datasource;
            }
            #endregion

            #region Override Methods
            public override NSView GetViewForItem(NSTableView tableView, NSTableColumn tableColumn, nint row)
            {
                // This pattern allows you reuse existing views when they are no-longer in use.
                // If the returned view is null, you instance up a new view
                // If a non-null view is returned, you modify it enough to reflect the new data
                NSTextField view = (NSTextField)tableView.MakeView(CellIdentifier, this);
                if (view == null)
                {
                    view = new NSTextField();
                    view.Identifier = CellIdentifier;
                    view.BackgroundColor = NSColor.Clear;
                    view.Bordered = false;
                    view.Selectable = false;
                    view.Editable = false;
                }

                view.StringValue = DataSource.Products[(int)row];

                return view;
            }

            public override bool ShouldSelectRow(NSTableView tableView, nint row)
            {
                System.Diagnostics.Debug.WriteLine($"Should select row {row}");
                return true;
            }

            #endregion
        }

        public override void AwakeFromNib()
        {
            var dataSource = new ProductTableDataSource();
            dataSource.Products.Add("one");
            dataSource.Products.Add("two");
            dataSource.Products.Add("three");

            PluginTableView.DataSource = dataSource;
            PluginTableView.Delegate = new ProductTableDelegate(dataSource);

            base.AwakeFromNib();
        }
    }
}

