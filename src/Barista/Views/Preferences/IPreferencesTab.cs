using AppKit;

namespace Barista.Views.Preferences
{
    public interface IPreferencesTab
    {
        string Name { get; }
        NSImage Icon { get; }
        NSView View { get; }
    }
}
