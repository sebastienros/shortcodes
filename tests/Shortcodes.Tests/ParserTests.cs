using System.Threading.Tasks;
using Xunit;

namespace Shortcodes.Tests
{
    public class ParserTests
    {
        private NamedShortcodeProvider _provider;

        public ParserTests()
        {
            _provider = new NamedShortcodeProvider
            {
                ["hello"] = (args, content) => new ValueTask<string>("Hello world!"),
                ["named_or_default"] = (args, content) => new ValueTask<string>("Hello " + args.NamedOrDefault("name")),
                ["upper"] = (args, content) => new ValueTask<string>(content.ToUpperInvariant()),
                ["positional"] = (args, content) => 
                {
                    string result = "";

                    for (var i=0; i<args.Count; i++)
                    {
                        result += $"{i}:{args.At(i)};";    
                    }

                    return new ValueTask<string>(result);
                }
            };
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
            var parser = new ShortcodesProcessor(_provider);
            Assert.Equal(expected, await parser.EvaluateAsync(input));
        }

        [Theory]
        [InlineData("[[hello]", "[[hello]")]
        [InlineData("[[[hello]", "[[[hello]")]
        [InlineData("[hello]]", "[hello]]")]
        [InlineData("[hello]]]", "[hello]]]")]
        [InlineData("[[upper]a[/upper]", "[[upper]a[/upper]")]
        [InlineData("[upper]a[/upper]]", "[upper]a[/upper]]")]
        public async Task IgnoresIncompleteShortcodes(string input, string expected)
        {
            var parser = new ShortcodesProcessor(_provider);

            Assert.Equal(expected, await parser.EvaluateAsync(input));
        }

        [Theory]
        [InlineData("[[hello]]", "[hello]")]
        [InlineData("[[[hello]]]", "[[hello]]")]
        [InlineData("[[[[hello]]]]", "[[[hello]]]")]
        public async Task EscapeSingleShortcodes(string input, string expected)
        {
            var parser = new ShortcodesProcessor(_provider);

            Assert.Equal(expected, await parser.EvaluateAsync(input));
        }

        [Theory]
        [InlineData("[[upper]a[/upper]]", "[upper]a[/upper]")]
        [InlineData("[[[upper]a[/upper]]]", "[[upper]a[/upper]]")]
        [InlineData("[[[[upper]a[/upper]]]]", "[[[upper]a[/upper]]]")]
        public async Task EscapeEnclosedShortcodes(string input, string expected)
        {
            var parser = new ShortcodesProcessor(_provider);

            Assert.Equal(expected, await parser.EvaluateAsync(input));
        }

        [Theory]
        [InlineData("[hello]a[/hello]", "Hello world!")]
        public async Task ProcessClosingShortcodes(string input, string expected)
        {
            var parser = new ShortcodesProcessor(_provider);

            Assert.Equal(expected, await parser.EvaluateAsync(input));
        }

        [Theory]
        [InlineData("[named_or_default name='world!']", "Hello world!")]
        [InlineData("[named_or_default 'world!']", "Hello world!")]
        public async Task NamedOrDefaultArguments(string input, string expected)
        {
            var parser = new ShortcodesProcessor(_provider);

            Assert.Equal(expected, await parser.EvaluateAsync(input));
        }

        [Theory]
        [InlineData("[upper]lorem[/upper]", "LOREM")]
        [InlineData("[upper]lorem[/upper] [upper]ipsum[/upper]", "LOREM IPSUM")]
        [InlineData("[upper]lorem [upper]ipsum[/upper][/upper]", "LOREM IPSUM")]
        [InlineData("[upper]lorem [hello][/upper]", "LOREM HELLO WORLD!")]
        [InlineData("[upper]lorem [upper]ipsum[/upper] [upper]dolor[/upper][/upper]", "LOREM IPSUM DOLOR")]
        [InlineData("[upper][/upper]", "")]
        public async Task FoldsRecursiceShortcodes(string input, string expected)
        {
            var parser = new ShortcodesProcessor(_provider);

            Assert.Equal(expected, await parser.EvaluateAsync(input));
        }

        [Theory]
        [InlineData("[positional '1' 'b' c='d' '123']", "0:1;1:b;2:;3:123;")]
        public async Task GetsArgumentsByPosition(string input, string expected)
        {
            var parser = new ShortcodesProcessor(_provider);

            Assert.Equal(expected, await parser.EvaluateAsync(input));
        }        
    }
}
