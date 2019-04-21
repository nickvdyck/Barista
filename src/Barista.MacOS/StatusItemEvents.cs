using System;
using AppKit;

namespace Barista.MacOS
{
    class StatusItemEvents : NSMenuDelegate
    {

        public Action<NSMenu> OnMenuWillOpen { get; set; }
        public Action<NSMenu> OnMenuDidClose { get; set; }
        public Action<NSMenu, NSMenuItem> OnMenuWillHighlight { get; set; }

        public override void MenuWillOpen(NSMenu menu)
        {
            OnMenuWillOpen?.Invoke(menu);
        }

        public override void MenuDidClose(NSMenu menu)
        {
            OnMenuDidClose?.Invoke(menu);
        }

        public override void MenuWillHighlightItem(NSMenu menu, NSMenuItem item)
        {
            OnMenuWillHighlight?.Invoke(menu, item);
        }
    }
}
