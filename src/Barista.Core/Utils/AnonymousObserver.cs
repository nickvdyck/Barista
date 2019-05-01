using System;

namespace Barista.Core.Utils
{
    internal sealed class AnonymousObserver<T> : ObserverBase<T>
    {
        private readonly Action<T> _onNext;
        private readonly Action<Exception> _onError;
        private readonly Action _onCompleted;

        public AnonymousObserver(Action<T> onNext, Action<Exception> onError, Action onCompleted)
        {
            _onNext = onNext ?? throw new ArgumentNullException(nameof(onNext));
            _onError = onError ?? throw new ArgumentNullException(nameof(onError));
            _onCompleted = onCompleted ?? throw new ArgumentNullException(nameof(onCompleted));
        }

        public AnonymousObserver(Action<T> onNext)
            : this(onNext, ActionStubs.Throw, ActionStubs.Noop)
        {
        }

        public AnonymousObserver(Action<T> onNext, Action<Exception> onError)
            : this(onNext, onError, ActionStubs.Noop)
        {
        }

        public AnonymousObserver(Action<T> onNext, Action onCompleted)
            : this(onNext, ActionStubs.Throw, onCompleted)
        {
        }

        protected override void OnNextCore(T value) => _onNext(value);

        protected override void OnErrorCore(Exception error) => _onError(error);

        protected override void OnCompletedCore() => _onCompleted();
    }
}
