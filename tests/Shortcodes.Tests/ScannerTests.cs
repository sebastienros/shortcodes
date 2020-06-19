using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Shortcodes.Tests
{
    public class ScannerTests
    {
        private StringBuilder _builder = new StringBuilder();

        private string EncodeNodes(List<Node> nodes)
        {
            _builder.Clear();

            foreach (var node in nodes)
            {
                switch (node)
                {
                    case Shortcode shortcode:
                        _builder.Append("[");

                        if (shortcode.Style == ShortcodeStyle.Close)
                        {
                            _builder.Append("/");
                        }

                        _builder.Append(shortcode.Identifier);

                        if (shortcode.Arguments.Any())
                        {
                            foreach (var argument in shortcode.Arguments)
                            {
                                _builder.Append(" ").Append(argument.Key).Append('=').Append(argument.Value);
                            }
                        }

                        if (shortcode.Style == ShortcodeStyle.SelfClosing)
                        {
                            _builder.Append(" /");
                        }

                        _builder.Append("]");
                        break;

                    case RawText raw:
                        _builder.Append($"R({raw.Text.Length})");
                        break;
                }
            }

            return _builder.ToString();
        }

        [Theory]
        [InlineData("[hello/]", "[hello /]")]
        [InlineData("[hello /]", "[hello /]")]
        [InlineData("[ hello /]", "[hello /]")]
        [InlineData(" [hello /] ", "R(1)[hello /]R(1)")]
        public void ShouldScanSelfClosingTags(string input, string encoded)
        {
            var scanner = new Scanner(input);
            var nodes = scanner.Scan();
            var result = EncodeNodes(nodes);

            Assert.Equal(encoded, result);
        }

        [Theory]
        [InlineData("[/hello]", "[/hello]")]
        [InlineData("[/hello ]", "[/hello]")]
        [InlineData("[/ hello]", "[/hello]")]
        [InlineData(" [/hello] ", "R(1)[/hello]R(1)")]
        public void ShouldScanCloseTags(string input, string encoded)
        {
            var scanner = new Scanner(input);
            var nodes = scanner.Scan();
            var result = EncodeNodes(nodes);

            Assert.Equal(encoded, result);
        }

        [Theory]
        [InlineData("[/hello", "R(7)")]
        [InlineData("[ /hello ]", "R(10)")]
        [InlineData("[/ hello[", "R(9)")]
        public void ShouldIgnoreMalformedTags(string input, string encoded)
        {
            var scanner = new Scanner(input);
            var nodes = scanner.Scan();
            var result = EncodeNodes(nodes);

            Assert.Equal(encoded, result);
        }

        [Theory]
        [InlineData("[hello][/hello]", "[hello][/hello]")]
        [InlineData("[hello] [/hello]", "[hello]R(1)[/hello]")]
        [InlineData("a[hello]b[/hello]c", "R(1)[hello]R(1)[/hello]R(1)")]
        [InlineData("a[hello]b[/hello]c[hello]d[/hello]e", "R(1)[hello]R(1)[/hello]R(1)[hello]R(1)[/hello]R(1)")]
        public void ShouldScanMixedTags(string input, string encoded)
        {
            var scanner = new Scanner(input);
            var nodes = scanner.Scan();
            var result = EncodeNodes(nodes);

            Assert.Equal(encoded, result);
        }

        [Theory]
        [InlineData("[hello a='b']", "[hello a=b]")]
        [InlineData("[hello a='b' c=\"d\"]", "[hello a=b c=d]")]
        [InlineData("[hello 'a']", "[hello 0=a]")]
        [InlineData("[hello 'a' b='c' 'd']", "[hello 0=a b=c 1=d]")]
        public void ShouldScanArguments(string input, string encoded)
        {
            var scanner = new Scanner(input);
            var nodes = scanner.Scan();
            var result = EncodeNodes(nodes);

            Assert.Equal(encoded, result);
        }

        [Theory]
        [InlineData("[hello a='b]", "R(12)")]
        [InlineData("[hello a]", "R(9)")]
        public void ShouldIgnoreMalformedArguments(string input, string encoded)
        {
            var scanner = new Scanner(input);
            var nodes = scanner.Scan();
            var result = EncodeNodes(nodes);

            Assert.Equal(encoded, result);
        }

        [Theory]
        [InlineData("[h a='\\u03A9']", "[h a=Ω]")]
        [InlineData("[h a='\\xe9']", "[h a=é]")]
        [InlineData("[h a='\\xE9']", "[h a=é]")]
        [InlineData("[h a='\\a']", "R(10)")]
        [InlineData("[h a='\\0']", "[h a=\0]")]
        [InlineData("[h a='\\\\']", "[h a=\\]")]
        [InlineData("[h a='\\\"']", "[h a=\"]")]
        [InlineData("[h a='\\\'']", "[h a=']")]
        [InlineData("[h a='\\b']", "[h a=\b]")]
        [InlineData("[h a='\\f']", "[h a=\f]")]
        [InlineData("[h a='\\n']", "[h a=\n]")]
        [InlineData("[h a='\\r']", "[h a=\r]")]
        [InlineData("[h a='\\t']", "[h a=\t]")]
        [InlineData("[h a='\\v']", "[h a=\v]")]
        public void ShouldEscapeStrings(string input, string encoded)
        {
            var scanner = new Scanner(input);
            var nodes = scanner.Scan();
            var result = EncodeNodes(nodes);

            Assert.Equal(encoded, result);
        }

        [Theory]
        [InlineData("[h a='\\u0']", "R(11)")]
        [InlineData("[h a='\\xe']", "R(11)")]
        public void ShouldNotParseInvalidEscapeSequence(string input, string encoded)
        {
            var scanner = new Scanner(input);
            var nodes = scanner.Scan();
            var result = EncodeNodes(nodes);

            Assert.Equal(encoded, result);
        }

        [Theory]
        [InlineData("[h a='\"']", "[h a=\"]")]
        [InlineData("[h a=\"'\"]", "[h a=']")]
        public void ShouldStringsWitBothQuotes(string input, string encoded)
        {
            var scanner = new Scanner(input);
            var nodes = scanner.Scan();
            var result = EncodeNodes(nodes);

            Assert.Equal(encoded, result);
        }

    }
}
