using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reactive.Subjects;
using System.Security.Cryptography;
using Barista.Common;
using Barista.Common.FileSystem;
using Barista.Domain;
using Barista.Scheduler;
using Stateless;

namespace Barista.State
{
    public class App : IObservable<ImmutableList<Plugin>>
    {
        private enum Trigger
        {
            SyncPlugins,
            Start,
            Stop
        }

        private enum State
        {
            Synchronizing,
            Started,
            Stopped,
        }

        private struct PluginUpdate
        {
            public ImmutableList<PluginMetadata> Added { get; set; }
            public ImmutableList<PluginMetadata> Removed { get; set; }
            public ImmutableList<PluginMetadata> Rest { get; set; }
        }

        private readonly StateMachine<State, Trigger> _machine;
        private readonly StateMachine<State, Trigger>.TriggerWithParameters<PluginUpdate> _startTrigger;
        private readonly JobScheduler _scheduler;
        private readonly IFileProvider _fileProvider;
        private readonly Subject<ImmutableList<Plugin>> _plugins = new Subject<ImmutableList<Plugin>>();
        private readonly IDisposable _watchSubscription;

        public ImmutableList<Plugin> Plugins { get; set; } = ImmutableList.CreateBuilder<Plugin>().ToImmutable();

        public App(IFileProvider fileProvider)
        {
            _fileProvider = fileProvider;
            _scheduler = new JobScheduler();

            _machine = new StateMachine<State, Trigger>(State.Stopped);
            _startTrigger = _machine.SetTriggerParameters<PluginUpdate>(Trigger.Start);

            _machine.Configure(State.Synchronizing)
                .Permit(Trigger.Start, State.Started)
                .Permit(Trigger.Stop, State.Stopped)
                .OnEntry(OnSynchronizePlugins);

            _machine.Configure(State.Started)
                .Permit(Trigger.SyncPlugins, State.Synchronizing)
                .Permit(Trigger.Stop, State.Stopped)
                .OnEntryFrom(_startTrigger, OnStart);

            _machine.Configure(State.Stopped)
                .Permit(Trigger.SyncPlugins, State.Synchronizing)
                .OnEntry(OnStopped);

            _watchSubscription = _fileProvider.Watch(OnPluginDirectoryChanged);
        }

        public bool IsStarted => _machine.IsInState(State.Started);

        public void Start() => _machine.Fire(Trigger.SyncPlugins);

        public void Stop() => _machine.Fire(Trigger.Stop);

        public IDisposable Subscribe(IObserver<ImmutableList<Plugin>> observer) => _plugins.Subscribe(observer);

        private void OnPluginDirectoryChanged()
        {
            System.Diagnostics.Debug.WriteLine("File Updated");
            if (!IsStarted) return;

            _machine.Fire(Trigger.SyncPlugins);
        }

        private void OnSynchronizePlugins()
        {
            var files = _fileProvider.GetDirectoryContents("");
            
            if (!files.Exists)
            {
                _machine.Fire(Trigger.Stop);
                return;
            }

            var builder = ImmutableList.CreateBuilder<PluginMetadata>();
            foreach (var file in files)
            {
                if (file.IsDirectory) continue;

                var metadata = PluginParser.FromFilePath(file.PhysicalPath);
                metadata.Checksum = GetChecksum(file.PhysicalPath);

                builder.Add(metadata);
            }

            var previous = Plugins.Select(p => p.Metadata);
            var current = builder.ToImmutableList();

            var added = current.Except(previous).ToImmutableList();
            var removed = previous.Except(current).ToImmutableList();

            _machine.Fire(_startTrigger, new PluginUpdate
            {
                Added = added,
                Removed = removed,
                Rest = previous.Except(removed).ToImmutableList(),
            });
        }

        private void OnStart(PluginUpdate update)
        {
            var builder = ImmutableList.CreateBuilder<Plugin>();

            foreach (var rest in update.Rest)
            {
                var plugin = Plugins.FirstOrDefault(p => p.Metadata == rest);
                if (plugin != null)
                {
                    builder.Add(plugin);
                }
            }

            // Stop schedules of any removed plugins
            foreach (var metadata in update.Removed)
            {
                var plugin = Plugins.FirstOrDefault(p => p.Metadata == metadata);
                plugin?.Stop();
            }

            foreach (var metadata in update.Added)
            {
                var plugin = new Plugin(metadata, _scheduler);
                builder.Add(plugin);
                plugin.Start();
            }

            Plugins = builder.ToImmutableList();
            _plugins.OnNext(Plugins);
        }

        private void OnStopped()
        {
            _watchSubscription.Dispose();
            _plugins.OnCompleted();

            foreach (var plugin in Plugins)
            {
                plugin.Stop();
            }
        }

        private static string GetChecksum(string filePath)
        {
            using var stream = File.OpenRead(filePath);
            using var sha = new SHA256Managed();
            byte[] checksum = sha.ComputeHash(stream);
            return BitConverter.ToString(checksum).Replace("-", string.Empty);
        }
    }
}
