using System;

namespace Shortcodes
{
    public class Shortcode : Node
    {
        public Shortcode(string identifier, ShortcodeStyle style, int openBraces, int closeBraces, int sourceIndex, int sourceLength, Arguments arguments)
        {
            Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
            Style = style;
            Content = null;
            OpenBraces = openBraces;
            CloseBraces = closeBraces;
            SourceIndex = sourceIndex;
            SourceLength = sourceLength;
            Arguments = arguments;
        }

        public string Identifier { get; }
        public ShortcodeStyle Style { get; }
        public string Content { get; set; }
        public Arguments Arguments { get; }
        public int SourceIndex { get; set; }
        public int SourceLength { get; set; }
        public int OpenBraces { get; set; } = 1;
        public int CloseBraces { get; set; } = 1;
    }
}
