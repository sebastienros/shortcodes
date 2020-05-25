using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shortcodes
{
    public class ShortcodesProcessor
    {
        public List<IShortcodeProvider> Providers = new List<IShortcodeProvider>();

        public ShortcodesProcessor()
        {
        }

        public async ValueTask<string> EvaluateAsync(string input)
        {
            // Don't do anything if brackets can't be found in the input text
            var openIndex = input.IndexOf("[", 0, StringComparison.OrdinalIgnoreCase);
            var closeIndex = input.IndexOf("]", 0, StringComparison.OrdinalIgnoreCase);

            if (openIndex < 0 || closeIndex < 0 || closeIndex < openIndex)
            {
                return input;
            }

            // Scan for tags
            var scanner = new Scanner(input);
            var nodes = scanner.Scan();

            // Fold all closing tags
            // [a] [b]text[hr][/b] => [a] [b]{text<hr>} 

            while (await FoldClosingTagsAsync(nodes)) ;

            using (var sb = StringBuilderPool.GetInstance())
            {
                foreach (var node in nodes)
                {
                    sb.Builder.Append(await RenderAsync(node));
                }

                return sb.ToString();
            }
        }

        private async ValueTask<bool> FoldClosingTagsAsync(List<Node> nodes)
        {
            for (var j = nodes.Count - 1; j >= 0; j--)
            {
                if (nodes[j] is Shortcode end && end.Style == ShortcodeStyle.Close)
                {
                    // Found an end tag
                    for (var i = 0; i < j; i++)
                    {
                        if (nodes[i] is Shortcode start && start.Style == ShortcodeStyle.Open && String.Equals(start.Identifier, end.Identifier, StringComparison.OrdinalIgnoreCase))
                        {
                            var text = "";

                            // Don't instantiate a builder if there is no inner node 
                            if (i < j - 1)
                            {
                                using (var sb = StringBuilderPool.GetInstance())
                                {
                                    for (var k = i + 1; k < j; k++)
                                    {
                                        sb.Builder.Append(await RenderAsync(nodes[k]));
                                    }

                                    text = sb.ToString();
                                }
                            }

                            nodes.RemoveRange(i + 1, j - i);

                            start.Content = text;

                            return true;
                        }
                    }                    
                }
            }

            return false;
        }

        public async ValueTask<string> RenderAsync(Node node)
        {
            switch (node)
            {
                case RawText raw:
                    return raw.Text;

                case Shortcode code:
                    foreach (var provider in Providers)
                    {
                        var result = await provider.EvaluateAsync(code.Identifier, code.Arguments, code.Content);

                        if (result != null)
                        {
                            return result;
                        }
                    }
                    break;
            }

            return "";
        }
    }
}
