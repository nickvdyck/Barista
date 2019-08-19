﻿using System;
using System.Collections.Immutable;

namespace Barista.Data
{
    public class PluginExecution
    {
        public Plugin Plugin { get; internal set; }
        public ImmutableList<ImmutableList<Item>> Items { get; internal set; }
        public bool Success { get; set; } = true;
        public DateTime LastExecution { get; set; }
    }
}
