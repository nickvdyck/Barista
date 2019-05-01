using System;
using Barista.Core.Utils;

namespace Barista.Core.Extensions
{
    public static class ObservableExtensions
    {
        public static IDisposable Subscribe<T>(this IObservable<T> observable, Action<T> onNext)
        {
            if (onNext == null)
            {
                throw new ArgumentNullException(nameof(onNext));
            }

            return observable.Subscribe(new AnonymousObserver<T>(onNext));
        }

        public static IDisposable Subscribe<T>(this IObservable<T> observable, Action<T> onNext, Action<Exception> onError, Action onCompleted)
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

            return observable.Subscribe(new AnonymousObserver<T>(onNext, onError, onCompleted));
        }
    }
}
