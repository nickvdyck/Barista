using System.Collections.Generic;
using Barista.Core.Data;
using Barista.Core.Jobs;
using Barista.Core.Plugins;
using Cronos;
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
            Assert.Equal(path, plugin.FilePath);
            Assert.Equal("emoji", plugin.Name);
            Assert.Equal("", plugin.Schedule);
            Assert.Equal(PluginType.Shell, plugin.Type);
            Assert.Equal(Cron.MinuteInterval(1), plugin.Cron);
            Assert.False(plugin.Disabled);
        }

        [Theory]
        [MemberData(nameof(GetPluginFilePathTestData))]
        public void PluginParser_FromFilePath_CreatesAPluginWithCorrectInterval(string filePath, string name, string schedule, CronExpression expression)
        {
            // Given, When
            var plugin = PluginParser.FromFilePath(filePath);

            // Then
            Assert.Equal(name, plugin.Name);
            Assert.Equal(schedule, plugin.Schedule);
            Assert.Equal(expression, plugin.Cron);
        }

        [Theory]
        [InlineData("party.sh", "party", PluginType.Shell)]
        [InlineData("snake.py", "snake", PluginType.Python)]
        [InlineData("screen.js", "screen", PluginType.JavaScript)]
        [InlineData("unknown.party", "unknown", PluginType.Unknown)]
        public void PluginParser_FromFilePath_CreatesAPluginWithTheCorrectType(string filePath, string name, PluginType pluginType)
        {
            // Given, When
            var plugin = PluginParser.FromFilePath(filePath);

            // Then
            Assert.Equal(name, plugin.Name);
            Assert.Equal(pluginType, plugin.Type);
        }

        [Theory]
        [InlineData("enabled.sh", "enabled", false)]
        [InlineData("_disabled.js", "disabled", true)]
        [InlineData("____disabled_strips_remaining.py", "disabled_strips_remaining", true)]
        public void PluginParser_FromFilePath_UnderscoreDisablesAPlugin(string filePath, string name, bool disabled)
        {
            // Given, When
            var plugin = PluginParser.FromFilePath(filePath);

            // Then
            Assert.Equal(name, plugin.Name);
            Assert.Equal(disabled, plugin.Disabled);
        }

        [Theory]
        [InlineData("without_extension", "without_extension")]
        [InlineData("with_trailing_dot.", "with_trailing_dot")]
        public void PluginParser_FromFilePath_CanParsePluginsWithoutExtensionAndWeirdFileNames(string filePath, string name)
        {
            // Given, When
            var plugin = PluginParser.FromFilePath(filePath);

            // Then
            Assert.Equal(PluginType.Unknown, plugin.Type);
            Assert.Equal(name, plugin.Name);
        }

        public static IEnumerable<object[]> GetPluginFilePathTestData()
        {
            yield return new object [] { "emoji.20s.sh", "emoji", "20s", Cron.SecondInterval(20) };
            yield return new object [] { "awesome.2m.sh", "awesome", "2m", Cron.MinuteInterval(2) };
            yield return new object [] { "noextension.10h", "noextension", "10h", Cron.HourInterval(10)};
        }
    }
}
