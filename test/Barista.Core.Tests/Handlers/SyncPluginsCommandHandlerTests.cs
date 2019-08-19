using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Barista.Common;
using Barista.Data;
using Barista.Data.Commands;
using Barista.Data.Events;
using Barista.Handlers;
using Barista.Common.FileSystem;
using Barista.Tests.Builders;
using Moq;
using Xunit;

namespace Barista.Core.Tests.Handlers
{
    public class SyncPluginsCommandHandlerTests
    {
        [Fact]
        public async Task SyncPluginsCommandHandler_Handle_CreatesAPluginRecordForEachFileAtTheRootOfTheDirectory()
        {
            // Given
            var provider = new Mock<IFileProvider>();
            var mediator = new Mock<IMediator>();
            var store = new PluginStore();

            var files = new List<IFileInfo>
            {
                MockFileInfo.Create("/root/one.sh"),
                MockFileInfo.Create("/root/subdir", isDirectory: true),
                MockFileInfo.Create("/root/two.sh"),
            };
            var directory = CreateMockDirectory(files);

            provider.Setup(p => p.GetDirectoryContents(It.IsAny<string>())).Returns(directory);

            // When
            var handler = new SyncPluginsCommandHandler(provider.Object, store, mediator.Object);

            await handler.Handle(new SyncPluginsCommand());

            // Then
            Assert.Equal(2, store.Plugins.Count);
            Assert.Contains(store.Plugins, p => p.Name == "one");
            Assert.Contains(store.Plugins, p => p.Name == "two");
        }

        [Fact]
        public async Task SyncPluginsCommandHandler_Handle_ThrowsAnErrorWhenTheRootDirectoryDoesNotExist()
        {
            // Given
            var provider = new Mock<IFileProvider>();
            var mediator = new Mock<IMediator>();
            var store = new PluginStore();
            var directory = CreateMockDirectory(new List<IFileInfo> { }, exists: false);

            provider.Setup(p => p.GetDirectoryContents(It.IsAny<string>())).Returns(directory);

            // When
            var handler = new SyncPluginsCommandHandler(provider.Object, store, mediator.Object);

            // Then
            await Assert.ThrowsAsync<Exception>(() => handler.Handle(new SyncPluginsCommand()));
        }

        [Fact]
        public async Task SyncPluginsCommandHandler_Handle_SendsCommandToSchedulePluginExecutions()
        {
            // Given
            var provider = new Mock<IFileProvider>();
            var mediator = new Mock<IMediator>();
            var store = new PluginStore();

            var files = new List<IFileInfo>
            {
                MockFileInfo.Create("/root/test.sh"),
                MockFileInfo.Create("/root/directory", isDirectory: true),
                MockFileInfo.Create("/root/reveal.py"),
                MockFileInfo.Create("/root/added.rb"),
            };
            var directory = CreateMockDirectory(files);

            provider.Setup(p => p.GetDirectoryContents(It.IsAny<string>())).Returns(directory);
            store.Update(new List<Plugin>
            {
                new PluginBuilder().SetFilePath("/root/test.sh").Build(),
                new PluginBuilder().SetFilePath("/root/reveal.py").Build(),
                new PluginBuilder().SetFilePath("/root/removed.js").Build(),
            });

            mediator.Setup(m => m.Publish(It.IsAny<PluginsUpdatedEvent>(), It.IsAny<CancellationToken>()))
                .Callback<IEvent, CancellationToken>((@event, token) =>
                {
                    var updatedEvent = (PluginsUpdatedEvent)@event;
                    // Then
                    Assert.Equal(1, updatedEvent.Added.Count);
                    Assert.Contains(updatedEvent.Added, p => p.Name == "added");

                    Assert.Equal(1, updatedEvent.Added.Count);
                    Assert.Contains(updatedEvent.Removed, p => p.Name == "removed");
                });

            // When
            var handler = new SyncPluginsCommandHandler(provider.Object, store, mediator.Object);
            await handler.Handle(new SyncPluginsCommand());

            // Then
            Assert.Equal(3, store.Plugins.Count);
            Assert.Contains(store.Plugins, plugin => plugin.Name == "test");
            Assert.Contains(store.Plugins, plugin => plugin.Name == "reveal");
            Assert.Contains(store.Plugins, plugin => plugin.Name == "added");
        }

        class MockFileInfo : IFileInfo
        {
            public static IFileInfo Create(string path, bool isDirectory = false)
            {
                return new MockFileInfo
                {
                    IsDirectory = isDirectory,
                    LastModified = DateTime.UtcNow,
                    Length = 123,
                    Name = Path.GetFileName(path),
                    PhysicalPath = path,
                };
            }

            public bool Exists { get; set; } = true;

            public bool IsDirectory { get; set; } = false;

            public DateTimeOffset LastModified { get; set; }

            public long Length { get; set; }

            public string Name { get; set; }

            public string PhysicalPath { get; set; }
        }

        public IDirectoryContents CreateMockDirectory(List<IFileInfo> files, bool exists = true)
        {
            var directory = new Mock<IDirectoryContents>();

            directory.Setup(d => d.GetEnumerator()).Returns(() => files.GetEnumerator());
            directory.Setup(d => d.Exists).Returns(exists);

            return directory.Object;
        }
    }
}
