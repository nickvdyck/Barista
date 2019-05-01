using Xunit;
using Barista.Core.Extensions;

namespace Barista.Core.Tests
{
    public class EmojizeTests
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
