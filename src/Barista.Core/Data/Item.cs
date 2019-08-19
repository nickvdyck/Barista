using System.Collections.Generic;

namespace Barista.Data
{
    public class Item : ItemBase
    {
        internal Plugin Plugin { get; set; }

        public IReadOnlyCollection<Item> Children { get; internal set; }

        public ItemType Type
        {
            get
            {
                if (Refresh)
                {
                    return ItemType.RefreshAction;
                }
                else if (BashScript != string.Empty)
                {
                    return ItemType.RunScriptAction;
                }
                else if (BashScript != string.Empty && Terminal)
                {
                    return ItemType.RunScriptInTerminalAction;
                }
                else if (Href != string.Empty)
                {
                    return ItemType.Link;
                }

                return ItemType.Empty;
            }
        }
    }
}
