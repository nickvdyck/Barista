using Foundation;

namespace Barista.MacOS
{
    public static class Settings
    {
        public static string GetPluginDirectory()
        {
            return NSUserDefaults.StandardUserDefaults.StringForKey("pluginDirectory");
        }
    }
}
