using System;
using Barista.Core.FileSystem;
using CoreServices;
using Foundation;

namespace Barista.MacOS.Utils
{
    public class FileSystemWatcher : IDisposable, IFileSystemWatcher
    {
        private readonly FSEventStream _eventStream;

        public event EventHandler Events;

        public FileSystemWatcher(string filePath)
        {
            _eventStream = new FSEventStream(new[] { filePath }, TimeSpan.FromMilliseconds(200), FSEventStreamCreateFlags.FileEvents);
            _eventStream.Events += OnFSEventStreamEvents;
            _eventStream.ScheduleWithRunLoop(NSRunLoop.Current);
            _eventStream.Start();
        }

        private void OnFSEventStreamEvents(object sender, FSEventStreamEventsArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Received an event");
            foreach (var ev in e.Events)
            {
                System.Diagnostics.Debug.WriteLine(ev.Flags);
                System.Diagnostics.Debug.WriteLine(ev.Path);
            }

            var t = new EventArgs();

            Events?.Invoke(this, t);
        }

        public void Dispose()
        {
            _eventStream.Stop();
            _eventStream.Events -= OnFSEventStreamEvents;
            _eventStream.Dispose();
        }
    }
}
