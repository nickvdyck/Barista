using Foundation;

namespace Barista.MacOS
{
    public static class Settings
    {
        public static string GetPluginDirectory()
        {
            //NSUserDefaults.StandardUserDefaults.SetString("pluginDirectory", "");

            return NSUserDefaults.StandardUserDefaults.StringForKey("pluginDirectory");
        }
    }
}
