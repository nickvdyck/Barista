// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Barista.Views.Preferences
{
	[Register ("PluginViewController")]
	partial class PluginViewController
	{
		[Outlet]
		AppKit.NSTableView PluginTableView { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (PluginTableView != null) {
				PluginTableView.Dispose ();
				PluginTableView = null;
			}
		}
	}
}
