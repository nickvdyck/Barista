using Barista.Core.Data;
using Barista.Core.Plugins;
using Xunit;

namespace Barista.Core.Tests
{
    public class PluginParserTests
    {

        [Fact]
        public void PluginParser_FromFilePath_CreatesAPluginWithCorrectDefaultSettings()
        {
            // Given
            var path = "emoji.sh";
            // When
            var plugin = PluginParser.FromFilePath(path);

            // Then
            Assert.Equal("emoji", plugin.Name);
            Assert.Equal("", plugin.Schedule);
            Assert.Equal(PluginType.Shell, plugin.Type);
        }

        [Fact]
        public void PluginParser_FromFilePath_CreatesAPluginWithCorrectInterval()
        {
            // Given, When
            var plugin = PluginParser.FromFilePath("emoji.20s.sh");
            var plugin2 = PluginParser.FromFilePath("awesome.2m.sh");

            // Then
            Assert.Equal("emoji", plugin.Name);
            Assert.Equal("20s", plugin.Schedule);
            Assert.Equal(PluginType.Shell, plugin.Type);

            Assert.Equal("awesome", plugin2.Name);
            Assert.Equal("2m", plugin2.Schedule);
            Assert.Equal(PluginType.Shell, plugin2.Type);
        }

        [Fact]
        public void PluginParser_FromFilePath_CreatesAPluginWithTheCorrectType()
        {
            // Given, When
            var plugin = PluginParser.FromFilePath("snake.py");
            var plugin2 = PluginParser.FromFilePath("screen.js");

            // Then
            Assert.Equal("snake", plugin.Name);
            Assert.Equal(PluginType.Python, plugin.Type);

            Assert.Equal("screen", plugin2.Name);
            Assert.Equal(PluginType.JavaScript, plugin2.Type);
        }

        [Fact]
        public void PluginParser_FromFilePath_UnderscoreDisablesAPlugin()
        {
            // Given, When
            var plugin = PluginParser.FromFilePath("enabled.sh");
            var plugin2 = PluginParser.FromFilePath("_disabled.js");

            // Then
            Assert.Equal("enabled", plugin.Name);
            Assert.True(plugin.Enabled);
            Assert.Equal("disabled", plugin2.Name);
            Assert.False(plugin2.Enabled);
        }
    }
}
