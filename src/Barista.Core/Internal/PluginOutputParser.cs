using System;
using System.Collections.Generic;
using System.Linq;
using Barista.Core.Data;
using Barista.Core.Utils;

namespace Barista.Core.Internal
{
    internal class PluginOutputParser : IPluginOutputParser
    {
        public IReadOnlyCollection<IPluginMenuItem> Parse(string output)
        {
            var chunks = output.Split(new[] { "---" }, StringSplitOptions.RemoveEmptyEntries);
            var menuItems = new List<IPluginMenuItem>();

            var title = chunks.FirstOrDefault().Trim();
            menuItems.Add(ParseMenuItem(title));

            foreach (var chunk in chunks.Skip(1))
            {
                var lines = chunk.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                var lineList = new List<string>();

                foreach (var line in lines)
                {
                    var record = ParseMenuItem(line);
                    menuItems.Add(record);
                }

                menuItems.Add(PluginMenuItemBase.Separator);
            }

            return menuItems;
        }

        internal static IPluginMenuItem ParseMenuItem(string line)
        {
            var parts = line.Split('|');
            var title = parts.FirstOrDefault();
            var attributes = parts.ElementAtOrDefault(1);

            var item = new PluginMenuItemBase
            {
                OriginalTitle = title
            };

            if (!string.IsNullOrEmpty(attributes))
            {
                var attrs = attributes.Trim().Split(' ');

                foreach (var attribute in attrs)
                {
                    var chunks = attribute.Split('=');

                    var key = chunks.FirstOrDefault();
                    var value = chunks.ElementAtOrDefault(1);

                    item.Settings.Add(key, value);
                }
            }

            return item;
        }
    }
}
