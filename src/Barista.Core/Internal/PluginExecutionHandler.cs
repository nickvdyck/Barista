using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Barista.Core.Data;

namespace Barista.Core.Internal
{
    using IPluginMenuItemsObserver = IObserver<IReadOnlyCollection<IPluginMenuItem>>;

    internal class PluginExecutionHandler : IObservable<IReadOnlyCollection<IPluginMenuItem>>
    {
        private readonly List<IPluginMenuItemsObserver> _observers = new List<IPluginMenuItemsObserver>();
        private IReadOnlyCollection<IPluginMenuItem> _lastExecution;
        private readonly Plugin _plugin;
        private readonly IPluginOutputParser _outputParser;

        public PluginExecutionHandler(Plugin plugin, IPluginOutputParser outputParser)
        {
            _plugin = plugin;
            _outputParser = outputParser;
        }

        public IDisposable Subscribe(IPluginMenuItemsObserver observer)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);

                if (_lastExecution != null) observer.OnNext(_lastExecution);
            }

            return new PluginExecutionHandlerUnsubscriber<IReadOnlyCollection<IPluginMenuItem>>(_observers, observer);
        }

        public async Task ExecutePlugin()
        {
            try
            {
                var info = new ProcessStartInfo
                {
                    FileName = _plugin.FilePath,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                };

                var process = Process.Start(info);

                var data = await process.StandardOutput.ReadToEndAsync();
                process.WaitForExit();

                var menuItems = _outputParser.Parse(data);
                _lastExecution = menuItems;
                _plugin.LastExecution = DateTime.Now;

                foreach (var observer in _observers)
                {
                    observer.OnNext(menuItems);
                }
            }
            catch (Exception ex)
            {
                foreach (var observer in _observers)
                {
                    observer.OnError(ex);
                }
            }
        }

        internal class PluginExecutionHandlerUnsubscriber<T> : IDisposable
        {
            private readonly List<IObserver<T>> _observers;
            private readonly IObserver<T> _observer;

            internal PluginExecutionHandlerUnsubscriber(List<IObserver<T>> observers, IObserver<T> observer)
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
}
