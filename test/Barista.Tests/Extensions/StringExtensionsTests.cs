using Xunit;
using Barista.Extensions;

namespace Barista.Tests.Extensions
{
    public class StringExtensionsTests
    {
        [Theory]
        [InlineData("Looks good :+1:", "Looks good 👍")]
        [InlineData("Can replace multiple :smile: or other handles: :star:, :punch:", "Can replace multiple 😄 or other handles: ⭐, 👊")]
        public void Emojize_ReplaceEmoji_ReplacesAllEmojiHandlesWithCorrectUnicodeSymbols(string input, string expected)
        {
            // Given, When
            var output = input.ReplaceEmoji();

            // Then
            Assert.Equal(expected, output);
        }
    }
}
