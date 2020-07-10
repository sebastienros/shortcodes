using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shortcodes
{
    public class ShortcodesProcessor
    {
        public List<IShortcodeProvider> Providers { get; }

        public ShortcodesProcessor()
        {
            Providers = new List<IShortcodeProvider>();
        }

        public ShortcodesProcessor(params IShortcodeProvider[] providers)
        {
            Providers = new List<IShortcodeProvider>(providers);
        }

        public ShortcodesProcessor(IEnumerable<IShortcodeProvider> providers)
        {
            Providers = new List<IShortcodeProvider>(providers);
        }

        public ShortcodesProcessor(Dictionary<string, ShortcodeDelegate> shortcodes) : this (new NamedShortcodeProvider(shortcodes))
        {
        }

        /// <summary>
        /// Evaluates a template with an optional context.
        /// </summary>
        /// <param name="input">The template to evaluate.</param>
        /// <param name="context">An optional <see>Context</see> instance.</param>
        /// <returns>A string with all shortcodes evaluated.</returns>
        public async ValueTask<string> EvaluateAsync(string input, Context context = null)
        {
            if (context == null)
            {
                context = new Context();
            }

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

            return await FoldClosingTagsAsync(input, nodes, 0, nodes.Count, context);
        }

        private async ValueTask<string> FoldClosingTagsAsync(string input, List<Node> nodes, int index, int length, Context context)
        {
            // This method should not be called when nodes has a single RawText element.
            // It's implementation assumes at least two nodes are provided.

            using var sb = StringBuilderPool.GetInstance();

            // The index of the next shortcode opening node
            var cursor = index;

            // Process the list 
            while (cursor <= index + length - 1)
            {
                Shortcode start = null;
                var head = 0;
                var tail = 0;
    
                // Find the next opening tag
                while (cursor < nodes.Count && start == null)
                {
                    var node = nodes[cursor];

                    if (node is Shortcode shortCode)
                    {
                        if (shortCode.Style == ShortcodeStyle.Open)
                        {
                            head = cursor;
                            start = shortCode;
                        }
                    }
                    else
                    {
                        var text = node as RawText;

                        sb.Builder.Append(text.Text);
                    }

                    cursor += 1;
                }

                // if start is null, then there is nothing to fold
                if (start == null)
                {
                    return sb.Builder.ToString();
                }

                Shortcode end = null;

                var depth = 1;

                // Find a matching closing tag
                while (cursor <= index + length - 1 && end == null)
                {
                    if (nodes[cursor] is Shortcode shortCode)
                    {
                        if (String.Equals(start.Identifier, shortCode.Identifier, StringComparison.OrdinalIgnoreCase))
                        {
                            if (shortCode.Style == ShortcodeStyle.Open)
                            {
                                // We need to count all opening shortcodes matching the start to account for:
                                // [a] [a] [/a] [/a]

                                depth += 1;
                            }
                            else
                            {
                                depth -= 1;
                                
                                if (depth == 0)
                                {
                                    tail = cursor;
                                    end = shortCode;
                                }
                            }
                        }
                    }

                    cursor += 1;
                }

                // Is is a single tag?
                if (end == null)
                {
                    cursor = head + 1;

                    // If there are more than one open/close brace we don't evaluate the shortcode
                    if (start.OpenBraces > 1 || start.CloseBraces > 1)
                    {
                        // We need to escape the braces if counts match 
                        var bracesToSkip = start.OpenBraces == start.CloseBraces ? 1 : 0;
                        
                        sb.Builder.Append('[', start.OpenBraces - bracesToSkip);
                        sb.Builder.Append(input.Substring(start.SourceIndex + start.OpenBraces, start.SourceLength - start.CloseBraces - start.OpenBraces + 1));
                        sb.Builder.Append(']', start.CloseBraces - bracesToSkip);
                    }
                    else
                    {
                        sb.Builder.Append(await RenderAsync(start, context));
                    }
                }
                else
                {
                    // If the braces are unbalanced we can't render the shortcode
                    var canRenderShortcode = start.OpenBraces == 1 && start.CloseBraces == 1 && end.OpenBraces == 1 && end.CloseBraces == 1;

                    if (canRenderShortcode)
                    {
                        // Are the tags adjacent?
                        if (tail - head == 1)
                        {
                            start.Content = "";
                            sb.Builder.Append(await RenderAsync(start, context));
                        }
                        // Is there a single Raw text between the tags?
                        else if (tail - head == 2)
                        {
                            var content = nodes[head+1] as RawText;
                            start.Content = content.Text;
                            sb.Builder.Append(await RenderAsync(start, context));
                        }
                        // Fold the inner nodes
                        else
                        {
                            var content = await FoldClosingTagsAsync(input, nodes, head + 1, tail - head - 1, context);
                            start.Content = content;
                            sb.Builder.Append(await RenderAsync(start, context));
                        }        
                    }
                    else
                    {
                        var bracesToSkip = start.OpenBraces == end.CloseBraces ? 1 : 0;

                        sb.Builder.Append('[', start.OpenBraces - bracesToSkip);
                        sb.Builder.Append(input.Substring(start.SourceIndex + start.OpenBraces, end.SourceIndex + end.SourceLength - end.CloseBraces - start.SourceIndex - start.OpenBraces + 1));
                        sb.Builder.Append(']', end.CloseBraces - bracesToSkip);
                    }        
                }
            }
            
            return sb.Builder.ToString();
        }

        public async ValueTask<string> RenderAsync(Node node, Context context)
        {
            switch (node)
            {
                case RawText raw:
                    return raw.Text;

                case Shortcode code:
                    foreach (var provider in Providers)
                    {
                        var result = await provider.EvaluateAsync(code.Identifier, code.Arguments, code.Content, context);

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
