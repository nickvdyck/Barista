using System;

namespace Barista.Core.Utils
{
    internal static class ActionStubs
    {
        public static readonly Action Noop = () => { };
        public static readonly Action<Exception> Throw = ex => { throw ex; };
    }
}
