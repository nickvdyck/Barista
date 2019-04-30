using System;
using System.Collections.Generic;
using Barista.Core.Data;
using Barista.Core.Utils;

namespace Barista.Core
{
    public static class PluginManagerExtensions
    {
        public static IDisposable Monitor(this PluginManager manager, Plugin plugin, Action<IReadOnlyCollection<IPluginMenuItem>> onNext)
        {
            if (onNext == null)
            {
                throw new ArgumentNullException(nameof(onNext));
            }

            return manager.Monitor(plugin, new AnonymousObserver<IReadOnlyCollection<IPluginMenuItem>>(onNext));
        }

        public static IDisposable Monitor(this PluginManager manager, Plugin plugin, Action<IReadOnlyCollection<IPluginMenuItem>> onNext, Action<Exception> onError, Action onCompleted)
        {
            if (onNext == null)
            {
                throw new ArgumentNullException(nameof(onNext));
            }

            if (onError == null)
            {
                throw new ArgumentNullException(nameof(onError));
            }

            if (onCompleted == null)
            {
                throw new ArgumentNullException(nameof(onCompleted));
            }

            return manager.Monitor(plugin, new AnonymousObserver<IReadOnlyCollection<IPluginMenuItem>>(onNext, onError, onCompleted));
        }
    }
}
