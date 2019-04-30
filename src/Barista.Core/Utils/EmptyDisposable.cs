using System;

namespace Barista.Core.Utils
{
    internal sealed class EmptyDisposable : IDisposable
    {
        private EmptyDisposable()
        {
        }

        public void Dispose()
        {
        }

        public static EmptyDisposable Instance { get; } = new EmptyDisposable();
    }
}
