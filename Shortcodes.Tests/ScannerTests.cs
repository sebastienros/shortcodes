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

                        if (shortcode.Arguments?.Count > 0)
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
    }
}
