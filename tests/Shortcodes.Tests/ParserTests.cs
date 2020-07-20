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
                ["hello"] = (args, content, ctx) => new ValueTask<string>("Hello world!"),
                ["named_or_default"] = (args, content, ctx) => new ValueTask<string>("Hello " + args.NamedOrDefault("name")),
                ["upper"] = (args, content, ctx) => new ValueTask<string>(content.ToUpperInvariant()),
                ["positional"] = (args, content, ctx) => 
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
        [InlineData(null)]
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
        [InlineData("[named_or_default Name='world!']", "Hello world!")]
        [InlineData("[named_or_default NAME='world!']", "Hello world!")]
        public async Task ArgumentsAreCaseInsensitive(string input, string expected)
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

        [Fact]
        public async Task PositionalArgumentMixedWithNamedArguments()
        {
            var provider = new NamedShortcodeProvider
            {
                ["hello"] = (args, content, ctx) => 
                {
                    Assert.Equal("1", args.At(0));
                    Assert.Equal("b", args.At(1));
                    Assert.Equal("d", args.Named("c"));
                    Assert.Equal("123", args.At(2));

                    return new ValueTask<string>("");
                }
            };
            
            var parser = new ShortcodesProcessor(_provider);
            await parser.EvaluateAsync("[hello 1 b c=d 123]");
        }
        
        [Theory]
        [InlineData("1234")]
        [InlineData("true")]
        [InlineData("123_456")]
        [InlineData("http://github.com")]
        [InlineData("http://github.com?foo=bar")]
        public async Task ValuesAreParsed(string input)
        {
            var parser = new ShortcodesProcessor(_provider);

            Assert.Equal($"0:{input};", await parser.EvaluateAsync($"[positional {input}]"));
        }

        [Fact]
        public async Task ContextIsShareAcrossShortcodes()
        {
            var parser = new ShortcodesProcessor(new NamedShortcodeProvider
            {
                ["inc"] = (args, content, ctx) => { ctx["x"] = ctx.GetOrSetValue("x", 0) + 1; return new ValueTask<string>(ctx["x"].ToString()); },
                ["val"] = (args, content, ctx) => new ValueTask<string>(ctx["x"].ToString())
            });

            Assert.Equal("122", await parser.EvaluateAsync($"[inc][inc][val]"));
        }

        [Fact]
        public async Task ShouldUseContextValue()
        {
            var parser = new ShortcodesProcessor(new NamedShortcodeProvider
            {
                ["hello"] = (args, content, ctx) => { return new ValueTask<string>("message: " + ctx["message"].ToString()); }
            });

            Assert.Equal("message: Hello World!", await parser.EvaluateAsync($"[hello]", new Context { ["message"] = "Hello World!" }));
        }
    }
}
