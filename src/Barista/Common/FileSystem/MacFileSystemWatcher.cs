using System;
using Barista.Common.FileSystem;
using CoreServices;
using Foundation;

namespace Barista.Common.FileSystem
{
    public class MacFileSystemWatcher : IDisposable, IFileSystemWatcher
    {
        private readonly FSEventStream _eventStream;

        public event EventHandler Events;

        public MacFileSystemWatcher(string filePath)
        {
            System.Diagnostics.Debug.WriteLine("Creating MacFileSystemWatcher");
            _eventStream = new FSEventStream(new[] { filePath }, TimeSpan.FromMilliseconds(500), FSEventStreamCreateFlags.FileEvents);
            _eventStream.Events += OnFSEventStreamEvents;
            _eventStream.ScheduleWithRunLoop(NSRunLoop.Current);
            _eventStream.Start();
        }

        private void OnFSEventStreamEvents(object sender, FSEventStreamEventsArgs e)
        {
            _eventStream.Stop();

            System.Diagnostics.Debug.WriteLine("Received an event");
            foreach (var ev in e.Events)
            {
                System.Diagnostics.Debug.WriteLine(ev.Flags);
                System.Diagnostics.Debug.WriteLine(ev.Path);
            }

            var t = new EventArgs();

            Events?.Invoke(this, t);

            _eventStream.Start();
        }

        public void Dispose()
        {
            _eventStream.Stop();
            _eventStream.Events -= OnFSEventStreamEvents;
            _eventStream.Dispose();
        }
    }
}
