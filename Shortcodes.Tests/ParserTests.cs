using System.Threading.Tasks;
using Xunit;

namespace Shortcodes.Tests
{
    public class ParserTests
    {
        private NamedShortcodeProvider _provider = new NamedShortcodeProvider();

        public ParserTests()
        {
            _provider.Shortcodes["hello"] = (args, content) => new ValueTask<string>("Hello world!");
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("  ")]
        [InlineData("a")]
        [InlineData("a b c")]
        [InlineData("Hello World!")]
        [InlineData("Hello [ World!")]
        [InlineData("Hello ] World!")]
        [InlineData("Hello ] [ World!")]
        public async Task DoesntProcessInputsWithoutBrackets(string input)
        {
            var parser = new ShortcodesProcessor();

            Assert.Same(input, await parser.EvaluateAsync(input));
        }

        [Theory]
        [InlineData("[hello]", "Hello world!")]
        [InlineData(" [hello] ", " Hello world! ")]
        [InlineData("a [hello] b", "a Hello world! b")]
        public async Task ProcessShortcodes(string input, string expected)
        {
            var parser = new ShortcodesProcessor();
            parser.Providers.Add(_provider);

            Assert.Equal(expected, await parser.EvaluateAsync(input));
        }

        [Theory]
        [InlineData("[[hello]", "[[hello]")]
        [InlineData("[hello]]", "[hello]]")]
        [InlineData("[[hello]]", "[[hello]]")]
        public async Task IgnoresIncompleteShortcodes(string input, string expected)
        {
            var parser = new ShortcodesProcessor();
            parser.Providers.Add(_provider);

            Assert.Equal(expected, await parser.EvaluateAsync(input));
        }

        [Theory]
        [InlineData("[hello]a[/hello]", "Hello world!")]
        public async Task ProcessClosingShortcodes(string input, string expected)
        {
            var parser = new ShortcodesProcessor();
            parser.Providers.Add(_provider);

            Assert.Equal(expected, await parser.EvaluateAsync(input));
        }
    }
}
