using System;
using System.IO;

namespace Barista.Core.FileSystem
{
    public class LocalFileProvider : IFileProvider
    {
        private readonly string _root;
        private readonly IFileSystemWatcher _watcher;

        public LocalFileProvider(string root, IFileSystemWatcher watcher)
        {
            _root = root;
            _watcher = watcher;
        }
        public IDirectoryContents GetDirectoryContents(string path = "")
        {
            try
            {
                var fullPath = Path.Combine(_root, path);

                if (fullPath == null || !Directory.Exists(fullPath))
                {
                    return NotFoundDirectoryContents.Singleton;
                }

                return new LocalDirectoryContents(fullPath);
            }
            catch (DirectoryNotFoundException)
            {
            }
            catch (IOException)
            {
            }

            return NotFoundDirectoryContents.Singleton;
        }

        public IFileInfo GetFileInfo(string filePath)
        {
            throw new System.NotImplementedException();
        }


        internal class WatchDisposer : IDisposable
        {
            private readonly Action _onChangeRef;
            private readonly IFileSystemWatcher _watcher;

            public WatchDisposer(Action onChange, IFileSystemWatcher watcher)
            {
                _onChangeRef = onChange;
                _watcher = watcher;

                _watcher.Events += OnChangeHandler;
            }

            private void OnChangeHandler(object sender, EventArgs e)
            {
                _onChangeRef?.Invoke();
            }

            public void Dispose()
            {
                _watcher.Events -= OnChangeHandler;

            }
        }

        public IDisposable Watch(Action onChange)
        {
            return new WatchDisposer(onChange, _watcher);
        }
    }
}
