﻿using System.Collections.Generic;

namespace Barista.Domain
{
    public class Item : ItemBase
    {
        public IReadOnlyCollection<Item> Children { get; internal set; }

        internal string PluginName { get; set; }

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
