using System;
using AppKit;

namespace Barista.MacOS.Preferences.Tabs
{
    public interface IPreferencesTab
    {
        string Name { get; }
        NSImage Icon { get; }
        NSView View { get; }
    }
}
