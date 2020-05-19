using System;
using System.Collections.Generic;

namespace Shortcodes
{
    public class Shortcode : Node
    {
        public Shortcode(string identifier, ShortcodeStyle style)
        {
            if (identifier == null)
            {
                throw new ArgumentNullException(nameof(identifier));
            }

            Identifier = identifier;
            Style = style;
            Content = null;
        }

        public string Identifier { get; }
        public ShortcodeStyle Style { get; }
        public string Content { get; set; }
        public Dictionary<string, string> Arguments { get; set; }
    }
}
