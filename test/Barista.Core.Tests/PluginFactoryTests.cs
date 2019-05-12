using Barista.Core.Data;
using Barista.Core.FileSystem;
using Barista.Core.Providers;
using Moq;
using Xunit;

namespace Barista.Core.Tests
{
    public class PluginFactoryTests
    {
        [Fact]
        public void PluginFactory_FromFilePath_CreatesAPluginWithCorrectDefaultSettings()
        {
            // Given
            var provider = new Mock<IFileProvider>();
            var factory = new PluginFileSystemProvider(provider.Object);

            // When
            var plugin = factory.FromFilePath("emoji.sh");

            // Then
            Assert.Equal("emoji", plugin.Name);
            Assert.Equal("", plugin.Schedule);
            Assert.Equal(PluginType.Shell, plugin.Type);
            Assert.Equal(60, plugin.Interval);
        }

        [Fact]
        public void PluginFactory_FromFilePath_CreatesAPluginWithCorrectInterval()
        {
            // Given
            var provider = new Mock<IFileProvider>();
            var factory = new PluginFileSystemProvider(provider.Object);

            // When
            var plugin = factory.FromFilePath("emoji.20s.sh");
            var plugin2 = factory.FromFilePath("awesome.2m.sh");

            // Then
            Assert.Equal("emoji", plugin.Name);
            Assert.Equal("20s", plugin.Schedule);
            Assert.Equal(PluginType.Shell, plugin.Type);
            Assert.Equal(20, plugin.Interval);

            Assert.Equal("awesome", plugin2.Name);
            Assert.Equal("2m", plugin2.Schedule);
            Assert.Equal(PluginType.Shell, plugin2.Type);
            Assert.Equal(120, plugin2.Interval);
        }

        [Fact]
        public void PluginFactory_FromFilePath_CreatesAPluginWithTheCorrectType()
        {
            // Given
            var provider = new Mock<IFileProvider>();
            var factory = new PluginFileSystemProvider(provider.Object);

            // When
            var plugin = factory.FromFilePath("snake.py");
            var plugin2 = factory.FromFilePath("screen.js");

            // Then
            Assert.Equal("snake", plugin.Name);
            Assert.Equal(PluginType.Python, plugin.Type);

            Assert.Equal("screen", plugin2.Name);
            Assert.Equal(PluginType.JavaScript, plugin2.Type);
        }

        [Fact]
        public void PluginFactory_FromFilePath_UnderscoreDisablesAPlugin()
        {
            // Given
            var provider = new Mock<IFileProvider>();
            var factory = new PluginFileSystemProvider(provider.Object);

            // When
            var plugin = factory.FromFilePath("enabled.sh");
            var plugin2 = factory.FromFilePath("_disabled.js");

            // Then
            Assert.Equal("enabled", plugin.Name);
            Assert.True(plugin.Enabled);
            Assert.Equal("disabled", plugin2.Name);
            Assert.False(plugin2.Enabled);
        }
    }
}
