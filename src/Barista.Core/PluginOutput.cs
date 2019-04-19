using System;
using System.Collections.Generic;

namespace Barista
{
    public class PluginOutput
    {
        public string Title { get; set; }
        public IReadOnlyList<string> Items { get; set; }
    }
}
