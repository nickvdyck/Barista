using System;
using System.Collections.Generic;
using Barista.Core.Data;

namespace Barista.Core.Events
{
    public class PluginEventsMonitor : IObservable<IPluginEvent>
    {
        private readonly List<IObserver<IPluginEvent>> _observers = new List<IObserver<IPluginEvent>>();
        public IDisposable Subscribe(IObserver<IPluginEvent> observer)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
            }

            return new PluginEventsMonitorUnsubscriber<IPluginEvent>(_observers, observer);
        }

        public void PluginExecuted(PluginExecution execution)
        {
            var e = new PluginExecutedEvent
            {
                Plugin = execution.Plugin,
                Execution = execution,
            };

            foreach (var observer in _observers)
            {
                observer.OnNext(e);
            }
        }

        public void PluginsChanged()
        {
            var e = new PluginChangedEvent();

            foreach (var observer in _observers)
            {
                observer.OnNext(e);
            }
        }
    }

    internal class PluginEventsMonitorUnsubscriber<T> : IDisposable
    {
        private readonly List<IObserver<T>> _observers;
        private readonly IObserver<T> _observer;

        internal PluginEventsMonitorUnsubscriber(List<IObserver<T>> observers, IObserver<T> observer)
        {
            _observers = observers;
            _observer = observer;
        }

        public void Dispose()
        {
            if (_observers.Contains(_observer)) _observers.Remove(_observer);
        }
    }
}
