using AppKit;

namespace Barista.MacOS.Views.Preferences
{
    public interface IPreferencesTab
    {
        string Name { get; }
        NSImage Icon { get; }
        NSView View { get; }
    }
}
